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
            container.RegisterExports(typeof(LazyClient), typeof(Service));

            container.Resolve<LazyClient>();
        }

        [Export]
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
        public class LazyClient
        {
            public IService Some { get; set; }

            public LazyClient([Import(typeof(Service))]Func<IService> getService)
            {
                Some = getService();
            }
        }
    }
}
