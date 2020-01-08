using System;
using ImTools;
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
        public void Main_test()
        {
            var c = new Container();
            
            c.Register<IFoo, Foo>(Reuse.Scoped);
            c.Register<IFoo, FooDecorator>(Reuse.Scoped, setup: Setup.Decorator);
            c.Register<FooFactory>(Reuse.Scoped);
            c.Register<IFoo>(Made.Of(_ => ServiceInfo.Of<FooFactory>(), 
                f => f.GetOrCreate(Arg.Of<IResolverContext>(), Arg.Of<FactoryDelegate, Func<string, IFoo>>(), Arg.Of<string>())), 
                setup: Setup.Decorator);

            using (var scope = c.OpenScope())
            {
                var foo1 = scope.Resolve<IFoo>(new object[] { "1" });
                var foo2 = scope.Resolve<IFoo>(new object[] { "2" });

                // Test that each IFoo is a different instance.
                Assert.AreNotSame(foo1, foo2);

                // That that I get the same instance when provided the same address while the named scope (address)
                // has not been disposed.
                var foo11 = scope.Resolve<IFoo>(new object[] { "1" });
                Assert.AreSame(foo1, foo11);

                var foo22 = scope.Resolve<IFoo>(new object[] { "2" });
                Assert.AreSame(foo2, foo22);

                // Test that after disposing FooDecorator, the named scope is closed and I get
                // a new instance of foo for address = "1". 
                foo1.Dispose();
                var foo111 = scope.Resolve<IFoo>(new object[] { "1" });
                Assert.AreNotSame(foo1, foo111);
            }
        }

        [Test]
        public void Main_test_with_strongly_typed_FactoryDelegate()
        {
            var c = new Container();

            c.Register<IFoo, Foo>(Reuse.Scoped);
            c.Register<IFoo, FooDecorator>(Reuse.Scoped, setup: Setup.Decorator);
            c.Register<FooFactory>(Reuse.Scoped);
            c.Register<IFoo>(Made.Of(_ => ServiceInfo.Of<FooFactory>(),
                    f => f.GetOrCreate2(Arg.Of<IResolverContext>(), Arg.Of<FactoryDelegate<Func<string, IFoo>>>(), Arg.Of<string>())),
                setup: Setup.Decorator);

            using (var scope = c.OpenScope())
            {
                var foo1 = scope.Resolve<IFoo>(new object[] { "1" });
                var foo2 = scope.Resolve<IFoo>(new object[] { "2" });

                // Test that each IFoo is a different instance.
                Assert.AreNotSame(foo1, foo2);

                // That that I get the same instance when provided the same address while the named scope (address)
                // has not been disposed.
                var foo11 = scope.Resolve<IFoo>(new object[] { "1" });
                Assert.AreSame(foo1, foo11);

                var foo22 = scope.Resolve<IFoo>(new object[] { "2" });
                Assert.AreSame(foo2, foo22);

                // Test that after disposing FooDecorator, the named scope is closed and I get
                // a new instance of foo for address = "1". 
                foo1.Dispose();
                var foo111 = scope.Resolve<IFoo>(new object[] { "1" });
                Assert.AreNotSame(foo1, foo111);
            }
        }

        [Test]
        public void Main_test_with_RegisterDelegate_and_strongly_typed_FactoryDelegate()
        {
            var c = new Container();

            c.Register<IFoo, Foo>(Reuse.Scoped);
            c.Register<IFoo, FooDecorator>(Reuse.Scoped, setup: Setup.Decorator);

            var _scopes = ImHashMap<string, IResolverContext>.Empty;
            c.RegisterDelegate<IResolverContext, FactoryDelegate<Func<string, IFoo>>, string, IFoo>(
                (ctx, decorateeFactory, address) =>
                {
                    // fancy ensuring that we have a single new Scope created and stored
                    var scopeEntry = _scopes.GetEntryOrDefault(address);
                    if (scopeEntry == null)
                    {
                        Ref.Swap(ref _scopes, address, (x, a) => x.AddOrKeep(a));
                        scopeEntry = _scopes.GetEntryOrDefault(address);
                    }

                    lock (scopeEntry)
                        if (scopeEntry.Value == null || scopeEntry.Value.IsDisposed)
                            scopeEntry.Value = ctx.OpenScope(address);

                    return decorateeFactory(scopeEntry.Value).Invoke(address);
                },

                setup: Setup.Decorator);


            using (var scope = c.OpenScope())
            {
                var foo1 = scope.Resolve<IFoo>(new object[] { "1" });
                var foo2 = scope.Resolve<IFoo>(new object[] { "2" });

                // Test that each IFoo is a different instance.
                Assert.AreNotSame(foo1, foo2);

                // That that I get the same instance when provided the same address while the named scope (address)
                // has not been disposed.
                var foo11 = scope.Resolve<IFoo>(new object[] { "1" });
                Assert.AreSame(foo1, foo11);

                var foo22 = scope.Resolve<IFoo>(new object[] { "2" });
                Assert.AreSame(foo2, foo22);

                // Test that after disposing FooDecorator, the named scope is closed and I get
                // a new instance of foo for address = "1". 
                foo1.Dispose();
                var foo111 = scope.Resolve<IFoo>(new object[] { "1" });
                Assert.AreNotSame(foo1, foo111);
            }
        }

        public interface IFoo : IDisposable
        {
            void DoSomething();
        }

        public class Foo : IFoo
        {
            public string Address { get; }

            public Foo(string address) => Address = address;

            public void DoSomething() { }

            public void Dispose() {}
        }

        public class FooDecorator : IFoo
        {
            private readonly IFoo _foo;
            private readonly IResolverContext _ctx;

            public FooDecorator(IFoo foo, IResolverContext ctx)
            {
                _foo = foo;
                _ctx = ctx;
            }

            public void DoSomething() => _foo.DoSomething();

            public void Dispose() => _ctx.Dispose();
        }

        public class FooFactory
        {
            private ImHashMap<string, IResolverContext> _scopes = ImHashMap<string, IResolverContext>.Empty;

            public IFoo GetOrCreate(IResolverContext ctx, FactoryDelegate fooFactory, string address)
            {
                // fancy ensuring that we have a single new Scope created and stored
                var scopeEntry = _scopes.GetEntryOrDefault(address);
                if (scopeEntry == null)
                {
                    Ref.Swap(ref _scopes, address, (x, a) => x.AddOrKeep(a));
                    scopeEntry = _scopes.GetEntryOrDefault(address);
                }

                lock (scopeEntry)
                    if (scopeEntry.Value == null || scopeEntry.Value.IsDisposed)
                        scopeEntry.Value = ctx.OpenScope(address);

                return ((Func<string, IFoo>)fooFactory(scopeEntry.Value)).Invoke(address);
            }

            public IFoo GetOrCreate2(IResolverContext ctx, FactoryDelegate<Func<string, IFoo>> fooFactory, string address)
            {
                // fancy ensuring that we have a single new Scope created and stored
                var scopeEntry = _scopes.GetEntryOrDefault(address);
                if (scopeEntry == null)
                {
                    Ref.Swap(ref _scopes, address, (x, a) => x.AddOrKeep(a));
                    scopeEntry = _scopes.GetEntryOrDefault(address);
                }

                lock (scopeEntry)
                    if (scopeEntry.Value == null || scopeEntry.Value.IsDisposed)
                        scopeEntry.Value = ctx.OpenScope(address);

                return fooFactory(scopeEntry.Value).Invoke(address);
            }

            // Closes all scopes - Note: alternatively we may add scope tracking in parent and rely on parent scope disposal
            public void Dispose() => _scopes.Visit(entry => entry.Value?.Dispose());
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
