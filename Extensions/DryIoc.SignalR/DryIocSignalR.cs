/*
The MIT License (MIT)

Copyright (c) 2014 Maksim Volkau

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

namespace DryIoc.SignalR
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Core.Interceptor;
    using Castle.DynamicProxy;
    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Hubs;

    /// <summary>DryIoc extension to support SignalR.</summary>
    public static class DryIocSignalR
    {
        /// <summary>
        /// </summary>
        public static IContainer WithSignalR(this IContainer container, 
            HubConfiguration hubConfiguration = null,
            IScopeContext scopeContext = null)
        {
            if (container.ScopeContext == null)
                container = container.With(
                    rules => rules.WithoutThrowOnRegisteringDisposableTransient(),
                    scopeContext ?? new AsyncExecutionFlowScopeContext());

            hubConfiguration = hubConfiguration ?? new HubConfiguration();
            hubConfiguration.Resolver = new DryIocDependencyResolver(container);
            return container;
        }
    }

    /// <summary>
    /// </summary>
    public sealed class DryIocDependencyResolver : DefaultDependencyResolver
    {
        /// <summary>
        /// </summary>
        public DryIocDependencyResolver(IContainer container)
        {
            _container = container;

            container.Register<IHubActivator, DryIocHubActivator>();
            container.RegisterInstance(container);

            container.Register<HubToCloseScopeOnDisposeInterceptor>(Reuse.Singleton);
            container.RegisterInterfaceInterceptor<IHub, HubToCloseScopeOnDisposeInterceptor>();
        }

        /// <summary>
        /// </summary>
        public override object GetService(Type serviceType)
        {
            return _container.Resolve(serviceType, IfUnresolved.ReturnDefault)
                ?? base.GetService(serviceType);
        }

        /// <summary>
        /// </summary>
        public override IEnumerable<object> GetServices(Type serviceType)
        {
            var services = _container.Resolve<object[]>(serviceType);
            var baseServices = base.GetServices(serviceType);

            return baseServices != null
                ? services.Concat(baseServices)
                : services.Length != 0 ? services
                : null;
        }

        /// <summary>
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _container.Dispose();
            base.Dispose(disposing);
        }

        private readonly IContainer _container;
    }

    /// <summary>
    /// </summary>
    public sealed class DryIocHubActivator : IHubActivator
    {
        private readonly IContainer _resolver;

        /// <summary>
        /// </summary>
        public DryIocHubActivator(IContainer resolver)
        {
            _resolver = resolver;
        }

        /// <summary>
        /// </summary>
        public IHub Create(HubDescriptor descriptor)
        {
            _resolver.OpenScope();
            var hub = _resolver.Resolve<IHub>(descriptor.HubType);
            return hub;
        }
    }

    /// <summary>
    /// </summary>
    public static class DryIocInterceptionTools
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TInterceptor"></typeparam>
        /// <param name="registrator"></param>
        public static void RegisterInterfaceInterceptor<TService, TInterceptor>(this IRegistrator registrator)
            where TInterceptor : class, IInterceptor
        {
            var serviceType = typeof(TService);
            if (!serviceType.IsInterface)
                throw new ArgumentException(string.Format("Intercepted service type {0} is not an interface", serviceType));

            var proxyType = ProxyBuilder.Value.CreateInterfaceProxyTypeWithTargetInterface(
                serviceType, ArrayTools.Empty<Type>(), ProxyGenerationOptions.Default);

            registrator.Register(serviceType, proxyType,
                made: Parameters.Of.Type<IInterceptor[]>(typeof(TInterceptor[])),
                setup: Setup.Decorator);
        }

        private static readonly Lazy<DefaultProxyBuilder> ProxyBuilder = new Lazy<DefaultProxyBuilder>(() => new DefaultProxyBuilder());
    }

    /// <summary>
    /// </summary>
    public sealed class HubToCloseScopeOnDisposeInterceptor : IInterceptor
    {
        /// <summary>
        /// </summary>
        public HubToCloseScopeOnDisposeInterceptor(IContainer container)
        {
            _container = container;
        }

        /// <summary>
        /// </summary>
        /// <param name="invocation"></param>
        public void Intercept(IInvocation invocation)
        {
            var method = invocation.Method;
            if (method.Name != "Dispose" &&
                method.GetParameters().IndexOf(p => p.ParameterType == typeof(bool)) != -1)
            {
                _container.ScopeContext.SetCurrent(scope =>
                {
                    if (scope == null) return null;
                    var parent = scope.Parent;
                    scope.Dispose();
                    return parent;
                });
            }
            invocation.Proceed();
        }

        private readonly IContainer _container;
    }

}
