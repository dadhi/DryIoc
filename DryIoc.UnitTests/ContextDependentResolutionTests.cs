using NUnit.Framework;
// ReSharper disable MemberHidesStaticFromOuterClass

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class ContextDependentResolutionTests
    {
        public static class LogFactory
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

            c.Register<ILogger>(made: Made.Of(r => FactoryMethod.Of(
                typeof(LogFactory).GetMethodOrNull("GetLog").MakeGenericMethod(r.Parent.ImplementationType))));

            Assert.That(c.Resolve<User2>().Logger, Is.InstanceOf<Logger<User2>>());
            Assert.That(c.Resolve<User1>().Logger, Is.InstanceOf<Logger<User1>>());
        }

        [Test]
        public void Can_select_what_factory_to_use_as_dependency_and_what_as_resolution_root()
        {
            var container = new Container();
            container.Register<IX, A>(setup: Setup.With(condition: request => !request.IsEmpty));
            container.Register<IX, B>(setup: Setup.With(condition: request => request.IsEmpty));
            container.Register<Y>();

            var y = container.Resolve<Y>();

            Assert.IsInstanceOf<A>(y.X);
        }

        [Test]
        public void Can_use_different_imlementations_based_on_context_using_condition()
        {
            var container = new Container();

            container.Register<ImportConditionObject1>();
            container.Register<ImportConditionObject2>();
            container.Register<ImportConditionObject3>();

            container.Register<IExportConditionInterface, ExportConditionalObject>(
                setup: Setup.With(condition: r => r.Parent.ImplementationType == typeof(ImportConditionObject1)));

            container.Register<IExportConditionInterface, ExportConditionalObject2>(
                setup: Setup.With(condition: r => r.Parent.ImplementationType == typeof(ImportConditionObject2)));

            container.Register<IExportConditionInterface, ExportConditionalObject3>(
                setup: Setup.With(condition: r => r.Parent.ImplementationType == typeof(ImportConditionObject3)));

            Assert.IsInstanceOf<ExportConditionalObject>(container.Resolve<ImportConditionObject1>().ExportConditionInterface);
            Assert.IsInstanceOf<ExportConditionalObject2>(container.Resolve<ImportConditionObject2>().ExportConditionInterface);
            Assert.IsInstanceOf<ExportConditionalObject3>(container.Resolve<ImportConditionObject3>().ExportConditionInterface);
        }

        #region CUT

        public interface IExportConditionInterface {}
        public class ExportConditionalObject : IExportConditionInterface {}
        public class ExportConditionalObject2 : IExportConditionInterface {}
        public class ExportConditionalObject3 : IExportConditionInterface {}
        public class ImportConditionObject1
        {
            public IExportConditionInterface ExportConditionInterface { get; set; }
            public ImportConditionObject1(IExportConditionInterface exportConditionInterface)
            {
                ExportConditionInterface = exportConditionInterface;
            }
        }

        public class ImportConditionObject2
        {
            public IExportConditionInterface ExportConditionInterface { get; set; }
            public ImportConditionObject2(IExportConditionInterface exportConditionInterface)
            {
                ExportConditionInterface = exportConditionInterface;
            }
        }

        public class ImportConditionObject3
        {
            public IExportConditionInterface ExportConditionInterface { get; set; }
            public ImportConditionObject3(IExportConditionInterface exportConditionInterface)
            {
                ExportConditionInterface = exportConditionInterface;
            }
        }

        internal interface IX { }
        internal class A : IX { }
        internal class B : IX { }
        internal class Y
        {
            public IX X;
            public Y(IX x) { X = x; }
        }

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