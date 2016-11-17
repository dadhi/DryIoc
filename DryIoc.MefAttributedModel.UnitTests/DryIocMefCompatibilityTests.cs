using System.Linq;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class DryIocMefCompatibilityTests
    {
        private IContainer Container => CreateContainer();

        private static IContainer CreateContainer()
        {
            var container = new Container().WithMef();

            container.RegisterExports(new[] { typeof(ILogTableManager).GetAssembly() });
            return container;
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

        [Test]
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

        [Test]
        public void DryIoc_supports_importing_service_as_untyped_property()
        {
            var importer = Container.Resolve<ImportUntypedService>();

            Assert.IsNotNull(importer);
            Assert.IsNotNull(importer);
            Assert.IsNotNull(importer.UntypedService);
            Assert.AreEqual(typeof(UntypedService), importer.UntypedService.GetType());
        }

        [Test]
        public void DryIoc_supports_importing_services_as_untyped_array()
        {
            var container = new Container().WithMef();

            container.RegisterExports(typeof(ImportManyUntypedServices), typeof(UntypedService));

            var importer = container.Resolve<ImportManyUntypedServices>();

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

        [Test]
        public void DryIoc_supports_ExportFactory_for_non_shared_parts()
        {
            var container = Container;
            var service = container.Resolve<UsesExportFactoryOfNonSharedService>();

            Assert.IsNotNull(service);
            Assert.IsNotNull(service.Factory);

            NonSharedDependency nonSharedDependency;
            using (var scope = container.OpenScope())
            {
                nonSharedDependency = scope.Resolve<NonSharedDependency>();
                Assert.IsFalse(nonSharedDependency.IsDisposed);
            }

            Assert.IsTrue(nonSharedDependency.IsDisposed);

            NonSharedService nonSharedService;
            using (var export = service.Factory.CreateExport())
            {
                nonSharedService = export.Value;
                Assert.IsNotNull(nonSharedService);
                Assert.IsFalse(nonSharedService.IsDisposed);

                Assert.IsNotNull(nonSharedService.NonSharedDependency);
                Assert.IsFalse(nonSharedService.NonSharedDependency.IsDisposed);

                Assert.IsNotNull(nonSharedService.SharedDependency);
                Assert.IsFalse(nonSharedService.SharedDependency.IsDisposed);
            }

            Assert.IsTrue(nonSharedService.IsDisposed);
            Assert.IsTrue(nonSharedService.NonSharedDependency.IsDisposed);
            Assert.IsFalse(nonSharedService.SharedDependency.IsDisposed);
        }

        [Test]
        public void DryIoc_supports_ExportFactory_for_parts_with_unspecified_creation_policy()
        {
            var container = Container;
            var service = container.Resolve<UsesExportFactoryOfUnspecifiedCreationPolicyService>();

            Assert.IsNotNull(service);
            Assert.IsNotNull(service.Factory);

            UnspecifiedCreationPolicyService unspecifiedCreationPolicyService;
            using (var export = service.Factory.CreateExport())
            {
                unspecifiedCreationPolicyService = export.Value;
                Assert.IsNotNull(unspecifiedCreationPolicyService);
                Assert.IsFalse(unspecifiedCreationPolicyService.IsDisposed);

                Assert.IsNotNull(unspecifiedCreationPolicyService.NonSharedDependency);
                Assert.IsFalse(unspecifiedCreationPolicyService.NonSharedDependency.IsDisposed);

                Assert.IsNotNull(unspecifiedCreationPolicyService.SharedDependency);
                Assert.IsFalse(unspecifiedCreationPolicyService.SharedDependency.IsDisposed);
            }

            Assert.IsTrue(unspecifiedCreationPolicyService.IsDisposed);
            Assert.IsTrue(unspecifiedCreationPolicyService.NonSharedDependency.IsDisposed);
            Assert.IsFalse(unspecifiedCreationPolicyService.SharedDependency.IsDisposed);
        }

        [Test]
        public void DryIoc_supports_ExportFactory_for_shared_parts()
        {
            var container = Container;
            var service = container.Resolve<UsesExportFactoryOfSharedService>();

            Assert.IsNotNull(service);
            Assert.IsNotNull(service.Factory);

            SharedService sharedService;
            using (var export = service.Factory.CreateExport())
            {
                sharedService = export.Value;
                Assert.IsNotNull(sharedService);
                Assert.IsFalse(sharedService.IsDisposed);
            }

            Assert.IsFalse(sharedService.IsDisposed);
            Assert.IsFalse(sharedService.NonSharedDependency.IsDisposed);
            Assert.IsFalse(sharedService.SharedDependency.IsDisposed);

            container.Dispose();
            Assert.IsTrue(sharedService.IsDisposed);
            Assert.IsTrue(sharedService.NonSharedDependency.IsDisposed);
            Assert.IsTrue(sharedService.SharedDependency.IsDisposed);
        }

        [Test]
        public void DryIoc_supports_ExportFactoryWithMetadata_for_non_shared_parts()
        {
            var container = Container;
            var service = container.Resolve<ImportsNamedServiceExportFactories>();
            Assert.IsNotNull(service);
            Assert.IsNotNull(service.NamedServiceFactories);

            var services = service.NamedServiceFactories.OrderBy(s => s.Metadata.Name).ToArray();
            Assert.AreEqual(2, services.Length);
            Assert.AreEqual("One", services[0].Metadata.Name);
            Assert.AreEqual("Two", services[1].Metadata.Name);

            LazyNamedService1 ls1;
            LazyNamedService2 ls2;

            using (var s1 = services[0].CreateExport())
            {
                Assert.IsNotNull(s1.Value);
                Assert.IsInstanceOf<LazyNamedService1>(s1.Value);
                Assert.IsFalse(s1.Value.IsDisposed);
                ls1 = (LazyNamedService1)s1.Value;

                using (var s2 = services[1].CreateExport())
                {
                    Assert.IsNotNull(s2.Value);
                    Assert.IsInstanceOf<LazyNamedService2>(s2.Value);
                    Assert.IsFalse(s2.Value.IsDisposed);
                    Assert.IsNotNull(s1.Value);
                    Assert.IsFalse(s1.Value.IsDisposed);

                    ls2 = (LazyNamedService2)s2.Value;
                    Assert.IsNotNull(ls2.NonSharedDependency);
                    Assert.IsFalse(ls2.NonSharedDependency.IsDisposed);
                }

                Assert.IsTrue(ls2.IsDisposed);
                Assert.IsNotNull(ls2.NonSharedDependency);
                Assert.IsTrue(ls2.NonSharedDependency.IsDisposed);
                Assert.IsFalse(s1.Value.IsDisposed);
            }

            Assert.IsTrue(ls1.IsDisposed);
        }

        [Test]
        public void DryIoc_can_import_many_lazy_services_with_metadata()
        {
            var service = Container.Resolve<ImportLazyNamedServices>();

            Assert.IsNotNull(service);
            Assert.IsNotNull(service.LazyNamedServices);

            var services = service.LazyNamedServices.OrderBy(l => l.Metadata.Name).ToArray();
            Assert.AreEqual(2, services.Length);
            Assert.AreEqual("One", services[0].Metadata.Name);
            Assert.AreEqual("Two", services[1].Metadata.Name);
            Assert.IsNotNull(services[0].Value);
            Assert.IsNotNull(services[1].Value);
        }

        [Test]
        public void DryIoc_optional_imports_of_nonexisting_service_are_null()
        {
            var service = Container.Resolve<NonExistingServiceOptionalImports>();

            Assert.IsNotNull(service);
            Assert.IsNull(service.NonExistingService);
            Assert.IsNull(service.LazyNonExistingService);
            Assert.IsNull(service.NonExistingServiceFactory);
            Assert.IsNull(service.LazyNonExistingServiceWithMetadata);
            Assert.IsNull(service.NonExistingServiceFactoryWithMetadata);
        }

        [Test]
        public void DryIoc_required_import_of_nonexisting_service_cannot_be_resolved()
        {
            Assert.Throws<ContainerException>(() => Container.Resolve<NonExistingServiceRequiredImport>());
            Assert.Throws<ContainerException>(() => Container.Resolve<NonExistingServiceRequiredLazyImport>());
            Assert.Throws<ContainerException>(() => Container.Resolve<NonExistingServiceRequiredExportFactoryImport>());
            Assert.Throws<ContainerException>(() => Container.Resolve<NonExistingServiceRequiredLazyWithMetadataImport>());
            Assert.Throws<ContainerException>(() => Container.Resolve<NonExistingServiceRequiredExportFactoryWithMetadataImport>());
        }

        [Test]
        public void DryIoc_supports_multiple_metadata_attributes()
        {
            var service = Container.Resolve<ImportStuffWithMultipleMetadataAttributes>();

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

        [Test]
        public void DryIoc_allows_importing_untyped_metadata()
        {
            var service = Container.Resolve<ImportUntypedInheritedMetadata>();

            Assert.IsNotNull(service);
            Assert.IsNotNull(service.UntypedMetadataServices);
            Assert.AreEqual(1, service.UntypedMetadataServices.Length);

            var metadata = service.UntypedMetadataServices.First().Metadata;
            Assert.IsNotNull(metadata);
            Assert.AreEqual(123L, metadata["ScriptID"]);
            Assert.AreEqual("Category", metadata["CategoryName"]);
            Assert.AreEqual("AlsoSupported", metadata["DryIocMetadata"]);
        }

        [Test]
        public void DryIoc_supports_metadata_attribute_hierarchy_properly()
        {
            var service = Container.Resolve<ImportUntypedInheritedMetadata>();

            Assert.IsNotNull(service);
            Assert.IsNotNull(service.TypedMetadataServices);
            Assert.AreEqual(1, service.TypedMetadataServices.Length);

            var metadata = service.TypedMetadataServices.First().Metadata;
            Assert.IsNotNull(metadata);
            Assert.AreEqual(123L, metadata.ScriptID);
            Assert.AreEqual("Category", metadata.CategoryName);
        }

        [Test]
        public void DryIoc_calls_ImportSatisfied_for_non_shared_parts_once()
        {
            var container = new Container().WithMef();
            container.RegisterExports(typeof(NonSharedWithImportSatisfiedNotification));

            var service1 = container.Resolve<NonSharedWithImportSatisfiedNotification>();
            var service2 = container.Resolve<NonSharedWithImportSatisfiedNotification>();

            Assert.AreNotSame(service1, service2);
            Assert.AreEqual(1, service1.ImportsSatisfied);
            Assert.AreEqual(1, service2.ImportsSatisfied);
        }

        [Test]
        public void DryIoc_calls_ImportSatisfied_for_shared_parts_once()
        {
            var container = Container;
            var service1 = container.Resolve<SharedWithImportSatisfiedNotification>();
            var service2 = container.Resolve<SharedWithImportSatisfiedNotification>();

            Assert.AreSame(service1, service2);
            Assert.AreEqual(1, service1.ImportsSatisfied);
            Assert.AreEqual(1, service2.ImportsSatisfied);
        }

        [Test]
        public void DryIoc_can_import_member_with_metadata()
        {
            var container = new Container().WithMef();

            // added explicit export registrations for better debug
            container.RegisterExports(
                typeof(MemberExportWithMetadataExample),
                typeof(UsesMemberExportWithMetadataExample));

            var service = container.Resolve<UsesMemberExportWithMetadataExample>();

            Assert.IsNotNull(service);
            Assert.IsNotNull(service.ImportedTestMethodExample);

            var metadata = service.ImportedTestMethodExample.Metadata;
            Assert.IsNotNull(metadata);
            Assert.AreEqual("Sample", metadata["Title"]);
        }
    }
}