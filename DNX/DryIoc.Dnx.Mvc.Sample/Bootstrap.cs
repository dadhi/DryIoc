using DryIoc;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.DependencyInjection;
using Web.Components;

namespace Web
{
    public static class Bootstrap
    {
        /***
         ** IoC container setup in case features offered by ServiceDescriptor are sufficient
         ***/
        public static void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<ISingletonService, FooSingletonService>();
            services.AddScoped<IPerRequestService, FooPerRequestService>();
            services.AddTransient<ITransientService, FooTransientService>();
        }

        /***
         ** IoC container setup in case features offered by ServiceDescriptor are not sufficient
         ***/
        public static void RegisterServices(IRegistrator registrator)
        {
            registrator.Register<ISingletonService>(Reuse.Singleton, 
                Made.Of(() => BarSingletonService.FactoryMethod()));
            registrator.Register<IPerRequestService>(Reuse.InCurrentScope,
                Made.Of(() => BarPerRequestService.FactoryMethod(Arg.Of<ISingletonService>())));
            registrator.Register<ITransientService>(Reuse.Transient, 
                Made.Of(() => BarTransientService.FactoryMethod(Arg.Of<IPerRequestService>())));

            registrator.Register<FooServiceHttpContext>(Reuse.InCurrentScope);
            registrator.Register<BarServiceHttpContext>(Made.Of(
                () => BarServiceHttpContext.FactoryMethod(Arg.Of<HttpContext>(), Arg.Of<ISingletonService>())), 
                Reuse.InCurrentScope);
        }
    }
}
