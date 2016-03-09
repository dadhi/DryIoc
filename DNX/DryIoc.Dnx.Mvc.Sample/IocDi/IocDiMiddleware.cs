using System;
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

        public IocDiMiddleware(RequestDelegate next)
        {
            if (next == null) throw new ArgumentNullException(nameof(next));
            _next = next;
        }

        public async Task Invoke(HttpContext context, IContainer scopedContainer)
        {
            scopedContainer.RegisterInstance(context, Reuse.InCurrentScope, IfAlreadyRegistered.Replace, preventDisposal: true);
            await _next(context);
        }
    }
}
