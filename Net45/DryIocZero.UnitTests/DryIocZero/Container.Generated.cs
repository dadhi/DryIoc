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
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting))
                service = Get6_ServiceWithMultipleCostructorsAndOneImporting(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ClGenB))
                service = Get7_ClGenB(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ClGen))
                service = Get8_ClGen(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NamedScopeService))
                service = Get9_NamedScopeService(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IOpGen<DryIoc.MefAttributedModel.UnitTests.CUT.Aa>))
                service = Get10_IOpGen(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.PreventDisposalService))
                service = Get11_PreventDisposalService(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAnotherDb))
                service = Get12_IAnotherDb(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.WeaklyReferencedService))
                service = Get13_WeaklyReferencedService(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ITransientService))
                service = Get14_ITransientService(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata))
                service = Get15_SingleServiceWithMetadata(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISingletonService))
                service = Get16_ISingletonService(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IOpGen<string>))
                service = Get17_IOpGen(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IOpGen<DryIoc.MefAttributedModel.UnitTests.CUT.Bb>))
                service = Get18_IOpGen(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1))
                service = Get19_ImportConditionObject1(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DependentService))
                service = Get20_DependentService(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DbMan))
                service = Get21_DbMan(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISomeDb))
                service = Get22_ISomeDb(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3))
                service = Get23_ImportConditionObject3(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IOpGen<string>))
                service = Get24_IOpGen(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B))
                service = Get27_B(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2))
                service = Get28_ImportConditionObject2(this);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ClGenA))
                service = Get29_ClGenA(this);
        }

        [ExcludeFromCodeCoverage]
        partial void ResolveGenerated(ref object service,
            Type serviceType, object serviceKey, Type requiredServiceType, Request preRequestParent, object[] args)
        {
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported)) 
            {
                if ("c".Equals(serviceKey))
                    service = Get0_MultiExported(this);

                else
                if ("b".Equals(serviceKey))
                    service = Get1_MultiExported(this);

                else
                if ("a".Equals(serviceKey))
                    service = Get2_MultiExported(this);
            }

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported)) 
            {
                if ("c".Equals(serviceKey))
                    service = Get3_IMultiExported(this);

                else
                if ("j".Equals(serviceKey))
                    service = Get4_IMultiExported(this);

                else
                if ("i".Equals(serviceKey))
                    service = Get5_IMultiExported(this);
            }

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAllOpts)) 
            {
                if ("a".Equals(serviceKey))
                    service = Get25_IAllOpts(this);

                else
                if (ImTools.KV.Of("a", 1).Equals(serviceKey))
                    service = Get26_IAllOpts(this);
            }

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface)) 
            {
                if ((serviceKey == null || DefaultKey.Of(0).Equals(serviceKey)) &&
                    requiredServiceType == null &&
                    Equals(preRequestParent, Request.Empty.Push(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1), default(System.Type), (object)null, 62, FactoryType.Service, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1), Reuse.Singleton, (RequestFlags.IsSingletonOrDependencyOfSingleton | RequestFlags.IsResolutionCall)))) 
                    service = GetDep0_IExportConditionInterface(this);

                else
                if ((serviceKey == null || DefaultKey.Of(2).Equals(serviceKey)) &&
                    requiredServiceType == null &&
                    Equals(preRequestParent, Request.Empty.Push(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3), default(System.Type), (object)null, 64, FactoryType.Service, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3), Reuse.Singleton, (RequestFlags.IsSingletonOrDependencyOfSingleton | RequestFlags.IsResolutionCall)))) 
                    service = GetDep1_IExportConditionInterface(this);

                else
                if ((serviceKey == null || DefaultKey.Of(1).Equals(serviceKey)) &&
                    requiredServiceType == null &&
                    Equals(preRequestParent, Request.Empty.Push(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2), default(System.Type), (object)null, 63, FactoryType.Service, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2), Reuse.Singleton, (RequestFlags.IsSingletonOrDependencyOfSingleton | RequestFlags.IsResolutionCall)))) 
                    service = GetDep3_IExportConditionInterface(this);
            }

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.A)) 
            {
                if (serviceKey == null &&
                    requiredServiceType == null &&
                    Equals(preRequestParent, Request.Empty.Push(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B), default(System.Type), (object)null, 66, FactoryType.Service, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B), Reuse.Transient, RequestFlags.IsResolutionCall))) 
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
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported))
            {
                yield return Of(Get0_MultiExported, "c");
                yield return Of(Get1_MultiExported, "b");
                yield return Of(Get2_MultiExported, "a");
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported))
            {
                yield return Of(Get3_IMultiExported, "c");
                yield return Of(Get4_IMultiExported, "j");
                yield return Of(Get5_IMultiExported, "i");
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting))
            {
                yield return Of(Get6_ServiceWithMultipleCostructorsAndOneImporting);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ClGenB))
            {
                yield return Of(Get7_ClGenB);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ClGen))
            {
                yield return Of(Get8_ClGen);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NamedScopeService))
            {
                yield return Of(Get9_NamedScopeService);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IOpGen<DryIoc.MefAttributedModel.UnitTests.CUT.Aa>))
            {
                yield return Of(Get10_IOpGen);
                yield return Of(Get18_IOpGen); // co-variant
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.PreventDisposalService))
            {
                yield return Of(Get11_PreventDisposalService);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAnotherDb))
            {
                yield return Of(Get12_IAnotherDb);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.WeaklyReferencedService))
            {
                yield return Of(Get13_WeaklyReferencedService);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ITransientService))
            {
                yield return Of(Get14_ITransientService);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata))
            {
                yield return Of(Get15_SingleServiceWithMetadata);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISingletonService))
            {
                yield return Of(Get16_ISingletonService);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IOpGen<string>))
            {
                yield return Of(Get17_IOpGen, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IOpGen<>));
                yield return Of(Get24_IOpGen);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IOpGen<DryIoc.MefAttributedModel.UnitTests.CUT.Bb>))
            {
                yield return Of(Get18_IOpGen);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1))
            {
                yield return Of(Get19_ImportConditionObject1);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DependentService))
            {
                yield return Of(Get20_DependentService);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DbMan))
            {
                yield return Of(Get21_DbMan);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISomeDb))
            {
                yield return Of(Get22_ISomeDb);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3))
            {
                yield return Of(Get23_ImportConditionObject3);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAllOpts))
            {
                yield return Of(Get25_IAllOpts, "a");
                yield return Of(Get26_IAllOpts, ImTools.KV.Of("a", 1));
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B))
            {
                yield return Of(Get27_B);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2))
            {
                yield return Of(Get28_ImportConditionObject2);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ClGenA))
            {
                yield return Of(Get29_ClGenA);
            }

        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported)
        internal static object Get0_MultiExported(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(170, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported)
        internal static object Get1_MultiExported(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(170, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported)
        internal static object Get2_MultiExported(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(170, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported)
        internal static object Get3_IMultiExported(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(170, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported)
        internal static object Get4_IMultiExported(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(170, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported)
        internal static object Get5_IMultiExported(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(170, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting)
        internal static object Get6_ServiceWithMultipleCostructorsAndOneImporting(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(48, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting(new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService()), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ClGenB)
        internal static object Get7_ClGenB(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(93, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ClGenB(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ClGen)
        internal static object Get8_ClGen(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(90, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ClGen(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NamedScopeService)
        internal static object Get9_NamedScopeService(IResolverContext r)
        {
            return CurrentScopeReuse.GetNameScoped(r, "a", true, 84, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedScopeService(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IOpGen<DryIoc.MefAttributedModel.UnitTests.CUT.Aa>)
        internal static object Get10_IOpGen(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(92, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ClGenA(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.PreventDisposalService)
        internal static object Get11_PreventDisposalService(IResolverContext r)
        {
            return ((HiddenDisposable)r.SingletonScope.GetOrAdd(74, () => new HiddenDisposable(new DryIoc.MefAttributedModel.UnitTests.CUT.PreventDisposalService()), 0)).Value;
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAnotherDb)
        internal static object Get12_IAnotherDb(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(53, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.WeaklyReferencedService)
        internal static object Get13_WeaklyReferencedService(IResolverContext r)
        {
            return ((System.WeakReference)r.SingletonScope.GetOrAdd(73, () => new System.WeakReference(new DryIoc.MefAttributedModel.UnitTests.CUT.WeaklyReferencedService()), 0)).Target.ThrowNewErrorIfNull("Reused service wrapped in WeakReference is Garbage Collected and no longer available.");
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ITransientService)
        internal static object Get14_ITransientService(IResolverContext r)
        {
            return new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService();
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata)
        internal static object Get15_SingleServiceWithMetadata(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(49, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISingletonService)
        internal static object Get16_ISingletonService(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(37, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IOpGen<string>)
        internal static object Get17_IOpGen(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(227, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OpGen<string>(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IOpGen<DryIoc.MefAttributedModel.UnitTests.CUT.Bb>)
        internal static object Get18_IOpGen(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(93, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ClGenB(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1)
        internal static object Get19_ImportConditionObject1(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(62, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1((DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface)r.RootOrSelf().Resolve(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1), default(System.Type), (object)null, 62, FactoryType.Service, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1), Reuse.Singleton, (RequestFlags.IsSingletonOrDependencyOfSingleton | RequestFlags.IsResolutionCall)), default(object[]))), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DependentService)
        internal static object Get20_DependentService(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(41, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DependentService(new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService(), (DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService)r.SingletonScope.GetOrAdd(37, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService(), 0), new DryIoc.MefAttributedModel.UnitTests.CUT.TransientOpenGenericService<string>(), (DryIoc.MefAttributedModel.UnitTests.CUT.OpenGenericServiceWithTwoParameters<bool, bool>)r.SingletonScope.GetOrAdd(229, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OpenGenericServiceWithTwoParameters<bool, bool>(), 0)), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DbMan)
        internal static object Get21_DbMan(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(53, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISomeDb)
        internal static object Get22_ISomeDb(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(53, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3)
        internal static object Get23_ImportConditionObject3(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(64, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3((DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface)r.RootOrSelf().Resolve(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3), default(System.Type), (object)null, 64, FactoryType.Service, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3), Reuse.Singleton, (RequestFlags.IsSingletonOrDependencyOfSingleton | RequestFlags.IsResolutionCall)), default(object[]))), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IOpGen<string>)
        internal static object Get24_IOpGen(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(90, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ClGen(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAllOpts)
        internal static object Get25_IAllOpts(IResolverContext r)
        {
            return CurrentScopeReuse.GetScoped(r, true, 82, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AllOpts(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAllOpts)
        internal static object Get26_IAllOpts(IResolverContext r)
        {
            return CurrentScopeReuse.GetScoped(r, true, 83, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AllOpts2(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B)
        internal static object Get27_B(IResolverContext r)
        {
            return new DryIoc.MefAttributedModel.UnitTests.CUT.B((DryIoc.MefAttributedModel.UnitTests.CUT.A)r.Resolve(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.A), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B), default(System.Type), (object)null, 66, FactoryType.Service, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B), Reuse.Transient, RequestFlags.IsResolutionCall), default(object[])));
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2)
        internal static object Get28_ImportConditionObject2(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(63, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2((DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface)r.RootOrSelf().Resolve(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface), null, IfUnresolved.Throw, default(System.Type), Request.Empty.Push(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2), default(System.Type), (object)null, 63, FactoryType.Service, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2), Reuse.Singleton, (RequestFlags.IsSingletonOrDependencyOfSingleton | RequestFlags.IsResolutionCall)), default(object[]))), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ClGenA)
        internal static object Get29_ClGenA(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(92, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ClGenA(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface)
        internal static object GetDep0_IExportConditionInterface(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(59, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject1(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface)
        internal static object GetDep1_IExportConditionInterface(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(61, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject3(), 0);
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.A)
        internal static object GetDep2_A(IResolverContext r)
        {
            return new DryIoc.MefAttributedModel.UnitTests.CUT.A();
        }

        // typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface)
        internal static object GetDep3_IExportConditionInterface(IResolverContext r)
        {
            return r.SingletonScope.GetOrAdd(60, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject2(), 0);
        }

    }
}
