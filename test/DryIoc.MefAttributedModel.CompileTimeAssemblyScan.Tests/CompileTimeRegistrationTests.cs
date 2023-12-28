using System.Linq;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using DryIocAttributes;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.CompileTimeAssemblyScan.Tests
{
    [TestFixture]
    public class CompileTimeRegistrationTests : ITest
    {
        public int Run()
        {
            Can_register_service_with_constants_alone();
            Can_register_decorator_and_wrapper_with_constants_alone();
            return 2;
        }

        [Test]
        public void Can_register_service_with_constants_alone()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(new[]
            {
                new ExportedRegistrationInfo
                {
                    ImplementationType = typeof(AnotherService),
                    Exports = new[] { new ExportInfo(typeof(IService), "another") },
                    Reuse = new ReuseInfo { ReuseType = ReuseType.Singleton },
                    HasMetadataAttribute = false,
                }
            });

            var service = container.Resolve<IService>("another");

            Assert.NotNull(service);
        }

        [Test]
        public void Can_register_decorator_and_wrapper_with_constants_alone()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(new[]
            {
                new ExportedRegistrationInfo
                {
                    ImplementationType = typeof(Service),
                    Exports = new[] { new ExportInfo(typeof(IService), "some") },
                    Reuse = new ReuseInfo { ReuseType = ReuseType.Singleton },
                    HasMetadataAttribute = false
                },

                new ExportedRegistrationInfo
                {
                    ImplementationType = typeof(AnotherService),
                    Exports = new[] { new ExportInfo(typeof(IService), null) },
                    Reuse = new ReuseInfo { ReuseType = ReuseType.Transient },
                    HasMetadataAttribute = false,
                    FactoryType = FactoryType.Decorator,
                },

                new ExportedRegistrationInfo
                {
                    ImplementationType = typeof(Wrap<>),
                    Exports = new[] { new ExportInfo(typeof(Wrap<>), null) },
                    Reuse = new ReuseInfo { ReuseType = ReuseType.Transient },
                    HasMetadataAttribute = false,
                    FactoryType = FactoryType.Wrapper
                },
            });

            var wrapped = container.Resolve<Wrap<IService>>("some");

            Assert.That(wrapped.Value, Is.InstanceOf<AnotherService>());
        }

        // [Test]
        // public void Can_use_compile_time_generated_registrations()
        // {
        //     var container = new Container().WithMef();
        //     container.RegisterExports(CompileTimeGeneratedRegistrator.Registrations);

        //     // multiple exports of the same type
        //     var importer = container.Resolve<SettingImportHelper>();
        //     Assert.IsNotNull(importer);
        //     Assert.IsNotNull(importer.ImportedValues);
        //     Assert.AreEqual(4, importer.ImportedValues.Length);
        //     Assert.IsTrue(importer.ImportedValues.Contains("Constants.ExportedValue"));
        //     Assert.IsTrue(importer.ImportedValues.Contains("SettingProvider1.ExportedValue"));
        //     Assert.IsTrue(importer.ImportedValues.Contains("SettingProvider2.ExportedValue"));
        //     Assert.IsTrue(importer.ImportedValues.Contains("SettingProvider3.ExportedValue"));

        //     // untyped import
        //     var importer2 = container.Resolve<ImportUntypedService>();
        //     Assert.IsNotNull(importer2);
        //     Assert.IsNotNull(importer2);
        //     Assert.IsNotNull(importer2.UntypedService);
        //     Assert.AreEqual(typeof(UntypedService), importer2.UntypedService.GetType());

        //     // missing optional imports
        //     var service = container.Resolve<NonExistingServiceOptionalImports>();
        //     Assert.IsNotNull(service);
        //     Assert.IsNull(service.NonExistingService);
        //     Assert.IsNull(service.LazyNonExistingService);
        //     Assert.IsNull(service.NonExistingServiceFactory);
        //     Assert.IsNull(service.LazyNonExistingServiceWithMetadata);
        //     Assert.IsNull(service.NonExistingServiceFactoryWithMetadata);

        //     // missing required imports
        //     Assert.Throws<ContainerException>(() => container.Resolve<NonExistingServiceRequiredImport>());
        //     Assert.Throws<ContainerException>(() => container.Resolve<NonExistingServiceRequiredLazyImport>());
        //     Assert.Throws<ContainerException>(() => container.Resolve<NonExistingServiceRequiredExportFactoryImport>());
        //     Assert.Throws<ContainerException>(() => container.Resolve<NonExistingServiceRequiredLazyWithMetadataImport>());
        //     Assert.Throws<ContainerException>(() => container.Resolve<NonExistingServiceRequiredExportFactoryWithMetadataImport>());

        //     // untyped inherited metadata
        //     var service2 = container.Resolve<ImportUntypedInheritedMetadata>();
        //     Assert.IsNotNull(service2);
        //     Assert.IsNotNull(service2.TypedMetadataServices);
        //     Assert.AreEqual(1, service2.TypedMetadataServices.Length);
        //     var metadata = service2.TypedMetadataServices.First().Metadata;
        //     Assert.IsNotNull(metadata);
        //     Assert.AreEqual(123L, metadata.ScriptID);
        //     Assert.AreEqual("Category", metadata.CategoryName);

        //     // action import with metadata
        //     var service3 = container.Resolve<UsesMemberExportWithMetadataExample>();
        //     Assert.IsNotNull(service3);
        //     Assert.IsNotNull(service3.ImportedTestMethodExample);
        //     var metadata3 = service3.ImportedTestMethodExample.Metadata;
        //     Assert.IsNotNull(metadata3);
        //     Assert.AreEqual("Sample", metadata3["Title"]);
        // }
    }

    public class Wrap<T>
    {
        public T Value { get; set; }

        public Wrap(T value)
        {
            Value = value;
        }
    }
}
