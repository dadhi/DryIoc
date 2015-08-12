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
            container.Register<IService, AnotherService>(serviceKey: "another");

            var service = container.Resolve<IService>();

            Assert.That(service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Given_named_and_default_registerations_Resolving_with_name_should_return_correspondingly_named_service()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>(serviceKey: "another");

            var service = container.Resolve<IService>("another");

            Assert.That(service, Is.InstanceOf<AnotherService>());
        }

        [Test]
        public void Given_two_named_registrations_Resolving_without_name_should_throw()
        {
            var container = new Container();
            container.Register<IService, Service>(serviceKey: "some");
            container.Register<IService, Service>(serviceKey: "another");

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<IService>());

            Assert.AreEqual(ex.Error, Error.UnableToResolveFromRegisteredServices);
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
        public void Resolving_non_registered_service_should_Throw()
        {
            var container = new Container();

            Assert.Throws<ContainerException>(() =>
                container.Resolve<IDependency>());
        }

        [Test]
        public void Registering_with_interface_for_service_implementation_should_Throw()
        {
            var container = new Container();

            Assert.Throws<ContainerException>(() =>
                container.Register(typeof(IDependency), typeof(IDependency)));
        }

        [Test]
        public void Registering_impl_type_without_public_constructor_and_without_constructor_selector_should_throw()
        {
            var container = new Container();

            Assert.Throws<ContainerException>(() =>
               container.Register(typeof(ServiceWithoutPublicConstructor)));
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
        public void IsRegistered_for_registered_service_should_return_true()
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
        public void IsRegistered_Should_return_false_for_concrete_generic_In_case_of_only_open_generic_registered_()
        {
            var container = new Container();
            container.Register(typeof(ServiceWithTwoGenericParameters<,>));

            Assert.IsFalse(container.IsRegistered<ServiceWithTwoGenericParameters<int, string>>());
            Assert.IsTrue(container.IsRegistered(typeof(ServiceWithTwoGenericParameters<,>)));
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
            container.Register(typeof(IService), typeof(Service), serviceKey: "blah");

            var ex = Assert.Throws<ContainerException>(() =>
                container.Register(typeof(IService), typeof(AnotherService), serviceKey: "blah"));

            Assert.That(ex.Message, Is.StringContaining("with duplicate key [blah]"));
        }

        [Test]
        public void Given_multiple_defaults_registered_Resolving_one_should_throw()
        {
            var container = new Container();

            container.Register(typeof(IService), typeof(Service));
            container.Register(typeof(IService), typeof(AnotherService));

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve(typeof(IService)));

            Assert.AreEqual(Error.ExpectedSingleDefaultFactory, ex.Error);
        }

        [Test]
        public void Possible_to_register_and_resolve_with_object_service_type()
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

            var service = container.Resolve<IService>(IfUnresolved.ReturnDefault);

            Assert.Null(service);
        }

        [Test]
        public void Register_once_for_default_service()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>(ifAlreadyRegistered: IfAlreadyRegistered.Keep);

            var service = container.Resolve<IService>();

            Assert.That(service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Register_once_for_default_service_Should_not_be_affected_by_already_registered_named_services()
        {
            var container = new Container();
            container.Register<IService, Service>(serviceKey: "a");
            container.Register<IService, AnotherService>(ifAlreadyRegistered: IfAlreadyRegistered.Keep);

            var service = container.Resolve<IService>();

            Assert.That(service, Is.InstanceOf<AnotherService>());
        }

        [Test]
        public void Register_once_for_default_service_when_couple_of_defaults_were_already_registered()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>();
            container.Register<IService, DisposableService>(ifAlreadyRegistered: IfAlreadyRegistered.Keep);

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<IService>());

            Assert.AreEqual(Error.ExpectedSingleDefaultFactory, ex.Error);
        }

        [Test]
        public void Register_once_for_named_service()
        {
            var container = new Container();
            container.Register<IService, Service>(serviceKey: "a");
            container.Register<IService, AnotherService>(serviceKey: "a", ifAlreadyRegistered: IfAlreadyRegistered.Keep);

            var service = container.Resolve<IService>("a");

            Assert.That(service, Is.InstanceOf<Service>());
        }

        [Test]
        [Description("https://bitbucket.org/dadhi/dryioc/issue/73/remove-reused-instance-when-unregister")]
        public void Unregister_singleton_without_swappable()
        {
            var container = new Container();

            container.Register<IContext, Context1>(Reuse.Singleton);

            var context = container.Resolve<IContext>();
            Assert.NotNull(context);
            Assert.AreSame(context, container.Resolve<IContext>());

            // Removes service instance from Singleton scope by setting it to null.
            container.RegisterInstance<IContext>(null, ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            // Removes service registration.
            container.Unregister<IContext>();

            var ex = Assert.Throws<ContainerException>(() => container.Resolve<IContext>());
            Assert.AreEqual(Error.UnableToResolveUnknownService, ex.Error);
        }

        [Test]
        [Description("https://github.com/ashmind/net-feature-tests/issues/23")]
        public void ReRegister_singleton_without_recycleable()
        {
            var container = new Container();
            // before request
            container.Register<IContext, Context1>(Reuse.Singleton);

            var r1 = container.Resolve<IContext>();
            r1.Data = "before";

            container.Register<IContext, Context2>(Reuse.Singleton,
                ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            var r2 = container.Resolve<IContext>();
            var r3 = container.Resolve<IContext>();
            Assert.AreNotEqual(r1, r2);
            Assert.AreEqual(r2, r3);
            Assert.AreEqual(null, r2.Data);
        }

        [Test]
        public void ReRegister_transient_with_key()
        {
            var c = new Container();
            c.Register<ILogger, Logger1>(serviceKey: "a");
            Assert.IsInstanceOf<Logger1>(c.Resolve<ILogger>("a"));

            c.Register<ILogger, Logger2>(serviceKey: "a", ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            Assert.IsInstanceOf<Logger2>(c.Resolve<ILogger>("a"));
        }

        [Test]
        [Description("https://github.com/ashmind/net-feature-tests/issues/23")]
        public void ReRegister_dependency_of_transient()
        {
            var c = new Container();
            c.Register<ILogger, Logger1>(setup: Setup.With(openResolutionScope: true));
            
            c.Register<UseLogger1>();
            var user = c.Resolve<UseLogger1>();
            Assert.IsInstanceOf<Logger1>(user.Logger);

            c.Register<ILogger, Logger2>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            user = c.Resolve<UseLogger1>();
            Assert.IsInstanceOf<Logger2>(user.Logger);
        }

        [Test]
        [Description("https://github.com/ashmind/net-feature-tests/issues/23")]
        public void ReRegister_dependency_of_singleton_without_recyclable()
        {
            var c = new Container();

            // If we know that Logger could be changed/re-registered, then register it as dynamic dependency
            c.Register<ILogger, Logger1>(setup: Setup.With(openResolutionScope: true));
            c.Register<UseLogger1>(Reuse.Singleton);
            var user1 = c.Resolve<UseLogger1>();

            c.Register<ILogger, Logger2>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            // It is a already resolved singleton,
            // so it should not be affected by new ILogger registration.
            
            var user12 = c.Resolve<UseLogger1>();
            Assert.AreSame(user1, user12);
            Assert.IsInstanceOf<Logger1>(user12.Logger);

            // To change that, you you need reregister the singleton to re-create it with new ILogger.
            c.Register<UseLogger1>(Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            var u13 = c.Resolve<UseLogger1>();
            Assert.AreNotSame(user12, u13);
            Assert.IsInstanceOf<Logger2>(u13.Logger);

            c.Register<UseLogger2>(Reuse.Singleton); // uses ILogger
            c.Resolve<UseLogger2>();
        }

        [Test]
        public void IResolver_will_be_injected_as_property_even_if_not_registered()
        {
            var container = new Container();
            container.Register<Beh>(made: PropertiesAndFields.Auto);

            var resolver = container.Resolve<Beh>().R;

            Assert.NotNull(resolver);
        }

        [Test]
        public void Should_Throw_if_implementation_is_not_assignable_to_service_type()
        {
            var container = new Container();

            var ex = Assert.Throws<ContainerException>(() => 
            container.Register(typeof(IService), typeof(Beh)));

            Assert.AreEqual(Error.RegisterImplementationNotAssignableToServiceType, ex.Error);
        }

        public class Beh
        {
            public IResolver R { get; set; }
        }
    }
}
