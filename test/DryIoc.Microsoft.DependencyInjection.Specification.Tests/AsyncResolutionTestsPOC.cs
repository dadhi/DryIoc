using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace DryIoc.Microsoft.DependencyInjection.Specification.Tests
{
    public static class AsyncExt
    {
        public static IServiceCollection AddSingleton<TService>(this IServiceCollection services, Func<IServiceProvider, Task<TService>> asyncFactory)
        {
            var factoryID = Factory.GetNextID();

            Task<TService> CreateServiceAsync(IServiceProvider sp)
            {
                var dryIoc = sp.GetRequiredService<IResolverContext>();
                var result = dryIoc.SingletonScope.GetOrAddViaFactoryDelegate(factoryID, r => asyncFactory(r), dryIoc);
                return (Task<TService>)result;
            }

            return services.AddSingleton<Func<IServiceProvider, Task<TService>>>(CreateServiceAsync);
        }

        public static Task<TService> GetRequiredServiceAsync<TService>(this IServiceProvider sp) =>
            sp.GetRequiredService<Func<IServiceProvider, Task<TService>>>().Invoke(sp);
    }

    public class AsyncResolutionTestsPOC
    {
        [Test]
        public async Task GetRequiredServiceAsync()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IRemoteConnectionFactory, TestConnectionFactory>();
            services.AddSingleton<IRemoteConnection>(sp =>
            {
                var factory = sp.GetRequiredService<IRemoteConnectionFactory>();
                return factory.ConnectAsync();
            });

            var providerFactory = new DryIocServiceProviderFactory();
            var provider = providerFactory.CreateServiceProvider(providerFactory.CreateBuilder(services));

            var connection1 = await provider.GetRequiredServiceAsync<IRemoteConnection>();
            Assert.IsNotNull(connection1);

            var connection2 = await provider.GetRequiredServiceAsync<IRemoteConnection>();
            Assert.AreSame(connection2, connection1);

            await connection2.PublishAsync("hello", "sailor");
        }

        public interface IRemoteConnection
        {
            Task PublishAsync(string channel, string message);
            Task DisposeAsync();
        }

        public interface IRemoteConnectionFactory
        {
            Task<IRemoteConnection> ConnectAsync();
        }
        class TestConnectionFactory : IRemoteConnectionFactory
        {
            public Task<IRemoteConnection> ConnectAsync() => Task.FromResult<IRemoteConnection>(new TestRemoteConnection());

        }
        class TestRemoteConnection : IRemoteConnection
        {
            public Task DisposeAsync() => Task.CompletedTask;
            public async Task PublishAsync(string channel, string message) 
            { 
                await Task.Delay(TimeSpan.FromMilliseconds(17));
                Assert.AreEqual("hello", channel);
                Assert.AreEqual("sailor", message);
            }
        }
    }
}