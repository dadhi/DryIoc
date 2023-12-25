using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue301_Breakage_in_scoped_enumeration_in_v4 : ITest
    {
        public int Run()
        {
            Test();
            return 1;
        }

        [Test]
        public void Test()
        {
            var c = new Container();

            c.RegisterMany<Mode1Comp>(Reuse.Transient, setup: Setup.With(openResolutionScope: true));
            c.RegisterMany<Mode2Comp>(Reuse.Transient, setup: Setup.With(openResolutionScope: true));

            c.Register<IOoui, Ooui1>(Reuse.ScopedTo<IComp>(), setup: Setup.With(
                condition: req => req.Any(x => x.ServiceType == typeof(IMode1Comp))));

            c.Register<IOoui, Ooui2>(Reuse.ScopedTo<IComp>(), setup: Setup.With(
                condition: req => req.Any(x => x.ServiceType == typeof(IMode2Comp))));

            c.RegisterMany<OouiHost>(Reuse.ScopedTo<IComp>());
            
            c.RegisterMany<Mode1>();
            c.RegisterMany<Mode2>();

            var mode1 = c.Resolve<Mode1>();
            Assert.IsInstanceOf<Ooui1>(mode1.Comp.Oouis.Single());

            var mode2 = c.Resolve<Mode2>();
            Assert.IsInstanceOf<Ooui2>(mode2.Comp.Oouis.Single());
        }

        public interface IComp
        {
            IEnumerable<IOoui> Oouis { get; }
        }

        public interface IOoui { }
        public class Ooui1 : IOoui { }
        public class Ooui2 : IOoui { }

        public class OouiHost
        {
            public readonly IEnumerable<IOoui> Oouis;

            public OouiHost(IEnumerable<IOoui> oouis) => Oouis = oouis;
        }

        public interface IMode1Comp : IComp { }
        public interface IMode2Comp : IComp { }

        public class Mode1Comp : IMode1Comp
        {
            public IEnumerable<IOoui> Oouis { get; }
            
            public Mode1Comp(OouiHost host) => Oouis = host.Oouis;
        }

        public class Mode2Comp : IMode2Comp
        {
            public IEnumerable<IOoui> Oouis { get; }

            public Mode2Comp(OouiHost host) => Oouis = host.Oouis;
        }

        public class Mode1
        {
            public IMode1Comp Comp { get; }

            public Mode1(IMode1Comp comp) => Comp = comp;
        }

        public class Mode2
        {
            public IMode2Comp Comp { get; }

            public Mode2(IMode2Comp comp) => Comp = comp;
        }
    }
}
