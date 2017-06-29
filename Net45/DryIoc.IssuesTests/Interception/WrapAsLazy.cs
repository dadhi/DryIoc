using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using ImTools;

namespace DryIoc.IssuesTests.Interception
{
    // Extension methods for wrapping dependencies as forcible lazy using Castle Dynamic Proxy.
    public static class WrapAsLazy
    {
        /// <summary>
        /// Registers a service that is always resolved as lazy wrapper.
        /// </summary>
        /// <typeparam name="TInterface">The type of the interface.</typeparam>
        /// <typeparam name="TClass">The type of the class.</typeparam>
        /// <param name="registrator">The registrator.</param>
        public static IRegistrator RegisterAsLazy<TInterface, TClass>(this IRegistrator registrator)
            where TInterface : class
            where TClass : TInterface
        {
            // perform normal registration
            registrator.Register<TInterface, TClass>();

            // registration of lazy interceptor
            registrator.Register(typeof(LazyInterceptor<>), ifAlreadyRegistered: IfAlreadyRegistered.Keep);

            // lazy proxy wrapper
            var proxyBuilder = new DefaultProxyBuilder();
            var proxyType = proxyBuilder.CreateInterfaceProxyTypeWithTargetInterface(typeof(TInterface),
                ArrayTools.Empty<Type>(), ProxyGenerationOptions.Default);

            // decorator for the generated proxy class
            var decoratorSetup = Setup.DecoratorWith(useDecorateeReuse: true);
            registrator.Register(typeof(TInterface), proxyType,
                setup: decoratorSetup,
                made: Made.Of(type => type.GetPublicInstanceConstructors().SingleOrDefault(constr => constr.GetParameters().Length != 0),
                    parameters: Parameters.Of
                        .Type<IInterceptor[]>(typeof(LazyInterceptor<TInterface>[]))
                        .Type<TInterface>(r => null)));

            return registrator;
        }

        /// <summary>
        /// Ensures that a service always resolves as lazy proxy.
        /// </summary>
        /// <typeparam name="TInterface">The type of the interface.</typeparam>
        /// <param name="registrator">The c.</param>
        /// <returns></returns>
        public static IRegistrator ResolveAsLazy<TInterface>(this IRegistrator registrator)
            where TInterface : class
        {
            // skip the service registration, assume it already exists
            // registration of lazy interceptor
            registrator.Register(typeof(LazyInterceptor<>), ifAlreadyRegistered: IfAlreadyRegistered.Keep);

            // lazy proxy wrapper
            var proxyBuilder = new DefaultProxyBuilder();
            var proxyType = proxyBuilder.CreateInterfaceProxyTypeWithTargetInterface(typeof(TInterface),
                ArrayTools.Empty<Type>(), ProxyGenerationOptions.Default);

            // decorator for the generated proxy class
            var decoratorSetup = Setup.DecoratorWith(useDecorateeReuse: true);
            registrator.Register(typeof(TInterface), proxyType,
                setup: decoratorSetup,
                made: Made.Of(type => type.GetPublicInstanceConstructors().SingleOrDefault(constr => constr.GetParameters().Length != 0),
                    parameters: Parameters.Of
                        .Type<IInterceptor[]>(typeof(LazyInterceptor<TInterface>[]))
                        .Type<TInterface>(r => null)));

            return registrator;
        }

        private class LazyInterceptor<T> : IInterceptor
            where T : class
        {
            public LazyInterceptor(Lazy<T> lazyTarget)
            {
                LazyTarget = lazyTarget;
            }

            private Lazy<T> LazyTarget { get; }

            public void Intercept(IInvocation invocation)
            {
                var target = invocation.InvocationTarget as T;
                if (target == null)
                {
                    // create the lazy value on the first invocation
                    (invocation as IChangeProxyTarget).ChangeInvocationTarget(LazyTarget.Value);
                    (invocation as IChangeProxyTarget).ChangeProxyTarget(LazyTarget.Value);
                }

                invocation.Proceed();
            }
        }
    }
}
