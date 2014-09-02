using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class ProvidedServiceTypeTests
    {
        [Test]
        public void Resolving_service_with_not_assignable_provided_type_should_Throw()
        {
            var container = new Container();
            container.Register<Service>();

            var ex = Assert.Throws<ContainerException>(
                () => container.Resolve<string>(typeof(Service)));

            Assert.That(ex.Message, Is
                .StringContaining("Service is not assignable to").And
                .StringContaining("String"));
        }

        [Test]
        public void Resolving_Lazy_service_with_provided_type_and_key_should_work()
        {
            var container = new Container();
            container.Register<Service>(named: 1);

            var service = container.Resolve<IService>(1, providedServiceType: typeof(Service));

            Assert.That(service, Is.Not.Null);
        }
    }
}
