namespace DryIoc.CompileTimeGeneration.Tests
{
    using System;
    public partial class ServiceFactory
    {
        public static HashTree<Type, FactoryDelegate> 
            DefaultResolutions = HashTree<Type, FactoryDelegate>.Empty;

        public static HashTree<KV<Type, object>, FactoryDelegate> 
            KeyedResolutions = HashTree<KV<Type, object>, FactoryDelegate>.Empty;

        static ServiceFactory()
        {

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.AppleFactory),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(142, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AppleFactory()));

/* Exception: AttributedModelException
----------------------
Unable to resolve dependency DryIoc.MefAttributedModel.UnitTests.CUT.IFooService with metadata [NotFound] in DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumerNotFound
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumerNotFound}
*/

/* Exception: AttributedModelException
----------------------
Unable to find single constructor with System.ComponentModel.Composition.ImportingConstructorAttribute in DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructors.
*/

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OrangeFactory),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(143, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OrangeFactory()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithImportedCtorParameter),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(117, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithImportedCtorParameter((DryIoc.MefAttributedModel.UnitTests.CUT.AnotherNamedService)r.Resolver.SingletonScope.GetOrAdd(119, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherNamedService()))));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler), "transact"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(127, () => new DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooHey),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(168, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooHey()));

/* Exception: ContainerException
----------------------
Unable to resolve DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool {13} as parameter "tool"
 in DryIoc.MefAttributedModel.UnitTests.CUT.OtherDependsOnExternalTool
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.OtherDependsOnExternalTool}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IForExport),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(172, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ForExport()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(115, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting(new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService())));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService), DefaultKey.Of(0)),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(101, () => new DryIoc.MefAttributedModel.UnitTests.CUT.Service()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService), DryIoc.MefAttributedModel.UnitTests.CUT.ServiceKey.One),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(156, () => new DryIoc.MefAttributedModel.UnitTests.CUT.KeyService()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService), DefaultKey.Of(1)),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(102, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService), DryIoc.MefAttributedModel.UnitTests.CUT.ServiceKey.OtherOne),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(157, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OtherKeyService()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Orange), DefaultKey.Of(0)),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(144, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.OrangeFactory)state.Get(11)).Create()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Orange), DefaultKey.Of(1)),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(146, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory)state.Get(13)).CreateOrange()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Orange), DefaultKey.Of(2)),
                    (state, r, scope) => ((DryIoc.MefAttributedModel.UnitTests.CUT.TransientOrangeFactory)state.Get(15)).Create());

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Orange), "orange"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(149, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory)state.Get(17)).CreateOrange()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(System.Func<string, DryIoc.MefAttributedModel.UnitTests.CUT.Orange>),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(154, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.FuncFactory)state.Get(19)).Create()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamedService), "blah"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(119, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherNamedService()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamedService), DefaultKey.Of(0)),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(118, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedService()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithFieldAndProperty),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(164, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithFieldAndProperty { Property = (DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService)r.Resolver.SingletonScope.GetOrAdd(175, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService()), Field = (DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService)r.Resolver.SingletonScope.GetOrAdd(175, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService()) }));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DbMan),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(120, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMetadata), DefaultKey.Of(0)),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(111, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OneServiceWithMetadata()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMetadata), DefaultKey.Of(1)),
                    (state, r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherServiceWithMetadata());

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMetadata), DefaultKey.Of(2)),
                    (state, r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.YetAnotherServiceWithMetadata());

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FuncFactory),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(153, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FuncFactory()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FactoryConsumer),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(136, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FactoryConsumer(new DryIoc.MefAttributedModel.UnitTests.CUT.IFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>[] { (DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>)r.Resolver.SingletonScope.GetOrAdd(176, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>(() => (DryIoc.MefAttributedModel.UnitTests.CUT.One)r.Resolver.SingletonScope.GetOrAdd(137, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One()))), (DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>)r.Resolver.SingletonScope.GetOrAdd(177, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>(() => (DryIoc.MefAttributedModel.UnitTests.CUT.One)r.Resolver.SingletonScope.GetOrAdd(137, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One()))) })));

/* Exception: ContainerException
----------------------
Unable to resolve String as parameter "message"
 in DryIoc.MefAttributedModel.UnitTests.CUT.FactoryWithArgsConsumer
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.FactoryWithArgsConsumer}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IFooService), DefaultKey.Of(0)),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(168, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooHey()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IFooService), DefaultKey.Of(1)),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(169, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooBlah()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), "transact"),
                    (state, r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_2_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandlerDecorator(_2_IHandler0)))((DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler)r.Resolver.SingletonScope.GetOrAdd(127, () => new DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler())));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), "slow"),
                    (state, r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_0_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))(new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_3_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.CustomHandlerDecorator(_3_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.LoggingHandlerDecorator(_0_IHandler0))))((DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler)r.Resolver.SingletonScope.GetOrAdd(126, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler())));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), "fast"),
                    (state, r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_1_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.RetryHandlerDecorator(_1_IHandler0)))((DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler)r.Resolver.SingletonScope.GetOrAdd(125, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler())));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), DryIoc.MefAttributedModel.UnitTests.CUT.BlahFooh.Blah),
                    (state, r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))((DryIoc.MefAttributedModel.UnitTests.CUT.BlahHandler)r.Resolver.SingletonScope.GetOrAdd(133, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BlahHandler())));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), DryIoc.MefAttributedModel.UnitTests.CUT.BlahFooh.Fooh),
                    (state, r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_5_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.FoohDecorator(_5_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0)))((DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler)r.Resolver.SingletonScope.GetOrAdd(134, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler())));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ITransientService),
                    (state, r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService());

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.One), "two"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(137, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.One), "one"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(137, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One()));

/* Exception: ContainerException
----------------------
Unable to resolve String as parameter "message"
 in DryIoc.MefAttributedModel.UnitTests.CUT.OneTransientService: DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMultipleImplentations {DefaultKey.Of(0)}
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMultipleImplentations, DefaultKey.Of(0)}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

/* Exception: ContainerException
----------------------
Unable to resolve String as parameter "message"
 in DryIoc.MefAttributedModel.UnitTests.CUT.AnotherTransientService: DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMultipleImplentations {DefaultKey.Of(1)}
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMultipleImplentations, DefaultKey.Of(1)}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

/* Exception: ContainerException
----------------------
Unable to resolve String as parameter "message"
 in DryIoc.MefAttributedModel.UnitTests.CUT.Two
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.Two}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Apple), DefaultKey.Of(0)),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(147, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory)state.Get(13)).CreateApple()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Apple), "apple"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(150, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory)state.Get(17)).CreateApple()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumer),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(170, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumer(new System.Lazy<DryIoc.MefAttributedModel.UnitTests.CUT.IFooService>(() => r.Resolver.Resolve<DryIoc.MefAttributedModel.UnitTests.CUT.IFooService>((DryIoc.DefaultKey)state.Get(42), DryIoc.IfUnresolved.Throw, default(System.Type))))));

/* Exception: ContainerException
----------------------
Unable to resolve DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool as parameter "tool"
 in DryIoc.MefAttributedModel.UnitTests.CUT.WithUnregisteredExternalEdependency
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.WithUnregisteredExternalEdependency}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler), DryIoc.MefAttributedModel.UnitTests.CUT.BlahFooh.Fooh),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(134, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported), "c"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(174, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported), "j"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(174, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported), "i"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(174, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NativeUser),
                    (state, r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.NativeUser(new DryIoc.MefAttributedModel.UnitTests.CUT.ForeignTool()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ForExportBase),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(173, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ForExportBaseImpl()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(116, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler), "fast"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(125, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(102, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooBlah),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(169, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooBlah()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample), 1),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(122, () => new DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IPrintToCode), 1),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(122, () => new DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample()));

/* Exception: ContainerException
----------------------
Unable to resolve String as parameter "message"
 in DryIoc.MefAttributedModel.UnitTests.CUT.ClientWithPrimitiveParameter
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.ClientWithPrimitiveParameter}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DependentService),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(108, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DependentService(new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService(), (DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService)r.Resolver.SingletonScope.GetOrAdd(104, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService()), new DryIoc.MefAttributedModel.UnitTests.CUT.TransientOpenGenericService<string>(), (DryIoc.MefAttributedModel.UnitTests.CUT.OpenGenericServiceWithTwoParameters<bool, bool>)r.Resolver.SingletonScope.GetOrAdd(181, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OpenGenericServiceWithTwoParameters<bool, bool>()))));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.BothExportManyAndExport),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(160, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BothExportManyAndExport()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ClientWithServiceAndPrimitiveProperty),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(124, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ClientWithServiceAndPrimitiveProperty { Service = (DryIoc.MefAttributedModel.UnitTests.CUT.KeyService)r.Resolver.SingletonScope.GetOrAdd(156, () => new DryIoc.MefAttributedModel.UnitTests.CUT.KeyService()) }));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IOne), "blah"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(159, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedOne()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IOne), DefaultKey.Of(0)),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(160, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BothExportManyAndExport()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamed), "blah"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(159, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedOne()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamed), "named"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(160, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BothExportManyAndExport()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamed), DefaultKey.Of(0)),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(160, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BothExportManyAndExport()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NamedOne), "blah"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(159, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedOne()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.HomeUser),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(162, () => new DryIoc.MefAttributedModel.UnitTests.CUT.HomeUser(_String0 => (DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool)r.Resolver.SingletonScope.GetOrAdd(182, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool(_String0)))));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OneDependsOnExternalTool),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(165, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OneDependsOnExternalTool((DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool)r.Resolver.SingletonScope.GetOrAdd(183, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool()))));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Service),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(101, () => new DryIoc.MefAttributedModel.UnitTests.CUT.Service()));

/* Exception: ContainerException
----------------------
Unable to resolve wrapper DryIoc.Meta<Lazy<DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool>, DryIoc.MefAttributedModel.UnitTests.CUT.MineMeta> {required: DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool} as parameter "tool"
 in DryIoc.MefAttributedModel.UnitTests.CUT.MyCode
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.MyCode}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.TransientOrangeFactory),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(151, () => new DryIoc.MefAttributedModel.UnitTests.CUT.TransientOrangeFactory()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(145, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler), "slow"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(126, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISingletonService),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(104, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported), "c"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(174, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported), "b"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(174, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported), "a"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(174, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.WithBothTheSameExports),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(158, () => new DryIoc.MefAttributedModel.UnitTests.CUT.WithBothTheSameExports()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(148, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAnotherDb),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(120, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OtherKeyService), DryIoc.MefAttributedModel.UnitTests.CUT.ServiceKey.OtherOne),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(157, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OtherKeyService()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISomeDb),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(120, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.KeyClient),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(155, () => new DryIoc.MefAttributedModel.UnitTests.CUT.KeyClient((DryIoc.MefAttributedModel.UnitTests.CUT.KeyService)r.Resolver.SingletonScope.GetOrAdd(156, () => new DryIoc.MefAttributedModel.UnitTests.CUT.KeyService()))));
        } // end of static constructor
    }
}