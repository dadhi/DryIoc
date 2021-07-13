using System.Linq;
using DryIoc.ImTools;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    public class Test
    {
        [Test]
        public void Should_be_able_to_get_two_keyed_registrations()
        {
            var container = new Container();

            container.Register<Iface, A>(serviceKey: Keys.A);
            container.Register<Iface, B>(serviceKey: Keys.B);

            var ab = container.Resolve<Iface[]>();
            Assert.AreEqual(2, ab.Length);

            var a = container.Resolve<Iface>(Keys.A);
            Assert.IsInstanceOf<A>(a);

            var rs = container.GetServiceRegistrations().Where(x => x.ServiceType == typeof(Iface)).ToArray();
            Assert.AreEqual(2, rs.Length);

            var fs = container.GetRegisteredFactories(typeof(Iface), null, FactoryType.Service).ToArray();
            Assert.AreEqual(2, fs.Length);
        }

        public interface Iface {}

        public class A : Iface {}
        public class B : Iface {}

        public enum Keys { A, B }
    }
}