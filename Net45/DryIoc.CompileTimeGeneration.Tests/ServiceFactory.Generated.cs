namespace DryIoc.CompileTimeGeneration.Tests
{
    using System;
    using System.Linq; // For Cast method required for LazyEnumerable<T>

    public partial class ServiceFactory
    {
        static ServiceFactory()
        {

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Service), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(115, () => new DryIoc.MefAttributedModel.UnitTests.CUT.Service()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooHey), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(197, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooHey()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IPrintToCode), 1, 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(146, () => new DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NativeUser), 
                (state, r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.NativeUser(new DryIoc.MefAttributedModel.UnitTests.CUT.ForeignTool()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(169, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamed), "blah", 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(188, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedOne()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamed), "named", 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(189, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BothExportManyAndExport()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamed), DefaultKey.Of(0), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(189, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BothExportManyAndExport()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService), DefaultKey.Of(0), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(115, () => new DryIoc.MefAttributedModel.UnitTests.CUT.Service()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService), DryIoc.MefAttributedModel.UnitTests.CUT.ServiceKey.One, 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(185, () => new DryIoc.MefAttributedModel.UnitTests.CUT.KeyService()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService), DefaultKey.Of(1), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(116, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService), DryIoc.MefAttributedModel.UnitTests.CUT.ServiceKey.OtherOne, 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(186, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OtherKeyService()));

            Register(typeof(System.Func<string, DryIoc.MefAttributedModel.UnitTests.CUT.Orange>), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(178, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.FuncFactory)r.Resolver.SingletonScope.GetOrAdd(177, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FuncFactory())).Create()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IOne), "blah", 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(188, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedOne()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IOne), DefaultKey.Of(0), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(189, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BothExportManyAndExport()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported), "c", 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(203, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported), "j", 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(203, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported), "i", 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(203, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.LazyDep), 
                (state, r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.LazyDep());

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FuncFactory), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(177, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FuncFactory()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(116, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ITransientService), 
                (state, r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService());

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OrangeFactory), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(167, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OrangeFactory()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(172, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IFooService), DefaultKey.Of(0), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(197, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooHey()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IFooService), DefaultKey.Of(1), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(198, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooBlah()));
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

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(129, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting(new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService())));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ClientWithServiceAndPrimitiveProperty), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(148, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ClientWithServiceAndPrimitiveProperty { Service = (DryIoc.MefAttributedModel.UnitTests.CUT.KeyService)r.Resolver.SingletonScope.GetOrAdd(185, () => new DryIoc.MefAttributedModel.UnitTests.CUT.KeyService()) }));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumer), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(199, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumer(new System.Lazy<DryIoc.MefAttributedModel.UnitTests.CUT.IFooService>(() => (DryIoc.MefAttributedModel.UnitTests.CUT.IFooService)((DryIoc.IResolver)r.Resolver).ResolveKeyed(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IFooService), (object)DryIoc.DefaultKey.Of(1), DryIoc.IfUnresolved.Throw, (System.Type)null, r.Resolver.GetOrCreateResolutionScope(ref scope, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumer), (object)null))))));
/* 
Resolution of DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructors
Failed with: DryIoc.MefAttributedModel.AttributedModelException
----------------------
Unable to find single constructor with System.ComponentModel.Composition.ImportingConstructorAttribute in DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructors.
*/

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISingletonService), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(118, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithImportedCtorParameter), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(131, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithImportedCtorParameter((DryIoc.MefAttributedModel.UnitTests.CUT.AnotherNamedService)r.Resolver.SingletonScope.GetOrAdd(133, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherNamedService()))));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler), "slow", 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(150, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FactoryConsumer), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(160, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FactoryConsumer(new DryIoc.MefAttributedModel.UnitTests.CUT.IFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>[] { (DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>)r.Resolver.SingletonScope.GetOrAdd(205, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>(() => (DryIoc.MefAttributedModel.UnitTests.CUT.One)r.Resolver.SingletonScope.GetOrAdd(161, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One()))), (DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>)r.Resolver.SingletonScope.GetOrAdd(206, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>(() => (DryIoc.MefAttributedModel.UnitTests.CUT.One)r.Resolver.SingletonScope.GetOrAdd(161, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One()))) })));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler), DryIoc.MefAttributedModel.UnitTests.CUT.BlahFooh.Fooh, 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(158, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.BirdFactory), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(179, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BirdFactory()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.BothExportManyAndExport), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(189, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BothExportManyAndExport()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Orange), DefaultKey.Of(0), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(168, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.OrangeFactory)r.Resolver.SingletonScope.GetOrAdd(167, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OrangeFactory())).Create()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Orange), DefaultKey.Of(1), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(170, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory)r.Resolver.SingletonScope.GetOrAdd(169, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory())).CreateOrange()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Orange), DefaultKey.Of(2), 
                (state, r, scope) => ((DryIoc.MefAttributedModel.UnitTests.CUT.TransientOrangeFactory)r.Resolver.SingletonScope.GetOrAdd(175, () => new DryIoc.MefAttributedModel.UnitTests.CUT.TransientOrangeFactory())).Create());

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Orange), "orange", 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(173, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory)r.Resolver.SingletonScope.GetOrAdd(172, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory())).CreateOrange()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.TransientOrangeFactory), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(175, () => new DryIoc.MefAttributedModel.UnitTests.CUT.TransientOrangeFactory()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.AppleFactory), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(166, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AppleFactory()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OneDependsOnExternalTool), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(194, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OneDependsOnExternalTool((DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool)r.Resolver.SingletonScope.GetOrAdd(207, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool()))));
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

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), "transact", 
                (state, r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_2_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandlerDecorator(_2_IHandler0)))((DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler)r.Resolver.SingletonScope.GetOrAdd(151, () => new DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler())));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), "slow", 
                (state, r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_0_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))(new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_3_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.CustomHandlerDecorator(_3_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.LoggingHandlerDecorator(_0_IHandler0))))((DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler)r.Resolver.SingletonScope.GetOrAdd(150, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler())));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), "fast", 
                (state, r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_1_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.RetryHandlerDecorator(_1_IHandler0)))((DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler)r.Resolver.SingletonScope.GetOrAdd(149, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler())));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), DryIoc.MefAttributedModel.UnitTests.CUT.BlahFooh.Blah, 
                (state, r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))((DryIoc.MefAttributedModel.UnitTests.CUT.BlahHandler)r.Resolver.SingletonScope.GetOrAdd(157, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BlahHandler())));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler), DryIoc.MefAttributedModel.UnitTests.CUT.BlahFooh.Fooh, 
                (state, r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_5_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.FoohDecorator(_5_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0)))((DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler)r.Resolver.SingletonScope.GetOrAdd(158, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler())));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(143, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1((DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject)r.Resolver.SingletonScope.GetOrAdd(140, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject()))));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(145, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3((DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject3)r.Resolver.SingletonScope.GetOrAdd(142, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject3()))));
/* 
Resolution of DryIoc.MefAttributedModel.UnitTests.CUT.HomeUser
Failed with: DryIoc.ContainerException
----------------------
Unable to resolve DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool
 in wrapper Func<String, DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool> {required: DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool} as parameter "getTool"
 in DryIoc.MefAttributedModel.UnitTests.CUT.HomeUser
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.HomeUser}.
Please ensure you have service registered (with proper key) - 95% of cases.
Remaining 5%: Service does not match the reuse scope, or service has wrong Setup.With(condition), or no Rules.WithUnknownServiceResolver(ForMyService).
*/
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

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooBlah), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(198, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooBlah()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMetadata), DefaultKey.Of(0), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(125, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OneServiceWithMetadata()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMetadata), DefaultKey.Of(1), 
                (state, r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherServiceWithMetadata());

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMetadata), DefaultKey.Of(2), 
                (state, r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.YetAnotherServiceWithMetadata());

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAnotherDb), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(134, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan()));
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

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OtherKeyService), DryIoc.MefAttributedModel.UnitTests.CUT.ServiceKey.OtherOne, 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(186, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OtherKeyService()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.UseLazyEnumerable), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(136, () => new DryIoc.MefAttributedModel.UnitTests.CUT.UseLazyEnumerable(new DryIoc.LazyEnumerable<DryIoc.MefAttributedModel.UnitTests.CUT.Me>(r.Resolver.ResolveMany(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Me), null, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Me), null, r.Resolver.GetOrCreateResolutionScope(ref scope, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.UseLazyEnumerable), (object)null)).Cast<DryIoc.MefAttributedModel.UnitTests.CUT.Me>()))));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamedService), "blah", 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(133, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherNamedService()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamedService), DefaultKey.Of(0), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(132, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedService()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler), "transact", 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(151, () => new DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler()));
/* 
Resolution of DryIoc.MefAttributedModel.UnitTests.CUT.WithUnregisteredExternalEdependency
Failed with: DryIoc.ContainerException
----------------------
Unable to resolve DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool as parameter "tool"
 in DryIoc.MefAttributedModel.UnitTests.CUT.WithUnregisteredExternalEdependency
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.WithUnregisteredExternalEdependency}.
Please ensure you have service registered (with proper key) - 95% of cases.
Remaining 5%: Service does not match the reuse scope, or service has wrong Setup.With(condition), or no Rules.WithUnknownServiceResolver(ForMyService).
*/

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NamedOne), "blah", 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(188, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedOne()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.LazyDepClient), 
                (state, r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.LazyDepClient((DryIoc.MefAttributedModel.UnitTests.CUT.LazyDep)((DryIoc.IResolver)r.Resolver).ResolveKeyed(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.LazyDep), (object)null, DryIoc.IfUnresolved.Throw, (System.Type)null, r.Resolver.GetOrCreateResolutionScope(ref scope, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.LazyDepClient), (object)null))));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.One), "two", 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(161, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.One), "one", 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(161, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(130, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithFieldAndProperty), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(193, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithFieldAndProperty { Property = (DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService)r.Resolver.SingletonScope.GetOrAdd(208, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService()), Field = (DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService)r.Resolver.SingletonScope.GetOrAdd(208, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService()) }));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler), "fast", 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(149, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.KeyClient), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(184, () => new DryIoc.MefAttributedModel.UnitTests.CUT.KeyClient((DryIoc.MefAttributedModel.UnitTests.CUT.KeyService)r.Resolver.SingletonScope.GetOrAdd(185, () => new DryIoc.MefAttributedModel.UnitTests.CUT.KeyService()))));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.WithBothTheSameExports), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(187, () => new DryIoc.MefAttributedModel.UnitTests.CUT.WithBothTheSameExports()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Me), 
                (state, r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.Me());
/* 
Resolution of DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumerNotFound
Failed with: DryIoc.MefAttributedModel.AttributedModelException
----------------------
Unable to resolve dependency DryIoc.MefAttributedModel.UnitTests.CUT.IFooService with metadata [NotFound] in DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumerNotFound
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumerNotFound}
*/

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IForExport), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(201, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ForExport()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(144, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2((DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject2)r.Resolver.SingletonScope.GetOrAdd(141, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject2()))));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISomeDb), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(134, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Chicken), DefaultKey.Of(0), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(181, () => DryIoc.MefAttributedModel.UnitTests.CUT.BirdFactory.Chicken));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Chicken), DefaultKey.Of(1), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(182, () => DryIoc.MefAttributedModel.UnitTests.CUT.StaticBirdFactory.Chicken));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DbMan), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(134, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample), 1, 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(146, () => new DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported), "c", 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(203, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported), "b", 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(203, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported), "a", 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(203, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Apple), DefaultKey.Of(0), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(171, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory)r.Resolver.SingletonScope.GetOrAdd(169, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory())).CreateApple()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Apple), "apple", 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(174, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory)r.Resolver.SingletonScope.GetOrAdd(172, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory())).CreateApple()));
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

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DependentService), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(122, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DependentService(new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService(), (DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService)r.Resolver.SingletonScope.GetOrAdd(118, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService()), new DryIoc.MefAttributedModel.UnitTests.CUT.TransientOpenGenericService<string>(), (DryIoc.MefAttributedModel.UnitTests.CUT.OpenGenericServiceWithTwoParameters<bool, bool>)r.Resolver.SingletonScope.GetOrAdd(211, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OpenGenericServiceWithTwoParameters<bool, bool>()))));
/* 
Resolution of DryIoc.MefAttributedModel.UnitTests.CUT.MyCode
Failed with: DryIoc.ContainerException
----------------------
Unable to resolve wrapper DryIoc.Meta<Lazy<DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool>, DryIoc.MefAttributedModel.UnitTests.CUT.MineMeta> {required: DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool} as parameter "tool"
 in DryIoc.MefAttributedModel.UnitTests.CUT.MyCode
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.MyCode}.
Please ensure you have service registered (with proper key) - 95% of cases.
Remaining 5%: Service does not match the reuse scope, or service has wrong Setup.With(condition), or no Rules.WithUnknownServiceResolver(ForMyService).
*/

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Duck), DefaultKey.Of(0), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(180, () => DryIoc.MefAttributedModel.UnitTests.CUT.BirdFactory.GetDuck()));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Duck), DefaultKey.Of(1), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(183, () => DryIoc.MefAttributedModel.UnitTests.CUT.StaticBirdFactory.Duck));

            Register(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ForExportBase), 
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(202, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ForExportBaseImpl()));
        } // end of static constructor
    }
}