using NSubstitute;
using NUnit.Framework;

namespace DryIoc.Samples
{
    [TestFixture]
    public class ResolveMocksForNonRegisteredServices
    {
        [Test]
        public void Resolve_mock_for_non_registered_service()
        {
            var container = new Container(Rules.Default.WithUnknownServiceResolvers(request =>
            {
                if (!request.ServiceType.IsAbstract)
                    return null; // Mock interface or abstract class only.
                return new DelegateFactory(_ => Substitute.For(new[] { request.ServiceType }, null));
            }));

            var sub = container.Resolve<INotImplementedService>();

            Assert.That(sub, Is.InstanceOf<INotImplementedService>());
        }

        [Test]
        public void Inject_mock_for_non_registered_service()
        {
            var container = new Container(Rules.Default.WithUnknownServiceResolvers(request =>
            {
                if (!request.ServiceType.IsAbstract)
                    return null; // Mock interface or abstract class only.
                return new DelegateFactory(_ => Substitute.For(new[] { request.ServiceType }, null));
            }));

            container.Register<TestClient>();
            var client = container.Resolve<TestClient>();

            Assert.That(client.Service, Is.InstanceOf<INotImplementedService>());
        }
    }

    public interface INotImplementedService { }

    public class TestClient
    {
        public INotImplementedService Service { get; set; }

        public TestClient(INotImplementedService service)
        {
            Service = service;
        }
    }
}
