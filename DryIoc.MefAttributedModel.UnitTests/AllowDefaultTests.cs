using System.ComponentModel.Composition;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class AllowDefaultTests
    {
        [Test]
        public void AllowDefault_could_be_applied_to_parameter()
        {
            var container = new Container().WithMefAttributedModel();

            container.RegisterExports(typeof(Client));

            var client = container.Resolve<Client>();
            Assert.That(client.Service, Is.Null);
        }

        [Test]
        public void AllowDefault_could_be_applied_to_property()
        {
            var container = new Container().WithMefAttributedModel();

            container.RegisterExports(typeof(ClientProp));

            var client = container.Resolve<ClientProp>();
            Assert.That(client.Service, Is.Null);
        }

        [Test]
        public void AllowDefault_false_applied_to_property_should_Throw()
        {
            var container = new Container().WithMefAttributedModel();

            container.RegisterExports(typeof(ClientPropThrow));

            var ex = Assert.Throws<ContainerException>(() => 
                container.Resolve<ClientPropThrow>());

            Assert.That(ex.Message, Is
                .StringContaining("Unable to resolve").And
                .StringContaining("IService as property \"Service\""));
        }

        [Test]
        public void AllowDefault_dependencies_could_be_nested()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(ClientNested), typeof(Client));

            var client = container.Resolve<ClientNested>();

            Assert.IsNotNull(client.Client);
            Assert.IsNull(client.Client.Service);
        }

        #region CUT

        [Export]
        public class Client
        {
            public IService Service { get; private set; }

            public Client([Import(AllowDefault = true)] IService service)
            {
                Service = service;
            }
        }

        [Export]
        public class ClientProp
        {
            [Import(AllowDefault = true)]
            public IService Service { get; set; }
        }

        [Export]
        public class ClientPropThrow
        {
            [Import(AllowDefault = false)]
            public IService Service { get; set; }
        }

        [Export]
        public class ClientNested
        {
            [Import(AllowDefault = true)]
            public Client Client { get; set; }
        }

        #endregion
    }
}
