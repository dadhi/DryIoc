using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using DryIoc.MefAttributedModel;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using ExpressionToCodeLib.Unstable_v2_Api;

namespace DryIoc.Zero.UnitTests
{
    [TestFixture]
    public class ZeroContainerTests
    {
        [Test]
        public void Can_Register_default_delegate()
        {
            var container = new ZeroContainer();
            container.Register(typeof(Potato), (r, scope) => new Potato());

            var potato = container.Resolve<Potato>();
            
            Assert.IsNotNull(potato);
        }

        [Test]
        public void Can_Register_keyed_delegate()
        {
            var container = new ZeroContainer();
            container.Register(typeof(Potato), "mashed", (r, scope) => new Potato());

            var potato = container.Resolve<Potato>("mashed");

            Assert.IsNotNull(potato);
        }

        internal class Potato {}

        [Test]
        public void Can_open_scope()
        {
            var container = new ZeroContainer();
            container.Register(typeof(Potato), (r, scope) => new Potato());
            using (var scope = container.OpenScope())
            {
                var potato = scope.Resolve<Potato>();
                Assert.IsNotNull(potato);
            }
        }

        [Test]
        public void Dispose_should_remove_registrations()
        {
            var container = new ZeroContainer();
            container.Register(typeof(Potato), (r, scope) => new Potato());
            container.Dispose();
            Assert.Throws<ZeroContainerException>(() => container.Resolve<Potato>());
        }

        [Test]
        public void Can_load_types_from_assembly_and_generate_some_resolutions()
        {
            var container = new Container(rules => rules
                .WithoutSingletonOptimization()
                .WithMefAttributedModel());

            var types = typeof(BirdFactory).GetAssembly().GetLoadedTypes();
            container.RegisterExports(types);

            var r = container.GetServiceRegistrations().FirstOrDefault(x => x.ServiceType == typeof(Chicken));
            var factoryExpr = container.Resolve<LambdaExpression>(r.OptionalServiceKey, IfUnresolved.Throw, r.ServiceType);

            Assert.DoesNotThrow(() => ExpressionStringify.With(true, true).ToCode(factoryExpr));
        }

        [Test]
        public void Generate_factory_delegate_for_exported_static_factory_method()
        {
            var container = new Container(rules => rules
                .WithoutSingletonOptimization()
                .WithMefAttributedModel());

            container.RegisterExports(typeof(BirdFactory));

            var r = container.GetServiceRegistrations().FirstOrDefault(x => x.ServiceType == typeof(Chicken));
            var factoryExpr = container.Resolve<LambdaExpression>(r.OptionalServiceKey, IfUnresolved.Throw, r.ServiceType);

            Assert.DoesNotThrow(() => ExpressionStringify.With(true, true).ToCode(factoryExpr));
        }

        [Test]
        public void Can_resolve_singleton()
        {
            var container = new ZeroContainer();

            var service = container.Resolve<ISomeDb>();
            Assert.NotNull(service);
            Assert.AreSame(service, container.Resolve<ISomeDb>());
        }

        [Test]
        public void Can_resolve_singleton_with_key()
        {
            var container = new ZeroContainer();

            var service = container.Resolve<IMultiExported>("j");
            Assert.NotNull(service);
            Assert.AreSame(service, container.Resolve<IMultiExported>("c"));
        }

        [Test]
        public void Will_throw_for_not_registered_service_type()
        {
            var container = new ZeroContainer();

            var ex = Assert.Throws<ZeroContainerException>(
                () => container.Resolve<NotRegistered>());

            Assert.AreEqual(ex.Error, Error.UnableToResolveService);
        }

        [Test]
        public void Will_return_null_for_not_registered_service_type_with_IfUnresolved_option()
        {
            var container = new ZeroContainer();

            var nullService = container.Resolve<NotRegistered>(IfUnresolved.ReturnDefault);

            Assert.IsNull(nullService);
        }

        [Test]
        public void Can_resolve_many()
        {
            var container = new ZeroContainer();

            var handlers = container.ResolveMany<IHandler>().ToArray();

            Assert.AreEqual(5, handlers.Length);
        }

        internal class NotRegistered {}
    }
}