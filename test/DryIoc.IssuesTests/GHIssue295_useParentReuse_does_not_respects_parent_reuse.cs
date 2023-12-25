using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue295_useParentReuse_does_not_respects_parent_reuse : ITest
    {
        public int Run()
        {
            RespectsParentReuse();
            return 1;
        }

        [Test]
        public void RespectsParentReuse()
        {
            var c = new Container();
            c.Register<Car>(Reuse.Scoped, setup: Setup.With(openResolutionScope: true));

            c.Register<Engine>(setup: Setup.With(useParentReuse: true)); // Scoped when injected in Car

            // WORKS!: this does work but requires you to know how to specify no-caching, and maybe it is a good thing.
            //c.Register(typeof(Replicator), 
            //    new ReflectionFactory(typeof(Replicator), Reuse.Transient) { Caching = FactoryCaching.DoNotCache });

            // DOES NOT WORK!
            // Hmm, the question though - does asResolutionCall prevents caching, ahahahaaa - NO~!
            // So, let's see what happens when we disable the caching for IsResolutionCall - WORKS!
            c.Register<Replicator>(Reuse.Transient, setup: Setup.With(asResolutionCall: true));

            c.Register<Energy>(setup: Setup.With(useParentReuse: true)); // Scoped when injected in Engine

            var car1 = c.Resolve<Car>();
            var car2 = c.Resolve<Car>();
            var engine1 = car1.Engine.Replicator.CreateTransientEngine();
            var engine2 = car1.Engine.Replicator.CreateTransientEngine();

            Assert.AreNotSame(car1, car2);
            Assert.AreNotSame(engine1, engine2);
            Assert.AreNotSame(engine1.Replicator, engine2.Replicator);
            Assert.AreNotSame(engine1.Replicator.Energy, engine2.Replicator.Energy);
        }

        class Car
        {
            public Engine Engine { get; }

            public Car(Engine engine) => Engine = engine;
        }

        class Engine
        {
            public Replicator Replicator { get; }

            public Engine(Replicator replicator) => Replicator = replicator;
        }

        class Replicator
        {
            public IContainer Container { get; }

            public Energy Energy { get; }

            public Replicator(IContainer container, Energy energy)
            {
                Container = container;
                Energy = energy;
            }

            public Engine CreateTransientEngine() =>
                Container.Resolve<Engine>();
        }

        class Energy
        {
        }
    }
}
