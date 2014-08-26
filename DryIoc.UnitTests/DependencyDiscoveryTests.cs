using System.Linq;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class DependencyDiscoveryTests
    {
        [Test]
        public void Specify_property_selector_when_registering_service()
        {
            var container = new Container();

            container.Register<SomeBlah>(dependencyDiscovery: DependencyDiscoveryRules.Empty.WithPropertiesAndFields(
                (type, request, registry) => type.GetProperties().Select(ServiceInfo.Of)));
            container.Register<IService, Service>();

            var blah = container.Resolve<SomeBlah>();
            Assert.That(blah.Uses, Is.InstanceOf<Service>());
        }

        public class SomeBlah
        {
            public IService Uses { get; set; }
        }
    }
}
