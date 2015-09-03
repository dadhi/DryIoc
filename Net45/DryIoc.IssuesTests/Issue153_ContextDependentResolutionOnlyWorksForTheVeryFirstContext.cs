using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue153_ContextDependentResolutionOnlyWorksForTheVeryFirstContext
    {
        [Test]
        public void Lazy_expression_should_not_be_exessively_compiled()
        {
            var c = new Container();
            c.Register<A>();
            c.Register<B>();
            c.Register(
                Made.Of(() => new Foo { S = Arg.Index<string>(0) }, r => r.Parent.ToString()),
                setup: Setup.With(cacheFactoryExpression: false));

            var y = c.Resolve<B>();
            var x = c.Resolve<A>();
            var x2 = c.Resolve<A>();
            var x3 = c.Resolve<A>();

            var s = y.Foo.Value.S;
            s = x.Foo.Value.S;
            s = x2.Foo.Value.S;
            s = x3.Foo.Value.S;
        }

        public class Foo { public string S; }

        public class A
        {
            public Lazy<Foo> Foo;
            public A(Lazy<Foo> foo) { Foo = foo; }
        }

        public class B
        {
            public Lazy<Foo> Foo;
            public B(Lazy<Foo> foo) { Foo = foo; }
        }
    }
}
