using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace DryIoc.Microsoft.DependencyInjection.Specification.Tests
{
    [TestFixture]
    public class GHIssue317_Error_for_register_IOptions_in_prism : ITest
    {
        public int Run()
        {
            Test1();
            return 1;
        }

        [Test]
        public void Test1()
        {
            var services = new ServiceCollection();
            services.AddTransient<MyService>();
            services.AddOptions<AgentOptions>();

            var providerFactory = new DryIocServiceProviderFactory();
            var provider = providerFactory.CreateServiceProvider(providerFactory.CreateBuilder(services));
            provider.GetRequiredService<IResolver>();

            var service = provider.GetRequiredService<MyService>();
            Assert.IsNotNull(service.AgentOptions);
        }

        public class MyService 
        {
            public IOptions<AgentOptions> AgentOptions { get; }

            public MyService(IOptions<AgentOptions> ao) => AgentOptions = ao;
        }

        public class AgentOptions
        {
            public AgentOptions()
            {
                WalletConfiguration = new WalletConfiguration { Id = "DefaultWallet" };
                WalletCredentials = new WalletCredentials { Key = "DefaultKey" };
            }

            public WalletConfiguration WalletConfiguration { get; }
            public WalletCredentials WalletCredentials { get; }
        }
        public class WalletCredentials
        {
            public string Key { get; internal set; }
        }

        public class WalletConfiguration
        {
            public string Id { get; internal set; }
        }

    }
}