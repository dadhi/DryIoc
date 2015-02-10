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
    }
}