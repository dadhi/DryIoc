using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue506_WithConcreteTypeDynamicRegistrations_hides_failed_dependency_resolution : ITest
    {
        public int Run()
        {
            Test();
            return 1;
        }

        [Test]
        public void Test()
        {
            var containerWithNiceException = new Container();
            containerWithNiceException.Register<Service>();

            const string expected = @"as parameter ""dependency""";

            var ex = Assert.Throws<ContainerException>(() => containerWithNiceException.Resolve<Service>());
            StringAssert.Contains(expected, ex.Message);

            var containerWithAutoConcreteTypes = new Container(rules => rules.WithConcreteTypeDynamicRegistrations(IfUnresolved.Throw, reuse: Reuse.Transient));

            var ex2 = Assert.Throws<ContainerException>(() => containerWithAutoConcreteTypes.Resolve<Service>());
            StringAssert.Contains(expected, ex2.Message);
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
    }
}
