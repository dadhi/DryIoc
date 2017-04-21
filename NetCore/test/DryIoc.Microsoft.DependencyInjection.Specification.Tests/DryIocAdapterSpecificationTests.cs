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
        public void Should_track_transient_disposable_in_scope()
        {
            TestServiceCollection services = new TestServiceCollection();
            services.AddTransient<IFakeService, FakeService>();
            IServiceProvider serviceProvider = this.CreateServiceProvider((IServiceCollection)services);
            FakeService fakeService = Assert.IsType<FakeService>((object)serviceProvider.GetService<IFakeService>());
            FakeService service2;
            FakeService service3;
            using (IServiceScope scope = serviceProvider.CreateScope())
            {
                service2 = (FakeService)scope.ServiceProvider.GetService<IFakeService>();
                service3 = (FakeService)scope.ServiceProvider.GetService<IFakeService>();
                Assert.False(service2.Disposed, "service2.Disposed");
                Assert.False(service3.Disposed, "service3.Disposed");
                Assert.NotSame(service2, service3);
            }
            Assert.True(service2.Disposed, "service2.Disposed");
            Assert.True(service3.Disposed, "service3.Disposed");
            Assert.False(fakeService.Disposed);
        }
    }
}
