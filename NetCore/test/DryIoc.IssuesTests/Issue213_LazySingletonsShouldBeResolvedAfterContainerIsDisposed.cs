using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue213_LazySingletonsShouldBeResolvedAfterContainerIsDisposed
    {
        [Test]
        public void LazySingletons_Should_Resolve_After_Container_Disposed()
        {
            var container = new Container();

            container.Register<Truc>(Reuse.Singleton, setup: Setup.With(preventDisposal: true));
            container.Register<Machine>(Reuse.Singleton);
            container.Register<Bidule>(Reuse.Scoped);

            Machine machine;
            using (var scope = container.OpenScope())
            {
                machine = scope.Resolve<Bidule>().Machine;
                Assert.IsNotNull(machine);
            }

            Assert.IsNotNull(machine.Truc);
        }

        public class Truc
        {
        }

        public class Machine
        {
            public Machine(Lazy<Truc> truc)
            {
                this.truc = truc;
            }

            private Lazy<Truc> truc;

            public Truc Truc => truc.Value;
        }

        public class Bidule
        {
            public Bidule(Machine machine)
            {
                Machine = machine;
            }

            public Machine Machine;
        }
    }
}
