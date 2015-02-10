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
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DbMan),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(531, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan()));

/* Exception: ArgumentException
----------------------
Type DryIoc.MefAttributedModel.UnitTests.CUT.TransientOpenGenericService`1[T] is a generic type definition
*/

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(527, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.One), "two"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(546, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.One), "one"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(546, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OtherKeyService), DryIoc.MefAttributedModel.UnitTests.CUT.ServiceKey.OtherOne),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(553, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OtherKeyService()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumer),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(563, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumer(new System.Lazy<DryIoc.MefAttributedModel.UnitTests.CUT.IFooService>(() => r.Resolver.Resolve<DryIoc.MefAttributedModel.UnitTests.CUT.IFooService>((DryIoc.DefaultKey)state.Get(5), DryIoc.IfUnresolved.Throw, default(System.Type))))));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ITransientService),
                    (state, r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService());

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
 in DryIoc.MefAttributedModel.UnitTests.CUT.FactoryWithArgsConsumer
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.FactoryWithArgsConsumer}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.KeyClient),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(551, () => new DryIoc.MefAttributedModel.UnitTests.CUT.KeyClient((DryIoc.MefAttributedModel.UnitTests.CUT.KeyService)r.Resolver.SingletonScope.GetOrAdd(552, () => new DryIoc.MefAttributedModel.UnitTests.CUT.KeyService()))));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISomeDb),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(531, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan()));

/* Exception: ArgumentException
----------------------
Type DryIoc.MefAttributedModel.UnitTests.CUT.OpenGenericServiceWithTwoParameters`2[T1,T2] is a generic type definition
*/

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAnotherDb),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(531, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported), "c"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(567, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported), "b"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(567, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported), "a"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(567, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported()));

/* Exception: ContainerException
----------------------
Unable to register not a factory provider for open-generic service DryIoc.MefAttributedModel.UnitTests.CUT.IOpenGenericService<>.
*/

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(513, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ForExportBase),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(566, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ForExportBaseImpl()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService), DefaultKey.Of(0)),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(512, () => new DryIoc.MefAttributedModel.UnitTests.CUT.Service()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService), DryIoc.MefAttributedModel.UnitTests.CUT.ServiceKey.One),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(552, () => new DryIoc.MefAttributedModel.UnitTests.CUT.KeyService()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService), DefaultKey.Of(1)),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(513, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService), DryIoc.MefAttributedModel.UnitTests.CUT.ServiceKey.OtherOne),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(553, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OtherKeyService()));

/* Exception: ContainerException
----------------------
Unable to register not a factory provider for open-generic service DryIoc.MefAttributedModel.UnitTests.CUT.ISomeDb<>.
*/

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooBlah),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(562, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooBlah()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DependentService),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(519, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DependentService(new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService(), (DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService)r.Resolver.SingletonScope.GetOrAdd(515, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService()), new DryIoc.MefAttributedModel.UnitTests.CUT.TransientOpenGenericService<string>(), (DryIoc.MefAttributedModel.UnitTests.CUT.OpenGenericServiceWithTwoParameters<bool, bool>)r.Resolver.SingletonScope.GetOrAdd(574, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OpenGenericServiceWithTwoParameters<bool, bool>()))));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IFooService), DefaultKey.Of(0)),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(561, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooHey()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IFooService), DefaultKey.Of(1)),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(562, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooBlah()));

/* Exception: ArgumentException
----------------------
Type DryIoc.MefAttributedModel.UnitTests.CUT.DbMan`1[T] is a generic type definition
*/

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithImportedCtorParameter),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(528, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithImportedCtorParameter((DryIoc.MefAttributedModel.UnitTests.CUT.AnotherNamedService)r.Resolver.SingletonScope.GetOrAdd(530, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherNamedService()))));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler), "transact"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(536, () => new DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NativeUser),
                    (state, r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.NativeUser(new DryIoc.MefAttributedModel.UnitTests.CUT.ForeignTool()));

/* Exception: AttributedModelException
----------------------
Unable to find single constructor with System.ComponentModel.Composition.ImportingConstructorAttribute in DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructors.
*/

/* Exception: AttributedModelException
----------------------
Unable to resolve dependency DryIoc.MefAttributedModel.UnitTests.CUT.IFooService with metadata [NotFound] in DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumerNotFound
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumerNotFound}
*/

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FactoryConsumer),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(545, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FactoryConsumer(new DryIoc.MefAttributedModel.UnitTests.CUT.IFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>[] { (DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>)r.Resolver.SingletonScope.GetOrAdd(577, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>(() => (DryIoc.MefAttributedModel.UnitTests.CUT.One)r.Resolver.SingletonScope.GetOrAdd(546, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One()))), (DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>)r.Resolver.SingletonScope.GetOrAdd(578, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>(() => (DryIoc.MefAttributedModel.UnitTests.CUT.One)r.Resolver.SingletonScope.GetOrAdd(546, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One()))) })));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISingletonService),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(515, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.HomeUser),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(555, () => new DryIoc.MefAttributedModel.UnitTests.CUT.HomeUser(_String0 => (DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool)r.Resolver.SingletonScope.GetOrAdd(579, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool(_String0)))));

/* Exception: ContainerException
----------------------
Unable to resolve wrapper DryIoc.Meta<Lazy<DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool>, DryIoc.MefAttributedModel.UnitTests.CUT.MineMeta> {required: DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool} as parameter "tool"
 in DryIoc.MefAttributedModel.UnitTests.CUT.MyCode
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.MyCode}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

/* Exception: ContainerException
----------------------
Unable to resolve String as parameter "message"
 in DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool as parameter "tool"
 in DryIoc.MefAttributedModel.UnitTests.CUT.WithUnregisteredExternalEdependency
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.WithUnregisteredExternalEdependency}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithFieldAndProperty),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(557, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithFieldAndProperty { Property = (DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService)r.Resolver.SingletonScope.GetOrAdd(580, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService()), Field = (DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService)r.Resolver.SingletonScope.GetOrAdd(580, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService()) }));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler), "fast"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(534, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamedService), "blah"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(530, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherNamedService()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamedService), DefaultKey.Of(0)),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(529, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedService()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(526, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting(new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService())));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooHey),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(561, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooHey()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OneDependsOnExternalTool),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(558, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OneDependsOnExternalTool((DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool)r.Resolver.SingletonScope.GetOrAdd(581, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool()))));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported), "c"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(567, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported), "j"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(567, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported), "i"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(567, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported()));

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Service),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(512, () => new DryIoc.MefAttributedModel.UnitTests.CUT.Service()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMetadata), DefaultKey.Of(0)),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(522, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OneServiceWithMetadata()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMetadata), DefaultKey.Of(1)),
                    (state, r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherServiceWithMetadata());

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMetadata), DefaultKey.Of(2)),
                    (state, r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.YetAnotherServiceWithMetadata());

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OtherDependsOnExternalTool),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(559, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OtherDependsOnExternalTool((DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool)r.Resolver.SingletonScope.GetOrAdd(581, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool()))));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IPrintToCode), 1),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(533, () => new DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample), 1),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(533, () => new DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), "transact"),
                    (state, r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_2_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandlerDecorator(_2_IHandler0)))((DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler)r.Resolver.SingletonScope.GetOrAdd(536, () => new DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler())));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), "slow"),
                    (state, r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_0_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))(new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_3_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.CustomHandlerDecorator(_3_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.LoggingHandlerDecorator(_0_IHandler0))))((DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler)r.Resolver.SingletonScope.GetOrAdd(535, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler())));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), "fast"),
                    (state, r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_1_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.RetryHandlerDecorator(_1_IHandler0)))((DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler)r.Resolver.SingletonScope.GetOrAdd(534, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler())));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), DryIoc.MefAttributedModel.UnitTests.CUT.BlahFooh.Blah),
                    (state, r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))((DryIoc.MefAttributedModel.UnitTests.CUT.BlahHandler)r.Resolver.SingletonScope.GetOrAdd(542, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BlahHandler())));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), DryIoc.MefAttributedModel.UnitTests.CUT.BlahFooh.Fooh),
                    (state, r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_5_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.FoohDecorator(_5_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0)))((DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler)r.Resolver.SingletonScope.GetOrAdd(543, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler())));

/* Exception: ContainerException
----------------------
Unable to resolve String as parameter "message"
 in DryIoc.MefAttributedModel.UnitTests.CUT.Two
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.Two}.
Please register service, or specify @requiredServiceType while resolving, or add Rules.WithUnknownServiceResolver(MyRule).
*/

                DefaultResolutions = DefaultResolutions.AddOrUpdate(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IForExport),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(565, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ForExport()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler), "slow"),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(535, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler()));

                KeyedResolutions = KeyedResolutions.AddOrUpdate(new KV<Type, object>(
                    typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler), DryIoc.MefAttributedModel.UnitTests.CUT.BlahFooh.Fooh),
                    (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(543, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler()));
        } // end of static constructor
    }
}