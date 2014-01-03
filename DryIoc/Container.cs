/*
The MIT License (MIT)

Copyright (c) 2013 Maksim Volkau

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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;

// TODO: Add doc-comments to all public API.
namespace DryIoc
{
    /// <summary>
    /// IoC Container. Documentation is available at https://bitbucket.org/dadhi/dryioc.
    /// </summary>
    public class Container : IRegistry, IDisposable
    {
        public Container(Action<IRegistry> setup = null)
        {
            _syncRoot = new object();
            _factories = new Dictionary<Type, FactoriesEntry>();
            _decorators = HashTree<Type, DecoratorsEntry[]>.Empty;
            _defaultResolutionCache = HashTree<Type, CompiledFactory>.Empty;
            _keyedResolutionCache = HashTree<Type, HashTree<object, CompiledFactory>>.Empty;

            _constants = new object[3];

            // Put reference to container into constants, to support container access inside expression. 
            // It is common for dynamic scenarios.
            _constants[REGISTRY_WEAKREF_CONST_INDEX] = new WeakReference(this);
            _constants[CURRENT_SCOPE_CONST_INDEX] = _constants[SINGLETON_SCOPE_CONST_INDEX] = new Scope();

            ResolutionRules = new ResolutionRules();
            (setup ?? DefaultSetup).Invoke(this);
        }

        public static Action<IRegistry> DefaultSetup = ContainerSetup.Default;

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
            ((Scope)_constants[CURRENT_SCOPE_CONST_INDEX]).Dispose();
        }

        #region Compiled Factory

        public static readonly ParameterExpression ConstantsParameter = Expression.Parameter(typeof(object[]), "constants");
        public static readonly ParameterExpression ResolutionScopeParameter = Expression.Parameter(typeof(Scope), "resolutionScope");

        public static readonly int REGISTRY_WEAKREF_CONST_INDEX = 0;
        public static readonly int SINGLETON_SCOPE_CONST_INDEX = 1;
        public static readonly int CURRENT_SCOPE_CONST_INDEX = 2;

        public static readonly Expression RegistryWeakRefExpression = Expression.Convert(
            Expression.ArrayIndex(ConstantsParameter, Expression.Constant(REGISTRY_WEAKREF_CONST_INDEX)),
            typeof(WeakReference));

        public static readonly Expression RegistryExpression = Expression.Convert(
            Expression.Property(RegistryWeakRefExpression, "Target"),
            typeof(IRegistry));

        public static readonly Expression SingletonScopeExpression = Expression.Convert(
            Expression.ArrayIndex(ConstantsParameter, Expression.Constant(SINGLETON_SCOPE_CONST_INDEX)),
            typeof(Scope));

        public static readonly Expression CurrentScopeExpression = Expression.Convert(
            Expression.ArrayIndex(ConstantsParameter, Expression.Constant(CURRENT_SCOPE_CONST_INDEX)),
            typeof(Scope));

        #endregion

        #region IRegistrator

        public Factory Register(Factory factory, Type serviceType, object serviceKey)
        {
            var implementationType = factory.ThrowIfNull().ImplementationType;
            if (implementationType != null && serviceType.ThrowIfNull() != typeof(object))
            {
                Throw.If(!implementationType.GetImplementedTypes().Contains(serviceType),
                    Error.EXPECTED_IMPL_TYPE_ASSIGNABLE_TO_SERVICE_TYPE, implementationType, serviceType);
            }

            lock (_syncRoot)
            {
                if (factory.Setup.Type == FactoryType.Decorator)
                {
                    _decorators = _decorators.AddOrUpdate(serviceType, new[] { new DecoratorsEntry(factory) }, Sugar.Append);
                    return factory;
                }

                var entry = _factories.GetOrAdd(serviceType, _ => new FactoriesEntry());
                if (serviceKey == null)
                {   // default factories will contain all the factories and LastDefault will just point to the latest, 
                    // for memory saving reasons.
                    if (entry.LastDefaultFactory != null)
                        entry.DefaultFactories = (entry.DefaultFactories
                            ?? HashTree<int, Factory>.Empty.AddOrUpdate(entry.MaxDefaultIndex++, entry.LastDefaultFactory))
                            .AddOrUpdate(entry.MaxDefaultIndex++, factory);
                    entry.LastDefaultFactory = factory;
                }
                else if (serviceKey is int)
                {
                    var index = (int)serviceKey;
                    entry.DefaultFactories = (entry.DefaultFactories ?? HashTree<int, Factory>.Empty).AddOrUpdate(index, factory);
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
            return ((IRegistry)this).GetFactoryOrDefault(serviceType.ThrowIfNull(), serviceName) != null;
        }

        #endregion

        #region IResolver

        object IResolver.ResolveDefault(Type serviceType, IfUnresolved ifUnresolved)
        {
            var compiledFactory =
                _defaultResolutionCache.GetValueOrDefault(serviceType) ??
                ResolveAndCacheFactory(serviceType, ifUnresolved);
            return compiledFactory(_constants, resolutionScope: null);
        }

        object IResolver.ResolveKeyed(Type serviceType, object serviceKey, IfUnresolved ifUnresolved)
        {
            var entry = _keyedResolutionCache.GetValueOrDefault(serviceType) ?? HashTree<object, CompiledFactory>.Empty;
            var compiledFactory = entry.GetValueOrDefault(serviceKey);
            if (compiledFactory == null)
            {
                var request = Request.Create(serviceType, serviceKey);
                var factory = ((IRegistry)this).GetOrAddFactory(request, ifUnresolved);
                if (factory == null) return null;
                compiledFactory = factory.GetExpression(request, this).ToFactoryExpression().CompileFactory();
                Interlocked.Exchange(ref _keyedResolutionCache,
                    _keyedResolutionCache.AddOrUpdate(serviceType, entry.AddOrUpdate(serviceKey, compiledFactory)));
            }

            return compiledFactory(_constants, resolutionScope: null);
        }

        private HashTree<Type, CompiledFactory> _defaultResolutionCache;
        private HashTree<Type, HashTree<object, CompiledFactory>> _keyedResolutionCache;

        private CompiledFactory ResolveAndCacheFactory(Type serviceType, IfUnresolved ifUnresolved)
        {
            var request = Request.Create(serviceType);
            var factory = ((IRegistry)this).GetOrAddFactory(request, ifUnresolved);
            if (factory == null) return EmptyCompiledFactory;
            var compiledFactory = factory.GetExpression(request, this).ToFactoryExpression().CompileFactory();
            Interlocked.Exchange(ref _defaultResolutionCache,
                _defaultResolutionCache.AddOrUpdate(serviceType, compiledFactory));
            return compiledFactory;
        }

        private static object EmptyCompiledFactory(object[] costants, Scope resolutionScope) { return null; }

        #endregion

        #region IRegistry

        public ResolutionRules ResolutionRules { get; private set; }

        public object[] Constants { get { return _constants; } }

        public Expression GetConstantExpression(object constant, Type constantType)
        {
            int constantIndex;
            lock (_syncRoot)
            {
                constantIndex = Array.IndexOf(_constants, constant);
                if (constantIndex == -1)
                {
                    _constants = _constants.AppendOrUpdate(constant);
                    constantIndex = _constants.Length - 1;
                }
            }

            var constantIndexExpr = Expression.Constant(constantIndex, typeof(int));
            var constantsAccesssExpr = Expression.ArrayIndex(ConstantsParameter, constantIndexExpr);
            return Expression.Convert(constantsAccesssExpr, constantType);
        }

        Factory IRegistry.GetOrAddFactory(Request request, IfUnresolved ifUnresolved)
        {
            Factory newFactory = null;
            lock (_syncRoot)
            {
                FactoriesEntry entry;
                Factory factory;
                if (_factories.TryGetValue(request.ServiceType, out entry) &&
                    entry.TryGet(out factory, request.ServiceType, request.ServiceKey, ResolutionRules.GetSingleRegisteredFactory))
                    return factory.GetFactoryPerRequestOrDefault(request, this) ?? factory;

                if (request.OpenGenericServiceType != null &&
                    _factories.TryGetValue(request.OpenGenericServiceType, out entry))
                {
                    Factory genericFactory;
                    if (entry.TryGet(out genericFactory, request.ServiceType, request.ServiceKey, ResolutionRules.GetSingleRegisteredFactory) ||
                        request.ServiceKey != null && // OR try find generic-wrapper by ignoring service key.
                        entry.TryGet(out genericFactory, request.ServiceType, null, ResolutionRules.GetSingleRegisteredFactory) &&
                        genericFactory.Setup.Type == FactoryType.GenericWrapper)
                    {
                        newFactory = genericFactory.GetFactoryPerRequestOrDefault(request, this);
                    }
                }
            }

            if (newFactory == null)
                newFactory = ResolutionRules.GetUnregisteredServiceFactoryOrDefault(request, this);

            if (newFactory == null)
                Throw.If(ifUnresolved == IfUnresolved.Throw, Error.UNABLE_TO_RESOLVE_SERVICE, request);
            else
                Register(newFactory, request.ServiceType, request.ServiceKey);

            return newFactory;
        }

        Expression IRegistry.GetDecoratorExpressionOrDefault(Request request)
        {
            // Decorators for non service types are not supported.
            if (request.FactoryType != FactoryType.Service)
                return null;

            // We are already resolving decorator for the service, so stop now.
            var parent = request.GetNonWrapperParentOrDefault();
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
                            factory = Register(factory.GetFactoryPerRequestOrDefault(request, this), serviceType, null);

                        if (decorator.CachedExpression == null)
                        {
                            IList<Type> unusedFunArgs;
                            var funcExpr = factory
                                .GetFuncWithArgsOrDefault(decoratorFuncType, decoratorRequest, this, out unusedFunArgs)
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
            condition = condition ?? AlwaysTrue;
            lock (_syncRoot)
            {
                FactoriesEntry entry;
                if (TryFindEntry(out entry, serviceType))
                {
                    if (entry.DefaultFactories != null)
                    {
                        foreach (var item in entry.DefaultFactories.TraverseInOrder())
                            if (condition(item.Value))
                                yield return item.Key;
                    }
                    else if (entry.LastDefaultFactory != null &&
                        condition(entry.LastDefaultFactory))
                        yield return 0;

                    if (entry.NamedFactories != null)
                    {
                        foreach (var pair in entry.NamedFactories)
                            if (condition(pair.Value))
                                yield return pair.Key;
                    }
                }
            }
        }

        Factory IRegistry.GetFactoryOrDefault(Type serviceType, object serviceKey)
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

            var factory = ((IRegistry)this).GetFactoryOrDefault(serviceType.GetGenericTypeDefinition(), null);
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

        private static bool AlwaysTrue(Factory _) { return true; }

        #endregion

        #region Implementation

        // Creates child container with singleton scope, constants and cache shared with parent. BUT with new CurrentScope.
        private Container(Container parent)
        {
            ResolutionRules = parent.ResolutionRules;

            var parentConstants = parent._constants;
            _constants = new object[parentConstants.Length];
            Array.Copy(parentConstants, 0, _constants, 0, parentConstants.Length);
            _constants[REGISTRY_WEAKREF_CONST_INDEX] = new WeakReference(this);
            _constants[CURRENT_SCOPE_CONST_INDEX] = new Scope();

            _syncRoot = parent._syncRoot;
            _factories = parent._factories;
            _decorators = parent._decorators;
            _defaultResolutionCache = parent._defaultResolutionCache;
            _keyedResolutionCache = parent._keyedResolutionCache;
        }

        private readonly object _syncRoot;
        private readonly Dictionary<Type, FactoriesEntry> _factories;
        private HashTree<Type, DecoratorsEntry[]> _decorators;
        private object[] _constants;

        private sealed class FactoriesEntry
        {
            public Factory LastDefaultFactory;
            public HashTree<int, Factory> DefaultFactories;
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
                            .ThrowIfNull(Error.EXPECTED_SINGLE_DEFAULT_FACTORY, serviceType,
                                factories.Select(_ => _.ImplementationType))
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

    public delegate object CompiledFactory(object[] constants, Scope resolutionScope);

    public static partial class FactoryCompiler
    {
        public static Expression<CompiledFactory> ToFactoryExpression(this Expression expression)
        {
            // Removing Convert from expression root, as the result will be converted after invoking CompliedFactory.
            if (expression.NodeType == ExpressionType.Convert)
                expression = ((UnaryExpression)expression).Operand;
            return Expression.Lambda<CompiledFactory>(expression, Container.ConstantsParameter, Container.ResolutionScopeParameter);
        }

        public static CompiledFactory CompileFactory(this Expression<CompiledFactory> factoryExpression)
        {
            CompiledFactory factory = null;
            CompileToMethod(factoryExpression, ref factory);
            // ReSharper disable ConstantNullCoalescingCondition
            return factory ?? factoryExpression.Compile();
            // ReSharper restore ConstantNullCoalescingCondition
        }

        // Partial method definition to be implemented in .NET40 version of Container.
        // It is optional and fine to be not implemented.
        static partial void CompileToMethod(Expression<CompiledFactory> factoryExpression, ref CompiledFactory resultFactory);
    }

    public static class ContainerSetup
    {
        public static Type[] FuncTypes = { typeof(Func<>), typeof(Func<,>), typeof(Func<,,>), typeof(Func<,,,>), typeof(Func<,,,,>) };

        public static void Minimal(IRegistry registry) { }

        public static void Default(IRegistry registry)
        {
            registry.ResolutionRules.UnregisteredServices =
                registry.ResolutionRules.UnregisteredServices.Append(
                    ResolveEnumerableAsStaticArray,
                    ResolveManyDynamically);

            var funcFactory = new FactoryProvider(
                (_, __) => new DelegateFactory(GetFuncExpression, Reuse.Singleton),
                GenericWrapperSetup.With(t => t[t.Length - 1]));
            foreach (var funcType in FuncTypes)
                registry.Register(funcType, funcFactory);

            var lazyFactory = new ReflectionFactory(typeof(Lazy<>),
                getConstructor: t => t.GetConstructor(new[] { typeof(Func<>).MakeGenericType(t.GetGenericArguments()) }),
                setup: GenericWrapperSetup.Default);
            registry.Register(typeof(Lazy<>), lazyFactory);

            var metaFactory = new FactoryProvider(GetMetaFactoryOrDefault, GenericWrapperSetup.With(t => t[0]));
            registry.Register(typeof(Meta<,>), metaFactory);

            var debugExprFactory = new FactoryProvider(
                (_, __) => new DelegateFactory(GetDebugExpression),
                GenericWrapperSetup.Default);
            registry.Register(typeof(DebugExpression<>), debugExprFactory);
        }

        public static ResolutionRules.ResolveUnregisteredService ResolveEnumerableAsStaticArray = (req, _) =>
        {
            if (!req.ServiceType.IsArray && req.OpenGenericServiceType != typeof(IEnumerable<>))
                return null;

            return new DelegateFactory((request, registry) =>
            {
                var collectionType = request.ServiceType;

                var itemType = collectionType.IsArray
                    ? collectionType.GetElementType()
                    : collectionType.GetGenericArguments()[0];

                var wrappedItemType = registry.GetWrappedServiceTypeOrSelf(itemType);

                // Composite pattern support: filter out composite root from available keys.
                var parent = request.GetNonWrapperParentOrDefault();
                Func<Factory, bool> condition = null;
                if (parent != null && parent.ServiceType == wrappedItemType)
                    condition = factory => factory.ID != parent.FactoryID;
                var itemKeys = registry.GetKeys(wrappedItemType, condition).ToArray();
                Throw.If(itemKeys.Length == 0, Error.UNABLE_TO_FIND_REGISTERED_ENUMERABLE_ITEMS, wrappedItemType, request);

                var itemExpressions = new List<Expression>(itemKeys.Length);
                foreach (var itemKey in itemKeys)
                {
                    var itemRequest = request.Push(itemType, itemKey);
                    var itemFactory = registry.GetOrAddFactory(itemRequest, IfUnresolved.ReturnNull);
                    if (itemFactory != null)
                        itemExpressions.Add(itemFactory.GetExpression(itemRequest, registry));
                }

                Throw.If(itemExpressions.Count == 0, Error.UNABLE_TO_RESOLVE_ENUMERABLE_ITEMS, itemType, request);
                var newArrayExpr = Expression.NewArrayInit(itemType.ThrowIfNull(), itemExpressions);
                return newArrayExpr;
            });
        };

        public static ResolutionRules.ResolveUnregisteredService ResolveManyDynamically = (req, _) =>
        {
            if (req.OpenGenericServiceType != typeof(Many<>))
                return null;

            return new DelegateFactory((request, registry) =>
            {
                var dynamicEnumerableType = request.ServiceType;
                var itemType = dynamicEnumerableType.GetGenericArguments()[0];

                var wrappedItemType = registry.GetWrappedServiceTypeOrSelf(itemType);

                // Composite pattern support: filter out composite root from available keys.
                var parentFactoryID = -1;
                var parent = request.GetNonWrapperParentOrDefault();
                if (parent != null && parent.ServiceType == wrappedItemType)
                    parentFactoryID = parent.FactoryID;

                var resolveMethod = _resolveManyDynamicallyMethod.MakeGenericMethod(itemType, wrappedItemType);
                var resolveMethodCallExpr = Expression.Call(resolveMethod,
                    Container.RegistryWeakRefExpression, Expression.Constant(parentFactoryID));

                return Expression.New(dynamicEnumerableType.GetConstructors()[0], resolveMethodCallExpr);
            },
            setup: ServiceSetup.With(FactoryCachePolicy.NotCacheExpression));
        };

        public static Expression GetFuncExpression(Request request, IRegistry registry)
        {
            var funcType = request.ServiceType;
            var funcTypeArgs = funcType.GetGenericArguments();
            var serviceType = funcTypeArgs[funcTypeArgs.Length - 1];

            var serviceRequest = request.PushPreservingParentKey(serviceType);
            var serviceFactory = registry.GetOrAddFactory(serviceRequest, IfUnresolved.Throw);

            if (funcTypeArgs.Length == 1)
                return Expression.Lambda(funcType, serviceFactory.GetExpression(serviceRequest, registry), null);

            IList<Type> unusedFuncArgs;
            var funcExpr = serviceFactory.GetFuncWithArgsOrDefault(funcType, serviceRequest, registry, out unusedFuncArgs)
                .ThrowIfNull(Error.UNSUPPORTED_FUNC_WITH_ARGS, funcType, serviceRequest)
                .ThrowIf(unusedFuncArgs != null, Error.SOME_FUNC_PARAMS_ARE_UNUSED, unusedFuncArgs, request);
            return funcExpr;
        }

        public static Expression GetDebugExpression(Request request, IRegistry registry)
        {
            var ctor = request.ServiceType.GetConstructors()[0];
            var serviceType = request.ServiceType.GetGenericArguments()[0];
            var serviceRequest = request.PushPreservingParentKey(serviceType);

            var factoryExpr = registry
                .GetOrAddFactory(serviceRequest, IfUnresolved.Throw)
                .GetExpression(serviceRequest, registry)
                .ToFactoryExpression();

            var factoryConstExpr = registry.GetConstantExpression(factoryExpr, typeof(Expression<CompiledFactory>));
            return Expression.New(ctor, factoryConstExpr);
        }

        public static Factory GetMetaFactoryOrDefault(Request request, IRegistry registry)
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
                    (resultMetadata = GetTypedMetadataOrDefault(factory, metadataType)) != null).FirstOrDefault();
                if (resultKey != null)
                    serviceKey = resultKey;
            }
            else
            {
                var factory = registry.GetFactoryOrDefault(wrappedServiceType, serviceKey);
                if (factory != null)
                    resultMetadata = GetTypedMetadataOrDefault(factory, metadataType);
            }

            if (resultMetadata == null)
                return null;

            return new DelegateFactory((_, __) =>
            {
                var serviceRequest = request.Push(serviceType, serviceKey);
                var serviceFactory = registry.GetOrAddFactory(serviceRequest, IfUnresolved.Throw);

                var metaCtor = request.ServiceType.GetConstructors()[0];
                var serviceExpr = serviceFactory.GetExpression(serviceRequest, registry);
                var metadataExpr = registry.GetConstantExpression(resultMetadata, metadataType);
                return Expression.New(metaCtor, serviceExpr, metadataExpr);
            });
        }

        #region Implementation

        private static object GetTypedMetadataOrDefault(Factory factory, Type metadataType)
        {
            var metadata = factory.Setup.Metadata;
            return metadata != null && metadataType.IsInstanceOfType(metadata) ? metadata : null;
        }

        private static readonly MethodInfo _resolveManyDynamicallyMethod =
            typeof(ContainerSetup).GetMethod("DoResolveManyDynamically", BindingFlags.Static | BindingFlags.NonPublic);

        internal static IEnumerable<TItem> DoResolveManyDynamically<TItem, TWrappedItem>(WeakReference registryRef, int parentFactoryID)
        {
            var itemType = typeof(TItem);
            var wrappedItemType = typeof(TWrappedItem);
            var registry = (registryRef.Target as IRegistry).ThrowIfNull(Error.CONTAINER_IS_GARBAGE_COLLECTED);

            var itemKeys = registry.GetKeys(wrappedItemType,
                parentFactoryID == -1 ? (Func<Factory, bool>)null : factory => factory.ID != parentFactoryID);

            foreach (var itemKey in itemKeys)
            {
                var item = registry.ResolveKeyed(itemType, itemKey, IfUnresolved.ReturnNull);
                if (item != null) // skip unresolved items
                    yield return (TItem)item;
            }
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

        public Factory GetUnregisteredServiceFactoryOrDefault(Request request, IRegistry registry)
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

        public static readonly string UNABLE_TO_FIND_REGISTERED_ENUMERABLE_ITEMS =
            "Unable to find registered services of item type (unwrapped) {0} when resolving {1}.";

        public static readonly string UNABLE_TO_RESOLVE_ENUMERABLE_ITEMS =
            "Unable to resolve any service of item type {0} when resolving {1}.";

        public static readonly string DELEGATE_FACTORY_EXPRESSION_RETURNED_NULL =
            "Delegate factory expression returned NULL when resolving {0}.";

        public static readonly string UNABLE_TO_GET_SOME_GENERIC_IMPL_TYPE_ARGS =
            "Unable to get some type arguments <{0}> for implementation {1} when resolving {2}.";

        public static readonly string UNABLE_TO_MATCH_IMPL_BASE_TYPES_WITH_SERVICE_TYPE = 
            "For open-generic implementation {0} Unable to match any base types ({1}) with requested service {2}.";
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
        /// Registers implementation type <typeparamref name="TImplementation"/> with itself as service type.
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

        public static Func<Type, bool> PublicTypes =
            type => (type.IsPublic || type.IsNestedPublic) && type != typeof(object);

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
            foreach (var serviceType in implementationType.GetImplementedTypes().Where(types ?? PublicTypes))
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
                (_, registry) =>
                {
                    var lambdaExpr = registry.GetConstantExpression(lambda, typeof(Func<IResolver, TService>));
                    return Expression.Invoke(lambdaExpr, Container.RegistryExpression);
                },
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

    public enum FactoryType { Service = 0, Decorator, GenericWrapper };

    public enum FactoryCachePolicy { CacheExpression, NotCacheExpression };

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
        public override FactoryCachePolicy CachePolicy { get { return FactoryCachePolicy.NotCacheExpression; } }
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

        public abstract Factory GetFactoryPerRequestOrDefault(Request request, IRegistry registry);

        public Expression GetExpression(Request request, IRegistry registry)
        {
            request = request.ResolveTo(this);

            var decorator = registry.GetDecoratorExpressionOrDefault(request);
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

        public LambdaExpression GetFuncWithArgsOrDefault(Type funcType, Request request, IRegistry registry, out IList<Type> unusedFuncArgs)
        {
            request = request.ResolveTo(this);

            var func = CreateFuncWithArgsOrDefault(funcType, request, registry, out unusedFuncArgs);
            if (func == null)
                return null;

            var decorator = registry.GetDecoratorExpressionOrDefault(request);
            if (decorator != null && !(decorator is LambdaExpression))
                return Expression.Lambda(funcType, decorator, func.Parameters);

            if (Reuse != null)
                func = Expression.Lambda(funcType, Reuse.Of(request, registry, ID, func.Body), func.Parameters);

            if (decorator != null)
                func = Expression.Lambda(funcType, Expression.Invoke(decorator, func.Body), func.Parameters);

            return func;
        }

        protected virtual LambdaExpression CreateFuncWithArgsOrDefault(Type funcType, Request request, IRegistry registry, out IList<Type> unusedFuncArgs)
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

        public override Factory GetFactoryPerRequestOrDefault(Request request, IRegistry _)
        {
            if (!_implementationType.IsGenericTypeDefinition)
                return null;

            var closedTypeArgs = GetClosedTypeArgsForOpenGenericImplType(_implementationType, request);
            var closedImplType = _implementationType.MakeGenericType(closedTypeArgs);

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

        protected override LambdaExpression CreateFuncWithArgsOrDefault(Type funcType, Request request, IRegistry registry, out IList<Type> unusedFuncArgs)
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

        private static Type[] GetClosedTypeArgsForOpenGenericImplType(Type implType, Request request)
        {
            var serviceTypeArgs = request.ServiceType.GetGenericArguments();
            if (implType == request.OpenGenericServiceType)
                return serviceTypeArgs;

            var baseTypes = implType.GetImplementedTypes(TypeTools.ReturnBaseOpenGenerics.AsIs, TypeTools.IncludeSelf.Exclude);
            Type matchedBaseType = null;
            IDictionary<string, Type> matchedTypeArgs = null;
            for (var i = 0; i < baseTypes.Length && matchedBaseType == null; i++)
            {
                var t = baseTypes[i];
                if (t.ContainsGenericParameters && t.GetGenericTypeDefinition() == request.OpenGenericServiceType)
                {
                    matchedTypeArgs = new Dictionary<string, Type>();
                    if (TypeTools.MatchClosedGenericWithBaseOpenGenericTypeArgs(serviceTypeArgs, t.GetGenericArguments(), ref matchedTypeArgs)) 
                        matchedBaseType = t;
                }
            }

            matchedBaseType.ThrowIfNull(Error.UNABLE_TO_MATCH_IMPL_BASE_TYPES_WITH_SERVICE_TYPE, implType, baseTypes, request);

            var implTypeArgs = implType.GetGenericArguments();
            for (var i = 0; i < implTypeArgs.Length; i++)
            {
                var implTypeArg = implTypeArgs[i];
                if (implTypeArg.IsGenericParameter)
                {
                    Type closedImplTypeArg;
                    if (matchedTypeArgs.TryGetValue(implTypeArg.Name, out closedImplTypeArg))
                        implTypeArgs[i] = closedImplTypeArg;
                }
            }

            Throw.If(Array.Exists(implTypeArgs, t => t.IsGenericParameter),
                Error.UNABLE_TO_GET_SOME_GENERIC_IMPL_TYPE_ARGS, implTypeArgs, implType, request);
            
            return implTypeArgs;
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

        // Start from creating request, then Resolve it with factory and Push new sub-requests.
        public static Request Create(Type serviceType, object serviceKey = null)
        {
            return new Request(null, serviceType, serviceKey);
        }

        public Request Push(Type serviceType, object serviceKey, DependencyInfo dependency = null)
        {
            return new Request(this, serviceType, serviceKey, dependency);
        }

        public Request PushPreservingParentKey(Type serviceType, DependencyInfo dependency = null)
        {
            return new Request(this, serviceType, ServiceKey, dependency);
        }

        public Request ResolveTo(Factory factory)
        {
            for (var p = Parent; p != null; p = p.Parent)
                Throw.If(p.FactoryID == factory.ID, Error.RECURSIVE_DEPENDENCY_DETECTED, this);
            return new Request(Parent, ServiceType, ServiceKey, Dependency, DecoratedFactoryID, factory);
        }

        public Request MakeDecorated()
        {
            return new Request(Parent, ServiceType, ServiceKey, Dependency, FactoryID);
        }

        public Request GetNonWrapperParentOrDefault()
        {
            var p = Parent;
            while (p != null && p.FactoryType == FactoryType.GenericWrapper)
                p = p.Parent;
            return p;
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

        #region Implementation

        private Request(Request parent, Type serviceType, object serviceKey, DependencyInfo dependency = null,
            int decoratedFactoryID = 0, Factory factory = null)
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
        }

        private string Print()
        {
            var key = ServiceKey is string ? "\"" + ServiceKey + "\""
                : ServiceKey is int ? "#" + ServiceKey
                : "unnamed";

            var kind = FactoryType == FactoryType.Decorator
                ? " decorator" : FactoryType == FactoryType.GenericWrapper
                ? " generic wrapper" : string.Empty;

            var type = ImplementationType != null && ImplementationType != ServiceType
                ? ImplementationType.Print() + " : " + ServiceType.Print()
                : ServiceType.Print();

            var dep = Dependency == null ? string.Empty : " (" + Dependency + ")";

            // example: "unnamed generic wrapper DryIoc.UnitTests.IService : DryIoc.UnitTests.Service (CtorParam service)"
            return key + kind + " " + type + dep;
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

        public override Factory GetFactoryPerRequestOrDefault(Request request, IRegistry registry)
        {
            return null;
        }

        protected override Expression CreateExpression(Request request, IRegistry registry)
        {
            return _getExpression(request, registry).ThrowIfNull(Error.DELEGATE_FACTORY_EXPRESSION_RETURNED_NULL, request);
        }

        #region Implementation

        private readonly Func<Request, IRegistry, Expression> _getExpression;

        #endregion
    }

    public class FactoryProvider : Factory
    {
        public FactoryProvider(Func<Request, IRegistry, Factory> getFactoryOrDefault, FactorySetup setup = null)
            : base(setup: setup)
        {
            _getFactoryOrDefault = getFactoryOrDefault.ThrowIfNull();
        }

        public override Factory GetFactoryPerRequestOrDefault(Request request, IRegistry registry)
        {
            var factory = _getFactoryOrDefault(request, registry);
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

        private readonly Func<Request, IRegistry, Factory> _getFactoryOrDefault;
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
        private HashTree<int, object> _items = HashTree<int, object>.Empty;
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
        public static readonly IReuse Singleton, InCurrentScope, InResolutionScope;

        static Reuse()
        {
            Singleton = new SingletonReuse();
            InCurrentScope = new ScopedReuse(Container.CurrentScopeExpression);
            InResolutionScope = new ScopedReuse(Expression.Call(typeof(Reuse), "InitScope", null, Container.ResolutionScopeParameter));
        }

        public static Expression GetScopedServiceExpression(Expression scope, int factoryID, Expression factoryExpr)
        {
            return Expression.Call(scope,
                _getOrAddToScopeMethod.MakeGenericMethod(factoryExpr.Type),
                Expression.Constant(factoryID),
                Expression.Lambda(factoryExpr, null));
        }

        // Used by reflection only (inside factory expression).
        public static Scope InitScope(ref Scope scope)
        {
            return scope = scope ?? new Scope();
        }

        #region Implementation

        private static readonly MethodInfo _getOrAddToScopeMethod = typeof(Scope).GetMethod("GetOrAdd");

        private sealed class ScopedReuse : IReuse
        {
            public ScopedReuse(Expression scope)
            {
                _scope = scope;
            }

            public Expression Of(Request _, IRegistry __, int factoryID, Expression factoryExpr)
            {
                return GetScopedServiceExpression(_scope, factoryID, factoryExpr);
            }

            private readonly Expression _scope;
        }

        private sealed class SingletonReuse : IReuse
        {
            public Expression Of(Request request, IRegistry registry, int factoryID, Expression factoryExpr)
            {
                // Create lazy singleton if we have Func somewhere in dependency chain.
                var parent = request.Parent;
                if (parent != null && parent.Enumerate().Any(p =>
                    p.OpenGenericServiceType != null && ContainerSetup.FuncTypes.Contains(p.OpenGenericServiceType)))
                    return GetScopedServiceExpression(Container.SingletonScopeExpression, factoryID, factoryExpr);

                // Create singleton now and put into constants.
                var constants = registry.Constants;
                var singletonScope = (Scope)constants[Container.SINGLETON_SCOPE_CONST_INDEX];
                var singleton = singletonScope.GetOrAdd(factoryID,
                    () => factoryExpr.ToFactoryExpression().CompileFactory().Invoke(constants, null));
                return registry.GetConstantExpression(singleton, factoryExpr.Type);
            }
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

        object[] Constants { get; }

        Factory GetOrAddFactory(Request request, IfUnresolved ifUnresolved);

        Factory GetFactoryOrDefault(Type serviceType, object serviceKey);

        Expression GetDecoratorExpressionOrDefault(Request request);

        IEnumerable<object> GetKeys(Type serviceType, Func<Factory, bool> condition);

        Type GetWrappedServiceTypeOrSelf(Type serviceType);

        Expression GetConstantExpression(object constant, Type constantType);
    }

    public sealed class Many<TService>
    {
        public readonly IEnumerable<TService> Items;

        public Many(IEnumerable<TService> items)
        {
            Items = items;
        }
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
        public readonly Expression<CompiledFactory> Expression;

        public DebugExpression(Expression<CompiledFactory> expression)
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

    public static class TypeTools
    {
        public enum ReturnBaseOpenGenerics { AsGenericTypeDefinition, AsIs }
        public enum IncludeSelf { IncludeAsFirst, Exclude }

        public static Type[] GetImplementedTypes(this Type type,
            ReturnBaseOpenGenerics returnBaseOpenGenerics = ReturnBaseOpenGenerics.AsGenericTypeDefinition,
            IncludeSelf includeSelf = IncludeSelf.IncludeAsFirst)
        {
            Type[] results;

            var interfaces = type.GetInterfaces();
            var interfaceStartIndex = includeSelf == IncludeSelf.IncludeAsFirst ? 1 : 0;
            var selfPlusInterfaceCount = interfaceStartIndex + interfaces.Length;

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

            if (includeSelf == IncludeSelf.IncludeAsFirst)
                results[0] = type;

            if (interfaces.Length == 1)
                results[interfaceStartIndex] = interfaces[0];
            else if (interfaces.Length > 1)
                Array.Copy(interfaces, 0, results, interfaceStartIndex, interfaces.Length);

            if (returnBaseOpenGenerics == ReturnBaseOpenGenerics.AsGenericTypeDefinition &&
                results.Length > interfaceStartIndex && type.IsGenericTypeDefinition)
            {
                for (var i = interfaceStartIndex; i < results.Length; i++)
                {
                    var interfaceOrBase = results[i];
                    if (interfaceOrBase.IsGenericType && interfaceOrBase.ContainsGenericParameters &&
                        !interfaceOrBase.IsGenericTypeDefinition)
                        results[i] = interfaceOrBase.GetGenericTypeDefinition();
                }
            }

            return results;
        }

        public static bool MatchClosedGenericWithBaseOpenGenericTypeArgs(Type[] closedArgs, Type[] openArgs)
        {
            IDictionary<string, Type> matches = new Dictionary<string, Type>();
            return MatchClosedGenericWithBaseOpenGenericTypeArgs(closedArgs, openArgs, ref matches);
        }

        public static bool MatchClosedGenericWithBaseOpenGenericTypeArgs(Type[] closedArgs, Type[] openArgs,
            ref IDictionary<string, Type> matches)
        {
            for (var i = 0; i < openArgs.Length; i++)
            {
                var openArg = openArgs[i];
                var closedArg = closedArgs[i];
                if (!openArg.IsGenericParameter)
                {
                    if (openArg != closedArg)
                    {
                        if (!openArg.IsGenericType || !openArg.ContainsGenericParameters || 
                            !closedArg.IsGenericType ||
                            closedArg.GetGenericTypeDefinition() != openArg.GetGenericTypeDefinition())
                            return false; // openArg and closedArg are different types

                        if (!MatchClosedGenericWithBaseOpenGenericTypeArgs(
                            closedArg.GetGenericArguments(), openArg.GetGenericArguments(), ref matches))
                            return false; // nested generic openArg and closedArg don't match
                    }
                }
                else
                {
                    Type matchedClosedArg;
                    if (!matches.TryGetValue(openArg.Name, out matchedClosedArg))
                        matches.Add(openArg.Name, closedArg);
                    else if (matchedClosedArg != closedArg)
                        return false; // different matchedClosedArg and closedArg are matched with single openArg
                }
            }

            return true;
        }

        public static void GetGenericImplTypeArgsFromBaseType(this Type closedBaseType, Type openBaseType, ref Type[] implTypeArgs)
        {
            var openBaseTypeArgs = openBaseType.GetGenericArguments();
            var closedBaseTypeArgs = closedBaseType.GetGenericArguments();

            for (var i = 0; i < openBaseTypeArgs.Length; i++)
            {
                var baseTypeArg = openBaseTypeArgs[i];
                if (baseTypeArg.IsGenericParameter)
                {
                    var matchingArgIndex = Array.FindIndex(implTypeArgs,
                        implTypeArg => implTypeArg.IsGenericParameter && implTypeArg.Name == baseTypeArg.Name);
                    if (matchingArgIndex != -1)
                        implTypeArgs[matchingArgIndex] = closedBaseTypeArgs[i];
                }
                else if (baseTypeArg.IsGenericType && baseTypeArg.ContainsGenericParameters)
                {
                    closedBaseTypeArgs[i].GetGenericImplTypeArgsFromBaseType(baseTypeArg, ref implTypeArgs);
                }
            }
        }

        public static string Print(this Type type,
            Func<Type, string> print = null /* by default prints Type.FullName or Type.Name for generic parameters */)
        {
            if (type == null) return null;
            // ReSharper disable ConstantNullCoalescingCondition
            var name = print == null ? (type.FullName ?? type.Name) : print(type); // TODO Move default option to setup.
            // ReSharper restore ConstantNullCoalescingCondition
            if (type.IsGenericType) // for generic types
            {
                var genericArgs = type.GetGenericArguments();
                var genericArgsString = type.IsGenericTypeDefinition
                    ? new string(',', genericArgs.Length - 1)
                    : String.Join(", ", genericArgs.Select(x => x.Print(print)).ToArray());
                name = name.Substring(0, name.IndexOf('`')) + "<" + genericArgsString + ">";
            }
            return name.Replace('+', '.'); // for nested classes
        }
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
            if (added.Length == 0)
                return source;
            if (source.Length == 0)
                return added;
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

    /// <summary>
    /// Immutable kind of http://en.wikipedia.org/wiki/AVL_tree, where actual node key is <typeparamref name="K"/> hash code.
    /// </summary>
    public sealed class HashTree<K, V>
    {
        public static readonly HashTree<K, V> Empty = new HashTree<K, V>();

        public readonly int Hash;
        public readonly K Key;
        public readonly V Value;
        public readonly KV<K, V>[] Conflicts;
        public readonly HashTree<K, V> Left, Right;
        public readonly int Height;

        public bool IsEmpty { get { return Height == 0; } }

        public delegate V UpdateValue(V current, V added);

        public HashTree<K, V> AddOrUpdate(K key, V value, UpdateValue updateValue = null)
        {
            return AddOrUpdate(key.GetHashCode(), key, value, updateValue ?? ReplaceValue);
        }

        public V GetValueOrDefault(K key, V defaultValue = default(V))
        {
            var t = this;
            var hash = key.GetHashCode();
            while (t.Height != 0 && t.Hash != hash)
                t = hash < t.Hash ? t.Left : t.Right;
            return t.Height != 0 && (ReferenceEquals(key, t.Key) || key.Equals(t.Key)) ? t.Value
                : t.GetConflictedValueOrDefault(key, defaultValue);
        }

        /// <summary>
        /// Depth-first in-order traversal as described in http://en.wikipedia.org/wiki/Tree_traversal
        /// The only difference is using fixed size array instead of stack for speed-up (~20% faster than stack).
        /// </summary>
        public IEnumerable<KV<K, V>> TraverseInOrder()
        {
            var parents = new HashTree<K, V>[Height];
            var parentCount = -1;
            var t = this;
            while (!t.IsEmpty || parentCount != -1)
            {
                if (!t.IsEmpty)
                {
                    parents[++parentCount] = t;
                    t = t.Left;
                }
                else
                {
                    t = parents[parentCount--];
                    yield return new KV<K, V>(t.Key, t.Value);
                    if (t.Conflicts != null)
                        for (var i = 0; i < t.Conflicts.Length; i++)
                            yield return t.Conflicts[i];
                    t = t.Right;
                }
            }
        }

        #region Implementation

        private HashTree() { }

        private HashTree(int hash, K key, V value, KV<K, V>[] conficts, HashTree<K, V> left, HashTree<K, V> right)
        {
            Hash = hash;
            Key = key;
            Value = value;
            Conflicts = conficts;
            Left = left;
            Right = right;
            Height = 1 + (left.Height > right.Height ? left.Height : right.Height);
        }

        private static V ReplaceValue(V _, V added) { return added; }

        private HashTree<K, V> AddOrUpdate(int hash, K key, V value, UpdateValue updateValue)
        {
            return Height == 0 ? new HashTree<K, V>(hash, key, value, null, Empty, Empty)
                : (hash == Hash ? ResolveConflicts(key, value, updateValue)
                : (hash < Hash
                    ? With(Left.AddOrUpdate(hash, key, value, updateValue), Right)
                    : With(Left, Right.AddOrUpdate(hash, key, value, updateValue)))
                        .EnsureBalanced());
        }

        private HashTree<K, V> ResolveConflicts(K key, V value, UpdateValue updateValue)
        {
            if (ReferenceEquals(Key, key) || Key.Equals(key))
                return new HashTree<K, V>(Hash, key, updateValue(Value, value), Conflicts, Left, Right);

            if (Conflicts == null)
                return new HashTree<K, V>(Hash, Key, Value, new[] { new KV<K, V>(key, value) }, Left, Right);

            var i = Conflicts.Length - 1;
            while (i >= 0 && !Equals(Conflicts[i].Key, Key)) i--;
            var conflicts = new KV<K, V>[i != -1 ? Conflicts.Length : Conflicts.Length + 1];
            Array.Copy(Conflicts, 0, conflicts, 0, Conflicts.Length);
            conflicts[i != -1 ? i : Conflicts.Length] = new KV<K, V>(key, i != -1 ? updateValue(Conflicts[i].Value, value) : value);
            return new HashTree<K, V>(Hash, Key, Value, conflicts, Left, Right);
        }

        private V GetConflictedValueOrDefault(K key, V defaultValue)
        {
            if (Conflicts != null)
                for (var i = 0; i < Conflicts.Length; i++)
                    if (Equals(Conflicts[i].Key, key))
                        return Conflicts[i].Value;
            return defaultValue;
        }

        private HashTree<K, V> EnsureBalanced()
        {
            var delta = Left.Height - Right.Height;
            return delta >= 2 ? With(Left.Right.Height - Left.Left.Height == 1 ? Left.RotateLeft() : Left, Right).RotateRight()
                : (delta <= -2 ? With(Left, Right.Left.Height - Right.Right.Height == 1 ? Right.RotateRight() : Right).RotateLeft()
                : this);
        }

        private HashTree<K, V> RotateRight()
        {
            return Left.With(Left.Left, With(Left.Right, Right));
        }

        private HashTree<K, V> RotateLeft()
        {
            return Right.With(With(Left, Right.Left), Right.Right);
        }

        private HashTree<K, V> With(HashTree<K, V> left, HashTree<K, V> right)
        {
            return new HashTree<K, V>(Hash, Key, Value, Conflicts, left, right);
        }

        #endregion
    }
}