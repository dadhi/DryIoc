using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue378_Resolve_a_Single_Instance_InWebRequest
    {
        public static IContainer ConfigureWinAppContainerAndRegisterServices()
        {
            var container = new Container(rules => rules
                .WithFactorySelector(Rules.SelectLastRegisteredFactory())
                .With(FactoryMethod.ConstructorWithResolvableArguments)
                .WithoutThrowOnRegisteringDisposableTransient()
                .WithDefaultReuse(Reuse.Singleton)); // Specify to use singleton for services with unspecified reuse

            // Singletons: specified explicitly and do not depend on container default reuse
            container.RegisterMany<A>(Reuse.Singleton);
            container.RegisterMany<B>(Reuse.Singleton);

            // Reuse is not specified and will depend on container default reuse
            container.RegisterMany<X>();

            return container;
        }

        public static IContainer ReconfigureForWeb(IContainer container)
        {
            return container.With(
                rules => rules.WithDefaultReuse(Reuse.InWebRequest),
                new AsyncExecutionFlowScopeContext());
        }

        [Test]
        public void Win_app_test()
        {
            var container = ConfigureWinAppContainerAndRegisterServices();

            var b = container.Resolve<B>();

            Assert.AreSame(b.A, container.Resolve<B>().A); // singletons
            Assert.AreSame(b.X, container.Resolve<B>().X);
        }

        [Test]
        public void Web_app_test()
        {
            var container = ConfigureWinAppContainerAndRegisterServices();
            container = ReconfigureForWeb(container);

            X xx;
            using (var scope = container.OpenScope(Reuse.WebRequestScopeName))
            {
                var bb = scope.Resolve<B>();
                Assert.AreSame(bb.A, scope.Resolve<B>().A); // singletons

                xx = bb.X;                                  // scoped
                Assert.AreSame(xx, scope.Resolve<B>().X);   // should be the same inside the scope
            }

            using (var scope = container.OpenScope(Reuse.WebRequestScopeName))
            {
                var b2 = scope.Resolve<B>();
                Assert.AreNotSame(xx, b2.X); // should Not be the same in other scope
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
