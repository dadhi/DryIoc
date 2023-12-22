/*
The MIT License (MIT)

Copyright (c) 2021 Maksim Volkau

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

using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions; // for the ServiceCollection.Replace
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Logging;

using DryIoc.Microsoft.DependencyInjection;

namespace DryIoc.AzureFunctions
{
    /// <summary>
    /// TBD
    /// </summary>
    public static class DryIocAzureFunctions
    {
        /// <summary>
        /// TBD
        /// </summary>
        public static IFunctionsHostBuilder UseDryIoc(this IFunctionsHostBuilder hostBuilder, IContainer container = null)
        {
            if (container == null)
                container = new Container(DryIocAdapter.MicrosoftDependencyInjectionRules);
            else if (!DryIocAdapter.HasMicrosoftDependencyInjectionRules(container.Rules))
                container = container.With(DryIocAdapter.WithMicrosoftDependencyInjectionRules);

            hostBuilder.Services.AddSingleton(s =>
            {
                container.Populate(hostBuilder.Services);

                container.RegisterDelegate(
                    r => r.Resolve<ILoggerFactory>().CreateLogger(LogCategories.CreateFunctionUserCategory(r.Resolve<FunctionName>().Name)), 
                    Reuse.Scoped);

                return container;
            });

            // Funny, but we can actually Replace the Azure Functions ServiceProvider
            hostBuilder.Services.Replace(ServiceDescriptor.Singleton(typeof(IJobActivator),   typeof(DryIocJobActivator)));
            hostBuilder.Services.Replace(ServiceDescriptor.Singleton(typeof(IJobActivatorEx), typeof(DryIocJobActivator)));

            hostBuilder.Services.AddScoped<DryIocScopedResolverContext>();

            return hostBuilder;
        }

        /// <summary></summary>
        public sealed class FunctionName
        {
            /// <summary>The name</summary>
            public readonly string Name;
            internal FunctionName(string name) => Name = name;
        }

        internal sealed class DryIocScopedResolverContext : IDisposable
        {
            public readonly IResolverContext Scope;
            public DryIocScopedResolverContext(IContainer container) => 
                Scope = container.OpenScope(); // todo: @clarify do we need the named scope here?
            public void Dispose() => Scope.Dispose();
        }

        internal sealed class DryIocJobActivator : IJobActivator, IJobActivatorEx
        {
            private readonly IServiceProvider _serviceProvider;
            public DryIocJobActivator(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

            public T CreateInstance<T>() =>
                _serviceProvider.GetRequiredService<DryIocScopedResolverContext>().Scope.Resolve<T>();

            public T CreateInstance<T>(IFunctionInstanceEx functionInstance)
            {
                var functionServices = functionInstance.InstanceServices;
                var scope = (functionServices.GetService<DryIocScopedResolverContext>() 
                        ?? _serviceProvider.GetRequiredService<DryIocScopedResolverContext>()).Scope;

                var loggerFactory = functionServices.GetService<ILoggerFactory>();
                if (loggerFactory != null)
                    scope.Use(loggerFactory);

                // adding the function Name so that we can resolve the ILogger
                scope.Use(new FunctionName(functionInstance.FunctionDescriptor.ShortName));

                return scope.Resolve<T>();
            }
        }
    }
}
