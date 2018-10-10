using System.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using NUnit.Framework;

namespace DryIoc.SignalR.UnitTests
{
    [TestFixture]
    public class DryIocSignalRTests
    {
        private IContainer _container;

        [SetUp]
        public void Init()
        {
            _container = new Container().WithSignalR(typeof(AHub).Assembly);
        }

        [TearDown]
        public void TearDown()
        {
            _container.Dispose();
        }

        [Test]
        public void Can_resolve_hub_activator()
        {
            var activator = _container.Resolve<IHubActivator>();

            Assert.IsInstanceOf<DryIocHubActivator>(activator);
        }

        [Test]
        public void Registered_hubs_wont_be_disposed_container()
        {
            var container = new Container();
            container.RegisterHubs(typeof(AHub));

            container.RegisterMany<Test>();
            var hub = container.Resolve<IHub>(typeof(AHub));
            container.Dispose();
            
            Assert.IsFalse(((AHub)hub).IsDisposed);
            hub.Dispose();
            Assert.IsTrue(((AHub)hub).IsDisposed);
        }

        [Test]
        public void Disposing_hub_does_not_throws_error()
        {
            var hubDescriptor = new HubDescriptor { HubType = typeof(AHub) };
            _container.Register<Test>(Reuse.InCurrentScope);

            using (var scope = _container.OpenScope())
            {
                var hubActivator = new DryIocHubActivator(scope);
                var hub = hubActivator.Create(hubDescriptor);
                hub.Dispose();
            }
        }

        [Test]
        public void Ensure_that_dependency_resolver_is_registered_in_container()
        {
            var resolver = _container.Resolve<IDependencyResolver>();

            Assert.IsInstanceOf<DryIocDependencyResolver>(resolver);
        }

        [Test]
        public void Ensure_that_resolver_returns_service_registered_in_container()
        {
            _container.Register<IBuggy, Buggy>();

            var buggy = _container.Resolve<IDependencyResolver>().GetService(typeof(IBuggy));

            Assert.IsInstanceOf<Buggy>(buggy);
        }

        [Test]
        public void Ensure_that_resolver_returns_service_registered_be_resolver()
        {
            var resolver = _container.Resolve<IDependencyResolver>();
            resolver.Register(typeof(IBuggy), () => new Buggy());
            var buggy = resolver.GetService(typeof(IBuggy));

            Assert.IsInstanceOf<Buggy>(buggy);
        }

        [Test]
        public void If_service_registered_both_in_container_and_resolver_that_container_resolution_is_preferred()
        {
            var resolver = _container.Resolve<IDependencyResolver>();

            _container.Register<IBuggy, NewBuggy>();
            resolver.Register(typeof(IBuggy), () => new Buggy());

            var buggy = resolver.GetService(typeof(IBuggy));

            Assert.IsInstanceOf<NewBuggy>(buggy);
        }

        [Test]
        public void Ensure_that_resolver_returns_all_services_registered_in_container()
        {
            _container.Register<IBuggy, Buggy>();

            var buggies = _container.Resolve<IDependencyResolver>().GetServices(typeof(IBuggy)).ToArray();

            Assert.AreEqual(1, buggies.Length);
        }

        [Test]
        public void Ensure_that_resolver_returns_all_services_registered_in_container_and_in_resolver()
        {
            _container.Register<IBuggy, Buggy>();
            var resolver = _container.Resolve<IDependencyResolver>();
            resolver.Register(typeof(IBuggy), () => new NewBuggy());

            var buggies = resolver.GetServices(typeof(IBuggy)).ToArray();

            Assert.AreEqual(2, buggies.Length);
        }

        [Test]
        public void If_no_services_registered_then_GetServices_should_return_null()
        {
            var buggies = _container.Resolve<IDependencyResolver>().GetServices(typeof(IBuggy));

            Assert.IsNull(buggies);
        }

        [Test]
        public void Disposing_resolver_should_dispose_the_container()
        {
            var resolver = _container.Resolve<IDependencyResolver>();
            resolver.Dispose();

            var ex = Assert.Throws<ContainerException>(() =>
            _container.Resolve<IDependencyResolver>());

            Assert.AreEqual(Error.ContainerIsDisposed, ex.Error);
        }

        public class AHub : Hub
        {
            public readonly Test Test;

            public bool IsDisposed { get; private set; }

            public AHub(Test test)
            {
                Test = test;
            }

            protected override void Dispose(bool disposing)
            {
                IsDisposed = true;
                base.Dispose(disposing);
            }
        }

        public class Test {}

        public interface IBuggy {}

        public class Buggy : IBuggy {}

        public class NewBuggy : IBuggy {}
    }
}