using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue507_Transient_resolve_with_opening_scope_using_factory_func_in_singleton : ITest
    {
        public int Run()
        {
            Test_Original_issue();
            Test_Simple();
            return 2;
        }

        [Test]
        public void Test_Simple()
        {
            var c = new Container();

            c.Register<Bar2>(Reuse.Singleton);
            c.Register<Foo2>(Reuse.Scoped);

            var b = c.Resolve<Bar2>(); // it should fail on resolve

            var gf = b.GetFoo;
            Assert.IsNotNull(gf);

            var f = gf(c.OpenScope());
            Assert.IsNotNull(f);

        }

        class Bar2
        {
            public readonly Func<IResolverContext, Foo2> GetFoo;
            public Bar2(Func<IResolverContext, Foo2> getFoo) => GetFoo = getFoo;
        }

        class Foo2
        {
        }

        [Test]
        public void Test_Original_issue()
        {
            var container = new Container(rules => rules.WithTrackingDisposableTransients());

            container.Register<Bar>(Reuse.Singleton); // if "Reuse.Singleton" gets removed, the scope ist opened (and injected as part of IResolverContext) as expected.
            container.Register<IFoo, Foo>(setup: Setup.With(openResolutionScope: true));

            container.Register<Dependency>(Reuse.ScopedTo<IFoo>());

            var bar = container.Resolve<Bar>();
            bar.DoStuffWithFoo();
        }

        class Bar
        {
            private readonly Func<IFoo> fooFactory;
            private IFoo foo;

            public void DoStuffWithFoo()
            {
                foo = fooFactory(); // expecting this to open a scope.
                foo.Dispose();
            }

            public Bar(Func<IFoo> fooFactory)
            {
                this.fooFactory = fooFactory;
            }
        }

        interface IFoo : IDisposable
        {
            Dependency Dep { get; }
        }

        class Foo : IFoo
        {
            public Dependency Dep { get; }

            private readonly IResolverContext _scope;
            public Foo(Dependency dep, IResolverContext scope)
            {
                Dep = dep;
                _scope = scope;

                Assert.IsTrue(scope.CurrentScope != null, "Expected to have a scope.");
            }

            public void Dispose() => _scope.Dispose();
        }

        class Dependency : IDisposable
        {
            public bool IsDisposed { get; private set; }
            public void Dispose() => IsDisposed = true;
        }
    }
}
