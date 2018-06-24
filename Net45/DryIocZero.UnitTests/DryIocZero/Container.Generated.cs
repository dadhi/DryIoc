/*
The MIT License (MIT)

Copyright (c) 2016 Maksim Volkau

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

/*
========================================================================================================
NOTE: The code below is generated automatically at compile-time and not supposed to be changed by hand.
========================================================================================================
There are 2 generation issues (may be not an error dependent on context):

The issues with run-time registrations may be solved by `container.RegisterPlaceholder<T>()` 
in Registrations.ttinclude. Then you can replace placeholders using `DryIocZero.Container.Register`
at runtime.

--------------------------------------------------------------------------------------------------------
1. DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructors
Error: Unable to find single constructor nor marked with System.ComponentModel.Composition.ImportingConstructorAttribute nor default constructor in DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructors when resolving: singleton DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructors #47
  from container
2. DryIoc.MefAttributedModel.UnitTests.CUT.OpGen<>
Error: Resolving open-generic service type is not possible for type: DryIoc.MefAttributedModel.UnitTests.CUT.OpGen<>.
*/

using System;
using System.Linq; // for Enumerable.Cast method required by LazyEnumerable<T>
using System.Collections.Generic;
using System.Threading;
using ImTools;
using static DryIocZero.ResolveManyResult;

// Specified `NamespaceUsings` if any:
using DryIoc.MefAttributedModel.UnitTests.CUT;

namespace DryIocZero
{
    partial class Container
    {
        [ExcludeFromCodeCoverage]
        partial void GetLastGeneratedFactoryID(ref int lastFactoryID)
        {
            lastFactoryID = 230; // generated: equals to last used Factory.FactoryID 
        }

        [ExcludeFromCodeCoverage]
        partial void ResolveGenerated(ref object service, Type serviceType)
        {
            if (serviceType == typeof(ISingletonService))
                service = Get0_ISingletonService(this);

            else
            if (serviceType == typeof(IOpGen<string>))
                service = Get1_IOpGen(this);

            else
            if (serviceType == typeof(ClGenB))
                service = Get2_ClGenB(this);

            else
            if (serviceType == typeof(SingleServiceWithMetadata))
                service = Get3_SingleServiceWithMetadata(this);

            else
            if (serviceType == typeof(IOpGen<Aa>))
                service = Get4_IOpGen(this);

            else
            if (serviceType == typeof(ImportConditionObject2))
                service = Get5_ImportConditionObject2(this);

            else
            if (serviceType == typeof(NamedScopeService))
                service = Get6_NamedScopeService(this);

            else
            if (serviceType == typeof(ITransientService))
                service = Get7_ITransientService(this);

            else
            if (serviceType == typeof(ClGen))
                service = Get8_ClGen(this);

            else
            if (serviceType == typeof(ImportConditionObject1))
                service = Get9_ImportConditionObject1(this);

            else
            if (serviceType == typeof(DependentService))
                service = Get10_DependentService(this);

            else
            if (serviceType == typeof(WeaklyReferencedService))
                service = Get11_WeaklyReferencedService(this);

            else
            if (serviceType == typeof(B))
                service = Get15_B(this);

            else
            if (serviceType == typeof(IOpGen<string>))
                service = Get16_IOpGen(this);

            else
            if (serviceType == typeof(PreventDisposalService))
                service = Get22_PreventDisposalService(this);

            else
            if (serviceType == typeof(ServiceWithMultipleCostructorsAndOneImporting))
                service = Get23_ServiceWithMultipleCostructorsAndOneImporting(this);

            else
            if (serviceType == typeof(IOpGen<Bb>))
                service = Get24_IOpGen(this);

            else
            if (serviceType == typeof(IAnotherDb))
                service = Get25_IAnotherDb(this);

            else
            if (serviceType == typeof(ImportConditionObject3))
                service = Get26_ImportConditionObject3(this);

            else
            if (serviceType == typeof(DbMan))
                service = Get27_DbMan(this);

            else
            if (serviceType == typeof(ClGenA))
                service = Get28_ClGenA(this);

            else
            if (serviceType == typeof(ISomeDb))
                service = Get29_ISomeDb(this);
        }

        [ExcludeFromCodeCoverage]
        partial void ResolveGenerated(ref object service,
            Type serviceType, object serviceKey, Type requiredServiceType, Request preRequestParent, object[] args)
        {
            if (serviceType == typeof(IMultiExported)) 
            {
                if ("c".Equals(serviceKey))
                    service = Get12_IMultiExported(this);

                else
                if ("j".Equals(serviceKey))
                    service = Get13_IMultiExported(this);

                else
                if ("i".Equals(serviceKey))
                    service = Get14_IMultiExported(this);
            }

            else
            if (serviceType == typeof(IAllOpts)) 
            {
                if (ImTools.KV.Of("a", 1).Equals(serviceKey))
                    service = Get17_IAllOpts(this);

                else
                if ("a".Equals(serviceKey))
                    service = Get18_IAllOpts(this);
            }

            else
            if (serviceType == typeof(MultiExported)) 
            {
                if ("c".Equals(serviceKey))
                    service = Get19_MultiExported(this);

                else
                if ("b".Equals(serviceKey))
                    service = Get20_MultiExported(this);

                else
                if ("a".Equals(serviceKey))
                    service = Get21_MultiExported(this);
            }

            else
            if (serviceType == typeof(IExportConditionInterface)) 
            {
                if (serviceKey == null &&
                    requiredServiceType == null &&
                    Equals(preRequestParent, Request.Empty.Push(typeof(ImportConditionObject1), default(System.Type), null, 62, FactoryType.Service, typeof(ImportConditionObject1), Reuse.Singleton, (RequestFlags.IsSingletonOrDependencyOfSingleton | RequestFlags.IsResolutionCall)))) 
                    service = GetDep0_IExportConditionInterface(this);

                else
                if (serviceKey == null &&
                    requiredServiceType == null &&
                    Equals(preRequestParent, Request.Empty.Push(typeof(ImportConditionObject3), default(System.Type), null, 64, FactoryType.Service, typeof(ImportConditionObject3), Reuse.Singleton, (RequestFlags.IsSingletonOrDependencyOfSingleton | RequestFlags.IsResolutionCall)))) 
                    service = GetDep1_IExportConditionInterface(this);
            }

            else
            if (serviceType == typeof(A)) 
            {
                if (serviceKey == null &&
                    requiredServiceType == null &&
                    Equals(preRequestParent, Request.Empty.Push(typeof(B), default(System.Type), null, 66, FactoryType.Service, typeof(B), Reuse.Transient, RequestFlags.IsResolutionCall))) 
                    service = GetDep2_A(this);
            }
        }

        [ExcludeFromCodeCoverage]
        partial void ResolveManyGenerated(ref IEnumerable<ResolveManyResult> services, Type serviceType) 
        {
            services = ResolveManyGenerated(serviceType);
        }

        [ExcludeFromCodeCoverage]
        private IEnumerable<ResolveManyResult> ResolveManyGenerated(Type serviceType)
        {
            if (serviceType == typeof(ISingletonService))
            {
                yield return Of(Get0_ISingletonService);
            }

            if (serviceType == typeof(IOpGen<string>))
            {
                yield return Of(Get1_IOpGen);
                yield return Of(Get16_IOpGen, typeof(IOpGen<>));
            }

            if (serviceType == typeof(ClGenB))
            {
                yield return Of(Get2_ClGenB);
            }

            if (serviceType == typeof(SingleServiceWithMetadata))
            {
                yield return Of(Get3_SingleServiceWithMetadata);
            }

            if (serviceType == typeof(IOpGen<Aa>))
            {
                yield return Of(Get4_IOpGen);
                yield return Of(Get24_IOpGen); // co-variant
            }

            if (serviceType == typeof(ImportConditionObject2))
            {
                yield return Of(Get5_ImportConditionObject2);
            }

            if (serviceType == typeof(NamedScopeService))
            {
                yield return Of(Get6_NamedScopeService);
            }

            if (serviceType == typeof(ITransientService))
            {
                yield return Of(Get7_ITransientService);
            }

            if (serviceType == typeof(ClGen))
            {
                yield return Of(Get8_ClGen);
            }

            if (serviceType == typeof(ImportConditionObject1))
            {
                yield return Of(Get9_ImportConditionObject1);
            }

            if (serviceType == typeof(DependentService))
            {
                yield return Of(Get10_DependentService);
            }

            if (serviceType == typeof(WeaklyReferencedService))
            {
                yield return Of(Get11_WeaklyReferencedService);
            }

            if (serviceType == typeof(IMultiExported))
            {
                yield return Of(Get12_IMultiExported, "c");
                yield return Of(Get13_IMultiExported, "j");
                yield return Of(Get14_IMultiExported, "i");
            }

            if (serviceType == typeof(B))
            {
                yield return Of(Get15_B);
            }

            if (serviceType == typeof(IAllOpts))
            {
                yield return Of(Get17_IAllOpts, ImTools.KV.Of("a", 1));
                yield return Of(Get18_IAllOpts, "a");
            }

            if (serviceType == typeof(MultiExported))
            {
                yield return Of(Get19_MultiExported, "c");
                yield return Of(Get20_MultiExported, "b");
                yield return Of(Get21_MultiExported, "a");
            }

            if (serviceType == typeof(PreventDisposalService))
            {
                yield return Of(Get22_PreventDisposalService);
            }

            if (serviceType == typeof(ServiceWithMultipleCostructorsAndOneImporting))
            {
                yield return Of(Get23_ServiceWithMultipleCostructorsAndOneImporting);
            }

            if (serviceType == typeof(IOpGen<Bb>))
            {
                yield return Of(Get24_IOpGen);
            }

            if (serviceType == typeof(IAnotherDb))
            {
                yield return Of(Get25_IAnotherDb);
            }

            if (serviceType == typeof(ImportConditionObject3))
            {
                yield return Of(Get26_ImportConditionObject3);
            }

            if (serviceType == typeof(DbMan))
            {
                yield return Of(Get27_DbMan);
            }

            if (serviceType == typeof(ClGenA))
            {
                yield return Of(Get28_ClGenA);
            }

            if (serviceType == typeof(ISomeDb))
            {
                yield return Of(Get29_ISomeDb);
            }

        }

        // typeof(ISingletonService)
        internal static object Get0_ISingletonService(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(37, () => new SingletonService(), 0);
        }

        // typeof(IOpGen<string>)
        internal static object Get1_IOpGen(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(90, () => new ClGen(), 0);
        }

        // typeof(ClGenB)
        internal static object Get2_ClGenB(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(93, () => new ClGenB(), 0);
        }

        // typeof(SingleServiceWithMetadata)
        internal static object Get3_SingleServiceWithMetadata(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(49, () => new SingleServiceWithMetadata(), 0);
        }

        // typeof(IOpGen<Aa>)
        internal static object Get4_IOpGen(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(92, () => new ClGenA(), 0);
        }

        // typeof(ImportConditionObject2)
        internal static object Get5_ImportConditionObject2(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(63, () => new ImportConditionObject2((ExportConditionalObject2)r.SingletonScope.GetOrAdd(60, () => new ExportConditionalObject2(), 0)), 0);
        }

        // typeof(NamedScopeService)
        internal static object Get6_NamedScopeService(IResolverContext r)
        {
            return CurrentScopeReuse.GetNameScoped(r, "a", true, 84, () => new NamedScopeService(), 0);
        }

        // typeof(ITransientService)
        internal static object Get7_ITransientService(IResolverContext r)
        {
            return new TransientService();
        }

        // typeof(ClGen)
        internal static object Get8_ClGen(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(90, () => new ClGen(), 0);
        }

        // typeof(ImportConditionObject1)
        internal static object Get9_ImportConditionObject1(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(62, () => 
                new ImportConditionObject1(
                    (IExportConditionInterface)r.RootOrSelf().Resolve(typeof(IExportConditionInterface), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(ImportConditionObject1), default(System.Type), null, 62, FactoryType.Service, typeof(ImportConditionObject1), Reuse.Singleton, (RequestFlags.IsSingletonOrDependencyOfSingleton | RequestFlags.IsResolutionCall)), default(object[]))), 0);
        }

        // typeof(DependentService)
        internal static object Get10_DependentService(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(41, () => new DependentService(new TransientService(), (SingletonService)r.SingletonScope.GetOrAdd(37, () => new SingletonService(), 0), new TransientOpenGenericService<string>(), (OpenGenericServiceWithTwoParameters<bool, bool>)r.SingletonScope.GetOrAdd(228, () => new OpenGenericServiceWithTwoParameters<bool, bool>(), 0)), 0);
        }

        // typeof(WeaklyReferencedService)
        internal static object Get11_WeaklyReferencedService(IResolverContext r)
        {
            return ((System.WeakReference)r.SingletonScope.GetOrAdd(73, () => new System.WeakReference(new WeaklyReferencedService()), 0)).Target.WeakRefReuseWrapperGCed();
        }

        // typeof(IMultiExported)
        internal static object Get12_IMultiExported(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(170, () => new MultiExported(), 0);
        }

        // typeof(IMultiExported)
        internal static object Get13_IMultiExported(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(170, () => new MultiExported(), 0);
        }

        // typeof(IMultiExported)
        internal static object Get14_IMultiExported(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(170, () => new MultiExported(), 0);
        }

        // typeof(B)
        internal static object Get15_B(IResolverContext r)
        {
            return new B((A)r.Resolve(typeof(A), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(B), default(System.Type), null, 66, FactoryType.Service, typeof(B), Reuse.Transient, RequestFlags.IsResolutionCall), default(object[])));
        }

        // typeof(IOpGen<string>)
        internal static object Get16_IOpGen(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(229, () => new OpGen<string>(), 0);
        }

        // typeof(IAllOpts)
        internal static object Get17_IAllOpts(IResolverContext r)
        {
            return CurrentScopeReuse.GetScoped(r, true, 83, () => new AllOpts2(), 0);
        }

        // typeof(IAllOpts)
        internal static object Get18_IAllOpts(IResolverContext r)
        {
            return CurrentScopeReuse.GetScoped(r, true, 82, () => new AllOpts(), 0);
        }

        // typeof(MultiExported)
        internal static object Get19_MultiExported(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(170, () => new MultiExported(), 0);
        }

        // typeof(MultiExported)
        internal static object Get20_MultiExported(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(170, () => new MultiExported(), 0);
        }

        // typeof(MultiExported)
        internal static object Get21_MultiExported(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(170, () => new MultiExported(), 0);
        }

        // typeof(PreventDisposalService)
        internal static object Get22_PreventDisposalService(IResolverContext r)
        {
            return ((HiddenDisposable)r.SingletonScope.GetOrAdd(74, () => new HiddenDisposable(new PreventDisposalService()), 0)).Value;
        }

        // typeof(ServiceWithMultipleCostructorsAndOneImporting)
        internal static object Get23_ServiceWithMultipleCostructorsAndOneImporting(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(48, () => new ServiceWithMultipleCostructorsAndOneImporting(new TransientService()), 0);
        }

        // typeof(IOpGen<Bb>)
        internal static object Get24_IOpGen(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(93, () => new ClGenB(), 0);
        }

        // typeof(IAnotherDb)
        internal static object Get25_IAnotherDb(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(53, () => new DbMan(), 0);
        }

        // typeof(ImportConditionObject3)
        internal static object Get26_ImportConditionObject3(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(64, () => new ImportConditionObject3((IExportConditionInterface)r.RootOrSelf().Resolve(typeof(IExportConditionInterface), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(ImportConditionObject3), default(System.Type), null, 64, FactoryType.Service, typeof(ImportConditionObject3), Reuse.Singleton, (RequestFlags.IsSingletonOrDependencyOfSingleton | RequestFlags.IsResolutionCall)), default(object[]))), 0);
        }

        // typeof(DbMan)
        internal static object Get27_DbMan(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(53, () => new DbMan(), 0);
        }

        // typeof(ClGenA)
        internal static object Get28_ClGenA(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(92, () => new ClGenA(), 0);
        }

        // typeof(ISomeDb)
        internal static object Get29_ISomeDb(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(53, () => new DbMan(), 0);
        }

        // typeof(IExportConditionInterface)
        internal static object GetDep0_IExportConditionInterface(IResolverContext r)
        {
            return r.RootOrSelf().Resolve(
                typeof(IExportConditionInterface), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(ImportConditionObject1), default(System.Type), null, 62, FactoryType.Service, typeof(ImportConditionObject1), Reuse.Singleton, (RequestFlags.IsSingletonOrDependencyOfSingleton | RequestFlags.IsResolutionCall)), default(object[]));
        }

        // typeof(IExportConditionInterface)
        internal static object GetDep1_IExportConditionInterface(IResolverContext r)
        {
            return r.RootOrSelf().Resolve(
                typeof(IExportConditionInterface), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(ImportConditionObject3), default(System.Type), null, 64, FactoryType.Service, typeof(ImportConditionObject3), Reuse.Singleton, (RequestFlags.IsSingletonOrDependencyOfSingleton | RequestFlags.IsResolutionCall)), default(object[]));
        }

        // typeof(A)
        internal static object GetDep2_A(IResolverContext r)
        {
            return r.Resolve(
                typeof(A), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(B), default(System.Type), null, 66, FactoryType.Service, typeof(B), Reuse.Transient, RequestFlags.IsResolutionCall), default(object[]));
        }

    }
}
