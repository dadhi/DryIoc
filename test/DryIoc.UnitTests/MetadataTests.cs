using System;
using System.Collections.Generic;
using System.Linq;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class MetadataTests
    {
        [Test]
        public void I_can_resolve_transient_service_with_metadata()
        {
            var container = new Container();
            container.Register(typeof(ServiceWithMetadata), setup: Setup.With(metadataOrFuncOfMetadata: new Metadata { Assigned = true }));

            var service = container.Resolve<Meta<ServiceWithMetadata, Metadata>>();

            Assert.That(service.Metadata.Assigned, Is.True);
        }

        [Test]
        public void Can_resolve_metadata_as_Tuple()
        {
            var container = new Container();
            container.Register(typeof(ServiceWithMetadata), setup: Setup.With(metadataOrFuncOfMetadata: new Metadata { Assigned = true }));

            var service = container.Resolve<Tuple<ServiceWithMetadata, Metadata>>();

            Assert.That(service.Item1, Is.InstanceOf<ServiceWithMetadata>());
            Assert.That(service.Item2.Assigned, Is.True);
        }

        [Test]
        public void Should_throw_in_case_of_service_with_unresolved_dependency()
        {
            var container = new Container();
            container.Register(typeof(ServiceWithDependencyAndWithMetadata), setup: Setup.With(new Metadata { Assigned = true }));
            
            var ex = Assert.Throws<ContainerException>(() => 
                container.Resolve<Tuple<ServiceWithDependencyAndWithMetadata, Metadata>[]>());

            Assert.AreEqual(Error.NameOf(Error.UnableToResolveUnknownService), Error.NameOf(ex.Error));
        }

        [Test]
        public void I_can_resolve_func_of_transient_service_with_metadata()
        {
            var container = new Container();
            container.Register(typeof(ServiceWithMetadata), setup: Setup.With(metadataOrFuncOfMetadata: new Metadata { Assigned = true }));

            var func = container.Resolve<Meta<Func<ServiceWithMetadata>, Metadata>>();

            Assert.That(func.Metadata.Assigned, Is.True);
            Assert.That(func.Value(), Is.Not.Null);
        }

        [Test]
        public void I_can_resolve_array_of_func_of_transient_service_with_metadata()
        {
            var container = new Container();
            container.Register(typeof(ServiceWithMetadata), setup: Setup.With(metadataOrFuncOfMetadata: new Metadata { Assigned = true }));

            var funcs = container.Resolve<Meta<Func<ServiceWithMetadata>, Metadata>[]>();

            Assert.That(funcs.Length, Is.EqualTo(1));
            Assert.That(funcs[0].Metadata.Assigned, Is.True);
            Assert.That(funcs[0].Value(), Is.Not.Null);
        }

        [Test]
        public void I_can_resolve_array_of_func_of_transient_services_with_metadata()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service), setup: Setup.With(metadataOrFuncOfMetadata: "One"));
            container.Register(typeof(IService), typeof(AnotherService), setup: Setup.With(metadataOrFuncOfMetadata: "Another"));

            var funcs = container.Resolve<Meta<Func<IService>, string>[]>();
            Assert.That(funcs.Length, Is.EqualTo(2));

            var func = funcs.First(x => x.Metadata.Equals("Another"));
            Assert.That(func.Value(), Is.InstanceOf<AnotherService>());
        }

        [Test]
        public void I_can_resolve_singleton_service_with_metadata()
        {
            var container = new Container();
            container.Register<ServiceWithMetadata>(Reuse.Singleton, setup: Setup.With(metadataOrFuncOfMetadata: new Metadata { Assigned = true }));

            var meta = container.Resolve<Meta<Func<ServiceWithMetadata>, Metadata>>();
            Assert.That(meta.Metadata.Assigned, Is.True);

            var anotherService = container.Resolve<Meta<ServiceWithMetadata, Metadata>>();
            Assert.That(meta.Value(), Is.SameAs(anotherService.Value));
        }

        [Test]
        public void I_can_resolve_lazy_service_with_metadata()
        {
            var container = new Container();
            container.Register<ServiceWithMetadata>(Reuse.Singleton, setup: Setup.With(metadataOrFuncOfMetadata: new Metadata { Assigned = true }));

            var meta = container.Resolve<Meta<Lazy<ServiceWithMetadata>, Metadata>>();

            Assert.That(meta.Metadata.Assigned, Is.True);
            Assert.That(meta.Value.Value, Is.InstanceOf<ServiceWithMetadata>());
        }

        [Test]
        public void I_can_resolve_array_of_lazy_with_metadata()
        {
            var container = new Container();
            container.Register<ServiceWithMetadata>(Reuse.Singleton, setup: Setup.With(metadataOrFuncOfMetadata: new Metadata { Assigned = true }));

            // Then
            var metas = container.Resolve<Meta<Lazy<ServiceWithMetadata>, Metadata>[]>();

            Assert.That(metas.Length, Is.EqualTo(1));
            Assert.That(metas[0].Metadata.Assigned, Is.True);
        }

        [Test]
        public void When_singleton_resolve_through_meta_lazy_It_should_not_be_instantiated()
        {
            var container = new Container();
            ServiceWithInstanceCount.InstanceCount = 0;
            container.Register<ServiceWithInstanceCount>(Reuse.Singleton, setup: Setup.With(metadataOrFuncOfMetadata: "hey"));

            container.Resolve<Meta<Lazy<ServiceWithInstanceCount>, string>>();

            Assert.That(ServiceWithInstanceCount.InstanceCount, Is.EqualTo(0));
        }

        [Test]
        public void I_can_resolve_open_generic_with_meta_array_dependency()
        {
            var container = new Container();
            container.Register(typeof(ServiceWithMetadata), setup: Setup.With(metadataOrFuncOfMetadata: new Metadata { Assigned = true }));
            container.Register(typeof(MetadataDrivenFactory<,>));

            var factory = container.Resolve<MetadataDrivenFactory<ServiceWithMetadata, Metadata>>();
            var service = factory.CreateOnlyIf(metadata => metadata.Assigned);

            Assert.That(service, Is.InstanceOf<ServiceWithMetadata>());
        }

        [Test]
        public void I_can_resolve_service_with_dependency_on_open_generic_with_meta_array_dependency()
        {
            var container = new Container();
            container.Register(typeof(ServiceWithDependencyOnOpenGenericWithMetaFactoryMany));
            container.Register(typeof(ServiceWithMetadata), setup: Setup.With(metadataOrFuncOfMetadata: new Metadata { Assigned = true }));
            container.Register(typeof(MetadataDrivenFactory<,>));

            var service = container.Resolve<ServiceWithDependencyOnOpenGenericWithMetaFactoryMany>();
            var dependency = service.Factory.CreateOnlyIf(metadata => metadata.Assigned);

            Assert.That(dependency, Is.InstanceOf<ServiceWithMetadata>());
        }

        [Test]
        public void Resolve_should_throw_if_metadata_is_not_registered()
        {
            var container = new Container();

            container.Register(typeof(IService), typeof(Service));

            Assert.Throws<ContainerException>(
                () => container.Resolve<Meta<IService, Metadata>>());
        }

        [Test]
        public void Resolve_should_throw_if_requested_metadata_is_of_different_type()
        {
            var container = new Container();

            container.Register(typeof(IService), typeof(Service), serviceKey: "oh my!");

            Assert.Throws<ContainerException>(
                () => container.Resolve<Meta<IService, Metadata>>());
        }

        [Test]
        public void When_one_service_is_registered_with_metadata_and_another_without_Resolved_array_should_contain_only_one()
        {
            var container = new Container();

            container.Register(typeof(IService), typeof(Service), setup: Setup.With(metadataOrFuncOfMetadata: "xx"));
            container.Register(typeof(IService), typeof(Service));

            var services = container.Resolve<Meta<IService, string>[]>();

            Assert.That(services.Length, Is.EqualTo(1));
            Assert.That(services.Single().Metadata, Is.EqualTo("xx"));
        }

        [Test]
        public void Should_resolve_open_generic_with_metadata()
        {
            var container = new Container();
            container.Register(typeof(IService<>), typeof(Service<>), setup: Setup.With(metadataOrFuncOfMetadata: "ho"));

            var service = container.Resolve<Meta<IService<int>, string>>();

            Assert.That(service.Value, Is.InstanceOf<Service<int>>());
        }

        [Test]
        public void Should_NOT_resolve_meta_with_name_if_no_such_name_registered()
        {
            var container = new Container();
            container.RegisterMany(new[] { typeof(Service<>) }, setup: Setup.With(metadataOrFuncOfMetadata: 3));

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<Meta<IService<object>, int>>("no-no-name"));

            StringAssert.Contains("Unable to resolve", ex.Message);
        }

        [Test]
        public void Should_resolve_any_named_service_with_corresponding_metadata_If_name_is_not_specified_in_resolve()
        {
            var container = new Container();
            container.RegisterMany(new[] { typeof(Service<>) }, setup: Setup.With(metadataOrFuncOfMetadata: 3), serviceKey: "some");

            var meta = container.Resolve<Meta<IService<string>, int>>();

            Assert.That(meta.Metadata, Is.EqualTo(3));
        }

        [Test]
        public void Should_NOT_resolve_any_Scoped_meta_of_service_from_the_root_container()
        {
            var container = new Container();

            container.Register<Zzz>();
            container.Register<IService<string>, Service<string>>(Reuse.Scoped, setup: Setup.With(metadataOrFuncOfMetadata: 3));

            var ex = Assert.Throws<ContainerException>(() => 
                container.Resolve<Zzz>());

        }

        class Zzz
        {
            public Zzz(Meta<IService<string>, int> s) {}
        }

        [Test]
        public void When_one_service_is_registered_with_name_and_other_is_default_only_one_named_should_be_resolved()
        {
            var container = new Container();

            container.Register<IService, Service>(serviceKey: "n", setup: Setup.With(metadataOrFuncOfMetadata: "m"));
            container.Register<IService, Service>();

            var services = container.Resolve<Meta<IService, string>[]>();

            Assert.That(services.Length, Is.EqualTo(1));
            Assert.That(services.Single().Metadata, Is.EqualTo("m"));
        }

        [Test]
        public void Can_inject_service_matching_the_key_value_metadata()
        {
            var container = new Container();

            container.Register<IService, OtherService>();
            container.Register<IService, Service>(
                setup: Setup.With(new Dictionary<string, object> { { "a", 1 }, { "b", 2 } }));

            container.Register<MetaConsumer>(
                made: Parameters.Of.Type<IService>(metadataKey: "b", metadata: 2));

            var consumer = container.Resolve<MetaConsumer>();
            Assert.IsInstanceOf<Service>(consumer.Service);
        }

        [Test]
        public void Can_inject_service_matching_the_key_value_metadata_via_parameter_name()
        {
            var container = new Container();

            container.Register<IService, OtherService>();
            container.Register<IService, Service>(
                setup: Setup.With(new Dictionary<string, object> { { "a", 1 }, { "b", 2 } }));

            container.Register<MetaConsumer>(
                made: Parameters.Of.Name("service", metadataKey: "b", metadata: 2));

            var consumer = container.Resolve<MetaConsumer>();
            Assert.IsInstanceOf<Service>(consumer.Service);
        }

        [Test]
        public void Can_inject_service_matching_the_key_value_metadata_via_ArgOf_spec()
        {
            var container = new Container();

            container.Register<IService, OtherService>();
            container.Register<IService, Service>(
                setup: Setup.With(new Dictionary<string, object> { { "a", 1 }, { "b", 2 } }));

            container.Register(Made.Of(() => new MetaConsumer(Arg.Of<IService>("b", 2))));

            var consumer = container.Resolve<MetaConsumer>();
            Assert.IsInstanceOf<Service>(consumer.Service);
        }

        [Test]
        public void Can_inject_service_matching_the_key_value_metadata_via_property()
        {
            var container = new Container();

            container.Register<IService, OtherService>();
            container.Register<IService, Service>(
                setup: Setup.With(new Dictionary<string, object> { { "a", 1 }, { "b", 2 } }));

            container.Register<MetaPropertyConsumer>(
                made: PropertiesAndFields.Of.Name("Service", metadataKey: "b", metadata: 2));

            var consumer = container.Resolve<MetaPropertyConsumer>();
            Assert.IsInstanceOf<Service>(consumer.Service);
        }

        [Test]
        public void Can_inject_func_of_service_matching_with_the_key_value_metadata()
        {
            var container = new Container();

            container.Register<IService, OtherService>();
            container.Register<IService, Service>(
                setup: Setup.With(new Dictionary<string, object> { { "a", 1 }, { "b", 2 } }));

            container.Register<FuncMetaConsumer>(
                made: Parameters.Of.Type<Func<IService>>(metadataKey: "b", metadata: 2));

            var consumer = container.Resolve<FuncMetaConsumer>();
            Assert.IsInstanceOf<Service>(consumer.Service);
        }

        [Test]
        public void Can_inject_lazy_of_service_matching_with_the_key_value_metadata()
        {
            var container = new Container();

            container.Register<IService, OtherService>();
            container.Register<IService, Service>(
                setup: Setup.With(new Dictionary<string, object> { { "a", 1 }, { "b", 2 } }));

            container.Register<LazyMetaConsumer>(
                made: Parameters.Of.Type<Lazy<IService>>(metadataKey: "b", metadata: 2));

            var consumer = container.Resolve<LazyMetaConsumer>();
            Assert.IsInstanceOf<Service>(consumer.Service);
        }

        [Test]
        public void Can_inject_service_matching_the_value_only_of_metadata()
        {
            var container = new Container();

            container.Register<IService, OtherService>(
                setup: Setup.With(1));
            container.Register<IService, Service>(
                setup: Setup.With(new Dictionary<string, object> { { "b", 2 } }));

            container.Register<MetaConsumer>(
                made: Parameters.Of.Type<IService>(metadata: 1));

            var consumer = container.Resolve<MetaConsumer>();
            Assert.IsInstanceOf<OtherService>(consumer.Service);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void Can_inject_collection_matching_the_metadata_key_value(bool lazyEnumarable)
        {
            IContainer container = new Container();
            if (lazyEnumarable)
                container = container.With(rules => rules.WithResolveIEnumerableAsLazyEnumerable());

            container.Register<IService, Service>(
                setup: Setup.With(new Dictionary<string, object> { { "a", 1 } }));
            container.Register<IService, OtherService>(
                setup: Setup.With(new Dictionary<string, object> { { "b", 2 } }));

            container.Register(Made.Of(() => new ManyMetaConsumer(Arg.Of<IEnumerable<IService>>("b", 2))));

            var consumer = container.Resolve<ManyMetaConsumer>();
            Assert.AreEqual(1, consumer.Services.Length);
        }

        public class OtherService : IService { }

        public class MetaConsumer
        {
            public IService Service { get; private set; }

            public MetaConsumer(IService service)
            {
                Service = service;
            }
        }

        public class MetaPropertyConsumer
        {
            public IService Service { get; set; }
        }

        public class FuncMetaConsumer
        {
            public IService Service { get; private set; }

            public FuncMetaConsumer(Func<IService> getService)
            {
                Service = getService();
            }
        }

        public class LazyMetaConsumer
        {
            public IService Service { get; private set; }

            public LazyMetaConsumer(Lazy<IService> service)
            {
                Service = service.Value;
            }
        }

        public class ManyMetaConsumer
        {
            public IService[] Services { get; private set; }

            public ManyMetaConsumer(IEnumerable<IService> services)
            {
                Services = services.ToArray();
            }
        }

        public class ServiceWithMetadata
        {

        }

        public class ServiceWithDependencyAndWithMetadata
        {
            public string Dependency { get; set; }
            public ServiceWithDependencyAndWithMetadata(string dependency)
            {
                Dependency = dependency;
            }
        }

        public class Metadata
        {
            public bool Assigned;
        }

        public class MetadataDrivenFactory<TService, TMetadata>
        {
            private readonly Meta<Func<TService>, TMetadata>[] _factories;

            public MetadataDrivenFactory(Meta<Func<TService>, TMetadata>[] factories)
            {
                _factories = factories;
            }

            public TService CreateOnlyIf(Func<TMetadata, bool> condition)
            {
                var factory = _factories.First(meta => condition(meta.Metadata));
                return factory.Value();
            }
        }

        public class ServiceWithDependencyOnOpenGenericWithMetaFactoryMany
        {
            public MetadataDrivenFactory<ServiceWithMetadata, Metadata> Factory { get; set; }

            public ServiceWithDependencyOnOpenGenericWithMetaFactoryMany(MetadataDrivenFactory<ServiceWithMetadata, Metadata> factory)
            {
                Factory = factory;
            }
        }
    }
}