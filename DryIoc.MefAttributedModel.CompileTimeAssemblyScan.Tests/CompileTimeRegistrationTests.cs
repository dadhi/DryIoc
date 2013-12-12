using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.CompileTimeAssemblyScan.Tests
{
    [TestFixture]
    public class CompileTimeRegistrationTests
    {
        [Test]
        public void Can_register_service_with_constants_alone()
        {
            var container = new Container(AttributedModel.DefaultSetup);
            container.RegisterExports(new[]
            {
                new TypeExportInfo
                {
                    Type = typeof(AnotherService),
                    Exports = new[] { 
                        new ExportInfo { ServiceType = typeof(IService), ServiceName = "another" }
                    },
                    IsSingleton = true,
                    MetadataAttributeIndex = -1,
                }
            });

            var service = container.Resolve<IService>("another");

            Assert.NotNull(service);
        }

        [Test]
        public void Can_register_decorator_and_wrapper_with_constants_alone()
        {
            var container = new Container(AttributedModel.DefaultSetup);
            container.RegisterExports(new[]
            {
                new TypeExportInfo
                {
                    Type = typeof(Service),
                    Exports = new[] {
                        new ExportInfo { ServiceType = typeof(IService), ServiceName = "some" } },
                    IsSingleton = true,
                    MetadataAttributeIndex = -1
                },

                new TypeExportInfo
                {
                    Type = typeof(AnotherService),
                    Exports = new[] {
                        new ExportInfo { ServiceType = typeof(IService), ServiceName = null } },
                    IsSingleton = false,
                    MetadataAttributeIndex = -1,
                    FactoryType = FactoryType.Decorator,
                },

                new TypeExportInfo
                {
                    Type = typeof(Wrap<>),
                    Exports = new[] {
                        new ExportInfo { ServiceType = typeof(Wrap<>), ServiceName = null } }, IsSingleton = false,
                    MetadataAttributeIndex = -1,
                    FactoryType = FactoryType.GenericWrapper
                },
            });

            var wrapped = container.Resolve<Wrap<IService>>("some");

            Assert.That(wrapped.Value, Is.InstanceOf<AnotherService>());
        }

        //[Test]
        //public void Can_use_compile_time_generated_registrations()
        //{
        //    var container = new Container(AttributedRegistrator.DefaultSetup);
        //    container.RegisterExported(CompileTimeGeneratedRegistrator.Registrations);

        //    Assert.DoesNotThrow(() =>
        //        container.Resolve<Meta<Func<IServiceWithMetadata>, IViewMetadata>[]>());
        //}
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
