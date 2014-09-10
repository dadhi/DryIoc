using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class ImportAttributeTests
    {
        [Test]
        public void Inject_service_as_parameter_of_required_service_type_specified_by_Import_ContractType()
        {
            var container = new Container().WithAttributedModel();
            container.RegisterExports(typeof(Client), typeof(Service));

            container.Resolve<Client>();
        }

        [Test]
        public void Inject_service_as_Func_of_Service_with_Import_contract_type()
        {
            var container = new Container().WithAttributedModel();
            container.RegisterExports(typeof(FuncClient), typeof(Service));

            var client = container.Resolve<FuncClient>();

            Assert.That(client.Some, Is.InstanceOf<Service>());
        }

        [Test]
        public void Inject_service_as_Lazy_of_Service_with_Import_contract_type()
        {
            var container = new Container().WithAttributedModel();
            container.RegisterExports(typeof(LazyClient), typeof(Service));

            var client = container.Resolve<LazyClient>();

            Assert.That(client.Some, Is.InstanceOf<Service>());
        }

        [Test]
        public void Inject_service_as_Lazy_Meta_of_Service_with_Import_contract_type()
        {
            var container = new Container().WithAttributedModel();
            container.RegisterExports(typeof(LazyMetaClient), typeof(Service));

            var client = container.Resolve<LazyMetaClient>();

            Assert.That(client.Some, Is.InstanceOf<Service>());
            Assert.That(client.Metadata, Is.EqualTo("blah"));
        }

        [Test]
        public void Inject_service_as_Func_Array_of_Service_with_Import_contract_type()
        {
            var container = new Container().WithAttributedModel();
            container.RegisterExports(typeof(FuncArrayClient), typeof(Service));

            var client = container.Resolve<FuncArrayClient>();

            Assert.That(client.Some, Is.InstanceOf<Service>());
        }

        [Test]
        public void Inject_service_as_Func_Array_of_Service_with_Import_contract_key_no_type_specified()
        {
            var container = new Container().WithAttributedModel();
            container.RegisterExports(typeof(FuncArrayKeyClient), typeof(Service));

            Assert.Throws<ContainerException>(
                () => container.Resolve<FuncArrayKeyClient>());

            container.RegisterExports(typeof(KeyService));
            container.Resolve<FuncArrayKeyClient>();
        }

        [Test]
        public void Inject_property_with_default_Import_should_work()
        {
            var container = new Container().WithAttributedModel();
            container.RegisterExports(typeof(PropertyClient), typeof(Service));

            var client = container.Resolve<PropertyClient>();

            Assert.That(client.Some, Is.InstanceOf<Service>());
        }

        [Test]
        public void Resolve_property_for_already_resolved_instance()
        {
            var container = new Container().WithAttributedModel();
            container.RegisterExports(typeof(Service));

            var client = new PropertyClient();
            container.ResolvePropertiesAndFields(client);

            Assert.That(client.Some, Is.InstanceOf<Service>());
        }

        [Test]
        public void Resolving_unregistered_property_marked_by_Import_should_throw()
        {
            var container = new Container().WithAttributedModel();
            container.RegisterExports(typeof(Service));

            var client = new NamedPropertyClient();
            Assert.Throws<ContainerException>(() => 
                container.ResolvePropertiesAndFields(client));

            Assert.That(client.Some, Is.Null);
        }

        [Test]
        public void Resolving_unregistered_property_without_attribute_model_should_not_throw_so_the_property_stays_null()
        {
            var container = new Container();
            container.Register<Service>();

            var client = new NamedPropertyClient();
            container.ResolvePropertiesAndFields(client);

            Assert.That(client.Some, Is.Null);
        }

        [Test]
        public void Resolving_registered_property_with_not_assignable_type_should_Throw()
        {
            var container = new Container().WithAttributedModel();
            container.Register<Service>();
            container.Register<BadTypePropertyClient>();

            var ex = Assert.Throws<ContainerException>(() => 
                container.Resolve<BadTypePropertyClient>());

            Assert.That(ex.Message, Is.StringStarting("Provided service type").And.StringContaining("is not assignable"));
        }

        [Test]
        public void Resolve_custom_generic_wrapper_marked_with_Import()
        {
            var container = new Container().WithAttributedModel();
            container.Register(typeof(MyWrapper<>), setup: GenericWrapperSetup.Default);

            container.RegisterExports(typeof(Service), typeof(CustomWrapperClient));

            var client = container.Resolve<CustomWrapperClient>();
            Assert.NotNull(client);
        }

        [Test]
        public void Resolve_array_of_specified_type_should_work()
        {
            var container = new Container();
            container.Register<IService, Service>();

            var services = container.Resolve<object[]>(typeof(IService));

            Assert.That(services.Length, Is.EqualTo(1));
        }

        [Test]
        public void Resolve_Enumerable_of_specified_type_should_work()
        {
            var container = new Container();
            container.Register<IService, Service>();

            var services = container.Resolve<IEnumerable<Func<object>>>(typeof(IService));

            Assert.That(services.Count(), Is.EqualTo(1));
            Assert.That(services.First().Invoke(), Is.InstanceOf<Service>());
        }

        [Test]
        public void Resolve_Many_of_provided_type_should_work()
        {
            var container = new Container();
            container.Register<IService, Service>();

            var services = container.Resolve<Many<object>>(typeof(IService));

            Assert.That(services.Items.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Resolve_Many_services_twice_with_different_provided_types_should_work()
        {
            var container = new Container();
            container.Register<IService, Service>(Reuse.InCurrentScope);

            var objects = container.Resolve<Many<object>>(typeof(IService));
            var services = container.Resolve<Many<IService>>(typeof(IService));

            CollectionAssert.AreEqual(objects.Items.Cast<IService>().ToArray(), services.Items.ToArray());
        }

        [Test]
        public void Resolve_Meta_of_provided_type_should_work()
        {
            var container = new Container();
            container.Register<IService, Service>(setup: Setup.WithMetadata("a"));
            container.Register<IService, AnotherService>(setup: Setup.WithMetadata("b"));

            var services = container.Resolve<Meta<Func<object>, string>[]>(typeof(IService));

            Assert.That(services[0].Metadata, Is.EqualTo("a"));
            Assert.That(services[0].Value(), Is.InstanceOf<Service>());
            Assert.That(services[1].Metadata, Is.EqualTo("b"));
        }

        #region CUT

        [Export, WithMetadata("blah")]
        public class Service : IService {}

        [Export]
        public class Client
        {
            public IService Some { get; set; }

            public Client([Import(typeof(Service))] IService service)
            {
                Some = service;
            }
        }

        [Export]
        public class FuncClient
        {
            public IService Some { get; set; }

            public FuncClient([Import(typeof (Service))] Func<IService> getService)
            {
                Some = getService();
            }
        }

        [Export]
        public class LazyClient
        {
            public IService Some { get; set; }

            public LazyClient([Import(typeof (Service))] Lazy<IService> getService)
            {
                Some = getService.Value;
            }
        }

        [Export]
        public class LazyMetaClient
        {
            public IService Some { get; set; }
            public string Metadata { get; set; }

            public LazyMetaClient([Import(typeof (Service))] Lazy<Meta<IService, string>> getService)
            {
                Some = getService.Value.Value;
                Metadata = getService.Value.Metadata;
            }
        }

        [Export]
        public class FuncArrayClient
        {
            public IService Some { get; set; }

            public FuncArrayClient([Import(typeof (Service))] Func<IService>[] getService)
            {
                Some = getService[0]();
            }
        }

        [Export("k", typeof (IService))]
        public class KeyService : IService {}

        [Export]
        public class FuncArrayKeyClient
        {
            public IService Some { get; set; }

            public FuncArrayKeyClient([Import("k")] Func<IService>[] getService)
            {
                Some = getService[0]();
            }
        }

        [Export]
        public class PropertyClient
        {
            [Import(typeof (Service))]
            public IService Some { get; set; }
        }

        [Export]
        public class NamedPropertyClient
        {
            [Import("k", typeof (Service))]
            public IService Some { get; set; }
        }

        [Export]
        public class BadTypePropertyClient
        {
            [Import(typeof (BadType))]
            public IService Some { get; set; }

            public class BadType {}
        }

        [Export]
        public class CustomWrapperClient
        {
            [Import(typeof(Service))]
            public MyWrapper<IService> Some { get; set; }
        }

        public class MyWrapper<T>
        {
            public T Value { get; set; }

            public MyWrapper(T value)
            {
                Value = value;
            }
        }

        #endregion
    }
}
