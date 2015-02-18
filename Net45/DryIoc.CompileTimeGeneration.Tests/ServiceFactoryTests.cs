using System.Linq;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.CompileTimeGeneration.Tests
{
    [TestFixture]
    public class ServiceFactoryTests
    {
        [Test]
        public void Can_resolve_singleton()
        {
            var factory = new ServiceFactory();

            var service = factory.Resolve<ISomeDb>();
            Assert.NotNull(service);
            Assert.AreSame(service, factory.Resolve<ISomeDb>());
        }

        [Test]
        public void Can_resolve_singleton_with_key()
        {
            var factory = new ServiceFactory();

            var service = factory.Resolve<IMultiExported>("j");
            Assert.NotNull(service);
            Assert.AreSame(service, factory.Resolve<IMultiExported>("c"));
        }

        [Test]
        public void Will_throw_for_not_registered_service_type()
        {
            var factory = new ServiceFactory();

            var ex = Assert.Throws<ContainerException>(() => factory.Resolve<NotRegistered>());

            Assert.AreEqual(ex.Error, Error.UNABLE_TO_RESOLVE_SERVICE);
        }

        [Test]
        public void Will_return_null_for_not_registered_service_type_with_IfUnresolved_option()
        {
            var factory = new ServiceFactory();

            var nullService = factory.Resolve<NotRegistered>(IfUnresolved.ReturnDefault);

            Assert.IsNull(nullService);
        }

        [Test, Ignore]
        public void Can_resolve_many()
        {
            var factory = new ServiceFactory();

            var handlers = factory.ResolveMany<IHandler>().ToArray();

            Assert.AreEqual(5, handlers.Length);
        }

        internal class NotRegistered {}
    }
}