using System;
using System.ComponentModel.Composition;
using DryIoc.FastExpressionCompiler.LightExpression;
using NUnit.Framework;
using DryIoc.MefAttributedModel;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue378_InconsistentResolutionFailure : ITest
    {
        public int Run()
        {
            Minimal_test();
            Resolve_undecorated_singleton_with_lazy_scoped_dependency_from_the_scope_should_succeed();
            Resolve_undecorated_non_eager_singleton_with_lazy_scoped_dependency_from_the_scope_should_succeed();
            Nested_scopes_undecorated_resolution_succeeds_but_decorated_resolution_with_the_FactoryMethod_fails();
            Nested_scopes_undecorated_resolution_succeeds_but_decorated_resolution_with_the_RegisterDelegate_fails_with_allow_captive_dependency_rule();
            Nested_scopes_decorated_resolution_should_throw_the_DependencyHasShorterReuseLifespan();

            return 6;
        }

        [Test]
        public void Minimal_test()
        {
            var c = new Container(rules => rules.WithoutThrowIfDependencyHasShorterReuseLifespan());

            c.Register<A>(Reuse.Singleton);
            c.Register<D>(Reuse.Singleton);

            A a;
            using (var s = c.OpenScope())
            {
                a = s.Resolve<A>();
            }

            Assert.IsNotNull(a.D.Value);
        }

        class A
        {
            public Lazy<D> D;
            public A(Lazy<D> d) => D = d;
        }

        [Test]
        public void Nested_scopes_undecorated_resolution_succeeds_but_decorated_resolution_with_the_FactoryMethod_fails()
        {
            var c = new Container().WithMef()
                .With(rules => rules
                .WithoutThrowIfDependencyHasShorterReuseLifespan()
                .WithDefaultReuse(Reuse.Scoped));

            // 1. default registrations
            c.RegisterExports(typeof(DecoratedService), typeof(UndecoratedService), typeof(DependencyService));

            // 2. IDecoratedService should be created via a factory method
            c.RegisterDelegate<Lazy<IDecoratedService>, IDecoratedService>(
                DecorateService, Reuse.Transient, setup: Setup.DecoratorWith(useDecorateeReuse: false));

            // global scope is opened once on application startup
            using (var globalScope = c.OpenScope())
            {
                // request scopes are created for each request
                using (var requestScope = globalScope.OpenScope())
                {
                    // succeeds
                    Assert.DoesNotThrow(() =>
                    {
                        var service = requestScope.Resolve<IUndecoratedService>();
                        service.Hello();
                    },
                    "Failed to use the undecorated service!");

                    // fails
                    Assert.DoesNotThrow(() =>
                    {
                        var decorated = requestScope.Resolve<IDecoratedService>();
                        decorated.Hello();
                    },
                    "Failed to use the decorated service!");
                }
            }
        }

        class D { }

        [Test]
        public void Nested_scopes_undecorated_resolution_succeeds_but_decorated_resolution_with_the_RegisterDelegate_fails_with_allow_captive_dependency_rule()
        {
            var c = new Container().WithMef()
                .With(rules => rules
                .WithoutThrowIfDependencyHasShorterReuseLifespan()
                .WithDefaultReuse(Reuse.Scoped));

            // The DependencyService would be scoped because of the container default reuse,
            // so it happens it will be resolved from the resolver of the singleton parent which is root container instead of scope
            c.RegisterExports(typeof(DecoratedService), typeof(DependencyService));

            // IDecoratedService should be created via a factory method
            c.RegisterDelegate<Lazy<IDecoratedService>, IDecoratedService>(DecorateService, 
                Reuse.Transient, Setup.DecoratorWith(useDecorateeReuse: false));

            // global scope is opened once on application startup
            using (var globalScope = c.OpenScope())
            {
                // request scopes are created for each request
                using (var requestScope = globalScope.OpenScope())
                {
                    Assert.DoesNotThrow(() =>
                    {
                        //var decoratedExpr = requestScope.Resolve<LambdaExpression>(typeof(IDecoratedService));
                        var decorated = requestScope.Resolve<IDecoratedService>();
                        decorated.Hello();
                    },
                    "Failed to use the decorated service!");
                }
            }
        }

        [Test]
        public void Resolve_undecorated_singleton_with_lazy_scoped_dependency_from_the_scope_should_succeed()
        {
            var c = new Container().WithMef()
                .With(rules => rules
                .WithoutThrowIfDependencyHasShorterReuseLifespan()
                .WithDefaultReuse(Reuse.Scoped));

            // The DependencyService would be scoped because of the container default reuse,
            // so it happens it will be resolved from the resolver of the singleton parent which is root container instead of scope
            c.RegisterExports(typeof(UndecoratedService), typeof(DependencyService));

            // global scope is opened once on application startup
            using (var globalScope = c.OpenScope())
            {
                // request scopes are created for each request
                using (var requestScope = globalScope.OpenScope())
                {
                    Assert.DoesNotThrow(() =>
                    {
                        var decorated = requestScope.Resolve<IUndecoratedService>();
                        decorated.Hello();
                    },
                    "Failed to resolve the undecorated singleton service with scoped dependency!");
                }
            }
        }

        [Test]
        public void Resolve_undecorated_non_eager_singleton_with_lazy_scoped_dependency_from_the_scope_should_succeed()
        {
            var c = new Container().WithMef()
                .With(rules => rules
                .WithoutThrowIfDependencyHasShorterReuseLifespan()
                .WithoutEagerCachingSingletonForFasterAccess()
                .WithDefaultReuse(Reuse.Scoped));

            // The DependencyService would be scoped because of the container default reuse,
            // so it happens it will be resolved from the resolver of the singleton parent which is root container instead of scope
            c.RegisterExports(typeof(UndecoratedService), typeof(DependencyService));

            // global scope is opened once on application startup
            using (var globalScope = c.OpenScope())
            {
                // request scopes are created for each request
                using (var requestScope = globalScope.OpenScope())
                {
                    Assert.DoesNotThrow(() =>
                    {
                        var decorated = requestScope.Resolve<IUndecoratedService>();
                        decorated.Hello();
                    },
                    "Failed to resolve the undecorated singleton service with scoped dependency!");
                }
            }
        }

        [Test]
        public void Nested_scopes_decorated_resolution_should_throw_the_DependencyHasShorterReuseLifespan()
        {
            var c = new Container().WithMef()
                .With(rules => rules
                .WithDefaultReuse(Reuse.Scoped));

            // The DependencyService would be scoped because of the container default reuse and 
            // the Decorator will also be a scoped because it does not specify as reuse.
            c.RegisterExports(typeof(DecoratedService), typeof(DependencyService));

            // IDecoratedService should be created via a factory method
            var decoratorSetup = Setup.DecoratorWith(useDecorateeReuse: false);
            c.RegisterDelegate<Lazy<IDecoratedService>, IDecoratedService>(DecorateService, setup: decoratorSetup);

            // global scope is opened once on application startup
            using (var globalScope = c.OpenScope())
            {
                // request scopes are created for each request
                using (var requestScope = globalScope.OpenScope())
                {
                    var ex = Assert.Throws<ContainerException>(() =>
                    {
                        //var decoratedExpr = requestScope.Resolve<LambdaExpression>(typeof(IDecoratedService));
                        var decorated = requestScope.Resolve<IDecoratedService>();
                        decorated.Hello();
                    });
                    Assert.AreEqual(Error.NameOf(Error.DependencyHasShorterReuseLifespan), ex.ErrorName);
                }
            }
        }

        static IDecoratedService DecorateService(Lazy<IDecoratedService> lazy)
        {
            WriteLine("CreateDecoratedService is called!");
            return lazy.Value;
        }

#if DEBUG
        private static void WriteLine(string s) => TestContext.Progress.WriteLine(s);
#else
        private static void WriteLine(string s) {}
#endif

        public interface IUndecoratedService
        {
            void Hello();
        }

        // Code smell: the singleton service uses a dependency of a shorter lifespan,
        // but the container is set up to allow this
        [Export(typeof(IUndecoratedService)), PartCreationPolicy(CreationPolicy.Shared)]
        public class UndecoratedService : IUndecoratedService
        {
            public UndecoratedService(Lazy<IDependencyService> dep)
            {
                WriteLine("UndecoratedService is created");
                Dependency = dep;
            }

            private Lazy<IDependencyService> Dependency { get; set; }

            public void Hello()
            {
                WriteLine("UndecoratedService is accessing the Dependency...");

                var cm = Dependency.Value;
                WriteLine("Dependency returned: " + cm.Value);
            }
        }

        public interface IDecoratedService
        {
            void Hello();
        }

        // Code smell: the singleton service uses a dependency of a shorter lifespan,
        // but the container is set up to allow this
        [Export(typeof(IDecoratedService)), PartCreationPolicy(CreationPolicy.Shared)]
        public class DecoratedService : IDecoratedService
        {
            public DecoratedService(Lazy<IDependencyService> dep)
            {
                WriteLine("DecoratedService is created");
                Dependency = dep;
            }

            private Lazy<IDependencyService> Dependency { get; set; }

            public void Hello()
            {
                WriteLine("DecoratedService is accessing the Dependency...");

                var cm = Dependency.Value;
                WriteLine("Dependency returned: " + cm.Value);
            }
        }

        public interface IDependencyService
        {
            string Value { get; }
        }

        // The only dependency of both decorated and undecorated services
        [Export(typeof(IDependencyService))]
        public class DependencyService : IDependencyService
        {
            public string Value => "Hello";
        }
    }
}
