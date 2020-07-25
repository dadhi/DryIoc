using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue295_useParentReuse_does_not_respects_parent_reuse
    {
        [Test, Ignore("todo: fixme - works without `useParentReuse: true` because code searches for first non Transient ancestor and skips any Transients in between")]
        public void RespectsParentReuse()
        {
            var c = new Container();
            c.Register<Car>(Reuse.Scoped, setup: Setup.With(openResolutionScope: true));

            c.Register<Engine>(setup: Setup.With(useParentReuse: true)); // Scoped

            c.Register<Replicator>(Reuse.Transient);
            
            c.Register<Energy>(setup: Setup.With(useParentReuse: true));

            var car1 = c.Resolve<Car>();
            var car2 = c.Resolve<Car>();
            var engine1 = car1.Engine.Replicator.CreateTransientEngine();
            var engine2 = car1.Engine.Replicator.CreateTransientEngine();

            Assert.AreNotSame(car1.Engine.Replicator.Energy, car2.Engine.Replicator.Energy);
            Assert.AreNotSame(engine1, engine2);
            // Failed
            Assert.AreNotSame(car1.Engine.Replicator.Energy, engine1.Replicator.Energy);
            // Failed
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
