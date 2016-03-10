using System;
using DryIoc;
using DryIoc.Dnx.DependencyInjection;
using DryIoc.MefAttributedModel;
using Microsoft.AspNet.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Web.IocDi
{
    public static class IocDiExtensions
    {
        public static IServiceProvider ConfigureDI(
            this IServiceCollection services, 
            Action<IRegistrator> configureServices)
        {
            var container = new Container()
                .WithDependencyInjectionAdapter(services)
                .WithMefAttributedModel();

            configureServices(container);

            return container.Resolve<IServiceProvider>();
        }
    }
}
