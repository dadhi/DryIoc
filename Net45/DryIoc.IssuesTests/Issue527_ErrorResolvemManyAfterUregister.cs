using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue527_ErrorResolvemManyAfterUregister
    {
        [Test]
        public void Test()
        {
            var container = new Container();

            container.Register<A>();
            container.Register<A>(serviceKey: 1);
            container.Register<A>(serviceKey: 2);
            var resMany = container.ResolveMany<A>(); // resolve 3 items
            Assert.AreEqual(3, resMany.Count());

            container.Unregister<A>(serviceKey: 1);

            var resMany2 = container.ResolveMany<A>();// resolve error
            Assert.AreEqual(2, resMany2.Count());

            container.Register<A>(serviceKey: 1); // register again
            var resMany3 = container.ResolveMany<A>();// no error, result 3 items
            Assert.AreEqual(3, resMany3.Count());

        }

        class A { }
    }
}
