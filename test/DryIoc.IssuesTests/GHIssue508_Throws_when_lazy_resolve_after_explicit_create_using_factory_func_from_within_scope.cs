using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue508_Throws_when_lazy_resolve_after_explicit_create_using_factory_func_from_within_scope : ITest
    {
        public int Run()
        {
            Example();
            return 1;
        }

        [Test]
        public void Example()
        {
            var container = new Container();

            container.Register<CarFactory>(Reuse.Singleton);
            container.Register<Mercedes>(Reuse.Scoped);
            container.Register<Engine>(Reuse.Scoped);

            using (var scope = container.OpenScope())
            {
                var carFactory = scope.Resolve<CarFactory>();
                var car = carFactory.BuildCar();
            }
        }

        class CarFactory
        {
            private readonly Func<int, Engine> engineFactory;
            private readonly Func<Mercedes> mercedesFactory;

            public Mercedes BuildCar()
            {
                var engine = engineFactory(120);
                var mercedes = mercedesFactory();

                var sameEngineRetrievedUsingFactoryWithParam = mercedes.GetEngineUsingFuncWithParam(99); // works like expected: returns same instance of engine created earlier in mercedesFactory(Func<Mercedes>). So parameter isn't used, because instance was already present in scope (which is the behavior I expected).
                Assert.AreEqual(engine, sameEngineRetrievedUsingFactoryWithParam);

                var sameEngine = mercedes.GetEngine(); // crashes: UnableToResolveUnknownService Int32 as parameter "maxRpm"; Expected it to don't crash but return the same instance of Engine that was created earlier using the engineFactory (engine is already present in the scope, so it doesn't need to be created and therefore doesn't need the parameter).
                Assert.AreEqual(engine, sameEngine);

                return mercedes;
            }

            public CarFactory(Func<int, Engine> engineFactory, Func<Mercedes> mercedesFactory)
            {
                this.engineFactory = engineFactory;
                this.mercedesFactory = mercedesFactory;
            }
        }

        class Mercedes
        {
            private Lazy<Engine> getEngine;
            private Func<int, Engine> EngineFactory;

            public Mercedes(Lazy<Engine> engineLazy, Func<int, Engine> engineFactory, IResolverContext rc)
            {
                this.getEngine = engineLazy;
                this.EngineFactory = n => {
                    var e = engineFactory(n);
                    rc.Use(e);
                    return e;
                };
            }

            public Engine GetEngine() => getEngine.Value;
            public Engine GetEngineUsingFuncWithParam(int maxRpm) => EngineFactory(maxRpm);
        }

        class Engine
        {
            public Engine(int maxRpm) => MaxRpm = maxRpm;
            public int MaxRpm { get; }
        }
    }
}