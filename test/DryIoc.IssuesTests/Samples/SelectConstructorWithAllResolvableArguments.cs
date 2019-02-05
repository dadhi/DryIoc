using NUnit.Framework;

namespace DryIoc.IssuesTests.Samples
{
    [TestFixture]
    public class SelectConstructorWithAllResolvableArguments
    {
        [Test]
        public void Container_is_providing_method_for_that()
        {
            var container = new Container();
            container.Register<IDependency, SomeDependency>();

            container.Register<SomeClient>(made: FactoryMethod.ConstructorWithResolvableArguments);

            var client = container.Resolve<SomeClient>();
            Assert.IsNotNull(client.Dependency);
        }

        public interface IDependency { }

        public class SomeDependency : IDependency { }

        public interface IService { }

        public class SomeClient
        {
            public readonly int Seed = 1;
            public readonly IService Service;
            public readonly IDependency Dependency;

            // Won't be selected because constructor with IDependency will be selected first.
            public SomeClient() { }

            // Won't be selected because nor Int32 nor IService is registered in Container.
            public SomeClient(int seed, IService service)
            {
                Service = service;
                Seed = seed;
            }

            // Will be selected because IDependency is registered in Container.
            public SomeClient(IDependency dependency)
            {
                Dependency = dependency;
            }
        }
    }
}
