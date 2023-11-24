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
using System.Reflection;
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
    /// By default container will use <see href="DryIocAdapter.MicrosoftDependencyInjectionRules" />
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
        public virtual IContainer CreateBuilder(IServiceCollection services)
        {
            if (_container != null)
                return _container.WithDependencyInjectionAdapter(services, _registerDescriptor, _registrySharing);
            return new Container(DryIocAdapter.MicrosoftDependencyInjectionRules)
                .WithDependencyInjectionAdapter(services, _registerDescriptor, _registrySharing, skipRulesCheck: true);
        }

        /// <inheritdoc />
        public virtual IServiceProvider CreateServiceProvider(IContainer container) =>
            container.BuildServiceProvider();
    }

    /// <summary>Adapts DryIoc container to be used as MS.DI service provider, plus provides the helpers
    /// to simplify work with adapted container.</summary>
    public static class DryIocAdapter
    {
        /// <summary>ParameterSelector to inject the service key into the parameter marked with [ServiceKey] attribute</summary>
        public static readonly ParameterSelector SelectServiceKeyForParameterWithServiceKeyAttribute =
            req => par =>
            {
                if (!par.IsDefined(typeof(ServiceKeyAttribute), false))
                    return null;
                return GetServiceKeyAsParameterValue(req, par);
            };

        /// <summary>ParameterSelector to inject the service key into the parameter marked with [ServiceKey] attribute</summary>
        public static readonly ParameterSelector SelectServiceKeyFor2ndParameterOfKeyedImplementationFactory =
            req => par =>
            {
                if (par.Position == 1)
                    return null;
                return GetServiceKeyAsParameterValue(req, par);
            };

        private static ParameterServiceInfo GetServiceKeyAsParameterValue(Request req, ParameterInfo par)
        {
            var serviceKey = req.ServiceKey;
            if (serviceKey == null)
                return null;
            if (!par.ParameterType.IsAssignableFrom(serviceKey.GetType()))
                throw new InvalidOperationException(
                    $"Unable to inject service key `{serviceKey.Print()}` into the #{par.Position} parameter `{par}` because of incompatible type.");
            return ParameterServiceInfo.Of(par, ServiceDetails.OfValue(serviceKey));
        }

        /// <summary>Creates the conforming rules for the Microsoft.Extension.DependencyInjection.</summary>
        public static Rules WithMicrosoftDependencyInjectionRules(this Rules rules) =>
            rules.WithBaseMicrosoftDependencyInjectionRules(SelectServiceKeyForParameterWithServiceKeyAttribute);

        /// <summary>The rules implementing the conventions of Microsoft.Extension.DependencyInjection</summary>
        public static readonly Rules MicrosoftDependencyInjectionRules =
            WithMicrosoftDependencyInjectionRules(Rules.Default);

        /// <summary>Checks if the rules "include" the same settings and conventions as the basic MicrosoftDependencyInjectionRules.
        /// It means that the rules may "include" other things, e.g. `WithConcreteTypeDynamicRegistrations`, etc.</summary>
        public static bool HasMicrosoftDependencyInjectionRules(this Rules rules) =>
            rules.HasBaseMicrosoftDependencyInjectionRules(MicrosoftDependencyInjectionRules) &&
            rules.Parameters == SelectServiceKeyForParameterWithServiceKeyAttribute;

        /// <summary>Adapts passed <paramref name="container"/> to Microsoft.DependencyInjection conventions,
        /// registers DryIoc implementations of <see cref="IServiceProvider"/> and <see cref="IServiceScopeFactory"/>,
        /// and returns NEW container.
        /// </summary>
        /// <param name="container">Source container to adapt.</param>
        /// <param name="descriptors">(optional) Specify service descriptors or use <see cref="Populate"/> later.</param>
        /// <param name="registerDescriptor">(optional) Custom registration action, should return true to skip normal registration.</param>
        /// <param name="registrySharing">(optional) Use DryIoc <see cref="RegistrySharing"/> capability.</param>
        /// <param name="skipRulesCheck">(optional) Skip the check if the container already has the MicrosoftDependencyInjectionRules.</param>
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
        ///     var serviceProvider = adaptedContainer.GetServiceProvider();
        ///
        ///]]></code>
        /// </example>
        /// <remarks>You still need to Dispose adapted container at the end / application shutdown.</remarks>
        public static IContainer WithDependencyInjectionAdapter(this IContainer container,
            IEnumerable<ServiceDescriptor> descriptors = null,
            Func<IRegistrator, ServiceDescriptor, bool> registerDescriptor = null,
            RegistrySharing registrySharing = RegistrySharing.Share,
            bool skipRulesCheck = false)
        {
            if (skipRulesCheck)
            {
                if (registrySharing != RegistrySharing.Share)
                    container = container.With(container.Rules, container.ScopeContext, registrySharing, container.SingletonScope);
            }
            else
            {
                var hasRules = HasMicrosoftDependencyInjectionRules(container.Rules);
                if (!hasRules)
                {
                    var newRules = WithMicrosoftDependencyInjectionRules(container.Rules);
                    container = container.With(newRules, container.ScopeContext, registrySharing, container.SingletonScope);
                }
                else if (registrySharing != RegistrySharing.Share)
                    container = container.With(container.Rules, container.ScopeContext, registrySharing, container.SingletonScope);
            }

            var serviceProvider = new DryIocServiceProvider(container);

            // those are singletons
            var singletons = container.SingletonScope;
            singletons.Use<IServiceProvider>(serviceProvider);
            singletons.Use<ISupportRequiredService>(serviceProvider);
            singletons.Use<IKeyedServiceProvider>(serviceProvider);

            singletons.Use<IServiceScopeFactory>(serviceProvider);
            singletons.Use<IServiceProviderIsService>(serviceProvider);
            singletons.Use<IServiceProviderIsKeyedService>(serviceProvider);

            if (descriptors != null)
                container.Populate(descriptors, registerDescriptor);

            return container;
        }

        /// <summary>Sugar to create the DryIoc container and adapter populated with services</summary>
        public static IServiceProvider CreateServiceProvider(this IServiceCollection services) =>
            new Container(MicrosoftDependencyInjectionRules).WithDependencyInjectionAdapter(services);

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

        /// <summary>Wraps the container in the service provider implementation.</summary>
        public static IServiceProvider BuildServiceProvider(this IContainer container) =>
            new DryIocServiceProvider(container);

        /// <summary>Gets the service provider.</summary>
        public static IServiceProvider GetServiceProvider(this IContainer container) =>
            container.BuildServiceProvider();

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
            container.WithCompositionRoot<TCompositionRoot>().BuildServiceProvider();

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
            if (descriptor.IsKeyedService)
            {
                serviceKey = descriptor.ServiceKey;
                if (serviceKey == KeyedService.AnyKey)
                    serviceKey = Registrator.AnyServiceKey;

                var implType = descriptor.KeyedImplementationType;
                if (implType != null)
                {
                    container.Register(ReflectionFactory.Of(implType, descriptor.Lifetime.ToReuse()), serviceType,
                        serviceKey, ifAlreadyRegistered, isStaticallyChecked: implType == serviceType || serviceType.IsAssignableFrom(implType));
                }
                else if (descriptor.KeyedImplementationFactory != null)
                {
                    var fac = descriptor.KeyedImplementationFactory;
                    container.RegisterFuncWithParameters(serviceType,
                        fac.GetType(), (Func<object, object, object>)fac.ToFuncWithObjParams,
                        SelectServiceKeyFor2ndParameterOfKeyedImplementationFactory,
                        descriptor.Lifetime.ToReuse(), Setup.Default, ifAlreadyRegistered, serviceKey);
                }
                else
                {
                    var instance = descriptor.KeyedImplementationInstance;
                    container.Register(InstanceFactory.Of(instance), serviceType,
                        serviceKey, ifAlreadyRegistered, isStaticallyChecked: true);
                    container.TrackDisposable(instance); // todo: @wip @incompatible calling this method depends on the `ifAlreadyRegistered` policy
                }
            }
            else
            {
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
                    container.TrackDisposable(instance); // todo: @wip @incompatible calling this method depends on the `ifAlreadyRegistered` policy
                }
            }
        }
    }

    // todo: @wip @remove
    // /// <summary>Bare-bones IServiceScope implementations</summary>
    // public sealed class DryIocServiceScope : IServiceScope
    // {
    //     /// <inheritdoc />
    //     public IServiceProvider ServiceProvider => _resolverContext;
    //     private readonly IResolverContext _resolverContext;

    //     /// <summary>Creating from resolver context</summary>
    //     public DryIocServiceScope(IResolverContext resolverContext) => 
    //         _resolverContext = resolverContext;

    //     /// <summary>Disposes the underlying resolver context</summary>
    //     public void Dispose() => _resolverContext.Dispose();
    // }

    /// <summary>Impl of `IsRegistered`, `GetRequiredService`, `CreateScope`.</summary>
    public sealed class DryIocServiceProvider : IDisposable,
        IServiceProvider, IServiceScopeFactory, IServiceScope,
        IServiceProviderIsService, ISupportRequiredService,
        IKeyedServiceProvider, IServiceProviderIsKeyedService
    {
        /// <summary>Exposes underlying (possible scoped) DryIoc container</summary>
        public readonly IContainer Container;

        /// <summary>Statefully wraps the passed <paramref name="container"/></summary>
        public DryIocServiceProvider(IContainer container) =>
            Container = container;

        IServiceScope IServiceScopeFactory.CreateScope()
        {
            var scopedContainer = Container.WithNewOpenScope();
            var scopedProvider = new DryIocServiceProvider(scopedContainer);
            var currentScope = scopedContainer.CurrentScope;
            currentScope.Use<IServiceProvider>(scopedProvider);
            currentScope.Use<ISupportRequiredService>(scopedProvider);
            currentScope.Use<IKeyedServiceProvider>(scopedProvider);
            return scopedProvider;
        }

        IServiceProvider IServiceScope.ServiceProvider => this;

        /// <inheritdoc />
        public object GetService(Type serviceType) =>
            Container.Resolve(serviceType,
                Container.Rules.ServiceProviderGetServiceShouldThrowIfUnresolved ? IfUnresolved.Throw : IfUnresolved.ReturnDefaultIfNotRegistered);

        /// <inheritdoc />
        public bool IsService(Type serviceType)
        {
            // I am not fully comprehend but MS.DI considers asking for the open-generic type even if it is registered to return `false`
            // Probably mixing here the fact that open type cannot be instantiated without providing the concrete type argument.
            // But I think it is conflating two things and making the reasoning harder.
            if (serviceType.IsGenericTypeDefinition)
                return false;

            if (serviceType == typeof(IServiceProviderIsService) |
                serviceType == typeof(ISupportRequiredService) |
                serviceType == typeof(IServiceScopeFactory) |
                serviceType == typeof(IServiceProvider) |
                serviceType == typeof(IKeyedServiceProvider) |
                serviceType == typeof(IServiceProviderIsKeyedService))
                return true;

            if (Container.IsRegistered(serviceType))
                return true;

            if (serviceType.IsGenericType &&
                Container.IsRegistered(serviceType.GetGenericTypeDefinition()))
                return true;

            return Container.IsRegistered(serviceType, factoryType: FactoryType.Wrapper);
        }

        /// <inheritdoc />
        public object GetRequiredService(Type serviceType) =>
            Container.Resolve(serviceType, IfUnresolved.Throw);

        /// <inheritdoc />
        public object GetKeyedService(Type serviceType, object serviceKey) =>
            Container.Resolve(serviceType, serviceKey,
                Container.Rules.ServiceProviderGetServiceShouldThrowIfUnresolved ? IfUnresolved.Throw : IfUnresolved.ReturnDefaultIfNotRegistered);

        /// <inheritdoc />
        public object GetRequiredKeyedService(Type serviceType, object serviceKey) =>
            Container.Resolve(serviceType, serviceKey, IfUnresolved.Throw);

        /// <inheritdoc />
        public bool IsKeyedService(Type serviceType, object serviceKey)
        {
            if (serviceType.IsGenericTypeDefinition)
                return false;

            if (Container.IsRegistered(serviceType, serviceKey))
                return true;

            if (serviceType.IsGenericType &&
                Container.IsRegistered(serviceType.GetGenericTypeDefinition(), serviceKey))
                return true;

            return Container.IsRegistered(serviceType, serviceKey, factoryType: FactoryType.Wrapper);
        }

        /// <inheritdoc />
        public void Dispose() => Container.Dispose();
    }
}
