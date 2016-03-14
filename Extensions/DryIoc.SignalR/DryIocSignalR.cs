/*
The MIT License (MIT)

Copyright (c) 2016 Maksim Volkau

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
    using System.Threading.Tasks;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.AspNet.SignalR;
    using Microsoft.AspNet.SignalR.Hubs;

    /// <summary>DryIoc extension to support SignalR.
    /// Provides DryIoc implementations of <see cref="IHubActivator"/> and <see cref="IDependencyResolver"/>.
    /// </summary>
    public static class DryIocSignalR
    {
        /// <summary>Adapts container for use with SignalR implicit convention:
        ///   1) Turns on Transient Disposable tracking; 
        ///   2) Sets reuse ambient context to <see cref="AsyncExecutionFlowScopeContext"/>.
        /// Registers <see cref="DryIocDependencyResolver"/> as <see cref="IDependencyResolver"/>
        /// and registers <see cref="DryIocHubActivator"/> as <see cref="IHubActivator"/>.
        /// You can resolve the abstractions from returned container.
        /// Related discussion and more info may be found here: https://stackoverflow.com/questions/10555791/using-simple-injector-with-signalr
        /// </summary>
        /// <param name="container">Container to adapt for SignalR.</param>
        /// <param name="scopeContext">(optional) Ambient scope context alternative to <see cref="AsyncExecutionFlowScopeContext"/>.
        /// Generally the ambient scope context is required for <see cref="DryIocHubActivator"/> to produce scope bound hubs.</param>
        /// <returns>Adapted container. It will be different from passed container.</returns>
        /// <example> <code lang="cs"><![CDATA[
        ///
        ///     // - Approach 1: with DryIocDependecyResolver
        ///     container = new Container().WithSignalR();
        ///     RouteTable.Routes.MapHubs(); // should go before setting the resolver, check SO link above for reasoning
        ///     GlobalHost.DependencyResolver = container.Resolve<IDependencyResolver>();
        ///
        ///     // Possible way to register Hubs:
        ///     container.RegisterMany(new[] { Assembly.GetExecutingAssembly() }, 
        ///         serviceTypeCondition: type => type.BaseType == typeof(Hub));
        ///
        ///     
        ///     // - Approach 2: Just use DryIocHubActivator with default DependencyResolver 
        ///     container = new Container().WithSignalR();
        ///     GlobalHost.DependencyResolver.Register(typeof(IHubActivator), () => new DryIocHubActivator(container));
        ///     RouteTable.Routes.MapHubs();
        ///
        ///  ]]></code></example>
        public static IContainer WithSignalR(this IContainer container, IScopeContext scopeContext = null)
        {
            if (container.ScopeContext == null)
                container = container.With(rules => rules
                    .WithTrackingDisposableTransients(),
                    scopeContext ?? new AsyncExecutionFlowScopeContext());

            container.Register<IHubActivator, DryIocHubActivator>();
            container.Register<IDependencyResolver, DryIocDependencyResolver>(Reuse.Singleton);

            return container;
        }
    }

    /// <summary>DryIoc implementation of <see cref="IDependencyResolver"/>.
    /// It uses <see cref="DefaultDependencyResolver"/> and combines directly registered services and
    /// default services on resolution.</summary>
    [SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible",
        Justification = "Not available in PCL.")]
    public sealed class DryIocDependencyResolver : DefaultDependencyResolver
    {
        /// <summary>Created resolver given DryIoc resolver.</summary>
        public DryIocDependencyResolver(IResolver resolver)
        {
            _resolver = resolver;
        }

        /// <summary>Try to resolve service suing DryIoc container, 
        /// and if not resolved fallbacks to base <see cref="DefaultDependencyResolver"/>.</summary>
        public override object GetService(Type serviceType)
        {
            return _resolver.Resolve(serviceType, IfUnresolved.ReturnDefault)
                ?? base.GetService(serviceType);
        }

        /// <summary>Combines services from DryIoc container and base <see cref="DependencyResolverExtensions"/>
        /// and returns in a single collection.</summary>
        public override IEnumerable<object> GetServices(Type serviceType)
        {
            var services = _resolver.Resolve<object[]>(serviceType);
            var baseServices = base.GetServices(serviceType);

            return baseServices != null
                ? services.Concat(baseServices)
                : services.Length != 0 ? services
                : null;
        }

        /// <summary>Disposes DryIoc container at the end.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                var disposable = _resolver as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
            base.Dispose(disposing);
        }

        private readonly IResolver _resolver;
    }

    /// <summary>Implements <see cref="IHubActivator"/>, 
    /// additionally before returning hub opens the scope to start Unit-of-Work,
    /// and disposes the scope together on the Hub dispose.</summary>
    public sealed class DryIocHubActivator : IHubActivator
    {
        /// <summary>Creates activator sing provided container.</summary> 
        /// <param name="container">Container to resolve hubs and open scope.</param>
        public DryIocHubActivator(IContainer container)
        {
            _container = container;
        }

        /// <summary>Opens scope via <see cref="IContainer.OpenScope"/> to start unit-of-work:
        /// so that <see cref="Reuse.InWebRequest"/> and <see cref="Reuse.InCurrentScope"/> hub dependencies are supported.
        /// Then creates hub by using <paramref name="descriptor"/> info. 
        /// Wraps the hub in <see cref="HubProxy"/> decorator, to hook disposing of open scope on disposing of hub.
        /// Returns the hub decorator.
        /// </summary>
        public IHub Create(HubDescriptor descriptor)
        {
            var scope = _container.OpenScope(Reuse.WebRequestScopeName);
            return new HubProxy(_container.Resolve<IHub>(descriptor.HubType), scope);
        }

        private readonly IContainer _container;

        internal sealed class HubProxy : IHub
        {
            public HubCallerContext Context
            {
                get { return _hub.Context; }
                set { _hub.Context = value; }
            }

            public IHubCallerConnectionContext<dynamic> Clients
            {
                get { return _hub.Clients; }
                set { _hub.Clients = value; }
            }

            public IGroupManager Groups
            {
                get { return _hub.Groups; }
                set { _hub.Groups = value; }
            }

            public HubProxy(IHub hub, IContainer scopedContainer)
            {
                _hub = hub;
                _scopedContainer = scopedContainer;
            }

            public void Dispose()
            {
                _scopedContainer.Dispose();
                _hub.Dispose();
            }

            public Task OnConnected()
            {
                return _hub.OnConnected();
            }

            public Task OnReconnected()
            {
                return _hub.OnReconnected();
            }

            public Task OnDisconnected(bool stopCalled)
            {
                return _hub.OnDisconnected(stopCalled);
            }

            private readonly IHub _hub;
            private readonly IContainer _scopedContainer;
        }
    }
}
