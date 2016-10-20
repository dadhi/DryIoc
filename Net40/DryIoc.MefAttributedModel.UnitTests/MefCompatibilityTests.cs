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
            var container = new Container().WithMef();

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
        public void Mef_can_import_many_lazy_services_with_metadata()
        {
            var service = Mef.GetExport<ImportLazyNamedServices>();

            Assert.IsNotNull(service);
            Assert.IsNotNull(service.Value);
            Assert.IsNotNull(service.Value.LazyNamedServices);

            var services = service.Value.LazyNamedServices.OrderBy(l => l.Metadata.Name).ToArray();
            Assert.AreEqual(2, services.Length);
            Assert.AreEqual("One", services[0].Metadata.Name);
            Assert.AreEqual("Two", services[1].Metadata.Name);
            Assert.IsNotNull(services[0].Value);
            Assert.IsNotNull(services[1].Value);
        }

        [Test]
        public void Mef_optional_imports_of_nonexisting_service_are_null()
        {
            var service = Mef.GetExport<NonExistingServiceOptionalImports>().Value;

            Assert.IsNotNull(service);
            Assert.IsNull(service.NonExistingService);
            Assert.IsNull(service.LazyNonExistingService);
            Assert.IsNull(service.NonExistingServiceFactory);
            Assert.IsNull(service.LazyNonExistingServiceWithMetadata);
            Assert.IsNull(service.NonExistingServiceFactoryWithMetadata);
        }

        [Test]
        public void Mef_supports_multiple_metadata_attributes()
        {
            var service = Mef.GetExport<ImportStuffWithMultipleMetadataAttributes>().Value;

            Assert.IsNotNull(service);
            Assert.IsNotNull(service.Scripts);
            Assert.AreEqual(1, service.Scripts.Length);
            Assert.AreEqual(123L, service.Scripts.First().Metadata.ScriptID);
            Assert.IsNotNull(service.Scripts.First().Value);
            Assert.IsNotNull(service.NamedServices);
            Assert.AreEqual(1, service.NamedServices.Length);
            Assert.AreEqual("MultipleMetadata", service.NamedServices.First().Metadata.Name);
            Assert.IsNotNull(service.NamedServices.First().Value);
        }
    }
}
