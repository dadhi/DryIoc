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
            Test_scoped_opening_scope();
            Test_singleton_opening_scope();
            return 2;
        }

        [Test]
        public void Test_scoped_opening_scope()
        {
            var container = new Container();
            container.Register<Foo>(Reuse.Scoped, setup: Setup.With(openResolutionScope: true));
            
            var foo = container.Resolve<Foo>();
            Assert.IsNotNull(foo);
            
            var actual = container.Resolve<IEnumerable<Foo>>();
            Assert.AreEqual(1, actual.Count());
        }

        [Test]
        public void Test_singleton_opening_scope()
        {
            var container = new Container();
            container.Register<Foo>(Reuse.Singleton, setup: Setup.With(openResolutionScope: true));
            
            var foo1 = container.Resolve<Foo>();
            Assert.IsNotNull(foo1);

            var foo2 = container.Resolve<Foo>();
            Assert.AreSame(foo1, foo2);
        }

        public class Foo {}
    }
}
