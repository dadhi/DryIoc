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
    using System.Web.Http;
    using System.Web.Http.Dependencies;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    public static class DryIocWebApi
    {
        public static IContainer WithWebApi(this IContainer container, HttpConfiguration config,
            IEnumerable<Assembly> controllerAssemblies = null)
        {
            container = container.ThrowIfNull().With(scopeContext: new AsyncExecutionFlowScopeContext());

            container.RegisterHttpControllers(controllerAssemblies);

            container.SetFilterProvider(config.Services);

            config.DependencyResolver = new DryIocDependencyResolver(container);

            return container;
        }

        public static void RegisterHttpControllers(this IContainer container, IEnumerable<Assembly> controllerAssemblies = null)
        {
            controllerAssemblies = controllerAssemblies ?? new[] { Assembly.GetExecutingAssembly() };
            container.RegisterMany(controllerAssemblies, typeof(IHttpController), ReuseInWeb.Request);
        }

        public static void SetFilterProvider(this IContainer container, ServicesContainer services)
        {
            var providers = services.GetFilterProviders();
            services.RemoveAll(typeof(IFilterProvider), _ => true);
            var filterProvider = new DryIocAggregatedFilterProvider(container, providers);
            services.Add(typeof(IFilterProvider), filterProvider);
            container.RegisterInstance<IFilterProvider>(filterProvider);
        }
    }

    public static class ReuseInWeb
    {
        public static readonly IReuse Request =
            Reuse.InCurrentNamedScope(AsyncExecutionFlowScopeContext.ROOT_SCOPE_NAME);
    }

    internal class DryIocDependencyResolver : IDependencyResolver
    {
        internal DryIocDependencyResolver(IContainer container)
        {
            _container = container;
        }

        public void Dispose()
        {
            if (_container == null) return;
            _container.Dispose();
            _container = null;
        }

        public object GetService(Type serviceType)
        {
            return _container.Resolve(serviceType, IfUnresolved.ReturnDefault);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _container.ResolveMany<object>(serviceType);
        }

        public IDependencyScope BeginScope()
        {
            return new DryIocDependencyScope(_container.OpenScope());
        }

        private IContainer _container;
    }

    internal class DryIocDependencyScope : IDependencyScope
    {
        public DryIocDependencyScope(IContainer scopedContainer)
        {
            _scopedContainer = scopedContainer;
        }

        public void Dispose()
        {
            if (_scopedContainer == null) return;
            _scopedContainer.Dispose();
            _scopedContainer = null;
        }


        public object GetService(Type serviceType)
        {
            return _scopedContainer.Resolve(serviceType, IfUnresolved.ReturnDefault);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _scopedContainer.ResolveMany<object>(serviceType);
        }

        private IContainer _scopedContainer;
    }

    public class DryIocAggregatedFilterProvider : IFilterProvider
    {
        public DryIocAggregatedFilterProvider(IContainer container, IEnumerable<IFilterProvider> providers)
        {
            _container = container;
            _providers = providers;
        }

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
}
