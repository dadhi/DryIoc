using System;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class DelegateFactoryTests
    {
        [Test]
        public void Given_Lambda_registration_Resolving_service_should_be_of_Lambda_provided_implementation()
        {
            var container = new Container();
            container.RegisterDelegate<IService>(_ => new Service());

            var service = container.Resolve<IService>();

            Assert.That(service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Lambda_registration_without_specified_service_type_should_use_implementation_type_as_service_type()
        {
            var container = new Container();
            container.RegisterDelegate(_ => new Service());

            Assert.Throws<ContainerException>(() => container.Resolve<IService>());

            var service = container.Resolve<Service>();
            Assert.That(service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Lambda_registration_could_be_resolved_as_Func()
        {
            var container = new Container();
            container.RegisterDelegate<IService>(_ => new Service());

            var func = container.Resolve<Func<IService>>();

            Assert.That(func(), Is.Not.Null.And.Not.SameAs(func()));
        }

        [Test]
        public void Lambda_registration_could_be_resolved_as_Lazy()
        {
            var container = new Container();
            container.RegisterDelegate<IService>(_ => new Service());

            var service = container.Resolve<Lazy<IService>>();

            Assert.That(service.Value, Is.Not.Null.And.SameAs(service.Value));
        }

        [Test]
        public void Given_lambda_registration_Injecting_it_as_dependency_should_work()
        {
            var container = new Container();
            container.Register<ServiceWithDependency>();

            var dependency = new Dependency();
            container.RegisterDelegate<IDependency>(_ => dependency);

            var service = container.Resolve<ServiceWithDependency>();

            Assert.That(service.Dependency, Is.SameAs(dependency));
        }

        [Test]
        public void While_registering_It_is_possible_to_resolve_lambda_parameters_from_container()
        {
            var container = new Container();
            container.Register<IDependency, Dependency>();
            container.RegisterDelegate(r => new ServiceWithDependency(r.Resolve<IDependency>()));

            var service = container.Resolve<ServiceWithDependency>();

            Assert.That(service.Dependency, Is.InstanceOf<Dependency>());
        }

        [Test]
        public void Resolving_non_registered_dependency_inside_lambda_should_throw()
        {
            var container = new Container();
            container.RegisterDelegate(r => new ServiceWithDependency(r.Resolve<IDependency>()));

            Assert.Throws<ContainerException>(() =>
                container.Resolve<ServiceWithDependency>());
        }

        [Test]
        public void Possible_to_Register_delegate_with_runtime_type_of_service()
        {
            var container = new Container();

            container.RegisterDelegate(typeof(IService), _ => new Service());

            var service = container.Resolve<IService>();
            Assert.That(service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Resolving_delegate_with_service_type_not_assignable_to_created_object_type_should_Throw()
        {
            var container = new Container();

            container.RegisterDelegate(typeof(IService), _ => "blah");

            Assert.Throws<ContainerException>(() =>
                container.Resolve<IService>());
        }

        [Test]
        public void Possible_to_Register_pre_created_instance_of_runtime_serive_type()
        {
            var container = new Container();

            container.RegisterInstance(typeof(string), "ring", named: "MyPrecious");

            var ring = container.Resolve<string>("MyPrecious");
            Assert.That(ring, Is.EqualTo("ring"));
        }

        [Test]
        public void Registering_pre_created_instance_not_assignable_to_runtime_serive_type_should_Throw()
        {
            var container = new Container();

            Assert.Throws<ContainerException>(() =>
                container.RegisterInstance(typeof(IService), "ring", named: "MyPrecious"));
        }

        [Test]
        public void Detect_recursive_dependency_when_registered_with_delegate()
        {
            var container = new Container();
            container.RegisterDelegate(r => new SomeClient(r.Resolve<ServiceWithClient>()));
            container.Register<ServiceWithClient>();

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<SomeClient>());

            // JITing method, just for test. 
            System.Runtime.CompilerServices.RuntimeHelpers.PrepareMethod(
                GetType().GetMethod("Detect_recursive_dependency_when_registered_with_delegate").MethodHandle);

            Assert.That(ex.Message, Is.StringContaining("Recursive dependency is detected in resolution of"));
        }

        internal class SomeClient
        {
            public ServiceWithClient Service { get; set; }

            public SomeClient(ServiceWithClient service)
            {
                Service = service;
            }
        }

        internal class ServiceWithClient
        {
            public SomeClient Client { get; set; }

            public ServiceWithClient(SomeClient client)
            {
                Client = client;
            }
        }
    }
}

