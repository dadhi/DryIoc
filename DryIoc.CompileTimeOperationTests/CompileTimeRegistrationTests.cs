using System;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.CompileTimeOperationTests
{
    [TestFixture]
    public class CompileTimeRegistrationTests
    {
        [Test]
        public void Can_register_service_with_constants_alone()
        {
            var container = new Container(AttributedRegistrator.DefaultSetup);
            container.RegisterExported(new[]
            {
                new RegistrationInfo
                {
                    ImplementationType = typeof(AnotherService),
                    IsSingleton = true,
                    MetadataAttributeIndex = -1,
                    Exports = new[]
                    {
                        new ExportInfo { ServiceType = typeof(IService), ServiceName = "another" }
                    },
                    FactorySetupInfo = null
                }
            });

            var service = container.Resolve<IService>("another");

            Assert.NotNull(service);
        }

        [Test]
        public void Can_register_decorator_and_wrapper_with_constants_alone()
        {
            var container = new Container(AttributedRegistrator.DefaultSetup);
            container.RegisterExported(new[]
            {
                new RegistrationInfo
                {
                    ImplementationType = typeof(Service),
                    IsSingleton = true,
                    MetadataAttributeIndex = -1,
                    Exports = new[] {
                        new ExportInfo { ServiceType = typeof(IService), ServiceName = "some" } },
                    FactorySetupInfo = null
                },

                new RegistrationInfo
                {
                    ImplementationType = typeof(AnotherService),
                    IsSingleton = false,
                    MetadataAttributeIndex = -1,
                    Exports = new[] {
                        new ExportInfo { ServiceType = typeof(IService), ServiceName = null } },
                    FactorySetupInfo = new DecoratorSetupInfo { ServiceName = null, CompareMetadata = false, Condition = null }
                },

                new RegistrationInfo
                {
                    ImplementationType = typeof(Wrap<>),
                    IsSingleton = false,
                    MetadataAttributeIndex = -1,
                    Exports = new[] {
                        new ExportInfo { ServiceType = typeof(Wrap<>), ServiceName = null } },
                    FactorySetupInfo = new GenericWrapperSetupInfo { ServiceTypeIndex = 0 }
                },
            });

            var wrapped = container.Resolve<Wrap<IService>>("some");

            Assert.That(wrapped.Value, Is.InstanceOf<AnotherService>());
        }

        [Test]
        public void Can_use_compile_time_generated_registrations()
        {
            var container = new Container(AttributedRegistrator.DefaultSetup);
            container.RegisterExported(CompileTimeGeneratedRegistrator.Registrations);

            Assert.DoesNotThrow(() =>
                container.Resolve<Meta<Func<IServiceWithMetadata>, IViewMetadata>[]>());
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
