using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class ExportImportWithKeyTests : ITest
    {
        public int Run()
        {
            Able_to_export_then_import_service_with_specific_key();
            return 1;
        }

        [Test]
        public void Able_to_export_then_import_service_with_specific_key()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(KeyClient), typeof(KeyService), typeof(OtherKeyService));

            var client = container.Resolve<KeyClient>();

            Assert.That(client.Service, Is.InstanceOf<KeyService>());
        }
    }
}
