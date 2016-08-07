using System;
using System.Collections.Generic;
using Autofac.Core;
using Autofac.Features.OwnedInstances;
using DryIoc;

namespace Autofac
{
    public class ContainerBuilder
    {
        public static Rules WithDefaultAutofacRules(Rules rules)
        {
            return rules
                .With(FactoryMethod.ConstructorWithResolvableArguments)
                .WithTrackingDisposableTransients()
                .WithFactorySelector(Rules.SelectLastRegisteredFactory())
                .WithUnknownServiceResolvers(ThrowDependencyResolutionException);
        }

        public static Factory ThrowDependencyResolutionException(Request request)
        {
            if (!request.IsResolutionRoot)
                throw new DependencyResolutionException($"Unable to resolve: {request}");
            return null;
        }

        public readonly IContainer Container;

        public readonly List<RegistrationInfo> Registrations;

        public ContainerBuilder() : this(new Container(WithDefaultAutofacRules)) {}

        public ContainerBuilder(IContainer container)
        {
            Container = container;
            Registrations = new List<RegistrationInfo>();

            Container.Register(typeof(Owned<>),
                setup: Setup.WrapperWith(openResolutionScope: true, preventDisposal: true));
        }

        public IContainer Build()
        {
            return Build(true);
        }

        private IContainer Build(bool withModules)
        {
            if (Registrations.Count == 0)
                return Container;

            foreach (var r in Registrations)
            {
                if (r.Factory != null)
                    Container.RegisterDelegate(r.ServiceType, r.Factory, r.Reuse);
                else if (r.Instance != null)
                    Container.RegisterInstance(r.ServiceType, r.Instance);
                else
                    Container.Register(r.ServiceType, r.ImplementationType, r.Reuse);
            }

            if (withModules)
            {
                var modules = Container.Resolve<IModule[]>();
                foreach (var module in modules)
                {
                    var moduleBuilder = new ContainerBuilder(Container);
                    module.Configure(moduleBuilder);
                    moduleBuilder.Build(false);
                }
            }

            return Container;
        }
    }

    public static class RegistrationExtensions
    {
        public static RegistrationInfo RegisterType<TImplementation>(this ContainerBuilder builder)
        {
            return builder.Add(typeof(TImplementation), typeof(TImplementation));
        }

        public static RegistrationInfo RegisterGeneric(this ContainerBuilder builder, Type genericTypeDefinition)
        {
            return builder.Add(genericTypeDefinition, genericTypeDefinition);
        }

        public static RegistrationInfo Register<TService>(
            this ContainerBuilder builder, Func<IResolver, TService> factory)
        {
            return builder.Add(typeof(TService), factory: resolver => factory(resolver));
        }

        public static RegistrationInfo RegisterInstance<T>(this ContainerBuilder builder, T instance)
            where T : class
        {
            return builder.Add(typeof(T), instance: instance);
        }


        public static void RegisterModule<TModule>(this ContainerBuilder builder) where TModule : IModule
        {
            builder.RegisterType<TModule>().As<IModule>().SingleInstance();
        }

        public static void RegisterModule(this ContainerBuilder builder, IModule module)
        {
            builder.RegisterInstance(module);
        }

        private static RegistrationInfo Add(this ContainerBuilder builder,
            Type serviceType,
            Type implementationType = null,
            Func<IResolver, object> factory = null,
            object instance = null)
        {
            var info = new RegistrationInfo
            {
                ServiceType = serviceType,
                ImplementationType = implementationType,
                Factory = factory,
                Instance = instance
            };
            builder.Registrations.Add(info);
            return info;
        }
    }

    public class RegistrationInfo
    {
        public Type ServiceType;
        public IReuse Reuse;

        // one of all
        public Type ImplementationType;
        public Func<IResolver, object> Factory;
        public object Instance;
    }

    public static class RegistrationBuilderExtensions
    {
        private static RegistrationInfo WithReuse(this RegistrationInfo info, IReuse reuse)
        {
            info.Reuse = reuse;
            return info;
        }

        public static RegistrationInfo SingleInstance(this RegistrationInfo info)
        {
            return info.WithReuse(Reuse.Singleton);
        }

        public static RegistrationInfo InstancePerOwned<TService>(this RegistrationInfo info)
        {
            return info.WithReuse(Reuse.InResolutionScopeOf<Owned<TService>>());
        }

        public static RegistrationInfo InstancePerMatchingLifetimeScope(this RegistrationInfo info, object scopeName)
        {
            return info.WithReuse(Reuse.InCurrentNamedScope(scopeName));
        }

        public static RegistrationInfo As<TService>(this RegistrationInfo info)
        {
            Throw.IfImplementationIsNotAssignableToService(info.ImplementationType, typeof(TService));
            info.ServiceType = typeof(TService);
            return info;
        }
    }

    public abstract class Module : IModule
    {
        protected abstract void Load(ContainerBuilder moduleBuilder);

        public void Configure(ContainerBuilder moduleBuilder)
        {
            Load(moduleBuilder);
        }
    }

    public static class Throw
    {
        public static void IfImplementationIsNotAssignableToService(Type implementationType, Type serviceType)
        {
            serviceType.ThrowIfNotImplementedBy(implementationType);
        }
    }
}

namespace Autofac.Core
{
    public interface IModule
    {
        void Configure(ContainerBuilder moduleBuilder);
    }

    public class DependencyResolutionException : Exception
    {
        public DependencyResolutionException(string message) : base(message) { }
    }
}

namespace Autofac.Features.OwnedInstances
{
    public class Owned<T> : IDisposable
    {
        public T Value { get; private set; }
        private readonly IDisposable _scope;

        public Owned(T value, IDisposable scope)
        {
            _scope = scope;
            Value = value;
        }

        public void Dispose()
        {
            if (_scope != null)
                _scope.Dispose();
            var disposable = Value as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }
    }
}

namespace Autofac.Core.Registration
{
    public class ComponentNotRegisteredException : DependencyResolutionException
    {
        public ComponentNotRegisteredException(string message) : base(message) {}
    }
}
