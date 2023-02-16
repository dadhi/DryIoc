using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DryIocDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // var rules = Rules.MicrosoftDependencyInjectionRules;
            var rules = Rules.MicrosoftDependencyInjectionRules.WithConcreteTypeDynamicRegistrations(reuse: Reuse.Transient);
            // var rules = Rules.Default.WithConcreteTypeDynamicRegistrations(reuse: Reuse.Transient);
            // var rules = Rules.Default.WithConcreteTypeDynamicRegistrations(reuse: Reuse.Transient).WithMicrosoftDependencyInjectionRules();

            var container = new Container(rules);

            var hostBuilder = Host.CreateDefaultBuilder()
                .UseServiceProviderFactory(new DryIocServiceProviderFactory(container))
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IService, Service>();
                });


            var host = hostBuilder.Build();

            // WORKAROUND for now!
            container = host.Services.GetService<IContainer>() as Container;

            try
            {
                var resolveInterfaceFromContainer = container.Resolve<IService>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Catch a exception when resolve interface from Container: " + ex.Message);
            }

            try
            {
                var resolveImplementFromContainer = container.Resolve<Service>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Catch a exception when resolve implement from Container" + ex.Message);
            }

            try
            {
                var resolveInterfaceFromServiceProvider = host.Services.GetRequiredService<IService>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Catch a exception when resolve interface from ServiceProvider" + ex.Message);
            }

            try
            {
                var resolveImplementFromServiceProvider = host.Services.GetRequiredService<Service>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Catch a exception when resolve implement from ServiceProvider" + ex.Message);
            }

            host.RunAsync();
        }
    }


    public interface IService
    {
    }

    public class Service : IService
    {
    }
}