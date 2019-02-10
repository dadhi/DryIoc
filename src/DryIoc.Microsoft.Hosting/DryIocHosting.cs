using System;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DryIoc.Microsoft.Hosting
{
    /// <summary>Entry extension methods </summary>
    public static class DryIocHosting
    {
        /// <summary>
        /// Add DryIoc service provider to the host with optional pre-configured container via <paramref name="configureContainer"/>:
        /// <code><![CDATA[
        ///     var container = new Container(rules => rules.With...);
        ///     builder.UseDryIoc(services => container.WithDependencyInjectionAdapter(services, throwIfUnresolved:...));
        /// ]]></code>
        /// </summary>
        public static IHostBuilder UseDryIoc(this IHostBuilder builder, Func<IServiceCollection, IContainer> configureContainer = null) => 
            builder.UseServiceProviderFactory(new DryIocServiceProviderFactory(configureContainer));

        /// <summary>The same as <see cref="UseDryIoc"/> but composition root to be used as additional service registration.
        /// It may be any type with <see cref="IRegistrator"/> or <see cref="IContainer"/> injected into its constructor,
        /// and used for registering application services:
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
        /// </summary>
        public static IHostBuilder UseDryIoc<TCompositionRoot>(this IHostBuilder builder, Func<IServiceCollection, IContainer> configureContainer = null) =>
            builder.UseServiceProviderFactory(new DryIocServiceProviderFactory(configureContainer, typeof(TCompositionRoot)));
    }

    /// <summary>Service provider via DryIoc container.</summary>
    public class DryIocServiceProviderFactory : IServiceProviderFactory<IContainer>
    {
        private readonly Func<IServiceCollection, IContainer> _configureContainer;
        private readonly Type _compositionRootType;

        /// <summary>Constructs the factory with optional pre-configured container via <paramref name="configureContainer"/>.</summary>
        public DryIocServiceProviderFactory(Func<IServiceCollection, IContainer> configureContainer = null, Type compositionRootType = null)
        {
            _configureContainer = configureContainer;
            _compositionRootType = compositionRootType;
        }

        /// <summary>Creates **new** container configured for Microsoft.Extensions.DependencyInjection
        /// with user provided configuration or by default.</summary>
        public IContainer CreateBuilder(IServiceCollection services) => 
            _configureContainer?.Invoke(services) ?? DryIocAdapter.Create(services);

        /// <summary>Resolves service provider from configured DryIoc container.</summary>
        public IServiceProvider CreateServiceProvider(IContainer containerBuilder)
        {
            // todo: Replace with `WithCompositionRoot` and `BuildServiceProvider` when it available
            if (_compositionRootType != null)
            {
                containerBuilder.Register(_compositionRootType);
                containerBuilder.Resolve(_compositionRootType);
            }
            return containerBuilder.GetServiceProvider();
        }
    }
}
