using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using DryIoc.Microsoft.DependencyInjection;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue555_ConcreteTypeDynamicRegistrations_is_not_working_with_MicrosoftDependencyInjectionRules : ITest
    {
        public int Run()
        {
            Test1();
            return 1;
        }

        [Test]
        public void Test1()
        {
            var rules = DryIocAdapter
                .MicrosoftDependencyInjectionRules
                .WithConcreteTypeDynamicRegistrations(reuse: Reuse.Transient);

            var container = new Container(rules);

            var factory = new DryIocServiceProviderFactory(container, RegistrySharing.Share);
            var msDiContainer = factory.CreateBuilder(new ServiceCollection()).GetContainer();
            Assert.AreSame(container, msDiContainer);
        }

        public interface IServiceA
        {
            string Text { get; }
        }

        public class ServiceA : IServiceA
        {
            public string Text { get; set; }
        }
    }
}
