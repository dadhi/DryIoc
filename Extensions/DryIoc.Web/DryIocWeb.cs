[assembly: System.Web.PreApplicationStartMethod(typeof(DryIoc.Web.DryIocHttpModuleInitializer), "Initialize")]

namespace DryIoc.Web
{
    using System;
    using System.Threading;
    using System.Collections;
    using System.Web;
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    /// <summary>Extension to get container with ambient <see cref="HttpContext.Current"/> scope context.</summary>
    public static class DryIocWeb
    {
        /// <summary>Creates new container from original with HttpContext or arbitrary/test context <paramref name="getContextItems"/>.</summary>
        /// <param name="container">Original container with some rules and registrations.</param>
        /// <param name="getContextItems">(optional) Arbitrary or test context to use instead of <see cref="HttpContext.Current"/>.</param>
        /// <returns>New container with the same rules and registrations/cache but with new ambient context.</returns>
        public static IContainer WithWebReuseInRequest(this IContainer container, Func<IDictionary> getContextItems = null)
        {
            return container.ThrowIfNull().With(scopeContext: new HttpContextScopeContext(getContextItems));
        }
    }

    /// <summary>Defines web reuse as reuse items in <see cref="HttpContextScopeContext"/>.</summary>
    public static class Reuse
    {
        /// <summary>Request reuse corresponds to reusing items in root open scope (physically stored in current <see cref="HttpContext"/>).</summary>
        public static readonly IReuse InRequest = 
            DryIoc.Reuse.InCurrentNamedScope(HttpContextScopeContext.ROOT_SCOPE_NAME);
    }

    /// <summary>Registers <see cref="DryIocHttpModule"/>.</summary>
    public static class DryIocHttpModuleInitializer
    {
        /// <summary>Registers once the type of <see cref="DryIocHttpModule"/>.</summary>
        public static void Initialize()
        {
            if (Interlocked.CompareExchange(ref _initialized, 1, 0) == 0)
                DynamicModuleUtility.RegisterModule(typeof(DryIocHttpModule));
        }

        private static int _initialized;
    }

    /// <summary>Hooks up <see cref="Container.OpenScope"/> on request beginning and scope dispose on request end.</summary>
    public sealed class DryIocHttpModule : IHttpModule
    {
        /// <summary>Initializes a module and prepares it to handle requests. </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication"/> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application </param>
        void IHttpModule.Init(HttpApplication context)
        {
            context.BeginRequest += (sender, _) =>
            {
                var httpContext = (sender as HttpApplication).ThrowIfNull().Context;
                var scopeContext = new HttpContextScopeContext(() => httpContext.Items);

                scopeContext.SetCurrent(parent => parent != null
                    ? Throw.For<IScope>(Error.SCOPE_IS_ALREADY_OPENED, parent)
                    : new Scope(null, scopeContext.RootScopeName));
            };

            context.EndRequest += (sender, _) =>
            {
                var httpContext = (sender as HttpApplication).ThrowIfNull().Context;
                var scopeContext = new HttpContextScopeContext(() => httpContext.Items);
                
                var currentScope = scopeContext.GetCurrentOrDefault().ThrowIfNull(Error.NO_OPENED_SCOPE_TO_DISPOSE);
                Throw.If(currentScope.Parent != null, Error.NOT_THE_ROOT_OPENED_SCOPE, currentScope.Parent);
                
                currentScope.Dispose();
            };
        }

        /// <summary>Disposes of the resources (other than memory) used by the module  that implements <see cref="IHttpModule"/>.</summary>
        void IHttpModule.Dispose() { }
    }

    /// <summary>Web-related exceptions.</summary>
    public static class Error
    {
#pragma warning disable 1591 // "Missing XML-comment"
        public static readonly int
            SCOPE_IS_ALREADY_OPENED = DryIoc.Error.Of(
                "Probably problems with Web setup: Someone already opened scope {0} before HttpApplication.BeginRequest."),
            NO_OPENED_SCOPE_TO_DISPOSE = DryIoc.Error.Of(
                "Probably problems with Web setup: No opened scope to Dispose."),
            NOT_THE_ROOT_OPENED_SCOPE = DryIoc.Error.Of(
                "Probably problems with Web setup: Opened scope is not the root scope.");
#pragma warning restore 1591
    }

    /// <summary>Stores current scope in <see cref="HttpContext.Items"/>.</summary>
    /// <remarks>Stateless context, so could be created multiple times and used from different places without side-effects.</remarks>
    public sealed class HttpContextScopeContext : IScopeContext
    {
        /// <summary>Provides default context items dictionary using <see cref="HttpContext.Current"/>.
        /// Could be overridden with any key-value dictionary where <see cref="HttpContext"/> is not available, e.g. in tests.</summary>
        public static Func<IDictionary> GetContextItems = () => HttpContext.Current.ThrowIfNull().Items;

        /// <summary>Fixed root scope name for the context.</summary>
        public static readonly object ROOT_SCOPE_NAME = typeof(HttpContextScopeContext);

        /// <summary>Returns fixed <see cref="ROOT_SCOPE_NAME"/>.</summary>
        public object RootScopeName { get { return ROOT_SCOPE_NAME; } }

        /// <summary>Creates the context optionally with arbitrary/test items storage.</summary>
        /// <param name="getContextItems">(optional) Arbitrary/test items storage.</param>
        public HttpContextScopeContext(Func<IDictionary> getContextItems = null)
        {
            _getContextItems = getContextItems ?? GetContextItems;
        }

        /// <summary>Returns current ambient scope stored in item storage.</summary> <returns>Current scope or null if there is no.</returns>
        public IScope GetCurrentOrDefault()
        {
            return _getContextItems()[RootScopeName] as IScope;
        }

        /// <summary>Sets the new scope as current using existing current as input.</summary>
        /// <param name="getNewCurrentScope">Delegate to get new scope.</param>
        /// <returns>Return new current scope.</returns>
        public IScope SetCurrent(Func<IScope, IScope> getNewCurrentScope)
        {
            var newCurrentScope = getNewCurrentScope.ThrowIfNull()(GetCurrentOrDefault());
            _getContextItems()[ROOT_SCOPE_NAME] = newCurrentScope;
            return newCurrentScope;
        }

        private readonly Func<IDictionary> _getContextItems;
    }
}
