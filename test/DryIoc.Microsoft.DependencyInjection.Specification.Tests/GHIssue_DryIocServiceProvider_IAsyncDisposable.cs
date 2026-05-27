using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;

namespace DryIoc.Microsoft.DependencyInjection.Specification.Tests
{
    [TestFixture]
    public class GHIssue_DryIocServiceProvider_IAsyncDisposable : ITest
    {
        public int Run()
        {
            DryIocServiceProvider_implements_IAsyncDisposable();
            AsyncServiceScope_calls_DisposeAsync_not_Dispose().GetAwaiter().GetResult();
            AsyncDisposable_scoped_service_is_disposed_asynchronously().GetAwaiter().GetResult();
            return 3;
        }

        [Test]
        public void DryIocServiceProvider_implements_IAsyncDisposable()
        {
            var services = new ServiceCollection();
            var factory = new DryIocServiceProviderFactory();
            var provider = factory.CreateServiceProvider(factory.CreateBuilder(services));

            Assert.IsInstanceOf<IAsyncDisposable>(provider,
                "DryIocServiceProvider must implement IAsyncDisposable so that AsyncServiceScope.DisposeAsync() takes the async path.");
        }

        [Test]
        public async Task AsyncServiceScope_calls_DisposeAsync_not_Dispose()
        {
            var services = new ServiceCollection();
            services.AddScoped<AsyncDisposableService>();

            var factory = new DryIocServiceProviderFactory();
            var provider = factory.CreateServiceProvider(factory.CreateBuilder(services));

            AsyncDisposableService svc;
            await using (var scope = provider.CreateAsyncScope())
            {
                svc = scope.ServiceProvider.GetRequiredService<AsyncDisposableService>();
            }

            Assert.IsTrue(svc.AsyncDisposed, "DisposeAsync should have been called.");
            Assert.IsFalse(svc.SyncDisposed, "Dispose (sync) should not have been called.");
        }

        [Test]
        public async Task AsyncDisposable_scoped_service_is_disposed_asynchronously()
        {
            var services = new ServiceCollection();
            services.AddScoped<AsyncDisposableService>();

            var factory = new DryIocServiceProviderFactory();
            var rootProvider = factory.CreateServiceProvider(factory.CreateBuilder(services));

            AsyncDisposableService svc;
            var scopeFactory = rootProvider.GetRequiredService<IServiceScopeFactory>();
            var scope = scopeFactory.CreateScope();
            svc = scope.ServiceProvider.GetRequiredService<AsyncDisposableService>();

            if (scope is IAsyncDisposable asyncScope)
                await asyncScope.DisposeAsync();
            else
                scope.Dispose();

            Assert.IsTrue(svc.AsyncDisposed, "DisposeAsync should have been called on the scoped service.");
        }

        internal sealed class AsyncDisposableService : IAsyncDisposable, IDisposable
        {
            public bool AsyncDisposed { get; private set; }
            public bool SyncDisposed { get; private set; }

            public ValueTask DisposeAsync()
            {
                AsyncDisposed = true;
                return default;
            }

            public void Dispose() => SyncDisposed = true;
        }
    }
}
