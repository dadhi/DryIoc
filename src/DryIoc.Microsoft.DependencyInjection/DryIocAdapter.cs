/*
The MIT License (MIT)

Copyright (c) 2016-2023 Maksim Volkau

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
using System.Runtime.CompilerServices;  // for MethodImplAttribute
using Microsoft.Extensions.DependencyInjection;

namespace DryIoc.Microsoft.DependencyInjection
{
    /// <summary>
    /// This DryIoc is supposed to be used with generic `IHostBuilder` like this:
    /// 
    /// <code><![CDATA[
    /// public class Program
    /// {
    ///     public static async Task Main(string[] args) => 
    ///         await CreateHostBuilder(args).Build().RunAsync();
    /// 
    ///     Rules WithMyRules(Rules currentRules) => currentRules;
    ///
    ///     public static IHostBuilder CreateHostBuilder(string[] args) =>
    ///         Host.CreateDefaultBuilder(args)
    ///             .UseServiceProviderFactory(new DryIocServiceProviderFactory(new Container(rules => WithMyRules(rules))))
    ///             .ConfigureWebHostDefaults(webBuilder =>
    ///             {
    ///                 webBuilder.UseStartup<Startup>();
    ///             });
    /// }
    /// ]]></code>
    /// 
    /// Then register your services in `Startup.ConfigureContainer`.
    /// 
    /// DON'T try to change the container rules there - they will be lost, 
    /// instead pass the pre-configured container to `DryIocServiceProviderFactory` as in example above.
    /// By default container will use <see href="DryIoc.Rules.MicrosoftDependencyInjectionRules" />
    /// 
    /// DON'T forget to add `services.AddControllers().AddControllersAsServices()` in `Startup.ConfigureServices` 
    /// in order to access DryIoc diagnostics for controllers, property-injection, etc.
    /// 
    /// </summary>
    public class DryIocServiceProviderFactory : IServiceProviderFactory<IContainer>
    {
        private readonly IContainer _container;
        private readonly Func<IRegistrator, ServiceDescriptor, bool> _registerDescriptor;
        private readonly RegistrySharing _registrySharing;

        /// <summary>
        /// We won't initialize the container here because it is logically expected to be done in `CreateBuilder`,
        /// so the factory constructor is just saving some options to use later.
        /// </summary>
        public DryIocServiceProviderFactory(
            IContainer container = null,
            Func<IRegistrator, ServiceDescriptor, bool> registerDescriptor = null) :
            this(container, RegistrySharing.CloneAndDropCache, registerDescriptor)
        { }

        /// <summary>
        /// `container` is the existing container which will be cloned with the MS.DI rules and its cache will be dropped,
        /// unless the `registrySharing` is set to the `RegistrySharing.Share` or to `RegistrySharing.CloneButKeepCache`.
        /// `registerDescriptor` is the custom service descriptor handler.
        /// </summary>
        public DryIocServiceProviderFactory(IContainer container, RegistrySharing registrySharing,
            Func<IRegistrator, ServiceDescriptor, bool> registerDescriptor = null)
        {
            _container = container;
            _registrySharing = registrySharing;
            _registerDescriptor = registerDescriptor;
        }

        /// <inheritdoc />
        public virtual IContainer CreateBuilder(IServiceCollection services) =>
            (_container ?? new Container(Rules.MicrosoftDependencyInjectionRules))
                .WithDependencyInjectionAdapter(services, _registerDescriptor, _registrySharing);

        /// <inheritdoc />
        public virtual IServiceProvider CreateServiceProvider(IContainer container) =>
            container.BuildServiceProvider();
    }

    /// <summary>Adapts DryIoc container to be used as MS.DI service provider, plus provides the helpers
    /// to simplify work with adapted container.</summary>
    public static class DryIocAdapter
    {
        /// <summary>Adapts passed <paramref name="container"/> to Microsoft.DependencyInjection conventions,
        /// registers DryIoc implementations of <see cref="IServiceProvider"/> and <see cref="IServiceScopeFactory"/>,
        /// and returns NEW container.
        /// </summary>
        /// <param name="container">Source container to adapt.</param>
        /// <param name="descriptors">(optional) Specify service descriptors or use <see cref="Populate"/> later.</param>
        /// <param name="registerDescriptor">(optional) Custom registration action, should return true to skip normal registration.</param>
        /// <param name="registrySharing">(optional) Use DryIoc <see cref="RegistrySharing"/> capability.</param>
        /// <example>
        /// <code><![CDATA[
        /// 
        ///     var container = new Container();
        ///
        ///     // you may register the services here:
        ///     container.Register<IMyService, MyService>(Reuse.Scoped)
        /// 
        ///     // applies the MS.DI rules and registers the infrastructure helpers and service collection to the container
        ///     var adaptedContainer = container.WithDependencyInjectionAdapter(services); 
        ///
        ///     // the container implements IServiceProvider
        ///     IServiceProvider serviceProvider = adaptedContainer;
        ///
        ///]]></code>
        /// </example>
        /// <remarks>You still need to Dispose adapted container at the end / application shutdown.</remarks>
        public static IContainer WithDependencyInjectionAdapter(this IContainer container,
            IEnumerable<ServiceDescriptor> descriptors = null,
            Func<IRegistrator, ServiceDescriptor, bool> registerDescriptor = null,
            RegistrySharing registrySharing = RegistrySharing.Share)
        {
            var hasMicrosoftDependencyInjectionRules = container.Rules.HasMicrosoftDependencyInjectionRules();
            if (!hasMicrosoftDependencyInjectionRules)
                container = container.With(container.Rules.WithMicrosoftDependencyInjectionRules(), container.ScopeContext, registrySharing, container.SingletonScope);
            else if (registrySharing != RegistrySharing.Share)
                container = container.With(container.Rules, container.ScopeContext, registrySharing, container.SingletonScope);

            var capabilities = new DryIocServiceProviderCapabilities(container);
            var singletons = container.SingletonScope;
            singletons.Use<IServiceProviderIsService>(capabilities);
            singletons.Use<ISupportRequiredService>(capabilities);
            singletons.Use<IServiceScopeFactory>(capabilities);

            if (descriptors != null)
                container.Populate(descriptors, registerDescriptor);

            return container;
        }

        /// <summary>Sugar to create the DryIoc container and adapter populated with services</summary>
        public static IServiceProvider CreateServiceProvider(this IServiceCollection services) =>
            new Container(DryIoc.Rules.MicrosoftDependencyInjectionRules).WithDependencyInjectionAdapter(services);

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

        /// <summary>It does not really build anything, it just gets the `IServiceProvider` from the container.</summary>
        public static IServiceProvider BuildServiceProvider(this IContainer container) =>
            container.GetServiceProvider();

        /// <summary>Just gets the `IServiceProvider` from the container.</summary>
        public static IServiceProvider GetServiceProvider(this IResolver container) =>
            container;

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

        /// <summary>Converts the MS.DI ServiceLifetime into the corresponding `DryIoc.IReuse`</summary>
        [MethodImpl((MethodImplOptions)256)]
        public static IReuse ToReuse(this ServiceLifetime lifetime) =>
            lifetime == ServiceLifetime.Singleton ? Reuse.Singleton :
            lifetime == ServiceLifetime.Scoped ? Reuse.ScopedOrSingleton : // see, that we have Reuse.ScopedOrSingleton here instead of Reuse.Scoped
            Reuse.Transient;

        /// <summary>Unpacks the service descriptor to register the service in DryIoc container
        /// with the default MS.DI convention of `IfAlreadyRegistered.AppendNotKeyed`</summary>
        public static void RegisterDescriptor(this IContainer container, ServiceDescriptor descriptor) =>
            container.RegisterDescriptor(descriptor, IfAlreadyRegistered.AppendNotKeyed);

        /// <summary>Unpacks the service descriptor to register the service in DryIoc container
        /// with the specific `IfAlreadyRegistered` policy and the optional `serviceKey`</summary>
        public static void RegisterDescriptor(this IContainer container, ServiceDescriptor descriptor, IfAlreadyRegistered ifAlreadyRegistered,
            object serviceKey = null)
        {
            var serviceType = descriptor.ServiceType;
            var implType = descriptor.ImplementationType;
            if (implType != null)
            {
                container.Register(ReflectionFactory.Of(implType, descriptor.Lifetime.ToReuse()), serviceType,
                    serviceKey, ifAlreadyRegistered, isStaticallyChecked: implType == serviceType || serviceType.IsAssignableFrom(implType));
            }
            else if (descriptor.ImplementationFactory != null)
            {
                container.Register(DelegateFactory.Of(descriptor.ImplementationFactory, descriptor.Lifetime.ToReuse()), serviceType,
                    serviceKey, ifAlreadyRegistered, isStaticallyChecked: true);
            }
            else
            {
                var instance = descriptor.ImplementationInstance;
                container.Register(InstanceFactory.Of(instance), serviceType,
                    serviceKey, ifAlreadyRegistered, isStaticallyChecked: true);
                container.TrackDisposable(instance); // todo: @wip @incompatible calling this method depend on the `ifAlreadyRegistered` policy
            }
        }
    }

    /// <summary>Bare-bones IServiceScope implementations</summary>
    public sealed class DryIocServiceScope : IServiceScope
    {
        /// <inheritdoc />
        public IServiceProvider ServiceProvider => _resolverContext;
        private readonly IResolverContext _resolverContext;

        /// <summary>Creating from resolver context</summary>
        public DryIocServiceScope(IResolverContext resolverContext) => _resolverContext = resolverContext;

        /// <summary>Disposes the underlying resolver context</summary>
        public void Dispose() => _resolverContext.Dispose();
    }

    /// <summary>Impl of `IsRegistered`, `GetRequiredService`, `CreateScope`.</summary>
    public sealed class DryIocServiceProviderCapabilities : IServiceProviderIsService, ISupportRequiredService, IServiceScopeFactory
    {
        private readonly IContainer _container;
        /// <summary>Statefully wraps the passed <paramref name="container"/></summary>
        public DryIocServiceProviderCapabilities(IContainer container) => _container = container;

        /// <inheritdoc />
        public bool IsService(Type serviceType)
        {
            // I am not fully comprehend but MS.DI considers asking for the open-generic type even if it is registered to return `false`
            // Probably mixing here the fact that open type cannot be instantiated without providing the concrete type argument.
            // But I think it is conflating two things and making the reasoning harder.
            if (serviceType.IsGenericTypeDefinition)
                return false;

            if (serviceType == typeof(IServiceProviderIsService) ||
                serviceType == typeof(ISupportRequiredService) ||
                serviceType == typeof(IServiceScopeFactory) ||
                serviceType == typeof(IServiceProvider))
                return true;

            if (_container.IsRegistered(serviceType))
                return true;

            if (serviceType.IsGenericType &&
                _container.IsRegistered(serviceType.GetGenericTypeDefinition()))
                return true;

            return _container.IsRegistered(serviceType, factoryType: FactoryType.Wrapper);
        }

        /// <inheritdoc />
        public object GetRequiredService(Type serviceType) => _container.Resolve(serviceType);

        /// <inheritdoc />
        public IServiceScope CreateScope() => new DryIocServiceScope(_container.WithNewOpenScope());
    }
}
