using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue66_Cannot_instantiate_DictionaryT
    {
        [Test]
        public void AutoConcreteTypeResolution_should_be_able_to_create_with_default_ctor()
        {
            var container = new Container(rules => rules
                .WithAutoConcreteTypeResolution());

            var dict = container.Resolve<Dictionary<Type, object>>();

            Assert.IsNotNull(dict);
        }

        [Test]
        public void WithConcreteTypeDynamicRegistrations_should_be_able_to_create_with_default_ctor()
        {
            var container = new Container(rules => rules
                .WithConcreteTypeDynamicRegistrations());

            var dict = container.Resolve<Dictionary<Type, object>>();

            Assert.IsNotNull(dict);
        }

        [Test]
        public void Should_throw_on_nested_unresolved_dep()
        {
            var container = new Container(rules => rules
                .WithConcreteTypeDynamicRegistrations());

            Assert.Throws<ContainerException>(() =>
                container.Resolve<A>());
        }

        [Test]
        public void Should_resolve_dep()
        {
            var container = new Container(rules => rules
                .WithConcreteTypeDynamicRegistrations());

            container.Register<I, X>();

            var a = container.Resolve<A>();
            Assert.IsNotNull(a.B.I);
        }

        [Test]
        public void Should_call_resolve_for_parameter_no_more_than_once()
        {
            var container = new Container(rules => rules
                .WithConcreteTypeDynamicRegistrations());

            container.Register<I, X>();

            var d = container.Resolve<D>();
            Assert.IsNotNull(d.I);
        }

        public class A
        {
            public B B { get; }
            public A(B b)
            {
                B = b;
            }
        }

        public class B
        {
            public I I { get; }
            public B(I i)
            {
                I = i;
            }
        }

        public interface I { }
        public class X : I { }

        public class D
        {
            public I I { get; }
            public D() { }
            public D(I i)
            {
                I = i;
            }
        }
    }
}
