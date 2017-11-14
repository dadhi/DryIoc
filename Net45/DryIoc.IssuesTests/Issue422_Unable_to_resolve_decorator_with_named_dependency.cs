using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue422_Unable_to_resolve_decorator_with_named_dependency
    {
        [Test]
        public void Test()
        {
            var container = new Container();

            container.Register<ClientFactory>();

            // One with condition to be resolved under ControllerA
            container.Register(Made.Of(
                x => ServiceInfo.Of<ClientFactory>(), 
                f => f.CreateClient("a")), 
                setup: Setup.With(condition: r => r.Parent.Any(p => p.ImplementationType == typeof(ControllerA))));

            // Another with condition to be resolved under ControllerB
            container.Register(Made.Of(
                x => ServiceInfo.Of<ClientFactory>(),
                f => f.CreateClient("b")),
                setup: Setup.With(condition: r => r.Parent.Any(p => p.ImplementationType == typeof(ControllerB))));

            container.Register<IClient, ClientDecorator>(setup: Setup.Decorator, 
                made: Made.Of(() => new ClientDecorator(Arg.Of<IClient>())));

            container.Register<ControllerA>();
            container.Register<ControllerB>();

            var controllerA = container.Resolve<ControllerA>();
            var controllerB = container.Resolve<ControllerB>();

            Assert.IsInstanceOf<ClientDecorator>(controllerA.client);
            Assert.AreEqual("a", ((Client)((ClientDecorator)controllerA.client).client).name);

            Assert.IsInstanceOf<ClientDecorator>(controllerB.client);
            Assert.AreEqual("b", ((Client)((ClientDecorator)controllerB.client).client).name);
        }

        public class ControllerA
        {
            public readonly IClient client;

            public ControllerA(IClient client)
            {
                this.client = client;
            }
        }

        public class ControllerB
        {
            public readonly IClient client;

            public ControllerB(IClient client)
            {
                this.client = client;
            }
        }

        public interface IClient { }

        public class Client : IClient
        {
            public string name { get; set; }

            public Client(string name)
            {
                this.name = name;
            }
        }

        public class ClientDecorator : IClient
        {
            public readonly IClient client;

            public ClientDecorator(IClient client)
            {
                this.client = client;
            }
        }

        public class ClientFactory
        {
            public IClient CreateClient(string name)
            {
                return new Client(name);
            }
        }

        public enum DepKind { A, B, C }
    }
}
