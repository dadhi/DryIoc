using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification;

// uncomment when I want to copy some test here for testing.
using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
using Xunit;

namespace DryIoc.Microsoft.DependencyInjection.Specification.Tests
{
    public class KeyedServicesTests : KeyedDependencyInjectionSpecificationTests
    {
        protected override IServiceProvider CreateServiceProvider(IServiceCollection collection) =>
            new DryIocServiceProviderFactory().CreateBuilder(collection).BuildServiceProvider();

        [Fact]
        public void ResolveKeyedServiceTransientFactory_DEBUG()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddKeyedTransient<IService>("service1", (sp, key) => new Service(key as string));

            var provider = CreateServiceProvider(serviceCollection);

            Assert.Null(provider.GetService<IService>());

            var first = provider.GetKeyedService<IService>("service1");
            var second = provider.GetKeyedService<IService>("service1");

            Assert.NotSame(first, second);
            Assert.Equal("service1", first.ToString());
            Assert.Equal("service1", second.ToString());
        }

        internal interface IService { }

        internal class Service : IService
        {
            private readonly string _id;

            public Service() => _id = Guid.NewGuid().ToString();

            public Service([ServiceKey] string id) => _id = id;

            public override string? ToString() => _id;
        }

    }
}
