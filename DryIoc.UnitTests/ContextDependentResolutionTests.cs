using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class ContextDependentResolutionTests
    {
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
                setup: Setup.With(cacheFactoryExpression: false));

            Assert.That(c.Resolve<User2>().Logger, Is.InstanceOf<Logger<User2>>());
            Assert.That(c.Resolve<User1>().Logger, Is.InstanceOf<Logger<User1>>());
        }

        [Test, Explicit("Not implemented yet: #22: Add Resolution condition to Factory setup")]
        public void Can_select_what_factory_to_use_as_dependency_and_what_as_resolution_root()
        {
            var container = new Container();
            container.Register<IX, A>(setup: Setup.With(condition: request => !request.IsEmpty));
            container.Register<IX, B>(setup: Setup.With(condition: request => request.IsEmpty));
            container.Register<Y>();

            var y = container.Resolve<Y>();
            Assert.IsInstanceOf<B>(y.X);
        }

        internal interface IX { }
        internal class A : IX { }
        internal class B : IX { }
        internal class Y
        {
            public IX X;
            public Y(IX x) { X = x; }
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
}