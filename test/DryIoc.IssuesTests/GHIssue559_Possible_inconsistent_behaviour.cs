using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue559_Possible_inconsistent_behaviour : ITest
    {
        public int Run()
        {
            Test1();
            return 1;
        }

        [Test]
        public void Test1()
        {
            var container = new Container();
            container.Register<Foo>(Reuse.Scoped, setup: Setup.With(openResolutionScope: true));
            
            var foo = container.Resolve<Foo>();
            Assert.IsNotNull(foo);
            
            var actual = container.Resolve<IEnumerable<Foo>>();
            Assert.AreEqual(0, actual.Count()); // todo: @fixme
        }

        public class Foo {}
    }
}
