namespace DryIoc.CompileTimeOperationTests
{
    using DryIoc;
    using AR = AttributedRegistrator;

    public static class CompileTimeGeneratedRegistrator
    {
        public static RegistrationInfo[] Registrations =
        {
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.FastHandler),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.IHandler), ServiceName = "fast" },
                },
                IsSingleton = true,
                MetadataAttributeIndex = 0,
                FactoryType = DryIoc.FactoryType.Service,
                GenericWrapper = null,
                Decorator = null
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.SlowHandler),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.IHandler), ServiceName = "slow" },
                },
                IsSingleton = true,
                MetadataAttributeIndex = 0,
                FactoryType = DryIoc.FactoryType.Service,
                GenericWrapper = null,
                Decorator = null
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.TransactHandler),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.IHandler), ServiceName = "transact" },
                },
                IsSingleton = true,
                MetadataAttributeIndex = 1,
                FactoryType = DryIoc.FactoryType.Service,
                GenericWrapper = null,
                Decorator = null
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.LoggingHandlerDecorator),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.IHandler), ServiceName = null },
                },
                IsSingleton = false,
                MetadataAttributeIndex = -1,
                FactoryType = DryIoc.FactoryType.Decorator,
                GenericWrapper = null,
                Decorator = new DecoratorInfo { ServiceName = "slow", ShouldCompareMetadata = false, ConditionType = null }
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.RetryHandlerDecorator),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.IHandler), ServiceName = null },
                },
                IsSingleton = false,
                MetadataAttributeIndex = 2,
                FactoryType = DryIoc.FactoryType.Decorator,
                GenericWrapper = null,
                Decorator = new DecoratorInfo { ServiceName = null, ShouldCompareMetadata = true, ConditionType = null }
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.TransactHandlerDecorator),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.IHandler), ServiceName = null },
                },
                IsSingleton = false,
                MetadataAttributeIndex = 2,
                FactoryType = DryIoc.FactoryType.Decorator,
                GenericWrapper = null,
                Decorator = new DecoratorInfo { ServiceName = "transact", ShouldCompareMetadata = true, ConditionType = null }
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.CustomHandlerDecorator),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.IHandler), ServiceName = null },
                },
                IsSingleton = false,
                MetadataAttributeIndex = -1,
                FactoryType = DryIoc.FactoryType.Decorator,
                GenericWrapper = null,
                Decorator = new DecoratorInfo { ServiceName = null, ShouldCompareMetadata = false, ConditionType = typeof(DryIoc.UnitTests.CUT.CustomHandlerDecorator.Condition) }
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.DecoratorWithFastHandlerImport),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.DecoratorWithFastHandlerImport), ServiceName = null },
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.IHandler), ServiceName = null },
                },
                IsSingleton = false,
                MetadataAttributeIndex = -1,
                FactoryType = DryIoc.FactoryType.Decorator,
                GenericWrapper = null,
                Decorator = new DecoratorInfo { ServiceName = null, ShouldCompareMetadata = false, ConditionType = null }
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.TransientService),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.ITransientService), ServiceName = null },
                },
                IsSingleton = false,
                MetadataAttributeIndex = -1,
                FactoryType = DryIoc.FactoryType.Service,
                GenericWrapper = null,
                Decorator = null
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.SingletonService),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.ISingletonService), ServiceName = null },
                },
                IsSingleton = true,
                MetadataAttributeIndex = -1,
                FactoryType = DryIoc.FactoryType.Service,
                GenericWrapper = null,
                Decorator = null
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.SingletonOpenGenericService<>),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.IOpenGenericService<>), ServiceName = null },
                },
                IsSingleton = true,
                MetadataAttributeIndex = -1,
                FactoryType = DryIoc.FactoryType.Service,
                GenericWrapper = null,
                Decorator = null
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.TransientOpenGenericService<>),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.TransientOpenGenericService<>), ServiceName = null },
                },
                IsSingleton = false,
                MetadataAttributeIndex = -1,
                FactoryType = DryIoc.FactoryType.Service,
                GenericWrapper = null,
                Decorator = null
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.OpenGenericServiceWithTwoParameters<,>),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.OpenGenericServiceWithTwoParameters<,>), ServiceName = null },
                },
                IsSingleton = true,
                MetadataAttributeIndex = -1,
                FactoryType = DryIoc.FactoryType.Service,
                GenericWrapper = null,
                Decorator = null
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.DependentService),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.DependentService), ServiceName = null },
                },
                IsSingleton = true,
                MetadataAttributeIndex = -1,
                FactoryType = DryIoc.FactoryType.Service,
                GenericWrapper = null,
                Decorator = null
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.OneTransientService),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.IServiceWithMultipleImplentations), ServiceName = null },
                },
                IsSingleton = false,
                MetadataAttributeIndex = 1,
                FactoryType = DryIoc.FactoryType.Service,
                GenericWrapper = null,
                Decorator = null
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.AnotherTransientService),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.IServiceWithMultipleImplentations), ServiceName = null },
                },
                IsSingleton = false,
                MetadataAttributeIndex = -1,
                FactoryType = DryIoc.FactoryType.Service,
                GenericWrapper = null,
                Decorator = null
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.OneServiceWithMetadata),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.IServiceWithMetadata), ServiceName = null },
                },
                IsSingleton = true,
                MetadataAttributeIndex = 0,
                FactoryType = DryIoc.FactoryType.Service,
                GenericWrapper = null,
                Decorator = null
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.AnotherServiceWithMetadata),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.IServiceWithMetadata), ServiceName = null },
                },
                IsSingleton = false,
                MetadataAttributeIndex = 0,
                FactoryType = DryIoc.FactoryType.Service,
                GenericWrapper = null,
                Decorator = null
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.YetAnotherServiceWithMetadata),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.IServiceWithMetadata), ServiceName = null },
                },
                IsSingleton = false,
                MetadataAttributeIndex = 2,
                FactoryType = DryIoc.FactoryType.Service,
                GenericWrapper = null,
                Decorator = null
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.ServiceWithMultipleCostructors),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.ServiceWithMultipleCostructors), ServiceName = null },
                },
                IsSingleton = true,
                MetadataAttributeIndex = -1,
                FactoryType = DryIoc.FactoryType.Service,
                GenericWrapper = null,
                Decorator = null
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting), ServiceName = null },
                },
                IsSingleton = true,
                MetadataAttributeIndex = -1,
                FactoryType = DryIoc.FactoryType.Service,
                GenericWrapper = null,
                Decorator = null
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.SingleServiceWithMetadata),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.SingleServiceWithMetadata), ServiceName = null },
                },
                IsSingleton = true,
                MetadataAttributeIndex = 1,
                FactoryType = DryIoc.FactoryType.Service,
                GenericWrapper = null,
                Decorator = null
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.ServiceWithImportedCtorParameter),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.ServiceWithImportedCtorParameter), ServiceName = null },
                },
                IsSingleton = true,
                MetadataAttributeIndex = -1,
                FactoryType = DryIoc.FactoryType.Service,
                GenericWrapper = null,
                Decorator = null
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.NamedService),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.INamedService), ServiceName = null },
                },
                IsSingleton = true,
                MetadataAttributeIndex = -1,
                FactoryType = DryIoc.FactoryType.Service,
                GenericWrapper = null,
                Decorator = null
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.AnotherNamedService),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.INamedService), ServiceName = "blah" },
                },
                IsSingleton = true,
                MetadataAttributeIndex = -1,
                FactoryType = DryIoc.FactoryType.Service,
                GenericWrapper = null,
                Decorator = null
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.DbMan),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.DbMan), ServiceName = null },
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.ISomeDb), ServiceName = null },
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.IAnotherDb), ServiceName = null },
                },
                IsSingleton = true,
                MetadataAttributeIndex = -1,
                FactoryType = DryIoc.FactoryType.Service,
                GenericWrapper = null,
                Decorator = null
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.DbMan<>),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.ISomeDb<>), ServiceName = null },
                },
                IsSingleton = true,
                MetadataAttributeIndex = -1,
                FactoryType = DryIoc.FactoryType.Service,
                GenericWrapper = null,
                Decorator = null
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.FactoryConsumer),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.FactoryConsumer), ServiceName = null },
                },
                IsSingleton = true,
                MetadataAttributeIndex = -1,
                FactoryType = DryIoc.FactoryType.Service,
                GenericWrapper = null,
                Decorator = null
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.One),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.One), ServiceName = "one" },
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.One), ServiceName = "two" },
                },
                IsSingleton = true,
                MetadataAttributeIndex = -1,
                FactoryType = DryIoc.FactoryType.Service,
                GenericWrapper = null,
                Decorator = null
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.DryFactory<>),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.IFactory<>), ServiceName = null },
                },
                IsSingleton = true,
                MetadataAttributeIndex = -1,
                FactoryType = DryIoc.FactoryType.GenericWrapper,
                GenericWrapper = new GenericWrapperInfo { ServiceTypeIndex = 0 },
                Decorator = null
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.FactoryWithArgsConsumer),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.FactoryWithArgsConsumer), ServiceName = null },
                },
                IsSingleton = true,
                MetadataAttributeIndex = -1,
                FactoryType = DryIoc.FactoryType.Service,
                GenericWrapper = null,
                Decorator = null
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.Two),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.Two), ServiceName = null },
                },
                IsSingleton = true,
                MetadataAttributeIndex = -1,
                FactoryType = DryIoc.FactoryType.Service,
                GenericWrapper = null,
                Decorator = null
            },
            new RegistrationInfo {
                ImplementationType = typeof(DryIoc.UnitTests.CUT.DryFactory<,>),
                Exports = new[] {
                    new ExportInfo { ServiceType = typeof(DryIoc.UnitTests.CUT.IFactory<,>), ServiceName = null },
                },
                IsSingleton = true,
                MetadataAttributeIndex = -1,
                FactoryType = DryIoc.FactoryType.GenericWrapper,
                GenericWrapper = new GenericWrapperInfo { ServiceTypeIndex = 1 },
                Decorator = null
            },
        };
    }
}