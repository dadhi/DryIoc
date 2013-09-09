using NUnit.Framework;

namespace DryIoc.UnitTests
{
	[TestFixture]
	public class DynamicFactoryTests
	{
		[Test]
		public void I_can_resolve_service_with_not_registered_lazy_parameter_using_dynamic_factory()
		{
			var container = new Container();
			container.Register<ServiceWithNotRegisteredLazyParameter>();
			container.Register(typeof(DynamicFactory<>));
			container.RegisterLambda(() => container);

			var service = container.Resolve<ServiceWithNotRegisteredLazyParameter>();

            Assert.That(service.Parameter.CanCreate, Is.False);

			container.Register<NotRegisteredService>();

            Assert.That(service.Parameter.CanCreate, Is.True);
            Assert.That(service.Parameter.Create(), Is.Not.Null);
		}
	}

	#region CUT

	public class ServiceWithNotRegisteredLazyParameter
	{
		public DynamicFactory<NotRegisteredService> Parameter { get; set; }

		public ServiceWithNotRegisteredLazyParameter(DynamicFactory<NotRegisteredService> parameter)
		{
			Parameter = parameter;
		}
	}

	public class NotRegisteredService
	{
	}

	public class DynamicFactory<T>
	{
		public DynamicFactory(Container container)
		{
			_container = container;
		}

		public bool CanCreate
		{
			get { return _container.IsRegistered(typeof (T)); }
		}

		public T Create()
		{
			return _container.Resolve<T>();
		}

		private readonly Container _container;
	}

	#endregion
}