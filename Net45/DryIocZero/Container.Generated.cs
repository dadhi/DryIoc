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
using System.Linq; // for Enumerable.Cast method required for LazyEnumerable<T>
using System.Collections.Generic;
using System.Threading;

using DryIoc; // for ImTools, not for Container

namespace DryIocZero
{
/* 
Errors during resolution:
-----------------------------
SERVICE: DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructors registered as factory {ID=30, ImplType=DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructors, Reuse=SingletonReuse {Lifespan=1000}}
ERROR: Unable to find single constructor with System.ComponentModel.Composition.ImportingConstructorAttribute in DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructors.

-----------------------------
end of exception list
*/

    partial class Container
    {
		private int _lastFactoryID = 136; // generated, equals to last used Factory.FactoryID 

		/// <summary>The unique factory ID, which may be used for runtime scoped registrations.</summary>
		/// <returns>New factory ID.</returns>
		public int GetNextFactoryID() 
		{
			return Interlocked.Increment(ref _lastFactoryID);
		}

        [ExcludeFromCodeCoverage]
        partial void ResolveGenerated(ref object service, Type serviceType, IScope scope)
        {
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISingletonService))
                service = Create_0(this, scope);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2))
                service = Create_1(this, scope);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.PreventDisposalService))
                service = Create_2(this, scope);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata))
                service = Create_3(this, scope);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISomeDb))
                service = Create_4(this, scope);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting))
                service = Create_5(this, scope);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DependentService))
                service = Create_6(this, scope);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ITransientService))
                service = Create_7(this, scope);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B))
                service = Create_8(this, scope);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1))
                service = Create_9(this, scope);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.WeaklyReferencedService))
                service = Create_10(this, scope);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DbMan))
                service = Create_11(this, scope);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3))
                service = Create_12(this, scope);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NamedScopeService))
                service = Create_19(this, scope);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAnotherDb))
                service = Create_21(this, scope);
        }

        [ExcludeFromCodeCoverage]
        partial void ResolveGenerated(ref object service, Type serviceType, object serviceKey, 
            Type requiredServiceType, RequestInfo preRequestParent, IScope scope)
        {
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported)) 
            {
                if ("c".Equals(serviceKey))
                    service = Create_13(this, scope);

                else
                if ("b".Equals(serviceKey))
                    service = Create_14(this, scope);

                else
                if ("a".Equals(serviceKey))
                    service = Create_15(this, scope);
            }

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported)) 
            {
                if ("c".Equals(serviceKey))
                    service = Create_16(this, scope);

                else
                if ("j".Equals(serviceKey))
                    service = Create_17(this, scope);

                else
                if ("i".Equals(serviceKey))
                    service = Create_18(this, scope);
            }

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAllOpts)) 
            {
                if ("a".Equals(serviceKey))
                    service = Create_20(this, scope);
            }
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.A)) 
            {
                if (serviceKey == null &&
                    requiredServiceType == null &&
                    Equals(preRequestParent, RequestInfo.Empty.Push(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B), 49, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B), default(IReuse)))) 
                    service = CreateDependency_0(this, scope);
            }

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface)) 
            {
                if ((serviceKey == null && DefaultKey.Of(2) is DefaultKey || serviceKey.Equals(DefaultKey.Of(2))) &&
                    requiredServiceType == null &&
                    Equals(preRequestParent, RequestInfo.Empty.Push(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3), 47, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3), Reuse.Singleton))) 
                    service = CreateDependency_1(this, scope);

                else
                if ((serviceKey == null && DefaultKey.Of(0) is DefaultKey || serviceKey.Equals(DefaultKey.Of(0))) &&
                    requiredServiceType == null &&
                    Equals(preRequestParent, RequestInfo.Empty.Push(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1), 45, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1), Reuse.Singleton))) 
                    service = CreateDependency_2(this, scope);
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
			if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISingletonService))
			{
				yield return new KV<object, FactoryDelegate>(null, (FactoryDelegate)Create_0);
			}

			if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2))
			{
				yield return new KV<object, FactoryDelegate>(null, (FactoryDelegate)Create_1);
			}

			if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.PreventDisposalService))
			{
				yield return new KV<object, FactoryDelegate>(null, (FactoryDelegate)Create_2);
			}

			if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata))
			{
				yield return new KV<object, FactoryDelegate>(null, (FactoryDelegate)Create_3);
			}

			if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISomeDb))
			{
				yield return new KV<object, FactoryDelegate>(null, (FactoryDelegate)Create_4);
			}

			if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting))
			{
				yield return new KV<object, FactoryDelegate>(null, (FactoryDelegate)Create_5);
			}

			if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DependentService))
			{
				yield return new KV<object, FactoryDelegate>(null, (FactoryDelegate)Create_6);
			}

			if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ITransientService))
			{
				yield return new KV<object, FactoryDelegate>(null, (FactoryDelegate)Create_7);
			}

			if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B))
			{
				yield return new KV<object, FactoryDelegate>(null, (FactoryDelegate)Create_8);
			}

			if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1))
			{
				yield return new KV<object, FactoryDelegate>(null, (FactoryDelegate)Create_9);
			}

			if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.WeaklyReferencedService))
			{
				yield return new KV<object, FactoryDelegate>(null, (FactoryDelegate)Create_10);
			}

			if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DbMan))
			{
				yield return new KV<object, FactoryDelegate>(null, (FactoryDelegate)Create_11);
			}

			if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3))
			{
				yield return new KV<object, FactoryDelegate>(null, (FactoryDelegate)Create_12);
			}

			if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported))
			{
				yield return new KV<object, FactoryDelegate>("c", (FactoryDelegate)Create_13);
				yield return new KV<object, FactoryDelegate>("b", (FactoryDelegate)Create_14);
				yield return new KV<object, FactoryDelegate>("a", (FactoryDelegate)Create_15);
			}

			if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported))
			{
				yield return new KV<object, FactoryDelegate>("c", (FactoryDelegate)Create_16);
				yield return new KV<object, FactoryDelegate>("j", (FactoryDelegate)Create_17);
				yield return new KV<object, FactoryDelegate>("i", (FactoryDelegate)Create_18);
			}

			if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NamedScopeService))
			{
				yield return new KV<object, FactoryDelegate>(null, (FactoryDelegate)Create_19);
			}

			if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAllOpts))
			{
				yield return new KV<object, FactoryDelegate>("a", (FactoryDelegate)Create_20);
			}

			if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAnotherDb))
			{
				yield return new KV<object, FactoryDelegate>(null, (FactoryDelegate)Create_21);
			}

		}

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISingletonService)
        internal static object Create_0(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(1, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService());
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2)
        internal static object Create_1(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(3, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2((DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject2)r.Scopes.SingletonScope.GetOrAdd(2, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject2())));
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.PreventDisposalService)
        internal static object Create_2(IResolverContext r, IScope scope)
        {
            return ((object[])r.Scopes.SingletonScope.GetOrAdd(4, () => new object[] { new DryIoc.MefAttributedModel.UnitTests.CUT.PreventDisposalService() }))[0];
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata)
        internal static object Create_3(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(5, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingleServiceWithMetadata());
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISomeDb)
        internal static object Create_4(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(6, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan());
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting)
        internal static object Create_5(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(7, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ServiceWithMultipleCostructorsAndOneImporting(new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService()));
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DependentService)
        internal static object Create_6(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(9, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DependentService(new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService(), (DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService)r.Scopes.SingletonScope.GetOrAdd(1, () => new DryIoc.MefAttributedModel.UnitTests.CUT.SingletonService()), new DryIoc.MefAttributedModel.UnitTests.CUT.TransientOpenGenericService<string>(), (DryIoc.MefAttributedModel.UnitTests.CUT.OpenGenericServiceWithTwoParameters<bool, bool>)r.Scopes.SingletonScope.GetOrAdd(8, () => new DryIoc.MefAttributedModel.UnitTests.CUT.OpenGenericServiceWithTwoParameters<bool, bool>())));
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ITransientService)
        internal static object Create_7(IResolverContext r, IScope scope)
        {
            return new DryIoc.MefAttributedModel.UnitTests.CUT.TransientService();
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B)
        internal static object Create_8(IResolverContext r, IScope scope)
        {
            return new DryIoc.MefAttributedModel.UnitTests.CUT.B((DryIoc.MefAttributedModel.UnitTests.CUT.A)r.Resolver.Resolve(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.A), null, false, default(System.Type), RequestInfo.Empty.Push(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B), 49, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B), default(IReuse)), scope = r.Scopes.GetOrNewResolutionScope(scope, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B), null)));
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1)
        internal static object Create_9(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(11, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1((DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface)r.Resolver.Resolve(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface), null, false, default(System.Type), RequestInfo.Empty.Push(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1), 45, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1), Reuse.Singleton), scope = r.Scopes.GetOrNewResolutionScope(scope, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1), null))));
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.WeaklyReferencedService)
        internal static object Create_10(IResolverContext r, IScope scope)
        {
            return ((System.WeakReference)r.Scopes.SingletonScope.GetOrAdd(12, () => new System.WeakReference(new DryIoc.MefAttributedModel.UnitTests.CUT.WeaklyReferencedService()))).Target.ThrowNewErrorIfNull("Reused service wrapped in WeakReference is Garbage Collected and no longer available.");
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DbMan)
        internal static object Create_11(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(6, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan());
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3)
        internal static object Create_12(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(14, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3((DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface)r.Resolver.Resolve(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface), null, false, default(System.Type), RequestInfo.Empty.Push(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3), 47, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3), Reuse.Singleton), scope = r.Scopes.GetOrNewResolutionScope(scope, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3), null))));
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported)
        internal static object Create_13(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(15, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported());
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported)
        internal static object Create_14(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(15, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported());
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported)
        internal static object Create_15(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(15, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported());
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported)
        internal static object Create_16(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(15, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported());
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported)
        internal static object Create_17(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(15, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported());
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported)
        internal static object Create_18(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(15, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported());
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.NamedScopeService)
        internal static object Create_19(IResolverContext r, IScope scope)
        {
            return r.Scopes.GetCurrentNamedScope("a", true).GetOrAdd(67, () => new DryIoc.MefAttributedModel.UnitTests.CUT.NamedScopeService());
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAllOpts)
        internal static object Create_20(IResolverContext r, IScope scope)
        {
            return r.Scopes.GetCurrentNamedScope(null, true).GetOrAdd(65, () => new DryIoc.MefAttributedModel.UnitTests.CUT.AllOpts());
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAnotherDb)
        internal static object Create_21(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(6, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan());
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.A)
        internal static object CreateDependency_0(IResolverContext r, IScope scope)
        {
            return new DryIoc.MefAttributedModel.UnitTests.CUT.A();
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface)
        internal static object CreateDependency_1(IResolverContext r, IScope scope)
        {
            return (DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject3)r.Scopes.SingletonScope.GetOrAdd(13, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject3());
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface)
        internal static object CreateDependency_2(IResolverContext r, IScope scope)
        {
            return (DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject1)r.Scopes.SingletonScope.GetOrAdd(10, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject1());
        }

	}
}