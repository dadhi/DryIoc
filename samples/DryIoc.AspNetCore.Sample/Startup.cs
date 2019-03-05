using System;
using DryIoc.MefAttributedModel;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DryIoc.AspNetCore.Sample
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services
                .AddLogging(logging => logging.AddConsole())
                .AddMvc()
                // Enables controllers to be resolved by DryIoc, OTHERWISE resolved by infrastructure
                .AddControllersAsServices();

            // Container in V4 is directly implementing `IServiceProvider`, so it is fine to return it.
            return new Container(rules =>
                    // optional: Enables property injection for Controllers
                    // In current setup `WithMef` it will be overriden by properties marked with `ImportAttribute`
                    rules.With(propertiesAndFields: request => request.ServiceType.Name.EndsWith("Controller") 
                        ? PropertiesAndFields.Properties()(request)
                        : null)
                )
                // optional: support for MEF Exported services
                .WithMef()
                .WithDependencyInjectionAdapter(services,
                    // optional: You may Log or Customize the infrastructure components registrations
                    MyCustomRegisterDescriptor)
                // Your registrations are defined in CompositionRoot class
                .WithCompositionRoot<MyCompositionRoot>();
        }

        bool MyCustomRegisterDescriptor(IRegistrator registrator, ServiceDescriptor descriptor)
        {
#if DEBUG
            if (descriptor.ServiceType == typeof(ILoggerFactory))
                Console.WriteLine($"{descriptor.ServiceType.Name} is registered as instance: {descriptor}");
#endif
            return false; // fallback to the default registration logic
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseMvc();

            // uncomment to test:
            //app.Run(async (context) =>
            //{
            //    await context.Response.WriteAsync("Hello World!");
            //});
        }
    }
}
