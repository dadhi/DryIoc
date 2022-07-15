using NUnit.Framework;
using System.Collections.Generic;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue504_Add_IDictionary_wrapper : ITest
    {
        public int Run()
        {
            Test_Resolve();
            Test_Resolve_with_LazyEnumerable();
            Test_Inject();
            Test_Inject_with_LazyEnumerable();
            return 4;
        }

        [Test]
        public void Test_Resolve()
        {
            var c = new Container();

            c.Register<I, A>(serviceKey: "A");
            c.Register<I, B>();
            c.Register<I, C>(serviceKey: "C");

            var d2 = c.Resolve<IDictionary<string, I>>();
            Assert.AreEqual(2, d2.Count);
            Assert.IsInstanceOf<A>(d2["A"]);
            Assert.IsInstanceOf<C>(d2["C"]);

            var d3 = c.Resolve<IDictionary<object, I>>();
            Assert.AreEqual(3, d3.Count);
            Assert.IsInstanceOf<A>(d3["A"]);
            Assert.IsInstanceOf<B>(d3[DefaultKey.Value]);
            Assert.IsInstanceOf<C>(d3["C"]);
        }

        [Test]
        public void Test_Resolve_with_LazyEnumerable()
        {
            var c = new Container(Rules.Default.WithResolveIEnumerableAsLazyEnumerable());

            c.Register<I, A>(serviceKey: "A");
            c.Register<I, B>();
            c.Register<I, C>(serviceKey: "C");

            var d2 = c.Resolve<IDictionary<string, I>>();
            Assert.AreEqual(2, d2.Count);
            Assert.IsInstanceOf<A>(d2["A"]);
            Assert.IsInstanceOf<C>(d2["C"]);

            var d3 = c.Resolve<IDictionary<object, I>>();
            Assert.AreEqual(3, d3.Count);
            Assert.IsInstanceOf<A>(d3["A"]);
            Assert.IsInstanceOf<B>(d3[DefaultKey.Value]);
            Assert.IsInstanceOf<C>(d3["C"]);
        }

        [Test]
        public void Test_Inject()
        {
            var c = new Container();

            c.Register<I, A>(serviceKey: "A");
            c.Register<I, B>();
            c.Register<I, C>(serviceKey: "C");
            c.Register<D>();

            var d = c.Resolve<D>();
            var d3 = d.Dict;
            Assert.AreEqual(3, d3.Count);
            Assert.IsInstanceOf<A>(d3["A"]);
            Assert.IsInstanceOf<B>(d3[DefaultKey.Value]);
            Assert.IsInstanceOf<C>(d3["C"]);
        }

        [Test]
        public void Test_Inject_with_LazyEnumerable()
        {
            var c = new Container(Rules.Default.WithResolveIEnumerableAsLazyEnumerable());

            c.Register<I, A>(serviceKey: "A");
            c.Register<I, B>();
            c.Register<I, C>(serviceKey: "C");
            c.Register<D>();

            var d = c.Resolve<D>();
            var d3 = d.Dict;
            Assert.AreEqual(3, d3.Count);
            Assert.IsInstanceOf<A>(d3["A"]);
            Assert.IsInstanceOf<B>(d3[DefaultKey.Value]);
            Assert.IsInstanceOf<C>(d3["C"]);
        }

        interface I {}
        class A : I {}
        class B : I {}
        class C : I {}
        class D
        {
            public readonly IDictionary<object, I> Dict;
            public D(IDictionary<object, I> dict) => Dict = dict;
        }
    }
}
