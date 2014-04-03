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

        [Test]
        public void When_registered_with_int_key_Then_could_be_resolved_as_default()
        {
            var container = new Container();
            container.Register<Service>(named: 1);

            var service = container.Resolve<Service>();

            Assert.IsNotNull(service);
        }

        [Test]
        public void When_registered_with_int_key_Then_could_be_resolved_with_that_key()
        {
            var container = new Container();
            container.Register<Service>(named: 1);

            var service = container.Resolve<Service>(1);

            Assert.IsNotNull(service);
        }

        [Test]
        public void When_registered_as_default_and_with_int_key_Then_resolving_as_default_should_Throw()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>(named: 1);

            Assert.Throws<ContainerException>(() =>
                container.Resolve<IService>());
        }

        [Test]
        public void When_registered_as_default_and_with_int_key_Then_resolving_with_int_key_should_succeed()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>(named: 1);

            var service = container.Resolve<IService>(1);

            Assert.That(service, Is.InstanceOf<AnotherService>());
        }

        [Test]
        public void When_registered_as_default_and_with_int_key_Then_resolving_default_with_int_key_should_succeed()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>(named: 1);

            var service = container.Resolve<IService>(0, IfUnresolved.Throw);

            Assert.That(service, Is.InstanceOf<Service>());
        }

        [Test]
        public void When_registered_with_negative_int_key_Then_could_be_resolved_with_that_key()
        {
            var container = new Container();
            container.Register<Service>(named: -1);

            var service = container.Resolve<Service>(-1);

            Assert.IsNotNull(service);
        }

        [Test]
        public void When_registered_with_negative_int_key_Then_could_be_resolved_default_key()
        {
            var container = new Container();
            container.Register<Service>(named: -1);

            var service = container.Resolve<Service>();

            Assert.IsNotNull(service);
        }

        [Test]
        public void Register_with_default_then_with_Zero_key_should_Throw()
        {
            var container = new Container();
            container.Register<IService, Service>();

            Assert.Throws<ContainerException>(() =>
                container.Register<IService, AnotherService>(named: 0));
        }

        [Test]
        public void Register_with_default_then_with_Zero_but_with_KeepRegistered_option_should_succeed()
        {
            var container = new Container();
            container.Register<IService, Service>();

            Assert.DoesNotThrow(() =>
                container.Register<IService, AnotherService>(named: 0, ifAlreadyRegistered: IfAlreadyRegistered.KeepAlreadyRegistered));
        }

        [Test]
        public void Register_with_the_same_int_key_should_Throw()
        {
            var container = new Container();
            container.Register<IService, Service>(named: 1);

            Assert.Throws<ContainerException>(() => 
                container.Register<IService, AnotherService>(named: 1));
        }
    }

    public enum ServiceColors { Red = 0, Green, Blue }
}
