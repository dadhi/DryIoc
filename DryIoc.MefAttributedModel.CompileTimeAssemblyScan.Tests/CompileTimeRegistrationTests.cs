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
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(new[]
            {
                new ExportedRegistrationInfo
                {
                    ImplementationType = typeof(AnotherService),
                    Exports = new[] { new ExportInfo(typeof(IService), "another") },
                    Reuse = SupportedReuse.Singleton,
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
                    Reuse = SupportedReuse.Singleton,
                    HasMetadataAttribute = false
                },

                new ExportedRegistrationInfo
                {
                    ImplementationType = typeof(AnotherService),
                    Exports = new[] { new ExportInfo(typeof(IService), null) },
                    Reuse = SupportedReuse.Transient,
                    HasMetadataAttribute = false,
                    FactoryType = FactoryType.Decorator,
                },

                new ExportedRegistrationInfo
                {
                    ImplementationType = typeof(Wrap<>),
                    Exports = new[] { new ExportInfo(typeof(Wrap<>), null) }, 
                    Reuse = SupportedReuse.Transient,
                    HasMetadataAttribute = false,
                    FactoryType = FactoryType.Wrapper
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
