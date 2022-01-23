using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue213_LazySingletonsShouldBeResolvedAfterContainerIsDisposed
    {
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
