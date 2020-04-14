using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DryIoc.AspNetCore31.WebApi.Sample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var mvcBuilder = services.AddControllers();
            
            // uses DI to construct the controllers - required for the DryIoc diagnostics, property injection, etc. to work;
            mvcBuilder.AddControllersAsServices();
        }

        /// <summary>
        /// Use this method to pass your custom pre-configured container to the `IHostBuilder.UseServiceProviderFactory` in "Program.cs"
        /// </summary>
        public static IContainer CreateMyPreConfiguredContainer() =>
            // This is an example configuration,
            // for possible options check the https://github.com/dadhi/DryIoc/blob/master/docs/DryIoc.Docs/RulesAndDefaultConventions.md
            new Container(rules =>

                // Configures property injection for Controllers, ensure that you've added `AddControllersAsServices` in `ConfigureServices`
                rules.With(propertiesAndFields: request => 
                    request.ServiceType.Name.EndsWith("Controller") ? PropertiesAndFields.Properties()(request) : null)
            );

        public void ConfigureContainer(IContainer container)
        {
            // You may place your registrations here or split them in different classes, or organize them in some kind of modules, e.g:
            BasicServicesRegistrator.Register(container);
            SpecialServicesRegistrator.Register(container);

            // NOTE:
            // Don't configure the container rules here because DryIoc uses the immutable container/configuration
            // and you customized container will be lost.
            // Instead you may use something like `CreateMyPreConfiguredContainer` above.
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<Startup>();

            // All DryIoc Container interfaces are available through the MS.DI services
            var container = app.ApplicationServices.GetRequiredService<IContainer>();
            logger.LogInformation($"You may use the DryIoc container here: '{container}'");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
