[assembly: System.Web.PreApplicationStartMethod(typeof(DryIoc.Web.HttpModuleInitializer), "Initialize")]

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

    public static class WebReuse
    {
        public static readonly IReuse InRequest = Reuse.InCurrentNamedScope(HttpContextScopeContext.ROOT_SCOPE_NAME);
    }

    /// <summary>Stores current scope in <see cref="HttpContext.Items"/>.</summary>
    /// <remarks>Stateless context, so could be created multiple times and used from different places without side-effects.</remarks>
    public sealed class HttpContextScopeContext : IScopeContext
    {
        public static readonly object ROOT_SCOPE_NAME = typeof(HttpContextScopeContext);

        public object RootScopeName { get { return ROOT_SCOPE_NAME; } }

        public HttpContextScopeContext(Func<IDictionary> getContextItems = null)
        {
            _getContextItems = getContextItems ?? (() => HttpContext.Current.Items);
        }

        public IScope GetCurrentOrDefault()
        {
            return _getContextItems()[RootScopeName] as IScope;
        }

        public IScope SetCurrent(Func<IScope, IScope> getNewCurrent)
        {
            var currentScope = GetCurrentOrDefault();
            var newScope = getNewCurrent.ThrowIfNull()(currentScope);
            _getContextItems()[ROOT_SCOPE_NAME] = newScope;
            return newScope;
        }

        private readonly Func<IDictionary> _getContextItems;
    }

    public static class HttpModuleInitializer
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
        void IHttpModule.Init(HttpApplication context)
        {
            context.BeginRequest += (sender, eventArgs) =>
            {
                var application = (sender as HttpApplication).ThrowIfNull();
                // TODO: Put new OpenedScope into application.Context.
            };
            
            context.EndRequest += (sender, eventArgs) =>
            {
                var application = (sender as HttpApplication).ThrowIfNull();
                // TODO: Get OpenedScope from application.Context and Dispose it.
            };
        }

        /// <summary>Disposes of the resources (other than memory) used by the module  that implements <see cref="IHttpModule"/>.</summary>
        void IHttpModule.Dispose() { }
    }
}
