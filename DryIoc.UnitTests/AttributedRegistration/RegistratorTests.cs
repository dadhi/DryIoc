using System;
using System.Linq;
using System.Reflection;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests.AttributedRegistration
{
	[TestFixture]
	public class RegistratorTests
	{
		[Test]
		public void I_can_resolve_service_with_dependencies()
		{
			GivenAssemblyWithExportedTypes();
			WhenIRegisterAllExportedTypes();

			var service = _container.Resolve<DependentService>();

            Assert.That(service.TransientService, Is.Not.Null);
            Assert.That(service.SingletonService, Is.Not.Null);
            Assert.That(service.TransientOpenGenericService, Is.Not.Null);
            Assert.That(service.OpenGenericServiceWithTwoParameters, Is.Not.Null);
		}

		[Test]
		public void I_can_resolve_transient_service()
		{
			GivenAssemblyWithExportedTypes();
			WhenIRegisterAllExportedTypes();

			var service = _container.Resolve<ITransientService>();
			var anotherService = _container.Resolve<ITransientService>();

            Assert.That(service, Is.Not.Null.And.Not.SameAs(anotherService));
		}

		[Test]
		public void I_can_resolve_singleton_service()
		{
			GivenAssemblyWithExportedTypes();
			WhenIRegisterAllExportedTypes();

			var service = _container.Resolve<ISingletonService>();
			var anotherService = _container.Resolve<ISingletonService>();

            Assert.That(service, Is.Not.Null.And.SameAs(anotherService));
		}

		[Test]
		public void I_can_resolve_singleton_open_generic_service()
		{
			GivenAssemblyWithExportedTypes();
			WhenIRegisterAllExportedTypes();

			var service = _container.Resolve<IOpenGenericService<int>>();
			var anotherService = _container.Resolve<IOpenGenericService<int>>();

            Assert.That(service, Is.Not.Null.And.SameAs(anotherService));
		}

		[Test]
		public void I_can_resolve_transient_open_generic_service()
		{
			GivenAssemblyWithExportedTypes();
			WhenIRegisterAllExportedTypes();

			var service = _container.Resolve<TransientOpenGenericService<object>>();
			var anotherService = _container.Resolve<TransientOpenGenericService<object>>();

            Assert.That(service, Is.Not.Null.And.Not.SameAs(anotherService));
		}

		[Test]
		public void I_can_resolve_open_generic_service_with_two_parameters()
		{
			GivenAssemblyWithExportedTypes();
			WhenIRegisterAllExportedTypes();

			var service = _container.Resolve<OpenGenericServiceWithTwoParameters<int, string>>();

            Assert.That(service, Is.Not.Null);
		}

		[Test]
		public void I_can_resolve_service_factory()
		{
			GivenAssemblyWithExportedTypes();
			WhenIRegisterAllExportedTypes();

			var serviceFactory = _container.Resolve<Func<ITransientService>>();

		    Assert.That(serviceFactory(), Is.Not.Null);
		}

		[Test]
		public void I_can_resolve_array_of_func_with_one_parameter()
		{
			GivenAssemblyWithExportedTypes();
			WhenIRegisterAllExportedTypes();

			var factories = _container.Resolve<Func<string, IServiceWithMultipleImplentations>[]>();
            Assert.That(factories.Length, Is.EqualTo(2));

			var oneService = factories[0].Invoke("0");
            Assert.That(oneService.Message, Is.EqualTo("0"));

			var anotherService = factories[1].Invoke("1");
            Assert.That(anotherService.Message, Is.EqualTo("1"));
		}

		[Test]
		public void I_can_resolve_meta_factory_many()
		{
			GivenAssemblyWithExportedTypes();
			WhenIRegisterAllExportedTypes();

			var factories = _container.Resolve<Meta<Func<IServiceWithMetadata>, IViewMetadata>[]>();
			Assert.That(factories.Length, Is.EqualTo(3));

			var factory = factories.First(meta => meta.Metadata.DisplayName.Equals("Down"));
			var service = factory.Value();
            Assert.That(service, Is.InstanceOf<AnotherServiceWithMetadata>());

			var anotherService = factory.Value();
			Assert.That(anotherService, Is.Not.SameAs(service));
		}

		[Test]
		public void Container_can_be_setup_to_select_one_constructor_based_on_attribute()
		{
			GivenAssemblyWithExportedTypes();
			WhenIRegisterAllExportedTypes();

			var service = _container.Resolve<ServiceWithMultipleCostructorsAndOneImporting>();

            Assert.That(service.Transient, Is.Not.Null);
		    Assert.That(service.Singleton, Is.Null);

		}

		[Test]
		public void Resolving_service_with_multiple_constructors_without_importing_attribute_should_fail()
		{
			GivenAssemblyWithExportedTypes();
			WhenIRegisterAllExportedTypes();

            Assert.Throws<ContainerException>(
				() => _container.Resolve<ServiceWithMultipleCostructors>());
		}

		[Test]
		public void Service_with_metadata_can_be_resolved_without_name()
		{
			GivenAssemblyWithExportedTypes();
			WhenIRegisterAllExportedTypes();

            Assert.DoesNotThrow(
				() => _container.Resolve<SingleServiceWithMetadata>());
		}

		#region Implementation

		private void WhenIRegisterAllExportedTypes()
		{
			_container = new Container();
			_container.RegisterExports(_assembly);
		}

		private void GivenAssemblyWithExportedTypes()
		{
			_assembly = typeof(DependentService).Assembly;
		}

		private Assembly _assembly;

		private Container _container;

		#endregion
	}
}