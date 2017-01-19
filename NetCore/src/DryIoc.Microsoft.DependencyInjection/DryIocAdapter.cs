/*
The MIT License (MIT)

Copyright (c) 2016 Maksim Volkau

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

namespace DryIoc.Microsoft.DependencyInjection
{
    /// <summary>DryIoc. 
    /// Basically it is a DryIoc implementation of <see cref="IServiceProvider"/>.</summary>
    public static class DryIocAdapter
    {
        /// <summary>Creates new container from the <paramref name="container"/> adapted to be used
        /// with AspNetCore Dependency Injection:
        /// - First configures container with DI conventions.
        /// - Then registers DryIoc implementations of <see cref="IServiceProvider"/> and <see cref="IServiceScopeFactory"/>.
        /// </summary>
        /// <param name="container">Source container to adapt.</param>
        /// <param name="descriptors">(optional) Specify service descriptors or use <see cref="Populate"/> later.</param>
        /// <param name="registerDescriptor">(optional) Custom registration action, should return true to skip normal registration.</param>
        /// <param name="throwIfUnresolved">(optional) Instructs DryIoc to throw exception
        /// for unresolved type instead of fallback to default Resolver.</param>
        /// <returns>New container adapted to AspNetCore DI conventions.</returns>
        /// <example>
        /// <code><![CDATA[
        ///     container = new Container().WithDependencyInjectionAdapter(services);
        ///     serviceProvider = container.Resolve<IServiceProvider>();
        ///     
        ///     // To register service per Request use Reuse.WebRequest or Reuse.InCurrenScope,
        ///     // both will work
        ///     container.Register<IMyService, MyService>(Reuse.InWebRequest)
        ///     // or
        ///     container.Register<IMyService, MyService>(Reuse.InCurrenScope)
        /// ]]></code>
        /// </example>
        public static IContainer WithDependencyInjectionAdapter(this IContainer container,
            IEnumerable<ServiceDescriptor> descriptors = null,
            Func<IRegistrator, ServiceDescriptor, bool> registerDescriptor = null,
            Func<Type, bool> throwIfUnresolved = null)
        {
            if (container.ScopeContext != null)
                throw new ArgumentException("Adapted container uses ambient scope context which is not supported by AspNetCore DI.");

            var adapter = container.With(rules => rules
                .With(FactoryMethod.ConstructorWithResolvableArguments)
                .WithFactorySelector(Rules.SelectLastRegisteredFactory())
                .WithTrackingDisposableTransients()
                .WithImplicitRootOpenScope());

            adapter.Register<IServiceProvider, DryIocServiceProvider>(Reuse.InCurrentScope, 
                Parameters.Of.Type(_ => throwIfUnresolved));

            // Scope factory should be scoped itself to enable nested scopes creation
            adapter.Register<IServiceScopeFactory, DryIocServiceScopeFactory>(Reuse.InCurrentScope);

            // Register asp net abstractions specified by descriptors in container 
            if (descriptors != null)
                adapter.Populate(descriptors, registerDescriptor);

            return adapter;
        }

        /// <summary>Convinient helper to consolidate DryIoc registrations in specified <typeparamref name="TCompositionRoot"/></summary>
        /// <typeparam name="TCompositionRoot">Class with will be created by container on Startup, give a chance to do registrations.</typeparam>
        /// <param name="container">Container with DI adapter.</param>
        /// <returns>Service provider for both adapted services collection and registrations done in <typeparamref name="TCompositionRoot"/>.</returns>
        /// <example>
        /// <code><![CDATA[
        /// // Example of CompositionRoot: 
        /// public class CompositionRoot
        /// {
        ///    // if you need the whole container then change parameter type from IRegistrator to IContainer
        ///    public CompositionRoot(IRegistrator r)
        ///    {
        ///        r.Register<ISingletonService, SingletonService>(Reuse.Singleton);
        ///        r.Register<ITransientService, TransientService>(Reuse.Transient);
        ///        r.Register<IScopedService, ScopedService>(Reuse.InCurrentScope);
        ///    }
        /// }
        /// ]]></code>
        /// </example>
        public static IServiceProvider ConfigureServiceProvider<TCompositionRoot>(this IContainer container)
        {
            container.Register<TCompositionRoot>();
            container.Resolve<TCompositionRoot>();
            return container.Resolve<IServiceProvider>();
        }

        /// <summary>Registers service descriptors into container. May be called multiple times with different service collections.</summary>
        /// <param name="container">The container.</param>
        /// <param name="descriptors">The service descriptors.</param>
        /// <param name="registerDescriptor">(optional) Custom registration action, should return true to skip normal registration.</param>
        /// <example>
        /// <code><![CDATA[
        ///     // example of normal descriptor registration together with factory method registration for SomeService.
        ///     container.Populate(services, (r, service) => {
        ///         if (service.ServiceType == typeof(SomeService)) {
        ///             r.Register<SomeService>(Made.Of(() => CreateCustomService()), Reuse.Singleton);
        ///             return true;
        ///         };
        ///         return false; // fallback to normal registrations for the rest of the descriptors.
        ///     });
        /// ]]></code>
        /// </example>
        public static void Populate(this IContainer container, IEnumerable<ServiceDescriptor> descriptors,
            Func<IRegistrator, ServiceDescriptor, bool> registerDescriptor = null)
        {
            foreach (var descriptor in descriptors)
            {
                if (registerDescriptor == null || !registerDescriptor(container, descriptor))
                    container.RegisterDescriptor(descriptor);
            }
        }

        /// <summary>Uses passed descriptor to register service in container: 
        /// maps DI Lifetime to DryIoc Reuse,
        /// and DI registration type to corresponding DryIoc Register, RegisterDelegate or RegisterInstance.</summary>
        /// <param name="container">The container.</param>
        /// <param name="descriptor">Service descriptor.</param>
        public static void RegisterDescriptor(this IContainer container, ServiceDescriptor descriptor)
        {
            var reuse = ConvertLifetimeToReuse(descriptor.Lifetime);

            if (descriptor.ImplementationType != null)
            {
                container.Register(descriptor.ServiceType, descriptor.ImplementationType, reuse);
            }
            else if (descriptor.ImplementationFactory != null)
            {
                container.RegisterDelegate(descriptor.ServiceType,
                    r => descriptor.ImplementationFactory(r.Resolve<IServiceProvider>()), 
                    reuse);
            }
            else
            {
                // todo: v1.1: plan to specify preventDisposal for the instances
                container.RegisterDelegate(descriptor.ServiceType, _ => descriptor.ImplementationInstance);
            }
        }

        private static IReuse ConvertLifetimeToReuse(ServiceLifetime lifetime)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    // todo: Wait until Singletons are actual singletons, then remove Rule.WithImplicitRootOpenScope from adapter
                    return Reuse.InCurrentNamedScope(Container.NonAmbientRootScopeName);
                case ServiceLifetime.Scoped:
                    return Reuse.InCurrentScope;
                case ServiceLifetime.Transient:
                    return Reuse.Transient;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, "Not supported lifetime");
            }
        }
    }

    /// <summary>Delegates service resolution to wrapped DryIoc scoped container.
    /// When disposed, disposes the scoped container.</summary>
    public sealed class DryIocServiceProvider : IServiceProvider, ISupportRequiredService, IDisposable
    {
        private readonly IContainer _scopedContainer;
        private readonly Func<Type, bool> _throwIfUnresolved;

        /// <summary>Uses passed container for scoped resolutions.</summary> 
        /// <param name="scopedContainer">subj.</param>
        /// <param name="throwIfUnresolved">(optional) Instructs DryIoc to throw exception
        /// for unresolved type instead of fallback to default Resolver.</param>
        public DryIocServiceProvider(IContainer scopedContainer, Func<Type, bool> throwIfUnresolved)
        {
            _scopedContainer = scopedContainer;
            _throwIfUnresolved = throwIfUnresolved;
        }

        /// <summary>Delegates resolution to scoped container. 
        /// Uses <see cref="IfUnresolved.ReturnDefault"/> policy to return default value in case of resolution error.</summary>
        /// <param name="serviceType">Service type to resolve.</param>
        /// <returns>Resolved service object.</returns>
        public object GetService(Type serviceType)
        {
            var ifUnresolvedReturnDefault = _throwIfUnresolved == null || !_throwIfUnresolved(serviceType);
            return _scopedContainer.Resolve(serviceType, ifUnresolvedReturnDefault);
        }

        /// <summary> Gets service of type <paramref name="serviceType" /> from the <see cref="T:System.IServiceProvider" /> implementing
        /// this interface. </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>A service object of type <paramref name="serviceType" />.
        /// Throws an exception if the <see cref="T:System.IServiceProvider" /> cannot create the object.</returns>
        public object GetRequiredService(Type serviceType)
        {
            return _scopedContainer.Resolve(serviceType);
        }

        /// <summary>Disposes scoped container and scope.</summary>
        public void Dispose()
        {
            _scopedContainer.Dispose();
        }
    }

    /// <summary>The goal of the factory is create / open new scope.
    /// The factory itself is scoped (not a singleton). 
    /// That means you need to resolve factory from outer scope to create nested scope.</summary>
    public sealed class DryIocServiceScopeFactory: IServiceScopeFactory
    {
        /// <summary>Using <see cref="Reuse.WebRequestScopeName"/> allows registration with both
        /// <see cref="Reuse.InCurrentScope"/> and <see cref="Reuse.InWebRequest"/>.</summary>
        public static readonly string DefaultScopeName = Reuse.WebRequestScopeName;

        private readonly IContainer _scopedContainer;

        /// <summary>Stores passed scoped container to open nested scope.</summary>
        /// <param name="scopedContainer">Scoped container to be used to create nested scope.</param>
        public DryIocServiceScopeFactory(IContainer scopedContainer)
        {
            _scopedContainer = scopedContainer;
        }

        /// <summary>Opens scope and wraps it into DI <see cref="IServiceScope"/> interface.</summary>
        /// <returns>DI wrapper of opened scope.</returns>
        /// <remarks>The scope name is defaulted to <see cref="Reuse.WebRequestScopeName"/>.</remarks>
        public IServiceScope CreateScope()
        {
            var scope = _scopedContainer.OpenScope(DefaultScopeName);
            return new DryIocServiceScope(scope.Resolve<IServiceProvider>());
        }

        private sealed class DryIocServiceScope : IServiceScope
        {
            public IServiceProvider ServiceProvider { get; }

            public DryIocServiceScope(IServiceProvider serviceProvider)
            {
                ServiceProvider = serviceProvider;
            }

            public void Dispose() => (ServiceProvider as IDisposable)?.Dispose();
        }
    }
}