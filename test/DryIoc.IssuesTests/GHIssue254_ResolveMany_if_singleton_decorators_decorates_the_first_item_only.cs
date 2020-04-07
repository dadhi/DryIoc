using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue254_ResolveMany_if_singleton_decorators_decorates_the_first_item_only
    {
        [Test]
        public void Test_RegisterDelegate_with_Decorator()
        {
            var c = new Container();

            c.Register<I, A>(Reuse.Singleton);
            c.Register<I, B>(Reuse.Singleton);

            c.RegisterDelegate<I, I>(i => new D(i), setup: Setup.Decorator);

            var ii = c.ResolveMany<I>().ToArray();

            Assert.IsInstanceOf<A>(((D)ii[0]).Inner);
            Assert.IsInstanceOf<B>(((D)ii[1]).Inner);
        }

        [Test][Ignore("todo: fixme")]
        public void Test_RegisterDelegate_with_singleton_Decorator()
        {
            var c = new Container();

            c.Register<I, A>(Reuse.Singleton);
            c.Register<I, B>(Reuse.Singleton);

            c.RegisterDelegate<I, I>(i => new D(i), Reuse.Singleton,  Setup.Decorator);

            var ii = c.ResolveMany<I>().ToArray();

            Assert.IsInstanceOf<A>(((D)ii[0]).Inner);
            Assert.IsInstanceOf<B>(((D)ii[1]).Inner);
        }

        [Test][Ignore("todo: fixme")]
        public void Test_RegisterDelegateDecorator_with_useDecorateeReuse_set_to_true_by_convention()
        {
            var c = new Container();

            c.Register<I, A>(Reuse.Singleton);
            c.Register<I, B>(Reuse.Singleton);

            c.RegisterDelegateDecorator<I>(_ => i => new D(i));

            var ii = c.ResolveMany<I>().ToArray();

            Assert.IsInstanceOf<A>(((D)ii[0]).Inner);
            Assert.IsInstanceOf<B>(((D)ii[1]).Inner);
        }

        interface I { }
        class A : I { }
        class B : I { }
        class D : I
        {
            public D(I i) { Inner = i; }

            public I Inner { get; }
        }
    }
}
