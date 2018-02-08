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

//========================================================================================================
// NOTE: The code below is generated automatically at compile-time and not supposed to be changed by hand.
//========================================================================================================
using System;
using System.Linq; // for Enumerable.Cast method required by LazyEnumerable<T>
using System.Collections.Generic;
using System.Threading;
using ImTools;

namespace DryIocZero
{
/* 
Generation is FAILED with 1 errors:
Note: You may try to register failing service with `container.RegisterPlaceholder<...>()` in Registrations.tt and then register ACTUAL service at runtime.
-------------------------------------------
1. DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructors
Error: Unable to find single constructor nor marked with System.ComponentModel.Composition.ImportingConstructorAttribute nor default constructor in DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructors when resolving: singleton DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructors #47
  from container

*/
    partial class Container
    {
        [ExcludeFromCodeCoverage]
        partial void GetLastGeneratedFactoryID(ref int lastFactoryID)
        {
            lastFactoryID = 225; // generated: equals to last used Factory.FactoryID 
        }

        [ExcludeFromCodeCoverage]
        partial void ResolveGenerated(ref object service, Type serviceType)
        {
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting))
                service = Get0_ServiceWithMultipleCostructorsAndOneImporting(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DbMan))
                service = Get1_DbMan(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B))
                service = Get2_B(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.PreventDisposalService))
                service = Get3_PreventDisposalService(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ITransientService))
                service = Get4_ITransientService(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata))
                service = Get5_SingleServiceWithMetadata(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3))
                service = Get6_ImportConditionObject3(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NamedScopeService))
                service = Get7_NamedScopeService(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DependentService))
                service = Get8_DependentService(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAnotherDb))
                service = Get9_IAnotherDb(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISomeDb))
                service = Get12_ISomeDb(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.WeaklyReferencedService))
                service = Get13_WeaklyReferencedService(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISingletonService))
                service = Get14_ISingletonService(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2))
                service = Get18_ImportConditionObject2(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1))
                service = Get22_ImportConditionObject1(this);
        }

        [ExcludeFromCodeCoverage]
        partial void ResolveGenerated(ref object service,
            Type serviceType, object serviceKey, Type requiredServiceType, Request preRequestParent, object[] args)
        {
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAllOpts)) 
            {
                if ("a".Equals(serviceKey))
                    service = Get10_IAllOpts(this);

                else
                if (ImTools.KV.Of<object, int>("a", 1).Equals(serviceKey))
                    service = Get11_IAllOpts(this);
            }

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported)) 
            {
                if ("c".Equals(serviceKey))
                    service = Get15_MultiExported(this);

                else
                if ("b".Equals(serviceKey))
                    service = Get16_MultiExported(this);

                else
                if ("a".Equals(serviceKey))
                    service = Get17_MultiExported(this);
            }

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported)) 
            {
                if ("c".Equals(serviceKey))
                    service = Get19_IMultiExported(this);

                else
                if ("j".Equals(serviceKey))
                    service = Get20_IMultiExported(this);

                else
                if ("i".Equals(serviceKey))
                    service = Get21_IMultiExported(this);
            }

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.A)) 
            {
                if (serviceKey == null &&
                    requiredServiceType == null &&
                    Equals(preRequestParent, Request.Empty.Push(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B), default(System.Type), (object)null, 66, FactoryType.Service, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B), Reuse.Transient, RequestFlags.IsResolutionCall))) 
                    service = GetDep0_A(this);
            }

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface)) 
            {
                if ((serviceKey == null || DefaultKey.Of(2).Equals(serviceKey)) &&
                    requiredServiceType == null &&
                    Equals(preRequestParent, Request.Empty.Push(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3), default(System.Type), (object)null, 64, FactoryType.Service, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3), Reuse.Singleton, (RequestFlags.IsSingletonOrDependencyOfSingleton | RequestFlags.IsResolutionCall)))) 
                    service = GetDep1_IExportConditionInterface(this);

                else
                if ((serviceKey == null || DefaultKey.Of(1).Equals(serviceKey)) &&
                    requiredServiceType == null &&
                    Equals(preRequestParent, Request.Empty.Push(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2), default(System.Type), (object)null, 63, FactoryType.Service, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2), Reuse.Singleton, (RequestFlags.IsSingletonOrDependencyOfSingleton | RequestFlags.IsResolutionCall)))) 
                    service = GetDep2_IExportConditionInterface(this);

                else
                if ((serviceKey == null || DefaultKey.Of(0).Equals(serviceKey)) &&
                    requiredServiceType == null &&
                    Equals(preRequestParent, Request.Empty.Push(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1), default(System.Type), (object)null, 62, FactoryType.Service, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1), Reuse.Singleton, (RequestFlags.IsSingletonOrDependencyOfSingleton | RequestFlags.IsResolutionCall)))) 
                    service = GetDep3_IExportConditionInterface(this);
            }
        }

        [ExcludeFromCodeCoverage]
        partial void ResolveManyGenerated(ref IEnumerable<KV<object, FactoryDelegate>> services, Type serviceType) 
        {
            services = ResolveManyGenerated(serviceType);
        }

        [ExcludeFromCodeCoverage]
        private IEnumerable<KV<object, FactoryDelegate>> ResolveManyGenerated(Type serviceType)
        {
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting))
            {
                yield return kv(Get0_ServiceWithMultipleCostructorsAndOneImporting);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DbMan))
            {
                yield return kv(Get1_DbMan);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B))
            {
                yield return kv(Get2_B);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.PreventDisposalService))
            {
                yield return kv(Get3_PreventDisposalService);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ITransientService))
            {
                yield return kv(Get4_ITransientService);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata))
            {
                yield return kv(Get5_SingleServiceWithMetadata);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3))
            {
                yield return kv(Get6_ImportConditionObject3);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NamedScopeService))
            {
                yield return kv(Get7_NamedScopeService);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DependentService))
            {
                yield return kv(Get8_DependentService);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAnotherDb))
            {
                yield return kv(Get9_IAnotherDb);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAllOpts))
            {
                yield return kv(Get10_IAllOpts, "a");
                yield return kv(Get11_IAllOpts, ImTools.KV.Of<object, int>("a", 1));
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISomeDb))
            {
                yield return kv(Get12_ISomeDb);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.WeaklyReferencedService))
            {
                yield return kv(Get13_WeaklyReferencedService);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISingletonService))
            {
                yield return kv(Get14_ISingletonService);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported))
            {
                yield return kv(Get15_MultiExported, "c");
                yield return kv(Get16_MultiExported, "b");
                yield return kv(Get17_MultiExported, "a");
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2))
            {
                yield return kv(Get18_ImportConditionObject2);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported))
            {
                yield return kv(Get19_IMultiExported, "c");
                yield return kv(Get20_IMultiExported, "j");
                yield return kv(Get21_IMultiExported, "i");
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1))
            {
                yield return kv(Get22_ImportConditionObject1);
            }

        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting)
        internal static object Get0_ServiceWithMultipleCostructorsAndOneImporting(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(48, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting(new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService()), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DbMan)
        internal static object Get1_DbMan(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(53, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B)
        internal static object Get2_B(IResolverContext r)
        {
            return new DryIoc.MefAttributedModel.UnitTests.CUT.B((DryIoc.MefAttributedModel.UnitTests.CUT.A)r.Resolve(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.A), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B), default(System.Type), (object)null, 66, FactoryType.Service, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B), Reuse.Transient, RequestFlags.IsResolutionCall), default(object[])));
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.PreventDisposalService)
        internal static object Get3_PreventDisposalService(IResolverContext r)
        {
            return ((HiddenDisposable)r.SingletonScope.GetOrAdd(74, () => new HiddenDisposable(new DryIoc.MefAttributedModel.UnitTests.CUT.PreventDisposalService()), 0)).Value;
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ITransientService)
        internal static object Get4_ITransientService(IResolverContext r)
        {
            return new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService();
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata)
        internal static object Get5_SingleServiceWithMetadata(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(49, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3)
        internal static object Get6_ImportConditionObject3(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(64, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3((DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface)r.RootOrSelf().Resolve(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3), default(System.Type), (object)null, 64, FactoryType.Service, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3), Reuse.Singleton, (RequestFlags.IsSingletonOrDependencyOfSingleton | RequestFlags.IsResolutionCall)), default(object[]))), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NamedScopeService)
        internal static object Get7_NamedScopeService(IResolverContext r)
        {
            return CurrentScopeReuse.GetNameScoped(r, "a", true, 84, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedScopeService(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DependentService)
        internal static object Get8_DependentService(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(41, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DependentService(new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService(), (DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService)r.SingletonScope.GetOrAdd(37, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService(), 0), new DryIoc.MefAttributedModel.UnitTests.CUT.TransientOpenGenericService<string>(), (DryIoc.MefAttributedModel.UnitTests.CUT.OpenGenericServiceWithTwoParameters<bool, bool>)r.SingletonScope.GetOrAdd(224, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OpenGenericServiceWithTwoParameters<bool, bool>(), 0)), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAnotherDb)
        internal static object Get9_IAnotherDb(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(53, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAllOpts)
        internal static object Get10_IAllOpts(IResolverContext r)
        {
            return CurrentScopeReuse.GetScoped(r, true, 82, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AllOpts(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAllOpts)
        internal static object Get11_IAllOpts(IResolverContext r)
        {
            return CurrentScopeReuse.GetScoped(r, true, 83, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AllOpts2(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISomeDb)
        internal static object Get12_ISomeDb(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(53, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.WeaklyReferencedService)
        internal static object Get13_WeaklyReferencedService(IResolverContext r)
        {
            return ((System.WeakReference)r.SingletonScope.GetOrAdd(73, () => new System.WeakReference(new DryIoc.MefAttributedModel.UnitTests.CUT.WeaklyReferencedService()), 0)).Target.ThrowNewErrorIfNull("Reused service wrapped in WeakReference is Garbage Collected and no longer available.");
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISingletonService)
        internal static object Get14_ISingletonService(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(37, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported)
        internal static object Get15_MultiExported(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(166, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported)
        internal static object Get16_MultiExported(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(166, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported)
        internal static object Get17_MultiExported(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(166, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2)
        internal static object Get18_ImportConditionObject2(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(63, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2((DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface)r.RootOrSelf().Resolve(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2), default(System.Type), (object)null, 63, FactoryType.Service, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2), Reuse.Singleton, (RequestFlags.IsSingletonOrDependencyOfSingleton | RequestFlags.IsResolutionCall)), default(object[]))), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported)
        internal static object Get19_IMultiExported(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(166, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported)
        internal static object Get20_IMultiExported(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(166, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported)
        internal static object Get21_IMultiExported(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(166, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1)
        internal static object Get22_ImportConditionObject1(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(62, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1((DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface)r.RootOrSelf().Resolve(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1), default(System.Type), (object)null, 62, FactoryType.Service, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1), Reuse.Singleton, (RequestFlags.IsSingletonOrDependencyOfSingleton | RequestFlags.IsResolutionCall)), default(object[]))), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.A)
        internal static object GetDep0_A(IResolverContext r)
        {
            return new DryIoc.MefAttributedModel.UnitTests.CUT.A();
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface)
        internal static object GetDep1_IExportConditionInterface(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(61, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject3(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface)
        internal static object GetDep2_IExportConditionInterface(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(60, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject2(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface)
        internal static object GetDep3_IExportConditionInterface(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(59, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject1(), 0);
        }


        private static KV<object, FactoryDelegate> kv(FactoryDelegate f, object key = null) => new KV<object, FactoryDelegate>(key, f);
    }
}
