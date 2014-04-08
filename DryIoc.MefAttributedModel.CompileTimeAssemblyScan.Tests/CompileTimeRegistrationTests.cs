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
            var container = new Container().WithAttributedModel();
            container.RegisterExports(new[]
            {
                new TypeExportInfo
                {
                    Type = typeof(AnotherService),
                    Exports = new[] { new ExportInfo(typeof(IService), "another") },
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
            var container = new Container().WithAttributedModel();
            container.RegisterExports(new[]
            {
                new TypeExportInfo
                {
                    Type = typeof(Service),
                    Exports = new[] { new ExportInfo(typeof(IService), "some") },
                    IsSingleton = true,
                    MetadataAttributeIndex = -1
                },

                new TypeExportInfo
                {
                    Type = typeof(AnotherService),
                    Exports = new[] { new ExportInfo(typeof(IService)) },
                    IsSingleton = false,
                    MetadataAttributeIndex = -1,
                    FactoryType = FactoryType.Decorator,
                },

                new TypeExportInfo
                {
                    Type = typeof(Wrap<>),
                    Exports = new[] { new ExportInfo(typeof(Wrap<>)) }, 
                    IsSingleton = false,
                    MetadataAttributeIndex = -1,
                    FactoryType = FactoryType.GenericWrapper
                },
            });

            var wrapped = container.Resolve<Wrap<IService>>("some");

            Assert.That(wrapped.Value, Is.InstanceOf<AnotherService>());
        }
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
