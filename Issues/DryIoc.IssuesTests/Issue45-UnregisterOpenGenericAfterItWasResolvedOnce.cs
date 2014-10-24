using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue45_UnregisterOpenGenericAfterItWasResolvedOnce
    {
        [Test]
        [Ignore("Not fixed yet")]// TODO: Tests fails because Service<int> factory is registered after resolve and not unregistered with its generic provider.
        public void Unregister_open_generic_after_it_was_resolved_once()
        {
            var container = new Container();
            container.Register(typeof(Service<>));
            var service = container.Resolve<Service<int>>();
            Assert.NotNull(service);

            container.Unregister(typeof(Service<>));
            container = container.WipeCache();

            Assert.Throws<ContainerException>(() =>
                container.Resolve<Service<int>>());
        }

        public class Service<T> { }
    }
}
