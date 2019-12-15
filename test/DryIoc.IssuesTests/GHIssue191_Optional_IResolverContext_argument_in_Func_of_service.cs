using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue191_Optional_IResolverContext_argument_in_Func_of_service
    {
        [Test]
        public void Can_resolve_as_FactoryDelegate()
        {
            var container = new Container();

            container.Register<A>();
            container.Register<B>();

            var f = container.Resolve<FactoryDelegate>(typeof(B));
            var b = f(container) as B;

            Assert.IsNotNull(b);
        }

        [Test]
        public void ResolverContext_is_ignored_in_resolved_Func()
        {
            var container = new Container();

            container.Register<A>();
            container.Register<B>();

            var f = container.Resolve<Func<IResolverContext, B>>();
            var b = f(container);

            Assert.IsNotNull(b);
        }

        [Test]
        public void ResolverContext_is_ignored_in_injected_Func()
        {
            var container = new Container();

            container.Register<A>();
            container.Register<C>();

            var c = container.Resolve<C>();
            Assert.IsNotNull(c);
            Assert.IsInstanceOf<A>(c.F(container));
        }

        public class A {}

        public class B
        {
            public readonly A A;
            public B(A a)
            {
                A = a;
            }
        }

        public class C
        {
            public readonly Func<IResolverContext, A> F;
            public C(Func<IResolverContext, A> f)
            {
                F = f;
            }
        }
    }
}
