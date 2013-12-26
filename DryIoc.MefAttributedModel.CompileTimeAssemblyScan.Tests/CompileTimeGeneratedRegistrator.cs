namespace DryIoc.MefAttributedModel.CompileTimeAssemblyScan.Tests
{
    public static class CompileTimeGeneratedRegistrator
    {
        public static readonly TypeExportInfo[] Registrations =
        {
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FactoryConsumer),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FactoryConsumer), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.One),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.One), ServiceName = "one" },
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.One), ServiceName = "two" },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<>),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<>), ServiceName = null },
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IFactory<>), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.GenericWrapper,
    GenericWrapper = new GenericWrapperInfo { ServiceTypeIndex = 0 }
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FactoryWithArgsConsumer),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FactoryWithArgsConsumer), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Two),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Two), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<,>),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<,>), ServiceName = null },
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IFactory<,>), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.GenericWrapper,
    GenericWrapper = new GenericWrapperInfo { ServiceTypeIndex = 1 }
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooHey),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooHey), ServiceName = null },
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IFooService), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = 1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooBlah),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooBlah), ServiceName = null },
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IFooService), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = 0,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumer),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumer), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumerNotFound),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumerNotFound), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler), ServiceName = "fast" },
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), ServiceName = "fast" },
    },
    IsSingleton = true,
    MetadataAttributeIndex = 0,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler), ServiceName = "slow" },
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), ServiceName = "slow" },
    },
    IsSingleton = true,
    MetadataAttributeIndex = 0,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler), ServiceName = "transact" },
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), ServiceName = "transact" },
    },
    IsSingleton = true,
    MetadataAttributeIndex = 0,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.LoggingHandlerDecorator),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.LoggingHandlerDecorator), ServiceName = null },
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), ServiceName = null },
    },
    IsSingleton = false,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Decorator,
    Decorator = new DecoratorInfo { ServiceName = "slow", ShouldCompareMetadata = false, ConditionType = null}
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.RetryHandlerDecorator),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.RetryHandlerDecorator), ServiceName = null },
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), ServiceName = null },
    },
    IsSingleton = false,
    MetadataAttributeIndex = 1,
    FactoryType = DryIoc.FactoryType.Decorator,
    Decorator = new DecoratorInfo { ServiceName = null, ShouldCompareMetadata = true, ConditionType = null}
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandlerDecorator),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandlerDecorator), ServiceName = null },
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), ServiceName = null },
    },
    IsSingleton = false,
    MetadataAttributeIndex = 1,
    FactoryType = DryIoc.FactoryType.Decorator,
    Decorator = new DecoratorInfo { ServiceName = "transact", ShouldCompareMetadata = true, ConditionType = null}
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.CustomHandlerDecorator),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), ServiceName = null },
    },
    IsSingleton = false,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Decorator,
    Decorator = new DecoratorInfo { ServiceName = null, ShouldCompareMetadata = false, ConditionType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.CustomHandlerDecorator.Condition)}
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport), ServiceName = null },
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), ServiceName = null },
    },
    IsSingleton = false,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Decorator,
    Decorator = new DecoratorInfo { ServiceName = null, ShouldCompareMetadata = false, ConditionType = null}
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ForExport),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IForExport), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ForExportBaseImpl),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ForExportBase), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported), ServiceName = "c" },
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported), ServiceName = "c" },
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported), ServiceName = "b" },
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported), ServiceName = "a" },
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported), ServiceName = "j" },
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported), ServiceName = "i" },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NativeUser),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NativeUser), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.HomeUser),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.HomeUser), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MyCode),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MyCode), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithFieldAndProperty),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithFieldAndProperty), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Service),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Service), ServiceName = null },
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService), ServiceName = null },
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.TransientService),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ITransientService), ServiceName = null },
    },
    IsSingleton = false,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISingletonService), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SingletonOpenGenericService<>),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IOpenGenericService<>), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.TransientOpenGenericService<>),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.TransientOpenGenericService<>), ServiceName = null },
    },
    IsSingleton = false,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OpenGenericServiceWithTwoParameters<,>),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OpenGenericServiceWithTwoParameters<,>), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DependentService),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DependentService), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OneTransientService),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMultipleImplentations), ServiceName = null },
    },
    IsSingleton = false,
    MetadataAttributeIndex = 1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.AnotherTransientService),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMultipleImplentations), ServiceName = null },
    },
    IsSingleton = false,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OneServiceWithMetadata),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMetadata), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = 0,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.AnotherServiceWithMetadata),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMetadata), ServiceName = null },
    },
    IsSingleton = false,
    MetadataAttributeIndex = 1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.YetAnotherServiceWithMetadata),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMetadata), ServiceName = null },
    },
    IsSingleton = false,
    MetadataAttributeIndex = 1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructors),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructors), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = 1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithImportedCtorParameter),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithImportedCtorParameter), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NamedService),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamedService), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.AnotherNamedService),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamedService), ServiceName = "blah" },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DbMan),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DbMan), ServiceName = null },
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISomeDb), ServiceName = null },
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAnotherDb), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DbMan<>),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DbMan<>), ServiceName = null },
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISomeDb<>), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
        };
    }
}