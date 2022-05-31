using System;
using DryIoc;

namespace RealisticUnitOfWork
{
    internal static class GeneratedResolutionGraph_MsDI
    {
        static Func<IResolverContext, object> GetFactory()
        {
            return (Func<IResolverContext, object>) ((IResolverContext r) => //$
                r.CurrentOrSingletonScope.GetOrAddViaFactoryDelegate(
                    256,
                    (Func<IResolverContext, object>) ((IResolverContext r) => //$
                        new R(
                            default(Single1) /* (!) Please provide the non-default value for the constant */,
                            default(Single2) /* (!) Please provide the non-default value for the constant */,
                            ((Scoped1) r.CurrentOrSingletonScope.GetOrAddViaFactoryDelegate(
                                257,
                                (Func<IResolverContext, object>) ((IResolverContext r) => //$
                                    new Scoped1(
                                        default(Single12) /* (!) Please provide the non-default value for the constant */,
                                        default(SingleObj12) /* (!) Please provide the non-default value for the constant */,
                                        ((ScopedFac12) r.CurrentOrSingletonScope.GetOrAddViaFactoryDelegate(
                                            275,
                                            default(Func<IResolverContext, object>) /* (!) Please provide the non-default value for the constant */,
                                            r)),
                                        new Trans12(
                                            new Trans13(
                                                default(Single14) /* (!) Please provide the non-default value for the constant */,
                                                new Trans14()),
                                            default(Single13) /* (!) Please provide the non-default value for the constant */,
                                            default(SingleObj13) /* (!) Please provide the non-default value for the constant */),
                                        default(Single1) /* (!) Please provide the non-default value for the constant */,
                                        default(SingleObj1) /* (!) Please provide the non-default value for the constant */,
                                        ((Scoped12) r.CurrentOrSingletonScope.GetOrAddViaFactoryDelegate(
                                            269,
                                            (Func<IResolverContext, object>) ((IResolverContext r) => //$
                                                new Scoped12(
                                                    default(Single13) /* (!) Please provide the non-default value for the constant */,
                                                    default(SingleObj13) /* (!) Please provide the non-default value for the constant */,
                                                    ((Scoped13) r.CurrentOrSingletonScope.GetOrAddViaFactoryDelegate(
                                                        279,
                                                        (Func<IResolverContext, object>) ((IResolverContext r) => //$
                                                            new Scoped13(
                                                                default(
                                                                    Single1) /* (!) Please provide the non-default value for the constant */,
                                                                ((Scoped14) r.CurrentOrSingletonScope
                                                                    .GetOrAddViaFactoryDelegate(
                                                                        289,
                                                                        (Func<IResolverContext, object>) ((IResolverContext r) => //$
                                                                            new Scoped14()),
                                                                        r)))),
                                                        r)),
                                                    ((ScopedFac13) r.CurrentOrSingletonScope.GetOrAddViaFactoryDelegate(
                                                        285,
                                                        default(
                                                            Func<IResolverContext, object>) /* (!) Please provide the non-default value for the constant */,
                                                        r)),
                                                    new Trans13(
                                                        default(Single14) /* (!) Please provide the non-default value for the constant */,
                                                        new Trans14()),
                                                    default(Single1) /* (!) Please provide the non-default value for the constant */,
                                                    default(SingleObj1) /* (!) Please provide the non-default value for the constant */)),
                                            r)))),
                                r)),
                            ((Scoped2) r.CurrentOrSingletonScope.GetOrAddViaFactoryDelegate(
                                258,
                                (Func<IResolverContext, object>) ((IResolverContext r) => //$
                                    new Scoped2(
                                        default(Single22) /* (!) Please provide the non-default value for the constant */,
                                        default(SingleObj22) /* (!) Please provide the non-default value for the constant */,
                                        ((ScopedFac22) r.CurrentOrSingletonScope.GetOrAddViaFactoryDelegate(
                                            276,
                                            default(Func<IResolverContext, object>) /* (!) Please provide the non-default value for the constant */,
                                            r)),
                                        new Trans22(
                                            new Trans23(
                                                default(Single24) /* (!) Please provide the non-default value for the constant */,
                                                new Trans24()),
                                            default(Single23) /* (!) Please provide the non-default value for the constant */,
                                            default(SingleObj23) /* (!) Please provide the non-default value for the constant */),
                                        default(Single2) /* (!) Please provide the non-default value for the constant */,
                                        default(SingleObj2) /* (!) Please provide the non-default value for the constant */,
                                        ((Scoped22) r.CurrentOrSingletonScope.GetOrAddViaFactoryDelegate(
                                            270,
                                            (Func<IResolverContext, object>) ((IResolverContext r) => //$
                                                new Scoped22(
                                                    default(Single23) /* (!) Please provide the non-default value for the constant */,
                                                    default(SingleObj23) /* (!) Please provide the non-default value for the constant */,
                                                    ((Scoped23) r.CurrentOrSingletonScope.GetOrAddViaFactoryDelegate(
                                                        280,
                                                        (Func<IResolverContext, object>) ((IResolverContext r) => //$
                                                            new Scoped23(
                                                                default(
                                                                    Single2) /* (!) Please provide the non-default value for the constant */,
                                                                ((Scoped24) r.CurrentOrSingletonScope
                                                                    .GetOrAddViaFactoryDelegate(
                                                                        290,
                                                                        (Func<IResolverContext, object>) ((IResolverContext r) => //$
                                                                            new Scoped24()),
                                                                        r)))),
                                                        r)),
                                                    ((ScopedFac23) r.CurrentOrSingletonScope.GetOrAddViaFactoryDelegate(
                                                        286,
                                                        default(
                                                            Func<IResolverContext, object>) /* (!) Please provide the non-default value for the constant */,
                                                        r)),
                                                    new Trans23(
                                                        default(Single24) /* (!) Please provide the non-default value for the constant */,
                                                        new Trans24()),
                                                    default(Single2) /* (!) Please provide the non-default value for the constant */,
                                                    default(SingleObj2) /* (!) Please provide the non-default value for the constant */)),
                                            r)))),
                                r)),
                            new Trans1(
                                new Trans13(
                                    default(Single14) /* (!) Please provide the non-default value for the constant */,
                                    new Trans14()),
                                new Trans23(
                                    default(Single24) /* (!) Please provide the non-default value for the constant */,
                                    new Trans24()),
                                default(Single13) /* (!) Please provide the non-default value for the constant */,
                                default(Single1) /* (!) Please provide the non-default value for the constant */,
                                default(SingleObj1) /* (!) Please provide the non-default value for the constant */),
                            new Trans2(
                                new Trans13(
                                    default(Single14) /* (!) Please provide the non-default value for the constant */,
                                    new Trans14()),
                                new Trans23(
                                    default(Single24) /* (!) Please provide the non-default value for the constant */,
                                    new Trans24()),
                                default(Single23) /* (!) Please provide the non-default value for the constant */,
                                default(Single2) /* (!) Please provide the non-default value for the constant */,
                                default(SingleObj2) /* (!) Please provide the non-default value for the constant */),
                            ((ScopedFac1) r.CurrentOrSingletonScope.GetOrAddViaFactoryDelegate(
                                263,
                                default(Func<IResolverContext, object>) /* (!) Please provide the non-default value for the constant */,
                                r)),
                            ((ScopedFac2) r.CurrentOrSingletonScope.GetOrAddViaFactoryDelegate(
                                264,
                                default(Func<IResolverContext, object>) /* (!) Please provide the non-default value for the constant */,
                                r)),
                            default(SingleObj1) /* (!) Please provide the non-default value for the constant */,
                            default(SingleObj2) /* (!) Please provide the non-default value for the constant */)),
                    r));
        }
    }
}