using System;
using System.Collections.Generic;

namespace DryIoc.Syntax.Autofac
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

        public readonly List<RegistrationBuilder> Registrations;

        public readonly List<Type> ModuleTypes;

        public ContainerBuilder() : this(new Container(WithDefaultAutofacRules)) { }

        public ContainerBuilder(IContainer container)
        {
            Container = container;
            Registrations = new List<RegistrationBuilder>();
            ModuleTypes = new List<Type>();

            Container.Register(typeof(Owned<>),
                setup: Setup.WrapperWith(openResolutionScope: true, preventDisposal: true));
        }

        public RegistrationBuilder RegisterType<TImplementation>()
        {
            var builder = new RegistrationBuilder
            {
                ServiceType = typeof(TImplementation),
                ImplementationType = typeof(TImplementation)
            };
            Registrations.Add(builder);
            return builder;
        }

        public RegistrationBuilder RegisterGeneric(Type genericTypeDefinition)
        {
            var builder = new RegistrationBuilder
            {
                ServiceType = genericTypeDefinition,
                ImplementationType = genericTypeDefinition
            };
            Registrations.Add(builder);
            return builder;
        }

        public RegistrationBuilder Register<TService>(Func<IResolver, TService> factoryDelegate)
        {
            var builder = new RegistrationBuilder
            {
                ServiceType = typeof(TService),
                FactoryDelegate = resolver => factoryDelegate(resolver)
            };
            Registrations.Add(builder);
            return builder;
        }

        public void RegisterModule<TModule>() where TModule : Module
        {
            ModuleTypes.Add(typeof(TModule));
        }

        public IContainer Build()
        {
            if (Registrations.Count != 0)
                foreach (var r in Registrations)
                {
                    if (r.FactoryDelegate != null)
                        Container.RegisterDelegate(r.ServiceType, r.FactoryDelegate, r.Reuse);
                    else
                        Container.Register(r.ServiceType, r.ImplementationType, r.Reuse);
                }

            if (ModuleTypes.Count != 0)
            {
                foreach (var moduleType in ModuleTypes)
                    Container.Register(typeof(IModule), moduleType, Reuse.Singleton);

                var modules = Container.Resolve<IModule[]>();
                foreach (var module in modules)
                {
                    var moduleBuilder = new ContainerBuilder(Container);
                    module.Configure(moduleBuilder);
                    moduleBuilder.Build();
                }
            }

            return Container;
        }
    }

    public class RegistrationBuilder
    {
        public Type ServiceType;
        public IReuse Reuse;

        public Type ImplementationType;
        public Func<IResolver, object> FactoryDelegate;
    }

    public static class RegistrationBuilderExtensions
    {
        private static RegistrationBuilder WithReuse(this RegistrationBuilder builder, IReuse reuse)
        {
            builder.Reuse = reuse;
            return builder;
        }


        public static RegistrationBuilder SingleInstance(this RegistrationBuilder builder)
        {
            return builder.WithReuse(Reuse.Singleton);
        }

        public static RegistrationBuilder InstancePerOwned<TService>(this RegistrationBuilder builder)
        {
            return builder.WithReuse(Reuse.InResolutionScopeOf<Owned<TService>>());
        }

        public static RegistrationBuilder InstancePerMatchingLifetimeScope(this RegistrationBuilder builder, object scopeName)
        {
            return builder.WithReuse(Reuse.InCurrentNamedScope(scopeName));
        }

        public static RegistrationBuilder As<TService>(this RegistrationBuilder builder)
        {
            Throw.IfImplementationIsNotAssignableToService(builder.ImplementationType, typeof(TService));
            builder.ServiceType = typeof(TService);
            return builder;
        }
    }

    public interface IModule
    {
        void Configure(ContainerBuilder moduleBuilder);
    }

    public abstract class Module : IModule
    {
        protected abstract void Load(ContainerBuilder moduleBuilder);

        public void Configure(ContainerBuilder moduleBuilder)
        {
            Load(moduleBuilder);
        }
    }

    public class Owned<T> : IDisposable
    {
        public T Value { get; private set; }

        public Owned(T value)
        {
            Value = value;
        }

        public void Dispose()
        {
            var disposable = Value as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }
    }

    public static class Throw
    {
        public static void IfImplementationIsNotAssignableToService(Type implementationType, Type serviceType)
        {
            serviceType.ThrowIfNotImplementedBy(implementationType);
        }
    }

    public class DependencyResolutionException : Exception
    {
        public DependencyResolutionException(string message) : base(message) { }
    }
}
