using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue4_Rule_for_Func_and_Lazy_to_be_resolved_even_without_requested_service_registered
    {
        [Test]
        public void Func_dependency_of_not_registered_service_should_work()
        {
            var c = new Container(rules => rules.WithFuncDoesNotNeedRegistration());

            c.Register<A>();
            var a = c.Resolve<A>();

            c.Register<B>();

            var b = a.LateResolveB();
            Assert.IsNotNull(b);
        }

        [Test]
        public void Lazy_resolve_of_not_registered_service_should_work()
        {
            var c = new Container(rules => rules.WithFuncDoesNotNeedRegistration());

            var b = c.Resolve<Lazy<B>>();
            Assert.IsNotNull(b);
        }

        public class A
        {
            private readonly Func<B> _func;

            public A(Func<B> func)
            {
                _func = func;
            }

            public B LateResolveB() => _func();
        }

        public class B {}
    }
}
