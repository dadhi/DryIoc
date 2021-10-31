using System;
using System.Collections.Generic;
using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection.Specification;

namespace DryIoc.Microsoft.DependencyInjection.Specification.Tests
{
    [TestFixture]
    public class GHIssue435_hangfire_use_dryioc_report_ContainerIsDisposed : DependencyInjectionSpecificationTests
    {
        protected override IServiceProvider CreateServiceProvider(IServiceCollection services) => 
            DryIocAdapter.Create(services);

        [Test]
        public void SingletonFactory_Test()
        {
            var collection = new ServiceCollection();

            collection.AddSingleton<ISingletonFactory>(r => new SingletonFactory(r));
            collection.AddTransient<IFakeService, FakeService>();

            var serviceProvider = CreateServiceProvider(collection);
        
            var scopeFactory = serviceProvider.GetService<IServiceScopeFactory>();
            ISingletonFactory singletonFactory;
            using (var scope = scopeFactory.CreateScope())
            {
                singletonFactory = (ISingletonFactory)scope.ServiceProvider.GetService(typeof(ISingletonFactory));
            }

            var fakeService = singletonFactory.GetService<IFakeService>();
            Assert.NotNull(fakeService);
        }

        public interface IFakeService { }
        public interface IFakeScopedService { }
        public interface IFakeSingletonService { }
        public class FakeService : IFakeService, IFakeScopedService, IFakeSingletonService, IDisposable
        {
            public bool Disposed { get; private set; }
            public void Dispose()
            {
                if (Disposed)
                    throw new ObjectDisposedException("FakeService");
                Disposed = true;
            }
        }

        public interface ISingletonFactory
        {
            T GetService<T>();
        }

        public class SingletonFactory : ISingletonFactory
        {
            private readonly IServiceProvider _serviceProvider;
            public SingletonFactory(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public T GetService<T>()
            {
                return (T)_serviceProvider.GetService(typeof(T));
            }
        }
    }
}
