using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification;
using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
using Xunit;

namespace DryIoc.Microsoft.DependencyInjection.Specification.Tests
{
    public class DryIocAdapterSpecificationTests : DependencyInjectionSpecificationTests
    {
        protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
        {
            return new Container().WithDependencyInjectionAdapter(serviceCollection).Resolve<IServiceProvider>();
        }

        internal class TestServiceCollection : List<ServiceDescriptor>, IServiceCollection, IList<ServiceDescriptor>, ICollection<ServiceDescriptor>, IEnumerable<ServiceDescriptor>
        {
        }

        [Fact]
        public void NestedScopedServiceCanBeResolved()
        {
            var services = new TestServiceCollection();
            services.AddScoped<IFakeScopedService, FakeService>();
            using (var scope1 = CreateServiceProvider(services).CreateScope())
            {
                using (var scope2 = scope1.ServiceProvider.CreateScope())
                {
                    var service1 = scope1.ServiceProvider.GetService<IFakeScopedService>();
                    var service2 = scope2.ServiceProvider.GetService<IFakeScopedService>();
                    Assert.NotNull(service1);
                    Assert.NotNull(service2);
                    Assert.NotSame(service1, service2);
                }
            }
        }
    }
}
