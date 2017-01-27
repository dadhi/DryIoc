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

            var root = container.OpenScope("Root");
            var scope1 = root.OpenScope("Scope1");

            var a = scope1.Resolve<A>();
            // because D is resolved inside A as: c.Resolve<D>()
            Assert.AreSame(scope1, a.D.Container);

            // because D is injected inside songleton C it receives the root container
            Assert.AreSame(container, a.B.C.D.Container);

            // because D is resolved directly
            var d = scope1.Resolve<D>();
            Assert.AreSame(scope1, d.Container);
        }

        public class A
        {
            private IContainer _container;

            public B B { get; }
            public D D { get; }

            public A(IContainer container, B b)
            {
                _container = container;
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
