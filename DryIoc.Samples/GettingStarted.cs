using NUnit.Framework;

namespace DryIoc.Samples
{
    [TestFixture]
    public class GettingStarted
    {
        [Test]
        public void Register_some_client_and_service_types_Then_resolve_client_with_injected_service()
        {
            var container = new Container();
            container.Register<IClient, SomeClient>();
            container.Register<IService, SomeService>(Reuse.Singleton);
            // or alternatively:
            //container.Register(typeof(IService), typeof(SomeService), Reuse.Singleton);
            //container.RegisterAll<SomeService>(Reuse.Singleton);

            var client = container.Resolve<IClient>();
            // or alternatively:
            //var client = container.Resolve(typeof(IClient));

            Assert.That(client, Is.InstanceOf<SomeClient>());
            Assert.That(client.Service, Is.InstanceOf<SomeService>());
        }

        [Test]
        public void Register_some_client_as_delegate_Then_resolve_client_with_injected_service()
        {
            var container = new Container();
            container.RegisterDelegate<IClient>(r => new SomeClient(r.Resolve<IService>()));
            container.RegisterDelegate<IService>(r => new SomeService(), Reuse.Singleton);

            var client = container.Resolve<IClient>();

            Assert.That(client, Is.InstanceOf<SomeClient>());
        }

        [Test]
        public void Specify_reuse_type_for_resolved_injected_objects()
        {
            var container = new Container();

            // Transient reuse means no reuse at all.
            // Every time client is resolved/injected a new object will be created.
            container.Register<IClient, SomeClient>(Reuse.Transient);
            // You can omit reuse parameter when registering Transient objects.
            // container.Register<IClient, SomeClient>();

            // Singleton means that service object will be created at first resolve/injection,
            // then the same instance will be returned for all subsequent resolves from this container.
            container.Register<IService, SomeService>(Reuse.Singleton);

            var client = container.Resolve<IClient>();
            var anotherClient = container.Resolve<IClient>();

            Assert.That(client, Is.Not.SameAs(anotherClient));
            Assert.That(client.Service, Is.SameAs(anotherClient.Service));
        }

        [Test]
        public void If_you_forgot_to_register_service_type_Container_will_guide_you_with_exception_message()
        {
            var container = new Container();
            container.Register<IClient, SomeClient>();
            // forgot to container.Register<IService, SomeService>(Reuse.Singleton);

            var exception = Assert.Throws<ContainerException>(() =>
                container.Resolve<IClient>());

            Assert.That(exception.Message, Is.EqualTo(
@"Unable to resolve DryIoc.Samples.IService as ctor-parameter 'service'
in DryIoc.Samples.IClient of DryIoc.Samples.SomeClient.
Please register service OR adjust container resolution rules."));
        }
    }

    public interface IService { }

    public class SomeService : IService { }

    public interface IClient
    {
        IService Service { get; }
    }

    public class SomeClient : IClient
    {
        public IService Service { get; private set; }

        public SomeClient(IService service)
        {
            Service = service;
        }
    }
}
