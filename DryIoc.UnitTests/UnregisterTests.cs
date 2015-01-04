using System;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class UnregisterTests
    {
        [Test]
        public void Unregister_default_registration_should_Succeed()
        {
            var container = new Container();
            container.Register<IService, Service>();
            Assert.IsTrue(container.IsRegistered<IService>());

            container.Unregister(typeof(IService));

            Assert.IsFalse(container.IsRegistered<IService>());
        }

        [Test]
        public void Unregister_unregistered_default_registration_should_not_throw()
        {
            var container = new Container();
            container.Register<IService, Service>();

            container.Unregister(typeof(IService));
            container.Unregister(typeof(IService));

            Assert.IsFalse(container.IsRegistered<IService>());
        }

        [Test]
        public void Unregister_then_register_new_impl_Should_resolve_new_impl()
        {
            var container = new Container();

            container.Register<IC, C>();

            container.Unregister<IC>();

            container.Register<IC, C1>();

            var c1 = container.Resolve<IC>();
            Assert.That(c1, Is.InstanceOf<C1>());
        }

        [Test]
        public void Unregister_default_with_default_key_shoud_work()
        {
            var container = new Container();
            container.Register<IService, Service>();

            container.Unregister(typeof(IService), DefaultKey.Value);

            Assert.IsFalse(container.IsRegistered<IService>());
        }

        [Test]
        public void Unregister_unregistered_default_with_default_key_shoud_work()
        {
            var container = new Container();
            container.Register<IService, Service>();

            container.Unregister(typeof(IService), DefaultKey.Value);
            container.Unregister(typeof(IService), DefaultKey.Value);

            Assert.IsFalse(container.IsRegistered<IService>());
        }

        [Test]
        public void Unregister_named_registration_from_default_should_Fail()
        {
            var container = new Container();
            container.Register<IService, Service>();

            container.Unregister(typeof(IService), "named");

            Assert.IsTrue(container.IsRegistered<IService>());
        }

        [Test]
        public void Unregister_then_register_named_impl_Should_return_new_impl()
        {
            var container = new Container();
            container.Register<IService, Service>(named: "a");

            container.Unregister(typeof(IService), "a");

            container.Register<IService, AnotherService>(named: "a");

            var b = container.Resolve<IService>("a");

            Assert.That(b, Is.InstanceOf<AnotherService>());
        }

        [Test]
        public void Unregister_then_register_one_of_named_impl_Should_return_new_impl()
        {
            var container = new Container();
            container.Register<IService, Service>(named: "a");
            container.Register<IService, AnotherService>(named: "b");

            container.Unregister(typeof(IService), "b");

            container.Register<IService, Service>(named: "b");

            var b = container.Resolve<IService>("b");

            Assert.That(b, Is.InstanceOf<Service>());
        }

        [Test]
        public void Unregister_default_with_not_applied_remove_condition_should_Fail()
        {
            var container = new Container();
            container.Register<IService, Service>();

            container.Unregister(typeof(IService), condition: f => f.Reuse is SingletonReuse);

            Assert.IsTrue(container.IsRegistered<IService>());
        }

        [Test]
        public void Unregister_without_specific_default_key_will_remove_all_registered_defaults()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>();

            container.Unregister(typeof(IService));

            Assert.IsFalse(container.IsRegistered<IService>());
        }

        [Test]
        public void Unregister_default_registrations_with_condition_successful_for_one_of_two_will_keep_the_second_registration()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>();

            container.Unregister(typeof(IService), condition: f => f.ImplementationType == typeof(Service));

            Assert.IsFalse(container.IsRegistered<IService>(condition: f => f.ImplementationType == typeof(Service)));
            Assert.IsTrue(container.IsRegistered<IService>(condition: f => f.ImplementationType == typeof(AnotherService)));
        }

        [Test]
        public void Unregister_unregistered_default_registrations_with_condition_should_not_throw()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>();

            container.Unregister(typeof(IService), condition: f => f.ImplementationType == typeof(Service));
            container.Unregister(typeof(IService), condition: f => f.ImplementationType == typeof(Service));

            Assert.IsFalse(container.IsRegistered<IService>(condition: f => f.ImplementationType == typeof(Service)));
            Assert.IsTrue(container.IsRegistered<IService>(condition: f => f.ImplementationType == typeof(AnotherService)));
        }

        [Test]
        public void Unregister_last_default_registrations_with_condition_successful_for_one_of_two_will_keep_the_second_registration()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>();

            container.Unregister(typeof(IService), condition: f => f.ImplementationType == typeof(AnotherService));

            Assert.IsTrue(container.IsRegistered<IService>(condition: f => f.ImplementationType == typeof(Service)));
            Assert.IsFalse(container.IsRegistered<IService>(condition: f => f.ImplementationType == typeof(AnotherService)));
        }

        [Test]
        public void Unregister_unregistered_last_default_registrations_with_condition_should_not_throw()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>();

            container.Unregister(typeof(IService), condition: f => f.ImplementationType == typeof(AnotherService));
            container.Unregister(typeof(IService), condition: f => f.ImplementationType == typeof(AnotherService));

            Assert.IsTrue(container.IsRegistered<IService>(condition: f => f.ImplementationType == typeof(Service)));
            Assert.IsFalse(container.IsRegistered<IService>(condition: f => f.ImplementationType == typeof(AnotherService)));
        }

        [Test]
        public void Unregister_specific_default_from_multiple_defaults_and_named_should_Succeed()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>();
            container.Register<IService, DisposableService>(named: 2);

            var lastDefaultKey = DefaultKey.Value.Next();
            container.Unregister(typeof(IService), lastDefaultKey);

            Assert.IsFalse(container.IsRegistered(typeof(IService), lastDefaultKey));
            Assert.IsTrue(container.IsRegistered(typeof(IService), DefaultKey.Value));
            Assert.IsTrue(container.IsRegistered(typeof(IService), 2));
        }

        [Test]
        public void Unregister_unregistred_specific_default_from_multiple_defaults_and_named_should_not_throw()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>();
            container.Register<IService, DisposableService>(named: 2);

            var lastDefaultKey = DefaultKey.Value.Next();
            container.Unregister(typeof(IService), lastDefaultKey);
            container.Unregister(typeof(IService), lastDefaultKey);

            Assert.IsFalse(container.IsRegistered(typeof(IService), lastDefaultKey));
            Assert.IsTrue(container.IsRegistered(typeof(IService), DefaultKey.Value));
            Assert.IsTrue(container.IsRegistered(typeof(IService), 2));
        }

        [Test]
        public void Unregister_named_registration_should_Succeed()
        {
            var container = new Container();
            container.Register<IService, Service>(named: 1);

            container.Unregister(typeof(IService), named: 1);

            Assert.IsFalse(container.IsRegistered<IService>(named: 1));
        }

        [Test]
        public void Unregister_unregistered_named_registration_should_Succeed()
        {
            var container = new Container();
            container.Register<IService, Service>(named: 1);

            container.Unregister(typeof(IService), named: 1);
            container.Unregister(typeof(IService), named: 1);

            Assert.IsFalse(container.IsRegistered<IService>(named: 1));
        }

        [Test]
        public void Unregister_without_key_from_named_registration_should_remove_all_registrations()
        {
            var container = new Container();
            container.Register<IService, Service>(named: 'a');

            container.Unregister<IService>();

            Assert.That(container.IsRegistered<IService>(named: 'a'), Is.False);
            Assert.That(container.IsRegistered<IService>(), Is.False);
        }

        [Test]
        public void Unregister_named_from_default_and_named_default_should_keep_default()
        {
            var container = new Container();
            container.Register<IService, Service>(named: 'a');
            container.Register<IService, AnotherService>();

            container.Unregister<IService>(named: 'a');

            Assert.That(container.IsRegistered<IService>(named: 'a'), Is.False);
            Assert.That(container.IsRegistered<IService>(DefaultKey.Value), Is.True);
        }

        [Test]
        public void Unregister_decorator_should_keep_decorated_service()
        {
            var container = new Container();
            container.Register<IHandler, FastHandler>();
            container.Register<IHandler, LoggingHandlerDecorator>(setup: SetupDecorator.Default);

            container.Unregister<IHandler>(factoryType: FactoryType.Decorator);

            Assert.IsFalse(container.IsRegistered<IHandler>(factoryType: FactoryType.Decorator));
            Assert.IsTrue(container.IsRegistered<IHandler>());
        }

        [Test]
        public void Unregister_unregistred_decorator_should_not_throw()
        {
            var container = new Container();
            container.Register<IHandler, LoggingHandlerDecorator>(setup: SetupDecorator.Default);

            container.Unregister<IHandler>(factoryType: FactoryType.Decorator, condition: f => f.ImplementationType == typeof(LoggingHandlerDecorator));
            container.Unregister<IHandler>(factoryType: FactoryType.Decorator, condition: f => f.ImplementationType == typeof(LoggingHandlerDecorator));

            Assert.IsFalse(container.IsRegistered<IHandler>(factoryType: FactoryType.Decorator));
        }

        [Test]
        public void Unregister_decorator_with_condition_should_keep_decorator_for_which_condition_is_failed()
        {
            var container = new Container();
            container.Register<IHandler, FastHandler>();
            container.Register<IHandler, LoggingHandlerDecorator>(setup: SetupDecorator.Default);
            container.Register<IHandler, NullHandlerDecorator>(setup: SetupDecorator.Default);

            container.Unregister<IHandler>(factoryType: FactoryType.Decorator,
                condition: f => f.ImplementationType == typeof(NullHandlerDecorator));

            Assert.IsFalse(container.IsRegistered<IHandler>(factoryType: FactoryType.Decorator,
                condition: f => f.ImplementationType == typeof(NullHandlerDecorator)));
            Assert.IsTrue(container.IsRegistered<IHandler>(factoryType: FactoryType.Decorator,
                condition: f => f.ImplementationType == typeof(LoggingHandlerDecorator)));
            Assert.IsTrue(container.IsRegistered<IHandler>());
        }

        [Test]
        public void Unregister_generic_wrapper_is_possible()
        {
            var container = new Container();
            Assert.IsTrue(container.IsRegistered(typeof(Lazy<>), factoryType: FactoryType.Wrapper));
            
            container.Unregister(typeof(Lazy<>), factoryType: FactoryType.Wrapper);
            Assert.IsFalse(container.IsRegistered(typeof(Lazy<>), factoryType: FactoryType.Wrapper));
        }

        [Test]
        public void Unregister_of_unregistered_generic_wrapper_should_not_throw()
        {
            var container = new Container();
            Assert.IsTrue(container.IsRegistered(typeof(Lazy<>), factoryType: FactoryType.Wrapper));

            container.Unregister(typeof(Lazy<>), factoryType: FactoryType.Wrapper);
            container.Unregister(typeof(Lazy<>), factoryType: FactoryType.Wrapper);
            Assert.IsFalse(container.IsRegistered(typeof(Lazy<>), factoryType: FactoryType.Wrapper));
        }

        [Test]
        public void Unregister_generic_wrapper_with_condition_is_possible()
        {
            var container = new Container();
            Assert.IsTrue(container.IsRegistered(typeof(Lazy<>), factoryType: FactoryType.Wrapper));

            container.Unregister(typeof(Lazy<>), factoryType: FactoryType.Wrapper,
                condition: f => f.ImplementationType == typeof(Func<>));

            Assert.IsTrue(container.IsRegistered(typeof(Lazy<>), factoryType: FactoryType.Wrapper));
        }

        internal class NullHandlerDecorator : IHandler { }

        internal interface IC {}
        internal class C : IC {}
        internal class C1 : IC {}
    }
}
