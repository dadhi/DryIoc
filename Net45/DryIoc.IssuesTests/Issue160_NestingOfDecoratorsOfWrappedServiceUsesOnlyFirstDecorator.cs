using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue160_NestingOfDecoratorsOfWrappedServiceUsesOnlyFirstDecorator
    {
        [Test]
        public void Decorate_Func_and_Lazy_in_chain()
        {
            var c = new Container();
            c.Register<I, A>();
            c.Register<I, F>(setup: Setup.Decorator);
            c.Register<I, L>(setup: Setup.Decorator);

            var l = (L)c.Resolve<I>();
            var f = (F)l.Lazy.Value;
            Assert.IsInstanceOf<A>(f.Func());
        }

        interface I { }

        class A : I { }
        class F : I
        {
            public Func<I> Func { get; set; }
            public F(Func<I> func)
            {
                Func = func;
            }
        }
        class L : I
        {
            public Lazy<I> Lazy { get; set; }
            public L(Lazy<I> lazy)
            {
                Lazy = lazy;
            }
        }
    }
}
