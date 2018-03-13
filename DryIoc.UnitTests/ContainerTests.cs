using System.Linq;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;
using ImTools;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class ContainerTests
    {
        [Test]
        public void Resolving_service_should_return_registered_implementation()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service));

            var service = container.Resolve(typeof(IService));

            Assert.IsInstanceOf<Service>(service);
        }

        [Test]
        public void Given_named_and_default_registrations_Resolving_without_name_returns_default()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>(serviceKey: "another");

            var service = container.Resolve<IService>();

            Assert.IsInstanceOf<Service>(service);
        }

        [Test]
        public void Given_named_and_default_registrations_Resolving_with_name_should_return_correspondingly_named_service()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>(serviceKey: "another");

            var service = container.Resolve<IService>("another");

            Assert.IsInstanceOf<AnotherService>(service);
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

            Assert.IsNotNull(service.Dependency);
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

            Assert.AreSame(one.Singleton, another.Singleton);
        }

        [Test]
        public void Given_open_generic_registration_When_resolving_two_generic_instances_Injected_singleton_dependency_should_be_the_same_in_both()
        {
            var container = new Container();
            container.Register<IDependency, Dependency>(Reuse.Singleton);
            container.Register(typeof(ServiceWithGenericDependency<>));

            var one = container.Resolve<ServiceWithGenericDependency<IDependency>>();
            var another = container.Resolve<ServiceWithGenericDependency<IDependency>>();

            Assert.AreSame(one.Dependency, another.Dependency);
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

            Assert.AreSame(service.One.Dependency, service.Another.Dependency);
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

            Assert.AreSame(service.One.LazyOne.Value, service.Another.LazyOne.Value);
        }

        [Test]
        public void IsRegistered_for_registered_service_should_return_true()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service));

            var isRegistered = container.IsRegistered(typeof(IService));

            Assert.IsTrue(isRegistered);
        }

        [Test]
        public void IsRegistered_for_NON_registered_service_should_return_false()
        {
            var container = new Container();

            var isRegistered = container.IsRegistered(typeof(IService));

            Assert.IsFalse(isRegistered);
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

            Assert.AreEqual(Error.UnableToRegisterDuplicateKey, ex.Error);
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

            Assert.IsInstanceOf<Service>(service);
        }

        [Test]
        public void Possible_ask_to_return_null_if_service_is_unresolved_instead_of_throwing_an_error()
        {
            var container = new Container();

            var service = container.Resolve<IService>(IfUnresolved.ReturnDefault);

            Assert.IsNull(service);
        }

        [Test]
        public void Register_once_for_default_service()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>(ifAlreadyRegistered: IfAlreadyRegistered.Keep);

            var service = container.Resolve<IService>();

            Assert.IsInstanceOf<Service>(service);
        }

        [Test]
        public void Register_once_for_default_service_Should_not_be_affected_by_already_registered_named_services()
        {
            var container = new Container();
            container.Register<IService, Service>(serviceKey: "a");
            container.Register<IService, AnotherService>(ifAlreadyRegistered: IfAlreadyRegistered.Keep);

            var service = container.Resolve<IService>();

            Assert.IsInstanceOf<AnotherService>(service);
        }

        [Test]
        public void Register_once_for_default_service_when_couple_of_defaults_were_already_registered()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>();
            container.Register<IService, OneService>(ifAlreadyRegistered: IfAlreadyRegistered.Keep);

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

            Assert.IsInstanceOf<Service>(service);
        }

        [Test]
        //[Description("https://bitbucket.org/dadhi/dryioc/issue/73/remove-reused-instance-when-unregister")]
        public void Unregister_singleton_without_swappable()
        {
            var container = new Container();

            container.Register<IContext, Context1>(Reuse.Singleton);

            var context = container.Resolve<IContext>();
            Assert.IsNotNull(context);
            Assert.AreSame(context, container.Resolve<IContext>());

            // Removes service instance from Singleton scope by setting it to null.
            container.UseInstance<IContext>(null);

            // Removes service registration.
            container.Unregister<IContext>();

            var ex = Assert.Throws<ContainerException>(() => container.Resolve<IContext>());
            Assert.AreEqual(Error.UnableToResolveUnknownService, ex.Error);
        }

        [Test]
        //[Description("https://github.com/ashmind/net-feature-tests/issues/23")]
        public void ReRegister_singleton_without_recyclable()
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
        //[Description("https://github.com/ashmind/net-feature-tests/issues/23")]
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
        //[Description("https://github.com/ashmind/net-feature-tests/issues/23")]
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

            // To change that, you you need re-register the singleton to re-create it with new ILogger.
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

            Assert.IsNotNull(resolver);
        }

        [Test]
        public void IContainer_will_be_injected_even_if_not_registered()
        {
            var container = new Container();
            container.Register<Beh>(made: PropertiesAndFields.Auto);

            var c = container.Resolve<Beh>().C;

            Assert.AreSame(container, c);
        }

        [Test]
        public void Container_interfaces_can_be_resolved_as_normal_services_in_scope()
        {
            var container = new Container();
            using (var scope = container.OpenScope())
            {
                Assert.AreSame(scope, scope.Resolve<IContainer>());
                Assert.AreSame(scope, scope.Resolve<IRegistrator>());
                Assert.AreSame(scope, scope.Resolve<IResolver>());
            }
        }

        [Test]
        public void IRegistrator_will_be_injected_even_if_not_registered()
        {
            var container = new Container();
            container.Register<Beh>(made: PropertiesAndFields.Auto);

            var reg = container.Resolve<Beh>().Reg;

            Assert.AreSame(container, reg);
        }

        [Test]
        public void Given_open_scope_the_scoped_IContainer_will_be_injected_even_if_not_registered()
        {
            var container = new Container();
            container.Register<Beh>(made: PropertiesAndFields.Auto);

            using (var scoped = container.OpenScope())
            {
                var c = scoped.Resolve<Beh>().C;
                Assert.AreSame(scoped, c);
            }

            Assert.AreSame(container, container.Resolve<Beh>().C);
        }

        [Test]
        public void Should_Throw_if_implementation_is_not_assignable_to_service_type()
        {
            var container = new Container();

            var ex = Assert.Throws<ContainerException>(() => 
            container.Register(typeof(IService), typeof(Beh)));

            Assert.AreEqual(Error.RegisteringImplementationNotAssignableToServiceType, ex.Error);
        }

        public class Beh
        {
            public IResolver R { get; set; }

            public IContainer C { get; set; }

            public IContainer Reg { get; set; }
        }

        [Test]
        public void In_ArrayTools_Remove_from_null_or_empty_array_should_return_null_or_original_array()
        {
            int[] ar = null;
            Assert.AreSame(ar, ar.RemoveAt(0));

            ar = new int[0];
            Assert.AreSame(ar, ar.RemoveAt(0));
        }

        [Test]
        public void In_ArrayTools_Remove_items_from_array_should_produce_another_array_without_removed_item()
        {
            var ar = new[] {2, 1};

            Assert.AreSame(ar, ar.RemoveAt(-1));
            CollectionAssert.AreEqual(new[] { 1 }, ar.RemoveAt(0));
            CollectionAssert.AreEqual(new[] { 2 }, ar.RemoveAt(1));
        }

        [Test]
        public void Disposed_container_should_throw_on_attempt_to_register()
        {
            var container = new Container();
            container.Dispose();

            var ex = Assert.Throws<ContainerException>(() => 
                container.UseInstance("a"));

            Assert.AreEqual(
                Error.NameOf(Error.ContainerIsDisposed),
                Error.NameOf(ex.Error));
        }

        [Test]
        public void The_container_with_disposed_singleton_should_be_marked_as_Disposed()
        {
            var container = new Container();
            container.Register<Abc>(Reuse.Singleton);
            container.Resolve<Abc>(); // creates and stores singleton

            var containerWithConcreteTypes = container.With(rules => rules
                .WithConcreteTypeDynamicRegistrations());

            containerWithConcreteTypes.Dispose();
            Assert.IsTrue(((Container)containerWithConcreteTypes).IsDisposed);

            Assert.IsTrue(container.IsDisposed);
            container.Dispose();
        }

        public class Abc { }

        [Test]
        public void Can_Validate_the_registrations_to_find_potential_errors_in_their_resolution()
        {
            var container = new Container();

            container.Register<Me.MyService>();

            var errors = container.Validate();
            Assert.AreEqual(1, errors.Length);
            Assert.AreEqual(Error.UnableToResolveUnknownService, errors[0].Value.Error);
        }

        [Test]
        public void Can_Validate_the_used_instance_dependency()
        {
            var container = new Container();

            container.Register<AA>();
            container.UseInstance("bb");

            var errors = container.Validate();
            Assert.AreEqual(0, errors.Length);
        }

        public class AA
        {
            public AA(string msg) { }
        }

        [Test]
        public void Can_generate_expressions_from_many_open_generic_registrations()
        {
            var c = new Container();

            c.RegisterMany(new[] { typeof(OG1<>), typeof(OG2<>) }, serviceTypeCondition: Registrator.Interfaces);

            var result = c.GenerateResolutionExpressions(regs => 
                regs.Select(r => r.ServiceType == typeof(IG<>) ? r.ToServiceInfo<IG<int>>() : r.ToServiceInfo()));

            Assert.IsEmpty(result.Errors);
            CollectionAssert.AreEquivalent(
                new[] { typeof(OG1<int>), typeof(OG2<int>) }, 
                result.Roots.Select(e => e.Value.Body.Type));
        }

        public interface IG<out T> { }
        public class OG1<T> : IG<T> { }
        public class OG2<T> : IG<T> { }

        public class CS: IG<string> { }

        public class Aa { }
        public class Bb : Aa { }

        public class OgAa : IG<Aa> { }
        public class OgBb : IG<Bb> { }

        [Test]
        public void Resolving_covariant_collection()
        {
            var c = new Container();

            c.Register<IG<Aa>, OgAa>();
            c.Register<IG<Bb>, OgBb>();

            var igs = c.Resolve<IG<Aa>[]>();

            Assert.AreEqual(2, igs.Length);
        }

        [Test]
        public void Resolving_closed_generic_with_wrong_type()
        {
            var c = new Container();
            c.RegisterMany(new[] { typeof(CS) }, serviceTypeCondition: Registrator.Interfaces);

            Assert.Throws<ContainerException>(() =>
            c.Resolve<IG<int>>(typeof(CS)));
        }

        [Test]
        public void Can_generate_expressions_from_closed_and_open_generic_registrations_via_required_service_type()
        {
            var c = new Container();

            c.RegisterMany(new[] { typeof(CS), typeof(OG1<>) }, serviceTypeCondition: Registrator.Interfaces);

            var result = c.GenerateResolutionExpressions(regs =>
                regs.Select(r => r.ServiceType == typeof(IG<>) ? r.ToServiceInfo<IG<string>>() : r.ToServiceInfo()));

            Assert.IsEmpty(result.Errors);
            CollectionAssert.AreEquivalent(
                new[] { typeof(CS), typeof(OG1<string>) },
                result.Roots.Select(e => e.Value.Body.Type));
        }

        [Test]
        public void Container_ToString_should_output_scope_info_for_open_scope()
        {
            var container = new Container();
            StringAssert.Contains("container", container.ToString());

            using (var scope = container.OpenScope("a-a-a"))
                StringAssert.Contains("a-a-a", scope.ToString());
        }

        class MyService
        {
            public MyService(RequiredDependency d) {}
        }

        class RequiredDependency { }

        [Test, Ignore("#505: Failed to register object with ArgumentNullException")]
        public void Can_register_an_object_by_type()
        {
            var c = new Container();
            c.Register<object>();
        }
    }
}
