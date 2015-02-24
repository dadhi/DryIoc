using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class RulesTests
    {
        [Test]
        public void It_is_possible_to_remove_Enumerable_support_per_container()
        {
            var container = new Container();
            container.Unregister(typeof(IEnumerable<>), factoryType: FactoryType.Wrapper);

            container.Register<Service>();

            Assert.Throws<ContainerException>(() =>
                container.Resolve<Service[]>());
        }

        [Test]
        public void Given_service_with_two_ctors_I_can_specify_what_ctor_to_choose_for_resolve()
        {
            var container = new Container();

            container.Register(typeof(Bla<>),
                withConstructor: t => t.GetConstructorOrNull(args: new[] { typeof(Func<>).MakeGenericType(t.GetGenericParamsAndArgs()[0]) }));

            container.Register(typeof(SomeService), typeof(SomeService));

            var bla = container.Resolve<Bla<SomeService>>();

            Assert.That(bla.Factory(), Is.InstanceOf<SomeService>());
        }

        [Test]
        public void I_should_be_able_to_add_rule_to_resolve_not_registered_service()
        {
            var container = new Container(Rules.Default.WithUnknownServiceResolver(request =>
                !request.ServiceType.IsValueType() && !request.ServiceType.IsAbstract()
                    ? new ReflectionFactory(request.ServiceType)
                    : null));

            var service = container.Resolve<NotRegisteredService>();

            Assert.That(service, Is.Not.Null);
        }

        [Test]
        public void When_service_registered_with_name_Then_it_could_be_resolved_with_ctor_parameter_ImportAttribute()
        {
            var container = new Container(rules => rules.With(parameters: GetServiceInfoFromImportAttribute));

            container.Register(typeof(INamedService), typeof(NamedService));
            container.Register(typeof(INamedService), typeof(AnotherNamedService), named: "blah");
            container.Register(typeof(ServiceWithImportedCtorParameter));

            var service = container.Resolve<ServiceWithImportedCtorParameter>();

            Assert.That(service.NamedDependency, Is.InstanceOf<AnotherNamedService>());
        }

        [Test]
        public void I_should_be_able_to_import_single_service_based_on_specified_metadata()
        {
            var container = new Container(rules => rules.With(parameters: GetServiceFromWithMetadataAttribute));

            container.Register(typeof(IFooService), typeof(FooHey), setup: Setup.With(metadata: FooMetadata.Hey));
            container.Register(typeof(IFooService), typeof(FooBlah), setup: Setup.With(metadata: FooMetadata.Blah));
            container.Register(typeof(FooConsumer));

            var service = container.Resolve<FooConsumer>();

            Assert.That(service.Foo.Value, Is.InstanceOf<FooBlah>());
        }

        [Test]
        public void You_can_specify_rules_to_resolve_last_registration_from_multiple_available()
        {
            var container = new Container(Rules.Default.WithFactorySelector(
                (request, factories) => factories.LastOrDefault(f => f.Key.Equals(request.ServiceKey)).Value));

            container.Register(typeof(IService), typeof(Service));
            container.Register(typeof(IService), typeof(AnotherService));
            var service = container.Resolve(typeof(IService));

            Assert.That(service, Is.InstanceOf<AnotherService>());
        }

        [Test]
        public void You_can_specify_rules_to_disable_registration_based_on_reuse_type()
        {
            var container = new Container(Rules.Default.WithFactorySelector(
                (request, factories) => factories.FirstOrDefault(f => f.Key.Equals(request.ServiceKey) && !(f.Value.Reuse is SingletonReuse)).Value));

            container.Register<IService, Service>(Reuse.Singleton);
            var service = container.Resolve(typeof(IService), IfUnresolved.ReturnDefault);

            Assert.That(service, Is.Null);
        }

        public static ParameterServiceInfo GetServiceInfoFromImportAttribute(ParameterInfo parameter, Request request)
        {
            var import = (ImportAttribute)parameter.GetAttributes(typeof(ImportAttribute)).FirstOrDefault();
            var details = import == null ? ServiceInfoDetails.Default
                : ServiceInfoDetails.Of(import.ContractType, import.ContractName);
            return ParameterServiceInfo.Of(parameter).WithDetails(details, request);
        }

        public static ParameterServiceInfo GetServiceFromWithMetadataAttribute(ParameterInfo parameter, Request request)
        {
            var import = (ImportWithMetadataAttribute)parameter.GetAttributes(typeof(ImportWithMetadataAttribute))
                .FirstOrDefault();
            if (import == null)
                return null;

            var registry = request.Container;
            var serviceType = parameter.ParameterType;
            serviceType = registry.UnwrapServiceType(serviceType);
            var metadata = import.Metadata;
            var factory = registry.GetAllServiceFactories(serviceType)
                .FirstOrDefault(kv => metadata.Equals(kv.Value.Setup.Metadata))
                .ThrowIfNull();

            return ParameterServiceInfo.Of(parameter).WithDetails(ServiceInfoDetails.Of(serviceType, factory.Key), request);
        }

        [Test]
        public void Can_turn_Off_singleton_optimization()
        {
            var container = new Container(r => r.WithoutSingletonOptimization());
            container.Register<FooHey>(Reuse.Singleton);

            var singleton = container.Resolve<FactoryExpression<FooHey>>();

            Assert.That(singleton.Value.ToString(), Is.StringContaining("SingletonScope"));
        }

        [Test]
        public void Can_hook_and_verify_registered_factories_ID()
        {
            var factoryIDs = new Dictionary<Type, string>();
            var container = new Container(rules => rules.WithoutSingletonOptimization());

            var factory = new ReflectionFactory(typeof(XX), Reuse.Singleton);
            factoryIDs[typeof(XX)] = factory.FactoryID.ToString();
            container.Register(typeof(XX), factory);

            factory = new ReflectionFactory(typeof(YY), Reuse.Singleton);
            container.Register(typeof(YY), factory);
            factoryIDs[typeof(YY)] = factory.FactoryID.ToString();

            StringAssert.Contains(factoryIDs[typeof(YY)], container.Resolve<FactoryExpression<YY>>().Value.ToString());
            StringAssert.Contains(factoryIDs[typeof(XX)], container.Resolve<FactoryExpression<XX>>().Value.ToString());
        }

        internal class XX { }
        internal class YY { }
        internal class ZZ { }

        #region CUT

        public class SomeService { }

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

        [AttributeUsage(AttributeTargets.Parameter)]
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

        class NotRegisteredService
        {
        }

        #endregion
    }
}