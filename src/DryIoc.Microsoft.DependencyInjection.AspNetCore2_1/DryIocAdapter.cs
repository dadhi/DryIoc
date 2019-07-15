/*
The MIT License (MIT)

Copyright (c) 2016-2019 Maksim Volkau

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
    /// <summary>Adapts DryIoc container to be used as MS.DI service provider, plus provides the helpers
    /// to simplify work with adapted container.</summary>
    public static class DryIocAdapter
    {
        /// Creates the container and the `IServiceProvider` because its implemented by `IContainer` -
        /// you get simply the best of both worlds.
        public static IContainer Create(
            IEnumerable<ServiceDescriptor> services,
            Func<IRegistrator, ServiceDescriptor, bool> registerService = null)
        {
            var container = new Container(Rules.MicrosoftDependencyInjectionRules);

            container.Use<IServiceScopeFactory>(r => new DryIocServiceScopeFactory(r));
            container.Populate(services, registerService);

            return container;
        }

        /// <summary>Adapts passed <paramref name="container"/> to Microsoft.DependencyInjection conventions,
        /// registers DryIoc implementations of <see cref="IServiceProvider"/> and <see cref="IServiceScopeFactory"/>,
        /// and returns NEW container.
        /// </summary>
        /// <param name="container">Source container to adapt.</param>
        /// <param name="descriptors">(optional) Specify service descriptors or use <see cref="Populate"/> later.</param>
        /// <param name="registerDescriptor">(optional) Custom registration action, should return true to skip normal registration.</param>
        /// <example>
        /// <code><![CDATA[
        /// 
        ///     var container = new Container();
        ///
        ///     // you may register the services here:
        ///     container.Register<IMyService, MyService>(Reuse.Scoped)
        /// 
        ///     var adaptedContainer = container.WithDependencyInjectionAdapter(services);
        ///     IServiceProvider serviceProvider = adaptedContainer; // the container implements IServiceProvider now
        ///
        ///]]></code>
        /// </example>
        /// <remarks>You still need to Dispose adapted container at the end / application shutdown.</remarks>
        public static IContainer WithDependencyInjectionAdapter(this IContainer container,
            IEnumerable<ServiceDescriptor> descriptors = null,
            Func<IRegistrator, ServiceDescriptor, bool> registerDescriptor = null)
        {
            if (container.Rules != Rules.MicrosoftDependencyInjectionRules)
                container = container.With(rules => rules
                    .With(FactoryMethod.ConstructorWithResolvableArguments)
                    .WithFactorySelector(Rules.SelectLastRegisteredFactory())
                    .WithTrackingDisposableTransients());

            container.Use<IServiceScopeFactory>(r => new DryIocServiceScopeFactory(r));

            // Registers service collection
            if (descriptors != null)
                container.Populate(descriptors, registerDescriptor);

#if NETSTANDARD1_0
            container.Use<IServiceProvider>(new DryIocServiceProvider(container));
#endif

            return container;
        }

        /// <summary>Adds services registered in <paramref name="compositionRootType"/> to container</summary>
        public static IContainer WithCompositionRoot(this IContainer container, Type compositionRootType)
        {
            container.Register(compositionRootType);
            container.Resolve(compositionRootType);
            return container;
        }

        /// <summary>Adds services registered in <typeparamref name="TCompositionRoot"/> to container</summary>
        public static IContainer WithCompositionRoot<TCompositionRoot>(this IContainer container) =>
            container.WithCompositionRoot(typeof(TCompositionRoot));

        /// It does not really build anything, it just gets the `IServiceProvider` from the container.
        public static IServiceProvider BuildServiceProvider(this IContainer container) =>
            container.GetServiceProvider();

        /// Just gets the `IServiceProvider` from the container.
        public static IServiceProvider GetServiceProvider(this IResolver container) =>
#if NETSTANDARD1_0
            container.Resolve<IServiceProvider>();
#else
            container;
#endif


        /// <summary>Facade to consolidate DryIoc registrations in <typeparamref name="TCompositionRoot"/></summary>
        /// <typeparam name="TCompositionRoot">The class will be created by container on Startup 
        /// to enable registrations with injected <see cref="IRegistrator"/> or full <see cref="IContainer"/>.</typeparam>
        /// <param name="container">Adapted container</param> <returns>Service provider</returns>
        /// <example>
        /// <code><![CDATA[
        /// public class ExampleCompositionRoot
        /// {
        ///    // if you need the whole container then change parameter type from IRegistrator to IContainer
        ///    public ExampleCompositionRoot(IRegistrator r)
        ///    {
        ///        r.Register<ISingletonService, SingletonService>(Reuse.Singleton);
        ///        r.Register<ITransientService, TransientService>(Reuse.Transient);
        ///        r.Register<IScopedService, ScopedService>(Reuse.InCurrentScope);
        ///    }
        /// }
        /// ]]></code>
        /// </example>
        public static IServiceProvider ConfigureServiceProvider<TCompositionRoot>(this IContainer container) =>
            container.WithCompositionRoot<TCompositionRoot>().GetServiceProvider();

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
            if (registerDescriptor == null)
                foreach (var descriptor in descriptors)
                    container.RegisterDescriptor(descriptor);
            else
                foreach (var descriptor in descriptors)
                    if (!registerDescriptor(container, descriptor))
                        container.RegisterDescriptor(descriptor);
        }

        /// <summary>Uses passed descriptor to register service in container: 
        /// maps DI Lifetime to DryIoc Reuse,
        /// and DI registration type to corresponding DryIoc Register, RegisterDelegate or RegisterInstance.</summary>
        /// <param name="container">The container.</param>
        /// <param name="descriptor">Service descriptor.</param>
        public static void RegisterDescriptor(this IContainer container, ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationType != null)
            {
                var reuse = descriptor.Lifetime == ServiceLifetime.Singleton ? Reuse.Singleton
                    : descriptor.Lifetime == ServiceLifetime.Scoped ? Reuse.ScopedOrSingleton
                    : Reuse.Transient;

                container.Register(descriptor.ServiceType, descriptor.ImplementationType, reuse);
            }
            else if (descriptor.ImplementationFactory != null)
            {
                var reuse = descriptor.Lifetime == ServiceLifetime.Singleton ? Reuse.Singleton
                    : descriptor.Lifetime == ServiceLifetime.Scoped ? Reuse.ScopedOrSingleton
                    : Reuse.Transient;

                container.RegisterDelegate(true, descriptor.ServiceType,
#if NETSTANDARD1_0
                    r => descriptor.ImplementationFactory(r.Resolve<IServiceProvider>()),
#else
                    descriptor.ImplementationFactory,
#endif
                    reuse);
            }
            else
            {
                container.RegisterInstance(true, descriptor.ServiceType, descriptor.ImplementationInstance);
            }
        }
    }

#if NETSTANDARD1_0
    /// Bare-bones IServiceScope implementations
    public sealed class DryIocServiceProvider : IServiceProvider
    {
        private readonly IResolverContext _resolverContext;

        /// Creating from resolver context
        public DryIocServiceProvider(IResolverContext resolverContext)
        {
            _resolverContext = resolverContext;
        }

        /// <inheritdoc />
        public object GetService(Type serviceType) => 
            _resolverContext.Resolve(serviceType, IfUnresolved.ReturnDefaultIfNotRegistered);
    }
#endif

    /// <summary>Creates/opens new scope in passed scoped container.</summary>
    public sealed class DryIocServiceScopeFactory : IServiceScopeFactory
    {
        private readonly IResolverContext _scopedResolver;

        /// <summary>Stores passed scoped container to open nested scope.</summary>
        /// <param name="scopedResolver">Scoped container to be used to create nested scope.</param>
        public DryIocServiceScopeFactory(IResolverContext scopedResolver)
        {
            _scopedResolver = scopedResolver;
        }

        /// <summary>Opens scope and wraps it into DI <see cref="IServiceScope"/> interface.</summary>
        /// <returns>DI wrapper of opened scope.</returns>
        public IServiceScope CreateScope()
        {
            var r = _scopedResolver;
            var scope = r.ScopeContext == null ? new Scope(r.CurrentScope) : r.ScopeContext.SetCurrent(p => new Scope(p));
            return new DryIocServiceScope(r.WithCurrentScope(scope));
        }
    }

    /// Bare-bones IServiceScope implementations
    public sealed class DryIocServiceScope : IServiceScope
    {
        /// <inheritdoc />
        public IServiceProvider ServiceProvider
        {
            get
            {
#if NETSTANDARD1_0
                return new DryIocServiceProvider(_resolverContext);
#else
                return _resolverContext;
#endif
            }
        }

        private readonly IResolverContext _resolverContext;

        /// Creating from resolver context
        public DryIocServiceScope(IResolverContext resolverContext)
        {
            _resolverContext = resolverContext;
        }

        /// Disposes the underlying resolver context 
        public void Dispose() => _resolverContext.Dispose();
    }


    /// This is a implementation supposed to be used with the `HostBuilder` like this:
    /// <code><![CDATA[
    /// static async Task Main()
    /// {
    ///     var host = new HostBuilder()
    ///         .ConfigureServices(services =>
    ///         {
    ///             services.AddHostedService<MyBootstrapService>();
    ///         })
    ///         .UseServiceProviderFactory(  new DryIocServiceProviderFactory()  )
    ///         .ConfigureContainer<Container>((hostContext, container) =>
    ///         {
    ///             container.Register<FooService>(Reuse.Scoped);
    ///             // etc.
    ///         })
    ///         .Build();
    ///
    ///     await host.RunAsync(); 
    /// }
    /// ]]></code>
    public class DryIocServiceProviderFactory : IServiceProviderFactory<IContainer>
    {
        private readonly IContainer _container;
        private readonly Func<IRegistrator, ServiceDescriptor, bool> _registerDescriptor;

        /// Some options to push to `.WithDependencyInjectionAdapter(...)`
        public DryIocServiceProviderFactory(
            IContainer container = null,
            Func<IRegistrator, ServiceDescriptor, bool> registerDescriptor = null)
        {
            _container = container;
            _registerDescriptor = registerDescriptor;
        }

        /// <inheritdoc />
        public IContainer CreateBuilder(IServiceCollection services) =>
            (_container ?? new Container()).WithDependencyInjectionAdapter(services, _registerDescriptor);

        /// <inheritdoc />
        public IServiceProvider CreateServiceProvider(IContainer container) => 
            container.BuildServiceProvider();
    }
}
