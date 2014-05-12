using System.Linq;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class IfAlreadyRegisteredTests
    {
        [Test]
        public void Can_update_registered_default_with_new_default()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>(ifAlreadyRegistered: IfAlreadyRegistered.UpdateRegistered);

            var service = container.Resolve<IService>();

            Assert.That(service, Is.InstanceOf<AnotherService>());
        }

        [Test]
        public void Can_update_registered_named_with_new_named()
        {
            var container = new Container();
            container.Register<IService, Service>(named: EnumKey.Some);
            container.Register<IService, AnotherService>(named: EnumKey.Some, ifAlreadyRegistered: IfAlreadyRegistered.UpdateRegistered);

            var service = container.Resolve<IService>(EnumKey.Some);

            Assert.That(service, Is.InstanceOf<AnotherService>());
        }

        [Test]
        public void Can_update_second_registered_default()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, DisposableService>();
            container.Register<IService, AnotherService>(ifAlreadyRegistered: IfAlreadyRegistered.UpdateRegistered);

            var services = container.Resolve<IService[]>();

            CollectionAssert.AreEqual(
                new[] { typeof(Service), typeof(AnotherService) },
                services.Select(service => service.GetType()));
        }
    }
}
