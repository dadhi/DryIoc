using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue213_LazySingletonsShouldBeResolvedAfterContainerIsDisposed
    {
        [Test, Ignore]
        public void LazySingletons_Should_Resolve_After_Container_Disposed()
        {
            var container = new Container();

            container.Register<Truc>(Reuse.Singleton, setup: Setup.With(preventDisposal: true));
            container.Register<Machin>(Reuse.Singleton);
            container.Register<Bidule>(Reuse.InWebRequest);

            Machin machine;
            using (var reqContainer = container.OpenScope(Reuse.WebRequestScopeName))
            {
                machine = reqContainer.Resolve<Bidule>().Machin;
                Assert.IsNotNull(machine);
            }
            Assert.IsNotNull(machine.Truc);
        }

        public class Truc
        {
        }

        public class Machin
        {
            public Machin(Lazy<Truc> truc)
            {
                this.truc = truc;
            }

            private Lazy<Truc> truc;

            public Truc Truc => truc.Value;
        }

        public class Bidule
        {
            public Bidule(Machin machin)
            {
                Machin = machin;
            }

            public Machin Machin;
        }
    }
}
