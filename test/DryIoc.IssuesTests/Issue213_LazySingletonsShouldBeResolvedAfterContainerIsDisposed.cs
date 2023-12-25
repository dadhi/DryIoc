using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue213_LazySingletonsShouldBeResolvedAfterContainerIsDisposed : ITest
    {
        public int Run()
        {
            Lazy_singletons_should_resolve_after_container_disposed();
            Lazy_singletons_should_resolve_after_container_disposed_without_throwing_for_captive_dependency();
            Func_singletons_should_resolve_after_container_disposed();
            Lazy_singleton_resolved_in_scope_can_got_a_value_outside_a_scope();
            Lazy_non_eager_singleton_resolved_in_scope_can_got_a_value_outside_a_scope();
            Lazy_non_eager_singleton_injected_in_scope_can_got_a_value_outside_a_scope();
            return 6;
        }

        [Test]
        public void Lazy_singletons_should_resolve_after_container_disposed()
        {
            var container = new Container();

            container.Register<Truc>(Reuse.Singleton, setup: Setup.With(preventDisposal: true));
            container.Register<LazyMachine>(Reuse.Singleton);
            container.Register<Bidule>(Reuse.Scoped);

            LazyMachine machine;
            using (var scope = container.OpenScope())
            {
                machine = scope.Resolve<Bidule>().Machine;
                Assert.IsNotNull(machine);
            }

            Assert.IsNotNull(machine.Truc);
        }

        [Test]
        public void Lazy_singletons_should_resolve_after_container_disposed_without_throwing_for_captive_dependency()
        {
            var container = new Container(rules => rules.WithoutThrowIfDependencyHasShorterReuseLifespan());

            container.Register<Truc>(Reuse.Singleton, setup: Setup.With(preventDisposal: true));
            container.Register<LazyMachine>(Reuse.Singleton);
            container.Register<Bidule>(Reuse.Scoped);

            LazyMachine machine;
            using (var scope = container.OpenScope())
            {
                machine = scope.Resolve<Bidule>().Machine;
                Assert.IsNotNull(machine);
            }

            Assert.IsNotNull(machine.Truc);
        }

        [Test]
        public void Func_singletons_should_resolve_after_container_disposed()
        {
            var container = new Container();

            container.Register<Truc>(Reuse.Singleton, setup: Setup.With(preventDisposal: true));
            container.Register<LazyMachine>(Reuse.Singleton);
            container.Register<Bidule>(Reuse.Scoped);

            LazyMachine machine;
            using (var scope = container.OpenScope())
            {
                machine = scope.Resolve<Bidule>().Machine;
                Assert.IsNotNull(machine);
            }

            Assert.IsNotNull(machine.Truc);
        }

        [Test]
        public void Lazy_singleton_resolved_in_scope_can_got_a_value_outside_a_scope()
        {
            var c = new Container();

            c.Register<S>(Reuse.Singleton);

            Lazy<S> s;
            using (var scope = c.OpenScope())
                s = scope.Resolve<Lazy<S>>();

            Assert.IsNotNull(s.Value);
        }

        [Test]
        public void Lazy_non_eager_singleton_resolved_in_scope_can_got_a_value_outside_a_scope()
        {
            var c = new Container(rules => rules.WithoutEagerCachingSingletonForFasterAccess());

            c.Register<S>(Reuse.Singleton);

            Lazy<S> s;
            using (var scope = c.OpenScope())
                s = scope.Resolve<Lazy<S>>();

            Assert.IsNotNull(s.Value);
        }

        [Test]
        public void Lazy_non_eager_singleton_injected_in_scope_can_got_a_value_outside_a_scope()
        {
            var c = new Container(rules => rules.WithoutEagerCachingSingletonForFasterAccess());

            c.Register<S>(Reuse.Singleton);
            c.Register<Q>(Reuse.Transient);

            Q q;
            using (var scope = c.OpenScope())
                q = scope.Resolve<Q>();

            Assert.IsNotNull(q.S.Value);
        }

        class S {}
        class Q { public Q(Lazy<S> s) => S = s; public Lazy<S> S; }

        public class Truc {}

        public class LazyMachine
        {
            private Lazy<Truc> truc;
            public Truc Truc => truc.Value;
            public LazyMachine(Lazy<Truc> truc) => this.truc = truc;
        }

        public class FuncMachine
        {
            private Func<Truc> truc;
            public Truc Truc => truc();
            public FuncMachine(Func<Truc> truc) => this.truc = truc;
        }

        public class Bidule
        {
            public LazyMachine Machine;
            public Bidule(LazyMachine machine) => Machine = machine;
        }
    }
}
