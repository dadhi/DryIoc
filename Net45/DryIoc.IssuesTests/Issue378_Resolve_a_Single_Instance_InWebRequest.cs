using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue378_Resolve_a_Single_Instance_InWebRequest
    {
        [Test, Ignore("fails")]
        public void Test()
        {
            // base like: Win app
            //------------------------------
            var container = new Container(rules => rules
                .WithFactorySelector(Rules.SelectLastRegisteredFactory())
                .With(FactoryMethod.ConstructorWithResolvableArguments)
                .WithoutThrowOnRegisteringDisposableTransient()
                .WithDefaultReuseInsteadOfTransient(Reuse.Singleton));

            // Singletons: specified explicitly and do not depend on container default reuse
            container.RegisterMany<A>(Reuse.Singleton);
            container.RegisterMany<B>(Reuse.Singleton);

            // Reuse is not specified and will depend on container default reuse
            container.RegisterMany<X>();

            var b = container.Resolve<B>();
            Assert.AreSame(b.A, container.Resolve<B>().A);
            Assert.AreSame(b.X, container.Resolve<B>().X);

            // Web app
            //------------------------------
            var webContainer = container.With(
                rules => rules.WithDefaultReuseInsteadOfTransient(Reuse.InWebRequest),
                new AsyncExecutionFlowScopeContext());

            B bb;
            X xx;
            using (var scope = webContainer.OpenScope(Reuse.WebRequestScopeName))
            {
                bb = container.Resolve<B>();
                Assert.AreSame(bb.A, scope.Resolve<B>().A);

                xx = bb.X;
                Assert.AreSame(xx, scope.Resolve<B>().X);
            }

            using (var scope = webContainer.OpenScope(Reuse.WebRequestScopeName))
            {
                Assert.AreSame(bb.A, scope.Resolve<B>().A);

                Assert.AreNotSame(xx, scope.Resolve<B>().X);
            }
        }

        public class A { }

        public class B
        {
            private readonly Func<X> _x;
            public X X { get { return _x.Invoke(); } }

            public A A { get; private set; }

            public B(Func<X> x, A a)
            {
                _x = x;
                A = a;
            }
        }

        public class X { }
    }
}
