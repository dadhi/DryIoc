using System.Linq;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class IfAlreadyRegisteredTests
    {
        [Test]
        public void By_default_appends_new_default_registration()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>();

            var services = container.Resolve<IService[]>();

            CollectionAssert.AreEqual(
                new[] {typeof(Service), typeof(AnotherService) }, 
                services.Select(s => s.GetType()));
        }

        [Test]
        public void I_can_say_to_Throw_on_new_default_registration()
        {
            var container = new Container();
            container.Register<IService, Service>(ifAlreadyRegistered: IfAlreadyRegistered.Throw);

            var ex = Assert.Throws<ContainerException>(() =>
            container.Register<IService, AnotherService>(ifAlreadyRegistered: IfAlreadyRegistered.Throw));

            Assert.AreEqual(ex.Error, Error.UNABLE_TO_REGISTER_DUPLICATE_DEFAULT);
        }

        [Test]
        public void I_can_say_to_Throw_on_new_default_registration_when_multi_keyed_registrations_present()
        {
            var container = new Container();
            container.Register<IService, Service>(named: 1, ifAlreadyRegistered: IfAlreadyRegistered.Throw);
            container.Register<IService, Service>(ifAlreadyRegistered: IfAlreadyRegistered.Throw);
            container.Register<IService, AnotherService>(named: 2, ifAlreadyRegistered: IfAlreadyRegistered.Throw);

            var ex = Assert.Throws<ContainerException>(() =>
            container.Register<IService, AnotherService>(ifAlreadyRegistered: IfAlreadyRegistered.Throw));

            Assert.AreEqual(ex.Error, Error.UNABLE_TO_REGISTER_DUPLICATE_DEFAULT);
        }

        [Test]
        public void Can_update_registered_default_with_new_default()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            var service = container.Resolve<IService>();

            Assert.That(service, Is.InstanceOf<AnotherService>());
        }

        [Test]
        public void Can_update_registered_named_with_new_named()
        {
            var container = new Container();
            container.Register<IService, Service>(named: EnumKey.Some);
            container.Register<IService, AnotherService>(named: EnumKey.Some, ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            var service = container.Resolve<IService>(EnumKey.Some);

            Assert.That(service, Is.InstanceOf<AnotherService>());
        }

        [Test]
        public void Can_update_latest_registered_default()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, DisposableService>();
            container.Register<IService, AnotherService>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            var services = container.Resolve<IService[]>();

            CollectionAssert.AreEqual(
                new[] { typeof(Service), typeof(AnotherService) },
                services.Select(service => service.GetType()));
        }
    }
}
