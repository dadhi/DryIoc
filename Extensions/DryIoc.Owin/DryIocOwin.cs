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

    public static class DryIocOwin
    {
        public static readonly string ScopedContainerKey = typeof(DryIocOwin).FullName;

        public static void UseDryIocOwinMiddleware(
            this IAppBuilder app, IContainer container,
            Action<IContainer> registerInScope = null)
        {
            if (!(container.ScopeContext is AsyncExecutionFlowScopeContext))
                container = container.With(scopeContext: new AsyncExecutionFlowScopeContext());
            
            app.Use(async (context, next) =>
            {
                using (var scopedContainer = container.OpenScope())
                {
                    scopedContainer.RegisterInstance(context);
                    if (registerInScope != null)
                        registerInScope(scopedContainer);
                    context.Set(ScopedContainerKey, scopedContainer);
                    await next();
                }
            });

            app.UseRegisteredMiddleware(container);
        }

        public static IContainer GetDryIocScopedContainer(this IOwinContext context)
        {
            return context.Get<IContainer>(ScopedContainerKey);
        }

        static void UseRegisteredMiddleware(this IAppBuilder app, IRegistrator registry)
        {
            var services = registry.GetServiceRegistrations()
                .Where(r => r.ServiceType.IsAssignableTo(typeof(OwinMiddleware)))
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
            var container = context.GetDryIocScopedContainer().ThrowIfNull();

            var middleware = container.Resolve<Func<OwinMiddleware, TServiceMiddleware>>(IfUnresolved.ReturnDefault);
            if (middleware == null)
                return Next.Invoke(context);
            
            return middleware(Next).Invoke(context);
        }
    }

    public static class ReuseInWeb
    {
        public static readonly IReuse Request = Reuse.InCurrentNamedScope(AsyncExecutionFlowScopeContext.ROOT_SCOPE_NAME);
    }
}
