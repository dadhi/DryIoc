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
    }
}
