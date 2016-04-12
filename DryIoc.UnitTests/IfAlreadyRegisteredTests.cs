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
            container.Register<I, X>();
            container.Register<I, Y>();

            var services = container.Resolve<I[]>();

            CollectionAssert.AreEqual(
                new[] {typeof(X), typeof(Y)},
                services.Select(s => s.GetType()).ToArray());
        }

        public interface I {}

        public class X : I {}

        public class Y : I {}

        [Test]
        public void I_can_say_to_Throw_on_new_default_registration()
        {
            var container = new Container();
            container.Register<IService, Service>(ifAlreadyRegistered: IfAlreadyRegistered.Throw);

            var ex = Assert.Throws<ContainerException>(() =>
                container.Register<IService, AnotherService>(ifAlreadyRegistered: IfAlreadyRegistered.Throw));

            Assert.AreEqual(ex.Error, Error.UnableToRegisterDuplicateDefault);
        }

        [Test]
        public void I_can_say_to_Throw_on_new_default_registration_when_multi_keyed_registrations_present()
        {
            var container = new Container();
            container.Register<IService, Service>(serviceKey: 1, ifAlreadyRegistered: IfAlreadyRegistered.Throw);
            container.Register<IService, Service>(ifAlreadyRegistered: IfAlreadyRegistered.Throw);
            container.Register<IService, AnotherService>(serviceKey: 2, ifAlreadyRegistered: IfAlreadyRegistered.Throw);

            var ex = Assert.Throws<ContainerException>(() =>
                container.Register<IService, AnotherService>(ifAlreadyRegistered: IfAlreadyRegistered.Throw));

            Assert.AreEqual(ex.Error, Error.UnableToRegisterDuplicateDefault);
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
            container.Register<IService, Service>(serviceKey: EnumKey.Some);
            container.Register<IService, AnotherService>(serviceKey: EnumKey.Some,
                ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            var service = container.Resolve<IService>(EnumKey.Some);

            Assert.That(service, Is.InstanceOf<AnotherService>());
        }

        [Test]
        public void The_replace_will_replace_all_previous_service_registrations()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, OneService>();
            container.Register<IService, AnotherService>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            var services = container.Resolve<IService[]>();

            Assert.AreEqual(1, services.Length);
            Assert.IsInstanceOf<AnotherService>(services[0]);
        }

        [Test]
        public void Can_register_distinct_implementations()
        {
            var container = new Container();
            container.Register<I, X>(ifAlreadyRegistered: IfAlreadyRegistered.AppendNewImplementation);
            container.Register<I, Y>(ifAlreadyRegistered: IfAlreadyRegistered.AppendNewImplementation);
            container.Register<I, Y>(ifAlreadyRegistered: IfAlreadyRegistered.AppendNewImplementation);

            var ies = container.Resolve<I[]>();

            Assert.AreEqual(2, ies.Length);
        }

        [Test]
        public void RegisterMany_services_of_single_implementation_with_AppendNewImplementation_option()
        {
            var container = new Container();
            container.RegisterMany<M>(ifAlreadyRegistered: IfAlreadyRegistered.AppendNewImplementation);

            Assert.IsNotNull(container.Resolve<IM>());
            Assert.IsNotNull(container.Resolve<IN>());
        }

        [Test]
        public void RegisterMany_services_of_single_implementation_with_Replace_option()
        {
            var container = new Container();
            container.RegisterMany<M>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            Assert.IsNotNull(container.Resolve<IM>());
            Assert.IsNotNull(container.Resolve<IN>());
        }

        [Test]
        public void RegisterMany_for_each_of_many_implementations_with_Replace_option()
        {
            var container = new Container();
            container.RegisterMany<M>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            container.RegisterMany<N>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            Assert.IsInstanceOf<N>(container.Resolve<IM>());
            Assert.IsInstanceOf<N>(container.Resolve<IN>());
        }

        [Test]
        public void RegisterMany_many_implementations_with_Replace_option()
        {
            var container = new Container();
            container.RegisterMany(new[] { typeof(M), typeof(N) }, 
                ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            Assert.IsInstanceOf<N>(container.Resolve<IM>());
            Assert.IsInstanceOf<N>(container.Resolve<IN>());
        }

        public interface IM {}
        public interface IN {}

        public class M : IM, IN {}
        public class N : IM, IN {}
    }
}
