using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue128_ResolveFailsWithSingletonsInOuterScope : ITest
    {
        public int Run()
        {
            Test();
            return 1;
        }

        [Test]
        public void Test()
        {
            var container = new Container();

            container.RegisterMany<Layr1>(Reuse.ScopedTo<IComp>(), nonPublicServiceTypes: true);
            container.RegisterMany<Layr2>(Reuse.ScopedTo<IComp>(), nonPublicServiceTypes: true);
            container.RegisterMany<Comp1>(Reuse.Singleton, nonPublicServiceTypes: true, setup: Setup.With(openResolutionScope: true));
            container.RegisterMany<Comp2>(Reuse.Singleton, nonPublicServiceTypes: true, setup: Setup.With(openResolutionScope: true));
            container.RegisterMany<Serv1>(Reuse.Singleton, nonPublicServiceTypes: true); 
            container.RegisterMany<Serv2>(Reuse.Singleton, nonPublicServiceTypes: true);

            container.Resolve<Serv1>();
        }

        public interface ILayr { }
        class Layr1 : ILayr { }

        class Layr2 : ILayr
        {
            public Layr2(Layr1 one, Serv2 s2) { }
        }

        public interface IComp { }
        class Comp1 : IComp
        {
            public Comp1(Layr1 one, Layr2 two) { }
        }

        class Comp2 : IComp
        {
            public Comp2(Layr1 one, Layr2 two, Serv2 s2) { }
        }

        class Serv1
        {
            public Serv1(Comp1 c1, Comp2 c2) { }
        }

        class Serv2 { }
    }
}
