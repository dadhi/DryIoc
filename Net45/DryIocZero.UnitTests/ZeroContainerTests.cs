using System.Linq;
using NUnit.Framework;
using DryIoc.MefAttributedModel.UnitTests.CUT;

namespace DryIocZero.UnitTests
{
    [TestFixture]
    public class ZeroContainerTests
    {
        [Test]
        public void Can_Register_default_delegate()
        {
            var container = new Container();
            container.RegisterDelegate(r => new Potato());

            var potato = container.Resolve(typeof(Potato), false);
            
            Assert.IsNotNull(potato);
        }

        [Test]
        public void Can_Register_keyed_delegate()
        {
            var container = new Container();
            container.RegisterDelegate(r => new Potato(), "mashed");

            var potato = container.Resolve(typeof(Potato), "mashed");

            Assert.IsNotNull(potato);
        }

        [Test]
        public void Can_Register_Instance()
        {
            var container = new Container();
            var potato = new Potato();
            container.RegisterInstance(potato);

            var resolvedPotato = container.Resolve(typeof(Potato), false);

            Assert.AreSame(potato, resolvedPotato);
        }

        internal class Potato {}

        [Test]
        public void Can_open_scope()
        {
            var container = new Container();
            container.Register(typeof(Potato), (r, scope) => new Potato());
            using (var scope = container.OpenScope())
            {
                var potato = scope.Resolve(typeof(Potato), false);
                Assert.IsNotNull(potato);
            }
        }

        [Test]
        public void Dispose_should_remove_registrations()
        {
            var container = new Container();
            container.Register(typeof(Potato), (r, scope) => new Potato());
            container.Dispose();
            Assert.Throws<ContainerException>(() => container.Resolve(typeof(Potato), false));
        }

        [Test]
        public void Can_resolve_singleton()
        {
            var container = new Container();

            var service = container.Resolve(typeof(ISomeDb), false);
            Assert.IsNotNull(service);
            Assert.AreSame(service, container.Resolve(typeof(ISomeDb), false));
        }

        [Test]
        public void Can_resolve_singleton_with_key()
        {
            var container = new Container();

            var service = container.Resolve(typeof(IMultiExported), "c");
            Assert.IsNotNull(service);
            Assert.AreSame(service, container.Resolve(typeof(IMultiExported), "c"));
        }

        [Test]
        public void Will_throw_for_not_registered_service_type()
        {
            var container = new Container();

            var ex = Assert.Throws<ContainerException>(
                () => container.Resolve(typeof(NotRegistered), false));

            Assert.AreEqual(ex.Error, Error.UnableToResolveDefaultService);
        }

        [Test]
        public void Will_return_null_for_not_registered_service_type_with_IfUnresolved_option()
        {
            var container = new Container();

            var nullService = container.Resolve(typeof(NotRegistered), true);

            Assert.IsNull(nullService);
        }

        [Test]
        public void Can_resolve_many()
        {
            var container = new Container();

            var handlers = container.ResolveMany(typeof(ISomeDb)).Cast<ISomeDb>().ToArray<ISomeDb>();

            Assert.AreEqual(1, handlers.Length);
        }

        internal class NotRegistered {}
    }
}