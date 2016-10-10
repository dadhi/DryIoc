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

        [Test, Ignore]
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

        [Test]
        public void Mef_supports_importing_service_as_untyped_property()
        {
            var importer = Mef.GetExport<ImportUntypedService>();

            Assert.IsNotNull(importer);
            Assert.IsNotNull(importer.Value);
            Assert.IsNotNull(importer.Value.UntypedService);
            Assert.AreEqual(typeof(UntypedService), importer.Value.UntypedService.GetType());
        }

        [Test]
        public void Mef_supports_importing_services_as_untyped_array()
        {
            var importer = Mef.GetExport<ImportManyUntypedServices>();

            Assert.IsNotNull(importer);
            Assert.IsNotNull(importer.Value);
            Assert.IsNotNull(importer.Value.UntypedServices);
            Assert.AreEqual(1, importer.Value.UntypedServices.Length);
            Assert.AreEqual(typeof(UntypedService), importer.Value.UntypedServices.First().GetType());
        }

        [Test]
        public void Mef_chooses_the_default_constructor_if_no_constructors_are_marked_with_ImportingConstructorAttribute()
        {
            var service = Mef.GetExport<IServiceWithTwoConstructors>();

            Assert.IsNotNull(service);
            Assert.IsNotNull(service.Value);
            Assert.IsTrue(service.Value.DefaultConstructorIsUsed);
        }

        [Test]
        public void DryIoc_supports_importing_static_factory_method()
        {
            // LogTableManagerConsumer creates ILogTableManager via unnamed factory method with parameters
            var export = Container.Resolve<LogTableManagerConsumer1>();

            Assert.IsNotNull(export);
            Assert.IsNotNull(export.LogTableManager);
            Assert.AreEqual("SCHEMA1.LOG_ENTRIES", export.LogTableManager.TableName);
        }

        [Test]
        public void DryIoc_supports_importing_named_static_factory_method()
        {
            // LogTableManagerConsumer creates ILogTableManager via named factory method with parameters
            var export = Container.Resolve<LogTableManagerConsumer2>();

            Assert.IsNotNull(export);
            Assert.IsNotNull(export.LogTableManager);
            Assert.AreEqual("SCHEMA2.LOG_ENTRIES", export.LogTableManager.TableName);
        }

        [Test]
        public void DryIoc_supports_exporting_instance_member_of_not_exported_type()
        {
            var container = new Container().WithMefAttributedModel();

            container.RegisterExports(typeof(Provider));
            var abc = container.Resolve<Abc>();

            Assert.IsNotNull(abc);
            Assert.IsNull(container.Resolve<Provider>(IfUnresolved.ReturnDefault));
        }

        [Test, Ignore("failes: we need to support multi keys in core")]
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

        [Test]
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

        [Test, Ignore("fails")]
        public void DryIoc_supports_importing_service_as_untyped_property()
        {
            var importer = Container.Resolve<ImportUntypedService>();

            Assert.IsNotNull(importer);
            Assert.IsNotNull(importer);
            Assert.IsNotNull(importer.UntypedService);
            Assert.AreEqual(typeof(UntypedService), importer.UntypedService.GetType());
        }

        [Test, Ignore("fails")]
        public void DryIoc_supports_importing_services_as_untyped_array()
        {
            var importer = Container.Resolve<ImportManyUntypedServices>();

            Assert.IsNotNull(importer);
            Assert.IsNotNull(importer);
            Assert.IsNotNull(importer.UntypedServices);
            Assert.AreEqual(1, importer.UntypedServices.Length);
        }

        [Test]
        public void DryIoc_chooses_the_default_constructor_if_no_constructors_are_marked_with_ImportingConstructorAttribute()
        {
            var service = Container.Resolve<IServiceWithTwoConstructors>();

            Assert.IsNotNull(service);
            Assert.IsTrue(service.DefaultConstructorIsUsed);
        }
    }
}
