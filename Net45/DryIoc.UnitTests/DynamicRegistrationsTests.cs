using System;
using System.Collections.Generic;
using System.Linq;
using ImTools;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class DynamicRegistrationsTests
    {
        private IEnumerable<KV<object, Factory>> GetX(FactoryType factoryType, Type serviceType, object key)
        {
            if (serviceType == typeof(X))
                return new[] {KV.Of<object, Factory>(null, new ReflectionFactory(typeof(A)))};
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

        private IEnumerable<KV<object, Factory>> GetManyX(FactoryType factoryType, Type serviceType, object serviceKey)
        {
            if (serviceType == typeof(X))
                return new[]
                {
                    // the keys should be unique
                    KV.Of<object, Factory>(1, new ReflectionFactory(typeof(A))),
                    KV.Of<object, Factory>(2, new ReflectionFactory(typeof(B))),
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

        public class X { }
        public class A : X { }
        public class B : X { }
    }
}
