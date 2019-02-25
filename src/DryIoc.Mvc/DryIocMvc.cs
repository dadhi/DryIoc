/*
The MIT License (MIT)

Copyright © 2013-2018 Maksim Volkau and Contributors

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
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Web.Mvc;
    using System.Web.Compilation;
    using Web;

    /// <summary>Set of container extension methods to set HttpContext scope, register Controllers, 
    /// set DryIoc FilterProvider and set DryIoc container as dependency resolver.</summary>
    /// <example> <code lang="cs"><![CDATA[
    /// protected void Application_Start()
    /// {
    ///     var container = new Container();
    /// 
    ///     // Enable basic MVC support. 
    ///     container = container.WithMvc(
    /// 
    ///         // optional: enable original DryIoc exceptions when resolving controllers - provides more info if resolve is failed
    ///         throwIfUnresolved: type => type.IsController(),
    /// 
    ///         // optional: provide the scope context with User error handler to find why request scope is not created / disposed
    ///         scopeContext: new HttpContextScopeContext(ex => MyLogger.LogError(ex))
    ///     );
    ///     
    ///     // Optionally enable support for MEF Export/ImportAttribute with DryIoc.MefAttributedModel package. 
    ///     // container = container.WithMefAttributedModel();
    ///     // container.RegisterExports(new[] { typeof(MyMvcApp).Assembly });
    /// 
    ///     // Additional registrations go here ...
    /// }
    /// ]]></code></example>
    public static class DryIocMvc
    {
        /// <summary>Creates new container from original one with <see cref="HttpContextScopeContext"/>.
        /// Then registers MVC controllers in container, 
        /// sets <see cref="DryIocFilterAttributeFilterProvider"/> as filter provider,
        /// and at last sets container as <see cref="DependencyResolver"/>.</summary>
        /// <param name="container">Original container.</param>
        /// <param name="controllerAssemblies">(optional) By default uses <see cref="BuildManager.GetReferencedAssemblies"/>.</param>
        /// <param name="scopeContext">(optional) Specific scope context to use, by default MVC uses <see cref="HttpContextScopeContext"/> 
        /// (if container does not have its own context specified).</param>
        /// <param name="throwIfUnresolved">(optional) Instructs DryIoc to throw exception
        /// for unresolved type instead of fallback to default Resolver.</param>
        /// <param name="controllerReuse">(optional) Defaults to <see cref="Reuse.InWebRequest"/></param>
        /// <returns>New container with applied Web context.</returns>
        public static IContainer WithMvc(this IContainer container,
            IEnumerable<Assembly> controllerAssemblies = null,
            IScopeContext scopeContext = null,
            Func<Type, bool> throwIfUnresolved = null,
            IReuse controllerReuse = null)
        {
            container.ThrowIfNull();

            if (container.ScopeContext == null)
                container = container.With(scopeContext: scopeContext ?? new HttpContextScopeContext());

            container.RegisterMvcControllers(controllerAssemblies, controllerReuse);

            container.SetFilterAttributeFilterProvider(FilterProviders.Providers);

            DependencyResolver.SetResolver(new DryIocDependencyResolver(container, throwIfUnresolved));

            return container;
        }

        /// <summary>Helps to find if type is controller type.</summary>
        public static bool IsController(this Type type) => type.IsAssignableTo(typeof(IController));

        /// <summary>Returns all application specific referenced assemblies (except from GAC and Dynamic).</summary>
        /// <returns>The assemblies.</returns>
        public static IEnumerable<Assembly> GetReferencedAssemblies() =>
            BuildManager.GetReferencedAssemblies().OfType<Assembly>()
                .Where(a => !a.IsDynamic && !a.GlobalAssemblyCache);

        /// <summary>Registers controllers types in container with InWebRequest reuse.</summary>
        /// <param name="container">Container to register controllers to.</param>
        /// <param name="controllerAssemblies">(optional) Uses <see cref="BuildManager.GetReferencedAssemblies"/> by default.</param>
        /// <param name="controllerReuse">(optional) Defaults to <see cref="Reuse.InWebRequest"/></param>
        public static void RegisterMvcControllers(this IContainer container, 
            IEnumerable<Assembly> controllerAssemblies = null, IReuse controllerReuse = null)
        {
            controllerAssemblies = controllerAssemblies ?? GetReferencedAssemblies();
            controllerReuse = controllerReuse ?? Reuse.InWebRequest;

            container.RegisterMany(controllerAssemblies, IsController, controllerReuse, 
                FactoryMethod.ConstructorWithResolvableArguments);
        }

        /// <summary>Replaces default Filter Providers with instance of <see cref="DryIocFilterAttributeFilterProvider"/>,
        /// add in addition registers aggregated filter to container..</summary>
        /// <param name="container">Container to register to.</param>
        /// <param name="filterProviders">Original filter providers.</param>
        public static void SetFilterAttributeFilterProvider(this IContainer container, Collection<IFilterProvider> filterProviders = null)
        {
            filterProviders = filterProviders ?? FilterProviders.Providers;
            var filterProvidersSnapshot = filterProviders.OfType<FilterAttributeFilterProvider>().ToArray();
            foreach (var provider in filterProvidersSnapshot)
                filterProviders.Remove(provider);

            var filterProvider = new DryIocFilterAttributeFilterProvider(container);
            filterProviders.Add(filterProvider);

            container.RegisterInstance<IFilterProvider>(filterProvider, IfAlreadyRegistered.Replace);
        }

        /// <summary>Registers both <see cref="DryIocDataAnnotationsModelValidator"/> and
        /// <see cref="DryIocValidatableObjectAdapter"/> and provides the <paramref name="container"/> to use
        /// as <see cref="IServiceProvider"/> for resolving dependencies.</summary>
        /// <param name="container"><see cref="IServiceProvider"/> implementation.</param>
        /// <returns>Returns source container for fluent access.</returns>
        public static IContainer WithDataAnnotationsValidator(this IContainer container)
        {
            var serviceProvider = new DryIocServiceProvider(container.ThrowIfNull());

            DataAnnotationsModelValidatorProvider.RegisterDefaultAdapterFactory((metadata, context, attribute) =>
                new DryIocDataAnnotationsModelValidator(serviceProvider, metadata, context, attribute));

            DataAnnotationsModelValidatorProvider.RegisterDefaultValidatableObjectAdapterFactory((metadata, context) =>
                new DryIocValidatableObjectAdapter(serviceProvider, metadata, context));

            return container;
        }
    }

    /// <summary>Resolver delegating to DryIoc container.</summary>
    public class DryIocDependencyResolver : IDependencyResolver
    {
        /// <summary>Creates resolver from DryIoc resolver.</summary>
        /// <param name="resolver">DryIoc resolver (container interface).</param>
        /// <param name="throwIfUnresolved">(optional) Instructs DryIoc to throw exception
        /// for unresolved type instead of fallback to default Resolver.</param>
        public DryIocDependencyResolver(IResolver resolver, Func<Type, bool> throwIfUnresolved = null)
        {
            _resolver = resolver;
            _throwIfUnresolved = throwIfUnresolved;
        }

        /// <summary> Resolves single registered services that support arbitrary object creation. </summary>
        /// <returns> The requested service or object. </returns>
        /// <param name="serviceType">The type of the requested service or object.</param>
        public object GetService(Type serviceType) => 
            _resolver.Resolve(serviceType,
                _throwIfUnresolved != null && _throwIfUnresolved(serviceType)
                    ? IfUnresolved.Throw : IfUnresolved.ReturnDefault);

        /// <summary> Resolves multiply registered services. </summary>
        /// <returns> The requested services. </returns>
        /// <param name="serviceType">The type of the requested services.</param>
        public IEnumerable<object> GetServices(Type serviceType) => 
            _resolver.ResolveMany<object>(serviceType);

        private readonly IResolver _resolver;
        private readonly Func<Type, bool> _throwIfUnresolved;
    }

    /// <summary>Defines an filter provider for filter attributes. Uses DryIoc container to inject filter properties.</summary>
    [ComVisible(false)]
    public class DryIocFilterAttributeFilterProvider : FilterAttributeFilterProvider
    {
        /// <summary>Creates filter provider.</summary> <param name="container"></param>
        public DryIocFilterAttributeFilterProvider(IContainer container)
        {
            _container = container;
        }

        /// <summary> Aggregates the filters from all of the filter providers into one collection. </summary>
        /// <returns> The collection filters from all of the filter providers. </returns>
        /// <param name="controllerContext">The controller context.</param><param name="actionDescriptor">The action descriptor.</param>
        public override IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            var filters = base.GetFilters(controllerContext, actionDescriptor).ToArray();
            for (var i = 0; i < filters.Length; i++)
                _container.InjectPropertiesAndFields(filters[i].Instance);
            return filters;
        }

        private readonly IContainer _container;
    }

    /// <summary>Service provider wrapping DryIoc <see cref="IResolver"/>.</summary>
    public sealed class DryIocServiceProvider : IServiceProvider
    {
        /// <summary>Constructs the wrapper over resolver</summary> <param name="resolver"></param>
        public DryIocServiceProvider(IResolver resolver)
        {
            _resolver = resolver;
        }

        /// <summary>Resolves the service for requested <paramref name="serviceType"/>.</summary>
        /// <param name="serviceType">Requested service type.</param> <returns>Resolved service object</returns>
        public object GetService(Type serviceType) => 
            _resolver.Resolve(serviceType);

        private readonly IResolver _resolver;
    }

    /// <summary>Provides a model validator and injects <see cref="IServiceProvider"/> 
    /// implemented by <see cref="Container"/>.</summary>
    [ComVisible(false)]
    public class DryIocDataAnnotationsModelValidator : DataAnnotationsModelValidator
    {
        /// <summary>Initializes a new instance of the  class.</summary>
        /// <param name="serviceProvider"><see cref="Container"/> to use for resolving dependencies.</param>
        /// <param name="metadata">The metadata for the model.</param>
        /// <param name="context">The controller context for the model.</param>
        /// <param name="attribute">The validation attribute for the model.</param>
        public DryIocDataAnnotationsModelValidator(IServiceProvider serviceProvider, ModelMetadata metadata, ControllerContext context, ValidationAttribute attribute) :
            base(metadata, context, attribute)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public override IEnumerable<ModelValidationResult> Validate(object container)
        {
            var context = new ValidationContext(container ?? Metadata.Model, _serviceProvider, items: null)
            {
                DisplayName = Metadata.GetDisplayName()
            };

            var result = Attribute.GetValidationResult(Metadata.Model, context);
            if (result != ValidationResult.Success)
                yield return new ModelValidationResult { Message = result?.ErrorMessage };
        }

        private readonly IServiceProvider _serviceProvider;
    }

    /// <summary>Provides an object adapter that can be validated,
    /// and injects <see cref="IServiceProvider"/> implementation as <see cref="Container"/>.</summary>
    [ComVisible(false)]
    public class DryIocValidatableObjectAdapter : ValidatableObjectAdapter
    {
        /// <summary>Initializes a new instance of the class.</summary>
        /// <param name="serviceProvider"><see cref="Container"/> to use for resolving dependencies.</param>
        /// <param name="metadata">The model metadata.</param>
        /// <param name="context">The controller context.</param>
        public DryIocValidatableObjectAdapter(IServiceProvider serviceProvider, ModelMetadata metadata, ControllerContext context) :
            base(metadata, context)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public override IEnumerable<ModelValidationResult> Validate(object container)
        {
            var model = Metadata.Model;
            if (model == null)
                return Enumerable.Empty<ModelValidationResult>();

            var validatable = model as IValidatableObject;
            if (validatable == null)
                return base.Validate(container);

            var validationContext = new ValidationContext(validatable, _serviceProvider, null);
            return ConvertResults(validatable.Validate(validationContext));
        }

        private static IEnumerable<ModelValidationResult> ConvertResults(IEnumerable<ValidationResult> results)
        {
            foreach (var result in results)
            {
                if (result != ValidationResult.Success)
                {
                    if (result.MemberNames == null || !result.MemberNames.Any())
                        yield return new ModelValidationResult { Message = result.ErrorMessage };
                    else
                        foreach (var memberName in result.MemberNames)
                            yield return new ModelValidationResult { Message = result.ErrorMessage, MemberName = memberName };
                }
            }
        }

        private readonly IServiceProvider _serviceProvider;
    }

    /// <summary>Implements request begin / end handlers based on <see cref="HttpContextScopeContext"/>.</summary>
    public class AsyncExecutionFlowScopeContextRequestHandler : IDryIocHttpModuleRequestHandler
    {
        /// Uses a default instance of shared scope context that can be propagated throw async / await boundaries
        internal static IScopeContext DefaultScopeContext = AsyncExecutionFlowScopeContext.Default;

        /// <inheritdoc />
        public void OnBeginRequest(object sender, EventArgs _)
        {
            DefaultScopeContext.SetCurrent(scope =>
                scope ?? new Scope(parent: null, name: Reuse.WebRequestScopeName));
        }

        /// <inheritdoc />
        public void OnEndRequest(object sender, EventArgs _)
        {
            var scope = DefaultScopeContext.GetCurrentOrDefault();
            if (scope?.Name.Equals(Reuse.WebRequestScopeName) == true)
                scope.Dispose();
        }
    }
}
