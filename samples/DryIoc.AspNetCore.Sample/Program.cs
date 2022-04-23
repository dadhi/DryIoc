using System;
using System.IO;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using DryIoc.Microsoft.DependencyInjection;

namespace DryIoc.AspNetCore.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = args,
                ContentRootPath = Directory.GetCurrentDirectory(),
                // etc.
            });


            // ## Dependency Injection stuff
            // -----------------------------------------------------------
            var container = new Container(rules =>
                // optional: Enables property injection for Controllers
                rules.With(propertiesAndFields: request => request.ServiceType.Name.EndsWith("Controller")
                    ? PropertiesAndFields.Properties()(request)
                    : null));

            container.RegisterMyBusinessLogic();

            // Here it goes the integration with the existing DryIoc container
            var diFactory = new DryIocServiceProviderFactory(container, ExampleOfCustomRegisterDescriptor);

            builder.Host.UseServiceProviderFactory(diFactory);

            builder.Services
                .AddMvc(options => options.EnableEndpointRouting = false)
                .AddControllersAsServices();

            // other things...
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            //-----------------------------------------------------------


            var app = builder.Build();

            app.UseMvc();
            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.Run();
        }

        private static bool ExampleOfCustomRegisterDescriptor(IRegistrator registrator, ServiceDescriptor descriptor)
        {
#if DEBUG
            if (descriptor.ServiceType == typeof(ILoggerFactory))
                Console.WriteLine($"{descriptor.ServiceType.Name} is registered as instance: {descriptor}");
#endif
            return false; // fallback to the default registration logic
        }
    }
}
