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
    using System.Runtime.Remoting.Messaging;
    using System.Web.Http;
    using System.Web.Http.Dependencies;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    internal static class DryIocWebApi
    {
        public static IContainer WithWebApiSupport(this IContainer container, HttpConfiguration httpConfiguration, params Assembly[] assemblies)
        {
            container = container.ThrowIfNull().With(scopeContext: new ExecutionFlowScopeContext());

            assemblies = !assemblies.IsNullOrEmpty() ? assemblies : new[] { Assembly.GetExecutingAssembly() };
            container.RegisterFromAssembly<IHttpController>(WebReuse.InRequest, assemblies);

            container.SetFilterProvider(httpConfiguration.Services);

            httpConfiguration.DependencyResolver = new DryIocDependencyResolver(container);

            return container;
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

    internal class DryIocAggregatedFilterProvider : IFilterProvider
    {
        public DryIocAggregatedFilterProvider(IResolver resolver, IEnumerable<IFilterProvider> providers)
        {
            _resolver = resolver;
            _providers = providers;
        }

        public IEnumerable<FilterInfo> GetFilters(HttpConfiguration configuration, HttpActionDescriptor actionDescriptor)
        {
            var filters = _providers.SelectMany(p => p.GetFilters(configuration, actionDescriptor)).ToArray();
            for (var i = 0; i < filters.Length; i++)
                _resolver.ResolvePropertiesAndFields(filters[i].Instance);
            return filters;
        }

        private readonly IResolver _resolver;
        private readonly IEnumerable<IFilterProvider> _providers;
    }

    public static class WebReuse
    {
        public static readonly IReuse InRequest = Reuse.InCurrentNamedScope(ExecutionFlowScopeContext.ROOT_SCOPE_NAME);
    }

    public sealed class ExecutionFlowScopeContext : IScopeContext
    {
        public static readonly object ROOT_SCOPE_NAME = typeof(ExecutionFlowScopeContext);

        public object RootScopeName { get { return ROOT_SCOPE_NAME; } }

        public IScope GetCurrentOrDefault()
        {
            var scope = (Copyable<IScope>)CallContext.LogicalGetData(_key);
            return scope == null ? null : scope.Value;
        }

        public void SetCurrent(Func<IScope, IScope> update)
        {
            var oldScope = GetCurrentOrDefault();
            var newScope = update.ThrowIfNull()(oldScope);
            CallContext.LogicalSetData(_key, new Copyable<IScope>(newScope));
        }

        #region Implementation

        private static readonly string _key = typeof(ExecutionFlowScopeContext).Name;

        [Serializable]
        private sealed class Copyable<T> : MarshalByRefObject
        {
            public readonly T Value;

            public Copyable(T value)
            {
                Value = value;
            }
        }

        #endregion
    }
}
