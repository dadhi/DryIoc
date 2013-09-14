using System;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class LambdaRegistrationTests
    {
        [Test]
        public void Given_Lambda_registration_Resolving_service_should_be_of_Lambda_provided_implementation()
        {
            var container = new Container();
            container.RegisterLambda<IService>(_ => new Service());

            var service = container.Resolve<IService>();

            Assert.That(service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Lambda_registration_without_specified_service_type_should_use_implementation_type_as_service_type()
        {
            var container = new Container();
            container.RegisterLambda(_ => new Service());

            Assert.Throws<ContainerException>(() => container.Resolve<IService>());

            var service = container.Resolve<Service>();
            Assert.That(service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Lambda_registration_could_be_resolved_as_Func()
        {
            var container = new Container();
            container.RegisterLambda<IService>(_ => new Service());

            var func = container.Resolve<Func<IService>>();

            Assert.That(func(), Is.Not.Null.And.Not.SameAs(func()));
        }

        [Test]
        public void Lambda_registration_could_be_resolved_as_Lazy()
        {
            var container = new Container();
            container.RegisterLambda<IService>(_ => new Service());

            var service = container.Resolve<Lazy<IService>>();

            Assert.That(service.Value, Is.Not.Null.And.SameAs(service.Value));
        }

        [Test]
        public void Given_lambda_registration_Injecting_it_as_dependency_should_work()
        {
            var container = new Container();
            container.Register<ServiceWithDependency>();

            var dependency = new Dependency();
            container.RegisterLambda<IDependency>(_ => dependency);

            var service = container.Resolve<ServiceWithDependency>();

            Assert.That(service.Dependency, Is.SameAs(dependency));
        }

        [Test]
        public void While_registering_It_is_possible_to_resolve_lambda_parameters_from_container()
        {
            var container = new Container();
            container.Register<IDependency, Dependency>();
            container.RegisterLambda(r => new ServiceWithDependency(r.Resolve<IDependency>()));

            var service = container.Resolve<ServiceWithDependency>();

            Assert.That(service.Dependency, Is.InstanceOf<Dependency>());
        }

        [Test]
        public void Resolving_non_registered_dependency_inside_lambda_should_throw()
        {
            var container = new Container();
            container.RegisterLambda(r => new ServiceWithDependency(r.Resolve<IDependency>()));

            Assert.Throws<ContainerException>(() => 
                container.Resolve<ServiceWithDependency>());
        }

        [Test]
        public void Registration_with_resolver_as_param_should_NOT_hold_reference_to_container_when_it_is_GCed()
        {
            var container = new Container();
            var containerWeakRef = new WeakReference(container);

            container.Register<IDependency, Dependency>();
            container.RegisterLambda(r => new ServiceWithDependency(r.Resolve<IDependency>()));
            container.Resolve<ServiceWithDependency>();
            container.Dispose();
// ReSharper disable RedundantAssignment
            container = null;
// ReSharper restore RedundantAssignment
            
            GC.Collect();

            Assert.That(containerWeakRef.IsAlive, Is.False);
        }
    }
}

