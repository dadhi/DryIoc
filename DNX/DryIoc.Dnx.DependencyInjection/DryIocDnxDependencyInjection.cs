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

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace DryIoc.Dnx.DependencyInjection
{
    /// <summary>Provides populating of service collection.</summary>
    public static class DryIocDnxDependencyInjection
    {
        /// <summary>Populates the container using the specified service descriptors. </summary>
        /// <param name="container">The container.</param>
        /// <param name="descriptors">The service descriptors.</param>
        public static void Populate(this IContainer container, IEnumerable<ServiceDescriptor> descriptors)
        {
            container.Register<IServiceProvider, DryIocServiceProvider>(Reuse.Singleton);
            container.Register<IServiceScopeFactory, DryIocServiceScopeFactory>(Reuse.Singleton);

            foreach (var descriptor in descriptors)
                RegisterDescriptor(container, descriptor);
        }

        private static IReuse MapLifetimeToReuse(ServiceLifetime lifetime)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    return Reuse.Singleton;
                case ServiceLifetime.Scoped:
                    return Reuse.InCurrentScope;
                case ServiceLifetime.Transient:
                    return Reuse.Transient;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, "Not supported lifetime");
            }
        }

        private static void RegisterDescriptor(IContainer container, ServiceDescriptor descriptor)
        {
            var reuse = MapLifetimeToReuse(descriptor.Lifetime);

            if (descriptor.ImplementationType != null)
            {
                container.Register(descriptor.ServiceType, descriptor.ImplementationType, reuse);
            }
            else if (descriptor.ImplementationFactory != null)
            {
                Func<IResolver, object> factory = r => descriptor.ImplementationFactory(r.Resolve<IServiceProvider>());
                container.RegisterDelegate(descriptor.ServiceType, factory, reuse);
            }
            else
            {
                container.RegisterInstance(descriptor.ServiceType, descriptor.ImplementationInstance, reuse);
            }
        }
    }

    public sealed class DryIocServiceProvider : IServiceProvider
    {
        private readonly IResolver _resolver;

        public DryIocServiceProvider(IResolver resolver)
        {
            _resolver = resolver;
        }

        public object GetService(Type serviceType)
        {
            return _resolver.Resolve(serviceType, ifUnresolvedReturnDefault: true);
        }
    }

    public sealed class DryIocServiceScopeFactory : IServiceScopeFactory
    {
        private readonly IContainer _container;

        public DryIocServiceScopeFactory(IContainer container)
        {
            _container = container;
        }

        public IServiceScope CreateScope()
        {
            return new DryIocServiceScope(_container.OpenScope());
        }

        private sealed class DryIocServiceScope : IServiceScope
        {
            private readonly IContainer _scopedContainer;

            public IServiceProvider ServiceProvider { get; private set; }

            public DryIocServiceScope(IContainer scopedContainer)
            {
                _scopedContainer = scopedContainer;
                ServiceProvider = scopedContainer.Resolve<IServiceProvider>();
            }

            public void Dispose()
            {
                _scopedContainer.Dispose();
            }
        }
    }
}