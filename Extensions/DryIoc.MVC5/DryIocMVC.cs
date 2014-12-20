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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace DryIoc.MVC5
{
    internal static class MvcExtensions
    {
        public static IContainer WithMvcSupport(this IContainer container)
        {
            container = container.ThrowIfNull().With(scopeContext: new HttpContextScopeContext());
            DependencyResolver.SetResolver(new DryIocDependencyResolver(container));
            return container.RegisterControllers().ResolveFilterAttributeFilterProviders();
        }

        public static IContainer RegisterControllers(this IContainer container, params Assembly[] assemblies)
        {
            foreach (var type in assemblies.SelectMany(assembly =>
                Polyfill.GetTypesFrom(assembly).Where(t => !t.IsAbstract && t.IsAssignableTo(typeof(IController)))))
                container.Register(type, Reuse.InHttpContext);
            return container;
        }

        public static IContainer ResolveFilterAttributeFilterProviders(this IContainer container)
        {
            var providers = FilterProviders.Providers;
            foreach (var provider in providers.OfType<FilterAttributeFilterProvider>().ToArray())
                providers.Remove(provider);
            providers.Add(new DryIocFilterAttributeFilterProvider(container));
            return container;
        }
    }

    public class DryIocDependencyResolver : IDependencyResolver
    {
        public DryIocDependencyResolver(IResolver resolver)
        {
            _resolver = resolver.ThrowIfNull();
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

    internal class DryIocFilterAttributeFilterProvider : FilterAttributeFilterProvider
    {
        public DryIocFilterAttributeFilterProvider(IContainer container)
        {
            _container = container;
            _container.RegisterInstance<IFilterProvider>(this);
        }

        public override IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            var filters = base.GetFilters(controllerContext, actionDescriptor).ToArray();
            foreach (var filter in filters)
                _container.ResolvePropertiesAndFields(filter.Instance);
            return filters;
        }

        private readonly IContainer _container;
    }

    public static class Reuse
    {
        public static readonly IReuse InHttpContext = DryIoc.Reuse.InCurrentNamedScope(HttpContextScopeContext.ROOT_SCOPE_NAME);
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
