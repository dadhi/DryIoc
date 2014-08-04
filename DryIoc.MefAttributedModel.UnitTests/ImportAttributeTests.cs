using System;
using System.ComponentModel.Composition;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class ImportAttributeTests
    {
        [Test]
        public void Inject_service_as_parameter_of_service_implemented_type_using_Import_ContractType()
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

        [Export][ExportWithMetadata("blah")]
        public class Service : IService { }

        [Export]
        public class Client
        {
            public IService Some { get; set; }

            public Client([Import(typeof(Service))]IService service)
            {
                Some = service;
            }
        }

        [Export]
        public class FuncClient
        {
            public IService Some { get; set; }

            public FuncClient([Import(typeof(Service))]Func<IService> getService)
            {
                Some = getService();
            }
        }

        [Export]
        public class LazyClient
        {
            public IService Some { get; set; }

            public LazyClient([Import(typeof(Service))]Lazy<IService> getService)
            {
                Some = getService.Value;
            }
        }

        [Export]
        public class LazyMetaClient
        {
            public IService Some { get; set; }
            public string Metadata { get; set; }

            public LazyMetaClient([Import(typeof(Service))]Lazy<Meta<IService, string>> getService)
            {
                Some = getService.Value.Value;
                Metadata = getService.Value.Metadata;
            }
        }

        [Export]
        public class FuncArrayClient
        {
            public IService Some { get; set; }

            public FuncArrayClient([Import(typeof(Service))]Func<IService>[] getService)
            {
                Some = getService[0]();
            }
        }
    }
}
