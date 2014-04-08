using System.ComponentModel.Composition;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class ExportImportWithKeyTests
    {
        [Test]
        public void Able_to_export_then_import_service_with_specific_key()
        {
            var container = new Container().WithAttributedModel();
            container.RegisterExports(typeof(KeyClient), typeof(KeyService), typeof(OtherKeyService));

            var client = container.Resolve<KeyClient>();

            Assert.That(client.Service, Is.InstanceOf<KeyService>());
        }
    }

    [Export]
    public class KeyClient
    {
        public IService Service { get; set; }

        public KeyClient([ImportWithKey(ServiceKey.One)]IService service)
        {
            Service = service;
        }
    }

    [ExportWithKey(ServiceKey.One, typeof(IService))]
    public class KeyService : IService {}

    [ExportAll(ContractKey = ServiceKey.OtherOne)]
    public class OtherKeyService : IService { }

    public enum ServiceKey { One, OtherOne }
}
