using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue574_IResolverContext_UseInstance_ShouldNotHaveSideEffectsOnOtherScopes
    {
        [Test]
        public void Can_add_to_single_keyed_singleton_the_instance()
        {
            var container = new Container();

            container.Register<A, AA>(Reuse.Singleton, serviceKey: "42");
            var aa = container.Resolve<A>(serviceKey: "42");
            Assert.IsInstanceOf<AA>(aa);

            container.RegisterInstance<A>(new AB(), IfAlreadyRegistered.Replace, serviceKey: "42");
            var ab = container.Resolve<A>(serviceKey: "42");
            Assert.IsInstanceOf<AB>(ab);
        }

        [Test]
        public void Can_add_to_multiple_keyed_singletons_the_instance()
        {
            var container = new Container();

            container.Register<A>(Reuse.Singleton);

            container.Register<A, AA>(Reuse.Singleton, serviceKey: "42");

            var aa = container.Resolve<A>(serviceKey: "42");
            Assert.IsInstanceOf<AA>(aa);

            container.RegisterInstance<A>(new AB(), IfAlreadyRegistered.Replace, serviceKey: "42");
            var ab = container.Resolve<A>(serviceKey: "42");
            Assert.IsInstanceOf<AB>(ab);
        }

        [Test]
        public void No_cache_issues_between_normal_registration_and_UseInstance()
        {
            var container = new Container();

            container.Register<A>();
            var _ = container.Resolve<A>();

            container.Register<A, AA>(Reuse.Singleton);
            var aa = container.Resolve<A>();
            Assert.IsInstanceOf<AA>(aa);

            using (var scope = container.OpenScope())
            {
                var ai = new A();
                scope.Use(ai);
                Assert.AreSame(ai, scope.Resolve<A>());
            }

            var aa2 = container.Resolve<A>();
            Assert.IsInstanceOf<AA>(aa2);
            Assert.AreSame(aa, aa2);
        }

        [Test]
        public void No_cache_issues_between_more_than_2_normal_registration_and_UseInstance()
        {
            var container = new Container();

            container.Register<A>(serviceKey: "a");
            container.Register<A, AA>();
            var aa = container.Resolve<A>();
            Assert.IsInstanceOf<AA>(aa);

            container.Register<A, AB>(Reuse.Singleton);
            var ab = container.Resolve<A>();
            Assert.IsInstanceOf<AB>(ab);

            using (var scope = container.OpenScope())
            {
                var ai = new A();
                scope.Use(ai);
                Assert.AreSame(ai, scope.Resolve<A>());
            }

            var ab2 = container.Resolve<A>();
            Assert.IsInstanceOf<AB>(ab2);
            Assert.AreSame(ab, ab2);
        }

        public class A { }
        public class AA : A { }
        public class AB : A { }

        [Test]
        public void ScopedFactory_ShouldResolveItselfWithinSelfScope_EvenIfThereAreParallelScopes()
        {
            var container = new Container();

            container.Register<IScopedFactory, ScopedFactory>(Reuse.Transient);

            var scopedFactory1 = container.Resolve<IScopedFactory>();
            var scopedFactory2 = scopedFactory1.Resolve<IScopedFactory>();
            Assert.AreSame(scopedFactory1, scopedFactory2);

            // doing the same once again

            // Produces new factory, resolving a transient from container.
            // When this new factory is created, it is opening new / separate scope and adding itself to it ... 
            var scopedFactory3 = container.Resolve<IScopedFactory>();
            Assert.AreNotSame(scopedFactory1, scopedFactory3); 

            // .. so the further resolution from scoped factory scope, produces the same as 3rd
            var scopedFactory4 = scopedFactory3.Resolve<IScopedFactory>();
            Assert.AreSame(scopedFactory3, scopedFactory4);
            Assert.AreNotSame(scopedFactory2, scopedFactory4);
        }

        [Test]
        public void ScopedFactory_ShouldResolveItselfWithinSelfScope_EvenIfThereAreParallelScopesAndNullArgsProvided()
        {
            var container = new Container();

            container.Register<IScopedFactory, ScopedFactory>(Reuse.Transient);

            var scopedFactory1 = container.Resolve<IScopedFactory>(args: null);
            var scopedFactory2 = scopedFactory1.Resolve<IScopedFactory>();
            Assert.AreSame(scopedFactory1, scopedFactory2);

            var scopedFactory3 = container.Resolve<IScopedFactory>(args: null);
            var scopedFactory4 = scopedFactory3.Resolve<IScopedFactory>();
            Assert.AreNotSame(scopedFactory1, scopedFactory3);
            Assert.AreSame(scopedFactory3, scopedFactory4);
        }

        interface IScopedFactory
        {
            T Resolve<T>();
        }

        class ScopedFactory : IScopedFactory
        {
            private readonly IResolverContext _scope;

            public ScopedFactory(IResolverContext context)
            {
                _scope = context.OpenScope();

                //use this factory within it`s scope
                _scope.Use<IScopedFactory>(this);
            }

            public T Resolve<T>() => _scope.Resolve<T>();
        }
    }
}
