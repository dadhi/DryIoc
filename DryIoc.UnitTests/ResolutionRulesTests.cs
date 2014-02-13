using System;
using System.Linq;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class ResolutionRulesTests
    {
        [Test]
        public void I_should_be_able_to_turn_off_Enumerable_support_for_container_instance()
        {
            var container = new Container();
            container.Register<Service>();

            container.ResolutionRules.ForUnregisteredService.Remove(OpenGenericsSupport.ResolveEnumerableAsStaticArray);

            Assert.Throws<ContainerException>(() =>
                container.Resolve<Service[]>());
        }

        [Test]
        public void Given_service_with_two_ctors_I_can_specify_what_ctor_to_choose_for_resolve()
        {
            var container = new Container();

            container.Register(typeof(Bla<>),
                withConstructor: t => t.GetConstructor(new[] { typeof(Func<>).MakeGenericType(t.GetGenericArguments()[0]) }));

            container.Register(typeof(SomeService), typeof(SomeService));

            var bla = container.Resolve<Bla<SomeService>>();

            Assert.That(bla.Factory(), Is.InstanceOf<SomeService>());
        }

        [Test]
        public void I_should_be_able_to_add_rule_to_resolve_not_registered_service()
        {
            var container = new Container();

            container.ResolutionRules.ForUnregisteredService.Append((request, registry) =>
                request.ServiceType.IsClass && !request.ServiceType.IsAbstract
                    ? new ReflectionFactory(request.ServiceType).WithContext(request, registry)
                    : null);

            var service = container.Resolve<NotRegisteredService>();

            Assert.That(service, Is.Not.Null);
        }

        [Test]
        public void When_service_registered_with_name_Then_it_could_be_resolved_with_ctor_parameter_ImportAttribute()
        {
            var container = new Container();
            container.ResolutionRules.ForConstructorParameter.Append((parameter, _, __) =>
            {
                object key;
                return TryGetServiceKeyFromImportAttribute(out key, parameter.GetCustomAttributes(false)) ? key : null;
            });

            container.Register(typeof(INamedService), typeof(NamedService));
            container.Register(typeof(INamedService), typeof(AnotherNamedService), named: "blah");
            container.Register(typeof(ServiceWithImportedCtorParameter));

            var service = container.Resolve<ServiceWithImportedCtorParameter>();

            Assert.That(service.NamedDependency, Is.InstanceOf<AnotherNamedService>());
        }

        [Test]
        public void I_should_be_able_to_import_single_service_based_on_specified_metadata()
        {
            var container = new Container();
            container.ResolutionRules.ForConstructorParameter.Append((parameter, parent, registry) =>
            {
                object key;
                var attributes = parameter.GetCustomAttributes(false);
                return TryGetServiceKeyWithMetadataAttribute(out key, parameter.ParameterType, parent, registry, attributes)
                    ? key : null;
            });

            container.Register(typeof(IFooService), typeof(FooHey), setup: ServiceSetup.WithMetadata(FooMetadata.Hey));
            container.Register(typeof(IFooService), typeof(FooBlah), setup: ServiceSetup.WithMetadata(FooMetadata.Blah));
            container.Register(typeof(FooConsumer));

            var service = container.Resolve<FooConsumer>();

            Assert.That(service.Foo.Value, Is.InstanceOf<FooBlah>());
        }

        [Test]
        public void I_should_be_able_to_manually_register_open_generic_singletons_and_resolve_them_directly()
        {
            var container = new Container();

            container.Register(
                typeof(IService<>),
                new FactoryProvider((request, _) => new ReflectionFactory(
                    typeof(Service<>).MakeGenericType(request.ServiceType.GetGenericArguments()),
                    Reuse.Singleton)));

            var service1 = container.Resolve<IService<int>>();
            var service2 = container.Resolve<IService<int>>();

            Assert.That(service1, Is.SameAs(service2));
        }

        [Test]
        public void I_should_be_able_to_manually_register_open_generic_singletons_and_resolve_then_as_dependency()
        {
            var container = new Container();
            container.Register(typeof(ServiceWithTwoSameGenericDependencies));

            container.Register(
                typeof(IService<>),
                new FactoryProvider((request, _) => new ReflectionFactory(
                    typeof(Service<>).MakeGenericType(request.ServiceType.GetGenericArguments()),
                    Reuse.Singleton)));

            var service = container.Resolve<ServiceWithTwoSameGenericDependencies>();

            Assert.That(service.Service1, Is.SameAs(service.Service2));
        }

        [Test]
        public void You_can_customize_resolving_single_implementation_from_multiple_registrations()
        {
            var container = new Container();
            container.ResolutionRules.ToGetSingleFactory = factories => factories.Last();

            container.Register(typeof(IService), typeof(Service));
            container.Register(typeof(IService), typeof(AnotherService));
            var service = container.Resolve(typeof(IService));

            Assert.That(service, Is.InstanceOf<AnotherService>());
        }

        public static bool TryGetServiceKeyFromImportAttribute(out object key, object[] attributes)
        {
            var import = GetSingleAttributeOrDefault<ImportAttribute>(attributes);
            key = import == null ? null : import.ContractName;
            return import != null;
        }

        public static bool TryGetServiceKeyWithMetadataAttribute(out object key, Type contractType, Request parent, IRegistry registry, object[] attributes)
        {
            key = null;
            var import = GetSingleAttributeOrDefault<ImportWithMetadataAttribute>(attributes);
            if (import == null)
                return false;

            var serviceType = registry.GetWrappedServiceTypeOrSelf(contractType);
            var metadata = import.Metadata;
            key = registry.GetAllFactories(serviceType).FirstOrDefault(kv => metadata.Equals(kv.Value.Setup.Metadata))
                .ThrowIfNull("Unable to resolve", serviceType, metadata, parent)
                .Key;
            return true;
        }

        private static TAttribute GetSingleAttributeOrDefault<TAttribute>(object[] attributes) where TAttribute : Attribute
        {
            TAttribute attr = null;
            for (var i = 0; i < attributes.Length && attr == null; i++)
                attr = attributes[i] as TAttribute;
            return attr;
        }
    }

    #region CUT

    public class SomeService
    {
    }

    public class Bla<T>
    {
        public string Message { get; set; }
        public Func<T> Factory { get; set; }

        public Bla(string message)
        {
            Message = message;
        }

        public Bla(Func<T> factory)
        {
            Factory = factory;
        }
    }

    enum FooMetadata { Hey, Blah }

    public interface IFooService
    {
    }

    public class FooHey : IFooService
    {
    }

    public class FooBlah : IFooService
    {
    }


    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class ImportWithMetadataAttribute : Attribute
    {
        public ImportWithMetadataAttribute(object metadata)
        {
            Metadata = metadata.ThrowIfNull();
        }

        public readonly object Metadata;
    }

    public class FooConsumer
    {
        public Lazy<IFooService> Foo { get; set; }

        public FooConsumer([ImportWithMetadata(FooMetadata.Blah)] Lazy<IFooService> foo)
        {
            Foo = foo;
        }
    }

    public class TransientOpenGenericService<T>
    {
        public T Value { get; set; }
    }

    public interface INamedService
    {
    }

    public class NamedService : INamedService
    {
    }

    public class AnotherNamedService : INamedService
    {
    }

    public class ServiceWithImportedCtorParameter
    {
        public INamedService NamedDependency { get; set; }

        public ServiceWithImportedCtorParameter([Import("blah")]INamedService namedDependency)
        {
            NamedDependency = namedDependency;
        }
    }

    #endregion
}