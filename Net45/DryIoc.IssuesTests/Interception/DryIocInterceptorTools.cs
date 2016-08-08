using System;
using Castle.DynamicProxy;

namespace DryIoc.Interception
{
    // Extension methods for interceptor registration using Castle Dynamic Proxy.
    public static class DryIocInterceptorTools
    {
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

        private static readonly Lazy<DefaultProxyBuilder> ProxyBuilder = 
            new Lazy<DefaultProxyBuilder>(() => new DefaultProxyBuilder());
    }
}