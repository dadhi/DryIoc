/*
The MIT License (MIT)

Copyright (c) 2013-2018 Maksim Volkau

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
using Autofac.Core;
using Autofac.Features.OwnedInstances;
using DryIoc;

namespace Autofac
{
    /// Oops, a container builder
    public class ContainerBuilder
    {
        /// Default conventions to use with Autofac
        public static Rules WithDefaultAutofacRules(Rules rules) => rules
            .With(FactoryMethod.ConstructorWithResolvableArguments)
            .WithFactorySelector(Rules.SelectLastRegisteredFactory())
            .WithTrackingDisposableTransients()
            .WithUnknownServiceResolvers(ThrowDependencyResolutionException);

        /// Throws an exception
        public static Factory ThrowDependencyResolutionException(Request request)
        {
            if (!request.IsResolutionRoot && request.IfUnresolved == IfUnresolved.Throw)
                throw new DependencyResolutionException($"Unable to resolve: {request}");
            return null;
        }

        /// Container
        public readonly IContainer Container;

        /// List of registrations
        public readonly List<RegistrationInfo> Registrations;

        /// Constructs a builder
        public ContainerBuilder() : this(new Container(WithDefaultAutofacRules)) {}

        /// Constructs a real builder using the container
        public ContainerBuilder(IContainer container)
        {
            Container = container;
            Registrations = new List<RegistrationInfo>();

            Container.Register(typeof(Owned<>),
                setup: Setup.WrapperWith(openResolutionScope: true, preventDisposal: true));
        }

        /// Builds a container adapter
        public ContainerAdapter Build() => Build(true);

        private ContainerAdapter Build(bool withModules)
        {
            if (Registrations.Count == 0)
                return new ContainerAdapter(Container);

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

            return new ContainerAdapter(Container);
        }
    }

    /// Oops, a lifetime scope
    public interface ILifetimeScope
    {
        /// Container
        IResolverContext Container { get; }
    }

    /// A lot of stuff
    public static class RegistrationExtensions
    {
        /// Registers a type
        public static RegistrationInfo RegisterType<TImplementation>(this ContainerBuilder builder) => 
            builder.Add(typeof(TImplementation), typeof(TImplementation));

        /// Registers a generic type definition
        public static RegistrationInfo RegisterGeneric(this ContainerBuilder builder, Type genericTypeDefinition) => 
            builder.Add(genericTypeDefinition, genericTypeDefinition);

        /// Registers a type
        public static RegistrationInfo Register<TService>(
            this ContainerBuilder builder, Func<IResolver, TService> factory) => 
            builder.Add(typeof(TService), factory: resolver => factory(resolver));

        /// Registers an instance
        public static RegistrationInfo RegisterInstance<T>(this ContainerBuilder builder, T instance) where T : class => 
            builder.Add(typeof(T), instance: instance);

        /// Registers a module
        public static void RegisterModule<TModule>(this ContainerBuilder builder) where TModule : IModule => 
            builder.RegisterType<TModule>().As<IModule>().SingleInstance();

        /// Registers a module
        public static void RegisterModule(this ContainerBuilder builder, IModule module) => 
            builder.RegisterInstance(module);

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

    /// Oops, a component context
    public interface IComponentContext
    {
        /// More containers
        IResolverContext Container { get; }
    }

    /// Container adapter
    public class ContainerAdapter : IComponentContext, ILifetimeScope, IDisposable
    {
        /// A container
        public IResolverContext Container { get; }

        /// Constructs an adapter
        public ContainerAdapter(IResolverContext container)
        {
            Container = container;
        }

        /// Disposes a container
        public void Dispose() => Container?.Dispose();
    }

    /// Oops, resolution stuff
    public static class ContainerExtensions
    {
        /// Actual Resolve method
        public static T Resolve<T>(this IComponentContext context) => 
            context.Container.Resolve<T>();

        /// Opens a scope
        public static ContainerAdapter BeginLifetimeScope(this ILifetimeScope scope) => 
            new ContainerAdapter(scope.Container.OpenScope());
    }

    /// Dto to hold a registration info
    public class RegistrationInfo
    {
        /// Service type
        public Type ServiceType;

        /// Reuse
        public IReuse Reuse;

        /// Impl. type
        public Type ImplementationType;

        /// Delegate
        public Func<IResolver, object> Factory;

        /// Instance
        public object Instance;
    }

    /// Registration builder
    public static class RegistrationBuilderExtensions
    {
        private static RegistrationInfo WithReuse(this RegistrationInfo info, IReuse reuse)
        {
            info.Reuse = reuse;
            return info;
        }

        /// Reuse.Singleton
        public static RegistrationInfo SingleInstance(this RegistrationInfo info) => info.WithReuse(Reuse.Singleton);

        /// Reuse.ScopedTo{TService}()
        public static RegistrationInfo InstancePerOwned<TService>(this RegistrationInfo info) => info.WithReuse(Reuse.ScopedTo<Owned<TService>>());

        /// Reuse.ScopeTo(name)
        public static RegistrationInfo InstancePerMatchingLifetimeScope(this RegistrationInfo info, object scopeName) => info.WithReuse(Reuse.ScopedTo(scopeName));

        /// Service type
        public static RegistrationInfo As<TService>(this RegistrationInfo info)
        {
            Throw.IfImplementationIsNotAssignableToService(info.ImplementationType, typeof(TService));
            info.ServiceType = typeof(TService);
            return info;
        }
    }

    /// Module
    public abstract class Module : IModule
    {
        /// Load builder
        protected abstract void Load(ContainerBuilder moduleBuilder);

        /// Configures builder
        public void Configure(ContainerBuilder moduleBuilder) => Load(moduleBuilder);
    }

    /// Throws
    public static class Throw
    {
        /// Some checks
        public static void IfImplementationIsNotAssignableToService(Type implementationType, Type serviceType) => 
            serviceType.ThrowIfNotImplementedBy(implementationType);
    }
}

namespace Autofac.Core
{
    /// Module
    public interface IModule
    {
        /// Configure
        void Configure(ContainerBuilder moduleBuilder);
    }

    /// Exception
    public class DependencyResolutionException : Exception
    {
        /// Creates a thingy
        public DependencyResolutionException(string message) : base(message) { }
    }
}

namespace Autofac.Features.OwnedInstances
{
    /// Controls a Dispose
    public class Owned<T> : IDisposable
    {
        /// Wrapped value
        public T Value { get; }
        private readonly IDisposable _lifetime;

        /// Constructs a wrapper
        public Owned(T value, IResolverContext lifetime)
        {
            _lifetime = lifetime;
            Value = value;
        }

        /// Disposes
        public void Dispose()
        {
            _lifetime?.Dispose();
            (Value as IDisposable)?.Dispose();
        }
    }
}

namespace Autofac.Core.Registration
{
    /// More exceptions
    public class ComponentNotRegisteredException : DependencyResolutionException
    {
        /// Constructor
        public ComponentNotRegisteredException(string message) : base(message) {}
    }
}
