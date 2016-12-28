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

            container.UseInstance("a", serviceKey: "namea");
            container.UseInstance("b", serviceKey: "nameb");

            container.Register<IClient>(Made.Of(
                x => ServiceInfo.Of<ClientFactory>(), 
                f => f.CreateClient(Arg.Of<string>("namea"))), 
                serviceKey: DepKind.A);

            container.Register<IClient, ClientDecorator>(setup: Setup.Decorator, 
                made: Made.Of(() => new ClientDecorator(Arg.Of<IClient>(DepKind.A))));

            var client = container.Resolve<IClient>(DepKind.A);

            Assert.IsInstanceOf<ClientDecorator>(client);
            Assert.AreEqual("a", ((Client)((ClientDecorator)client).client).name);
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
