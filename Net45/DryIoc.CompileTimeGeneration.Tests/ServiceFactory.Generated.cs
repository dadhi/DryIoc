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

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OtherKeyService), DryIoc.MefAttributedModel.UnitTests.CUT.ServiceKey.OtherOne),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(59, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OtherKeyService()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IForExport),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(71, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ForExport()));

/* Exception: ContainerException
----------------------
Unable to resolve String as parameter "message"
 in DryIoc.MefAttributedModel.UnitTests.CUT.Two
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.Two}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NativeUser),
                    (state, r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.NativeUser(new DryIoc.MefAttributedModel.UnitTests.CUT.ForeignTool()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAnotherDb),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(37, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OneDependsOnExternalTool),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(64, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OneDependsOnExternalTool((DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool)r.Resolver.SingletonScope.GetOrAdd(75, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool()))));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ITransientService),
                    (state, r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService());

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.KeyClient),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(57, () => new DryIoc.MefAttributedModel.UnitTests.CUT.KeyClient((DryIoc.MefAttributedModel.UnitTests.CUT.KeyService)r.Resolver.SingletonScope.GetOrAdd(58, () => new DryIoc.MefAttributedModel.UnitTests.CUT.KeyService()))));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported), "c"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(73, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported), "j"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(73, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported), "i"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(73, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported), "c"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(73, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported), "b"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(73, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported), "a"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(73, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(32, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting(new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService())));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooBlah),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(68, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooBlah()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler), "slow"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(41, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler), "fast"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(40, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(33, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FactoryConsumer),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(51, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FactoryConsumer(new DryIoc.MefAttributedModel.UnitTests.CUT.IFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>[] { (DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>)r.Resolver.SingletonScope.GetOrAdd(76, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>(() => (DryIoc.MefAttributedModel.UnitTests.CUT.One)r.Resolver.SingletonScope.GetOrAdd(52, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One()))), (DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>)r.Resolver.SingletonScope.GetOrAdd(77, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>(() => (DryIoc.MefAttributedModel.UnitTests.CUT.One)r.Resolver.SingletonScope.GetOrAdd(52, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One()))) })));

/* Exception: AttributedModelException
----------------------
Unable to resolve dependency DryIoc.MefAttributedModel.UnitTests.CUT.IFooService with metadata [NotFound] in DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumerNotFound
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumerNotFound}
*/

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ForExportBase),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(72, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ForExportBaseImpl()));

/* Exception: AttributedModelException
----------------------
Unable to find single constructor with System.ComponentModel.Composition.ImportingConstructorAttribute in DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructors.
*/

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), "transact"),
                    (state, r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_2_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandlerDecorator(_2_IHandler0)))((DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler)r.Resolver.SingletonScope.GetOrAdd(42, () => new DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler())));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), "slow"),
                    (state, r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_0_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))(new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_3_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.CustomHandlerDecorator(_3_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.LoggingHandlerDecorator(_0_IHandler0))))((DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler)r.Resolver.SingletonScope.GetOrAdd(41, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler())));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), "fast"),
                    (state, r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_1_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.RetryHandlerDecorator(_1_IHandler0)))((DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler)r.Resolver.SingletonScope.GetOrAdd(40, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler())));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), DryIoc.MefAttributedModel.UnitTests.CUT.BlahFooh.Blah),
                    (state, r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))((DryIoc.MefAttributedModel.UnitTests.CUT.BlahHandler)r.Resolver.SingletonScope.GetOrAdd(48, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BlahHandler())));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), DryIoc.MefAttributedModel.UnitTests.CUT.BlahFooh.Fooh),
                    (state, r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_5_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.FoohDecorator(_5_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0)))((DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler)r.Resolver.SingletonScope.GetOrAdd(49, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler())));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumer),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(69, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumer(new System.Lazy<DryIoc.MefAttributedModel.UnitTests.CUT.IFooService>(() => r.Resolver.Resolve<DryIoc.MefAttributedModel.UnitTests.CUT.IFooService>((DryIoc.DefaultKey)state.Get(25), DryIoc.IfUnresolved.Throw, default(System.Type))))));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISingletonService),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(21, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithImportedCtorParameter),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(34, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithImportedCtorParameter((DryIoc.MefAttributedModel.UnitTests.CUT.AnotherNamedService)r.Resolver.SingletonScope.GetOrAdd(36, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherNamedService()))));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler), DryIoc.MefAttributedModel.UnitTests.CUT.BlahFooh.Fooh),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(49, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler()));

/* Exception: ContainerException
----------------------
Unable to resolve DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool as parameter "tool"
 in DryIoc.MefAttributedModel.UnitTests.CUT.WithUnregisteredExternalEdependency
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.WithUnregisteredExternalEdependency}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DbMan),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(37, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan()));

/* Exception: ContainerException
----------------------
Unable to resolve DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool
 in wrapper Func<String, DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool> {required: DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool} as parameter "getTool"
 in DryIoc.MefAttributedModel.UnitTests.CUT.HomeUser
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.HomeUser}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

/* Exception: ContainerException
----------------------
Unable to resolve String as parameter "message"
 in DryIoc.MefAttributedModel.UnitTests.CUT.FactoryWithArgsConsumer
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.FactoryWithArgsConsumer}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamedService), "blah"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(36, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherNamedService()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamedService), DefaultKey.Of(0)),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(35, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedService()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithFieldAndProperty),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(63, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithFieldAndProperty { Property = (DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService)r.Resolver.SingletonScope.GetOrAdd(79, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService()), Field = (DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService)r.Resolver.SingletonScope.GetOrAdd(79, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService()) }));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler), "transact"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(42, () => new DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.One), "two"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(52, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.One), "one"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(52, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMetadata), DefaultKey.Of(0)),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(28, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OneServiceWithMetadata()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMetadata), DefaultKey.Of(1)),
                    (state, r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherServiceWithMetadata());

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMetadata), DefaultKey.Of(2)),
                    (state, r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.YetAnotherServiceWithMetadata());

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Service),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(18, () => new DryIoc.MefAttributedModel.UnitTests.CUT.Service()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooHey),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(67, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooHey()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService), DefaultKey.Of(0)),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(18, () => new DryIoc.MefAttributedModel.UnitTests.CUT.Service()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService), DryIoc.MefAttributedModel.UnitTests.CUT.ServiceKey.One),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(58, () => new DryIoc.MefAttributedModel.UnitTests.CUT.KeyService()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService), DefaultKey.Of(1)),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(19, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService), DryIoc.MefAttributedModel.UnitTests.CUT.ServiceKey.OtherOne),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(59, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OtherKeyService()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISomeDb),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(37, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IFooService), DefaultKey.Of(0)),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(67, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooHey()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IFooService), DefaultKey.Of(1)),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(68, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooBlah()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IPrintToCode), 1),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(39, () => new DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample()));

/* Exception: ContainerException
----------------------
Unable to resolve wrapper DryIoc.Meta<Lazy<DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool>, DryIoc.MefAttributedModel.UnitTests.CUT.MineMeta> {required: DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool} as parameter "tool"
 in DryIoc.MefAttributedModel.UnitTests.CUT.MyCode
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.MyCode}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(19, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OtherDependsOnExternalTool),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(65, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OtherDependsOnExternalTool((DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool)r.Resolver.SingletonScope.GetOrAdd(75, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool()))));

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

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample), 1),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(39, () => new DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DependentService),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(25, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DependentService(new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService(), (DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService)r.Resolver.SingletonScope.GetOrAdd(21, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService()), new DryIoc.MefAttributedModel.UnitTests.CUT.TransientOpenGenericService<string>(), (DryIoc.MefAttributedModel.UnitTests.CUT.OpenGenericServiceWithTwoParameters<bool, bool>)r.Resolver.SingletonScope.GetOrAdd(81, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OpenGenericServiceWithTwoParameters<bool, bool>()))));
        } // end of static constructor
    }
}