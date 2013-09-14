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
			container.RegisterLambda<IService<string>>(_ => new ClosedGenericClass());

			var services = container.Resolve<IEnumerable<IService<string>>>();

			Assert.That(services.Single(), Is.Not.Null);
		}

		[Test]
		public void ResolveEnumerableOfFunc_ServiceRegisteredWithFunc_ShouldReturnEnumerableOfFunc()
		{
			var container = new Container();
			container.RegisterLambda<IService<string>>(_ => new ClosedGenericClass());

			var services = container.Resolve<IEnumerable<Func<IService<string>>>>();

			var factory = services.Single();
			var service = factory();
			Assert.IsNotNull(service);
			var anotherService = factory();
			Assert.AreNotSame(service, anotherService);
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
		public void Resolve_ServiceWithIEnumerableDependency_ReturnsInstanceWithIEnumerableDependency()
		{
			var container = new Container();
			container.Register(typeof(IDependency), typeof(Dependency));
			container.Register(typeof(IDependency), typeof(Dependency), named: "Foo2");
			container.Register(typeof(IService), typeof(ServiceWithEnumerableDependency));

			var service = (ServiceWithEnumerableDependency)container.Resolve<IService>();

            Assert.That(service.Foos, Is.InstanceOf<IEnumerable<IDependency>>());
		}

		[Test]
		public void Resolving_IEnumerable_should_for_not_registered_service_should_return_empty_collection()
		{
			var container = new Container();

		    var count = container.Resolve<IEnumerable<IService>>().Count();

            Assert.That(count, Is.EqualTo(0));
		}

		[Test]
		public void ReResolving_Enumerable_after_registering_another_service_should_contain_that_service()
		{
			// Arrange
			var container = new Container();
            container.Register<ServiceWithEnumerableDependency>();
            container.Register<IDependency, Foo1>();

            var service = container.Resolve<ServiceWithEnumerableDependency>();
			Assert.That(service.Foos.Count(), Is.EqualTo(1));

			// Act
            container.Register<IDependency, Foo2>();
            var serviceAfter = container.Resolve<ServiceWithEnumerableDependency>();

            Assert.That(serviceAfter.Foos.Count(), Is.EqualTo(2));
		}

        [Test]
        public void ReResolving_Enumerable_as_dependency_after_registering_another_service_should_contain_that_service()
        {
            // Arrange
            var container = new Container();
            container.Register(typeof(IService), typeof(Service));
            var servicesBefore = container.Resolve<IEnumerable<IService>>().ToArray();
            Assert.That(servicesBefore.Length, Is.EqualTo(1));

            // Act
            container.Register(typeof(IService), typeof(AnotherService), named: "another");
            var servicesAfter = container.Resolve<IEnumerable<IService>>().ToArray();

            Assert.That(servicesAfter.Length, Is.EqualTo(2));
        }

		[Test]
		public void I_should_be_able_to_resolve_Lazy_of_Func_of_IEnumerable()
		{
			// Arrange
			var container = new Container();
			container.Register(typeof(IService), typeof(Service), named: "blah");
			container.Register(typeof(IService), typeof(Service), named: "crew");

			// Act
			var result = container.Resolve<Lazy<Func<IEnumerable<IService>>>>();

            Assert.That(result, Is.InstanceOf<Lazy<Func<IEnumerable<IService>>>>());
            Assert.That(result.Value.Invoke().Count(), Is.EqualTo(2));
		}
	}
}