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
            _container = new Container().WithSignalR();
        }

        [Test]
        public void Can_resolve_hub_activator()
        {
            var activator = _container.Resolve<IHubActivator>();

            Assert.IsInstanceOf<DryIocHubActivator>(activator);
        }

        [Test]
        public void Creating_hub_opens_scope()
        {
            _container.Register<AHub>(Reuse.InCurrentScope);

            var hubDescriptor = new HubDescriptor { HubType = typeof(AHub) };
            var hubActivator = new DryIocHubActivator(_container);

            using (hubActivator.Create(hubDescriptor))
                Assert.IsNotNull(_container.ScopeContext.GetCurrentOrDefault());
        }

        [Test]
        public void Disposing_hub_closes_scope()
        {
            _container.Register<AHub>(Reuse.InCurrentScope);

            var hubDescriptor = new HubDescriptor { HubType = typeof(AHub) };
            var hubActivator = new DryIocHubActivator(_container);
            var hub = hubActivator.Create(hubDescriptor);

            hub.Dispose();

            Assert.IsNull(_container.ScopeContext.GetCurrentOrDefault());
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


        public class AHub : Hub { }

        public interface IBuggy {}

        public class Buggy : IBuggy {}

        public class NewBuggy : IBuggy {}
    }
}