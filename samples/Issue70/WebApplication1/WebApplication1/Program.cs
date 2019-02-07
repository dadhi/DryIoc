using System;
using System.Threading;
using System.Threading.Tasks;
using DryIoc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DryIoc.Microsoft.DependencyInjection;
using Container = DryIoc.Container;
using IContainer = DryIoc.IContainer;

namespace WebApplication1
{
    internal class FooService
    {
    }

    class MyBootstrapService : IHostedService
    {
        public MyBootstrapService(FooService bar) { }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    class Program
    {
        static async Task Main()
        {
            var host = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHostedService<MyBootstrapService>();
                })
                .UseServiceProviderFactory(new DryIocServiceProviderFactory())
                .ConfigureContainer<Container>((hostContext, container) =>
                {
                    //container.Register<FooService>(Reuse.Transient);
                    // etc.
                })
                .Build();

            await host.RunAsync();
        }
    }
}
