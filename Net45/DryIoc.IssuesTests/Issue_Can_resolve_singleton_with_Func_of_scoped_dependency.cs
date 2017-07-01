using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue_Can_resolve_singleton_with_Func_of_scoped_dependency
    {
        [Test]
        public void Test()
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
    }
}
