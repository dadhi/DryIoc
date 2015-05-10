using System;
using System.Linq; // for Enumerable.Cast method required for LazyEnumerable<T>

namespace DryIoc.Zero.UnitTests
{
    public static class ServiceFactory
    {
        public static object Resolve(this IResolverContextProvider r, Type type)
        {
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ClientWithServiceAndPrimitiveProperty))
                return Create_1(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OrangeFactory))
                return Create_2(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FactoryConsumer))
                return Create_9(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.TransientOrangeFactory))
                return Create_13(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService))
                return Create_20(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FuncFactory))
                return Create_21(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Service))
                return Create_22(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.LazyDepClient))
                return Create_27(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory))
                return Create_28(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory))
                return Create_29(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISomeDb))
                return Create_31(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata))
                return Create_32(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithFieldAndProperty))
                return Create_33(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooBlah))
                return Create_43(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumer))
                return Create_44(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ITransientService))
                return Create_45(r, null);
            if (type == typeof(System.Func<string, DryIoc.MefAttributedModel.UnitTests.CUT.Orange>))
                return Create_46(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1))
                return Create_47(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IForExport))
                return Create_48(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NativeUser))
                return Create_49(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISingletonService))
                return Create_50(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ForExportBase))
                return Create_52(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MyCode))
                return Create_53(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.HomeUser))
                return Create_56(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3))
                return Create_58(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAnotherDb))
                return Create_59(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IDecoratedResult))
                return Create_61(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Me))
                return Create_62(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting))
                return Create_63(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.AppleFactory))
                return Create_70(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithImportedCtorParameter))
                return Create_76(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DbMan))
                return Create_77(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.OneDependsOnExternalTool))
                return Create_78(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.BothExportManyAndExport))
                return Create_79(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.BirdFactory))
                return Create_80(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooHey))
                return Create_82(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2))
                return Create_84(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.KeyClient))
                return Create_85(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.UseLazyEnumerable))
                return Create_86(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.LazyDep))
                return Create_87(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.WithBothTheSameExports))
                return Create_88(r, null);
            if (type == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DependentService))
                return Create_89(r, null);
            return null;
        }

        internal static object Create_1(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(51, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ClientWithServiceAndPrimitiveProperty { Service = (DryIoc.MefAttributedModel.UnitTests.CUT.KeyService)r.Resolver.SingletonScope.GetOrAdd(91, () => new DryIoc.MefAttributedModel.UnitTests.CUT.KeyService()) });
        }

        internal static object Create_2(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(73, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OrangeFactory());
        }

        internal static object Create_3(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(94, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedOne());
        }

        internal static object Create_4(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(95, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BothExportManyAndExport());
        }

        internal static object Create_5(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(95, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BothExportManyAndExport());
        }

        internal static object Create_6(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(77, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory)r.Resolver.SingletonScope.GetOrAdd(75, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory())).CreateApple());
        }

        internal static object Create_7(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(80, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory)r.Resolver.SingletonScope.GetOrAdd(78, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory())).CreateApple());
        }

        internal static object Create_8(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(52, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler());
        }

        internal static object Create_9(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(66, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FactoryConsumer(new DryIoc.MefAttributedModel.UnitTests.CUT.IFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>[] { (DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>)r.Resolver.SingletonScope.GetOrAdd(110, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>(() => (DryIoc.MefAttributedModel.UnitTests.CUT.One)r.Resolver.SingletonScope.GetOrAdd(67, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One()))), (DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>)r.Resolver.SingletonScope.GetOrAdd(111, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DryFactory<DryIoc.MefAttributedModel.UnitTests.CUT.One>(() => (DryIoc.MefAttributedModel.UnitTests.CUT.One)r.Resolver.SingletonScope.GetOrAdd(67, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One()))) }));
        }

        internal static object Create_10(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(109, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported());
        }

        internal static object Create_11(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(109, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported());
        }

        internal static object Create_12(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(109, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported());
        }

        internal static object Create_13(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(81, () => new DryIoc.MefAttributedModel.UnitTests.CUT.TransientOrangeFactory());
        }

        internal static object Create_14(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(74, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.OrangeFactory)r.Resolver.SingletonScope.GetOrAdd(73, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OrangeFactory())).Create());
        }

        internal static object Create_15(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(76, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory)r.Resolver.SingletonScope.GetOrAdd(75, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory())).CreateOrange());
        }

        internal static object Create_16(IResolverContextProvider r, IScope scope)
        {
            return ((DryIoc.MefAttributedModel.UnitTests.CUT.TransientOrangeFactory)r.Resolver.SingletonScope.GetOrAdd(81, () => new DryIoc.MefAttributedModel.UnitTests.CUT.TransientOrangeFactory())).Create();
        }

        internal static object Create_17(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(79, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory)r.Resolver.SingletonScope.GetOrAdd(78, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory())).CreateOrange());
        }

        internal static object Create_18(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(103, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooHey());
        }

        internal static object Create_19(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(104, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooBlah());
        }

        internal static object Create_20(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(19, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService());
        }

        internal static object Create_21(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(83, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FuncFactory());
        }

        internal static object Create_22(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(18, () => new DryIoc.MefAttributedModel.UnitTests.CUT.Service());
        }

        internal static object Create_23(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(36, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherNamedService());
        }

        internal static object Create_24(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(35, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedService());
        }

        internal static object Create_25(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(94, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedOne());
        }

        internal static object Create_26(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(95, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BothExportManyAndExport());
        }

        internal static object Create_27(IResolverContextProvider r, IScope scope)
        {
            return new DryIoc.MefAttributedModel.UnitTests.CUT.LazyDepClient((DryIoc.MefAttributedModel.UnitTests.CUT.LazyDep)((DryIoc.IResolver)r.Resolver).ResolveKeyed(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.LazyDep), (object)null, DryIoc.IfUnresolved.Throw, (System.Type)null, r.Resolver.GetOrCreateResolutionScope(ref scope, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.LazyDepClient), (object)null)));
        }

        internal static object Create_28(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(75, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FruitFactory());
        }

        internal static object Create_29(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(78, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedFruitFactory());
        }

        internal static object Create_30(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(53, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler());
        }

        internal static object Create_31(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(37, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan());
        }

        internal static object Create_32(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(33, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata());
        }

        internal static object Create_33(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(99, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithFieldAndProperty { Property = (DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService)r.Resolver.SingletonScope.GetOrAdd(112, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService()), Field = (DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService)r.Resolver.SingletonScope.GetOrAdd(112, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService()) });
        }

        internal static object Create_34(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(87, () => DryIoc.MefAttributedModel.UnitTests.CUT.BirdFactory.Chicken);
        }

        internal static object Create_35(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(88, () => DryIoc.MefAttributedModel.UnitTests.CUT.StaticBirdFactory.Chicken);
        }

        internal static object Create_36(IResolverContextProvider r, IScope scope)
        {
            return new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_2_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandlerDecorator(_2_IHandler0)))((DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler)r.Resolver.SingletonScope.GetOrAdd(54, () => new DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler()));
        }

        internal static object Create_37(IResolverContextProvider r, IScope scope)
        {
            return new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_0_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))(new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_3_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.CustomHandlerDecorator(_3_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.LoggingHandlerDecorator(_0_IHandler0))))((DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler)r.Resolver.SingletonScope.GetOrAdd(53, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SlowHandler()));
        }

        internal static object Create_38(IResolverContextProvider r, IScope scope)
        {
            return new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_1_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.RetryHandlerDecorator(_1_IHandler0)))((DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler)r.Resolver.SingletonScope.GetOrAdd(52, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FastHandler()));
        }

        internal static object Create_39(IResolverContextProvider r, IScope scope)
        {
            return new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0))((DryIoc.MefAttributedModel.UnitTests.CUT.BlahHandler)r.Resolver.SingletonScope.GetOrAdd(60, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BlahHandler()));
        }

        internal static object Create_40(IResolverContextProvider r, IScope scope)
        {
            return new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_4_IHandler0 => new System.Func<DryIoc.MefAttributedModel.UnitTests.CUT.IHandler, DryIoc.MefAttributedModel.UnitTests.CUT.IHandler>(_5_IHandler0 => new DryIoc.MefAttributedModel.UnitTests.CUT.FoohDecorator(_5_IHandler0))(new DryIoc.MefAttributedModel.UnitTests.CUT.DecoratorWithFastHandlerImport(_4_IHandler0)))((DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler)r.Resolver.SingletonScope.GetOrAdd(61, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler()));
        }

        internal static object Create_41(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(67, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One());
        }

        internal static object Create_42(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(67, () => new DryIoc.MefAttributedModel.UnitTests.CUT.One());
        }

        internal static object Create_43(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(104, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooBlah());
        }

        internal static object Create_44(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(105, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumer(new System.Lazy<DryIoc.MefAttributedModel.UnitTests.CUT.IFooService>(() => (DryIoc.MefAttributedModel.UnitTests.CUT.IFooService)((DryIoc.IResolver)r.Resolver).ResolveKeyed(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IFooService), (object)DryIoc.DefaultKey.Of(1), DryIoc.IfUnresolved.Throw, (System.Type)null, r.Resolver.GetOrCreateResolutionScope(ref scope, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.FooConsumer), (object)null)))));
        }

        internal static object Create_45(IResolverContextProvider r, IScope scope)
        {
            return new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService();
        }

        internal static object Create_46(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(84, () => ((DryIoc.MefAttributedModel.UnitTests.CUT.FuncFactory)r.Resolver.SingletonScope.GetOrAdd(83, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FuncFactory())).Create());
        }

        internal static object Create_47(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(46, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1((DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject)r.Resolver.SingletonScope.GetOrAdd(43, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject())));
        }

        internal static object Create_48(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(107, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ForExport());
        }

        internal static object Create_49(IResolverContextProvider r, IScope scope)
        {
            return new DryIoc.MefAttributedModel.UnitTests.CUT.NativeUser(new DryIoc.MefAttributedModel.UnitTests.CUT.ForeignTool());
        }

        internal static object Create_50(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(21, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService());
        }

        internal static object Create_51(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(54, () => new DryIoc.MefAttributedModel.UnitTests.CUT.TransactHandler());
        }

        internal static object Create_52(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(108, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ForExportBaseImpl());
        }

        internal static object Create_53(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(98, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MyCode(new DryIoc.Meta<System.Lazy<DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool>, DryIoc.MefAttributedModel.UnitTests.CUT.MineMeta>(new System.Lazy<DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool>(() => (DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool)((DryIoc.IResolver)r.Resolver).ResolveKeyed(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool), (object)DryIoc.DefaultKey.Of(0), DryIoc.IfUnresolved.Throw, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool), r.Resolver.GetOrCreateResolutionScope(ref scope, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MyCode), (object)null))), DryIoc.MefAttributedModel.UnitTests.CUT.MineMeta.Green)));
        }

        internal static object Create_54(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(86, () => DryIoc.MefAttributedModel.UnitTests.CUT.BirdFactory.GetDuck());
        }

        internal static object Create_55(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(89, () => DryIoc.MefAttributedModel.UnitTests.CUT.StaticBirdFactory.Duck);
        }

        internal static object Create_56(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(97, () => new DryIoc.MefAttributedModel.UnitTests.CUT.HomeUser(_String0 => (DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool)r.Resolver.SingletonScope.GetOrAdd(114, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool())));
        }

        internal static object Create_57(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(61, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FoohHandler());
        }

        internal static object Create_58(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(48, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3((DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject3)r.Resolver.SingletonScope.GetOrAdd(45, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject3())));
        }

        internal static object Create_59(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(37, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan());
        }

        internal static object Create_60(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(92, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OtherKeyService());
        }

        internal static object Create_61(IResolverContextProvider r, IScope scope)
        {
            return new DryIoc.MefAttributedModel.UnitTests.CUT.LazyDecorator(new System.Lazy<DryIoc.MefAttributedModel.UnitTests.CUT.IDecoratedResult>(() => (DryIoc.MefAttributedModel.UnitTests.CUT.IDecoratedResult)((DryIoc.IResolver)r.Resolver).ResolveKeyed(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IDecoratedResult), (object)null, DryIoc.IfUnresolved.Throw, (System.Type)null, r.Resolver.GetOrCreateResolutionScope(ref scope, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IDecoratedResult), (object)null))));
        }

        internal static object Create_62(IResolverContextProvider r, IScope scope)
        {
            return new DryIoc.MefAttributedModel.UnitTests.CUT.Me();
        }

        internal static object Create_63(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(32, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting(new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService()));
        }

        internal static object Create_64(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(109, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported());
        }

        internal static object Create_65(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(109, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported());
        }

        internal static object Create_66(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(109, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported());
        }

        internal static object Create_67(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(28, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OneServiceWithMetadata());
        }

        internal static object Create_68(IResolverContextProvider r, IScope scope)
        {
            return new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherServiceWithMetadata();
        }

        internal static object Create_69(IResolverContextProvider r, IScope scope)
        {
            return new DryIoc.MefAttributedModel.UnitTests.CUT.YetAnotherServiceWithMetadata();
        }

        internal static object Create_70(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(72, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AppleFactory());
        }

        internal static object Create_71(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(94, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedOne());
        }

        internal static object Create_72(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(18, () => new DryIoc.MefAttributedModel.UnitTests.CUT.Service());
        }

        internal static object Create_73(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(91, () => new DryIoc.MefAttributedModel.UnitTests.CUT.KeyService());
        }

        internal static object Create_74(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(19, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherService());
        }

        internal static object Create_75(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(92, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OtherKeyService());
        }

        internal static object Create_76(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(34, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithImportedCtorParameter((DryIoc.MefAttributedModel.UnitTests.CUT.AnotherNamedService)r.Resolver.SingletonScope.GetOrAdd(36, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AnotherNamedService())));
        }

        internal static object Create_77(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(37, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan());
        }

        internal static object Create_78(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(100, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OneDependsOnExternalTool((DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool)r.Resolver.SingletonScope.GetOrAdd(116, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExternalTool())));
        }

        internal static object Create_79(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(95, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BothExportManyAndExport());
        }

        internal static object Create_80(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(85, () => new DryIoc.MefAttributedModel.UnitTests.CUT.BirdFactory());
        }

        internal static object Create_81(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(49, () => new DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample());
        }

        internal static object Create_82(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(103, () => new DryIoc.MefAttributedModel.UnitTests.CUT.FooHey());
        }

        internal static object Create_83(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(49, () => new DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample());
        }

        internal static object Create_84(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(47, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2((DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject2)r.Resolver.SingletonScope.GetOrAdd(44, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject2())));
        }

        internal static object Create_85(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(90, () => new DryIoc.MefAttributedModel.UnitTests.CUT.KeyClient((DryIoc.MefAttributedModel.UnitTests.CUT.KeyService)r.Resolver.SingletonScope.GetOrAdd(91, () => new DryIoc.MefAttributedModel.UnitTests.CUT.KeyService())));
        }

        internal static object Create_86(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(39, () => new DryIoc.MefAttributedModel.UnitTests.CUT.UseLazyEnumerable(new DryIoc.LazyEnumerable<DryIoc.MefAttributedModel.UnitTests.CUT.Me>(r.Resolver.ResolveMany(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Me), null, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.Me), null, r.Resolver.GetOrCreateResolutionScope(ref scope, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.UseLazyEnumerable), (object)null)).Cast<DryIoc.MefAttributedModel.UnitTests.CUT.Me>())));
        }

        internal static object Create_87(IResolverContextProvider r, IScope scope)
        {
            return new DryIoc.MefAttributedModel.UnitTests.CUT.LazyDep();
        }

        internal static object Create_88(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(93, () => new DryIoc.MefAttributedModel.UnitTests.CUT.WithBothTheSameExports());
        }

        internal static object Create_89(IResolverContextProvider r, IScope scope)
        {
            return r.Resolver.SingletonScope.GetOrAdd(25, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DependentService(new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService(), (DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService)r.Resolver.SingletonScope.GetOrAdd(21, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService()), new DryIoc.MefAttributedModel.UnitTests.CUT.TransientOpenGenericService<string>(), (DryIoc.MefAttributedModel.UnitTests.CUT.OpenGenericServiceWithTwoParameters<bool, bool>)r.Resolver.SingletonScope.GetOrAdd(118, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OpenGenericServiceWithTwoParameters<bool, bool>())));
        }

    }
}