

using System;
using System.Linq; // for Enumerable.Cast method required for LazyEnumerable<T>
using System.Collections.Generic;

namespace DryIoc.Zero
{
    partial class ZeroContainer
    {
/* 
Exceptions happened during resolution:
======================================

======================================
end of exception list
*/
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public object ResolveGenerated(Type serviceType, IScope scope)
        {
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.LazyDep))
                return Create_4(this, scope);
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAnotherDb))
                return Create_5(this, scope);
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISomeDb))
                return Create_6(this, scope);
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DbMan))
                return Create_7(this, scope);
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B))
                return Create_11(this, scope);
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.A))
                return Create_12(this, scope);
            return null;
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public object ResolveGenerated(Type serviceType, object serviceKey, IScope scope)
        {
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported)) 
            {
                if ("c".Equals(serviceKey))
                    return Create_1(this, scope);
                if ("j".Equals(serviceKey))
                    return Create_2(this, scope);
                if ("i".Equals(serviceKey))
                    return Create_3(this, scope);
            }
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported)) 
            {
                if ("c".Equals(serviceKey))
                    return Create_8(this, scope);
                if ("b".Equals(serviceKey))
                    return Create_9(this, scope);
                if ("a".Equals(serviceKey))
                    return Create_10(this, scope);
            }
            return null;
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public IEnumerable<KV> ResolveManyGenerated(Type serviceType)
        {
            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IMultiExported))
            {
                yield return new KV("c", (StatelessFactoryDelegate)Create_1);
                yield return new KV("j", (StatelessFactoryDelegate)Create_2);
                yield return new KV("i", (StatelessFactoryDelegate)Create_3);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.LazyDep))
            {
                yield return new KV(null, (StatelessFactoryDelegate)Create_4);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IAnotherDb))
            {
                yield return new KV(null, (StatelessFactoryDelegate)Create_5);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ISomeDb))
            {
                yield return new KV(null, (StatelessFactoryDelegate)Create_6);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.DbMan))
            {
                yield return new KV(null, (StatelessFactoryDelegate)Create_7);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported))
            {
                yield return new KV("c", (StatelessFactoryDelegate)Create_8);
                yield return new KV("b", (StatelessFactoryDelegate)Create_9);
                yield return new KV("a", (StatelessFactoryDelegate)Create_10);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.B))
            {
                yield return new KV(null, (StatelessFactoryDelegate)Create_11);
            }

            if (serviceType == typeof(DryIoc.MefAttributedModel.UnitTests.CUT.A))
            {
                yield return new KV(null, (StatelessFactoryDelegate)Create_12);
            }

        }

        internal static object Create_1(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(0, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported());
        }

        internal static object Create_2(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(0, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported());
        }

        internal static object Create_3(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(0, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported());
        }

        internal static object Create_4(IResolverContext r, IScope scope)
        {
            return new DryIoc.MefAttributedModel.UnitTests.CUT.LazyDep();
        }

        internal static object Create_5(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(1, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan());
        }

        internal static object Create_6(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(1, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan());
        }

        internal static object Create_7(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(1, () => new DryIoc.MefAttributedModel.UnitTests.CUT.DbMan());
        }

        internal static object Create_8(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(0, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported());
        }

        internal static object Create_9(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(0, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported());
        }

        internal static object Create_10(IResolverContext r, IScope scope)
        {
            return r.Scopes.SingletonScope.GetOrAdd(0, () => new DryIoc.MefAttributedModel.UnitTests.CUT.MultiExported());
        }

        internal static object Create_11(IResolverContext r, IScope scope)
        {
            return new DryIoc.MefAttributedModel.UnitTests.CUT.B((DryIoc.MefAttributedModel.UnitTests.CUT.A)r.Resolver.ResolveKeyed(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.A), (object)null, DryIoc.IfUnresolved.Throw, default(System.Type), scope));
        }

        internal static object Create_12(IResolverContext r, IScope scope)
        {
            return new DryIoc.MefAttributedModel.UnitTests.CUT.A();
        }

    }
}