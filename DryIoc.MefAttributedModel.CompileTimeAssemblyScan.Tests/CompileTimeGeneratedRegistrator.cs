
namespace DryIoc.MefAttributedModel.CompileTimeAssemblyScan.Tests
{
    public static class CompileTimeGeneratedRegistrator
    {
        public static readonly RegistrationInfo[] Registrations =
        {
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Service),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Service), null),
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService), null),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService), null),
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService), null),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.TransientService),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ITransientService), null),
        },
    IsSingleton = false,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISingletonService), null),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SingletonOpenGenericService<>),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IOpenGenericService<>), null),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.TransientOpenGenericService<>),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.TransientOpenGenericService<>), null),
        },
    IsSingleton = false,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OpenGenericServiceWithTwoParameters<,>),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OpenGenericServiceWithTwoParameters<,>), null),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DependentService),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DependentService), null),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OneTransientService),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMultipleImplentations), null),
        },
    IsSingleton = false,
    MetadataAttributeIndex = 1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.AnotherTransientService),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMultipleImplentations), null),
        },
    IsSingleton = false,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OneServiceWithMetadata),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMetadata), null),
        },
    IsSingleton = true,
    MetadataAttributeIndex = 0,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.AnotherServiceWithMetadata),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMetadata), null),
        },
    IsSingleton = false,
    MetadataAttributeIndex = 0,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.YetAnotherServiceWithMetadata),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMetadata), null),
        },
    IsSingleton = false,
    MetadataAttributeIndex = 0,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructors),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructors), null),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting), null),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata), null),
        },
    IsSingleton = true,
    MetadataAttributeIndex = 0,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithImportedCtorParameter),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithImportedCtorParameter), null),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NamedService),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamedService), null),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.AnotherNamedService),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamedService), "blah"),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DbMan),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DbMan), null),
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISomeDb), null),
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAnotherDb), null),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DbMan<>),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DbMan<>), null),
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISomeDb<>), null),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample), 1),
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IPrintToCode), 1),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler), "fast"),
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), "fast"),
        },
    IsSingleton = true,
    MetadataAttributeIndex = 1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler), "slow"),
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), "slow"),
        },
    IsSingleton = true,
    MetadataAttributeIndex = 0,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler), "transact"),
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), "transact"),
        },
    IsSingleton = true,
    MetadataAttributeIndex = 0,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.LoggingHandlerDecorator),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.LoggingHandlerDecorator), null),
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), null),
        },
    IsSingleton = false,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Decorator,
Decorator = new DecoratorInfo(false, null, "slow")
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.RetryHandlerDecorator),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.RetryHandlerDecorator), null),
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), null),
        },
    IsSingleton = false,
    MetadataAttributeIndex = 1,
    FactoryType = DryIoc.FactoryType.Decorator,
Decorator = new DecoratorInfo(true, null, null)
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandlerDecorator),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandlerDecorator), null),
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), null),
        },
    IsSingleton = false,
    MetadataAttributeIndex = 1,
    FactoryType = DryIoc.FactoryType.Decorator,
Decorator = new DecoratorInfo(true, null, "transact")
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.CustomHandlerDecorator),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), null),
        },
    IsSingleton = false,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Decorator,
Decorator = new DecoratorInfo(false, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.CustomHandlerDecorator.Condition), null)
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport), null),
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), null),
        },
    IsSingleton = false,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Decorator,
Decorator = new DecoratorInfo(false, null, null)
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.BlahHandler),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), DryIoc.MefAttributedModel.UnitTests.CUT.BlahFooh.Blah),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler), DryIoc.MefAttributedModel.UnitTests.CUT.BlahFooh.Fooh),
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), DryIoc.MefAttributedModel.UnitTests.CUT.BlahFooh.Fooh),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FoohDecorator),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FoohDecorator), null),
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), null),
        },
    IsSingleton = false,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Decorator,
Decorator = new DecoratorInfo(false, null, DryIoc.MefAttributedModel.UnitTests.CUT.BlahFooh.Fooh)
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FactoryConsumer),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FactoryConsumer), null),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.One),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.One), "two"),
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.One), "one"),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<>),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<>), null),
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IFactory<>), null),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.GenericWrapper,
    GenericWrapper = new GenericWrapperInfo { ServiceTypeIndex = 0 }
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FactoryWithArgsConsumer),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FactoryWithArgsConsumer), null),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Two),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Two), null),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<,>),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<,>), null),
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IFactory<,>), null),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.GenericWrapper,
    GenericWrapper = new GenericWrapperInfo { ServiceTypeIndex = 1 }
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.KeyClient),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.KeyClient), null),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.KeyService),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService), DryIoc.MefAttributedModel.UnitTests.CUT.ServiceKey.One),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OtherKeyService),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OtherKeyService), DryIoc.MefAttributedModel.UnitTests.CUT.ServiceKey.OtherOne),
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService), DryIoc.MefAttributedModel.UnitTests.CUT.ServiceKey.OtherOne),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NativeUser),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NativeUser), null),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.HomeUser),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.HomeUser), null),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MyCode),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MyCode), null),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithFieldAndProperty),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithFieldAndProperty), null),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooHey),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooHey), null),
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IFooService), null),
        },
    IsSingleton = true,
    MetadataAttributeIndex = 0,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooBlah),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooBlah), null),
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IFooService), null),
        },
    IsSingleton = true,
    MetadataAttributeIndex = 0,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumer),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumer), null),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumerNotFound),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumerNotFound), null),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ForExport),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IForExport), null),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ForExportBaseImpl),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ForExportBase), null),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported), "c"),
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported), "c"),
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported), "b"),
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported), "a"),
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported), "i"),
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported), "j"),
        },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}, 
        };
    }
}