using NUnit.Framework;

namespace DryIoc.Samples
{
    [TestFixture]
    public class RegisterInjectResolve
    {
        [Test]
        public void Register_and_resolve()
        {
            var container = new Container();
            container.Register<IService, Service>();
            // or alternatively:
            //container.Register(typeof(IService), typeof(Service));
            //container.RegisterAll<Service>();
            //container.Register<IService, Service>(Reuse.Transient);

            var service = container.Resolve<IService>();
            // or alternatively:
            //var service = container.Resolve(typeof(IService));

            Assert.That(service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Register_and_resolve_singleton()
        {
            var container = new Container();
            container.Register<IService, Service>(Reuse.Singleton);

            var one = container.Resolve<IService>();
            var another = container.Resolve<IService>();

            Assert.That(one, Is.SameAs(another));
        }

        [Test]
        public void Register_and_resolve_open_generic()
        {
            var container = new Container();
            container.Register(typeof(IService<>), typeof(Service<>));

            var service = container.Resolve<IService<int>>();

            Assert.That(service, Is.InstanceOf<Service<int>>());
        }

        [Test]
        public void Register_and_resolve_using_custom_delegate()
        {
            var container = new Container();
            container.RegisterDelegate<IService>(_ => new Service());

            var service = container.Resolve<IService>();

            Assert.That(service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Register_and_resolve_singleton_using_custom_delegate()
        {
            var container = new Container();
            container.RegisterDelegate<IService>(_ => new Service(), Reuse.Singleton);

            var one = container.Resolve<IService>();
            var another = container.Resolve<IService>();

            Assert.That(one, Is.SameAs(another));
        }

        [Test]
        public void Inject_registered_service_in_constructor_parameter_of_other_registered_service()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<ServiceClient>();

            var client = container.Resolve<ServiceClient>();

            Assert.That(client.Service, Is.InstanceOf<Service>());
        }
    }

    public interface IService { }

    public class Service : IService { }

    public interface IService<T> { }

    public class Service<T> : IService<T> { }

    public class ServiceClient
    {
        public IService Service { get; private set; }

        public ServiceClient(IService service)
        {
            Service = service;
        }
    }
}
