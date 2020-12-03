using NUnit.Framework;
using System;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue347_The_AsResolutionCall_option_and_or_WithFuncAndLazyWithoutRegistration_rule_are_not_respected
    {
        [Test, Ignore("fixme")]
        public void RecursiveDependencyIssue_with_asResolutionCall()
        {
            var container = new Container();
            container.Register<A>(Reuse.Singleton);
            container.Register<B>();
            container.Register<C>(setup: Setup.With(asResolutionCall: true));

            var serviceA = container.Resolve<A>();
            Assert.IsNotNull(serviceA);

            var actual = serviceA.Get(); 
            Assert.AreSame(actual, serviceA);
        }

        [Test, Ignore("fixme")]
        public void RecursiveDependencyIssue_WithFuncAndLazyWithoutRegistration()
        {
            var container = new Container(rules => rules.WithFuncAndLazyWithoutRegistration());
            container.Register<A>(Reuse.Singleton);
            container.Register<B>();
            container.Register<C>();

            var serviceA = container.Resolve<A>();
            Assert.IsNotNull(serviceA);

            var actual = serviceA.Get(); 
            Assert.AreSame(actual, serviceA);
        }

        [Test]
        public void RecursiveDependencyIssue_with_Lazy_instead_of_Func()
        {
            var container = new Container();
            container.Register<A>(Reuse.Singleton);
            container.Register<B, B_with_Lazy_C>();
            container.Register<C>();

            var serviceA = container.Resolve<A>();
            Assert.IsNotNull(serviceA);

            var actual = serviceA.Get(); 
            Assert.AreSame(actual, serviceA);
        }

        class A
        {
            private readonly B _b;
            public A(B b) { _b = b; }
            public A Get() { return _b.Get(); }
        }

        class B
        {
            private readonly Func<C> _c;
            public B(Func<C> c) { _c = c; }
            public virtual A Get() { return _c().Get(); }
        }

        class B_with_Lazy_C : B
        {
            private readonly Lazy<C> _c;
            public B_with_Lazy_C(Lazy<C> c) : base(null) { _c = c; }
            public override A Get() { return _c.Value.Get(); }
        }

        class C
        {
            private readonly A _a;
            public C(A a) { _a = a; }
            public A Get() { return _a; }
        }
    }
}
