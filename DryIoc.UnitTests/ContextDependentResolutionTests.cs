using System;
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

            var factories = HashTree<Type, Factory>.Empty;
            container.Register<ILogger>(new FactoryProvider(request =>
            {
                var parent = request.GetNonWrapperParentOrEmpty().ThrowIf(p => p.IsEmpty);
                var typeArg = parent.ImplementationType ?? parent.ServiceType;

                var factory = factories.GetValueOrDefault(typeArg);
                if (factory == null)
                    factories = factories.AddOrUpdate(typeArg, factory = new ReflectionFactory(typeof(Logger<>).MakeGenericType(typeArg)));
                return factory;
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

            var factories = HashTree<Type, Factory>.Empty;
            container.Register<ILogger>(new FactoryProvider(request =>
            {
                var parent = request.GetNonWrapperParentOrEmpty().ThrowIf(p => p.IsEmpty);
                var typeArg = parent.ImplementationType ?? parent.ServiceType;

                var factory = factories.GetValueOrDefault(typeArg);
                if (factory == null)
                    factories = factories.AddOrUpdate(typeArg, factory = new ReflectionFactory(typeof(Logger<>).MakeGenericType(typeArg), Reuse.Singleton));
                return factory;
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

            var factories = HashTree<Type, Factory>.Empty;
            container.Register<ILogger>(
                new FactoryProvider(request =>
                {
                    var implType = typeof(PlainLogger);
                    if (request.GetNonWrapperParentOrEmpty().ImplementationType == typeof(User2))
                        implType = typeof(FastLogger);

                    var factory = factories.GetValueOrDefault(implType);
                    if (factory == null)
                        factories = factories.AddOrUpdate(implType, factory = new ReflectionFactory(implType));
                    return factory;
                }));

            var user2 = container.Resolve<User2>();
            Assert.That(user2.Logger, Is.InstanceOf<FastLogger>());

            var user1 = container.Resolve<User1>();
            Assert.That(user1.Logger, Is.InstanceOf<PlainLogger>());
        }

        public static class LofFactory
        {
            public static ILogger GetLog<T>()
            {
                return new Logger<T>();
            }
        }

        [Test]
        public void Can_use_FactoryMethod_to_register_ILogger_with_generic_implementation_dependent_on_parent()
        {
            var c = new Container();
            c.Register<User1>();
            c.Register<User2>();

            c.Register<ILogger>(with: InjectionRules.With(r => FactoryMethod.Of(
                typeof(LofFactory).GetDeclaredMethodOrNull("GetLog")
                .MakeGenericMethod(r.GetNonWrapperParentOrEmpty().ImplementationType))),
                setup: Setup.With(expressionCaching: false));

            Assert.That(c.Resolve<User2>().Logger, Is.InstanceOf<Logger<User2>>());
            Assert.That(c.Resolve<User1>().Logger, Is.InstanceOf<Logger<User1>>());
        }

        [Test]
        public void If_FactoryProvider_is_returns_null_factory_it_should_Throw_Unable_to_resolve()
        {
            var container = new Container();
            container.Register<object>(new FactoryProvider(request => null));

            var ex = Assert.Throws<ContainerException>(() => container.Resolve<object>());
            Assert.AreEqual(ex.Error, Error.UNABLE_TO_RESOLVE_SERVICE);
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
        public ILogger Logger { get; private set; }

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

    public class StrUser
    {
        public string Dependency { get; private set; }

        public StrUser(string dependency)
        {
            Dependency = dependency;
        }
    }

    #endregion
}