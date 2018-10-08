/*
The MIT License (MIT)

Copyright (c) 2014 Maksim Volkau

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

namespace DryIoc.Owin
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::Owin;
    using Microsoft.Owin;

    /// <summary>Inserts DryIoc container into OWIN pipeline. Enables to Use middleware registered in DryIoc container.</summary>
    public static class DryIocOwin
    {
        /// <summary>Key of scoped container stored in <see cref="IOwinContext"/>.</summary>
        public static readonly string ScopedContainerKeyInContext = typeof(DryIocOwin).FullName;

        /// <summary>First, inserts request scope into pipeline via <see cref="InsertOpenScope"/>.
        /// Then adds to application builder the registered OWIN middlewares
        /// wrapped in <see cref="DryIocWrapperMiddleware{TServiceMiddleware}"/> via <see cref="UseRegisteredMiddlewares"/>.</summary>
        /// <param name="app">App builder</param> 
        /// <param name="container">Container</param>
        /// <param name="registerInScope">(optional) Action for using/registering instances in scope before setting scope into context.</param>
        /// <param name="scopeContext">(optional) Scope context to use. By default sets the <see cref="AsyncExecutionFlowScopeContext"/>.</param>
        /// <returns>App builder to enable method chaining.</returns>
        /// <remarks>IMPORTANT: if passed <paramref name="container"/> did not have a scope context set,
        /// then the new container with context will be created and used. If you want to hold on this new container,
        /// you may first call <see cref="InsertOpenScope"/>, store the result container, 
        /// then call <see cref="UseRegisteredMiddlewares"/>.</remarks>
        public static IAppBuilder UseDryIocOwinMiddleware(
            this IAppBuilder app, IContainer container,
            Action<IResolverContext> registerInScope = null,
            IScopeContext scopeContext = null)
        {
            var containerWithAmbientContext = container.InsertOpenScope(app, registerInScope, scopeContext);
            return app.UseRegisteredMiddlewares(containerWithAmbientContext);
        }

        /// <summary>Inserts `container.OpenScope()` into pipeline 
        /// and stores the scope in context to be used by the rest of pipeline.
        /// Additionally may register external instance into open scope.</summary>
        /// <param name="app">App builder to use.</param>
        /// <param name="container">DryIoc container for opening scope.</param>
        /// <param name="registerInScope">(optional) e.g. `r => r.UseInstance(someSettings)`</param>
        /// <param name="scopeContext">(optional) Scope context to use. By default sets the <see cref="AsyncExecutionFlowScopeContext"/>.</param>
        /// <returns>IMPORTANT: if passed <paramref name="container"/> did not have a scope context,
        /// then the method will return NEW container with scope context set. 
        /// Use the returned container to pass to <see cref="UseRegisteredMiddlewares"/>.</returns>
        public static IContainer InsertOpenScope(this IContainer container, IAppBuilder app, 
            Action<IResolverContext> registerInScope = null, 
            IScopeContext scopeContext = null)
        {
            if (container.ScopeContext == null)
                container = container.With(scopeContext: scopeContext ?? new AsyncExecutionFlowScopeContext());

            app.Use(async (context, next) =>
            {
                using (var scope = container.OpenScope(Reuse.WebRequestScopeName))
                {
                    scope.UseInstance(context);
                    registerInScope?.Invoke(scope);
                    context.Set(ScopedContainerKeyInContext, scope);
                    await next();
                }
            });

            return container;
        }

        /// <summary>Adds to application builder the registered OWIN middlewares 
        /// wrapped in <see cref="DryIocWrapperMiddleware{TServiceMiddleware}"/>.</summary>
        /// <param name="app">App builder to use.</param>
        /// <param name="registry">Container registry to find registered <see cref="OwinMiddleware"/>.</param>
        /// <returns>App builder to enable method chaining.</returns>
        public static IAppBuilder UseRegisteredMiddlewares(this IAppBuilder app, IRegistrator registry)
        {
            foreach (var middlewareType in registry.DiscoverRegisteredMiddlewares())
                app = app.Use(middlewareType);
            return app;
        }

        /// <summary>Retrieves scope container stored in OWIN context.</summary>
        /// <param name="context"></param> <returns>Scoped container.</returns>
        public static IContainer GetDryIocScopedContainer(this IOwinContext context)
        {
            return context.Get<IContainer>(ScopedContainerKeyInContext);
        }

        private static IEnumerable<Type> DiscoverRegisteredMiddlewares(this IRegistrator registry)
        {
            return registry.GetServiceRegistrations()
                .Where(r => r.ServiceType.IsAssignableTo(typeof(OwinMiddleware)))
                // note: ordering is important and set to registration order by default
                .OrderBy(r => r.FactoryRegistrationOrder)
                .Select(r => typeof(DryIocWrapperMiddleware<>)
                    .MakeGenericType(r.Factory.ImplementationType ?? r.ServiceType));
        }
    }

    internal sealed class DryIocWrapperMiddleware<TServiceMiddleware> : OwinMiddleware
        where TServiceMiddleware : OwinMiddleware
    {
        public DryIocWrapperMiddleware(OwinMiddleware next) : base(next) { }

        public override Task Invoke(IOwinContext context)
        {
            var scopedContainer = context.GetDryIocScopedContainer().ThrowIfNull();

            var middleware = scopedContainer.Resolve<Func<OwinMiddleware, TServiceMiddleware>>(IfUnresolved.ReturnDefault);
            if (middleware == null)
                return Next.Invoke(context);

            return middleware(Next).Invoke(context);
        }
    }
}
