using System.Linq;
using DryIoc.ImTools;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue228_Updated_DryIoc_from_4_to_4_1_in_Unity_Engine_project_keyed_register_resolve_wont_work_anymore : ITest
    {
        public int Run()
        {
            // For_multiple_same_key_registrations_all_should_be_returned();
            Should_be_able_to_get_two_keyed_registrations();
            return 2;
        }

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

        [Test]
        public void For_multiple_same_key_registrations_all_should_be_returned()
        {
            var container = new Container(Rules.Default.WithMultipleSameServiceKeyForTheServiceType());

            container.Register<Iface, A>(serviceKey: Keys.A);
            container.Register<Iface, B>(serviceKey: Keys.A);

            var ab = container.Resolve<Iface[]>();
            Assert.AreEqual(2, ab.Length);
            Assert.IsInstanceOf<A>(ab[0]);
            Assert.IsInstanceOf<B>(ab[1]);

            ab = container.Resolve<Iface[]>(serviceKey: Keys.A);
            Assert.AreEqual(2, ab.Length);
            Assert.IsInstanceOf<A>(ab[0]);
            Assert.IsInstanceOf<B>(ab[1]);

            ab = container.ResolveMany<Iface>().ToArray();
            Assert.AreEqual(2, ab.Length);
            Assert.IsInstanceOf<A>(ab[0]);
            Assert.IsInstanceOf<B>(ab[1]);

            ab = container.ResolveMany<Iface>(serviceKey: Keys.A).ToArray();
            Assert.AreEqual(2, ab.Length);
            Assert.IsInstanceOf<A>(ab[0]);
            Assert.IsInstanceOf<B>(ab[1]);

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<Iface>(Keys.A));

            Assert.AreEqual(Error.NameOf(Error.UnableToResolveUnknownService), Error.NameOf(ex.Error));

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