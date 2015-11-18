/*
The MIT License (MIT)

Copyright (c) 2013 Maksim Volkau

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



using System;
using System.Linq; // for Enumerable.Cast method required for LazyEnumerable<T>
using System.Collections.Generic;

namespace DryIocZero
{
/* 
Errors during resolution:
-----------------------------
Generation went without errors.
-----------------------------
end of exception list
*/

    partial class Container
    {
        [ExcludeFromCodeCoverage]
        partial void ResolveGenerated(ref object service, Type serviceType, IScope scope)
        {
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISomeDb))
                service = Create_0(this, scope);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAnotherDb))
                service = Create_1(this, scope);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.WeaklyReferencedService))
                service = Create_2(this, scope);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B))
                service = Create_3(this, scope);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.PreventDisposalService))
                service = Create_7(this, scope);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DbMan))
                service = Create_8(this, scope);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3))
                service = Create_9(this, scope);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2))
                service = Create_13(this, scope);

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1))
                service = Create_14(this, scope);
        }

        [ExcludeFromCodeCoverage]
        partial void ResolveGenerated(ref object service, Type serviceType, object serviceKey, 
            Type requiredServiceType, RequestInfo preRequestParent, IScope scope)
        {
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported)) 
            {
                if ("c".Equals(serviceKey))
                    service = Create_4(this, scope);

                else
                if ("b".Equals(serviceKey))
                    service = Create_5(this, scope);

                else
                if ("a".Equals(serviceKey))
                    service = Create_6(this, scope);
            }

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported)) 
            {
                if ("c".Equals(serviceKey))
                    service = Create_10(this, scope);

                else
                if ("j".Equals(serviceKey))
                    service = Create_11(this, scope);

                else
                if ("i".Equals(serviceKey))
                    service = Create_12(this, scope);
            }
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface)) 
            {
                if ((serviceKey == null && DefaultKey.Of(0) is DefaultKey || serviceKey.Equals(DefaultKey.Of(0))) &&
                    requiredServiceType == null &&
                    Equals(preRequestParent, RequestInfo.Empty.Push(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1), 42, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1), 1000))) 
                    service = CreateDependency_0(this, scope);

                else
                if ((serviceKey == null && DefaultKey.Of(2) is DefaultKey || serviceKey.Equals(DefaultKey.Of(2))) &&
                    requiredServiceType == null &&
                    Equals(preRequestParent, RequestInfo.Empty.Push(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3), 44, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3), 1000))) 
                    service = CreateDependency_2(this, scope);
            }

            else
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.A)) 
            {
                if (serviceKey == null &&
                    requiredServiceType == null &&
                    Equals(preRequestParent, RequestInfo.Empty.Push(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B), 46, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B), 0))) 
                    service = CreateDependency_1(this, scope);
            }
        }

        [ExcludeFromCodeCoverage]
        partial void ResolveManyGenerated(ref IEnumerable<KV> services, Type serviceType) 
        {
            services = ResolveManyGenerated(serviceType);
        }

        [ExcludeFromCodeCoverage]
        private IEnumerable<KV> ResolveManyGenerated(Type serviceType)
        {
			if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISomeDb))
			{
				yield return new KV(null, (FactoryDelegate)Create_0);
			}

			if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAnotherDb))
			{
				yield return new KV(null, (FactoryDelegate)Create_1);
			}

			if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.WeaklyReferencedService))
			{
				yield return new KV(null, (FactoryDelegate)Create_2);
			}

			if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B))
			{
				yield return new KV(null, (FactoryDelegate)Create_3);
			}

			if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported))
			{
				yield return new KV("c", (FactoryDelegate)Create_4);
				yield return new KV("b", (FactoryDelegate)Create_5);
				yield return new KV("a", (FactoryDelegate)Create_6);
			}

			if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.PreventDisposalService))
			{
				yield return new KV(null, (FactoryDelegate)Create_7);
			}

			if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DbMan))
			{
				yield return new KV(null, (FactoryDelegate)Create_8);
			}

			if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3))
			{
				yield return new KV(null, (FactoryDelegate)Create_9);
			}

			if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported))
			{
				yield return new KV("c", (FactoryDelegate)Create_10);
				yield return new KV("j", (FactoryDelegate)Create_11);
				yield return new KV("i", (FactoryDelegate)Create_12);
			}

			if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2))
			{
				yield return new KV(null, (FactoryDelegate)Create_13);
			}

			if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1))
			{
				yield return new KV(null, (FactoryDelegate)Create_14);
			}

		}

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISomeDb)
        internal static object Create_0(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(0, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan());
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAnotherDb)
        internal static object Create_1(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(0, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan());
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.WeaklyReferencedService)
        internal static object Create_2(IResolverContext r, IScope scope)
        {
            return ((System.WeakReference)r.Scopes.SingletonScope.GetOrAdd(1, () => new System.WeakReference(new DryIoc.MefAttributedModel.UnitTests.CUT.WeaklyReferencedService()))).Target.ThrowNewErrorIfNull("Reused service wrapped in WeakReference is Garbage Collected and no longer available.");
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B)
        internal static object Create_3(IResolverContext r, IScope scope)
        {
            return new DryIoc.MefAttributedModel.UnitTests.CUT.B((DryIoc.MefAttributedModel.UnitTests.CUT.A)r.Resolver.Resolve(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.A), (object)null, false, default(System.Type), RequestInfo.Empty.Push(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B), 46, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B), 0), scope));
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported)
        internal static object Create_4(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(2, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported());
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported)
        internal static object Create_5(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(2, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported());
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported)
        internal static object Create_6(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(2, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported());
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.PreventDisposalService)
        internal static object Create_7(IResolverContext r, IScope scope)
        {
            return ((object[])r.Scopes.SingletonScope.GetOrAdd(3, () => new object[] { new DryIoc.MefAttributedModel.UnitTests.CUT.PreventDisposalService() }))[0];
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DbMan)
        internal static object Create_8(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(0, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan());
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3)
        internal static object Create_9(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(5, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3((DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface)r.Resolver.Resolve(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface), (object)null, false, default(System.Type), RequestInfo.Empty.Push(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3), 44, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject3), 1000), scope)));
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported)
        internal static object Create_10(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(2, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported());
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported)
        internal static object Create_11(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(2, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported());
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported)
        internal static object Create_12(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(2, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported());
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2)
        internal static object Create_13(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(7, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject2((DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject2)r.Scopes.SingletonScope.GetOrAdd(6, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject2())));
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1)
        internal static object Create_14(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(9, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1((DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface)r.Resolver.Resolve(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface), (object)null, false, default(System.Type), RequestInfo.Empty.Push(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1), 42, typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ImportConditionObject1), 1000), scope)));
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface)
        internal static object CreateDependency_0(IResolverContext r, IScope scope)
        {
            return (DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject1)r.Scopes.SingletonScope.GetOrAdd(8, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject1());
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.A)
        internal static object CreateDependency_1(IResolverContext r, IScope scope)
        {
            return new DryIoc.MefAttributedModel.UnitTests.CUT.A();
        }

		// typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IExportConditionInterface)
        internal static object CreateDependency_2(IResolverContext r, IScope scope)
        {
            return (DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject3)r.Scopes.SingletonScope.GetOrAdd(4, () => new DryIoc.MefAttributedModel.UnitTests.CUT.ExportConditionalObject3());
        }

	}
}