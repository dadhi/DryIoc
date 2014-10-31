using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    // NOTE: Tests fails because Service<int> factory is registered after resolve and not unregistered with its generic provider.
    [Ignore("Not fixed yet")]
    public class Issue45_UnregisterOpenGenericAfterItWasResolvedOnce
    {
        [Test]
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

        [Test]
        public void Unregister_wrapper_after_it_was_resolved_once()
        {
            var container = new Container();
            container.Register<Service>();
            var lazyService = container.Resolve<Func<Service>>();
            Assert.NotNull(lazyService());

            container.Unregister(typeof(Func<>), factoryType: FactoryType.Wrapper);
            container = container.WipeCache();

            Assert.Throws<ContainerException>(() =>
                container.Resolve<Func<Service>>());
        }

        public class Service<T> { }

        public class Service { }
    }
}
