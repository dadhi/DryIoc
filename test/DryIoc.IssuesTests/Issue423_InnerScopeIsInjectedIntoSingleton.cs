using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue423_InnerScopeIsInjectedIntoSingleton : ITest
    {
        public int Run()
        {
            Original_test();
            Test_with_IFactory_wrapper();
            return 2;
        }

        [Test]
        public void Original_test()
        {
            var container = new Container();
            container.Register<Singleton>(Reuse.Singleton);

            var scope = container.OpenScope();
            var singleton = scope.Resolve<Singleton>();

            scope.Dispose();

            Assert.AreSame(container, singleton.Container);
        }

        public class Singleton
        {
            public IContainer Container { get; }

            public Singleton(IContainer container)
            {
                Container = container;
            }
        }

        [Test]
        public void Test_with_IFactory_wrapper()
        {
            var c = new Container();

            c.Register(typeof(IFactory<>), typeof(Factory<>), setup: Setup.Wrapper);

            c.Register<SingletonWithFactory>(Reuse.Singleton);
            c.Register<Service>();

            var scope = c.OpenScope();
            var singleton = scope.Resolve<SingletonWithFactory>();

            scope.Dispose();

            var service = singleton.ServiceFactory.Create();
            Assert.IsNotNull(service);
        }

        public class SingletonWithFactory
        {
            public IFactory<Service> ServiceFactory { get; }

            public SingletonWithFactory(IFactory<Service> serviceFactory)
            {
                ServiceFactory = serviceFactory;
            }
        }

        public class Service { }

        public interface IFactory<T>
        {
            T Create();
        }

        class Factory<T> : IFactory<T>
        {
            private readonly IResolver _resolver;

            public Factory(IResolver resolver)
            {
                _resolver = resolver;
            }

            public T Create()
            {
                return _resolver.Resolve<T>();
            }
        }
    }
}
