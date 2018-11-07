/*
The MIT License (MIT)

Copyright (c) 2013-2018 Maksim Volkau

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
        public static IContainer WithHttpContextScopeContext(this IContainer container, Func<IDictionary> getContextItems = null) => 
            container.ThrowIfNull().With(scopeContext: new HttpContextScopeContext(getContextItems));
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

    /// <summary>Defines API for request begin / end handlers</summary>
    public interface IDryIocHttpModuleRequestHandler
    {
        /// On request begin handler
        void OnBeginRequest(object sender, EventArgs eventArgs);

        /// On request end handler
        void OnEndRequest(object sender, EventArgs eventArgs);
    }

    /// <summary>Hooks up <see cref="ResolverContext.OpenScope"/> on request beginning and scope dispose on request end.</summary>
    public class DryIocHttpModule : IHttpModule
    {
        /// Defaults to HttpContextScopeContext
        public static IDryIocHttpModuleRequestHandler RequestHandler = new HttpContextScopeContextRequestHandler(); 

        /// <summary>Initializes a module and prepares it to handle requests. </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication"/> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application </param>
        void IHttpModule.Init(HttpApplication context)
        {
            context.BeginRequest += RequestHandler.OnBeginRequest;
            context.EndRequest += RequestHandler.OnEndRequest;
        }

        /// <summary>Disposes of the resources (other than memory) used by the module  that implements <see cref="IHttpModule"/>.</summary>
        void IHttpModule.Dispose() { }
    }

    /// <summary>Implements request begin / end handlers based on <see cref="HttpContextScopeContext"/>.</summary>
    public class HttpContextScopeContextRequestHandler : IDryIocHttpModuleRequestHandler
    {
        /// <inheritdoc />
        public void OnBeginRequest(object sender, EventArgs _)
        {
            var httpContext = ((HttpApplication)sender).Context;
            var scopeContext = new HttpContextScopeContext(() => httpContext.Items);
            scopeContext.SetCurrent(SetOrKeepCurrentRequestScope);
        }

        /// <inheritdoc />
        public void OnEndRequest(object sender, EventArgs _)
        {
            var httpContext = ((HttpApplication)sender).Context;
            var scopeContext = new HttpContextScopeContext(() => httpContext.Items);

            var currentScope = scopeContext.GetCurrentOrDefault();
            if (currentScope != null && Reuse.WebRequestScopeName.Equals(currentScope.Name)) currentScope.Dispose();
        }

        // If current scope does not have WebRequestScopeName 
        // then create new scope with this name, 
        // otherwise - use current.
        private static IScope SetOrKeepCurrentRequestScope(IScope current) => 
            current != null && Reuse.WebRequestScopeName.Equals(current.Name)
                ? current
                : new Scope(current, Reuse.WebRequestScopeName);
    }

    /// <summary>Stores current scope in <see cref="HttpContext.Items"/>.</summary>
    /// <remarks>Stateless context, so could be created multiple times and used from different places without side-effects.</remarks>
    public sealed class HttpContextScopeContext : IScopeContext
    {
        /// <summary>Provides default context items dictionary using <see cref="HttpContext.Current"/>.
        /// Could be overridden with any key-value dictionary where <see cref="HttpContext"/> is not available, e.g. in tests.</summary>
        public static Func<IDictionary> GetContextItems = () => HttpContext.Current?.Items;

        /// <summary>Creates the context optionally with arbitrary/test items storage.</summary>
        /// <param name="getContextItems">(optional) Context items to use.</param>
        public HttpContextScopeContext(Func<IDictionary> getContextItems = null)
        {
            _getContextItems = getContextItems ?? GetContextItems;
        }

        /// <summary>Creates the context optionally with arbitrary/test items storage.</summary>
        /// <param name="catchScopeContextErrors">Enable User handling of scope get/set. 
        /// When specified the result Get scope will be null, the result Set scope will remain the old scope (null if was not set before).</param>
        /// <param name="getContextItems">(optional) Context items to use.</param>
        public HttpContextScopeContext(Action<Exception> catchScopeContextErrors, Func<IDictionary> getContextItems = null)
        {
            _getContextItems = getContextItems ?? GetContextItems;
            _catchScopeContextErrors = catchScopeContextErrors;
        }

        /// <summary>Fixed root scope name for the context.</summary>
        public static readonly string ScopeContextName = typeof(HttpContextScopeContext).FullName;

        /// <summary>Returns fixed name.</summary>
        public string RootScopeName => ScopeContextName;

        /// <summary>Returns current ambient scope stored in item storage.</summary> <returns>Current scope or null if there is no.</returns>
        public IScope GetCurrentOrDefault()
        {
            try
            {
                var contextItems = _getContextItems();
                return contextItems == null || !contextItems.Contains(RootScopeName)
                    ? null
                    : contextItems[RootScopeName] as IScope;
            }
            catch (Exception ex)
            {
                if (_catchScopeContextErrors == null)
                    throw;
                _catchScopeContextErrors(ex);
                return null;
            }
        }

        /// <summary>Sets the new scope as current using existing current as input.</summary>
        /// <param name="setCurrentScope">Delegate to get new scope.</param>
        /// <returns>New current scope.</returns>
        public IScope SetCurrent(SetCurrentScopeHandler setCurrentScope)
        {
            var oldScope = GetCurrentOrDefault();
            var newScope = setCurrentScope(oldScope);
            try
            {
                _getContextItems()[RootScopeName] = newScope;
                return newScope;
            }
            catch (Exception ex)
            {
                if (_catchScopeContextErrors == null)
                    throw;
                _catchScopeContextErrors(ex);
                return oldScope;
            }
        }

        /// <summary>Nothing to dispose.</summary>
        public void Dispose() { }

        private readonly Func<IDictionary> _getContextItems;
        private readonly Action<Exception> _catchScopeContextErrors;
    }
}
