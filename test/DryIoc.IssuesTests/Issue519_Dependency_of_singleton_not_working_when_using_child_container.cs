using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue519_Dependency_of_singleton_not_working_when_using_child_container
    {
        [Test]
        public void Test()
        {
            var c = new Container();
            c.Register(typeof(IService), typeof(Service), Reuse.Singleton);

            var childContainer = c.WithRegistrationsCopy().OpenScope();

            childContainer.Use<IServiceDependency>(new ServiceDependency());

            var ex = Assert.Throws<ContainerException>(() =>
                childContainer.Resolve<IService>());

            Assert.AreEqual(
                Error.NameOf(Error.UnableToResolveUnknownService),
                ex.ErrorName);
        }

        public interface IService
        {
            IServiceDependency Dependency { get; }
        }

        public class Service : IService
        {
            public Service(IServiceDependency dependency)
            {
                Dependency = dependency;
            }

            public IServiceDependency Dependency { get; set; }
        }

        public interface IServiceDependency { }

        public class ServiceDependency : IServiceDependency { }
    }
}
