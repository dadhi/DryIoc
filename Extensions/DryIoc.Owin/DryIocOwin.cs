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
    using System.Threading.Tasks;
    using global::Owin;
    using Microsoft.Owin;

    /// <summary>Inserts DryIoc container into OWIN pipeline. Enables to Use middleware registered in DryIoc container.</summary>
    public static class DryIocOwin
    {
        /// <summary>Key of scoped container stored in <see cref="IOwinContext"/>.</summary>
        public static readonly string ScopedContainerKeyInContext = typeof(DryIocOwin).FullName;

        /// <summary>Inserts scoped container into pipeline and stores scoped container in context.
        /// 
        /// Optionally registers instances in scope with provided action.</summary>
        /// <param name="app">App builder</param> <param name="container">Container</param>
        /// <param name="registerInScope">(optional) Action for registering something in scope before setting scope into context.</param>
        /// <param name="scopeContext">(optional) Specific scope context to use. 
        /// If not specified using current container context. <see cref="AsyncExecutionFlowScopeContext"/> is default in .NET 4.5.</param>
        public static void UseDryIocOwinMiddleware(
            this IAppBuilder app, IContainer container,
            Action<IContainer> registerInScope = null,
            IScopeContext scopeContext = null)
        {
            if (container.ScopeContext == null)
                container = container.With(scopeContext: scopeContext ?? new AsyncExecutionFlowScopeContext());
            
            app.Use(async (context, next) =>
            {
                using (var scope = container.OpenScope(Reuse.WebRequestScopeName))
                {
                    scope.RegisterInstance(context, Reuse.InWebRequest, IfAlreadyRegistered.Replace);
                    if (registerInScope != null)
                        registerInScope(scope);
                    context.Set(ScopedContainerKeyInContext, scope);
                    await next();
                }
            });

            app.UseRegisteredMiddlewares(container);
        }

        /// <summary>Retrieves scope container stored in OWIN context.</summary>
        /// <param name="context"></param> <returns>Scoped container.</returns>
        public static IContainer GetDryIocScopedContainer(this IOwinContext context)
        {
            return context.Get<IContainer>(ScopedContainerKeyInContext);
        }

        private static void UseRegisteredMiddlewares(this IAppBuilder app, IRegistrator registry)
        {
            var services = registry.GetServiceRegistrations()
                .Where(r => r.ServiceType.IsAssignableTo(typeof(OwinMiddleware)))
                // note: ordering is important and set to registration order by default
                .OrderBy(r => r.FactoryRegistrationOrder) 
                .Select(r => typeof(DryIocWrapperMiddleware<>)
                    .MakeGenericType(r.Factory.ImplementationType ?? r.ServiceType))
                .ToArray();

            if (!services.IsNullOrEmpty())
                foreach (var service in services)
                    app.Use(service);
        }
    }

    internal sealed class DryIocWrapperMiddleware<TServiceMiddleware> : OwinMiddleware 
        where TServiceMiddleware : OwinMiddleware
    {
        public DryIocWrapperMiddleware(OwinMiddleware next) : base(next) {}

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
