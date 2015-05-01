using System.Linq; // for Enumerable.Cast method required for LazyEnumerable<T>

namespace DryIoc.Zero.UnitTests
{
    public sealed class CompositionRoot : ICompositionRoot
    {
        public static readonly ICompositionRoot Default = new CompositionRoot();        
        public void RegisterGeneratedRoots(IFactoryDelegateRegistrator registrator)
        {
            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample), 1, 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(49, () => new DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.TransientOrangeFactory), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(78, () => new DryIoc.MefAttributedModel.UnitTests.CUT.TransientOrangeFactory()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.One), "two", 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(64, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.One), "one", 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(64, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IPrintToCode), 1, 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(49, () => new DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample()));

/* 
Resolution of DryIoc.MefAttributedModel.UnitTests.CUT.OtherDependsOnExternalTool
Failed with: DryIoc.ContainerException
----------------------
Unable to resolve DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool {13} as parameter "tool"
 in DryIoc.MefAttributedModel.UnitTests.CUT.OtherDependsOnExternalTool
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.OtherDependsOnExternalTool}.
Please ensure you have service registered (with proper key) - 95% of cases.
Remaining 5%: Service does not match the reuse scope, or service has wrong Setup.With(condition), or no Rules.WithUnknownServiceResolver(ForMyService).
*/
            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler), "slow", 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(53, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler()));

            registrator.Register(typeof(System.Func<string, DryIoc.MefAttributedModel.UnitTests.CUT.Orange>), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(81, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.FuncFactory)r.Resolver.SingletonScope.GetOrAdd(80, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FuncFactory())).Create()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Service), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(18, () => new DryIoc.MefAttributedModel.UnitTests.CUT.Service()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MyCode), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(95, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MyCode(new DryIoc.Meta<System.Lazy<DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool>, DryIoc.MefAttributedModel.UnitTests.CUT.MineMeta>(new System.Lazy<DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool>(() => (DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool)((DryIoc.IResolver)r.Resolver).ResolveKeyed(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool), (object)DryIoc.DefaultKey.Of(0), DryIoc.IfUnresolved.Throw, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool), r.Resolver.GetOrCreateResolutionScope(ref scope, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MyCode), (object)null))), DryIoc.MefAttributedModel.UnitTests.CUT.MineMeta.Green))));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ITransientService), 
                (r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService());

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DbMan), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(37, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FuncFactory), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(80, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FuncFactory()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(19, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Duck), DefaultKey.Of(0), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(83, () => DryIoc.MefAttributedModel.UnitTests.CUT.BirdFactory.GetDuck()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Duck), DefaultKey.Of(1), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(86, () => DryIoc.MefAttributedModel.UnitTests.CUT.StaticBirdFactory.Duck));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.LazyDep), 
                (r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.LazyDep());

/* 
Resolution of DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMultipleImplentations
Failed with: DryIoc.ContainerException
----------------------
Unable to resolve String as parameter "message"
 in DryIoc.MefAttributedModel.UnitTests.CUT.OneTransientService: DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMultipleImplentations {DefaultKey.Of(0)}
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMultipleImplentations, DefaultKey.Of(0)}.
Please ensure you have service registered (with proper key) - 95% of cases.
Remaining 5%: Service does not match the reuse scope, or service has wrong Setup.With(condition), or no Rules.WithUnknownServiceResolver(ForMyService).
*/
/* 
Resolution of DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMultipleImplentations
Failed with: DryIoc.ContainerException
----------------------
Unable to resolve String as parameter "message"
 in DryIoc.MefAttributedModel.UnitTests.CUT.AnotherTransientService: DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMultipleImplentations {DefaultKey.Of(1)}
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMultipleImplentations, DefaultKey.Of(1)}.
Please ensure you have service registered (with proper key) - 95% of cases.
Remaining 5%: Service does not match the reuse scope, or service has wrong Setup.With(condition), or no Rules.WithUnknownServiceResolver(ForMyService).
*/
            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Chicken), DefaultKey.Of(0), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(84, () => DryIoc.MefAttributedModel.UnitTests.CUT.BirdFactory.Chicken));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Chicken), DefaultKey.Of(1), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(85, () => DryIoc.MefAttributedModel.UnitTests.CUT.StaticBirdFactory.Chicken));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.WithBothTheSameExports), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(90, () => new DryIoc.MefAttributedModel.UnitTests.CUT.WithBothTheSameExports()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(46, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1((DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject)r.Resolver.SingletonScope.GetOrAdd(43, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject()))));

/* 
Resolution of DryIoc.MefAttributedModel.UnitTests.CUT.FactoryWithArgsConsumer
Failed with: DryIoc.ContainerException
----------------------
Unable to resolve String as parameter "message"
 in DryIoc.MefAttributedModel.UnitTests.CUT.FactoryWithArgsConsumer
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.FactoryWithArgsConsumer}.
Please ensure you have service registered (with proper key) - 95% of cases.
Remaining 5%: Service does not match the reuse scope, or service has wrong Setup.With(condition), or no Rules.WithUnknownServiceResolver(ForMyService).
*/
            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(75, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OneDependsOnExternalTool), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(97, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OneDependsOnExternalTool((DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool)r.Resolver.SingletonScope.GetOrAdd(109, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool()))));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.HomeUser), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(94, () => new DryIoc.MefAttributedModel.UnitTests.CUT.HomeUser(_String0 => (DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool)r.Resolver.SingletonScope.GetOrAdd(107, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool()))));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamedService), "blah", 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(36, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherNamedService()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamedService), DefaultKey.Of(0), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(35, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedService()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported), "c", 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(106, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported), "j", 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(106, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported), "i", 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(106, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported()));

/* 
Resolution of DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface
Failed with: DryIoc.ContainerException
----------------------
Unable to resolve DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface {DefaultKey.Of(0)}
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface, DefaultKey.Of(0)}.
Please ensure you have service registered (with proper key) - 95% of cases.
Remaining 5%: Service does not match the reuse scope, or service has wrong Setup.With(condition), or no Rules.WithUnknownServiceResolver(ForMyService).
*/
/* 
Resolution of DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface
Failed with: DryIoc.ContainerException
----------------------
Unable to resolve DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface {DefaultKey.Of(1)}
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface, DefaultKey.Of(1)}.
Please ensure you have service registered (with proper key) - 95% of cases.
Remaining 5%: Service does not match the reuse scope, or service has wrong Setup.With(condition), or no Rules.WithUnknownServiceResolver(ForMyService).
*/
/* 
Resolution of DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface
Failed with: DryIoc.ContainerException
----------------------
Unable to resolve DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface {DefaultKey.Of(2)}
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface, DefaultKey.Of(2)}.
Please ensure you have service registered (with proper key) - 95% of cases.
Remaining 5%: Service does not match the reuse scope, or service has wrong Setup.With(condition), or no Rules.WithUnknownServiceResolver(ForMyService).
*/
            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Apple), DefaultKey.Of(0), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(74, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory)r.Resolver.SingletonScope.GetOrAdd(72, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory())).CreateApple()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Apple), "apple", 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(77, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory)r.Resolver.SingletonScope.GetOrAdd(75, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory())).CreateApple()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(72, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(33, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata()));

/* 
Resolution of DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumerNotFound
Failed with: DryIoc.MefAttributedModel.AttributedModelException
----------------------
Unable to resolve dependency DryIoc.MefAttributedModel.UnitTests.CUT.IFooService with metadata [NotFound] in DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumerNotFound
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumerNotFound}
*/
            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler), "fast", 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(52, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.AppleFactory), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(69, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AppleFactory()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Me), 
                (r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.Me());

/* 
Resolution of DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructors
Failed with: DryIoc.MefAttributedModel.AttributedModelException
----------------------
Unable to find single constructor with System.ComponentModel.Composition.ImportingConstructorAttribute in DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructors.
*/
            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DependentService), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(25, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DependentService(new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService(), (DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService)r.Resolver.SingletonScope.GetOrAdd(21, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService()), new DryIoc.MefAttributedModel.UnitTests.CUT.TransientOpenGenericService<string>(), (DryIoc.MefAttributedModel.UnitTests.CUT.OpenGenericServiceWithTwoParameters<bool, bool>)r.Resolver.SingletonScope.GetOrAdd(111, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OpenGenericServiceWithTwoParameters<bool, bool>()))));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithImportedCtorParameter), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(34, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithImportedCtorParameter((DryIoc.MefAttributedModel.UnitTests.CUT.AnotherNamedService)r.Resolver.SingletonScope.GetOrAdd(36, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherNamedService()))));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ClientWithServiceAndPrimitiveProperty), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(51, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ClientWithServiceAndPrimitiveProperty { Service = (DryIoc.MefAttributedModel.UnitTests.CUT.KeyService)r.Resolver.SingletonScope.GetOrAdd(88, () => new DryIoc.MefAttributedModel.UnitTests.CUT.KeyService()) }));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.UseLazyEnumerable), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(39, () => new DryIoc.MefAttributedModel.UnitTests.CUT.UseLazyEnumerable(new DryIoc.LazyEnumerable<DryIoc.MefAttributedModel.UnitTests.CUT.Me>(r.Resolver.ResolveMany(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Me), null, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Me), null, r.Resolver.GetOrCreateResolutionScope(ref scope, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.UseLazyEnumerable), (object)null)).Cast<DryIoc.MefAttributedModel.UnitTests.CUT.Me>()))));

/* 
Resolution of DryIoc.MefAttributedModel.UnitTests.CUT.ClientWithPrimitiveParameter
Failed with: DryIoc.ContainerException
----------------------
Unable to resolve String as parameter "message"
 in DryIoc.MefAttributedModel.UnitTests.CUT.ClientWithPrimitiveParameter
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.ClientWithPrimitiveParameter}.
Please ensure you have service registered (with proper key) - 95% of cases.
Remaining 5%: Service does not match the reuse scope, or service has wrong Setup.With(condition), or no Rules.WithUnknownServiceResolver(ForMyService).
*/
            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.WithUnregisteredExternalEdependency), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(99, () => new DryIoc.MefAttributedModel.UnitTests.CUT.WithUnregisteredExternalEdependency((DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool)r.Resolver.SingletonScope.GetOrAdd(107, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool()))));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamed), "blah", 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(91, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedOne()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamed), "named", 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(92, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BothExportManyAndExport()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamed), DefaultKey.Of(0), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(92, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BothExportManyAndExport()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooHey), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(100, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooHey()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler), DryIoc.MefAttributedModel.UnitTests.CUT.BlahFooh.Fooh, 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(61, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumer), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(102, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumer(new System.Lazy<DryIoc.MefAttributedModel.UnitTests.CUT.IFooService>(() => (DryIoc.MefAttributedModel.UnitTests.CUT.IFooService)((DryIoc.IResolver)r.Resolver).ResolveKeyed(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IFooService), (object)DryIoc.DefaultKey.Of(1), DryIoc.IfUnresolved.Throw, (System.Type)null, r.Resolver.GetOrCreateResolutionScope(ref scope, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumer), (object)null))))));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMetadata), DefaultKey.Of(0), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(28, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OneServiceWithMetadata()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMetadata), DefaultKey.Of(1), 
                (r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherServiceWithMetadata());

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMetadata), DefaultKey.Of(2), 
                (r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.YetAnotherServiceWithMetadata());

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.KeyClient), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(87, () => new DryIoc.MefAttributedModel.UnitTests.CUT.KeyClient((DryIoc.MefAttributedModel.UnitTests.CUT.KeyService)r.Resolver.SingletonScope.GetOrAdd(88, () => new DryIoc.MefAttributedModel.UnitTests.CUT.KeyService()))));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.BothExportManyAndExport), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(92, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BothExportManyAndExport()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(48, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3((DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject3)r.Resolver.SingletonScope.GetOrAdd(45, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject3()))));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler), "transact", 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(54, () => new DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.BirdFactory), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(82, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BirdFactory()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OtherKeyService), DryIoc.MefAttributedModel.UnitTests.CUT.ServiceKey.OtherOne, 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(89, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OtherKeyService()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooBlah), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(101, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooBlah()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IOne), "blah", 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(91, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedOne()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IOne), DefaultKey.Of(0), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(92, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BothExportManyAndExport()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IForExport), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(104, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ForExport()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FactoryConsumer), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(63, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FactoryConsumer(new DryIoc.MefAttributedModel.UnitTests.CUT.IFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>[] { (DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>)r.Resolver.SingletonScope.GetOrAdd(112, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>(() => (DryIoc.MefAttributedModel.UnitTests.CUT.One)r.Resolver.SingletonScope.GetOrAdd(64, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One()))), (DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>)r.Resolver.SingletonScope.GetOrAdd(113, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>(() => (DryIoc.MefAttributedModel.UnitTests.CUT.One)r.Resolver.SingletonScope.GetOrAdd(64, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One()))) })));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), "transact", 
                (r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_2_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandlerDecorator(_2_IHandler0)))((DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler)r.Resolver.SingletonScope.GetOrAdd(54, () => new DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler())));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), "slow", 
                (r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_0_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))(new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_3_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.CustomHandlerDecorator(_3_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.LoggingHandlerDecorator(_0_IHandler0))))((DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler)r.Resolver.SingletonScope.GetOrAdd(53, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler())));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), "fast", 
                (r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_1_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.RetryHandlerDecorator(_1_IHandler0)))((DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler)r.Resolver.SingletonScope.GetOrAdd(52, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler())));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), DryIoc.MefAttributedModel.UnitTests.CUT.BlahFooh.Blah, 
                (r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))((DryIoc.MefAttributedModel.UnitTests.CUT.BlahHandler)r.Resolver.SingletonScope.GetOrAdd(60, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BlahHandler())));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), DryIoc.MefAttributedModel.UnitTests.CUT.BlahFooh.Fooh, 
                (r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_5_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.FoohDecorator(_5_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0)))((DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler)r.Resolver.SingletonScope.GetOrAdd(61, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler())));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService), DefaultKey.Of(0), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(18, () => new DryIoc.MefAttributedModel.UnitTests.CUT.Service()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService), DryIoc.MefAttributedModel.UnitTests.CUT.ServiceKey.One, 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(88, () => new DryIoc.MefAttributedModel.UnitTests.CUT.KeyService()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService), DefaultKey.Of(1), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(19, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService), DryIoc.MefAttributedModel.UnitTests.CUT.ServiceKey.OtherOne, 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(89, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OtherKeyService()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithFieldAndProperty), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(96, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithFieldAndProperty { Property = (DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService)r.Resolver.SingletonScope.GetOrAdd(114, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService()), Field = (DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService)r.Resolver.SingletonScope.GetOrAdd(114, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService()) }));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Orange), DefaultKey.Of(0), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(71, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.OrangeFactory)r.Resolver.SingletonScope.GetOrAdd(70, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OrangeFactory())).Create()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Orange), DefaultKey.Of(1), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(73, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory)r.Resolver.SingletonScope.GetOrAdd(72, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory())).CreateOrange()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Orange), DefaultKey.Of(2), 
                (r, scope) => ((DryIoc.MefAttributedModel.UnitTests.CUT.TransientOrangeFactory)r.Resolver.SingletonScope.GetOrAdd(78, () => new DryIoc.MefAttributedModel.UnitTests.CUT.TransientOrangeFactory())).Create());

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Orange), "orange", 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(76, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory)r.Resolver.SingletonScope.GetOrAdd(75, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory())).CreateOrange()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IFooService), DefaultKey.Of(0), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(100, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooHey()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IFooService), DefaultKey.Of(1), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(101, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooBlah()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OrangeFactory), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(70, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OrangeFactory()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.LazyDepClient), 
                (r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.LazyDepClient((DryIoc.MefAttributedModel.UnitTests.CUT.LazyDep)((DryIoc.IResolver)r.Resolver).ResolveKeyed(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.LazyDep), (object)null, DryIoc.IfUnresolved.Throw, (System.Type)null, r.Resolver.GetOrCreateResolutionScope(ref scope, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.LazyDepClient), (object)null))));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NativeUser), 
                (r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.NativeUser(new DryIoc.MefAttributedModel.UnitTests.CUT.ForeignTool()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(32, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting(new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService())));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NamedOne), "blah", 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(91, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedOne()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISingletonService), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(21, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(47, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2((DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject2)r.Resolver.SingletonScope.GetOrAdd(44, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject2()))));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ForExportBase), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(105, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ForExportBaseImpl()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported), "c", 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(106, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported), "b", 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(106, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported), "a", 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(106, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported()));

            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAnotherDb), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(37, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan()));

/* 
Resolution of DryIoc.MefAttributedModel.UnitTests.CUT.Two
Failed with: DryIoc.ContainerException
----------------------
Unable to resolve String as parameter "message"
 in DryIoc.MefAttributedModel.UnitTests.CUT.Two
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.Two}.
Please ensure you have service registered (with proper key) - 95% of cases.
Remaining 5%: Service does not match the reuse scope, or service has wrong Setup.With(condition), or no Rules.WithUnknownServiceResolver(ForMyService).
*/
            registrator.Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISomeDb), 
                (r, scope) => r.Resolver.SingletonScope.GetOrAdd(37, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan()));

        } // end of registrations
    }
}