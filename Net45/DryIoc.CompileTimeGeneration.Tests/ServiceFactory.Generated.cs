namespace DryIoc.CompileTimeGeneration.Tests
{
    using System;
    public partial class ServiceFactory
    {
        public static HashTree<Type, FactoryDelegate> 
            DefaultResolutions = HashTree<Type, FactoryDelegate>.Empty;

        public static HashTree<Type, HashTree<object, FactoryDelegate>> 
            KeyedResolutions =  HashTree<Type, HashTree<object, FactoryDelegate>>.Empty;

        static ServiceFactory()
        {

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "transact",
                (state, r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_2_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandlerDecorator(_2_IHandler0)))((DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler)r.Resolver.SingletonScope.GetOrAdd(874, () => new DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler()))),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "slow",
                (state, r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_0_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))(new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_3_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.CustomHandlerDecorator(_3_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.LoggingHandlerDecorator(_0_IHandler0))))((DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler)r.Resolver.SingletonScope.GetOrAdd(873, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler()))),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "fast",
                (state, r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_1_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.RetryHandlerDecorator(_1_IHandler0)))((DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler)r.Resolver.SingletonScope.GetOrAdd(872, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler()))),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DryIoc.MefAttributedModel.UnitTests.CUT.BlahFooh.Blah,
                (state, r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))((DryIoc.MefAttributedModel.UnitTests.CUT.BlahHandler)r.Resolver.SingletonScope.GetOrAdd(880, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BlahHandler()))),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DryIoc.MefAttributedModel.UnitTests.CUT.BlahFooh.Fooh,
                (state, r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_5_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.FoohDecorator(_5_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0)))((DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler)r.Resolver.SingletonScope.GetOrAdd(881, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler()))),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.BothExportManyAndExport),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(907, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BothExportManyAndExport()));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IFooService),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DefaultKey.Of(0),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(915, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooHey())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IFooService),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DefaultKey.Of(1),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(916, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooBlah())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

/* Exception: typeof(ContainerException)
----------------------
Unable to resolve DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool {13} as parameter "tool"
 in DryIoc.MefAttributedModel.UnitTests.CUT.OtherDependsOnExternalTool
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.OtherDependsOnExternalTool}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ForExportBase),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(920, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ForExportBaseImpl()));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "slow",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(873, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DefaultKey.Of(0),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(848, () => new DryIoc.MefAttributedModel.UnitTests.CUT.Service())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DryIoc.MefAttributedModel.UnitTests.CUT.ServiceKey.One,
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(903, () => new DryIoc.MefAttributedModel.UnitTests.CUT.KeyService())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DefaultKey.Of(1),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(849, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DryIoc.MefAttributedModel.UnitTests.CUT.ServiceKey.OtherOne,
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(904, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OtherKeyService())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.HomeUser),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(909, () => new DryIoc.MefAttributedModel.UnitTests.CUT.HomeUser(_String0 => (DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool)r.Resolver.SingletonScope.GetOrAdd(922, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool(_String0)))));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ClientWithServiceAndPrimitiveProperty),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(871, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ClientWithServiceAndPrimitiveProperty { Service = (DryIoc.MefAttributedModel.UnitTests.CUT.KeyService)r.Resolver.SingletonScope.GetOrAdd(903, () => new DryIoc.MefAttributedModel.UnitTests.CUT.KeyService()) }));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OrangeFactory),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(890, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OrangeFactory()));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumer),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(917, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumer(new System.Lazy<DryIoc.MefAttributedModel.UnitTests.CUT.IFooService>(() => r.Resolver.Resolve<DryIoc.MefAttributedModel.UnitTests.CUT.IFooService>((DryIoc.DefaultKey)state.Get(17), DryIoc.IfUnresolved.Throw, default(System.Type))))));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamedService),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "blah",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(866, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherNamedService())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamedService),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DefaultKey.Of(0),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(865, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedService())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

/* Exception: typeof(ContainerException)
----------------------
Unable to resolve String as parameter "message"
 in DryIoc.MefAttributedModel.UnitTests.CUT.Two
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.Two}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IOne),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "blah",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(906, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedOne())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IOne),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DefaultKey.Of(0),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(907, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BothExportManyAndExport())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NativeUser),
                (state, r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.NativeUser(new DryIoc.MefAttributedModel.UnitTests.CUT.ForeignTool()));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooBlah),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(916, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooBlah()));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISingletonService),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(851, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService()));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DependentService),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(855, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DependentService(new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService(), (DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService)r.Resolver.SingletonScope.GetOrAdd(851, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService()), new DryIoc.MefAttributedModel.UnitTests.CUT.TransientOpenGenericService<string>(), (DryIoc.MefAttributedModel.UnitTests.CUT.OpenGenericServiceWithTwoParameters<bool, bool>)r.Resolver.SingletonScope.GetOrAdd(925, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OpenGenericServiceWithTwoParameters<bool, bool>()))));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.AppleFactory),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(889, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AppleFactory()));

/* Exception: typeof(ContainerException)
----------------------
Unable to resolve String as parameter "message"
 in DryIoc.MefAttributedModel.UnitTests.CUT.FactoryWithArgsConsumer
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.FactoryWithArgsConsumer}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.TransientOrangeFactory),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(898, () => new DryIoc.MefAttributedModel.UnitTests.CUT.TransientOrangeFactory()));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FuncFactory),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(900, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FuncFactory()));

/* Exception: typeof(ContainerException)
----------------------
Unable to resolve String as parameter "message"
 in DryIoc.MefAttributedModel.UnitTests.CUT.OneTransientService: DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMultipleImplentations {DefaultKey.Of(0)}
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMultipleImplentations, DefaultKey.Of(0)}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

/* Exception: typeof(ContainerException)
----------------------
Unable to resolve String as parameter "message"
 in DryIoc.MefAttributedModel.UnitTests.CUT.AnotherTransientService: DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMultipleImplentations {DefaultKey.Of(1)}
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMultipleImplentations, DefaultKey.Of(1)}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

/* Exception: typeof(ContainerException)
----------------------
Unable to resolve wrapper DryIoc.Meta<Lazy<DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool>, DryIoc.MefAttributedModel.UnitTests.CUT.MineMeta> {required: DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool} as parameter "tool"
 in DryIoc.MefAttributedModel.UnitTests.CUT.MyCode
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.MyCode}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

/* Exception: typeof(ContainerException)
----------------------
Unable to resolve String as parameter "message"
 in DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool as parameter "tool"
 in DryIoc.MefAttributedModel.UnitTests.CUT.WithUnregisteredExternalEdependency
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.WithUnregisteredExternalEdependency}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.KeyClient),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(902, () => new DryIoc.MefAttributedModel.UnitTests.CUT.KeyClient((DryIoc.MefAttributedModel.UnitTests.CUT.KeyService)r.Resolver.SingletonScope.GetOrAdd(903, () => new DryIoc.MefAttributedModel.UnitTests.CUT.KeyService()))));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IForExport),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(919, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ForExport()));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DryIoc.MefAttributedModel.UnitTests.CUT.BlahFooh.Fooh,
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(881, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAnotherDb),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(867, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan()));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(System.Func<string, DryIoc.MefAttributedModel.UnitTests.CUT.Orange>),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(901, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.FuncFactory)state.Get(34)).Create()));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMetadata),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DefaultKey.Of(0),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(858, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OneServiceWithMetadata())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMetadata),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DefaultKey.Of(1),
                (state, r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherServiceWithMetadata()),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMetadata),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DefaultKey.Of(2),
                (state, r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.YetAnotherServiceWithMetadata()),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "c",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(921, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "b",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(921, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "a",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(921, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(892, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory()));

/* Exception: typeof(AttributedModelException)
----------------------
Unable to find single constructor with System.ComponentModel.Composition.ImportingConstructorAttribute in DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructors.
*/

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "fast",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(872, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamed),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "blah",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(906, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedOne())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamed),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "named",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(907, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BothExportManyAndExport())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamed),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DefaultKey.Of(0),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(907, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BothExportManyAndExport())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ITransientService),
                (state, r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService());

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Orange),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DefaultKey.Of(0),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(891, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.OrangeFactory)state.Get(48)).Create())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Orange),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DefaultKey.Of(1),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(893, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory)state.Get(50)).CreateOrange())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Orange),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DefaultKey.Of(2),
                (state, r, scope) => ((DryIoc.MefAttributedModel.UnitTests.CUT.TransientOrangeFactory)state.Get(52)).Create()),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Orange),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "orange",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(896, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory)state.Get(54)).CreateOrange())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NamedOne),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "blah",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(906, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedOne())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.WithBothTheSameExports),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(905, () => new DryIoc.MefAttributedModel.UnitTests.CUT.WithBothTheSameExports()));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithFieldAndProperty),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(911, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithFieldAndProperty { Property = (DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService)r.Resolver.SingletonScope.GetOrAdd(927, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService()), Field = (DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService)r.Resolver.SingletonScope.GetOrAdd(927, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService()) }));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OneDependsOnExternalTool),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(912, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OneDependsOnExternalTool((DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool)r.Resolver.SingletonScope.GetOrAdd(928, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool()))));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "c",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(921, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "j",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(921, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "i",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(921, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

/* Exception: typeof(ContainerException)
----------------------
Unable to resolve String as parameter "message"
 in DryIoc.MefAttributedModel.UnitTests.CUT.ClientWithPrimitiveParameter
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.ClientWithPrimitiveParameter}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.One),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "two",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(884, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.One),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "one",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(884, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(895, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory()));

/* Exception: typeof(AttributedModelException)
----------------------
Unable to resolve dependency DryIoc.MefAttributedModel.UnitTests.CUT.IFooService with metadata [NotFound] in DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumerNotFound
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumerNotFound}
*/

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "transact",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(874, () => new DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooHey),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(915, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooHey()));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Service),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(848, () => new DryIoc.MefAttributedModel.UnitTests.CUT.Service()));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OtherKeyService),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DryIoc.MefAttributedModel.UnitTests.CUT.ServiceKey.OtherOne,
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(904, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OtherKeyService())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FactoryConsumer),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(883, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FactoryConsumer(new DryIoc.MefAttributedModel.UnitTests.CUT.IFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>[] { (DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>)r.Resolver.SingletonScope.GetOrAdd(929, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>(() => (DryIoc.MefAttributedModel.UnitTests.CUT.One)r.Resolver.SingletonScope.GetOrAdd(884, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One()))), (DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>)r.Resolver.SingletonScope.GetOrAdd(930, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>(() => (DryIoc.MefAttributedModel.UnitTests.CUT.One)r.Resolver.SingletonScope.GetOrAdd(884, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One()))) })));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Apple),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DefaultKey.Of(0),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(894, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory)state.Get(50)).CreateApple())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Apple),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "apple",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(897, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory)state.Get(54)).CreateApple())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DbMan),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(867, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan()));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(862, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting(new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService())));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithImportedCtorParameter),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(864, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithImportedCtorParameter((DryIoc.MefAttributedModel.UnitTests.CUT.AnotherNamedService)r.Resolver.SingletonScope.GetOrAdd(866, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherNamedService()))));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISomeDb),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(867, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan()));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(849, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService()));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IPrintToCode),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                1,
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(869, () => new DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(863, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata()));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                1,
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(869, () => new DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));
        } // end of static constructor
    }
}