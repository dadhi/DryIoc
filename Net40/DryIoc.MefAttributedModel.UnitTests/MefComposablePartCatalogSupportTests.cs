using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class MefComposablePartCatalogSupportTests
    {
        [Test]
        public void Mef_works()
        {
            var catalog = new TypeCatalog(typeof(Yes), typeof(No));

            var container = new CompositionContainer(catalog);

            var yes = container.GetExportedValue<Yes>();
            Assert.IsNotNull(yes);
        }

        [Test]
        public void DryIoc_can_consume_catalog()
        {
            ComposablePartCatalog catalog = new TypeCatalog(typeof(Yes), typeof(No), typeof(Ok));
            foreach (var part in catalog.Parts)
            {
                ExportInfo[] exports = null;
                foreach (var exportDefinition in part.ExportDefinitions)
                {
                    string serviceTypeFullName = null;
                    object exportTypeObject;
                    if (exportDefinition.Metadata.TryGetValue("ExportTypeIdentity", out exportTypeObject))
                        serviceTypeFullName = (string)exportTypeObject;
                    var contractName = exportDefinition.ContractName;
                    var serviceKey = string.Equals(contractName, serviceTypeFullName) ? null : contractName;
                    var export = new ExportInfo(null, serviceKey) { ServiceTypeFullName = serviceTypeFullName };

                    exports = exports.AppendOrUpdate(export);
                }

                var registration = new ExportedRegistrationInfo { Exports = exports };

            }
        }

        public interface IAnswer { }

        [Export]
        public class No { }


        [Export("hey", typeof(IAnswer))]
        public class Ok : IAnswer { }

        [Export]
        public class Yes
        {
            [ImportingConstructor]
            public Yes(No no) { }
        }
    }

}
