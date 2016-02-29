using System;
using System.Collections.Generic;
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
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ISingletonService, FooSingletonService>();
            services.AddScoped<IPerRequestService, FooPerRequestService>();
            services.AddTransient<ITransientService, FooTransientService>();
        }

        /***
         ** IoC container setup in case features offered by ServiceDescriptor are not sufficient
         ***/
        public static void ConfigureServices(IRegistrator registrator)
        {
            registrator.Register<ISingletonService>(reuse: Reuse.Singleton, made: Made.Of(() => BarSingletonService.FactoryMethod()));
            registrator.Register<IPerRequestService>(reuse: Reuse.InCurrentScope, made: Made.Of(() => BarPerRequestService.FactoryMethod(Arg.Of<ISingletonService>())));
            registrator.Register<ITransientService>(reuse: Reuse.Transient, made: Made.Of(() => BarTransientService.FactoryMethod(Arg.Of<IPerRequestService>())));
        }

        public static IEnumerable<Tuple<Type, object>> GetScopeInstances(HttpContext httpContext)
        {
            IServiceProvider serviceProvider = httpContext.RequestServices ?? httpContext.ApplicationServices;

            yield return CreateTuple(new FooServiceHttpContext(httpContext));
            yield return CreateTuple(BarServiceHttpContext.FactoryMethod(httpContext, (ISingletonService)serviceProvider.GetService(typeof(ISingletonService))));
        }

        private static Tuple<Type, object> CreateTuple<TInstance>(TInstance instance)
        { return Tuple.Create(typeof(TInstance), (object)instance); }
    }
}
