[assembly: System.Web.PreApplicationStartMethod(typeof(DryIoc.Web.DryIocHttpModuleInitializer), "Initialize")]

namespace DryIoc.Web
{
    using System;
    using System.Threading;
    using System.Collections;
    using System.Web;
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    internal static class DryIocWeb
    {
        public static IContainer WithWebReuseInRequest(this IContainer container)
        {
            return container.ThrowIfNull().With(scopeContext: new HttpContextScopeContext());
        }
    }

    /// <summary>Stores current scope in <see cref="HttpContext.Items"/>.</summary>
    /// <remarks>Stateless context, so could be created multiple times and used from different places without side-effects.</remarks>
    public sealed class HttpContextScopeContext : IScopeContext
    {
        public static Func<IDictionary> GetContextItemsDefault = () => HttpContext.Current.Items;

        public static readonly object ROOT_SCOPE_NAME = typeof(HttpContextScopeContext);

        public object RootScopeName { get { return ROOT_SCOPE_NAME; } }

        public HttpContextScopeContext(Func<IDictionary> getContextItems = null)
        {
            _getContextItems = getContextItems ?? GetContextItemsDefault;
        }

        public IScope GetCurrentOrDefault()
        {
            return _getContextItems()[RootScopeName] as IScope;
        }

        public IScope SetCurrent(Func<IScope, IScope> getNewCurrentScope)
        {
            var newCurrentScope = getNewCurrentScope.ThrowIfNull()(GetCurrentOrDefault());
            _getContextItems()[ROOT_SCOPE_NAME] = newCurrentScope;
            return newCurrentScope;
        }

        private readonly Func<IDictionary> _getContextItems;
    }

    public static class WebReuse
    {
        public static readonly IReuse InRequest = Reuse.InCurrentNamedScope(HttpContextScopeContext.ROOT_SCOPE_NAME);
    }

    public static class DryIocHttpModuleInitializer
    {
        public static void Initialize()
        {
            if (Interlocked.CompareExchange(ref initialized, 1, 0) == 0)
                DynamicModuleUtility.RegisterModule(typeof(DryIocHttpModule));
        }

        private static int initialized;
    }

    public class DryIocHttpModule : IHttpModule
    {
        /// <summary>Initializes a module and prepares it to handle requests. </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication"/> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application </param>
        void IHttpModule.Init(HttpApplication context)
        {
            context.BeginRequest += (sender, _) =>
            {
                var app = sender as HttpApplication;
                var scopeContext = new HttpContextScopeContext();
                scopeContext.SetCurrent(current => 
                    new Scope(current.ThrowIf(current != null, Error.Of("Someone set root context scope before you.")), 
                        HttpContextScopeContext.ROOT_SCOPE_NAME));
            };
            context.EndRequest += (sender, _) =>
            {
                var app = sender as HttpApplication;
                var scopeContext = new HttpContextScopeContext();
                var scope = scopeContext.GetCurrentOrDefault().ThrowIfNull(Error.Of("No root opened scope found."));
                scope.ThrowIf(scope.Parent != null).Dispose();
            };
        }

        /// <summary>Disposes of the resources (other than memory) used by the module  that implements <see cref="IHttpModule"/>.</summary>
        void IHttpModule.Dispose() { }
    }
}
