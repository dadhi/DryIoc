using System;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class InitializerTests
    {
        [Test]
        public void Can_register_initializer_as_decorator_delegate()
        {
            var container = new Container();
            container.Register<InitializableService>();
            container.RegisterDelegate<Func<InitializableService, InitializableService>>(r => x =>
            {
                x.Initialize("blah");
                return x;
            }, setup: DecoratorSetup.Default);

            var service = container.Resolve<InitializableService>();

            Assert.That(service.Data, Is.EqualTo("blah"));
        }

        [Test]
        public void Can_register_initializer_as_decorator_delegate_of_generic_impl()
        {
            var container = new Container();
            container.Register<IInitializable<InitializableService>, InitializableService>();
            container.RegisterDelegate<Func<IInitializable<InitializableService>, IInitializable<InitializableService>>>(
                r => x => x.Initialize("blah"), 
                setup: DecoratorSetup.Default);

            var service = (InitializableService)container.Resolve<IInitializable<InitializableService>>();

            Assert.That(service.Data, Is.EqualTo("blah"));
        }

        [Test]
        public void Can_register_initializer_as_decorator_delegate_with_dedicated_method()
        {
            var container = new Container();
            container.Register<InitializableService>();
            container.RegisterInitializer<IInitializable>(x => x.Initialize("yeah"));

            var service = container.Resolve<InitializableService>();

            Assert.That(service.Data, Is.EqualTo("yeah"));
        }

        [Test]
        public void Can_register_and_inject_initializer_as_decorator_delegate_with_dedicated_method()
        {
            var container = new Container();
            container.Register<ClientOfInitializableService>();
            container.Register<InitializableService>();
            container.RegisterInitializer<IInitializable>(x => x.Initialize("yeah"));

            var client = container.Resolve<ClientOfInitializableService>();

            Assert.That(client.Service.Data, Is.EqualTo("yeah"));
        }

        [Test]
        public void Can_chain_initializers_as_decorator_delegate_with_dedicated_method()
        {
            var container = new Container();
            container.Register<ClientOfInitializableService>();
            container.Register<InitializableService>();
            container.RegisterInitializer<IInitializable>(x => x.Initialize("yeah"));
            container.RegisterInitializer<IInitializable<InitializableService>>(x => x.Initialize("blah"));

            var client = container.Resolve<ClientOfInitializableService>();

            Assert.That(client.Service.Data, Is.StringContaining("yeah"));
            Assert.That(client.Service.Data, Is.StringContaining("blah"));
        }

        [Test]
        public void Can_register_and_call_one_initializer_multiple_times_with_different_parameters()
        {
            var container = new Container();
            container.Register<ClientOfInitializableService>();
            container.Register<InitializableService>();
            container.RegisterInitializer<IInitializable>(x => x.Initialize("green"));
            container.RegisterInitializer<IInitializable>(x => x.Initialize("-blah"));

            var client = container.Resolve<ClientOfInitializableService>();

            Assert.That(client.Service.Data, Is.EqualTo("green-blah"));
        }

        public interface IInitializable<T>
        {
            T Initialize(string data);
        }

        public interface IInitializable
        {
            void Initialize(string data);
        }

        public class InitializableService : IInitializable<InitializableService>, IInitializable
        {
            public string Data = String.Empty;

            public InitializableService Initialize(string data)
            {
                Data += data;
                return this;
            }

            void IInitializable.Initialize(string data)
            {
                Data += data;
            }
        }

        public class ClientOfInitializableService
        {
            public InitializableService Service { get; private set; }

            public ClientOfInitializableService(InitializableService service)
            {
                Service = service;
            }
        }
    }

    public static class DecoratorContainerExt 
    {
        public static void RegisterInitializer<T>(this IRegistrator registrator, Action<T> decorate)
        {
            registrator.RegisterDelegate(r => decorate, setup: DecoratorSetup.Default);
        }
    }
}
