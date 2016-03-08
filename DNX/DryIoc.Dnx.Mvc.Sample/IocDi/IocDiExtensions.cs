using System;
using DryIoc;
using DryIoc.Dnx.DependencyInjection;
using Microsoft.AspNet.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Web.IocDi
{
    public static class IocDiExtensions
    {
        public static IServiceProvider WithIocDiSimple(this IServiceCollection services, Action<IServiceCollection> configureServices)
        {
            configureServices(services);
            var container = new Container().WithDependencyInjectionAdapter(services);
            return container.Resolve<IServiceProvider>();
        }

        public static IServiceProvider WithIocDiFull(this IServiceCollection services, Action<IRegistrator> configureServices)
        {
            var container = new Container().WithDependencyInjectionAdapter(services);
            configureServices(container);
            var serviceProvider = container.Resolve<IServiceProvider>();
            return serviceProvider;
        }

        public static void UseIocDi(this IApplicationBuilder app)
        {
            app.UseMiddleware<IocDiMiddleware>();
        }
    }
}
