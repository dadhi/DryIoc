using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue508_SelectLastRegisteredFactory_and_resolving_collection_of_open_generic_isnot_working_as_intended
    {
        [Test]
        public void ResolvesDifferentInstancesForServiceWhenResolvingEnumerable()
        {
            var c = new Container(r => r.WithFactorySelector(Rules.SelectLastRegisteredFactory()));

            c.Register(typeof(IFoo<>), typeof(Foo<>), Reuse.Singleton);
            c.Register(typeof(IFoo<>), typeof(Foo<>), Reuse.Singleton);
            c.Register(typeof(IFoo<>), typeof(Foo<>), Reuse.Singleton);

            var foos = c.Resolve<IFoo<int>[]>();

            Assert.AreEqual(3, foos.Length);

            Assert.AreNotSame(foos[0], foos[1]);
            Assert.AreNotSame(foos[1], foos[2]);

            var lastFoo = c.Resolve<IFoo<int>>();
            Assert.AreSame(lastFoo, foos[2]);
        }

        interface IFoo<T> { }
        class Foo<T> : IFoo<T> { }
    }
}
