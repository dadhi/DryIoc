using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;

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
    /// <item>Minimal mode with all default rules switched off via Container constructor parameter.</item>
    /// <item>Control of service resolution via <see cref="ResolutionRules"/>.</item>
    /// </list>
    /// </para>
    /// <para>
    /// TODO: vNext
    /// <list type="bullet">
    /// <item>upd: Remove most of Container doc-comments.</item>
    /// <item>fix: Rules are not thread-safe regarding replacing the rule in place, cause the rules are provided as arrays.</item>
    /// </list>
    /// </para>
    /// </summary>
    public class Container : IRegistry, IDisposable
    {
        public Container(Action<IRegistry> setup = null)
        {
            _syncRoot = new object();
            _factories = new Dictionary<Type, FactoriesEntry>();
            _decorators = HashTree<Type, DecoratorsEntry[]>.Using(Sugar.Append);
            _keyedResolutionCache = HashTree<Type, KeyedResolutionCacheEntry>.Empty;
            _defaultResolutionCache = IntTree<KV<Type, CompiledFactory>>.Empty;
            ResolutionRules = new ResolutionRules();

            CurrentScope = SingletonScope = new Scope();

            (setup ?? DefaultSetup).Invoke(this);
        }

        public static Action<IRegistry> DefaultSetup = ContainerSetup.Default;

        public static CompiledFactory CompileExpression(Expression expression, Request request = null)
        {
            if (request != null)
            {
                var varAssignments = request.VarAssignments;
                if (varAssignments.Count == 1)
                {
                    expression = varAssignments.Values.First().Value;
                }
                else if (varAssignments.Count > 1)
                {
                    var vars = varAssignments.Values;
                    expression = Expression.Block(
                        vars.Select(x => x.Key),
                        vars.Select(x => Expression.Assign(x.Key, x.Value)).Concat(
                            new[] { expression }));
                }
            }

            return Expression.Lambda<CompiledFactory>(expression, Reuse.Parameters).Compile();
        }

        public Container OpenScope()
        {
            return new Container(this);
        }

        public ResolutionRules.ResolveUnregisteredService UseRegistrationsFrom(IRegistry registry)
        {
            ResolutionRules.ResolveUnregisteredService
                useRegistryRule = (request, _) => registry.GetOrAddFactory(request, IfUnresolved.ReturnNull);
            ResolutionRules.UnregisteredServices = ResolutionRules.UnregisteredServices.Append(useRegistryRule);
            return useRegistryRule;
        }

        public void Dispose()
        {
            CurrentScope.Dispose();
        }

        #region IRegistrator

        public Factory Register(Factory factory, Type serviceType, object serviceKey)
        {
            var implementationType = factory.ThrowIfNull().ImplementationType;
            if (implementationType != null && serviceType.ThrowIfNull() != typeof(object))
                Throw.If(!implementationType.EnumerateSelfAndImplementedTypes().Contains(serviceType),
                    Error.EXPECTED_IMPL_TYPE_ASSIGNABLE_TO_SERVICE_TYPE, implementationType, serviceType);

            lock (_syncRoot)
            {
                if (factory.Setup.Type == FactoryType.Decorator)
                {
                    _decorators = _decorators.AddOrUpdate(serviceType, new[] { new DecoratorsEntry(factory) });
                    return factory;
                }

                var entry = _factories.GetOrAdd(serviceType, _ => new FactoriesEntry());
                if (serviceKey == null)
                {   // default factories will contain all the factories and LastDefault will just point to the latest.
                    if (entry.LastDefaultFactory != null)
                        entry.DefaultFactories = (entry.DefaultFactories
                            ?? IntTree<Factory>.Empty.AddOrUpdate(entry.MaxDefaultIndex++, entry.LastDefaultFactory))
                            .AddOrUpdate(entry.MaxDefaultIndex++, factory);
                    entry.LastDefaultFactory = factory;
                }
                else if (serviceKey is int)
                {
                    var index = (int)serviceKey;
                    entry.DefaultFactories = (entry.DefaultFactories ?? IntTree<Factory>.Empty).AddOrUpdate(index, factory);
                    entry.MaxDefaultIndex = Math.Max(entry.MaxDefaultIndex, index) + 1;
                }
                else if (serviceKey is string)
                {
                    var name = (string)serviceKey;
                    var named = entry.NamedFactories = entry.NamedFactories ?? new Dictionary<string, Factory>();
                    if (named.ContainsKey(name))
                        Throw.If(true, Error.DUPLICATE_SERVICE_NAME, serviceType, name, named[name].ImplementationType);
                    named.Add(name, factory);
                }
            }

            return factory;
        }

        public bool IsRegistered(Type serviceType, string serviceName)
        {
            return ((IRegistry)this).GetFactoryOrNull(serviceType.ThrowIfNull(), serviceName) != null;
        }

        #endregion

        #region IResolver

        public object ResolveDefault(Type serviceType, IfUnresolved ifUnresolved)
        {
            var factory = _defaultResolutionCache.GetValueOrDefault(serviceType.GetHashCode());
            return (factory != null && serviceType == factory.Key ? factory.Value : ResolveAndCacheFactory(serviceType, ifUnresolved))
                (CurrentScope, null/* resolutionRootScope */);
        }

        public object ResolveKeyed(Type serviceType, object serviceKey, IfUnresolved ifUnresolved)
        {
            var entry = _keyedResolutionCache.GetValueOrDefault(serviceType);
            var result = entry != null ? entry.GetCompiledFactoryOrNull(serviceKey.ThrowIfNull()) : null;
            if (result == null) // nothing in cache now, try to resolve and cache.
            {
                var request = new Request(null, serviceType, serviceKey);
                var factory = ((IRegistry)this).GetOrAddFactory(request, ifUnresolved);
                if (factory == null) return null;
                result = CompileExpression(factory.GetExpression(request, this), request);
                Interlocked.Exchange(ref _keyedResolutionCache, _keyedResolutionCache.AddOrUpdate(serviceType,
                    (entry ?? KeyedResolutionCacheEntry.Empty).Add(serviceKey, result)));
            }

            return result(CurrentScope, null /* resolutionRootScope */);
        }

        public delegate object CompiledFactory(Scope openScope, Scope resolutionRootScope);

        private IntTree<KV<Type, CompiledFactory>> _defaultResolutionCache;
        private HashTree<Type, KeyedResolutionCacheEntry> _keyedResolutionCache;

        private CompiledFactory ResolveAndCacheFactory(Type serviceType, IfUnresolved ifUnresolved)
        {
            var request = new Request(null, serviceType, null);
            var factory = ((IRegistry)this).GetOrAddFactory(request, ifUnresolved);
            if (factory == null) return EmptyCompiledFactory;

            var expression = factory.GetExpression(request, this);
            var result = CompileExpression(expression, request);

            Interlocked.Exchange(ref _defaultResolutionCache,
                _defaultResolutionCache.AddOrUpdate(serviceType.GetHashCode(), new KV<Type, CompiledFactory>(serviceType, result)));
            return result;
        }

        private static object EmptyCompiledFactory(Scope openScope, Scope resolutionRootsScope) { return null; }

        private sealed class KeyedResolutionCacheEntry
        {
            public static readonly KeyedResolutionCacheEntry Empty = new KeyedResolutionCacheEntry();

            private IntTree<CompiledFactory> _indexed = IntTree<CompiledFactory>.Empty;
            private HashTree<string, CompiledFactory> _named = HashTree<string, CompiledFactory>.Empty;

            public CompiledFactory GetCompiledFactoryOrNull(object key)
            {
                return key is int ? _indexed.GetValueOrDefault((int)key) : _named.GetValueOrDefault((string)key);
            }

            public KeyedResolutionCacheEntry Add(object key, CompiledFactory factory)
            {
                return key is int
                    ? new KeyedResolutionCacheEntry { _indexed = _indexed.AddOrUpdate((int)key, factory), _named = _named }
                    : new KeyedResolutionCacheEntry { _indexed = _indexed, _named = _named.AddOrUpdate((string)key, factory) };
            }
        }

        #endregion

        #region IRegistry

        public ResolutionRules ResolutionRules { get; private set; }

        public Scope SingletonScope { get; private set; }
        public Scope CurrentScope { get; private set; }

        Factory IRegistry.GetOrAddFactory(Request request, IfUnresolved ifUnresolved)
        {
            Factory newFactory = null;
            lock (_syncRoot)
            {
                FactoriesEntry entry;
                Factory factory;
                if (_factories.TryGetValue(request.ServiceType, out entry) &&
                    entry.TryGet(out factory, request.ServiceType, request.ServiceKey, ResolutionRules.GetSingleRegisteredFactory))
                    return factory.GetFactoryPerRequestOrNull(request, this) ?? factory;

                if (request.OpenGenericServiceType != null &&
                    _factories.TryGetValue(request.OpenGenericServiceType, out entry))
                {
                    Factory genericFactory;
                    if (entry.TryGet(out genericFactory, request.ServiceType, request.ServiceKey, ResolutionRules.GetSingleRegisteredFactory) ||
                        request.ServiceKey != null && // OR try find generic-wrapper by ignoring service key.
                        entry.TryGet(out genericFactory, request.ServiceType, null, ResolutionRules.GetSingleRegisteredFactory) &&
                        genericFactory.Setup.Type == FactoryType.GenericWrapper)
                    {
                        newFactory = genericFactory.GetFactoryPerRequestOrNull(request, this);
                    }
                }
            }

            if (newFactory == null)
                newFactory = ResolutionRules.GetUnregisteredServiceFactoryOrNull(request, this);

            if (newFactory == null)
                Throw.If(ifUnresolved == IfUnresolved.Throw, Error.UNABLE_TO_RESOLVE_SERVICE, request);
            else
                Register(newFactory, request.ServiceType, request.ServiceKey);

            return newFactory;
        }

        Expression IRegistry.GetDecoratorExpressionOrNull(Request request)
        {
            // Decorators for non service types are not supported.
            if (request.FactoryType != FactoryType.Service)
                return null;

            // We are already resolving decorator for the service, so stop now.
            var parent = request.GetNonWrapperParentOrNull();
            if (parent != null && parent.DecoratedFactoryID == request.FactoryID)
                return null;

            var serviceType = request.ServiceType;
            var decoratorFuncType = typeof(Func<,>).MakeGenericType(serviceType, serviceType);

            LambdaExpression resultFuncDecorator = null;
            var funcDecorators = _decorators.GetValueOrDefault(decoratorFuncType);
            if (funcDecorators != null)
            {
                var decoratorRequest = request.MakeDecorated();
                for (var i = 0; i < funcDecorators.Length; i++)
                {
                    var decorator = funcDecorators[i].Factory;
                    if (((DecoratorSetup)decorator.Setup).IsApplicable(request))
                    {
                        var newDecorator = decorator.GetExpression(decoratorRequest, this);
                        if (resultFuncDecorator == null)
                        {
                            var decorated = Expression.Parameter(serviceType, "decorated");
                            resultFuncDecorator = Expression.Lambda(Expression.Invoke(newDecorator, decorated), decorated);
                        }
                        else
                        {
                            var decorateDecorator = Expression.Invoke(newDecorator, resultFuncDecorator.Body);
                            resultFuncDecorator = Expression.Lambda(decorateDecorator, resultFuncDecorator.Parameters[0]);
                        }
                    }
                }
            }

            IEnumerable<DecoratorsEntry> decorators = _decorators.GetValueOrDefault(serviceType);
            var openGenericDecoratorIndex = decorators == null ? 0 : ((DecoratorsEntry[])decorators).Length;
            if (request.OpenGenericServiceType != null)
            {
                var openGenericDecorators = _decorators.GetValueOrDefault(request.OpenGenericServiceType);
                if (openGenericDecorators != null)
                    decorators = decorators == null ? openGenericDecorators : decorators.Concat(openGenericDecorators);
            }

            Expression resultDecorator = resultFuncDecorator;
            if (decorators != null)
            {
                var decoratorRequest = request.MakeDecorated();
                var decoratorIndex = 0;
                var enumerator = decorators.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var decorator = enumerator.Current.ThrowIfNull();
                    var factory = decorator.Factory;
                    if (((DecoratorSetup)factory.Setup).IsApplicable(request))
                    {
                        // Cache closed generic registration produced by open-generic decorator.
                        if (decoratorIndex++ >= openGenericDecoratorIndex)
                            factory = Register(factory.GetFactoryPerRequestOrNull(request, this), serviceType, null);

                        if (decorator.CachedExpression == null)
                        {
                            IList<Type> unusedFunArgs;
                            var funcExpr = factory
                                .GetFuncWithArgsOrNull(decoratorFuncType, decoratorRequest, this, out unusedFunArgs)
                                .ThrowIfNull(Error.DECORATOR_FACTORY_SHOULD_SUPPORT_FUNC_RESOLUTION, decoratorFuncType);

                            decorator.CachedExpression = unusedFunArgs != null ? funcExpr.Body : funcExpr;
                        }

                        if (resultDecorator == null || !(decorator.CachedExpression is LambdaExpression))
                            resultDecorator = decorator.CachedExpression;
                        else
                        {
                            if (!(resultDecorator is LambdaExpression))
                                resultDecorator = Expression.Invoke(decorator.CachedExpression, resultDecorator);
                            else
                            {
                                var prevDecorators = ((LambdaExpression)resultDecorator);
                                var decorateDecorator = Expression.Invoke(decorator.CachedExpression, prevDecorators.Body);
                                resultDecorator = Expression.Lambda(decorateDecorator, prevDecorators.Parameters[0]);
                            }
                        }
                    }
                }
            }

            return resultDecorator;
        }

        IEnumerable<object> IRegistry.GetKeys(Type serviceType, Func<Factory, bool> condition)
        {
            lock (_syncRoot)
            {
                FactoriesEntry entry;
                if (TryFindEntry(out entry, serviceType))
                {
                    if (entry.DefaultFactories != null)
                    {
                        foreach (var item in entry.DefaultFactories.TraverseInOrder())
                            if (condition == null || condition(item.Value))
                                yield return item.Key;
                    }
                    else if (entry.LastDefaultFactory != null)
                    {
                        if (condition == null || condition(entry.LastDefaultFactory))
                            yield return 0;
                    }

                    if (entry.NamedFactories != null)
                    {
                        foreach (var pair in entry.NamedFactories)
                            if (condition == null || condition(pair.Value))
                                yield return pair.Key;
                    }
                }
            }
        }

        Factory IRegistry.GetFactoryOrNull(Type serviceType, object serviceKey)
        {
            lock (_syncRoot)
            {
                FactoriesEntry entry;
                Factory factory;
                if (TryFindEntry(out entry, serviceType) &&
                    entry.TryGet(out factory, serviceType, serviceKey, ResolutionRules.GetSingleRegisteredFactory))
                    return factory;
                return null;
            }
        }

        Type IRegistry.GetWrappedServiceTypeOrSelf(Type serviceType)
        {
            if (!serviceType.IsGenericType)
                return serviceType;

            var factory = ((IRegistry)this).GetFactoryOrNull(serviceType.GetGenericTypeDefinition(), null);
            if (factory == null || factory.Setup.Type != FactoryType.GenericWrapper)
                return serviceType;

            var wrapperSetup = ((GenericWrapperSetup)factory.Setup);
            var wrappedType = wrapperSetup.GetWrappedServiceType(serviceType.GetGenericArguments());
            return wrappedType == serviceType ? serviceType
                : ((IRegistry)this).GetWrappedServiceTypeOrSelf(wrappedType); // unwrap recursively.
        }

        private bool TryFindEntry(out FactoriesEntry entry, Type serviceType)
        {
            return _factories.TryGetValue(serviceType, out entry) || serviceType.IsGenericType &&
                   _factories.TryGetValue(serviceType.GetGenericTypeDefinition().ThrowIfNull(), out entry);
        }

        #endregion

        #region Implementation

        private Container(Container source)
        {
            CurrentScope = new Scope();
            SingletonScope = source.SingletonScope;
            ResolutionRules = source.ResolutionRules;
            _syncRoot = source._syncRoot;
            _factories = source._factories;
            _decorators = source._decorators;
            _keyedResolutionCache = source._keyedResolutionCache;
            _defaultResolutionCache = source._defaultResolutionCache;
        }

        private readonly object _syncRoot;
        private readonly Dictionary<Type, FactoriesEntry> _factories;
        private HashTree<Type, DecoratorsEntry[]> _decorators;

        private sealed class FactoriesEntry
        {
            public Factory LastDefaultFactory;
            public IntTree<Factory> DefaultFactories;
            public int MaxDefaultIndex;
            public Dictionary<string, Factory> NamedFactories;

            public bool TryGet(out Factory result, Type serviceType, object serviceKey,
                Func<IEnumerable<Factory>, Factory> getSingleFactory = null)
            {
                result = null;
                if (serviceKey == null)
                {
                    if (DefaultFactories == null)
                        result = LastDefaultFactory;
                    else
                    {
                        var factories = DefaultFactories.TraverseInOrder().Select(_ => _.Value);
                        result = getSingleFactory
                            .ThrowIfNull(Error.EXPECTED_SINGLE_DEFAULT_FACTORY, serviceType, factories.Select(_ => _.ImplementationType))
                            .Invoke(factories);
                    }
                }
                else
                {
                    if (serviceKey is string)
                    {
                        if (NamedFactories != null)
                            NamedFactories.TryGetValue((string)serviceKey, out result);
                    }
                    else if (serviceKey is int)
                    {
                        var index = (int)serviceKey;
                        if (DefaultFactories == null && index == 0)
                            result = LastDefaultFactory;
                        else if (DefaultFactories != null)
                            result = DefaultFactories.GetValueOrDefault(index);
                    }
                }

                return result != null;
            }
        }

        private sealed class DecoratorsEntry
        {
            public readonly Factory Factory;
            public Expression CachedExpression;

            public DecoratorsEntry(Factory factory, Expression cachedExpression = null)
            {
                Factory = factory;
                CachedExpression = cachedExpression;
            }
        }

        #endregion
    }

    public static class ContainerSetup
    {
        public static Type[] FuncTypes = { typeof(Func<>), typeof(Func<,>), typeof(Func<,,>), typeof(Func<,,,>), typeof(Func<,,,,>) };

        public static void Minimal(IRegistry registry) { }

        public static void Default(IRegistry registry)
        {
            registry.ResolutionRules.UnregisteredServices =
                registry.ResolutionRules.UnregisteredServices.Append(GetEnumerableDynamicallyOrNull);

            var funcFactory = new FactoryProvider(
                (_, __) => new DelegateFactory(GetFuncExpression, Reuse.Singleton),
                GenericWrapperSetup.With(t => t[t.Length - 1]));
            foreach (var funcType in FuncTypes)
                registry.Register(funcType, funcFactory);

            var lazyFactory = new ReflectionFactory(typeof(Lazy<>),
                getConstructor: t => t.GetConstructor(new[] { typeof(Func<>).MakeGenericType(t.GetGenericArguments()) }),
                setup: GenericWrapperSetup.Default);
            registry.Register(typeof(Lazy<>), lazyFactory);

            var metaFactory = new FactoryProvider(GetMetaFactoryOrNull, GenericWrapperSetup.With(t => t[0]));
            registry.Register(typeof(Meta<,>), metaFactory);

            var debugExprFactory = new FactoryProvider(
                (_, __) => new DelegateFactory(GetDebugExpression),
                GenericWrapperSetup.Default);
            registry.Register(typeof(DebugExpression<>), debugExprFactory);
        }

        public static ResolutionRules.ResolveUnregisteredService GetEnumerableDynamicallyOrNull = (request, registry) =>
        {
            if (!request.ServiceType.IsArray && request.OpenGenericServiceType != typeof(IEnumerable<>))
                return null;

            return new DelegateFactory((req, reg) =>
            {
                var collectionType = req.ServiceType;
                var collectionIsArray = collectionType.IsArray;
                var itemType = collectionIsArray
                    ? collectionType.GetElementType()
                    : collectionType.GetGenericArguments()[0];
                var wrappedItemType = reg.GetWrappedServiceTypeOrSelf(itemType);

                var resolver = Expression.Constant(new DynamicEnumerableResolver(reg, itemType, wrappedItemType));
                var resolveMethod = (collectionIsArray
                    ? DynamicEnumerableResolver.ResolveArrayMethod
                    : DynamicEnumerableResolver.ResolveEnumerableMethod).MakeGenericMethod(itemType);
                return Expression.Call(resolver, resolveMethod, Expression.Constant(req));
            },
            setup: ServiceSetup.With(FactoryCachePolicy.DoNotCacheExpression));
        };

        public static Expression GetFuncExpression(Request request, IRegistry registry)
        {
            var funcType = request.ServiceType;
            var funcTypeArgs = funcType.GetGenericArguments();
            var serviceType = funcTypeArgs[funcTypeArgs.Length - 1];

            var serviceRequest = request.PushWithParentKey(serviceType);
            var serviceFactory = registry.GetOrAddFactory(serviceRequest, IfUnresolved.Throw);

            if (funcTypeArgs.Length == 1)
                return Expression.Lambda(funcType, serviceFactory.GetExpression(serviceRequest, registry), null);

            IList<Type> unusedFuncArgs;
            var funcExpr = serviceFactory.GetFuncWithArgsOrNull(funcType, serviceRequest, registry, out unusedFuncArgs)
                .ThrowIfNull(Error.UNSUPPORTED_FUNC_WITH_ARGS, funcType, serviceRequest)
                .ThrowIf(unusedFuncArgs != null, Error.SOME_FUNC_PARAMS_ARE_UNUSED, unusedFuncArgs, request);
            return funcExpr;
        }

        public static Expression GetDebugExpression(Request request, IRegistry registry)
        {
            var ctor = request.ServiceType.GetConstructors()[0];
            var serviceType = request.ServiceType.GetGenericArguments()[0];

            var serviceRequest = request.PushWithParentKey(serviceType);
            var serviceExpr = registry.GetOrAddFactory(serviceRequest, IfUnresolved.Throw).GetExpression(serviceRequest, registry);
            var expression = Expression.New(ctor, Expression.Lambda<Container.CompiledFactory>(serviceExpr, Reuse.Parameters));
            return expression;
        }

        public static Factory GetMetaFactoryOrNull(Request request, IRegistry registry)
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
                    (resultMetadata = GetTypedMetadataOrNull(factory, metadataType)) != null).FirstOrDefault();
                if (resultKey != null)
                    serviceKey = resultKey;
            }
            else
            {
                var factory = registry.GetFactoryOrNull(wrappedServiceType, serviceKey);
                if (factory != null)
                    resultMetadata = GetTypedMetadataOrNull(factory, metadataType);
            }

            if (resultMetadata == null)
                return null;

            return new DelegateFactory((_, __) =>
            {
                var serviceRequest = request.Push(serviceType, serviceKey);
                var serviceFactory = registry.GetOrAddFactory(serviceRequest, IfUnresolved.Throw);

                var metaCtor = request.ServiceType.GetConstructors()[0];
                var serviceExpr = serviceFactory.GetExpression(serviceRequest, registry);
                var metadataExpr = Expression.Constant(resultMetadata, metadataType);
                return Expression.New(metaCtor, serviceExpr, metadataExpr);
            });
        }

        #region Implementation

        private static object GetTypedMetadataOrNull(Factory factory, Type metadataType)
        {
            var metadata = factory.Setup.Metadata;
            return metadata != null && metadataType.IsInstanceOfType(metadata) ? metadata : null;
        }

        internal sealed class DynamicEnumerableResolver
        {
            public static readonly MethodInfo ResolveEnumerableMethod =
                typeof(DynamicEnumerableResolver).GetMethod("ResolveEnumerable");

            public static readonly MethodInfo ResolveArrayMethod =
                typeof(DynamicEnumerableResolver).GetMethod("ResolveArray");

            public DynamicEnumerableResolver(IRegistry registry, Type itemType, Type unwrappedItemType)
            {
                _registry = new WeakReference(registry);
                _unwrappedItemType = unwrappedItemType;
                _itemType = itemType;
            }

            public IEnumerable<T> ResolveEnumerable<T>(Request request)
            {
                var registry = (_registry.Target as IRegistry).ThrowIfNull(Error.CONTAINER_IS_GARBAGE_COLLECTED);

                var parent = request.GetNonWrapperParentOrNull();
                // Composite pattern support: filter out composite root from available keys.
                var keys = parent != null && parent.ServiceType == _unwrappedItemType
                    ? registry.GetKeys(_unwrappedItemType, factory => factory.ID != parent.FactoryID)
                    : registry.GetKeys(_unwrappedItemType, null);

                foreach (var key in keys)
                {
                    var service = registry.ResolveKeyed(_itemType, key, IfUnresolved.ReturnNull);
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
    }

    public sealed class ResolutionRules
    {
        public Func<IEnumerable<Factory>, Factory> GetSingleRegisteredFactory;

        public delegate Factory ResolveUnregisteredService(Request request, IRegistry registry);
        public ResolveUnregisteredService[] UnregisteredServices = new ResolveUnregisteredService[0];

        public delegate object ResolveConstructorParameterServiceKey(ParameterInfo parameter, Request parent, IRegistry registry);
        public ResolveConstructorParameterServiceKey[] ConstructorParameters = new ResolveConstructorParameterServiceKey[0];

        public delegate bool ResolveMemberServiceKey(out object key, MemberInfo member, Request parent, IRegistry registry);
        public ResolveMemberServiceKey[] PropertiesAndFields = new ResolveMemberServiceKey[0];

        public Factory GetUnregisteredServiceFactoryOrNull(Request request, IRegistry registry)
        {
            Factory factory = null;
            var rules = UnregisteredServices;
            if (rules != null)
                for (var i = 0; i < rules.Length && factory == null; i++)
                    factory = rules[i].Invoke(request, registry);
            return factory;
        }

        public object GetConstructorParameterKeyOrDefault(ParameterInfo parameter, Request parent, IRegistry registry)
        {
            if (parent.FactoryType != FactoryType.Service)
                return parent.ServiceKey; // propagate key from wrapper or decorator.

            object key = null;
            var rules = ConstructorParameters;
            if (rules != null)
                for (var i = 0; i < rules.Length && key == null; i++)
                    key = rules[i].Invoke(parameter, parent, registry);
            return key;
        }

        public bool ShouldResolvePropertiesAndFields
        {
            get
            {
                var rules = PropertiesAndFields;
                return rules != null && rules.Length != 0;
            }
        }

        public bool TryGetPropertyOrFieldServiceKey(out object key, MemberInfo member, Request parent, IRegistry registry)
        {
            var rules = PropertiesAndFields;
            if (rules != null)
                for (var i = 0; i < rules.Length; i++)
                    if (rules[i].Invoke(out key, member, parent, registry))
                        return true;
            key = null;
            return false;
        }
    }

    public static class Error
    {
        public static readonly string UNABLE_TO_RESOLVE_SERVICE =
@"Unable to resolve service {0}. 
Please register service OR adjust resolution rules.";

        public static readonly string UNSUPPORTED_FUNC_WITH_ARGS =
            "Unsupported resolution as {0} of {1}.";

        public static readonly string EXPECTED_IMPL_TYPE_ASSIGNABLE_TO_SERVICE_TYPE =
            "Expecting implementation type {0} to be assignable to service type {1} but it is not.";

        public static readonly string EXPECTED_SINGLE_DEFAULT_FACTORY =
@"Expecting single default registration of {0} but found many:
{1}.";

        public static readonly string EXPECTED_NON_ABSTRACT_IMPL_TYPE =
            "Expecting not abstract and not interface implementation type, but found {0}.";

        public static readonly string NO_PUBLIC_CONSTRUCTOR_DEFINED =
            "There is no public constructor defined for {0}.";

        public static readonly string CONSTRUCTOR_MISSES_SOME_PARAMETERS =
            "Constructor [{0}] of {1} misses some arguments required for {2} dependency.";

        public static readonly string UNABLE_TO_SELECT_CONSTRUCTOR =
@"Unable to select single constructor from {0} available in {1}.
Please provide constructor selector when registering service.";

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

        public static readonly string DUPLICATE_SERVICE_NAME =
            "Service {0} with duplicate name '{1}' is already registered with implementation {2}.";

        public static readonly string GENERIC_WRAPPER_EXPECTS_SINGLE_TYPE_ARG_BY_DEFAULT =
@"Generic Wrapper is working with single service type only, but found many: 
{0}.
Please specify service type selector in Generic Wrapper setup upon registration.";

        public static readonly string SOME_FUNC_PARAMS_ARE_UNUSED =
@"Found some unused Func parameters: 
{0} 
when resolving {1}.";

        public static readonly string DECORATOR_FACTORY_SHOULD_SUPPORT_FUNC_RESOLUTION =
            "Decorator factory should support resolution as {0}, but it does not.";
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
        /// <param name="getConstructor">Optional strategy to select constructor when multiple available.</param>
        /// <param name="setup">Optional factory setup, by default is (<see cref="ServiceSetup"/>)</param>
        /// <param name="named">Optional registration name.</param>
        public static void Register(this IRegistrator registrator, Type serviceType,
            Type implementationType, IReuse reuse = null, GetConstructor getConstructor = null, FactorySetup setup = null,
            string named = null)
        {
            registrator.Register(new ReflectionFactory(implementationType, reuse, getConstructor, setup), serviceType, named);
        }

        /// <summary>
        /// Registers service of <paramref name="implementationType"/>. ServiceType will be the same as <paramref name="implementationType"/>.
        /// </summary>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="implementationType">Implementation type. Concrete and open-generic class are supported.</param>
        /// <param name="reuse">Optional <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="withConstructor">Optional strategy to select constructor when multiple available.</param>
        /// <param name="setup">Optional factory setup, by default is (<see cref="ServiceSetup"/>)</param>
        /// <param name="named">Optional registration name.</param>
        public static void Register(this IRegistrator registrator,
            Type implementationType, IReuse reuse = null, GetConstructor withConstructor = null, FactorySetup setup = null,
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
        /// <param name="setup">Optional factory setup, by default is (<see cref="ServiceSetup"/>)</param>
        /// <param name="named">Optional registration name.</param>
        public static void Register<TService, TImplementation>(this IRegistrator registrator,
            IReuse reuse = null, GetConstructor withConstructor = null, FactorySetup setup = null,
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
        /// <param name="setup">Optional factory setup, by default is (<see cref="ServiceSetup"/>)</param>
        /// <param name="named">Optional registration name.</param>
        public static void Register<TImplementation>(this IRegistrator registrator,
            IReuse reuse = null, GetConstructor withConstructor = null, FactorySetup setup = null,
            string named = null)
        {
            registrator.Register(new ReflectionFactory(typeof(TImplementation), reuse, withConstructor, setup), typeof(TImplementation), named);
        }

        public static Func<Type, bool> PublicTypes = t => (t.IsPublic || t.IsNestedPublic) && t != typeof(object);

        /// <summary>
        /// Registers single registration for all implemented public interfaces and base classes.
        /// </summary>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="implementationType">Service implementation type. Concrete and open-generic class are supported.</param>
        /// <param name="reuse">Optional <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="withConstructor">Optional strategy to select constructor when multiple available.</param>
        /// <param name="setup">Optional factory setup, by default is (<see cref="ServiceSetup"/>)</param>
        /// <param name="named">Optional registration name.</param>
        /// <param name="types">Optional condition to include selected types only. Default value is <see cref="PublicTypes"/></param>
        public static void RegisterAll(this IRegistrator registrator,
            Type implementationType, IReuse reuse = null, GetConstructor withConstructor = null, FactorySetup setup = null,
            string named = null, Func<Type, bool> types = null)
        {
            var registration = new ReflectionFactory(implementationType, reuse, withConstructor, setup);
            foreach (var serviceType in implementationType.EnumerateSelfAndImplementedTypes().Where(types ?? PublicTypes))
                registrator.Register(registration, serviceType, named);
        }

        /// <summary>
        /// Registers single registration for all implemented public interfaces and base classes.
        /// </summary>
        /// <typeparam name="TImplementation">The type of service.</typeparam>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="reuse">Optional <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="withConstructor">Optional strategy to select constructor when multiple available.</param>
        /// <param name="setup">Optional factory setup, by default is (<see cref="ServiceSetup"/>)</param>
        /// <param name="named">Optional registration name.</param>
        public static void RegisterAll<TImplementation>(this IRegistrator registrator,
            IReuse reuse = null, GetConstructor withConstructor = null, FactorySetup setup = null,
            string named = null)
        {
            registrator.RegisterAll(typeof(TImplementation), reuse, withConstructor, setup, named);
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
        /// <param name="setup">Optional factory setup, by default is (<see cref="ServiceSetup"/>)</param>
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
        /// <param name="setup">Optional factory setup, by default is (<see cref="ServiceSetup"/>)</param>
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
        /// <param name="ifUnresolved">Optional, say to how to handle unresolved service case.</param>
        /// <returns>The requested service instance.</returns>
        public static object Resolve(this IResolver resolver, Type serviceType, IfUnresolved ifUnresolved = IfUnresolved.Throw)
        {
            return resolver.ResolveDefault(serviceType, ifUnresolved);
        }

        /// <summary>
        /// Returns an instance of statically known <typepsaramref name="TService"/> type.
        /// </summary>
        /// <typeparam name="TService">The type of the requested service.</typeparam>
        /// <param name="resolver">Any <see cref="IResolver"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="ifUnresolved">Optional, say to how to handle unresolved service case.</param>
        /// <returns>The requested service instance.</returns>
        public static TService Resolve<TService>(this IResolver resolver, IfUnresolved ifUnresolved = IfUnresolved.Throw)
        {
            return (TService)resolver.ResolveDefault(typeof(TService), ifUnresolved);
        }

        /// <summary>
        /// Returns an instance of statically known <typepsaramref name="TService"/> type.
        /// </summary>
        /// <param name="serviceType">The type of the requested service.</param>
        /// <param name="resolver">Any <see cref="IResolver"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="serviceName">Service name.</param>
        /// <param name="ifUnresolved">Optional, say to how to handle unresolved service case.</param>
        /// <returns>The requested service instance.</returns>
        public static object Resolve(this IResolver resolver, Type serviceType, string serviceName, IfUnresolved ifUnresolved = IfUnresolved.Throw)
        {
            return serviceName == null
                ? resolver.ResolveDefault(serviceType, ifUnresolved)
                : resolver.ResolveKeyed(serviceType, serviceName, ifUnresolved);
        }

        /// <summary>
        /// Returns an instance of statically known <typepsaramref name="TService"/> type.
        /// </summary>
        /// <typeparam name="TService">The type of the requested service.</typeparam>
        /// <param name="resolver">Any <see cref="IResolver"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="serviceName">Service name.</param>
        /// <param name="ifUnresolved">Optional, say to how to handle unresolved service case.</param>
        /// <returns>The requested service instance.</returns>
        public static TService Resolve<TService>(this IResolver resolver, string serviceName, IfUnresolved ifUnresolved = IfUnresolved.Throw)
        {
            return (TService)resolver.Resolve(typeof(TService), serviceName, ifUnresolved);
        }

        public static readonly BindingFlags MembersToResolve = BindingFlags.Public | BindingFlags.Instance;

        /// <summary>
        /// For given instance resolves and sets non-initialized (null) properties from container.
        /// It does not throw if property is not resolved, so you might need to check property value afterwards.
        /// </summary>
        /// <param name="resolver">Any <see cref="IResolver"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="instance">Service instance with properties to resolve and initialize.</param>
        /// <param name="getServiceName">Optional function to find service name, otherwise service name will be null.</param>
        public static void ResolvePropertiesAndFields(this IResolver resolver, object instance, Func<MemberInfo, string> getServiceName = null)
        {
            var implType = instance.ThrowIfNull().GetType();
            getServiceName = getServiceName ?? (_ => null);

            foreach (var property in implType.GetProperties(MembersToResolve).Where(p => p.GetSetMethod() != null))
            {
                var value = resolver.Resolve(property.PropertyType, getServiceName(property), IfUnresolved.ReturnNull);
                if (value != null)
                    property.SetValue(instance, value, null);
            }

            foreach (var field in implType.GetFields(MembersToResolve).Where(f => !f.IsInitOnly))
            {
                var value = resolver.Resolve(field.FieldType, getServiceName(field), IfUnresolved.ReturnNull);
                if (value != null)
                    field.SetValue(instance, value);
            }
        }
    }

    public enum FactoryType { Service, Decorator, GenericWrapper };

    public enum FactoryCachePolicy { CacheExpression, DoNotCacheExpression };

    public abstract class FactorySetup
    {
        public abstract FactoryType Type { get; }
        public virtual FactoryCachePolicy CachePolicy { get { return FactoryCachePolicy.CacheExpression; } }
        public virtual object Metadata { get { return null; } }
    }

    public class ServiceSetup : FactorySetup
    {
        public static readonly ServiceSetup Default = new ServiceSetup();

        public static ServiceSetup With(FactoryCachePolicy cachePolicy = FactoryCachePolicy.CacheExpression, object metadata = null)
        {
            return cachePolicy == FactoryCachePolicy.CacheExpression && metadata == null ? Default : new ServiceSetup(cachePolicy, metadata);
        }

        public static ServiceSetup WithMetadata(object metadata = null)
        {
            return metadata == null ? Default : new ServiceSetup(metadata: metadata);
        }

        public override FactoryType Type { get { return FactoryType.Service; } }
        public override FactoryCachePolicy CachePolicy { get { return _cachePolicy; } }
        public override object Metadata { get { return _metadata; } }

        #region Implementation

        private ServiceSetup(FactoryCachePolicy cachePolicy = FactoryCachePolicy.CacheExpression, object metadata = null)
        {
            _cachePolicy = cachePolicy;
            _metadata = metadata;
        }

        private readonly FactoryCachePolicy _cachePolicy;

        private readonly object _metadata;

        #endregion
    }

    public class GenericWrapperSetup : FactorySetup
    {
        public static readonly GenericWrapperSetup Default = new GenericWrapperSetup();

        public static GenericWrapperSetup With(Func<Type[], Type> selectServiceTypeArg)
        {
            return selectServiceTypeArg == null ? Default : new GenericWrapperSetup(selectServiceTypeArg);
        }

        public override FactoryType Type { get { return FactoryType.GenericWrapper; } }
        public readonly Func<Type[], Type> GetWrappedServiceType;

        #region Implementation

        private GenericWrapperSetup(Func<Type[], Type> selectServiceTypeFromGenericArgs = null)
        {
            GetWrappedServiceType = selectServiceTypeFromGenericArgs ?? SelectSingleByDefault;
        }

        private static Type SelectSingleByDefault(Type[] typeArgs)
        {
            Throw.If(typeArgs.Length != 1, Error.GENERIC_WRAPPER_EXPECTS_SINGLE_TYPE_ARG_BY_DEFAULT, typeArgs);
            return typeArgs[0];
        }

        #endregion
    }

    public class DecoratorSetup : FactorySetup
    {
        public static readonly DecoratorSetup Default = new DecoratorSetup();

        public static DecoratorSetup With(Func<Request, bool> condition = null)
        {
            return condition == null ? Default : new DecoratorSetup(condition);
        }

        public override FactoryType Type { get { return FactoryType.Decorator; } }
        public override FactoryCachePolicy CachePolicy { get { return FactoryCachePolicy.DoNotCacheExpression; } }
        public readonly Func<Request, bool> IsApplicable;

        #region Implementation

        private DecoratorSetup(Func<Request, bool> condition = null)
        {
            IsApplicable = condition ?? (_ => true);
        }

        #endregion
    }

    public abstract class Factory
    {
        public static readonly FactorySetup DefaultSetup = ServiceSetup.Default;

        public readonly int ID;
        public readonly IReuse Reuse;

        public FactorySetup Setup
        {
            get { return _setup; }
            protected internal set { _setup = value ?? DefaultSetup; }
        }

        public virtual Type ImplementationType { get { return null; } }

        protected Factory(IReuse reuse = null, FactorySetup setup = null)
        {
            ID = Interlocked.Increment(ref _idSeedAndCount);
            Reuse = reuse;
            Setup = setup;
        }

        public abstract Factory GetFactoryPerRequestOrNull(Request request, IRegistry registry);

        public Expression GetExpression(Request request, IRegistry registry)
        {
            request = request.ResolveTo(this);

            var decorator = registry.GetDecoratorExpressionOrNull(request);
            if (decorator != null && !(decorator is LambdaExpression))
                return decorator;

            var result = _cachedExpression;
            if (result == null)
            {
                result = CreateExpression(request, registry);
                if (Reuse != null)
                    result = Reuse.Of(request, registry, ID, result);
                if (Setup.CachePolicy == FactoryCachePolicy.CacheExpression)
                    Interlocked.Exchange(ref _cachedExpression, result);
            }

            if (decorator != null)
                result = Expression.Invoke(decorator, result);

            return result;
        }

        protected abstract Expression CreateExpression(Request request, IRegistry registry);

        public LambdaExpression GetFuncWithArgsOrNull(Type funcType, Request request, IRegistry registry, out IList<Type> unusedFuncArgs)
        {
            request = request.ResolveTo(this);

            var func = CreateFuncWithArgsOrNull(funcType, request, registry, out unusedFuncArgs);
            if (func == null)
                return null;

            var decorator = registry.GetDecoratorExpressionOrNull(request);
            if (decorator != null && !(decorator is LambdaExpression))
                return Expression.Lambda(funcType, decorator, func.Parameters);

            if (Reuse != null)
                func = Expression.Lambda(funcType, Reuse.Of(request, registry, ID, func.Body), func.Parameters);

            if (decorator != null)
                func = Expression.Lambda(funcType, Expression.Invoke(decorator, func.Body), func.Parameters);

            return func;
        }

        protected virtual LambdaExpression CreateFuncWithArgsOrNull(Type funcType, Request request, IRegistry registry, out IList<Type> unusedFuncArgs)
        {
            unusedFuncArgs = null;
            return null;
        }

        #region Implementation

        private static int _idSeedAndCount;
        private FactorySetup _setup;
        private Expression _cachedExpression;

        #endregion
    }

    public delegate ConstructorInfo GetConstructor(Type implementationType);

    public sealed class ReflectionFactory : Factory
    {
        public override Type ImplementationType { get { return _implementationType; } }

        public ReflectionFactory(Type implementationType, IReuse reuse = null, GetConstructor getConstructor = null, FactorySetup setup = null)
            : base(reuse, setup)
        {
            _implementationType = implementationType.ThrowIfNull()
                .ThrowIf(implementationType.IsAbstract, Error.EXPECTED_NON_ABSTRACT_IMPL_TYPE, implementationType);
            _getConstructor = getConstructor;
        }

        public override Factory GetFactoryPerRequestOrNull(Request request, IRegistry _)
        {
            if (!_implementationType.IsGenericTypeDefinition) return null;
            var closedImplType = _implementationType.MakeGenericType(request.ServiceType.GetGenericArguments());
            return new ReflectionFactory(closedImplType, Reuse, _getConstructor, Setup);
        }

        protected override Expression CreateExpression(Request request, IRegistry registry)
        {
            var ctor = GetConstructor(ImplementationType);
            var ctorParams = ctor.GetParameters();
            Expression[] paramExprs = null;
            if (ctorParams.Length != 0)
            {
                paramExprs = new Expression[ctorParams.Length];
                for (var i = 0; i < ctorParams.Length; i++)
                {
                    var ctorParam = ctorParams[i];
                    var paramKey = registry.ResolutionRules.GetConstructorParameterKeyOrDefault(ctorParam, request, registry);
                    var paramRequest = request.Push(ctorParam.ParameterType, paramKey,
                        new DependencyInfo(DependencyKind.CtorParam, ctorParam.Name));
                    paramExprs[i] = registry.GetOrAddFactory(paramRequest, IfUnresolved.Throw).GetExpression(paramRequest, registry);
                }
            }

            return InitMembersIfRequired(Expression.New(ctor, paramExprs), request, registry);
        }

        protected override LambdaExpression CreateFuncWithArgsOrNull(Type funcType, Request request, IRegistry registry, out IList<Type> unusedFuncArgs)
        {
            var funcParamTypes = funcType.GetGenericArguments();
            funcParamTypes.ThrowIf(funcParamTypes.Length == 1, Error.EXPECTED_FUNC_WITH_MULTIPLE_ARGS, funcType);

            var ctor = GetConstructor(ImplementationType);
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
                    var paramKey = registry.ResolutionRules.GetConstructorParameterKeyOrDefault(ctorParam, request, registry);
                    var paramRequest = request.Push(ctorParam.ParameterType, paramKey,
                        new DependencyInfo(DependencyKind.CtorParam, ctorParam.Name));
                    ctorParamExprs[cp] = registry.GetOrAddFactory(paramRequest, IfUnresolved.Throw).GetExpression(paramRequest, registry);
                }
            }

            // Find unused Func parameters (present in Func but not in constructor) and create "_" (ignored) Parameter expressions for them.
            // In addition store unused parameter in output list for client review.
            unusedFuncArgs = null;
            for (var fp = 0; fp < funcInputParamExprs.Length; fp++)
            {
                if (funcInputParamExprs[fp] == null) // unused parameter
                {
                    if (unusedFuncArgs == null) unusedFuncArgs = new List<Type>(2);
                    var funcParamType = funcParamTypes[fp];
                    unusedFuncArgs.Add(funcParamType);
                    funcInputParamExprs[fp] = Expression.Parameter(funcParamType, "_");
                }
            }

            var newExpr = Expression.New(ctor, ctorParamExprs);
            return Expression.Lambda(funcType, InitMembersIfRequired(newExpr, request, registry), funcInputParamExprs);
        }

        #region Implementation

        private readonly Type _implementationType;
        private readonly GetConstructor _getConstructor;

        public ConstructorInfo GetConstructor(Type type)
        {
            if (_getConstructor != null)
                return _getConstructor(type);

            var constructors = type.GetConstructors();
            Throw.If(constructors.Length == 0, Error.NO_PUBLIC_CONSTRUCTOR_DEFINED, type);
            Throw.If(constructors.Length > 1, Error.UNABLE_TO_SELECT_CONSTRUCTOR, constructors.Length, type);
            return constructors[0];
        }

        private Expression InitMembersIfRequired(NewExpression newService, Request request, IRegistry registry)
        {
            if (!registry.ResolutionRules.ShouldResolvePropertiesAndFields)
                return newService;

            var properties = ImplementationType.GetProperties(Resolver.MembersToResolve).Where(p => p.GetSetMethod() != null);
            var fields = ImplementationType.GetFields(Resolver.MembersToResolve).Where(f => !f.IsInitOnly);

            var bindings = new List<MemberBinding>();
            foreach (var member in properties.Cast<MemberInfo>().Concat(fields.Cast<MemberInfo>()))
            {
                object key;
                if (registry.ResolutionRules.TryGetPropertyOrFieldServiceKey(out key, member, request, registry))
                {
                    var memberRequest = request.Push(member.GetMemberType(), key,
                        new DependencyInfo(member is PropertyInfo ? DependencyKind.Property : DependencyKind.Field, member.Name));
                    var memberExpr = registry.GetOrAddFactory(memberRequest, IfUnresolved.Throw).GetExpression(memberRequest, registry);
                    bindings.Add(Expression.Bind(member, memberExpr));
                }
            }

            return bindings.Count == 0 ? (Expression)newService : Expression.MemberInit(newService, bindings);
        }

        #endregion
    }

    public sealed class Request
    {
        public readonly Request Parent; // can be null for resolution root
        public readonly Type ServiceType;
        public readonly object ServiceKey; // null for default, string for named or integer index for multiple defaults.
        public readonly Type OpenGenericServiceType;
        public readonly DependencyInfo Dependency;

        public readonly int DecoratedFactoryID;

        public readonly int FactoryID;
        public readonly FactoryType FactoryType;
        public readonly Type ImplementationType;
        public readonly object Metadata;
        public Request(Request parent, Type serviceType, object serviceKey, DependencyInfo dependency = null,
            int decoratedFactoryID = 0, Factory factory = null,
            Dictionary<int, KV<ParameterExpression, Expression>> varAssignments = null)
        {
            Parent = parent;
            ServiceKey = serviceKey;
            Dependency = dependency;

            ServiceType = serviceType.ThrowIfNull()
                .ThrowIf(serviceType.IsGenericTypeDefinition, Error.EXPECTED_CLOSED_GENERIC_SERVICE_TYPE, serviceType);

            OpenGenericServiceType = serviceType.IsGenericType ? serviceType.GetGenericTypeDefinition() : null;

            DecoratedFactoryID = decoratedFactoryID;
            if (factory != null)
            {
                FactoryType = factory.Setup.Type;
                FactoryID = factory.ID;
                ImplementationType = factory.ImplementationType;
                Metadata = factory.Setup.Metadata;
            }

            VarAssignments = varAssignments ?? new Dictionary<int, KV<ParameterExpression, Expression>>();
        }

        public Request GetNonWrapperParentOrNull()
        {
            var p = Parent;
            while (p != null && p.FactoryType == FactoryType.GenericWrapper)
                p = p.Parent;
            return p;
        }

        public Request Push(Type serviceType, object serviceKey, DependencyInfo dependency = null)
        {
            return new Request(this, serviceType, serviceKey, dependency, varAssignments: VarAssignments);
        }

        public Request PushWithParentKey(Type serviceType, DependencyInfo dependency = null)
        {
            return new Request(this, serviceType, ServiceKey, dependency, varAssignments: VarAssignments);
        }

        public Request ResolveTo(Factory factory)
        {
            for (var p = Parent; p != null; p = p.Parent)
                Throw.If(p.FactoryID == factory.ID, Error.RECURSIVE_DEPENDENCY_DETECTED, this);
            return new Request(Parent, ServiceType, ServiceKey, Dependency, DecoratedFactoryID, factory, VarAssignments);
        }

        public Request MakeDecorated()
        {
            return new Request(Parent, ServiceType, ServiceKey, Dependency, FactoryID, varAssignments: VarAssignments);
        }

        public IEnumerable<Request> Enumerate()
        {
            for (var x = this; x != null; x = x.Parent)
                yield return x;
        }

        public override string ToString()
        {
            var message = new StringBuilder().Append(Print());
            return Parent == null ? message.ToString()
                 : Parent.Enumerate().Aggregate(message,
                    (m, r) => m.AppendLine().Append(" in ").Append(r.Print())).ToString();
        }

        private string Print()
        {
            var key = ServiceKey is string ? "\"" + ServiceKey + "\""
                : ServiceKey is int ? "#" + ServiceKey
                    : "unnamed";

            var kind = FactoryType == FactoryType.Decorator ? " decorator"
                : FactoryType == FactoryType.GenericWrapper ? " generic wrapper"
                    : string.Empty;

            var type = ImplementationType != null && ImplementationType != ServiceType
                ? ImplementationType.Print() + " : " + ServiceType.Print()
                : ServiceType.Print();

            var dep = Dependency == null ? string.Empty : " (" + Dependency + ")";

            // example: "unnamed generic wrapper DryIoc.UnitTests.IService : DryIoc.UnitTests.Service (CtorParam service)"
            return key + kind + " " + type + dep;
        }

        public void AddVarAssignment(int id, ParameterExpression varExpr, Expression assignedExpr)
        {
            VarAssignments[id] = new KV<ParameterExpression, Expression>(varExpr, assignedExpr);
        }

        public readonly Dictionary<int, KV<ParameterExpression, Expression>> VarAssignments;
    }

    public class DelegateFactory : Factory
    {
        public DelegateFactory(Func<Request, IRegistry, Expression> getExpression, IReuse reuse = null, FactorySetup setup = null)
            : base(reuse, setup)
        {
            _getExpression = getExpression.ThrowIfNull();
        }

        public override Factory GetFactoryPerRequestOrNull(Request request, IRegistry registry)
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

    public class FactoryProvider : Factory
    {
        public FactoryProvider(Func<Request, IRegistry, Factory> getFactoryOrNull, FactorySetup setup = null)
            : base(setup: setup)
        {
            _getFactoryOrNull = getFactoryOrNull.ThrowIfNull();
        }

        public override Factory GetFactoryPerRequestOrNull(Request request, IRegistry registry)
        {
            var factory = _getFactoryOrNull(request, registry);
            if (factory != null && factory.Setup == DefaultSetup)
                factory.Setup = Setup; // propagate provider setup if it is not specified by client.
            return factory;
        }

        //ncrunch: no coverage start
        protected override Expression CreateExpression(Request request, IRegistry registry)
        {
            throw new NotSupportedException();
        }
        //ncrunch: no coverage end

        private readonly Func<Request, IRegistry, Factory> _getFactoryOrNull;
    }

    public enum DependencyKind { CtorParam, Property, Field }

    public sealed class DependencyInfo
    {
        public readonly DependencyKind Kind;
        public readonly string Name;

        public DependencyInfo(DependencyKind kind, string name)
        {
            Kind = kind;
            Name = name;
        }

        public override string ToString()
        {
            return Kind + " " + Name;
        }
    }

    public sealed class Scope : IDisposable
    {
        public T GetOrAdd<T>(int id, Func<T> factory)
        {
            Throw.If(_disposed == 1, Error.SCOPE_IS_DISPOSED);
            lock (_syncRoot)
            {
                var item = _items.GetValueOrDefault(id);
                if (item == null)
                    _items = _items.AddOrUpdate(id, item = factory());
                return (T)item;
            }
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
                return;

            foreach (var item in _items.TraverseInOrder().Select(x => x.Value).OfType<IDisposable>())
                item.Dispose();
            _items = null;
        }

        #region Implementation

        private readonly object _syncRoot = new object();
        private IntTree<object> _items = IntTree<object>.Empty;
        private int _disposed;

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

        internal static readonly ParameterExpression[] Parameters;

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
                // save scope into separate var to prevent closure on registry.
                var singletonScope = registry.SingletonScope;

                // Create lazy singleton if we have Func somewhere in dependency chain.
                var parent = request.Parent;
                if (parent != null &&
                    parent.Enumerate().Any(p =>
                        p.OpenGenericServiceType != null &&
                        ContainerSetup.FuncTypes.Contains(p.OpenGenericServiceType)))
                    return GetScopedServiceExpression(Expression.Constant(singletonScope), factoryID, factoryExpr);

                // Otherwise we can create singleton instance right here, and put it into Scope for later disposal.
                var currentScope = registry.CurrentScope; // same as for singletonScope

                var singleton = singletonScope.GetOrAdd(factoryID, () => Container.CompileExpression(factoryExpr)(currentScope, null));
                var singletonType = factoryExpr.Type;
                var singletonConstExpr = Expression.Constant(singleton, singletonType);

#if NET40_AND_UP
                var singletonVarExpr = Expression.Variable(singletonType);
                request.AddVarAssignment(factoryID, singletonVarExpr, singletonConstExpr);
                return singletonVarExpr;
#else
                return singletonConstExpr;
#endif
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

    public enum IfUnresolved { Throw, ReturnNull }

    public interface IResolver
    {
        object ResolveDefault(Type serviceType, IfUnresolved ifUnresolved);

        object ResolveKeyed(Type serviceType, object serviceKey, IfUnresolved ifUnresolved);
    }

    public interface IRegistrator
    {
        Factory Register(Factory factory, Type serviceType, object serviceKey);

        bool IsRegistered(Type serviceType, string serviceName);
    }

    public interface IRegistry : IResolver, IRegistrator
    {
        ResolutionRules ResolutionRules { get; }

        Scope SingletonScope { get; }

        Scope CurrentScope { get; }

        Factory GetOrAddFactory(Request request, IfUnresolved ifUnresolved);

        Factory GetFactoryOrNull(Type serviceType, object serviceKey);

        Expression GetDecoratorExpressionOrNull(Request request);

        IEnumerable<object> GetKeys(Type serviceType, Func<Factory, bool> condition);

        Type GetWrappedServiceTypeOrSelf(Type serviceType);
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

    [DebuggerDisplay("{Expression}")]
    public sealed class DebugExpression<TService>
    {
        public readonly Expression<Container.CompiledFactory> Expression;

        public DebugExpression(Expression<Container.CompiledFactory> expression)
        {
            Expression = expression;
        }
    }

    public class ContainerException : InvalidOperationException
    {
        public ContainerException(string message) : base(message) { }
    }

    public static class Throw
    {
        public static Func<string, Exception> GetException = message => new ContainerException(message);

        public static Func<object, string> PrintArg = Sugar.Print;

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

        private static string Format(this string message, object arg0 = null, object arg1 = null, object arg2 = null)
        {
            return string.Format(message, PrintArg(arg0), PrintArg(arg1), PrintArg(arg2));
        }

        private static readonly string ARG_IS_NULL = "Argument of type {0} is null.";
        private static readonly string ARG_HAS_IMVALID_CONDITION = "Argument of type {0} has invalid condition.";
    }

    public static class Sugar
    {
        public static string Print(object x)
        {
            return x is string ? (string)x
                : (x is Type ? ((Type)x).Print()
                : (x is IEnumerable<Type> ? ((IEnumerable)x).Print(";" + Environment.NewLine)
                : (x is IEnumerable ? ((IEnumerable)x).Print()
                : (string.Empty + x))));
        }

        public static string Print(this IEnumerable items, string separator = ", ", Func<object, string> printItem = null)
        {
            if (items == null) return null;
            printItem = printItem ?? Print;
            var builder = new StringBuilder();
            foreach (var item in items)
                (builder.Length == 0 ? builder : builder.Append(separator)).Append(printItem(item));
            return builder.ToString();
        }

        public static string Print(this Type type, Func<Type, string> print = null /* prints Type.FullName by default */)
        {
            if (type == null) return null;
            var name = print == null ? type.FullName : print(type);
            if (type.IsGenericType) // for generic types
            {
                var genericArgs = type.GetGenericArguments();
                var genericArgsString = type.IsGenericTypeDefinition
                    ? new string(',', genericArgs.Length - 1)
                    : string.Join(", ", genericArgs.Select(x => x.Print(print)).ToArray());
                name = name.Substring(0, name.IndexOf('`')) + "<" + genericArgsString + ">";
            }
            return name.Replace('+', '.'); // for nested classes
        }

        public static Type[] EnumerateSelfAndImplementedTypes(this Type type)
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

        public static Type GetMemberType(this MemberInfo member)
        {
            var mt = member.MemberType;
            mt.ThrowIf(mt != MemberTypes.Field && mt != MemberTypes.Property);
            return mt == MemberTypes.Field ? ((FieldInfo)member).FieldType : ((PropertyInfo)member).PropertyType;
        }

        public static V GetOrAdd<K, V>(this IDictionary<K, V> source, K key, Func<K, V> valueFactory)
        {
            V value;
            if (!source.TryGetValue(key, out value))
                source.Add(key, value = valueFactory(key));
            return value;
        }

        public static T[] Append<T>(this T[] source, params T[] added)
        {
            var result = new T[source.Length + added.Length];
            Array.Copy(source, 0, result, 0, source.Length);
            if (added.Length == 1)
                result[source.Length] = added[0];
            else
                Array.Copy(added, 0, result, source.Length, added.Length);
            return result;
        }

        public static T[] AppendOrUpdate<T>(this T[] source, T value, int index = -1)
        {
            var sourceLength = source.Length;
            index = index < 0 ? sourceLength : index;
            var result = new T[index < sourceLength ? sourceLength : sourceLength + 1];
            Array.Copy(source, result, sourceLength);
            result[index] = value;
            return result;
        }
    }

    /// <summary>
    /// Immutable AVL-tree (http://en.wikipedia.org/wiki/AVL_tree) with node key of type int.
    /// </summary>
    public sealed class IntTree<V>
    {
        public static readonly IntTree<V> Empty = new IntTree<V>();
        public bool IsEmpty { get { return Height == 0; } }

        public readonly int Key;
        public readonly V Value;

        public readonly int Height;
        public readonly IntTree<V> Left, Right;

        public delegate V UpdateValue(V existing, V added);

        public IntTree<V> AddOrUpdate(int key, V value, UpdateValue updateValue = null)
        {
            return Height == 0 ? new IntTree<V>(key, value, Empty, Empty)
                : (key == Key ? new IntTree<V>(key, updateValue == null ? value : updateValue(Value, value), Left, Right)
                : (key < Key
                    ? With(Left.AddOrUpdate(key, value, updateValue), Right)
                    : With(Left, Right.AddOrUpdate(key, value, updateValue))).EnsureBalanced());
        }

        public V GetValueOrDefault(int key, V defaultValue = default(V))
        {
            for (var node = this; node.Height != 0; node = key < node.Key ? node.Left : node.Right)
                if (node.Key == key)
                    return node.Value;
            return defaultValue;
        }

        /// <summary>Depth-first in-order traversal as described in http://en.wikipedia.org/wiki/Tree_traversal
        /// The only difference is using fixed size array instead of stack for speed-up (~20% faster than stack).</summary>
        public IEnumerable<IntTree<V>> TraverseInOrder()
        {
            var parents = new IntTree<V>[Height];
            var parentCount = -1;
            var node = this;
            while (!node.IsEmpty || parentCount != -1)
            {
                if (!node.IsEmpty)
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

        #region Implementation

        private IntTree() { }

        private IntTree(int key, V value, IntTree<V> left, IntTree<V> right)
        {
            Key = key;
            Value = value;
            Left = left;
            Right = right;
            Height = 1 + (left.Height > right.Height ? left.Height : right.Height);
        }

        private IntTree<V> EnsureBalanced()
        {
            var delta = Left.Height - Right.Height;
            return delta >= 2 ? With(Left.Right.Height - Left.Left.Height == 1 ? Left.RotateLeft() : Left, Right).RotateRight()
                : (delta <= -2 ? With(Left, Right.Left.Height - Right.Right.Height == 1 ? Right.RotateRight() : Right).RotateLeft()
                : this);
        }

        private IntTree<V> RotateRight()
        {
            return Left.With(Left.Left, With(Left.Right, Right));
        }

        private IntTree<V> RotateLeft()
        {
            return Right.With(With(Left, Right.Left), Right.Right);
        }

        private IntTree<V> With(IntTree<V> left, IntTree<V> right)
        {
            return new IntTree<V>(Key, Value, left, right);
        }

        #endregion
    }

    public sealed class HashTree<K, V>
    {
        public static readonly HashTree<K, V> Empty = new HashTree<K, V>(IntTree<KV<K, V>>.Empty, null);

        public static HashTree<K, V> Using(Func<V, V, V> updateValue)
        {
            return new HashTree<K, V>(IntTree<KV<K, V>>.Empty, updateValue);
        }

        public HashTree<K, V> AddOrUpdate(K key, V value)
        {
            return new HashTree<K, V>(_tree.AddOrUpdate(key.GetHashCode(), new KV<K, V>(key, value), UpdateConflicts), _updateValue);
        }

        public V GetValueOrDefault(K key)
        {
            var item = _tree.GetValueOrDefault(key.GetHashCode());
            return item != null && (ReferenceEquals(key, item.Key) || key.Equals(item.Key)) ? item.Value : GetConflictedOrDefault(item, key);
        }

        #region Implementation

        private HashTree(IntTree<KV<K, V>> tree, Func<V, V, V> updateValue)
        {
            _tree = tree;
            _updateValue = updateValue;
        }

        private readonly IntTree<KV<K, V>> _tree;
        private readonly Func<V, V, V> _updateValue;

        private KV<K, V> UpdateConflicts(KV<K, V> existing, KV<K, V> added)
        {
            var conflicts = existing is KVWithConflicts ? ((KVWithConflicts)existing).Conflicts : null;
            if (ReferenceEquals(existing.Key, added.Key) || existing.Key.Equals(added.Key))
                return conflicts == null ? UpdateValue(existing, added)
                     : new KVWithConflicts(UpdateValue(existing, added), conflicts);

            if (conflicts == null)
                return new KVWithConflicts(existing, new[] { added });

            var i = conflicts.Length - 1;
            while (i >= 0 && !Equals(conflicts[i].Key, added.Key)) --i;
            if (i != -1) added = UpdateValue(existing, added);
            return new KVWithConflicts(existing, conflicts.AppendOrUpdate(added, i));
        }

        private KV<K, V> UpdateValue(KV<K, V> existing, KV<K, V> added)
        {
            return _updateValue == null ? added : new KV<K, V>(existing.Key, _updateValue(existing.Value, added.Value));
        }

        private static V GetConflictedOrDefault(KV<K, V> item, K key)
        {
            var conflicts = item is KVWithConflicts ? ((KVWithConflicts)item).Conflicts : null;
            if (conflicts != null)
                for (var i = 0; i < conflicts.Length; i++)
                    if (Equals(conflicts[i].Key, key))
                        return conflicts[i].Value;
            return default(V);
        }

        private sealed class KVWithConflicts : KV<K, V>
        {
            public readonly KV<K, V>[] Conflicts;

            public KVWithConflicts(KV<K, V> kv, KV<K, V>[] conflicts)
                : base(kv.Key, kv.Value)
            {
                Conflicts = conflicts;
            }
        }

        #endregion
    }

    public class KV<K, V>
    {
        public readonly K Key;
        public readonly V Value;

        public KV(K key, V value)
        {
            Key = key;
            Value = value;
        }
    }
}