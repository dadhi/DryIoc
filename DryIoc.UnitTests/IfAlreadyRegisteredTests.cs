using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class IfAlreadyRegisteredTests
    {
        [Test]
        public void Can_replace_registered_default_with_new_default()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>(ifAlreadyRegistered: IfAlreadyRegistered.ReplaceRegistered);

            var service = container.Resolve<IService>();

            Assert.That(service, Is.InstanceOf<AnotherService>());
        }

        [Test]
        public void Can_replace_registered_indexed_default_with_new_default()
        {
            var container = new Container();
            container.Register<IService, Service>(named: 0);
            container.Register<IService, AnotherService>(ifAlreadyRegistered: IfAlreadyRegistered.ReplaceRegistered);

            var service = container.Resolve<IService>();

            Assert.That(service, Is.InstanceOf<AnotherService>());
        }

        [Test]
        public void Can_replace_registered_default_with_new_indexed_default()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>(named: 0, ifAlreadyRegistered: IfAlreadyRegistered.ReplaceRegistered);

            var service = container.Resolve<IService>();

            Assert.That(service, Is.InstanceOf<AnotherService>());
        }

        [Test]
        public void Can_replace_registered_named_with_new_named()
        {
            var container = new Container();
            container.Register<IService, Service>(named: EnumKey.Some);
            container.Register<IService, AnotherService>(named: EnumKey.Some, ifAlreadyRegistered: IfAlreadyRegistered.ReplaceRegistered);

            var service = container.Resolve<IService>(EnumKey.Some);

            Assert.That(service, Is.InstanceOf<AnotherService>());
        }
    }
}
