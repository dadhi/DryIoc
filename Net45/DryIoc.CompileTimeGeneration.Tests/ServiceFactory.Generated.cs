namespace DryIoc.CompileTimeGeneration.Tests
{
    using System;
    using System.Linq; // For Cast method required for LazyEnumerable<T>

    public partial class ServiceFactory
    {
        static ServiceFactory()
        {

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "transact",
                (state, r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_2_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandlerDecorator(_2_IHandler0)))((DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler)r.Resolver.SingletonScope.GetOrAdd(238, () => new DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler()))),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "slow",
                (state, r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_0_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))(new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_3_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.CustomHandlerDecorator(_3_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.LoggingHandlerDecorator(_0_IHandler0))))((DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler)r.Resolver.SingletonScope.GetOrAdd(237, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler()))),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "fast",
                (state, r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_1_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.RetryHandlerDecorator(_1_IHandler0)))((DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler)r.Resolver.SingletonScope.GetOrAdd(236, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler()))),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DryIoc.MefAttributedModel.UnitTests.CUT.BlahFooh.Blah,
                (state, r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))((DryIoc.MefAttributedModel.UnitTests.CUT.BlahHandler)r.Resolver.SingletonScope.GetOrAdd(244, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BlahHandler()))),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IHandler),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DryIoc.MefAttributedModel.UnitTests.CUT.BlahFooh.Fooh,
                (state, r, scope) => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_5_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.FoohDecorator(_5_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0)))((DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler)r.Resolver.SingletonScope.GetOrAdd(245, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler()))),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(216, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting(new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService())));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FuncFactory),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(264, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FuncFactory()));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAnotherDb),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(221, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan()));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Orange),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DefaultKey.Of(0),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(255, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.OrangeFactory)r.Resolver.SingletonScope.GetOrAdd(254, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OrangeFactory())).Create())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Orange),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DefaultKey.Of(1),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(257, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory)r.Resolver.SingletonScope.GetOrAdd(256, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory())).CreateOrange())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Orange),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DefaultKey.Of(2),
                (state, r, scope) => ((DryIoc.MefAttributedModel.UnitTests.CUT.TransientOrangeFactory)r.Resolver.SingletonScope.GetOrAdd(262, () => new DryIoc.MefAttributedModel.UnitTests.CUT.TransientOrangeFactory())).Create()),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Orange),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "orange",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(260, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory)r.Resolver.SingletonScope.GetOrAdd(259, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory())).CreateOrange())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.KeyClient),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(266, () => new DryIoc.MefAttributedModel.UnitTests.CUT.KeyClient((DryIoc.MefAttributedModel.UnitTests.CUT.KeyService)r.Resolver.SingletonScope.GetOrAdd(267, () => new DryIoc.MefAttributedModel.UnitTests.CUT.KeyService()))));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DependentService),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(209, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DependentService(new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService(), (DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService)r.Resolver.SingletonScope.GetOrAdd(205, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService()), new DryIoc.MefAttributedModel.UnitTests.CUT.TransientOpenGenericService<string>(), (DryIoc.MefAttributedModel.UnitTests.CUT.OpenGenericServiceWithTwoParameters<bool, bool>)r.Resolver.SingletonScope.GetOrAdd(287, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OpenGenericServiceWithTwoParameters<bool, bool>()))));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "slow",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(237, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DbMan),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(221, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan()));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooBlah),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(280, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooBlah()));

/* Exception: typeof(ContainerException)
----------------------
Unable to resolve String as parameter "message"
 in DryIoc.MefAttributedModel.UnitTests.CUT.ClientWithPrimitiveParameter
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.ClientWithPrimitiveParameter}.
Please ensure you have service registered (with proper key) - 95% of cases.
Remaining 5%: There is no Rules.WithUnknownServiceResolver(ForMyService), or service does not match the reuse scope, or service has wrong Setup.With(condition).
*/

/* Exception: typeof(ContainerException)
----------------------
Unable to resolve String as parameter "message"
 in DryIoc.MefAttributedModel.UnitTests.CUT.Two
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.Two}.
Please ensure you have service registered (with proper key) - 95% of cases.
Remaining 5%: There is no Rules.WithUnknownServiceResolver(ForMyService), or service does not match the reuse scope, or service has wrong Setup.With(condition).
*/

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Apple),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DefaultKey.Of(0),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(258, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory)r.Resolver.SingletonScope.GetOrAdd(256, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory())).CreateApple())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Apple),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "apple",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(261, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory)r.Resolver.SingletonScope.GetOrAdd(259, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory())).CreateApple())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NamedOne),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "blah",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(270, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedOne())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(231, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2((DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject2)r.Resolver.SingletonScope.GetOrAdd(228, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject2()))));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamedService),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "blah",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(220, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherNamedService())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamedService),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DefaultKey.Of(0),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(219, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedService())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

/* Exception: typeof(ContainerException)
----------------------
Unable to resolve String as parameter "message"
 in DryIoc.MefAttributedModel.UnitTests.CUT.FactoryWithArgsConsumer
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.FactoryWithArgsConsumer}.
Please ensure you have service registered (with proper key) - 95% of cases.
Remaining 5%: There is no Rules.WithUnknownServiceResolver(ForMyService), or service does not match the reuse scope, or service has wrong Setup.With(condition).
*/

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(203, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService()));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(217, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata()));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.AppleFactory),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(253, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AppleFactory()));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                1,
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(233, () => new DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

/* Exception: typeof(ContainerException)
----------------------
Unable to resolve DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface {DefaultKey.Of(0)}
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface, DefaultKey.Of(0)}.
Please ensure you have service registered (with proper key) - 95% of cases.
Remaining 5%: There is no Rules.WithUnknownServiceResolver(ForMyService), or service does not match the reuse scope, or service has wrong Setup.With(condition).
*/

/* Exception: typeof(ContainerException)
----------------------
Unable to resolve DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface {DefaultKey.Of(1)}
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface, DefaultKey.Of(1)}.
Please ensure you have service registered (with proper key) - 95% of cases.
Remaining 5%: There is no Rules.WithUnknownServiceResolver(ForMyService), or service does not match the reuse scope, or service has wrong Setup.With(condition).
*/

/* Exception: typeof(ContainerException)
----------------------
Unable to resolve DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface {DefaultKey.Of(2)}
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface, DefaultKey.Of(2)}.
Please ensure you have service registered (with proper key) - 95% of cases.
Remaining 5%: There is no Rules.WithUnknownServiceResolver(ForMyService), or service does not match the reuse scope, or service has wrong Setup.With(condition).
*/

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FactoryConsumer),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(247, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FactoryConsumer(new DryIoc.MefAttributedModel.UnitTests.CUT.IFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>[] { (DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>)r.Resolver.SingletonScope.GetOrAdd(289, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>(() => (DryIoc.MefAttributedModel.UnitTests.CUT.One)r.Resolver.SingletonScope.GetOrAdd(248, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One()))), (DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>)r.Resolver.SingletonScope.GetOrAdd(290, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>(() => (DryIoc.MefAttributedModel.UnitTests.CUT.One)r.Resolver.SingletonScope.GetOrAdd(248, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One()))) })));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ITransientService),
                (state, r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService());

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OtherKeyService),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DryIoc.MefAttributedModel.UnitTests.CUT.ServiceKey.OtherOne,
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(268, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OtherKeyService())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

/* Exception: typeof(ContainerException)
----------------------
Unable to resolve String as parameter "message"
 in DryIoc.MefAttributedModel.UnitTests.CUT.OneTransientService: DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMultipleImplentations {DefaultKey.Of(0)}
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMultipleImplentations, DefaultKey.Of(0)}.
Please ensure you have service registered (with proper key) - 95% of cases.
Remaining 5%: There is no Rules.WithUnknownServiceResolver(ForMyService), or service does not match the reuse scope, or service has wrong Setup.With(condition).
*/

/* Exception: typeof(ContainerException)
----------------------
Unable to resolve String as parameter "message"
 in DryIoc.MefAttributedModel.UnitTests.CUT.AnotherTransientService: DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMultipleImplentations {DefaultKey.Of(1)}
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMultipleImplentations, DefaultKey.Of(1)}.
Please ensure you have service registered (with proper key) - 95% of cases.
Remaining 5%: There is no Rules.WithUnknownServiceResolver(ForMyService), or service does not match the reuse scope, or service has wrong Setup.With(condition).
*/

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(System.Func<string, DryIoc.MefAttributedModel.UnitTests.CUT.Orange>),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(265, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.FuncFactory)r.Resolver.SingletonScope.GetOrAdd(264, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FuncFactory())).Create()));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IFooService),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DefaultKey.Of(0),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(279, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooHey())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IFooService),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DefaultKey.Of(1),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(280, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooBlah())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithImportedCtorParameter),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(218, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithImportedCtorParameter((DryIoc.MefAttributedModel.UnitTests.CUT.AnotherNamedService)r.Resolver.SingletonScope.GetOrAdd(220, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherNamedService()))));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.LazyDep),
                (state, r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.LazyDep());

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "transact",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(238, () => new DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.BothExportManyAndExport),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(271, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BothExportManyAndExport()));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Service),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(202, () => new DryIoc.MefAttributedModel.UnitTests.CUT.Service()));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NativeUser),
                (state, r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.NativeUser(new DryIoc.MefAttributedModel.UnitTests.CUT.ForeignTool()));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "fast",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(236, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISomeDb),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(221, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan()));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OrangeFactory),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(254, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OrangeFactory()));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IPrintToCode),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                1,
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(233, () => new DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(230, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1((DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject)r.Resolver.SingletonScope.GetOrAdd(227, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject()))));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooHey),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(279, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooHey()));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumer),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(281, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumer(new System.Lazy<DryIoc.MefAttributedModel.UnitTests.CUT.IFooService>(() => (DryIoc.MefAttributedModel.UnitTests.CUT.IFooService)((DryIoc.IResolver)r.Resolver).ResolveKeyed(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IFooService), (object)DryIoc.DefaultKey.Of(1), DryIoc.IfUnresolved.Throw, (System.Type)null, r.Resolver.GetOrCreateResolutionScope(ref scope, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumer), (object)null))))));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.One),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "two",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(248, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.One),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "one",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(248, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.TransientOrangeFactory),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(262, () => new DryIoc.MefAttributedModel.UnitTests.CUT.TransientOrangeFactory()));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IOne),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "blah",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(270, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedOne())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IOne),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DefaultKey.Of(0),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(271, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BothExportManyAndExport())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ClientWithServiceAndPrimitiveProperty),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(235, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ClientWithServiceAndPrimitiveProperty { Service = (DryIoc.MefAttributedModel.UnitTests.CUT.KeyService)r.Resolver.SingletonScope.GetOrAdd(267, () => new DryIoc.MefAttributedModel.UnitTests.CUT.KeyService()) }));

/* Exception: typeof(ContainerException)
----------------------
Unable to resolve DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool as parameter "tool"
 in DryIoc.MefAttributedModel.UnitTests.CUT.WithUnregisteredExternalEdependency
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.WithUnregisteredExternalEdependency}.
Please ensure you have service registered (with proper key) - 95% of cases.
Remaining 5%: There is no Rules.WithUnknownServiceResolver(ForMyService), or service does not match the reuse scope, or service has wrong Setup.With(condition).
*/

/* Exception: typeof(ContainerException)
----------------------
Unable to resolve DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool {13} as parameter "tool"
 in DryIoc.MefAttributedModel.UnitTests.CUT.OtherDependsOnExternalTool
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.OtherDependsOnExternalTool}.
Please ensure you have service registered (with proper key) - 95% of cases.
Remaining 5%: There is no Rules.WithUnknownServiceResolver(ForMyService), or service does not match the reuse scope, or service has wrong Setup.With(condition).
*/

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OneDependsOnExternalTool),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(276, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OneDependsOnExternalTool((DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool)r.Resolver.SingletonScope.GetOrAdd(292, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool()))));

/* Exception: typeof(AttributedModelException)
----------------------
Unable to resolve dependency DryIoc.MefAttributedModel.UnitTests.CUT.IFooService with metadata [NotFound] in DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumerNotFound
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumerNotFound}
*/

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(256, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory()));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DefaultKey.Of(0),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(202, () => new DryIoc.MefAttributedModel.UnitTests.CUT.Service())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DryIoc.MefAttributedModel.UnitTests.CUT.ServiceKey.One,
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(267, () => new DryIoc.MefAttributedModel.UnitTests.CUT.KeyService())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DefaultKey.Of(1),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(203, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IService),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DryIoc.MefAttributedModel.UnitTests.CUT.ServiceKey.OtherOne,
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(268, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OtherKeyService())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(259, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory()));

/* Exception: typeof(AttributedModelException)
----------------------
Unable to find single constructor with System.ComponentModel.Composition.ImportingConstructorAttribute in DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructors.
*/

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISingletonService),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(205, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService()));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.WithBothTheSameExports),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(269, () => new DryIoc.MefAttributedModel.UnitTests.CUT.WithBothTheSameExports()));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DryIoc.MefAttributedModel.UnitTests.CUT.BlahFooh.Fooh,
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(245, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamed),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "blah",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(270, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedOne())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamed),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "named",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(271, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BothExportManyAndExport())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.INamed),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DefaultKey.Of(0),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(271, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BothExportManyAndExport())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithFieldAndProperty),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(275, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithFieldAndProperty { Property = (DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService)r.Resolver.SingletonScope.GetOrAdd(293, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService()), Field = (DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService)r.Resolver.SingletonScope.GetOrAdd(293, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService()) }));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IServiceWithMetadata),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                DefaultKey.Of(0),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(212, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OneServiceWithMetadata())),
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

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Me),
                (state, r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.Me());

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ForExportBase),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(284, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ForExportBaseImpl()));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(232, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3((DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject3)r.Resolver.SingletonScope.GetOrAdd(229, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject3()))));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.LazyDepClient),
                (state, r, scope) => new DryIoc.MefAttributedModel.UnitTests.CUT.LazyDepClient((DryIoc.MefAttributedModel.UnitTests.CUT.LazyDep)((DryIoc.IResolver)r.Resolver).ResolveKeyed(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.LazyDep), (object)null, DryIoc.IfUnresolved.Throw, (System.Type)null, r.Resolver.GetOrCreateResolutionScope(ref scope, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.LazyDepClient), (object)null))));

/* Exception: typeof(ContainerException)
----------------------
Unable to resolve wrapper DryIoc.Meta<Lazy<DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool>, DryIoc.MefAttributedModel.UnitTests.CUT.MineMeta> {required: DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool} as parameter "tool"
 in DryIoc.MefAttributedModel.UnitTests.CUT.MyCode
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.MyCode}.
Please ensure you have service registered (with proper key) - 95% of cases.
Remaining 5%: There is no Rules.WithUnknownServiceResolver(ForMyService), or service does not match the reuse scope, or service has wrong Setup.With(condition).
*/

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.UseLazyEnumerable),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(223, () => new DryIoc.MefAttributedModel.UnitTests.CUT.UseLazyEnumerable(new DryIoc.LazyEnumerable<DryIoc.MefAttributedModel.UnitTests.CUT.Me>(r.Resolver.ResolveMany(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Me), null, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Me), null, r.Resolver.GetOrCreateResolutionScope(ref scope, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.UseLazyEnumerable), (object)null)).Cast<DryIoc.MefAttributedModel.UnitTests.CUT.Me>()))));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "c",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(285, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "j",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(285, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "i",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(285, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            DefaultResolutions = DefaultResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IForExport),
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(283, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ForExport()));

/* Exception: typeof(ContainerException)
----------------------
Unable to resolve DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool
 in wrapper Func<String, DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool> {required: DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool} as parameter "getTool"
 in DryIoc.MefAttributedModel.UnitTests.CUT.HomeUser
 in wrapper DryIoc.FactoryExpression<Object> {required: DryIoc.MefAttributedModel.UnitTests.CUT.HomeUser}.
Please ensure you have service registered (with proper key) - 95% of cases.
Remaining 5%: There is no Rules.WithUnknownServiceResolver(ForMyService), or service does not match the reuse scope, or service has wrong Setup.With(condition).
*/

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "c",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(285, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "b",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(285, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));

            KeyedResolutions = KeyedResolutions.AddOrUpdate(
                typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported),     HashTree<object, FactoryDelegate>.Empty.AddOrUpdate(
                "a",
                (state, r, scope) => r.Resolver.SingletonScope.GetOrAdd(285, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported())),
                (oldEntry, newEntry) => oldEntry.AddOrUpdate(newEntry.Key, newEntry.Value));
        } // end of static constructor
    }
}