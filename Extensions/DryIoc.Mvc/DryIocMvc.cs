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
    using System.Runtime.InteropServices;
    using System.Web.Mvc;
    using System.Web.Compilation;
    using Web;

    /// <summary>
    /// <example> <code lang="cs"><![CDATA[
    /// protected void Application_Start()
    /// {
    ///     var container = new Container().WithMvc();
    ///     
    ///     // Optionally enable support for MEF Export/ImportAttribute with DryIoc.MefAttributedModel package. 
    ///     // container = container.WithMefAttributedModel();
    ///     // container.RegisterExports(new[] { typeof(MyMvcApp).Assembly });
    /// 
    ///     // Additional registrations go here ...
    /// }
    /// ]]></code></example>
    /// </summary>
    public static class DryIocMvc
    {
        /// <summary>Returns all referenced assemblies except from GAC and dynamic.</summary>
        /// <returns>Assemblies.</returns>
        public static IEnumerable<Assembly> GetReferencedAssemblies()
        {
            return BuildManager.GetReferencedAssemblies().OfType<Assembly>()
                .Where(a => !a.IsDynamic && !a.GlobalAssemblyCache);
        }

        /// <summary>Creates new container from original one with <see cref="HttpContextScopeContext"/>.
        /// Then registers MVC controllers in container, 
        /// sets <see cref="DryIocAggregatedFilterAttributeFilterProvider"/> as filter provider,
        /// and at last sets container as <see cref="DependencyResolver"/>.</summary>
        /// <param name="container">Original container.</param>
        /// <param name="controllerAssemblies">(optional) By default uses <see cref="GetReferencedAssemblies"/>.</param>
        /// <returns>New container with applied Web context.</returns>
        public static IContainer WithMvc(this IContainer container, IEnumerable<Assembly> controllerAssemblies = null)
        {
            container = container.ThrowIfNull().With(scopeContext: new HttpContextScopeContext());

            container.RegisterMvcControllers(controllerAssemblies);

            container.SetFilterAttributeFilterProvider(FilterProviders.Providers);

            DependencyResolver.SetResolver(new DryIocDependencyResolver(container));

            return container;
        }

        /// <summary>Registers controllers types in container with <see cref="WebReuse.InRequest"/> reuse.</summary>
        /// <param name="container">Container to register controllers to.</param>
        /// <param name="controllerAssemblies">(optional) Uses <see cref="GetReferencedAssemblies"/> by default.</param>
        public static void RegisterMvcControllers(this IContainer container, IEnumerable<Assembly> controllerAssemblies = null)
        {
            controllerAssemblies = controllerAssemblies ?? GetReferencedAssemblies();
            container.RegisterMany(controllerAssemblies, typeof(IController), WebReuse.InRequest);
        }

        /// <summary>Replaces default Filter Providers with instance of <see cref="DryIocAggregatedFilterAttributeFilterProvider"/>,
        /// add in addition registers aggregated filter to container..</summary>
        /// <param name="container">Container to register to.</param>
        /// <param name="filterProviders">Original filter providers.</param>
        public static void SetFilterAttributeFilterProvider(this IContainer container, Collection<IFilterProvider> filterProviders = null)
        {
            filterProviders = filterProviders ?? FilterProviders.Providers;

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

    [ComVisible(false)]
    public class DryIocAggregatedFilterAttributeFilterProvider : FilterAttributeFilterProvider
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
}
