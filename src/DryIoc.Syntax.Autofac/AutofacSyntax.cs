/*
The MIT License (MIT)

Copyright (c) 2013-2023 Maksim Volkau
Copyright (c) Autofac Project. All rights reserved.

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
using System.Reflection;
using Autofac.Core;
using Autofac.Features.OwnedInstances;
using DryIoc;

namespace Autofac
{
    /// <summary>A container builder</summary>
    public class ContainerBuilder
    {
        /// <summary>Default conventions to use with Autofac</summary>
        public static Rules WithDefaultAutofacRules(Rules rules) => rules
            .With(FactoryMethod.ConstructorWithResolvableArguments)
            .WithFactorySelector(Rules.SelectLastRegisteredFactory())
            .WithTrackingDisposableTransients()
            .WithUnknownServiceResolvers(ThrowDependencyResolutionException);

        /// <summary>Throws an exception</summary>
        public static Factory ThrowDependencyResolutionException(Request request)
        {
            if (!request.IsResolutionRoot && request.IfUnresolved == IfUnresolved.Throw)
                throw new DependencyResolutionException($"Unable to resolve: {request}");
            return null;
        }

        /// <summary>Container</summary>
        public readonly IContainer Container;

        /// <summary>List of registrations</summary>
        public readonly List<RegistrationInfo> Registrations;

        /// <summary>Constructs a builder</summary>
        public ContainerBuilder() : this(new Container(WithDefaultAutofacRules)) { }

        /// <summary>Constructs a real builder using the container</summary>
        public ContainerBuilder(IContainer container)
        {
            Container = container;
            Registrations = new List<RegistrationInfo>();

            Container.Register(typeof(Owned<>),
                setup: Setup.WrapperWith(openResolutionScope: true, preventDisposal: true));
        }

        /// <summary>Builds a container adapter</summary>
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
                {
                    var made = Made.Default;
                    if (r.Parameters != null)
                    {
                        var ps = Parameters.Of;
                        foreach (var p in r.Parameters)
                        {
                            switch (p)
                            {
                                case NamedParameter named:
                                    ps = ps.Details((_, x) => x.Name.Equals(named.Name) ? ServiceDetails.Of(named.Value) : null);
                                    break;

                                case TypedParameter typed:
                                    ps = ps.Details((_, x) => x.ParameterType.IsAssignableFrom(typed.Type) ? ServiceDetails.Of(typed.Value) : null);
                                    break;

                                case ResolvedParameter _:
                                default:
                                    ps = ps.Details((ctx, x) =>
                                        p.CanSupplyValue(x, new ContainerAdapter(ctx.Container), out var fac)
                                            ? ServiceDetails.Of(fac())
                                            : null);
                                    break;
                            }
                        }
                        made = ps;
                    }

                    Container.Register(r.ServiceType, r.ImplementationType, r.Reuse, made);
                }
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

    /// <summary>A lifetime scope</summary>
    public interface ILifetimeScope
    {
        /// <summary>Container</summary>
        IResolverContext Container { get; }
    }

    /// <summary>Registration API</summary>
    public static class RegistrationExtensions
    {
        /// <summary>Registers a type</summary>
        public static RegistrationInfo RegisterType<TImplementation>(this ContainerBuilder builder) =>
            builder.Add(typeof(TImplementation), typeof(TImplementation));

        /// <summary>Registers a generic type definition</summary>
        public static RegistrationInfo RegisterGeneric(this ContainerBuilder builder, Type genericTypeDefinition) =>
            builder.Add(genericTypeDefinition, genericTypeDefinition);

        /// <summary>Registers a type</summary>
        public static RegistrationInfo Register<TService>(
            this ContainerBuilder builder, Func<IResolver, TService> factory) =>
            builder.Add(typeof(TService), factory: resolver => factory(resolver));

        /// <summary>Registers an instance</summary>
        public static RegistrationInfo RegisterInstance<T>(this ContainerBuilder builder, T instance) where T : class =>
            builder.Add(typeof(T), instance: instance);

        /// <summary>Registers a module</summary>
        public static void RegisterModule<TModule>(this ContainerBuilder builder) where TModule : IModule =>
            builder.RegisterType<TModule>().As<IModule>().SingleInstance();

        /// <summary>Registers a module</summary>
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
                ImplementationType = implementationType ?? serviceType,
                Factory = factory,
                Instance = instance
            };

            builder.Registrations.Add(info);
            return info;
        }
    }

    /// <summary>Just a IResolverContext holder</summary>
    public interface IComponentContext
    {
        /// <summary>Autofac Container is similar to the DryIoc IResolverContext - something to resolve from or creates scopes with</summary>
        IContainer Container { get; }
    }

    /// <summary>Container adapter</summary>
    public class ContainerAdapter : IComponentContext, ILifetimeScope, IDisposable
    {
        /// <inheritdoc />
        public IContainer Container { get; }

        IResolverContext ILifetimeScope.Container => Container;

        /// <summary>Constructs an adapter</summary>
        public ContainerAdapter(IContainer container) => Container = container;

        /// <inheritdoc />
        public void Dispose() => Container?.Dispose();
    }

    /// <summary>Resolution helpers</summary>
    public static class ContainerExtensions
    {
        /// <summary>Actual Resolve method</summary>
        public static T Resolve<T>(this IComponentContext context) =>
            context.Container.Resolve<T>();

        /// <summary>Resolves the service</summary>
        public static object ResolveService(this IComponentContext context, KeyedService ks) =>
            context.Container.Resolve(ks.ServiceType, ks.ServiceKey);

        /// <summary>Check if service is registered</summary>
        public static bool IsRegisteredService(this IComponentContext context, KeyedService ks) =>
            context.Container.IsRegistered(ks.ServiceType, ks.ServiceKey);

        /// <summary>Opens a scope</summary>
        public static ContainerAdapter BeginLifetimeScope(this ILifetimeScope scope) =>
            new ContainerAdapter((IContainer)scope.Container.OpenScope());
    }

    /// <summary>Dto to hold a registration info</summary>
    public class RegistrationInfo
    {
        /// <summary>Service type</summary>
        public Type ServiceType { get; internal set; }

        /// <summary>Impl. type</summary>
        public Type ImplementationType { get; internal set; }

        /// <summary>Factory delegate</summary>
        public Func<IResolver, object> Factory { get; internal set; }

        /// <summary>Instance</summary>
        public object Instance { get; internal set; }

        /// <summary>Reuse</summary>
        public IReuse Reuse { get; internal set; }

        /// <summary>Parameter specifications</summary>
        public List<Parameter> Parameters { get; internal set; }
    }

    /// <summary>Registration builder</summary>
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

        /// <summary>Reuse.ScopedTo(name)</summary>
        public static RegistrationInfo InstancePerMatchingLifetimeScope(this RegistrationInfo info, object scopeName) => info.WithReuse(Reuse.ScopedTo(scopeName));

        /// <summary>Specifies the service type</summary>
        public static RegistrationInfo As(this RegistrationInfo info, Type serviceType)
        {
            Throw.IfImplementationIsNotAssignableToService(info.ImplementationType, serviceType);
            info.ServiceType = serviceType;
            return info;
        }

        /// <summary>Specifies the service type</summary>
        public static RegistrationInfo As<TService>(this RegistrationInfo info) => info.As(typeof(TService));

        /// <summary>Specifies the service type</summary>
        public static RegistrationInfo AsSelf(this RegistrationInfo info)
        {
            info.ServiceType = info.ImplementationType;
            return info;
        }

        /// <summary>Adds the parameter specification</summary>
        public static RegistrationInfo WithParameter(this RegistrationInfo info, Parameter parameter)
        {
            if (info.Parameters == null)
                info.Parameters = new List<Parameter>();
            info.Parameters.Add(parameter);
            return info;
        }

        /// <summary>With the named parameter</summary>
        public static RegistrationInfo WithParameter<T>(this RegistrationInfo info, string name, T value) =>
            info.WithParameter(new NamedParameter(name, value));

        /// <summary>With the typed parameter</summary>
        public static RegistrationInfo WithParameter<T>(this RegistrationInfo info, Type type, T value) =>
            info.WithParameter(new TypedParameter(type, value));

        /// <summary>With the resolved parameter</summary>
        public static RegistrationInfo WithParameter(this RegistrationInfo info,
            Func<ParameterInfo, IComponentContext, bool> predicate, Func<ParameterInfo, IComponentContext, object> valueAccessor) =>
            info.WithParameter(new ResolvedParameter(predicate, valueAccessor));
    }

    /// <summary>Module</summary>
    public abstract class Module : IModule
    {
        /// <summary>Load builder</summary>
        protected abstract void Load(ContainerBuilder moduleBuilder);

        /// <summary>Configures builder</summary>
        public void Configure(ContainerBuilder moduleBuilder) => Load(moduleBuilder);
    }

    /// <summary>Throws</summary>
    public static class Throw
    {
        /// <summary>Some checks</summary>
        public static void IfImplementationIsNotAssignableToService(Type implementationType, Type serviceType) =>
            serviceType.ThrowIfNotImplementedBy(implementationType);
    }
}

namespace Autofac.Core
{
    /// <summary>Module</summary>
    public interface IModule
    {
        /// <summary>Configure</summary>
        void Configure(ContainerBuilder moduleBuilder);
    }

    /// <summary>Exception</summary>
    public class DependencyResolutionException : Exception
    {
        /// <summary>Creates a thingy</summary>
        public DependencyResolutionException(string message) : base(message) { }
    }

    /// <summary>Interface supported by services that carry type information.</summary>
    public interface IServiceWithType
    {
        /// <summary>Gets the type of the service.</summary>
        Type ServiceType { get; }

        /// <summary>Return a new service of the same kind, but carrying</summary>
        Service ChangeType(Type newType);
    }

    /// <summary>Services are the lookup keys used to locate component instances.</summary>
    public abstract class Service
    {
        /// <summary>Gets a human-readable description of the service.</summary>
        public abstract string Description { get; }

        /// <inheritdoc />
        public override string ToString() => Description;
    }

    /// <summary>Identifies a service using a key in addition to its type.</summary>
    public sealed class KeyedService : Service, IServiceWithType, IEquatable<KeyedService>
    {
        /// <summary>Initializes a new instance of the class.</summary>
        public KeyedService(object serviceKey, Type serviceType)
        {
            ServiceKey = serviceKey ?? throw new ArgumentNullException(nameof(serviceKey));
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
        }

        /// <summary>Gets the key of the service.</summary>
        public object ServiceKey { get; }

        /// <summary>Gets the type of the service.</summary>
        public Type ServiceType { get; }

        /// <summary>Gets a human-readable description of the service.</summary>
        public override string Description => ServiceKey + " (" + ServiceType.FullName + ")";

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        public bool Equals(KeyedService other) =>
             other != null && ServiceKey.Equals(other.ServiceKey) && ServiceType == other.ServiceType;

        /// <inheritdoc />
        public override bool Equals(object obj) => Equals(obj as KeyedService);

        /// <inheritdoc />
        public override int GetHashCode() => ServiceKey.GetHashCode() ^ ServiceType.GetHashCode();

        /// <summary>Return a new service of the same kind but carrying <paramref name="newType"/> as the <see cref="ServiceType"/>.</summary>
        public Service ChangeType(Type newType) => new KeyedService(ServiceKey, newType);
    }

    /// <summary>Used in order to provide a value to a constructor parameter or property on an instance being created by the container.</summary>
    public abstract class Parameter
    {
        /// <summary>Returns true if the parameter is able to provide a value to a particular site.</summary>
        public abstract bool CanSupplyValue(ParameterInfo pi, IComponentContext context, out Func<object> valueProvider);
    }

    /// <summary>Base class for parameters that provide a constant value.</summary>
    public abstract class ConstantParameter : Parameter
    {
        private readonly Predicate<ParameterInfo> _predicate;

        /// <summary>Gets the value of the parameter.</summary>
        public object Value { get; }

        /// <summary>Initializes a new instance of the class</summary>
        protected ConstantParameter(object value, Predicate<ParameterInfo> predicate)
        {
            Value = value;
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        /// <inheritdoc />
        public override bool CanSupplyValue(ParameterInfo pi, IComponentContext context, out Func<object> valueProvider)
        {
            valueProvider = _predicate(pi) ? (Func<object>)(() => Value) : null;
            return valueProvider != null;
        }
    }

    /// <summary>Named parameter specification</summary>
    public class NamedParameter : ConstantParameter
    {
        /// <summary>The name of the parameter.</summary>
        public string Name { get; }

        /// <summary>Creates the named parameter specification</summary>
        public NamedParameter(string name, object value) : base(value, pi => pi.Name == name) =>
            Name = !string.IsNullOrEmpty(name) ? name : throw new ArgumentException("Name is null or empty", nameof(name));
    }

    /// <summary>Typed parameter specification</summary>
    public class TypedParameter : ConstantParameter
    {
        /// <summary>Target parameter type</summary>
        public Type Type { get; private set; }

        /// <summary>Creates the specification</summary>
        public TypedParameter(Type type, object value = null) : base(value, pi => pi.ParameterType.IsAssignableFrom(type)) =>
            Type = type ?? throw new ArgumentNullException(nameof(type));

        /// <summary>Helper factory method</summary>
        public static TypedParameter From<T>(T value) => new TypedParameter(typeof(T), value);
    }

    /// <summary>Flexible parameter type allows arbitrary values to be retrieved from the resolution context.</summary>
    public class ResolvedParameter : Parameter
    {
        private readonly Func<ParameterInfo, IComponentContext, bool> _predicate;
        private readonly Func<ParameterInfo, IComponentContext, object> _valueAccessor;

        /// <summary>Initializes a new instance of the class.</summary>
        public ResolvedParameter(Func<ParameterInfo, IComponentContext, bool> predicate, Func<ParameterInfo, IComponentContext, object> valueAccessor)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
            _valueAccessor = valueAccessor ?? throw new ArgumentNullException(nameof(valueAccessor));
        }

        /// <inheritdoc />
        public override bool CanSupplyValue(ParameterInfo pi, IComponentContext context, out Func<object> valueProvider)
        {
            valueProvider = _predicate(pi, context) ? (Func<object>)(() => _valueAccessor(pi, context)) : null;
            return valueProvider != null;
        }

        ///<summary>Factory method</summary>
        public static ResolvedParameter ForNamed<TService>(string serviceName) =>
            ForKeyed<TService>(serviceName);

        ///<summary>Factory method</summary>
        public static ResolvedParameter ForKeyed<TService>(object serviceKey)
        {
            var ks = new KeyedService(serviceKey, typeof(TService));
            return new ResolvedParameter(
                (pi, c) => pi.ParameterType == typeof(TService) && c.IsRegisteredService(ks),
                (pi, c) => c.ResolveService(ks));
        }
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
        public ComponentNotRegisteredException(string message) : base(message) { }
    }
}
