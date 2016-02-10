using System;
using System.Collections.Generic;
using System.Reflection;
using DryIoc;
using DryIoc.Dnx.DependencyInjection;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Web.IocDi
{
    public delegate IEnumerable<Tuple<Type, object>> GetScopeInstancesDelegate(HttpContext httpContext);

    public static class IocDiExtensions
    {
        public static IServiceProvider WithIocDiSimple(this IServiceCollection services, Action<IServiceCollection> configureServices)
        {
            configureServices(services);
            var container = new Container(cfg => cfg.WithUnknownServiceResolvers(CreateFactoryForTypesMarkedWithResolveAs));
            return container.GetDryIocServiceProvider(services);
        }

        public static IServiceProvider WithIocDiFull(this IServiceCollection services, Action<IRegistrator> configureServices)
        {
            var container = new Container(cfg => cfg.WithUnknownServiceResolvers(CreateFactoryForTypesMarkedWithResolveAs));
            var serviceProvider = container.GetDryIocServiceProvider(services);
            configureServices((IContainer)serviceProvider.GetService(typeof(IContainer)));
            return serviceProvider;
        }

        public static void UseIocDi(this IApplicationBuilder app, GetScopeInstancesDelegate getScopeInstancesDelegate)
        { app.UseMiddleware<IocDiMiddleware>(getScopeInstancesDelegate); }

        private static ReflectionFactory CreateFactoryForTypesMarkedWithResolveAs(Request request)
        {
            var serviceType = request.ServiceType;
            var serviceTypeInfo = serviceType.GetTypeInfo();

            var resolveAsSelf = serviceTypeInfo.GetCustomAttribute(typeof(ResolveAsSelfAttribute)) as ResolveAsSelfAttribute;
            if (resolveAsSelf != null) return new ReflectionFactory(serviceType);

            var resolveAs = serviceTypeInfo.GetCustomAttribute(typeof(ResolveAsAttribute)) as ResolveAsAttribute;
            if (resolveAsSelf != null) return new ReflectionFactory(resolveAs.ServiceType);

            return null; // not for me...
        }
    }
}
