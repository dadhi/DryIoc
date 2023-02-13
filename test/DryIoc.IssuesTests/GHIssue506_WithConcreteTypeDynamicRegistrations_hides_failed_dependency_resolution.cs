using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue506_WithConcreteTypeDynamicRegistrations_hides_failed_dependency_resolution : ITest
    {
        public int Run()
        {
            // Test1();
            Test2();
            return 2;
        }

        [Test]
        public void Test1()
        {
            var containerWithNiceException = new Container();
            containerWithNiceException.Register<Service>();

            var ex = Assert.Throws<ContainerException>(() => containerWithNiceException.Resolve<Service>());

            const string expected = @"as parameter ""dependency""";
            StringAssert.Contains(expected, ex.Message);

            var containerWithAutoConcreteTypes = new Container(rules => rules.WithConcreteTypeDynamicRegistrations(IfUnresolved.Throw, reuse: Reuse.Transient));

            var ex2 = Assert.Throws<ContainerException>(() => containerWithAutoConcreteTypes.Resolve<Service>());
            StringAssert.Contains(expected, ex2.Message);
        }

        [Test]
        public void Test2()
        {
            var c = new Container(Rules.MicrosoftDependencyInjectionRules.WithConcreteTypeDynamicRegistrations());
            var o = c.Resolve<OtherService>();
            Assert.IsNotNull(o.Dependency);
        }

        internal class Service
        {
            public Service(IDependency dependency)
            {
            }
        }

        internal interface IDependency
        {
        }

        internal class Dependency : IDependency
        {
        }

        internal class OtherService
        {
            public readonly Dependency Dependency;
            public OtherService(Dependency dependency) => Dependency = dependency;
        }
    }
}
