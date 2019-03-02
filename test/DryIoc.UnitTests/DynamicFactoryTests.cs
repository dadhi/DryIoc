using System;
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
            container.RegisterInstance(container);

            var service = container.Resolve<ServiceWithNotRegisteredLazyParameter>();

            Assert.That(service.Parameter.CanCreate, Is.False);

            container.Register<NotRegisteredService>();

            Assert.That(service.Parameter.CanCreate, Is.True);
            Assert.That(service.Parameter.Create(), Is.Not.Null);
        }

        [Test]
        public void Can_resolve_Func_of_Lazy()
        {
            var container = new Container();
            container.Register<IServiceWithDependency, ServiceWithDependency>();
            container.Register(typeof(Service));
            container.Register(typeof(LazyDynamic<>), setup: Setup.Wrapper);

            var func = container.Resolve<Func<LazyDynamic<IServiceWithDependency>>>();
            var service = func().Value.Value;

            Assert.That(service.Dependency, Is.InstanceOf<Service>());
        }

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
                get { return _container.IsRegistered(typeof(T)); }
            }

            public T Create()
            {
                return _container.Resolve<T>();
            }

            private readonly Container _container;
        }

        internal class LazyDynamic<T>
        {
            public readonly Lazy<T> Value;

            public LazyDynamic(IResolver resolver)
            {
                Value = new Lazy<T>(() => resolver.Resolve<T>());
            }
        }

        internal class Service { }

        internal interface IServiceWithDependency
        {
            Service Dependency { get; }
        }

        internal class ServiceWithDependency : IServiceWithDependency
        {
            public Service Dependency { get; private set; }

            public ServiceWithDependency(Service dependency)
            {
                Dependency = dependency;
            }
        }
    }
}