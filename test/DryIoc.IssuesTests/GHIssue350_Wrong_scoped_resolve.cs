using NUnit.Framework;
using FastExpressionCompiler.LightExpression;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue350_Wrong_scoped_resolve
    {
        [Test]
        public void TheBug()
        {
            var container = new Container(rules => rules
                .WithMicrosoftDependencyInjectionRules()
                .WithFuncAndLazyWithoutRegistration());

            container.Register<A>();
            container.Register<B>();
            container.Register<S, S1>(Reuse.ScopedTo("FirstScope"));
            container.Register<S, S2>(Reuse.ScopedTo("SecondScope"));

            using (var scope1 = container.OpenScope("FirstScope"))
            {
                var a = scope1.Resolve<A>();
                Assert.IsInstanceOf<S1>(a.B.S);
            }

            using (var scope2 = container.OpenScope("SecondScope"))
            {
                // throws: DryIoc.ContainerException : code: Error.NoMatchedScopeFound; 
                // message: Unable to find matching scope with name "FirstScope" starting from 
                // the current scope {Name=SecondScope}.
                // var aExpr = scope2.Resolve<LambdaExpression>(typeof(A));

                var a = scope2.Resolve<A>();
                Assert.IsInstanceOf<S2>(a.B.S);
            }
        }

        class A
        {
            public B B { get; }
            public A(B b) => B = b;
        }

        class B
        {
            public S S { get; }
            public B(S s) => S = s;
        }

        interface S {}
        class S1 : S {}
        class S2 : S {}
    }
}
