using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue435_ReuseSingleton_prevents_the_correct_container_injection_within_explicit_resolve
    {
        [Test]
        public void ScopesResolution()
        {
            var container = new Container();

            container.Register<A>();
            container.Register<B>();
            container.Register<C>(Reuse.Singleton);
            container.Register<D>();

            var scope = container.OpenScope("scopeX");
            var nestedScope = scope.OpenScope("nestedScopeX");

            var a = nestedScope.Resolve<A>();

            // because D is resolved inside A as: c.Resolve<D>()
            Assert.AreSame(nestedScope, a.D.Container);

            // because D is injected inside singleton C it receives the root container
            Assert.AreSame(container, a.B.C.D.Container);

            // because D is resolved directly
            var d = nestedScope.Resolve<D>();
            Assert.AreSame(nestedScope, d.Container);
        }

        public class A
        {
            public IContainer Container;

            public B B { get; }
            public D D { get; }

            public A(IContainer container, B b)
            {
                Container = container;
                B = b;
                D = container.Resolve<D>();
            }
        }

        public class B
        {
            public C C { get; }

            public B(C c)
            {
                C = c;
            }
        }

        public class C
        {
            public D D { get; }

            public C(D d)
            {
                D = d;
            }
        }

        public class D
        {
            public IContainer Container { get; }

            public D(IContainer container)
            {
                Container = container;
            }
        }
    }
}
