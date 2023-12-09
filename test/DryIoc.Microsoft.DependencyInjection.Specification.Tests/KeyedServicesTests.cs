using System;
using DryIoc.ImTools;
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
        public void ResolveKeyedServiceTransientFactory_COPY()
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

        [Fact]
        public void ResolveKeyedServiceSingletonInstanceWithKeyedParameter_COPY()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddKeyedSingleton<IService, Service>("service1");
            serviceCollection.AddKeyedSingleton<IService, Service>("service2");
            serviceCollection.AddSingleton<OtherService>();

            var provider = CreateServiceProvider(serviceCollection);

            Assert.Null(provider.GetService<IService>());
            var svc = provider.GetService<OtherService>();
            Assert.NotNull(svc);
            Assert.Equal("service1", svc.Service1.ToString());
            Assert.Equal("service2", svc.Service2.ToString());
        }

        [Fact]
        public void ResolveKeyedServiceSingletonFactoryWithAnyKey_COPY()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddKeyedSingleton<IService>(KeyedService.AnyKey, (_, key) => new Service((string)key));

            var provider = CreateServiceProvider(serviceCollection);

            Assert.Null(provider.GetService<IService>());

            for (var i = 0; i < 3; i++)
            {
                var key = "service" + i;
                var s1 = provider.GetKeyedService<IService>(key);
                var s2 = provider.GetKeyedService<IService>(key);
                Assert.Same(s1, s2);
                Assert.Equal(key, s1.ToString());
            }
        }

        [Fact]
        public void ResolveKeyedServiceSingletonFactoryWithAnyKey_OpenGenericService()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddKeyedSingleton(typeof(IOGService<>), KeyedService.AnyKey, typeof(OGService<>));

            var provider = CreateServiceProvider(serviceCollection);

            Assert.Null(provider.GetService<IOGService<string>>());

            for (var i = 0; i < 3; i++)
            {
                var key = "service" + i;
                var s1 = provider.GetKeyedService<IOGService<string>>(key);
                var s2 = provider.GetKeyedService<IOGService<string>>(key);
                Assert.Same(s1, s2);
                Assert.Equal(key, s1.ToString());

                var s3 = provider.GetKeyedService<IOGService<int>>(key);
                var s4 = provider.GetKeyedService<IOGService<int>>(key);
                Assert.Same(s3, s4);
                Assert.Equal(key, s3.ToString());
            }
        }

        [Fact]
        public void ResolveKeyedServiceSingletonInstanceWithAnyKey_COPY()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddKeyedSingleton<IService, Service>(KeyedService.AnyKey);

            var provider = CreateServiceProvider(serviceCollection);

            Assert.Null(provider.GetService<IService>());

            var serviceKey1 = "some-key";
            var svc1 = provider.GetKeyedService<IService>(serviceKey1);
            Assert.NotNull(svc1);
            Assert.Equal(serviceKey1, svc1.ToString());

            var serviceKey2 = "some-other-key";
            var svc2 = provider.GetKeyedService<IService>(serviceKey2);
            Assert.NotNull(svc2);
            Assert.Equal(serviceKey2, svc2.ToString());
        }

        [Fact]
        public void ResolveKeyedServicesSingletonInstanceWithAnyKey_COPY()
        {
            var service1 = new FakeService();
            var service2 = new FakeService();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddKeyedSingleton<IFakeOpenGenericService<PocoClass>>(KeyedService.AnyKey, service1);
            serviceCollection.AddKeyedSingleton<IFakeOpenGenericService<PocoClass>>("some-key", service2);

            var provider = CreateServiceProvider(serviceCollection);

            var services = provider.GetKeyedServices<IFakeOpenGenericService<PocoClass>>("some-key").ToArrayOrSelf();
            Assert.Equal(new[] { service1, service2 }, services);
        }

        internal interface IService { }

        internal class Service : IService
        {
            private readonly string _id;

            public Service() => _id = Guid.NewGuid().ToString();

            public Service([ServiceKey] string id) => _id = id;

            public override string ToString() => _id;
        }

        internal interface IOGService<T> { }

        internal class OGService<T> : IOGService<T>
        {
            private readonly string _id;

            public OGService() => _id = "*";

            public OGService([ServiceKey] string id) => _id = id;

            public override string ToString() => _id;
        }

        internal class OtherService
        {
            public OtherService(
                [FromKeyedServices("service1")] IService service1,
                [FromKeyedServices("service2")] IService service2)
            {
                Service1 = service1;
                Service2 = service2;
            }

            public IService Service1 { get; }

            public IService Service2 { get; }
        }
    }
}
