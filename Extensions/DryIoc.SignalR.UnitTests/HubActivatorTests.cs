using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using NUnit.Framework;

namespace DryIoc.SignalR.UnitTests
{
    [TestFixture]
    public class HubActivatorTests
    {
        private readonly IContainer _container;

        public HubActivatorTests()
        {
            _container = new Container();
        }

        [Test]
        public void Can_resolve_hub_activator()
        {
            var activator = _container.WithSignalR().Resolve<IHubActivator>();

            Assert.IsInstanceOf<DryIocHubActivator>(activator);
        }

        [Test, Ignore("fails")]
        public void Confirmed_that_hub_activator_creates_intercepted_proxy()
        {
            var container = _container.WithSignalR();

            container.Register<AHub>(Reuse.InCurrentScope);
            var hubDescriptor = new HubDescriptor { HubType = typeof(AHub) };

            var activator = new DryIocHubActivator(container);
            var hub = activator.Create(hubDescriptor);

            Assert.AreEqual("HubToCloseScopeOnDisposeInterceptor", hub.GetType().Name);
        }

        public class AHub : Hub { }
    }
}