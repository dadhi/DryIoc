namespace DryIoc.AttributedRegistration.CompileTimeAssemblyScan.Tests
{
    public static class CompileTimeGeneratedRegistrator
    {
        public static readonly RegistrationInfo[] Registrations =
        {
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.FastHandler),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.FastHandler), ServiceName = "fast" },
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.IHandler), ServiceName = "fast" },
    },
    IsSingleton = true,
    MetadataAttributeIndex = 0,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.SlowHandler),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.SlowHandler), ServiceName = "slow" },
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.IHandler), ServiceName = "slow" },
    },
    IsSingleton = true,
    MetadataAttributeIndex = 1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.TransactHandler),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.TransactHandler), ServiceName = "transact" },
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.IHandler), ServiceName = "transact" },
    },
    IsSingleton = true,
    MetadataAttributeIndex = 0,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.LoggingHandlerDecorator),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.LoggingHandlerDecorator), ServiceName = null },
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.IHandler), ServiceName = null },
    },
    IsSingleton = false,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Decorator,
    Decorator = new DecoratorInfo { ServiceName = "slow", ShouldCompareMetadata = false, ConditionType = null}
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.RetryHandlerDecorator),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.RetryHandlerDecorator), ServiceName = null },
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.IHandler), ServiceName = null },
    },
    IsSingleton = false,
    MetadataAttributeIndex = 1,
    FactoryType = DryIoc.FactoryType.Decorator,
    Decorator = new DecoratorInfo { ServiceName = null, ShouldCompareMetadata = true, ConditionType = null}
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.TransactHandlerDecorator),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.TransactHandlerDecorator), ServiceName = null },
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.IHandler), ServiceName = null },
    },
    IsSingleton = false,
    MetadataAttributeIndex = 0,
    FactoryType = DryIoc.FactoryType.Decorator,
    Decorator = new DecoratorInfo { ServiceName = "transact", ShouldCompareMetadata = true, ConditionType = null}
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.CustomHandlerDecorator),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.IHandler), ServiceName = null },
    },
    IsSingleton = false,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Decorator,
    Decorator = new DecoratorInfo { ServiceName = null, ShouldCompareMetadata = false, ConditionType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.CustomHandlerDecorator.Condition)}
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.DecoratorWithFastHandlerImport),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.DecoratorWithFastHandlerImport), ServiceName = null },
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.IHandler), ServiceName = null },
    },
    IsSingleton = false,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Decorator,
    Decorator = new DecoratorInfo { ServiceName = null, ShouldCompareMetadata = false, ConditionType = null}
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.NativeUser),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.NativeUser), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.HomeUser),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.HomeUser), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.MyCode),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.MyCode), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.ServiceWithFieldAndProperty),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.ServiceWithFieldAndProperty), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.Service),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.Service), ServiceName = null },
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.IService), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.AnotherService),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.AnotherService), ServiceName = null },
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.IService), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.TransientService),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.ITransientService), ServiceName = null },
    },
    IsSingleton = false,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.SingletonService),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.ISingletonService), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.SingletonOpenGenericService<>),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.IOpenGenericService<>), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.TransientOpenGenericService<>),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.TransientOpenGenericService<>), ServiceName = null },
    },
    IsSingleton = false,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.OpenGenericServiceWithTwoParameters<,>),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.OpenGenericServiceWithTwoParameters<,>), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.DependentService),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.DependentService), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.OneTransientService),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.IServiceWithMultipleImplentations), ServiceName = null },
    },
    IsSingleton = false,
    MetadataAttributeIndex = 1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.AnotherTransientService),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.IServiceWithMultipleImplentations), ServiceName = null },
    },
    IsSingleton = false,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.OneServiceWithMetadata),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.IServiceWithMetadata), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = 0,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.AnotherServiceWithMetadata),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.IServiceWithMetadata), ServiceName = null },
    },
    IsSingleton = false,
    MetadataAttributeIndex = 0,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.YetAnotherServiceWithMetadata),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.IServiceWithMetadata), ServiceName = null },
    },
    IsSingleton = false,
    MetadataAttributeIndex = 1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.ServiceWithMultipleCostructors),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.ServiceWithMultipleCostructors), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.SingleServiceWithMetadata),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.SingleServiceWithMetadata), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = 1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.ServiceWithImportedCtorParameter),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.ServiceWithImportedCtorParameter), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.NamedService),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.INamedService), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.AnotherNamedService),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.INamedService), ServiceName = "blah" },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.DbMan),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.DbMan), ServiceName = null },
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.ISomeDb), ServiceName = null },
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.IAnotherDb), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.DbMan<>),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.DbMan<>), ServiceName = null },
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.ISomeDb<>), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.FactoryConsumer),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.FactoryConsumer), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.One),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.One), ServiceName = "two" },
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.One), ServiceName = "one" },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.DryFactory<>),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.DryFactory<>), ServiceName = null },
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.IFactory<>), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.GenericWrapper,
    GenericWrapper = new GenericWrapperInfo { ServiceTypeIndex = 0 }
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.FactoryWithArgsConsumer),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.FactoryWithArgsConsumer), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.Two),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.Two), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.DryFactory<,>),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.DryFactory<,>), ServiceName = null },
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.IFactory<,>), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.GenericWrapper,
    GenericWrapper = new GenericWrapperInfo { ServiceTypeIndex = 1 }
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.FooHey),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.FooHey), ServiceName = null },
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.IFooService), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = 1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.FooBlah),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.FooBlah), ServiceName = null },
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.IFooService), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = 1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.FooConsumer),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.FooConsumer), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.FooConsumerNotFound),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.FooConsumerNotFound), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.ForExport),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.IForExport), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.ForExportBaseImpl),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.ForExportBase), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.MultiExported),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.MultiExported), ServiceName = "c" },
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.IMultiExported), ServiceName = "c" },
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.MultiExported), ServiceName = "a" },
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.MultiExported), ServiceName = "b" },
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.IMultiExported), ServiceName = "i" },
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.IMultiExported), ServiceName = "j" },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
        };
    }
}