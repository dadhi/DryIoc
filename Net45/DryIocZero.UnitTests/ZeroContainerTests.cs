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
            container.Register(typeof(Potato), (r, scope) => new Potato());

            var potato = container.ResolveDefault(typeof(Potato), false);
            
            Assert.IsNotNull(potato);
        }

        [Test]
        public void Can_Register_keyed_delegate()
        {
            var container = new Container();
            container.Register(typeof(Potato), "mashed", (r, scope) => new Potato());

            var potato = container.ResolveKeyed(typeof(Potato), "mashed", false, null, null);

            Assert.IsNotNull(potato);
        }

        internal class Potato {}

        [Test]
        public void Can_open_scope()
        {
            var container = new Container();
            container.Register(typeof(Potato), (r, scope) => new Potato());
            using (var scope = container.OpenScope())
            {
                var potato = scope.ResolveDefault(typeof(Potato), false);
                Assert.IsNotNull(potato);
            }
        }

        [Test]
        public void Dispose_should_remove_registrations()
        {
            var container = new Container();
            container.Register(typeof(Potato), (r, scope) => new Potato());
            container.Dispose();
            Assert.Throws<ContainerException>(() => container.ResolveDefault(typeof(Potato), false));
        }

        [Test]
        public void Can_resolve_singleton()
        {
            var container = new Container();

            var service = container.ResolveDefault(typeof(ISomeDb), false);
            Assert.NotNull(service);
            Assert.AreSame(service, container.ResolveDefault(typeof(ISomeDb), false));
        }

        [Test]
        public void Can_resolve_singleton_with_key()
        {
            var container = new Container();

            var service = container.ResolveKeyed(typeof(IMultiExported), "c", false, null, null);
            Assert.NotNull(service);
            Assert.AreSame(service, container.ResolveKeyed(typeof(IMultiExported), "c", false, null, null));
        }

        [Test]
        public void Will_throw_for_not_registered_service_type()
        {
            var container = new Container();

            var ex = Assert.Throws<ContainerException>(
                () => container.ResolveDefault(typeof(NotRegistered), false));

            Assert.AreEqual(ex.Error, Error.UnableToResolveService);
        }

        [Test]
        public void Will_return_null_for_not_registered_service_type_with_IfUnresolved_option()
        {
            var container = new Container();

            var nullService = container.ResolveDefault(typeof(NotRegistered), true);

            Assert.IsNull(nullService);
        }

        [Test]
        public void Can_resolve_many()
        {
            var container = new Container();

            var handlers = container.ResolveMany(typeof(ISomeDb), null, null, null, null).Cast<ISomeDb>().ToArray<ISomeDb>();

            Assert.AreEqual(1, handlers.Length);
        }

        internal class NotRegistered {}
    }
}