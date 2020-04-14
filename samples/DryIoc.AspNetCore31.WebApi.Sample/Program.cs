using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DryIoc.AspNetCore31.WebApi.Sample
{
    public class Program
    {
        public static async Task Main(string[] args) => 
            await CreateHostBuilder(args).Build().RunAsync();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.ClearProviders(); // removes all providers from LoggerFactory
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddDebug();
                })
                .UseServiceProviderFactory(new DryIocServiceProviderFactory(Startup.CreateMyPreConfiguredContainer()))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
