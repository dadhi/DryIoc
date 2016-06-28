using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class MefCompatibilityTests
    {
        private CompositionContainer Mef => new CompositionContainer(new AssemblyCatalog(typeof(ILogTableManager).Assembly));

        private IContainer Container => CreateContainer();

        private static IContainer CreateContainer()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(new[] { typeof(ILogTableManager).GetAssembly() });
            return container;
        }

        [Test]
        public void Mef_supports_importing_static_factory_method()
        {
            // LogTableManagerConsumer creates ILogTableManager via unnamed factory method with parameters
            var export = Mef.GetExport<LogTableManagerConsumer1>();

            Assert.IsNotNull(export);
            Assert.IsNotNull(export.Value);
            Assert.IsNotNull(export.Value.LogTableManager);
            Assert.AreEqual("SCHEMA1.LOG_ENTRIES", export.Value.LogTableManager.TableName);
        }

        [Test]
        public void Mef_supports_importing_named_static_factory_method()
        {
            // LogTableManagerConsumer creates ILogTableManager via named factory method with parameters
            var export = Mef.GetExport<LogTableManagerConsumer2>();

            Assert.IsNotNull(export);
            Assert.IsNotNull(export.Value);
            Assert.IsNotNull(export.Value.LogTableManager);
            Assert.AreEqual("SCHEMA2.LOG_ENTRIES", export.Value.LogTableManager.TableName);
        }

        [Test]
        public void Mef_supports_named_value_imports_and_exports()
        {
            // SettingImportHelper gathers all exported string settings from the catalog
            var importer = Mef.GetExport<SettingImportHelper>();

            Assert.IsNotNull(importer);
            Assert.IsNotNull(importer.Value);
            Assert.IsNotNull(importer.Value.ImportedValues);
            Assert.AreEqual(4, importer.Value.ImportedValues.Length);

            Assert.IsTrue(importer.Value.ImportedValues.Contains("Constants.ExportedValue"));
            Assert.IsTrue(importer.Value.ImportedValues.Contains("SettingProvider1.ExportedValue"));
            Assert.IsTrue(importer.Value.ImportedValues.Contains("SettingProvider2.ExportedValue"));
            Assert.IsTrue(importer.Value.ImportedValues.Contains("SettingProvider3.ExportedValue"));
        }

        [Test]
        public void Mef_supports_multiple_contract_names_on_same_service_type()
        {
            var importer = Mef.GetExport<ImportAllProtocolVersions>();

            Assert.IsNotNull(importer);
            Assert.IsNotNull(importer.Value);
            Assert.IsNotNull(importer.Value.Protocols);
            Assert.AreEqual(4, importer.Value.Protocols.Length);

            Assert.IsTrue(importer.Value.Protocols.Any(v => v.Version == "1.0"));
            Assert.IsTrue(importer.Value.Protocols.Any(v => v.Version == "2.0"));
            Assert.IsTrue(importer.Value.Protocols.Any(v => v.Version == "3.0"));
            Assert.IsTrue(importer.Value.Protocols.Any(v => v.Version == "4.0"));
        }

        [Test] // fails
        public void DryIoc_supports_importing_static_factory_method()
        {
            // LogTableManagerConsumer creates ILogTableManager via unnamed factory method with parameters
            var export = Container.Resolve<LogTableManagerConsumer1>();

            Assert.IsNotNull(export);
            Assert.IsNotNull(export.LogTableManager);
            Assert.AreEqual("SCHEMA1.LOG_ENTRIES", export.LogTableManager.TableName);
        }

        [Test] // fails
        public void DryIoc_supports_importing_named_static_factory_method()
        {
            // LogTableManagerConsumer creates ILogTableManager via named factory method with parameters
            var export = Container.Resolve<LogTableManagerConsumer2>();

            Assert.IsNotNull(export);
            Assert.IsNotNull(export.LogTableManager);
            Assert.AreEqual("SCHEMA2.LOG_ENTRIES", export.LogTableManager.TableName);
        }

        [Test] // fails
        public void DryIoc_supports_named_value_imports_and_exports()
        {
            // SettingImportHelper gathers all exported string settings from the catalog
            var importer = Container.Resolve<SettingImportHelper>();

            Assert.IsNotNull(importer);
            Assert.IsNotNull(importer.ImportedValues);
            Assert.AreEqual(4, importer.ImportedValues.Length);

            Assert.IsTrue(importer.ImportedValues.Contains("Constants.ExportedValue"));
            Assert.IsTrue(importer.ImportedValues.Contains("SettingProvider1.ExportedValue"));
            Assert.IsTrue(importer.ImportedValues.Contains("SettingProvider2.ExportedValue"));
            Assert.IsTrue(importer.ImportedValues.Contains("SettingProvider3.ExportedValue"));
        }

        [Test] // fails
        public void DryIoc_supports_multiple_contract_names_on_same_service_type()
        {
            var importer = Container.Resolve<ImportAllProtocolVersions>();

            Assert.IsNotNull(importer);
            Assert.IsNotNull(importer.Protocols);
            Assert.AreEqual(4, importer.Protocols.Length);

            Assert.IsTrue(importer.Protocols.Any(v => v.Version == "1.0"));
            Assert.IsTrue(importer.Protocols.Any(v => v.Version == "2.0"));
            Assert.IsTrue(importer.Protocols.Any(v => v.Version == "3.0"));
            Assert.IsTrue(importer.Protocols.Any(v => v.Version == "4.0"));
        }
    }
}
