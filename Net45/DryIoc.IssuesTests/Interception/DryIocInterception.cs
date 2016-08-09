using System;
using System.Linq;
using Castle.DynamicProxy;

namespace DryIoc.Interception
{
    // Extension methods for interceptor registration using Castle Dynamic Proxy.
    public static class DryIocInterception
    {
        private static readonly Lazy<DefaultProxyBuilder> ProxyBuilder =
            new Lazy<DefaultProxyBuilder>(() => new DefaultProxyBuilder());

        public static void InterceptInterface<TServiceInterface, TInterceptor>(this IRegistrator registrator)
            where TInterceptor : class, IInterceptor
        {
            var serviceType = typeof(TServiceInterface);
            if (!serviceType.IsInterface)
                throw new ArgumentException(string.Format("Intercepted service type {0} is not an interface", serviceType));

            var proxyType = ProxyBuilder.Value.CreateInterfaceProxyTypeWithTargetInterface(
                serviceType, ArrayTools.Empty<Type>(), ProxyGenerationOptions.Default);

            registrator.Register(serviceType, proxyType,
                made: Parameters.Of.Type<IInterceptor[]>(typeof(TInterceptor[])),
                setup: Setup.Decorator);
        }

        public static void InterceptClass<TService, TInterceptor>(this IRegistrator registrator, object serviceKey = null) 
            where TInterceptor : class, IInterceptor
        {
            var serviceType = typeof(TService);
            if (!serviceType.IsClass)
                throw new ArgumentException(string.Format("Intercepted service type {0} is not a class", serviceType));

            var proxyType = ProxyBuilder.Value.CreateClassProxyType(
                serviceType, ArrayTools.Empty<Type>(), ProxyGenerationOptions.Default);

            var decoratorSetup = serviceKey == null
                ? Setup.Decorator
                : Setup.DecoratorWith(r => serviceKey.Equals(r.ServiceKey));

            registrator.Register(serviceType, proxyType,
                made: Made.Of(type => type.GetPublicInstanceConstructors().SingleOrDefault(c => c.GetParameters().Length != 0), 
                    Parameters.Of.Type<IInterceptor[]>(typeof(TInterceptor[]))), 
                setup: decoratorSetup);
        }
    }
}