using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue315_Decorator_of_wrapper : ITest
    {
        public int Run()
        {
            Test(true);
            Test(false);
            Collection_decorator_can_be_used_with_item_decorators(true);
            Collection_decorator_can_be_used_with_item_decorators(false);
            return 2;
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Test(bool lazyEnumerable)
        {
            IContainer c = new Container();
            if (lazyEnumerable)
                c = c.With(rules => rules.WithResolveIEnumerableAsLazyEnumerable());

            c.Register<A>();
            c.Register<A, A1>();

            c.Register(Made.Of(() => Arg.Of<IEnumerable<A>>().Reverse()),
                setup: Setup.Decorator);

            var a1 = c.Resolve<IEnumerable<A>>().First();
            
            Assert.IsInstanceOf<A1>(a1);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Collection_decorator_can_be_used_with_item_decorators(bool lazyEnumerable)
        {
            IContainer c = new Container();
            if (lazyEnumerable)
                c = c.With(rules => rules.WithResolveIEnumerableAsLazyEnumerable());

            c.Register<A>();
            c.Register<A, A1>();
            c.Register<A, D>(setup: Setup.Decorator);

            c.Register(Made.Of(() => Arg.Of<IEnumerable<A>>().Reverse()),
                setup: Setup.Decorator);

            var a1 = c.Resolve<IEnumerable<A>>().First();

            Assert.IsInstanceOf<D>(a1);
            Assert.IsInstanceOf<A1>(((D)a1).A);
        }

        public class A { }
        public class A1 : A { }

        public class D : A
        {
            public A A { get; private set; }

            public D(Func<A> getA)
            {
                A = getA();
            }
        }
    }
}
