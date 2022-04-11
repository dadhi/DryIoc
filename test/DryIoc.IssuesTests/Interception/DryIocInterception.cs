using System;
using Castle.DynamicProxy;
using DryIoc.ImTools;

namespace DryIoc.Interception
{
    // Extension methods for interceptor registration using Castle Dynamic Proxy.
    public static class DryIocInterception
    {
        public static void Intercept<TService, TInterceptor>(this IRegistrator registrator, object serviceKey = null)
            where TInterceptor : class, IInterceptor => 
            registrator.Intercept<TInterceptor>(typeof(TService), serviceKey);

        public static void Intercept<TInterceptor>(this IRegistrator registrator, Type serviceType, object serviceKey = null)
            where TInterceptor : class, IInterceptor =>
            registrator.Intercept(serviceType, Parameters.Of.Type(typeof(IInterceptor[]), typeof(TInterceptor[])), serviceKey);

        public static void Intercept(this IRegistrator registrator,
            Type serviceType, ParameterSelector interceptorsParameterSelector, object serviceKey = null)
        {
            Type proxyType;
            if (serviceType.IsInterface)
                proxyType = ProxyBuilder.CreateInterfaceProxyTypeWithTargetInterface(
                    serviceType, ArrayTools.Empty<Type>(), ProxyGenerationOptions.Default);
            else if (serviceType.IsClass)
                proxyType = ProxyBuilder.CreateClassProxyTypeWithTarget(
                    serviceType, ArrayTools.Empty<Type>(), ProxyGenerationOptions.Default);
            else
                throw new ArgumentException(
                    $"Intercepted service type {serviceType} is not a supported, cause it is nor a class nor an interface");

            registrator.Register(serviceType, proxyType,
                made: Made.Of(
                    pt => pt.PublicConstructors().FindFirst(ctor => ctor.GetParameters().Length != 0),
                    interceptorsParameterSelector),
                setup: Setup.DecoratorOf(useDecorateeReuse: true, decorateeServiceKey: serviceKey));
        }

        private static DefaultProxyBuilder ProxyBuilder => _proxyBuilder ?? (_proxyBuilder = new DefaultProxyBuilder());
        private static DefaultProxyBuilder _proxyBuilder;
    }
}
