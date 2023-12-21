using NUnit.Framework;
using DryIoc.Microsoft.DependencyInjection;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue350_Wrong_scoped_resolve
    {
        [Test]
        public void TheBug_2_is_not_a_bug_and_depends_on_the_registration_order()
        {
            var container = new Container(rules =>
                DryIocAdapter.WithMicrosoftDependencyInjectionRules(rules)
                .WithFuncAndLazyWithoutRegistration());

            container.Register<A>();
            container.Register<S, S1>(Reuse.ScopedTo("FirstScope"));
            container.Register<B, B2>(Reuse.ScopedTo("SecondScope"));
            container.Register<B, B1>();

            using (var scope = container.OpenScope("FirstScope"))
            {
                // no issue
                var a = scope.Resolve<A>();
                Assert.IsInstanceOf<B1>(a.B);
                Assert.IsInstanceOf<S1>(a.B.S);
            }

            using (var context = container.OpenScope("SecondScope"))
            {
               //DryIoc.ContainerException : code: Error.NoMatchedScopeFound;
               //message: Unable to find matching scope with name "FirstScope" starting from the current scope {Name=SecondScope}.
                Assert.Throws<ContainerException>(() =>
                context.Resolve<A>());
            }
        }

        [Test]
        public void TheBug_2_without_MS_DI_rules()
        {
            var container = new Container(rules => rules
                .WithFuncAndLazyWithoutRegistration());

            container.Register<A>();
            container.Register<S, S1>(Reuse.ScopedTo("FirstScope"));
            container.Register<B, B2>(Reuse.ScopedTo("SecondScope"));
            container.Register<B, B1>();

            using (var scope = container.OpenScope("FirstScope"))
            {
                // no issue
                var a = scope.Resolve<A>();
                Assert.IsInstanceOf<B1>(a.B);
                Assert.IsInstanceOf<S1>(a.B.S);
            }

            using (var context = container.OpenScope("SecondScope"))
            {
               //DryIoc.ContainerException : code: Error.NoMatchedScopeFound;
               //message: Unable to find matching scope with name "FirstScope" starting from the current scope {Name=SecondScope}.
                var a = context.Resolve<A>();
                Assert.IsInstanceOf<B2>(a.B);
                Assert.IsNull(a.B.S);
            }
        }

        [Test]
        public void TheBug_2_change_resgitration_order()
        {
            var container = new Container(rules =>
                DryIocAdapter.WithMicrosoftDependencyInjectionRules(rules)
                .WithFuncAndLazyWithoutRegistration());

            container.Register<A>();
            container.Register<B, B1>();
            container.Register<S, S1>(Reuse.ScopedTo("FirstScope"));
            container.Register<B, B2>(Reuse.ScopedTo("SecondScope"));

            using (var scope = container.OpenScope("FirstScope"))
            {
                // no issue
                var a = scope.Resolve<A>();
                Assert.IsInstanceOf<B1>(a.B);
                Assert.IsInstanceOf<S1>(a.B.S);
            }

            using (var context = container.OpenScope("SecondScope"))
            {
               //DryIoc.ContainerException : code: Error.NoMatchedScopeFound;
               //message: Unable to find matching scope with name "FirstScope" starting from the current scope {Name=SecondScope}.
                var a = context.Resolve<A>();
                Assert.IsInstanceOf<B2>(a.B);
                Assert.IsNull(a.B.S);
            }
        }


        [Test]
        public void TheBug()
        {
            var container = new Container(rules =>
                DryIocAdapter.WithMicrosoftDependencyInjectionRules(rules)
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
            var container = new Container(rules =>
                DryIocAdapter.WithMicrosoftDependencyInjectionRules(rules)
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

        class B1 : B { public B1(S s) : base(s) {} }
        class B2 : B { public B2() : base(null) {} }

        interface S {}
        class S1 : S {}
        class S2 : S {}
    }
}
