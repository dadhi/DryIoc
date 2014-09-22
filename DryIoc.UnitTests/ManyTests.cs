using System;
using System.Collections.Generic;
using System.Linq;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
	[TestFixture]
	public class ManyTests
	{
		[Test]
		public void Resolving_many_with_default_and_one_named_service_will_return_both_services()
		{
			var container = new Container();
			container.Register(typeof(IService), typeof(Service));
			container.Register(typeof(IService), typeof(AnotherService), named: "another");

			var many = container.Resolve<Many<Func<IService>>>();

            Assert.That(many.Items.Count(), Is.EqualTo(2));
		}

		[Test]
		public void I_can_resolve_many_singletons()
		{
			var container = new Container();
			container.Register(typeof(IService), typeof(Service), Reuse.Singleton);

			var many = container.Resolve<Many<IService>>();

            Assert.That(many.Items.Count(), Is.EqualTo(1));
		}

		[Test]
		public void I_can_resolve_many_mixed_of_singletons_and_transients()
		{
			var container = new Container();
			container.Register(typeof(IService), typeof(Service), Reuse.Singleton);
			container.Register(typeof(IService), typeof(AnotherService), named: "another");

			var many = container.Resolve<Many<IService>>();

            Assert.That(many.Items.Count(), Is.EqualTo(2));
		}

		[Test]
		public void Resolving_many_of_single_service_registered_with_delegate_should_return_this_service()
		{
			var container = new Container();
			container.RegisterDelegate<IService<string>>(_ => new ClosedGenericClass());

			var many = container.Resolve<Many<IService<string>>>();

			Assert.That(many.Items.Single(), Is.Not.Null);
		}

		[Test]
		public void I_can_resolve_many_open_generics()
		{
			var container = new Container();
			container.Register(typeof(IService<>), typeof(Service<>), Reuse.Singleton);

			var many = container.Resolve<Many<IService<int>>>();

            Assert.That(many.Items.Single(), Is.InstanceOf<Service<int>>());
		}

		[Test]
		public void I_can_resolve_many_lazy_singletons()
		{
			var container = new Container();
			ServiceWithInstanceCount.InstanceCount = 0;

			container.Register(typeof(ServiceWithInstanceCount), Reuse.Singleton);

			var services = container.Resolve<Many<Lazy<ServiceWithInstanceCount>>>().Items;
            Assert.That(ServiceWithInstanceCount.InstanceCount, Is.EqualTo(0));

			var service = services.First().Value;
            Assert.That(service, Is.Not.Null);

            Assert.That(ServiceWithInstanceCount.InstanceCount, Is.EqualTo(1));
		}

		[Test]
		public void I_can_inject_many_as_dependency()
		{
			var container = new Container();
			container.Register(typeof(IDependency), typeof(Dependency));
			container.Register(typeof(IDependency), typeof(Dependency), named: "Foo2");
			container.Register(typeof(IService), typeof(ServiceWithManyDependencies));

            var service = (ServiceWithManyDependencies)container.Resolve<IService>();

            Assert.That(service.Foos, Is.InstanceOf<IEnumerable<IDependency>>());
		}

		[Test]
		public void Resolving_many_for_not_registered_services_should_NOT_throw_BUT_return_an_empty_items()
		{
			var container = new Container();

		    var items = container.Resolve<Many<IService>>().Items;

            Assert.That(items.Count(), Is.EqualTo(0));
		}

        [Test]
        public void When_many_is_reresolved_after_registering_another_service_Then_many_should_contain_that_service()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service));
            var servicesBefore = container.Resolve<Many<IService>>().Items;
            Assert.That(servicesBefore.Count(), Is.EqualTo(1));

            container.Register(typeof(IService), typeof(AnotherService), named: "another");

            var servicesAfter = container.Resolve<Many<IService>>().Items;
            Assert.That(servicesAfter.Count(), Is.EqualTo(2));
        }

        [Test]
        public void When_many_dependency_is_reresolved_after_registering_another_service_Then_many_should_contain_that_service()
        {
            var container = new Container();
            container.Register<ServiceWithManyDependencies>();
            container.Register<IDependency, Foo1>();

            var service = container.Resolve<ServiceWithManyDependencies>();
            Assert.That(service.Foos.Count(), Is.EqualTo(1));

            container.Register<IDependency, Foo2>();

            var serviceAfter = container.Resolve<ServiceWithManyDependencies>();
            Assert.That(serviceAfter.Foos.Count(), Is.EqualTo(2));
        }

		[Test]
		public void I_should_be_able_to_resolve_Lazy_of_Func_of_Many()
		{
			var container = new Container();
			container.Register(typeof(IService), typeof(Service));
			container.Register(typeof(IService), typeof(AnotherService));

			var result = container.Resolve<Lazy<Func<Many<IService>>>>();

            Assert.That(result, Is.InstanceOf<Lazy<Func<Many<IService>>>>());
            Assert.That(result.Value.Invoke().Items.Count(), Is.EqualTo(2));
		}

        [Test]
        public void I_should_be_able_to_resolve_Meta_of_Many()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service), setup: Setup.WithMetadata("a"));
            container.Register(typeof(IService), typeof(AnotherService), setup: Setup.WithMetadata("b"));

            var result = container.Resolve<Meta<Many<IService>, string>>();

            Assert.That(result, Is.InstanceOf<Meta<Many<IService>, string>>());
            Assert.That(result.Metadata, Is.EqualTo("a"));
            var services = result.Value.Items.ToArray();
            Assert.That(services[0], Is.InstanceOf<Service>());
            Assert.That(services[1], Is.InstanceOf<AnotherService>());
        }

	    [Test]
	    public void If_some_item_is_not_resolved_then_it_would_not_throw()
	    {
            var container = new Container();
	        container.Register<Service>(setup: Setup.WithMetadata(1));

	        var servicesWithBoolMeta = container.Resolve<Many<Meta<Service, bool>>>().Items;
            Assert.That(servicesWithBoolMeta.Count(), Is.EqualTo(0));

            var servicesWithIntMeta = container.Resolve<Many<Meta<Service, int>>>().Items;
            Assert.That(servicesWithIntMeta.Count(), Is.EqualTo(1));
	    }
	}
}