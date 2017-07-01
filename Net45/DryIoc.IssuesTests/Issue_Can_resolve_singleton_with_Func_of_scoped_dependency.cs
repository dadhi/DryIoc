using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue_Can_resolve_singleton_with_Func_of_scoped_dependency
    {
        [Test]
        public void Can_resolve_singleton_with_Func_of_scoped_dependency()
        {
            var c = new Container(scopeContext: new AsyncExecutionFlowScopeContext());

            c.Register<A>(Reuse.Singleton);
            c.Register<B>(Reuse.InCurrentScope);

            var a = c.Resolve<A>();

            using (c.OpenScope())
                Assert.IsNotNull(a.GetB());
        }

        public class A
        {
            public readonly Func<B> GetB;

            public A(Func<B> getB)
            {
                GetB = getB;
            }
        }

        public class B { }

        [Test]
        public void Can_select_scoped_over_singleton_or_transient()
        {
            var c = new Container();

            c.Register<X, Aa>(Reuse.Singleton);
            c.Register<X, Bb>(Reuse.InCurrentScope);
            c.Register<X, Cc>(Reuse.Transient);

            using (var s = c.OpenScope())
                Assert.IsNotNull(s.Resolve<X>());
        }

        [Test]
        public void Can_select_singleton_over_scoped()
        {
            var c = new Container();

            c.Register<X, Aa>(Reuse.Singleton);
            c.Register<X, Bb>(Reuse.InCurrentScope);

            c.Resolve<X>();
        }

        public class X { }
        public class Aa : X { }
        public class Bb : X { }
        public class Cc : X { }
    }
}
