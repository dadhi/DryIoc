using System;
using System.Threading;
using System.Threading.Tasks;
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

    class BootstrapService : IHostedService
    {
        public BootstrapService(FooService bar) { }

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
                    services.AddHostedService<BootstrapService>();
                })
                .UseServiceProviderFactory(new DryIocServiceProviderFactory())
                .ConfigureContainer<Container>((hostContext, container) =>
                {
                    // no errors from DryIoc when the following line is uncommented
                    //                container.Register<FooService>(Reuse.Transient);
                })
                .ConfigureHostConfiguration(builder => builder.AddEnvironmentVariables())
                .Build();

            await host.RunAsync();
        }

        internal class DryIocServiceProviderFactory : IServiceProviderFactory<IContainer>
        {
            public IContainer CreateBuilder(IServiceCollection services) => 
                new Container().WithDependencyInjectionAdapter(services);

            public IServiceProvider CreateServiceProvider(IContainer container) => container;
        }
    }
}
