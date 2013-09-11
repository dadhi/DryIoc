// TODO:
// For version 1.0.0
// - Consolidate code related to rules in Setup class.
// - Adjust ResolveProperties to be consistent with Import property or field.
// - Add condition to Decorator.
// - Convert SkipCache flag to enum.
// - Evaluate Code Coverage.
// + Rename FunIoc to more stand out DryIoc.
// + Add CustomReuse (reuse in custom scope?) example test, e.g. for ThreadStatic singleton.
// + Rename Singletons.

// Goals:
// - Finalize Public API.
// + Add distinctive features:. ImportUsing.
//
// Features:
// - Add distinctive features: Export CustomFactory<TService>.
// - Make Request to return Empty for resolution root parent. So it will simplify ImplementationType checks. May be add IsResolutionRoot property as well.
// - Make a single consistent approach to ResolveProperties and PropertyOrFieldResolutionRules.
// - Move Container Setup related code to dedicated class/container-property Setup.
// - Decorator support for Func<..> service, may be supported if implement Decorator the same way as Reuse or Init - as Expression Decorator.
//
// Internals:
// - Rename request to DependencyChain.
// - Remove Factory.PrototypeID.
// - Change IReuse signature to use ContainerScope and CurrentScope instead of IRegistry.
// - Remove Container Singleton parameter from CompiledFactory.
// - Add service Target (ctor parameter or fieldProperty) to request to show them in error: "Unable to resolve constructor parameter/field/property service".
// - Rename Request.TryGetNonWrapperParent to just Parent and make it to return default value.
// - Make Decorator caching work without SkipCache=true;
// - Remove non required custom delegates, the ones easy to figure out with good parameters.

#define SYSTEM_LAZY_IS_NOT_AVAILABLE
#pragma warning disable 420 // a reference to a volatile field will not be treated as volatile
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DryIoc.UnitTests")]

namespace DryIoc
{
    /// <summary>
    /// <para>
    /// Supports registration of:
    /// <list type="bullet">
    ///	<item>Transient or Singleton instance reuse (Transient means - not reuse, so it just a null). 
    /// Custom reuse policy is possible via specifying your own <see cref="IReuse"/> implementation.</item>
    /// <item>Arbitrary lambda factory to return service.</item>
    ///	<item>Optional service name.</item>
    ///	<item>Open generics are registered the same way as concrete types.</item>
    ///	<item>User-defined service metadata.</item>
    /// <item>Check if service is registered via <see cref="Registrator.IsRegistered"/>.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Supports resolution of:
    /// <list type="bullet">
    /// <item>Service instance.</item>
    /// <item>Automatic constructor parameters injection.</item>
    /// <item>User-defined service constructor selection, Throws if not defined.</item>
    ///	<item>Func&lt;TService&gt; - will create Transient TService each time when invoked but Singleton only once on first invoke.</item>
    ///	<item>Lazy&lt;TService&gt; - will create instance only once on first access to Value property.</item>
    /// <item>Func&lt;..., TService&gt; - Func with parameters. Parameters identified by Type, not by name. Order of parameters does not matter.</item>
    /// <item>Meta&lt;TService, TMetadata&gt; - service wrapped in Meta with registered TMetadata. TService could be a registered type or Func, Lazy variations.</item>
    /// <item>IEnumerable&lt;TService&gt; and TService[] - TService could be a registered type or Func, Lazy, Meta variations.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Additional features:
    /// <list type="bullet">
    /// <item>Bare-bone mode with all default rules switched off via Container constructor parameter.</item>
    /// <item>Control of service resolution via <see cref="Setup"/>.</item>
    /// </list>
    /// </para>
    /// </summary>
    public class Container : IRegistry, IDisposable
    {
        public readonly Setup Setup;

        public Container(bool coreOnly = false)
        {
            _syncRoot = new object();
            _registry = new Dictionary<Type, RegistryEntry>();
            _keyedResolutionCache = HashTree<Type, KeyedResolutionCacheEntry>.Empty;
            _defaultResolutionCache = HashTree<Type, CompiledFactory>.Empty;
            _genericWrappers = HashTree<Type, GenericWrapperEntry>.Empty;

            Setup = new Setup();
            CurrentScope = SingletonScope = new Scope();

            if (!coreOnly) // Default setup, May I just pass custom setup instead flag?
            {
                Setup.AddNonRegisteredServiceResolutionRule(TryResolveEnumerableOrArray);

                foreach (var funcType in FuncTypes)
                    //RegisterGenericWrapper(new CustomFactoryProvider(TryResolveFunc), funcType, t => t[t.Length - 1]);
                    RegisterGenericWrapper(new FuncFactory(), funcType, t => t[t.Length - 1]);

                RegisterGenericWrapper(
                    new ReflectionFactory(typeof(Lazy<>), selectConstructor: t => t.GetConstructor(new[] { typeof(Func<>) })),
                    typeof(Lazy<>), t => t[0]);

                RegisterGenericWrapper(new CustomFactoryProvider(TryResolveMeta), typeof(Meta<,>), t => t[0]);

                RegisterGenericWrapper(new CustomFactoryProvider(TryResolveFactoryExpression), typeof(FactoryExpression<>), t => t[0]);
            }
        }

        public Container OpenScope()
        {
            return new Container(this);
        }

        public TryGetFactory UseRegistrationsFrom(IRegistry anotherRegistry)
        {
            return Setup.AddNonRegisteredServiceResolutionRule((request, _) => anotherRegistry.GetOrAddFactory(request, true));
        }

        public void Dispose()
        {
            CurrentScope.Dispose();
        }

        public static CompiledFactory CompileExpression(Expression expression)
        {
            return Expression.Lambda<CompiledFactory>(expression, Reuse.Parameters).Compile();
        }

        #region IRegistry

        public Scope SingletonScope { get; private set; }

        public Scope CurrentScope { get; private set; }

        Factory IRegistry.GetOrAddFactory(Request request, bool shouldReturnNull)
        {
            Factory result;
            lock (_syncRoot)
            {
                // Decorator.
                RegistryEntry entry;
                Decorator decorator;
                if (_registry.TryGetValue(request.ServiceType, out entry) &&
                    entry.TryGetDecorator(out decorator, request))
                {
                    result = decorator.Factory.TryProvideFactoryFor(request, this) ?? decorator.Factory;
                    request.SetResult(result, FactoryType.Decorator);
                    return result;
                }

                // Open-generic Decorator.
                RegistryEntry openGenericEntry = null;
                if (request.OpenGenericServiceType != null &&
                    _registry.TryGetValue(request.OpenGenericServiceType, out openGenericEntry) &&
                    openGenericEntry.TryGetDecorator(out decorator, request) &&
                    (decorator = decorator.TryProvideDecoratorFor(request, this)) != null)
                {
                    request.SetResult(decorator.Factory, FactoryType.Decorator);

                    RegisterDecorator(decorator, request.ServiceType); // TODO: May be by providing ServiceKey we could stop using SkipCache.
                                                                       // TODO: Do we need to register closed gen decorator at all?
                        
                    return decorator.Factory;
                }

                // Service.
                if (entry != null && 
                    entry.TryGetServiceFactory(out result, request.ServiceType, request.ServiceKey))
                {
                    result = result.TryProvideFactoryFor(request, this) ?? result;
                    request.SetResult(result);
                    return result;
                }

                // Open-generic Service.
                if (openGenericEntry != null &&
                    openGenericEntry.TryGetServiceFactory(out result, request.ServiceType, request.ServiceKey) &&
                    (result = result.TryProvideFactoryFor(request, this)) != null)
                {
                    request.SetResult(result);
                    Register(result, request.ServiceType, request.ServiceKey as string);
                    return result;
                }

                // Open-generic Wrapper.
                GenericWrapperEntry wrapper;
                if (request.OpenGenericServiceType != null &&
                    (wrapper = _genericWrappers.TryGet(request.OpenGenericServiceType)) != null &&
                    (result = wrapper.Factory.TryProvideFactoryFor(request, this)) != null)
                {
                    request.SetResult(result, FactoryType.GenericWrapper);
                    Register(result, request.ServiceType, request.ServiceKey as string);
                    return result;
                }
            }

            var rules = Setup.NonRegisteredServiceResolutionRules;
            for (var i = 0; i < rules.Length; i++)
            {
                result = rules[i].Invoke(request, this);
                if (result != null)
                {
                    request.SetResult(result);
                    Register(result, request.ServiceType, request.ServiceKey as string);
                    return result;
                }
            }

            Throw.If(!shouldReturnNull, Error.UNABLE_TO_RESOLVE_SERVICE, request, request.PrintServiceInfo());
            return null;
        }

        IEnumerable<object> IRegistry.GetKeys(Type serviceType, Func<Factory, bool> condition)
        {
            lock (_syncRoot)
            {
                RegistryEntry entry;
                if (TryFindEntry(out entry, serviceType))
                {
                    if (entry.Defaults != null)
                    {
                        for (var i = 0; i < entry.Defaults.Count; i++)
                            if (condition == null || condition(entry.Defaults[i]))
                                yield return i;
                    }
                    else if (entry.LastDefault != null)
                    {
                        if (condition == null || condition(entry.LastDefault))
                            yield return 0;
                    }

                    if (entry.Named != null)
                    {
                        foreach (var pair in entry.Named)
                            if (condition == null || condition(pair.Value))
                                yield return pair.Key;
                    }
                }
            }
        }

        Factory IRegistry.TryGetFactory(Type serviceType, object serviceKey)
        {
            lock (_syncRoot)
            {
                RegistryEntry entry;
                Factory factory;
                if (TryFindEntry(out entry, serviceType) &&
                    entry.TryGetServiceFactory(out factory, serviceType, serviceKey))
                    return factory;
                return null;
            }
        }

        Type IRegistry.GetWrappedServiceTypeOrSelf(Type serviceType)
        {
            if (!serviceType.IsGenericType)
                return serviceType;

            var entry = _genericWrappers.TryGet(serviceType.GetGenericTypeDefinition());
            if (entry == null)
                return serviceType;

            var wrappedType = entry.GetWrappedServiceType(serviceType.GetGenericArguments());
            return wrappedType == serviceType ? serviceType
                : ((IRegistry)this).GetWrappedServiceTypeOrSelf(wrappedType); // unwrap further.
        }

        object IRegistry.TryGetConstructorParamKey(ParameterInfo parameter, Request parent)
        {
            var rules = Setup.ConstructorParamServiceKeyResolutionRules;
            object resultKey = null;
            for (var i = 0; i < rules.Length && resultKey == null; i++)
                resultKey = rules[i].Invoke(parameter, parent, this);
            return resultKey ?? (parent.FactoryType != FactoryType.Service ? parent.ServiceKey : null);
        }

        bool IRegistry.ShouldResolvePropertyOrField
        {
            get { return Setup.PropertyOrFieldResolutionRules.Length != 0; }
        }

        bool IRegistry.TryGetPropertyOrFieldKey(out object resultKey, MemberInfo propertyOrField, Request parent)
        {
            var rules = Setup.PropertyOrFieldResolutionRules;
            resultKey = null;
            var gotIt = false;
            for (var i = 0; i < rules.Length && !gotIt; i++)
                gotIt = rules[i].Invoke(out resultKey, propertyOrField, parent, this);
            return gotIt;
        }

        private bool TryFindEntry(out RegistryEntry entry, Type serviceType)
        {
            return _registry.TryGetValue(serviceType, out entry) || serviceType.IsGenericType &&
                   _registry.TryGetValue(serviceType.GetGenericTypeDefinition().ThrowIfNull(), out entry);
        }

        private static void ThrowUnexpectedMultipleDefaults(ICollection<Factory> factories, Type serviceType)
        {
            var implementations = factories.Select(x => x.ImplementationType).Aggregate(
                new StringBuilder().AppendLine(),
                (i, t) => i.AppendLine(t != null ? t.Print() + ";" : "[not provided]"));
            Throw.If(true, Error.EXPECTED_SINGLE_DEFAULT_FACTORY, serviceType, factories.Count, implementations);
        }

        #endregion

        #region IRegistrator

        public void Register(Factory factory, Type serviceType, string serviceName)
        {
            ThrowIfServiceTypeIsNotImplementedBy(factory, serviceType);
            lock (_syncRoot)
            {
                var entry = _registry.GetOrAdd(serviceType, _ => new RegistryEntry());
                if (serviceName != null)
                {
                    if (entry.Named == null)
                        entry.Named = new Dictionary<string, Factory>();
                    try
                    {
                        entry.Named.Add(serviceName, factory);
                    }
                    catch (ArgumentException)
                    {
                        var implType = entry.Named[serviceName].ImplementationType;
                        Throw.If(true, Error.DUPLICATE_SERVICE_NAME_REGISTRATION,
                            serviceType, serviceName, implType != null ? implType.Print() : "<custom>");
                    }
                }
                else
                {
                    if (entry.LastDefault != null)
                    {
                        if (entry.Defaults == null)
                            entry.Defaults = new List<Factory> { entry.LastDefault };
                        entry.Defaults.Add(factory);
                    }
                    entry.LastDefault = factory;
                }
            }
        }

        public void RegisterDecorator(Decorator decorator, Type serviceType)
        {
            ThrowIfServiceTypeIsNotImplementedBy(decorator.Factory, serviceType);
            Throw.If(!decorator.Factory.Setup.SkipCache, Error.DECORATOR_FACTORY_SHOULD_NOT_CACHE_EXPRESSION, serviceType);
            lock (_syncRoot)
            {
                var entry = _registry.GetOrAdd(serviceType, _ => new RegistryEntry());
                if (entry.Decorators == null) entry.Decorators = new List<Decorator>();
                entry.Decorators.Add(decorator);
            }
        }

        public void RegisterGenericWrapper(Factory factory, Type serviceType, SelectGenericTypeArg getWrappedServiceType)
        {
            ThrowIfServiceTypeIsNotImplementedBy(factory, serviceType);
            lock (_syncRoot)
                _genericWrappers = _genericWrappers.AddOrUpdate(serviceType, new GenericWrapperEntry(factory, getWrappedServiceType));
        }

        public bool IsRegistered(Type serviceType, string serviceName)
        {
            return ((IRegistry)this).TryGetFactory(serviceType.ThrowIfNull(), serviceName) != null ||
                   serviceName == null && serviceType.IsGenericType &&
                   _genericWrappers.TryGet(serviceType.GetGenericTypeDefinition()) != null;
        }

        private static void ThrowIfServiceTypeIsNotImplementedBy(Factory factory, Type serviceType)
        {
            serviceType.ThrowIfNull();
            var implementationType = factory.ThrowIfNull().ImplementationType;
            if (implementationType != null && serviceType != typeof(object))
                Throw.If(!implementationType.GetSelfAndImplemented().Contains(serviceType),
                    Error.EXPECTED_IMPL_TYPE_ASSIGNABLE_TO_SERVICE_TYPE, implementationType, serviceType);
        }

        #endregion

        #region IResolver

        public object ResolveDefault(Type serviceType, bool shouldReturnNull)
        {
            var result = _defaultResolutionCache.TryGet(serviceType) ?? ResolveAndCacheFactory(serviceType, shouldReturnNull);
            return result == null ? null : result(CurrentScope, null/* resolutionRootScope */);
        }

        public object ResolveKeyed(Type serviceType, object serviceKey, bool shouldReturnNull)
        {
            var entry = _keyedResolutionCache.TryGet(serviceType);
            var result = entry != null ? entry.TryGet(serviceKey.ThrowIfNull()) : null;
            if (result == null) // nothing in cache, try resolve and cache.
            {
                var request = new Request(serviceType, serviceKey);
                var factory = ((IRegistry)this).GetOrAddFactory(request, shouldReturnNull);
                if (factory == null) return null;
                result = CompileExpression(factory.GetExpression(request, this));
                _keyedResolutionCache = _keyedResolutionCache.AddOrUpdate(serviceType,
                    (entry ?? KeyedResolutionCacheEntry.Empty).Add(serviceKey, result));
            }

            return result(CurrentScope, null /* resolutionRootScope */);
        }

        public delegate object CompiledFactory(Scope openScope, Scope resolutionRootScope);

        private HashTree<Type, CompiledFactory> _defaultResolutionCache;
        private HashTree<Type, KeyedResolutionCacheEntry> _keyedResolutionCache;

        private CompiledFactory ResolveAndCacheFactory(Type serviceType, bool shouldReturnNull)
        {
            var request = new Request(serviceType, null);
            var factory = ((IRegistry)this).GetOrAddFactory(request, shouldReturnNull);
            if (factory == null) return null;
            var result = CompileExpression(factory.GetExpression(request, this));
            _defaultResolutionCache = _defaultResolutionCache.AddOrUpdate(serviceType, result);
            return result;
        }

        private sealed class KeyedResolutionCacheEntry
        {
            public static readonly KeyedResolutionCacheEntry Empty = new KeyedResolutionCacheEntry();

            private HashTree<CompiledFactory> _indexed = HashTree<CompiledFactory>.Empty;
            private HashTree<string, CompiledFactory> _named = HashTree<string, CompiledFactory>.Empty;

            public CompiledFactory TryGet(object key)
            {
                return key is int ? _indexed.TryGet((int)key) : _named.TryGet((string)key);
            }

            public KeyedResolutionCacheEntry Add(object key, CompiledFactory factory)
            {
                return key is int
                    ? new KeyedResolutionCacheEntry { _indexed = _indexed.AddOrUpdate((int)key, factory), _named = _named }
                    : new KeyedResolutionCacheEntry { _indexed = _indexed, _named = _named.AddOrUpdate((string)key, factory) };
            }
        }

        #endregion

        #region Static Setup

        public static Func<Type, bool> PublicTypes = t => (t.IsPublic || t.IsNestedPublic) && t != typeof(object);

        public static Type[] FuncTypes = { typeof(Func<>), typeof(Func<,>), typeof(Func<,,>), typeof(Func<,,,>), typeof(Func<,,,,>) };

        public static Factory TryResolveFunc(Request _, IRegistry __)
        {
            return new CustomFactory(CreateFunc, Reuse.Singleton);
        }

        private static Expression CreateFunc(Request request, IRegistry registry)
        {
            var funcType = request.ServiceType;
            var funcTypeArgs = funcType.GetGenericArguments();
            var serviceType = funcTypeArgs[funcTypeArgs.Length - 1];

            var serviceRequest = request.PushPreservingKey(serviceType);
            var serviceFactory = registry.GetOrAddFactory(serviceRequest, false);

            if (funcTypeArgs.Length == 1)
                return Expression.Lambda(funcType, serviceFactory.GetExpression(serviceRequest, registry), null);

            var funcExpr = serviceFactory.TryGetFuncWithArgsExpression(funcType, serviceRequest, registry);
            return funcExpr.ThrowIfNull(Error.UNSUPPORTED_FUNC_WITH_ARGS, funcType);
        }

        public static Factory TryResolveMeta(Request request, IRegistry registry)
        {
            var genericArgs = request.ServiceType.GetGenericArguments();
            var serviceType = genericArgs[0];
            var metadataType = genericArgs[1];

            var wrappedServiceType = registry.GetWrappedServiceTypeOrSelf(serviceType);
            object resultMetadata = null;
            var serviceKey = request.ServiceKey;
            if (serviceKey == null)
            {
                var resultKey = registry.GetKeys(wrappedServiceType, factory =>
                    (resultMetadata = TryGetTypedMetadata(factory, metadataType)) != null).FirstOrDefault();
                if (resultKey != null)
                    serviceKey = resultKey;
            }
            else
            {
                var factory = registry.TryGetFactory(wrappedServiceType, serviceKey);
                if (factory != null)
                    resultMetadata = TryGetTypedMetadata(factory, metadataType);
            }

            if (resultMetadata == null)
                return null;

            return new CustomFactory((_, __) =>
            {
                var serviceRequest = request.Push(serviceType, serviceKey);
                var serviceFactory = registry.GetOrAddFactory(serviceRequest, false);

                var metaCtor = request.ServiceType.GetConstructors()[0];
                var serviceExpr = serviceFactory.GetExpression(serviceRequest, registry);
                var metadataExpr = Expression.Constant(resultMetadata, metadataType);
                return Expression.New(metaCtor, serviceExpr, metadataExpr);
            });
        }

        private static object TryGetTypedMetadata(Factory factory, Type metadataType)
        {
            var metadata = factory.Setup.Metadata;
            return metadata != null && metadataType.IsInstanceOfType(metadata) ? metadata : null;
        }

        public static Factory TryResolveEnumerableOrArray(Request req, IRegistry _)
        {
            if (!req.ServiceType.IsArray && req.OpenGenericServiceType != typeof(IEnumerable<>))
                return null;

            return new CustomFactory((request, registry) =>
                {
                    var collectionType = request.ServiceType;
                    var collectionIsArray = collectionType.IsArray;
                    var itemType = collectionIsArray ? collectionType.GetElementType() : collectionType.GetGenericArguments()[0];
                    var wrappedItemType = registry.GetWrappedServiceTypeOrSelf(itemType);

                    var resolver = Expression.Constant(new EnumerableResolver(registry, itemType, wrappedItemType));
                    var resolveMethod = (collectionIsArray
                        ? EnumerableResolver.ResolveArrayMethod
                        : EnumerableResolver.ResolveEnumerableMethod).MakeGenericMethod(itemType);
                    return Expression.Call(resolver, resolveMethod, Expression.Constant(request));
                },
                setup: Factory.With(skipCache: true));
        }

        internal sealed class EnumerableResolver
        {
            public static readonly MethodInfo ResolveEnumerableMethod = typeof(EnumerableResolver).GetMethod("ResolveEnumerable");
            public static readonly MethodInfo ResolveArrayMethod = typeof(EnumerableResolver).GetMethod("ResolveArray");

            public EnumerableResolver(IRegistry registry, Type itemType, Type unwrappedItemType)
            {
                _registry = new WeakReference(registry);
                _unwrappedItemType = unwrappedItemType;
                _itemType = itemType;
            }

            public IEnumerable<T> ResolveEnumerable<T>(Request request)
            {
                var registry = (_registry.Target as IRegistry).ThrowIfNull(Error.CONTAINER_IS_GARBAGE_COLLECTED);

                var parent = request.TryGetNonWrapperParent(); // Composite pattern support: filter out composite root from available keys.
                var keys = parent != null && parent.ServiceType == _unwrappedItemType
                    ? registry.GetKeys(_unwrappedItemType, factory => factory.ID != parent.FactoryID)
                    : registry.GetKeys(_unwrappedItemType, null);

                foreach (var key in keys)
                {
                    var service = registry.ResolveKeyed(_itemType, key, shouldReturnNull: true);
                    if (service != null) // skip unresolved items
                        yield return (T)service;
                }
            }

            public T[] ResolveArray<T>(Request request)
            {
                return ResolveEnumerable<T>(request).ToArray();
            }

            private readonly WeakReference _registry;
            private readonly Type _unwrappedItemType;
            private readonly Type _itemType;
        }

        #endregion

        #region Diagnostics

        [DebuggerDisplay("Factory Expression: {Expression}")]
        public sealed class FactoryExpression<TService>
        {
            public readonly Expression<CompiledFactory> Expression;

            public FactoryExpression(Expression<CompiledFactory> expression)
            {
                Expression = expression;
            }
        }

        public static Factory TryResolveFactoryExpression(Request request, IRegistry registry)
        {
            var factoryCtor = request.ServiceType.GetConstructors()[0];
            var serviceType = request.ServiceType.GetGenericArguments()[0];

            var serviceRequest = request.PushPreservingKey(serviceType);
            var serviceExpr = registry.GetOrAddFactory(serviceRequest, false).GetExpression(serviceRequest, registry);
            var newFactory = Expression.New(factoryCtor, Expression.Lambda<CompiledFactory>(serviceExpr, Reuse.Parameters));

            return new CustomFactory((_, __) => newFactory);
        }

        #endregion

        #region Implementation

        private Container(Container source)
        {
            CurrentScope = new Scope();
            SingletonScope = source.SingletonScope;
            Setup = source.Setup;
            _syncRoot = source._syncRoot;
            _registry = source._registry;
            _genericWrappers = source._genericWrappers;
            _keyedResolutionCache = source._keyedResolutionCache;
            _defaultResolutionCache = source._defaultResolutionCache;
        }

        private readonly object _syncRoot;
        private readonly Dictionary<Type, RegistryEntry> _registry;
        private HashTree<Type, GenericWrapperEntry> _genericWrappers;

        private sealed class RegistryEntry
        {
            public Factory LastDefault;
            public List<Factory> Defaults;
            public Dictionary<string, Factory> Named;
            public List<Decorator> Decorators;

            public bool TryGetServiceFactory(out Factory result, Type serviceType, object serviceKey)
            {
                result = null;

                if (serviceKey == null)
                {
                    if (Defaults != null)
                        ThrowUnexpectedMultipleDefaults(Defaults, serviceType);
                    result = LastDefault;
                }
                else
                {
                    if (serviceKey is string)
                    {
                        if (Named != null)
                            Named.TryGetValue((string)serviceKey, out result);
                    }
                    else if (serviceKey is int)
                    {
                        var index = (int)serviceKey;
                        if (Defaults == null && index == 0)
                            result = LastDefault;
                        else if (Defaults != null && index < Defaults.Count)
                            result = Defaults[index];
                    }
                }

                return result != null;
            }

            public bool TryGetDecorator(out Decorator result, Request request)
            {
                result = null;
                if (Decorators == null)
                    return false;

                List<int> appliedDecoratorIDs = null; // gather already applied decorators to check for recursion
                for (var p = request.TryGetNonWrapperParent();
                   p != null && p.FactoryType == FactoryType.Decorator;
                   p = p.TryGetNonWrapperParent())
                {
                   (appliedDecoratorIDs ?? (appliedDecoratorIDs = new List<int>())).Add(p.FactoryProviderID);
                }

                result = Decorators.FindLast(d =>
                    (appliedDecoratorIDs == null || !appliedDecoratorIDs.Contains(d.Factory.ProviderID)) && 
                    d.IsApplicable(request));

                return result != null;
            }
        }

        private sealed class GenericWrapperEntry
        {
            public readonly Factory Factory;
            public readonly SelectGenericTypeArg GetWrappedServiceType;

            public GenericWrapperEntry(Factory factory, SelectGenericTypeArg getWrappedServiceType)
            {
                Factory = factory.ThrowIfNull();
                GetWrappedServiceType = getWrappedServiceType.ThrowIfNull();
            }
        }

        #endregion
    }

    public class Decorator
    {
        public Factory Factory;

        public Decorator(Factory factory, Func<Request, bool> isApplicable = null)
        {
            Factory = factory;
            _isApplicable = isApplicable;
        }

        public bool IsApplicable(Request request)
        {
            return _isApplicable == null || _isApplicable(request);
        }

        public Decorator TryProvideDecoratorFor(Request request, IRegistry registry)
        {
            var specificFactory = Factory.TryProvideFactoryFor(request, registry);
            return specificFactory == null ? null : new Decorator(specificFactory, _isApplicable);
        }

        private readonly Func<Request, bool> _isApplicable;
    }

    public abstract class FactoryProvider : Factory
    {
        protected override Expression CreateExpression(Request request, IRegistry registry)
        {
            throw new NotSupportedException();
        }
    }

    public class FuncFactory : FactoryProvider
    {
        public override Factory TryProvideFactoryFor(Request request, IRegistry registry)
        {
            return new CustomFactory(CreateFunc, DryIoc.Reuse.Singleton);
        }

        private static Expression CreateFunc(Request request, IRegistry registry)
        {
            var funcType = request.ServiceType;
            var funcTypeArgs = funcType.GetGenericArguments();
            var serviceType = funcTypeArgs[funcTypeArgs.Length - 1];

            var serviceRequest = request.PushPreservingKey(serviceType);
            var serviceFactory = registry.GetOrAddFactory(serviceRequest, false);

            if (funcTypeArgs.Length == 1)
                return Expression.Lambda(funcType, serviceFactory.GetExpression(serviceRequest, registry), null);

            var funcExpr = serviceFactory.TryGetFuncWithArgsExpression(funcType, serviceRequest, registry);
            return funcExpr.ThrowIfNull(Error.UNSUPPORTED_FUNC_WITH_ARGS, funcType);
        }
    }

    public sealed class Setup
    {
        public Setup()
        {
            NonRegisteredServiceResolutionRules = new TryGetFactory[0];
            ConstructorParamServiceKeyResolutionRules = new TryGetConstructorParamServiceKey[0];
            PropertyOrFieldResolutionRules = new TryGetPropertyOrFieldServiceKey[0];
        }

        public delegate object TryGetConstructorParamServiceKey(ParameterInfo parameter, Request parent, IRegistry registry);

        public delegate bool TryGetPropertyOrFieldServiceKey(out object resultKey, MemberInfo propertyOrField, Request parent, IRegistry registry);

        public TryGetFactory[] NonRegisteredServiceResolutionRules { get; private set; }

        public void SetNonRegisteredServiceResolutionRules(IEnumerable<TryGetFactory> newRules)
        {
            NonRegisteredServiceResolutionRules = newRules.ThrowIfNull().ToArray();
        }

        public TryGetFactory AddNonRegisteredServiceResolutionRule(TryGetFactory rule)
        {
            SetNonRegisteredServiceResolutionRules(NonRegisteredServiceResolutionRules.Concat(new[] { rule }));
            return rule;
        }

        public void RemoveNonRegisteredServiceResolutionRule(TryGetFactory rule)
        {
            SetNonRegisteredServiceResolutionRules(NonRegisteredServiceResolutionRules.Except(new[] { rule }));
        }

        public TryGetConstructorParamServiceKey[] ConstructorParamServiceKeyResolutionRules { get; private set; }

        public void SetConstructorParamServiceKeyResolutionRules(IEnumerable<TryGetConstructorParamServiceKey> newRules)
        {
            ConstructorParamServiceKeyResolutionRules = newRules.ThrowIfNull().ToArray();
        }

        public TryGetConstructorParamServiceKey AddConstructorParamServiceKeyResolutionRule(TryGetConstructorParamServiceKey rule)
        {
            SetConstructorParamServiceKeyResolutionRules(ConstructorParamServiceKeyResolutionRules.Concat(new[] { rule }));
            return rule;
        }

        public void RemoveConstructorParamServiceKeyResolutionRule(TryGetConstructorParamServiceKey rule)
        {
            SetConstructorParamServiceKeyResolutionRules(ConstructorParamServiceKeyResolutionRules.Except(new[] { rule }));
        }

        public TryGetPropertyOrFieldServiceKey[] PropertyOrFieldResolutionRules { get; private set; }

        public void SetPropertyOrFieldResolutionRules(IEnumerable<TryGetPropertyOrFieldServiceKey> newRules)
        {
            PropertyOrFieldResolutionRules = newRules.ThrowIfNull().ToArray();
        }

        public TryGetPropertyOrFieldServiceKey AddPropertyOrFieldResolutionRule(TryGetPropertyOrFieldServiceKey rule)
        {
            SetPropertyOrFieldResolutionRules(PropertyOrFieldResolutionRules.Concat(new[] { rule }));
            return rule;
        }

        public void RemovePropertyOrFieldResolutionRule(TryGetPropertyOrFieldServiceKey rule)
        {
            SetPropertyOrFieldResolutionRules(PropertyOrFieldResolutionRules.Except(new[] { rule }));
        }
    }

    public static class Error
    {
        public static readonly string UNABLE_TO_RESOLVE_SERVICE =
            "Unable to resolve service {0}." + Environment.NewLine +
            "Please register service {1} Or adjust Container Setup.";

        public static readonly string UNSUPPORTED_FUNC_WITH_ARGS =
            "Unsupported resolution as {0}.";

        public static readonly string EXPECTED_IMPL_TYPE_ASSIGNABLE_TO_SERVICE_TYPE =
            "Expecting implementation type {0} to be assignable to service type {1} but it is not.";

        public static readonly string EXPECTED_SINGLE_DEFAULT_FACTORY =
            "Expecting single default registration of {0} but found {1} registrations: {2}";

        public static readonly string EXPECTED_NON_ABSTRACT_IMPL_TYPE =
            "Expecting not abstract and not interface implementation type, but found {0}.";

        public static readonly string NO_PUBLIC_CONSTRUCTOR_DEFINED =
            "There is no public constructor defined for {0}.";

        public static readonly string CONSTRUCTOR_MISSES_PARAMETERS =
            "Constructor [{0}] of {1} misses some arguments required for {2} dependency.";

        public static readonly string UNABLE_TO_SELECT_CONSTRUCTOR =
            "Unable to select single constructor from {0} available in {1}. " + Environment.NewLine +
            "Please provide constructor selector when registering service.";

        public static readonly string EXPECTED_FUNC_WITH_MULTIPLE_ARGS =
            "Expecting Func with one or more arguments but found {0}.";

        public static readonly string EXPECTED_CLOSED_GENERIC_SERVICE_TYPE =
            "Expecting closed-generic service type but found {0}.";

        public static readonly string DEPENDENCY_CYCLE_DETECTED =
            "Recursive dependency is detected in resolution request:\n{0}.";

        public static readonly string SCOPE_IS_DISPOSED =
            "Scope is disposed and all in-scope instances are no longer available.";

        public static readonly string CONTAINER_IS_GARBAGE_COLLECTED =
            "Container is no longer available (has been garbage-collected already).";

        public static readonly string DECORATOR_FACTORY_SHOULD_NOT_CACHE_EXPRESSION =
            "Decorator factory of {0} should not cache expression. Please specify FactoryOptions.SkipCache=true.";

        public static readonly string DUPLICATE_SERVICE_NAME_REGISTRATION = 
            "Service {0} with duplicate name '{1}' is already registered with implementation {2}.";
    }

    public static class Registrator
    {
        /// <summary>
        /// Registers service <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="serviceType">The service type to register</param>
        /// <param name="factory"><see cref="Factory"/> details object.</param>
        /// <param name="named">Optional name of the service.</param>
        public static void Register(this IRegistrator registrator, Type serviceType, Factory factory, string named = null)
        {
            registrator.Register(factory, serviceType, named);
        }

        /// <summary>
        /// Registers service of <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of service.</typeparam>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="factory"><see cref="Factory"/> details object.</param>
        /// <param name="named">Optional name of the service.</param>
        public static void Register<TService>(this IRegistrator registrator, Factory factory, string named = null)
        {
            registrator.Register(factory, typeof(TService), named);
        }

        /// <summary>
        /// Registers service <paramref name="serviceType"/> with corresponding <paramref name="implementationType"/>.
        /// </summary>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="serviceType">The service type to register.</param>
        /// <param name="implementationType">Implementation type. Concrete and open-generic class are supported.</param>
        /// <param name="reuse">Optional <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="withConstructor">Optional strategy to select constructor when multiple available.</param>
        /// <param name="setup">Optional factory setup, by default is (<see cref="FactorySetup.Service"/>)</param>
        /// <param name="named">Optional registration name.</param>
        public static void Register(this IRegistrator registrator, Type serviceType,
            Type implementationType, IReuse reuse = null, ConstructorSelector withConstructor = null, FactorySetup setup = null,
            string named = null)
        {
            registrator.Register(new ReflectionFactory(implementationType, reuse, withConstructor, setup), serviceType, named);
        }

        /// <summary>
        /// Registers service of <paramref name="implementationType"/>. ServiceType will be the same as <paramref name="implementationType"/>.
        /// </summary>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="implementationType">Implementation type. Concrete and open-generic class are supported.</param>
        /// <param name="reuse">Optional <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="withConstructor">Optional strategy to select constructor when multiple available.</param>
        /// <param name="setup">Optional factory setup, by default is (<see cref="FactorySetup.Service"/>)</param>
        /// <param name="named">Optional registration name.</param>
        public static void Register(this IRegistrator registrator,
            Type implementationType, IReuse reuse = null, ConstructorSelector withConstructor = null, FactorySetup setup = null,
            string named = null)
        {
            registrator.Register(new ReflectionFactory(implementationType, reuse, withConstructor, setup), implementationType, named);
        }

        /// <summary>
        /// Registers service of <typeparamref name="TService"/> type implemented by <typeparamref name="TImplementation"/> type.
        /// </summary>
        /// <typeparam name="TService">The type of service.</typeparam>
        /// <typeparam name="TImplementation">The type of service.</typeparam>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="reuse">Optional <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="withConstructor">Optional strategy to select constructor when multiple available.</param>
        /// <param name="setup">Optional factory setup, by default is (<see cref="FactorySetup.Service"/>)</param>
        /// <param name="named">Optional registration name.</param>
        public static void Register<TService, TImplementation>(this IRegistrator registrator,
            IReuse reuse = null, ConstructorSelector withConstructor = null, FactorySetup setup = null,
            string named = null)
            where TImplementation : TService
        {
            registrator.Register(new ReflectionFactory(typeof(TImplementation), reuse, withConstructor, setup), typeof(TService), named);
        }

        /// <summary>
        /// Registers single registration for all implemented public interfaces and base classes.
        /// </summary>
        /// <typeparam name="TImplementation">The type of service.</typeparam>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="reuse">Optional <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="withConstructor">Optional strategy to select constructor when multiple available.</param>
        /// <param name="setup">Optional factory setup, by default is (<see cref="FactorySetup.Service"/>)</param>
        /// <param name="named">Optional registration name.</param>
        public static void Register<TImplementation>(this IRegistrator registrator,
            IReuse reuse = null, ConstructorSelector withConstructor = null, FactorySetup setup = null,
            string named = null)
        {
            registrator.Register(new ReflectionFactory(typeof(TImplementation), reuse, withConstructor, setup), typeof(TImplementation), named);
        }

        /// <summary>
        /// Registers single registration for all implemented public interfaces and base classes.
        /// </summary>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="implementationType">Service implementation type. Concrete and open-generic class are supported.</param>
        /// <param name="reuse">Optional <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="withConstructor">Optional strategy to select constructor when multiple available.</param>
        /// <param name="setup">Optional factory setup, by default is (<see cref="FactorySetup.Service"/>)</param>
        /// <param name="named">Optional registration name.</param>
        public static void RegisterPublicTypes(this IRegistrator registrator,
            Type implementationType, IReuse reuse = null, ConstructorSelector withConstructor = null, FactorySetup setup = null,
            string named = null)
        {
            var registration = new ReflectionFactory(implementationType, reuse, withConstructor, setup);
            foreach (var serviceType in implementationType.GetSelfAndImplemented().Where(Container.PublicTypes))
                registrator.Register(registration, serviceType, named);
        }

        /// <summary>
        /// Registers single registration for all implemented public interfaces and base classes.
        /// </summary>
        /// <typeparam name="TImplementation">The type of service.</typeparam>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="reuse">Optional <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="withConstructor">Optional strategy to select constructor when multiple available.</param>
        /// <param name="setup">Optional factory setup, by default is (<see cref="FactorySetup.Service"/>)</param>
        /// <param name="named">Optional registration name.</param>
        public static void RegisterPublicTypes<TImplementation>(this IRegistrator registrator,
            IReuse reuse = null, ConstructorSelector withConstructor = null, FactorySetup setup = null,
            string named = null)
        {
            registrator.RegisterPublicTypes(typeof(TImplementation), reuse, withConstructor, setup, named);
        }

        /// <summary>
        /// Registers a factory delegate for creating an instance of <typeparamref name="TService"/> 
        /// </summary>
        /// <typeparam name="TService">The type of service.</typeparam>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="lambda">The delegate used to create a instance of <typeparamref name="TService"/>.</param>
        /// <param name="reuse">Optional <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="setup">Optional factory setup, by default is (<see cref="FactorySetup.Service"/>)</param>
        /// <param name="named">Optional service name.</param>
        public static void RegisterLambda<TService>(this IRegistrator registrator,
            Func<TService> lambda, IReuse reuse = null, FactorySetup setup = null,
            string named = null)
        {
            var lambdaCall = Expression.Call(Expression.Constant(lambda), "Invoke", null, null);
            registrator.Register(new CustomFactory((_, __) => lambdaCall, reuse, setup), typeof(TService), named);
        }

        /// <summary>
        /// Registers a pre-created service instance of <typeparamref name="TService"/> 
        /// </summary>
        /// <typeparam name="TService">The type of service.</typeparam>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="instance">The pre-created instance of <typeparamref name="TService"/>.</param>
        /// <param name="setup">Optional factory setup, by default is (<see cref="FactorySetup.Service"/>)</param>
        /// <param name="named">Optional service name.</param>
        public static void RegisterInstance<TService>(this IRegistrator registrator,
            TService instance, FactorySetup setup = null,
            string named = null)
        {
            registrator.RegisterLambda(() => instance, Reuse.Transient, setup, named);
        }

        /// <summary>
        /// Registers decorator of <paramref name="decoratorType"/> type for instances of <paramref name="serviceType"/> type.
        /// </summary>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="serviceType">The service type to register.</param>
        /// <param name="decoratorType">Implementation type. Concrete and open-generic class are supported.</param>
        /// <param name="withConstructor">Optional strategy to select constructor when multiple available.</param>
        public static void Decorate(this IRegistrator registrator,
            Type serviceType, Type decoratorType, ConstructorSelector withConstructor = null)
        {
            var factory = new ReflectionFactory(decoratorType, Reuse.Transient, withConstructor, new FactorySetup.Decorator());
            registrator.RegisterDecorator(new Decorator(factory), serviceType);
        }

        /// <summary>
        /// Registers decorator of <typeparamref name="TDecorator"/> type for instances of <typeparamref name="TService"/> type.
        /// </summary>
        /// <typeparam name="TService">The type of service.</typeparam>
        /// <typeparam name="TDecorator">The type of service.</typeparam>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="withConstructor">Optional strategy to select constructor when multiple available.</param>
        public static void Decorate<TService, TDecorator>(this IRegistrator registrator, ConstructorSelector withConstructor = null)
            where TDecorator : TService
        {
            registrator.Decorate(typeof(TService), typeof(TDecorator));
        }

        /// <summary>
        /// Returns true if <paramref name="serviceType"/> is registered in container or its open generic definition is registered in container.
        /// </summary>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="serviceType">The type of the registered service.</param>
        /// <param name="named">Optional service name</param>
        /// <returns>True if <paramref name="serviceType"/> is registered, false - otherwise.</returns>
        public static bool IsRegistered(this IRegistrator registrator, Type serviceType, string named = null)
        {
            return registrator.IsRegistered(serviceType, named);
        }

        /// <summary>
        /// Returns true if <typeparamref name="TService"/> type is registered in container or its open generic definition is registered in container.
        /// </summary>
        /// <typeparam name="TService">The type of service.</typeparam>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="named">Optional service name</param>
        /// <returns>True if <typeparamref name="TService"/> name="serviceType"/> is registered, false - otherwise.</returns>
        public static bool IsRegistered<TService>(this IRegistrator registrator, string named = null)
        {
            return registrator.IsRegistered(typeof(TService), named);
        }
    }

    public static class Resolver
    {
        /// <summary>
        /// Returns an instance of statically known <typepsaramref name="TService"/> type.
        /// </summary>
        /// <param name="serviceType">The type of the requested service.</param>
        /// <param name="resolver">Any <see cref="IResolver"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <returns>The requested service instance.</returns>
        public static object Resolve(this IResolver resolver, Type serviceType)
        {
            return resolver.ResolveDefault(serviceType, false);
        }

        /// <summary>
        /// Returns an instance of statically known <typepsaramref name="TService"/> type.
        /// </summary>
        /// <typeparam name="TService">The type of the requested service.</typeparam>
        /// <param name="resolver">Any <see cref="IResolver"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <returns>The requested service instance.</returns>
        public static TService Resolve<TService>(this IResolver resolver)
        {
            return (TService)resolver.ResolveDefault(typeof(TService), false);
        }

        /// <summary>
        /// Returns an instance of statically known <typepsaramref name="TService"/> type.
        /// </summary>
        /// <param name="serviceType">The type of the requested service.</param>
        /// <param name="resolver">Any <see cref="IResolver"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="serviceName">Service name.</param>
        /// <returns>The requested service instance.</returns>
        public static object Resolve(this IResolver resolver, Type serviceType, string serviceName)
        {
            return serviceName == null
                ? resolver.ResolveDefault(serviceType, false)
                : resolver.ResolveKeyed(serviceType, serviceName, false);
        }

        /// <summary>
        /// Returns an instance of statically known <typepsaramref name="TService"/> type.
        /// </summary>
        /// <typeparam name="TService">The type of the requested service.</typeparam>
        /// <param name="resolver">Any <see cref="IResolver"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="serviceName">Service name.</param>
        /// <returns>The requested service instance.</returns>
        public static TService Resolve<TService>(this IResolver resolver, string serviceName)
        {
            return (TService)(serviceName == null
                ? resolver.ResolveDefault(typeof(TService), false)
                : resolver.ResolveKeyed(typeof(TService), serviceName, false));
        }

        /// <summary>
        /// For given instance resolves and sets non-initialized (null) properties from container.
        /// It does not throw if property is not resolved, so you might need to check property value afterwards.
        /// </summary>
        /// <param name="resolver">Any <see cref="IResolver"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="instance">Service instance with properties to resolve and initialize.</param>
        /// <param name="getServiceName">Optional function to find service name, otherwise service name will be null.</param>
        public static void ResolveProperties(this IResolver resolver, object instance, Func<PropertyInfo, string> getServiceName = null)
        {
            instance.ThrowIfNull();

            var properties = instance.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetSetMethod() != null); // with public setter

            foreach (var property in properties)
            {
                var serviceKey = getServiceName != null ? getServiceName(property) : null;
                var resolvedValue = serviceKey == null
                    ? resolver.ResolveDefault(property.PropertyType, true)
                    : resolver.ResolveKeyed(property.PropertyType, serviceKey, true);
                if (resolvedValue != null)
                    property.SetValue(instance, resolvedValue, null);
            }
        }
    }

    public delegate Expression Init(Expression source);

    public abstract class FactorySetup
    {
        public static readonly FactorySetup Default = new Service();

        public virtual Init Init { get { return null; } }
        public virtual bool SkipCache { get { return false; } }
        public virtual object Metadata { get { return null; } }

        public class Service : FactorySetup
        {
            public override Init Init { get { return _init; } }
            public override bool SkipCache { get { return _skipCache; } }
            public override object Metadata { get { return _metadata; } }

            public Service(Init init = null, bool skipCache = false, object metadata = null)
            {
                _init = init;
                _skipCache = skipCache;
                _metadata = metadata;
            }

            private readonly Init _init;
            private readonly bool _skipCache;
            private readonly object _metadata;
        }

        public class GenericWrapper : FactorySetup
        {
            public readonly SelectGenericTypeArg SelectGenericTypeArg;

            public GenericWrapper(SelectGenericTypeArg selectGenericTypeArg = null)
            {
                SelectGenericTypeArg = selectGenericTypeArg ?? SelectSingleOrThrow;
            }

            private static Type SelectSingleOrThrow(Type[] typeArgs)
            {
                return typeArgs.ThrowIf(typeArgs.Length != 1)[0];
            }
        }

        public class Decorator : FactorySetup
        {
            public override bool SkipCache { get { return true; } }
            public readonly Func<Request, bool> IsApplicable;

            public Decorator(Func<Request, bool> isApplicable = null)
            {
                IsApplicable = isApplicable ?? ApplicableIndeed;
            }

            private static bool ApplicableIndeed(Request _)
            {
                return true;
            }
        }
    }
    
    public abstract class Factory
    {
        public static volatile int Count;

        public readonly int ID;

        public virtual int ProviderID { get { return ID; } }

        public readonly IReuse Reuse;

        public readonly FactorySetup Setup;

        public virtual Type ImplementationType { get { return null; } }

        protected Factory(IReuse reuse = null, FactorySetup setup = null)
        {
            ID = Interlocked.Increment(ref Count);
            Reuse = reuse;
            Setup = setup ?? FactorySetup.Default;
        }

        public Expression GetExpression(Request request, IRegistry registry)
        {
            if (Setup.SkipCache || _cachedExpression == null)
            {
                var expression = CreateExpression(request, registry);
                if (Setup.Init != null)
                    expression = Setup.Init(expression);
                if (Reuse != null)
                    expression = Reuse.Of(request, registry, ID, expression);
                if (Setup.SkipCache)
                    return expression;
                Interlocked.CompareExchange(ref _cachedExpression, expression, null);
            }

            return _cachedExpression;
        }

        public LambdaExpression TryGetFuncWithArgsExpression(Type funcType, Request request, IRegistry registry)
        {
            var func = TryCreateFuncWithArgsExpression(funcType, request, registry);
            if (func == null)
                return null;
            var expression = func.Body;
            if (Setup.Init != null)
                expression = Setup.Init(expression);
            if (Reuse != null)
                func = Expression.Lambda(funcType, Reuse.Of(request, registry, ID, expression), func.Parameters);
            return func;
        }

        public abstract Factory TryProvideFactoryFor(Request request, IRegistry registry);
        
        protected abstract Expression CreateExpression(Request request, IRegistry registry);

        protected virtual LambdaExpression TryCreateFuncWithArgsExpression(Type funcType, Request request, IRegistry registry)
        {
            return null;
        }

        #region Implementation

        private volatile Expression _cachedExpression;

        #endregion

        public static FactorySetup With(Init init = null, bool skipCache = false, object metadata = null)
        {
            return new FactorySetup.Service(init, skipCache, metadata);
        }

        public static FactorySetup WithMetadata(object metadata)
        {
            return new FactorySetup.Service(metadata: metadata);
        }
    }

    public delegate ConstructorInfo ConstructorSelector(Type implementationType);

    public sealed class ReflectionFactory : Factory
    {
        public override Type ImplementationType { get { return _implementationType; } }

        public override int ProviderID { get { return _providerID; } }

        public ReflectionFactory(Type implementationType, IReuse reuse = null, ConstructorSelector selectConstructor = null, 
            FactorySetup setup = null)
            : base(reuse, setup)
        {
            _implementationType = implementationType.ThrowIfNull();
             Throw.If(implementationType.IsAbstract, Error.EXPECTED_NON_ABSTRACT_IMPL_TYPE, implementationType);
            _selectConstructor = selectConstructor;
            _providerID = ID;
        }

        public override Factory TryProvideFactoryFor(Request request, IRegistry _)
        {
            if (!_implementationType.IsGenericTypeDefinition) return null;
            var closedImplType = _implementationType.MakeGenericType(request.ServiceType.GetGenericArguments());
            return new ReflectionFactory(closedImplType, Reuse, _selectConstructor, Setup) { _providerID = ID };
        }

        protected override Expression CreateExpression(Request request, IRegistry registry)
        {
            var ctor = SelectConstructor();
            var ctorParams = ctor.GetParameters();
            Expression[] paramExprs = null;
            if (ctorParams.Length != 0)
            {
                paramExprs = new Expression[ctorParams.Length];
                for (var i = 0; i < ctorParams.Length; i++)
                {
                    var ctorParam = ctorParams[i];
                    var paramKey = registry.TryGetConstructorParamKey(ctorParam, request);
                    var paramRequest = request.Push(ctorParam.ParameterType, paramKey);
                    paramExprs[i] = registry.GetOrAddFactory(paramRequest, false).GetExpression(paramRequest, registry);
                }
            }

            return WithInitializer(Expression.New(ctor, paramExprs), request, registry);
        }

        protected override LambdaExpression TryCreateFuncWithArgsExpression(Type funcType, Request request, IRegistry registry)
        {
            var funcParamTypes = funcType.GetGenericArguments();
            funcParamTypes.ThrowIf(funcParamTypes.Length == 1, Error.EXPECTED_FUNC_WITH_MULTIPLE_ARGS, funcType);

            var ctor = SelectConstructor();
            var ctorParams = ctor.GetParameters();
            var ctorParamCount = ctorParams.Length;

            var funcInputParamCount = funcParamTypes.Length - 1;
            var funcInputParamTypes = funcParamTypes.Take(funcInputParamCount).ToArray();
            Throw.If(ctorParamCount < funcInputParamCount, Error.CONSTRUCTOR_MISSES_PARAMETERS, ctor, ImplementationType, funcType);

            var ctorParamExprs = new Expression[ctorParamCount];
            var funcInputParamExprs = new ParameterExpression[funcInputParamCount];

            for (var cp = 0; cp < ctorParamCount; cp++)
            {
                var ctorParam = ctorParams[cp];
                ParameterExpression funcInputParamExpr = null;
                for (var fp = 0; fp < funcInputParamCount && funcInputParamExpr == null; fp++)
                    if (funcInputParamTypes[fp] == ctorParam.ParameterType && funcInputParamExprs[fp] == null)
                        funcInputParamExprs[fp] = funcInputParamExpr = Expression.Parameter(ctorParam.ParameterType, ctorParam.Name);

                if (funcInputParamExpr != null)
                {
                    ctorParamExprs[cp] = funcInputParamExpr;
                }
                else
                {
                    var paramKey = registry.TryGetConstructorParamKey(ctorParam, request);
                    var paramRequest = request.Push(ctorParam.ParameterType, paramKey);
                    ctorParamExprs[cp] = registry.GetOrAddFactory(paramRequest, false).GetExpression(paramRequest, registry);
                }
            }

            var newExpr = Expression.New(ctor, ctorParamExprs);
            return Expression.Lambda(funcType, WithInitializer(newExpr, request, registry), funcInputParamExprs);
        }

        #region Implementation

        private readonly Type _implementationType;

        private readonly ConstructorSelector _selectConstructor;

        private int _providerID;

        private ConstructorInfo SelectConstructor()
        {
            var constructors = _implementationType.GetConstructors();
            if (constructors.Length == 1)
                return constructors[0];

            Throw.If(constructors.Length == 0, Error.NO_PUBLIC_CONSTRUCTOR_DEFINED, _implementationType);
            _selectConstructor.ThrowIfNull(Error.UNABLE_TO_SELECT_CONSTRUCTOR, constructors.Length, _implementationType);
            return _selectConstructor(_implementationType);
        }

        private Expression WithInitializer(NewExpression newService, Request request, IRegistry registry)
        {
            if (!registry.ShouldResolvePropertyOrField)
                return newService;

            var properties = ImplementationType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetSetMethod() != null)
                .Cast<MemberInfo>();
            var fields = ImplementationType.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => !f.IsInitOnly)
                .Cast<MemberInfo>();

            var bindings = new List<MemberBinding>();
            foreach (var propOrField in properties.Concat(fields))
            {
                object propOrFieldKey;
                if (registry.TryGetPropertyOrFieldKey(out propOrFieldKey, propOrField, request))
                {
                    var propOrFieldType = propOrField.MemberType == MemberTypes.Field
                        ? ((FieldInfo)propOrField).FieldType
                        : ((PropertyInfo)propOrField).PropertyType;

                    var propOrFieldRequest = request.Push(propOrFieldType, propOrFieldKey);
                    var propOrFieldExpr = registry.GetOrAddFactory(propOrFieldRequest, false).GetExpression(propOrFieldRequest, registry);
                    bindings.Add(Expression.Bind(propOrField, propOrFieldExpr));
                }
            }

            return bindings.Count == 0 ? (Expression)newService : Expression.MemberInit(newService, bindings);
        }

        #endregion
    }

    public class CustomFactory : Factory
    {
        public CustomFactory(Func<Request, IRegistry, Expression> getExpression, IReuse reuse = null, FactorySetup setup = null)
            : base(reuse, setup)
        {
            _getExpression = getExpression.ThrowIfNull();
        }

        public override Factory TryProvideFactoryFor(Request request, IRegistry registry)
        {
            return null;
        }

        protected override Expression CreateExpression(Request request, IRegistry registry)
        {
            return _getExpression(request, registry);
        }

        #region Implementation

        private readonly Func<Request, IRegistry, Expression> _getExpression;

        #endregion
    }

    public delegate Factory TryGetFactory(Request request, IRegistry registry);

    public class CustomFactoryProvider : Factory
    {
        public CustomFactoryProvider(TryGetFactory tryGetFactory)
        {
            _tryGetFactory = tryGetFactory.ThrowIfNull();
        }

        public override Factory TryProvideFactoryFor(Request request, IRegistry registry)
        {
            return _tryGetFactory(request, registry);
        }

        protected override Expression CreateExpression(Request request, IRegistry registry)
        {
            throw new NotSupportedException();
        }

        #region Implementation

        private readonly TryGetFactory _tryGetFactory;

        #endregion
    }

    public enum FactoryType { Service, Decorator, GenericWrapper };

    public sealed class Request : IEnumerable<Request>
    {
        public readonly Request Parent; // can be null for resolution root
        public readonly Type ServiceType;
        public readonly Type OpenGenericServiceType;
        public readonly object ServiceKey; // null for default, string for named or integer index for multiple defaults.

        public FactoryType FactoryType { get; private set; }
        public int FactoryID { get; private set; }
        public int FactoryProviderID { get; private set; }
        public Type ImplementationType { get; private set; }

        public Request(Type serviceType, object serviceKey, Request parent = null)
        {
            ServiceType = serviceType.ThrowIfNull();
            Throw.If(serviceType.IsGenericTypeDefinition, Error.EXPECTED_CLOSED_GENERIC_SERVICE_TYPE, serviceType);
            OpenGenericServiceType = serviceType.IsGenericType ? serviceType.GetGenericTypeDefinition() : null;
            ServiceKey = serviceKey;
            Parent = parent;
        }

        public Request TryGetNonWrapperParent()
        {
            var p = Parent;
            while (p != null && p.FactoryType == FactoryType.GenericWrapper)
                p = p.Parent;
            return p;
        }

        public Request TryGetParent(Func<Request, bool> condition)
        {
            var p = Parent;
            while (p != null && !condition(p))
                p = p.Parent;
            return p;
        }

        public Request Push(Type serviceType, object serviceKey)
        {
            return new Request(serviceType, serviceKey, this);
        }

        public Request PushPreservingKey(Type serviceType)
        {
            return new Request(serviceType, ServiceKey, this);
        }

        public void SetResult(Factory factory, FactoryType factoryType = FactoryType.Service)
        {
            FactoryID = factory.ID;
            FactoryProviderID = factory.ProviderID;
            ImplementationType = factory.ImplementationType;
            FactoryType = factoryType;
            Throw.If(TryGetParent(r => r.FactoryID == FactoryID) != null, Error.DEPENDENCY_CYCLE_DETECTED, this);
        }

        public IEnumerator<Request> GetEnumerator()
        {
            for (var x = this; x != null; x = x.Parent)
                yield return x;
        }

        public string PrintServiceInfo(bool outputIndex = false)
        {
            var message = ServiceType.Print();
            if (ServiceKey is string)
                message += " named \"" + ServiceKey + "\"";
            else if (ServiceKey is int && outputIndex)
                message += " #" + ServiceKey;
            else
                message += " without name";

            if (ImplementationType != null && ImplementationType != ServiceType)
                message = ImplementationType.Print() + " : " + message;

            return message;
        }

        public override string ToString()
        {
            var message = new StringBuilder().Append(PrintServiceInfo(true));
            return Parent == null ? message.ToString()
                : Parent.Aggregate(message, (m, r) => m.AppendLine().Append(" in ").Append(r.PrintServiceInfo(true))).ToString();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public sealed class Scope : IDisposable
    {
        public T GetOrAdd<T>(int id, Func<T> factory)
        {
            Throw.If(_disposed == 1, Error.SCOPE_IS_DISPOSED);
            lock (_syncRoot)
            {
                var item = _items.TryGet(id);
                if (item == null)
                    _items = _items.AddOrUpdate(id, item = factory());
                return (T)item;
            }
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
                return;

            foreach (var item in _items.Select(x => x.Value).OfType<IDisposable>())
                item.Dispose();
            _items = null;
        }

        #region Implementation

        private readonly object _syncRoot = new object();
        private HashTree<object> _items = HashTree<object>.Empty;
        private volatile int _disposed;

        #endregion
    }

    public interface IReuse
    {
        Expression Of(Request request, IRegistry registry, int factoryID, Expression factoryExpr);
    }

    public static class Reuse
    {
        public static readonly IReuse Transient = null; // no reuse.
        public static readonly IReuse Singleton, InCurrentScope, DuringResolution;

        public static readonly ParameterExpression[] Parameters;

        static Reuse()
        {
            Singleton = new SingletonReuse();

            var currentScope = Expression.Parameter(typeof(Scope), "currentScope");
            InCurrentScope = new InScopeReuse(currentScope);

            var resolutionScope = Expression.Parameter(typeof(Scope), "resolutionScope");
            var createScopeOnceMethod = typeof(Reuse).GetMethod("CreateScopeOnce", BindingFlags.NonPublic | BindingFlags.Static);
            DuringResolution = new InScopeReuse(Expression.Call(null, createScopeOnceMethod, (Expression)resolutionScope));

            Parameters = new[] { currentScope, resolutionScope };
        }

        public static Expression GetScopedServiceExpression(Expression scope, int factoryID, Expression factoryExpr)
        {
            return Expression.Call(scope,
                _getOrAddToScopeMethod.MakeGenericMethod(factoryExpr.Type),
                Expression.Constant(factoryID),
                Expression.Lambda(factoryExpr, null));
        }

        #region Implementation

        private static readonly MethodInfo _getOrAddToScopeMethod = typeof(Scope).GetMethod("GetOrAdd");

        // ReSharper disable UnusedMember.Local
        // Used only by reflection
        private static Scope CreateScopeOnce(ref Scope scope)
        {
            return scope ?? (scope = new Scope());
        }

        // ReSharper restore UnusedMember.Local

        private sealed class SingletonReuse : IReuse
        {
            public Expression Of(Request request, IRegistry registry, int factoryID, Expression factoryExpr)
            {
                var singletonScope = registry.SingletonScope;

                // Create lazy scoped singleton if we have Func somewhere in dependency chain. TODO: check that "dependency chain" buzzword appeared.
                var funcParent = request.TryGetParent(r =>
                    r.OpenGenericServiceType != null && Container.FuncTypes.Contains(r.OpenGenericServiceType));
                if (funcParent != null)
                    return GetScopedServiceExpression(Expression.Constant(singletonScope), factoryID, factoryExpr);

                // Otherwise we can create singleton instance right here, and put it into Scope for later disposal.
                var currentScope = registry.CurrentScope;
                // Save into separate var to prevent closure on registry variable in factory lambda below.
                var singletonInstance = singletonScope.GetOrAdd(factoryID,
                    () => Container.CompileExpression(factoryExpr)(currentScope, null));
                return Expression.Constant(singletonInstance, factoryExpr.Type);
            }
        }

        private sealed class InScopeReuse : IReuse
        {
            public InScopeReuse(Expression scope)
            {
                _scope = scope;
            }

            public Expression Of(Request _, IRegistry __, int factoryID, Expression factoryExpr)
            {
                return GetScopedServiceExpression(_scope, factoryID, factoryExpr);
            }

            private readonly Expression _scope;
        }

        #endregion
    }

    public interface IResolver
    {
        object ResolveDefault(Type serviceType, bool shouldReturnNull);

        object ResolveKeyed(Type serviceType, object serviceKey, bool shouldReturnNull);
    }

    public delegate Type SelectGenericTypeArg(Type[] typeArgs);

    public interface IRegistrator
    {
        void Register(Factory factory, Type serviceType, string serviceName);

        void RegisterDecorator(Decorator factory, Type serviceType);

        void RegisterGenericWrapper(Factory factory, Type serviceType, SelectGenericTypeArg getWrappedServiceType);

        bool IsRegistered(Type serviceType, string serviceName);
    }

    public interface IRegistry : IResolver, IRegistrator
    {
        Scope SingletonScope { get; }
        Scope CurrentScope { get; }

        Factory GetOrAddFactory(Request request, bool returnNullOnError);

        Factory TryGetFactory(Type serviceType, object serviceKey);

        IEnumerable<object> GetKeys(Type serviceType, Func<Factory, bool> condition);

        Type GetWrappedServiceTypeOrSelf(Type serviceType);

        object TryGetConstructorParamKey(ParameterInfo parameter, Request parent);

        bool ShouldResolvePropertyOrField { get; }

        bool TryGetPropertyOrFieldKey(out object resultKey, MemberInfo propertyOrField, Request parent);
    }

    public sealed class Meta<TService, TMetadata>
    {
        public readonly TService Value;

        public readonly TMetadata Metadata;

        public Meta(TService service, TMetadata metadata)
        {
            Value = service;
            Metadata = metadata;
        }
    }

#if SYSTEM_LAZY_IS_NOT_AVAILABLE
    [DebuggerStepThrough]
    public sealed class Lazy<T>
    {
        public Lazy(Func<T> valueFactory)
        {
            _valueFactory = valueFactory.ThrowIfNull();
        }

        public bool IsValueCreated
        {
            get { return _isValueCreated; }
        }

        public T Value
        {
            get { return _isValueCreated ? _value : Create(); }
        }

        #region Implementation

        private Func<T> _valueFactory;

        private T _value;

        private readonly object _valueCreationLock = new object();

        private volatile bool _isValueCreated;

        private T Create()
        {
            lock (_valueCreationLock)
            {
                if (!_isValueCreated)
                {
                    var factory = _valueFactory.ThrowIfNull("Recursive creation of Lazy value is detected.");
                    _valueFactory = null;
                    _value = factory();
                    _isValueCreated = true;
                }
            }

            return _value;
        }

        #endregion
    }
#endif

    public class ContainerException : InvalidOperationException
    {
        public ContainerException(string message) : base(message) { }
    }

    public static class Throw
    {
        public static Func<string, Exception> GetException = message => new ContainerException(message);

        public static T ThrowIfNull<T>(this T arg, string message = null, object arg0 = null, object arg1 = null, object arg2 = null) where T : class
        {
            if (arg != null) return arg;
            throw GetException(message == null ? Format(ARG_IS_NULL, typeof(T)) : Format(message, arg0, arg1, arg2));
        }

        public static T ThrowIf<T>(this T arg, bool throwCondition, string message = null, object arg0 = null, object arg1 = null, object arg2 = null)
        {
            if (!throwCondition) return arg;
            throw GetException(message == null ? Format(ARG_HAS_IMVALID_CONDITION, typeof(T)) : Format(message, arg0, arg1, arg2));
        }

        public static void If(bool throwCondition, string message, object arg0 = null, object arg1 = null, object arg2 = null)
        {
            if (!throwCondition) return;
            throw GetException(Format(message, arg0, arg1, arg2));
        }

        #region Implementation

        private static string Format(this string message, object arg0 = null, object arg1 = null, object arg2 = null)
        {
            return string.Format(message, Print(arg0), Print(arg1), Print(arg2));
        }

        private static string Print(object obj)
        {
            return obj == null ? null : obj is Type ? ((Type)obj).Print() : obj.ToString();
        }

        private static readonly string ARG_IS_NULL = "Argument of type {0} is null.";
        private static readonly string ARG_HAS_IMVALID_CONDITION = "Argument of type {0} has invalid condition.";

        #endregion
    }

    public static class Sugar
    {
        public static string Print(this Type type, Func<Type, string> output = null /* prints Type.FullName by default */)
        {
            var name = output == null ? type.FullName : output(type);
            if (type.IsGenericType) // for generic types
            {
                var genericArgs = type.GetGenericArguments();
                var genericArgsString = type.IsGenericTypeDefinition
                    ? new string(',', genericArgs.Length - 1)
                    : String.Join(", ", genericArgs.Select(x => x.Print(output)).ToArray());
                name = name.Substring(0, name.IndexOf('`')) + "<" + genericArgsString + ">";
            }
            return name.Replace('+', '.'); // for nested classes
        }

        public static Type[] GetSelfAndImplemented(this Type type)
        {
            Type[] results;

            var interfaces = type.GetInterfaces();
            var selfPlusInterfaceCount = 1 + interfaces.Length;

            var baseType = type.BaseType;
            if (baseType == null || baseType == typeof(object))
                results = new Type[selfPlusInterfaceCount];
            else
            {
                List<Type> baseBaseTypes = null;
                for (var bb = baseType.BaseType; bb != null && bb != typeof(object); bb = bb.BaseType)
                    (baseBaseTypes ?? (baseBaseTypes = new List<Type>(2))).Add(bb);

                if (baseBaseTypes == null)
                    results = new Type[selfPlusInterfaceCount + 1];
                else
                {
                    results = new Type[selfPlusInterfaceCount + 1 + baseBaseTypes.Count];
                    baseBaseTypes.CopyTo(results, selfPlusInterfaceCount + 1);
                }

                results[selfPlusInterfaceCount] = baseType;
            }

            results[0] = type;

            if (selfPlusInterfaceCount == 2)
                results[1] = interfaces[0];
            else if (selfPlusInterfaceCount > 2)
                Array.Copy(interfaces, 0, results, 1, interfaces.Length);

            if (results.Length > 1 && type.IsGenericTypeDefinition)
            {
                for (var i = 1; i < results.Length; i++)
                {
                    var interfaceOrBase = results[i];
                    if (interfaceOrBase.IsGenericType && !interfaceOrBase.IsGenericTypeDefinition &&
                        interfaceOrBase.ContainsGenericParameters)
                        results[i] = interfaceOrBase.GetGenericTypeDefinition();
                }
            }

            return results;
        }

        public static V GetOrAdd<K, V>(this IDictionary<K, V> source, K key, Func<K, V> valueFactory)
        {
            V value;
            if (!source.TryGetValue(key, out value))
                source.Add(key, value = valueFactory(key));
            return value;
        }

        public static T[] AddOrUpdateCopy<T>(this T[] source, T value, int index = -1)
        {
            var sourceLength = source.Length;
            index = index < 0 ? sourceLength : index;
            var target = new T[index < sourceLength ? sourceLength : sourceLength + 1];
            Array.Copy(source, target, sourceLength);
            target[index] = value;
            return target;
        }
    }

    public sealed class HashTree<V> : IEnumerable<HashTree<V>>
    {
        public static readonly HashTree<V> Empty = new HashTree<V>();
        public bool IsEmpty { get { return Height == 0; } }

        public readonly HashTree<V> Left, Right;
        public readonly int Height;
        public readonly int Key;
        public readonly V Value;

        public delegate V UpdateValue(V old, V added);

        public HashTree<V> AddOrUpdate(int key, V newValue, UpdateValue update = null)
        {
            return Height == 0 ? new HashTree<V>(key, newValue, Empty, Empty)
                : (key == Key ? new HashTree<V>(key, update == null ? newValue : update(Value, newValue), Left, Right)
                    : (key < Key
                        ? With(Left.AddOrUpdate(key, newValue), Right)
                        : With(Left, Right.AddOrUpdate(key, newValue))).EnsureBalanced());
        }

        public V TryGet(int key, V defaultValue = default(V))
        {
            var current = this;
            while (current.Height != 0 && key != current.Key)
                current = key < current.Key ? current.Left : current.Right;
            return current.Height != 0 ? current.Value : defaultValue;
        }

        public IEnumerator<HashTree<V>> GetEnumerator()
        {
            var leftNodes = Stack<HashTree<V>>.Empty;
            for (var node = this; !node.IsEmpty || !leftNodes.IsEmpty; node = node.Right) // Go right from last returned left node.
            {
                for (; !node.IsEmpty; node = node.Left)  // Go left until leaf
                    leftNodes = leftNodes.Push(node);    // and collect nodes for later.
                node = leftNodes.Head;
                leftNodes = leftNodes.Tail;
                yield return node;                       // Return node from last collected left node.
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region Implementation

        private HashTree() { }

        private HashTree(int key, V value, HashTree<V> left, HashTree<V> right)
        {
            Key = key;
            Value = value;
            Left = left;
            Right = right;
            Height = 1 + (left.Height > right.Height ? left.Height : right.Height);
        }

        private HashTree<V> EnsureBalanced()
        {
            var delta = Left.Height - Right.Height;
            return delta >= 2 ? With(Left.Right.Height - Left.Left.Height == 1 ? Left.RotateLeft() : Left, Right).RotateRight()
                : (delta <= -2 ? With(Left, Right.Left.Height - Right.Right.Height == 1 ? Right.RotateRight() : Right).RotateLeft()
                : this);
        }

        private HashTree<V> RotateRight()
        {
            return Left.With(Left.Left, With(Left.Right, Right));
        }

        private HashTree<V> RotateLeft()
        {
            return Right.With(With(Left, Right.Left), Right.Right);
        }

        private HashTree<V> With(HashTree<V> left, HashTree<V> right)
        {
            return new HashTree<V>(Key, Value, left, right);
        }

        #endregion
    }

    public sealed class HashTree<K, V> : IEnumerable<HashTree<K, V>>
    {
        public static readonly HashTree<K, V> Empty = new HashTree<K, V>(HashTree<KV>.Empty);

        public HashTree<K, V> AddOrUpdate(K key, V value)
        {
            return new HashTree<K, V>(_tree.AddOrUpdate(key.GetHashCode(), new KV { Key = key, Value = value }, Update));
        }

        public V TryGet(K key)
        {
            var item = _tree.TryGet(key.GetHashCode());
            return item != null && (ReferenceEquals(key, item.Key) || key.Equals(item.Key)) ? item.Value : TryGetConflicted(item, key);
        }

        public IEnumerator<HashTree<K, V>> GetEnumerator()
        {
            return _tree.Select(t => new HashTree<K, V>(t)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region Implementation

        private readonly HashTree<KV> _tree;

        private HashTree(HashTree<KV> tree)
        {
            _tree = tree;
        }

        private static KV Update(KV old, KV added)
        {
            var conflicts = old is KVWithConflicts ? ((KVWithConflicts)old).Conflicts : null;

            if (ReferenceEquals(old.Key, added.Key) || old.Key.Equals(added.Key))
                return conflicts == null ? added
                    : new KVWithConflicts { Key = added.Key, Value = added.Value, Conflicts = conflicts };

            var newConflicts = conflicts == null ? new[] { added }
                : conflicts.AddOrUpdateCopy(added, Array.FindIndex(conflicts, x => Equals(x.Key, added.Key)));

            return new KVWithConflicts { Key = old.Key, Value = old.Value, Conflicts = newConflicts };
        }

        private static V TryGetConflicted(KV item, K key)
        {
            var conflicts = item is KVWithConflicts ? ((KVWithConflicts)item).Conflicts : null;
            if (conflicts != null)
                for (var i = 0; i < conflicts.Length; i++)
                    if (Equals(conflicts[i].Key, key))
                        return conflicts[i].Value;
            return default(V);
        }

        private class KV
        {
            public K Key;
            public V Value;
        }

        private sealed class KVWithConflicts : KV
        {
            public KV[] Conflicts;
        }

        #endregion
    }

    public sealed class Stack<T>
    {
        public static readonly Stack<T> Empty = new Stack<T>(default(T), null);
        public bool IsEmpty { get { return Tail == null; } }

        public readonly T Head;
        public readonly Stack<T> Tail;

        public Stack<T> Push(T tree)
        {
            return new Stack<T>(tree, this);
        }

        private Stack(T head, Stack<T> tail)
        {
            Head = head;
            Tail = tail;
        }
    }
}

#pragma warning restore 420