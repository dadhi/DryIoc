using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class DynamicRegistrationsTests
    {
        private IEnumerable<DynamicRegistration> GetX(FactoryType factoryType, Type serviceType, object key)
        {
            if (serviceType == typeof(X))
                return new[] { new DynamicRegistration(new ReflectionFactory(typeof(A))) };
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

        private IEnumerable<DynamicRegistration> GetManyX(FactoryType factoryType, Type serviceType, object serviceKey)
        {
            if (serviceType == typeof(X))
                return new[]
                {
                    new DynamicRegistration(new ReflectionFactory(typeof(A))), 
                    new DynamicRegistration(new ReflectionFactory(typeof(B))),
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
        public void Can_specify_to_exclude_dynamic_registration_if_there_is_a_normal_one()
        {
            var container = new Container(rules => rules.WithDynamicRegistrations(
                (_, serviceType, serviceKey) =>
                {
                    if (serviceType == typeof(X))
                        return new[] { new DynamicRegistration(new ReflectionFactory(typeof(A)), IfAlreadyRegistered.Keep) };
                    return null;
                }));

            container.Register<X, B>();

            var x = container.Resolve<X>();

            Assert.IsInstanceOf<B>(x);
        }

        [Test, Ignore]
        public void Can_validate_dynamic_registration()
        {
            var container = new Container(rules => rules.WithDynamicRegistrations(
                (_, serviceType, serviceKey) =>
                {
                    if (serviceType == typeof(X))
                        return new[] { new DynamicRegistration(new ReflectionFactory(typeof(C))) };
                    return null;
                }));

            var x = container.Resolve<X>();

            Assert.IsInstanceOf<A>(x);
        }

        public class X { }
        public class A : X { }
        public class B : X { }
        public class C { }
    }
}
