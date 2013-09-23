using System;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class ContainerCustomizationTests
    {
        [Test]
        public void I_should_be_able_to_turn_off_Enumerable_support_for_container_instance()
        {
            var container = new Container();
            container.Register<Service>();

            container.Setup.RemoveNonRegisteredServiceResolutionRule(Container.TryResolveEnumerableOrArray);

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
        public void Given_barebone_container_I_should_be_able_to_resolve_service()
        {
            var container = new Container(true);
            container.Register(typeof(Service));

            var service = container.Resolve<Service>();
            
            Assert.That(service, Is.Not.Null);
        }

        [Test]
        public void Given_barebone_container_I_should_be_able_to_resolve_service_with_dependencies()
        {
            var container = new Container(true);
            container.Register(typeof(IDependency), typeof(Dependency));
            container.Register(typeof(ServiceWithDependency));

            var service = container.Resolve<ServiceWithDependency>();

            Assert.That(service, Is.Not.Null);
        }

        [Test]
        public void Given_core_only_container_I_can_NOT_resolve_func_of_service()
        {
            var container = new Container(true);
            container.Register(typeof(Service));

            Assert.Throws<ContainerException>(() =>
                container.Resolve<Func<Service>>());
        }

        [Test]
        public void Given_barebone_container_I_should_be_able_to_resolve_open_generic_service()
        {
            var container = new Container(true);
            container.Register(typeof(TransientOpenGenericService<>));

            var service = container.Resolve<TransientOpenGenericService<int>>();

            Assert.That(service, Is.Not.Null);
        }

        [Test]
        public void I_should_be_able_to_add_rule_to_resolve_not_registered_service()
        {
            var container = new Container();
            container.Setup.AddNonRegisteredServiceResolutionRule((request, _) =>
                request.ServiceType.IsClass && !request.ServiceType.IsAbstract 
                    ? new ReflectionFactory(request.ServiceType) 
                    : null);

            var service = container.Resolve<NotRegisteredService>();

            Assert.That(service, Is.Not.Null);
        }

        [Test]
        public void When_service_registered_with_name_Then_it_could_be_resolved_with_ctor_parameter_ImportAttribute()
        {
            var container = new Container();
            container.Setup.AddConstructorParamServiceKeyResolutionRule(AttributedRegistrator.TryGetKeyFromImportAttribute);

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
            container.Setup.AddConstructorParamServiceKeyResolutionRule(AttributedRegistrator.TryGetKeyWithMetadataAttribute);

            container.Register(typeof(IFooService), typeof(FooHey), setup: ServiceSetup.WithMetadata(FooMetadata.Hey));
            container.Register(typeof(IFooService), typeof(FooBlah), setup: ServiceSetup.WithMetadata(FooMetadata.Blah));
            container.Register(typeof(FooConsumer));

            var service = container.Resolve<FooConsumer>();

            Assert.That(service.Foo.Value, Is.InstanceOf<FooBlah>());
        }

        [Test]
        public void Can_resolve_services_from_parent_container()
        {
            var parentContainer = new Container();
            parentContainer.Register(typeof(IFruit), typeof(Orange));

            var container = new Container();
            container.Register(typeof(IJuice), typeof(FruitJuice));

            container.UseRegistrationsFrom(parentContainer);

            var juice = container.Resolve<IJuice>();

            Assert.That(juice, Is.InstanceOf<FruitJuice>());
        }

        [Test]
        public void Once_resolved_I_can_NOT_stop_resolving_services_from_parent_container()
        {
            var parentContainer = new Container();
            parentContainer.Register(typeof(IFruit), typeof(Orange));

            var container = new Container();
            container.Register(typeof(IJuice), typeof(FruitJuice));

            var useRegistrationsFromParent = container.UseRegistrationsFrom(parentContainer);
            var juice = container.Resolve<IJuice>();
            Assert.That(juice, Is.InstanceOf<FruitJuice>());

            container.Setup.RemoveNonRegisteredServiceResolutionRule(useRegistrationsFromParent);
            Assert.DoesNotThrow(
                () => container.Resolve<IJuice>());
        }

        [Test]
        public void I_should_be_able_to_manually_register_open_generic_singletons_and_resolve_them_directly()
        {
            var container = new Container();

            container.Register(
                typeof(IService<>),
                new CustomFactoryProvider((request, _) => new ReflectionFactory(
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
                new CustomFactoryProvider((request, _) => new ReflectionFactory(
                    typeof(Service<>).MakeGenericType(request.ServiceType.GetGenericArguments()),
                    Reuse.Singleton)));

            var service = container.Resolve<ServiceWithTwoSameGenericDependencies>();

            Assert.That(service.Service1, Is.SameAs(service.Service2));
        }
    }

    #region CUT

    internal class SomeService
    {
    }

    internal class Bla<T>
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

    public class FooHey : IFooService
    {
    }

    public class FooBlah : IFooService
    {
    }

    public class FooConsumer
    {
        public Lazy<IFooService> Foo { get; set; }

        public FooConsumer([ImportWithMetadata(FooMetadata.Blah)] Lazy<IFooService> foo)
        {
            Foo = foo;
        }
    }

    public class FruitJuice : IJuice
    {
        public IFruit Fruit { get; set; }

        public FruitJuice(IFruit fruit)
        {
            Fruit = fruit;
        }
    }

    public interface IFruit
    {
    }

    class Orange : IFruit
    {
    }

    public interface IJuice
    {
    }

    #endregion
}