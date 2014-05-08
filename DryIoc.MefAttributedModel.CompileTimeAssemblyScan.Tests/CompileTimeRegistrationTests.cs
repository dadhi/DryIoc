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
                new RegistrationInfo
                {
                    ImplementationType = typeof(AnotherService),
                    Exports = new[] { new ExportInfo(typeof(IService), "another") },
                    IsSingleton = true,
                    HasMetadataAttribute = false,
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
                new RegistrationInfo
                {
                    ImplementationType = typeof(Service),
                    Exports = new[] { new ExportInfo(typeof(IService), "some") },
                    IsSingleton = true,
                    HasMetadataAttribute = false
                },

                new RegistrationInfo
                {
                    ImplementationType = typeof(AnotherService),
                    Exports = new[] { new ExportInfo(typeof(IService), null) },
                    IsSingleton = false,
                    HasMetadataAttribute = false,
                    FactoryType = FactoryType.Decorator,
                },

                new RegistrationInfo
                {
                    ImplementationType = typeof(Wrap<>),
                    Exports = new[] { new ExportInfo(typeof(Wrap<>), null) }, 
                    IsSingleton = false,
                    HasMetadataAttribute = false,
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
