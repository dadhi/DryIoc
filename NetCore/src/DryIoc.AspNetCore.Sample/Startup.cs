using System;
//using DryIoc.MefAttributedModel;
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
                .AddMvc()

                // Enables constrollers to be resolved by DryIoc, OTHERWISE resolved by infrastructure
                .AddControllersAsServices();

            return new Container()
                // optional: support for MEF service discovery
                //.WithMef()
                // setup DI adapter
                .WithDependencyInjectionAdapter(services,
                    // optional: get original DryIoc.ContainerException if specified type is not resolved, 
                    // and prevent fallback to default resolution by infrastructure
                    throwIfUnresolved: type => type.Name.EndsWith("Controller"),

                    // optional: You may Log or Customize the infrastructure components registrations
                    registerDescriptor: (registrator, descriptor) =>
                    {
#if DEBUG
                        if (descriptor.ServiceType == typeof(ILoggerFactory))
                            Console.WriteLine($"Logger factory is regsitered as instance: {descriptor.ImplementationInstance != null}");
#endif
                        return false; // fallback to default registration logic
                    })

                // Your registrations are defined in CompositionRoot class
                .ConfigureServiceProvider<CompositionRoot>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

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
