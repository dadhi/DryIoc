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
using Autofac.Core;
using Autofac.Features.OwnedInstances;
using DryIoc;

namespace Autofac
{
    public class ContainerBuilder
    {
        public static Rules WithDefaultAutofacRules(Rules rules) => rules
            .With(FactoryMethod.ConstructorWithResolvableArguments)
            .WithFactorySelector(Rules.SelectLastRegisteredFactory())
            .WithTrackingDisposableTransients()
            .WithUnknownServiceResolvers(ThrowDependencyResolutionException);

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

        public ContainerAdapter Build()
        {
            return Build(true);
        }

        private ContainerAdapter Build(bool withModules)
        {
            if (Registrations.Count == 0)
                return new ContainerAdapter(Container);

            foreach (var r in Registrations)
            {
                if (r.Factory != null)
                    Container.RegisterDelegate(r.ServiceType, r.Factory, r.Reuse);
                else if (r.Instance != null)
                    Container.UseInstance(r.ServiceType, r.Instance);
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

    public interface ILifetimeScope
    {
        IResolverContext Container { get; }
    }

    public static class RegistrationExtensions
    {
        public static RegistrationInfo RegisterType<TImplementation>(this ContainerBuilder builder) => 
            builder.Add(typeof(TImplementation), typeof(TImplementation));

        public static RegistrationInfo RegisterGeneric(this ContainerBuilder builder, Type genericTypeDefinition) => 
            builder.Add(genericTypeDefinition, genericTypeDefinition);

        public static RegistrationInfo Register<TService>(
            this ContainerBuilder builder, Func<IResolver, TService> factory) => 
            builder.Add(typeof(TService), factory: resolver => factory(resolver));

        public static RegistrationInfo RegisterInstance<T>(this ContainerBuilder builder, T instance) where T : class => 
            builder.Add(typeof(T), instance: instance);

        public static void RegisterModule<TModule>(this ContainerBuilder builder) where TModule : IModule => 
            builder.RegisterType<TModule>().As<IModule>().SingleInstance();

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

    public interface IComponentContext
    {
        IResolverContext Container { get; }
    }

    public class ContainerAdapter : IComponentContext, ILifetimeScope, IDisposable
    {
        public IResolverContext Container { get; }

        public ContainerAdapter(IResolverContext container)
        {
            Container = container;
        }

        public void Dispose() => Container?.Dispose();
    }

    public static class ContainerExtensions
    {
        public static T Resolve<T>(this IComponentContext context) => 
            context.Container.Resolve<T>();

        public static ContainerAdapter BeginLifetimeScope(this ILifetimeScope scope) => 
            new ContainerAdapter(scope.Container.OpenScope());
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
            return info.WithReuse(Reuse.ScopedTo<Owned<TService>>());
        }

        public static RegistrationInfo InstancePerMatchingLifetimeScope(this RegistrationInfo info, object scopeName)
        {
            return info.WithReuse(Reuse.ScopedTo(scopeName));
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
        public T Value { get; }
        private readonly IDisposable _lifetime;

        public Owned(T value, IResolverContext lifetime)
        {
            _lifetime = lifetime;
            Value = value;
        }

        public void Dispose()
        {
            _lifetime?.Dispose();
            (Value as IDisposable)?.Dispose();
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
