/*
The MIT License (MIT)

Copyright (c) 2013 Maksim Volkau

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

namespace DryIoc.WebApi
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Dependencies;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;
    using System.Net.Http;

    /// <summary>WebApi DI bootstrapper with DryIoc. </summary>
    public static class DryIocWebApi
    {
        /// <summary>Configures container to work with ASP.NET WepAPI by: 
        /// setting container scope context to <see cref="AsyncExecutionFlowScopeContext"/>,
        /// registering HTTP controllers, setting filter provider and dependency resolver.</summary>
        /// <param name="container">Original container.</param> <param name="config">Http configuration.</param>
        /// <param name="controllerAssemblies">(optional) Assemblies to look for controllers, default is Executing Assembly.</param>
        /// <param name="scopeContext">(optional) Specific scope context to use, if not specified using
        /// <see cref="AsyncExecutionFlowScopeContext"/> as default in NET 4.5. scope context.</param>
        /// <returns>New container.</returns>
        public static IContainer WithWebApi(this IContainer container, HttpConfiguration config, 
            IEnumerable<Assembly> controllerAssemblies = null, IScopeContext scopeContext = null)
        {
            container.ThrowIfNull();

            if (scopeContext != null)
                container = container.With(scopeContext: scopeContext);
            else if (!(container.ScopeContext is AsyncExecutionFlowScopeContext))
                container = container.With(scopeContext: new AsyncExecutionFlowScopeContext());

            container.RegisterHttpControllers(controllerAssemblies);

            container.SetFilterProvider(config.Services);

            InsertRegisterRequestMessageHandler(config);

            config.DependencyResolver = new DryIocDependencyResolver(container);

            return container;
        }

        /// <summary>Registers controllers found in provided assemblies with per-request reuse.</summary>
        /// <param name="container">Container.</param>
        /// <param name="controllerAssemblies">Assemblies to look for controllers.</param>
        public static void RegisterHttpControllers(this IContainer container, IEnumerable<Assembly> controllerAssemblies = null)
        {
            container.ThrowIfNull();
            controllerAssemblies = controllerAssemblies ?? new[] { Assembly.GetExecutingAssembly() };
            container.RegisterMany(controllerAssemblies, typeof(IHttpController), Reuse.InRequest);
        }

        /// <summary>Replaces all filter providers in services with <see cref="DryIocFilterProvider"/>, and registers it in container.</summary>
        /// <param name="container">DryIoc container.</param> <param name="services">Services</param>
        public static void SetFilterProvider(this IContainer container, ServicesContainer services)
        {
            var providers = services.GetFilterProviders();
            services.RemoveAll(typeof(IFilterProvider), _ => true);
            var filterProvider = new DryIocFilterProvider(container, providers);
            services.Add(typeof(IFilterProvider), filterProvider);
            container.RegisterInstance<IFilterProvider>(filterProvider);
        }

        /// <summary>Inserts DryIoc delegating request handler into message handlers.</summary>
        /// <param name="config">Current configuration.</param>
        public static void InsertRegisterRequestMessageHandler(HttpConfiguration config)
        {
            var handlers = config.ThrowIfNull().MessageHandlers;
            if (!handlers.Any(h => h is RegisterRequestMessageHandler))
                handlers.Insert(0, new RegisterRequestMessageHandler());
        }
    }

    /// <summary>Defines per request scope reuse bound to <see cref="AsyncExecutionFlowScopeContext"/>.</summary>
    public static class Reuse
    {
        /// <summary>Reuse object. Actually it is a reuse in top current scope of context.</summary>
        public static readonly IReuse InRequest =
            DryIoc.Reuse.InCurrentNamedScope(AsyncExecutionFlowScopeContext.ROOT_SCOPE_NAME);
    }

    /// <summary>Resolve based on DryIoc container.</summary>
    public sealed class DryIocDependencyResolver : IDependencyResolver
    {
        /// <summary>Creates dependency resolver.</summary> <param name="container">Container.</param>
        internal DryIocDependencyResolver(IContainer container)
        {
            _container = container;
        }

        /// <summary>Disposes container.</summary>
        public void Dispose()
        {
            if (_container == null) return;
            _container.Dispose();
            _container = null;
        }

        /// <summary>Retrieves a service from the scope or null if unable to resolve service.</summary>
        /// <returns>The retrieved service.</returns> <param name="serviceType">The service to be retrieved.</param>
        public object GetService(Type serviceType)
        {
            return _container.Resolve(serviceType, IfUnresolved.ReturnDefault);
        }

        /// <summary>Retrieves a collection of services from the scope or empty collection.</summary>
        /// <returns>The retrieved collection of services.</returns>
        /// <param name="serviceType">The collection of services to be retrieved.</param>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _container.ResolveMany<object>(serviceType);
        }

        /// <summary>Opens scope from underlying container.</summary>
        /// <returns>Opened scope wrapped in dependency scope.</returns>
        public IDependencyScope BeginScope()
        {
            return new DryIocDependencyScope(_container.OpenScope());
        }

        private IContainer _container;
    }

    /// <summary>Dependency scope adapter to scoped DryIoc container (created by <see cref="IContainer.OpenScope"/>).</summary>
    public sealed class DryIocDependencyScope : IDependencyScope
    {
        /// <summary>Wrapped DryIoc container.</summary>
        public IContainer Container { get; private set; }

        /// <summary>Adapts input container.</summary> <param name="scopedContainer">Container returned by OpenScope method.</param>
        public DryIocDependencyScope(IContainer scopedContainer)
        {
            Container = scopedContainer;
        }

        /// <summary>Disposed underlying scoped container.</summary>
        public void Dispose()
        {
            if (Container == null) return;
            Container.Dispose();
            Container = null;
        }

        /// <summary>Retrieves a service from the scope or returns null if not resolved.</summary>
        /// <returns>The retrieved service.</returns> <param name="serviceType">The service to be retrieved.</param>
        public object GetService(Type serviceType)
        {
            return Container.Resolve(serviceType, IfUnresolved.ReturnDefault);
        }

        /// <summary>Retrieves a collection of services from the scope or empty collection.</summary>
        /// <returns>The retrieved collection of services.</returns>
        /// <param name="serviceType">The collection of services to be retrieved.</param>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            return Container.ResolveMany<object>(serviceType);
        }
    }

    /// <summary>Aggregated filter provider.</summary>
    public sealed class DryIocFilterProvider : IFilterProvider
    {
        /// <summary>Creates filter provider.</summary>
        /// <param name="container"></param> <param name="providers"></param>
        public DryIocFilterProvider(IContainer container, IEnumerable<IFilterProvider> providers)
        {
            _container = container;
            _providers = providers;
        }

        /// <summary> Returns an enumeration of filters. </summary>
        /// <returns> An enumeration of filters. </returns>
        /// <param name="configuration">The HTTP configuration.</param><param name="actionDescriptor">The action descriptor.</param>
        public IEnumerable<FilterInfo> GetFilters(HttpConfiguration configuration, HttpActionDescriptor actionDescriptor)
        {
            var filters = _providers.SelectMany(p => p.GetFilters(configuration, actionDescriptor)).ToArray();
            for (var i = 0; i < filters.Length; i++)
                _container.InjectPropertiesAndFields(filters[i].Instance);
            return filters;
        }

        private readonly IContainer _container;
        private readonly IEnumerable<IFilterProvider> _providers;
    }

    /// <summary>Registers current <see cref="HttpRequestMessage"/> into dependency scope.</summary>
    internal sealed class RegisterRequestMessageHandler : DelegatingHandler
    {
        /// <summary>Registers request into dependency scope and sends proceed the pipeline.</summary> 
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RegisterInDependencyScope(request);
            return base.SendAsync(request, cancellationToken);
        }

        /// <summary>Registers request into current dependency scope.</summary>
        /// <param name="request">Request to register.</param>
        public void RegisterInDependencyScope(HttpRequestMessage request)
        {
            var dependencyScope = request.ThrowIfNull().GetDependencyScope();

            dependencyScope.ThrowIfNotOf(typeof(DryIocDependencyScope), 
                Error.REQUEST_MESSAGE_DOESNOT_REFERENCE_DRYIOC_DEPENDENCY_SCOPE);
            
            var container = ((DryIocDependencyScope)dependencyScope).Container;
            container.RegisterInstance(request, Reuse.InRequest, IfAlreadyRegistered.Replace);
        }
    }

    /// <summary>Possible web exceptions.</summary>
    public static class Error
    {
#pragma warning disable 1591 // "Missing XML-comment"
        public static readonly int
            REQUEST_MESSAGE_DOESNOT_REFERENCE_DRYIOC_DEPENDENCY_SCOPE = DryIoc.Error.Of(
                "Expecting request message dependency scope to be of type {1} but found: {0}.");
#pragma warning restore 1591
    }
}
