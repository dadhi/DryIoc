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

namespace DryIoc.Mvc
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using System.Web;
    using System.Web.Mvc;

    /// <summary>
    /// <example> <code lang="cs"><![CDATA[
    /// protected void Application_Start()
    /// {
    ///     var container = new Container().WithMvcSupport(typeof(MyMvcApp).Assembly);
    ///     
    ///     // Optionally enable support for MEF Export/ImportAttribute with DryIoc.MefAttributedModel package. 
    ///     // container = container.WithMefAttributedModel();
    ///     // container.RegisterExports(typeof(MyMvcApp).Assembly);
    /// 
    ///     // If required register additional services to container here ...
    /// }
    /// ]]></code></example>
    /// </summary>
    public static class DryIocMvc
    {
        public static IContainer WithMvcSupport(this IContainer container, params Assembly[] assemblies)
        {
            container = container.ThrowIfNull().With(scopeContext: new HttpContextScopeContext());
            
            assemblies = !assemblies.IsNullOrEmpty() ? assemblies : new[] { Assembly.GetExecutingAssembly() };
            container.RegisterFromAssembly<IController>(WebReuse.InRequest, assemblies);

            container.SetFilterAttributeFilterProvider(FilterProviders.Providers);

            DependencyResolver.SetResolver(new DryIocDependencyResolver(container));

            return container;
        }

        public static void SetFilterAttributeFilterProvider(this IContainer container, Collection<IFilterProvider> filterProviders)
        {
            var filterAttributeFilterProviders = filterProviders.OfType<FilterAttributeFilterProvider>().ToArray();
            for (var i = filterAttributeFilterProviders.Length - 1; i >= 0; --i)
                filterProviders.RemoveAt(i);

            var filterProvider = new DryIocAggregatedFilterAttributeFilterProvider(container);
            filterProviders.Add(filterProvider);

            container.RegisterInstance<IFilterProvider>(filterProvider);
        }
    }

    public class DryIocDependencyResolver : IDependencyResolver
    {
        public DryIocDependencyResolver(IResolver resolver)
        {
            _resolver = resolver;
        }

        public object GetService(Type serviceType)
        {
            return _resolver.Resolve(serviceType, IfUnresolved.ReturnDefault);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _resolver.ResolveMany<object>(serviceType);
        }

        private readonly IResolver _resolver;
    }

    internal class DryIocAggregatedFilterAttributeFilterProvider : FilterAttributeFilterProvider
    {
        public DryIocAggregatedFilterAttributeFilterProvider(IResolver resolver)
        {
            _resolver = resolver;
        }

        public override IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            var filters = base.GetFilters(controllerContext, actionDescriptor).ToArray();
            for (var i = 0; i < filters.Length; i++)
                _resolver.ResolvePropertiesAndFields(filters[i].Instance);
            return filters;
        }

        private readonly IResolver _resolver;
    }

    public static class WebReuse
    {
        public static readonly IReuse InRequest = Reuse.InCurrentNamedScope(HttpContextScopeContext.ROOT_SCOPE_NAME);
    }

    public sealed class HttpContextScopeContext : IScopeContext
    {
        public static readonly object ROOT_SCOPE_NAME = typeof(HttpContextScopeContext);

        public object RootScopeName { get { return ROOT_SCOPE_NAME; } }

        public IScope GetCurrentOrDefault()
        {
            var httpContext = HttpContext.Current;
            return httpContext == null ? _fallbackScope : (IScope)httpContext.Items[ROOT_SCOPE_NAME];
        }

        public void SetCurrent(Func<IScope, IScope> update)
        {
            var currentOrDefault = GetCurrentOrDefault();
            var newScope = update.ThrowIfNull().Invoke(currentOrDefault);
            var httpContext = HttpContext.Current;
            if (httpContext == null)
            {
                _fallbackScope = newScope;
            }
            else
            {
                httpContext.Items[ROOT_SCOPE_NAME] = newScope;
                _fallbackScope = null;
            }
        }

        private IScope _fallbackScope;
    }
}
