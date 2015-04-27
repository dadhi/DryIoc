using System.Linq;
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
            var factory = new ZeroContainer();
            factory.Register(typeof(Potato), (r, scope) => new Potato());

            var potato = factory.Resolve<Potato>();
            
            Assert.IsNotNull(potato);
        }

        [Test]
        public void Can_Register_keyed_delegate()
        {
            var factory = new ZeroContainer();
            factory.Register(typeof(Potato), "mashed", (r, scope) => new Potato());

            var potato = factory.Resolve<Potato>("mashed");

            Assert.IsNotNull(potato);
        }

        internal class Potato {}

        [Test]
        public void Can_load_types_from_assembly_and_generate_some_resolutions()
        {
            var container = new Container(rules => rules
                .WithoutSingletonOptimization()
                .WithMefAttributedModel());

            var types = typeof(BirdFactory).GetAssembly().GetLoadedTypes();
            container.RegisterExports(types);

            var r = container.GetServiceRegistrations().FirstOrDefault(x => x.ServiceType == typeof(Chicken));
            var factoryExpr = container.Resolve<FactoryExpression<object>>(r.OptionalServiceKey, IfUnresolved.Throw, r.ServiceType);

            Assert.DoesNotThrow(() => ExpressionStringify.With(true, true).ToCode(factoryExpr.Value));
        }

        [Test]
        public void Generate_factory_delegate_for_exported_static_factory_method()
        {
            var container = new Container(rules => rules
                .WithoutSingletonOptimization()
                .WithMefAttributedModel());

            container.RegisterExports(typeof(BirdFactory));

            var r = container.GetServiceRegistrations().FirstOrDefault(x => x.ServiceType == typeof(Chicken));
            var factoryExpr = container.Resolve<FactoryExpression<object>>(r.OptionalServiceKey, IfUnresolved.Throw, r.ServiceType);

            Assert.DoesNotThrow(() => ExpressionStringify.With(true, true).ToCode(factoryExpr.Value));
        }

        [Test]
        public void Can_resolve_singleton()
        {
            var factory = new ZeroContainer();
            CompositionRoot.Default.RegisterGeneratedRoots(factory);

            var service = factory.Resolve<ISomeDb>();
            Assert.NotNull(service);
            Assert.AreSame(service, factory.Resolve<ISomeDb>());
        }

        [Test]
        public void Can_resolve_singleton_with_key()
        {
            var factory = new ZeroContainer();
            CompositionRoot.Default.RegisterGeneratedRoots(factory);

            var service = factory.Resolve<IMultiExported>("j");
            Assert.NotNull(service);
            Assert.AreSame(service, factory.Resolve<IMultiExported>("c"));
        }

        [Test]
        public void Will_throw_for_not_registered_service_type()
        {
            var factory = new ZeroContainer();
            CompositionRoot.Default.RegisterGeneratedRoots(factory);

            var ex = Assert.Throws<ZeroContainerException>(
                () => factory.Resolve<NotRegistered>());

            Assert.AreEqual(ex.Error, Error.UNABLE_TO_RESOLVE_SERVICE);
        }

        [Test]
        public void Will_return_null_for_not_registered_service_type_with_IfUnresolved_option()
        {
            var factory = new ZeroContainer();

            var nullService = factory.Resolve<NotRegistered>(IfUnresolved.ReturnDefault);

            Assert.IsNull(nullService);
        }

        [Test]
        public void Can_resolve_many()
        {
            var factory = new ZeroContainer();
            CompositionRoot.Default.RegisterGeneratedRoots(factory);

            var handlers = factory.ResolveMany<IHandler>().ToArray();

            Assert.AreEqual(5, handlers.Length);
        }

        internal class NotRegistered {}
    }
}