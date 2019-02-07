using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;

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
                .ConfigureContainer<IContainer>((hostContext, container) =>
                {
                    //container.Register<FooService>(Reuse.Transient);
                    // etc.
                })
                .Build();

            await host.RunAsync();
        }
    }
}
