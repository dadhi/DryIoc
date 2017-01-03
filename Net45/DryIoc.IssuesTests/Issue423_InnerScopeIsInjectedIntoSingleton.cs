using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue423_InnerScopeIsInjectedIntoSingleton
    {
        [Test]
        public void Original_test()
        {
            var container = new Container();
            container.Register<Singleton>(Reuse.Singleton);

            var scope = container.OpenScope();
            var singleton = scope.Resolve<Singleton>();

            Assert.AreSame(container, singleton.Container);
        }

        [Test]
        public void Test_with_IFactory_wrapper()
        {
            var c = new Container();

            c.Register(typeof(IFactory<>), typeof(Factory<>), setup: Setup.Wrapper);

            c.Register<Singleton>(Reuse.Singleton);
            c.Register<Service>();

            var scope = c.OpenScope();
            var singleton = scope.Resolve<Singleton>();

            scope.Dispose();

            var service = singleton.ServiceFactory.Create();
            Assert.IsNotNull(service);
        }

        public class Singleton
        {
            public IFactory<Service> ServiceFactory { get; }
            public IContainer Container { get; }

            public Singleton(IFactory<Service> serviceFactory, IContainer container)
            {
                ServiceFactory = serviceFactory;
                Container = container;
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
