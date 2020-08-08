using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue294_in_parameter_modifier_breaks_service_resolution
    {
        [Test]
        public void Test()
        {
            var c = new Container();
            c.Register<IService, Service>();
            c.Register<IDependency, Dependency>();

            var service = c.Resolve<IService>();

            Assert.IsNotNull(service.Dependency);
        }

        public interface IService
        {
            IDependency Dependency { get; }
        }

        public class Service : IService
        {
            public IDependency Dependency { get; }

            public Service(in IDependency dependency) => Dependency = dependency;
        }

        public interface IDependency
        {
        }

        public class Dependency : IDependency
        {
        }
    }
}
