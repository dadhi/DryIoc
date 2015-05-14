using System;
using System.Collections.Generic;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class DelegateFactoryTests
    {
        [Test]
        public void Can_register_custom_delegates_without_ReflectionFactory()
        {
            var container = new Container();

            container.Register<ServiceLocator>();

            container.Register<SingleInstanceFactory>(
                made: Made.Of(r => ServiceInfo.Of<ServiceLocator>(), l => l.X));
            container.Register<MultiInstanceFactory>(
                made: Made.Of(r => ServiceInfo.Of<ServiceLocator>(), l => l.Y));

            container.Resolve<SingleInstanceFactory>();
            container.Resolve<MultiInstanceFactory>();
        }

        public delegate object SingleInstanceFactory(Type serviceType);
        public delegate IEnumerable<object> MultiInstanceFactory(Type serviceType);

        public class ServiceLocator
        {
            private readonly IResolver _r;

            public readonly SingleInstanceFactory X;
            public readonly MultiInstanceFactory Y;

            public ServiceLocator(IResolver r)
            {
                _r = r;
                X = GetInstance;
                Y = GetAllInstances;
            }

            public object GetInstance(Type serviceType)
            {
                return _r.Resolve(serviceType);
            }

            public IEnumerable<object> GetAllInstances(Type serviceType)
            {
                return _r.ResolveMany<object>(serviceType);
            }
        }

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

            container.RegisterDelegate(typeof (IService), _ => new Service());

            var service = container.Resolve<IService>();
            Assert.That(service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Resolving_delegate_with_service_type_not_assignable_to_created_object_type_should_Throw()
        {
            var container = new Container();

            container.RegisterDelegate(typeof (IService), _ => "blah");

            Assert.Throws<ContainerException>(() =>
                container.Resolve<IService>());
        }

        internal class MyClass
        {
            public IService MyService { get; set; }
        }
    }
}

