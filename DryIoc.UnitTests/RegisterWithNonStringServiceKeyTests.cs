using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class RegisterWithNonStringServiceKeyTests
    {
        [Test]
        public void Register_and_resolve_service_with_enumeration_key_should_work()
        {
            var container = new Container();
            container.Register<IService, Service>(named: ServiceColors.Red);

            var service = container.Resolve<IService>(ServiceColors.Red);

            Assert.IsNotNull(service);
        }

        [Test]
        public void Register_with_one_and_resolve_with_another_key_should_Throw()
        {
            var container = new Container();
            container.Register<IService, Service>(named: ServiceColors.Red);

            Assert.Throws<ContainerException>(() => 
                container.Resolve<IService>(ServiceColors.Green));
        }

    }

    public enum ServiceColors { Red, Green, Blue }
}
