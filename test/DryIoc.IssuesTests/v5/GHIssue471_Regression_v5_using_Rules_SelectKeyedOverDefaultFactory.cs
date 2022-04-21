using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue471_Regression_v5_using_Rules_SelectKeyedOverDefaultFactory : ITest
    {
        public int Run()
        {
            Test();
            return 1;
        }

        [Test]
        public void Test()
        {
            var container = new Container();

            var dependencyKey = DependencyKey.SomeKey;
            var serviceKey = ServiceKey.AnotherKey;

            container.Register<IDependency, Dependency>(Reuse.Singleton, serviceKey: DependencyKey.SomeKey);
            container.Register<IService, Service>(Reuse.Scoped, serviceKey: ServiceKey.AnotherKey);

            var myScope = container
              .With(rules => rules.WithFactorySelector(Rules.SelectKeyedOverDefaultFactory(dependencyKey)))
              .OpenScope();

            var service = myScope.Resolve<IService>(serviceKey: serviceKey);
            
        }

        enum DependencyKey { SomeKey }
        enum ServiceKey { AnotherKey }

        interface IDependency { }
        class Dependency : IDependency { }
        interface IService { }
        class Service : IService
        {
            public IDependency D;
            public Service(IDependency d) => D = d;
        }
    }
}