using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using NUnit.Framework;

namespace DryIoc.SignalR.UnitTests
{
    [TestFixture]
    public class ScopeTests
    {
        [Test]
        public void Can_specify_to_not_use_ScopeContext()
        {
            var container = new Container();
            var srContainer = container.WithSignalR(scopeContext: null);
            srContainer.Register<MyHub>();
            srContainer.Register<Scoped>(Reuse.InCurrentScope);

            var hubActivator = srContainer.Resolve<IHubActivator>();
            hubActivator.Create(new HubDescriptor { HubType = typeof(MyHub) });
        }

        public class MyHub : Hub
        {
            public Scoped Scoped { get; private set; }

            public MyHub(Scoped scoped)
            {
                Scoped = scoped;
            }
        }

        public class Scoped { }
    }
}
