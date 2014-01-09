using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class ContextDependentResolutionTests
    {
        [Test]
        public void I_should_be_able_to_resolve_context_dependent_type()
        {
            var container = new Container();
            container.Register(typeof(Client));

            var factories = new Dictionary<Type, Factory>();
            container.Register<ILogger>(new FactoryProvider((request, _) =>
            {
                var parent = request.GetNonWrapperParentOrDefault();
                parent = parent.ThrowIfNull("{0} should be resolved only as dependency in other service.", request.ServiceType);
                var typeArg = parent.ImplementationType ?? parent.ServiceType;
                return factories.GetOrAdd(typeArg, t => new ReflectionFactory(typeof(Logger<>).MakeGenericType(t)));
            }));

            var client = container.Resolve<Client>();
            var log = client.Logger.Log("hello");

            StringAssert.Contains(typeof(Client).ToString(), log);
        }

        [Test]
        public void I_should_be_able_to_resolve_context_dependent_type_as_singleton()
        {
            var container = new Container();
            container.Register(typeof(Client));
            container.Register(typeof(ClientOfClient));

            var factories = new Dictionary<Type, Factory>();
            container.Register<ILogger>(new FactoryProvider((request, _) =>
            {
                var parent = request.GetNonWrapperParentOrDefault();
                parent = parent.ThrowIfNull("{0} should be resolved only as dependency in other service.", request.ServiceType);
                var genericArg = parent.ImplementationType ?? parent.ServiceType;
                return factories.GetOrAdd(genericArg, t => new ReflectionFactory(typeof(Logger<>).MakeGenericType(t), Reuse.Singleton));
            }));

            var client = container.Resolve<Client>();
            var clientOfClient = container.Resolve<ClientOfClient>();

            Assert.That(client.Logger, Is.SameAs(clientOfClient.Client.Logger));
        }

        [Test]
        public void I_can_resolve_service_based_on_client_implementation_type()
        {
            var container = new Container();
            container.Register<User1>();
            container.Register<User2>();

            var factories = new Dictionary<Type, Factory>();
            container.Register<ILogger>(
                new FactoryProvider((request, _) =>
                {
                    var implType = typeof(PlainLogger);
                    var parent = request.GetNonWrapperParentOrDefault();
                    if (parent != null && parent.ImplementationType == typeof(User2))
                        implType = typeof(FastLogger);
                    return factories.GetOrAdd(implType, t => new ReflectionFactory(t));
                }));

            var user2 = container.Resolve<User2>();
            Assert.That(user2.Logger, Is.InstanceOf<FastLogger>());

            var user1 = container.Resolve<User1>();
            Assert.That(user1.Logger, Is.InstanceOf<PlainLogger>());
        }

        [Test]
        public void If_FactoryProvider_is_returns_null_factory_it_should_Throw_Unable_to_resolve()
        {
            var container = new Container();
            container.Register<object>(new FactoryProvider((request, registry) => null));

            Assert.Throws<ContainerException>(() =>
                container.Resolve<object>());
        }
    }

    #region CUT

    public interface ILogger
    {
        string Log(string message);
    }

    public class Logger<T> : ILogger
    {
        public string Log(string message)
        {
            return typeof(T) + ": " + message;
        }
    }

    public class PlainLogger : ILogger
    {
        public string Log(string message)
        {
            return message;
        }
    }

    public class FastLogger : ILogger
    {
        public string Log(string message)
        {
            return message;
        }
    }

    public class Client
    {
        public ILogger Logger { get; set; }

        public Client(ILogger logger)
        {
            Logger = logger;
        }
    }

    public class ClientOfClient
    {
        public Client Client { get; set; }

        public ClientOfClient(Client client)
        {
            Client = client;
        }
    }

    public class User1
    {
        public ILogger Logger { get; set; }

        public User1(ILogger logger)
        {
            Logger = logger;
        }
    }

    public class User2
    {
        public ILogger Logger { get; set; }

        public User2(ILogger logger)
        {
            Logger = logger;
        }
    }

    #endregion
}