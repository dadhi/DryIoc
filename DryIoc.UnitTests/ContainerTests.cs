using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class ContainerTests
    {
        [Test]
        public void Resolving_service_should_return_registered_impelementation()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service));

            var service = container.Resolve(typeof(IService));

            Assert.That(service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Given_named_and_default_registerations_Resolving_without_name_returns_default()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>(named: "another");

            var service = container.Resolve<IService>();

            Assert.That(service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Given_named_and_default_registerations_Resolving_with_name_should_return_correspondingly_named_service()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>(named: "another");

            var service = container.Resolve<IService>("another");

            Assert.That(service, Is.InstanceOf<AnotherService>());
        }

        [Test]
        public void Given_two_named_registerations_Resolving_without_name_should_throw()
        {
            var container = new Container();
            container.Register<IService, Service>(named: "some");
            container.Register<IService, Service>(named: "another");

            Assert.Throws<ContainerException>(() =>
                container.Resolve<IService>());
        }

        [Test]
        public void Resolving_singleton_twice_should_return_same_instances()
        {
            var container = new Container();
            container.Register(typeof(ISingleton), typeof(Singleton), Reuse.Singleton);

            var one = container.Resolve(typeof(ISingleton));
            var another = container.Resolve(typeof(ISingleton));

            Assert.AreEqual(one, another);
        }

        [Test]
        public void Resolving_non_registered_service_should_throw()
        {
            var container = new Container();

            Assert.Throws<ContainerException>(() =>
                container.Resolve<IDependency>());
        }

        [Test]
        public void Registering_with_interface_for_service_implementation_should_throw()
        {
            var container = new Container();

            Assert.Throws<ContainerException>(() =>
                container.Register(typeof(IDependency), typeof(IDependency)));
        }

        [Test]
        public void Given_no_constructor_selector_specified_in_registration_Resolving_implementation_with_multiple_constructors_should_throw()
        {
            var container = new Container();
            container.Register<ServiceWithMultipleCostructors>();

            Assert.Throws<ContainerException>(() =>
                container.Resolve<ServiceWithMultipleCostructors>());
        }

        [Test]
        public void Given_registered_service_Injecting_it_as_dependency_should_work()
        {
            var container = new Container();
            container.Register(typeof(IDependency), typeof(Dependency));
            container.Register(typeof(ServiceWithDependency));

            var service = container.Resolve<ServiceWithDependency>();

            Assert.That(service.Dependency, Is.Not.Null);
        }

        [Test]
        public void Resolving_service_with_NON_registered_dependency_should_throw()
        {
            var container = new Container();
            container.Register(typeof(ServiceWithDependency));

            Assert.Throws<ContainerException>(() =>
                container.Resolve<ServiceWithDependency>());
        }

        [Test]
        public void Resolving_service_with_recursive_dependency_should_throw()
        {
            var container = new Container();
            container.Register(typeof(IDependency), typeof(FooWithDependency));
            container.Register(typeof(IService), typeof(ServiceWithRecursiveDependency));

            Assert.Throws<ContainerException>(() =>
                container.Resolve<IService>());
        }

        [Test]
        public void Given_two_resolved_service_instances_Injected_singleton_dependency_should_be_the_same_in_both()
        {
            var container = new Container();
            container.Register<ISingleton, Singleton>(Reuse.Singleton);
            container.Register<ServiceWithSingletonDependency>();

            var one = container.Resolve<ServiceWithSingletonDependency>();
            var another = container.Resolve<ServiceWithSingletonDependency>();

            Assert.That(one.Singleton, Is.SameAs(another.Singleton));
        }

        [Test]
        public void Given_open_generic_registration_When_resolving_two_generic_instances_Injected_singleton_dependency_should_be_the_same_in_both()
        {
            var container = new Container();
            container.Register<IDependency, Dependency>(Reuse.Singleton);
            container.Register(typeof(ServiceWithGenericDependency<>));

            var one = container.Resolve<ServiceWithGenericDependency<IDependency>>();
            var another = container.Resolve<ServiceWithGenericDependency<IDependency>>();

            Assert.That(one.Dependency, Is.SameAs(another.Dependency));
        }

        [Test]
        public void When_resolving_service_with_two_dependencies_dependent_on_singleton_Then_same_singleton_instance_should_be_used()
        {
            var container = new Container();
            container.Register(typeof(ServiceWithTwoParametersBothDependentOnSameService));
            container.Register(typeof(ServiceWithDependency));
            container.Register(typeof(AnotherServiceWithDependency));
            container.Register(typeof(IDependency), typeof(Dependency), Reuse.Singleton);

            var service = container.Resolve<ServiceWithTwoParametersBothDependentOnSameService>();

            Assert.That(service.One.Dependency, Is.SameAs(service.Another.Dependency));
        }

        [Test]
        public void When_resolving_service_with_two_dependencies_dependent_on_Lazy_singleton_Then_same_singleton_instance_should_be_used()
        {
            var container = new Container();
            container.Register(typeof(ServiceWithTwoDependenciesWithLazySingletonDependency));
            container.Register(typeof(ServiceWithLazyDependency));
            container.Register(typeof(AnotherServiceWithLazyDependency));
            container.Register(typeof(IDependency), typeof(Dependency), Reuse.Singleton);

            var service = container.Resolve<ServiceWithTwoDependenciesWithLazySingletonDependency>();

            Assert.That(service.One.LazyOne.Value, Is.SameAs(service.Another.LazyOne.Value));
        }

        [Test]
        public void IsRegistered_for_registered_service_should_return_true ()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service));

            var isRegistered = container.IsRegistered(typeof(IService));

            Assert.That(isRegistered, Is.True);
        }

        [Test]
        public void IsRegistered_for_NON_registered_service_should_return_false()
        {
            var container = new Container();

            var isRegistered = container.IsRegistered(typeof(IService));

            Assert.That(isRegistered, Is.False);
        }

        [Test]
        public void Given_open_generic_is_registered_IsRegistered_for_closed_generic_should_return_true()
        {
            var container = new Container();
            container.Register(typeof(ServiceWithTwoGenericParameters<,>));

            var registered = container.IsRegistered<ServiceWithTwoGenericParameters<int, string>>();

            Assert.That(registered, Is.True);
        }

        [Test]
        public void Registering_second_default_implementation_should_not_throw()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service));

            Assert.DoesNotThrow(() =>
                container.Register(typeof(IService), typeof(AnotherService)));
        }

        [Test]
        public void Registering_service_with_duplicate_name_should_throw()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service), named: "blah");

            Assert.Throws<ContainerException>(
                () => container.Register(typeof(IService), typeof(AnotherService), named: "blah"));
        }

        [Test]
        public void Given_multiple_defaults_registered_Resolving_one_should_throw()
        {
            var container = new Container(ContainerSetup.Minimal);

            container.Register(typeof(IService), typeof(Service));
            container.Register(typeof(IService), typeof(AnotherService));

            Assert.Throws<ContainerException>(() =>
                container.Resolve(typeof(IService)));
        }

        [Test]
        public void Resolving_service_without_public_constructor_should_throw()
        {
            var container = new Container();

            container.Register(typeof(ServiceWithoutPublicConstructor));

            Assert.Throws<ContainerException>(
                () => container.Resolve<ServiceWithoutPublicConstructor>());
        }

        [Test]
        public void Possible_to_register_and_resolve_object_as_service_type()
        {
            var container = new Container();
            container.Register<object, Service>();

            var service = container.Resolve<object>();

            Assert.That(service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Possible_ask_to_return_null_if_service_is_unresolved_instead_of_throwing_an_error()
        {
            var container = new Container();

            var service = container.Resolve<IService>(IfUnresolved.ReturnNull);

            Assert.Null(service);
        }
    }
}
