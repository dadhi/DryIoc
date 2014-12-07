using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture, Ignore("Not supported")]
    public class Issue61_PropagateRequestToResolveInRegisteredDelegate
    {
        [Test]
        public void Detect_recursive_dependency_when_registered_as_delegate()
        {
            var container = new Container();
            container.RegisterDelegate(r => new SomeClient(r.Resolve<ServiceWithClient>()));
            container.Register<ServiceWithClient>();

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<SomeClient>());

            Assert.That(ex.Message, Is.StringContaining("Recursive dependency is detected"));
        }

        [Test]
        public void Recursive_dependency_could_be_detected_when_resolving_properties_in_delegate_factory()
        {
            var container = new Container();
            container.RegisterDelegate(r => r.ResolvePropertiesAndFields(new SomeClientWithProps()));
            container.Register<ServiceWithClientWithProps>();

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<SomeClientWithProps>());

            Assert.That(ex.Message, Is.StringContaining("Recursive dependency is detected"));
        }

        [Test]
        public void Detect_recursive_dependency_when_dependency_registered_as_delegate()
        {
            var container = new Container();

            container.RegisterDelegate(r => new SomeClient(r.Resolve<ServiceWithClient>()));
            container.Register<ServiceWithClient>();
            container.Register<ClientFriend>();

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<ClientFriend>());

            Assert.That(ex.Message, Is.StringContaining("Recursive dependency is detected"));
        }

        [Test]
        public void Detect_recusive_dependency_for_custom_specified_parameter_with_factory_delegate()
        {
            var container = new Container();
            container.RegisterDelegate(r => new SomeClient(r.Resolve<ServiceWithClient>()));
            container.Register<ServiceWithClient>(rules: Parameters.Of.Name("client", r => r.Resolve<SomeClient>()));

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<SomeClient>());

            Assert.That(ex.Message, Is
                .StringContaining("Recursive dependency is detected when resolving").And
                .StringContaining("SomeClient <--recursive"));
        }

        internal class SomeClient
        {
            public ServiceWithClient Service { get; set; }

            public SomeClient(ServiceWithClient service)
            {
                Service = service;
            }
        }

        internal class ServiceWithClient
        {
            public SomeClient Client { get; set; }

            public ServiceWithClient(SomeClient client)
            {
                Client = client;
            }
        }

        internal class ClientFriend
        {
            public SomeClient Client { get; private set; }

            public ClientFriend(SomeClient client)
            {
                Client = client;
            }
        }

        internal class SomeClientWithProps
        {
            public ServiceWithClientWithProps Service { get; set; }
        }

        internal class ServiceWithClientWithProps
        {
            public SomeClientWithProps Client { get; set; }

            public ServiceWithClientWithProps(SomeClientWithProps client)
            {
                Client = client;
            }
        }
    }
}
