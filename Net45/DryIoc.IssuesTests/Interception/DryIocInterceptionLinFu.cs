using System;
using System.Linq;
using Castle.DynamicProxy;
using ImTools;

namespace DryIoc.Interception.LinFu
{
    // Extension methods for interceptor registration using Castle Dynamic Proxy.
    public static class DryIocInterception
    {
        public static void Intercept<TService, TInterceptor>(this IRegistrator registrator, object serviceKey = null)
            where TInterceptor : class, IInterceptor
        {
            var serviceType = typeof(TService);

            Type proxyType;
            if (serviceType.IsInterface())
                proxyType = ProxyBuilder.CreateInterfaceProxyTypeWithTargetInterface(
                    serviceType, ArrayTools.Empty<Type>(), ProxyGenerationOptions.Default);
            else if (serviceType.IsClass())
                proxyType = ProxyBuilder.CreateClassProxyType(
                    serviceType, ArrayTools.Empty<Type>(), ProxyGenerationOptions.Default);
            else
                throw new ArgumentException(string.Format(
                    "Intercepted service type {0} is not a supported: it is nor class nor interface", serviceType));

            var decoratorSetup = serviceKey == null
                ? Setup.DecoratorWith(useDecorateeReuse: true)
                : Setup.DecoratorWith(r => serviceKey.Equals(r.ServiceKey), useDecorateeReuse: true);

            registrator.Register(serviceType, proxyType,
                made: Made.Of(type => type.PublicConstructors().SingleOrDefault(c => c.GetParameters().Length != 0),
                    Parameters.Of.Type<IInterceptor[]>(typeof(TInterceptor[]))),
                setup: decoratorSetup);
        }

        private static DefaultProxyBuilder ProxyBuilder => _proxyBuilder ?? (_proxyBuilder = new DefaultProxyBuilder());

        private static DefaultProxyBuilder _proxyBuilder;
    }
}