using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace DryIoc.Microsoft.DependencyInjection.Specification.Tests
{
    [TestFixture]
    public class GetRequiredServiceTests
    {
        [Test]
        public void GetService_should_throw_with_the_global_rule_set()
        {
            var services = new ServiceCollection();
            var container = new Container(rules => rules.WithServiceProviderGetServiceShouldThrowIfUnresolved());
            var providerFactory = new DryIocServiceProviderFactory(container);
            var provider = providerFactory.CreateServiceProvider(providerFactory.CreateBuilder(services));

            Assert.Throws<ContainerException>(() =>
                provider.GetService<Buz>());
        }

        [Test]
        public void GetRequiredService_throws_InvalidOperationException_now()
        {
            var services = new ServiceCollection();
            var providerFactory = new DryIocServiceProviderFactory();
            var provider = providerFactory.CreateServiceProvider(providerFactory.CreateBuilder(services));

            try
            {
                provider.GetRequiredService<Buz>();
                Assert.Fail("Expected InvalidOperationException but got none.");
            }
            catch (InvalidOperationException e)
            {
                Assert.IsInstanceOf<ContainerException>(e);
            }
            catch (Exception e)
            {
                Assert.Fail($"Expected {nameof(InvalidOperationException)} but got {e.GetType().Name}");
            }
        }

        public class Buz { }
    }
}