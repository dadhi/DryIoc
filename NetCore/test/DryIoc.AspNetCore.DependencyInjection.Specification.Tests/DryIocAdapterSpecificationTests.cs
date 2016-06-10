using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification;
using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
using Xunit;

namespace DryIoc.AspNetCore.DependencyInjection.Specification.Tests
{
    public class DryIocAdapterSpecificationTests : DependencyInjectionSpecificationTests
    {
        protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
        {
            return new Container().WithDependencyInjectionAdapter(serviceCollection).Resolve<IServiceProvider>();
        }

        internal class ServiceCollection : List<ServiceDescriptor>, IServiceCollection
        {
        }

        [Fact]
        public void DisposingScopeDisposesService2()
        {
            ServiceCollection services = new ServiceCollection();
            services.AddSingleton<IFakeSingletonService, FakeService>();
            services.AddScoped<IFakeScopedService, FakeService>();
            services.AddTransient<IFakeService, FakeService>();
            IServiceProvider serviceProvider = this.CreateServiceProvider((IServiceCollection)services);
            FakeService fakeService1 = Assert.IsType<FakeService>((object)serviceProvider.GetService<IFakeService>());
            FakeService fakeService2;
            FakeService fakeService3;
            FakeService fakeService4;
            FakeService fakeService5;
            using (IServiceScope scope = serviceProvider.GetService<IServiceScopeFactory>().CreateScope())
            {
                fakeService2 = (FakeService)scope.ServiceProvider.GetService<IFakeScopedService>();
                fakeService3 = (FakeService)scope.ServiceProvider.GetService<IFakeService>();
                fakeService4 = (FakeService)scope.ServiceProvider.GetService<IFakeService>();
                fakeService5 = (FakeService)scope.ServiceProvider.GetService<IFakeSingletonService>();
                Assert.False(fakeService2.Disposed);
                Assert.False(fakeService3.Disposed);
                Assert.False(fakeService4.Disposed);
                Assert.False(fakeService5.Disposed);
            }
            Assert.True(fakeService2.Disposed, "fakeService2.Disposed");
            Assert.True(fakeService3.Disposed, "fakeService3.Disposed");
            Assert.True(fakeService4.Disposed, "fakeService4.Disposed");
            Assert.False(fakeService5.Disposed);
            var disposable = serviceProvider as IDisposable;
            if (disposable == null)
                return;
            disposable.Dispose();
            Assert.True(fakeService5.Disposed, "fakeService5.Disposed");
            Assert.True(fakeService1.Disposed, "fakeService1.Disposed");
        }
    }
}
