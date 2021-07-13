using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class DynamicRegistrationsTests
    {
        private IEnumerable<DynamicRegistration> GetX(Type serviceType, object key)
        {
            if (serviceType == typeof(X))
                return new[] { new DynamicRegistration(ReflectionFactory.Of(typeof(A))) };
            return null;
        }

        [Test]
        public void Can_resolve_service()
        {
            var container = new Container(rules => rules.WithDynamicRegistrations(GetX));

            var x = container.Resolve<X>();

            Assert.IsInstanceOf<A>(x);
        }

        [Test]
        public void Can_resolve_service_array()
        {
            var container = new Container(rules => rules.WithDynamicRegistrations(GetX));

            var x = container.Resolve<X[]>();

            Assert.IsInstanceOf<A>(x[0]);
        }

        [Test]
        public void Can_resolve_service_enumerable()
        {
            var container = new Container(rules => rules.WithDynamicRegistrations(GetX));

            var x = container.ResolveMany<X>().ToArray();

            Assert.IsInstanceOf<A>(x[0]);
        }

        private IEnumerable<DynamicRegistration> GetManyX(Type serviceType, object serviceKey)
        {
            if (serviceType == typeof(X))
                return new[]
                {
                    new DynamicRegistration(ReflectionFactory.Of(typeof(A))),
                    new DynamicRegistration(ReflectionFactory.Of(typeof(B))),
                };
            return null;
        }

        [Test]
        public void Can_resolve_multi_service_array()
        {
            var container = new Container(rules => rules.WithDynamicRegistrations(GetManyX));

            var xs = container.Resolve<X[]>();

            Assert.AreEqual(2, xs.Length);
            Assert.IsInstanceOf<A>(xs[0]);
            Assert.IsInstanceOf<B>(xs[1]);
        }

        [Test]
        public void Can_exclude_dynamic_registration_if_there_is_a_normal_one()
        {
            var container = new Container(rules => rules.WithDynamicRegistrations(
                (serviceType, serviceKey) =>
                {
                    if (serviceType == typeof(X))
                        return new[] { new DynamicRegistration(ReflectionFactory.Of(typeof(A)), IfAlreadyRegistered.Keep) };
                    return null;
                }));

            container.Register<X, B>();

            var x = container.Resolve<X>();

            Assert.IsInstanceOf<B>(x);
        }

        public class F<T> 
        {
            public readonly T X;
            public F(T x) => X = x;
        }

        [Test]
        public void Can_replace_normal_by_dynamic_registration()
        {
            var container = new Container(rules => rules.WithDynamicRegistrations(
                (serviceType, serviceKey) =>
                {
                    if (serviceType == typeof(X))
                        return new[] { new DynamicRegistration(ReflectionFactory.Of(typeof(A)), IfAlreadyRegistered.Replace) };
                    return null;
                }));

            container.Register<X, B>();

            var x = container.Resolve<X>();

            Assert.IsInstanceOf<A>(x);
        }

        [Test]
        public void Can_replace_normal_with_dynamic_and_dynamic_with_dynamic_registration()
        {
            var container = new Container(rules => rules.WithDynamicRegistrations(
                (serviceType, serviceKey) => serviceType == typeof(X)
                    ? new[] { new DynamicRegistration(ReflectionFactory.Of(typeof(A)), IfAlreadyRegistered.Replace) }
                    : null,
                (serviceType, serviceKey) => serviceType == typeof(X)
                    ? new[] { new DynamicRegistration(ReflectionFactory.Of(typeof(A)), IfAlreadyRegistered.Replace) }
                    : null));

            container.Register<X, B>();

            var x = container.Resolve<X>();

            Assert.IsInstanceOf<A>(x);
        }

        [Test]
        public void Can_append_new_implementation_via_dynamic_registration()
        {
            var container = new Container(rules => rules.WithDynamicRegistrations(
                (serviceType, serviceKey) => serviceType == typeof(X)
                     ? new[] { new DynamicRegistration(ReflectionFactory.Of(typeof(A)), IfAlreadyRegistered.AppendNewImplementation) }
                     : null,
                (serviceType, serviceKey) => serviceType == typeof(X)
                     ? new[] { new DynamicRegistration(ReflectionFactory.Of(typeof(A)), IfAlreadyRegistered.AppendNewImplementation) }
                     : null,
                (serviceType, serviceKey) => serviceType == typeof(X)
                    ? new[] { new DynamicRegistration(ReflectionFactory.Of(typeof(B)), IfAlreadyRegistered.AppendNewImplementation) }
                    : null));

            container.Register<X>();

            var xs = container.ResolveMany<X>().Select(x => x.GetType());

            CollectionAssert.AreEquivalent(new[] { typeof(X), typeof(A), typeof(B) }, xs);
        }

        [Test]
        public void Can_append_non_keyed_via_dynamic_registration()
        {
            var container = new Container(rules => rules.WithDynamicRegistrations(
                (serviceType, serviceKey) => serviceType == typeof(X)
                     ? new[] { new DynamicRegistration(ReflectionFactory.Of(typeof(A))) }
                     : null,
                (serviceType, serviceKey) => serviceType == typeof(X)
                     ? new[] { new DynamicRegistration(ReflectionFactory.Of(typeof(A))) }
                     : null,
                (serviceType, serviceKey) => serviceType == typeof(X)
                     ? new[] { new DynamicRegistration(ReflectionFactory.Of(typeof(B))) }
                     : null,
                (serviceType, serviceKey) => serviceType == typeof(X)
                    ? new[] { new DynamicRegistration(ReflectionFactory.Of(typeof(B)), serviceKey: "b") }
                    : null));

            container.Register<X>();

            var xs = container.ResolveMany<X>().Select(x => x.GetType());

            CollectionAssert.AreEquivalent(new[] { typeof(X), typeof(A), typeof(A), typeof(B), typeof(B) }, xs);
        }

        [Test]
        public void Will_keep_first_keyed_registration_by_default()
        {
            var container = new Container(rules => rules.WithDynamicRegistrations(
                (serviceType, serviceKey) => serviceType == typeof(X)
                    ? new[] { new DynamicRegistration(ReflectionFactory.Of(typeof(A)), serviceKey: "a") }
                    : null,
                (serviceType, serviceKey) => serviceType == typeof(X)
                    ? new[] { new DynamicRegistration(ReflectionFactory.Of(typeof(B)), serviceKey: "a") }
                    : null));

            container.Register<X>(serviceKey: "a");

            var a = container.Resolve<X>("a");

            Assert.IsInstanceOf<X>(a);
        }

        [Test]
        public void Can_replace_keyed_registration()
        {
            var container = new Container(rules => rules.WithDynamicRegistrations(
                (serviceType, serviceKey) => serviceType == typeof(X)
                 ? new[] { new DynamicRegistration(ReflectionFactory.Of(typeof(A)), IfAlreadyRegistered.Replace, "a") }
                 : null,
                (serviceType, serviceKey) => serviceType == typeof(X)
                    ? new[] { new DynamicRegistration(ReflectionFactory.Of(typeof(B)), IfAlreadyRegistered.Replace, "a") }
                    : null));

            container.Register<X>(serviceKey: "a");

            var a = container.Resolve<X>("a");

            Assert.IsInstanceOf<B>(a);
        }

        [Test]
        public void Can_validate_dynamic_registration()
        {
            var container = new Container(rules => rules.WithDynamicRegistrations(
                (serviceType, serviceKey) =>
                {
                    if (serviceType == typeof(X))
                        return new[] { new DynamicRegistration(ReflectionFactory.Of(typeof(C))) };
                    return null;
                }));

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<X>());

            Assert.AreEqual(Error.NameOf(Error.RegisteringImplementationNotAssignableToServiceType), ex.ErrorName);
        }

        [Test]
        public void Should_exclude_decorators_from_dynamic_services()
        {
            var container = new Container()
                .WithAutoFallbackDynamicRegistrations(
                    (t, k) => new[] { typeof(D) },
                    type => type.ToFactory(setup: Setup.Decorator));

            container.Register<X>();

            var d = container.Resolve<D>(IfUnresolved.ReturnDefault);

            Assert.IsNull(d);
        }

        [Test]
        public void Can_register_dynamic_decorator()
        {
            var container = new Container()
                .WithAutoFallbackDynamicRegistrations(
                    (t, k) => new[] { typeof(D) },
                    type => type.ToFactory(setup: Setup.Decorator));

            container.Register<X, A>();

            var x = container.Resolve<X>();

            Assert.IsInstanceOf<D>(x);
            Assert.IsInstanceOf<A>(((D)x).X);
        }

        public class X { }
        public class A : X { }
        public class B : X { }
        public class C { }

        public class D : X
        {
            public X X { get; private set; }

            public D(X x)
            {
                X = x;
            }
        }

        [Test]
        public void Issue_521()
        {
            var container = new Container(r => r.WithConcreteTypeDynamicRegistrations());

            container.Register<SomeClass>();
            var instance = container.Resolve<SomeClass>();

            Assert.IsInstanceOf<GenericClass<int>>(instance.GenericClass);
        }

        public class SomeClass
        {
            public readonly GenericClass<int> GenericClass;

            public SomeClass(GenericClass<int> genericClass)
            {
                GenericClass = genericClass;
            }
        }

        public class GenericClass<T>
        {
        }
    }
}
