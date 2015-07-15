using System;
using NSubstitute;
using NUnit.Framework;

namespace DryIoc.IssuesTests.Samples
{
    [TestFixture]
    public class ResolveMocksForNonRegisteredServices
    {
        [Test]
        public void Resolve_mock_for_non_registered_service()
        {
            var container = new Container(rules => rules.WithUnknownServiceResolvers(request =>
            {
                var serviceType = request.ServiceType;
                if (!serviceType.IsAbstract)
                    return null; // Mock interface or abstract class only.

                return new ReflectionFactory(made: Made.Of(
                    () => Substitute.For(Arg.Index<Type[]>(0), Arg.Index<object[]>(1)),
                    _ => new[] { serviceType }, _ => (object[])null));
            }));

            var sub = container.Resolve<INotImplementedService>();

            Assert.That(sub, Is.InstanceOf<INotImplementedService>());
        }

        [Test]
        public void Inject_mock_for_non_registered_service()
        {
            var container = new Container(rules => rules.WithUnknownServiceResolvers(request =>
            {
                var serviceType = request.ServiceType;
                if (!serviceType.IsAbstract)
                    return null; // Mock interface or abstract class only.
                
                return new ReflectionFactory(made: Made.Of(
                    () => Substitute.For(Arg.Index<Type[]>(0), Arg.Index<object[]>(1)),
                    _ => new[] { serviceType }, _ => (object[])null));
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
