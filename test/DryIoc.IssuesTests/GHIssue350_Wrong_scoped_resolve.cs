using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue350_Wrong_scoped_resolve
    {
        [Test, Ignore("fixme")]
        public void TheBug()
        {
            var container = new Container(rules => rules
                .WithMicrosoftDependencyInjectionRules()
                .WithFuncAndLazyWithoutRegistration());

            container.Register<A>();
            container.Register<B>();
            container.Register<S, S1>(Reuse.ScopedTo("FirstScope"));
            container.Register<S, S2>(Reuse.ScopedTo("SecondScope"));


            using (var scope = container.OpenScope("FirstScope"))
            {
                var a = scope.Resolve<A>();
                Assert.IsInstanceOf<S1>(a.B.S);
            }

            using (var context = container.OpenScope("SecondScope"))
            {
                // throws: DryIoc.ContainerException : code: Error.NoMatchedScopeFound; 
                // message: Unable to find matching scope with name "FirstScope" starting from 
                // the current scope {Name=SecondScope}.
                var a = context.Resolve<A>();
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
