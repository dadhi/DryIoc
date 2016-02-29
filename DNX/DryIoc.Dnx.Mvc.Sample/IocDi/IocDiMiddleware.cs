using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DryIoc;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Web.IocDi
{
    public sealed class IocDiMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly GetScopeInstancesDelegate _getScopeInstancesDelegate;

        public IocDiMiddleware(RequestDelegate next, GetScopeInstancesDelegate getScopeInstancesDelegate)
        {
            if (next == null) throw new ArgumentNullException(nameof(next));
            if (getScopeInstancesDelegate == null) throw new ArgumentNullException(nameof(getScopeInstancesDelegate));

            _next = next;
            _getScopeInstancesDelegate = getScopeInstancesDelegate;
        }

        public async Task Invoke(HttpContext context)
        {
            using (IocDiContainer.EnsureIocDi(context, _getScopeInstancesDelegate(context)))
            {
                await _next(context);
            }
        }

        private sealed class IocDiContainer : IServiceProvider, IDisposable
        {
            private readonly HttpContext _context;
            private readonly IServiceProvider _priorRequestServices;
            private IServiceScope _scope;

            private IocDiContainer(HttpContext context, IServiceScope scope)
            {
                _context = context;
                _priorRequestServices = context.RequestServices;
                _scope = scope;
                _context.RequestServices = scope.ServiceProvider;
            }

            public static IDisposable EnsureIocDi(HttpContext context, IEnumerable<Tuple<Type, object>> scopeInstances)
            {
                var scopeFactory = (IServiceScopeFactory)context.RequestServices.GetService(typeof(IServiceScopeFactory));
                IServiceScope scope = scopeFactory.CreateScope();

                var scopedContainer = scope.ServiceProvider.GetService<IContainer>();
                foreach (var scopeInstance in scopeInstances)
                {
                    scopedContainer.RegisterInstance(scopeInstance.Item1, scopeInstance.Item2, Reuse.InCurrentScope, IfAlreadyRegistered.Replace);
                }

                return new IocDiContainer(context, scope);
            }

            object IServiceProvider.GetService(Type serviceType) { return _scope.ServiceProvider.GetService(serviceType); }

            #region IDisposable Support
            void IDisposable.Dispose()
            {
                if (_scope == null) return; // already disposed

                _context.RequestServices = _priorRequestServices;
                _scope.Dispose();
                _scope = null;
            }
            #endregion
        }
    }
}
