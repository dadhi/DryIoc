// TODO:
// For version 1.0.0
// - Add condition to Decorator.
// - Consolidate code related to rules in Setup class.
// - Adjust ResolveProperties to be consistent with Import property or field.
// - Convert SkipCache flag to enum.
// - Evaluate Code Coverage.
// - Fix code generation example.
// - Review performance one again with fresh view.
// + Add registration for lambda with IResolver parameter for something like RegisterDelegate(c => new Blah(c.Resolve(IDependency))).

// Goals:
// - Finalize Public API.
// + Add distinctive features:. ImportUsing.
//
// Features:
// - Add service Target (ctor parameter or fieldProperty) to request to show them in error: "Unable to resolve constructor parameter/field/property service".
// - Include properties Func with arguments support. What properties should be included: only marked for container resolution or all settable?
// - Add distinctive features: Export DelegateFactory<TService>.
// - Make Request to return Empty for resolution root parent. So it will simplify ImplementationType checks. May be add IsResolutionRoot property as well.
// - Make a single consistent approach to ResolveProperties and PropertyOrFieldResolutionRules.
// - Move Container Setup related code to dedicated class/container-property Setup.
// - Add parameter to Resolve to skip resolution cache, and probably all other caches.
// - Rename ExportPublicTypes to AutoExport or something more concise.
// + Decorator support for Func<..> service, may be supported if implement Decorator the same way as Reuse or Init - as Expression Decorator.
//
// Internals:
// - Speedup:
// - # Replace HashTrees in RegistryEntry with arrays.
// + # Test speed of removing return value from HashTree.TryGet. - No difference.
// + # Make Type specific HashTree with reference equality comparison. - No difference with Generic HashTree.
// + # Replace Stack in HashTree enumeration with array. - Got 5/6 speed improvement.
// - Remake Request ResolveTo to not mutate Request.
// - Automatically propagate Setup on Factory.TryGetFactoryFor(Request ...).
// - Rename request to DependencyChain.
// + Make Decorator caching work without SkipCache=true;
// + Remove Container Singleton parameter from CompiledFactory.

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

        public Container() : this(false) { }

        public Container(bool coreOnly)
        {
            _syncRoot = new object();
            _registry = new Dictionary<Type, RegistryEntry>();
            _keyedResolutionCache = HashTree<Type, KeyedResolutionCacheEntry>.Empty;
            _defaultResolutionCache = HashTree<Type, CompiledFactory>.Empty;

            Setup = new Setup();
            CurrentScope = SingletonScope = new Scope();

            if (!coreOnly) // TODO: Default setup, May I just pass custom setup instead flag?
            {
                Setup.AddNonRegisteredServiceResolutionRule(TryResolveEnumerableOrArray);

                var funcFactory = new FuncGenericWrapper();
                foreach (var funcType in FuncTypes)
                    this.Register(funcType, funcFactory);

                this.Register(typeof(Lazy<>), Reuse.Transient, t => t.GetConstructor(new[] { typeof(Func<>) }), FactorySetup.AsGenericWrapper());

                this.Register(typeof(Meta<,>), new CustomFactoryProvider(TryResolveMeta, FactorySetup.AsGenericWrapper(t => t[0])));

                this.Register(typeof(FactoryExpression<>), new CustomFactoryProvider(TryResolveFactoryExpression, FactorySetup.AsGenericWrapper()));
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
                // Service.
                RegistryEntry entry;
                if (_registry.TryGetValue(request.ServiceType, out entry) &&
                    entry.TryGetFactory(out result, request.ServiceType, request.ServiceKey))
                {
                    result = result.TryProvideFactoryFor(request, this) ?? result;
                    request.ResolveTo(result);
                    return result;
                }

                // Open-generic Service.
                RegistryEntry openGenericEntry = null;
                if (request.OpenGenericServiceType != null &&
                    _registry.TryGetValue(request.OpenGenericServiceType, out openGenericEntry) &&
                    openGenericEntry.TryGetFactory(out result, request.ServiceType, request.ServiceKey) &&
                    (result = result.TryProvideFactoryFor(request, this)) != null)
                {
                    request.ResolveTo(result);
                    Register(result, request.ServiceType, request.ServiceKey);
                    return result;
                }

                // Generic wrapper.
                if (openGenericEntry != null && request.ServiceKey != null && // if null it means we are already looked for it above.
                    openGenericEntry.TryGetFactory(out result, request.ServiceType, null) &&
                    result.Setup.Type == FactoryType.GenericWrapper &&
                    (result = result.TryProvideFactoryFor(request, this)) != null)
                {
                    request.ResolveTo(result);
                    Register(result, request.ServiceType, request.ServiceKey);
                    return result;
                }
            }

            var rules = Setup.NonRegisteredServiceResolutionRules;
            for (var i = 0; i < rules.Length; i++)
            {
                result = rules[i].Invoke(request, this);
                if (result != null)
                {
                    request.ResolveTo(result);
                    Register(result, request.ServiceType, request.ServiceKey);
                    return result;
                }
            }

            Throw.If(!shouldReturnNull, Error.UNABLE_TO_RESOLVE_SERVICE, request, request.PrintServiceInfo());
            return null;
        }

        LambdaExpression IRegistry.TryGetDecoratorFuncExpression(Request request, out bool isDecoratedServiceIgnored)
        {
            isDecoratedServiceIgnored = false;

            // Decorators for non service types are not supported.
            if (request.FactoryType != FactoryType.Service)
                return null;

            // We are already resolving decorator for the service, so stop now.
            var parent = request.TryGetNonWrapperParent();
            if (parent != null && parent.DecoratedFactoryID == request.FactoryID)
                return null;

            var serviceType = request.ServiceType;
            var decoratorFuncType = typeof(Func<,>).MakeGenericType(serviceType, serviceType);
            
            LambdaExpression resultFuncExpr = null;

            lock (_syncRoot)
            {
                RegistryEntry entry;
                if (_registry.TryGetValue(decoratorFuncType, out entry) && entry.Decorators != null)
                {
                    var decoratorRequest = new Request(decoratorFuncType, request.ServiceKey, request.Parent);
                    for (var i = 0; i < entry.Decorators.Count; i++)
                    {
                        var decorator = entry.Decorators[i];
                        if (((FactorySetup.Decorator)decorator.Setup).IsApplicable(request))
                        {
                            decoratorRequest.ResolveTo(decorator);
                            var funcObjectExpr = decorator.GetExpression(decoratorRequest, this);
                            if (resultFuncExpr == null)
                            {
                                var decoratedParamExpr = Expression.Parameter(serviceType, "decorated");
                                resultFuncExpr = Expression.Lambda(Expression.Invoke(funcObjectExpr, decoratedParamExpr), decoratedParamExpr);
                            }
                            else
                            {
                                resultFuncExpr = Expression.Lambda(Expression.Invoke(funcObjectExpr, resultFuncExpr.Body), resultFuncExpr.Parameters[0]);
                            }
                        }
                    }
                }

                var serviceDecorators = new List<Factory>();

                if (_registry.TryGetValue(serviceType, out entry) && entry.Decorators != null)
                {
                    serviceDecorators.AddRange(entry.Decorators
                        .Where(f => ((FactorySetup.Decorator)f.Setup).IsApplicable(request)));
                }

                if (request.OpenGenericServiceType != null &&
                    _registry.TryGetValue(request.OpenGenericServiceType, out entry) && entry.Decorators != null)
                {
                    serviceDecorators.AddRange(entry.Decorators
                        .Where(f => ((FactorySetup.Decorator)f.Setup).IsApplicable(request))
                        .Select(f => f.TryProvideFactoryFor(request, this)));
                }

                if (serviceDecorators.Count != 0)
                {
                    var decoratorRequest = new Request(request.ServiceType, request.ServiceKey, request.Parent);
                    for (var i = 0; i < serviceDecorators.Count; i++)
                    {
                        var decorator = serviceDecorators[i];
                        decoratorRequest.ResolveTo(decorator, request.FactoryID);
                        var decoratorSetup = ((FactorySetup.Decorator)decorator.Setup);
                        var funcExpr = decoratorSetup.CachedDecoratorFuncExpr;
                        if (funcExpr == null)
                        {
                            IList<Type> unusedFuncParams;
                            funcExpr = decorator.TryCreateFuncWithArgsExpression(decoratorFuncType, decoratorRequest, this, out unusedFuncParams);
                            decoratorSetup.CachedDecoratorFuncExpr = funcExpr;
                            decoratorSetup.IsDecoratedServiceIgnored = unusedFuncParams != null;
                        }

                        resultFuncExpr = resultFuncExpr == null ? funcExpr
                            : Expression.Lambda(Expression.Invoke(funcExpr, resultFuncExpr.Body), resultFuncExpr.Parameters[0]);

                        // Once ignored, decorated service should stay ignored.
                        isDecoratedServiceIgnored = !isDecoratedServiceIgnored && decoratorSetup.IsDecoratedServiceIgnored;
                    }
                }
            }

            return resultFuncExpr;
        }

        IEnumerable<object> IRegistry.GetKeys(Type serviceType, Func<Factory, bool> condition)
        {
            lock (_syncRoot)
            {
                RegistryEntry entry;
                if (TryFindEntry(out entry, serviceType))
                {
                    if (entry.Indexed != null)
                    {
                        foreach (var item in entry.Indexed)
                            if (condition == null || condition(item.Value))
                                yield return item.Key;
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
                    entry.TryGetFactory(out factory, serviceType, serviceKey))
                    return factory;
                return null;
            }
        }

        Type IRegistry.GetWrappedServiceTypeOrSelf(Type serviceType)
        {
            if (!serviceType.IsGenericType)
                return serviceType;

            var factory = ((IRegistry)this).TryGetFactory(serviceType.GetGenericTypeDefinition(), null);
            if (factory == null || factory.Setup.Type != FactoryType.GenericWrapper)
                return serviceType;

            var wrapperSetup = ((FactorySetup.GenericWrapper)factory.Setup);
            var wrappedType = wrapperSetup.GetWrappedServiceType(serviceType.GetGenericArguments());
            return wrappedType == serviceType ? serviceType
                : ((IRegistry)this).GetWrappedServiceTypeOrSelf(wrappedType); // unwrap recursively.
        }

        object IRegistry.TryGetConstructorParamKey(ParameterInfo parameter, Request parent)
        {
            if (parent.FactoryType == FactoryType.GenericWrapper ||
                parent.FactoryType == FactoryType.Decorator)
                return parent.ServiceKey; // propagate key from wrapper or decorator.

            object resultKey = null;
            var rules = Setup.ConstructorParamServiceKeyResolutionRules;
            if (rules != null)
                for (var i = 0; i < rules.Length && resultKey == null; i++)
                    resultKey = rules[i].Invoke(parameter, parent, this);
            return resultKey;
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

        private static void ThrowUnexpectedMultipleDefaults(Factory[] factories, Type serviceType)
        {
            var implementations = factories.Select(x => x.ImplementationType).Aggregate(
                new StringBuilder().AppendLine(),
                (i, t) => i.AppendLine(t != null ? t.Print() + ";" : "[not provided]"));
            Throw.If(true, Error.EXPECTED_SINGLE_DEFAULT_FACTORY, serviceType, factories.Length, implementations);
        }

        #endregion

        #region IRegistrator

        public void Register(Factory factory, Type serviceType, object serviceKey)
        {
            ThrowIfNotImplemented(serviceType.ThrowIfNull(), factory.ThrowIfNull().ImplementationType);
            lock (_syncRoot)
            {
                var entry = _registry.GetOrAdd(serviceType, _ => new RegistryEntry());

                if (factory.Setup.Type == FactoryType.Decorator)
                {
                    entry.Decorators = entry.Decorators ?? new List<Factory>();
                    entry.Decorators.Add(factory);
                    return;
                }

                if (serviceKey == null)
                {
                    if (entry.LastDefault != null)
                    {
                        entry.Indexed = entry.Indexed ?? HashTree<Factory>.Empty.AddOrUpdate(entry.MaxIndex++, entry.LastDefault);
                        entry.Indexed = entry.Indexed.AddOrUpdate(entry.MaxIndex++, factory);
                    }
                    entry.LastDefault = factory;
                }
                else if (serviceKey is int)
                {
                    var index = (int)serviceKey;
                    entry.Indexed = (entry.Indexed ?? HashTree<Factory>.Empty).AddOrUpdate(index, factory);
                    entry.MaxIndex = Math.Max(entry.MaxIndex, index) + 1;
                }
                else if (serviceKey is string)
                {
                    var name = serviceKey.ToString();
                    entry.Named = entry.Named ?? new Dictionary<string, Factory>();
                    try
                    {
                        entry.Named.Add(name, factory);
                    }
                    catch (ArgumentException)
                    {
                        var implType = entry.Named[name].ImplementationType;
                        Throw.If(true, Error.DUPLICATE_SERVICE_NAME_REGISTRATION,
                            serviceType, serviceKey, implType != null ? implType.Print() : "<custom>");
                    }
                }
            }
        }

        public bool IsRegistered(Type serviceType, string serviceName)
        {
            return ((IRegistry)this).TryGetFactory(serviceType.ThrowIfNull(), serviceName) != null;
        }

        private static void ThrowIfNotImplemented(Type serviceType, Type implementationType)
        {
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

            return new DelegateFactory((_, __) =>
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

            return new DelegateFactory((request, registry) =>
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
                setup: FactorySetup.With(skipCache: true));
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

            return new DelegateFactory((_, __) => newFactory);
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
            _keyedResolutionCache = source._keyedResolutionCache;
            _defaultResolutionCache = source._defaultResolutionCache;
        }

        private readonly object _syncRoot;
        private readonly Dictionary<Type, RegistryEntry> _registry;

        private sealed class RegistryEntry
        {
            public Factory LastDefault;
            public HashTree<Factory> Indexed;
            public int MaxIndex;
            public Dictionary<string, Factory> Named;
            public List<Factory> Decorators;

            public bool TryGetFactory(out Factory result, Type serviceType, object serviceKey)
            {
                result = null;
                if (serviceKey == null)
                {
                    if (Indexed != null)
                        ThrowUnexpectedMultipleDefaults(Indexed.Select(x => x.Value).ToArray(), serviceType);
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
                        if (Indexed == null && index == 0)
                            result = LastDefault;
                        else if (Indexed != null)
                            result = Indexed.TryGet(index);
                    }
                }

                return result != null;
            }
        }

        #endregion
    }

    public class FuncGenericWrapper : Factory
    {
        public FuncGenericWrapper()
            : base(setup: FactorySetup.AsGenericWrapper(types => types[types.Length - 1])) { }

        public override Factory TryProvideFactoryFor(Request request, IRegistry registry)
        {
            return new DelegateFactory(CreateFunc, DryIoc.Reuse.Singleton, Setup);
        }

        protected override Expression CreateExpression(Request request, IRegistry registry)
        {
            throw new NotSupportedException();
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

            IList<Type> unusedFuncParams;
            var funcExpr = serviceFactory
                .TryCreateFuncWithArgsExpression(funcType, serviceRequest, registry, out unusedFuncParams)
                .ThrowIfNull(Error.UNSUPPORTED_FUNC_WITH_ARGS, funcType);

            if (unusedFuncParams != null)
                Throw.If(true, Error.SOME_FUNC_PARAMS_ARE_UNUSED, unusedFuncParams.Print(), request);

            if (serviceFactory.Reuse != null)
            {
                var serviceExpr = serviceFactory.Reuse.Of(serviceRequest, registry, serviceFactory.ID, funcExpr.Body);
                funcExpr = Expression.Lambda(funcType, serviceExpr, funcExpr.Parameters);
            }

            bool isDecoratedServiceIgnored; // TODO: When it could be set to true?
            var decoratorExpr = registry.TryGetDecoratorFuncExpression(serviceRequest, out isDecoratedServiceIgnored);
            if (decoratorExpr != null)
            {
                if (isDecoratedServiceIgnored)
                {

                }
                funcExpr = Expression.Lambda(funcType, Expression.Invoke(decoratorExpr, funcExpr.Body), funcExpr.Parameters);
            }

            return funcExpr;
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

    public static partial class Error
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

        public static readonly string CONSTRUCTOR_MISSES_SOME_PARAMETERS =
            "Constructor [{0}] of {1} misses some arguments required for {2} dependency.";

        public static readonly string UNABLE_TO_SELECT_CONSTRUCTOR =
            "Unable to select single constructor from {0} available in {1}. " + Environment.NewLine +
            "Please provide constructor selector when registering service.";

        public static readonly string EXPECTED_FUNC_WITH_MULTIPLE_ARGS =
            "Expecting Func with one or more arguments but found {0}.";

        public static readonly string EXPECTED_CLOSED_GENERIC_SERVICE_TYPE =
            "Expecting closed-generic service type but found {0}.";

        public static readonly string RECURSIVE_DEPENDENCY_DETECTED =
            "Recursive dependency is detected in resolution of:\n{0}.";

        public static readonly string SCOPE_IS_DISPOSED =
            "Scope is disposed and all in-scope instances are no longer available.";

        public static readonly string CONTAINER_IS_GARBAGE_COLLECTED =
            "Container is no longer available (has been garbage-collected already).";

        public static readonly string DUPLICATE_SERVICE_NAME_REGISTRATION =
            "Service {0} with duplicate name '{1}' is already registered with implementation {2}.";

        public static readonly string GENERIC_WRAPPER_EXPECTS_SINGLE_TYPE_ARG_BY_DEFAULT =
            "Generic Wrapper expects single type argument by default, but found many: {0}.";

        public static string SOME_FUNC_PARAMS_ARE_UNUSED =
            "Found some unused Func parameters ({0}) when resolving: {1}.";
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
            Type implementationType, IReuse reuse = null, SelectConstructor withConstructor = null, FactorySetup setup = null,
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
            Type implementationType, IReuse reuse = null, SelectConstructor withConstructor = null, FactorySetup setup = null,
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
            IReuse reuse = null, SelectConstructor withConstructor = null, FactorySetup setup = null,
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
            IReuse reuse = null, SelectConstructor withConstructor = null, FactorySetup setup = null,
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
            Type implementationType, IReuse reuse = null, SelectConstructor withConstructor = null, FactorySetup setup = null,
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
            IReuse reuse = null, SelectConstructor withConstructor = null, FactorySetup setup = null,
            string named = null)
        {
            registrator.RegisterPublicTypes(typeof(TImplementation), reuse, withConstructor, setup, named);
        }

        /// <summary>
        /// Registers a factory delegate for creating an instance of <typeparamref name="TService"/>.
        /// Delegate can use <see cref="IResolver"/> parameter to resolve any required dependencies, e.g.:
        /// <code>RegisterDelegate&lt;ICar&gt;(r => new Car(r.Resolve&lt;IEngine&gt;()))</code>
        /// </summary>
        /// <typeparam name="TService">The type of service.</typeparam>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="lambda">The delegate used to create a instance of <typeparamref name="TService"/>.</param>
        /// <param name="reuse">Optional <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="setup">Optional factory setup, by default is (<see cref="FactorySetup.Service"/>)</param>
        /// <param name="named">Optional service name.</param>
        public static void RegisterDelegate<TService>(this IRegistrator registrator,
            Func<IResolver, TService> lambda, IReuse reuse = null, FactorySetup setup = null,
            string named = null)
        {
            var factory = new DelegateFactory(
                (_, resolver) => Expression.Invoke(Expression.Constant(lambda), Expression.Constant(resolver, typeof(IResolver))),
                reuse, setup);
            registrator.Register(factory, typeof(TService), named);
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
            registrator.RegisterDelegate(_ => instance, Reuse.Transient, setup, named);
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

    public enum FactoryType { Service, Decorator, GenericWrapper };

    public delegate Expression Init(Expression source);

    public abstract class FactorySetup
    {
        public abstract FactoryType Type { get; }
        public virtual bool SkipCache { get { return false; } }
        public virtual object Metadata { get { return null; } }

        public static FactorySetup WithMetadata(object metadata = null)
        {
            return metadata == null ? Service.Default : new Service(metadata: metadata);
        }

        public static FactorySetup With(Init init = null, bool skipCache = false, object metadata = null)
        {
            return new Service(skipCache, metadata);
        }

        public static FactorySetup AsGenericWrapper(Func<Type[], Type> selectServiceType = null)
        {
            return selectServiceType == null ? GenericWrapper.Default : new GenericWrapper(selectServiceType);
        }

        public static FactorySetup AsDecorator(Func<Request, bool> isApplicable = null)
        {
            return new Decorator(isApplicable);
        }

        public class Service : FactorySetup
        {
            public static readonly FactorySetup Default = new Service();

            public override FactoryType Type { get { return FactoryType.Service; } }
            public override bool SkipCache { get { return _skipCache; } }
            public override object Metadata { get { return _metadata; } }

            internal Service(bool skipCache = false, object metadata = null)
            {
                _skipCache = skipCache;
                _metadata = metadata;
            }

            private readonly bool _skipCache;
            private readonly object _metadata;
        }

        public class GenericWrapper : FactorySetup
        {
            public static readonly FactorySetup Default = new GenericWrapper();

            public override FactoryType Type { get { return FactoryType.GenericWrapper; } }
            public readonly Func<Type[], Type> GetWrappedServiceType;

            internal GenericWrapper(Func<Type[], Type> selectServiceTypeFromGenericArgs = null)
            {
                GetWrappedServiceType = selectServiceTypeFromGenericArgs ?? SelectSingleByDefault;
            }

            private static Type SelectSingleByDefault(Type[] typeArgs)
            {
                Throw.If(typeArgs.Length != 1, Error.GENERIC_WRAPPER_EXPECTS_SINGLE_TYPE_ARG_BY_DEFAULT, typeArgs.Print());
                return typeArgs[0];
            }
        }

        public class Decorator : FactorySetup
        {
            public override FactoryType Type { get { return FactoryType.Decorator; } }
            public override bool SkipCache { get { return true; } }

            public readonly Func<Request, bool> IsApplicable;
            public LambdaExpression CachedDecoratorFuncExpr;
            public bool IsDecoratedServiceIgnored;

            public Decorator(Func<Request, bool> isApplicable = null)
            {
                IsApplicable = isApplicable ?? IsApplicableByDefault;
            }

            private static bool IsApplicableByDefault(Request _)
            {
                return true;
            }
        }
    }

    public abstract class Factory
    {
        public static volatile int IDSeedAndCount;

        public readonly int ID;
        public readonly IReuse Reuse;
        public readonly FactorySetup Setup;

        public virtual Type ImplementationType { get { return null; } }

        protected Factory(IReuse reuse = null, FactorySetup setup = null)
        {
            ID = Interlocked.Increment(ref IDSeedAndCount);
            Reuse = reuse;
            Setup = setup ?? FactorySetup.Service.Default;
        }

        public abstract Factory TryProvideFactoryFor(Request request, IRegistry registry);

        public Expression GetExpression(Request request, IRegistry registry)
        {
            if (!Setup.SkipCache && _cachedExpression != null)
                return _cachedExpression;

            Expression result = null;

            bool isDecoratedServiceIgnored;
            var decorator = registry.TryGetDecoratorFuncExpression(request, out isDecoratedServiceIgnored);
            if (decorator == null || !isDecoratedServiceIgnored)
            {
                // Normal expression creation pipeline, not affected by decorator.
                result = CreateExpression(request, registry);
                if (Reuse != null)
                    result = Reuse.Of(request, registry, ID, result);
                if (!Setup.SkipCache)
                    Interlocked.CompareExchange(ref _cachedExpression, result, null);
            }

            if (decorator != null)
                result = Expression.Invoke(decorator, result ?? request.ServiceType.GetDefaultExpression());

            return result;
        }

        protected abstract Expression CreateExpression(Request request, IRegistry registry);

        public virtual LambdaExpression TryCreateFuncWithArgsExpression(Type funcType, Request request, IRegistry registry, out IList<Type> unusedFuncParams)
        {
            unusedFuncParams = null;
            return null;
        }

        #region Implementation

        private volatile Expression _cachedExpression;

        #endregion
    }

    public delegate ConstructorInfo SelectConstructor(Type implementationType);

    public sealed class ReflectionFactory : Factory
    {
        public override Type ImplementationType { get { return _implementationType; } }

        public ReflectionFactory(Type implementationType, IReuse reuse = null, SelectConstructor withConstructor = null, FactorySetup setup = null)
            : base(reuse, setup)
        {
            _implementationType = implementationType.ThrowIfNull()
                .ThrowIf(implementationType.IsAbstract, Error.EXPECTED_NON_ABSTRACT_IMPL_TYPE, implementationType);
            _withConstructor = withConstructor;
        }

        public override Factory TryProvideFactoryFor(Request request, IRegistry _)
        {
            if (!_implementationType.IsGenericTypeDefinition) return null;
            var closedImplType = _implementationType.MakeGenericType(request.ServiceType.GetGenericArguments());
            return new ReflectionFactory(closedImplType, Reuse, _withConstructor, Setup);
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

            return AddInitializerIfRequired(Expression.New(ctor, paramExprs), request, registry);
        }

        public override LambdaExpression TryCreateFuncWithArgsExpression(Type funcType, Request request, IRegistry registry, out IList<Type> unusedFuncParams)
        {
            var funcParamTypes = funcType.GetGenericArguments();
            funcParamTypes.ThrowIf(funcParamTypes.Length == 1, Error.EXPECTED_FUNC_WITH_MULTIPLE_ARGS, funcType);

            var ctor = SelectConstructor();
            var ctorParams = ctor.GetParameters();
            var ctorParamExprs = new Expression[ctorParams.Length];
            var funcInputParamExprs = new ParameterExpression[funcParamTypes.Length - 1]; // (minus Func return parameter).

            for (var cp = 0; cp < ctorParams.Length; cp++)
            {
                var ctorParam = ctorParams[cp];
                for (var fp = 0; fp < funcParamTypes.Length - 1; fp++)
                {
                    var funcParamType = funcParamTypes[fp];
                    if (ctorParam.ParameterType == funcParamType &&
                        funcInputParamExprs[fp] == null) // Skip if Func parameter was already used for constructor.
                    {
                        ctorParamExprs[cp] = funcInputParamExprs[fp] = Expression.Parameter(funcParamType, ctorParam.Name);
                        break;
                    }
                }

                if (ctorParamExprs[cp] == null) // If no matching constructor parameter found in Func, resolve it from Container.
                {
                    var paramKey = registry.TryGetConstructorParamKey(ctorParam, request);
                    var paramRequest = request.Push(ctorParam.ParameterType, paramKey);
                    ctorParamExprs[cp] = registry.GetOrAddFactory(paramRequest, false).GetExpression(paramRequest, registry);
                }
            }

            // Find unused Func parameters (present in Func but not in constructor) and create "_" (ignored) Parameter expressions for them.
            // In addition store unused parameter in output list for client review.
            unusedFuncParams = null;
            for (var fp = 0; fp < funcInputParamExprs.Length; fp++)
            {
                if (funcInputParamExprs[fp] == null) // unused parameter
                {
                    if (unusedFuncParams == null) unusedFuncParams = new List<Type>(2);
                    var funcParamType = funcParamTypes[fp];
                    unusedFuncParams.Add(funcParamType);
                    funcInputParamExprs[fp] = Expression.Parameter(funcParamType, "_");
                }
            }

            var newExpr = Expression.New(ctor, ctorParamExprs);
            return Expression.Lambda(funcType, AddInitializerIfRequired(newExpr, request, registry), funcInputParamExprs);
        }

        #region Implementation

        private readonly Type _implementationType;

        private readonly SelectConstructor _withConstructor;

        private ConstructorInfo SelectConstructor()
        {
            var constructors = _implementationType.GetConstructors();
            if (constructors.Length == 1)
                return constructors[0];

            Throw.If(constructors.Length == 0, Error.NO_PUBLIC_CONSTRUCTOR_DEFINED, _implementationType);
            _withConstructor.ThrowIfNull(Error.UNABLE_TO_SELECT_CONSTRUCTOR, constructors.Length, _implementationType);
            return _withConstructor(_implementationType);
        }

        private Expression AddInitializerIfRequired(NewExpression newService, Request request, IRegistry registry)
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

    public class DelegateFactory : Factory
    {
        public DelegateFactory(Func<Request, IRegistry, Expression> getExpression, IReuse reuse = null, FactorySetup setup = null)
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
        public CustomFactoryProvider(TryGetFactory tryGetFactory, FactorySetup setup = null)
            : base(setup: setup)
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

    public sealed class Request : IEnumerable<Request>
    {
        public readonly Request Parent; // can be null for resolution root
        public readonly Type ServiceType;
        public readonly Type OpenGenericServiceType;
        public readonly object ServiceKey; // null for default, string for named or integer index for multiple defaults.

        public FactoryType FactoryType { get; private set; }
        public int FactoryID { get; private set; }
        public int DecoratedFactoryID { private set; get; }
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

        public void ResolveTo(Factory factory, int decoratedFactoryID = -1)
        {
            FactoryType = factory.Setup.Type;
            FactoryID = factory.ID;
            DecoratedFactoryID = decoratedFactoryID;
            ImplementationType = factory.ImplementationType;
            Throw.If(TryGetParent(r => r.FactoryID == FactoryID) != null, Error.RECURSIVE_DEPENDENCY_DETECTED, this);
        }

        public IEnumerator<Request> GetEnumerator()
        {
            for (var x = this; x != null; x = x.Parent)
                yield return x;
        }

        public string PrintServiceInfo(bool showIndex = false)
        {
            var message = ServiceType.Print();

            message += ServiceKey is string ? " (\"" + ServiceKey + "\")"
                : showIndex && ServiceKey is int ? " (#" + ServiceKey + ")"
                : " (unnamed)";

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

                // Create lazy scoped singleton if we have Func somewhere in dependency chain.
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

    public interface IRegistrator
    {
        void Register(Factory factory, Type serviceType, object serviceKey);

        bool IsRegistered(Type serviceType, string serviceName);
    }

    public interface IRegistry : IResolver, IRegistrator
    {
        Scope SingletonScope { get; }
        Scope CurrentScope { get; }

        Factory GetOrAddFactory(Request request, bool returnNullOnError);

        Factory TryGetFactory(Type serviceType, object serviceKey);

        LambdaExpression TryGetDecoratorFuncExpression(Request request, out bool isDecoratedServiceIgnored);

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

        public static T ThrowIfNull<T>(this T arg, string message = null, object arg0 = null, object arg1 = null, object arg2 = null, object arg3 = null) where T : class
        {
            if (arg != null) return arg;
            throw GetException(message == null ? Format(ARG_IS_NULL, typeof(T)) : Format(message, arg0, arg1, arg2, arg3));
        }

        public static T ThrowIf<T>(this T arg, bool throwCondition, string message = null, object arg0 = null, object arg1 = null, object arg2 = null, object arg3 = null)
        {
            if (!throwCondition) return arg;
            throw GetException(message == null ? Format(ARG_HAS_IMVALID_CONDITION, typeof(T)) : Format(message, arg0, arg1, arg2, arg3));
        }

        public static void If(bool throwCondition, string message, object arg0 = null, object arg1 = null, object arg2 = null, object arg3 = null)
        {
            if (!throwCondition) return;
            throw GetException(Format(message, arg0, arg1, arg2, arg3));
        }

        #region Implementation

        private static string Format(this string message, object arg0 = null, object arg1 = null, object arg2 = null, object arg3 = null)
        {
            return string.Format(message, Print(arg0), Print(arg1), Print(arg2), Print(arg3));
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

        public static string Print<T>(this IEnumerable<T> items, Func<T, string> print = null, string separator = ", ")
        {
            if (items == null) return null;
            print = print ?? (x => x is Type ? (x as Type).Print() : x.ToString());
            return items.Aggregate(new StringBuilder(),
                (s, x) => (s.Length != 0 ? s.Append(separator) : s).Append(print(x))).ToString();
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

        public static Expression GetDefaultExpression(this Type type)
        {
            return type.IsValueType ? (Expression)Expression.New(type) : Expression.Constant(null, type);
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

        // Depth-first In-order traversal as described in http://en.wikipedia.org/wiki/Tree_traversal
        // The only difference is using fixed size array instead of stack for speed-up: 5/6 vs. stack.
        public IEnumerator<HashTree<V>> GetEnumerator()
        {
            var parents = new HashTree<V>[Height];
            var parentCount = -1;
            var node = this;
            while (node.Height != 0 || parentCount != -1)
            {
                if (node.Height != 0)
                {
                    parents[++parentCount] = node;
                    node = node.Left;
                }
                else
                {
                    node = parents[parentCount--];
                    yield return node;
                    node = node.Right;
                }
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
}

#pragma warning restore 420