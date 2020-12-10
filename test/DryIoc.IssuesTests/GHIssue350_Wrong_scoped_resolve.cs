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
                var a = scope2.Resolve<A>();
                Assert.IsInstanceOf<S2>(a.B.S);
            }
        }

        [Test]
        public void TheBug_without_MS_DI_rules()
        {
            var container = new Container(rules => rules
                // .WithMicrosoftDependencyInjectionRules()
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
                var a = scope2.Resolve<A>();
                Assert.IsInstanceOf<S2>(a.B.S);
            }
        }

        [Test]
        public void TheBug_simplified()
        {
            var container = new Container(rules => rules
                .WithMicrosoftDependencyInjectionRules()
                .WithFuncAndLazyWithoutRegistration());

            container.Register<S, S1>(Reuse.ScopedTo("FirstScope"));
            container.Register<S, S2>(Reuse.ScopedTo("SecondScope"));

            using (var scope1 = container.OpenScope("FirstScope"))
            {
                var a = scope1.Resolve<S>();
                Assert.IsInstanceOf<S1>(a);
            }

            using (var scope2 = container.OpenScope("SecondScope"))
            {
                var a = scope2.Resolve<S>();
                Assert.IsInstanceOf<S2>(a);
            }
        }

        [Test]
        public void TheBug_simplified_without_MS_DI_rules()
        {
            var container = new Container(rules => rules
                // .WithMicrosoftDependencyInjectionRules()
                .WithFuncAndLazyWithoutRegistration());

            container.Register<S, S1>(Reuse.ScopedTo("FirstScope"));
            container.Register<S, S2>(Reuse.ScopedTo("SecondScope"));

            using (var scope1 = container.OpenScope("FirstScope"))
            {
                var a = scope1.Resolve<S>();
                Assert.IsInstanceOf<S1>(a);
            }

            using (var scope2 = container.OpenScope("SecondScope"))
            {
                var a = scope2.Resolve<S>();
                Assert.IsInstanceOf<S2>(a);
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
