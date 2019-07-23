using System;
using System.Linq.Expressions;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue153_ContextDependentResolutionOnlyWorksForTheVeryFirstContext
    {
        [Test]
        public void Context_based_setup_should_work_for_different_context()
        {
            var c = new Container();
            c.Register<C>();
            c.Register<D>();
            c.Register(Made.Of(() => new Str { S = Arg.Index<string>(0) }, r => r.Parent.ImplementationType.Name));

            var x = c.Resolve<C>();
            var y = c.Resolve<D>();

            var cExpr = c.Resolve<LambdaExpression>(typeof(C));
            StringAssert.DoesNotContain("Resolve(", cExpr.ToString());

            var dExpr = c.Resolve<LambdaExpression>(typeof(D));
            StringAssert.DoesNotContain("Resolve(", dExpr.ToString());

            Assert.AreEqual("C", x.S.S);
            Assert.AreEqual("D", y.S.S);
        }

        class Str { public string S { get; set; } }

        class C
        {
            public readonly Str S;
            public C(Str s) { S = s; }
        }

        class D
        {
            public readonly Str S;
            public D(Str s) { S = s; }
        }

        [Test]
        public void Lazy_expression_should_not_be_excessively_compiled()
        {
            var c = new Container();
            c.Register<A>();
            c.Register<B>();
            c.Register(Made.Of(() => new Foo { S = Arg.Index<string>(0) }, r => r.Parent.ToString()));

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
