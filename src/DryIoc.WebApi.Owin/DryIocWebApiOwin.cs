/*
The MIT License (MIT)

Copyright (c) 2013-2023 Maksim Volkau

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

namespace DryIoc.WebApi.Owin
{
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Hosting;
    using global::Owin;
    using DryIoc.Owin;

    /// <summary>Set of extension methods for gluing WebApi and OWIN together.</summary>
    public static class DryIocWebApiOwin
    {
        /// <summary>Inserts delegating handler that uses OWIN scoped container for WebApi dependency scope.</summary>
        /// <param name="app">App Builder</param> <param name="config"></param> <returns>App Builder</returns>
        public static IAppBuilder UseDryIocWebApi(this IAppBuilder app, HttpConfiguration config)
        {
            var handlers = config.ThrowIfNull().MessageHandlers;
            if (!handlers.OfType<SetRequestDependencyScopeHandler>().Any())
                handlers.Insert(0, new SetRequestDependencyScopeHandler());
            return app;
        }
    }

    /// <summary>Inserts <see cref="DryIocDependencyScope"/>from OWIN context to Request properties.</summary>
    public sealed class SetRequestDependencyScopeHandler : DelegatingHandler
    {
        /// <summary>Inserts <see cref="DryIocDependencyScope"/>from OWIN context to Request properties.</summary>
        /// <param name="request">Target request</param> <param name="cancellationToken"></param>
        /// <returns>Returns continuation.</returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var owinContext = request.GetOwinContext();
            var scopedContainer = owinContext?.GetDryIocScopedContainer();
            if (scopedContainer != null) // Use existing OWIN ScopeContainer for WebApi dependency resolver
                request.Properties[HttpPropertyKeys.DependencyScope] = new DryIocDependencyScope(scopedContainer);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
