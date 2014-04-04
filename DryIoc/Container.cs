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

namespace DryIoc
{
    /// <summary>
    /// IoC Container. Documentation is available at https://bitbucket.org/dadhi/dryioc.
    /// TODO:
    /// + finished: IsRegistered specialized by factory type.
    /// - change: Use any object as Key, not only String. The condition is support for GetHashCode and Equals.
    /// - add: Keyed{T, TKey}
    /// - add: Resolve as IDictionary{KeyType, ServiceType}.
    /// - finish: CreateChildContainer and CreateScope.
    /// - finish: Unregister.

    /// </summary>
    public class Container : IRegistry, IDisposable
    {
        public static ResolutionRules DefaultResolutionRules = DryIoc.ResolutionRules.Empty.With(
            OpenGenericsSupport.ResolveOpenGenerics,
            OpenGenericsSupport.ResolveEnumerableOrArray);

        public Container(ResolutionRules resolutionRules = null)
        {
            _resolutionRules = Ref.Of(resolutionRules ?? DefaultResolutionRules);

            _factories = Ref.Of(HashTree<Type, object>.Empty);
            _decorators = Ref.Of(HashTree<Type, Factory[]>.Empty);
            _genericWrappers = Ref.Of(OpenGenericsSupport.GenericWrappers);

            _singletonScope = _currentScope = new Scope();

            _resolvedDefaultDelegates = HashTree<Type, FactoryDelegate>.Empty;
            _resolvedKeyedDelegates = HashTree<Type, HashTree<object, FactoryDelegate>>.Empty;
            _resolvedExpressions = new ResolvedExpressions();
        }

        public Container(Container source)
        {
            source.ThrowIfNull();

            _resolutionRules = source._resolutionRules;

            _factories = source._factories;
            _decorators = source._decorators;
            _genericWrappers = source._genericWrappers;

            _singletonScope = source._singletonScope;
            _currentScope = source._currentScope;

            _resolvedDefaultDelegates = source._resolvedDefaultDelegates;
            _resolvedKeyedDelegates = source._resolvedKeyedDelegates;
            _resolvedExpressions = source._resolvedExpressions;
        }

        public Container CreateReuseScope()
        {
            return new Container(this) { _currentScope = new Scope() };
        }

        public Container CreateChildContainer()
        {
            var rules = ResolutionRules.Value;
            rules = rules.With(rules.ToResolveUnregisteredService.Append((request, _) =>
            {
                var parentRegistry = (IRegistry)this;
                var factory = parentRegistry.GetOrAddFactory(request, IfUnresolved.ReturnNull);
                if (factory == null)
                    return null;
                return new DelegateFactory((req, __) => factory.GetExpression(req, parentRegistry));
            }));
            return new Container(rules);
        }

        public void Dispose()
        {
            _currentScope.Dispose();
        }

        public Request CreateRequest(Type serviceType, object serviceKey = null)
        {
            return new Request(_resolvedExpressions, null, serviceType, serviceKey);
        }

        #region IRegistrator

        public void Register(Factory factory, Type serviceType, object serviceKey, IfAlreadyRegistered ifAlreadyRegistered)
        {
            factory.ThrowIfNull().ThrowIfCannotBeRegisteredWithServiceType(serviceType.ThrowIfNull());
            switch (factory.Setup.Type)
            {
                case FactoryType.Decorator:
                    _decorators.Swap(x => x.AddOrUpdate(serviceType, new[] { factory }, ArrayTools.Append));
                    break;
                case FactoryType.GenericWrapper:
                    _genericWrappers.Swap(x => x.AddOrUpdate(serviceType, factory));
                    break;
                default:
                    AddFactory(factory, serviceType, serviceKey, ifAlreadyRegistered);
                    break;
            }
        }

        public bool IsRegistered(Type serviceType, object serviceKey, FactoryType factoryType, Func<Factory, bool> condition)
        {
            serviceType = serviceType.ThrowIfNull();
            switch (factoryType)
            {
                case FactoryType.GenericWrapper:
                    Throw.If(!serviceType.IsGenericType, Error.IS_REGISTERED_FOR_GENERIC_WRAPPER_CALLED_WITH_NONGENERIC_SERVICE_TYPE, serviceType);
                    var wrapper = _genericWrappers.Value.GetValueOrDefault(serviceType.GetGenericTypeDefinition());
                    return wrapper != null && (condition == null || condition(wrapper));

                case FactoryType.Decorator:
                    var decorators = _decorators.Value.GetValueOrDefault(serviceType);
                    return decorators != null && (condition == null || decorators.Any(condition));

                default:
                    var getSingleFactory = condition == null ? (ResolutionRules.GetSingleFactory)
                        ((_, factories) => factories.First()) : ((_, factories) => factories.FirstOrDefault(condition));
                    return GetFactoryOrDefault(serviceType, serviceKey, getSingleFactory, ifNotFoundLookForOpenGenericServiceType: true) != null;
            }
        }

        public enum HandleCache { Wipe, Keep }
        public bool Unregister(Type serviceType, object serviceKey, FactoryType factoryType, Func<Factory, bool> condition, HandleCache handleCache)
        {
            // if type/key is not registered - do nothing Or return false?
            if (!IsRegistered(serviceType, serviceKey, factoryType, condition))
                return false;

            switch (factoryType)
            {
                case FactoryType.GenericWrapper:
                    _genericWrappers.Swap(_ => _.RemoveOrUpdate(serviceType));
                    break;
                case FactoryType.Decorator:
                    _decorators.Swap(_ => _.RemoveOrUpdate(serviceType));
                    break;
                default:
                    _factories.Swap(_ => _.RemoveOrUpdate(serviceType,
                        (Type key, object entry, out object newEntry) =>
                        {
                            newEntry = entry;     // by default keep existing entry
                            if (serviceKey == null)
                            {
                                if (entry is Factory)
                                    return false; // remove node

                                var keyedEntry = ((FactoriesEntry)entry);
                                var factories = keyedEntry.Factories;
                                var indexedFactories = factories.Enumerate().Where(x => x.Key is int).ToArray();
                                if (indexedFactories.Length != 0)
                                {
                                    for (var i = 0; i < indexedFactories.Length; i++)
                                        factories = factories.RemoveOrUpdate(indexedFactories[i].Key);
                                    if (factories.IsEmpty)
                                        return false; // remove node
                                    newEntry = new FactoriesEntry(-1, factories);
                                }
                            }
                            else
                            {
                                if (entry is FactoriesEntry)
                                {
                                    var keyedEntry = ((FactoriesEntry)entry);
                                    var factories = keyedEntry.Factories.RemoveOrUpdate(serviceKey);
                                    if (factories.IsEmpty)
                                        return false;
                                    newEntry = new FactoriesEntry(keyedEntry.LatestIndex, factories);
                                }
                            }

                            return true;
                        }));
                    break;
            }

            if (handleCache == HandleCache.Wipe)
            {
                // wipe cache
            }

            return true;
        }

        #endregion

        #region IResolver

        object IResolver.ResolveDefault(Type serviceType, IfUnresolved ifUnresolved)
        {
            var compiledFactory = _resolvedDefaultDelegates.GetValueOrDefault(serviceType)
                ?? ResolveAndCacheDefaultDelegate(serviceType, ifUnresolved);
            return compiledFactory(_resolvedExpressions.Objects.Value, _currentScope, resolutionScope: null);
        }

        object IResolver.ResolveKeyed(Type serviceType, object serviceKey, IfUnresolved ifUnresolved)
        {
            var factoryDelegates = _resolvedKeyedDelegates.GetValueOrDefault(serviceType);
            if (factoryDelegates != null)
            {
                var factoryDelegate = factoryDelegates.GetValueOrDefault(serviceKey);
                if (factoryDelegate != null)
                    return factoryDelegate(_resolvedExpressions.Objects.Value, _currentScope, resolutionScope: null);
            }

            var request = CreateRequest(serviceType, serviceKey);
            var factory = ((IRegistry)this).GetOrAddFactory(request, ifUnresolved);
            if (factory == null)
                return null;

            var newFactoryDelegate = factory.GetExpression(request, this).CompileToDelegate();
            Interlocked.Exchange(ref _resolvedKeyedDelegates,
                _resolvedKeyedDelegates.AddOrUpdate(serviceType,
                (factoryDelegates ?? HashTree<object, FactoryDelegate>.Empty).AddOrUpdate(serviceKey, newFactoryDelegate)));
            return newFactoryDelegate(_resolvedExpressions.Objects.Value, _currentScope, resolutionScope: null);
        }

        private FactoryDelegate ResolveAndCacheDefaultDelegate(Type serviceType, IfUnresolved ifUnresolved)
        {
            var request = CreateRequest(serviceType);
            var factory = ((IRegistry)this).GetOrAddFactory(request, ifUnresolved);
            if (factory == null)
                return delegate { return null; };
            var newFactoryDelegate = factory.GetExpression(request, this).CompileToDelegate();
            Interlocked.Exchange(ref _resolvedDefaultDelegates,
                _resolvedDefaultDelegates.AddOrUpdate(serviceType, newFactoryDelegate));
            return newFactoryDelegate;
        }

        #endregion

        #region IRegistry

        public Ref<ResolutionRules> ResolutionRules { get { return _resolutionRules; } }

        RegistryWeakRef IRegistry.SelfWeakRef
        {
            get { return _selfWeakRef ?? (_selfWeakRef = new RegistryWeakRef(this)); }
        }

        Scope IRegistry.SingletonScope { get { return _singletonScope; } }
        Scope IRegistry.CurrentScope { get { return _currentScope; } }

        Factory IRegistry.GetOrAddFactory(Request request, IfUnresolved ifUnresolved)
        {
            var rules = ResolutionRules.Value;
            var factory = GetFactoryOrDefault(request.ServiceType, request.ServiceKey, rules.ToGetSingleFactory);
            if (factory != null && factory.ProvidesFactoryPerRequest)
                factory = factory.GetFactoryPerRequestOrDefault(request, this);

            if (factory != null)
                return factory;

            var ruleFactory = rules.ToResolveUnregisteredService.GetFirstNonDefault(r => r(request, this));
            if (ruleFactory != null)
            {
                Register(ruleFactory, request.ServiceType, request.ServiceKey, IfAlreadyRegistered.ThrowIfDuplicateKey);
                return ruleFactory;
            }

            Throw.If(ifUnresolved == IfUnresolved.Throw, Error.UNABLE_TO_RESOLVE_SERVICE, request);
            return null;
        }

        Factory IRegistry.GetFactoryOrDefault(Type serviceType, object serviceKey)
        {
            return GetFactoryOrDefault(
                serviceType.ThrowIfNull(), serviceKey,
                ResolutionRules.Value.ToGetSingleFactory,
                ifNotFoundLookForOpenGenericServiceType: true);
        }

        IEnumerable<KV<object, Factory>> IRegistry.GetAllFactories(Type serviceType)
        {
            var entry = _factories.Value.GetValueOrDefault(serviceType);
            if (entry == null && serviceType.IsGenericType && !serviceType.IsGenericTypeDefinition)
                entry = _factories.Value.GetValueOrDefault(serviceType.GetGenericTypeDefinition());
            return entry == null ? Enumerable.Empty<KV<object, Factory>>()
                : entry is Factory ? new[] { new KV<object, Factory>(0, (Factory)entry) }
                : ((FactoriesEntry)entry).Factories.Enumerate();
        }

        Expression IRegistry.GetDecoratorExpressionOrDefault(Request request)
        {
            // Decorators for non service types are not supported.
            if (request.ResolvedFactory.Setup.Type != FactoryType.Service)
                return null;

            // We are already resolving decorator for the service, so stop now.
            var parent = request.GetNonWrapperParentOrDefault();
            if (parent != null && parent.ResolvedFactory.Setup.Type == FactoryType.Decorator)
                return null;

            var serviceType = request.ServiceType;
            var decoratorFuncType = typeof(Func<,>).MakeGenericType(serviceType, serviceType);

            var decorators = _decorators.Value;
            LambdaExpression resultFuncDecorator = null;
            var funcDecorators = decorators.GetValueOrDefault(decoratorFuncType);
            if (funcDecorators != null)
            {
                for (var i = 0; i < funcDecorators.Length; i++)
                {
                    var decorator = funcDecorators[i];
                    var decoratorRequest = request.ResolveWith(decorator);
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

            var serviceDecorators = decorators.GetValueOrDefault(serviceType);
            var openGenericDecoratorIndex = serviceDecorators == null ? 0 : serviceDecorators.Length;
            var openGenericServiceType = request.OpenGenericServiceType;
            if (openGenericServiceType != null)
                serviceDecorators = serviceDecorators.Append(decorators.GetValueOrDefault(openGenericServiceType));

            Expression resultDecorator = resultFuncDecorator;
            if (serviceDecorators != null)
            {
                for (var i = 0; i < serviceDecorators.Length; i++)
                {
                    var decorator = serviceDecorators[i];
                    var decoratorRequest = request.ResolveWith(decorator);
                    if (((DecoratorSetup)decorator.Setup).IsApplicable(request))
                    {
                        // Cache closed generic registration produced by open-generic decorator.
                        if (i >= openGenericDecoratorIndex && decorator.ProvidesFactoryPerRequest)
                        {
                            decorator = decorator.GetFactoryPerRequestOrDefault(request, this);
                            Register(decorator, serviceType, null, IfAlreadyRegistered.ThrowIfDuplicateKey);
                        }

                        var decoratorExpr = request.ResolvedExpressions.GetCachedOrDefault(decorator.ID);
                        if (decoratorExpr == null)
                        {
                            IList<Type> unusedFunArgs;
                            var funcExpr = decorator
                                .GetFuncWithArgsOrDefault(decoratorFuncType, decoratorRequest, this, out unusedFunArgs)
                                .ThrowIfNull(Error.DECORATOR_FACTORY_SHOULD_SUPPORT_FUNC_RESOLUTION, decoratorFuncType);
                            decoratorExpr = unusedFunArgs != null ? funcExpr.Body : funcExpr;
                            request.ResolvedExpressions.AddToCache(decorator.ID, decoratorExpr);
                        }

                        if (resultDecorator == null || !(decoratorExpr is LambdaExpression))
                            resultDecorator = decoratorExpr;
                        else
                        {
                            if (!(resultDecorator is LambdaExpression))
                                resultDecorator = Expression.Invoke(decoratorExpr, resultDecorator);
                            else
                            {
                                var prevDecorators = ((LambdaExpression)resultDecorator);
                                var decorateDecorator = Expression.Invoke(decoratorExpr, prevDecorators.Body);
                                resultDecorator = Expression.Lambda(decorateDecorator, prevDecorators.Parameters[0]);
                            }
                        }
                    }
                }
            }

            return resultDecorator;
        }

        Factory IRegistry.GetGenericWrapperOrDefault(Type openGenericServiceType)
        {
            return _genericWrappers.Value.GetValueOrDefault(openGenericServiceType);
        }

        Type IRegistry.GetWrappedServiceTypeOrSelf(Type serviceType)
        {
            if (!serviceType.IsGenericType)
                return serviceType;

            var factory = _genericWrappers.Value.GetValueOrDefault(serviceType.GetGenericTypeDefinition());
            if (factory == null || factory.Setup.Type != FactoryType.GenericWrapper)
                return serviceType;

            var wrapperSetup = ((GenericWrapperSetup)factory.Setup);
            var wrappedType = wrapperSetup.GetWrappedServiceType(serviceType.GetGenericArguments());
            return ((IRegistry)this).GetWrappedServiceTypeOrSelf(wrappedType);
        }

        #endregion

        #region Factories Add/Get

        private sealed class FactoriesEntry
        {
            public readonly int? LatestIndex;
            public readonly HashTree<object, Factory> Factories;

            public FactoriesEntry(int? latestIndex, HashTree<object, Factory> factories)
            {
                LatestIndex = latestIndex;
                Factories = factories;
            }
        }

        private void AddFactory(Factory factory, Type serviceType, object serviceKey, IfAlreadyRegistered ifAlreadyRegistered)
        {
            if (serviceKey == null)
            {
                _factories.Swap(x => x.AddOrUpdate(serviceType, factory, (oldValue, _) =>
                {
                    if (oldValue is Factory)
                        return ifAlreadyRegistered == IfAlreadyRegistered.KeepAlreadyRegistered
                            ? oldValue
                            : new FactoriesEntry(1, HashTree<object, Factory>.Empty
                                .AddOrUpdate(0, (Factory)oldValue).AddOrUpdate(1, factory));

                    var oldEntry = ((FactoriesEntry)oldValue);
                    var oldLatestIndex = oldEntry.LatestIndex;
                    if (!oldLatestIndex.HasValue) // if were not default registrations, then add first one.
                        return new FactoriesEntry(0, oldEntry.Factories.AddOrUpdate(0, factory));

                    // if they were, but we need to keep old, just return the old.
                    if (ifAlreadyRegistered == IfAlreadyRegistered.KeepAlreadyRegistered) 
                        return oldValue;

                    var newLatestIndex = oldLatestIndex + 1;
                    return new FactoriesEntry(newLatestIndex, oldEntry.Factories.AddOrUpdate(newLatestIndex, factory));
                }));
            }
            else // for non default service key
            {
                var index = serviceKey is int ? ((int?)serviceKey) : null;
                var newEntry = new FactoriesEntry(index, HashTree<object, Factory>.Empty.AddOrUpdate(serviceKey, factory));
                _factories.Swap(x => x.AddOrUpdate(serviceType, newEntry, (oldValue, _) =>
                {
                    if (oldValue is Factory)
                        return index.HasValue && index == 0 // if default service key
                            ? oldValue.ThrowIf(ifAlreadyRegistered == IfAlreadyRegistered.ThrowIfDuplicateKey,
                                Error.DUPLICATE_SERVICE_KEY, serviceType, "0 (default key)", oldValue)
                            : new FactoriesEntry(index.HasValue && index > 0 ? index : 0,
                                newEntry.Factories.AddOrUpdate(0, (Factory)oldValue));

                    var oldEntry = ((FactoriesEntry)oldValue);
                    var oldLatestIndex = oldEntry.LatestIndex;

                    var newLatestIndex = !index.HasValue ? oldLatestIndex : !oldLatestIndex.HasValue ? index
                        : index > oldLatestIndex ? index : oldLatestIndex;
                        
                    return new FactoriesEntry(newLatestIndex,
                        oldEntry.Factories.AddOrUpdate(serviceKey, factory, (oldFactory, __) =>
                            oldFactory.ThrowIf(ifAlreadyRegistered == IfAlreadyRegistered.ThrowIfDuplicateKey,
                                Error.DUPLICATE_SERVICE_KEY, serviceType, serviceKey, oldFactory)));
                }));
            }
        }

        private Factory GetFactoryOrDefault(
            Type serviceType, object serviceKey,
            ResolutionRules.GetSingleFactory getSingleRegisteredFactory,
            bool ifNotFoundLookForOpenGenericServiceType = false)
        {
            var entry = _factories.Value.GetValueOrDefault(serviceType);
            if (entry == null && ifNotFoundLookForOpenGenericServiceType &&
                serviceType.IsGenericType && !serviceType.IsGenericTypeDefinition)
                entry = _factories.Value.GetValueOrDefault(serviceType.GetGenericTypeDefinition());

            if (entry != null)
            {
                if (entry is Factory)
                    return serviceKey == null || (serviceKey is int && (int)serviceKey == 0)
                        ? (Factory)entry : null;

                var factories = ((FactoriesEntry)entry).Factories;
                if (serviceKey != null)
                    return factories.GetValueOrDefault(serviceKey);

                var indexedFactories = factories.Enumerate().Where(x => x.Key is int).Select(x => x.Value).ToArray();
                if (indexedFactories.Length == 1)
                    return indexedFactories[0];

                if (indexedFactories.Length > 1)
                    return getSingleRegisteredFactory
                        .ThrowIfNull(Error.EXPECTED_SINGLE_DEFAULT_FACTORY, serviceType, factories)
                        .Invoke(serviceType, indexedFactories);
            }

            return null;
        }

        #endregion

        #region Internal State

        private RegistryWeakRef _selfWeakRef;
        private readonly Ref<ResolutionRules> _resolutionRules;

        private readonly Ref<HashTree<Type, object>> _factories; // where object is Factory or KeyedFactoriesEntry
        private readonly Ref<HashTree<Type, Factory[]>> _decorators;
        private readonly Ref<HashTree<Type, Factory>> _genericWrappers;

        private readonly Scope _singletonScope;
        private Scope _currentScope;

        private HashTree<Type, FactoryDelegate> _resolvedDefaultDelegates;
        private HashTree<Type, HashTree<object, FactoryDelegate>> _resolvedKeyedDelegates;
        private readonly ResolvedExpressions _resolvedExpressions;

        #endregion
    }

    public sealed class ResolvedExpressions
    {
        public static readonly ParameterExpression ObjectsParameter = Expression.Parameter(typeof(AppendableArray<object>), "objects");
        public static readonly ParameterExpression CurrentScopeParameter = Expression.Parameter(typeof(Scope), "currentScope");
        public static readonly ParameterExpression ResolutionScopeParameter = Expression.Parameter(typeof(Scope), "resolutionScope");

        public readonly Ref<AppendableArray<object>> Objects = Ref.Of(AppendableArray<object>.Empty);
        public HashTree<int, Expression> FactoryExpressions = HashTree<int, Expression>.Empty;

        public Expression ToExpression(object obj, Type objectType)
        {
            var index = -1;
            Objects.Swap(x =>
            {
                index = x.IndexOf(obj);
                if (index == -1)
                    index = (x = x.Append(obj)).Length - 1;
                return x;
            });

            var indexExpr = Expression.Constant(index.ThrowIf(index == -1), typeof(int));
            var objectExpr = Expression.Call(ObjectsParameter, _appendableArrayGetMethod, indexExpr);
            return Expression.Convert(objectExpr, objectType);
        }

        public Expression ToExpression<T>(T obj)
        {
            return ToExpression(obj, typeof(T));
        }

        public Expression ToExpression(IRegistry registry)
        {
            return Expression.Property(ToExpression(registry.SelfWeakRef), "Target");
        }

        public Expression GetCachedOrDefault(int factoryID)
        {
            return FactoryExpressions.GetFirstValueByHashOrDefault(factoryID);
        }

        public void AddToCache(int factoryID, Expression factoryExpression)
        {
            Interlocked.Exchange(ref FactoryExpressions, FactoryExpressions.AddOrUpdate(factoryID, factoryExpression));
        }

        private static readonly MethodInfo _appendableArrayGetMethod = typeof(AppendableArray<object>).GetMethod("Get");
    }

    public sealed class RegistryWeakRef
    {
        public RegistryWeakRef(IRegistry registry)
        {
            _weakRef = new WeakReference(registry);
        }

        public IRegistry Target
        {
            get { return (_weakRef.Target as IRegistry).ThrowIfNull(Error.CONTAINER_IS_GARBAGE_COLLECTED); }
        }

        private readonly WeakReference _weakRef;
    }

    public sealed class AppendableArray<T>
    {
        public static readonly AppendableArray<T> Empty = new AppendableArray<T>();

        public readonly int Length;

        public AppendableArray<T> Append(T value)
        {
            return new AppendableArray<T>(Length + 1,
                _tree.AddOrUpdate(Length >> NODE_ARRAY_BIT_COUNT, new[] { value }, ArrayTools.Append));
        }

        public int IndexOf(object value, int defaultIndex = -1)
        {
            foreach (var node in _tree.Enumerate())
            {
                var indexInNode = node.Value.IndexOf(x => ReferenceEquals(x, value) || Equals(x, value));
                if (indexInNode != -1)
                    return node.Key << NODE_ARRAY_BIT_COUNT | indexInNode;
            }

            return defaultIndex;
        }

        public object Get(int index)
        {
            return index <= NODE_ARRAY_BITS ? _tree.Value[index]
                : _tree.GetFirstValueByHashOrDefault(index >> NODE_ARRAY_BIT_COUNT)[index & NODE_ARRAY_BITS];
        }

        #region Implementation

        private const int NODE_ARRAY_BITS = 31;     // (11111 binary). So the array would be size of 32. Make it 15 (1111) and BIT_COUNT=4 for array of size 16
        private const int NODE_ARRAY_BIT_COUNT = 5; // number of bits in NODE_ARRAY_BITS.

        private readonly HashTree<int, T[]> _tree;

        private AppendableArray() : this(0, HashTree<int, T[]>.Empty) { }

        private AppendableArray(int length, HashTree<int, T[]> tree)
        {
            Length = length;
            _tree = tree;
        }

        #endregion
    }

    public delegate object FactoryDelegate(AppendableArray<object> objects, Scope currentScope, Scope resolutionScope);

    public static partial class FactoryCompiler
    {
        public static Expression<FactoryDelegate> ToFactoryExpression(this Expression expression)
        {
            // Removing not required Convert from expression root, because CompiledFactory result still be converted at the end.
            if (expression.NodeType == ExpressionType.Convert)
                expression = ((UnaryExpression)expression).Operand;
            return Expression.Lambda<FactoryDelegate>(expression,
                ResolvedExpressions.ObjectsParameter, ResolvedExpressions.CurrentScopeParameter, ResolvedExpressions.ResolutionScopeParameter);
        }

        public static FactoryDelegate CompileToDelegate(this Expression expression)
        {
            var factoryExpression = expression.ToFactoryExpression();
            FactoryDelegate factoryDelegate = null;
            CompileToMethod(factoryExpression, ref factoryDelegate);
            // ReSharper disable ConstantNullCoalescingCondition
            return factoryDelegate ?? factoryExpression.Compile();
            // ReSharper restore ConstantNullCoalescingCondition
        }

        // Partial method definition to be implemented in .NET40 version of Container.
        // It is optional and fine to be not implemented.
        static partial void CompileToMethod(Expression<FactoryDelegate> factoryExpr, ref FactoryDelegate result);
    }

    public static class OpenGenericsSupport
    {
        public static readonly Type[] FuncTypes = { typeof(Func<>), typeof(Func<,>), typeof(Func<,,>), typeof(Func<,,,>), typeof(Func<,,,,>) };
        public static readonly HashTree<Type, Factory> GenericWrappers;

        static OpenGenericsSupport()
        {
            GenericWrappers = HashTree<Type, Factory>.Empty;
            GenericWrappers = GenericWrappers.AddOrUpdate(typeof(Many<>),
                new FactoryProvider(
                    (_, __) => new DelegateFactory(GetManyExpression),
                    GenericWrapperSetup.Default));

            var funcFactory = new FactoryProvider(
                (_, __) => new DelegateFactory(GetFuncExpression),
                GenericWrapperSetup.With(t => t[t.Length - 1]));
            foreach (var funcType in FuncTypes)
                GenericWrappers = GenericWrappers.AddOrUpdate(funcType, funcFactory);

            GenericWrappers = GenericWrappers.AddOrUpdate(typeof(Lazy<>),
                new ReflectionFactory(typeof(Lazy<>),
                    getConstructor: t => t.GetConstructor(new[] { typeof(Func<>).MakeGenericType(t.GetGenericArguments()) }),
                    setup: GenericWrapperSetup.Default));

            GenericWrappers = GenericWrappers.AddOrUpdate(typeof(Meta<,>),
                new FactoryProvider(GetMetaFactoryOrDefault, GenericWrapperSetup.With(t => t[0])));

            GenericWrappers = GenericWrappers.AddOrUpdate(typeof(DebugExpression<>),
                new FactoryProvider((_, __) => new DelegateFactory(GetDebugExpression), GenericWrapperSetup.Default));
        }

        public static readonly ResolutionRules.ResolveUnregisteredService ResolveOpenGenerics = (request, registry) =>
        {
            var openGenericServiceType = request.OpenGenericServiceType;
            if (openGenericServiceType == null)
                return null;

            var factory = registry.GetFactoryOrDefault(openGenericServiceType, request.ServiceKey)
                ?? registry.GetGenericWrapperOrDefault(openGenericServiceType);

            if (factory != null && factory.ProvidesFactoryPerRequest)
                factory = factory.GetFactoryPerRequestOrDefault(request, registry);

            return factory;
        };

        public static readonly ResolutionRules.ResolveUnregisteredService ResolveEnumerableOrArray = (request, registry) =>
        {
            if (!request.ServiceType.IsArray && request.OpenGenericServiceType != typeof(IEnumerable<>))
                return null;

            return new DelegateFactory(
                setup: GenericWrapperSetup.Default,
                getExpression: (req, reg) =>
                {
                    var collectionType = req.ServiceType;

                    var itemType = collectionType.IsArray
                        ? collectionType.GetElementType()
                        : collectionType.GetGenericArguments()[0];

                    var wrappedItemType = reg.GetWrappedServiceTypeOrSelf(itemType);

                    // Composite pattern support: filter out composite root from available keys.
                    var items = reg.GetAllFactories(wrappedItemType);
                    var parent = req.GetNonWrapperParentOrDefault();
                    if (parent != null && parent.ServiceType == wrappedItemType)
                    {
                        var parentFactoryID = parent.ResolvedFactory.ID;
                        items = items.Where(x => x.Value.ID != parentFactoryID);
                    }

                    var itemArray = items.ToArray();
                    Throw.If(itemArray.Length == 0, Error.UNABLE_TO_FIND_REGISTERED_ENUMERABLE_ITEMS, wrappedItemType,
                        req);

                    var itemExpressions = new List<Expression>(itemArray.Length);
                    for (var i = 0; i < itemArray.Length; i++)
                    {
                        var item = itemArray[i];
                        var itemRequest = req.Push(itemType, item.Key);
                        var itemFactory = reg.GetOrAddFactory(itemRequest, IfUnresolved.ReturnNull);
                        if (itemFactory != null)
                            itemExpressions.Add(itemFactory.GetExpression(itemRequest, registry));
                    }

                    Throw.If(itemExpressions.Count == 0, Error.UNABLE_TO_RESOLVE_ENUMERABLE_ITEMS, itemType, req);
                    var newArrayExpr = Expression.NewArrayInit(itemType.ThrowIfNull(), itemExpressions);
                    return newArrayExpr;
                });
        };

        public static Expression GetManyExpression(Request request, IRegistry registry)
        {
            var dynamicEnumerableType = request.ServiceType;
            var itemType = dynamicEnumerableType.GetGenericArguments()[0];

            var wrappedItemType = registry.GetWrappedServiceTypeOrSelf(itemType);

            // Composite pattern support: filter out composite root from available keys.
            var parentFactoryID = 0;
            var parent = request.GetNonWrapperParentOrDefault();
            if (parent != null && parent.ServiceType == wrappedItemType)
                parentFactoryID = parent.ResolvedFactory.ID;

            var resolveMethod = _resolveManyDynamicallyMethod.MakeGenericMethod(itemType, wrappedItemType);

            var registryRefExpr = request.ResolvedExpressions.ToExpression(registry.SelfWeakRef);
            var resolveCallExpr = Expression.Call(resolveMethod, registryRefExpr, Expression.Constant(parentFactoryID));

            return Expression.New(dynamicEnumerableType.GetConstructors()[0], resolveCallExpr);
        }

        public static Expression GetFuncExpression(Request request, IRegistry registry)
        {
            var funcType = request.ServiceType;
            var funcTypeArgs = funcType.GetGenericArguments();
            var serviceType = funcTypeArgs[funcTypeArgs.Length - 1];

            var serviceRequest = request.Push(serviceType, request.ServiceKey);
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
            var serviceRequest = request.Push(serviceType, request.ServiceKey);
            var factory = registry.GetOrAddFactory(serviceRequest, IfUnresolved.Throw);
            var factoryExpr = factory.GetExpression(serviceRequest, registry).ToFactoryExpression();
            return Expression.New(ctor, request.ResolvedExpressions.ToExpression(factoryExpr));
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
                var result = registry.GetAllFactories(wrappedServiceType).FirstOrDefault(kv =>
                    kv.Value.Setup.Metadata != null && metadataType.IsInstanceOfType(kv.Value.Setup.Metadata));
                if (result != null)
                {
                    serviceKey = result.Key;
                    resultMetadata = result.Value.Setup.Metadata;
                }
            }
            else
            {
                var factory = registry.GetFactoryOrDefault(wrappedServiceType, serviceKey);
                if (factory != null)
                {
                    var metadata = factory.Setup.Metadata;
                    resultMetadata = metadata != null && metadataType.IsInstanceOfType(metadata) ? metadata : null;
                }
            }

            if (resultMetadata == null)
                return null;

            return new DelegateFactory((req, _) =>
            {
                var serviceRequest = req.Push(serviceType, serviceKey);
                var serviceExpr = registry.GetOrAddFactory(serviceRequest, IfUnresolved.Throw).GetExpression(serviceRequest, registry);
                var metaCtor = req.ServiceType.GetConstructors()[0];
                var metadataExpr = req.ResolvedExpressions.ToExpression(resultMetadata, metadataType);
                return Expression.New(metaCtor, serviceExpr, metadataExpr);
            });
        }

        #region Implementation

        private static readonly MethodInfo _resolveManyDynamicallyMethod =
            typeof(OpenGenericsSupport).GetMethod("ResolveManyDynamically", BindingFlags.Static | BindingFlags.NonPublic);

        internal static IEnumerable<TService> ResolveManyDynamically<TService, TWrappedService>(
            RegistryWeakRef registryWeakRef, int parentFactoryID)
        {
            var itemType = typeof(TService);
            var wrappedItemType = typeof(TWrappedService);
            var registry = registryWeakRef.Target;

            var items = registry.GetAllFactories(wrappedItemType);
            if (parentFactoryID != -1)
                items = items.Where(kv => kv.Value.ID != parentFactoryID);

            foreach (var item in items)
            {
                var service = registry.ResolveKeyed(itemType, item.Key, IfUnresolved.ReturnNull);
                if (service != null) // skip unresolved items
                    yield return (TService)service;
            }
        }

        #endregion
    }

    public sealed class ResolutionRules
    {
        public static readonly ResolutionRules Empty = new ResolutionRules();

        public delegate Factory GetSingleFactory(Type serviceType, IEnumerable<Factory> factories);
        public GetSingleFactory ToGetSingleFactory { get; private set; }
        public ResolutionRules With(GetSingleFactory toGetSingleFactory)
        {
            return new ResolutionRules(this) { ToGetSingleFactory = toGetSingleFactory };
        }

        public delegate Factory ResolveUnregisteredService(Request request, IRegistry registry);
        public ResolveUnregisteredService[] ToResolveUnregisteredService { get; private set; }
        public ResolutionRules With(params ResolveUnregisteredService[] toResolveUnregisteredService)
        {
            return new ResolutionRules(this) { ToResolveUnregisteredService = toResolveUnregisteredService };
        }

        public delegate object ResolveConstructorParameterServiceKey(ParameterInfo parameter, Request parent, IRegistry registry);
        public ResolveConstructorParameterServiceKey[] ToResolveConstructorParameterServiceKey { get; private set; }
        public ResolutionRules With(params ResolveConstructorParameterServiceKey[] toResolveConstructorParameterServiceKey)
        {
            return new ResolutionRules(this) { ToResolveConstructorParameterServiceKey = toResolveConstructorParameterServiceKey };
        }

        public static readonly BindingFlags PropertyOrFieldFlags = BindingFlags.Public | BindingFlags.Instance;

        public delegate bool ResolvePropertyOrFieldWithServiceKey(out object key, MemberInfo member, Request parent, IRegistry registry);
        public ResolvePropertyOrFieldWithServiceKey[] ToResolvePropertyOrFieldWithServiceKey { get; private set; }
        public ResolutionRules With(params ResolvePropertyOrFieldWithServiceKey[] toResolvePropertyOrFieldWithServiceKey)
        {
            return new ResolutionRules(this) { ToResolvePropertyOrFieldWithServiceKey = toResolvePropertyOrFieldWithServiceKey };
        }

        #region Implementation

        private ResolutionRules() { }

        private ResolutionRules(ResolutionRules rules)
        {
            ToGetSingleFactory = rules.ToGetSingleFactory;
            ToResolveUnregisteredService = rules.ToResolveUnregisteredService;
            ToResolveConstructorParameterServiceKey = rules.ToResolveConstructorParameterServiceKey;
            ToResolvePropertyOrFieldWithServiceKey = rules.ToResolvePropertyOrFieldWithServiceKey;
        }

        #endregion
    }

    public static class Error
    {
        public static readonly string UNABLE_TO_RESOLVE_SERVICE =
            "Unable to resolve service {0}.\n Please register service OR adjust resolution rules.";

        public static readonly string UNSUPPORTED_FUNC_WITH_ARGS =
            "Unsupported resolution as {0} of {1}.";

        public static readonly string EXPECTED_IMPL_TYPE_ASSIGNABLE_TO_SERVICE_TYPE =
            "Expecting implementation type {0} to be assignable to service type {1} but it is not.";

        public static readonly string UNABLE_TO_REGISTER_NON_FACTORY_PROVIDER_FOR_OPEN_GENERIC_SERVICE =
            "Unable to register not a factory provider for open-generic service {0}.";

        public static readonly string UNABLE_TO_REGISTER_OPEN_GENERIC_IMPL_WITH_NON_GENERIC_SERVICE =
            "Unable to register open-generic implementation {0} with non-generic service {1}.";

        public static readonly string UNABLE_TO_REGISTER_OPEN_GENERIC_IMPL_CAUSE_SERVICE_DOES_NOT_SPECIFY_ALL_TYPE_ARGS =
            "Unable to register open-generic implementation {0} because service {1} should specify all of its type arguments, but specifies only {2}.";

        public static readonly string USUPPORTED_REGISTRATION_OF_NON_GENERIC_IMPL_TYPE_DEFINITION_BUT_WITH_GENERIC_ARGS =
            "Unsupported registration of implementation {0} which is not a generic type definition but contains generic parameters.\n" +
            "Consider to register generic type definition {1} instead.";

        public static readonly string USUPPORTED_REGISTRATION_OF_NON_GENERIC_SERVICE_TYPE_DEFINITION_BUT_WITH_GENERIC_ARGS =
            "Unsupported registration of service {0} which is not a generic type definition but contains generic parameters.\n" +
            "Consider to register generic type definition {1} instead.";

        public static readonly string EXPECTED_SINGLE_DEFAULT_FACTORY =
            "Expecting single default registration of {0} but found many:\n{1}.";

        public static readonly string EXPECTED_NON_ABSTRACT_IMPL_TYPE =
            "Expecting not abstract and not interface implementation type, but found {0}.";

        public static readonly string NO_PUBLIC_CONSTRUCTOR_DEFINED =
            "There is no public constructor defined for {0}.";

        public static readonly string CONSTRUCTOR_MISSES_SOME_PARAMETERS =
            "Constructor [{0}] of {1} misses some arguments required for {2} dependency.";

        public static readonly string UNABLE_TO_SELECT_CONSTRUCTOR =
            "Unable to select single constructor from {0} available in {1}.\n" +
            "Please provide constructor selector when registering service.";

        public static readonly string EXPECTED_FUNC_WITH_MULTIPLE_ARGS =
            "Expecting Func with one or more arguments but found {0}.";

        public static readonly string EXPECTED_CLOSED_GENERIC_SERVICE_TYPE =
            "Expecting closed-generic service type but found {0}.";

        public static readonly string RECURSIVE_DEPENDENCY_DETECTED =
            "Recursive dependency is detected in resolution of:\n{0}.";

        public static readonly string SCOPE_IS_DISPOSED =
            "Scope is disposed and scoped instances are no longer available.";

        public static readonly string CONTAINER_IS_GARBAGE_COLLECTED =
            "Container is no longer available (has been garbage-collected).";

        public static readonly string DUPLICATE_SERVICE_KEY =
            "Service {0} with the same key '{1}' is already registered as {2}.";

        public static readonly string GENERIC_WRAPPER_EXPECTS_SINGLE_TYPE_ARG_BY_DEFAULT =
            "Generic Wrapper is working with single service type only, but found many:\n{0}.\n" +
            "Please specify service type selector in Generic Wrapper setup upon registration.";

        public static readonly string SOME_FUNC_PARAMS_ARE_UNUSED =
            "Found some unused Func parameters:\n{0}\nwhen resolving {1}.";

        public static readonly string DECORATOR_FACTORY_SHOULD_SUPPORT_FUNC_RESOLUTION =
            "Decorator factory should support resolution as {0}, but it does not.";

        public static readonly string UNABLE_TO_FIND_REGISTERED_ENUMERABLE_ITEMS =
            "Unable to find registered services of item type (unwrapped) {0} when resolving {1}.";

        public static readonly string UNABLE_TO_RESOLVE_ENUMERABLE_ITEMS =
            "Unable to resolve any service of item type {0} when resolving {1}.";

        public static readonly string DELEGATE_FACTORY_EXPRESSION_RETURNED_NULL =
            "Delegate factory expression returned NULL when resolving {0}.";

        public static readonly string UNABLE_TO_MATCH_IMPL_BASE_TYPES_WITH_SERVICE_TYPE =
            "Unable to match service with any of open-generic implementation {0} implemented types {1} when resolving {2}.";

        public static readonly string UNABLE_TO_FIND_OPEN_GENERIC_IMPL_TYPE_ARG_IN_SERVICE =
            "Unable to find for open-generic implementation {0} the type argument {1} when resolving {2}.";

        public static readonly string IS_REGISTERED_FOR_GENERIC_WRAPPER_CALLED_WITH_NONGENERIC_SERVICE_TYPE =
            "IsRegistered for GenericWrapper called with non generic service type {0}.";
    }

    public static class Registrator
    {
        /// <summary>
        /// Registers service <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="serviceType">The service type to register</param>
        /// <param name="factory"><see cref="Factory"/> details object.</param>
        /// <param name="named">Optional service key (name). Could be of any type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="ifAlreadyRegistered">Optional policy to deal with case when service with such type and name is already registered.</param>
        public static void Register(this IRegistrator registrator, Type serviceType, Factory factory,
            object named = null, IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.ThrowIfDuplicateKey)
        {
            registrator.Register(factory, serviceType, named, ifAlreadyRegistered);
        }

        /// <summary>
        /// Registers service of <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of service.</typeparam>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="factory"><see cref="Factory"/> details object.</param>
        /// <param name="named">Optional service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="ifAlreadyRegistered">Optional policy to deal with case when service with such type and name is already registered.</param>
        public static void Register<TService>(this IRegistrator registrator, Factory factory,
            object named = null, IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.ThrowIfDuplicateKey)
        {
            registrator.Register(factory, typeof(TService), named, ifAlreadyRegistered);
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
        /// <param name="named">Optional service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="ifAlreadyRegistered">Optional policy to deal with case when service with such type and name is already registered.</param>
        public static void Register(this IRegistrator registrator, Type serviceType,
            Type implementationType, IReuse reuse = null, GetConstructor getConstructor = null, FactorySetup setup = null,
            object named = null, IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.ThrowIfDuplicateKey)
        {
            var factory = new ReflectionFactory(implementationType, reuse, getConstructor, setup);
            registrator.Register(factory, serviceType, named, ifAlreadyRegistered);
        }

        /// <summary>
        /// Registers service of <paramref name="implementationType"/>. ServiceType will be the same as <paramref name="implementationType"/>.
        /// </summary>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="implementationType">Implementation type. Concrete and open-generic class are supported.</param>
        /// <param name="reuse">Optional <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="withConstructor">Optional strategy to select constructor when multiple available.</param>
        /// <param name="setup">Optional factory setup, by default is (<see cref="ServiceSetup"/>)</param>
        /// <param name="named">Optional service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="ifAlreadyRegistered">Optional policy to deal with case when service with such type and name is already registered.</param>
        public static void Register(this IRegistrator registrator,
            Type implementationType, IReuse reuse = null, GetConstructor withConstructor = null, FactorySetup setup = null,
            object named = null, IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.ThrowIfDuplicateKey)
        {
            var factory = new ReflectionFactory(implementationType, reuse, withConstructor, setup);
            registrator.Register(factory, implementationType, named, ifAlreadyRegistered);
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
        /// <param name="named">Optional service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="ifAlreadyRegistered">Optional policy to deal with case when service with such type and name is already registered.</param>
        public static void Register<TService, TImplementation>(this IRegistrator registrator,
            IReuse reuse = null, GetConstructor withConstructor = null, FactorySetup setup = null,
            object named = null, IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.ThrowIfDuplicateKey)
            where TImplementation : TService
        {
            var factory = new ReflectionFactory(typeof(TImplementation), reuse, withConstructor, setup);
            registrator.Register(factory, typeof(TService), named, ifAlreadyRegistered);
        }

        /// <summary>
        /// Registers implementation type <typeparamref name="TImplementation"/> with itself as service type.
        /// </summary>
        /// <typeparam name="TImplementation">The type of service.</typeparam>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="reuse">Optional <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="withConstructor">Optional strategy to select constructor when multiple available.</param>
        /// <param name="setup">Optional factory setup, by default is (<see cref="ServiceSetup"/>)</param>
        /// <param name="named">Optional service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="ifAlreadyRegistered">Optional policy to deal with case when service with such type and name is already registered.</param>
        public static void Register<TImplementation>(this IRegistrator registrator,
            IReuse reuse = null, GetConstructor withConstructor = null, FactorySetup setup = null,
            object named = null, IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.ThrowIfDuplicateKey)
        {
            var factory = new ReflectionFactory(typeof(TImplementation), reuse, withConstructor, setup);
            registrator.Register(factory, typeof(TImplementation), named, ifAlreadyRegistered);
        }

        public static Func<Type, bool> RegisterAllDefaultTypes = t => (t.IsPublic || t.IsNestedPublic) && t != typeof(object);

        /// <summary>
        /// Registers single registration for all implemented public interfaces and base classes.
        /// </summary>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="implementationType">Service implementation type. Concrete and open-generic class are supported.</param>
        /// <param name="reuse">Optional <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="withConstructor">Optional strategy to select constructor when multiple available.</param>
        /// <param name="setup">Optional factory setup, by default is (<see cref="ServiceSetup"/>)</param>
        /// <param name="types">Optional condition to include selected types only. Default value is <see cref="RegisterAllDefaultTypes"/></param>
        /// <param name="named">Optional service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="ifAlreadyRegistered">Optional policy to deal with case when service with such type and name is already registered.</param>
        public static void RegisterAll(this IRegistrator registrator,
            Type implementationType, IReuse reuse = null, GetConstructor withConstructor = null, FactorySetup setup = null,
            Func<Type, bool> types = null, object named = null, IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.ThrowIfDuplicateKey)
        {
            var registration = new ReflectionFactory(implementationType, reuse, withConstructor, setup);

            var implementedTypes = implementationType.GetImplementedTypes(TypeTools.IncludeItself.AsFirst);
            var implementedServiceTypes = implementedTypes.Where(types ?? RegisterAllDefaultTypes);
            if (implementationType.IsGenericTypeDefinition)
            {
                var implTypeArgs = implementationType.GetGenericArguments();
                implementedServiceTypes = implementedServiceTypes
                    .Where(t => t.IsGenericType && t.ContainsGenericParameters && t.ContainsAllGenericParameters(implTypeArgs))
                    .Select(t => t.GetGenericTypeDefinition());
            }

            var atLeastOneRegistered = false;
            foreach (var serviceType in implementedServiceTypes)
            {
                registrator.Register(registration, serviceType, named, ifAlreadyRegistered);
                atLeastOneRegistered = true;
            }

            Throw.If(!atLeastOneRegistered, "Unable to register any of implementation {0} implemented services {1}.",
                implementationType, implementedTypes);
        }

        /// <summary>
        /// Registers single registration for all implemented public interfaces and base classes.
        /// </summary>
        /// <typeparam name="TImplementation">The type of service.</typeparam>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="reuse">Optional <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="withConstructor">Optional strategy to select constructor when multiple available.</param>
        /// <param name="setup">Optional factory setup, by default is (<see cref="ServiceSetup"/>)</param>
        /// <param name="named">Optional service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="ifAlreadyRegistered">Optional policy to deal with case when service with such type and name is already registered.</param>
        public static void RegisterAll<TImplementation>(this IRegistrator registrator,
            IReuse reuse = null, GetConstructor withConstructor = null, FactorySetup setup = null,
            object named = null, IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.ThrowIfDuplicateKey)
        {
            registrator.RegisterAll(typeof(TImplementation), reuse, withConstructor, setup, null, named, ifAlreadyRegistered);
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
        /// <param name="named">Optional service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="ifAlreadyRegistered">Optional policy to deal with case when service with such type and name is already registered.</param>
        public static void RegisterDelegate<TService>(this IRegistrator registrator,
            Func<IResolver, TService> lambda, IReuse reuse = null, FactorySetup setup = null,
            object named = null, IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.ThrowIfDuplicateKey)
        {
            var factory = new DelegateFactory((request, registry) =>
                Expression.Invoke(request.ResolvedExpressions.ToExpression(lambda), request.ResolvedExpressions.ToExpression(registry)),
                reuse, setup);
            registrator.Register(factory, typeof(TService), named, ifAlreadyRegistered);
        }

        /// <summary>
        /// Registers a pre-created service instance of <typeparamref name="TService"/> 
        /// </summary>
        /// <typeparam name="TService">The type of service.</typeparam>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="instance">The pre-created instance of <typeparamref name="TService"/>.</param>
        /// <param name="setup">Optional factory setup, by default is (<see cref="ServiceSetup"/>)</param>
        /// <param name="named">Optional service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="ifAlreadyRegistered">Optional policy to deal with case when service with such type and name is already registered.</param>
        public static void RegisterInstance<TService>(this IRegistrator registrator,
            TService instance, FactorySetup setup = null,
            object named = null, IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.ThrowIfDuplicateKey)
        {
            registrator.RegisterDelegate(_ => instance, Reuse.Transient, setup, named, ifAlreadyRegistered);
        }

        /// <summary>
        /// Returns true if <paramref name="serviceType"/> is registered in container or its open generic definition is registered in container.
        /// </summary>
        /// <param name="registrator">Usually <see cref="Container"/> to explore or any other <see cref="IRegistrator"/> implementation.</param>
        /// <param name="serviceType">The type of the registered service.</param>
        /// <param name="named">Optional service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="factoryType">Optional factory type to lookup, <see cref="FactoryType.Service"/> by default.</param>
        /// <param name="condition">Optional condition to specify what registered factory do you expect.</param>
        /// <returns>True if <paramref name="serviceType"/> is registered, false - otherwise.</returns>
        public static bool IsRegistered(this IRegistrator registrator, Type serviceType,
            object named = null, FactoryType factoryType = FactoryType.Service, Func<Factory, bool> condition = null)
        {
            return registrator.IsRegistered(serviceType, named, factoryType, condition);
        }

        /// <summary>
        /// Returns true if <typeparamref name="TService"/> type is registered in container or its open generic definition is registered in container.
        /// </summary>
        /// <typeparam name="TService">The type of service.</typeparam>
        /// <param name="registrator">Usually <see cref="Container"/> to explore or any other <see cref="IRegistrator"/> implementation.</param>
        /// <param name="named">Optional service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="factoryType">Optional factory type to lookup, <see cref="FactoryType.Service"/> by default.</param>
        /// <param name="condition">Optional condition to specify what registered factory do you expect.</param>
        /// <returns>True if <typeparamref name="TService"/> name="serviceType"/> is registered, false - otherwise.</returns>
        public static bool IsRegistered<TService>(this IRegistrator registrator,
            object named = null, FactoryType factoryType = FactoryType.Service, Func<Factory, bool> condition = null)
        {
            return registrator.IsRegistered(typeof(TService), named, factoryType, condition);
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
        /// <param name="serviceKey">Service key (any type with <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/> defined).</param>
        /// <param name="ifUnresolved">Optional, say to how to handle unresolved service case.</param>
        /// <returns>The requested service instance.</returns>
        public static object Resolve(this IResolver resolver, Type serviceType, object serviceKey, IfUnresolved ifUnresolved = IfUnresolved.Throw)
        {
            return serviceKey == null
                ? resolver.ResolveDefault(serviceType, ifUnresolved)
                : resolver.ResolveKeyed(serviceType, serviceKey, ifUnresolved);
        }

        /// <summary>
        /// Returns an instance of statically known <typepsaramref name="TService"/> type.
        /// </summary>
        /// <typeparam name="TService">The type of the requested service.</typeparam>
        /// <param name="resolver">Any <see cref="IResolver"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="serviceKey">Service key (any type with <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/> defined).</param>
        /// <param name="ifUnresolved">Optional, say to how to handle unresolved service case.</param>
        /// <returns>The requested service instance.</returns>
        public static TService Resolve<TService>(this IResolver resolver, object serviceKey, IfUnresolved ifUnresolved = IfUnresolved.Throw)
        {
            return (TService)resolver.Resolve(typeof(TService), serviceKey, ifUnresolved);
        }

        /// <summary>
        /// For given instance resolves and sets non-initialized (null) properties from container.
        /// It does not throw if property is not resolved, so you might need to check property value afterwards.
        /// </summary>
        /// <param name="resolver">Any <see cref="IResolver"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="instance">Service instance with properties to resolve and initialize.</param>
        /// <param name="getServiceKey">Optional function to get service key, if not specified service key will be null.</param>
        public static void ResolvePropertiesAndFields(this IResolver resolver, object instance, Func<MemberInfo, object> getServiceKey = null)
        {
            var implType = instance.ThrowIfNull().GetType();
            getServiceKey = getServiceKey ?? (_ => null);

            foreach (var property in implType.GetProperties(ResolutionRules.PropertyOrFieldFlags).Where(p => p.GetSetMethod() != null))
            {
                var value = resolver.Resolve(property.PropertyType, getServiceKey(property), IfUnresolved.ReturnNull);
                if (value != null)
                    property.SetValue(instance, value, null);
            }

            foreach (var field in implType.GetFields(ResolutionRules.PropertyOrFieldFlags).Where(f => !f.IsInitOnly))
            {
                var value = resolver.Resolve(field.FieldType, getServiceKey(field), IfUnresolved.ReturnNull);
                if (value != null)
                    field.SetValue(instance, value);
            }
        }
    }

    public sealed class Request
    {
        public readonly ResolvedExpressions ResolvedExpressions;
        public readonly Request Parent;             // null for resolution root
        public readonly Type ServiceType;
        public readonly object ServiceKey;          // null by default, string for named or integer index for multiple defaults
        public readonly object DependencyInfo;      // either Reflection.ParameterInfo, PropertyInfo or FieldInfo. Used for Print only
        public readonly Factory ResolvedFactory;

        public Type OpenGenericServiceType
        {
            get { return ServiceType.IsGenericType ? ServiceType.GetGenericTypeDefinition() : null; }
        }

        public Type ImplementationType
        {
            get { return ResolvedFactory.ImplementationType; }
        }

        public Request Push(Type serviceType, object serviceKey, object dependencyInfo = null)
        {
            return new Request(ResolvedExpressions, this, serviceType, serviceKey, dependencyInfo);
        }

        public Request ResolveWith(Factory factory)
        {
            for (var p = Parent; p != null; p = p.Parent)
                Throw.If(p.ResolvedFactory != null && p.ResolvedFactory.ID == factory.ID && p.ResolvedFactory.Setup.Type == FactoryType.Service,
                    Error.RECURSIVE_DEPENDENCY_DETECTED, this);
            return new Request(ResolvedExpressions, Parent, ServiceType, ServiceKey, DependencyInfo, factory);
        }

        public Request GetNonWrapperParentOrDefault()
        {
            var p = Parent;
            while (p != null && p.ResolvedFactory.Setup.Type == FactoryType.GenericWrapper)
                p = p.Parent;
            return p;
        }

        public IEnumerable<Request> Enumerate()
        {
            for (var x = this; x != null; x = x.Parent)
                yield return x;
        }

        // Prints something like "DryIoc.UnitTests.IService 'blah' (ctorParam 'blah') of DryIoc.UnitTests.Service"
        public string Print()
        {
            var str = new StringBuilder();

            if (ResolvedFactory != null && ResolvedFactory.Setup.Type != FactoryType.Service)
                str.Append(Enum.GetName(typeof(FactoryType), ResolvedFactory.Setup.Type)).Append(' ');

            str.Append(ServiceType.Print());

            if (ServiceKey != null)
                if (ServiceKey is string)
                    str.Append(" '").Append(ServiceKey).Append("'");
                else
                    str.Append(" #").Append(ServiceKey);

            if (DependencyInfo != null)
            {
                str.Append(" (");
                if (DependencyInfo is ParameterInfo)
                    str.Append("ctorParam '").Append(((ParameterInfo)DependencyInfo).Name);
                else if (DependencyInfo is PropertyInfo)
                    str.Append("property '").Append(((PropertyInfo)DependencyInfo).Name);
                else if (DependencyInfo is FieldInfo)
                    str.Append("field '").Append(((FieldInfo)DependencyInfo).Name);
                str.Append("')");
            }

            if (ResolvedFactory != null && ResolvedFactory.ImplementationType != null)
                str.Append(" of ").Append(ResolvedFactory.ImplementationType.Print());

            return str.ToString();
        }

        public override string ToString()
        {
            var message = new StringBuilder().Append(Print());
            return Parent == null ? message.ToString()
                 : Parent.Enumerate().Aggregate(message,
                    (m, r) => m.AppendLine().Append(" in ").Append(r.Print())).ToString();
        }

        #region Implementation

        internal Request(ResolvedExpressions resolvedExpressions, Request parent, Type serviceType,
            object serviceKey = null, object dependencyInfo = null, Factory factory = null)
        {
            ResolvedExpressions = resolvedExpressions;
            Parent = parent;
            ServiceType = serviceType.ThrowIfNull()
                .ThrowIf(serviceType.IsGenericTypeDefinition, Error.EXPECTED_CLOSED_GENERIC_SERVICE_TYPE, serviceType);
            ServiceKey = serviceKey;
            DependencyInfo = dependencyInfo;
            ResolvedFactory = factory;
        }

        #endregion
    }

    public enum FactoryType { Service, Decorator, GenericWrapper };

    public enum FactoryCachePolicy { ShouldNotCacheExpression, CouldCacheExpression };

    public abstract class FactorySetup
    {
        public abstract FactoryType Type { get; }
        public virtual FactoryCachePolicy CachePolicy { get { return FactoryCachePolicy.ShouldNotCacheExpression; } }
        public virtual object Metadata { get { return null; } }
    }

    public class ServiceSetup : FactorySetup
    {
        public static readonly ServiceSetup Default = new ServiceSetup();

        public static ServiceSetup With(FactoryCachePolicy cachePolicy = FactoryCachePolicy.CouldCacheExpression, object metadata = null)
        {
            return cachePolicy == FactoryCachePolicy.CouldCacheExpression && metadata == null ? Default : new ServiceSetup(cachePolicy, metadata);
        }

        public static ServiceSetup WithMetadata(object metadata = null)
        {
            return metadata == null ? Default : new ServiceSetup(metadata: metadata);
        }

        public override FactoryType Type { get { return FactoryType.Service; } }
        public override FactoryCachePolicy CachePolicy { get { return _cachePolicy; } }
        public override object Metadata { get { return _metadata; } }

        #region Implementation

        private ServiceSetup(FactoryCachePolicy cachePolicy = FactoryCachePolicy.CouldCacheExpression, object metadata = null)
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

        public virtual void ThrowIfCannotBeRegisteredWithServiceType(Type serviceType)
        {
            if (serviceType.IsGenericTypeDefinition && !ProvidesFactoryPerRequest)
                throw Error.UNABLE_TO_REGISTER_NON_FACTORY_PROVIDER_FOR_OPEN_GENERIC_SERVICE.Of(serviceType);
        }

        public virtual bool ProvidesFactoryPerRequest { get { return false; } }

        //ncrunch: no coverage start
        public virtual Factory GetFactoryPerRequestOrDefault(Request request, IRegistry registry) { return null; }
        //ncrunch: no coverage end

        public abstract Expression CreateExpression(Request request, IRegistry registry);

        public virtual LambdaExpression CreateFuncWithArgsOrDefault(Type funcType, Request request, IRegistry registry, out IList<Type> unusedFuncArgs)
        {
            unusedFuncArgs = null;
            return null;
        }

        public Expression GetExpression(Request request, IRegistry registry)
        {
            request = request.ResolveWith(this);
            var decorator = registry.GetDecoratorExpressionOrDefault(request);
            if (decorator != null && !(decorator is LambdaExpression))
                return decorator;

            Expression expression = null;
            if (Setup.CachePolicy == FactoryCachePolicy.CouldCacheExpression)
                expression = request.ResolvedExpressions.GetCachedOrDefault(ID);
            if (expression == null)
            {
                expression = CreateExpression(request, registry);
                if (Reuse != null)
                    expression = Reuse.Of(request, registry, ID, expression);
                if (Setup.CachePolicy == FactoryCachePolicy.CouldCacheExpression)
                    request.ResolvedExpressions.AddToCache(ID, expression);
            }

            if (decorator != null)
                expression = Expression.Invoke(decorator, expression);

            return expression;
        }

        public LambdaExpression GetFuncWithArgsOrDefault(Type funcType, Request request, IRegistry registry, out IList<Type> unusedFuncArgs)
        {
            request = request.ResolveWith(this);
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

        public override string ToString()
        {
            var str = new StringBuilder();
            str.Append("Factory {ID=").Append(ID);
            if (ImplementationType != null)
                str.Append(", ImplType=").Append(ImplementationType.Print());
            return str.ToString();
        }

        #region Implementation

        private static int _idSeedAndCount;
        private FactorySetup _setup;

        #endregion
    }

    public delegate ConstructorInfo GetConstructor(Type implementationType);

    public sealed class ReflectionFactory : Factory
    {
        public override Type ImplementationType { get { return _implementationType; } }

        public override bool ProvidesFactoryPerRequest { get { return _providesFactoryPerRequest; } }

        public ReflectionFactory(Type implementationType, IReuse reuse = null, GetConstructor getConstructor = null, FactorySetup setup = null)
            : base(reuse, setup)
        {
            _implementationType = implementationType.ThrowIfNull()
                .ThrowIf(implementationType.IsAbstract, Error.EXPECTED_NON_ABSTRACT_IMPL_TYPE, implementationType);
            _providesFactoryPerRequest = _implementationType.IsGenericTypeDefinition;
            _getConstructor = getConstructor;
        }

        public override void ThrowIfCannotBeRegisteredWithServiceType(Type serviceType)
        {
            base.ThrowIfCannotBeRegisteredWithServiceType(serviceType);

            var implType = _implementationType;
            if (!implType.IsGenericTypeDefinition)
            {
                if (implType.IsGenericType && implType.ContainsGenericParameters)
                    throw Error.USUPPORTED_REGISTRATION_OF_NON_GENERIC_IMPL_TYPE_DEFINITION_BUT_WITH_GENERIC_ARGS.Of(
                        implType, implType.GetGenericTypeDefinition());

                if (implType != serviceType && serviceType != typeof(object) &&
                    Array.IndexOf(implType.GetImplementedTypes(), serviceType) == -1)
                    throw Error.EXPECTED_IMPL_TYPE_ASSIGNABLE_TO_SERVICE_TYPE.Of(implType, serviceType);
            }
            else if (implType != serviceType)
            {
                if (serviceType.IsGenericTypeDefinition)
                {
                    var implementedTypes = implType.GetImplementedTypes();
                    var implementedOpenGenericTypes = implementedTypes.Where(t =>
                        t.IsGenericType && t.ContainsGenericParameters && t.GetGenericTypeDefinition() == serviceType);

                    var implTypeArgs = implType.GetGenericArguments();
                    Throw.If(!implementedOpenGenericTypes.Any(t => t.ContainsAllGenericParameters(implTypeArgs)),
                        Error.UNABLE_TO_REGISTER_OPEN_GENERIC_IMPL_CAUSE_SERVICE_DOES_NOT_SPECIFY_ALL_TYPE_ARGS,
                        implType, serviceType, implementedOpenGenericTypes);
                }
                else if (implType.IsGenericType && serviceType.ContainsGenericParameters)
                    throw Error.USUPPORTED_REGISTRATION_OF_NON_GENERIC_SERVICE_TYPE_DEFINITION_BUT_WITH_GENERIC_ARGS.Of(
                        serviceType, serviceType.GetGenericTypeDefinition());
                else
                    throw Error.UNABLE_TO_REGISTER_OPEN_GENERIC_IMPL_WITH_NON_GENERIC_SERVICE.Of(implType, serviceType);
            }
        }

        public override Factory GetFactoryPerRequestOrDefault(Request request, IRegistry _)
        {
            var closedTypeArgs = _implementationType == request.OpenGenericServiceType
                ? request.ServiceType.GetGenericArguments()
                : GetClosedTypeArgsForGenericImplementationType(_implementationType, request);

            var closedImplType = _implementationType.MakeGenericType(closedTypeArgs);

            return new ReflectionFactory(closedImplType, Reuse, _getConstructor, Setup);
        }

        public override Expression CreateExpression(Request request, IRegistry registry)
        {
            var ctor = GetConstructor(_implementationType);
            var ctorParams = ctor.GetParameters();
            Expression[] paramExprs = null;
            if (ctorParams.Length != 0)
            {
                paramExprs = new Expression[ctorParams.Length];
                for (var i = 0; i < ctorParams.Length; i++)
                {
                    var ctorParam = ctorParams[i];
                    var paramKey = Setup.Type != FactoryType.Service ? request.ServiceKey // propagate key from wrapper or decorator.
                        : registry.ResolutionRules.Value.ToResolveConstructorParameterServiceKey.GetFirstNonDefault(r => r(ctorParam, request, registry));

                    var paramRequest = request.Push(ctorParam.ParameterType, paramKey, ctorParam);
                    paramExprs[i] = registry.GetOrAddFactory(paramRequest, IfUnresolved.Throw).GetExpression(paramRequest, registry);
                }
            }

            var newExpr = Expression.New(ctor, paramExprs);
            return InitMembersIfRequired(_implementationType, newExpr, request, registry);
        }

        public override LambdaExpression CreateFuncWithArgsOrDefault(Type funcType, Request request, IRegistry registry, out IList<Type> unusedFuncArgs)
        {
            var funcParamTypes = funcType.GetGenericArguments();
            funcParamTypes.ThrowIf(funcParamTypes.Length == 1, Error.EXPECTED_FUNC_WITH_MULTIPLE_ARGS, funcType);

            var ctor = GetConstructor(_implementationType);
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
                    var paramKey = Setup.Type != FactoryType.Service ? request.ServiceKey // propagate key from wrapper or decorator.
                        : registry.ResolutionRules.Value.ToResolveConstructorParameterServiceKey.GetFirstNonDefault(r => r(ctorParam, request, registry));

                    var paramRequest = request.Push(ctorParam.ParameterType, paramKey, ctorParam);
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
            var newExprInitialized = InitMembersIfRequired(_implementationType, newExpr, request, registry);
            return Expression.Lambda(funcType, newExprInitialized, funcInputParamExprs);
        }

        #region Implementation

        private readonly Type _implementationType;
        private readonly bool _providesFactoryPerRequest;
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

        private static Expression InitMembersIfRequired(Type implementationType, NewExpression newService, Request request, IRegistry registry)
        {
            var rules = registry.ResolutionRules.Value;
            if (rules.ToResolvePropertyOrFieldWithServiceKey.IsNullOrEmpty())
                return newService;

            var props = implementationType.GetProperties(ResolutionRules.PropertyOrFieldFlags).Where(p => p.GetSetMethod() != null);
            var fields = implementationType.GetFields(ResolutionRules.PropertyOrFieldFlags).Where(f => !f.IsInitOnly);

            var bindings = new List<MemberBinding>();
            foreach (var member in props.Cast<MemberInfo>().Concat(fields.Cast<MemberInfo>()))
            {
                var m = member;
                object memberKey = null;
                if (rules.ToResolvePropertyOrFieldWithServiceKey.GetFirstNonDefault(r => r(out memberKey, m, request, registry)))
                {
                    var memberType = member is PropertyInfo ? ((PropertyInfo)member).PropertyType : ((FieldInfo)member).FieldType;
                    var memberRequest = request.Push(memberType, memberKey, member);
                    var memberExpr = registry.GetOrAddFactory(memberRequest, IfUnresolved.Throw).GetExpression(memberRequest, registry);
                    bindings.Add(Expression.Bind(member, memberExpr));
                }
            }

            return bindings.Count == 0 ? (Expression)newService : Expression.MemberInit(newService, bindings);
        }

        private static Type[] GetClosedTypeArgsForGenericImplementationType(Type implType, Request request)
        {
            var serviceTypeArgs = request.ServiceType.GetGenericArguments();
            var serviceTypeGenericDefinition = request.OpenGenericServiceType;

            var openImplTypeArgs = implType.GetGenericArguments();
            var implementedTypes = implType.GetImplementedTypes();

            Type[] resultImplTypeArgs = null;
            for (var i = 0; resultImplTypeArgs == null && i < implementedTypes.Length; i++)
            {
                var implementedType = implementedTypes[i];
                if (implementedType.IsGenericType && implementedType.ContainsGenericParameters &&
                    implementedType.GetGenericTypeDefinition() == serviceTypeGenericDefinition)
                {
                    var matchedTypeArgs = new Type[openImplTypeArgs.Length];
                    if (MatchServiceWithImplementedTypeArgs(ref matchedTypeArgs,
                        openImplTypeArgs, implementedType.GetGenericArguments(), serviceTypeArgs))
                        resultImplTypeArgs = matchedTypeArgs;
                }
            }

            resultImplTypeArgs = resultImplTypeArgs.ThrowIfNull(
                Error.UNABLE_TO_MATCH_IMPL_BASE_TYPES_WITH_SERVICE_TYPE, implType, implementedTypes, request);

            var unmatchedArgIndex = Array.IndexOf(resultImplTypeArgs, null);
            if (unmatchedArgIndex != -1)
                throw Error.UNABLE_TO_FIND_OPEN_GENERIC_IMPL_TYPE_ARG_IN_SERVICE.Of(
                    implType, openImplTypeArgs[unmatchedArgIndex], request);

            return resultImplTypeArgs;
        }

        private static bool MatchServiceWithImplementedTypeArgs(ref Type[] matchedImplArgs,
            Type[] openImplementationArgs, Type[] openImplementedArgs, Type[] closedServiceArgs)
        {
            for (var i = 0; i < openImplementedArgs.Length; i++)
            {
                var openImplementedArg = openImplementedArgs[i];
                var closedServiceArg = closedServiceArgs[i];
                if (openImplementedArg.IsGenericParameter)
                {
                    var matchedIndex = openImplementationArgs.IndexOf(t => t.Name == openImplementedArg.Name);
                    if (matchedIndex != -1)
                    {
                        if (matchedImplArgs[matchedIndex] == null)
                            matchedImplArgs[matchedIndex] = closedServiceArg;
                        else if (matchedImplArgs[matchedIndex] != closedServiceArg)
                            return false; // more than one closedServiceArg is matching with single openArg
                    }
                }
                else if (openImplementedArg != closedServiceArg)
                {
                    if (!openImplementedArg.IsGenericType || !openImplementedArg.ContainsGenericParameters ||
                        !closedServiceArg.IsGenericType ||
                        closedServiceArg.GetGenericTypeDefinition() != openImplementedArg.GetGenericTypeDefinition())
                        return false; // openArg and closedArg are different types

                    if (!MatchServiceWithImplementedTypeArgs(ref matchedImplArgs, openImplementationArgs,
                        openImplementedArg.GetGenericArguments(), closedServiceArg.GetGenericArguments()))
                        return false; // nested match failed due either one of above reasons.
                }
            }

            return true;
        }

        #endregion
    }

    public sealed class DelegateFactory : Factory
    {
        public DelegateFactory(Func<Request, IRegistry, Expression> getExpression, IReuse reuse = null, FactorySetup setup = null)
            : base(reuse, setup)
        {
            _getExpression = getExpression.ThrowIfNull();
        }

        public override Expression CreateExpression(Request request, IRegistry registry)
        {
            return _getExpression(request, registry).ThrowIfNull(Error.DELEGATE_FACTORY_EXPRESSION_RETURNED_NULL, request);
        }

        #region Implementation

        private readonly Func<Request, IRegistry, Expression> _getExpression;

        #endregion
    }

    public sealed class DelegateFactory2 : Factory
    {
        public readonly FactoryDelegate FactoryDelegate;

        public DelegateFactory2(FactoryDelegate factoryDelegate, IReuse reuse = null, FactorySetup setup = null)
            : base(reuse, setup)
        {
            FactoryDelegate = factoryDelegate.ThrowIfNull();
        }

        public override Expression CreateExpression(Request request, IRegistry registry)
        {
            var expression = Expression.Invoke(
                request.ResolvedExpressions.ToExpression(FactoryDelegate), 
                ResolvedExpressions.ObjectsParameter,
                ResolvedExpressions.CurrentScopeParameter,
                ResolvedExpressions.ResolutionScopeParameter);
            return expression;
        }
    }

    public sealed class FactoryProvider : Factory
    {
        public override bool ProvidesFactoryPerRequest { get { return true; } }

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
        public override Expression CreateExpression(Request request, IRegistry registry)
        {
            throw new NotSupportedException();
        }
        //ncrunch: no coverage end

        private readonly Func<Request, IRegistry, Factory> _getFactoryOrDefault;
    }

    public enum DependencyKind { CtorParam, Property, Field }

    public sealed class Scope : IDisposable
    {
        public static readonly MethodInfo GetOrAddMethod = typeof(Scope).GetMethod("GetOrAdd");
        public T GetOrAdd<T>(int id, Func<T> factory)
        {
            Throw.If(_disposed == 1, Error.SCOPE_IS_DISPOSED);
            lock (_syncRoot)
            {
                var item = _items.GetFirstValueByHashOrDefault(id);
                if (item == null)
                    _items = _items.AddOrUpdate(id, item = factory());
                return (T)item;
            }
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
                return;

            lock (_syncRoot)
            {
                if (!_items.IsEmpty)
                    foreach (var item in _items.Enumerate().Select(x => x.Value).OfType<IDisposable>())
                        item.Dispose();
                _items = null;
            }
        }

        #region Implementation

        private HashTree<int, object> _items = HashTree<int, object>.Empty;
        private int _disposed;

        // Sync root is required to create single only instance of item. The same as for Lazy<T>
        private readonly object _syncRoot = new object();

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
            InCurrentScope = new ScopedReuse(ResolvedExpressions.CurrentScopeParameter);
            InResolutionScope = new ScopedReuse(Expression.Call(GetScopeMethod, ResolvedExpressions.ResolutionScopeParameter));
        }

        public static Expression GetScopedServiceExpression(Expression scope, int factoryID, Expression factoryExpr)
        {
            return Expression.Call(scope,
                Scope.GetOrAddMethod.MakeGenericMethod(factoryExpr.Type),
                Expression.Constant(factoryID), Expression.Lambda(factoryExpr, null));
        }

        public static readonly MethodInfo GetScopeMethod = typeof(Reuse).GetMethod("GetScope");
        public static Scope GetScope(ref Scope scope)
        {
            return scope = scope ?? new Scope();
        }

        public sealed class ScopedReuse : IReuse
        {
            public ScopedReuse(Expression scopeExpr)
            {
                _scopeExpr = scopeExpr;
            }

            public Expression Of(Request _, IRegistry __, int factoryID, Expression factoryExpr)
            {
                return GetScopedServiceExpression(_scopeExpr, factoryID, factoryExpr);
            }

            private readonly Expression _scopeExpr;
        }

        public sealed class SingletonReuse : IReuse
        {
            public Expression Of(Request request, IRegistry registry, int factoryID, Expression factoryExpr)
            {
                // Create lazy singleton if we have Func somewhere in dependency chain.
                var parent = request.Parent;
                if (parent != null && parent.Enumerate().Any(p =>
                {
                    var openGenericServiceType = p.OpenGenericServiceType;
                    return openGenericServiceType != null && OpenGenericsSupport.FuncTypes.Contains(openGenericServiceType);
                }))
                    return GetScopedServiceExpression(
                        request.ResolvedExpressions.ToExpression(registry.SingletonScope),
                        factoryID, factoryExpr);

                // Create singleton object now and put it into store.
                var currentScope = registry.CurrentScope;
                var singleton = registry.SingletonScope.GetOrAdd(factoryID,
                    () => factoryExpr.CompileToDelegate().Invoke(request.ResolvedExpressions.Objects.Value, currentScope, null));
                return request.ResolvedExpressions.ToExpression(singleton, factoryExpr.Type);
            }
        }
    }

    public enum IfUnresolved { Throw, ReturnNull }

    public interface IResolver
    {
        object ResolveDefault(Type serviceType, IfUnresolved ifUnresolved);

        object ResolveKeyed(Type serviceType, object serviceKey, IfUnresolved ifUnresolved);
    }

    public enum IfAlreadyRegistered { ThrowIfDuplicateKey, KeepAlreadyRegistered }

    public interface IRegistrator
    {
        void Register(Factory factory, Type serviceType, object serviceKey, IfAlreadyRegistered ifAlreadyRegistered);

        bool IsRegistered(Type serviceType, object serviceName, FactoryType factoryType, Func<Factory, bool> condition);
    }

    public interface IRegistry : IResolver, IRegistrator
    {
        RegistryWeakRef SelfWeakRef { get; }
        Ref<ResolutionRules> ResolutionRules { get; }
        Scope CurrentScope { get; }
        Scope SingletonScope { get; }

        Factory GetOrAddFactory(Request request, IfUnresolved ifUnresolved);

        Factory GetFactoryOrDefault(Type serviceType, object serviceKey);
        Factory GetGenericWrapperOrDefault(Type openGenericServiceType);
        Expression GetDecoratorExpressionOrDefault(Request request);

        IEnumerable<KV<object, Factory>> GetAllFactories(Type serviceType);

        Type GetWrappedServiceTypeOrSelf(Type serviceType);
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
        public readonly Expression<FactoryDelegate> Expression;

        public DebugExpression(Expression<FactoryDelegate> expression)
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

        public static Func<object, string> PrintArg = PrintTools.Print;

        public static T ThrowIfNull<T>(this T arg, string message = null, object arg0 = null, object arg1 = null, object arg2 = null) where T : class
        {
            if (arg != null) return arg;
            throw GetException(message == null ? Format(ERROR_ARG_IS_NULL, typeof(T)) : Format(message, arg0, arg1, arg2));
        }

        public static T ThrowIf<T>(this T arg, bool throwCondition, string message = null, object arg0 = null, object arg1 = null, object arg2 = null)
        {
            if (!throwCondition) return arg;
            throw GetException(message == null ? Format(ERROR_ARG_HAS_IMVALID_CONDITION, typeof(T)) : Format(message, arg0, arg1, arg2));
        }

        public static void If(bool throwCondition, string message, object arg0 = null, object arg1 = null, object arg2 = null)
        {
            if (!throwCondition) return;
            throw GetException(Format(message, arg0, arg1, arg2));
        }

        public static Exception Of(this string message, object arg0 = null, object arg1 = null, object arg2 = null)
        {
            return GetException(Format(message, arg0, arg1, arg2));
        }

        private static string Format(this string message, object arg0 = null, object arg1 = null, object arg2 = null)
        {
            return string.Format(message, PrintArg(arg0), PrintArg(arg1), PrintArg(arg2));
        }

        public static readonly string ERROR_ARG_IS_NULL = "Argument of type {0} is null.";
        public static readonly string ERROR_ARG_HAS_IMVALID_CONDITION = "Argument of type {0} has invalid condition.";
    }

    public static class TypeTools
    {
        // ReSharper disable ConstantNullCoalescingCondition
        public static Func<Type, string> PrintDetailsDefault = t => t.FullName ?? t.Name;
        // ReSharper restore ConstantNullCoalescingCondition

        public static string Print(this Type type, Func<Type, string> printDetails = null)
        {
            if (type == null) return null;
            printDetails = printDetails ?? PrintDetailsDefault;
            var name = printDetails(type);
            if (type.IsGenericType) // for generic types
            {
                var genericArgs = type.GetGenericArguments();
                var genericArgsString = type.IsGenericTypeDefinition
                    ? new string(',', genericArgs.Length - 1)
                    : String.Join(", ", genericArgs.Select(x => x.Print(printDetails)).ToArray());
                name = name.Substring(0, name.IndexOf('`')) + "<" + genericArgsString + ">";
            }
            return name.Replace('+', '.'); // for nested classes
        }

        public enum IncludeItself { No, AsFirst }

        /// <summary>
        /// Returns all type interfaces and base types except object.
        /// </summary>
        public static Type[] GetImplementedTypes(this Type type, IncludeItself includeItself = IncludeItself.No)
        {
            Type[] results;

            var interfaces = type.GetInterfaces();
            var interfaceStartIndex = includeItself == IncludeItself.AsFirst ? 1 : 0;
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

            if (includeItself == IncludeItself.AsFirst)
                results[0] = type;

            if (interfaces.Length == 1)
                results[interfaceStartIndex] = interfaces[0];
            else if (interfaces.Length > 1)
                Array.Copy(interfaces, 0, results, interfaceStartIndex, interfaces.Length);

            return results;
        }

        public static bool ContainsAllGenericParameters(this Type similarType, Type[] genericParameters)
        {
            var argNames = new string[genericParameters.Length];
            for (var i = 0; i < genericParameters.Length; i++)
                argNames[i] = genericParameters[i].Name;

            MarkTargetGenericParameters(similarType.GetGenericArguments(), ref argNames);

            for (var i = 0; i < argNames.Length; i++)
                if (argNames[i] != null)
                    return false;

            return true;
        }

        #region Implementation

        private static void MarkTargetGenericParameters(Type[] sourceTypeArgs, ref string[] targetArgNames)
        {
            for (var i = 0; i < sourceTypeArgs.Length; i++)
            {
                var sourceTypeArg = sourceTypeArgs[i];
                if (sourceTypeArg.IsGenericParameter)
                {
                    var matchingTargetArgIndex = Array.IndexOf(targetArgNames, sourceTypeArg.Name);
                    if (matchingTargetArgIndex != -1)
                        targetArgNames[matchingTargetArgIndex] = null;
                }
                else if (sourceTypeArg.IsGenericType && sourceTypeArg.ContainsGenericParameters)
                    MarkTargetGenericParameters(sourceTypeArg.GetGenericArguments(), ref targetArgNames);
            }
        }

        #endregion
    }

    public static class ArrayTools
    {
        public static bool IsNullOrEmpty<T>(this T[] source)
        {
            return source == null || source.Length == 0;
        }

        public static T[] Append<T>(this T[] source, params T[] added)
        {
            if (added == null || added.Length == 0)
                return source;
            if (source == null || source.Length == 0)
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
            if (source == null || source.Length == 0)
                return new[] { value };
            var sourceLength = source.Length;
            index = index < 0 ? sourceLength : index;
            var result = new T[index < sourceLength ? sourceLength : sourceLength + 1];
            Array.Copy(source, result, sourceLength);
            result[index] = value;
            return result;
        }

        public static int IndexOf<T>(this T[] source, Func<T, bool> predicate)
        {
            if (source == null || source.Length == 0)
                return -1;
            for (var i = 0; i < source.Length; ++i)
                if (predicate(source[i]))
                    return i;
            return -1;
        }

        public static T[] Remove<T>(this T[] source, T value)
        {
            if (source == null || source.Length == 0)
                return source;
            var valueIndex = source.IndexOf(x => Equals(x, value));
            if (valueIndex == -1)
                return source;
            if (source.Length == 1)
                return new T[0];
            var result = new T[source.Length - 1];
            if (valueIndex != 0)
                Array.Copy(source, 0, result, 0, valueIndex);
            if (valueIndex != result.Length)
                Array.Copy(source, valueIndex + 1, result, valueIndex, result.Length - valueIndex);
            return result;
        }

        public static R GetFirstNonDefault<T, R>(this T[] source, Func<T, R> selector)
        {
            var result = default(R);
            if (source != null && source.Length != 0)
                for (var i = 0; i < source.Length && Equals(result, default(R)); i++)
                    result = selector(source[i]);
            return result;
        }
    }

    public static class PrintTools
    {
        public static string Print(object x)
        {
            return x is string ? (string)x
                : (x is Type ? ((Type)x).Print()
                : (x is IEnumerable<Type> ? ((IEnumerable)x).Print(";" + Environment.NewLine, ifEmpty: "<empty>")
                : (x is IEnumerable ? ((IEnumerable)x).Print(ifEmpty: "<empty>")
                : (string.Empty + x))));
        }

        public static string Print(this IEnumerable items,
            string separator = ", ", Func<object, string> printItem = null, string ifEmpty = null)
        {
            if (items == null) return null;
            printItem = printItem ?? Print;
            var builder = new StringBuilder();
            foreach (var item in items)
                (builder.Length == 0 ? builder : builder.Append(separator)).Append(printItem(item));
            var result = builder.ToString();
            return result != string.Empty ? result : (ifEmpty ?? string.Empty);
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
    /// Immutable kind of http://en.wikipedia.org/wiki/AVL_tree where actual node key is hash code of <typeparamref name="K"/>.
    /// </summary>
    public sealed class HashTree<K, V>
    {
        public static readonly HashTree<K, V> Empty = new HashTree<K, V>();

        public readonly K Key;
        public readonly V Value;

        public readonly int Hash;
        public readonly KV<K, V>[] Conflicts;
        public readonly HashTree<K, V> Left, Right;
        public readonly int Height;

        public bool IsEmpty { get { return Height == 0; } }

        public delegate V UpdateValue(V oldValue, V value);

        public HashTree<K, V> AddOrUpdate(K key, V value, UpdateValue updateValue = null)
        {
            return AddOrUpdate(key.GetHashCode(), key, value, updateValue);
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

        public V GetFirstValueByHashOrDefault(int hash, V defaultValue = default(V))
        {
            var t = this;
            while (t.Height != 0 && t.Hash != hash)
                t = hash < t.Hash ? t.Left : t.Right;
            return t.Height != 0 ? t.Value : defaultValue;
        }

        /// <summary>
        /// Depth-first in-order traversal as described in http://en.wikipedia.org/wiki/Tree_traversal
        /// The only difference is using fixed size array instead of stack for speed-up (~20% faster than stack).
        /// </summary>
        public IEnumerable<KV<K, V>> Enumerate()
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

        public delegate bool UpdateValueInstead(K key, V oldValue, out V newValue);

        /// <summary>
        /// Based on Eric Lippert's http://blogs.msdn.com/b/ericlippert/archive/2008/01/21/immutability-in-c-part-nine-academic-plus-my-avl-tree-implementation.aspx
        /// </summary>
        public HashTree<K, V> RemoveOrUpdate(K key, UpdateValueInstead updateValue = null)
        {
            return RemoveOrUpdate(key.GetHashCode(), key, updateValue);
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

        private HashTree<K, V> AddOrUpdate(int hash, K key, V value, UpdateValue updateValue)
        {
            return Height == 0 ? new HashTree<K, V>(hash, key, value, null, Empty, Empty)
                : (hash == Hash ? UpdateValueAndResolveConflicts(key, value, updateValue)
                : (hash < Hash
                    ? With(Left.AddOrUpdate(hash, key, value, updateValue), Right)
                    : With(Left, Right.AddOrUpdate(hash, key, value, updateValue)))
                        .KeepBalanced());
        }

        private HashTree<K, V> UpdateValueAndResolveConflicts(K key, V value, UpdateValue updateValue)
        {
            if (ReferenceEquals(Key, key) || Key.Equals(key))
                return new HashTree<K, V>(Hash, key, updateValue == null ? value : updateValue(Value, value), Conflicts, Left, Right);

            if (Conflicts == null)
                return new HashTree<K, V>(Hash, Key, Value, new[] { new KV<K, V>(key, value) }, Left, Right);

            var i = Conflicts.Length - 1;
            while (i >= 0 && !Equals(Conflicts[i].Key, Key)) i--;
            var conflicts = new KV<K, V>[i != -1 ? Conflicts.Length : Conflicts.Length + 1];
            Array.Copy(Conflicts, 0, conflicts, 0, Conflicts.Length);
            conflicts[i != -1 ? i : Conflicts.Length] =
                new KV<K, V>(key, i != -1 && updateValue != null ? updateValue(Conflicts[i].Value, value) : value);
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

        private HashTree<K, V> RemoveOrUpdate(int hash, K key, UpdateValueInstead updateValueInstead = null, bool ignoreKey = false)
        {
            if (Height == 0)
                return this;

            HashTree<K, V> result;
            if (hash == Hash) // found matched Node
            {
                if (ignoreKey || Equals(Key, key))
                {
                    if (!ignoreKey)
                    {
                        V newValue;
                        if (updateValueInstead != null && updateValueInstead(Key, Value, out newValue))
                            return new HashTree<K, V>(Hash, Key, newValue, Conflicts, Left, Right);

                        if (Conflicts != null)
                        {
                            if (Conflicts.Length == 1)
                                return new HashTree<K, V>(Hash, Conflicts[0].Key, Conflicts[0].Value, null, Left, Right);
                            var shrinkedConflicts = new KV<K, V>[Conflicts.Length - 1];
                            Array.Copy(Conflicts, 1, shrinkedConflicts, 0, shrinkedConflicts.Length);
                            return new HashTree<K, V>(Hash, Conflicts[0].Key, Conflicts[0].Value, shrinkedConflicts, Left, Right);
                        }
                    }

                    if (Height == 1) // remove node
                        return Empty;

                    if (Right.IsEmpty)
                        result = Left;
                    else if (Left.IsEmpty)
                        result = Right;
                    else
                    {
                        // we have two children, so remove the next highest node and replace this node with it.
                        var successor = Right;
                        while (!successor.Left.IsEmpty) successor = successor.Left;
                        result = successor.With(Left, Right.RemoveOrUpdate(successor.Hash, default(K), ignoreKey: true));
                    }
                }
                else if (Conflicts != null)
                {
                    var index = Conflicts.Length - 1;
                    while (index >= 0 && !Equals(Conflicts[index].Key, key)) --index;
                    if (index == -1)        // key is not found in conflicts - just return
                        return this;

                    V newValue;
                    var conflict = Conflicts[index];
                    if (updateValueInstead != null && updateValueInstead(conflict.Key, conflict.Value, out newValue))
                    {
                        var updatedConflicts = new KV<K, V>[Conflicts.Length];
                        Array.Copy(Conflicts, 0, updatedConflicts, 0, updatedConflicts.Length);
                        updatedConflicts[index] = new KV<K, V>(conflict.Key, newValue);
                        return new HashTree<K, V>(Hash, Key, Value, updatedConflicts, Left, Right);
                    }

                    if (Conflicts.Length == 1)
                        return new HashTree<K, V>(Hash, Key, Value, null, Left, Right);
                    var shrinkedConflicts = new KV<K, V>[Conflicts.Length - 1];
                    var newIndex = 0;
                    for (var i = 0; i < Conflicts.Length; ++i)
                        if (i != index) shrinkedConflicts[newIndex++] = Conflicts[i];
                    return new HashTree<K, V>(Hash, Key, Value, shrinkedConflicts, Left, Right);
                }
                else return this; // if key is not matching and no conflicts to lookup - just return
            }
            else if (hash < Hash)
                result = With(Left.RemoveOrUpdate(hash, key, updateValueInstead, ignoreKey), Right);
            else
                result = With(Left, Right.RemoveOrUpdate(hash, key, updateValueInstead, ignoreKey));
            return result.KeepBalanced();
        }

        private HashTree<K, V> KeepBalanced()
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

    public static class Ref
    {
        public static Ref<T> Of<T>(T value) where T : class
        {
            return new Ref<T>(value);
        }
    }

    public sealed class Ref<T> where T : class
    {
        public T Value { get { return _value; } }

        public Ref(T initialValue = default(T))
        {
            _value = initialValue;
        }

        public T Swap(Func<T, T> update)
        {
            var retryCount = 0;
            while (true)
            {
                var oldValue = _value;
                var newValue = update(oldValue);
                if (Interlocked.CompareExchange(ref _value, newValue, oldValue) == oldValue)
                    return oldValue;
                if (++retryCount > RETRY_COUNT_UNTIL_THROW)
                    throw new InvalidOperationException(ERROR_EXCEEDED_RETRY_COUNT);
            }
        }

        public T Set(T value)
        {
            return Interlocked.Exchange(ref _value, value);
        }

        private T _value;

        private const int RETRY_COUNT_UNTIL_THROW = 10;
        private static readonly string ERROR_EXCEEDED_RETRY_COUNT =
            "Ref retried to Update for " + RETRY_COUNT_UNTIL_THROW + " times But there is always someone else intervened.";
    }
}