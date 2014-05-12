using System;
using System.Collections.Generic;
using System.Linq;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class EnumerableAndArrayTests
    {
        [Test]
        public void Resolving_array_with_default_and_one_named_service_will_return_both_services()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service));
            container.Register(typeof(IService), typeof(AnotherService), named: "another");

            var services = container.Resolve<Func<IService>[]>();

            Assert.That(services.Length, Is.EqualTo(2));
        }

        [Test]
        public void I_can_resolve_array_of_singletons()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service), Reuse.Singleton);

            var services = container.Resolve<IService[]>();

            Assert.That(services.Length, Is.EqualTo(1));
        }

        [Test]
        public void I_can_resolve_mixed_array_of_singletons_and_transients()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service), Reuse.Singleton);
            container.Register(typeof(IService), typeof(AnotherService), named: "another");

            var services = container.Resolve<IService[]>();

            Assert.That(services.Length, Is.EqualTo(2));
        }

        [Test]
        public void Resolving_enumerable_of_service_should_return_enumerable_type()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service));

            var services = container.Resolve<IEnumerable<IService>>();

            Assert.That(services, Is.InstanceOf<IEnumerable<IService>>());
        }

        [Test]
        public void Resolving_enumerable_with_default_and_one_named_service_will_return_both_services()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service));
            container.Register(typeof(IService), typeof(AnotherService), named: "AnotherService");

            var services = container.Resolve<IEnumerable<IService>>();

            Assert.That(services.Count(), Is.EqualTo(2));
        }

        [Test]
        public void Resolving_enumerable_of_service_registered_with_func_should_return_enumerable_with_single_service()
        {
            var container = new Container();
            container.RegisterDelegate<IService<string>>(_ => new ClosedGenericClass());

            var services = container.Resolve<IEnumerable<IService<string>>>();

            Assert.That(services.Single(), Is.Not.Null);
        }

        [Test]
        public void I_can_resolve_array_of_open_generics()
        {
            var container = new Container();
            container.Register(typeof(IService<>), typeof(Service<>), Reuse.Singleton);

            var services = container.Resolve<IEnumerable<IService<int>>>();

            Assert.That(services.Single(), Is.InstanceOf<Service<int>>());
        }

        [Test]
        public void I_can_resolve_array_of_lazy_singletons()
        {
            var container = new Container();
            ServiceWithInstanceCount.InstanceCount = 0;

            container.Register(typeof(ServiceWithInstanceCount), Reuse.Singleton);

            var services = container.Resolve<IEnumerable<Lazy<ServiceWithInstanceCount>>>();
            Assert.That(ServiceWithInstanceCount.InstanceCount, Is.EqualTo(0));

            var service = services.First().Value;
            Assert.That(service, Is.Not.Null);

            Assert.That(ServiceWithInstanceCount.InstanceCount, Is.EqualTo(1));
        }

        [Test]
        public void I_can_inject_enumerable_as_dependency()
        {
            var container = new Container();
            container.Register(typeof(IDependency), typeof(Dependency));
            container.Register(typeof(IDependency), typeof(Dependency), named: "Foo2");
            container.Register(typeof(IService), typeof(ServiceWithEnumerableDependencies));

            var service = (ServiceWithEnumerableDependencies)container.Resolve<IService>();

            Assert.That(service.Foos, Is.InstanceOf<IEnumerable<IDependency>>());
        }

        [Test]
        public void Resolving_array_of_not_registered_services_should_throw()
        {
            var container = new Container();

            Assert.Throws<ContainerException>(() =>
                container.Resolve<IService[]>());
        }

        [Test]
        public void When_enumerable_is_reresolved_after_registering_another_service_Then_enumerable_should_NOT_contain_that_service()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service));
            var servicesBefore = container.Resolve<IService[]>();
            Assert.That(servicesBefore.Length, Is.EqualTo(1));

            container.Register(typeof(IService), typeof(AnotherService), named: "another");

            var servicesAfter = container.Resolve<IService[]>();
            Assert.That(servicesAfter.Length, Is.EqualTo(1));
        }

        [Test]
        public void When_enumerable_dependency_is_reresolved_after_registering_another_service_Then_enumerable_should_NOT_contain_that_service()
        {
            var container = new Container();
            container.Register<ServiceWithEnumerableDependencies>();
            container.Register<IDependency, Foo1>();

            var service = container.Resolve<ServiceWithEnumerableDependencies>();
            Assert.That(service.Foos.Count(), Is.EqualTo(1));

            container.Register<IDependency, Foo2>();
            var serviceAfter = container.Resolve<ServiceWithEnumerableDependencies>();

            Assert.That(serviceAfter.Foos.Count(), Is.EqualTo(1));
        }

        [Test]
        public void I_should_be_able_to_resolve_Lazy_of_Func_of_IEnumerable()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service), named: "blah");
            container.Register(typeof(IService), typeof(Service), named: "crew");

            var result = container.Resolve<Lazy<Func<IEnumerable<IService>>>>();

            Assert.That(result, Is.InstanceOf<Lazy<Func<IEnumerable<IService>>>>());
            Assert.That(result.Value.Invoke().Count(), Is.EqualTo(2));
        }

        [Test]
        public void If_some_item_is_not_resolved_then_it_would_throw()
        {
            var container = new Container();
            container.Register<Service>(setup: ServiceSetup.WithMetadata(1));

            Assert.Throws<ContainerException>(() =>
                container.Resolve<IEnumerable<Meta<Service, bool>>>());
        }

        [Test]
        public void Resove_func_of_default_service_then_array_of_default_and_named_service_should_Succeed()
        {
            var container = new Container();
            container.Register<IService, Service>(named: 1);
            container.Register<IService, Service>();

            container.Resolve<Func<IService>>();
            var funcs = container.Resolve<Func<IService>[]>();

            Assert.That(funcs.Length, Is.EqualTo(2));
        }
    }
}