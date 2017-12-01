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

namespace DryIoc
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Diagnostics.CodeAnalysis;  // for SupressMessage
    using System.Diagnostics;               // for StackTrace
    using System.Runtime.CompilerServices;  // for MethodImpl aggressive inlining

    using ImTools;
    using FastExpressionCompiler;

#if FEC_EXPRESSION_INFO

    using static FastExpressionCompiler.ExpressionInfo;
    using Expr = FastExpressionCompiler.ExpressionInfo;
    using ConstExpr = FastExpressionCompiler.ConstantExpressionInfo;
    using ParamExpr = FastExpressionCompiler.ParameterExpressionInfo;
    using NewExpr = FastExpressionCompiler.NewExpressionInfo;
    using UnaryExpr = FastExpressionCompiler.UnaryExpressionInfo;
    using MemberAssignmentExpr = FastExpressionCompiler.MemberAssignmentInfo;
    using FactoryDelegateExpr = FastExpressionCompiler.ExpressionInfo<FactoryDelegate>;

#else

    using static System.Linq.Expressions.Expression;
    using Expr = System.Linq.Expressions.Expression;
    using ConstExpr = System.Linq.Expressions.ConstantExpression;
    using ParamExpr = System.Linq.Expressions.ParameterExpression;
    using NewExpr = System.Linq.Expressions.NewExpression;
    using UnaryExpr = System.Linq.Expressions.UnaryExpression;
    using MemberAssignmentExpr = System.Linq.Expressions.MemberAssignment;
    using FactoryDelegateExpr = System.Linq.Expressions.Expression<FactoryDelegate>;

#endif

    /// <summary>IoC Container. Documentation is available at https://bitbucket.org/dadhi/dryioc. </summary>
    public sealed partial class Container : IContainer
    {
        /// <summary>Creates new container with default rules <see cref="DryIoc.Rules.Default"/>.</summary>
        public Container() : this(Rules.Default, Ref.Of(Registry.Default), NewSingletonScope())
        { }

        /// <summary>Creates new container, optionally providing <see cref="Rules"/> to modify default container behavior.</summary>
        /// <param name="rules">(optional) Rules to modify container default resolution behavior.
        /// If not specified, then <see cref="DryIoc.Rules.Default"/> will be used.</param>
        /// <param name="scopeContext">(optional) Scope context to use for scoped reuse.</param>
        public Container(Rules rules = null, IScopeContext scopeContext = null)
            : this(rules ?? Rules.Default, Ref.Of(Registry.Default), NewSingletonScope(), scopeContext)
        { }

        /// <summary>Creates new container with configured rules.</summary>
        /// <param name="configure">Allows to modify <see cref="DryIoc.Rules.Default"/> rules.</param>
        /// <param name="scopeContext">(optional) Scope context to use for <see cref="Reuse.InCurrentScope"/>.</param>
        public Container(Func<Rules, Rules> configure, IScopeContext scopeContext = null)
            : this(configure.ThrowIfNull()(Rules.Default) ?? Rules.Default, scopeContext)
        { }

        /// <summary>Helper to create singleton scope</summary>
        public static IScope NewSingletonScope() => new Scope(name: "<singletons>");

        /// <summary>Outputs info about container disposal state and current scopes.</summary>
        public override string ToString()
        {
            if (IsDisposed)
            {
                var s = "Container is disposed." + Environment.NewLine;
                if (_disposeStackTrace != null)
                    s += "Dispose stack-trace " + _disposeStackTrace;
                else
                    s += "You may include Dispose stack-trace into the message via:" + Environment.NewLine +
                        "container.With(rules => rules.WithCaptureContainerDisposeStackTrace())";
                return s;
            }

            var scope = CurrentScope;
            var scopeStr = scope == null ? "container"
                : (_scopeContext != null ? "ambiently " : string.Empty) + "scoped container with " + scope;

            return scopeStr;
        }

        /// <summary>Dispose either open scope, or container with singletons, if no scope opened.</summary>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                return;

            // nice to have, but we can leave without it if something goes wrong
            if (Rules.CaptureContainerDisposeStackTrace)
                try { _disposeStackTrace = new StackTrace(); } catch { }

            if (_currentScope != null) // scoped container
            {
                _scopeContext?.SetCurrent(scope => scope == _currentScope ? scope.Parent : scope);
                _currentScope.Dispose();
            }
            else // whole Container with singletons.
            {
                _defaultFactoryDelegateCache = Ref.Of(FactoryDelegateCache.Empty());
                _registry.Swap(Registry.Empty);
                Rules = Rules.Default;

                _singletonScope.Dispose(); // will also dispose any tracked scopes
                _scopeContext?.Dispose();
            }
        }

#region Static state

        /// <summary>Resolver context parameter expression in FactoryDelegate.</summary>
        public static readonly ParamExpr ResolverContextParamExpr =
            Parameter(typeof(IResolverContext), "r");

        private static readonly Type[] _factoryDelegateParamTypes = { typeof(IResolverContext) };
        private static readonly ParamExpr[] _factoryDelegateParamExprs = { ResolverContextParamExpr };

        /// <summary>Wraps service creation expression (body) into <see cref="FactoryDelegate"/> and returns result lambda expression.</summary>
        public static FactoryDelegateExpr WrapInFactoryExpression(Expr expression) =>
            Lambda<FactoryDelegate>(OptimizeExpression(expression), _factoryDelegateParamExprs);

        /// <summary>First wraps the input service expression into lambda expression and
        /// then compiles lambda expression to actual <see cref="FactoryDelegate"/> used for service resolution.</summary>
        /// <param name="expression">Service creation expression.</param>
        /// <returns>Compiled factory delegate to use for service resolution.</returns>
        public static FactoryDelegate CompileToDelegate(Expr expression)
        {
            expression = OptimizeExpression(expression);

            // Optimization: just extract singleton from expression without compiling
            if (expression.NodeType == ExpressionType.Constant)
            {
                var value = ((ConstExpr)expression).Value;
                return _ => value;
            }

            var factoryDelegate = ExpressionCompiler.TryCompile<FactoryDelegate>(
                expression, _factoryDelegateParamExprs, _factoryDelegateParamTypes, typeof(object));
            if (factoryDelegate != null)
                return factoryDelegate;

            // fallback for platforms when FastExpressionCompiler is not supported,
            // or just in case when some expression is not supported (did not found one yet)
            var lambdaExpr = Lambda<FactoryDelegate>(expression, _factoryDelegateParamExprs);

#if FEC_EXPRESSION_INFO
            return lambdaExpr.ToLambdaExpression().Compile();
#else
            return lambdaExpr.Compile();
#endif
        }

        /// <summary>Strips the unnecessary or adds the necessary cast to expression return result.</summary>
        /// <param name="expression">Expression to process.</param> <returns>Processed expression.</returns>
        public static Expr OptimizeExpression(Expr expression)
        {
            if (expression.NodeType == ExpressionType.Convert)
                expression = ((UnaryExpr)expression).Operand;
            else if (expression.Type.IsValueType())
                expression = Convert(expression, typeof(object));
            return expression;
        }

#endregion

#region IRegistrator

        /// <summary>Returns all registered service factories with their Type and optional Key.</summary>
        /// <returns>Existing registrations.</returns>
        /// <remarks>Decorator and Wrapper types are not included.</remarks>
        public IEnumerable<ServiceRegistrationInfo> GetServiceRegistrations() =>
            _registry.Value.GetServiceRegistrations();

        /// <summary>Stores factory into container using <paramref name="serviceType"/> and <paramref name="serviceKey"/> as key
        /// for later lookup.</summary>
        /// <param name="factory">Any subtypes of <see cref="Factory"/>.</param>
        /// <param name="serviceType">Type of service to resolve later.</param>
        /// <param name="serviceKey">(optional) Service key of any type with <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>
        /// implemented.</param>
        /// <param name="ifAlreadyRegistered">(optional) Says how to handle existing registration with the same
        /// <paramref name="serviceType"/> and <paramref name="serviceKey"/>.</param>
        /// <param name="isStaticallyChecked">Confirms that service and implementation types are statically checked by compiler.</param>
        /// <returns>True if factory was added to registry, false otherwise.
        /// False may be in case of <see cref="IfAlreadyRegistered.Keep"/> setting and already existing factory.</returns>
        public void Register(Factory factory, Type serviceType, object serviceKey, IfAlreadyRegistered? ifAlreadyRegistered, bool isStaticallyChecked)
        {
            ThrowIfContainerDisposed();
            factory.ThrowIfNull().ThrowIfInvalidRegistration(serviceType, serviceKey, isStaticallyChecked, Rules);

            if (!ifAlreadyRegistered.HasValue)
                ifAlreadyRegistered = Rules.DefaultIfAlreadyRegistered;

            // Improves performance a bit by attempt to swapping registry while it is still unchanged,
            // if attempt fails then fallback to normal Swap with retry.
            var registry = _registry.Value;
            if (!_registry.TrySwapIfStillCurrent(registry,
                registry.Register(factory, serviceType, ifAlreadyRegistered.Value, serviceKey)))
                _registry.Swap(r => r.Register(factory, serviceType, ifAlreadyRegistered.Value, serviceKey));

            _defaultFactoryDelegateCache = _registry.Value.DefaultFactoryDelegateCache;
        }

        /// <summary>Returns true if there is registered factory with the service type and key.
        /// To check if only default factory is registered specify <see cref="DefaultKey.Value"/> as <paramref name="serviceKey"/>.
        /// Otherwise, if no <paramref name="serviceKey"/> specified then True will returned for any registered factories, even keyed.
        /// Additionally you can specify <paramref name="condition"/> to be applied to registered factories.</summary>
        /// <param name="serviceType">Service type to look for.</param>
        /// <param name="serviceKey">Service key to look for.</param>
        /// <param name="factoryType">Expected registered factory type.</param>
        /// <param name="condition">Expected factory condition.</param>
        /// <returns>True if factory is registered, false if not.</returns>
        public bool IsRegistered(Type serviceType, object serviceKey, FactoryType factoryType, Func<Factory, bool> condition)
        {
            ThrowIfContainerDisposed();
            var factories = _registry.Value.GetRegisteredFactories(serviceType.ThrowIfNull(), serviceKey, factoryType, condition);
            return !factories.IsNullOrEmpty();
        }

        /// <summary>Removes specified factory from registry.
        /// Factory is removed only from registry, if there is relevant cache, it will be kept.
        /// Use <see cref="ContainerTools.WithoutCache"/> to remove all the cache.</summary>
        /// <param name="serviceType">Service type to look for.</param>
        /// <param name="serviceKey">Service key to look for.</param>
        /// <param name="factoryType">Expected factory type.</param>
        /// <param name="condition">Expected factory condition.</param>
        public void Unregister(Type serviceType, object serviceKey, FactoryType factoryType, Func<Factory, bool> condition)
        {
            ThrowIfContainerDisposed();
            _registry.Swap(r => r.Unregister(factoryType, serviceType, serviceKey, condition));
            _defaultFactoryDelegateCache = _registry.Value.DefaultFactoryDelegateCache;
        }

#endregion

#region IResolver

        object IResolver.Resolve(Type serviceType, IfUnresolved ifUnresolved)
        {
            var cachedDelegate = _defaultFactoryDelegateCache.Value.GetValueOrDefault(serviceType);
            if (cachedDelegate != null)
                return cachedDelegate(this);
            return ResolveAndCacheDefaultFactoryDelegate(serviceType, ifUnresolved);
        }

        private object ResolveAndCacheDefaultFactoryDelegate(Type serviceType, IfUnresolved ifUnresolved)
        {
            ThrowIfContainerDisposed();

            var request = Request.Create(this, serviceType, ifUnresolved: ifUnresolved);
            var factory = ((IContainer)this).ResolveFactory(request); // HACK: may mutate request, but it should be safe

            // The situation is possible for multiple default services registered.
            if (request.ServiceKey != null)
                return ((IResolver)this).Resolve(serviceType,
                    request.ServiceKey, ifUnresolved, null, RequestInfo.Empty, null);

            var factoryDelegate = factory?.GetDelegateOrDefault(request);
            if (factoryDelegate == null)
                return null;

            var registryValue = _registry.Value;
            var service = factoryDelegate(this);

            // Additionally disable caching when no services registered, not to cache an empty collection wrapper or alike.
            if (!registryValue.Services.IsEmpty)
            {
                var cacheRef = registryValue.DefaultFactoryDelegateCache;
                var cache = cacheRef.Value;
                if (!cacheRef.TrySwapIfStillCurrent(cache, cache.AddOrUpdate(serviceType, factoryDelegate)))
                    cacheRef.Swap(_ => _.AddOrUpdate(serviceType, factoryDelegate));
            }

            return service;
        }

        object IResolver.Resolve(Type serviceType, object serviceKey,
            IfUnresolved ifUnresolved, Type requiredServiceType, RequestInfo preResolveParent, object[] args)
        {
            preResolveParent = preResolveParent ?? RequestInfo.Empty;

            var cacheEntryKey = serviceKey == null ? (object)serviceType
                : new KV<object, object>(serviceType, serviceKey);

            object cacheContextKey = requiredServiceType;

            if (!preResolveParent.IsEmpty)
                cacheContextKey = cacheContextKey == null ? (object)preResolveParent
                    : new KV<object, RequestInfo>(cacheContextKey, preResolveParent);
            else if (preResolveParent.OpensResolutionScope)
                cacheContextKey = cacheContextKey == null ? (object)true
                    : new KV<object, bool>(cacheContextKey, true);

            var currentScopeName = CurrentScope?.Name;
            if (currentScopeName != null)
                cacheContextKey = cacheContextKey == null ? currentScopeName
                    : new KV<object, object>(cacheContextKey, currentScopeName);

            // Try get from cache first
            var cacheRef = _registry.Value.KeyedFactoryDelegateCache;
            var cacheEntry = cacheRef.Value.GetValueOrDefault(cacheEntryKey);
            if (cacheEntry != null)
            {
                var cachedFactoryDelegate = cacheContextKey == null ? cacheEntry.Key
                    : (cacheEntry.Value ?? ImHashMap<object, FactoryDelegate>.Empty).GetValueOrDefault(cacheContextKey);
                if (cachedFactoryDelegate != null)
                    return cachedFactoryDelegate(this);
            }

            // Cache is missed, so get the factory and put it into cache:
            ThrowIfContainerDisposed();

            var request = Request.Create(this,
                serviceType, serviceKey, ifUnresolved, requiredServiceType, preResolveParent, args);

            var factory = ((IContainer)this).ResolveFactory(request);

            // Request service key may be changed when resolving the factory, so we need to look into cache again for new key
            if (serviceKey == null && request.ServiceKey != null)
            {
                cacheEntryKey = new KV<object, object>(serviceType, request.ServiceKey);
                cacheEntry = cacheRef.Value.GetValueOrDefault(cacheEntryKey);
                if (cacheEntry != null)
                {
                    var cachedDelegate = cacheContextKey == null ? cacheEntry.Key
                        : (cacheEntry.Value ?? ImHashMap<object, FactoryDelegate>.Empty).GetValueOrDefault(cacheContextKey);
                    if (cachedDelegate != null)
                        return cachedDelegate(this);
                }
            }

            var factoryDelegate = factory?.GetDelegateOrDefault(request);
            if (factoryDelegate == null)
                return null;

            var service = factoryDelegate(this);

            // Cache factory only when we successfully called the factory delegate, to prevent failing delegates to be cached.
            // Additionally disable caching when no services registered, not to cache an empty collection wrapper or alike.
            if (!_registry.Value.Services.IsEmpty)
            {
                var cachedFactories = cacheEntry?.Value ?? ImHashMap<object, FactoryDelegate>.Empty;
                cacheEntry = cacheContextKey == null
                    ? KV.Of(factoryDelegate, cachedFactories)
                    : KV.Of(cacheEntry?.Key, cachedFactories.AddOrUpdate(cacheContextKey, factoryDelegate));

                var cache = cacheRef.Value;
                if (!cacheRef.TrySwapIfStillCurrent(cache, cache.AddOrUpdate(cacheEntryKey, cacheEntry)))
                    cacheRef.Swap(it => it.AddOrUpdate(cacheEntryKey, cacheEntry));
            }

            return service;
        }

        IEnumerable<object> IResolver.ResolveMany(Type serviceType, object serviceKey,
            Type requiredServiceType, RequestInfo preResolveParent, object[] args)
        {
            var requiredItemType = requiredServiceType ?? serviceType;

            // Emulating the collection parent so that collection related rules and conditions were applied
            // the same way as if resolving IEnumerable<T>
            if (preResolveParent == null || preResolveParent.IsEmpty)
                preResolveParent = RequestInfo.Empty.Push(
                    typeof(IEnumerable<object>), requiredItemType, serviceKey, IfUnresolved.Throw,
                    0, FactoryType.Wrapper, implementationType: null, reuse: null, flags: RequestFlags.IsServiceCollection);

            var container = (IContainer)this;
            IEnumerable<ServiceRegistrationInfo> items = container.GetAllServiceFactories(requiredItemType)
                .Where(f => f.Value != null)
                .Select(f => new ServiceRegistrationInfo(f.Value, requiredItemType, f.Key));

            IEnumerable<ServiceRegistrationInfo> openGenericItems = null;
            if (requiredItemType.IsClosedGeneric())
            {
                var requiredItemOpenGenericType = requiredItemType.GetGenericDefinitionOrNull();
                openGenericItems = container.GetAllServiceFactories(requiredItemOpenGenericType)
                    .Where(f => f.Value != null)
                    .Select(f => new ServiceRegistrationInfo(f.Value, requiredServiceType,
                        // note: Special service key with info about open-generic service type
                        new[] { requiredItemOpenGenericType, f.Key }));
            }

            // Append registered generic types with compatible variance,
            // e.g. for IHandler<in E> - IHandler<A> is compatible with IHandler<B> if B : A.
            IEnumerable<ServiceRegistrationInfo> variantGenericItems = null;
            if (requiredItemType.IsGeneric() && container.Rules.VariantGenericTypesInResolvedCollection)
            {
                variantGenericItems = container.GetServiceRegistrations()
                    .Where(it => it.ServiceType.IsGeneric()
                        && it.ServiceType.GetGenericTypeDefinition() == requiredItemType.GetGenericTypeDefinition()
                        && it.ServiceType != requiredItemType
                        && it.ServiceType.IsAssignableTo(requiredItemType));
            }

            if (serviceKey != null) // include only single item matching key.
            {
                items = items.Where(it => serviceKey.Equals(it.OptionalServiceKey));
                if (openGenericItems != null)
                    openGenericItems = openGenericItems // extract the actual key from combined type and key
                        .Where(it => serviceKey.Equals(((object[])it.OptionalServiceKey)[1]));
                if (variantGenericItems != null)
                    variantGenericItems = variantGenericItems
                        .Where(it => serviceKey.Equals(it.OptionalServiceKey));
            }

            var metadataKey = preResolveParent.MetadataKey;
            var metadata = preResolveParent.Metadata;
            if (metadataKey != null || metadata != null)
            {
                items = items.Where(it => it.Factory.Setup.MatchesMetadata(metadataKey, metadata));
                if (openGenericItems != null)
                    openGenericItems = openGenericItems
                        .Where(it => it.Factory.Setup.MatchesMetadata(metadataKey, metadata));
                if (variantGenericItems != null)
                    variantGenericItems = variantGenericItems
                        .Where(it => it.Factory.Setup.MatchesMetadata(metadataKey, metadata));
            }

            // Exclude composite parent service from items, skip decorators
            var parent = preResolveParent;
            if (parent.FactoryType != FactoryType.Service)
                parent = parent.FirstOrDefault(p => p.FactoryType == FactoryType.Service) ?? RequestInfo.Empty;

            if (!parent.IsEmpty && parent.GetActualServiceType() == requiredItemType)
            {
                items = items.Where(x => x.Factory.FactoryID != parent.FactoryID);

                if (openGenericItems != null)
                    openGenericItems = openGenericItems
                        .Where(it => !it.Factory.FactoryGenerator.GeneratedFactories.Enumerate()
                            .Any(f => f.Value.FactoryID == parent.FactoryID
                                && f.Key.Key == parent.ServiceType && f.Key.Value == parent.ServiceKey));

                if (variantGenericItems != null)
                    variantGenericItems = variantGenericItems
                        .Where(x => x.Factory.FactoryID != parent.FactoryID);
            }

            var allItems = items;
            if (openGenericItems != null)
                allItems = items.Concat(openGenericItems);
            if (variantGenericItems != null)
                allItems = allItems.Concat(variantGenericItems);

            // Resolve in registration order
            foreach (var item in allItems.OrderBy(it => it.FactoryRegistrationOrder))
            {
                var service = container.Resolve(serviceType, item.OptionalServiceKey,
                    IfUnresolved.ReturnDefaultIfNotRegistered, item.ServiceType, preResolveParent, args);
                if (service != null) // skip unresolved items
                    yield return service;
            }
        }

        private void ThrowIfContainerDisposed()
        {
            if (IsDisposed)
                Throw.It(Error.ContainerIsDisposed, this.ToString());
        }

#endregion

#region IResolverContext

        /// <inheritdoc />
        public IResolverContext Parent => _parent;

        /// <inheritdoc />
        public IResolverContext Root => _root;

        /// <inheritdoc />
        public IScope SingletonScope => _singletonScope;

        /// <inheritdoc />
        public IScope CurrentScope => 
            _scopeContext == null ? _currentScope : _scopeContext.GetCurrentOrDefault();

        /// <inheritdoc />
        public IScopeContext ScopeContext => _scopeContext;

        /// <inheritdoc />
        public IResolverContext OpenScope(object name = null, bool trackInParent = false)
        {
            ThrowIfContainerDisposed();

            var openedScope = new Scope(_currentScope, name);

            // Replacing current context scope with new nested only if current is the same as nested parent, otherwise throw.
            if (_scopeContext != null)
                _scopeContext.SetCurrent(scope =>
                    openedScope.ThrowIf(scope != _currentScope, Error.NotDirectScopeParent, _currentScope, scope));

            if (trackInParent)
                (_currentScope ?? _singletonScope).TrackDisposable(openedScope);

            return new Container(Rules, _registry,
                _singletonScope, _scopeContext, openedScope, _disposed, _disposeStackTrace,
                parent: this, root: _root ?? this);
        }

        void IResolverContext.UseInstance(Type serviceType, object instance, IfAlreadyRegistered ifAlreadyRegistered,
            bool preventDisposal, bool weaklyReferenced, object serviceKey)
        {
            ThrowIfContainerDisposed();

            if (instance != null)
                instance.ThrowIfNotOf(serviceType, Error.RegisteringInstanceNotAssignableToServiceType);

            if (weaklyReferenced)
                instance = new WeakReference(instance);
            else if (preventDisposal)
                instance = new HiddenDisposable(instance);

            var scope = _currentScope ?? _singletonScope;
            var reuse = scope == _singletonScope ? Reuse.Singleton : Reuse.Scoped;
            var instanceType = instance == null ? typeof(object) : instance.GetType();

            _registry.Swap(r =>
            {
                var entry = r.Services.GetValueOrDefault(serviceType);

                // no entries, first registration, usual/hot path
                if (entry == null)
                {
                    // add new entry with instance factory
                    var instanceFactory = InstanceFactory.Of(instance, instanceType, scope, reuse);
                    entry = serviceKey == null
                        ? (object)instanceFactory
                        : FactoriesEntry.Empty.With(instanceFactory, serviceKey);
                }
                else
                {
                    // have some registrations of instance, find if we should replace, add, or throw
                    var singleDefaultFactory = entry as Factory;
                    if (singleDefaultFactory != null)
                    {
                        if (serviceKey != null)
                        {
                            // @ifAlreadyRegistered doe no make sense for keyed, because there are no other keyed
                            entry = FactoriesEntry.Empty.With(singleDefaultFactory)
                                .With(InstanceFactory.Of(instance, instanceType, scope, reuse), serviceKey);
                        }
                        else // for default instance
                        {
                            switch (ifAlreadyRegistered)
                            {
                                case IfAlreadyRegistered.Replace:
                                case IfAlreadyRegistered.AppendNotKeyed:
                                    if (ifAlreadyRegistered == IfAlreadyRegistered.Replace)
                                    {
                                        var reusedFactory = singleDefaultFactory as InstanceFactory;
                                        if (reusedFactory != null)
                                        {
                                            scope.SetOrAdd(reusedFactory.FactoryID, instance);
                                            break;
                                        }

                                        // for non-instance single registration we may replace with non-scoped instance only
                                        if (reuse != Reuse.Scoped)
                                            entry = InstanceFactory.Of(instance, instanceType, scope, reuse);
                                    }
                                    entry = FactoriesEntry.Empty.With(singleDefaultFactory)
                                        .With(InstanceFactory.Of(instance, instanceType, scope, reuse));
                                    break;
                                case IfAlreadyRegistered.Throw:
                                    Throw.It(Error.UnableToRegisterDuplicateDefault, serviceType, singleDefaultFactory);
                                    break;
                                case IfAlreadyRegistered.AppendNewImplementation: // otherwise Keep the old one
                                    if (singleDefaultFactory.CanAccessImplementationType &&
                                        singleDefaultFactory.ImplementationType != instanceType)
                                        entry = FactoriesEntry.Empty.With(singleDefaultFactory)
                                            .With(InstanceFactory.Of(instance, instanceType, scope, reuse));
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    else // for multiple existing or single keyed factory
                    {
                        var singleKeyedOrManyDefaultFactories = (FactoriesEntry)entry;
                        if (serviceKey != null)
                        {
                            var keyedFactory = singleKeyedOrManyDefaultFactories.Factories.GetValueOrDefault(serviceKey);
                            if (keyedFactory == null)
                            {
                                entry = singleKeyedOrManyDefaultFactories
                                    .With(InstanceFactory.Of(instance, instanceType, scope, reuse), serviceKey);
                            }
                            else // when keyed instance is found
                            {
                                switch (ifAlreadyRegistered)
                                {
                                    case IfAlreadyRegistered.Replace:
                                        var reusedFactory = keyedFactory as InstanceFactory;
                                        if (reusedFactory != null)
                                            scope.SetOrAdd(reusedFactory.FactoryID, instance);
                                        else
                                            Throw.It(Error.UnableToUseInstanceForExistingNonInstanceFactory,
                                                KV.Of(serviceKey, instance), keyedFactory);
                                        break;
                                    case IfAlreadyRegistered.Keep:
                                        break;
                                    default:
                                        Throw.It(Error.UnableToRegisterDuplicateKey, serviceType, serviceKey, keyedFactory);
                                        break;
                                }
                            }
                        }
                        else // for default instance
                        {
                            var defaultFactories = singleKeyedOrManyDefaultFactories.LastDefaultKey == null
                                ? ArrayTools.Empty<Factory>()
                                : singleKeyedOrManyDefaultFactories.Factories.Enumerate()
                                    .Match(it => it.Key is DefaultKey, it => it.Value)
                                    .ToArrayOrSelf();

                            if (defaultFactories.Length == 0) // no default factories among the multiple existing keyed factories
                            {
                                entry = singleKeyedOrManyDefaultFactories
                                    .With(InstanceFactory.Of(instance, instanceType, scope, reuse));
                            }
                            else // there are existing default factories
                            {
                                switch (ifAlreadyRegistered)
                                {
                                    case IfAlreadyRegistered.Replace:
                                        // replace single 
                                        if (defaultFactories.Length == 1 && defaultFactories[0] is InstanceFactory)
                                        {
                                            scope.SetOrAdd(defaultFactories[0].FactoryID, instance);
                                        }
                                        else // multiple default or a keyed factory
                                        {
                                            // scoped instance may be appended only, and not replacing anything
                                            if (reuse == Reuse.Scoped)
                                            {
                                                entry = singleKeyedOrManyDefaultFactories.With(
                                                    InstanceFactory.Of(instance, instanceType, scope, reuse));
                                                break;
                                            }

                                            // here is the replacement goes on
                                            var keyedFactories = singleKeyedOrManyDefaultFactories.Factories.Enumerate()
                                                .Match(it => !(it.Key is DefaultKey)).ToArrayOrSelf();

                                            if (keyedFactories.Length == 0) // replaces all default factories?
                                                entry = InstanceFactory.Of(instance, instanceType, scope, reuse);
                                            else
                                            {
                                                var factoriesEntry = FactoriesEntry.Empty;
                                                for (int i = 0; i < keyedFactories.Length; i++)
                                                    factoriesEntry = factoriesEntry
                                                        .With(keyedFactories[i].Value, keyedFactories[i].Key);
                                                entry = factoriesEntry
                                                    .With(InstanceFactory.Of(instance, instanceType, scope, reuse));
                                            }
                                        }

                                        break;
                                    case IfAlreadyRegistered.AppendNotKeyed:
                                        entry = singleKeyedOrManyDefaultFactories
                                            .With(InstanceFactory.Of(instance, instanceType, scope, reuse));
                                        break;
                                    case IfAlreadyRegistered.Throw:
                                        Throw.It(Error.UnableToRegisterDuplicateDefault, serviceType, defaultFactories);
                                        break;
                                    case IfAlreadyRegistered.AppendNewImplementation: // otherwise Keep the old one
                                        var duplicateImplIndex = defaultFactories.IndexOf(
                                            it => it.CanAccessImplementationType &&
                                            it.ImplementationType == instanceType);
                                        if (duplicateImplIndex == -1) // add new implementation
                                            entry = singleKeyedOrManyDefaultFactories
                                                .With(InstanceFactory.Of(instance, instanceType, scope, reuse));
                                        // otherwise do nothing - keep the old entry
                                        break;
                                    default: // IfAlreadyRegistered.Keep
                                        break;
                                }
                            }
                        }
                    }
                }

                // add instance entry to service registrations
                return r.WithServices(r.Services.AddOrUpdate(serviceType, entry));
            });
        }

        void IResolverContext.InjectPropertiesAndFields(object instance, string[] propertyAndFieldNames)
        {
            var instanceType = instance.ThrowIfNull().GetType();

            PropertiesAndFieldsSelector propertiesAndFields = null;
            if (!propertyAndFieldNames.IsNullOrEmpty())
            {
                var matchedMembers = instanceType.GetTypeInfo().DeclaredMembers.Match(
                    m => (m is PropertyInfo || m is FieldInfo) && propertyAndFieldNames.IndexOf(m.Name) != -1,
                    PropertyOrFieldServiceInfo.Of);
                propertiesAndFields = _ => matchedMembers;
            }

            propertiesAndFields =
                propertiesAndFields
                ?? Rules.PropertiesAndFields
                ?? PropertiesAndFields.Auto;

            var request = Request.Create(this, instanceType)
                .WithResolvedFactory(new InstanceFactory(instanceType, Reuse.Transient));

            var requestInfo = request.RequestInfo;
            var resolver = (IResolver)this;

            foreach (var serviceInfo in propertiesAndFields(request))
                if (serviceInfo != null)
                {
                    var details = serviceInfo.Details;
                    var value = resolver.Resolve(serviceInfo.ServiceType, details.ServiceKey,
                        details.IfUnresolved, details.RequiredServiceType, requestInfo, args: null);
                    if (value != null)
                        serviceInfo.SetValue(instance, value);
                }
        }

#endregion

#region IContainer

        /// <summary>The rules object defines policies per container for registration and resolution.</summary>
        public Rules Rules { get; private set; }

        /// <summary>Indicates that container is disposed.</summary>
        public bool IsDisposed => _disposed == 1 || _singletonScope.IsDisposed;

        /// <inheritdoc />
        public IContainer With(Rules rules, IScopeContext scopeContext,
            WithRegistrationsOptions registrations, WithSingletonOptions singleton)
        {
            ThrowIfContainerDisposed();

            rules = rules ?? Rules;
            scopeContext = scopeContext ?? _scopeContext;

            var registry =
                registrations == WithRegistrationsOptions.Share ? _registry :
                registrations == WithRegistrationsOptions.Clone ? Ref.Of(_registry.Value)
                : Ref.Of(_registry.Value.WithoutCache());

            var singletonScope =
                singleton == WithSingletonOptions.Keep ? _singletonScope :
                singleton == WithSingletonOptions.Drop ? NewSingletonScope() :
                _singletonScope.Clone();

            return new Container(rules, registry, singletonScope, scopeContext,
                _currentScope, _disposed, _disposeStackTrace, _parent, _root);
        }

        /// <summary>Produces new container which prevents any further registrations.</summary>
        /// <param name="ignoreInsteadOfThrow">(optional) Controls what to do with the next registration: ignore or throw exception.
        /// Throws exception by default.</param>
        /// <returns>New container preserving all current container state but disallowing registrations.</returns>
        public IContainer WithNoMoreRegistrationAllowed(bool ignoreInsteadOfThrow = false)
        {
            var readonlyRegistry = Ref.Of(_registry.Value.WithNoMoreRegistrationAllowed(ignoreInsteadOfThrow));
            return new Container(Rules, readonlyRegistry,
                _singletonScope, _scopeContext, _currentScope,
                _disposed, _disposeStackTrace, _parent, _root);
        }

        /// <inheritdoc />
        public bool ClearCache(Type serviceType, FactoryType? factoryType, object serviceKey)
        {
            if (factoryType != null)
                return _registry.Value.ClearCache(serviceType, serviceKey, factoryType.Value);

            var registry = _registry.Value;

            var clearedServices = registry.ClearCache(serviceType, serviceKey, FactoryType.Service);
            var clearedWrapper = registry.ClearCache(serviceType, serviceKey, FactoryType.Wrapper);
            var clearedDecorator = registry.ClearCache(serviceType, serviceKey, FactoryType.Decorator);

            return clearedServices || clearedWrapper || clearedDecorator;
        }

        Factory IContainer.ResolveFactory(Request request)
        {
            var factory = ((IContainer)this).GetServiceFactoryOrDefault(request);
            if (factory == null)
            {
                factory = GetWrapperFactoryOrDefault(request);
                if (factory != null)
                    return factory;

                if (factory == null && !Rules.UnknownServiceResolvers.IsNullOrEmpty())
                    for (var i = 0; factory == null && i < Rules.UnknownServiceResolvers.Length; i++)
                        factory = Rules.UnknownServiceResolvers[i](request);
            }

            if (factory != null && factory.FactoryGenerator != null)
                factory = factory.FactoryGenerator.GetGeneratedFactory(request);

            if (factory == null && request.IfUnresolved == IfUnresolved.Throw)
                ThrowUnableToResolve(request);

            return factory;
        }

        internal static void ThrowUnableToResolve(Request request)
        {
            var container = request.Container;

            var registrations = container
                .GetAllServiceFactories(request.ServiceType, bothClosedAndOpenGenerics: true)
                .Aggregate(new StringBuilder(), (s, f) =>
                    (f.Value.Reuse?.CanApply(request) ?? true
                        ? s.Append("  ")
                        : s.Append("  without matching scope "))
                        .AppendLine(f.ToString()));

            if (registrations.Length != 0)
                Throw.It(Error.UnableToResolveFromRegisteredServices,
                    request, registrations);
            else
                Throw.It(Error.UnableToResolveUnknownService, request,
                    container.Rules.DynamicRegistrationProviders.EmptyIfNull().Length,
                    container.Rules.UnknownServiceResolvers.EmptyIfNull().Length);
        }

        Factory IContainer.GetServiceFactoryOrDefault(Request request)
        {
            var serviceType = GetRegisteredServiceType(request);
            var serviceKey = request.ServiceKey;

            // For requested keyed service just lookup for key and return whatever the result
            if (serviceKey != null)
                return GetKeyedServiceFactoryOrDefault(request, serviceType, serviceKey);

            if (Rules.FactorySelector != null)
                return GetRuleSelectedServiceFactoryOrDefault(Rules.FactorySelector, request, serviceType);

            var registeredFactories = GetRegisteredServiceFactoriesOrNull(serviceType);
            var factories = GetCombinedRegisteredAndDynamicFactories(
                registeredFactories, true, FactoryType.Service, serviceType);
            if (factories.IsNullOrEmpty())
                return null;

            // First, filter out non default normal and dynamic factories
            var defaultFactories = factories.Match(f => f.Key is DefaultKey || f.Key is DefaultDynamicKey);
            if (defaultFactories.Length == 0)
                return null;

            var matchedFactories = MatchFactories(defaultFactories, request);

            // For multiple matched factories, if the single one has a condition, then use it
            if (matchedFactories.Length > 1)
            {
                var conditionedFactories = matchedFactories.Match(f => f.Value.Setup.Condition != null);
                if (conditionedFactories.Length == 1)
                    matchedFactories = conditionedFactories;
            }

            // Hurrah! The result is a single matched factory
            if (matchedFactories.Length == 1)
            {
                var factory = matchedFactories[0];
                if (defaultFactories.Length > 1)
                {
                    if (request.IsResolutionCall)
                        request.ChangeServiceKey(factory.Key);
                    else
                    {
                        var setup = factory.Value.Setup;
                        if (!setup.AsResolutionCall && setup.Condition != null)
                            factory.Value.Setup = setup.WithAsResolutionCall();
                    }
                }

                return factory.Value;
            }

            if (matchedFactories.Length > 1 && request.IfUnresolved == IfUnresolved.Throw)
                Throw.It(Error.ExpectedSingleDefaultFactory, matchedFactories, request);

            // Return null to allow fallback strategies
            return null;
        }

        private static KV<object, Factory>[] MatchFactories(KV<object, Factory>[] matchedFactories, Request request)
        {
            // Check factories condition, even for the single factory
            matchedFactories = matchedFactories.Match(f => f.Value.CheckCondition(request));
            if (matchedFactories.Length == 0)
                return matchedFactories;

            // Check metadata, even for the single factory
            var metadataKey = request.MetadataKey;
            var metadata = request.Metadata;
            if (metadataKey != null || metadata != null)
            {
                matchedFactories = matchedFactories
                    .Match(f => f.Value.Setup.MatchesMetadata(metadataKey, metadata));
                if (matchedFactories.Length == 0)
                    return matchedFactories;
            }

            // Check the for matching scopes. Only for more than 1 factory, 
            // for the single factory the check will be down the road
            // Issue: #175
            if (matchedFactories.Length > 1 &&
                request.Rules.ImplicitCheckForReuseMatchingScope)
            {
                matchedFactories = MatchFactoriesByReuse(matchedFactories, request);
                if (matchedFactories.Length == 0)
                    return matchedFactories;
            }

            // Match open-generic implementation with closed service type. Performance is OK because the generated factories are cached -
            // so there should not be repeating of the check, and not match of Performance decrease.
            if (matchedFactories.Length > 1)
            {
                matchedFactories = matchedFactories.Match(f =>
                    f.Value.FactoryGenerator == null ||
                    f.Value.FactoryGenerator.GetGeneratedFactory(request, ifErrorReturnDefault: true) != null);
                if (matchedFactories.Length == 0)
                    return matchedFactories;
            }

            return matchedFactories;
        }

        IEnumerable<KV<object, Factory>> IContainer.GetAllServiceFactories(Type serviceType, bool bothClosedAndOpenGenerics)
        {
            var serviceFactories = _registry.Value.Services;

            var entry = serviceFactories.GetValueOrDefault(serviceType);

            var factories = GetRegistryEntryKeyFactoryPairs(entry).ToArrayOrSelf();

            if (bothClosedAndOpenGenerics && serviceType.IsClosedGeneric())
            {
                var openGenericEntry = serviceFactories.GetValueOrDefault(serviceType.GetGenericTypeDefinition());
                if (openGenericEntry != null)
                {
                    var openGenericFactories = GetRegistryEntryKeyFactoryPairs(openGenericEntry).ToArrayOrSelf();
                    factories = factories.Append(openGenericFactories);
                }
            }

            return GetCombinedRegisteredAndDynamicFactories(factories,
                bothClosedAndOpenGenerics, FactoryType.Service, serviceType);
        }

        private static IEnumerable<KV<object, Factory>> GetRegistryEntryKeyFactoryPairs(object entry) =>
            entry == null
                ? ArrayTools.Empty<KV<object, Factory>>()
                : entry is Factory ? new[] { new KV<object, Factory>(DefaultKey.Value, (Factory)entry) }
                : ((FactoriesEntry)entry).Factories.Enumerate();

        Expr IContainer.GetDecoratorExpressionOrDefault(Request request)
        {
            // return early if no decorators registered
            if (_registry.Value.Decorators.IsEmpty &&
                request.Rules.DynamicRegistrationProviders.IsNullOrEmpty())
                return null;

            var arrayElementType = request.ServiceType.GetArrayElementTypeOrNull();
            if (arrayElementType != null)
                request = request.WithChangedServiceInfo(info => info
                    .With(typeof(IEnumerable<>).MakeGenericType(arrayElementType)));

            // Define the list of ids for the already applied decorators
            int[] appliedDecoratorIDs = null;

            var container = request.Container;

            var serviceType = request.ServiceType;
            var decorators = container.GetDecoratorFactoriesOrDefault(serviceType);

            // Combine with required service type if different from service type
            var requiredServiceType = request.GetActualServiceType();
            if (requiredServiceType != serviceType)
                decorators = decorators.Append(container.GetDecoratorFactoriesOrDefault(requiredServiceType));

            if (!decorators.IsNullOrEmpty())
            {
                appliedDecoratorIDs = GetAppliedDecoratorIDs(request);
                if (!appliedDecoratorIDs.IsNullOrEmpty())
                    decorators = decorators.Match(d => appliedDecoratorIDs.IndexOf(d.FactoryID) == -1);
            }

            // Append open-generic decorators
            var genericDecorators = ArrayTools.Empty<Factory>();
            var openGenServiceType = serviceType.GetGenericDefinitionOrNull();
            if (openGenServiceType != null)
                genericDecorators = container.GetDecoratorFactoriesOrDefault(openGenServiceType);

            // Combine with open-generic required type if they are different from service type
            if (requiredServiceType != serviceType)
            {
                var openGenRequiredType = requiredServiceType.GetGenericDefinitionOrNull();
                if (openGenRequiredType != null && openGenRequiredType != openGenServiceType)
                    genericDecorators = genericDecorators.Append(
                        container.GetDecoratorFactoriesOrDefault(openGenRequiredType));
            }

            // Append generic type argument decorators, registered as Object
            // Note: the condition for type arguments should be checked before generating the closed generic version
            var typeArgDecorators = container.GetDecoratorFactoriesOrDefault(typeof(object));
            if (!typeArgDecorators.IsNullOrEmpty())
                genericDecorators = genericDecorators.Append(
                    typeArgDecorators.Match(d => d.CheckCondition(request)));

            // Filter out already applied generic decorators
            // And combine with rest of decorators
            if (!genericDecorators.IsNullOrEmpty())
            {
                appliedDecoratorIDs = appliedDecoratorIDs ?? GetAppliedDecoratorIDs(request);
                if (!appliedDecoratorIDs.IsNullOrEmpty())
                    genericDecorators = genericDecorators
                        .Match(d => d.FactoryGenerator == null
                            ? appliedDecoratorIDs.IndexOf(d.FactoryID) == -1
                            : d.FactoryGenerator.GeneratedFactories.Enumerate()
                                .All(f => appliedDecoratorIDs.IndexOf(f.Value.FactoryID) == -1));

                // Generate closed-generic versions
                if (!genericDecorators.IsNullOrEmpty())
                {
                    genericDecorators = genericDecorators
                        .Map(d => d.FactoryGenerator == null ? d
                            : d.FactoryGenerator.GetGeneratedFactory(request, ifErrorReturnDefault: true))
                        .Match(d => d != null);
                    decorators = decorators.Append(genericDecorators);
                }
            }

            // Filter out the recursive decorators by doing the same recursive check
            // that Request.WithResolvedFactory does. Fixes: #267
            if (!decorators.IsNullOrEmpty())
                decorators = decorators.Match(d =>
                {
                    for (var p = request.DirectRuntimeParent; !p.IsEmpty; p = p.DirectRuntimeParent)
                        if (p.FactoryID == d.FactoryID)
                            return false;
                    return true;
                });

            // Return earlier if no decorators found, or we have filtered out everything
            if (decorators.IsNullOrEmpty())
                return null;

            Factory decorator;
            if (decorators.Length == 1)
            {
                decorator = decorators[0];
                if (!decorator.CheckCondition(request))
                    return null;
            }
            else
            {
                // Within remaining decorators find one with maximum Order
                // or if no Order for all decorators, then the last registered - with maximum FactoryID
                decorator = decorators
                    .OrderByDescending(d => ((Setup.DecoratorSetup)d.Setup).Order)
                    .ThenByDescending(d => d.FactoryID)
                    .FirstOrDefault(d => d.CheckCondition(request));
            }

            var decoratorExpr = decorator?.GetExpressionOrDefault(request);
            if (decoratorExpr == null)
                return null;

            // decorator of arrays should be converted back from IEnumerable to array.
            if (arrayElementType != null)
            {
                var toArrayMethod = typeof(Enumerable).Method(nameof(Enumerable.ToArray)); // todo: move to lazy field
                decoratorExpr = Call(toArrayMethod.MakeGenericMethod(arrayElementType), decoratorExpr);
            }

            return decoratorExpr;
        }

        private static int[] GetAppliedDecoratorIDs(Request request)
        {
            var parent = request.DirectParent;
            if (parent.IsEmpty)
                return ArrayTools.Empty<int>();
            return parent.TakeWhile(p =>
                p.FactoryType == FactoryType.Wrapper ||
                p.FactoryType == FactoryType.Decorator &&
                (p.DecoratedFactoryID == 0 || p.DecoratedFactoryID == request.FactoryID))
                .Where(p => p.FactoryType == FactoryType.Decorator)
                .Select(d => d.FactoryID)
                .ToArray();
        }

        Factory IContainer.GetWrapperFactoryOrDefault(Type serviceType)
        {
            // searches for open-generic wrapper, otherwise for concrete one
            // note: currently impossible to have both open and closed generic wrapper of the same generic type
            serviceType = serviceType.GetGenericDefinitionOrNull() ?? serviceType;
            return _registry.Value.Wrappers.GetValueOrDefault(serviceType);
        }

        Factory[] IContainer.GetDecoratorFactoriesOrDefault(Type serviceType)
        {
            var decorators = ArrayTools.Empty<Factory>();

            var allDecorators = _registry.Value.Decorators;
            if (!allDecorators.IsEmpty)
                decorators = allDecorators.GetValueOrDefault(serviceType) ?? ArrayTools.Empty<Factory>();

            decorators = GetCombinedRegisteredAndDynamicFactories(
                    decorators.Map(d => new KV<object, Factory>(DefaultKey.Value, d)),
                    true, FactoryType.Decorator, serviceType)
                .Map(it => it.Value);

            return decorators;
        }

        Type IContainer.GetWrappedType(Type serviceType, Type requiredServiceType)
        {
            if (requiredServiceType != null && requiredServiceType.IsOpenGeneric())
                return ((IContainer)this).GetWrappedType(serviceType, null);

            serviceType = requiredServiceType ?? serviceType;

            var wrappedType = serviceType.GetArrayElementTypeOrNull();
            if (wrappedType == null)
            {
                var factory = ((IContainer)this).GetWrapperFactoryOrDefault(serviceType);
                if (factory != null)
                {
                    wrappedType = ((Setup.WrapperSetup)factory.Setup)
                        .GetWrappedTypeOrNullIfWrapsRequired(serviceType);
                    if (wrappedType == null)
                        return null;
                }
            }

            return wrappedType == null ? serviceType
                : ((IContainer)this).GetWrappedType(wrappedType, null);
        }

        /// <summary>Adds factory expression to cache identified by factory ID (<see cref="Factory.FactoryID"/>).</summary>
        /// <param name="factoryID">Key in cache.</param>
        /// <param name="factoryExpression">Value to cache.</param>
        public void CacheFactoryExpression(int factoryID, Expr factoryExpression)
        {
            var registry = _registry.Value;
            if (!registry.Services.IsEmpty)
            {
                var cacheRef = registry.FactoryExpressionCache;
                var cacheVal = cacheRef.Value;
                if (!cacheRef.TrySwapIfStillCurrent(cacheVal, cacheVal.AddOrUpdate(factoryID, factoryExpression)))
                    cacheRef.Swap(val => val.AddOrUpdate(factoryID, factoryExpression));
            }
        }

        /// <summary>Searches and returns cached factory expression, or null if not found.</summary>
        /// <param name="factoryID">Factory ID to lookup by.</param> <returns>Found expression or null.</returns>
        public Expr GetCachedFactoryExpressionOrDefault(int factoryID) =>
            _registry.Value.FactoryExpressionCache.Value.GetValueOrDefault(factoryID);

        /// <summary>Converts known items into custom expression or wraps it in a constant expression.</summary>
        public Expr GetItemExpression(object item, Type itemType = null, 
            bool throwIfStateRequired = false)
        {
            if (item == null)
                return itemType == null ? Constant(null) : Constant(null, itemType);

            var convertible = item as IConvertibleToExpression;
            if (convertible != null)
                return convertible.ToExpression(it => GetItemExpression(it));

            var actualItemType = item.GetType();
            if (actualItemType.GetGenericDefinitionOrNull() == typeof(KV<,>))
            {
                var kvArgTypes = actualItemType.GetGenericParamsAndArgs();
                var kvOfMethod = _kvOfMethod.MakeGenericMethod(kvArgTypes);
                return Call(kvOfMethod,
                    GetItemExpression(actualItemType.Field("Key").GetValue(item),
                        kvArgTypes[0], throwIfStateRequired),
                    GetItemExpression(actualItemType.Field("Value").GetValue(item),
                        kvArgTypes[1], throwIfStateRequired));
            }

            if (actualItemType.IsPrimitive() || 
                actualItemType.IsAssignableTo(typeof(Type)))
                return itemType == null ? Constant(item) : Constant(item, itemType);

            if (actualItemType.IsArray)
            {
                var elems = ((object[])item)
                    .Map(it => GetItemExpression(it, null, throwIfStateRequired));
                return NewArrayInit(actualItemType.GetElementType().ThrowIfNull(), elems);
            }

            var itemExpr = Rules.ItemToExpressionConverter?.Invoke(item, itemType);
            if (itemExpr != null)
                return itemExpr;

            Throw.If(throwIfStateRequired || Rules.ThrowIfRuntimeStateRequired,
                Error.StateIsRequiredToUseItem, item);

            return itemType == null ? Constant(item) : Constant(item, itemType);
        }

        private static readonly MethodInfo _kvOfMethod = typeof(KV).Method(nameof(KV.Of));

#endregion

        #region Factories Add/Get

        internal sealed class FactoriesEntry
        {
            public readonly DefaultKey LastDefaultKey;
            public readonly ImHashMap<object, Factory> Factories;

            // lastDefaultKey may be null
            public FactoriesEntry(DefaultKey lastDefaultKey, ImHashMap<object, Factory> factories)
            {
                LastDefaultKey = lastDefaultKey;
                Factories = factories;
            }

            public static readonly FactoriesEntry Empty =
                new FactoriesEntry(null, ImHashMap<object, Factory>.Empty);

            public FactoriesEntry With(Factory factory, object serviceKey = null)
            {
                var lastDefaultKey = serviceKey != null
                    ? LastDefaultKey // if service key is specified, the default one remains the same
                    : LastDefaultKey == null
                        ? DefaultKey.Value
                        : LastDefaultKey.Next();

                var factories = Factories.AddOrUpdate(serviceKey ?? lastDefaultKey, factory);

                return new FactoriesEntry(lastDefaultKey, factories);
            }
        }

        private static Type GetRegisteredServiceType(Request request)
        {
            var requiredServiceType = request.RequiredServiceType;
            if (requiredServiceType != null && requiredServiceType.IsOpenGeneric())
                return requiredServiceType;

            // Special case when open-generic required service type is encoded in ServiceKey as array of { ReqOpenGenServType, ServKey }
            // presumes that required service type is closed generic
            var actualServiceType = request.GetActualServiceType();
            if (actualServiceType.IsClosedGeneric())
            {
                var serviceKey = request.ServiceKey;
                var serviceKeyWithOpenGenericRequiredType = serviceKey as object[];
                if (serviceKeyWithOpenGenericRequiredType != null &&
                    serviceKeyWithOpenGenericRequiredType.Length == 2)
                {
                    var openGenericType = serviceKeyWithOpenGenericRequiredType[0] as Type;
                    if (openGenericType != null &&
                        openGenericType == actualServiceType.GetGenericDefinitionOrNull())
                    {
                        actualServiceType = openGenericType;
                        serviceKey = serviceKeyWithOpenGenericRequiredType[1];

                        // note: Mutates the request
                        request.ChangeServiceKey(serviceKey);
                    }
                }
            }

            return actualServiceType;
        }

        private KV<object, Factory>[] GetRegisteredServiceFactoriesOrNull(Type serviceType, object serviceKey = null)
        {
            var serviceFactories = _registry.Value.Services;
            var entry = serviceFactories.GetValueOrDefault(serviceType);

            // For closed-generic lookup type, when entry is not found or the key in entry is not found,
            // go to the open-generic services
            if (serviceType.IsClosedGeneric())
            {
                if (entry == null ||
                    serviceKey != null && (
                        entry is Factory && !serviceKey.Equals(DefaultKey.Value) ||
                        entry is FactoriesEntry && ((FactoriesEntry)entry).Factories.GetValueOrDefault(serviceKey) == null))
                {
                    var lookupOpenGenericType = serviceType.GetGenericTypeDefinition();
                    var openGenericEntry = serviceFactories.GetValueOrDefault(lookupOpenGenericType);
                    if (openGenericEntry != null)
                        entry = openGenericEntry;
                }
            }

            if (entry == null)
                return null;

            var factory = entry as Factory;
            if (factory != null)
                return new[] { new KV<object, Factory>(DefaultKey.Value, factory) };

            return ((FactoriesEntry)entry).Factories.Enumerate().ToArray();
        }

        private KV<object, Factory>[] GetCombinedRegisteredAndDynamicFactories(
            KV<object, Factory>[] registeredFactories, bool bothClosedAndOpenGenerics,
            FactoryType factoryType, Type serviceType, object serviceKey = null)
        {
            if (!registeredFactories.IsNullOrEmpty() &&
                Rules.UseDynamicRegistrationsAsFallback)
                return registeredFactories;

            var dynamicRegistrationProviders = Rules.DynamicRegistrationProviders;
            if (dynamicRegistrationProviders.IsNullOrEmpty())
                return registeredFactories;

            var resultFactories = registeredFactories;

            // assign unique continuous keys across all of dynamic providers,
            // to prevent duplicate keys and peeking the wrong factory by collection wrappers
            var dynamicKey = DefaultDynamicKey.Value;

            for (var i = 0; i < dynamicRegistrationProviders.Length; i++)
            {
                var dynamicRegistrationProvider = dynamicRegistrationProviders[i];
                var dynamicRegistrations = dynamicRegistrationProvider(serviceType, serviceKey).ToArrayOrSelf();
                if (bothClosedAndOpenGenerics && serviceType.IsClosedGeneric())
                {
                    var openGenServiceType = serviceType.GetGenericTypeDefinition();
                    var openGenDynamicRegistrations = dynamicRegistrationProvider(openGenServiceType, serviceKey);
                    if (openGenDynamicRegistrations != null)
                        dynamicRegistrations = dynamicRegistrations.Append(openGenDynamicRegistrations);
                }

                if (dynamicRegistrations.IsNullOrEmpty())
                    continue;

                if (resultFactories.IsNullOrEmpty())
                {
                    resultFactories = dynamicRegistrations.Match(it =>
                        it.Factory.FactoryType == factoryType &&
                        it.Factory.ThrowIfInvalidRegistration(serviceType, serviceKey, false, Rules),
                        it => KV.Of(it.ServiceKey ?? (dynamicKey = dynamicKey.Next()), it.Factory))
                        .ToArrayOrSelf();
                    continue;
                }

                var remainingDynamicFactories = dynamicRegistrations
                    .Match(it =>
                    {
                        if (it.Factory.FactoryType != factoryType ||
                            !it.Factory.ThrowIfInvalidRegistration(serviceType, serviceKey, false, Rules))
                            return false;

                        if (it.ServiceKey == null) // for the default dynamic factory
                        {
                            switch (it.IfAlreadyRegistered)
                            {
                                // accept the default if result factories don't contain it already
                                case IfAlreadyRegistered.Keep:
                                case IfAlreadyRegistered.Throw:
                                    return resultFactories.IndexOf(f => f.Key is DefaultKey || f.Key is DefaultDynamicKey) == -1;

                                // remove the default from the result factories
                                case IfAlreadyRegistered.Replace:
                                    resultFactories = resultFactories.Match(f => !(f.Key is DefaultKey || f.Key is DefaultDynamicKey));
                                    return true;

                                case IfAlreadyRegistered.AppendNotKeyed:
                                    return true;

                                case IfAlreadyRegistered.AppendNewImplementation:
                                    // if we cannot access to dynamic implementation type, assume that the type is new implementation
                                    if (!it.Factory.CanAccessImplementationType)
                                        return true;

                                    // keep dynamic factory if there is no result factory with the same implementation type
                                    return resultFactories.IndexOf(f =>
                                        f.Value.CanAccessImplementationType &&
                                        f.Value.ImplementationType == it.Factory.ImplementationType) == -1;
                            }
                        }
                        else // for the keyed dynamic factory
                        {
                            switch (it.IfAlreadyRegistered)
                            {
                                // remove the result factory with the same key
                                case IfAlreadyRegistered.Replace:
                                    resultFactories = resultFactories.Match(f => !f.Key.Equals(it.ServiceKey));
                                    return true;

                                // keep the dynamic factory with the new service key, otherwise skip it
                                default:
                                    return resultFactories.IndexOf(f => f.Key.Equals(it.ServiceKey)) == -1;
                            }
                        }

                        return true;
                    },
                        it => KV.Of(it.ServiceKey ?? (dynamicKey = dynamicKey.Next()), it.Factory));

                resultFactories = resultFactories.Append(remainingDynamicFactories);
            }

            return resultFactories;
        }

        private Factory GetKeyedServiceFactoryOrDefault(Request request, Type serviceType, object serviceKey)
        {
            var registeredFactories = GetRegisteredServiceFactoriesOrNull(serviceType, serviceKey);
            var registeredAndDynamicFactories = GetCombinedRegisteredAndDynamicFactories(
                registeredFactories, true,
                FactoryType.Service, serviceType, serviceKey);

            var factory = registeredAndDynamicFactories.FindFirst(
                f => serviceKey.Equals(f.Key) && f.Value.CheckCondition(request));

            return factory?.Value;
        }

        private Factory GetRuleSelectedServiceFactoryOrDefault(
            Rules.FactorySelectorRule factorySelector, Request request, Type serviceType)
        {
            var allFactories = ((IContainer)this)
                .GetAllServiceFactories(serviceType, bothClosedAndOpenGenerics: true)
                .ToArrayOrSelf();
            if (allFactories.Length == 0)
                return null;

            // Sort in registration order
            if (allFactories.Length > 1)
                Array.Sort(allFactories, _keyFactoryComparer);

            var matchedFactories = MatchFactories(allFactories, request);
            if (matchedFactories.Length == 0)
                return null;

            var factory = factorySelector(request,
                matchedFactories.Map(f => new KeyValuePair<object, Factory>(f.Key, f.Value)));
            if (factory == null)
                return null;

            // Issue: #508
            if (allFactories.Length > 1)
            {
                var selectedFactoryKey = matchedFactories.FindFirst(f => f.Value.FactoryID == factory.FactoryID).Key;
                request.ChangeServiceKey(selectedFactoryKey);
            }

            return factory;
        }

        private static readonly KeyFactoryComparer _keyFactoryComparer = new KeyFactoryComparer();
        private struct KeyFactoryComparer : IComparer<KV<object, Factory>>
        {
            public int Compare(KV<object, Factory> first, KV<object, Factory> next) =>
                first.Value.FactoryID - next.Value.FactoryID;
        }

        private static KV<object, Factory>[] MatchFactoriesByReuse(
            KV<object, Factory>[] matchedFactories, Request request)
        {
            var reuseMatchedFactories = matchedFactories.Match(it => it.Value.Reuse?.CanApply(request) ?? true);
            if (reuseMatchedFactories.Length == 1)
            {
                matchedFactories = reuseMatchedFactories;
            }
            else if (reuseMatchedFactories.Length > 1)
            {
                var minLifespan = int.MaxValue;
                bool multipleFactories = false;
                KV<object, Factory> minLifespanFactory = null;
                for (int i = 0; i < reuseMatchedFactories.Length; i++)
                {
                    var factory = reuseMatchedFactories[i];
                    var reuse = factory.Value.Reuse;
                    var lifespan = reuse == null || reuse == Reuse.Transient ? int.MaxValue : reuse.Lifespan;
                    if (lifespan < minLifespan)
                    {
                        minLifespan = lifespan;
                        minLifespanFactory = factory;
                        multipleFactories = false;
                    }
                    else if (lifespan == minLifespan)
                    {
                        multipleFactories = true;
                    }
                }

                if (!multipleFactories && minLifespanFactory != null)
                    matchedFactories = new KV<object, Factory>[] { minLifespanFactory };
            }

            if (matchedFactories.Length == 1)
            {
                // add asResolutionCall for the factory to prevent caching of 
                // in-lined expression in context with not matching condition
                // issue: #382
                var matchedFactory = matchedFactories[0];
                if (!request.IsResolutionCall)
                    matchedFactory.Value.Setup = matchedFactory.Value.Setup.WithAsResolutionCall();
                else
                    request.ChangeServiceKey(matchedFactory.Key);
            }

            return matchedFactories;
        }

        private Factory GetWrapperFactoryOrDefault(Request request)
        {
            var serviceType = request.GetActualServiceType();
            // note: wrapper ignores the service key, and propagate service key to wrapped service

            var itemType = serviceType.GetArrayElementTypeOrNull();
            if (itemType != null)
                serviceType = typeof(IEnumerable<>).MakeGenericType(itemType);

            var factory = ((IContainer)this).GetWrapperFactoryOrDefault(serviceType);
            if (factory == null)
                return null;

            // tries to generate the factory to match the request
            if (factory.FactoryGenerator != null)
            {
                factory = factory.FactoryGenerator.GetGeneratedFactory(request);
                if (factory == null)
                    return null;
            }

            var condition = factory.Setup.Condition;
            if (condition != null && !condition(request))
                return null;

            return factory;
        }

#endregion

#region Implementation

        private int _disposed;
        private StackTrace _disposeStackTrace;

        private readonly Ref<Registry> _registry;
        private Ref<ImHashMap<Type, FactoryDelegate>[]> _defaultFactoryDelegateCache;

        private readonly IScope _singletonScope;
        private readonly IScope _currentScope;
        private readonly IScopeContext _scopeContext;

        private readonly IResolverContext _root;
        private readonly IResolverContext _parent;

        internal sealed class InstanceFactory : Factory
        {
            public static InstanceFactory Of(object instance,
                Type instanceType, IScope scope, IReuse reuse)
            {
                var instanceFactory = new InstanceFactory(instanceType, reuse);
                scope.SetOrAdd(instanceFactory.FactoryID, instance);
                return instanceFactory;
            }

            public override Type ImplementationType => _instanceType;
            private readonly Type _instanceType;

            public InstanceFactory(Type instanceType, IReuse reuse) : base(reuse)
            {
                _instanceType = instanceType;
            }

            /// <summary>Called from Resolve method</summary>
            public override FactoryDelegate GetDelegateOrDefault(Request request)
            {
                if (request.IsResolutionRoot)
                {
                    var decoratedExpr = request.Container.GetDecoratorExpressionOrDefault(request.WithResolvedFactory(this));
                    if (decoratedExpr != null)
                        return CompileToDelegate(decoratedExpr);
                }

                return GetInstanceFromScopeChainOrSingletons;
            }

            /// <summary>Called for Injection as dependency.</summary>
            public override Expr GetExpressionOrDefault(Request request) =>
                request.Container.GetDecoratorExpressionOrDefault(request.WithResolvedFactory(this)) ??
                CreateExpressionOrDefault(request);

            public override Expr CreateExpressionOrDefault(Request request) =>
                Resolver.CreateResolutionExpression(request, isRuntimeDependency: true);

#region Implementation

            private object GetInstanceFromScopeChainOrSingletons(IResolverContext r)
            {
                for (var scope = r.CurrentScope; scope != null; scope = scope.Parent)
                {
                    var result = GetAndUnwrapOrDefault(scope, FactoryID);
                    if (result != null)
                        return result;
                }

                var instance = GetAndUnwrapOrDefault(r.SingletonScope, FactoryID);
                return instance.ThrowIfNull(Error.UnableToFindSingletonInstance);
            }

            private static object GetAndUnwrapOrDefault(IScope scope, int factoryId)
            {
                object value;
                if (!scope.TryGet(out value, factoryId))
                    return null;
                return (value as WeakReference)?.Target.ThrowIfNull(Error.WeakRefReuseWrapperGCed)
                   ?? (value as HiddenDisposable)?.Value
                   ?? value;
            }

#endregion
        }

        internal sealed class Registry
        {
            public static readonly Registry Empty = new Registry();
            public static readonly Registry Default = new Registry(WrappersSupport.Wrappers);

            // Factories:
            public readonly ImHashMap<Type, object> Services;
            public readonly ImHashMap<Type, Factory[]> Decorators;
            public readonly ImHashMap<Type, Factory> Wrappers;

            // Cache:
            public readonly Ref<ImMap<Expr>> FactoryExpressionCache;

            public readonly Ref<ImHashMap<Type, FactoryDelegate>[]> DefaultFactoryDelegateCache;

            // key: KV where Key is ServiceType and object is ServiceKey
            // value: FactoryDelegate or/and IntTreeMap<{requiredServicType+preResolvedParent}, FactoryDelegate>
            public readonly Ref<ImHashMap<object, KV<FactoryDelegate, ImHashMap<object, FactoryDelegate>>>> KeyedFactoryDelegateCache;

            private enum IsChangePermitted { Permitted, Error, Ignored }
            private readonly IsChangePermitted _isChangePermitted;

            public Registry WithoutCache()
            {
                return new Registry(Services, Decorators, Wrappers,
                    Ref.Of(FactoryDelegateCache.Empty()),
                    Ref.Of(ImHashMap<object, KV<FactoryDelegate, ImHashMap<object, FactoryDelegate>>>.Empty),
                    Ref.Of(ImMap<Expr>.Empty),
                    _isChangePermitted);
            }

            private Registry(ImHashMap<Type, Factory> wrapperFactories = null)
                : this(ImHashMap<Type, object>.Empty,
                    ImHashMap<Type, Factory[]>.Empty,
                    wrapperFactories ?? ImHashMap<Type, Factory>.Empty,
                    Ref.Of(FactoryDelegateCache.Empty()),
                    Ref.Of(ImHashMap<object, KV<FactoryDelegate, ImHashMap<object, FactoryDelegate>>>.Empty),
                    Ref.Of(ImMap<Expr>.Empty),
                    IsChangePermitted.Permitted)
            { }

            private Registry(
                ImHashMap<Type, object> services,
                ImHashMap<Type, Factory[]> decorators,
                ImHashMap<Type, Factory> wrappers,
                Ref<ImHashMap<Type, FactoryDelegate>[]> defaultFactoryDelegateCache,
                Ref<ImHashMap<object, KV<FactoryDelegate, ImHashMap<object, FactoryDelegate>>>> keyedFactoryDelegateCache,
                Ref<ImMap<Expr>> factoryExpressionCache,
                IsChangePermitted isChangePermitted)
            {
                Services = services;
                Decorators = decorators;
                Wrappers = wrappers;
                DefaultFactoryDelegateCache = defaultFactoryDelegateCache;
                KeyedFactoryDelegateCache = keyedFactoryDelegateCache;
                FactoryExpressionCache = factoryExpressionCache;
                _isChangePermitted = isChangePermitted;
            }

            internal Registry WithServices(ImHashMap<Type, object> services)
            {
                return services == Services ? this :
                    new Registry(services, Decorators, Wrappers,
                        DefaultFactoryDelegateCache.NewRef(), KeyedFactoryDelegateCache.NewRef(),
                        FactoryExpressionCache.NewRef(), _isChangePermitted);
            }

            private Registry WithDecorators(ImHashMap<Type, Factory[]> decorators)
            {
                return decorators == Decorators ? this :
                    new Registry(Services, decorators, Wrappers,
                        DefaultFactoryDelegateCache.NewRef(), KeyedFactoryDelegateCache.NewRef(),
                        FactoryExpressionCache.NewRef(), _isChangePermitted);
            }

            private Registry WithWrappers(ImHashMap<Type, Factory> wrappers)
            {
                return wrappers == Wrappers ? this :
                    new Registry(Services, Decorators, wrappers,
                        DefaultFactoryDelegateCache.NewRef(), KeyedFactoryDelegateCache.NewRef(),
                        FactoryExpressionCache.NewRef(), _isChangePermitted);
            }

            public IEnumerable<ServiceRegistrationInfo> GetServiceRegistrations()
            {
                foreach (var entry in Services.Enumerate())
                {
                    var serviceType = entry.Key;
                    var factory = entry.Value as Factory;
                    if (factory != null)
                        yield return new ServiceRegistrationInfo(factory, serviceType, null);
                    else
                    {
                        var factories = ((FactoriesEntry)entry.Value).Factories;
                        foreach (var f in factories.Enumerate())
                            yield return new ServiceRegistrationInfo(f.Value, serviceType, f.Key);
                    }
                }
            }

            public Registry Register(Factory factory, Type serviceType, IfAlreadyRegistered ifAlreadyRegistered, object serviceKey)
            {
                if (_isChangePermitted != IsChangePermitted.Permitted)
                    return _isChangePermitted == IsChangePermitted.Ignored ? this
                        : Throw.For<Registry>(Error.NoMoreRegistrationsAllowed,
                            serviceType, serviceKey != null ? "with key " + serviceKey : string.Empty, factory);

                return factory.FactoryType == FactoryType.Service
                        ? WithService(factory, serviceType, serviceKey, ifAlreadyRegistered)
                    : factory.FactoryType == FactoryType.Decorator
                        ? WithDecorators(
                            Decorators.AddOrUpdate(serviceType, new[] { factory }, ArrayTools.Append))
                        : WithWrappers(
                            Wrappers.AddOrUpdate(serviceType, factory));
            }

            public Factory[] GetRegisteredFactories(Type serviceType, object serviceKey, FactoryType factoryType,
                Func<Factory, bool> condition)
            {
                serviceType = serviceType.ThrowIfNull();
                switch (factoryType)
                {
                    case FactoryType.Wrapper:
                        var arrayElementType = serviceType.GetArrayElementTypeOrNull();
                        if (arrayElementType != null)
                            serviceType = typeof(IEnumerable<>).MakeGenericType(arrayElementType);

                        var wrapper = Wrappers.GetValueOrDefault(serviceType.GetGenericDefinitionOrNull() ?? serviceType);
                        return wrapper != null && (condition == null || condition(wrapper))
                            ? new[] { wrapper }
                            : null;

                    case FactoryType.Decorator:
                        var decorators = Decorators.GetValueOrDefault(serviceType);

                        var openGenServiceType = serviceType.GetGenericDefinitionOrNull();
                        if (openGenServiceType != null)
                            decorators = decorators.Append(Decorators.GetValueOrDefault(openGenServiceType));

                        if (decorators != null && decorators.Length != 0)
                            return condition == null
                                ? decorators
                                : decorators.Match(condition);
                        return null;

                    default:
                        var entry = Services.GetValueOrDefault(serviceType);
                        if (entry == null)
                            return null;

                        var factory = entry as Factory;
                        if (factory != null)
                        {
                            if (serviceKey == null || DefaultKey.Value.Equals(serviceKey))
                                return condition == null || condition(factory)
                                    ? new[] { factory }
                                    : null;
                            return null;
                        }

                        var factories = ((FactoriesEntry)entry).Factories;
                        if (serviceKey == null)
                            return condition == null
                                ? factories.Enumerate().Map(f => f.Value).ToArrayOrSelf()
                                : factories.Enumerate().Match(f => condition(f.Value), f => f.Value).ToArrayOrSelf();

                        factory = factories.GetValueOrDefault(serviceKey);
                        return factory != null && (condition == null || condition(factory))
                            ? new[] { factory }
                            : null;
                }
            }

            public bool ClearCache(Type serviceType, object serviceKey, FactoryType factoryType)
            {
                var factories = GetRegisteredFactories(serviceType, serviceKey, factoryType, null);
                if (factories.IsNullOrEmpty())
                    return false;

                for (var i = 0; i < factories.Length; i++)
                    WithoutFactoryCache(factories[i], serviceType, serviceKey);

                return true;
            }

            private Registry WithService(Factory factory, Type serviceType, object serviceKey, IfAlreadyRegistered ifAlreadyRegistered)
            {
                Factory replacedFactory = null;
                ImHashMap<object, Factory> replacedFactories = null;
                ImHashMap<Type, object> services;
                if (serviceKey == null)
                {
                    services = Services.AddOrUpdate(serviceType, factory, (oldEntry, newEntry) =>
                    {
                        if (oldEntry == null)
                            return newEntry;

                        var newFactory = (Factory)newEntry;

                        var oldFactoriesEntry = oldEntry as FactoriesEntry;
                        if (oldFactoriesEntry != null && oldFactoriesEntry.LastDefaultKey == null) // no default registered yet
                            return oldFactoriesEntry.With(newFactory);

                        var oldFactory = oldFactoriesEntry == null ? (Factory)oldEntry : null;
                        switch (ifAlreadyRegistered)
                        {
                            case IfAlreadyRegistered.Throw:
                                oldFactory = oldFactory ?? oldFactoriesEntry.Factories.GetValueOrDefault(oldFactoriesEntry.LastDefaultKey);
                                return Throw.For<object>(Error.UnableToRegisterDuplicateDefault, serviceType, oldFactory);

                            case IfAlreadyRegistered.Keep:
                                return oldEntry;

                            case IfAlreadyRegistered.Replace:
                                if (oldFactoriesEntry != null)
                                {
                                    var newFactories = oldFactoriesEntry.Factories;
                                    if (oldFactoriesEntry.LastDefaultKey != null)
                                    {
                                        newFactories = ImHashMap<object, Factory>.Empty;
                                        var removedFactories = ImHashMap<object, Factory>.Empty;
                                        foreach (var f in newFactories.Enumerate())
                                            if (f.Key is DefaultKey)
                                                removedFactories = removedFactories.AddOrUpdate(f.Key, f.Value);
                                            else
                                                newFactories = newFactories.AddOrUpdate(f.Key, f.Value);
                                        replacedFactories = removedFactories;
                                    }

                                    return new FactoriesEntry(DefaultKey.Value,
                                        newFactories.AddOrUpdate(DefaultKey.Value, newFactory));
                                }

                                replacedFactory = oldFactory;
                                return newEntry;

                            case IfAlreadyRegistered.AppendNewImplementation:
                                var implementationType = newFactory.ImplementationType;
                                if (implementationType == null ||
                                    oldFactory != null && oldFactory.ImplementationType != implementationType ||
                                    oldFactoriesEntry != null && oldFactoriesEntry.Factories.Enumerate()
                                        .All(f => f.Value.ImplementationType != implementationType))
                                {
                                    return (oldFactoriesEntry ?? FactoriesEntry.Empty.With(oldFactory)).With(newFactory);
                                }

                                return oldEntry;

                            default:
                                return (oldFactoriesEntry ?? FactoriesEntry.Empty.With(oldFactory)).With(newFactory);
                        }
                    });
                }
                else // serviceKey != null
                {
                    var factories = FactoriesEntry.Empty.With(factory, serviceKey);
                    services = Services.AddOrUpdate(serviceType, factories, (oldEntry, newEntry) =>
                    {
                        if (oldEntry == null)
                            return newEntry;

                        if (oldEntry is Factory) // if registered is default, just add it to new entry
                            return ((FactoriesEntry)newEntry).With((Factory)oldEntry);

                        var oldFactories = (FactoriesEntry)oldEntry;
                        return new FactoriesEntry(oldFactories.LastDefaultKey,
                            oldFactories.Factories.AddOrUpdate(serviceKey, factory, (oldFactory, newFactory) =>
                            {
                                if (oldFactory == null)
                                    return factory;

                                switch (ifAlreadyRegistered)
                                {
                                    case IfAlreadyRegistered.Keep:
                                        return oldFactory;

                                    case IfAlreadyRegistered.Replace:
                                        replacedFactory = oldFactory;
                                        return newFactory;

                                    default:
                                        return Throw.For<Factory>(Error.UnableToRegisterDuplicateKey, serviceType, serviceKey, oldFactory);
                                }
                            }));
                    });
                }

                var registry = this;
                if (registry.Services != services)
                {
                    registry = new Registry(services, Decorators, Wrappers,
                        DefaultFactoryDelegateCache.NewRef(), KeyedFactoryDelegateCache.NewRef(),
                        FactoryExpressionCache.NewRef(), _isChangePermitted);

                    if (replacedFactory != null)
                        registry = registry.WithoutFactoryCache(replacedFactory, serviceType, serviceKey);
                    else if (replacedFactories != null)
                        foreach (var f in replacedFactories.Enumerate())
                            registry = registry.WithoutFactoryCache(f.Value, serviceType, serviceKey);
                }

                return registry;
            }

            public Registry Unregister(FactoryType factoryType, Type serviceType, object serviceKey, Func<Factory, bool> condition)
            {
                if (_isChangePermitted != IsChangePermitted.Permitted)
                    return _isChangePermitted == IsChangePermitted.Ignored ? this
                        : Throw.For<Registry>(Error.NoMoreUnregistrationsAllowed,
                            serviceType, serviceKey != null ? "with key " + serviceKey : string.Empty, factoryType);

                switch (factoryType)
                {
                    case FactoryType.Wrapper:
                        Factory removedWrapper = null;
                        var registry = WithWrappers(Wrappers.Update(serviceType, null, (factory, _null) =>
                        {
                            if (factory != null && condition != null && !condition(factory))
                                return factory;
                            removedWrapper = factory;
                            return null;
                        }));

                        return removedWrapper == null ? this : registry.WithoutFactoryCache(removedWrapper, serviceType);

                    case FactoryType.Decorator:
                        Factory[] removedDecorators = null;
                        registry = WithDecorators(Decorators.Update(serviceType, null, (factories, _null) =>
                        {
                            var remaining = condition == null ? null : factories.Match(f => !condition(f));
                            removedDecorators = remaining == null || remaining.Length == 0 ? factories : factories.Except(remaining).ToArray();
                            return remaining;
                        }));

                        if (removedDecorators.IsNullOrEmpty())
                            return this;

                        foreach (var removedDecorator in removedDecorators)
                            registry = registry.WithoutFactoryCache(removedDecorator, serviceType);

                        return registry;

                    default:
                        return UnregisterServiceFactory(serviceType, serviceKey, condition);
                }
            }

            private Registry UnregisterServiceFactory(Type serviceType, object serviceKey = null, Func<Factory, bool> condition = null)
            {
                object removed = null; // Factory or FactoriesEntry or Factory[]
                ImHashMap<Type, object> services;

                if (serviceKey == null && condition == null) // simplest case with simplest handling
                    services = Services.Update(serviceType, null, (entry, _null) =>
                    {
                        removed = entry;
                        return null;
                    });
                else
                    services = Services.Update(serviceType, null, (entry, _null) =>
                    {
                        if (entry == null)
                            return null;

                        if (entry is Factory)
                        {
                            if ((serviceKey != null && !DefaultKey.Value.Equals(serviceKey)) ||
                                (condition != null && !condition((Factory)entry)))
                                return entry; // keep entry
                            removed = entry; // otherwise remove it (the only case if serviceKey == DefaultKey.Value)
                            return null;
                        }

                        var factoriesEntry = (FactoriesEntry)entry;
                        var oldFactories = factoriesEntry.Factories;
                        var remainingFactories = ImHashMap<object, Factory>.Empty;
                        if (serviceKey == null) // automatically means condition != null
                        {
                            // keep factories for which condition is true
                            foreach (var factory in oldFactories.Enumerate())
                                if (condition != null && !condition(factory.Value))
                                    remainingFactories = remainingFactories.AddOrUpdate(factory.Key, factory.Value);
                        }
                        else // serviceKey is not default, which automatically means condition == null
                        {
                            // set to null factory with specified key if its found
                            remainingFactories = oldFactories;
                            var factory = oldFactories.GetValueOrDefault(serviceKey);
                            if (factory != null)
                                remainingFactories = oldFactories.Height > 1
                                    ? oldFactories.Update(serviceKey, null)
                                    : ImHashMap<object, Factory>.Empty;
                        }

                        if (remainingFactories.IsEmpty)
                        {
                            // if no more remaining factories, then delete the whole entry
                            removed = entry;
                            return null;
                        }

                        removed =
                            oldFactories.Enumerate()
                                .Except(remainingFactories.Enumerate())
                                .Select(f => f.Value)
                                .ToArray();

                        if (remainingFactories.Height == 1 && DefaultKey.Value.Equals(remainingFactories.Key))
                            return remainingFactories.Value; // replace entry with single remaining default factory

                        // update last default key if current default key was removed
                        var newDefaultKey = factoriesEntry.LastDefaultKey;
                        if (newDefaultKey != null && remainingFactories.GetValueOrDefault(newDefaultKey) == null)
                            newDefaultKey = remainingFactories.Enumerate().Select(x => x.Key)
                                .OfType<DefaultKey>().OrderByDescending(key => key.RegistrationOrder).FirstOrDefault();
                        return new FactoriesEntry(newDefaultKey, remainingFactories);
                    });

                if (removed == null)
                    return this;

                var registry = WithServices(services);

                if (removed is Factory)
                    return registry.WithoutFactoryCache((Factory)removed, serviceType, serviceKey);

                var removedFactories = removed as Factory[]
                    ?? ((FactoriesEntry)removed).Factories.Enumerate().Select(f => f.Value).ToArray();

                foreach (var removedFactory in removedFactories)
                    registry = registry.WithoutFactoryCache(removedFactory, serviceType, serviceKey);

                return registry;
            }

            // Does not change registry, returns Registry just for convenience of method chaining
            private Registry WithoutFactoryCache(Factory factory, Type serviceType, object serviceKey = null)
            {
                if (factory.FactoryGenerator != null)
                {
                    foreach (var f in factory.FactoryGenerator.GeneratedFactories.Enumerate())
                        WithoutFactoryCache(f.Value, f.Key.Key, f.Key.Value);
                }
                else
                {
                    // clean expression cache using FactoryID as key
                    FactoryExpressionCache.Swap(_ => _.Update(factory.FactoryID, null));

                    // clean default factory cache
                    DefaultFactoryDelegateCache.Swap(_ => _.Update(serviceType, null));

                    // clean keyed/context cache from keyed and context based resolutions
                    var keyedCacheKey = serviceKey == null ? (object)serviceType : new KV<object, object>(serviceType, serviceKey);
                    KeyedFactoryDelegateCache.Swap(_ => _.Update(keyedCacheKey, null));
                }

                return this;
            }

            public Registry WithNoMoreRegistrationAllowed(bool ignoreInsteadOfThrow)
            {
                var isChangePermitted = ignoreInsteadOfThrow ? IsChangePermitted.Ignored : IsChangePermitted.Error;
                return new Registry(Services, Decorators, Wrappers,
                    DefaultFactoryDelegateCache, KeyedFactoryDelegateCache, FactoryExpressionCache,
                    isChangePermitted);
            }
        }

        private Container(Rules rules, Ref<Registry> registry, IScope singletonScope,
            IScopeContext scopeContext = null, IScope currentScope = null,
            int disposed = 0, StackTrace disposeStackTrace = null,
            IResolverContext parent = null, IResolverContext root = null)
        {
            _disposed = disposed;
            _disposeStackTrace = disposeStackTrace;

            _parent = parent;
            _root = root;

            Rules = rules;

            _registry = registry;
            _defaultFactoryDelegateCache = registry.Value.DefaultFactoryDelegateCache;

            _singletonScope = singletonScope;

            _currentScope = currentScope;
            _scopeContext = scopeContext;
        }

#endregion
    }

    // Hides/wraps object with disposable interface.
    internal sealed class HiddenDisposable
    {
        public static ConstructorInfo Ctor = typeof(HiddenDisposable).GetTypeInfo().DeclaredConstructors.First();
        internal static FieldInfo ValueField = typeof(HiddenDisposable).GetTypeInfo().DeclaredFields.First();
        public readonly object Value;
        public HiddenDisposable(object value)
        {
            Value = value;
        }
    }

    internal static class FactoryDelegateCache
    {
        private const int NumberOfMapBuckets = 16;
        private const int NumberOfMapMask = NumberOfMapBuckets - 1;  // get last 4 bits, fast (hash % NumberOfTrees)

        public static ImHashMap<Type, FactoryDelegate>[] Empty()
        {
            return new ImHashMap<Type, FactoryDelegate>[NumberOfMapBuckets];
        }

        [MethodImpl((MethodImplOptions)256)]
        public static FactoryDelegate GetValueOrDefault(this ImHashMap<Type, FactoryDelegate>[] maps, Type key)
        {
            var hash = key.GetHashCode();

            var map = maps[hash & NumberOfMapMask];
            if (map == null)
                return null;

            for (; map.Height != 0; map = hash < map.Hash ? map.Left : map.Right)
            {
                if (map.Key == key)
                    return map.Value;
                if (map.Hash == hash)
                    return map.GetConflictedValueOrDefault(key, null);
            }

            return null;
        }

        [MethodImpl((MethodImplOptions)256)]
        public static ImHashMap<Type, FactoryDelegate>[] AddOrUpdate(this ImHashMap<Type, FactoryDelegate>[] maps,
            Type key, FactoryDelegate value)
        {
            var hash = key.GetHashCode();

            var mapIndex = hash & NumberOfMapMask;
            var map = maps[mapIndex];
            if (map == null)
                map = ImHashMap<Type, FactoryDelegate>.Empty;

            map = map.AddOrUpdate(hash, key, value);

            var newMaps = new ImHashMap<Type, FactoryDelegate>[NumberOfMapBuckets];
            Array.Copy(maps, 0, newMaps, 0, NumberOfMapBuckets);
            newMaps[mapIndex] = map;

            return newMaps;
        }

        [MethodImpl((MethodImplOptions)256)]
        public static ImHashMap<Type, FactoryDelegate>[] Update(this ImHashMap<Type, FactoryDelegate>[] maps,
            Type key, FactoryDelegate value)
        {
            var hash = key.GetHashCode();

            var mapIndex = hash & NumberOfMapMask;
            var map = maps[mapIndex];
            if (map == null)
                return maps;

            var newMap = map.Update(hash, key, value, null);
            if (newMap == map)
                return maps;

            var newMaps = new ImHashMap<Type, FactoryDelegate>[NumberOfMapBuckets];
            Array.Copy(maps, 0, newMaps, 0, NumberOfMapBuckets);
            newMaps[mapIndex] = newMap;

            return newMaps;
        }
    }

    /// <summary>Container extended features.</summary>
    public static class ContainerTools
    {
        /// <summary>The default key for services registered into container created by <see cref="CreateFacade"/></summary>
        public const string FacadeKey = "<facade-key>";

        /// <summary>Allows to register new specially keyed services which will facade the same default service,
        /// registered earlier. May be used to "override" resgitrations when testing the container</summary>
        public static IContainer CreateFacade(this IContainer container, string facadeKey = FacadeKey) =>
            container.With(rules => rules.WithFactorySelector(Rules.SelectKeyedOverDefaultFactory(FacadeKey)));

        /// <summary>Shares all of container state except the cache and the new rules.</summary>
        public static IContainer With(this IContainer container, Func<Rules, Rules> configure = null, IScopeContext scopeContext = null) =>
            container.With(configure?.Invoke(container.Rules), scopeContext,
                WithRegistrationsOptions.CloneWithoutCache);

        /// <summary>Returns new container with all expression, delegate, items cache removed/reset.
        /// But it will preserve resolved services in Singleton/Current scope.</summary>
        public static IContainer WithoutCache(this IContainer container) =>
            container.With(container.Rules, container.ScopeContext,
                WithRegistrationsOptions.CloneWithoutCache);

        /// <summary>Creates new container with state shared with original except singletons and cache.
        /// Dropping cache is required because singletons are cached in resolution state.</summary>
        public static IContainer WithoutSingletonsAndCache(this IContainer container) =>
            container.With(container.Rules, container.ScopeContext,
                WithRegistrationsOptions.CloneWithoutCache, WithSingletonOptions.Drop);

        /// <summary>Shares all parts with original container But copies registration, so the new registration
        /// won't be visible in original. Registrations include decorators and wrappers as well.</summary>
        public static IContainer WithRegistrationsCopy(this IContainer container, bool preserveCache = false) =>
            container.With(container.Rules, container.ScopeContext, WithRegistrationsOptions.Clone);

        /// <summary>For given instance resolves and sets properties and fields.
        /// It respects <see cref="Rules.PropertiesAndFields"/> rules set per container,
        /// or if rules are not set it uses <see cref="PropertiesAndFields.Auto"/>.</summary>
        public static TService InjectPropertiesAndFields<TService>(this IResolverContext r, TService instance) =>
            r.InjectPropertiesAndFields<TService>(instance, null);

        /// <summary>For given instance resolves and sets properties and fields. You may specify what 
        /// properties and fields.</summary>
        public static TService InjectPropertiesAndFields<TService>(this IResolverContext r, TService instance,
            params string[] propertyAndFieldNames)
        {
            r.InjectPropertiesAndFields(instance, propertyAndFieldNames);
            return instance;
        }

        /// <summary>Creates service using container for injecting parameters without registering anything in <paramref name="container"/>.</summary>
        /// <param name="container">Container to use for type creation and injecting its dependencies.</param>
        /// <param name="concreteType">Type to instantiate. Wrappers (Func, Lazy, etc.) is also supported.</param>
        /// <param name="made">(optional) Injection rules to select constructor/factory method, inject parameters, properties and fields.</param>
        /// <returns>Object instantiated by constructor or object returned by factory method.</returns>
        public static object New(this IContainer container, Type concreteType, Made made = null)
        {
            var containerCopy = container.WithRegistrationsCopy();
            var implType = containerCopy.GetWrappedType(concreteType, null);
            containerCopy.Register(implType, made: made);
            // No need to Dispose facade because it shares singleton/open scopes with source container, and disposing source container does the job.
            return containerCopy.Resolve(concreteType, IfUnresolved.Throw);
        }

        /// <summary>Creates service using container for injecting parameters without registering anything in <paramref name="container"/>.</summary>
        /// <typeparam name="T">Type to instantiate.</typeparam>
        /// <param name="container">Container to use for type creation and injecting its dependencies.</param>
        /// <param name="made">(optional) Injection rules to select constructor/factory method, inject parameters, properties and fields.</param>
        /// <returns>Object instantiated by constructor or object returned by factory method.</returns>
        public static T New<T>(this IContainer container, Made made = null) =>
            (T)container.New(typeof(T), made);

        /// <summary>Creates service given strongly-typed creation expression.
        /// Can be used to invoke arbitrary method returning some value with injecting its parameters from container.</summary>
        /// <typeparam name="T">Method or constructor result type.</typeparam>
        /// <param name="container">Container to use for injecting dependencies.</param>
        /// <param name="made">Creation expression.</param>
        /// <returns>Created result.</returns>
        public static T New<T>(this IContainer container, Made.TypedMade<T> made) =>
            (T)container.New(typeof(T), made);

        /// <summary>Registers new service type with factory for registered service type.
        /// Throw if no such registered service type in container.</summary>
        /// <param name="container">Container</param> <param name="serviceType">New service type.</param>
        /// <param name="registeredServiceType">Existing registered service type.</param>
        /// <param name="serviceKey">(optional)</param> <param name="registeredServiceKey">(optional)</param>
        /// <remarks>Does nothing if registration is already exists.</remarks>
        public static void RegisterMapping(this IContainer container, Type serviceType, Type registeredServiceType,
            object serviceKey = null, object registeredServiceKey = null)
        {
            var request = Request.Create(container, registeredServiceType, registeredServiceKey);
            var factory = container.GetServiceFactoryOrDefault(request);
            factory.ThrowIfNull(Error.RegisterMappingNotFoundRegisteredService,
                registeredServiceType, registeredServiceKey);
            container.Register(factory, serviceType, serviceKey, IfAlreadyRegistered.Keep, false);
        }

        /// <summary>Registers new service type with factory for registered service type.
        /// Throw if no such registered service type in container.</summary>
        /// <param name="container">Container</param>
        /// <typeparam name="TService">New service type.</typeparam>
        /// <typeparam name="TRegisteredService">Existing registered service type.</typeparam>
        /// <param name="serviceKey">(optional)</param> <param name="registeredServiceKey">(optional)</param>
        /// <remarks>Does nothing if registration is already exists.</remarks>
        public static void RegisterMapping<TService, TRegisteredService>(this IContainer container,
            object serviceKey = null, object registeredServiceKey = null) =>
            container.RegisterMapping(typeof(TService), typeof(TRegisteredService), serviceKey, registeredServiceKey);

        /// <summary>Register a service without implementation which can be provided later in terms
        /// of normal registration with IfAlreadyRegistered.Replace parameter.
        /// When the implementation is still not provided when the placeholder service is accessed,
        /// then the exception will be thrown.
        /// This feature allows you to postpone decision on implementation until it is later known.</summary>
        /// <remarks>Internally the empty factory is registered with the setup asResolutionCall set to true.
        /// That means, instead of placing service instance into graph expression we put here redirecting call to
        /// container Resolve.</remarks>
        public static void RegisterPlaceholder(this IContainer container, Type serviceType, object serviceKey = null) =>
            container.Register(FactoryPlaceholder.Default, serviceType, serviceKey, IfAlreadyRegistered.AppendNotKeyed, true);

        /// <summary>Register a service without implementation which can be provided later in terms
        /// of normal registration with IfAlreadyRegistered.Replace parameter.
        /// When the implementation is still not provided when the placeholder service is accessed,
        /// then the exception will be thrown.
        /// This feature allows you to postpone decision on implementation until it is later known.</summary>
        /// <remarks>Internally the empty factory is registered with the setup asResolutionCall set to true.
        /// That means, instead of placing service instance into graph expression we put here redirecting call to
        /// container Resolve.</remarks>
        public static void RegisterPlaceholder<TService>(this IContainer container, object serviceKey = null) =>
            container.RegisterPlaceholder(typeof(TService), serviceKey);

        /// <summary>Obsolete: please use WithAutoFallbackDynamicRegistration</summary>
        [Obsolete("Please use WithAutoFallbackDynamicRegistration instead")]
        public static IContainer WithAutoFallbackResolution(this IContainer container,
            IEnumerable<Type> implTypes,
            Func<IReuse, Request, IReuse> changeDefaultReuse = null,
            Func<Request, bool> condition = null) =>
            container.ThrowIfNull().With(rules =>
                rules.WithUnknownServiceResolvers(
                    Rules.AutoRegisterUnknownServiceRule(implTypes, changeDefaultReuse, condition)));

        /// <summary>Obsolete: please use WithAutoFallbackDynamicRegistration</summary>
        [Obsolete("Please use WithAutoFallbackDynamicRegistration instead")]
        public static IContainer WithAutoFallbackResolution(this IContainer container,
            IEnumerable<Assembly> implTypeAssemblies,
            Func<IReuse, Request, IReuse> changeDefaultReuse = null,
            Func<Request, bool> condition = null)
        {
            var types = implTypeAssemblies.ThrowIfNull()
                .SelectMany(assembly => assembly.GetLoadedTypes())
                .Where(Registrator.IsImplementationType)
                .ToArray();
            return container.WithAutoFallbackResolution(types, changeDefaultReuse, condition);
        }

        /// <summary>Provides automatic fallback resolution mechanism for not normally registered
        /// services. Underneath uses <see cref="Rules.WithDynamicRegistrations"/>.</summary>
        public static IContainer WithAutoFallbackDynamicRegistrations(this IContainer container,
            Func<Type, object, IEnumerable<Type>> getImplTypes, Func<Type, Factory> factory = null) =>
            container.ThrowIfNull()
                .With(rules => rules.WithDynamicRegistrationsAsFallback(
                    Rules.AutoFallbackDynamicRegistrations(getImplTypes, factory)));

        /// <summary>Provides automatic fallback resolution mechanism for not normally registered
        /// services. Underneath uses <see cref="Rules.WithDynamicRegistrations"/>.</summary>
        public static IContainer WithAutoFallbackDynamicRegistrations(this IContainer container, params Type[] implTypes) =>
            container.WithAutoFallbackDynamicRegistrations((_, __) => implTypes);

        /// <summary>Provides automatic fallback resolution mechanism for not normally registered
        /// services. Underneath uses <see cref="Rules.WithDynamicRegistrations"/>.</summary>
        public static IContainer WithAutoFallbackDynamicRegistrations(this IContainer container,
            IReuse reuse, params Type[] implTypes) =>
            container.WithAutoFallbackDynamicRegistrations((_, __) => implTypes, implType => new ReflectionFactory(implType, reuse));

        /// <summary>Provides automatic fallback resolution mechanism for not normally registered
        /// services. Underneath uses <see cref="Rules.WithDynamicRegistrations"/>.</summary>
        public static IContainer WithAutoFallbackDynamicRegistrations(this IContainer container,
            IReuse reuse, Setup setup, params Type[] implTypes) =>
            container.WithAutoFallbackDynamicRegistrations(
                (ignoredServiceType, ignoredServiceKey) => implTypes,
                implType => new ReflectionFactory(implType, reuse, setup: setup));

        /// <summary>Provides automatic fallback resolution mechanism for not normally registered
        /// services. Underneath uses <see cref="Rules.WithDynamicRegistrations"/>.</summary>
        public static IContainer WithAutoFallbackDynamicRegistrations(this IContainer container,
            Func<Type, object, IEnumerable<Assembly>> getImplTypeAssemblies,
            Func<Type, Factory> factory = null) =>
            container.ThrowIfNull().With(rules => rules.WithDynamicRegistrations(
                Rules.AutoFallbackDynamicRegistrations(
                    (serviceType, serviceKey) =>
                    {
                        var assemblies = getImplTypeAssemblies(serviceType, serviceKey);
                        if (assemblies == null)
                            return ArrayTools.Empty<Type>();
                        return assemblies
                            .SelectMany(ReflectionTools.GetLoadedTypes)
                            .Where(Registrator.IsImplementationType)
                            .ToArray();
                    },
                    factory)));

        /// <summary>Provides automatic fallback resolution mechanism for not normally registered
        /// services. Underneath uses <see cref="Rules.WithDynamicRegistrations"/>.</summary>
        public static IContainer WithAutoFallbackDynamicRegistrations(this IContainer container,
            params Assembly[] implTypeAssemblies) =>
            container.WithAutoFallbackDynamicRegistrations((_, __) => implTypeAssemblies);

        /// <summary>Provides automatic fallback resolution mechanism for not normally registered
        /// services. Underneath uses <see cref="Rules.WithDynamicRegistrations"/>.</summary>
        public static IContainer WithAutoFallbackDynamicRegistrations(this IContainer container,
            IEnumerable<Assembly> implTypeAssemblies) =>
            container.WithAutoFallbackDynamicRegistrations((_, __) => implTypeAssemblies);

        /// <summary>Creates new container with provided parameters and properties
        /// to pass the custom dependency values for injection. The old parameters and properties are overridden,
        /// but not replaced.</summary>
        /// <param name="container">Container to work with.</param>
        /// <param name="parameters">(optional) Parameters specification, can be used to proved custom values.</param>
        /// <param name="propertiesAndFields">(optional) Properties and fields specification, can be used to proved custom values.</param>
        /// <returns>New container with adjusted rules.</returns>
        /// <example><code lang="cs"><![CDATA[
        ///     var c = container.WithDependencies(Parameters.Of.Type<string>(_ => "Nya!"));
        ///     var a = c.Resolve<A>(); // where A accepts string parameter in constructor
        ///     Assert.AreEqual("Nya!", a.Message)
        /// ]]></code></example>
        public static IContainer WithDependencies(this IContainer container,
            ParameterSelector parameters = null, PropertiesAndFieldsSelector propertiesAndFields = null) =>
            container.With(rules => rules.With(Made.Of(
                parameters: rules.Parameters.OverrideWith(parameters),
                propertiesAndFields: rules.PropertiesAndFields.OverrideWith(propertiesAndFields)),
                overrideRegistrationMade: true));

        /// <summary>Pre-defined what-registrations predicate for <seealso cref="GenerateResolutionExpressions"/>.</summary>
        public static Func<ServiceRegistrationInfo, bool> SetupAsResolutionRoots =
            r => r.Factory.Setup.AsResolutionRoot;

        /// <summary>Generates all resolution root and calls expressions.</summary>
        /// <param name="container">For container</param>
        /// <param name="resolutions">Result resolution factory expressions. They could be compiled and used for actual service resolution.</param>
        /// <param name="resolutionCallDependencies">Resolution call dependencies (implemented via Resolve call): e.g. dependencies wrapped in Lazy{T}.</param>
        /// <param name="whatRegistrations">(optional) Allow to filter what registration to resolve. By default applies to all registrations.
        /// You may use <see cref="SetupAsResolutionRoots"/> to generate only for registrations with <see cref="Setup.AsResolutionRoot"/>.</param>
        /// <returns>Errors happened when resolving corresponding registrations.</returns>
        public static KeyValuePair<ServiceRegistrationInfo, ContainerException>[] GenerateResolutionExpressions(
            this IContainer container,
            out KeyValuePair<ServiceRegistrationInfo, Expression<FactoryDelegate>>[] resolutions,
            out KeyValuePair<RequestInfo, Expression>[] resolutionCallDependencies,
            Func<ServiceRegistrationInfo, bool> whatRegistrations = null)
        {
            var generatingContainer = container.With(rules => rules
                .WithoutEagerCachingSingletonForFasterAccess()
                .WithoutImplicitCheckForReuseMatchingScope()
                .WithDependencyResolutionCallExpressions());

            var registrations = generatingContainer.GetServiceRegistrations()
                // ignore open-generic registrations because they may be resolved only when closed.
                .Where(r => !r.ServiceType.IsOpenGeneric());

            if (whatRegistrations != null)
                registrations = registrations.Where(whatRegistrations);

            var exprs = new List<KeyValuePair<ServiceRegistrationInfo, Expression<FactoryDelegate>>>();
            var errors = new List<KeyValuePair<ServiceRegistrationInfo, ContainerException>>();
            foreach (var registration in registrations)
            {
                try
                {
                    var request = Request.Create(generatingContainer, registration.ServiceType, registration.OptionalServiceKey);
                    var factoryExpr = Container.WrapInFactoryExpression(registration.Factory.GetExpressionOrDefault(request));

                    exprs.Add(new KeyValuePair<ServiceRegistrationInfo, Expression<FactoryDelegate>>(registration, factoryExpr
#if FEC_EXPRESSION_INFO
                        .ToLambdaExpression()
#endif
                    ));

                }
                catch (ContainerException ex)
                {
                    errors.Add(new KeyValuePair<ServiceRegistrationInfo, ContainerException>(registration, ex));
                }
            }

            resolutions = exprs.ToArray();

            resolutionCallDependencies = 
                generatingContainer.Rules.DependencyResolutionCallExpressions.Value.Enumerate()
                .Select(r => new KeyValuePair<RequestInfo, Expression>(r.Key, r.Value))
                .ToArray();

            return errors.ToArray();
        }

        /// <summary>Used to find potential problems when resolving the passed services <paramref name="resolutionRoots"/>.
        /// Method will collect the exceptions when resolving or injecting the specific registration.
        /// Does not create any actual service objects.</summary>
        /// <param name="container">for container</param>
        /// <param name="resolutionRoots">(optional) Examined resolved services. If empty will try to resolve every service in container.</param>
        /// <returns>Exceptions happened for corresponding registrations.</returns>
        public static KeyValuePair<ServiceRegistrationInfo, ContainerException>[] Validate(
            this IContainer container, params Type[] resolutionRoots) =>
            container.Validate(resolutionRoots.IsNullOrEmpty()
                ? (Func<ServiceRegistrationInfo, bool>)null
                : registration => resolutionRoots.IndexOf(registration.ServiceType) != -1);

        /// <summary>Used to find potential problems in service registration setup.
        /// Method tries to generate expressions for specified registrations, collects happened exceptions, and
        /// returns them to user. Does not create any actual service objects.</summary>
        public static KeyValuePair<ServiceRegistrationInfo, ContainerException>[] Validate(this IContainer container,
            Func<ServiceRegistrationInfo, bool> whatRegistrations = null)
        {
            KeyValuePair<ServiceRegistrationInfo, Expression<FactoryDelegate>>[] ignoredRoots;
            KeyValuePair<RequestInfo, Expression>[] ignoredDeps;
            return container.GenerateResolutionExpressions(out ignoredRoots, out ignoredDeps, whatRegistrations);
        }

        /// <summary>Replaced with Validate</summary>
        [Obsolete("Replaced with Validate", false)]
        public static KeyValuePair<ServiceRegistrationInfo, ContainerException>[] VerifyResolutions(this IContainer container,
            Func<ServiceRegistrationInfo, bool> whatRegistrations = null) => 
            container.Validate(whatRegistrations);

        /// <summary>Converts to self or system expression</summary>
        public static Expression ToSystemExpression(this Expr expr) =>
            expr
#if FEC_EXPRESSION_INFO
            .ToExpression()
#endif
            ;

        /// <summary>Represents construction of whole request info stack as expression.</summary>
        /// <param name="container">Required to access container facilities for expression conversion.</param>
        /// <param name="request">Request info to convert to expression.</param>
        /// <param name="opensResolutionScope">(optional) Will add <see cref="RequestFlags.OpensResolutionScope"/> to result expression.</param>
        /// <returns>Returns result expression.</returns>
        public static Expr RequestInfoToExpression(this IContainer container,
            RequestInfo request, bool opensResolutionScope = false)
        {
            if (request.IsEmpty)
                return opensResolutionScope
                    ? _emptyOpensResolutionScopeRequestInfoExpr.Value
                    : _emptyRequestInfoExpr.Value;

            // recursively ask for parent expression until it is empty
            var parentRequestInfoExpr = container.RequestInfoToExpression(request.DirectParent);

            var serviceType = request.ServiceType;
            var factoryID = request.FactoryID;
            var implementationType = request.ImplementationType;
            var requiredServiceType = request.RequiredServiceType;
            var serviceKey = request.ServiceKey;
            var metadataKey = request.MetadataKey;
            var metadata = request.Metadata;
            var factoryType = request.FactoryType;
            var ifUnresolved = request.IfUnresolved;
            var flags = request.Flags;
            if (opensResolutionScope)
                flags |= RequestFlags.OpensResolutionScope;

            var serviceTypeExpr = Constant(serviceType, typeof(Type));
            var factoryIdExpr = Constant(factoryID, typeof(int));
            var implTypeExpr = Constant(implementationType, typeof(Type));
            var reuseExpr = request.Reuse == null
                ? Constant(null, typeof(IReuse))
                : request.Reuse.ToExpression(it => container.GetItemExpression(it));

            if (ifUnresolved == IfUnresolved.Throw &&
                requiredServiceType == null && serviceKey == null && metadataKey == null && metadata == null &&
                factoryType == FactoryType.Service && flags == default(RequestFlags))
                return Call(parentRequestInfoExpr, RequestInfo.PushMethodWith4Args.Value,
                    serviceTypeExpr, factoryIdExpr, implTypeExpr, reuseExpr);

            var requiredServiceTypeExpr = Constant(requiredServiceType, typeof(Type));
            var servicekeyExpr = Convert(container.GetItemExpression(serviceKey), typeof(object));
            var factoryTypeExpr = Constant(factoryType, typeof(FactoryType));
            var flagsExpr = Constant(flags, typeof(RequestFlags));

            if (ifUnresolved == IfUnresolved.Throw &&
                metadataKey == null && metadata == null)
                return Call(parentRequestInfoExpr, RequestInfo.PushMethodWith8Args.Value,
                    serviceTypeExpr, requiredServiceTypeExpr, servicekeyExpr,
                    factoryIdExpr, factoryTypeExpr, implTypeExpr, reuseExpr, flagsExpr);

            var ifUnresolvedExpr = Constant(ifUnresolved, typeof(IfUnresolved));

            if (metadataKey == null && metadata == null)
                return Call(parentRequestInfoExpr, RequestInfo.PushMethodWith9Args.Value,
                    serviceTypeExpr, requiredServiceTypeExpr, servicekeyExpr, ifUnresolvedExpr,
                    factoryIdExpr, factoryTypeExpr, implTypeExpr, reuseExpr, flagsExpr);

            var metadataKeyExpr = Constant(metadataKey, typeof(string));
            var metadataExpr = Convert(container.GetItemExpression(metadata), typeof(object));

            return Call(parentRequestInfoExpr, RequestInfo.PushMethodWith11Args.Value,
                serviceTypeExpr, requiredServiceTypeExpr, servicekeyExpr, metadataKeyExpr, metadataExpr, ifUnresolvedExpr,
                factoryIdExpr, factoryTypeExpr, implTypeExpr, reuseExpr, flagsExpr);
        }

        private static readonly Lazy<Expr> _emptyRequestInfoExpr = new Lazy<Expr>(() =>
            Field(null, typeof(RequestInfo).Field(nameof(RequestInfo.Empty))));

        private static readonly Lazy<Expr> _emptyOpensResolutionScopeRequestInfoExpr = new Lazy<Expr>(() =>
            Field(null, typeof(RequestInfo).Field(nameof(RequestInfo.EmptyOpensResolutionScope))));

        /// <summary>Clears delegate and expression cache for specified <typeparamref name="T"/>.
        /// But does not clear instances of already resolved/created singletons and scoped services!</summary>
        /// <typeparam name="T">Target service or wrapper type.</typeparam>
        /// <param name="container">Container to operate.</param>
        /// <param name="factoryType">(optional) If not specified, clears cache for all <see cref="FactoryType"/>.</param>
        /// <param name="serviceKey">(optional) If omitted, the cache will be cleared for all registrations of <typeparamref name="T"/>.</param>
        /// <returns>True if type is found in the cache and cleared - false otherwise.</returns>
        public static bool ClearCache<T>(this IContainer container, FactoryType? factoryType = null, object serviceKey = null) =>
            container.ClearCache(typeof(T), factoryType, serviceKey);

        /// <summary>Clears delegate and expression cache for specified service.
        /// But does not clear instances of already resolved/created singletons and scoped services!</summary>
        /// <param name="container">Container to operate.</param>
        /// <param name="serviceType">Target service type.</param>
        /// <param name="factoryType">(optional) If not specified, clears cache for all <see cref="FactoryType"/>.</param>
        /// <param name="serviceKey">(optional) If omitted, the cache will be cleared for all registrations of <paramref name="serviceType"/>.</param>
        /// <returns>True if type is found in the cache and cleared - false otherwise.</returns>
        public static bool ClearCache(this IContainer container, Type serviceType,
            FactoryType? factoryType = null, object serviceKey = null) =>
            container.ClearCache(serviceType, factoryType, serviceKey);
    }

    /// <summary>Interface used to convert reuse instance to expression.</summary>
    public interface IConvertibleToExpression
    {
        /// <summary>Returns expression representation without closure.</summary>
        /// <param name="fallbackConverter">Delegate converting of sub-items, constants to container.</param>
        /// <returns>Expression representation.</returns>
        Expr ToExpression(Func<object, Expr> fallbackConverter);
    }

    /// <summary>Used to represent multiple default service keys.
    /// Exposes <see cref="RegistrationOrder"/> to determine order of service added.</summary>
    public sealed class DefaultKey : IConvertibleToExpression
    {
        /// <summary>Default value.</summary>
        public static readonly DefaultKey Value = new DefaultKey(0);

        /// <summary>Allows to determine service registration order.</summary>
        public readonly int RegistrationOrder;

        /// <summary>Returns the default key with specified registration order.</summary>
        public static DefaultKey Of(int registrationOrder) =>
            registrationOrder == 0 ? Value : new DefaultKey(registrationOrder);

        private static readonly MethodInfo _ofMethod = 
            typeof(DefaultKey).Method(nameof(Of));

        /// <inheritdoc />
        public Expr ToExpression(Func<object, Expr> fallbackConverter) =>
            Call(_ofMethod, Constant(RegistrationOrder));

        /// <summary>Returns next default key with increased <see cref="RegistrationOrder"/>.</summary>
        public DefaultKey Next() => Of(RegistrationOrder + 1);

        /// <summary>Compares keys based on registration order. The null (represents default) key is considered equal.</summary>
        public override bool Equals(object key) =>
            key == null || (key as DefaultKey)?.RegistrationOrder == RegistrationOrder;

        /// <summary>Returns registration order as hash.</summary>
        public override int GetHashCode() => RegistrationOrder;

        /// <summary>Prints registration order to string.</summary>
        public override string ToString() => 
            GetType().Name + "(" + RegistrationOrder + ")";

        private DefaultKey(int registrationOrder)
        {
            RegistrationOrder = registrationOrder;
        }
    }

    // todo: Simplify by either inheriting or composing with DefaultKey
    /// <summary>Represents default key for dynamic registrations</summary>
    public sealed class DefaultDynamicKey : IConvertibleToExpression
    {
        /// <summary>Default value.</summary>
        public static readonly DefaultDynamicKey Value = new DefaultDynamicKey(0);

        /// <summary>Associated ID.</summary>
        public readonly int RegistrationOrder;

        /// <summary>Returns dynamic key with specified ID.</summary>
        public static DefaultDynamicKey Of(int registrationOrder) =>
            registrationOrder == 0 ? Value : new DefaultDynamicKey(registrationOrder);

        private static readonly MethodInfo _ofMethod =
            typeof(DefaultDynamicKey).Method(nameof(Of));

        /// <inheritdoc />
        public Expr ToExpression(Func<object, Expr> fallbackConverter) =>
            Call(_ofMethod, Constant(RegistrationOrder));

        /// <summary>Returns next dynamic key with increased <see cref="RegistrationOrder"/>.</summary> 
        public DefaultDynamicKey Next() => Of(RegistrationOrder + 1);

        /// <summary>Compares key's IDs. The null (default) key is considered equal!</summary>
        public override bool Equals(object key) =>
            key == null || (key as DefaultDynamicKey)?.RegistrationOrder == RegistrationOrder;

        /// <summary>Returns key index as hash.</summary>
        public override int GetHashCode() => RegistrationOrder;

        /// <summary>Prints registration order to string.</summary>
        public override string ToString() => GetType().Name + "(" + RegistrationOrder + ")";

        private DefaultDynamicKey(int registrationOrder)
        {
            RegistrationOrder = registrationOrder;
        }
    }

    /// <summary>Extends IResolver to provide an access to scope hierarchy.</summary>
    public interface IResolverContext : IResolver, IDisposable
    {
        /// <summary>True if container is disposed.</summary>
        bool IsDisposed { get; }

        /// <summary>Parent context of the scoped context.</summary>
        IResolverContext Parent { get; }

        /// <summary>The root context of the scoped context.</summary>
        IResolverContext Root { get; }

        /// <summary>Singleton scope, always associated with root scope.</summary>
        IScope SingletonScope { get; }

        /// <summary>Current opened scope.</summary>
        IScope CurrentScope { get; }

        /// <summary>Optional scope context associated with container.</summary>
        IScopeContext ScopeContext { get; }

        /// <summary>Opens scope with optional name.</summary>
        /// <param name="name">(optional)</param>
        /// <param name="trackInParent">(optional) Instructs to additionally store the opened scope in parent, 
        /// so it will be disposed when parent is disposed. If no parent scope is available the scope will be tracked by Singleton scope.
        /// Used to dispose a resolution scope.</param>
        /// <returns>Scoped resolver context.</returns>
        /// <example><code lang="cs"><![CDATA[
        /// using (var scope = container.OpenScope())
        /// {
        ///     var handler = scope.Resolve<IHandler>();
        ///     handler.Handle(data);
        /// }
        /// ]]></code></example>
        IResolverContext OpenScope(object name = null, bool trackInParent = false);

        /// <summary>Allows to put instance into the scope.</summary>
        void UseInstance(Type serviceType, object instance, IfAlreadyRegistered IfAlreadyRegistered,
            bool preventDisposal, bool weaklyReferenced, object serviceKey);

        /// <summary>For given instance resolves and sets properties and fields.</summary>
        void InjectPropertiesAndFields(object instance, string[] propertyAndFieldNames);
    }

    /// <summary>Provides the shortcuts to <see cref="IResolverContext"/></summary>
    public static class ResolverContext
    {
        /// <summary>Just a sugar that allow to get root or self container.</summary>
        public static IResolverContext RootOrSelf(this IResolverContext r) => r.Root ?? r;

        internal static readonly PropertyInfo ParentProperty =
            typeof(IResolverContext).Property(nameof(IResolverContext.Parent));

        internal static readonly MethodInfo OpenScopeMethod =
            typeof(IResolverContext).Method(nameof(IResolverContext.OpenScope));

        /// <summary>Returns root or self resolver based on request.</summary>
        public static Expr GetRootOrSelfExpr(Request request) =>
            request.DirectRuntimeParent.IsSingletonOrDependencyOfSingleton && !request.OpensResolutionScope
                ? RootOrSelfExpr
                : Container.ResolverContextParamExpr;

        /// <summary>Resolver context parameter expression in FactoryDelegate.</summary>
        public static readonly Expr ParentExpr =
            Property(Container.ResolverContextParamExpr, ParentProperty);

        /// <summary>Resolver parameter expression in FactoryDelegate.</summary>
        public static readonly Expr RootOrSelfExpr =
            Call(typeof(ResolverContext).Method(nameof(RootOrSelf)), Container.ResolverContextParamExpr);

        /// <summary>Resolver parameter expression in FactoryDelegate.</summary>
        public static readonly Expr SingletonScopeExpr =
            Property(Container.ResolverContextParamExpr,
                typeof(IResolverContext).Property(nameof(IResolverContext.SingletonScope)));

        /// <summary>Access to scopes in FactoryDelegate.</summary>
        public static readonly Expr CurrentScopeExpr =
            Property(Container.ResolverContextParamExpr,
                typeof(IResolverContext).Property(nameof(IResolverContext.CurrentScope)));

        /// <summary>Provides access to the current scope.</summary>
        public static IScope GetCurrentScope(this IResolverContext r, bool throwIfNotFound) =>
            r.CurrentScope ?? (throwIfNotFound ? Throw.For<IScope>(Error.NoCurrentScope) : null);

        /// <summary>Gets current scope matching the <paramref name="name"/></summary>
        public static IScope GetNamedScope(this IResolverContext r, object name, bool throwIfNotFound)
        {
            var scope = r.CurrentScope;
            if (scope == null)
                return throwIfNotFound ? Throw.For<IScope>(Error.NoCurrentScope) : null;

            if (name == null)
                return scope;

            var scopeName = name as IScopeName;
            if (scopeName != null)
            {
                for (; scope != null; scope = scope.Parent)
                    if (scopeName.Match(scope.Name))
                        return scope;
            }
            else
            {
                for (; scope != null; scope = scope.Parent)
                    if (ReferenceEquals(name, scope.Name) || name.Equals(scope.Name))
                        return scope;
            }

            return throwIfNotFound ? Throw.For<IScope>(Error.NoMatchedScopeFound, scope, name) : null;
        }
    }

    /// <summary>The result generated delegate used for service creation.</summary>
    public delegate object FactoryDelegate(IResolverContext r);

    /// <summary>Adds to Container support for:
    /// <list type="bullet">
    /// <item>Open-generic services</item>
    /// <item>Service generics wrappers and arrays using <see cref="Rules.UnknownServiceResolvers"/> extension point.
    /// Supported wrappers include: Func of <see cref="FuncTypes"/>, Lazy, Many, IEnumerable, arrays, Meta, KeyValuePair, DebugExpression.
    /// All wrapper factories are added into collection of <see cref="Wrappers"/>.
    /// unregistered resolution rule.</item>
    /// </list></summary>
    public static class WrappersSupport
    {
        /// <summary>Supported Func types.</summary>
        public static readonly Type[] FuncTypes =
        {
            typeof(Func<>), typeof(Func<,>), typeof(Func<,,>), typeof(Func<,,,>), typeof(Func<,,,,>),
            typeof(Func<,,,,,>), typeof(Func<,,,,,,>), typeof(Func<,,,,,,,>)
        };

        /// <summary>Supported Action types. Yeah, action I can resolve or inject void returning method as action.</summary>
        public static readonly Type[] ActionTypes =
        {
            typeof(Action), typeof(Action<>), typeof(Action<,>), typeof(Action<,,>), typeof(Action<,,,>),
            typeof(Action<,,,,>), typeof(Action<,,,,,>), typeof(Action<,,,,,,>)
        };

        /// <summary>Supported open-generic collection types.</summary>
        public static readonly Type[] ArrayInterfaces =
            typeof(object[]).GetImplementedInterfaces()
                .Match(t => t.IsGeneric(), t => t.GetGenericTypeDefinition());

        /// <summary>Returns true if type is supported <see cref="FuncTypes"/>, and false otherwise.</summary>
        public static bool IsFunc(this Type type)
        {
            var genericDefinition = type.GetGenericDefinitionOrNull();
            return genericDefinition != null && FuncTypes.IndexOf(genericDefinition) != -1;
        }

        /// <summary>Registered wrappers by their concrete or generic definition service type.</summary>
        public static readonly ImHashMap<Type, Factory> Wrappers = BuildSupportedWrappers();

        private static ImHashMap<Type, Factory> BuildSupportedWrappers()
        {
            var wrappers = ImHashMap<Type, Factory>.Empty;

            var arrayExpr = new ExpressionFactory(GetArrayExpression, setup: Setup.Wrapper);

            var arrayInterfaces = ArrayInterfaces;
            for (var i = 0; i < arrayInterfaces.Length; i++)
                wrappers = wrappers.AddOrUpdate(arrayInterfaces[i], arrayExpr);

            wrappers = wrappers.AddOrUpdate(typeof(LazyEnumerable<>),
                new ExpressionFactory(GetLazyEnumerableExpressionOrDefault, setup: Setup.Wrapper));

            wrappers = wrappers.AddOrUpdate(typeof(Lazy<>),
                new ExpressionFactory(r => GetLazyExpressionOrDefault(r), setup: Setup.Wrapper));

            wrappers = wrappers.AddOrUpdate(typeof(KeyValuePair<,>),
                new ExpressionFactory(GetKeyValuePairExpressionOrDefault, setup: Setup.WrapperWith(1)));

            wrappers = wrappers.AddOrUpdate(typeof(Meta<,>),
                new ExpressionFactory(GetMetaExpressionOrDefault, setup: Setup.WrapperWith(0)));

            wrappers = wrappers.AddOrUpdate(typeof(Tuple<,>),
                new ExpressionFactory(GetMetaExpressionOrDefault, setup: Setup.WrapperWith(0)));

            wrappers = wrappers.AddOrUpdate(typeof(LambdaExpression),
                new ExpressionFactory(GetLambdaExpressionExpressionOrDefault, setup: Setup.Wrapper));

            wrappers = wrappers.AddOrUpdate(typeof(Func<>),
                new ExpressionFactory(GetFuncOrActionExpressionOrDefault, setup: Setup.Wrapper));

            for (var i = 0; i < FuncTypes.Length; i++)
                wrappers = wrappers.AddOrUpdate(FuncTypes[i],
                    new ExpressionFactory(GetFuncOrActionExpressionOrDefault, setup: Setup.WrapperWith(i)));

            for (var i = 0; i < ActionTypes.Length; i++)
                wrappers = wrappers.AddOrUpdate(ActionTypes[i],
                    new ExpressionFactory(GetFuncOrActionExpressionOrDefault,
                    setup: Setup.WrapperWith(unwrap: _ => typeof(void))));

            wrappers = wrappers.AddContainerInterfaces();
            return wrappers;
        }

        private static ImHashMap<Type, Factory> AddContainerInterfaces(this ImHashMap<Type, Factory> wrappers)
        {
            var asContainerWrapper = Setup.WrapperWith(preventDisposal: true);

            wrappers = wrappers.AddOrUpdate(typeof(IResolver),
                new ExpressionFactory(ResolverContext.GetRootOrSelfExpr,
                Reuse.Transient,
                setup: asContainerWrapper));

            var containerFactory = new ExpressionFactory(r =>
                Convert(ResolverContext.GetRootOrSelfExpr(r), r.ServiceType),
                Reuse.Transient,
                setup: asContainerWrapper);

            wrappers = wrappers
                .AddOrUpdate(typeof(IContainer), containerFactory)
                .AddOrUpdate(typeof(IRegistrator), containerFactory)
                .AddOrUpdate(typeof(IResolverContext), containerFactory);

            return wrappers;
        }

        private static Expr GetArrayExpression(Request request)
        {
            var collectionType = request.GetActualServiceType();
            var container = request.Container;
            var rules = container.Rules;

            var itemType = collectionType.GetArrayElementTypeOrNull() ?? collectionType.GetGenericParamsAndArgs()[0];

            if (rules.ResolveIEnumerableAsLazyEnumerable)
            {
                var lazyEnumerableExpr = GetLazyEnumerableExpressionOrDefault(request);
                if (collectionType.GetGenericDefinitionOrNull() != typeof(IEnumerable<>))
                {
                    var toArrayMethod = typeof(Enumerable).Method(nameof(Enumerable.ToArray)); // todo: move to lazy field
                    return Call(toArrayMethod.MakeGenericMethod(itemType), lazyEnumerableExpr);
                }
                return lazyEnumerableExpr;
            }

            var requiredItemType = container.GetWrappedType(itemType, request.RequiredServiceType);

            var items = container.GetAllServiceFactories(requiredItemType)
                .Map(kv => new ServiceRegistrationInfo(kv.Value, requiredItemType, kv.Key))
                .ToArrayOrSelf();

            if (requiredItemType.IsClosedGeneric())
            {
                var requiredItemOpenGenericType = requiredItemType.GetGenericDefinitionOrNull();
                var openGenericItems = container.GetAllServiceFactories(requiredItemOpenGenericType)
                    .Map(f => new ServiceRegistrationInfo(f.Value, requiredItemType,
                            // note: Special service key with info about open-generic factory service type
                            optionalServiceKey: new[] { requiredItemOpenGenericType, f.Key }))
                    .ToArrayOrSelf();
                items = items.Append(openGenericItems);
            }

            // Append registered generic types with compatible variance,
            // e.g. for IHandler<in E> - IHandler<A> is compatible with IHandler<B> if B : A.
            if (requiredItemType.IsGeneric() &&
                rules.VariantGenericTypesInResolvedCollection)
            {
                var variantGenericItems = container.GetServiceRegistrations()
                    .Match(it =>
                        it.ServiceType.IsGeneric() &&
                        it.ServiceType.GetGenericTypeDefinition() == requiredItemType.GetGenericTypeDefinition() &&
                        it.ServiceType != requiredItemType &&
                        it.ServiceType.IsAssignableTo(requiredItemType))
                    .ToArrayOrSelf();
                items = items.Append(variantGenericItems);
            }

            // Composite pattern support: filter out composite parent service skip wrappers and decorators
            var parent = request.Parent;
            if (parent.FactoryType != FactoryType.Service)
                parent = parent.FirstOrDefault(p => p.FactoryType == FactoryType.Service) ?? RequestInfo.Empty;

            if (!parent.IsEmpty && parent.GetActualServiceType() == requiredItemType) // check fast for the parent of the same type
                items = items.Match(x => x.Factory.FactoryID != parent.FactoryID &&
                    (x.Factory.FactoryGenerator == null || !x.Factory.FactoryGenerator.GeneratedFactories.Enumerate().Any(f =>
                        f.Value.FactoryID == parent.FactoryID &&
                        f.Key.Key == parent.ServiceType && f.Key.Value == parent.ServiceKey)));

            // Return collection of single matched item if key is specified.
            var serviceKey = request.ServiceKey;
            if (serviceKey != null)
                items = items.Match(it => serviceKey.Equals(it.OptionalServiceKey));

            var metadataKey = request.MetadataKey;
            var metadata = request.Metadata;
            if (metadataKey != null || metadata != null)
                items = items.Match(it => it.Factory.Setup.MatchesMetadata(metadataKey, metadata));

            var itemExprs = ArrayTools.Empty<Expr>();
            if (!items.IsNullOrEmpty())
            {
                Array.Sort(items); // to resolve the items in order of registration

                for (var i = 0; i < items.Length; i++)
                {
                    var item = items[i];
                    var itemRequest = request.Push(itemType, item.OptionalServiceKey,
                        IfUnresolved.ReturnDefaultIfNotRegistered, requiredServiceType: item.ServiceType);

                    var itemFactory = container.ResolveFactory(itemRequest);
                    if (itemFactory != null)
                    {
                        var itemExpr = itemFactory.GetExpressionOrDefault(itemRequest);
                        if (itemExpr != null)
                            itemExprs = itemExprs.AppendOrUpdate(itemExpr);
                    }
                }
            }

            return NewArrayInit(itemType, itemExprs);
        }

        private static Expr GetLazyEnumerableExpressionOrDefault(Request request)
        {
            var container = request.Container;
            var collectionType = request.ServiceType;
            var itemType = collectionType.GetArrayElementTypeOrNull() ?? collectionType.GetGenericParamsAndArgs()[0];
            var requiredItemType = container.GetWrappedType(itemType, request.RequiredServiceType);

            var resolverExpr = ResolverContext.GetRootOrSelfExpr(request);
            var preResolveParentExpr = container.RequestInfoToExpression(request.RequestInfo);

            var callResolveManyExpr = Call(resolverExpr, Resolver.ResolveManyMethod,
                Constant(itemType),
                container.GetItemExpression(request.ServiceKey),
                Constant(requiredItemType),
                preResolveParentExpr,
                request.GetInputArgsExpr());

            // cast to object is not required cause Resolve already return IEnumerable<object>
            if (itemType != typeof(object))
            {
                var castMethod = typeof(Enumerable).Method(nameof(Enumerable.Cast));
                callResolveManyExpr = Call(castMethod.MakeGenericMethod(itemType), callResolveManyExpr);
            }

            return New(typeof(LazyEnumerable<>).MakeGenericType(itemType).Constructor(), callResolveManyExpr);
        }

        /// <summary>Gets the expression for <see cref="Lazy{T}"/> wrapper.</summary>
        /// <param name="request">The resolution request.</param>
        /// <param name="nullWrapperForUnresolvedService">if set to <c>true</c> then check for service registration before creating resolution expression.</param>
        /// <returns>Expression: r => new Lazy{TService}(() => r.Resolve{TService}(key, ifUnresolved, requiredType));</returns>
        public static Expr GetLazyExpressionOrDefault(Request request, bool nullWrapperForUnresolvedService = false)
        {
            var lazyType = request.GetActualServiceType();
            var serviceType = lazyType.GetGenericParamsAndArgs()[0];
            var serviceRequest = request.Push(serviceType);

            var serviceFactory = request.Container.ResolveFactory(serviceRequest);
            if (serviceFactory == null)
                return request.IfUnresolved == IfUnresolved.Throw
                    ? null
                    : Constant(null, lazyType);

            serviceRequest = serviceRequest.WithResolvedFactory(serviceFactory,
                skipRecursiveDependencyCheck: true);

            // creates: r => new Lazy(() => r.Resolve<X>(key))
            // or for singleton : r => new Lazy(() => r.Root.Resolve<X>(key))
            var serviceExpr = Resolver.CreateResolutionExpression(serviceRequest);

            // The conversion is required in .NET 3.5 to handle lack of covariance for Func<out T>
            // So that Func<Derived> may be used for Func<Base>
            if (serviceExpr.Type != serviceType)
                serviceExpr = Convert(serviceExpr, serviceType);

            var factoryExpr = Lambda(serviceExpr, ArrayTools.Empty<ParamExpr>());
            var serviceFuncType = typeof(Func<>).MakeGenericType(serviceType);
            var wrapperCtor = lazyType.GetConstructorOrNull(args: serviceFuncType);
            return New(wrapperCtor, factoryExpr);
        }

        private static Expr GetFuncOrActionExpressionOrDefault(Request request)
        {
            var wrapperType = request.GetActualServiceType();
            var isAction = wrapperType == typeof(Action);
            if (!isAction)
            {
                var openGenericWrapperType = wrapperType.GetGenericDefinitionOrNull().ThrowIfNull();
                var funcIndex = FuncTypes.IndexOf(openGenericWrapperType);
                if (funcIndex == -1)
                {
                    isAction = ActionTypes.IndexOf(openGenericWrapperType) != -1;
                    Throw.If(!isAction);
                }
            }

            var argTypes = wrapperType.GetGenericParamsAndArgs();
            var argCount = isAction ? argTypes.Length : argTypes.Length - 1;
            var serviceType = isAction ? typeof(void) : argTypes[argCount];

            var flags = RequestFlags.IsWrappedInFunc;

            var argExprs = new ParamExpr[argCount]; // may be empty, that's OK
            if (argCount != 0)
            {
                for (var i = 0; i < argCount; ++i)
                {
                    var argType = argTypes[i];
                    var argName = "_" + argType.Name + i; // valid unique argument names for code generation
                    argExprs[i] = Parameter(argType, argName);
                }

                request = request.WithInputArgs(argExprs);
            }

            var serviceRequest = request.Push(serviceType, flags: flags);
            var serviceFactory = request.Container.ResolveFactory(serviceRequest);
            if (serviceFactory == null)
                return null;

            var serviceExpr = serviceFactory.GetExpressionOrDefault(serviceRequest);
            if (serviceExpr == null)
                return null;

            // The conversion is required in .NET 3.5 to handle lack of covariance for Func<out T>
            // So that Func<Derived> may be used for Func<Base>
            if (!isAction && serviceExpr.Type != serviceType)
                serviceExpr = Convert(serviceExpr, serviceType);

            return Lambda(wrapperType, serviceExpr, argExprs);
        }

        private static Expr GetLambdaExpressionExpressionOrDefault(Request request)
        {
            var serviceType = request.RequiredServiceType
                .ThrowIfNull(Error.ResolutionNeedsRequiredServiceType, request);
            request = request.Push(serviceType);
            var expr = request.Container.ResolveFactory(request)?.GetExpressionOrDefault(request);
            if (expr == null)
                return null;
            return Constant(
                Container.WrapInFactoryExpression(expr)
#if FEC_EXPRESSION_INFO
                .ToLambdaExpression()
#endif
                , typeof(LambdaExpression));
        }

        private static Expr GetKeyValuePairExpressionOrDefault(Request request)
        {
            var keyValueType = request.GetActualServiceType();
            var typeArgs = keyValueType.GetGenericParamsAndArgs();
            var serviceKeyType = typeArgs[0];
            var serviceKey = request.ServiceKey;
            if (serviceKey == null && serviceKeyType.IsValueType() ||
                serviceKey != null && !serviceKeyType.IsTypeOf(serviceKey))
                return null;

            var serviceType = typeArgs[1];
            var serviceRequest = request.Push(serviceType, serviceKey);
            var serviceFactory = request.Container.ResolveFactory(serviceRequest);
            var serviceExpr = serviceFactory == null ? null : serviceFactory.GetExpressionOrDefault(serviceRequest);
            if (serviceExpr == null)
                return null;

            var keyExpr = request.Container.GetItemExpression(serviceKey, serviceKeyType);
            var pairExpr = New(keyValueType.Constructor(), keyExpr, serviceExpr);
            return pairExpr;
        }

        /// <summary>Discovers and combines service with its setup metadata.
        /// Works with any generic type with first Type arg - Service type and second Type arg - Metadata type,
        /// and constructor with Service and Metadata arguments respectively.
        /// - if service key is not specified in request then method will search for all
        /// registered factories with the same metadata type ignoring keys.
        /// - if metadata is IDictionary{string, object},
        ///  then the First value matching the TMetadata type will be returned.</summary>
        /// <param name="request">Requested service.</param>
        /// <returns>Wrapper creation expression.</returns>
        public static Expr GetMetaExpressionOrDefault(Request request)
        {
            var metaType = request.GetActualServiceType();
            var typeArgs = metaType.GetGenericParamsAndArgs();

            var metaCtor = metaType.GetConstructorOrNull(args: typeArgs)
                .ThrowIfNull(Error.NotFoundMetaCtorWithTwoArgs, typeArgs, request);

            var metadataType = typeArgs[1];
            var serviceType = typeArgs[0];

            var container = request.Container;
            var requiredServiceType = container.GetWrappedType(serviceType, request.RequiredServiceType);

            var factories = container
                .GetAllServiceFactories(requiredServiceType, bothClosedAndOpenGenerics: true)
                .ToArrayOrSelf();

            if (factories.Length == 0)
                return null;

            var serviceKey = request.ServiceKey;
            if (serviceKey != null)
            {
                factories = factories.Match(f => serviceKey.Equals(f.Key));
                if (factories.Length == 0)
                    return null;
            }

            // if the service keys for some reason are not unique
            factories = factories
                .Match(f =>
                {
                    var metadata = f.Value.Setup.Metadata;
                    if (metadata == null)
                        return false;

                    if (metadataType == typeof(object))
                        return true;

                    var metadataDict = metadata as IDictionary<string, object>;
                    if (metadataDict != null)
                        return metadataType == typeof(IDictionary<string, object>)
                            || metadataDict.Values.Any(m => metadataType.IsTypeOf(m));

                    return metadataType.IsTypeOf(metadata);
                });

            if (factories.Length == 0)
                return null;

            // Prevent non-determinism when more than 1 factory is matching the metadata
            if (factories.Length > 1)
            {
                if (request.IfUnresolved == IfUnresolved.Throw)
                    Throw.It(Error.UnableToSelectFromManyRegistrationsWithMatchingMetadata, metadataType, factories, request);
                return null;
            }

            var factory = factories[0];
            if (factory == null)
                return null;

            serviceKey = factory.Key;

            var serviceRequest = request.Push(serviceType, serviceKey);
            var serviceFactory = container.ResolveFactory(serviceRequest);
            if (serviceFactory == null)
                return null;

            var serviceExpr = serviceFactory.GetExpressionOrDefault(serviceRequest);
            if (serviceExpr == null)
                return null;

            var resultMetadata = factory.Value.Setup.Metadata;
            if (metadataType != typeof(object))
            {
                var resultMetadataDict = resultMetadata as IDictionary<string, object>;
                if (resultMetadataDict != null && metadataType != typeof(IDictionary<string, object>))
                    resultMetadata = resultMetadataDict.Values.FirstOrDefault(m => metadataType.IsTypeOf(m));
            }

            var metadataExpr = request.Container.GetItemExpression(resultMetadata, metadataType);
            return New(metaCtor, serviceExpr, metadataExpr);
        }
    }

    /// <summary>Represents info required for dynamic registration: service key, factory,
    /// and <see cref="IfAlreadyRegistered"/> option how to combine dynamic with normal registrations.</summary>
    public sealed class DynamicRegistration
    {
        /// <summary>Factory</summary>
        public readonly Factory Factory;

        /// <summary>Optional: will be <see cref="DryIoc.IfAlreadyRegistered.AppendNotKeyed"/> by default.</summary>
        public readonly IfAlreadyRegistered IfAlreadyRegistered;

        /// <summary>Optional service key: if null the default <see cref="DefaultDynamicKey"/> will be used. </summary>
        public readonly object ServiceKey;

        /// <summary>Constructs the info</summary>
        /// <param name="factory"></param>
        /// <param name="ifAlreadyRegistered">(optional) Defines how to combine with normal registrations.
        /// Will use <see cref="DryIoc.IfAlreadyRegistered.AppendNotKeyed"/> by default.</param>
        /// <param name="serviceKey">(optional) Service key.</param>
        public DynamicRegistration(Factory factory,
            IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.AppendNotKeyed, object serviceKey = null)
        {
            Factory = factory.ThrowIfNull();
            ServiceKey = serviceKey;
            IfAlreadyRegistered = ifAlreadyRegistered;
        }
    }

    /// <summary> Defines resolution/registration rules associated with Container instance. They may be different for different containers.</summary>
    public sealed class Rules
    {
        /// <summary>Default rules as staring point.</summary>
        public static readonly Rules Default = new Rules();

        /// <summary>Default value for <see cref="MaxObjectGraphSize"/></summary>
        public const int DefaultMaxObjectGraphSize = 32;

        /// <summary>Max number of dependencies including nested ones,
        /// before splitting the graph with Resolve calls.</summary>
        public int MaxObjectGraphSize { get; private set; }

        /// <summary>Sets <see cref="MaxObjectGraphSize"/>. Everything low than 1 will be the 1.
        /// To disable the limit please use <see cref="WithoutMaxObjectGraphSize"/></summary>
        public Rules WithMaxObjectGraphSize(int size) =>
            new Rules(_settings, FactorySelector, DefaultReuse,
                _made, DefaultIfAlreadyRegistered, size < 1 ? 1 : size,
                DependencyResolutionCallExpressions, ItemToExpressionConverter,
                DynamicRegistrationProviders, UnknownServiceResolvers);

        /// <summary>Disables the <see cref="MaxObjectGraphSize"/> limitation,
        /// so that object graph won't be split due this setting.</summary>
        public Rules WithoutMaxObjectGraphSize() =>
            new Rules(_settings, FactorySelector, DefaultReuse,
                _made, DefaultIfAlreadyRegistered, -1,
                DependencyResolutionCallExpressions, ItemToExpressionConverter,
                DynamicRegistrationProviders, UnknownServiceResolvers);

        /// <summary>Shorthand to <see cref="Made.FactoryMethod"/></summary>
        public FactoryMethodSelector FactoryMethod => _made.FactoryMethod;

        /// <summary>Shorthand to <see cref="Made.Parameters"/></summary>
        public ParameterSelector Parameters => _made.Parameters;

        /// <summary>Shorthand to <see cref="Made.PropertiesAndFields"/></summary>
        public PropertiesAndFieldsSelector PropertiesAndFields => _made.PropertiesAndFields;

        /// <summary>Instructs to override per-registration made settings with these rules settings.</summary>
        public bool OverrideRegistrationMade =>
            (_settings & Settings.OverrideRegistrationMade) != 0;

        /// <summary>Returns new instance of the rules new Made composed out of
        /// provided factory method, parameters, propertiesAndFields.</summary>
        public Rules With(
            FactoryMethodSelector factoryMethod = null,
            ParameterSelector parameters = null,
            PropertiesAndFieldsSelector propertiesAndFields = null) =>
            With(Made.Of(factoryMethod, parameters, propertiesAndFields));

        /// <summary>Returns new instance of the rules with specified <see cref="Made"/>.</summary>
        /// <param name="made">New Made.Of rules.</param>
        /// <param name="overrideRegistrationMade">Instructs to override registration level Made.Of</param>
        /// <returns>New rules.</returns>
        public Rules With(Made made, bool overrideRegistrationMade = false) =>
            new Rules(
                _settings | (overrideRegistrationMade ? Settings.OverrideRegistrationMade : 0),
                FactorySelector, DefaultReuse,
                _made == Made.Default
                    ? made
                    : Made.Of(
                        made.FactoryMethod ?? _made.FactoryMethod,
                        made.Parameters ?? _made.Parameters,
                        made.PropertiesAndFields ?? _made.PropertiesAndFields),
                DefaultIfAlreadyRegistered, DefaultMaxObjectGraphSize,
                DependencyResolutionCallExpressions, ItemToExpressionConverter,
                DynamicRegistrationProviders, UnknownServiceResolvers);

        /// <summary>Defines single factory selector delegate.</summary>
        /// <param name="request">Provides service request leading to factory selection.</param>
        /// <param name="factories">Registered factories with corresponding key to select from.</param>
        /// <returns>Single selected factory, or null if unable to select.</returns>
        public delegate Factory FactorySelectorRule(Request request, KeyValuePair<object, Factory>[] factories);

        /// <summary>Rules to select single matched factory default and keyed registered factory/factories.
        /// Selectors applied in specified array order, until first returns not null <see cref="Factory"/>.
        /// Default behavior is throw on multiple registered default factories, cause it is not obvious what to use.</summary>
        public FactorySelectorRule FactorySelector { get; private set; }

        /// <summary>Sets <see cref="FactorySelector"/></summary>
        public Rules WithFactorySelector(FactorySelectorRule rule) =>
            new Rules(_settings, rule, DefaultReuse,
                _made, DefaultIfAlreadyRegistered, DefaultMaxObjectGraphSize,
                DependencyResolutionCallExpressions, ItemToExpressionConverter,
                DynamicRegistrationProviders, UnknownServiceResolvers);

        /// <summary>Select last registered factory from multiple default.</summary>
        public static FactorySelectorRule SelectLastRegisteredFactory() => GetLastFactoryByKey;

        private static Factory GetLastFactoryByKey(Request request, KeyValuePair<object, Factory>[] factories)
        {
            var serviceKey = request.ServiceKey;
            for (var i = factories.Length - 1; i >= 0; i--)
            {
                var factory = factories[i];
                if (factory.Key.Equals(serviceKey))
                    return factory.Value;
            }
            return null;
        }

        //we are watching you...public static
        /// <summary>Prefer specified service key (if found) over default key.
        /// Help to override default registrations in Open Scope scenarios:
        /// I may register service with key and resolve it as default in current scope.</summary>
        public static FactorySelectorRule SelectKeyedOverDefaultFactory(object serviceKey) =>
            (req, factories) =>
                factories.FindFirst(f => f.Key.Equals(serviceKey)).Value ??
                factories.FindFirst(f => f.Key.Equals(null)).Value;

        /// <summary>Specify the method signature for returning multiple keyed factories.
        /// This is dynamic analog to the normal Container Registry.</summary>
        /// <param name="serviceType">Requested service type.</param>
        /// <param name="serviceKey">(optional) If <c>null</c> will request all factories of <paramref name="serviceType"/></param>
        /// <returns>Key-Factory pairs.</returns>
        public delegate IEnumerable<DynamicRegistration> DynamicRegistrationProvider(Type serviceType, object serviceKey);

        /// <summary>Providers for resolving multiple not-registered services. Null by default.</summary>
        public DynamicRegistrationProvider[] DynamicRegistrationProviders { get; private set; }

        /// <summary>Appends dynamic registration rules.</summary>
        /// <param name="rules">Rules to append.</param> <returns>New Rules.</returns>
        public Rules WithDynamicRegistrations(params DynamicRegistrationProvider[] rules) =>
            // todo: Should I use Settings.UseDynamicRegistrationsAsFallback, 5 tests are failing only 
            new Rules(_settings, FactorySelector, DefaultReuse,
                _made, DefaultIfAlreadyRegistered, DefaultMaxObjectGraphSize,
                DependencyResolutionCallExpressions, ItemToExpressionConverter,
                DynamicRegistrationProviders.Append(rules), UnknownServiceResolvers);

        /// <summary>Appends dynamic registration rules 
        /// And additionally specifies to use dynamic registrations only when no normal registrations found!</summary>
        /// <param name="rules">Rules to append.</param> <returns>New Rules.</returns>
        public Rules WithDynamicRegistrationsAsFallback(params DynamicRegistrationProvider[] rules) =>
            new Rules(_settings | Settings.UseDynamicRegistrationsAsFallback, FactorySelector, DefaultReuse,
                _made, DefaultIfAlreadyRegistered, DefaultMaxObjectGraphSize,
                DependencyResolutionCallExpressions, ItemToExpressionConverter,
                DynamicRegistrationProviders.Append(rules), UnknownServiceResolvers);

        /// <summary>Specifies to use dynamic registrations only when no normal registrations found</summary>
        public bool UseDynamicRegistrationsAsFallback =>
            (_settings & Settings.UseDynamicRegistrationsAsFallback) != 0;

        /// <summary>Defines delegate to return factory for request not resolved by registered factories or prior rules.
        /// Applied in specified array order until return not null <see cref="Factory"/>.</summary>
        public delegate Factory UnknownServiceResolver(Request request);

        /// <summary>Gets rules for resolving not-registered services. Null by default.</summary>
        public UnknownServiceResolver[] UnknownServiceResolvers { get; private set; }

        /// <summary>Appends resolver to current unknown service resolvers.</summary>
        public Rules WithUnknownServiceResolvers(params UnknownServiceResolver[] rules) =>
            new Rules(_settings, FactorySelector, DefaultReuse,
                _made, DefaultIfAlreadyRegistered, DefaultMaxObjectGraphSize,
                DependencyResolutionCallExpressions, ItemToExpressionConverter,
                DynamicRegistrationProviders, UnknownServiceResolvers.Append(rules));

        /// <summary>Removes specified resolver from unknown service resolvers, and returns new Rules.
        /// If no resolver was found then <see cref="UnknownServiceResolvers"/> will stay the same instance,
        /// so it could be check for remove success or fail.</summary>
        public Rules WithoutUnknownServiceResolver(UnknownServiceResolver rule) =>
            new Rules(_settings, FactorySelector, DefaultReuse,
                _made, DefaultIfAlreadyRegistered, DefaultMaxObjectGraphSize,
                DependencyResolutionCallExpressions, ItemToExpressionConverter,
                DynamicRegistrationProviders, UnknownServiceResolvers.Remove(rule));

        /// <summary>Sugar on top of <see cref="WithUnknownServiceResolvers"/> to simplify setting the diagnostic action.
        /// Does not guard you from action throwing an exception. Actually can be used to throw your custom exception
        /// instead of <see cref="ContainerException"/>.</summary>
        public Rules WithUnknownServiceHandler(Action<Request> handler) =>
            WithUnknownServiceResolvers(request =>
            {
                handler(request);
                return null;
            });

        /// <summary>Obsolete: Replaced by ConcreteTypeDynamicRegistrations</summary>
        [Obsolete("Replaced by " + nameof(ConcreteTypeDynamicRegistrations), false)]
        public static UnknownServiceResolver AutoResolveConcreteTypeRule(Func<Request, bool> condition = null) =>
            request =>
            {
                var concreteServiceType = request.GetActualServiceType();
                if (concreteServiceType.IsAbstract() || condition != null && !condition(request))
                    return null;

                var openGenericServiceType = concreteServiceType.GetGenericDefinitionOrNull();
                if (openGenericServiceType != null &&
                    WrappersSupport.Wrappers.GetValueOrDefault(openGenericServiceType) != null)
                    return null;

                var factory = new ReflectionFactory(concreteServiceType,
                    made: DryIoc.FactoryMethod.ConstructorWithResolvableArgumentsIncludingNonPublic);

                // try resolve expression first and return null,
                // to enable fallback to other rules if unresolved
                var requestOrDefault = request
                    .WithChangedServiceInfo(_ => _.WithIfUnresolved(IfUnresolved.ReturnDefault));

                var factoryExpr = factory.GetExpressionOrDefault(requestOrDefault);
                if (factoryExpr == null)
                    return null;

                return factory;
            };

        /// <summary>Rule to automatically resolves non-registered service type which is: nor interface, nor abstract.
        /// For constructor selection we are using <see cref="DryIoc.FactoryMethod.ConstructorWithResolvableArguments"/>.
        /// The resolution creates transient services.</summary>
        /// <param name="condition">(optional) Condition for requested service type and key.</param>
        /// <param name="reuse">(optional) Reuse for concrete types.</param>
        /// <returns>New rule.</returns>
        public static DynamicRegistrationProvider ConcreteTypeDynamicRegistrations(
            Func<Type, object, bool> condition = null, IReuse reuse = null)
        {
            return AutoFallbackDynamicRegistrations((serviceType, serviceKey) =>
            {
                if (serviceType.IsAbstract() ||
                    serviceType.IsOpenGeneric() || // service type in principle should be concrete, so should not be open-generic
                    condition != null && !condition(serviceType, serviceKey))
                    return null;

                // exclude concrete service types which are pre-defined DryIoc wrapper types
                var openGenericServiceType = serviceType.GetGenericDefinitionOrNull();
                if (openGenericServiceType != null &&
                    WrappersSupport.Wrappers.GetValueOrDefault(openGenericServiceType) != null)
                    return null;

                return new[] { serviceType }; // use concrete service type as implementation type
            },
            implType =>
            {
                ReflectionFactory factory = null;

                factory = new ReflectionFactory(implType, reuse,
                    made: DryIoc.FactoryMethod.ConstructorWithResolvableArgumentsIncludingNonPublic,

                    // condition checks that factory is resolvable
                    setup: Setup.With(condition: req =>
                        null != factory.GetExpressionOrDefault(
                            req.WithChangedServiceInfo(r => r.WithIfUnresolved(IfUnresolved.ReturnDefault)))));

                return factory;
            });
        }

        /// <summary>Automatically resolves non-registered service type which is: nor interface, nor abstract.
        /// The resolution creates Transient services.</summary>
        /// <param name="condition">(optional) Condition for requested service type and key.</param>
        /// <param name="reuse">(optional) Reuse.</param>
        /// <returns>New rules.</returns>
        public Rules WithConcreteTypeDynamicRegistrations(
            Func<Type, object, bool> condition = null, IReuse reuse = null) =>
            WithDynamicRegistrationsAsFallback(ConcreteTypeDynamicRegistrations(condition, reuse));

        /// <summary>Replaced with WithConcreteTypeDynamicRegistrations</summary>
        [Obsolete("Replaced with WithConcreteTypeDynamicRegistrations", false)]
        public Rules WithAutoConcreteTypeResolution(Func<Request, bool> condition = null) =>
            WithUnknownServiceResolvers(AutoResolveConcreteTypeRule(condition));

        /// <summary>Creates dynamic fallback registrations for the requested service type
        /// with provided <paramref name="getImplementationTypes"/>.
        /// Fallback means that the dynamic registrations will be applied Only if no normal registrations
        /// exist for the requested service type, hence the "fallback".</summary>
        /// <param name="getImplementationTypes">Implementation types to select for service.</param>
        /// <param name="factory">(optional) Handler to customize the factory, e.g.
        /// specify reuse or setup. Handler should not return <c>null</c>.</param>
        /// <returns>Registration provider.</returns>
        public static DynamicRegistrationProvider AutoFallbackDynamicRegistrations(
            Func<Type, object, IEnumerable<Type>> getImplementationTypes,
            Func<Type, Factory> factory = null)
        {
            // cache factory for implementation type to enable reuse semantics
            var factories = Ref.Of(ImHashMap<Type, Factory>.Empty);

            return (serviceType, serviceKey) =>
            {
                var implementationTypes = getImplementationTypes(serviceType, serviceKey);

                return implementationTypes.Match(
                    implType => implType.ImplementsServiceType(serviceType),
                    implType =>
                    {
                        var implFactory = factories.Value.GetValueOrDefault(implType);
                        if (implFactory == null)
                        {
                            factories.Swap(existingFactories =>
                            {
                                implFactory = existingFactories.GetValueOrDefault(implType);
                                if (implFactory != null)
                                    return existingFactories;

                                implFactory = factory != null
                                    ? factory(implType).ThrowIfNull()
                                    : new ReflectionFactory(implType);

                                return existingFactories.AddOrUpdate(implType, implFactory);
                            });
                        }

                        return new DynamicRegistration(implFactory, IfAlreadyRegistered.Keep);
                    });
            };
        }

        /// <summary>Obsolete: replaced by <see cref="AutoFallbackDynamicRegistrations"/></summary>
        [Obsolete("Replaced by AutoFallbackDynamicRegistrations", false)]
        public static UnknownServiceResolver AutoRegisterUnknownServiceRule(IEnumerable<Type> implTypes,
            Func<IReuse, Request, IReuse> changeDefaultReuse = null, Func<Request, bool> condition = null) =>
            request =>
            {
                if (condition != null && !condition(request))
                    return null;

                var scope = request.Container.CurrentScope;
                var reuse = scope != null ? Reuse.ScopedTo(scope.Name) : Reuse.Singleton;

                if (changeDefaultReuse != null)
                    reuse = changeDefaultReuse(reuse, request);

                var requestedServiceType = request.GetActualServiceType();
                request.Container.RegisterMany(implTypes, reuse,
                    serviceTypeCondition: serviceType =>
                        serviceType.IsOpenGeneric() && requestedServiceType.IsClosedGeneric()
                            ? serviceType == requestedServiceType.GetGenericTypeDefinition()
                            : serviceType == requestedServiceType);

                return request.Container.GetServiceFactoryOrDefault(request);
            };

        /// <summary>See <see cref="WithDefaultReuse"/></summary>
        public IReuse DefaultReuse { get; private set; }

        /// <summary>The reuse used in case if reuse is unspecified (null) in Register methods.</summary>
        public Rules WithDefaultReuse(IReuse reuse) =>
            new Rules(_settings, FactorySelector, reuse ?? Reuse.Transient,
                _made, DefaultIfAlreadyRegistered, DefaultMaxObjectGraphSize,
                DependencyResolutionCallExpressions, ItemToExpressionConverter,
                DynamicRegistrationProviders, UnknownServiceResolvers);

        /// <summary>Replaced by WithDefaultReuse because for some cases InsteadOfTransient does not make sense.</summary>
        [Obsolete("Replaced by WithDefaultReuse because for some cases ..InsteadOfTransient does not make sense.", error: false)]
        public Rules WithDefaultReuseInsteadOfTransient(IReuse reuse) => WithDefaultReuse(reuse);

        /// <summary>Given item object and its type should return item "pure" expression presentation,
        /// without side-effects or external dependencies.
        /// e.g. for string "blah" <code lang="cs"><![CDATA[]]>Expression.Constant("blah", typeof(string))</code>.
        /// If unable to convert should return null.</summary>
        /// <param name="item">Item object. Item is not null.</param>
        /// <param name="itemType">Item type. Item type is not null.</param>
        /// <returns>Expression or null.</returns>
        public delegate Expr ItemToExpressionConverterRule(object item, Type itemType);

        /// <summary><see cref="WithItemToExpressionConverter"/>.</summary>
        public ItemToExpressionConverterRule ItemToExpressionConverter { get; private set; }

        /// <summary>Specifies custom rule to convert non-primitive items to their expression representation.
        /// That may be required because DryIoc by default does not support non-primitive service keys and registration metadata.
        /// To enable non-primitive values support DryIoc need a way to recreate them as expression tree.</summary>
        public Rules WithItemToExpressionConverter(ItemToExpressionConverterRule itemToExpressionOrDefault) =>
            new Rules(_settings, FactorySelector, DefaultReuse,
                _made, DefaultIfAlreadyRegistered, DefaultMaxObjectGraphSize,
                DependencyResolutionCallExpressions, itemToExpressionOrDefault,
                DynamicRegistrationProviders, UnknownServiceResolvers);

        /// <summary><see cref="WithoutThrowIfDependencyHasShorterReuseLifespan"/>.</summary>
        public bool ThrowIfDependencyHasShorterReuseLifespan =>
            (_settings & Settings.ThrowIfDependencyHasShorterReuseLifespan) != 0;

        /// <summary>Turns off throwing exception when dependency has shorter reuse lifespan than its parent or ancestor.</summary>
        /// <returns>New rules with new setting value.</returns>
        public Rules WithoutThrowIfDependencyHasShorterReuseLifespan() =>
            WithSettings(_settings & ~Settings.ThrowIfDependencyHasShorterReuseLifespan);

        /// <summary><see cref="WithoutThrowOnRegisteringDisposableTransient"/></summary>
        public bool ThrowOnRegisteringDisposableTransient =>
            (_settings & Settings.ThrowOnRegisteringDisposableTransient) != 0;

        /// <summary>Turns Off the rule <see cref="ThrowOnRegisteringDisposableTransient"/>.
        /// Allows to register disposable transient but it is up to you to handle their disposal.
        /// You can use <see cref="WithTrackingDisposableTransients"/> to actually track disposable transient in
        /// container, so that disposal will be handled by container.</summary>
        public Rules WithoutThrowOnRegisteringDisposableTransient() =>
            WithSettings(_settings & ~Settings.ThrowOnRegisteringDisposableTransient);

        /// <summary><see cref="WithTrackingDisposableTransients"/></summary>
        public bool TrackingDisposableTransients =>
            (_settings & Settings.TrackingDisposableTransients) != 0;

        /// <summary>Turns tracking of disposable transients in dependency parent scope, or in current scope if service
        /// is resolved directly.
        ///
        /// If there is no open scope at the moment then resolved transient won't be tracked and it is up to you
        /// to dispose it! That's is similar situation to creating service by new - you have full control.
        ///
        /// If dependency wrapped in Func somewhere in parent chain then it also won't be tracked, because
        /// Func supposedly means multiple object creation and for container it is not clear what to do, so container
        /// delegates that to user. Func here is the similar to Owned relationship type in Autofac library.
        /// </summary>
        /// <remarks>Turning this setting On automatically turns off <see cref="ThrowOnRegisteringDisposableTransient"/>.</remarks>
        public Rules WithTrackingDisposableTransients() =>
            WithSettings((_settings | Settings.TrackingDisposableTransients)
                & ~Settings.ThrowOnRegisteringDisposableTransient);

        /// <summary><see cref="WithoutEagerCachingSingletonForFasterAccess"/>.</summary>
        public bool EagerCachingSingletonForFasterAccess =>
            (_settings & Settings.EagerCachingSingletonForFasterAccess) != 0;

        /// <summary>Turns off optimization: creating singletons during resolution of object graph.</summary>
        public Rules WithoutEagerCachingSingletonForFasterAccess() =>
            WithSettings(_settings & ~Settings.EagerCachingSingletonForFasterAccess);

        /// <summary><see cref="WithDependencyResolutionCallExpressions"/>.</summary>
        public Ref<ImHashMap<RequestInfo, Expression>> DependencyResolutionCallExpressions { get; private set; }

        /// <summary>Specifies to generate ResolutionCall dependency creation expression and stores the result 
        /// in the-per rules collection.</summary>
        public Rules WithDependencyResolutionCallExpressions() =>
            new Rules(_settings, FactorySelector, DefaultReuse,
                _made, DefaultIfAlreadyRegistered, DefaultMaxObjectGraphSize,
                Ref.Of(ImHashMap<RequestInfo, Expression>.Empty), ItemToExpressionConverter,
                DynamicRegistrationProviders, UnknownServiceResolvers);

        /// <summary><see cref="ImplicitCheckForReuseMatchingScope"/></summary>
        public bool ImplicitCheckForReuseMatchingScope =>
            (_settings & Settings.ImplicitCheckForReuseMatchingScope) != 0;

        /// <summary>Removes implicit Factory <see cref="Setup.Condition"/> for non-transient service.
        /// The Condition filters out factory without matching scope.</summary>
        public Rules WithoutImplicitCheckForReuseMatchingScope() =>
            WithSettings(_settings & ~Settings.ImplicitCheckForReuseMatchingScope);

        /// <summary><see cref="WithResolveIEnumerableAsLazyEnumerable"/>.</summary>
        public bool ResolveIEnumerableAsLazyEnumerable =>
            (_settings & Settings.ResolveIEnumerableAsLazyEnumerable) != 0;

        /// <summary>Specifies to resolve IEnumerable as LazyEnumerable.</summary>
        public Rules WithResolveIEnumerableAsLazyEnumerable() =>
            WithSettings(_settings | Settings.ResolveIEnumerableAsLazyEnumerable);

        /// <summary><see cref="WithoutVariantGenericTypesInResolvedCollection"/>.</summary>
        public bool VariantGenericTypesInResolvedCollection =>
            (_settings & Settings.VariantGenericTypesInResolvedCollection) != 0;

        /// <summary>Flag instructs to include covariant compatible types in resolved collection.</summary>
        /// <returns>Returns new rules with flag set.</returns>
        public Rules WithoutVariantGenericTypesInResolvedCollection() =>
            WithSettings(_settings & ~Settings.VariantGenericTypesInResolvedCollection);

        /// <summary><seew cref="WithDefaultIfAlreadyRegistered"/>.</summary>
        public IfAlreadyRegistered DefaultIfAlreadyRegistered { get; }

        /// <summary>Specifies default setting for container. By default is <see cref="IfAlreadyRegistered.AppendNotKeyed"/>.
        /// Example of use: specify Keep as a container default, then set AppendNonKeyed for explicit collection registrations.</summary>
        public Rules WithDefaultIfAlreadyRegistered(IfAlreadyRegistered rule) =>
            new Rules(_settings, FactorySelector, DefaultReuse,
                _made, rule, DefaultMaxObjectGraphSize,
                DependencyResolutionCallExpressions, ItemToExpressionConverter,
                DynamicRegistrationProviders, UnknownServiceResolvers);

        /// <summary><see cref="WithThrowIfRuntimeStateRequired"/>.</summary>
        public bool ThrowIfRuntimeStateRequired =>
            (_settings & Settings.ThrowIfRuntimeStateRequired) != 0;

        /// <summary>Specifies to throw an exception in attempt to resolve service which require runtime state for resolution.
        /// Runtime state may be introduced by RegisterDelegate, RegisterInstance, or registering with non-primitive service key, or metadata.</summary>
        public Rules WithThrowIfRuntimeStateRequired() =>
            WithSettings(_settings | Settings.ThrowIfRuntimeStateRequired);

        /// <summary><see cref="WithCaptureContainerDisposeStackTrace"/>.</summary>
        public bool CaptureContainerDisposeStackTrace =>
            (_settings & Settings.CaptureContainerDisposeStackTrace) != 0;

        /// <summary>Instructs to capture Dispose stack-trace to include it later into <see cref="Error.ContainerIsDisposed"/>
        /// exception for easy diagnostics.</summary>
        public Rules WithCaptureContainerDisposeStackTrace() =>
            WithSettings(_settings | Settings.CaptureContainerDisposeStackTrace);

        /// <summary>Allows Func with args specify its own reuse (sharing) behavior.</summary>
        public bool IgnoringReuseForFuncWithArgs =>
            (_settings & Settings.IgnoringReuseForFuncWithArgs) != 0;

        /// <summary>Allows Func with args specify its own reuse (sharing) behavior.</summary>
        public Rules WithIgnoringReuseForFuncWithArgs() =>
            WithSettings(_settings | Settings.IgnoringReuseForFuncWithArgs);

#region Implementation

        private Rules()
        {
            _made = Made.Default;
            _settings = DEFAULT_SETTINGS;
            DefaultReuse = Reuse.Transient;
            DefaultIfAlreadyRegistered = IfAlreadyRegistered.AppendNotKeyed;
            MaxObjectGraphSize = DefaultMaxObjectGraphSize;
        }

        private Rules(Settings settings,
            FactorySelectorRule factorySelector,
            IReuse defaultReuse,
            Made made,
            IfAlreadyRegistered defaultIfAlreadyRegistered,
            int maxObjectGraphSize,
            Ref<ImHashMap<RequestInfo, Expression>> dependencyResolutionCallExpressions,
            ItemToExpressionConverterRule itemToExpressionConverter,
            DynamicRegistrationProvider[] dynamicRegistrationProviders,
            UnknownServiceResolver[] unknownServiceResolvers)
        {
            _settings = settings;
            _made = made;
            FactorySelector = factorySelector;
            DefaultReuse = defaultReuse;
            DefaultIfAlreadyRegistered = defaultIfAlreadyRegistered;
            MaxObjectGraphSize = maxObjectGraphSize;
            DependencyResolutionCallExpressions = dependencyResolutionCallExpressions;
            ItemToExpressionConverter = itemToExpressionConverter;
            DynamicRegistrationProviders = dynamicRegistrationProviders;
            UnknownServiceResolvers = unknownServiceResolvers;
        }

        private Rules WithSettings(Settings newSettings) =>
            new Rules(newSettings,
                FactorySelector, DefaultReuse, _made, DefaultIfAlreadyRegistered, DefaultMaxObjectGraphSize,
                DependencyResolutionCallExpressions, ItemToExpressionConverter,
                DynamicRegistrationProviders, UnknownServiceResolvers);

        private Made _made;

        [Flags]
        private enum Settings
        {
            ThrowIfDependencyHasShorterReuseLifespan = 1 << 1,
            ThrowOnRegisteringDisposableTransient = 1 << 2,
            TrackingDisposableTransients = 1 << 3,
            ImplicitCheckForReuseMatchingScope = 1 << 4,
            VariantGenericTypesInResolvedCollection = 1 << 5,
            ResolveIEnumerableAsLazyEnumerable = 1 << 6,
            EagerCachingSingletonForFasterAccess = 1 << 7,
            ImplicitRootOpenScope = 1 << 8,
            ThrowIfRuntimeStateRequired = 1 << 9,
            CaptureContainerDisposeStackTrace = 1 << 10,
            UseDynamicRegistrationsAsFallback = 1 << 11,
            IgnoringReuseForFuncWithArgs = 1 << 12,
            OverrideRegistrationMade = 1 << 13
        }

        private const Settings DEFAULT_SETTINGS
            = Settings.ThrowIfDependencyHasShorterReuseLifespan
            | Settings.ThrowOnRegisteringDisposableTransient
            | Settings.ImplicitCheckForReuseMatchingScope
            | Settings.VariantGenericTypesInResolvedCollection
            | Settings.EagerCachingSingletonForFasterAccess;

        private Settings _settings;

#endregion
    }

    /// <summary>Wraps constructor or factory method optionally with factory instance to create service.</summary>
    public sealed class FactoryMethod
    {
        /// <summary>Constructor or method to use for service creation.</summary>
        public readonly MemberInfo ConstructorOrMethodOrMember;

        /// <summary>Identifies factory service if factory method is instance member.</summary>
        public readonly ServiceInfo FactoryServiceInfo;

        /// <summary>Wraps method and factory instance.</summary>
        /// <param name="ctorOrMethodOrMember">Constructor, static or instance method, property or field.</param>
        /// <param name="factoryInfo">Factory info to resolve in case of instance <paramref name="ctorOrMethodOrMember"/>.</param>
        /// <returns>New factory method wrapper.</returns>
        public static FactoryMethod Of(MemberInfo ctorOrMethodOrMember, ServiceInfo factoryInfo = null)
        {
            return new FactoryMethod(ctorOrMethodOrMember, factoryInfo);
        }

        /// <summary>Discovers the static factory method or member by name in <typeparamref name="TFactory"/>.
        /// Should play nice with C# <c>nameof</c> operator.</summary>
        /// <param name="methodOrMemberName">Name or method or member.</param>
        /// <typeparam name="TFactory">Class with static member.</typeparam>
        /// <returns>Factory method info.</returns>
        public static FactoryMethod Of<TFactory>(string methodOrMemberName) =>
            Of(typeof(TFactory).GetAllMembers()
                .SingleOrDefault(m => m.Name == methodOrMemberName)
                .ThrowIfNull());

        /// <summary>Pretty prints wrapped method.</summary> <returns>Printed string.</returns>
        public override string ToString() =>
            new StringBuilder().Print(ConstructorOrMethodOrMember.DeclaringType)
                .Append("::").Append(ConstructorOrMethodOrMember).ToString();

        /// <summary>Easy way to specify non-public and most resolvable constructor.</summary>
        /// <param name="mostResolvable">(optional) Instructs to select constructor with max number of params which all are resolvable.</param>
        /// <param name="includeNonPublic">(optional) Consider the non-public constructors.</param>
        /// <returns>Constructor or null if not found.</returns>
        public static FactoryMethodSelector Constructor(bool mostResolvable = false, bool includeNonPublic = false)
        {
            return request =>
            {
                var implType = request.ImplementationType
                    .ThrowIfNull(Error.ImplTypeIsNotSpecifiedForAutoCtorSelection, request);

                var ctors = implType.GetAllConstructors(includeNonPublic).ToArrayOrSelf();
                if (ctors.Length == 0)
                    return null;

                // if there is only one constructor then use it
                if (ctors.Length == 1)
                    return Of(ctors[0]);

                // stop here for non-auto selection
                if (!mostResolvable)
                    return null;

                var containerRules = request.Rules;
                var selector = containerRules.OverrideRegistrationMade
                    ? request.Made.Parameters.OverrideWith(containerRules.Parameters)
                    : containerRules.Parameters.OverrideWith(request.Made.Parameters);
                var parameterSelector = selector(request);

                var ctorsWithMaxParamsFirst = ctors
                    .OrderByDescending(it => it.GetParameters().Length)
                    .ToArray();

                // First the check for normal resolution without Func<a, b, c>
                if (!request.IsWrappedInFuncWithArgs())
                    return Of(ctorsWithMaxParamsFirst
                        .FindFirst(it => it
                            .GetParameters()
                            .FindFirst(p => !IsResolvableParameter(p, parameterSelector, request)) == null)
                        .ThrowIfNull(Error.UnableToFindCtorWithAllResolvableArgs, request));

                // For Func<a, b, c> the constructor should contain all input arguments and
                // the rest should be resolvable.
                var funcType = !request.DirectRuntimeParent.IsEmpty
                    ? request.DirectRuntimeParent.ServiceType
                    : request.PreResolveParent.ServiceType;

                var inputArgTypes = funcType.GetGenericParamsAndArgs();
                var inputArgCount = inputArgTypes.Length - 1;

                // Will store already resolved parameters to not repeat work twice
                List<ParameterInfo> resolvedParams = null;

                ConstructorInfo ctor = null;
                for (var i = 0; ctor == null && i < ctorsWithMaxParamsFirst.Length; i++)
                {
                    ctor = ctorsWithMaxParamsFirst[i];
                    var ctorParams = ctor.GetParameters();

                    // Important: we will not consider constructors with less parameters than in Func,
                    // even if all constructor parameters are matched with some in Func. 
                    if (ctorParams.Length < inputArgCount)
                    {
                        ctor = null;
                        continue;
                    }

                    var alreadyFoundInputArgTypes = 0; // bit mask to track and exclude alreay found input args
                    for (int j = 0; j < ctorParams.Length; j++)
                    {
                        var param = ctorParams[j];

                        // search for parameter in input arguments
                        var isParamInInputArgs = false;
                        for (var k = 0; k < inputArgCount; k++) // important to exclude last parameter
                        {
                            if (inputArgTypes[k] == param.ParameterType)
                            {
                                if ((alreadyFoundInputArgTypes & (1 << k)) == 0)
                                {
                                    alreadyFoundInputArgTypes |= 1 << k; // remember that we checked the input arg already
                                    isParamInInputArgs = true;
                                    break;
                                }
                            }
                        }

                        // if parameter is not provided as Func input argument, 
                        // check if it is resolvable by container
                        if (!isParamInInputArgs)
                        {
                            if (resolvedParams == null ||
                                resolvedParams.IndexOf(param) == -1) // not resolve yet
                            {
                                if (!IsResolvableParameter(param, parameterSelector, request))
                                {
                                    ctor = null;
                                    break; // if parameter is not resolvable, stop considering this constructor
                                }
                                (resolvedParams ?? (resolvedParams = new List<ParameterInfo>())).Add(param);
                            }
                        }
                    }
                }

                return Of(ctor.ThrowIfNull(Error.UnableToFindMatchingCtorForFuncWithArgs, funcType, request));
            };
        }

        /// <summary>Easy way to specify non-public or / and most resolvable constructor.</summary>
        /// <param name="includeNonPublic">(optional) Consider the non-public constructors.</param>
        /// <returns>Constructor or null if not found.</returns>
        public static FactoryMethodSelector DefaultConstructor(bool includeNonPublic = false)
        {
            return request =>
            {
                var implType = request.ImplementationType
                    .ThrowIfNull(Error.ImplTypeIsNotSpecifiedForAutoCtorSelection, request);
                var defaultCtor = implType.GetConstructorOrNull(includeNonPublic, args: ArrayTools.Empty<Type>());
                return defaultCtor != null ? Of(defaultCtor) : null;
            };
        }

        /// <summary>Searches for public constructor with most resolvable parameters or throws <see cref="ContainerException"/> if not found.
        /// Works both for resolving service and Func{TArgs..., TService}</summary>
        public static readonly FactoryMethodSelector ConstructorWithResolvableArguments =
            Constructor(mostResolvable: true);

        /// <summary>Searches for constructor (including non public ones) with most
        /// resolvable parameters or throws <see cref="ContainerException"/> if not found.
        /// Works both for resolving service and Func{TArgs..., TService}</summary>
        public static readonly FactoryMethodSelector ConstructorWithResolvableArgumentsIncludingNonPublic =
            Constructor(mostResolvable: true, includeNonPublic: true);

        /// <summary>Checks that parameter is selected on requested path and with provided parameter selector.</summary>
        /// <param name="parameter"></param> <param name="parameterSelector"></param> <param name="request"></param>
        /// <returns>True if parameter is resolvable.</returns>
        public static bool IsResolvableParameter(ParameterInfo parameter,
            Func<ParameterInfo, ParameterServiceInfo> parameterSelector, Request request)
        {
            var parameterServiceInfo = parameterSelector(parameter) ?? ParameterServiceInfo.Of(parameter);
            var parameterRequest = request.Push
                (parameterServiceInfo.WithDetails(ServiceDetails.IfUnresolvedReturnDefault));
            if (parameterServiceInfo.Details.HasCustomValue)
            {
                var customValue = parameterServiceInfo.Details.CustomValue;
                return customValue == null || customValue.GetType().IsAssignableTo(parameterRequest.GetActualServiceType());
            }

            var factory = parameterRequest.Container.ResolveFactory(parameterRequest);
            return factory != null && factory.GetExpressionOrDefault(parameterRequest) != null;
        }

        private FactoryMethod(MemberInfo constructorOrMethodOrMember, ServiceInfo factoryServiceInfo = null)
        {
            ConstructorOrMethodOrMember = constructorOrMethodOrMember;
            FactoryServiceInfo = factoryServiceInfo;
        }
    }

    /// <summary>Rules how to: <list type="bullet">
    /// <item>Select constructor for creating service with <see cref="FactoryMethod"/>.</item>
    /// <item>Specify how to resolve constructor parameters with <see cref="Parameters"/>.</item>
    /// <item>Specify what properties/fields to resolve and how with <see cref="PropertiesAndFields"/>.</item>
    /// </list></summary>
    public class Made
    {
        /// <summary>Returns delegate to select constructor based on provided request.</summary>
        public FactoryMethodSelector FactoryMethod { get; private set; }

        /// <summary>Return type of strongly-typed factory method expression.</summary>
        public Type FactoryMethodKnownResultType { get; private set; }

        /// <summary>True is made has properties or parameters with custom value.
        /// That's mean the whole made become context based which affects caching</summary>
        public bool HasCustomDependencyValue { get; private set; }

        /// <summary>Specifies how constructor parameters should be resolved:
        /// parameter service key and type, throw or return default value if parameter is unresolved.</summary>
        public ParameterSelector Parameters { get; private set; }

        /// <summary>Specifies what <see cref="ServiceInfo"/> should be used when resolving property or field.</summary>
        public PropertiesAndFieldsSelector PropertiesAndFields { get; private set; }

        /// <summary>Container will use some sensible defaults for service creation.</summary>
        public static readonly Made Default = new Made();

        /// <summary>Creates rules with only <see cref="FactoryMethod"/> specified.</summary>
        public static implicit operator Made(FactoryMethodSelector factoryMethod) =>
            Of(factoryMethod);

        /// <summary>Creates rules with only <see cref="Parameters"/> specified.</summary>
        public static implicit operator Made(ParameterSelector parameters) =>
            Of(parameters: parameters);

        /// <summary>Creates rules with only <see cref="PropertiesAndFields"/> specified.</summary>
        public static implicit operator Made(PropertiesAndFieldsSelector propertiesAndFields) =>
            Of(propertiesAndFields: propertiesAndFields);

        /// <summary>Specifies injections rules for Constructor, Parameters, Properties and Fields. If no rules specified returns <see cref="Default"/> rules.</summary>
        /// <param name="factoryMethod">(optional)</param> <param name="parameters">(optional)</param> <param name="propertiesAndFields">(optional)</param>
        /// <returns>New injection rules or <see cref="Default"/>.</returns>
        public static Made Of(FactoryMethodSelector factoryMethod = null,
            ParameterSelector parameters = null, PropertiesAndFieldsSelector propertiesAndFields = null) =>
            factoryMethod == null && parameters == null && propertiesAndFields == null
                ? Default : new Made(factoryMethod, parameters, propertiesAndFields);

        /// <summary>Specifies injections rules for Constructor, Parameters, Properties and Fields. If no rules specified returns <see cref="Default"/> rules.</summary>
        /// <param name="factoryMethod">Known factory method.</param>
        /// <param name="parameters">(optional)</param> <param name="propertiesAndFields">(optional)</param>
        /// <returns>New injection rules.</returns>
        public static Made Of(FactoryMethod factoryMethod,
            ParameterSelector parameters = null, PropertiesAndFieldsSelector propertiesAndFields = null)
        {
            var methodReturnType = factoryMethod.ConstructorOrMethodOrMember.GetReturnTypeOrDefault();

            // Normalizes open-generic type to open-generic definition,
            // because for base classes and return types it may not be the case (they may be partialy closed).
            if (methodReturnType != null && methodReturnType.IsOpenGeneric())
                methodReturnType = methodReturnType.GetGenericTypeDefinition();

            return new Made(_ => factoryMethod, parameters, propertiesAndFields, methodReturnType);
        }

        /// <summary>Creates rules with only <see cref="FactoryMethod"/> specified.</summary>
        public static Made Of(MemberInfo factoryMethodOrMember, ServiceInfo factoryInfo = null,
            ParameterSelector parameters = null, PropertiesAndFieldsSelector propertiesAndFields = null) =>
            Of(DryIoc.FactoryMethod.Of(factoryMethodOrMember, factoryInfo), parameters, propertiesAndFields);

        /// <summary>Creates factory specification with method or member selector based on request.
        /// Where <paramref name="getMethodOrMember"/>Method, or constructor, or member selector.</summary>
        public static Made Of(Func<Request, MemberInfo> getMethodOrMember, ServiceInfo factoryInfo = null,
            ParameterSelector parameters = null, PropertiesAndFieldsSelector propertiesAndFields = null) =>
            Of(r => DryIoc.FactoryMethod.Of(getMethodOrMember(r), factoryInfo),
                parameters, propertiesAndFields);

        /// <summary>Creates factory specification with method or member selector based on request.
        /// Where <paramref name="getMethodOrMember"/>Method, or constructor, or member selector.</summary>
        public static Made Of(Func<Request, MemberInfo> getMethodOrMember, Func<Request, ServiceInfo> factoryInfo,
            ParameterSelector parameters = null, PropertiesAndFieldsSelector propertiesAndFields = null) =>
            Of(r => DryIoc.FactoryMethod.Of(getMethodOrMember(r), factoryInfo(r)),
                parameters, propertiesAndFields);

        /// <summary>Defines how to select constructor from implementation type.
        /// Where <paramref name="getConstructor"/> is delegate taking implementation type as input 
        /// and returning selected constructor info.</summary>
        public static Made Of(Func<Type, ConstructorInfo> getConstructor, ParameterSelector parameters = null,
            PropertiesAndFieldsSelector propertiesAndFields = null) =>
            Of(r => DryIoc.FactoryMethod.Of(getConstructor(r.ImplementationType)
                .ThrowIfNull(Error.GotNullConstructorFromFactoryMethod, r)),
                parameters, propertiesAndFields);

        /// <summary>Defines factory method using expression of constructor call (with properties), or static method call.</summary>
        /// <typeparam name="TService">Type with constructor or static method.</typeparam>
        /// <param name="serviceReturningExpr">Expression tree with call to constructor with properties:
        /// <code lang="cs"><![CDATA[() => new Car(Arg.Of<IEngine>()) { Color = Arg.Of<Color>("CarColor") }]]></code>
        /// or static method call <code lang="cs"><![CDATA[() => Car.Create(Arg.Of<IEngine>())]]></code></param>
        /// <param name="argValues">(optional) Primitive custom values for dependencies.</param>
        /// <returns>New Made specification.</returns>
        public static TypedMade<TService> Of<TService>(
            Expression<Func<TService>> serviceReturningExpr,
            params Func<RequestInfo, object>[] argValues) =>
            FromExpression<TService>(null, serviceReturningExpr, argValues);

        /// <summary>Defines creation info from factory method call Expression without using strings.
        /// You can supply any/default arguments to factory method, they won't be used, it is only to find the <see cref="MethodInfo"/>.</summary>
        /// <typeparam name="TFactory">Factory type.</typeparam> <typeparam name="TService">Factory product type.</typeparam>
        /// <param name="getFactoryInfo">Returns or resolves factory instance.</param>
        /// <param name="serviceReturningExpr">Method, property or field expression returning service.</param>
        /// <param name="argValues">(optional) Primitive custom values for dependencies.</param>
        /// <returns>New Made specification.</returns>
        public static TypedMade<TService> Of<TFactory, TService>(
            Func<Request, ServiceInfo.Typed<TFactory>> getFactoryInfo,
            Expression<Func<TFactory, TService>> serviceReturningExpr,
            params Func<RequestInfo, object>[] argValues)
            where TFactory : class
        {
            getFactoryInfo.ThrowIfNull();
            // NOTE: cannot convert to method group because of lack of covariance support in .Net 3.5
            return FromExpression<TService>(r => getFactoryInfo(r).ThrowIfNull(), serviceReturningExpr, argValues);
        }

        private static TypedMade<TService> FromExpression<TService>(
            Func<Request, ServiceInfo> getFactoryInfo, LambdaExpression serviceReturningExpr,
            params Func<RequestInfo, object>[] argValues)
        {
            var callExpr = serviceReturningExpr.ThrowIfNull().Body;
            if (callExpr.NodeType == ExpressionType.Convert) // proceed without Cast expression.
                return FromExpression<TService>(getFactoryInfo,
                    Expression.Lambda(((UnaryExpression)callExpr).Operand, ArrayTools.Empty<ParameterExpression>()),
                    argValues);

            MemberInfo ctorOrMethodOrMember;
            IList<Expression> argExprs = null;
            IList<MemberBinding> memberBindingExprs = null;
            ParameterInfo[] parameters = null;

            if (callExpr.NodeType == ExpressionType.New || callExpr.NodeType == ExpressionType.MemberInit)
            {
                var newExpr = callExpr as NewExpression ?? ((MemberInitExpression)callExpr).NewExpression;
                ctorOrMethodOrMember = newExpr.Constructor;
                parameters = newExpr.Constructor.GetParameters();
                argExprs = newExpr.Arguments;
                if (callExpr is MemberInitExpression)
                    memberBindingExprs = ((MemberInitExpression)callExpr).Bindings;
            }
            else if (callExpr.NodeType == ExpressionType.Call)
            {
                var methodCallExpr = (MethodCallExpression)callExpr;
                ctorOrMethodOrMember = methodCallExpr.Method;
                parameters = methodCallExpr.Method.GetParameters();
                argExprs = methodCallExpr.Arguments;
            }
            else if (callExpr.NodeType == ExpressionType.Invoke)
            {
                var invokeExpr = (InvocationExpression)callExpr;
                var invokedDelegateExpr = invokeExpr.Expression;
                var invokeMethod = invokedDelegateExpr.Type.Method("Invoke");
                ctorOrMethodOrMember = invokeMethod;
                parameters = invokeMethod.GetParameters();
                argExprs = invokeExpr.Arguments;
            }

            else if (callExpr.NodeType == ExpressionType.MemberAccess)
            {
                var member = ((MemberExpression)callExpr).Member;
                Throw.If(!(member is PropertyInfo) && !(member is FieldInfo),
                    Error.UnexpectedFactoryMemberExpression, member);
                ctorOrMethodOrMember = member;
            }
            else return Throw.For<TypedMade<TService>>(Error.NotSupportedMadeExpression, callExpr);

            FactoryMethodSelector factoryMethod = request =>
                DryIoc.FactoryMethod.Of(ctorOrMethodOrMember, getFactoryInfo == null ? null : getFactoryInfo(request));

            var hasCustomValue = false;

            var parameterSelector = parameters.IsNullOrEmpty() ? null
                : ComposeParameterSelectorFromArgs(ref hasCustomValue, parameters, argExprs, argValues);

            var propertiesAndFieldsSelector =
                memberBindingExprs == null || memberBindingExprs.Count == 0 ? null
                : ComposePropertiesAndFieldsSelector(ref hasCustomValue, memberBindingExprs, argValues);

            return new TypedMade<TService>(factoryMethod, parameterSelector, propertiesAndFieldsSelector, hasCustomValue);
        }

        /// <summary>Typed version of <see cref="Made"/> specified with statically typed expression tree.</summary>
        /// <typeparam name="TService">Type that expression returns.</typeparam>
        public sealed class TypedMade<TService> : Made
        {
            /// <summary>Creates typed version.</summary>
            /// <param name="factoryMethod"></param> <param name="parameters"></param> <param name="propertiesAndFields"></param>
            /// <param name="hasCustomValue"></param>
            internal TypedMade(FactoryMethodSelector factoryMethod = null,
                ParameterSelector parameters = null, PropertiesAndFieldsSelector propertiesAndFields = null,
                bool hasCustomValue = false)
                : base(factoryMethod, parameters, propertiesAndFields, typeof(TService), hasCustomValue)
            { }
        }

#region Implementation

        private Made(FactoryMethodSelector factoryMethod = null, ParameterSelector parameters = null, PropertiesAndFieldsSelector propertiesAndFields = null,
            Type factoryMethodKnownResultType = null, bool hasCustomValue = false)
        {
            FactoryMethod = factoryMethod;
            Parameters = parameters;
            PropertiesAndFields = propertiesAndFields;
            FactoryMethodKnownResultType = factoryMethodKnownResultType;
            HasCustomDependencyValue = hasCustomValue;
        }

        private static ParameterSelector ComposeParameterSelectorFromArgs(ref bool hasCustomValue,
            ParameterInfo[] parameterInfos, IList<Expression> argExprs, params Func<RequestInfo, object>[] argValues)
        {
            var parameters = DryIoc.Parameters.Of;
            for (var i = 0; i < argExprs.Count; i++)
            {
                var parameter = parameterInfos[i];
                var methodCallExpr = argExprs[i] as MethodCallExpression;
                if (methodCallExpr != null)
                {
                    Throw.If(methodCallExpr.Method.DeclaringType != typeof(Arg),
                        Error.UnexpectedExpressionInsteadOfArgMethod, methodCallExpr);

                    if (methodCallExpr.Method.Name == Arg.ArgIndexMethodName)
                    {
                        var getArgValue = GetArgCustomValueProvider(methodCallExpr, argValues);
                        parameters = parameters.Details((r, p) => p.Equals(parameter)
                            ? ServiceDetails.Of(getArgValue(r.RequestInfo))
                            : null);
                        hasCustomValue = true;
                    }
                    else // handle service details
                    {
                        var defaultValue = parameter.IsOptional ? parameter.DefaultValue : null;
                        var argDetails = GetArgServiceDetails(methodCallExpr, parameter.ParameterType, IfUnresolved.Throw, defaultValue);
                        parameters = parameters.Details((r, p) => p.Equals(parameter) ? argDetails : null);
                    }
                }
                else
                {
                    var customValue = GetArgExpressionValueOrThrow(argExprs[i]);
                    parameters = parameters.Details((r, p) => p.Equals(parameter) ? ServiceDetails.Of(customValue) : null);
                }
            }
            return parameters;
        }

        private static PropertiesAndFieldsSelector ComposePropertiesAndFieldsSelector(ref bool hasCustomValue,
            IList<MemberBinding> memberBindings, params Func<RequestInfo, object>[] argValues)
        {
            var propertiesAndFields = DryIoc.PropertiesAndFields.Of;
            for (var i = 0; i < memberBindings.Count; i++)
            {
                var memberAssignment = (memberBindings[i] as MemberAssignment).ThrowIfNull();
                var member = memberAssignment.Member;

                var methodCallExpr = memberAssignment.Expression as MethodCallExpression;
                if (methodCallExpr == null) // not an Arg.Of: e.g. constant or variable
                {
                    var customValue = GetArgExpressionValueOrThrow(memberAssignment.Expression);
                    propertiesAndFields = propertiesAndFields.OverrideWith(r => new[]
                    {
                        PropertyOrFieldServiceInfo.Of(member).WithDetails(ServiceDetails.Of(customValue))
                    });
                }
                else
                {
                    Throw.If(methodCallExpr.Method.DeclaringType != typeof(Arg),
                        Error.UnexpectedExpressionInsteadOfArgMethod, methodCallExpr);

                    if (methodCallExpr.Method.Name == Arg.ArgIndexMethodName) // handle custom value
                    {
                        var getArgValue = GetArgCustomValueProvider(methodCallExpr, argValues);
                        propertiesAndFields = propertiesAndFields.OverrideWith(r => new[]
                        {
                            PropertyOrFieldServiceInfo.Of(member)
                                .WithDetails(ServiceDetails.Of(getArgValue(r.RequestInfo)))
                        });
                        hasCustomValue = true;
                    }
                    else
                    {
                        var memberType = member.GetReturnTypeOrDefault();
                        var argServiceDetails = GetArgServiceDetails(methodCallExpr, memberType, IfUnresolved.ReturnDefault, null);
                        propertiesAndFields = propertiesAndFields.OverrideWith(r => new[]
                        {
                            PropertyOrFieldServiceInfo.Of(member).WithDetails(argServiceDetails)
                        });
                    }
                }
            }
            return propertiesAndFields;
        }

        private static Func<RequestInfo, object> GetArgCustomValueProvider(MethodCallExpression methodCallExpr, Func<RequestInfo, object>[] argValues)
        {
            Throw.If(argValues.IsNullOrEmpty(), Error.ArgValueIndexIsProvidedButNoArgValues);

            var argIndexExpr = methodCallExpr.Arguments[0];
            var argIndex = (int)GetArgExpressionValueOrThrow(argIndexExpr);

            Throw.If(argIndex < 0 || argIndex >= argValues.Length,
                Error.ArgValueIndexIsOutOfProvidedArgValues, argIndex, argValues);

            var getArgValue = argValues[argIndex];
            return getArgValue;
        }

        private static ServiceDetails GetArgServiceDetails(MethodCallExpression methodCallExpr,
            Type dependencyType, IfUnresolved defaultIfUnresolved, object defaultValue)
        {
            var requiredServiceType = methodCallExpr.Method.GetGenericArguments().Last();
            if (requiredServiceType == dependencyType)
                requiredServiceType = null;

            var serviceKey = default(object);
            var metadataKey = default(string);
            var metadata = default(object);
            var ifUnresolved = defaultIfUnresolved;

            var hasPrevArg = false;

            var argExprs = methodCallExpr.Arguments;
            if (argExprs.Count == 2 &&
                argExprs[0].Type == typeof(string) &&
                argExprs[1].Type != typeof(IfUnresolved)) // matches the Of overload for metadata
            {
                metadataKey = (string)GetArgExpressionValueOrThrow(argExprs[0]);
                metadata = GetArgExpressionValueOrThrow(argExprs[1]);
            }
            else
            {
                for (var a = 0; a < argExprs.Count; a++)
                {
                    var argValue = GetArgExpressionValueOrThrow(argExprs[a]);
                    if (argValue != null)
                    {
                        if (argValue is IfUnresolved)
                        {
                            ifUnresolved = (IfUnresolved)argValue;
                            if (hasPrevArg) // the only possible argument is default value.
                            {
                                defaultValue = serviceKey;
                                serviceKey = null;
                            }
                        }
                        else
                        {
                            serviceKey = argValue;
                            hasPrevArg = true;
                        }
                    }
                }
            }

            return ServiceDetails.Of(requiredServiceType, serviceKey, ifUnresolved, defaultValue, metadataKey, metadata);
        }

        private static object GetArgExpressionValueOrThrow(Expression argExpr)
        {
            var valueExpr = argExpr as ConstantExpression;
            if (valueExpr != null)
                return valueExpr.Value;

            var convert = argExpr as UnaryExpression; // e.g. (object)SomeEnum.Value
            if (convert != null && convert.NodeType == ExpressionType.Convert)
                return GetArgExpressionValueOrThrow(convert.Operand as ConstantExpression);

            var member = argExpr as MemberExpression;
            if (member != null)
            {
                var memberOwner = member.Expression as ConstantExpression;
                if (memberOwner != null && memberOwner.Type.IsClosureType())
                {
                    var memberField = member.Member as FieldInfo;
                    if (memberField != null)
                        return memberField.GetValue(memberOwner.Value);
                }
            }

            return Throw.For<object>(Error.UnexpectedExpressionInsteadOfConstant, argExpr);
        }

#endregion
    }

    /// <summary>Class for defining parameters/properties/fields service info in <see cref="Made"/> expressions.
    /// Its methods are NOT actually called, they just used to reflect service info from call expression.</summary>
    public static class Arg
    {
        /// <summary>Specifies required service type of parameter or member. If required type is the same as parameter/member type,
        /// the method is just a placeholder to help detect constructor or factory method, and does not have additional meaning.</summary>
        public static TRequired Of<TRequired>() => default(TRequired);

        /// <summary>Specifies both service and required service types.</summary>
        public static TService Of<TService, TRequired>() => default(TService);

        /// <summary>Specifies required service type of parameter or member. Plus specifies if-unresolved policy.</summary>
        public static TRequired Of<TRequired>(IfUnresolved ifUnresolved) => default(TRequired);

        /// <summary>Specifies both service and required service types.</summary>
        public static TService Of<TService, TRequired>(IfUnresolved ifUnresolved) => default(TService);

        /// <summary>Specifies required service type of parameter or member. Plus specifies service key.</summary>
        public static TRequired Of<TRequired>(object serviceKey) => default(TRequired);

        /// <summary>Specifies both service and required service types.</summary>
        public static TService Of<TService, TRequired>(object serviceKey) => default(TService);

        /// <summary>Specifies required service type of parameter or member. Plus specifies service key.</summary>
        public static TRequired Of<TRequired>(string metadataKey, object metadata) => default(TRequired);

        /// <summary>Specifies both service and required service types.</summary>
        public static TService Of<TService, TRequired>(string metadataKey, object metadata) => default(TService);

        /// <summary>Specifies required service type of parameter or member. Plus specifies if-unresolved policy. Plus specifies service key.</summary>
        public static TRequired Of<TRequired>(IfUnresolved ifUnresolved, object serviceKey) => default(TRequired);

        /// <summary>Specifies both service and required service types.</summary>
        public static TService Of<TService, TRequired>(IfUnresolved ifUnresolved, object serviceKey) => default(TService);

        /// <summary>Specifies required service type, default value and <see cref="IfUnresolved.ReturnDefault"/>.</summary>
        public static TRequired Of<TRequired>(TRequired defaultValue, IfUnresolved ifUnresolved) => default(TRequired);

        /// <summary>Specifies required service type, default value and <see cref="IfUnresolved.ReturnDefault"/>.</summary>
        public static TRequired Of<TRequired>(TRequired defaultValue, IfUnresolved ifUnresolved, object serviceKey) => default(TRequired);

        /// <summary>Specifies argument index starting from 0 to use corresponding custom value factory,
        /// similar to String.Format <c>"{0}, {1}, etc"</c>.</summary>
        public static T Index<T>(int argIndex) => default(T);

        /// <summary>Name is close to method itself to not forget when renaming the method.</summary>
        public static string ArgIndexMethodName = "Index";
    }

    /// <summary>Contains <see cref="IRegistrator"/> extension methods to simplify general use cases.</summary>
    public static class Registrator
    {
        /// <summary>Registers service of <paramref name="serviceType"/>.</summary>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="serviceType">The service type to register</param>
        /// <param name="factory"><see cref="Factory"/> details object.</param>
        /// <param name="ifAlreadyRegistered">(optional) policy to deal with case when service with such type and name is already registered.</param>
        /// <param name="serviceKey">(optional Could be of any type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        public static void Register(this IRegistrator registrator, Type serviceType, Factory factory,
            IfAlreadyRegistered? ifAlreadyRegistered = null, object serviceKey = null) =>
            registrator.Register(factory, serviceType, serviceKey, ifAlreadyRegistered, false);

        /// <summary>Registers service <paramref name="serviceType"/> with corresponding <paramref name="implementationType"/>.</summary>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="serviceType">The service type to register.</param>
        /// <param name="implementationType">Implementation type. Concrete and open-generic class are supported.</param>
        /// <param name="reuse">(optional) <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>.
        ///     Default value means no reuse, aka Transient.</param>
        /// <param name="made">(optional) specifies <see cref="Made"/>.</param>
        /// <param name="setup">(optional) Factory setup, by default is (<see cref="Setup.Default"/>)</param>
        /// <param name="ifAlreadyRegistered">(optional) Policy to deal with case when service with such type and name is already registered.</param>
        /// <param name="serviceKey">(optional) Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        public static void Register(this IRegistrator registrator, Type serviceType, Type implementationType,
            IReuse reuse = null, Made made = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null,
            object serviceKey = null) =>
            registrator.Register(new ReflectionFactory(implementationType, reuse, made, setup),
                serviceType, serviceKey, ifAlreadyRegistered, false);

        /// <summary>Registers service of <paramref name="serviceAndMayBeImplementationType"/>. ServiceType will be the same as <paramref name="serviceAndMayBeImplementationType"/>.</summary>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="serviceAndMayBeImplementationType">Implementation type. Concrete and open-generic class are supported.</param>
        /// <param name="reuse">(optional) <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="made">(optional) specifies <see cref="Made"/>.</param>
        /// <param name="setup">(optional) factory setup, by default is (<see cref="Setup.Default"/>)</param>
        /// <param name="ifAlreadyRegistered">(optional) policy to deal with case when service with such type and name is already registered.</param>
        /// <param name="serviceKey">(optional) Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        public static void Register(this IRegistrator registrator, Type serviceAndMayBeImplementationType,
            IReuse reuse = null, Made made = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null,
            object serviceKey = null) =>
            registrator.Register(new ReflectionFactory(serviceAndMayBeImplementationType, reuse, made, setup),
                serviceAndMayBeImplementationType, serviceKey, ifAlreadyRegistered, false);

        /// <summary>Registers service of <typeparamref name="TService"/> type implemented by <typeparamref name="TImplementation"/> type.</summary>
        /// <typeparam name="TService">The type of service.</typeparam>
        /// <typeparam name="TImplementation">The type of service.</typeparam>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="reuse">(optional) <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="made">(optional) specifies <see cref="Made"/>.</param>
        /// <param name="setup">(optional) factory setup, by default is (<see cref="Setup.Default"/>)</param>
        /// <param name="ifAlreadyRegistered">(optional) policy to deal with case when service with such type and name is already registered.</param>
        /// <param name="serviceKey">(optional) Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        public static void Register<TService, TImplementation>(this IRegistrator registrator,
            IReuse reuse = null, Made made = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null,
            object serviceKey = null)
            where TImplementation : TService =>
            registrator.Register(new ReflectionFactory(typeof(TImplementation), reuse, made, setup),
                typeof(TService), serviceKey, ifAlreadyRegistered, isStaticallyChecked: true);

        /// <summary>Registers implementation type <typeparamref name="TImplementation"/> with itself as service type.</summary>
        /// <typeparam name="TImplementation">The type of service.</typeparam>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="reuse">(optional) <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="made">(optional) specifies <see cref="Made"/>.</param>
        /// <param name="setup">(optional) Factory setup, by default is (<see cref="Setup.Default"/>)</param>
        /// <param name="ifAlreadyRegistered">(optional) Policy to deal with case when service with such type and name is already registered.</param>
        /// <param name="serviceKey">(optional) Service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        public static void Register<TImplementation>(this IRegistrator registrator,
            IReuse reuse = null, Made made = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null,
            object serviceKey = null) =>
            registrator.Register<TImplementation, TImplementation>(reuse, made, setup, ifAlreadyRegistered, serviceKey);

        /// <summary>Registers service type returned by Made expression.</summary>
        /// <typeparam name="TService">The type of service.</typeparam>
        /// <typeparam name="TMadeResult">The type returned by Made expression.</typeparam>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="made">Made specified with strongly-typed service creation expression.</param>
        /// <param name="reuse">(optional) <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="setup">(optional) Factory setup, by default is (<see cref="Setup.Default"/>)</param>
        /// <param name="ifAlreadyRegistered">(optional) Policy to deal with case when service with such type and name is already registered.</param>
        /// <param name="serviceKey">(optional) Service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        public static void Register<TService, TMadeResult>(this IRegistrator registrator,
            Made.TypedMade<TMadeResult> made, IReuse reuse = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null,
            object serviceKey = null) where TMadeResult : TService =>
            registrator.Register(new ReflectionFactory(default(Type), reuse, made, setup),
                typeof(TService), serviceKey, ifAlreadyRegistered, isStaticallyChecked: true);

        /// <summary>Registers service type returned by Made expression.</summary>
        /// <typeparam name="TService">The type of service.</typeparam>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="made">Made specified with strongly-typed service creation expression.</param>
        /// <param name="reuse">(optional) <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="setup">(optional) Factory setup, by default is (<see cref="Setup.Default"/>)</param>
        /// <param name="ifAlreadyRegistered">(optional) Policy to deal with case when service with such type and name is already registered.</param>
        /// <param name="serviceKey">(optional) Service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        public static void Register<TService>(this IRegistrator registrator,
            Made.TypedMade<TService> made, IReuse reuse = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null,
            object serviceKey = null) =>
            registrator.Register<TService, TService>(made, reuse, setup, ifAlreadyRegistered, serviceKey);

        /// <summary>Action that could be used by User to customize register many default behavior.</summary>
        /// <param name="r">Registrator provided to do any arbitrary registration User wants.</param>
        /// <param name="serviceTypes">Valid service type that could be used with <paramref name="implType"/>.</param>
        /// <param name="implType">Concrete or open-generic implementation type.</param>
        public delegate void RegisterManyAction(IRegistrator r, Type[] serviceTypes, Type implType);

        // todo: Perf: Add optional @isStaticallyChecked to skip check for implemented types.
        /// <summary>Registers many service types with the same implementation.</summary>
        /// <param name="registrator">Registrator/Container</param>
        /// <param name="serviceTypes">1 or more service types.</param>
        /// <param name="implementationType">Should implement service types. Will throw if not.</param>
        /// <param name="reuse">(optional)</param> <param name="made">(optional) How to create implementation instance.</param>
        /// <param name="setup">(optional)</param> <param name="ifAlreadyRegistered">(optional) By default <see cref="IfAlreadyRegistered.AppendNotKeyed"/></param>
        /// <param name="serviceKey">(optional)</param>
        public static void RegisterMany(this IRegistrator registrator, Type[] serviceTypes, Type implementationType,
            IReuse reuse = null, Made made = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null,
            object serviceKey = null)
        {
            var factory = new ReflectionFactory(implementationType, reuse, made, setup);
            if (serviceTypes.Length == 1)
                registrator.Register(serviceTypes[0], factory, ifAlreadyRegistered, serviceKey);
            else
                for (var i = 0; i < serviceTypes.Length; i++)
                    registrator.Register(serviceTypes[i], factory, ifAlreadyRegistered, serviceKey);
        }

        /// <summary>List of types excluded by default from RegisterMany convention.</summary>
        public static readonly string[] ExcludedGeneralPurposeServiceTypes =
        {
            "System.IDisposable",
            "System.ValueType",
            "System.ICloneable",
            "System.IEquatable",
            "System.IComparable",
            "System.Runtime.Serialization.ISerializable",
            "System.Collections.IStructuralEquatable",
            "System.Collections.IEnumerable",
            "System.Collections.IList",
            "System.Collections.ICollection",
        };

        /// <summary>Checks that type is not in the list of <see cref="ExcludedGeneralPurposeServiceTypes"/>.</summary>
        /// <param name="type">Type to check</param> <returns>True if not in the list.</returns>
        public static bool IsExcludedGeneralPurposeServiceType(this Type type) =>
            ExcludedGeneralPurposeServiceTypes.IndexOf((type.Namespace + "." + type.Name).Split('`')[0]) != -1;

        /// <summary>Returns only those types that could be used as service types of <paramref name="type"/>. It means that
        /// for open-generic <paramref name="type"/> its service type should supply all type arguments.</summary>
        /// <param name="type">Source type: may be concrete, abstract or generic definition.</param>
        /// <param name="nonPublicServiceTypes">(optional) Include non public service types.</param>
        /// <returns>Array of types or empty.</returns>
        public static Type[] GetImplementedServiceTypes(this Type type, bool nonPublicServiceTypes = false)
        {
            var implementedTypes = type.GetImplementedTypes(ReflectionTools.AsImplementedType.SourceType);

            var serviceTypes = implementedTypes.Match(t =>
                (nonPublicServiceTypes || t.IsPublicOrNestedPublic()) &&
                !t.IsPrimitive() &&
                !t.IsExcludedGeneralPurposeServiceType());

            if (type.IsGenericDefinition())
                serviceTypes = serviceTypes.Match(
                    t => t.ContainsAllGenericTypeParameters(type.GetGenericParamsAndArgs()),
                    t => t.GetGenericDefinitionOrNull());

            return serviceTypes;
        }

        /// <summary>Returns the sensible services automatically discovered for RegisterMany implementation type.
        /// Excludes the collection wrapper interfaces.</summary>
        /// <param name="type">Source type, may be concrete, abstract or generic definition.</param>
        /// <param name="nonPublicServiceTypes">(optional) Include non public service types.</param>
        /// <returns>Array of types or empty.</returns>
        public static Type[] GetRegisterManyImplementedServiceTypes(this Type type, bool nonPublicServiceTypes = false) =>
            GetImplementedServiceTypes(type, nonPublicServiceTypes)
                .Match(t => !t.IsGenericDefinition() || WrappersSupport.ArrayInterfaces.IndexOf(t) == -1);

        /// <summary>Returns the types suitable to be an implementation types for <see cref="ReflectionFactory"/>:
        /// actually a non abstract and not compiler generated classes.</summary>
        /// <param name="assembly">Assembly to get types from.</param>
        /// <returns>Types.</returns>
        public static IEnumerable<Type> GetImplementationTypes(this Assembly assembly) =>
            Portable.GetAssemblyTypes(assembly).Where(IsImplementationType);

        /// <summary>Checks if type can be used as implementation type for reflection factory,
        /// and therefore registered to container. Usually used to discover implementation types from assembly.</summary>
        /// <param name="type">Type to check.</param> <returns>True if implementation type.</returns>
        public static bool IsImplementationType(this Type type) =>
            type.IsClass() && !type.IsAbstract() && !type.IsCompilerGenerated();

        /// <summary>Checks if <paramref name="type"/> implements the <paramref name="serviceType"/>,
        /// along the line checking if <paramref name="type"/> and <paramref name="serviceType"/>
        /// are valid implementation and service types.</summary>
        /// <param name="type">Implementation type.</param>
        /// <param name="serviceType">Service type.</param>
        /// <param name="checkIfOpenGenericImplementsClosedGeneric">(optional)</param>
        /// <returns>Check result.</returns>
        public static bool ImplementsServiceType(this Type type, Type serviceType,
            bool checkIfOpenGenericImplementsClosedGeneric = false)
        {
            if (!type.IsImplementationType())
                return false;

            var serviceTypes = type.GetImplementedServiceTypes(nonPublicServiceTypes: true);
            if (serviceTypes.Length == 0)
                return false;

            if (!type.IsOpenGeneric())
                return serviceTypes.IndexOf(serviceType) != -1;

            if (!checkIfOpenGenericImplementsClosedGeneric &&
                !serviceType.IsOpenGeneric())
                return false;

            if (!serviceType.IsGeneric()) // should be generic to supply arguments to implType
                return false;

            return serviceTypes.IndexOf(serviceType.GetGenericTypeDefinition()) != -1;
        }

        /// <summary>Registers many implementations with the auto-figured service types.</summary>
        /// <param name="registrator">Registrator/Container to register with.</param>
        /// <param name="implTypes">Implementation type provider.</param>
        /// <param name="action">(optional) User specified registration action:
        /// may be used to filter registrations or specify non-default registration options, e.g. Reuse or ServiceKey, etc.</param>
        /// <param name="nonPublicServiceTypes">(optional) Include non public service types.</param>
        public static void RegisterMany(this IRegistrator registrator, IEnumerable<Type> implTypes, RegisterManyAction action,
            bool nonPublicServiceTypes = false)
        {
            foreach (var implType in implTypes)
            {
                var serviceTypes = implType.GetRegisterManyImplementedServiceTypes(nonPublicServiceTypes);
                if (serviceTypes.IsNullOrEmpty())
                    continue;

                if (action == null)
                    registrator.RegisterMany(serviceTypes, implType);
                else
                    action(registrator, serviceTypes, implType);
            }
        }

        /// <summary>Registers many implementations with their auto-figured service types.</summary>
        /// <param name="registrator">Registrator/Container to register with.</param>
        /// <param name="implTypes">Implementation type provider.</param>
        /// <param name="reuse">(optional) Reuse to apply to all service registrations.</param>
        /// <param name="made">(optional) Allow to select constructor/method to create service, specify how to inject its parameters and properties/fields.</param>
        /// <param name="setup">(optional) Factory setup, by default is <see cref="Setup.Default"/>, check <see cref="Setup"/> class for available setups.</param>
        /// <param name="ifAlreadyRegistered">(optional) Policy to deal with existing service registrations.</param>
        /// <param name="serviceTypeCondition">(optional) Condition to select only specific service type to register.</param>
        /// <param name="nonPublicServiceTypes">(optional) Include non public service types.</param>
        /// <param name="serviceKey">(optional) service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        public static void RegisterMany(this IRegistrator registrator, IEnumerable<Type> implTypes,
            IReuse reuse = null, Made made = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null,
            Func<Type, bool> serviceTypeCondition = null, bool nonPublicServiceTypes = false,
            object serviceKey = null) =>
            registrator.RegisterMany(implTypes, (r, serviceTypes, implType) =>
            {
                if (serviceTypeCondition != null)
                    serviceTypes = serviceTypes.Match(serviceTypeCondition);
                if (serviceTypes.Length != 0)
                    r.RegisterMany(serviceTypes, implType, reuse, made, setup, ifAlreadyRegistered, serviceKey);
            },
            nonPublicServiceTypes);

        /// <summary>Registers single registration for all implemented public interfaces and base classes.</summary>
        /// <typeparam name="TImplementation">The type to get service types from.</typeparam>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="reuse">(optional) <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="made">(optional) Allow to select constructor/method to create service, specify how to inject its parameters and properties/fields.</param>
        /// <param name="setup">(optional) Factory setup, by default is <see cref="Setup.Default"/>, check <see cref="Setup"/> class for available setups.</param>
        /// <param name="ifAlreadyRegistered">(optional) policy to deal with case when service with such type and name is already registered.</param>
        /// <param name="serviceTypeCondition">(optional) Condition to select only specific service type to register.</param>
        /// <param name="nonPublicServiceTypes">(optional) Include non public service types.</param>
        /// <param name="serviceKey">(optional) service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        public static void RegisterMany<TImplementation>(this IRegistrator registrator,
            IReuse reuse = null, Made made = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null,
            Func<Type, bool> serviceTypeCondition = null, bool nonPublicServiceTypes = false,
            object serviceKey = null)
        {
            registrator.RegisterMany(new[] { typeof(TImplementation) }, (r, serviceTypes, implType) =>
            {
                if (serviceTypeCondition != null)
                    serviceTypes = serviceTypes.Match(serviceTypeCondition);
                if (serviceTypes.Length != 0)
                    r.RegisterMany(serviceTypes, implType, reuse, made, setup, ifAlreadyRegistered, serviceKey);
            },
            nonPublicServiceTypes);
        }

        /// <summary>Registers single registration for all implemented public interfaces and base classes.</summary>
        /// <typeparam name="TMadeResult">The type returned by Made factory expression.</typeparam>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="made">Made specified with strongly-typed factory expression.</param>
        /// <param name="reuse">(optional) <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="setup">(optional) Factory setup, by default is <see cref="Setup.Default"/>, check <see cref="Setup"/> class for available setups.</param>
        /// <param name="ifAlreadyRegistered">(optional) policy to deal with case when service with such type and name is already registered.</param>
        /// <param name="serviceTypeCondition">(optional) Condition to select only specific service type to register.</param>
        /// <param name="nonPublicServiceTypes">(optional) Include non public service types.</param>
        /// <param name="serviceKey">(optional) service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        public static void RegisterMany<TMadeResult>(this IRegistrator registrator, Made.TypedMade<TMadeResult> made,
            IReuse reuse = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null,
            Func<Type, bool> serviceTypeCondition = null, bool nonPublicServiceTypes = false,
            object serviceKey = null) =>
            registrator.RegisterMany<TMadeResult>(reuse, made.ThrowIfNull(), setup,
                ifAlreadyRegistered, serviceTypeCondition, nonPublicServiceTypes, serviceKey);

        /// <summary>Registers many implementations with their auto-figured service types.</summary>
        /// <param name="registrator">Registrator/Container to register with.</param>
        /// <param name="implTypeAssemblies">Assemblies with implementation/service types to register.</param>
        /// <param name="action">(optional) User specified registration action:
        /// may be used to filter registrations or specify non-default registration options, e.g. Reuse or ServiceKey, etc..</param>
        /// <param name="nonPublicServiceTypes">(optional) Include non public service types.</param>
        public static void RegisterMany(this IRegistrator registrator, IEnumerable<Assembly> implTypeAssemblies,
            RegisterManyAction action = null, bool nonPublicServiceTypes = false)
        {
            var implTypes = implTypeAssemblies.ThrowIfNull().SelectMany(GetImplementationTypes);
            registrator.RegisterMany(implTypes, action, nonPublicServiceTypes);
        }

        // todo: Add overload to specify list of service types to support case when 
        // I know contracts (service types) and provide implementation locations (assemblies)
        // and do not care about concrete implementation which is good principle.
        /// <summary>Registers many implementations with their auto-figured service types.</summary>
        /// <param name="registrator">Registrator/Container to register with.</param>
        /// <param name="implTypeAssemblies">Assemblies with implementation/service types to register.</param>
        /// <param name="serviceTypeCondition">Condition to select only specific service type to register.</param>
        /// <param name="reuse">(optional) Reuse to apply to all service registrations.</param>
        /// <param name="made">(optional) Allow to select constructor/method to create service, specify how to inject its parameters and properties/fields.</param>
        /// <param name="setup">(optional) Factory setup, by default is <see cref="Setup.Default"/>, check <see cref="Setup"/> class for available setups.</param>
        /// <param name="ifAlreadyRegistered">(optional) Policy to deal with existing service registrations.</param>
        /// <param name="nonPublicServiceTypes">(optional) Include non public service types.</param>
        /// <param name="serviceKey">(optional) service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        public static void RegisterMany(this IRegistrator registrator,
            IEnumerable<Assembly> implTypeAssemblies, Func<Type, bool> serviceTypeCondition,
            IReuse reuse = null, Made made = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null,
            bool nonPublicServiceTypes = false, object serviceKey = null)
        {
            var implTypes = implTypeAssemblies.ThrowIfNull().SelectMany(GetImplementationTypes);
            registrator.RegisterMany(implTypes,
                reuse, made, setup, ifAlreadyRegistered, serviceTypeCondition, nonPublicServiceTypes, serviceKey);
        }

        /// <summary>Registers a factory delegate for creating an instance of <typeparamref name="TService"/>.
        /// Delegate can use resolver context parameter to resolve any required dependencies, e.g.:
        /// <code lang="cs"><![CDATA[container.RegisterDelegate<ICar>(r => new Car(r.Resolve<IEngine>()))]]></code></summary>
        /// <typeparam name="TService">The type of service.</typeparam>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="factoryDelegate">The delegate used to create a instance of <typeparamref name="TService"/>.</param>
        /// <param name="reuse">(optional) <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="setup">(optional) factory setup, by default is (<see cref="Setup.Default"/>)</param>
        /// <param name="ifAlreadyRegistered">(optional) policy to deal with case when service with such type and name is already registered.</param>
        /// <param name="serviceKey">(optional). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <remarks>IMPORTANT: The method should be used as the last resort only! Though powerful it is a black-box for container,
        /// which prevents diagnostics, plus it is easy to get memory leaks (due variables captured in delegate closure),
        /// and impossible to use in compile-time scenarios.
        /// Consider using <see cref="Made"/> instead:
        /// <code lang="cs"><![CDATA[container.Register<ICar>(Made.Of(() => new Car(Arg.Of<IEngine>())))]]></code>.
        /// </remarks>
        public static void RegisterDelegate<TService>(this IRegistrator registrator, Func<IResolverContext, TService> factoryDelegate,
            IReuse reuse = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null,
            object serviceKey = null) =>
            registrator.Register(new DelegateFactory(r => factoryDelegate(r), reuse, setup),
                typeof(TService), serviceKey, ifAlreadyRegistered, isStaticallyChecked: false);

        /// <summary>Registers a factory delegate for creating an instance of <paramref name="serviceType"/>.
        /// Delegate can use resolver context parameter to resolve any required dependencies, e.g.:
        /// <code lang="cs"><![CDATA[container.RegisterDelegate<ICar>(r => new Car(r.Resolve<IEngine>()))]]></code></summary>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="serviceType">Service type to register.</param>
        /// <param name="factoryDelegate">The delegate used to create a instance of <paramref name="serviceType"/>.</param>
        /// <param name="reuse">(optional) <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="setup">(optional) factory setup, by default is (<see cref="Setup.Default"/>)</param>
        /// <param name="ifAlreadyRegistered">(optional) policy to deal with case when service with such type and name is already registered.</param>
        /// <param name="serviceKey">(optional) Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <remarks>IMPORTANT: The method should be used as the last resort only! Though powerful it is a black-box for container,
        /// which prevents diagnostics, plus it is easy to get memory leaks (due variables captured in delegate closure),
        /// and impossible to use in compile-time scenarios.
        /// Consider using <see cref="Made"/> instead:
        /// <code lang="cs"><![CDATA[container.Register<ICar>(Made.Of(() => new Car(Arg.Of<IEngine>())))]]></code>.
        /// </remarks>
        public static void RegisterDelegate(this IRegistrator registrator,
            Type serviceType, Func<IResolverContext, object> factoryDelegate,
            IReuse reuse = null, Setup setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null,
            object serviceKey = null)
        {
            if (serviceType.IsOpenGeneric())
                Throw.It(Error.RegisteringOpenGenericRequiresFactoryProvider, serviceType);
            FactoryDelegate checkedDelegate = r => factoryDelegate(r)
                .ThrowIfNotOf(serviceType, Error.RegedFactoryDlgResultNotOfServiceType, r);
            var factory = new DelegateFactory(checkedDelegate, reuse, setup);
            registrator.Register(factory, serviceType, serviceKey, ifAlreadyRegistered, false);
        }

        /// <summary>Registers decorator function that gets decorated value as input and returns decorator.
        /// Note: Delegate decorator will use <see cref="Reuse"/> of decoratee service.</summary>
        /// <typeparam name="TService">Registered service type to decorate.</typeparam>
        /// <param name="registrator">Registrator/Container.</param>
        /// <param name="getDecorator">Delegate returning decorating function.</param>
        /// <param name="condition">(optional) Condition for decorator application.</param>
        public static void RegisterDelegateDecorator<TService>(this IRegistrator registrator,
            Func<IResolverContext, Func<TService, TService>> getDecorator, Func<Request, bool> condition = null)
        {
            getDecorator.ThrowIfNull();

            // unique key to binds decorator factory and decorator registrations
            var factoryKey = new object();

            registrator.RegisterDelegate(_ =>
                new DecoratorDelegateFactory<TService>(getDecorator),
                serviceKey: factoryKey);

            registrator.Register(Made.Of(
                _ => ServiceInfo.Of<DecoratorDelegateFactory<TService>>(serviceKey: factoryKey),
                f => f.Decorate(Arg.Of<TService>(), Arg.Of<IResolverContext>())),
                setup: Setup.DecoratorWith(condition, useDecorateeReuse: true));
        }

        internal sealed class DecoratorDelegateFactory<TDecoratee>
        {
            private readonly Func<IResolverContext, Func<TDecoratee, TDecoratee>> _getDecorator;

            public DecoratorDelegateFactory(Func<IResolverContext, Func<TDecoratee, TDecoratee>> getDecorator)
            {
                _getDecorator = getDecorator;
            }

            public TDecoratee Decorate(TDecoratee decoratee, IResolverContext r) => _getDecorator(r)(decoratee);
        }

        /// <summary>Obsolete: replaced with UseInstance</summary>
        [Obsolete("Replace by UseInstance", false)]
        public static void RegisterInstance(this IResolverContext r, Type serviceType, object instance,
            IReuse ignored = null, IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.AppendNotKeyed,
            bool preventDisposal = false, bool weaklyReferenced = false, object serviceKey = null) =>
            r.UseInstance(serviceType, instance, ifAlreadyRegistered, preventDisposal, weaklyReferenced, serviceKey);

        /// <summary>Obsolete: replaced with UseInstance</summary>
        [Obsolete("Replace by UseInstance", false)]
        public static void RegisterInstance<TService>(this IResolverContext r, TService instance,
            IReuse reuse = null, IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.AppendNotKeyed,
            bool preventDisposal = false, bool weaklyReferenced = false, object serviceKey = null) =>
            r.RegisterInstance(typeof(TService), instance, reuse, ifAlreadyRegistered, preventDisposal, weaklyReferenced, serviceKey);

        /// <summary>Stores the externally created instance into open scope or singleton,
        /// replacing the existing registration and instance if any.</summary>
        /// <typeparam name="TService">Specified instance type. May be a base type or interface of instance actual type.</typeparam>
        /// <param name="r">Container to register</param>
        /// <param name="instance">Instance to register</param>
        /// <param name="preventDisposal">(optional) Prevents disposing of disposable instance by container.</param>
        /// <param name="weaklyReferenced">(optional)Stores the weak reference to instance, allowing to GC it.</param>
        /// <param name="serviceKey">(optional) Service key to identify instance from many.</param>
        public static void UseInstance<TService>(this IResolverContext r, TService instance,
            bool preventDisposal = false, bool weaklyReferenced = false, object serviceKey = null) =>
            r.UseInstance(typeof(TService), instance, IfAlreadyRegistered.Replace, preventDisposal, weaklyReferenced, serviceKey);

        /// <summary>Stores the externally created instance into open scope or singleton,
        /// replacing the existing registration and instance if any.</summary>
        /// <param name="r">Context</param>
        /// <param name="serviceType">Runtime service type to register instance with</param>
        /// <param name="instance">Instance to register</param>
        /// <param name="preventDisposal">(optional) Prevents disposing of disposable instance by container.</param>
        /// <param name="weaklyReferenced">(optional)Stores the weak reference to instance, allowing to GC it.</param>
        /// <param name="serviceKey">(optional) Service key to identify instance from many.</param>
        public static void UseInstance(this IResolverContext r, Type serviceType, object instance,
            bool preventDisposal = false, bool weaklyReferenced = false, object serviceKey = null) =>
            r.UseInstance(serviceType, instance, IfAlreadyRegistered.Replace, preventDisposal, weaklyReferenced, serviceKey);

        /// <summary>Stores the externally created instance into open scope or singleton,
        /// replacing the existing registration and instance if any.</summary>
        /// <typeparam name="TService">Specified instance type. May be a base type or interface of instance actual type.</typeparam>
        /// <param name="r">Context</param>
        /// <param name="instance">Instance to register</param>
        /// <param name="ifAlreadyRegistered">The default is <see cref="IfAlreadyRegistered.Replace"/>.</param>
        /// <param name="preventDisposal">(optional) Prevents disposing of disposable instance by container.</param>
        /// <param name="weaklyReferenced">(optional)Stores the weak reference to instance, allowing to GC it.</param>
        /// <param name="serviceKey">(optional) Service key to identify instance from many.</param>
        public static void UseInstance<TService>(this IResolverContext r, TService instance, IfAlreadyRegistered ifAlreadyRegistered,
            bool preventDisposal = false, bool weaklyReferenced = false, object serviceKey = null) =>
            r.UseInstance(typeof(TService), instance, ifAlreadyRegistered, preventDisposal, weaklyReferenced, serviceKey);

        /// <summary>Stores the externally created instance into open scope or singleton,
        /// replacing the existing registration and instance if any.</summary>
        /// <param name="r">Context</param>
        /// <param name="serviceType">Runtime service type to register instance with</param>
        /// <param name="instance">Instance to register</param>
        /// <param name="ifAlreadyRegistered">The default is <see cref="IfAlreadyRegistered.Replace"/>.</param>
        /// <param name="preventDisposal">(optional) Prevents disposing of disposable instance by container.</param>
        /// <param name="weaklyReferenced">(optional)Stores the weak reference to instance, allowing to GC it.</param>
        /// <param name="serviceKey">(optional) Service key to identify instance from many.</param>
        public static void UseInstance(this IResolverContext r, Type serviceType, object instance, IfAlreadyRegistered ifAlreadyRegistered,
            bool preventDisposal = false, bool weaklyReferenced = false, object serviceKey = null) =>
            r.UseInstance(serviceType, instance, ifAlreadyRegistered, preventDisposal, weaklyReferenced, serviceKey);

        /// <summary>Registers initializing action that will be called after service is resolved 
        /// just before returning it to the caller.  You can register multiple initializers for single service.
        /// Or you can register initializer for <see cref="Object"/> type to be applied 
        /// for all services and use <paramref name="condition"/> to specify the target services.</summary>
        /// <typeparam name="TTarget">Any type implemented by requested service type including service type itself and object type.</typeparam>
        /// <param name="registrator">Usually is <see cref="Container"/> object.</param>
        /// <param name="initialize">Delegate with <typeparamref name="TTarget"/> object and
        /// <see cref="IResolver"/> to resolve additional services required by initializer.</param>
        /// <param name="condition">(optional) Additional condition to select required target.</param>
        public static void RegisterInitializer<TTarget>(this IRegistrator registrator,
            Action<TTarget, IResolver> initialize, Func<Request, bool> condition = null)
        {
            initialize.ThrowIfNull();
            registrator.Register<object>(
                made: Made.Of(r => _initializerMethod.MakeGenericMethod(typeof(TTarget), r.ServiceType),
                parameters: Parameters.Of
                    .Type<IResolver>(r => // specify as parameter to prevent applying initializer for injected resolver too
                        r.IsSingletonOrDependencyOfSingleton ? r.Container.RootOrSelf() : r.Container)
                    .Type(_ => initialize)),
                setup: Setup.DecoratorWith(
                    useDecorateeReuse: true,
                    condition: r =>
                        r.ServiceType.IsAssignableTo(typeof(TTarget)) &&
                        (condition == null || condition(r))));
        }

        private static readonly MethodInfo _initializerMethod =
            typeof(Registrator).Method(nameof(Initializer), includeNonPublic: true);

        internal static TService Initializer<TTarget, TService>(
            TService service, IResolver resolver, Action<TTarget, IResolver> initialize) where TService : TTarget
        {
            initialize(service, resolver);
            return service;
        }

        /// <summary>Registers dispose action for reused target service.</summary>
        /// <typeparam name="TService">Target service type.</typeparam>
        /// <param name="registrator">Registrator to use.</param>
        /// <param name="dispose">Actual dispose action to be invoke when scope is disposed.</param>
        /// <param name="condition">(optional) Additional way to identify the service.</param>
        public static void RegisterDisposer<TService>(this IRegistrator registrator,
            Action<TService> dispose, Func<Request, bool> condition = null)
        {
            dispose.ThrowIfNull();

            var disposerKey = new object();

            registrator.RegisterDelegate(_ => new Disposer<TService>(dispose),
                serviceKey: disposerKey,
                setup: Setup.With(useParentReuse: true));

            registrator.Register(Made.Of(
                r => ServiceInfo.Of<Disposer<TService>>(serviceKey: disposerKey),
                f => f.TrackForDispose(Arg.Of<TService>())),
                setup: Setup.DecoratorWith(condition, useDecorateeReuse: true));
        }

        internal sealed class Disposer<T> : IDisposable
        {
            private readonly Action<T> _dispose;
            private int _state;
            private const int Tracked = 1, Disposed = 2;
            private T _item;

            public Disposer(Action<T> dispose)
            {
                _dispose = dispose.ThrowIfNull();
            }

            public T TrackForDispose(T item)
            {
                if (Interlocked.CompareExchange(ref _state, Tracked, 0) != 0)
                    Throw.It(Error.Of("Something is {0} already."), _state == Tracked ? " tracked" : "disposed");
                _item = item;
                return item;
            }

            public void Dispose()
            {
                if (Interlocked.CompareExchange(ref _state, Disposed, Tracked) != Tracked)
                    return;
                var item = _item;
                if (item != null)
                {
                    _dispose(item);
                    _item = default(T);
                }
            }
        }

        /// <summary>Returns true if <paramref name="serviceType"/> is registered in container or its open generic definition is registered in container.</summary>
        /// <param name="registrator">Usually <see cref="Container"/> to explore or any other <see cref="IRegistrator"/> implementation.</param>
        /// <param name="serviceType">The type of the registered service.</param>
        /// <param name="serviceKey">(optional) Identifies registration via service key.
        /// Not provided or <c>null</c> service key means to check the <paramref name="serviceType"/> alone with any service key.</param>
        /// <param name="factoryType">(optional) factory type to lookup, <see cref="FactoryType.Service"/> by default.</param>
        /// <param name="condition">(optional) condition to specify what registered factory do you expect.</param>
        /// <returns>True if <paramref name="serviceType"/> is registered, false - otherwise.</returns>
        public static bool IsRegistered(this IRegistrator registrator, Type serviceType,
            object serviceKey = null, FactoryType factoryType = FactoryType.Service, Func<Factory, bool> condition = null) =>
            registrator.IsRegistered(serviceType, serviceKey, factoryType, condition);

        /// <summary>Returns true if <typeparamref name="TService"/> type is registered in container or its open generic definition is registered in container.</summary>
        /// <typeparam name="TService">The type of service.</typeparam>
        /// <param name="registrator">Usually <see cref="Container"/> to explore or any other <see cref="IRegistrator"/> implementation.</param>
        /// <param name="serviceKey">(optional) Identifies registration via service key.
        /// Not provided or <c>null</c> service key means to check the <typeparamref name="TService"/> alone with any service key.</param>
        /// <param name="factoryType">(optional) factory type to lookup, <see cref="FactoryType.Service"/> by default.</param>
        /// <param name="condition">(optional) condition to specify what registered factory do you expect.</param>
        /// <returns>True if <typeparamref name="TService"/> name="serviceType"/> is registered, false - otherwise.</returns>
        public static bool IsRegistered<TService>(this IRegistrator registrator,
            object serviceKey = null, FactoryType factoryType = FactoryType.Service, Func<Factory, bool> condition = null) =>
            registrator.IsRegistered(typeof(TService), serviceKey, factoryType, condition);

        /// <summary>Removes specified registration from container.</summary>
        /// <param name="registrator">Usually <see cref="Container"/> to explore or any other <see cref="IRegistrator"/> implementation.</param>
        /// <param name="serviceType">Type of service to remove.</param>
        /// <param name="serviceKey">(optional) Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="factoryType">(optional) Factory type to lookup, <see cref="FactoryType.Service"/> by default.</param>
        /// <param name="condition">(optional) Condition for Factory to be removed.</param>
        public static void Unregister(this IRegistrator registrator, Type serviceType,
            object serviceKey = null, FactoryType factoryType = FactoryType.Service, Func<Factory, bool> condition = null) =>
            registrator.Unregister(serviceType, serviceKey, factoryType, condition);

        /// <summary>Removes specified registration from container.</summary>
        /// <typeparam name="TService">The type of service to remove.</typeparam>
        /// <param name="registrator">Usually <see cref="Container"/> or any other <see cref="IRegistrator"/> implementation.</param>
        /// <param name="serviceKey">(optional) Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="factoryType">(optional) Factory type to lookup, <see cref="FactoryType.Service"/> by default.</param>
        /// <param name="condition">(optional) Condition for Factory to be removed.</param>
        public static void Unregister<TService>(this IRegistrator registrator,
            object serviceKey = null, FactoryType factoryType = FactoryType.Service, Func<Factory, bool> condition = null) =>
            registrator.Unregister(typeof(TService), serviceKey, factoryType, condition);
    }

    /// <summary>Extension methods for <see cref="IResolver"/>.</summary>
    public static class Resolver
    {
        internal static readonly MethodInfo ResolveMethod =
            typeof(IResolver).Method(nameof(IResolver.Resolve), typeof(Type), typeof(object),
                typeof(IfUnresolved), typeof(Type), typeof(RequestInfo), typeof(object[]));

        internal static readonly MethodInfo ResolveManyMethod =
            typeof(IResolver).Method(nameof(IResolver.ResolveMany));

        /// <summary>Resolves instance of service type from container. Throws exception if unable to resolve.</summary>
        public static object Resolve(this IResolver resolver, Type serviceType) =>
            resolver.Resolve(serviceType, IfUnresolved.Throw);

        /// <summary>Resolves instance of service type from container.</summary>
        public static object Resolve(this IResolver resolver, Type serviceType, IfUnresolved ifUnresolved) =>
            resolver.Resolve(serviceType, ifUnresolved);

        /// <summary>Resolves instance of type TService from container.</summary>
        public static TService Resolve<TService>(this IResolver resolver,
            IfUnresolved ifUnresolved = IfUnresolved.Throw) =>
            (TService)resolver.Resolve(typeof(TService), ifUnresolved);

        /// <summary>Tries to resolve instance of service type from container.</summary>
        public static object Resolve(this IResolver resolver, Type serviceType, bool ifUnresolvedReturnDefault) =>
            resolver.Resolve(serviceType, ifUnresolvedReturnDefault ? IfUnresolved.ReturnDefault : IfUnresolved.Throw);

        /// <summary>Tries to resolve instance of TService from container.</summary>
        public static object Resolve<TService>(this IResolver resolver, bool ifUnresolvedReturnDefault) =>
            resolver.Resolve(typeof(TService), ifUnresolvedReturnDefault);

        /// <summary>Returns instance of <paramref name="serviceType"/> searching for <paramref name="requiredServiceType"/>.
        /// In case of <paramref name="serviceType"/> being generic wrapper like Func, Lazy, IEnumerable, etc. 
        /// <paramref name="requiredServiceType"/> allow you to specify wrapped service type.</summary>
        /// <example><code lang="cs"><![CDATA[
        ///     container.Register<IService, Service>();
        ///     var services = container.Resolve(typeof(IEnumerable<object>), typeof(IService));
        /// ]]></code></example>
        public static object Resolve(this IResolver resolver, Type serviceType, Type requiredServiceType,
            IfUnresolved ifUnresolved = IfUnresolved.Throw, object[] args = null, object serviceKey = null) =>
            resolver.Resolve(serviceType, serviceKey, ifUnresolved, requiredServiceType, RequestInfo.Empty, args);

        /// <summary>Returns instance of <typeparamref name="TService"/> searching for <paramref name="requiredServiceType"/>.
        /// In case of <typeparamref name="TService"/> being generic wrapper like Func, Lazy, IEnumerable, etc. 
        /// <paramref name="requiredServiceType"/> allow you to specify wrapped service type.</summary>
        /// <example><code lang="cs"><![CDATA[
        ///     container.Register<IService, Service>();
        ///     var services = container.Resolve<IEnumerable<object>>(typeof(IService));
        /// ]]></code></example>
        public static TService Resolve<TService>(this IResolver resolver, Type requiredServiceType,
            IfUnresolved ifUnresolved = IfUnresolved.Throw, object[] args = null, object serviceKey = null) =>
            (TService)resolver.Resolve(typeof(TService), serviceKey, ifUnresolved, requiredServiceType, RequestInfo.Empty, args);

        /// <summary>Returns instance of <typeparamref name="TService"/> searching for <typeparamref name="TRequiredService"/>.
        /// In case of <typeparamref name="TService"/> being generic wrapper like Func, Lazy, IEnumerable, etc. 
        /// <typeparamref name="TRequiredService"/> allow you to specify wrapped service type.</summary>
        /// <example><code lang="cs"><![CDATA[
        ///     container.Register<IService, Service>();
        ///     var services = container.Resolve<IEnumerable<object>, IService>();
        /// ]]></code></example>
        public static TService Resolve<TService, TRequiredService>(this IResolver resolver,
            IfUnresolved ifUnresolved = IfUnresolved.Throw, object[] args = null, object serviceKey = null) =>
            (TService)resolver.Resolve(typeof(TService), serviceKey, ifUnresolved, typeof(TRequiredService),
                RequestInfo.Empty, args);

        /// <summary>Returns instance of <paramref name="serviceType"/> searching for <paramref name="requiredServiceType"/>.
        /// In case of <paramref name="serviceType"/> being generic wrapper like Func, Lazy, IEnumerable, etc., <paramref name="requiredServiceType"/>
        /// could specify wrapped service type.</summary>
        /// <remarks>Using <paramref name="requiredServiceType"/> implicitly support Covariance for generic wrappers even in .Net 3.5.</remarks>
        /// <example><code lang="cs"><![CDATA[
        ///     container.Register<IService, Service>();
        ///     var services = container.Resolve(typeof(Lazy<object>), "someKey", requiredServiceType: typeof(IService));
        /// ]]></code></example>
        public static object Resolve(this IResolver resolver, Type serviceType, object serviceKey,
            IfUnresolved ifUnresolved = IfUnresolved.Throw, Type requiredServiceType = null,
            object[] args = null) =>
            resolver.Resolve(serviceType, serviceKey, ifUnresolved, requiredServiceType,
                RequestInfo.Empty, args);

        /// <summary>Returns instance of <typepsaramref name="TService"/> type.</summary>
        /// <typeparam name="TService">The type of the requested service.</typeparam>
        /// <returns>The requested service instance.</returns>
        /// <remarks>Using <paramref name="requiredServiceType"/> implicitly support Covariance for generic wrappers even in .Net 3.5.</remarks>
        public static TService Resolve<TService>(this IResolver resolver, object serviceKey,
            IfUnresolved ifUnresolved = IfUnresolved.Throw, Type requiredServiceType = null,
            object[] args = null) =>
            (TService)resolver.Resolve(typeof(TService), serviceKey, ifUnresolved, requiredServiceType,
                RequestInfo.Empty, args);

        /// <summary>Resolves the service supplying all or some of its dependencies 
        /// (including nested) with the <paramref name="args"/>. The rest of dependencies is injected from
        /// container.</summary>
        public static object Resolve(this IResolver resolver, Type serviceType, object[] args,
            IfUnresolved ifUnresolved = IfUnresolved.Throw, Type requiredServiceType = null,
            object serviceKey = null) =>
            resolver.Resolve(serviceType, serviceKey, ifUnresolved, requiredServiceType,
                RequestInfo.Empty, args);

        /// <summary>Resolves the service supplying all or some of its dependencies 
        /// (including nested) with the <paramref name="args"/>. The rest of dependencies is injected from
        /// container.</summary>
        public static TService Resolve<TService>(this IResolver resolver, object[] args,
            IfUnresolved ifUnresolved = IfUnresolved.Throw, Type requiredServiceType = null,
            object serviceKey = null) =>
            (TService)resolver.Resolve(typeof(TService), serviceKey, ifUnresolved, requiredServiceType,
                RequestInfo.Empty, args);

        /// <summary>Returns all registered services instances including all keyed and default registrations.
        /// Use <paramref name="behavior"/> to return either all registered services at the moment of resolve (dynamic fresh view) or
        /// the same services that were returned with first <see cref="ResolveMany{TService}"/> call (fixed view).</summary>
        /// <typeparam name="TService">Return collection item type. 
        /// It denotes registered service type if <paramref name="requiredServiceType"/> is not specified.</typeparam>
        /// <remarks>The same result could be achieved by directly calling:
        /// <code lang="cs"><![CDATA[
        ///     container.Resolve<LazyEnumerable<IService>>();  // for dynamic result - default behavior
        ///     container.Resolve<IService[]>();                // for fixed array
        ///     container.Resolve<IEnumerable<IService>>();     // same as fixed array
        /// ]]></code>
        /// </remarks>
        public static IEnumerable<TService> ResolveMany<TService>(this IResolver resolver,
            Type requiredServiceType = null, ResolveManyBehavior behavior = ResolveManyBehavior.AsLazyEnumerable,
            object[] args = null, object serviceKey = null) =>
            behavior == ResolveManyBehavior.AsLazyEnumerable
                ? resolver.ResolveMany(typeof(TService), serviceKey, requiredServiceType, RequestInfo.Empty, args).Cast<TService>()
                : resolver.Resolve<IEnumerable<TService>>(serviceKey, IfUnresolved.Throw, requiredServiceType, args);

        /// <summary>Returns all registered services as objects, including all keyed and default registrations.</summary>
        public static IEnumerable<object> ResolveMany(this IResolver resolver, Type serviceType,
            ResolveManyBehavior behavior = ResolveManyBehavior.AsLazyEnumerable,
            object[] args = null, object serviceKey = null) =>
            resolver.ResolveMany<object>(serviceType, behavior, args, serviceKey);

        internal static readonly ConstructorInfo ResolutionScopeNameCtor = 
            typeof(ResolutionScopeName).GetTypeInfo().DeclaredConstructors.First();

        // todo: what is this isRuntimeDependency, consult #517
        internal static Expr CreateResolutionExpression(Request request,
            bool openResolutionScope = false, bool isRuntimeDependency = false)
        {
            request.ContainsNestedResolutionCall = true;

            var container = request.Container;

            if (!isRuntimeDependency &&
                container.Rules.DependencyResolutionCallExpressions != null)
                PopulateDependencyResolutionCallExpressions(request);

            var serviceTypeExpr = Constant(request.ServiceType, typeof(Type));
            var ifUnresolvedExpr = Constant(request.IfUnresolved, typeof(IfUnresolved));
            var requiredServiceTypeExpr = Constant(request.RequiredServiceType, typeof(Type));
            var serviceKeyExpr = container.GetItemExpression(request.ServiceKey, typeof(object));

            var resolverExpr = ResolverContext.GetRootOrSelfExpr(request);

            if (openResolutionScope)
            {
                // Generates code:
                // r => r.OpenScope(new ResolutionScopeName(serviceType, serviceKey)).Resolve(serviceType, serviceKey)
                var actualServiceTypeExpr = Constant(request.GetActualServiceType(), typeof(Type));
                var scopeNameExpr = New(ResolutionScopeNameCtor, actualServiceTypeExpr, serviceKeyExpr);
                var trackInParent = Constant(true);

                resolverExpr = Call(Container.ResolverContextParamExpr,
                    ResolverContext.OpenScopeMethod, scopeNameExpr, trackInParent);
            }

            // Only parent is converted to be passed to Resolve.
            // The current request is formed by rest of Resolve parameters.
            var parentRequestInfo =
                request.DirectRuntimeParent.IsEmpty
                    ? request.PreResolveParent
                    : request.DirectRuntimeParent.RequestInfo;

            var preResolveParentExpr = container.RequestInfoToExpression(
                parentRequestInfo, openResolutionScope);

            var resolveCallExpr = Call(resolverExpr, ResolveMethod, serviceTypeExpr, serviceKeyExpr,
                ifUnresolvedExpr, requiredServiceTypeExpr, preResolveParentExpr, request.GetInputArgsExpr());

            return Convert(resolveCallExpr, request.ServiceType);
        }

        private static void PopulateDependencyResolutionCallExpressions(Request request)
        {
            // Actually calls nested Resolution Call and stores produced expression in collection
            // if the collection to accumulate call expressions is defined and:
            // 1. Resolve call is the first nested in chain
            // 2. Resolve call is not repeated for recursive dependency, e.g. new A(new Lazy<r => r.Resolve<B>()>) and new B(new A())
            var preResolveParent = request.PreResolveParent;
            if (!preResolveParent.IsEmpty &&
                (request.DirectRuntimeParent.IsEmpty || 
                preResolveParent.EqualsWithoutParent(request.DirectRuntimeParent)))
                return;

            var container = request.Container;

            var newRequest = Request.Create(container, request.ServiceType, request.ServiceKey,
                request.IfUnresolved, request.RequiredServiceType, request.DirectParent);

            var factory = container.ResolveFactory(newRequest);
            if (factory == null || factory is FactoryPlaceholder)
                return;

            var factoryExpr = factory.GetExpressionOrDefault(newRequest);
            if (factoryExpr == null)
                return;

            container.Rules.DependencyResolutionCallExpressions
                .Swap(it => it.AddOrUpdate(newRequest.RequestInfo,
                    Container.OptimizeExpression(factoryExpr).ToSystemExpression()));
        }
    }

    /// <summary>Specifies result of <see cref="Resolver.ResolveMany{TService}"/>: either dynamic(lazy) or fixed view.</summary>
    public enum ResolveManyBehavior
    {
        /// <summary>Lazy/dynamic item resolve.</summary>
        AsLazyEnumerable,
        /// <summary>Fixed array of item at time of resolve, newly registered/removed services won't be listed.</summary>
        AsFixedArray
    }

    /// <summary>Provides information required for service resolution: service type,
    /// and optional <see cref="ServiceDetails"/>: key, what to do if service unresolved, and required service type.</summary>
    public interface IServiceInfo
    {
        /// <summary>The required piece of info: service type.</summary>
        Type ServiceType { get; }

        /// <summary>Additional optional details: service key, if-unresolved policy, required service type.</summary>
        ServiceDetails Details { get; }

        /// <summary>Creates info from service type and details.</summary>
        /// <param name="serviceType">Required service type.</param> <param name="details">Optional details.</param> <returns>Create info.</returns>
        IServiceInfo Create(Type serviceType, ServiceDetails details);
    }

    /// <summary>Provides optional service resolution details: service key, required service type, what return when service is unresolved,
    /// default value if service is unresolved, custom service value.</summary>
    public class ServiceDetails
    {
        /// <summary>Default details if not specified, use default setting values, e.g. <see cref="DryIoc.IfUnresolved.Throw"/></summary>
        public static readonly ServiceDetails Default = Of();

        /// <summary>Default details with <see cref="IfUnresolved.ReturnDefault"/> option.</summary>
        public static readonly ServiceDetails IfUnresolvedReturnDefault =
            Of(ifUnresolved: IfUnresolved.ReturnDefault);

        /// <summary>Default details with <see cref="IfUnresolved.ReturnDefaultIfNotRegistered"/> option.</summary>
        public static readonly ServiceDetails IfUnresolvedReturnDefaultIfNotRegistered =
            Of(ifUnresolved: IfUnresolved.ReturnDefaultIfNotRegistered);

        /// <summary>Creates new details out of provided settings, or returns default if all settings have default value.</summary>
        public static ServiceDetails Of(Type requiredServiceType = null,
            object serviceKey = null, IfUnresolved ifUnresolved = IfUnresolved.Throw,
            object defaultValue = null, string metadataKey = null, object metadata = null)
        {
            // IfUnresolved.Throw does not make sense when default value is provided, so normalizing it to ReturnDefault
            if (defaultValue != null && ifUnresolved == IfUnresolved.Throw)
                ifUnresolved = IfUnresolved.ReturnDefault;

            return new ServiceDetails(requiredServiceType, ifUnresolved,
                serviceKey, metadataKey, metadata, defaultValue, hasCustomValue: false);
        }

        /// <summary>Sets custom value for service. This setting is orthogonal to the rest.
        /// Using default value with invalid ifUnresolved.Throw option to indicate custom value.</summary>
        public static ServiceDetails Of(object value) =>
            new ServiceDetails(null, IfUnresolved.Throw, null, null, null, value, hasCustomValue: true);

        /// <summary>Service type to search in registry. Should be assignable to user requested service type.</summary>
        public readonly Type RequiredServiceType;

        /// <summary>Service key provided with registration.</summary>
        public readonly object ServiceKey;

        /// <summary>Metadata key to find in metadata dictionary in resolved service.</summary>
        public readonly string MetadataKey;

        /// <summary>Metadata value to find in resolved service.</summary>
        public readonly object Metadata;

        /// <summary>Policy to deal with unresolved request.</summary>
        public readonly IfUnresolved IfUnresolved;

        /// <summary>Indicates that the custom value is specified.</summary>
        public readonly bool HasCustomValue;

        /// <summary>Either default or custom value depending on <see cref="IfUnresolved"/> setting.</summary>
        private readonly object _value;

        /// <summary>Value to use in case <see cref="IfUnresolved"/> is set to not Throw.</summary>
        public object DefaultValue => IfUnresolved != IfUnresolved.Throw ? _value : null;

        /// <summary>Custom value specified for dependency. The IfUnresolved.Throw is the marker of custom value comparing to default value.</summary>
        public object CustomValue => IfUnresolved == IfUnresolved.Throw ? _value : null;

        /// <summary>Pretty prints service details to string for debugging and errors.</summary> <returns>Details string.</returns>
        public override string ToString()
        {
            var s = new StringBuilder();

            if (HasCustomValue)
                return s.Append("{CustomValue=").Print(CustomValue ?? "null", "\"").Append("}").ToString();

            if (RequiredServiceType != null)
                s.Append("{RequiredServiceType=").Print(RequiredServiceType);
            if (ServiceKey != null)
                (s.Length == 0 ? s.Append('{') : s.Append(", ")).Append("ServiceKey=").Print(ServiceKey, "\"");
            if (MetadataKey != null || Metadata != null)
                (s.Length == 0 ? s.Append('{') : s.Append(", "))
                    .Append("Metadata=").Append(new KeyValuePair<string, object>(MetadataKey, Metadata));
            if (IfUnresolved != IfUnresolved.Throw)
                (s.Length == 0 ? s.Append('{') : s.Append(", ")).Append(IfUnresolved);
            return (s.Length == 0 ? s : s.Append('}')).ToString();
        }

        private ServiceDetails(Type requiredServiceType, IfUnresolved ifUnresolved,
            object serviceKey, string metadataKey, object metadata,
            object value, bool hasCustomValue)
        {
            RequiredServiceType = requiredServiceType;
            IfUnresolved = ifUnresolved;
            ServiceKey = serviceKey;
            MetadataKey = metadataKey;
            Metadata = metadata;
            _value = value;
            HasCustomValue = hasCustomValue;
        }
    }

    /// <summary>Contains tools for combining or propagating of <see cref="IServiceInfo"/> independent of its concrete implementations.</summary>
    public static class ServiceInfoTools
    {
        /// <summary>Creates service info with new type but keeping the details.</summary>
        /// <param name="source">Source info.</param> <param name="serviceType">New service type.</param>
        /// <returns>New info.</returns>
        public static IServiceInfo With(this IServiceInfo source, Type serviceType) =>
            source.Create(serviceType, source.Details);

        /// <summary>Creates new info with new IfUnresolved behavior or returns the original info if behavior is not different,
        /// or the passed info is not a <see cref="ServiceDetails.HasCustomValue"/>.</summary>
        /// <param name="source">Registered service type to search for.</param>
        /// <param name="ifUnresolved">New If unresolved behavior.</param>
        /// <returns>New info if the new details are different from the old one, and original info otherwise.</returns>
        public static IServiceInfo WithIfUnresolved(this IServiceInfo source, IfUnresolved ifUnresolved)
        {
            var details = source.Details;
            if (details.IfUnresolved == ifUnresolved || details.HasCustomValue)
                return source;

            if (details == ServiceDetails.Default)
                details = ifUnresolved == IfUnresolved.ReturnDefault
                    ? ServiceDetails.IfUnresolvedReturnDefault
                    : ServiceDetails.IfUnresolvedReturnDefaultIfNotRegistered;
            else
                details = ServiceDetails.Of(details.RequiredServiceType, details.ServiceKey,
                    ifUnresolved, details.DefaultValue, details.MetadataKey, details.Metadata);

            return source.Create(source.ServiceType, details);
        }

        // todo: Should be renamed or better to be removed, the whole operation should be hidden behind abstraction
        // Remove request parameter as it is not used anymore
        /// <summary>Combines service info with details: the main task is to combine service and required service type.</summary>
        /// <typeparam name="T">Type of <see cref="IServiceInfo"/>.</typeparam>
        /// <param name="serviceInfo">Source info.</param> <param name="details">Details to combine with info.</param>
        /// <param name="request">Owner request.</param> <returns>Original source or new combined info.</returns>
        public static T WithDetails<T>(this T serviceInfo, ServiceDetails details, Request request = null/*ignored*/)
            where T : IServiceInfo
        {
            details = details ?? ServiceDetails.Default;
            var sourceDetails = serviceInfo.Details;
            if (!details.HasCustomValue &&
                sourceDetails != ServiceDetails.Default &&
                sourceDetails != details)
            {
                var serviceKey = details.ServiceKey ?? sourceDetails.ServiceKey;
                var metadataKey = details.MetadataKey ?? sourceDetails.MetadataKey;
                var metadata = metadataKey == details.MetadataKey ? details.Metadata : sourceDetails.Metadata;
                var defaultValue = details.DefaultValue ?? sourceDetails.DefaultValue;

                details = ServiceDetails.Of(details.RequiredServiceType, serviceKey,
                    details.IfUnresolved, defaultValue, metadataKey, metadata);
            }

            return WithRequiredServiceType(serviceInfo, details);
        }

        internal static T WithRequiredServiceType<T>(T serviceInfo, ServiceDetails details)
            where T : IServiceInfo
        {
            var serviceType = serviceInfo.ServiceType;
            var requiredServiceType = details.RequiredServiceType;

            if (requiredServiceType != null && requiredServiceType == serviceType)
                details = ServiceDetails.Of(null,
                    details.ServiceKey, details.IfUnresolved, details.DefaultValue,
                    details.MetadataKey, details.Metadata);

            return serviceType == serviceInfo.ServiceType
                   && (details == null || details == serviceInfo.Details)
                ? serviceInfo // if service type unchanged and details absent, or details are the same return original info.
                : (T)serviceInfo.Create(serviceType, details); // otherwise: create new.
        }

        /// <summary>Enables propagation/inheritance of info between dependency and its owner:
        /// for instance <see cref="ServiceDetails.RequiredServiceType"/> for wrappers.</summary>
        /// <param name="dependency">Dependency info.</param>
        /// <param name="owner">Dependency holder/owner info.</param>
        /// <param name="container">required for <see cref="IContainer.GetWrappedType"/></param>
        /// <param name="ownerType">(optional)to be removed</param>
        /// <returns>Either input dependency info, or new info with properties inherited from the owner.</returns>
        public static IServiceInfo InheritInfoFromDependencyOwner(this IServiceInfo dependency,
            IServiceInfo owner, IContainer container, FactoryType ownerType = FactoryType.Service)
        {
            var ownerDetails = owner.Details;
            if (ownerDetails == null || ownerDetails == ServiceDetails.Default)
                return dependency;

            var dependencyDetails = dependency.Details;

            var ownerIfUnresolved = ownerDetails.IfUnresolved;
            var ifUnresolved = dependencyDetails.IfUnresolved;
            if (ownerIfUnresolved == IfUnresolved.ReturnDefault) // ReturnDefault is always inherited
                ifUnresolved = ownerIfUnresolved;

            var serviceType = dependency.ServiceType;
            var requiredServiceType = dependencyDetails.RequiredServiceType;
            var ownerRequiredServiceType = ownerDetails.RequiredServiceType;

            var serviceKey = dependencyDetails.ServiceKey;
            var metadataKey = dependencyDetails.MetadataKey;
            var metadata = dependencyDetails.Metadata;

            // Inherit some things through wrappers and decorators
            if (ownerType == FactoryType.Wrapper ||
                ownerType == FactoryType.Decorator &&
                container.GetWrappedType(serviceType, requiredServiceType).IsAssignableTo(owner.ServiceType))
            {
                if (ownerIfUnresolved == IfUnresolved.ReturnDefaultIfNotRegistered)
                    ifUnresolved = ownerIfUnresolved;

                if (serviceKey == null)
                    serviceKey = ownerDetails.ServiceKey;

                if (metadataKey == null && metadata == null)
                {
                    metadataKey = ownerDetails.MetadataKey;
                    metadata = ownerDetails.Metadata;
                }
            }

            if (ownerType != FactoryType.Service && ownerRequiredServiceType != null &&
                requiredServiceType == null) // if only dependency does not have its own
                requiredServiceType = ownerRequiredServiceType;

            if (serviceType == dependency.ServiceType && serviceKey == dependencyDetails.ServiceKey &&
                metadataKey == dependencyDetails.MetadataKey && metadata == dependencyDetails.Metadata &&
                ifUnresolved == dependencyDetails.IfUnresolved && requiredServiceType == dependencyDetails.RequiredServiceType)
                return dependency;

            if (serviceType == requiredServiceType)
                requiredServiceType = null;

            var serviceDetails = ServiceDetails.Of(requiredServiceType,
                serviceKey, ifUnresolved, dependencyDetails.DefaultValue,
                metadataKey, metadata);

            return dependency.Create(serviceType, serviceDetails);
        }

        /// <summary>Returns required service type if it is specified and assignable to service type,
        /// otherwise returns service type.</summary>
        /// <returns>The type to be used for lookup in registry.</returns>
        public static Type GetActualServiceType(this IServiceInfo info)
        {
            var requiredServiceType = info.Details.RequiredServiceType;
            return requiredServiceType != null && requiredServiceType.IsAssignableTo(info.ServiceType)
                ? requiredServiceType : info.ServiceType;
        }

        /// <summary>Appends info string representation into provided builder.</summary>
        /// <param name="s">String builder to print to.</param> <param name="info">Info to print.</param>
        /// <returns>String builder with appended info.</returns>
        public static StringBuilder Print(this StringBuilder s, IServiceInfo info)
        {
            s.Print(info.ServiceType);
            var details = info.Details.ToString();
            return details == string.Empty ? s : s.Append(' ').Append(details);
        }
    }

    /// <summary>Represents custom or resolution root service info, there is separate representation for parameter,
    /// property and field dependencies.</summary>
    public class ServiceInfo : IServiceInfo
    {
        /// <summary>Empty service info for convenience.</summary>
        public static readonly IServiceInfo Empty = new ServiceInfo(null);

        /// <summary>Creates info out of provided settings</summary>
        public static ServiceInfo Of(Type serviceType,
            IfUnresolved ifUnresolved = IfUnresolved.Throw, object serviceKey = null) =>
            Of(serviceType, null, ifUnresolved, serviceKey);

        /// <summary>Creates info out of provided settings</summary>
        public static ServiceInfo Of(Type serviceType, Type requiredServiceType,
            IfUnresolved ifUnresolved = IfUnresolved.Throw, object serviceKey = null,
            string metadataKey = null, object metadata = null)
        {
            serviceType.ThrowIfNull();

            // remove unnecessary details if service and required type are the same
            if (serviceType == requiredServiceType)
                requiredServiceType = null;

            return serviceKey == null && requiredServiceType == null
                && metadataKey == null && metadata == null
                ? (ifUnresolved == IfUnresolved.Throw ? new ServiceInfo(serviceType)
                    : ifUnresolved == IfUnresolved.ReturnDefault ? new WithDetails(serviceType, ServiceDetails.IfUnresolvedReturnDefault)
                    : new WithDetails(serviceType, ServiceDetails.IfUnresolvedReturnDefaultIfNotRegistered))
                : new WithDetails(serviceType,
                ServiceDetails.Of(requiredServiceType, serviceKey, ifUnresolved, null, metadataKey, metadata));
        }

        /// <summary>Creates service info using typed <typeparamref name="TService"/>.</summary>
        public static Typed<TService> Of<TService>(IfUnresolved ifUnresolved = IfUnresolved.Throw, object serviceKey = null) =>
            serviceKey == null && ifUnresolved == IfUnresolved.Throw
                ? new Typed<TService>()
                : new TypedWithDetails<TService>(ServiceDetails.Of(null, serviceKey, ifUnresolved));

        /// <summary>Strongly-typed version of Service Info.</summary> <typeparam name="TService">Service type.</typeparam>
        public class Typed<TService> : ServiceInfo
        {
            /// <summary>Creates service info object.</summary>
            public Typed() : base(typeof(TService)) { }
        }

        /// <summary>Type of service to create. Indicates registered service in registry.</summary>
        public Type ServiceType { get; private set; }

        /// <summary>Additional settings. If not specified uses <see cref="ServiceDetails.Default"/>.</summary>
        public virtual ServiceDetails Details => ServiceDetails.Default;

        /// <summary>Creates info from service type and details.</summary>
        public IServiceInfo Create(Type serviceType, ServiceDetails details) =>
            details == ServiceDetails.Default ? new ServiceInfo(serviceType) : new WithDetails(serviceType, details);

        /// <summary>Prints info to string using <see cref="ServiceInfoTools.Print"/>.</summary> <returns>Printed string.</returns>
        public override string ToString() =>
            new StringBuilder().Print(this).ToString();

#region Implementation

        private ServiceInfo(Type serviceType)
        {
            ServiceType = serviceType;
        }

        private class WithDetails : ServiceInfo
        {
            public override ServiceDetails Details => _details;
            public WithDetails(Type serviceType, ServiceDetails details) : base(serviceType) { _details = details; }
            private readonly ServiceDetails _details;
        }

        private class TypedWithDetails<TService> : Typed<TService>
        {
            public override ServiceDetails Details => _details;
            public TypedWithDetails(ServiceDetails details) { _details = details; }
            private readonly ServiceDetails _details;
        }

#endregion
    }

    /// <summary>Provides <see cref="IServiceInfo"/> for parameter,
    /// by default using parameter name as <see cref="IServiceInfo.ServiceType"/>.</summary>
    /// <remarks>For parameter default setting <see cref="ServiceDetails.IfUnresolved"/> is <see cref="IfUnresolved.Throw"/>.</remarks>
    public class ParameterServiceInfo : IServiceInfo
    {
        /// <summary>Creates service info from parameter alone, setting service type to parameter type,
        /// and setting resolution policy to <see cref="IfUnresolved.ReturnDefault"/> if parameter is optional.</summary>
        /// <param name="parameter">Parameter to create info for.</param>
        /// <returns>Parameter service info.</returns>
        public static ParameterServiceInfo Of(ParameterInfo parameter)
        {
            parameter.ThrowIfNull();

            var isOptional = parameter.IsOptional;
            var defaultValue = isOptional ? parameter.DefaultValue : null;
            var hasDefaultValue = defaultValue != null && parameter.ParameterType.IsTypeOf(defaultValue);

            return !isOptional
                ? new ParameterServiceInfo(parameter)
                : new WithDetails(parameter, !hasDefaultValue
                    ? ServiceDetails.IfUnresolvedReturnDefault
                    : ServiceDetails.Of(ifUnresolved: IfUnresolved.ReturnDefault, defaultValue: defaultValue));
        }

        /// <summary>Service type specified by <see cref="ParameterInfo.ParameterType"/>.</summary>
        public virtual Type ServiceType => Parameter.ParameterType;

        /// <summary>Optional service details.</summary>
        public virtual ServiceDetails Details => ServiceDetails.Default;

        /// <summary>Creates info from service type and details.</summary>
        public IServiceInfo Create(Type serviceType, ServiceDetails details) =>
            serviceType == ServiceType ? new WithDetails(Parameter, details) : new TypeWithDetails(Parameter, serviceType, details);

        /// <summary>Parameter info.</summary>
        public readonly ParameterInfo Parameter;

        /// <summary>Prints info to string using <see cref="ServiceInfoTools.Print"/>.</summary> <returns>Printed string.</returns>
        public override string ToString() =>
            new StringBuilder().Print(this).Append(" as parameter ").Print(Parameter.Name, "\"").ToString();

#region Implementation

        private ParameterServiceInfo(ParameterInfo parameter) { Parameter = parameter; }

        private class WithDetails : ParameterServiceInfo
        {
            public override ServiceDetails Details { get { return _details; } }
            public WithDetails(ParameterInfo parameter, ServiceDetails details)
                : base(parameter)
            { _details = details; }
            private readonly ServiceDetails _details;
        }

        private sealed class TypeWithDetails : WithDetails
        {
            public override Type ServiceType { get { return _serviceType; } }
            public TypeWithDetails(ParameterInfo parameter, Type serviceType, ServiceDetails details)
                : base(parameter, details)
            { _serviceType = serviceType; }
            private readonly Type _serviceType;
        }

#endregion
    }

    /// <summary>Base class for property and field dependency info.</summary>
    public abstract class PropertyOrFieldServiceInfo : IServiceInfo
    {
        /// <summary>Create member info out of provide property or field.</summary>
        /// <param name="member">Member is either property or field.</param> <returns>Created info.</returns>
        public static PropertyOrFieldServiceInfo Of(MemberInfo member) =>
            member.ThrowIfNull() is PropertyInfo
                ? (PropertyOrFieldServiceInfo)new Property((PropertyInfo)member)
                : new Field((FieldInfo)member);

        /// <summary>The required service type. It will be either <see cref="FieldInfo.FieldType"/> or <see cref="PropertyInfo.PropertyType"/>.</summary>
        public abstract Type ServiceType { get; }

        /// <summary>Optional details: service key, if-unresolved policy, required service type.</summary>
        public virtual ServiceDetails Details => ServiceDetails.IfUnresolvedReturnDefaultIfNotRegistered;

        /// <summary>Creates info from service type and details.</summary>
        /// <param name="serviceType">Required service type.</param> <param name="details">Optional details.</param> <returns>Create info.</returns>
        public abstract IServiceInfo Create(Type serviceType, ServiceDetails details);

        /// <summary>Either <see cref="PropertyInfo"/> or <see cref="FieldInfo"/>.</summary>
        public abstract MemberInfo Member { get; }

        /// <summary>Sets property or field value on provided holder object.</summary>
        /// <param name="holder">Holder of property or field.</param> <param name="value">Value to set.</param>
        public abstract void SetValue(object holder, object value);

#region Implementation

        private class Property : PropertyOrFieldServiceInfo
        {
            public override Type ServiceType { get { return _property.PropertyType; } }
            public override IServiceInfo Create(Type serviceType, ServiceDetails details)
            {
                return serviceType == ServiceType
                    ? new WithDetails(_property, details)
                    : new TypeWithDetails(_property, serviceType, details);
            }

            public override MemberInfo Member { get { return _property; } }
            public override void SetValue(object holder, object value)
            {
                _property.SetValue(holder, value, null);
            }

            public override string ToString()
            {
                return new StringBuilder().Print(this).Append(" as property ").Print(_property.Name, "\"").ToString();
            }

            private readonly PropertyInfo _property;
            public Property(PropertyInfo property)
            {
                _property = property;
            }

            private class WithDetails : Property
            {
                public override ServiceDetails Details { get { return _details; } }
                public WithDetails(PropertyInfo property, ServiceDetails details)
                    : base(property)
                { _details = details; }
                private readonly ServiceDetails _details;
            }

            private sealed class TypeWithDetails : WithDetails
            {
                public override Type ServiceType { get { return _serviceType; } }
                public TypeWithDetails(PropertyInfo property, Type serviceType, ServiceDetails details)
                    : base(property, details)
                { _serviceType = serviceType; }
                private readonly Type _serviceType;
            }
        }

        private class Field : PropertyOrFieldServiceInfo
        {
            public override Type ServiceType { get { return _field.FieldType; } }
            public override IServiceInfo Create(Type serviceType, ServiceDetails details)
            {
                return serviceType == null
                    ? new WithDetails(_field, details)
                    : new TypeWithDetails(_field, serviceType, details);
            }

            public override MemberInfo Member { get { return _field; } }
            public override void SetValue(object holder, object value)
            {
                _field.SetValue(holder, value);
            }

            public override string ToString()
            {
                return new StringBuilder().Print(this).Append(" as field ").Print(_field.Name, "\"").ToString();
            }

            private readonly FieldInfo _field;
            public Field(FieldInfo field)
            {
                _field = field;
            }

            private class WithDetails : Field
            {
                public override ServiceDetails Details { get { return _details; } }
                public WithDetails(FieldInfo field, ServiceDetails details)
                    : base(field)
                { _details = details; }
                private readonly ServiceDetails _details;
            }

            private sealed class TypeWithDetails : WithDetails
            {
                public override Type ServiceType { get { return _serviceType; } }
                public TypeWithDetails(FieldInfo field, Type serviceType, ServiceDetails details)
                    : base(field, details)
                { _serviceType = serviceType; }
                private readonly Type _serviceType;
            }
        }

#endregion
    }

    /// <summary>Stored check results of two kinds: inherited down dependency chain and not.</summary>
    [Flags]
    public enum RequestFlags
    {
        /// <summary>Not inherited</summary>
        TracksTransientDisposable = 1 << 1,
        /// <summary>Not inherited</summary>
        IsServiceCollection = 1 << 2,

        /// <summary>Inherited</summary>
        IsSingletonOrDependencyOfSingleton = 1 << 3,
        /// <summary>Inherited</summary>
        IsWrappedInFunc = 1 << 4,

        /// <summary>Non inherited</summary>
        OpensResolutionScope = 1 << 6
    }

    /// <summary>Tracks the requested service and resolved factory details in a chain of nested dependencies.</summary>
    public sealed class Request : IEnumerable<Request>
    {
        /// <summary>Not inherited down dependency chain.</summary>
        public static readonly RequestFlags NotInheritedFlags
            = RequestFlags.TracksTransientDisposable
            | RequestFlags.IsServiceCollection;

        private static readonly Request _empty =
            new Request(null, null, ServiceInfo.Empty, null, null, null, default(RequestFlags), null);

        /// <summary>Creates empty request associated with container.
        /// The shared part of request is stored in request context. Pre-request info is also store once in shared context.</summary>
        /// <param name="container">Associated container - part of request context.</param>
        /// <param name="serviceType">Service type to resolve.</param>
        /// <param name="serviceKey">(optional) Service key to resolve.</param>
        /// <param name="ifUnresolved">(optional) How to handle unresolved service.</param>
        /// <param name="requiredServiceType">(optional) Actual registered or unwrapped service type to look for.</param>
        /// <param name="preResolveParent">(optional) Request info preceding Resolve call.</param>
        /// <param name="args">(optional) Arguments provided by Resolve call.</param>
        /// <returns>New request with provided info.</returns>
        public static Request Create(IContainer container, Type serviceType,
            object serviceKey = null, IfUnresolved ifUnresolved = IfUnresolved.Throw, Type requiredServiceType = null,
            RequestInfo preResolveParent = null, object[] args = null)
        {
            serviceType.ThrowIfNull()
                .ThrowIf(serviceType.IsOpenGeneric(), Error.ResolvingOpenGenericServiceTypeIsNotPossible);

            if (preResolveParent == null)
                preResolveParent = RequestInfo.Empty;

            IServiceInfo serviceInfo = ServiceInfo.Of(serviceType, requiredServiceType, ifUnresolved, serviceKey);

            // inherit some flags and service details from parent (if any)
            var flags = default(RequestFlags);
            if (!preResolveParent.IsEmpty)
            {
                serviceInfo = serviceInfo.InheritInfoFromDependencyOwner(
                    preResolveParent.ServiceInfo, container, preResolveParent.FactoryType);

                // filter out not propagated flags
                flags = preResolveParent.Flags & ~NotInheritedFlags;
            }

            var funcArgs = args?.Map(a => Constant(a));

            var resolverContext = new RequestContext(container, preResolveParent);
            return new Request(resolverContext, _empty, serviceInfo, null, null, funcArgs, flags, null);
        }

        /// <summary>Indicates that request is empty initial request: there is no <see cref="RequestInfo"/> in such a request.</summary>
        public bool IsEmpty =>
            DirectRuntimeParent == null;

#region State carried with each request (think how to minimize it)

        /// <summary>Request parent with all runtime info available.</summary>
        public readonly Request DirectRuntimeParent;

        /// <summary>Resolved factory, initially is null.</summary>
        public readonly Factory Factory;

        /// <summary>Func input type arguments.</summary>
        /// <remarks>Mutable to track used arguments</remarks>
        public readonly Expr[] InputArgs;

        // Shared info in requests in chain.
        private readonly RequestContext _requestContext;

        private IServiceInfo _serviceInfo;

        private readonly IReuse _reuse;

        private readonly RequestFlags _flags;

        private readonly Factory _decoratedFactory;

#endregion

        /// <summary>Returns true if request is First in Resolve call.</summary>
        public bool IsResolutionCall =>
            !IsEmpty && DirectRuntimeParent.IsEmpty;

        /// <summary>Returns true if request is First in First Resolve call.</summary>
        public bool IsResolutionRoot =>
            IsResolutionCall && PreResolveParent.IsEmpty;

        /// <summary>Returns true if request is First in First Resolve call.</summary>
        public bool OpensResolutionScope =>
            IsResolutionCall && (PreResolveParent.Flags & RequestFlags.OpensResolutionScope) != 0;

        /// <summary>Request prior to Resolve call.</summary>
        public RequestInfo PreResolveParent =>
            _requestContext.PreResolveParent;

        /// <summary>Checks if request is wrapped in Func,
        ///  where Func is one of request immediate wrappers.</summary>
        /// <returns>True if has Func ancestor.</returns>
        public bool IsWrappedInFunc() =>
            (_flags & RequestFlags.IsWrappedInFunc) != 0;

        /// <summary>Checks if request has parent with service type of Func with arguments.</summary>
        public bool IsWrappedInFuncWithArgs() => InputArgs != null;

        /// <summary>Returns expression for func arguments.</summary>
        public Expr GetInputArgsExpr() =>
            InputArgs == null
            ? Constant(null, typeof(object[]))
            : (Expr)NewArrayInit(typeof(object),
                InputArgs.Map(it => it.Type.IsValueType() ? Convert(it, typeof(object)) : it));

        /// <summary>Indicates that requested service is transient disposable that should be tracked.</summary>
        public bool TracksTransientDisposable =>
            (_flags & RequestFlags.TracksTransientDisposable) != 0;

        /// <summary>Indicates the request is singleton or has singleton upper in dependency chain.</summary>
        public bool IsSingletonOrDependencyOfSingleton =>
            (_flags & RequestFlags.IsSingletonOrDependencyOfSingleton) != 0;

        /// <summary>Gathers the info from resolved dependency graph.
        /// If dependency injected <c>asResolutionCall</c> the whole graph is not cacheable (issue #416).</summary>
        /// <returns>True if contains, false - otherwise or if not known.</returns>
        public bool ContainsNestedResolutionCall
        {
            get { return _requestContext.ContainsNestedResolutionCall; }
            set { if (value) _requestContext.ContainsNestedResolutionCall = true; }
        }

        /// <summary>Provides approximate number of dependencies in resolution graph (starting from Resolve method),
        /// excluding registered delegates, instances, and wrappers.</summary>
        public int DependencyCount => _requestContext.DependencyCount;

        /// <summary>Returns true if object graph should be split due <see cref="DryIoc.Rules.MaxObjectGraphSize"/> setting.</summary>
        public bool ShouldSplitObjectGraph() =>
            FactoryType == FactoryType.Service &&
            (Rules.MaxObjectGraphSize != -1 && DependencyCount > Rules.MaxObjectGraphSize);

        /// <summary>Returns service parent of request, skipping intermediate wrappers if any.</summary>
        public RequestInfo Parent => RequestInfo.Parent;

        /// <summary>Returns direct parent either it service or not (wrapper).
        /// In comparison with logical <see cref="Parent"/> which returns first service parent skipping wrapper if any.</summary>
        public RequestInfo DirectParent => RequestInfo.DirectParent;

        /// <summary>Provides access to container currently bound to request.
        /// By default it is container initiated request by calling resolve method,
        /// but could be changed along the way: for instance when resolving from parent container.</summary>
        public IContainer Container => _requestContext.Container;

        /// <summary>Currne scope</summary>
        public IScope CurrentScope => Container.CurrentScope;

        /// <summary>Singletons</summary>
        public IScope SingletonScope => Container.SingletonScope;

        /// <summary>Shortcut to issued container rules.</summary>
        public Rules Rules => Container.Rules;

        /// <summary>(optional) Made spec used for resolving request.</summary>
        public Made Made => Factory?.Made;

        /// <summary>Current flags</summary>
        public RequestFlags Flags => _flags;

        /// <summary>Requested service type.</summary>
        public Type ServiceType => _serviceInfo.ServiceType;

        /// <summary>Optional service key to identify service of the same type.</summary>
        public object ServiceKey => _serviceInfo.Details.ServiceKey;

        /// <summary>Metadata key to find in metadata dictionary in resolved service.</summary>
        public string MetadataKey => _serviceInfo.Details.MetadataKey;

        /// <summary>Metadata or the value (if key specified) to find in resolved service.</summary>
        public object Metadata => _serviceInfo.Details.Metadata;

        /// <summary>Policy to deal with unresolved service.</summary>
        public IfUnresolved IfUnresolved => _serviceInfo.Details.IfUnresolved;

        /// <summary>Required service type if specified.</summary>
        public Type RequiredServiceType => _serviceInfo.Details.RequiredServiceType;

        /// <summary>Implementation FactoryID.</summary>
        /// <remarks>The default unassigned value of ID is 0.</remarks>
        public int FactoryID => Factory.ThrowIfNull().FactoryID;

        /// <summary>Type of factory: Service, Wrapper, or Decorator.</summary>
        public FactoryType FactoryType => Factory.ThrowIfNull().FactoryType;

        /// <summary>Service implementation type if known.</summary>
        public Type ImplementationType => Factory.ThrowIfNull().ImplementationType;

        /// <summary>Service reuse.</summary>
        public IReuse Reuse => _reuse;

        /// <summary>Relative number representing reuse lifespan.</summary>
        public int ReuseLifespan =>
            Reuse == null ? 0 : Reuse.Lifespan;

        /// <summary>Relative number representing reuse lifespan.</summary>
        public int DecoratedFactoryID =>
            _decoratedFactory == null ? 0 : _decoratedFactory.FactoryID;

        /// <summary>Required or service type.</summary>
        public Type GetActualServiceType() =>
            _serviceInfo.GetActualServiceType();

        /// <summary>Known implementation, or otherwise actual service type.</summary>
        public Type GetKnownImplementationOrServiceType() =>
            ImplementationType ?? GetActualServiceType();

        /// <summary>Creates new request with provided info, and links current request as a parent.
        /// Allows to set some additional flags.</summary>
        /// <remarks>Existing/parent request should be resolved to factory (<see cref="WithResolvedFactory"/>), before pushing info into it.</remarks>
        public Request Push(IServiceInfo info, RequestFlags flags = default(RequestFlags))
        {
            info.ThrowIfNull();

            if (Factory == null) // the check throw condition because this.ToString is too expensive
                Throw.It(Error.PushingToRequestWithoutFactory, info, this);

            var inheritedInfo = info.InheritInfoFromDependencyOwner(_serviceInfo, Container, FactoryType);
            var inheritedFlags = _flags & ~NotInheritedFlags | flags;

            return new Request(_requestContext, this, inheritedInfo, null, null, InputArgs, inheritedFlags, null);
        }

        /// <summary>Composes service description into <see cref="IServiceInfo"/> and calls Push.
        /// Allot to set some additional flags by caller.</summary>
        public Request Push(Type serviceType,
            object serviceKey = null, IfUnresolved ifUnresolved = IfUnresolved.Throw, Type requiredServiceType = null,
            IScope scope = null, RequestInfo preResolveParent = null, RequestFlags flags = default(RequestFlags))
        {
            serviceType.ThrowIfNull()
                .ThrowIf(serviceType.IsOpenGeneric(), Error.ResolvingOpenGenericServiceTypeIsNotPossible);
            return Push(ServiceInfo.Of(serviceType, requiredServiceType, ifUnresolved, serviceKey), flags);
        }

        /// <summary>Allow to switch current service info to new one: for instance it is used be decorators.</summary>
        /// <param name="getInfo">Gets new info to switch to.</param>
        /// <returns>New request with new service info but the same implementation and context.</returns>
        public Request WithChangedServiceInfo(Func<IServiceInfo, IServiceInfo> getInfo) =>
            getInfo(_serviceInfo) == _serviceInfo ? this
                : new Request(_requestContext, DirectRuntimeParent, getInfo(_serviceInfo),
                    Factory, _reuse, InputArgs, _flags, _decoratedFactory);

        // note: Mutates the request, required for proper caching
        /// <summary>Sets service key to passed value. Required for multiple default services to change null key to
        /// actual <see cref="DefaultKey"/></summary>
        public void ChangeServiceKey(object serviceKey)
        {
            var i = _serviceInfo;
            var d = i.Details;
            var newDetails = ServiceDetails.Of(d.RequiredServiceType, serviceKey, d.IfUnresolved, d.DefaultValue);
            _serviceInfo = i.Create(i.ServiceType, newDetails);
        }

        /// <summary>Prepends input arguments ot existing arguments in request. The prepending is done because
        /// nested Func/Action input argument has a priority over outer argument.
        /// The arguments are provided by Func and Action wrappers, or by object array from Resolve call.</summary>
        public Request WithInputArgs(Expr[] argExpressions) =>
            new Request(_requestContext, DirectRuntimeParent, _serviceInfo, Factory, _reuse,
                argExpressions.Append(InputArgs), _flags, _decoratedFactory);

        /// <summary>Returns new request with set implementation details.</summary>
        /// <param name="factory">Factory to which request is resolved.</param>
        /// <param name="skipRecursiveDependencyCheck">(optional) does not check for recursive dependency.
        /// Use with caution. Make sense for Resolution expression.</param>
        /// <param name="skipCaptiveDependencyCheck">(optional) allows to skip reuse mismatch aka captive dependency check.</param>
        /// <returns>New request with set factory.</returns>
        public Request WithResolvedFactory(Factory factory,
            bool skipRecursiveDependencyCheck = false,
            bool skipCaptiveDependencyCheck = false)
        {
            if (IsEmpty)
                return this;

            Factory decoratedFactory = null;
            if (Factory != null)
            {
                // resolving only once, no need to check recursion again.
                if (Factory.FactoryID == factory.FactoryID)
                    return this;

                if (Factory.FactoryType != FactoryType.Decorator &&
                    factory.FactoryType == FactoryType.Decorator)
                    decoratedFactory = Factory;
            }

            if (factory.FactoryType == FactoryType.Service && !skipRecursiveDependencyCheck)
                for (var p = DirectRuntimeParent; !p.IsEmpty; p = p.DirectRuntimeParent)
                    if (p.FactoryID == factory.FactoryID)
                        Throw.It(Error.RecursiveDependencyDetected, Print(factory.FactoryID));

            IReuse reuse;
            if (IsWrappedInFuncWithArgs() && Rules.IgnoringReuseForFuncWithArgs)
                reuse = DryIoc.Reuse.Transient;
            else
                reuse = factory.Reuse ?? GetDefaultReuse(factory);

            if (reuse.Lifespan != 0 &&
                !skipCaptiveDependencyCheck &&
                Rules.ThrowIfDependencyHasShorterReuseLifespan)
                ThrowIfReuseHasShorterLifespanThanParent(reuse);

            var flags = _flags;
            if (reuse == DryIoc.Reuse.Singleton)
            {
                flags |= RequestFlags.IsSingletonOrDependencyOfSingleton;
            }
            else if (reuse == DryIoc.Reuse.Transient) // check for disposable transient
            {
                reuse = GetTransientDisposableTrackingReuse(factory);
                if (reuse != DryIoc.Reuse.Transient)
                    flags |= RequestFlags.TracksTransientDisposable;
            }

            _requestContext.IncrementDependencyCount();
            return new Request(_requestContext,
                DirectRuntimeParent, _serviceInfo, factory, reuse, InputArgs, flags, decoratedFactory);
        }

        private IReuse GetDefaultReuse(Factory factory)
        {
            if (factory.Setup.UseParentReuse)
                return GetFirstParentNonTransientReuseUntilFunc();

            if (factory.FactoryType == FactoryType.Decorator &&
                ((Setup.DecoratorSetup)factory.Setup).UseDecorateeReuse)
                return Reuse; // use reuse of resolved service factory for decorator

            return factory.FactoryType == FactoryType.Wrapper ? DryIoc.Reuse.Transient : Rules.DefaultReuse;
        }

        private IReuse GetTransientDisposableTrackingReuse(Factory factory)
        {
            // Track transient disposable in parent scope (if any), or open scope (if any)
            var setup = factory.Setup;
            var tracksTransientDisposable =
                !setup.PreventDisposal &&
                (setup.TrackDisposableTransient || !setup.AllowDisposableTransient && Rules.TrackingDisposableTransients) &&
                (factory.ImplementationType ?? GetActualServiceType()).IsAssignableTo(typeof(IDisposable));

            if (!tracksTransientDisposable)
                return DryIoc.Reuse.Transient;

            var parentReuse = GetFirstParentNonTransientReuseUntilFunc();
            if (parentReuse != DryIoc.Reuse.Transient)
                return parentReuse;

            if (IsWrappedInFunc())
                return DryIoc.Reuse.Transient;

            // If no parent with reuse found, then track in current open scope or in singletons scope
            return DryIoc.Reuse.ScopedOrSingleton;
        }

        private void ThrowIfReuseHasShorterLifespanThanParent(IReuse reuse)
        {
            if (!DirectRuntimeParent.IsEmpty)
                for (var p = DirectRuntimeParent; !p.IsEmpty; p = p.DirectRuntimeParent)
                {
                    if (p.OpensResolutionScope ||
                        p.FactoryType == FactoryType.Wrapper && p.GetActualServiceType().IsFunc())
                        break;

                    if (p.FactoryType == FactoryType.Service && p.ReuseLifespan > reuse.Lifespan)
                        Throw.It(Error.DependencyHasShorterReuseLifespan, PrintCurrent(), reuse, p);
                }

            if (!PreResolveParent.IsEmpty)
            {
                for (var p = PreResolveParent; !p.IsEmpty; p = p.DirectParent)
                {
                    if (p.OpensResolutionScope ||
                        p.FactoryType == FactoryType.Wrapper && p.GetActualServiceType().IsFunc())
                        break;

                    if (p.FactoryType == FactoryType.Service && p.ReuseLifespan > reuse.Lifespan)
                        Throw.It(Error.DependencyHasShorterReuseLifespan, PrintCurrent(), reuse, p);
                }
            }
        }

        private IReuse GetFirstParentNonTransientReuseUntilFunc()
        {
            if (!DirectRuntimeParent.IsEmpty)
                for (var p = DirectRuntimeParent; !p.IsEmpty; p = p.DirectRuntimeParent)
                {
                    if (p.FactoryType == FactoryType.Wrapper && p.GetActualServiceType().IsFunc())
                        return DryIoc.Reuse.Transient;

                    if (p.FactoryType != FactoryType.Wrapper && p.Reuse != DryIoc.Reuse.Transient)
                        return p.Reuse;
                }

            if (!PreResolveParent.IsEmpty)
                for (var p = PreResolveParent; !p.IsEmpty; p = p.DirectParent)
                {
                    if (p.FactoryType == FactoryType.Wrapper && p.GetActualServiceType().IsFunc())
                        return DryIoc.Reuse.Transient;

                    if (p.FactoryType != FactoryType.Wrapper && p.Reuse != DryIoc.Reuse.Transient)
                        return p.Reuse;
                }

            return DryIoc.Reuse.Transient;
        }

        /// <summary>Serializable request info stripped off run-time info.</summary>
        public RequestInfo RequestInfo
        {
            get
            {
                if (IsEmpty)
                    return PreResolveParent;

                var parentRequestInfo = DirectRuntimeParent.IsEmpty ? PreResolveParent : DirectRuntimeParent.RequestInfo;
                if (Factory == null)
                    return parentRequestInfo.Push(_serviceInfo);

                return parentRequestInfo.Push(_serviceInfo,
                    Factory.FactoryID, Factory.FactoryType, Factory.ImplementationType, _reuse, _flags,
                    DecoratedFactoryID);
            }
        }

        /// <summary>If request corresponds to dependency injected into parameter,
        /// then method calls <paramref name="parameter"/> handling and returns its result.
        /// If request corresponds to property or field, then method calls respective handler.
        /// If request does not correspond to dependency, then calls <paramref name="root"/> handler.</summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="root">(optional) handler for resolution call or root.</param>
        /// <param name="parameter">(optional) handler for parameter dependency</param>
        /// <param name="property">(optional) handler for property dependency</param>
        /// <param name="field">(optional) handler for field dependency</param>
        /// <returns>Result of applied handler or default <typeparamref name="TResult"/>.</returns>
        public TResult Is<TResult>(
            Func<TResult> root = null,
            Func<ParameterInfo, TResult> parameter = null,
            Func<PropertyInfo, TResult> property = null,
            Func<FieldInfo, TResult> field = null)
        {
            var serviceInfo = _serviceInfo;
            if (serviceInfo is ParameterServiceInfo)
            {
                if (parameter != null)
                    return parameter(((ParameterServiceInfo)serviceInfo).Parameter);
            }
            else if (serviceInfo is PropertyOrFieldServiceInfo)
            {
                var propertyOrFieldServiceInfo = (PropertyOrFieldServiceInfo)serviceInfo;
                var propertyInfo = propertyOrFieldServiceInfo.Member as PropertyInfo;
                if (propertyInfo != null)
                {
                    if (property != null)
                        return property(propertyInfo);
                }
                else if (field != null)
                    return field((FieldInfo)propertyOrFieldServiceInfo.Member);
            }
            else if (root != null)
                return root();

            return default(TResult);
        }

        /// <summary>Obsolete: now request is directly implements the <see cref="IEnumerable{T}"/>.</summary>
        public IEnumerable<Request> Enumerate() => this;

        /// <summary>Enumerates all runtime request stack parents, BUT does not got further to pre-resolve <see cref="RequestInfo"/>.</summary>
        public IEnumerator<Request> GetEnumerator()
        {
            for (var r = this; !r.IsEmpty; r = r.DirectRuntimeParent)
                yield return r;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>Prints current request info only (no parents printed) to provided builder.</summary>
        /// <param name="s">Builder to print too.</param>
        /// <returns>(optional) Builder to appended info to, or new builder if not specified.</returns>
        public StringBuilder PrintCurrent(StringBuilder s = null)
        {
            s = s ?? new StringBuilder();

            if (IsEmpty)
                return s.Append("{empty}");

            if (Factory != null)
            {
                if (_reuse != DryIoc.Reuse.Transient)
                    s.Append(Reuse is SingletonReuse ? "singleton" : "scoped").Append(' ');

                var factoryType = Factory.FactoryType;
                if (factoryType != FactoryType.Service)
                    s.Append(factoryType.ToString().ToLower()).Append(' ');

                var implType = Factory.ImplementationType;
                if (implType != null && implType != ServiceType)
                    s.Print(implType).Append(": ");
            }

            s.Append(_serviceInfo);

            if (_decoratedFactory != null)
                s.Append(" of ").Append(_decoratedFactory);

            if (InputArgs != null)
                s.AppendFormat(" with [{0}] Func args", InputArgs);

            return s;
        }

        /// <summary>Prints full stack of requests starting from current one using <see cref="PrintCurrent"/>.</summary>
        /// <param name="recursiveFactoryID">Flag specifying that in case of found recursion/repetition of requests,
        /// mark repeated requests.</param>
        /// <returns>Builder with appended request stack info.</returns>
        public StringBuilder Print(int recursiveFactoryID = -1)
        {
            if (IsEmpty)
                return new StringBuilder("<empty>");

            var s = PrintCurrent(new StringBuilder());

            s = recursiveFactoryID == -1 ? s : s.Append(" <--recursive");
            foreach (var r in DirectRuntimeParent)
            {
                s = r.PrintCurrent(s.AppendLine().Append("  in "));
                if (r.FactoryID == recursiveFactoryID)
                    s = s.Append(" <--recursive");
            }

            if (!PreResolveParent.IsEmpty)
                s = s.AppendLine().Append("  in ").Append(PreResolveParent);

            s.AppendLine().Append("  from ").Append(Container.ToString());
            return s;
        }

        /// <summary>Prints whole request chain.</summary>
        public override string ToString() => Print().ToString();

#region Implementation

        private Request(RequestContext requestContext, Request directRuntimeParent, IServiceInfo serviceInfo,
            Factory factory, IReuse reuse, Expr[] inputArgExprs, RequestFlags flags,
            Factory decoratedFactory)
        {
            _requestContext = requestContext;
            DirectRuntimeParent = directRuntimeParent;
            _serviceInfo = serviceInfo;
            Factory = factory;
            _reuse = reuse;
            InputArgs = inputArgExprs;
            _flags = flags;
            _decoratedFactory = decoratedFactory;
        }

        // Used for tracking shared state in request chain
        internal sealed class RequestContext
        {
            public readonly IContainer Container;
            public readonly RequestInfo PreResolveParent;

            // Mutable state
            public bool ContainsNestedResolutionCall;
            public int DependencyCount;

            public RequestContext(IContainer container, RequestInfo preResolveParent)
            {
                Container = container;
                PreResolveParent = preResolveParent;
            }

            public void IncrementDependencyCount() => Interlocked.Increment(ref DependencyCount);
        }

#endregion
    }

    /// <summary>Type of services supported by Container.</summary>
    public enum FactoryType
    {
        /// <summary>(default) Defines normal service factory</summary>
        Service,
        /// <summary>Defines decorator factory</summary>
        Decorator,
        /// <summary>Defines wrapper factory.</summary>
        Wrapper
    };

    /// <summary>Base class to store optional <see cref="Factory"/> settings.</summary>
    public abstract class Setup
    {
        /// <summary>Factory type is required to be specified by concrete setups as in
        /// <see cref="ServiceSetup"/>, <see cref="DecoratorSetup"/>, <see cref="WrapperSetup"/>.</summary>
        public abstract FactoryType FactoryType { get; }

        /// <summary>Predicate to check if factory could be used for resolved request.</summary>
        public Func<Request, bool> Condition { get; private set; }

        /// <summary>Arbitrary metadata object associated with Factory/Implementation, may be a dictionary of key-values.</summary>
        public virtual object Metadata => null;

        /// <summary>Returns true if passed meta key and value match the setup metadata.</summary>
        public bool MatchesMetadata(string metadataKey, object metadata)
        {
            if (metadataKey == null)
                return Equals(metadata, Metadata);

            object metaValue;
            var metaDict = Metadata as IDictionary<string, object>;
            return metaDict != null
                && metaDict.TryGetValue(metadataKey, out metaValue)
                && Equals(metadata, metaValue);
        }

        /// <summary>Indicates that injected expression should be:
        /// <c><![CDATA[r.Resolver.Resolve<IDependency>(...)]]></c>
        /// instead of: <c><![CDATA[new Dependency(...)]]></c></summary>
        public bool AsResolutionCall => (_settings & Settings.AsResolutionCall) != 0;

        internal Setup WithAsResolutionCall()
        {
            if (AsResolutionCall)
                return this;
            var copy = (Setup)MemberwiseClone();
            copy._settings |= Settings.AsResolutionCall;
            return copy;
        }

        /// <summary>Marks service (not a wrapper or decorator) registration that is expected to be resolved via Resolve call.</summary>
        public bool AsResolutionRoot => (_settings & Settings.AsResolutionRoot) != 0;

        /// <summary>Opens scope, also implies <see cref="AsResolutionCall"/>.</summary>
        public bool OpenResolutionScope => (_settings & Settings.OpenResolutionScope) != 0;

        /// <summary>Prevents disposal of reused instance if it is disposable.</summary>
        public bool PreventDisposal => (_settings & Settings.PreventDisposal) != 0;

        /// <summary>Stores reused instance as WeakReference.</summary>
        public bool WeaklyReferenced => (_settings & Settings.WeaklyReferenced) != 0;

        /// <summary>Allows registering transient disposable.</summary>
        public bool AllowDisposableTransient => (_settings & Settings.AllowDisposableTransient) != 0;

        /// <summary>Turns On tracking of disposable transient dependency in parent scope or in open scope if resolved directly.</summary>
        public bool TrackDisposableTransient => (_settings & Settings.TrackDisposableTransient) != 0;

        /// <summary>Instructs to use parent reuse. Applied only if <see cref="Factory.Reuse"/> is not specified.</summary>
        public bool UseParentReuse => (_settings & Settings.UseParentReuse) != 0;

        /// <summary>Sets the base settings.</summary>
        private Setup(Func<Request, bool> condition = null,
            bool openResolutionScope = false, bool asResolutionCall = false,
            bool asResolutionRoot = false, bool preventDisposal = false, bool weaklyReferenced = false,
            bool allowDisposableTransient = false, bool trackDisposableTransient = false,
            bool useParentReuse = false)
        {
            Condition = condition;

            if (asResolutionCall)
                _settings |= Settings.AsResolutionCall;
            if (openResolutionScope)
            {
                _settings |= Settings.OpenResolutionScope;
                _settings |= Settings.AsResolutionCall;
            }
            if (preventDisposal)
                _settings |= Settings.PreventDisposal;
            if (weaklyReferenced)
                _settings |= Settings.WeaklyReferenced;
            if (allowDisposableTransient)
                _settings |= Settings.AllowDisposableTransient;
            if (trackDisposableTransient)
            {
                _settings |= Settings.TrackDisposableTransient;
                _settings |= Settings.AllowDisposableTransient;
            }
            if (asResolutionRoot)
                _settings |= Settings.AsResolutionRoot;
            if (useParentReuse)
                _settings |= Settings.UseParentReuse;
        }

        [Flags]
        private enum Settings
        {
            AsResolutionCall = 1 << 1,
            OpenResolutionScope = 1 << 2,
            PreventDisposal = 1 << 3,
            WeaklyReferenced = 1 << 4,
            AllowDisposableTransient = 1 << 5,
            TrackDisposableTransient = 1 << 6,
            AsResolutionRoot = 1 << 7,
            UseParentReuse = 1 << 8
        }

        private Settings _settings; // note: mutable because of setting the AsResolutionCall

        /// <summary>Default setup for service factories.</summary>
        public static readonly Setup Default = new ServiceSetup();

        /// <summary>Constructs setup object out of specified settings.
        /// If all settings are default then <see cref="Default"/> setup will be returned.
        /// <paramref name="metadataOrFuncOfMetadata"/> is metadata object or Func returning metadata object.</summary>
        public static Setup With(
            object metadataOrFuncOfMetadata = null, Func<Request, bool> condition = null,
            bool openResolutionScope = false, bool asResolutionCall = false, bool asResolutionRoot = false,
            bool preventDisposal = false, bool weaklyReferenced = false,
            bool allowDisposableTransient = false, bool trackDisposableTransient = false,
            bool useParentReuse = false)
        {
            if (metadataOrFuncOfMetadata == null && condition == null &&
                openResolutionScope == false && asResolutionCall == false && asResolutionRoot == false &&
                preventDisposal == false && weaklyReferenced == false &&
                allowDisposableTransient == false && trackDisposableTransient == false &&
                useParentReuse == false)
                return Default;

            return new ServiceSetup(condition,
                metadataOrFuncOfMetadata, openResolutionScope, asResolutionCall, asResolutionRoot,
                preventDisposal, weaklyReferenced, allowDisposableTransient, trackDisposableTransient,
                useParentReuse);
        }

        /// <summary>Default setup which will look for wrapped service type as single generic parameter.</summary>
        public static readonly Setup Wrapper = new WrapperSetup();

        /// <summary>Returns generic wrapper setup.</summary>
        /// <param name="wrappedServiceTypeArgIndex">Default is -1 for generic wrapper with single type argument. Need to be set for multiple type arguments.</param>
        /// <param name="alwaysWrapsRequiredServiceType">Need to be set when generic wrapper type arguments should be ignored.</param>
        /// <param name="unwrap">(optional) Delegate returning wrapped type from wrapper type. <b>Overwrites other options.</b></param>
        /// <param name="openResolutionScope">(optional) Opens the new scope.</param>
        /// <param name="asResolutionCall">(optional) Injects decorator as resolution call.</param>
        /// <param name="preventDisposal">(optional) Prevents disposal of reused instance if it is disposable.</param>
        /// <param name="condition">(optional)</param>
        /// <returns>New setup or default <see cref="Setup.Wrapper"/>.</returns>
        public static Setup WrapperWith(int wrappedServiceTypeArgIndex = -1,
            bool alwaysWrapsRequiredServiceType = false, Func<Type, Type> unwrap = null,
            bool openResolutionScope = false, bool asResolutionCall = false, bool preventDisposal = false,
            Func<Request, bool> condition = null) 
            => wrappedServiceTypeArgIndex == -1 && !alwaysWrapsRequiredServiceType && unwrap == null
            && !openResolutionScope && !preventDisposal && condition == null
                ? Wrapper
                : new WrapperSetup(wrappedServiceTypeArgIndex, alwaysWrapsRequiredServiceType, unwrap,
                    condition, openResolutionScope, asResolutionCall, preventDisposal);

        /// <summary>Default decorator setup: decorator is applied to service type it registered with.</summary>
        public static readonly Setup Decorator = new DecoratorSetup();

        /// <summary>Creates setup with optional condition.</summary>
        /// <param name="condition">Applied to decorated service, if true then decorator is applied.</param>
        /// <param name="order">(optional) If provided specifies relative decorator position in decorators chain.</param>
        /// <param name="useDecorateeReuse">If provided specifies relative decorator position in decorators chain.
        /// Greater number means further from decoratee - specify negative number to stay closer.
        /// Decorators without order (Order is 0) or with equal order are applied in registration order
        /// - first registered are closer decoratee.</param>
        /// <param name="openResolutionScope">The decorator opens resolution scope</param>
        /// <returns>New setup with condition or <see cref="Decorator"/>.</returns>
        public static Setup DecoratorWith(Func<Request, bool> condition = null, int order = 0,
            bool useDecorateeReuse = false, bool openResolutionScope = false) =>
            condition == null && order == 0 && !useDecorateeReuse && !openResolutionScope
                ? Decorator
                : new DecoratorSetup(condition, order, useDecorateeReuse, openResolutionScope);

        /// <summary>Setup for decorator of type <paramref name="decorateeType"/>.</summary>
        public static Setup DecoratorOf(Type decorateeType = null,
            int order = 0, bool useDecorateeReuse = false, object decorateeServiceKey = null)
        {
            Func<Request, bool> condition
                = decorateeType == null
                    ? (r => r.ServiceKey == null || r.ServiceKey.Equals(null))
                : decorateeServiceKey == null
                    ? (r => r.GetKnownImplementationOrServiceType().IsAssignableTo(decorateeType))
                : (Func<Request, bool>)(r =>
                    decorateeServiceKey.Equals(r.ServiceKey) &&
                    r.GetKnownImplementationOrServiceType().IsAssignableTo(decorateeType));

            return DecoratorWith(condition, order, useDecorateeReuse);
        }

        /// <summary>Setup for decorator of type <typeparamref name="TDecoratee"/>.</summary>
        public static Setup DecoratorOf<TDecoratee>(
            int order = 0, bool useDecorateeReuse = false, object decorateeServiceKey = null) =>
            DecoratorOf(typeof(TDecoratee), order, useDecorateeReuse, decorateeServiceKey);

        /// <summary>Service setup.</summary>
        internal sealed class ServiceSetup : Setup
        {
            /// <inheritdoc />
            public override FactoryType FactoryType => FactoryType.Service;

            /// <summary>Evaluates metadata if it specified as Func of object, and replaces Func with its result!.
            /// Otherwise just returns metadata object.</summary>
            /// <remarks>Invocation of Func metadata is Not thread-safe. Please take care of that inside the Func.</remarks>
            public override object Metadata =>
                _metadataOrFuncOfMetadata is Func<object>
                    ? (_metadataOrFuncOfMetadata = ((Func<object>)_metadataOrFuncOfMetadata).Invoke())
                    : _metadataOrFuncOfMetadata;

            /// <summary>All settings are set to defaults.</summary>
            public ServiceSetup() { }

            /// <summary>Specify an individual settings</summary>
            public ServiceSetup(Func<Request, bool> condition, object metadataOrFuncOfMetadata,
                bool openResolutionScope, bool asResolutionCall, bool asResolutionRoot,
                bool preventDisposal, bool weaklyReferenced,
                bool allowDisposableTransient, bool trackDisposableTransient,
                bool useParentReuse)
                : base(condition, openResolutionScope, asResolutionCall, asResolutionRoot,
                    preventDisposal, weaklyReferenced, allowDisposableTransient, trackDisposableTransient,
                    useParentReuse)
            {
                _metadataOrFuncOfMetadata = metadataOrFuncOfMetadata;
            }

            private object _metadataOrFuncOfMetadata;
        }

        /// <summary>Setup applied for wrappers.</summary>
        internal sealed class WrapperSetup : Setup
        {
            /// <summary>Returns <see cref="DryIoc.FactoryType.Wrapper"/> type.</summary>
            public override FactoryType FactoryType => FactoryType.Wrapper;

            /// <summary>Delegate to get wrapped type from provided wrapper type.
            /// If wrapper is generic, then wrapped type is usually a generic parameter.</summary>
            public readonly int WrappedServiceTypeArgIndex;

            /// <summary>Per name.</summary>
            public readonly bool AlwaysWrapsRequiredServiceType;

            /// <summary>Delegate returning wrapped type from wrapper type. Overwrites other options.</summary>
            public readonly Func<Type, Type> Unwrap;

            /// <summary>Default setup</summary>
            /// <param name="wrappedServiceTypeArgIndex">Default is -1 for generic wrapper with single type argument.
            /// Need to be set for multiple type arguments.</param>
            public WrapperSetup(int wrappedServiceTypeArgIndex = -1)
            {
                WrappedServiceTypeArgIndex = wrappedServiceTypeArgIndex;
            }

            /// <summary>Constructs wrapper setup from optional wrapped type selector and reuse wrapper factory.</summary>
            /// <param name="wrappedServiceTypeArgIndex">Default is -1 for generic wrapper with single type argument. Need to be set for multiple type arguments.</param>
            /// <param name="alwaysWrapsRequiredServiceType">Need to be set when generic wrapper type arguments should be ignored.</param>
            /// <param name="unwrap">Delegate returning wrapped type from wrapper type.  Overwrites other options.</param>
            /// <param name="openResolutionScope">Opens the new scope.</param><param name="asResolutionCall"></param>
            /// <param name="preventDisposal">Prevents disposal of reused instance if it is disposable.</param>
            /// <param name="condition">Predicate to check if factory could be used for resolved request.</param>
            public WrapperSetup(int wrappedServiceTypeArgIndex, bool alwaysWrapsRequiredServiceType, Func<Type, Type> unwrap,
                Func<Request, bool> condition, bool openResolutionScope, bool asResolutionCall, bool preventDisposal)
                : base(condition, openResolutionScope: openResolutionScope, asResolutionCall: asResolutionCall, preventDisposal: preventDisposal)
            {
                WrappedServiceTypeArgIndex = wrappedServiceTypeArgIndex;
                AlwaysWrapsRequiredServiceType = alwaysWrapsRequiredServiceType;
                Unwrap = unwrap;
            }

            internal void ThrowIfInvalidRegistration(Type serviceType)
            {
                if (AlwaysWrapsRequiredServiceType || Unwrap != null)
                    return;

                if (!serviceType.IsGeneric())
                    return;

                var typeArgCount = serviceType.GetGenericParamsAndArgs().Length;
                var typeArgIndex = WrappedServiceTypeArgIndex;
                Throw.If(typeArgCount > 1 && typeArgIndex == -1,
                    Error.GenericWrapperWithMultipleTypeArgsShouldSpecifyArgIndex, serviceType);

                var index = typeArgIndex != -1 ? typeArgIndex : 0;
                Throw.If(index > typeArgCount - 1,
                    Error.GenericWrapperTypeArgIndexOutOfBounds, serviceType, index);
            }

            /// <summary>Unwraps service type or returns its.</summary>
            /// <param name="serviceType"></param> <returns>Wrapped type or self.</returns>
            public Type GetWrappedTypeOrNullIfWrapsRequired(Type serviceType)
            {
                if (Unwrap != null)
                    return Unwrap(serviceType);

                if (AlwaysWrapsRequiredServiceType || !serviceType.IsGeneric())
                    return null;

                var typeArgs = serviceType.GetGenericParamsAndArgs();
                var typeArgIndex = WrappedServiceTypeArgIndex;
                serviceType.ThrowIf(typeArgs.Length > 1 && typeArgIndex == -1,
                    Error.GenericWrapperWithMultipleTypeArgsShouldSpecifyArgIndex);

                typeArgIndex = typeArgIndex != -1 ? typeArgIndex : 0;
                serviceType.ThrowIf(typeArgIndex > typeArgs.Length - 1,
                    Error.GenericWrapperTypeArgIndexOutOfBounds, typeArgIndex);

                return typeArgs[typeArgIndex];
            }
        }

        /// <summary>Setup applied to decorators.</summary>
        internal sealed class DecoratorSetup : Setup
        {
            /// <summary>Returns Decorator factory type.</summary>
            public override FactoryType FactoryType => FactoryType.Decorator;

            /// <summary>If provided specifies relative decorator position in decorators chain.
            /// Greater number means further from decoratee - specify negative number to stay closer.
            /// Decorators without order (Order is 0) or with equal order are applied in registration order
            /// - first registered are closer decoratee.</summary>
            public readonly int Order;

            // todo: It does not consider the keys of decorated services, therefore it will be shared between all services in collection
            /// <summary>Instructs to use decorated service reuse. Decorated service may be decorator itself.</summary>
            public readonly bool UseDecorateeReuse;

            /// <summary>Default setup.</summary>
            public DecoratorSetup() { }

            /// <summary>Creates decorator setup with optional condition.</summary>
            /// <param name="condition">(optional) Applied to decorated service to find that service is the decorator target.</param>
            /// <param name="order">(optional) If provided specifies relative decorator position in decorators chain.
            /// Greater number means further from decoratee - specify negative number to stay closer.
            /// Decorators without order (Order is 0) or with equal order are applied in registration order
            /// - first registered are closer decoratee.</param>
            /// <param name="useDecorateeReuse">(optional) Instructs to use decorated service reuse.
            /// Decorated service may be decorator itself.</param>
            /// <param name="openResolutionScope">Opens resolution scope</param>
            public DecoratorSetup(Func<Request, bool> condition, int order, bool useDecorateeReuse,
                bool openResolutionScope = false)
                : base(condition, openResolutionScope)
            {
                Order = order;
                UseDecorateeReuse = useDecorateeReuse;
            }
        }
    }

    /// <summary>Facility for creating concrete factories from some template/prototype. Example:
    /// creating closed-generic type reflection factory from registered open-generic prototype factory.</summary>
    public interface IConcreteFactoryGenerator
    {
        /// <summary>Generated factories so far, identified by the service type and key pair.</summary>
        ImHashMap<KV<Type, object>, ReflectionFactory> GeneratedFactories { get; }

        /// <summary>Returns factory per request. May track already generated factories and return one without regenerating.</summary>
        /// <param name="request">Request to resolve.</param>
        /// <param name="ifErrorReturnDefault">If set to true - returns null if unable to generate,
        /// otherwise error result depends on <see cref="Request.IfUnresolved"/>.</param>
        /// <returns>Returns new factory per request.</returns>
        Factory GetGeneratedFactory(Request request, bool ifErrorReturnDefault = false);
    }

    /// <summary>Base class for different ways to instantiate service:
    /// <list type="bullet">
    /// <item>Through reflection - <see cref="ReflectionFactory"/></item>
    /// <item>Using custom delegate - <see cref="DelegateFactory"/></item>
    /// <item>Using custom expression - <see cref="ExpressionFactory"/></item>
    /// </list>
    /// For all of the types Factory should provide result as <see cref="Expr"/> and <see cref="FactoryDelegate"/>.
    /// Factories are supposed to be immutable and stateless.
    /// Each created factory has an unique ID set in <see cref="FactoryID"/>.</summary>
    public abstract class Factory
    {
        /// <summary>Get next factory ID in a atomic way.</summary><returns>The ID.</returns>
        public static int GetNextID() =>
            Interlocked.Increment(ref _lastFactoryID);

        /// <summary>Unique factory id generated from static seed.</summary>
        public int FactoryID { get; internal set; }

        /// <summary>Reuse policy for created services.</summary>
        public virtual IReuse Reuse => _reuse;

        /// <summary>Setup may contain different/non-default factory settings.</summary>
        public virtual Setup Setup
        {
            get { return _setup; }
            internal set { _setup = value ?? Setup.Default; }
        }

        /// <summary>Checks that condition is met for request or there is no condition setup.</summary>
        public bool CheckCondition(Request request) =>
            (Setup.Condition == null || Setup.Condition(request));

        /// <summary>Shortcut for <see cref="DryIoc.Setup.FactoryType"/>.</summary>
        public FactoryType FactoryType => Setup.FactoryType;

        /// <summary>Non-abstract closed implementation type. May be null if not known beforehand, e.g. in <see cref="DelegateFactory"/>.</summary>
        public virtual Type ImplementationType => null;

        /// <summary>Allow inheritors to define lazy implementation type</summary>
        public virtual bool CanAccessImplementationType => true;

        /// <summary>Indicates that Factory is factory provider and
        /// consumer should call <see cref="IConcreteFactoryGenerator.GetGeneratedFactory"/>  to get concrete factory.</summary>
        public virtual IConcreteFactoryGenerator FactoryGenerator => null;

        /// <summary>Settings <b>(if any)</b> to select Constructor/FactoryMethod, Parameters, Properties and Fields.</summary>
        public virtual Made Made => Made.Default;

        /// <summary>Initializes reuse and setup. Sets the <see cref="FactoryID"/></summary>
        /// <param name="reuse">(optional)</param> <param name="setup">(optional)</param>
        protected Factory(IReuse reuse = null, Setup setup = null)
        {
            FactoryID = GetNextID();
            _reuse = reuse;
            _setup = setup ?? Setup.Default;
        }

        /// <summary>The main factory method to create service expression, e.g. "new Client(new Service())".
        /// If <paramref name="request"/> has <see cref="Request.InputArgs"/> specified, they could be used in expression.</summary>
        /// <param name="request">Service request.</param>
        /// <returns>Created expression.</returns>
        public abstract Expr CreateExpressionOrDefault(Request request);

        /// <summary>Allows derived factories to override or reuse caching policy used by
        /// GetExpressionOrDefault. By default only service setup and no  user passed arguments may be cached.</summary>
        /// <param name="request">Context.</param> <returns>True if factory expression could be cached.</returns>
        protected virtual bool IsFactoryExpressionCacheable(Request request)
            => Setup.FactoryType == FactoryType.Service
            && Setup.Condition == null
            && !Setup.UseParentReuse
            && !Setup.AsResolutionCall

            && request.InputArgs == null
            && !request.IsResolutionCall
            && !(request.Reuse is CurrentScopeReuse)
            && !request.TracksTransientDisposable;

        private bool ShouldBeInjectedAsResolutionCall(Request request) =>
            !request.IsResolutionCall // is not already a resolution call
            && (Setup.AsResolutionCall || request.ShouldSplitObjectGraph() || Setup.UseParentReuse)
            && request.GetActualServiceType() != typeof(void); // exclude action

        /// <summary>Returns service expression: either by creating it with <see cref="CreateExpressionOrDefault"/> or taking expression from cache.
        /// Before returning method may transform the expression  by applying <see cref="Reuse"/>, or/and decorators if found any.</summary>
        public virtual Expr GetExpressionOrDefault(Request request)
        {
            request = request.WithResolvedFactory(this);

            // preventing recursion
            if (!request.OpensResolutionScope &&
                (Setup.OpenResolutionScope || ShouldBeInjectedAsResolutionCall(request)))
                return Resolver.CreateResolutionExpression(request, Setup.OpenResolutionScope);

            var container = request.Container;

            // First look for decorators
            if (FactoryType != FactoryType.Decorator)
            {
                var decoratorExpr = container.GetDecoratorExpressionOrDefault(request);
                if (decoratorExpr != null)
                    return decoratorExpr;
            }

            // Then optimize for already resolved singleton object, otherwise goes normal ApplyReuse route
            if (request.Rules.EagerCachingSingletonForFasterAccess &&
                request.Reuse is SingletonReuse &&
                !Setup.PreventDisposal &&
                !Setup.WeaklyReferenced)
            {
                object singleton;
                if (request.SingletonScope.TryGet(out singleton, FactoryID))
                    return Constant(singleton, request.ServiceType);
            }

            // Then check the expression cache
            var isCacheable = IsFactoryExpressionCacheable(request);
            if (isCacheable)
            {
                var cachedExpr = container.GetCachedFactoryExpressionOrDefault(FactoryID);
                if (cachedExpr != null)
                    return cachedExpr;
            }

            // Then create new expression
            var serviceExpr = CreateExpressionOrDefault(request);
            if (serviceExpr != null)
            {
                // can be checked only after expression is created
                if (request.ContainsNestedResolutionCall)
                    isCacheable = false;

                if (request.Reuse != DryIoc.Reuse.Transient &&
                    request.GetActualServiceType() != typeof(void))
                {
                    var originalServiceExprType = serviceExpr.Type;

                    serviceExpr = ApplyReuse(serviceExpr, request);

                    if (serviceExpr.NodeType == ExpressionType.Constant)
                        isCacheable = false;

                    if (serviceExpr.Type != originalServiceExprType)
                        serviceExpr = Convert(serviceExpr, originalServiceExprType);
                }

                if (isCacheable)
                    container.CacheFactoryExpression(FactoryID, serviceExpr);

            }
            else if (request.IfUnresolved == IfUnresolved.Throw)
            {
                Container.ThrowUnableToResolve(request);
            }

            return serviceExpr;
        }

        /// <summary>Applies reuse to created expression, by wrapping passed expression into scoped access
        /// and producing the result expression.</summary>
        protected virtual Expr ApplyReuse(Expr serviceExpr, Request request)
        {
            var reuse = request.Reuse;

            // optimization for already activated singleton
            if (serviceExpr.NodeType == ExpressionType.Constant &&
                reuse is SingletonReuse && request.Rules.EagerCachingSingletonForFasterAccess &&
                !Setup.PreventDisposal && !Setup.WeaklyReferenced)
                return serviceExpr;

            // Optimize: eagerly create singleton during the construction of object graph,
            // but only for root singleton and not for singleton dependency inside singleton, because of double compilation work
            if (reuse is SingletonReuse &&
                request.Rules.EagerCachingSingletonForFasterAccess &&
                // except: For decorators and wrappers, when tracking transient disposable and for lazy consumption in Func
                FactoryType == FactoryType.Service &&
                !request.TracksTransientDisposable &&
                !request.IsWrappedInFunc())
            {
                var factoryDelegate = Container.CompileToDelegate(serviceExpr);

                if (Setup.WeaklyReferenced)
                {
                    var factory = factoryDelegate;
                    factoryDelegate = r => new WeakReference(factory(r));
                }
                else if (Setup.PreventDisposal)
                {
                    var factory = factoryDelegate;
                    factoryDelegate = r => new HiddenDisposable(factory(r));
                }

                var singleton = request.SingletonScope
                    .GetOrAdd(FactoryID, () => factoryDelegate(request.Container));

                serviceExpr = Constant(singleton);
            }
            else
            {
                if (Setup.WeaklyReferenced)
                    serviceExpr = New(typeof(WeakReference).GetConstructorOrNull(args: typeof(object)), serviceExpr);
                else if (Setup.PreventDisposal)
                    serviceExpr = New(HiddenDisposable.Ctor, serviceExpr);
                serviceExpr = reuse.Apply(request, serviceExpr);
            }

            // Unwrap WeakReference and/or array preventing disposal
            if (Setup.WeaklyReferenced)
                serviceExpr = Call(
                    typeof(ThrowInGeneratedCode).Method(nameof(ThrowInGeneratedCode.ThrowNewErrorIfNull)),
                    Property(Convert(serviceExpr, typeof(WeakReference)),
                        typeof(WeakReference).Property(nameof(WeakReference.Target))),
                    Constant(Error.Messages[Error.WeakRefReuseWrapperGCed]));
            else if (Setup.PreventDisposal)
                serviceExpr = Field(
                    Convert(serviceExpr, typeof(HiddenDisposable)),
                    HiddenDisposable.ValueField);

            return serviceExpr;
        }

        /// <summary>Creates factory delegate from service expression and returns it.
        /// to compile delegate from expression but could be overridden by concrete factory type: e.g. <see cref="DelegateFactory"/></summary>
        /// <param name="request">Service request.</param>
        /// <returns>Factory delegate created from service expression.</returns>
        public virtual FactoryDelegate GetDelegateOrDefault(Request request)
        {
            var expression = GetExpressionOrDefault(request);
            if (expression == null)
                return null;
            return Container.CompileToDelegate(expression);
        }

        internal virtual bool ThrowIfInvalidRegistration(Type serviceType, object serviceKey, bool isStaticallyChecked, Rules rules)
        {
            if (!isStaticallyChecked)
                serviceType.ThrowIfNull();

            var setup = Setup;

            if (setup.FactoryType == FactoryType.Service)
            {
                // Warn about registering disposable transient
                var reuse = Reuse ?? rules.DefaultReuse;
                if (reuse != DryIoc.Reuse.Transient)
                    return true;

                if (setup.AllowDisposableTransient ||
                    !rules.ThrowOnRegisteringDisposableTransient)
                    return true;

                if (setup.UseParentReuse ||
                    setup.FactoryType == FactoryType.Decorator && ((Setup.DecoratorSetup)setup).UseDecorateeReuse)
                    return true;

                var knownImplOrServiceType = CanAccessImplementationType ? ImplementationType : serviceType;
                if (knownImplOrServiceType.IsAssignableTo(typeof(IDisposable)))
                    Throw.It(Error.RegisteredDisposableTransientWontBeDisposedByContainer,
                        serviceType, serviceKey ?? "{no key}", this);
            }
            else if (setup.FactoryType == FactoryType.Wrapper)
            {
                ((Setup.WrapperSetup)setup).ThrowIfInvalidRegistration(serviceType);
            }
            else if (setup.FactoryType == FactoryType.Decorator)
            {
                if (serviceKey != null)
                    Throw.It(Error.DecoratorShouldNotBeRegisteredWithServiceKey, serviceKey);
            }

            return true;
        }

        /// <summary>Returns nice string representation of factory.</summary>
        /// <returns>String representation.</returns>
        public override string ToString()
        {
            var s = new StringBuilder().Append("{ID=").Append(FactoryID);
            if (ImplementationType != null)
                s.Append(", ImplType=").Print(ImplementationType);
            if (Reuse != null)
                s.Append(", Reuse=").Print(Reuse);
            if (Setup.FactoryType != Setup.Default.FactoryType)
                s.Append(", FactoryType=").Append(Setup.FactoryType);
            if (Setup.Metadata != null)
                s.Append(", Metadata=").Print(Setup.Metadata, quote: "\"");
            if (Setup.Condition != null)
                s.Append(", HasCondition");

            if (Setup.OpenResolutionScope)
                s.Append(", OpensResolutionScope");
            else if (Setup.AsResolutionCall)
                s.Append(", AsResolutionCall");

            return s.Append("}").ToString();
        }

#region Implementation

        private static int _lastFactoryID;
        private IReuse _reuse;
        private Setup _setup;

#endregion
    }

    /// <summary>Declares delegate to get single factory method or constructor for resolved request.</summary>
    public delegate FactoryMethod FactoryMethodSelector(Request request);

    /// <summary>Specifies how to get parameter info for injected parameter and resolved request</summary>
    public delegate Func<ParameterInfo, ParameterServiceInfo> ParameterSelector(Request request);

    /// <summary>Specifies what properties or fields to inject and how.</summary>
    public delegate IEnumerable<PropertyOrFieldServiceInfo> PropertiesAndFieldsSelector(Request request);

    /// <summary>DSL for specifying <see cref="ParameterSelector"/> injection rules.</summary>
    public static class Parameters
    {
        /// <summary>Returns default service info wrapper for each parameter info.</summary>
        public static ParameterSelector Of = request => ParameterServiceInfo.Of;

        /// <summary>Returns service info which considers each parameter as optional.</summary>
        public static ParameterSelector IfUnresolvedReturnDefault =
            request => pi => ParameterServiceInfo.Of(pi).WithDetails(ServiceDetails.IfUnresolvedReturnDefault);

        /// <summary>Combines source selector with other. Other is used as fallback when source returns null.</summary>
        public static ParameterSelector OverrideWith(this ParameterSelector source, ParameterSelector other) =>
            source == null || source == Of ? other ?? Of
            : other == null || other == Of ? source
            : request => parameterInfo =>
            {
                // try other selector first
                var otherSelector = other(request);
                if (otherSelector != null)
                {
                    var parameterServiceInfo = otherSelector(parameterInfo);
                    if (parameterServiceInfo != null)
                        return parameterServiceInfo;
                }

                // fallback to source selector if other is failed
                var sourceSelector = source(request);
                if (sourceSelector != null)
                    return sourceSelector(parameterInfo);

                return null;
            };

        /// <summary>Obsolete: please use <see cref="OverrideWith"/></summary>
        [Obsolete("Replaced with OverrideWith", false)]
        public static ParameterSelector And(this ParameterSelector source, ParameterSelector other) =>
            source.OverrideWith(other);

        /// <summary>Overrides source parameter rules with specific parameter details. If it is not your parameter just return null.</summary>
        /// <param name="source">Original parameters rules</param>
        /// <param name="getDetailsOrNull">Should return specific details or null.</param>
        /// <returns>New parameters rules.</returns>
        public static ParameterSelector Details(this ParameterSelector source, Func<Request, ParameterInfo, ServiceDetails> getDetailsOrNull)
        {
            getDetailsOrNull.ThrowIfNull();
            return request => parameter =>
            {
                var details = getDetailsOrNull(request, parameter);
                if (details != null)
                    return ParameterServiceInfo.Of(parameter).WithDetails(details);

                // for default source selector, return null to enable fallback to any non-default selector
                // defined outside, usually by OverrideWith
                if (source == Of)
                    return null;

                return source(request)(parameter);
            };
        }

        /// <summary>Adds to <paramref name="source"/> selector service info for parameter identified by <paramref name="name"/>.</summary>
        /// <param name="source">Original parameters rules.</param> <param name="name">Name to identify parameter.</param>
        /// <param name="requiredServiceType">(optional)</param> <param name="serviceKey">(optional)</param>
        /// <param name="ifUnresolved">(optional) By default throws exception if unresolved.</param>
        /// <param name="defaultValue">(optional) Specifies default value to use when unresolved.</param>
        /// <param name="metadataKey">(optional) Required metadata key</param> <param name="metadata">Required metadata or value.</param>
        /// <returns>New parameters rules.</returns>
        public static ParameterSelector Name(this ParameterSelector source, string name,
            Type requiredServiceType = null, object serviceKey = null,
            IfUnresolved ifUnresolved = IfUnresolved.Throw, object defaultValue = null,
            string metadataKey = null, object metadata = null) =>
            source.Details((r, p) => !p.Name.Equals(name) ? null
                : ServiceDetails.Of(requiredServiceType, serviceKey, ifUnresolved, defaultValue, metadataKey, metadata));

        /// <summary>Specify parameter by name and set custom value to it.</summary>
        public static ParameterSelector Name(this ParameterSelector source,
            string name, Func<Request, ParameterInfo, ServiceDetails> getServiceDetails) =>
            source.Details((r, p) => p.Name.Equals(name) ? getServiceDetails(r, p) : null);

        /// <summary>Specify parameter by name and set custom value to it.</summary>
        public static ParameterSelector Name(this ParameterSelector source,
            string name, Func<Request, object> getCustomValue) =>
             source.Name(name, (r, p) => ServiceDetails.Of(getCustomValue(r)));

        /// <summary>Adds to <paramref name="source"/> selector service info for parameter identified by type <typeparamref name="T"/>.</summary>
        /// <typeparam name="T">Type of parameter.</typeparam> <param name="source">Source selector.</param>
        /// <param name="requiredServiceType">(optional)</param> <param name="serviceKey">(optional)</param>
        /// <param name="ifUnresolved">(optional) By default throws exception if unresolved.</param>
        /// <param name="defaultValue">(optional) Specifies default value to use when unresolved.</param>
        /// <param name="metadataKey">(optional) Required metadata key</param> <param name="metadata">Required metadata or value.</param>
        /// <returns>Combined selector.</returns>
        public static ParameterSelector Type<T>(this ParameterSelector source,
            Type requiredServiceType = null, object serviceKey = null,
            IfUnresolved ifUnresolved = IfUnresolved.Throw, object defaultValue = null,
            string metadataKey = null, object metadata = null) =>
            source.Details((r, p) => !typeof(T).IsAssignableTo(p.ParameterType) ? null
                : ServiceDetails.Of(requiredServiceType, serviceKey, ifUnresolved, defaultValue, metadataKey, metadata));

        /// <summary>Specify parameter by type and set its details.</summary>
        public static ParameterSelector Type<T>(this ParameterSelector source,
            Func<Request, ParameterInfo, ServiceDetails> getServiceDetails) =>
            source.Details((r, p) => p.ParameterType == typeof(T) ? getServiceDetails(r, p) : null);

        /// <summary>Specify parameter by type and set custom value to it.</summary>
        public static ParameterSelector Type<T>(this ParameterSelector source, Func<Request, T> getCustomValue) =>
            source.Type<T>((r, p) => ServiceDetails.Of(getCustomValue(r)));

        /// <summary>Adds to <paramref name="source"/> selector service info for parameter identified by type <paramref name="parameterType"/>.</summary>
        /// <param name="source">Source selector.</param> <param name="parameterType">The type of the parameter.</param>
        /// <param name="requiredServiceType">(optional)</param> <param name="serviceKey">(optional)</param>
        /// <param name="ifUnresolved">(optional) By default throws exception if unresolved.</param>
        /// <param name="defaultValue">(optional) Specifies default value to use when unresolved.</param>
        /// <param name="metadataKey">(optional) Required metadata key</param> <param name="metadata">Required metadata or value.</param>
        /// <returns>Combined selector.</returns>
        public static ParameterSelector Type(this ParameterSelector source, Type parameterType,
            Type requiredServiceType = null, object serviceKey = null,
            IfUnresolved ifUnresolved = IfUnresolved.Throw, object defaultValue = null,
            string metadataKey = null, object metadata = null) =>
            source.Details((r, p) => !parameterType.IsAssignableTo(p.ParameterType) ? null
                : ServiceDetails.Of(requiredServiceType, serviceKey, ifUnresolved, defaultValue, metadataKey, metadata));

        /// <summary>Specify parameter by type and set custom value to it.</summary>
        /// <param name="source">Original parameters rules.</param>
        /// <param name="parameterType">The type of the parameter.</param>
        /// <param name="getCustomValue">Custom value provider.</param>
        /// <returns>New parameters rules.</returns>
        public static ParameterSelector Type(this ParameterSelector source,
            Type parameterType, Func<Request, object> getCustomValue) =>
            source.Details((r, p) => p.ParameterType == parameterType ? ServiceDetails.Of(getCustomValue(r)) : null);
    }

    /// <summary>DSL for specifying <see cref="PropertiesAndFieldsSelector"/> injection rules.</summary>
    public static partial class PropertiesAndFields
    {
        /// <summary>Say to not resolve any properties or fields.</summary>
        public static PropertiesAndFieldsSelector Of = request => null;

        /// <summary>Public assignable instance members of any type except object, string, primitives types, and arrays of those.</summary>
        public static PropertiesAndFieldsSelector Auto = All(withNonPublic: false, withPrimitive: false);

        /// <summary>Public, declared, assignable, non-primitive properties.</summary>
        public static PropertiesAndFieldsSelector Properties(
            bool withNonPublic = false, bool withBase = false,
            IfUnresolved ifUnresolved = IfUnresolved.ReturnDefaultIfNotRegistered) =>
            All(withNonPublic: withNonPublic, withPrimitive: false, withFields: false, withBase: withBase, ifUnresolved: ifUnresolved);

        /// <summary>Should return service info for input member (property or field).</summary>
        public delegate PropertyOrFieldServiceInfo GetServiceInfo(MemberInfo member, Request request);

        /// <summary>Generates selector property and field selector with settings specified by parameters.
        /// If all parameters are omitted the return all public not primitive members.</summary>
        public static PropertiesAndFieldsSelector All(
            bool withNonPublic = true,
            bool withPrimitive = true,
            bool withFields = true,
            bool withBase = true,
            IfUnresolved ifUnresolved = IfUnresolved.ReturnDefaultIfNotRegistered,
            GetServiceInfo serviceInfo = null)
        {
            GetServiceInfo info = (m, r) =>
                serviceInfo != null ? serviceInfo(m, r) :
                PropertyOrFieldServiceInfo.Of(m).WithDetails(ServiceDetails.Of(ifUnresolved: ifUnresolved));

            return req =>
            {
                var properties = req.ImplementationType.GetMembers(_ => _.DeclaredProperties, includeBase: withBase)
                    .Match(p => p.IsInjectable(withNonPublic, withPrimitive), p => info(p, req));

                if (!withFields)
                    return properties;

                var fields = req.ImplementationType
                    .GetMembers(_ => _.DeclaredFields, includeBase: withBase)
                    .Match(f => f.IsInjectable(withNonPublic, withPrimitive), f => info(f, req));

                return properties.Append(fields);
            };
        }

        /// <summary>Combines source properties and fields with other. Other will override the source condition.</summary>
        /// <param name="source">Source selector.</param> <param name="other">Specific other selector to add.</param>
        /// <returns>Combined result selector.</returns>
        public static PropertiesAndFieldsSelector OverrideWith(
            this PropertiesAndFieldsSelector source, PropertiesAndFieldsSelector other)
        {
            return source == null || source == Of ? (other ?? Of)
                : other == null || other == Of ? source
                : r =>
                {
                    var sourceMembers = source(r).ToArrayOrSelf();
                    var otherMembers = other(r).ToArrayOrSelf();
                    return sourceMembers == null || sourceMembers.Length == 0 ? otherMembers
                        : otherMembers == null || otherMembers.Length == 0 ? sourceMembers
                        : otherMembers.Append(
                            sourceMembers.Match(s => s != null &&
                                otherMembers.All(o => o == null || !s.Member.Name.Equals(o.Member.Name))));
                };
        }

        /// <summary>Obsolete: please use <see cref="OverrideWith"/></summary>
        [Obsolete("Replaced with OverrideWith", false)]
        public static PropertiesAndFieldsSelector And(
            this PropertiesAndFieldsSelector source, PropertiesAndFieldsSelector other) =>
            source.OverrideWith(other);

        /// <summary>Specifies service details (key, if-unresolved policy, required type) for property/field with the name.</summary>
        /// <param name="source">Original member selector.</param> <param name="name">Member name.</param> <param name="getDetails">Details.</param>
        /// <returns>New selector.</returns>
        public static PropertiesAndFieldsSelector Details(this PropertiesAndFieldsSelector source,
            string name, Func<Request, ServiceDetails> getDetails)
        {
            name.ThrowIfNull();
            getDetails.ThrowIfNull();
            return source.OverrideWith(request =>
            {
                var implType = request.GetKnownImplementationOrServiceType();

                var property = implType
                    .GetMembers(it => it.DeclaredProperties, includeBase: true)
                    .FindFirst(it => it.Name == name);
                if (property != null && property.IsInjectable(true, true))
                {
                    var details = getDetails(request);
                    return details == null ? null
                        : new[] { PropertyOrFieldServiceInfo.Of(property).WithDetails(details) };
                }

                var field = implType
                    .GetMembers(it => it.DeclaredFields, includeBase: true)
                    .FindFirst(it => it.Name == name);
                if (field != null && field.IsInjectable(true, true))
                {
                    var details = getDetails(request);
                    return details == null ? null
                        : new[] { PropertyOrFieldServiceInfo.Of(field).WithDetails(details) };
                }

                return Throw.For<IEnumerable<PropertyOrFieldServiceInfo>>(
                    Error.NotFoundSpecifiedWritablePropertyOrField, name, request);
            });
        }

        /// <summary>Adds to <paramref name="source"/> selector service info for property/field identified by <paramref name="name"/>.</summary>
        /// <param name="source">Source selector.</param> <param name="name">Name to identify member.</param>
        /// <param name="requiredServiceType">(optional)</param> <param name="serviceKey">(optional)</param>
        /// <param name="ifUnresolved">(optional) By default returns default value if unresolved.</param>
        /// <param name="defaultValue">(optional) Specifies default value to use when unresolved.</param>
        /// <param name="metadataKey">(optional) Required metadata key</param> <param name="metadata">Required metadata or value.</param>
        /// <returns>Combined selector.</returns>
        public static PropertiesAndFieldsSelector Name(this PropertiesAndFieldsSelector source, string name,
            Type requiredServiceType = null, object serviceKey = null,
            IfUnresolved ifUnresolved = IfUnresolved.ReturnDefault, object defaultValue = null,
            string metadataKey = null, object metadata = null) =>
            source.Details(name, r => ServiceDetails.Of(
                requiredServiceType, serviceKey, ifUnresolved, defaultValue, metadataKey, metadata));

        /// <summary>Specifies custom value for property/field with specific name.</summary>
        public static PropertiesAndFieldsSelector Name(this PropertiesAndFieldsSelector source,
            string name, Func<Request, object> getCustomValue) =>
            source.Details(name, r => ServiceDetails.Of(getCustomValue(r)));

        /// <summary>Returns true if property matches flags provided.</summary>
        /// <param name="property">Property to match</param>
        /// <param name="withNonPublic">Says to include non public properties.</param>
        /// <param name="withPrimitive">Says to include properties of primitive type.</param>
        /// <returns>True if property is matched and false otherwise.</returns>
        public static bool IsInjectable(this PropertyInfo property,
            bool withNonPublic = false, bool withPrimitive = false) =>
            property.CanWrite
            && !property.IsExplicitlyImplemented()
            && !property.IsStatic()
            && !property.IsIndexer() // first checks that property is assignable in general and not an indexer
            && (withNonPublic || property.GetSetMethodOrNull() != null)
            && (withPrimitive || !property.PropertyType.IsPrimitive(orArrayOfPrimitives: true));

        /// <summary>Returns true if field matches flags provided.</summary>
        /// <param name="field">Field to match.</param>
        /// <param name="withNonPublic">Says to include non public fields.</param>
        /// <param name="withPrimitive">Says to include fields of primitive type.</param>
        /// <returns>True if property is matched and false otherwise.</returns>
        public static bool IsInjectable(this FieldInfo field,
            bool withNonPublic = false, bool withPrimitive = false) =>
            !field.IsInitOnly && !field.IsBackingField()
                && (withNonPublic || field.IsPublic)
                && (withPrimitive || !field.FieldType.IsPrimitive(orArrayOfPrimitives: true));
    }

    /// <summary>Reflects on <see cref="ImplementationType"/> constructor parameters and members,
    /// creates expression for each reflected dependency, and composes result service expression.</summary>
    public sealed class ReflectionFactory : Factory
    {
        /// <summary>Non-abstract service implementation type. May be open generic.</summary>
        public override Type ImplementationType
        {
            get
            {
                if (_implementationType == null && _implementationTypeProvider != null)
                    SetKnownImplementationType(_implementationTypeProvider(), Made);
                return _implementationType;
            }
        }

        /// <summary>False for lazy implementation type, to prevent its early materialization.</summary>
        public override bool CanAccessImplementationType =>
            _implementationType != null || _implementationTypeProvider == null;

        /// <summary>Provides closed-generic factory for registered open-generic variant.</summary>
        public override IConcreteFactoryGenerator FactoryGenerator => _factoryGenerator;

        /// <summary>Injection rules set for Constructor/FactoryMethod, Parameters, Properties and Fields.</summary>
        public override Made Made => _made;

        /// <summary>Creates factory providing implementation type, optional reuse and setup.</summary>
        /// <param name="implementationType">(optional) Optional if Made.FactoryMethod is present Non-abstract close or open generic type.</param>
        /// <param name="reuse">(optional)</param> <param name="made">(optional)</param> <param name="setup">(optional)</param>
        public ReflectionFactory(Type implementationType = null, IReuse reuse = null, Made made = null, Setup setup = null)
            : base(reuse, setup)
        {
            _made = made ?? Made.Default;
            SetKnownImplementationType(implementationType, _made);
        }

        /// <summary>Creates factory providing implementation type, optional reuse and setup.</summary>
        /// <param name="implementationTypeProvider">Provider of non-abstract close or open generic type.</param>
        /// <param name="reuse">(optional)</param> <param name="made">(optional)</param> <param name="setup">(optional)</param>
        public ReflectionFactory(Func<Type> implementationTypeProvider, IReuse reuse = null, Made made = null, Setup setup = null)
            : base(reuse, setup)
        {
            _made = made ?? Made.Default;
            _implementationTypeProvider = implementationTypeProvider.ThrowIfNull();
        }

        /// <summary>Add to base rules: do not cache if Made is context based.</summary>
        protected override bool IsFactoryExpressionCacheable(Request request) =>
            base.IsFactoryExpressionCacheable(request)
                 && (Made == Made.Default
                 // Property injection.
                 || (Made.FactoryMethod == null
                    && Made.Parameters == null
                    && (Made.PropertiesAndFields == PropertiesAndFields.Auto ||
                        Made.PropertiesAndFields == PropertiesAndFields.Of))
                 // No caching for context dependent Made which is:
                 // - We don't know the result returned by factory method - it depends on request
                 // - or even if we do know the result type, some dependency is using custom value which depends on request
                 || (Made.FactoryMethodKnownResultType != null && !Made.HasCustomDependencyValue));

        /// <summary>Creates service expression.</summary>
        public override Expr CreateExpressionOrDefault(Request request)
        {
            var factoryMethod = GetFactoryMethod(request);

            var container = request.Container;

            // If factory method is instance method, then resolve factory instance first.
            Expr factoryExpr = null;
            if (factoryMethod.FactoryServiceInfo != null)
            {
                var factoryRequest = request.Push(factoryMethod.FactoryServiceInfo);
                var factoryFactory = container.ResolveFactory(factoryRequest);
                if (factoryFactory == null)
                    return null;
                factoryExpr = factoryFactory.GetExpressionOrDefault(factoryRequest);
                if (factoryExpr == null)
                    return null;
            }

            var containerRules = container.Rules;

            var paramExprs = ArrayTools.Empty<Expr>();
            var ctorOrMember = factoryMethod.ConstructorOrMethodOrMember;
            var ctorOrMethod = ctorOrMember as MethodBase;
            if (ctorOrMethod != null)
            {
                var parameters = ctorOrMethod.GetParameters();
                if (parameters.Length != 0)
                {
                    paramExprs = new Expr[parameters.Length];

                    var selectorSelector =
                        containerRules.OverrideRegistrationMade
                        ? Made.Parameters.OverrideWith(containerRules.Parameters)
                        : containerRules.Parameters.OverrideWith(Made.Parameters);

                    var parameterSelector = selectorSelector(request);

                    var funcArgs = request.InputArgs;
                    var funcArgsUsedMask = 0;

                    for (var i = 0; i < parameters.Length; i++)
                    {
                        var param = parameters[i];
                        Expr paramExpr = null;

                        if (funcArgs != null)
                        {
                            for (var fa = 0; fa < funcArgs.Length && paramExpr == null; ++fa)
                            {
                                var funcArgExpr = funcArgs[fa];
                                if ((funcArgsUsedMask & 1 << fa) == 0 &&                  // not yet used func argument
                                    funcArgExpr.Type.IsAssignableTo(param.ParameterType)) // and it assignable to parameter
                                {
                                    paramExpr = funcArgExpr;
                                    funcArgsUsedMask |= 1 << fa;  // mark that argument was used
                                }
                            }
                        }

                        // If parameter expression still null (no Func argument to substitute), try to resolve it
                        if (paramExpr == null)
                        {
                            var paramInfo = parameterSelector(param) ?? ParameterServiceInfo.Of(param);
                            var paramRequest = request.Push(paramInfo);

                            if (paramInfo.Details.HasCustomValue)
                            {
                                var customValue = paramInfo.Details.CustomValue;
                                if (customValue != null)
                                    customValue.ThrowIfNotOf(paramRequest.ServiceType, Error.InjectedCustomValueIsOfDifferentType, paramRequest);
                                paramExpr = container.GetItemExpression(customValue, paramRequest.ServiceType);
                            }
                            else
                            {
                                var paramFactory = container.ResolveFactory(paramRequest);
                                paramExpr = paramFactory == null ? null : paramFactory.GetExpressionOrDefault(paramRequest);

                                // Meant that parent Or parameter itself allows default value,
                                // otherwise we did not get null but exception
                                if (paramExpr == null)
                                {
                                    // Check if parameter dependency itself (without propagated parent details)
                                    // does not allow default, then stop checking the rest of parameters.
                                    if (paramInfo.Details.IfUnresolved == IfUnresolved.Throw)
                                        return null;

                                    var defaultValue = paramInfo.Details.DefaultValue;
                                    paramExpr = defaultValue != null
                                        ? container.GetItemExpression(defaultValue)
                                        : paramRequest.ServiceType.GetDefaultValueExpression();
                                }
                            }
                        }

                        paramExprs[i] = paramExpr;
                    }
                }
            }

            return CreateServiceExpression(ctorOrMember, factoryExpr, paramExprs, request);
        }

        internal override bool ThrowIfInvalidRegistration(Type serviceType, object serviceKey, bool isStaticallyChecked, Rules rules)
        {
            base.ThrowIfInvalidRegistration(serviceType, serviceKey, isStaticallyChecked, rules);

            if (!CanAccessImplementationType)
                return true;

            var implType = ImplementationType;
            if (Made.FactoryMethod == null && rules.FactoryMethod == null)
            {
                var ctors = implType.GetPublicInstanceConstructors().ToArrayOrSelf();
                if (ctors.Length == 1)
                    _knownSingleCtor = ctors[0];
                else if (ctors.Length == 0)
                    Throw.It(Error.UnableToSelectSinglePublicConstructorFromNone, implType);
                else
                    Throw.It(Error.UnableToSelectSinglePublicConstructorFromMultiple, implType, ctors);
            }

            if (isStaticallyChecked || implType == null)
                return true;

            if (!implType.IsGenericDefinition())
            {
                if (implType.IsOpenGeneric())
                    Throw.It(Error.RegisteringNotAGenericTypedefImplType,
                        implType, implType.GetGenericDefinitionOrNull());

                else if (implType != serviceType && serviceType != typeof(object) &&
                    implType.GetImplementedTypes().IndexOf(t => t == serviceType) == -1)
                    Throw.It(Error.RegisteringImplementationNotAssignableToServiceType, implType, serviceType);
            }
            else if (implType != serviceType)
            {
                if (serviceType.IsGenericDefinition())
                    ThrowIfImplementationAndServiceTypeParamsDontMatch(implType, serviceType);

                else if (implType.IsGeneric() && serviceType.IsOpenGeneric())
                    Throw.It(Error.RegisteringNotAGenericTypedefServiceType,
                        serviceType, serviceType.GetGenericTypeDefinition());

                else if (!serviceType.IsGeneric())
                    Throw.It(Error.RegisteringOpenGenericImplWithNonGenericService, implType, serviceType);

                else if (implType.GetImplementedServiceTypes().IndexOf(serviceType.GetGenericTypeDefinition()) == -1)
                    Throw.It(Error.RegisteringImplementationNotAssignableToServiceType, implType, serviceType);
            }

            return true;
        }

        private static void ThrowIfImplementationAndServiceTypeParamsDontMatch(Type implType, Type serviceType)
        {
            var implTypeParams = implType.GetGenericParamsAndArgs();
            var implementedTypes = implType.GetImplementedTypes();

            var implementedTypeFound = false;
            var containsAllTypeParams = false;
            for (var i = 0; !containsAllTypeParams && i < implementedTypes.Length; ++i)
            {
                var implementedType = implementedTypes[i];
                implementedTypeFound = implementedType.GetGenericDefinitionOrNull() == serviceType;
                containsAllTypeParams = implementedTypeFound
                    && implementedType.ContainsAllGenericTypeParameters(implTypeParams);
            }

            if (!implementedTypeFound)
                Throw.It(Error.RegisteringImplementationNotAssignableToServiceType, implType, serviceType);

            if (!containsAllTypeParams)
                Throw.It(Error.RegisteringOpenGenericServiceWithMissingTypeArgs,
                    implType, serviceType,
                    implementedTypes.Where(t => t.GetGenericDefinitionOrNull() == serviceType));
        }

#region Implementation

        private Type _implementationType; // non-readonly to be set by lazy type provider
        private readonly Func<Type> _implementationTypeProvider;
        private readonly Made _made;
        private ClosedGenericFactoryGenerator _factoryGenerator;
        private ConstructorInfo _knownSingleCtor;

        private sealed class ClosedGenericFactoryGenerator : IConcreteFactoryGenerator
        {
            public ImHashMap<KV<Type, object>, ReflectionFactory> GeneratedFactories => _generatedFactories.Value;

            public ClosedGenericFactoryGenerator(ReflectionFactory openGenericFactory)
            {
                _openGenericFactory = openGenericFactory;
            }

            public Factory GetGeneratedFactory(Request request, bool ifErrorReturnDefault = false)
            {
                var serviceType = request.GetActualServiceType();

                var generatedFactoryKey = new KV<Type, object>(serviceType, request.ServiceKey);

                var generatedFactories = _generatedFactories.Value;
                if (!generatedFactories.IsEmpty)
                {
                    var generatedFactory = generatedFactories.GetValueOrDefault(generatedFactoryKey);
                    if (generatedFactory != null)
                        return generatedFactory;
                }

                var openFactory = _openGenericFactory;
                request = request.WithResolvedFactory(openFactory,
                    skipRecursiveDependencyCheck: ifErrorReturnDefault, skipCaptiveDependencyCheck: ifErrorReturnDefault);

                var implType = openFactory._implementationType;

                var closedTypeArgs = implType == null || implType == serviceType.GetGenericDefinitionOrNull()
                  ? serviceType.GetGenericParamsAndArgs()
                  : implType.IsGenericParameter ? new[] { serviceType }
                  : GetClosedTypeArgsOrNullForOpenGenericType(implType, serviceType, request, ifErrorReturnDefault);

                if (closedTypeArgs == null)
                    return null;

                var made = openFactory.Made;
                if (made.FactoryMethod != null)
                {
                    var factoryMethod = made.FactoryMethod(request);
                    if (factoryMethod == null)
                        return ifErrorReturnDefault ? null
                            : Throw.For<Factory>(Error.GotNullFactoryWhenResolvingService, request);

                    var checkMatchingType = implType != null && implType.IsGenericParameter;
                    var closedFactoryMethod = GetClosedFactoryMethodOrDefault(
                        factoryMethod, closedTypeArgs, request, checkMatchingType);

                    // may be null only for IfUnresolved.ReturnDefault or check for matching type is failed
                    if (closedFactoryMethod == null)
                        return null;

                    made = Made.Of(closedFactoryMethod, made.Parameters, made.PropertiesAndFields);
                }

                Type closedImplType = null;
                if (implType != null)
                {
                    if (implType.IsGenericParameter)
                        closedImplType = closedTypeArgs[0];
                    else
                        closedImplType = Throw.IfThrows<ArgumentException, Type>(
                            () => implType.MakeGenericType(closedTypeArgs),
                            !ifErrorReturnDefault && request.IfUnresolved == IfUnresolved.Throw,
                            Error.NoMatchedGenericParamConstraints, implType, request);

                    if (closedImplType == null)
                        return null;
                }

                var closedGenericFactory = new ReflectionFactory(closedImplType, openFactory.Reuse, made, openFactory.Setup);

                // Storing generated factory ID to service type/key mapping
                // to find/remove generated factories when needed
                _generatedFactories.Swap(_ => _.AddOrUpdate(generatedFactoryKey, closedGenericFactory));
                return closedGenericFactory;
            }

            private readonly ReflectionFactory _openGenericFactory;
            private readonly Ref<ImHashMap<KV<Type, object>, ReflectionFactory>>
                _generatedFactories = Ref.Of(ImHashMap<KV<Type, object>, ReflectionFactory>.Empty);
        }

        private void SetKnownImplementationType(Type implType, Made made)
        {
            var knownImplType = implType;

            var factoryMethodResultType = Made.FactoryMethodKnownResultType;
            if (implType == null ||
                implType == typeof(object) || // required as currently object represents the open-generic type argument T registrations
                implType.IsAbstract())
            {
                if (made.FactoryMethod == null)
                {
                    if (implType == null)
                        Throw.It(Error.RegisteringNullImplementationTypeAndNoFactoryMethod);
                    if (implType.IsAbstract())
                        Throw.It(Error.RegisteringAbstractImplementationTypeAndNoFactoryMethod, implType);
                }

                knownImplType = null; // Ensure that we do not have abstract implementation type

                // Using non-abstract factory method result type is safe for conditions and diagnostics
                if (factoryMethodResultType != null &&
                    factoryMethodResultType != typeof(object) &&
                    !factoryMethodResultType.IsAbstract())
                    knownImplType = factoryMethodResultType;
            }
            else if (factoryMethodResultType != null
                  && factoryMethodResultType != implType)
            {
                implType.ThrowIfNotImplementedBy(factoryMethodResultType,
                    Error.RegisteredFactoryMethodResultTypesIsNotAssignableToImplementationType);
            }

            var openGenericImplType = knownImplType ?? implType;
            if (openGenericImplType == typeof(object) || // for open-generic T implementation
                openGenericImplType != null && (         // for open-generic X<T> implementation
                openGenericImplType.IsGenericDefinition() ||
                openGenericImplType.IsGenericParameter))
                _factoryGenerator = new ClosedGenericFactoryGenerator(this);

            _implementationType = knownImplType;
        }

        private Expr CreateServiceExpression(MemberInfo ctorOrMember, Expr factoryExpr, Expr[] paramExprs, Request request)
        {
            var rules = request.Rules;

            var ctor = ctorOrMember as ConstructorInfo;
            if (ctor == null) // generate a method call or property/field access
            {
                var serviceExpr 
                    = ctorOrMember is MethodInfo ? Call(factoryExpr, (MethodInfo)ctorOrMember, paramExprs)
                    : ctorOrMember is PropertyInfo ? Property(factoryExpr, (PropertyInfo)ctorOrMember)
                    : (Expr)Field(factoryExpr, (FieldInfo)ctorOrMember);

                var returnType = serviceExpr.Type;
                var serviceType = request.GetActualServiceType();

                if (returnType == typeof(object))
                    return Convert(serviceExpr, serviceType);

                if (!returnType.IsAssignableTo(serviceType))
                    return request.IfUnresolved != IfUnresolved.Throw ? null :
                        Throw.For<Expr>(Error.ServiceIsNotAssignableFromFactoryMethod, serviceType, ctorOrMember, request);

                return serviceExpr;
            }

            // Optimize singleton creation by bypassing Expression.New and using Activator.CreateInstance.
            // Why? Because singleton is created only once and it does not make sense
            // to create an optimal delegate for multiple singleton creation. We spend more time 
            // to create the delegate itself.
            // Moreover, the singleton dependency may be a singleton or Transient,
            // so we may activate Transients on a spot as well.
            if (request.Reuse is SingletonReuse &&
                rules.EagerCachingSingletonForFasterAccess &&

                ctor.IsPublic &&
                rules.PropertiesAndFields == null &&
                Made.PropertiesAndFields == null &&

                FactoryType == FactoryType.Service &&
                !Setup.PreventDisposal &&
                !Setup.WeaklyReferenced &&
                !request.TracksTransientDisposable &&
                !request.IsWrappedInFunc())
            {
                var singletonFactory = GetActivator(ctor.DeclaringType, paramExprs);
                if (singletonFactory != null)
                {
                    var singleton = request.SingletonScope.GetOrAdd(FactoryID, singletonFactory);
                    return Constant(singleton);
                }
            }

            var newServiceExpr = New(ctor, paramExprs);
            if (rules.PropertiesAndFields == null && Made.PropertiesAndFields == null)
                return newServiceExpr;

            var selector = rules.OverrideRegistrationMade
                ? Made.PropertiesAndFields.OverrideWith(rules.PropertiesAndFields)
                : rules.PropertiesAndFields.OverrideWith(Made.PropertiesAndFields);

            var propertiesAndFields = selector(request);
            if (propertiesAndFields == null)
                return newServiceExpr;

            var assignments = ArrayTools.Empty<MemberAssignmentExpr>();
            var container = request.Container;
            foreach (var member in propertiesAndFields)
            {
                if (member == null)
                    continue;

                Expr memberExpr;
                var memberRequest = request.Push(member);
                if (member.Details.HasCustomValue)
                {
                    var customValue = member.Details.CustomValue;
                    if (customValue != null)
                        customValue.ThrowIfNotOf(memberRequest.ServiceType, Error.InjectedCustomValueIsOfDifferentType, memberRequest);
                    memberExpr = container.GetItemExpression(customValue, memberRequest.ServiceType);
                }
                else
                {
                    var memberFactory = container.ResolveFactory(memberRequest);
                    memberExpr = memberFactory?.GetExpressionOrDefault(memberRequest);
                    if (memberExpr == null && request.IfUnresolved == IfUnresolved.ReturnDefault)
                        return null;
                }

                if (memberExpr != null)
                    assignments = assignments.AppendOrUpdate(Bind(member.Member, memberExpr));
            }

            return assignments.Length == 0 ? (Expr)newServiceExpr : MemberInit(newServiceExpr, assignments);
        }

        private static CreateScopedValue GetActivator(Type type, IList<object> argExprs)
        {
            if (argExprs == null || argExprs.Count == 0)
                return () => Activator.CreateInstance(type, ArrayTools.Empty<object>());

            var args = new object[argExprs.Count];
            for (var i = 0; i < args.Length; ++i)
            {
                var argExpr = argExprs[i];
                var convertExpr = argExpr as UnaryExpr;
                if (convertExpr != null && convertExpr.NodeType == ExpressionType.Convert)
                    argExpr = convertExpr.Operand;

                var constExpr = argExpr as ConstExpr;
                if (constExpr != null)
                    args[i] = constExpr.Value;
                else
                {
                    var argNewExpr = argExpr as NewExpr;
                    if (argNewExpr == null)
                        return null;

                    var activator = GetActivator(argNewExpr.Type, argNewExpr.Arguments.ToArrayOrSelf());
                    if (activator == null)
                        return null;

                    args[i] = activator();
                }
            }

            return () => Activator.CreateInstance(type, args);
        }

        private FactoryMethod GetFactoryMethod(Request request)
        {
            var implType = request.ImplementationType;
            var factoryMethodSelector = Made.FactoryMethod ?? request.Rules.FactoryMethod;
            if (factoryMethodSelector == null)
            {
                // there is a guarantee of single constructor, which was checked on factory registration
                var ctor = _knownSingleCtor ?? implType.GetPublicInstanceConstructors().First();
                return FactoryMethod.Of(ctor);
            }

            var factoryMethod = factoryMethodSelector(request);
            if (factoryMethod != null && !(factoryMethod.ConstructorOrMethodOrMember is ConstructorInfo))
            {
                var member = factoryMethod.ConstructorOrMethodOrMember;
                var isStaticMember = member.IsStatic();

                Throw.If(isStaticMember && factoryMethod.FactoryServiceInfo != null,
                    Error.FactoryObjProvidedButMethodIsStatic, factoryMethod.FactoryServiceInfo, factoryMethod, request);

                Throw.If(!isStaticMember && factoryMethod.FactoryServiceInfo == null,
                    Error.FactoryObjIsNullInFactoryMethod, factoryMethod, request);
            }

            return factoryMethod.ThrowIfNull(Error.UnableToGetConstructorFromSelector, implType, request);
        }

        private static Type[] GetClosedTypeArgsOrNullForOpenGenericType(
            Type openImplType, Type closedServiceType, Request request, bool ifErrorReturnDefault)
        {
            var serviceTypeArgs = closedServiceType.GetGenericParamsAndArgs();
            var serviceTypeGenericDef = closedServiceType.GetGenericTypeDefinition();

            var implTypeParams = openImplType.GetGenericParamsAndArgs();
            var implTypeArgs = new Type[implTypeParams.Length];

            var implementedTypes = openImplType.GetImplementedTypes();

            var matchFound = false;
            for (var i = 0; !matchFound && i < implementedTypes.Length; ++i)
            {
                var implementedType = implementedTypes[i];
                if (implementedType.IsOpenGeneric() &&
                    implementedType.GetGenericDefinitionOrNull() == serviceTypeGenericDef)
                {
                    matchFound = MatchServiceWithImplementedTypeParams(
                        implTypeArgs, implTypeParams, implementedType.GetGenericParamsAndArgs(), serviceTypeArgs);
                }
            }

            if (!matchFound)
                return ifErrorReturnDefault || request.IfUnresolved != IfUnresolved.Throw ? null
                    : Throw.For<Type[]>(Error.NoMatchedImplementedTypesWithServiceType,
                        openImplType, implementedTypes, request);

            MatchOpenGenericConstraints(implTypeParams, implTypeArgs);

            var notMatchedIndex = Array.IndexOf(implTypeArgs, null);
            if (notMatchedIndex != -1)
                return ifErrorReturnDefault || request.IfUnresolved != IfUnresolved.Throw ? null
                    : Throw.For<Type[]>(Error.NotFoundOpenGenericImplTypeArgInService,
                        openImplType, implTypeParams[notMatchedIndex], request);

            return implTypeArgs;
        }

        private static void MatchOpenGenericConstraints(Type[] implTypeParams, Type[] implTypeArgs)
        {
            for (var i = 0; i < implTypeParams.Length; i++)
            {
                var implTypeArg = implTypeArgs[i];
                if (implTypeArg == null) continue; // skip yet unknown type arg

                var implTypeParam = implTypeParams[i];
                var implTypeParamConstraints = implTypeParam.GetGenericParamConstraints();
                if (implTypeParamConstraints.IsNullOrEmpty()) continue; // skip case with no constraints

                var constraintMatchFound = false;
                for (var j = 0; !constraintMatchFound && j < implTypeParamConstraints.Length; ++j)
                {
                    var implTypeParamConstraint = implTypeParamConstraints[j];
                    if (implTypeParamConstraint != implTypeArg &&
                        implTypeParamConstraint.IsOpenGeneric())
                    {
                        // match type parameters inside constraint
                        var implTypeArgArgs = implTypeArg.IsGeneric()
                            ? implTypeArg.GetGenericParamsAndArgs()
                            : new[] { implTypeArg };

                        var implTypeParamConstraintParams = implTypeParamConstraint.GetGenericParamsAndArgs();
                        constraintMatchFound = MatchServiceWithImplementedTypeParams(
                            implTypeArgs, implTypeParams, implTypeParamConstraintParams, implTypeArgArgs);
                    }
                }
            }
        }

        private static bool MatchServiceWithImplementedTypeParams(
            Type[] resultImplArgs, Type[] implParams, Type[] serviceParams, Type[] serviceArgs,
            int resultCount = 0)
        {
            if (serviceArgs.Length != serviceParams.Length)
                return false;

            for (var i = 0; i < serviceParams.Length; i++)
            {
                var serviceArg = serviceArgs[i];
                var implementedParam = serviceParams[i];
                if (implementedParam.IsGenericParameter)
                {
                    var paramIndex = implParams.IndexOf(implementedParam);
                    if (paramIndex != -1)
                    {
                        if (resultImplArgs[paramIndex] == null)
                        {
                            resultImplArgs[paramIndex] = serviceArg;
                            if (++resultCount == resultImplArgs.Length)
                                return true;
                        }
                        else if (resultImplArgs[paramIndex] != serviceArg)
                            return false; // more than one service type arg is matching with single impl type param
                    }
                }
                else if (implementedParam != serviceArg)
                {
                    if (!implementedParam.IsOpenGeneric() ||
                        implementedParam.GetGenericDefinitionOrNull() != serviceArg.GetGenericDefinitionOrNull())
                        return false; // type param and arg are of different types

                    if (!MatchServiceWithImplementedTypeParams(resultImplArgs, implParams,
                        implementedParam.GetGenericParamsAndArgs(), serviceArg.GetGenericParamsAndArgs()))
                        return false; // nested match failed due either one of above reasons.
                }
            }

            return true;
        }

        private static FactoryMethod GetClosedFactoryMethodOrDefault(
            FactoryMethod factoryMethod, Type[] serviceTypeArgs, Request request,
            bool shouldReturnOnError = false)
        {
            var factoryMember = factoryMethod.ConstructorOrMethodOrMember;
            var factoryInfo = factoryMethod.FactoryServiceInfo;

            var factoryResultType = factoryMember.GetReturnTypeOrDefault();
            var implTypeParams = factoryResultType.IsGenericParameter
                ? new[] { factoryResultType }
                : factoryResultType.GetGenericParamsAndArgs();

            // Get method declaring type, and if its open-generic,
            // then close it first. It is required to get actual method.
            var factoryImplType = factoryMember.DeclaringType.ThrowIfNull();
            if (factoryImplType.IsOpenGeneric())
            {
                var factoryImplTypeParams = factoryImplType.GetGenericParamsAndArgs();
                var resultFactoryImplTypeArgs = new Type[factoryImplTypeParams.Length];

                var isFactoryImplTypeClosed = MatchServiceWithImplementedTypeParams(
                    resultFactoryImplTypeArgs, factoryImplTypeParams,
                    implTypeParams, serviceTypeArgs);

                if (!isFactoryImplTypeClosed)
                    return shouldReturnOnError || request.IfUnresolved != IfUnresolved.Throw ? null
                        : Throw.For<FactoryMethod>(Error.NoMatchedFactoryMethodDeclaringTypeWithServiceTypeArgs,
                            factoryImplType, new StringBuilder().Print(serviceTypeArgs, itemSeparator: ", "), request);

                // For instance factory match its service type from the implementation factory type.
                if (factoryInfo != null)
                {
                    // Open-generic service type is always normalized as generic type definition
                    var factoryServiceType = factoryInfo.ServiceType;

                    // Look for service type equivalent within factory implementation type base classes and interfaces,
                    // because we need identical type arguments to match.
                    if (factoryServiceType != factoryImplType)
                        factoryServiceType = factoryImplType.GetImplementedTypes()
                            .FindFirst(t => t.IsGeneric() && t.GetGenericTypeDefinition() == factoryServiceType)
                            .ThrowIfNull();

                    var factoryServiceTypeParams = factoryServiceType.GetGenericParamsAndArgs();
                    var resultFactoryServiceTypeArgs = new Type[factoryServiceTypeParams.Length];

                    var isFactoryServiceTypeClosed = MatchServiceWithImplementedTypeParams(
                        resultFactoryServiceTypeArgs, factoryServiceTypeParams,
                        factoryImplTypeParams, resultFactoryImplTypeArgs);

                    // Replace factory info with close factory service type
                    if (isFactoryServiceTypeClosed)
                    {
                        MatchOpenGenericConstraints(factoryImplTypeParams, resultFactoryImplTypeArgs);

                        factoryServiceType = factoryServiceType.GetGenericTypeDefinition().ThrowIfNull();
                        var closedFactoryServiceType = Throw.IfThrows<ArgumentException, Type>(
                            () => factoryServiceType.MakeGenericType(resultFactoryServiceTypeArgs),
                            !shouldReturnOnError && request.IfUnresolved == IfUnresolved.Throw,
                            Error.NoMatchedGenericParamConstraints, factoryServiceType, request);

                        if (closedFactoryServiceType == null)
                            return null;

                        // Copy factory info with closed factory type
                        factoryInfo = ServiceInfo.Of(closedFactoryServiceType).WithDetails(factoryInfo.Details);
                    }
                }

                MatchOpenGenericConstraints(factoryImplTypeParams, resultFactoryImplTypeArgs);

                // Close the factory type implementation
                // and get factory member to use from it.
                var closedFactoryImplType = Throw.IfThrows<ArgumentException, Type>(
                    () => factoryImplType.MakeGenericType(resultFactoryImplTypeArgs),
                    !shouldReturnOnError && request.IfUnresolved == IfUnresolved.Throw,
                    Error.NoMatchedGenericParamConstraints, factoryImplType, request);

                if (closedFactoryImplType == null)
                    return null;

                // Find corresponding member again, now from closed type
                var factoryMethodBase = factoryMember as MethodBase;
                if (factoryMethodBase != null)
                {
                    var factoryMethodParameters = factoryMethodBase.GetParameters();
                    var targetMethods = closedFactoryImplType.GetMembers(t => t.DeclaredMethods, includeBase: true)
                        .Match(m => m.Name == factoryMember.Name && m.GetParameters().Length == factoryMethodParameters.Length)
                        .ToArrayOrSelf();

                    if (targetMethods.Length == 1)
                        factoryMember = targetMethods[0];
                    else // Fallback to MethodHandle only if methods have similar signatures
                    {
                        var methodHandleProperty = typeof(MethodBase).GetTypeInfo()
                            .DeclaredProperties
                            .FindFirst(it => it.Name == "MethodHandle")
                            .ThrowIfNull(Error.OpenGenericFactoryMethodDeclaringTypeIsNotSupportedOnThisPlatform,
                                factoryImplType, closedFactoryImplType, factoryMethodBase.Name);
                        factoryMember = MethodBase.GetMethodFromHandle(
                            (RuntimeMethodHandle)methodHandleProperty.GetValue(factoryMethodBase, ArrayTools.Empty<object>()),
                            closedFactoryImplType.TypeHandle);
                    }
                }
                else if (factoryMember is FieldInfo)
                {
                    factoryMember = closedFactoryImplType.GetMembers(t => t.DeclaredFields, includeBase: true)
                        .Single(f => f.Name == factoryMember.Name);
                }
                else if (factoryMember is PropertyInfo)
                {
                    factoryMember = closedFactoryImplType.GetMembers(t => t.DeclaredProperties, includeBase: true)
                        .Single(f => f.Name == factoryMember.Name);
                }
            }

            // If factory method is actual method and still open-generic after closing its declaring type,
            // then match remaining method type parameters and make closed method
            var openFactoryMethod = factoryMember as MethodInfo;
            if (openFactoryMethod != null && openFactoryMethod.ContainsGenericParameters)
            {
                var methodTypeParams = openFactoryMethod.GetGenericArguments();
                var resultMethodTypeArgs = new Type[methodTypeParams.Length];

                var isMethodClosed = MatchServiceWithImplementedTypeParams(
                    resultMethodTypeArgs, methodTypeParams, implTypeParams, serviceTypeArgs);

                if (!isMethodClosed)
                    return shouldReturnOnError || request.IfUnresolved != IfUnresolved.Throw ? null
                        : Throw.For<FactoryMethod>(Error.NoMatchedFactoryMethodWithServiceTypeArgs,
                            openFactoryMethod, new StringBuilder().Print(serviceTypeArgs, itemSeparator: ", "),
                            request);

                MatchOpenGenericConstraints(methodTypeParams, resultMethodTypeArgs);

                factoryMember = Throw.IfThrows<ArgumentException, MethodInfo>(
                    () => openFactoryMethod.MakeGenericMethod(resultMethodTypeArgs),
                    !shouldReturnOnError && request.IfUnresolved == IfUnresolved.Throw,
                    Error.NoMatchedGenericParamConstraints, factoryImplType, request);

                if (factoryMember == null)
                    return null;
            }

            return FactoryMethod.Of(factoryMember, factoryInfo);
        }

#endregion
    }

    /// <summary>Creates service expression using client provided expression factory delegate.</summary>
    public sealed class ExpressionFactory : Factory
    {
        /// <summary>Wraps provided delegate into factory.</summary>
        /// <param name="getServiceExpression">Delegate that will be used internally to create service expression.</param>
        /// <param name="reuse">(optional) Reuse.</param> <param name="setup">(optional) Setup.</param>
        public ExpressionFactory(Func<Request, Expr> getServiceExpression,
            IReuse reuse = null, Setup setup = null)
            : base(reuse, setup)
        {
            _getServiceExpression = getServiceExpression.ThrowIfNull();
        }

        /// <summary>Creates service expression using wrapped delegate.</summary>
        /// <param name="request">Request to resolve.</param> <returns>Expression returned by stored delegate.</returns>
        public override Expr CreateExpressionOrDefault(Request request) =>
            _getServiceExpression(request);

        private readonly Func<Request, Expr> _getServiceExpression;
    }

    /// <summary>This factory is the thin wrapper for user provided delegate
    /// and where possible it uses delegate directly: without converting it to expression.</summary>
    public sealed class DelegateFactory : Factory
    {
        /// <summary>Non-abstract closed implementation type.</summary>
        public override Type ImplementationType => _knownImplementationType;

        /// <summary>Creates factory.</summary>
        public DelegateFactory(FactoryDelegate factoryDelegate,
           IReuse reuse = null, Setup setup = null, Type knownImplementationType = null)
           : base(reuse, setup)
        {
            _factoryDelegate = factoryDelegate.ThrowIfNull();
            _knownImplementationType = knownImplementationType;
        }

        /// <summary>Create expression by wrapping call to stored delegate with provided request.</summary>
        public override Expr CreateExpressionOrDefault(Request request)
        {
            var factoryDelegateExpr = request.Container.GetItemExpression(_factoryDelegate);
            var resolverExpr = ResolverContext.GetRootOrSelfExpr(request);
            var invokeExpr = Invoke(factoryDelegateExpr, resolverExpr);
            return Convert(invokeExpr, request.GetActualServiceType());
        }

        /// <summary>If possible returns delegate directly, without creating expression trees, just wrapped in <see cref="FactoryDelegate"/>.
        /// If decorator found for request then factory fall-backs to expression creation.</summary>
        /// <param name="request">Request to resolve.</param>
        /// <returns>Factory delegate directly calling wrapped delegate, or invoking expression if decorated.</returns>
        public override FactoryDelegate GetDelegateOrDefault(Request request)
        {
            request = request.WithResolvedFactory(this);

            // Wrap the delegate in respective expression for non-simple use
            if (request.Reuse != DryIoc.Reuse.Transient ||
                FactoryType == FactoryType.Service && request.Container.GetDecoratorExpressionOrDefault(request) != null)
                return base.GetDelegateOrDefault(request);

            // Otherwise just use delegate as-is
            return _factoryDelegate;
        }

        private readonly FactoryDelegate _factoryDelegate;
        private readonly Type _knownImplementationType;
    }

    internal sealed class FactoryPlaceholder : Factory
    {
        public static readonly Factory Default = new FactoryPlaceholder();

        // Always resolved asResolutionCall, to create a hole in object graph to be filled in later
        public override Setup Setup => _setup;
        private static readonly Setup _setup = Setup.With(asResolutionCall: true);

        public override Expr CreateExpressionOrDefault(Request request) =>
            Throw.For<Expr>(Error.NoImplementationForPlaceholder, request);
    }

    /// <summary>Should return value stored in scope.</summary>
    public delegate object CreateScopedValue();

    /// <summary>Lazy object storage that will create object with provided factory on first access,
    /// then will be returning the same object for subsequent access.</summary>
    public interface IScope : IEnumerable<IScope>, IDisposable
    {
        /// <summary>Parent scope in scope stack. Null for root scope.</summary>
        IScope Parent { get; }

        /// <summary>Optional name object associated with scope.</summary>
        object Name { get; }

        /// <summary>True if scope is disposed.</summary>
        bool IsDisposed { get; }

        /// <summary>Looks up for stored item by id.</summary>
        bool TryGet(out object item, int id);

        /// <summary>Creates, stores, and returns stored disposable by id.</summary>
        object GetOrAdd(int id, CreateScopedValue createValue, int disposalIndex = -1);

        /// <summary>Tracked item will be disposed with the scope. 
        /// Smaller <paramref name="disposalIndex"/> will be disposed first.</summary>
        object TrackDisposable(object item, int disposalIndex = -1);

        /// <summary>Sets (replaces) value at specified id, or adds value if no existing id found.</summary>
        void SetOrAdd(int id, object item);

        /// <summary>Clones the scope.</summary>
        IScope Clone();
    }

    /// <summary>Scope implementation to hold and dispose stored <see cref="IDisposable"/> items.
    /// <c>lock</c> is used internally to ensure that object factory called only once.</summary>
    public sealed class Scope : IScope
    {
        /// <summary>Parent scope in scope stack. Null for the root scope.</summary>
        public IScope Parent { get; }

        /// <summary>Optional name associated with scope.</summary>
        public object Name { get; }

        /// <summary>True if scope is disposed.</summary>
        public bool IsDisposed => _disposed == 1;

        /// <summary>Creates scope with optional parent and name.</summary>
        public Scope(IScope parent = null, object name = null)
            : this(parent, name, ImMap<object>.Empty, ImMap<IDisposable>.Empty, int.MaxValue)
        { }

        private Scope(IScope parent, object name, ImMap<object> items,
            ImMap<IDisposable> disposables, int nextDisposalIndex)
        {
            Parent = parent;
            Name = name;
            _items = items;
            _disposables = disposables;
            _nextDisposalIndex = nextDisposalIndex;
        }

        internal static readonly MethodInfo GetOrAddMethod =
            typeof(IScope).Method(nameof(IScope.GetOrAdd));

        /// <inheritdoc />
        public object GetOrAdd(int id, CreateScopedValue createValue, int disposalIndex = -1)
        {
            object value;
            return _items.TryFind(id, out value)
                ? value : TryGetOrAdd(id, createValue, disposalIndex);
        }

        private object TryGetOrAdd(int id, CreateScopedValue createValue, int disposalIndex = -1)
        {
            if (_disposed == 1)
                Throw.It(Error.ScopeIsDisposed, ToString());

            object item;
            lock (_locker)
            {
                if (_items.TryFind(id, out item)) // double check locking
                    return item;

                item = createValue();

                // Swap is required because if _items changed inside createValue, then we need to retry
                var items = _items;
                if (Interlocked.CompareExchange(ref _items, items.AddOrUpdate(id, item), items) != items)
                    Ref.Swap(ref _items, it => it.AddOrUpdate(id, item));
            }

            return TrackDisposable(item, disposalIndex);
        }

        /// <summary>Sets (replaces) value at specified id, or adds value if no existing id found.</summary>
        /// <param name="id">To set value at. Should be >= 0.</param> <param name="item">Value to set.</param>
        public void SetOrAdd(int id, object item)
        {
            if (_disposed == 1)
                Throw.It(Error.ScopeIsDisposed, ToString());
            var items = _items;

            // try to atomically replaced items with the one set item, if attempt failed then lock and replace
            if (Interlocked.CompareExchange(ref _items, items.AddOrUpdate(id, item), items) != items)
                lock (_locker)
                    _items = _items.AddOrUpdate(id, item);

            TrackDisposable(item);
        }

        /// <inheritdoc />
        public IScope Clone() =>
            new Scope(Parent, Name, _items, _disposables, _nextDisposalIndex);

        /// <inheritdoc />
        public bool TryGet(out object item, int id)
        {
            if (_disposed == 1)
                Throw.It(Error.ScopeIsDisposed, ToString());
            return _items.TryFind(id, out item);
        }

        internal static readonly MethodInfo TrackDisposableMethod =
            typeof(IScope).Method(nameof(IScope.TrackDisposable));

        /// <summary>Can be used to manually add service for disposal</summary>
        public object TrackDisposable(object item, int disposalIndex = -1)
        {
            if (item == this)
                return item;

            var disposable = item as IDisposable;
            if (disposable != null)
            {
                if (disposalIndex == -1)
                    disposalIndex = Interlocked.Decrement(ref _nextDisposalIndex);

                var it = _disposables;
                if (Interlocked.CompareExchange(ref _disposables, it.AddOrUpdate(disposalIndex, disposable), it) != it)
                    Ref.Swap(ref _disposables, _ => _.AddOrUpdate(disposalIndex, disposable));
            }
            return item;
        }

        /// <summary>Enumerates all the parent scopes upwards starting from this one.</summary>
        public IEnumerator<IScope> GetEnumerator()
        {
            for (IScope scope = this; scope != null; scope = scope.Parent)
                yield return scope;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>Disposes all stored <see cref="IDisposable"/> objects and empties item storage.</summary>
        /// <remarks>If item disposal throws exception, then it won't be propagated outside,
        /// so the rest of the items may proceed to be disposed.</remarks>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                return;

            var disposables = _disposables;
            if (!disposables.IsEmpty)
                foreach (var disposable in disposables.Enumerate())
                {
                    // Ignoring disposing exception, as it is not important to proceed the disposal
                    try { disposable.Value.Dispose(); }
                    catch (Exception) { }
                }

            _disposables = ImMap<IDisposable>.Empty;

            _items = ImMap<object>.Empty;
        }

        /// <summary>Prints scope info (name and parent) to string for debug purposes.</summary>
        public override string ToString() =>
            (IsDisposed ? "disposed" : "") + "{"
            + (Name != null ? "Name=" + Name : "no name")
            + (Parent != null ? ", Parent=" + Parent : "")
            + "}";

#region Implementation

        private ImMap<object> _items;
        private ImMap<IDisposable> _disposables;
        private int _nextDisposalIndex;
        private int _disposed;

        // todo: Improve perf by scaling lockers count with the items amount
        // Sync root is required to create object only once. The same reason as for Lazy<T>.
        private readonly object _locker = new object();

#endregion
    }

    /// <summary>Delegate to get new scope from old/existing current scope.</summary>
    /// <param name="oldScope">Old/existing scope to change.</param>
    /// <returns>New scope or old if do not want to change current scope.</returns>
    public delegate IScope SetCurrentScopeHandler(IScope oldScope);

    /// <summary>Provides ambient current scope and optionally scope storage for container,
    /// examples are HttpContext storage, Execution context, Thread local.</summary>
    public interface IScopeContext : IDisposable
    {
        /// <summary>Returns current scope or null if no ambient scope available at the moment.</summary>
        /// <returns>Current scope or null.</returns>
        IScope GetCurrentOrDefault();

        /// <summary>Changes current scope using provided delegate. Delegate receives current scope as input and
        /// should return new current scope.</summary>
        /// <param name="setCurrentScope">Delegate to change the scope.</param>
        /// <remarks>Important: <paramref name="setCurrentScope"/> may be called multiple times in concurrent environment.
        /// Make it predictable by removing any side effects.</remarks>
        /// <returns>New current scope. So it is convenient to use method in "using (var newScope = ctx.SetCurrent(...))".</returns>
        IScope SetCurrent(SetCurrentScopeHandler setCurrentScope);
    }

    /// <summary>Tracks one current scope per thread, so the current scope in different tread would be different or null,
    /// if not yet tracked. Context actually stores scope references internally, so it should be disposed to free them.</summary>
    public sealed class ThreadScopeContext : IScopeContext, IDisposable
    {
        /// <summary>Provides static name for context. It is OK because its constant.</summary>
        public static readonly string ScopeContextName = "ThreadScopeContext";

        /// <summary>Returns current scope in calling Thread or null, if no scope tracked.</summary>
        /// <returns>Found scope or null.</returns>
        public IScope GetCurrentOrDefault() =>
            _scopes.GetValueOrDefault(Portable.GetCurrentManagedThreadID()) as IScope;

        /// <summary>Change current scope for the calling Thread.</summary>
        /// <param name="setCurrentScope">Delegate to change the scope given current one (or null).</param>
        /// <remarks>Important: <paramref name="setCurrentScope"/> may be called multiple times in concurrent environment.
        /// Make it predictable by removing any side effects.</remarks>
        public IScope SetCurrent(SetCurrentScopeHandler setCurrentScope)
        {
            var threadId = Portable.GetCurrentManagedThreadID();
            IScope newScope = null;
            Ref.Swap(ref _scopes, scopes =>
                scopes.AddOrUpdate(threadId, newScope = setCurrentScope(scopes.GetValueOrDefault(threadId) as IScope)));
            return newScope;
        }

        /// <summary>Disposed all stored/tracked scopes and empties internal scope storage.</summary>
        public void Dispose()
        {
            if (!_scopes.IsEmpty)
                foreach (var scope in _scopes.Enumerate().Where(scope => scope.Value is IDisposable))
                    scope.Value.Dispose();
            _scopes = ImMap<IScope>.Empty;
        }

        private ImMap<IScope> _scopes = ImMap<IScope>.Empty;
    }

    /// <summary>Simplified scope agnostic reuse abstraction. More easy to implement,
    ///  and more powerful as can be based on other storage beside reuse.</summary>
    public interface IReuse : IConvertibleToExpression
    {
        /// <summary>Relative to other reuses lifespan value.</summary>
        int Lifespan { get; }

        /// <summary>Optional name. Use to find matching scope by the name.
        /// It also may be interpreted as object[] Names for matching with multiple scopes </summary>
        object Name { get; }

        /// <summary>Returns true if reuse can be applied: may check if scope or other reused item storage is present.</summary>
        /// <param name="request">Service request.</param> <returns>Check result.</returns>
        bool CanApply(Request request);

        /// <summary>Returns composed expression.</summary>
        /// <param name="request">info</param>
        /// <param name="serviceFactoryExpr">Service creation expression</param>
        /// <returns>Subject</returns>
        Expr Apply(Request request, Expr serviceFactoryExpr);
    }

    /// <summary>Returns container bound scope for storing singleton objects.</summary>
    public sealed class SingletonReuse : IReuse
    {
        /// <summary>Big lifespan.</summary>
        public const int DefaultLifespan = 1000;

        /// <summary>Relative to other reuses lifespan value.</summary>
        public int Lifespan => DefaultLifespan;

        /// <inheritdoc />
        public object Name => null;

        /// <summary>Returns true because singleton is always available.</summary>
        public bool CanApply(Request request) => true;

        /// <summary>Returns expression call to GetOrAddItem.</summary>
        public Expr Apply(Request request, Expr serviceFactoryExpr) =>
            request.TracksTransientDisposable
            ? Call(ResolverContext.SingletonScopeExpr, Scope.TrackDisposableMethod,
                serviceFactoryExpr, _minusOneExpr)
            : Call(ResolverContext.SingletonScopeExpr, Scope.GetOrAddMethod,
                Constant(request.FactoryID), Lambda<CreateScopedValue>(serviceFactoryExpr), _minusOneExpr);

        private static readonly ConstExpr _minusOneExpr = Constant(-1);

        private static readonly Lazy<Expr> _singletonReuseExpr = new Lazy<Expr>(() =>
            Field(null, typeof(Reuse).Field(nameof(Reuse.Singleton))));

        /// <inheritdoc />
        public Expr ToExpression(Func<object, Expr> fallbackConverter) =>
            _singletonReuseExpr.Value;

        /// <summary>Pretty prints reuse name and lifespan</summary> <returns>Printed string.</returns>
        public override string ToString() =>
            GetType().Name + " {Lifespan=" + Lifespan + "}";
    }

    /// <summary>Specifies that instances are created, stored and disposed together with some scope.</summary>
    public sealed class CurrentScopeReuse : IReuse
    {
        /// <summary>Less than Singleton's</summary>
        public const int DefaultLifespan = 100;

        /// <summary>Relative to other reuses lifespan value.</summary>
        public int Lifespan { get; }

        /// <inheritdoc />
        public object Name { get; }

        /// <summary>Returns true if scope is open and the name is matching with reuse <see cref="Name"/>.</summary>
        public bool CanApply(Request request) =>
            _scopedOrSingleton || Name == null
                ? request.Container.CurrentScope != null
                : request.Container.GetNamedScope(Name, false) != null;

        /// <summary>Creates scoped item creation and access expression.</summary>
        public Expr Apply(Request request, Expr serviceFactoryExpr)
        {
            var rExpr = Container.ResolverContextParamExpr;

            if (request.TracksTransientDisposable)
            {
                if (_scopedOrSingleton)
                    return Call(_trackScopedOrSingletonMethod, rExpr, serviceFactoryExpr);

                var ifNoScopeThrowExpr = Constant(request.IfUnresolved == IfUnresolved.Throw);
                if (Name == null)
                    return Call(_trackScopedMethod, rExpr, ifNoScopeThrowExpr, serviceFactoryExpr);

                var nameExpr = request.Container.GetItemExpression(Name, typeof(object));
                return Call(_trackNameScopedMethod, rExpr, nameExpr, ifNoScopeThrowExpr, serviceFactoryExpr);
            }
            else
            {
                var factoryLambdaExpr = Lambda<CreateScopedValue>(serviceFactoryExpr);
                var idExpr = Constant(request.FactoryID);
                var disposalIndexExpr = Constant(-1);
                if (_scopedOrSingleton)
                    return Call(_getScopedOrSingletonMethod, rExpr, idExpr, factoryLambdaExpr, disposalIndexExpr);

                var ifNoScopeThrowExpr = Constant(request.IfUnresolved == IfUnresolved.Throw);
                if (Name == null)
                    return Call(_getScopedMethod, rExpr, ifNoScopeThrowExpr, idExpr, factoryLambdaExpr, disposalIndexExpr);

                var nameExpr = request.Container.GetItemExpression(Name, typeof(object));
                return Call(_getNameScopedMethod, rExpr, nameExpr, ifNoScopeThrowExpr, idExpr, factoryLambdaExpr, disposalIndexExpr);
            }
        }

        /// <inheritdoc />
        public Expr ToExpression(Func<object, Expr> fallbackConverter) =>
            Name == null && !_scopedOrSingleton ? _scopedExpr.Value
                : _scopedOrSingleton ? _scopedOrSingletonExpr.Value
                : Call(_scopedToMethod.Value, fallbackConverter(Name));

        /// <summary>Pretty prints reuse to string.</summary> <returns>Reuse string.</returns>
        public override string ToString()
        {
            var s = new StringBuilder(GetType().Name + " {");
            if (Name != null)
                s.Append("Name=").Print(Name, "\"").Append(", ");
            return s.Append("Lifespan=").Append(Lifespan).Append("}").ToString();
        }

        /// <summary>Creates reuse optionally specifying its name.</summary>
        public CurrentScopeReuse(object name = null, int lifespan = DefaultLifespan,
            bool scopedOrSingleton = false)
        {
            Name = name;
            Lifespan = lifespan;
            _scopedOrSingleton = scopedOrSingleton;
        }

        private bool _scopedOrSingleton;

        internal static object TrackScopedOrSingleton(IResolverContext r, object item) =>
            (r.CurrentScope ?? r.SingletonScope).TrackDisposable(item);

        private static readonly MethodInfo _trackScopedOrSingletonMethod =
            typeof(CurrentScopeReuse).Method(nameof(TrackScopedOrSingleton), true);

        internal static object GetScopedOrSingleton(IResolverContext r,
            int id, CreateScopedValue createValue, int disposalIndex) =>
            (r.CurrentScope ?? r.SingletonScope).GetOrAdd(id, createValue, disposalIndex);

        private static readonly MethodInfo _getScopedOrSingletonMethod =
            typeof(CurrentScopeReuse).Method(nameof(GetScopedOrSingleton), true);

        internal static object GetScoped(IResolverContext r,
            bool throwIfNoScope, int id, CreateScopedValue createValue, int disposalIndex) =>
            r.GetCurrentScope(throwIfNoScope)?.GetOrAdd(id, createValue, disposalIndex);

        private static readonly MethodInfo _getScopedMethod =
            typeof(CurrentScopeReuse).Method(nameof(GetScoped), true);

        internal static object GetNameScoped(IResolverContext r,
            object scopeName, bool throwIfNoScope, int id, CreateScopedValue createValue, int disposalIndex) =>
            r.GetNamedScope(scopeName, throwIfNoScope)?.GetOrAdd(id, createValue, disposalIndex);

        private static readonly MethodInfo _getNameScopedMethod =
            typeof(CurrentScopeReuse).Method(nameof(GetNameScoped), true);

        internal static object TrackScoped(IResolverContext r, bool throwIfNoScope, object item) =>
            r.GetCurrentScope(throwIfNoScope)?.TrackDisposable(item);

        private static readonly MethodInfo _trackScopedMethod =
            typeof(CurrentScopeReuse).Method(nameof(TrackScoped), true);

        internal static object TrackNameScoped(IResolverContext r,
            object scopeName, bool throwIfNoScope, object item) =>
            r.GetNamedScope(scopeName, throwIfNoScope)?.TrackDisposable(item);

        private static readonly MethodInfo _trackNameScopedMethod =
            typeof(CurrentScopeReuse).Method(nameof(TrackNameScoped), true);

        private readonly Lazy<Expr> _scopedExpr = new Lazy<Expr>(() =>
            Field(null, typeof(Reuse).Field(nameof(Reuse.Scoped))));

        private readonly Lazy<MethodInfo> _scopedToMethod = new Lazy<MethodInfo>(() =>
            typeof(Reuse).Method(nameof(Reuse.ScopedTo), typeof(object)));

        private readonly Lazy<Expr> _scopedOrSingletonExpr = new Lazy<Expr>(() =>
            Field(null, typeof(Reuse).Field(nameof(Reuse.ScopedOrSingleton))));
    }

    /// <summary>Abstracts way to match reuse and scope names</summary>
    public interface IScopeName
    {
        /// <summary>Does the job.</summary>
        bool Match(object scopeName);
    }

    /// <summary>Represents multiple names</summary>
    public sealed class CompositeScopeName : IScopeName
    {
        /// <summary>Wraps the multiple names</summary>
        public static CompositeScopeName Of(object[] names) => new CompositeScopeName(names);

        /// <summary>Matches all the name in a loop until first match is found, otherwise returns false.</summary>
        public bool Match(object scopeName)
        {
            for (int i = 0; i < _names.Length; i++)
            {
                var name = _names[i];
                if (name == scopeName)
                    return true;
                var aScopeName = name as IScopeName;
                if (aScopeName != null && aScopeName.Match(scopeName))
                    return true;
                if (scopeName != null && scopeName.Equals(name))
                    return true;
            }

            return false;
        }

        private CompositeScopeName(object[] names)
        {
            _names = names;
        }

        private readonly object[] _names;
    }

    /// <summary>Holds the name for the resolution scope.</summary>
    public sealed class ResolutionScopeName : IScopeName
    {
        /// <summary>Creates scope with specified service type and key</summary>
        public static ResolutionScopeName Of(Type serviceType = null, object serviceKey = null) =>
            new ResolutionScopeName(serviceType, serviceKey);

        /// <summary>Creates scope with specified service type and key.</summary>
        public static ResolutionScopeName Of<TService>(object serviceKey = null) =>
            new ResolutionScopeName(typeof(TService), serviceKey);

        /// <summary>Type of service opening the scope.</summary>
        public readonly Type ServiceType;

        /// <summary>Optional service key of service opening the scope.</summary>
        public readonly object ServiceKey;

        private ResolutionScopeName(Type serviceType, object serviceKey)
        {
            ServiceType = serviceType;
            ServiceKey = serviceKey;
        }

        /// <inheritdoc />
        public bool Match(object scopeName)
        {
            var name = scopeName as ResolutionScopeName;
            if (name == null)
                return false;

            return (ServiceType == null ||
                name.ServiceType.IsAssignableTo(ServiceType) ||
                ServiceType.IsOpenGeneric() && name.ServiceType.GetGenericDefinitionOrNull().IsAssignableTo(ServiceType)) &&
                (ServiceKey == null || ServiceKey.Equals(name.ServiceKey));

        }

        /// <summary>String representation for easy debugging and understood error messages.</summary>
        public override string ToString()
        {
            var s = new StringBuilder(GetType().Name).Print(ServiceType);
            if (ServiceKey != null)
                s.Append(',').Print(ServiceKey);
            return s.Append(")").ToString();
        }
    }

    /// <summary>Specifies pre-defined reuse behaviors supported by container:
    /// used when registering services into container with <see cref="Registrator"/> methods.</summary>
    public static class Reuse
    {
        /// <summary>Synonym for absence of reuse.</summary>
        public static readonly IReuse Transient = new TransientReuse();

        /// <summary>Specifies to store single service instance per <see cref="Container"/>.</summary>
        public static readonly IReuse Singleton = new SingletonReuse();

        /// <summary>Same as InCurrentScope. From now on will be the default name.</summary>
        public static readonly IReuse Scoped = new CurrentScopeReuse();

        /// <summary>Same as InCurrentNamedScope. From now on will be the default name.</summary>
        public static IReuse ScopedTo(object name) => new CurrentScopeReuse(name);

        /// <summary>Scoped to multiple names.</summary>
        public static IReuse ScopedTo(params object[] names) =>
            names.IsNullOrEmpty() ? Scoped :
            names.Length == 1 ? ScopedTo(names[0]) :
            new CurrentScopeReuse(CompositeScopeName.Of(names));

        /// <summary>Same as InResolutionScopeOf. From now on will be the default name.</summary>
        public static IReuse ScopedTo(Type serviceType = null, object serviceKey = null) =>
            serviceType == null && serviceKey == null ? Scoped
            : new CurrentScopeReuse(ResolutionScopeName.Of(serviceType, serviceKey));

        /// <summary>Same as InResolutionScopeOf. From now on will be the default name.</summary>
        public static IReuse ScopedTo<TService>(object serviceKey = null) =>
            ScopedTo(typeof(TService), serviceKey);

        /// <summary>The same as <see cref="InCurrentScope"/> but if no open scope available will fallback to <see cref="Singleton"/></summary>
        /// <remarks>The <see cref="Error.DependencyHasShorterReuseLifespan"/> is applied the same way as for <see cref="InCurrentScope"/> reuse.</remarks>
        public static readonly IReuse ScopedOrSingleton = new CurrentScopeReuse(scopedOrSingleton: true);

        /// <summary>Obsolete: same as <see cref="Scoped"/>.</summary>
        [Obsolete("Now it is the same as Reuse.Scoped, please prefer it or use Reuse.ScopedTo to specify bound service")]
        public static readonly IReuse InResolutionScope = Scoped;

        /// <summary>Obsolete: same as <see cref="Scoped"/>.</summary>
        public static readonly IReuse InCurrentScope = Scoped;

        /// <summary>Returns current scope reuse with specific name to match with scope.
        /// If name is not specified then function returns <see cref="InCurrentScope"/>.</summary>
        /// <param name="name">(optional) Name to match with scope.</param>
        /// <returns>Created current scope reuse.</returns>
        public static IReuse InCurrentNamedScope(object name = null) =>
            ScopedTo(name);

        /// <summary>Creates reuse to search for <paramref name="assignableFromServiceType"/> and <paramref name="serviceKey"/>
        /// in existing resolution scope hierarchy. If parameters are not specified or null, then <see cref="Scoped"/> will be returned.</summary>
        public static IReuse InResolutionScopeOf(Type assignableFromServiceType = null, object serviceKey = null) =>
            ScopedTo(assignableFromServiceType, serviceKey);

        /// <summary>Creates reuse to search for <typeparamref name="TAssignableFromServiceType"/> and <paramref name="serviceKey"/>
        /// in existing resolution scope hierarchy.</summary>
        public static IReuse InResolutionScopeOf<TAssignableFromServiceType>(object serviceKey = null) =>
            ScopedTo<TAssignableFromServiceType>(serviceKey);

        /// <summary>Same as Scoped but requires <see cref="ThreadScopeContext"/>.</summary>
        public static readonly IReuse InThread = Scoped;

        /// <summary>Special name that by convention recognized by <see cref="InWebRequest"/>.</summary>
        public static readonly string WebRequestScopeName = "WebRequestScopeName";

        /// <summary>Web request is just convention for reuse in <see cref="InCurrentNamedScope"/> with special name <see cref="WebRequestScopeName"/>.</summary>
        public static readonly IReuse InWebRequest = InCurrentNamedScope(WebRequestScopeName);

#region Implementation

        /// <summary>No-reuse</summary>
        private sealed class TransientReuse : IReuse
        {
            public int Lifespan => 0;

            public object Name => null;

            public Expr Apply(Request _, Expr serviceFactoryExpr) => serviceFactoryExpr;

            public bool CanApply(Request request) => true;

            private readonly Lazy<Expr> _transientReuseExpr = new Lazy<Expr>(() =>
                Field(null, typeof(Reuse).Field(nameof(Transient))));

            public Expr ToExpression(Func<object, Expr> fallbackConverter) =>
                _transientReuseExpr.Value;

            public override string ToString() => "TransientReuse";
        }

#endregion
    }

    /// <summary>Policy to handle unresolved service.</summary>
    public enum IfUnresolved
    {
        /// <summary>If service is unresolved for whatever means, it will throw the respective exception.</summary>
        Throw,
        /// <summary>If service is unresolved for whatever means, it will return default(serviceType) value.</summary>
        ReturnDefault,
        /// <summary>If service is not registered, then it will return default, for other errors it will throw.</summary>
        ReturnDefaultIfNotRegistered,
    }

    /// <summary>Nested dependency path information.</summary>
    public sealed class RequestInfo : IEnumerable<RequestInfo>
    {
#region State carried with each request info (think how to minimize it)

        /// <summary>Represents an empty info.</summary>
        public static readonly RequestInfo Empty = new RequestInfo();

        /// <summary>Represents an empty info and indicates an open resolution scope.</summary>
        public static readonly RequestInfo EmptyOpensResolutionScope = new RequestInfo(opensResolutionScope: true);

        /// <summary>Parent request or null for root resolution request.</summary>
        public readonly RequestInfo DirectParent;

        /// <summary>Wraps the resolved service lookup details.</summary>
        public readonly IServiceInfo ServiceInfo;

        /// <summary>Resolved factory ID, used to identify applied decorator.</summary>
        public readonly int FactoryID;

        /// <summary>Type of factory: Service, Wrapper, or Decorator.</summary>
        public readonly FactoryType FactoryType;

        /// <summary>Service implementation type if known.</summary>
        public readonly Type ImplementationType;

        /// <summary>Service reuse.</summary>
        public readonly IReuse Reuse;

        /// <summary>The options and check results propagated with request from <see cref="RequestFlags"/>.</summary>
        public readonly RequestFlags Flags;

        /// <summary>ID of decorated factory in case of decorator factory type</summary>
        public readonly int DecoratedFactoryID;

#endregion

        /// <summary>Returns true for an empty request.</summary>
        public bool IsEmpty => ServiceInfo == null;

        /// <summary>Returns true if request is the first in a chain.</summary>
        public bool IsResolutionRoot => !IsEmpty && DirectParent.IsEmpty;

        /// <summary>Returns service parent skipping wrapper if any. 
        /// To get direct parent use <see cref="DirectParent"/>.</summary>
        public RequestInfo Parent
        {
            get
            {
                if (IsEmpty)
                    return Empty;
                var p = DirectParent;
                while (!p.IsEmpty && p.FactoryType == FactoryType.Wrapper)
                    p = p.DirectParent;
                return p;
            }
        }

        /// <summary>Requested service type.</summary>
        public Type ServiceType =>
            ServiceInfo?.ServiceType;

        /// <summary>Required service type if specified.</summary>
        public Type RequiredServiceType =>
            ServiceInfo?.Details.RequiredServiceType;

        /// <summary>Returns <see cref="RequiredServiceType"/> if it is specified and assignable to <see cref="ServiceType"/>,
        /// otherwise returns <see cref="ServiceType"/>.</summary>
        public Type GetActualServiceType() =>
            ServiceInfo.GetActualServiceType();

        /// <summary>Returns known implementation, or otherwise actual service type.</summary>
        /// <returns>The subject.</returns>
        public Type GetKnownImplementationOrServiceType() =>
            ImplementationType ?? GetActualServiceType();

        /// <summary>Policy to deal with unresolved request.</summary>
        public IfUnresolved IfUnresolved =>
            ServiceInfo == null ? IfUnresolved.Throw : ServiceInfo.Details.IfUnresolved;

        /// <summary>Optional service key to identify service of the same type.</summary>
        public object ServiceKey =>
            ServiceInfo?.Details.ServiceKey;

        /// <summary>Metadata key to find in metadata dictionary in resolved service.</summary>
        public string MetadataKey =>
            ServiceInfo?.Details.MetadataKey;

        /// <summary>Metadata or the value (if key specified) to find in resolved service.</summary>
        public object Metadata =>
            ServiceInfo?.Details.Metadata;

        /// <summary>Relative number representing reuse lifespan.</summary>
        public int ReuseLifespan =>
            Reuse == null ? 0 : Reuse.Lifespan;

        /// <summary>Indicates the service opening resolution scope.</summary>
        public bool OpensResolutionScope =>
            (Flags & RequestFlags.OpensResolutionScope) != 0;

        #region Used in generated expression

        internal static readonly Lazy<MethodInfo> PushMethodWith4Args = new Lazy<MethodInfo>(GetPushMethodWith4Args);
        private static MethodInfo GetPushMethodWith4Args() => typeof(RequestInfo).Method(nameof(Push),
            typeof(Type), typeof(int), typeof(Type), typeof(IReuse));

        /// <summary>Creates info by supplying all the properties and chaining it with current (parent) info.</summary>
        public RequestInfo Push(Type serviceType, 
            int factoryID, Type implementationType, IReuse reuse) =>
            Push(serviceType, null, null, null, null, IfUnresolved.Throw,
                factoryID, FactoryType.Service, implementationType, reuse, default(RequestFlags));

        internal static readonly Lazy<MethodInfo> PushMethodWith8Args = new Lazy<MethodInfo>(GetPushMethodWith8Args);
        private static MethodInfo GetPushMethodWith8Args() => typeof(RequestInfo).Method(nameof(Push),
            typeof(Type), typeof(Type), typeof(object), 
            typeof(int), typeof(FactoryType), typeof(Type), typeof(IReuse), typeof(RequestFlags));

        /// <summary>Creates info by supplying all the properties and chaining it with current (parent) info.</summary>
        public RequestInfo Push(Type serviceType, Type requiredServiceType, object serviceKey,
            int factoryID, FactoryType factoryType, Type implementationType, IReuse reuse, RequestFlags flags) =>
            Push(serviceType, requiredServiceType, serviceKey, null, null, IfUnresolved.Throw,
                factoryID, factoryType, implementationType, reuse, flags);

        internal static readonly Lazy<MethodInfo> PushMethodWith9Args = new Lazy<MethodInfo>(GetPushMethodWith9Args);
        private static MethodInfo GetPushMethodWith9Args() => typeof(RequestInfo).Method(nameof(Push),
            typeof(Type), typeof(Type), typeof(object), typeof(IfUnresolved),
            typeof(int), typeof(FactoryType), typeof(Type), typeof(IReuse), typeof(RequestFlags));

        /// <summary>Creates info by supplying all the properties and chaining it with current (parent) info.</summary>
        public RequestInfo Push(Type serviceType, Type requiredServiceType, object serviceKey, IfUnresolved ifUnresolved,
            int factoryID, FactoryType factoryType, Type implementationType, IReuse reuse, RequestFlags flags) =>
            Push(serviceType, requiredServiceType, serviceKey, null, null, ifUnresolved,
                factoryID, factoryType, implementationType, reuse, flags);

        internal static readonly Lazy<MethodInfo> PushMethodWith11Args = new Lazy<MethodInfo>(GetPushMethodWith11Args);
        private static MethodInfo GetPushMethodWith11Args() => typeof(RequestInfo).Method(nameof(Push),
            typeof(Type), typeof(Type), typeof(object), typeof(string), typeof(object), typeof(IfUnresolved),
            typeof(int), typeof(FactoryType), typeof(Type), typeof(IReuse), typeof(RequestFlags));

        /// <summary>Creates info by supplying all the properties and chaining it with current (parent) info.</summary>
        public RequestInfo Push(Type serviceType, Type requiredServiceType, object serviceKey, string metadataKey, object metadata, IfUnresolved ifUnresolved,
            int factoryID, FactoryType factoryType, Type implementationType, IReuse reuse, RequestFlags flags) =>
            Push(DryIoc.ServiceInfo.Of(serviceType, requiredServiceType, ifUnresolved, serviceKey, metadataKey, metadata),
                factoryID, factoryType, implementationType, reuse, flags);

        #endregion

        /// <summary>Creates info by supplying all the properties and chaining it with current (parent) info.</summary>
        public RequestInfo Push(IServiceInfo serviceInfo,
            int factoryID = 0, FactoryType factoryType = FactoryType.Service, Type implementationType = null, IReuse reuse = null,
            RequestFlags flags = default(RequestFlags), int decoratedFactoryID = 0) =>
            new RequestInfo(this, serviceInfo, factoryID, factoryType, implementationType, reuse, flags, decoratedFactoryID);

        /// <summary>Obsolete: now request is directly implements the <see cref="IEnumerable{T}"/>.</summary>
        public IEnumerable<RequestInfo> Enumerate() => this;

        /// <summary>Returns all non-empty requests starting from the current request and ending with the root parent.
        /// Returns empty sequence for an empty request.</summary>
        public IEnumerator<RequestInfo> GetEnumerator()
        {
            for (var i = this; !i.IsEmpty; i = i.DirectParent)
                yield return i;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>Prints request without parents into the passed builder.</summary>
        public StringBuilder PrintCurrent(StringBuilder s)
        {
            if (IsEmpty)
                return s.Append("{empty}");

            if (Reuse != null && Reuse != DryIoc.Reuse.Transient)
                s.Append(Reuse is SingletonReuse ? "singleton" : "scoped").Append(' ');

            if (FactoryType != FactoryType.Service)
                s.Append(FactoryType.ToString().ToLower()).Append(' ');

            if (ImplementationType != null && ImplementationType != ServiceType)
                s.Print(ImplementationType).Append(": ");

            s.Append(ServiceInfo);
            return s;
        }

        /// <summary>Prints request with all its parents.</summary>
        public StringBuilder Print(StringBuilder s)
        {
            s = PrintCurrent(s);
            if (!DirectParent.IsEmpty)
                DirectParent.Print(s.AppendLine().Append("  in ")); // recursion
            return s;
        }

        /// <summary>Prints request with all its parents to string.</summary> <returns>The string.</returns>
        public override string ToString() =>
            Print(new StringBuilder()).ToString();

        /// <summary>Returns true if request info and passed object are equal, and their parents recursively are equal.</summary>
        public override bool Equals(object obj) =>
            Equals(obj as RequestInfo);

        /// <summary>Returns true if request info and passed info are equal, and their parents recursively are equal.</summary>
        public bool Equals(RequestInfo other) =>
            other != null && EqualsWithoutParent(other)
                && (DirectParent == null && other.DirectParent == null
                || (DirectParent != null && DirectParent.EqualsWithoutParent(other.DirectParent)));

        /// <summary>Compares info's regarding properties but not their parents.</summary>
        public bool EqualsWithoutParent(RequestInfo other) =>
            other.ServiceType == ServiceType

            && other.Flags == Flags

            && other.RequiredServiceType == RequiredServiceType
            && other.IfUnresolved == IfUnresolved
            && Equals(other.ServiceKey, ServiceKey)
            && other.MetadataKey == MetadataKey
            && Equals(other.Metadata, Metadata)

            && other.FactoryType == FactoryType
            && other.ImplementationType == ImplementationType
            && other.ReuseLifespan == ReuseLifespan;

        /// <summary>Compares info's regarding properties but not their parents.</summary>
        public bool EqualsWithoutParent(Request other) =>
            other.ServiceType == ServiceType

            && other.Flags == Flags

            && other.RequiredServiceType == RequiredServiceType
            && other.IfUnresolved == IfUnresolved
            && Equals(other.ServiceKey, ServiceKey)
            && other.MetadataKey == MetadataKey
            && Equals(other.Metadata, Metadata)

            && other.FactoryType == FactoryType
            && other.ImplementationType == ImplementationType
            && other.ReuseLifespan == ReuseLifespan;

        // todo: Calculate and persist the hash on Push
        /// <summary>Calculates the combined hash code based on factory IDs.</summary>
        public override int GetHashCode()
        {
            if (IsEmpty)
                return 0;

            var hash = FactoryID;
            var parent = DirectParent;
            while (!parent.IsEmpty)
            {
                hash = CombineHashCodes(hash, parent.FactoryID);
                parent = parent.DirectParent;
            }

            return hash;
        }

        private RequestInfo(bool opensResolutionScope = false)
        {
            if (opensResolutionScope)
                Flags = RequestFlags.OpensResolutionScope;
        }

        private RequestInfo(RequestInfo directParent, IServiceInfo serviceInfo,
            int factoryID, FactoryType factoryType, Type implementationType, IReuse reuse,
            RequestFlags flags,
            int decoratedFactoryID)
        {
            DirectParent = directParent;

            ServiceInfo = serviceInfo;

            // Implementation info:
            FactoryID = factoryID;
            FactoryType = factoryType;
            ImplementationType = implementationType;
            Reuse = reuse;

            Flags = flags;

            DecoratedFactoryID = decoratedFactoryID;
        }

        // Inspired by System.Tuple.CombineHashCodes
        private static int CombineHashCodes(int h1, int h2)
        {
            unchecked
            {
                return (h1 << 5) + h1 ^ h2;
            }
        }
    }

    /// <summary>Declares minimal API for service resolution. 
    /// Resolve default and keyed is separated because of optimization for faster resolution of the forner.</summary>
    public interface IResolver
    {
        /// <summary>Resolves default (non-keyed) service from container and returns created service object.</summary>
        /// <param name="serviceType">Service type to search and to return.</param>
        /// <param name="ifUnresolved">Says what to do if service is unresolved.</param>
        /// <returns>Created service object or default based on <paramref name="ifUnresolved"/> provided.</returns>
        object Resolve(Type serviceType, IfUnresolved ifUnresolved);

        /// <summary>Resolves service instance from container.</summary>
        /// <param name="serviceType">Service type to search and to return.</param>
        /// <param name="serviceKey">(optional) service key used for registering service.</param>
        /// <param name="ifUnresolved">(optional) Says what to do if service is unresolved.</param>
        /// <param name="requiredServiceType">(optional) Registered or wrapped service type to use instead of <paramref name="serviceType"/>,
        ///     or wrapped type for generic wrappers.  The type should be assignable to return <paramref name="serviceType"/>.</param>
        /// <param name="preResolveParent">(optional) Dependency chain info.</param>
        /// <param name="args">(optional) For Func{args} propagation through Resolve call boundaries.</param>
        /// <returns>Created service object or default based on <paramref name="ifUnresolved"/> parameter.</returns>
        object Resolve(Type serviceType, object serviceKey,
            IfUnresolved ifUnresolved, Type requiredServiceType, RequestInfo preResolveParent, object[] args);

        /// <summary>Resolves all services registered for specified <paramref name="serviceType"/>, or if not found returns
        /// empty enumerable. If <paramref name="serviceType"/> specified then returns only (single) service registered with
        /// this type.</summary>
        /// <param name="serviceType">Return type of an service item.</param>
        /// <param name="serviceKey">(optional) Resolve only single service registered with the key.</param>
        /// <param name="requiredServiceType">(optional) Actual registered service to search for.</param>
        /// <param name="preResolveParent">Dependency resolution path info.</param>
        /// <param name="args">(optional) For Func{args} propagation through Resolve call boundaries.</param>
        /// <returns>Enumerable of found services or empty. Does Not throw if no service found.</returns>
        IEnumerable<object> ResolveMany(Type serviceType, object serviceKey,
            Type requiredServiceType, RequestInfo preResolveParent, object[] args);
    }

    /// <summary>Specifies options to handle situation when registered service is already present in the registry.</summary>
    public enum IfAlreadyRegistered
    {
        /// <summary>Appends new default registration or throws registration with the same key.</summary>
        AppendNotKeyed,
        /// <summary>Throws if default or registration with the same key is already exist.</summary>
        Throw,
        /// <summary>Keeps old default or keyed registration ignoring new registration: ensures Register-Once semantics.</summary>
        Keep,
        /// <summary>Replaces old registration with new one.</summary>
        Replace,
        /// <summary>Adds the new implementation or null (Made.Of),
        /// otherwise keeps the previous registration of the same implementation type.</summary>
        AppendNewImplementation
    }

    /// <summary>Define registered service structure.</summary>
    public struct ServiceRegistrationInfo : IComparable<ServiceRegistrationInfo>
    {
        /// <summary>Required service type.</summary>
        public Type ServiceType;

        /// <summary>Is null single default service, or actual service key, or <see cref="DefaultKey"/> for multiple default services.</summary>
        public object OptionalServiceKey;

        /// <summary>Registered factory.</summary>
        public Factory Factory;

        /// <summary>Provides registration order across all factory registrations in container.</summary>
        /// <remarks>May be repeated for factory registered with multiple services.</remarks>
        public int FactoryRegistrationOrder;

        /// <summary>Creates info. Registration order is figured out automatically based on Factory.</summary>
        /// <param name="factory"></param> <param name="serviceType"></param> <param name="optionalServiceKey"></param>
        public ServiceRegistrationInfo(Factory factory, Type serviceType, object optionalServiceKey)
        {
            ServiceType = serviceType;
            OptionalServiceKey = optionalServiceKey;
            Factory = factory;
            FactoryRegistrationOrder = factory.FactoryID;
        }

        /// <inheritdoc />
        public int CompareTo(ServiceRegistrationInfo other) =>
            FactoryRegistrationOrder - other.FactoryRegistrationOrder;

        /// <summary>Pretty-prints info to string.</summary> <returns>The string.</returns>
        public override string ToString()
        {
            var s = new StringBuilder();

            s.Print(ServiceType);

            if (OptionalServiceKey != null)
                s.Append(" with ServiceKey=").Print(OptionalServiceKey, "\"");

            s.Append(" registered as factory ").Append(Factory);

            return s.ToString();
        }
    }

    /// <summary>Defines operations that for changing registry, and checking if something exist in registry.</summary>
    public interface IRegistrator
    {
        /// <summary>Returns all registered service factories with their Type and optional Key.</summary>
        /// <returns>Existing registrations.</returns>
        /// <remarks>Decorator and Wrapper types are not included.</remarks>
        IEnumerable<ServiceRegistrationInfo> GetServiceRegistrations();

        /// <summary>Registers factory in registry with specified service type and key for lookup.
        /// Returns true if factory was added to registry, false otherwise. False may be in case of <see cref="IfAlreadyRegistered.Keep"/>
        /// setting and already existing factory</summary>
        /// <param name="factory">To register.</param>
        /// <param name="serviceType">Service type as unique key in registry for lookup.</param>
        /// <param name="serviceKey">Service key as complementary lookup for the same service type.</param>
        /// <param name="ifAlreadyRegistered">Policy how to deal with already registered factory with same service type and key.</param>
        /// <param name="isStaticallyChecked">Confirms that service and implementation types are statically checked by compiler.</param>
        /// <returns>True if factory was added to registry, false otherwise.
        /// False may be in case of <see cref="IfAlreadyRegistered.Keep"/> setting and already existing factory.</returns>
        void Register(Factory factory, Type serviceType, object serviceKey, IfAlreadyRegistered? ifAlreadyRegistered, bool isStaticallyChecked);

        /// <summary>Returns true if expected factory is registered with specified service key and type.</summary>
        /// <param name="serviceType">Type to lookup.</param>
        /// <param name="serviceKey">(optional) Identifies registration via service key.
        /// Not provided or <c>null</c> service key means to check the <paramref name="serviceType"/> alone with any service key.</param>
        /// <param name="factoryType">Expected factory type.</param>
        /// <param name="condition">Expected factory condition.</param>
        /// <returns>True if expected factory found in registry.</returns>
        bool IsRegistered(Type serviceType, object serviceKey, FactoryType factoryType, Func<Factory, bool> condition);

        /// <summary>Removes factory with specified service type and key from registry.</summary>
        /// <param name="serviceType">Type to lookup.</param>
        /// <param name="serviceKey">Key to lookup for the same type.</param>
        /// <param name="factoryType">Expected factory type.</param>
        /// <param name="condition">Expected factory condition.</param>
        void Unregister(Type serviceType, object serviceKey, FactoryType factoryType, Func<Factory, bool> condition);
    }

    /// <summary>What to do with scope.</summary>
    public enum WithSingletonOptions
    {
        /// <summary>Keep state</summary>
        Keep = 0,
        /// <summary>Remove state</summary>
        Drop,
        /// <summary>Clone or copy state</summary>
        Clone
    }

    /// <summary>What to do with registration.</summary>
    public enum WithRegistrationsOptions
    {
        /// <summary>Both containers share the resgitrations, changed in one, will change in another.</summary>
        Share = 0,
        /// <summary>Both resgitrations and cache</summary>
        Clone,
        /// <summary>Clones registrations but drops the cache</summary>
        CloneWithoutCache
    }

    /// <summary>Combines registrator and resolver roles, plus rules and scope management.</summary>
    public interface IContainer : IRegistrator, IResolverContext
    {
        /// <summary>Rules for defining resolution/registration behavior throughout container.</summary>
        Rules Rules { get; }

        /// <summary>Creates new container from the current one.</summary>
        IContainer With(Rules rules, IScopeContext scopeContext,
            WithRegistrationsOptions registrations,
            WithSingletonOptions singletonOptions = WithSingletonOptions.Keep);

        /// <summary>Produces new container which prevents any further registrations.</summary>
        /// <param name="ignoreInsteadOfThrow">(optional)Controls what to do with registrations: ignore or throw exception.
        /// Throws exception by default.</param>
        /// <returns>New container preserving all current container state but disallowing registrations.</returns>
        IContainer WithNoMoreRegistrationAllowed(bool ignoreInsteadOfThrow = false);

        /// <summary>Searches for requested factory in registry, and then using <see cref="DryIoc.Rules.UnknownServiceResolvers"/>.</summary>
        /// <param name="request">Factory request.</param>
        /// <returns>Found factory, otherwise null if <see cref="Request.IfUnresolved"/> is set to <see cref="IfUnresolved.ReturnDefault"/>.</returns>
        Factory ResolveFactory(Request request);

        /// <summary>Searches for registered service factory and returns it, or null if not found.
        /// Will use <see cref="DryIoc.Rules.FactorySelector"/> if specified.</summary>
        /// <param name="request">Factory request.</param>
        /// <returns>Found factory or null.</returns>
        Factory GetServiceFactoryOrDefault(Request request);

        /// <summary>Finds all registered default and keyed service factories and returns them.
        /// It skips decorators and wrappers.</summary>
        /// <param name="serviceType">Service type to look for, may be open-generic type too.</param>
        /// <param name="bothClosedAndOpenGenerics">(optional) For generic serviceType instructs to look for
        /// both closed and open-generic registrations.</param>
        /// <returns>Enumerable of found pairs.</returns>
        /// <remarks>Returned Key item should not be null - it should be <see cref="DefaultKey.Value"/>.</remarks>
        IEnumerable<KV<object, Factory>> GetAllServiceFactories(Type serviceType, bool bothClosedAndOpenGenerics = false);

        /// <summary>Searches for registered wrapper factory and returns it, or null if not found.</summary>
        /// <param name="serviceType">Service type to look for.</param> <returns>Found wrapper factory or null.</returns>
        Factory GetWrapperFactoryOrDefault(Type serviceType);

        /// <summary>Returns all decorators registered for the service type.</summary> <returns>Decorator factories.</returns>
        Factory[] GetDecoratorFactoriesOrDefault(Type serviceType);

        /// <summary>Creates decorator expression: it could be either Func{TService,TService},
        /// or service expression for replacing decorators.</summary>
        /// <param name="request">Decorated service request.</param>
        /// <returns>Decorator expression.</returns>
        Expr GetDecoratorExpressionOrDefault(Request request);

        /// <summary>If <paramref name="serviceType"/> is generic type then this method checks if the type registered as generic wrapper,
        /// and recursively unwraps and returns its type argument. This type argument is the actual service type we want to find.
        /// Otherwise, method returns the input <paramref name="serviceType"/>.</summary>
        /// <param name="serviceType">Type to unwrap. Method will return early if type is not generic.</param>
        /// <param name="requiredServiceType">Required service type or null if don't care.</param>
        /// <returns>Unwrapped service type in case it corresponds to registered generic wrapper, or input type in all other cases.</returns>
        Type GetWrappedType(Type serviceType, Type requiredServiceType);

        /// <summary>Adds factory expression to cache identified by factory ID (<see cref="Factory.FactoryID"/>).</summary>
        /// <param name="factoryID">Key in cache.</param>
        /// <param name="factoryExpression">Value to cache.</param>
        void CacheFactoryExpression(int factoryID, Expr factoryExpression);

        /// <summary>Searches and returns cached factory expression, or null if not found.</summary>
        /// <param name="factoryID">Factory ID to lookup by.</param> <returns>Found expression or null.</returns>
        Expr GetCachedFactoryExpressionOrDefault(int factoryID);

        /// <summary>Converts known items into custom expression or wraps in a constant expression.</summary>
        /// <param name="item">Item to convert.</param>
        /// <param name="itemType">(optional) Type of item, otherwise item <see cref="object.GetType()"/>.</param>
        /// <param name="throwIfStateRequired">(optional) Throws for non-primitive and not-recognized items,
        /// identifying that result expression require run-time state. For compiled expression it means closure in lambda delegate.</param>
        /// <returns>Returns constant or state access expression for added items.</returns>
        Expr GetItemExpression(object item, Type itemType = null, bool throwIfStateRequired = false);

        /// <summary>Clears cache for specified service(s). But does not clear instances of already resolved/created singletons and scoped services!</summary>
        /// <param name="serviceType">Target service type.</param>
        /// <param name="factoryType">(optional) If not specified, clears cache for all <see cref="FactoryType"/>.</param>
        /// <param name="serviceKey">(optional) If omitted, the cache will be cleared for all registrations of <paramref name="serviceType"/>.</param>
        /// <returns>True if target service was found, false - otherwise.</returns>
        bool ClearCache(Type serviceType, FactoryType? factoryType, object serviceKey);
    }

    /// <summary>Resolves all registered services of <typeparamref name="TService"/> type on demand,
    /// when enumerator <see cref="IEnumerator.MoveNext"/> called. If service type is not found, empty returned.</summary>
    /// <typeparam name="TService">Service type to resolve.</typeparam>
    public sealed class LazyEnumerable<TService> : IEnumerable<TService>
    {
        /// <summary>Exposes internal items enumerable.</summary>
        public readonly IEnumerable<TService> Items;

        /// <summary>Wraps lazy resolved items.</summary> <param name="items">Lazy resolved items.</param>
        public LazyEnumerable(IEnumerable<TService> items)
        {
            Items = items.ThrowIfNull();
        }

        /// <summary>Return items enumerator.</summary> 
        public IEnumerator<TService> GetEnumerator() => Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>Wrapper type to box service with associated arbitrary metadata object.</summary>
    /// <typeparam name="T">Service type.</typeparam>
    /// <typeparam name="TMetadata">Arbitrary metadata object type.</typeparam>
    public sealed class Meta<T, TMetadata>
    {
        /// <summary>Value or object with associated metadata.</summary>
        public readonly T Value;

        /// <summary>Associated metadata object. Could be anything.</summary>
        public readonly TMetadata Metadata;

        /// <summary>Boxes value and its associated metadata together.</summary>
        /// <param name="value">value</param> <param name="metadata">any metadata object</param>
        public Meta(T value, TMetadata metadata)
        {
            Value = value;
            Metadata = metadata;
        }
    }

    /// <summary>Exception that container throws in case of error. Dedicated exception type simplifies
    /// filtering or catching container relevant exceptions from client code.</summary>
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable",
        Justification = "Not available in PCL.")]
    public class ContainerException : InvalidOperationException
    {
        /// <summary>Error code of exception, possible values are listed in <see cref="Error"/> class.</summary>
        public readonly int Error;

        /// <summary>Creates exception by wrapping <paramref name="errorCode"/> and its message,
        /// optionally with <paramref name="innerException"/> exception.</summary>
        public static ContainerException Of(ErrorCheck errorCheck, int errorCode,
            object arg0, object arg1 = null, object arg2 = null, object arg3 = null,
            Exception innerException = null)
        {
            var messageFormat = GetMessage(errorCheck, errorCode);
            var message = string.Format(messageFormat, Print(arg0), Print(arg1), Print(arg2), Print(arg3));
            return new ContainerException(errorCode, message, innerException);
        }

        /// <summary>Gets error message based on provided args.</summary> <param name="errorCheck"></param> <param name="errorCode"></param>
        protected static string GetMessage(ErrorCheck errorCheck, int errorCode) =>
            errorCode == -1 ? Throw.GetDefaultMessage(errorCheck) : DryIoc.Error.Messages[errorCode];

        /// <summary>Prints argument for formatted message.</summary> <param name="arg">To print.</param> <returns>Printed string.</returns>
        protected static string Print(object arg) =>
            arg == null ? string.Empty : new StringBuilder().Print(arg).ToString();

        /// <summary>Creates exception with message describing cause and context of error,
        /// and leading/system exception causing it.</summary>
        /// <param name="error">Error code.</param> <param name="message">Error message.</param>
        /// <param name="innerException">Underlying system/leading exception.</param>
        protected ContainerException(int error, string message, Exception innerException)
            : base(message, innerException)
        {
            Error = error;
        }

        /// <summary>Creates exception with message describing cause and context of error.</summary>
        /// <param name="error">Error code.</param> <param name="message">Error message.</param>
        protected ContainerException(int error, string message)
            : this(error, message, null) { }
    }

    /// <summary>Defines error codes and error messages for all DryIoc exceptions (DryIoc extensions may define their own.)</summary>
    public static class Error
    {
        /// <summary>First error code to identify error range for other possible error code definitions.</summary>
        public static readonly int FirstErrorCode = 0;

        /// <summary>List of error messages indexed with code.</summary>
        public static readonly List<string> Messages = new List<string>(100);

#pragma warning disable 1591 // "Missing XML-comment"
        public static readonly int
            UnableToResolveUnknownService = Of(
                "Unable to resolve {0}" + Environment.NewLine +
                "Where no service registrations found" + Environment.NewLine +
                "  and no dynamic registrations found in {1} Rules.DynamicServiceProviders" + Environment.NewLine +
                "  and nothing in {2} Rules.UnknownServiceResolvers"),

            UnableToResolveFromRegisteredServices = Of(
                "Unable to resolve {0}" + Environment.NewLine +
                "  with normal and dynamic registrations:" + Environment.NewLine + "{1}"),

            ExpectedSingleDefaultFactory = Of(
                "Expecting single default registration but found many:" + Environment.NewLine + "{0}" +
                Environment.NewLine +
                "When resolving {1}." + Environment.NewLine +
                "Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory."),

            RegisteringImplementationNotAssignableToServiceType = Of(
                "Registering implementation type {0} is not assignable to service type {1}."),
            RegisteredFactoryMethodResultTypesIsNotAssignableToImplementationType = Of(
                "Registered factory method return type {1} should be assignable to implementation type {0} but it is not."),
            RegisteringOpenGenericRequiresFactoryProvider = Of(
                "Unable to register delegate factory for open-generic service {0}." + Environment.NewLine +
                "You need to specify concrete (closed) service type returned by delegate."),
            RegisteringOpenGenericImplWithNonGenericService = Of(
                "Unable to register open-generic implementation {0} with non-generic service {1}."),
            RegisteringOpenGenericServiceWithMissingTypeArgs = Of(
                "Unable to register open-generic implementation {0} because service {1} should specify all type arguments, but specifies only {2}."),
            RegisteringNotAGenericTypedefImplType = Of(
                "Unsupported registration of implementation {0} which is not a generic type definition but contains generic parameters." +
                Environment.NewLine +
                "Consider to register generic type definition {1} instead."),
            RegisteringNotAGenericTypedefServiceType = Of(
                "Unsupported registration of service {0} which is not a generic type definition but contains generic parameters." +
                Environment.NewLine +
                "Consider to register generic type definition {1} instead."),
            RegisteringNullImplementationTypeAndNoFactoryMethod = Of(
                "Registering without implementation type and without FactoryMethod to use instead."),
            RegisteringAbstractImplementationTypeAndNoFactoryMethod = Of(
                "Registering abstract implementation type {0} when it is should be concrete. Also there is not FactoryMethod to use instead."),
            UnableToSelectSinglePublicConstructorFromMultiple = Of(
                "Unable to select single public constructor from implementation type {0}:" + Environment.NewLine +
                "{1}"),
            UnableToSelectSinglePublicConstructorFromNone = Of(
                "Unable to select single public constructor from implementation type {0} because it does not have one."),
            NoMatchedImplementedTypesWithServiceType = Of(
                "Unable to match service with open-generic {0} implementing {1} when resolving {2}."),
            NoMatchedFactoryMethodDeclaringTypeWithServiceTypeArgs = Of(
                "Unable to match open-generic factory method Declaring type {0} with requested service type arguments <{1}> when resolving {2}."),
            NoMatchedFactoryMethodWithServiceTypeArgs = Of(
                "Unable to match open-generic factory method {0} with requested service type arguments <{1}> when resolving {2}."),
            OpenGenericFactoryMethodDeclaringTypeIsNotSupportedOnThisPlatform = Of(
                "[Specific to this .NET version] Unable to match method or constructor {0} from open-generic declaring type {1} to closed-generic type {2}, " +
                Environment.NewLine +
                "Please give the method an unique name to distinguish it from other overloads."),
            ResolvingOpenGenericServiceTypeIsNotPossible = Of(
                "Resolving open-generic service type is not possible for type: {0}."),
            RecursiveDependencyDetected = Of(
                "Recursive dependency is detected when resolving" + Environment.NewLine + "{0}."),
            ScopeIsDisposed = Of(
                "Scope {0} is disposed and scoped instances are disposed and no longer available."),
            NotFoundOpenGenericImplTypeArgInService = Of(
                "Unable to find for open-generic implementation {0} the type argument {1} when resolving {2}."),
            UnableToGetConstructorFromSelector = Of(
                "Unable to get constructor of {0} using provided constructor selector when resolving {1}."),
            UnableToFindCtorWithAllResolvableArgs = Of(
                "Unable to find constructor with all resolvable parameters when resolving {0}."),
            UnableToFindMatchingCtorForFuncWithArgs = Of(
                "Unable to find constructor with all parameters matching Func signature {0} " + Environment.NewLine
                + "and the rest of parameters resolvable from Container when resolving: {1}."),
            RegedFactoryDlgResultNotOfServiceType = Of(
                "Registered factory delegate returns service {0} is not assignable to {2}."),
            NotFoundSpecifiedWritablePropertyOrField = Of(
                "Unable to find writable property or field \"{0}\" when resolving: {1}."),
            PushingToRequestWithoutFactory = Of(
                "Pushing next info {0} to request not yet resolved to factory: {1}"),
            NoMatchedGenericParamConstraints = Of(
                "Open-generic service does not match with registered open-generic implementation constraints {0} when resolving: {1}."),
            GenericWrapperWithMultipleTypeArgsShouldSpecifyArgIndex = Of(
                "Generic wrapper type {0} should specify what type argument is wrapped, but it does not."),
            GenericWrapperTypeArgIndexOutOfBounds = Of(
                "Registered generic wrapper {0} specified type argument index {1} is out of type argument list."),
            DependencyHasShorterReuseLifespan = Of(
                "Dependency {0} reuse {1} lifespan shorter than its parent's: {2}" + Environment.NewLine +
                "To turn Off this error, specify the rule with new Container(rules => rules.WithoutThrowIfDependencyHasShorterReuseLifespan())."),
            WeakRefReuseWrapperGCed = Of(
                "Reused service wrapped in WeakReference is Garbage Collected and no longer available."),
            ServiceIsNotAssignableFromFactoryMethod = Of(
                "Service of {0} is not assignable from factory method {1} when resolving: {2}."),
            FactoryObjIsNullInFactoryMethod = Of(
                "Unable to use null factory object with *instance* factory method {0} when resolving: {1}."),
            FactoryObjProvidedButMethodIsStatic = Of(
                "Factory instance provided {0} But factory method is static {1} when resolving: {2}."),
            GotNullConstructorFromFactoryMethod = Of(
                "Got null constructor when resolving {0}"),
            UnableToRegisterDuplicateDefault = Of(
                "Service {0} without key is already registered as {1}."),
            UnableToRegisterDuplicateKey = Of(
                "Unable to register service {0} with duplicate key '{1}'" + Environment.NewLine +
                " There is already registered service with the same key: {2}."),
            NoCurrentScope = Of(
                "No current scope available: probably you are registering to, or resolving from outside of scope."),
            ContainerIsDisposed = Of(
                "Container is disposed and should not be used: {0}"),
            NotDirectScopeParent = Of(
                "Unable to OpenScope [{0}] because parent scope [{1}] is not current context scope [{2}]." +
                Environment.NewLine +
                "It is probably other scope was opened in between OR you forgot to Dispose some other scope!"),
            NoMatchedScopeFound = Of(
                "Unable to find matching scope with name {1} starting from the current scope {0}."),
            NotSupportedMadeExpression = Of(
                "Only expression of method call, property getter, or new statement (with optional property initializer) is supported, but found: {0}."),
            UnexpectedFactoryMemberExpression = Of(
                "Expected property getter, but found {0}."),
            UnexpectedExpressionInsteadOfArgMethod = Of(
                "Expected DryIoc.Arg method call to specify parameter/property/field, but found: {0}."),
            UnexpectedExpressionInsteadOfConstant = Of(
                "Expected constant expression to specify parameter/property/field, but found something else: {0}."),
            InjectedCustomValueIsOfDifferentType = Of(
                "Injected value {0} is not assignable to {2}."),
            StateIsRequiredToUseItem = Of(
                "Runtime state is required to inject (or use) the: {0}. " + Environment.NewLine +
                "The reason is using RegisterDelegate, UseInstance, RegisterInitializer/Disposer, or registering with non-primitive service key, or metadata." +
                Environment.NewLine +
                "You can convert run-time value to expression via container.With(rules => rules.WithItemToExpressionConverter(YOUR_ITEM_TO_EXPRESSION_CONVERTER))."),
            ArgValueIndexIsProvidedButNoArgValues = Of(
                "Arg.Index of value is used but no values are passed"),
            ArgValueIndexIsOutOfProvidedArgValues = Of(
                "Arg.Index {0} is outside of provided values: {1}"),
            ResolutionNeedsRequiredServiceType = Of(
                "Expecting required service type but it is not specified when resolving: {0}"),
            RegisterMappingNotFoundRegisteredService = Of(
                "When registering mapping, Container is unable to find factory of registered service type {0} and key {1}."),
            RegisteringInstanceNotAssignableToServiceType = Of(
                "Registered instance {0} is not assignable to serviceType {1}."),
            NoMoreRegistrationsAllowed = Of(
                "Container does not allow further registrations." + Environment.NewLine +
                "Attempting to register {0}{1} with implementation factory {2}."),
            NoMoreUnregistrationsAllowed = Of(
                "Container does not allow further registry modification." + Environment.NewLine +
                "Attempting to Unregister {0}{1} with factory type {2}."),
            GotNullFactoryWhenResolvingService = Of(
                "Got null factory method when resolving {0}"),
            RegisteredDisposableTransientWontBeDisposedByContainer = Of(
                "Registered Disposable Transient service {0} with key {1} registered as {2} won't be disposed by container." +
                " DryIoc does not hold reference to resolved transients, and therefore does not control their dispose." +
                " To silence this exception Register<YourService>(setup: Setup.With(allowDisposableTransient: true)) " +
                " or set the rule Container(rules => rules.WithoutThrowOnRegisteringDisposableTransient())." +
                " To enable tracking use Register<YourService>(setup: Setup.With(trackDisposableTransient: true)) " +
                " or set the rule Container(rules => rules.WithTrackingDisposableTransients())"),
            UnableToUseInstanceForExistingNonInstanceFactory = Of(
                "Unable to use the keyed instance {0} because of existing non-instance keyed registration: {1}"),
            NotFoundMetaCtorWithTwoArgs = Of(
                "Expecting Meta wrapper public constructor with two arguments {0} but not found when resolving: {1}"),
            UnableToSelectFromManyRegistrationsWithMatchingMetadata = Of(
                "Unable to select from multiple registrations matching the Metadata type {0}:" + Environment.NewLine +
                "{1}" + Environment.NewLine +
                "When resolving: {2}"),
            ImplTypeIsNotSpecifiedForAutoCtorSelection = Of(
                "Implementation type is not specified when using automatic constructor selection: {0}"),
            NoImplementationForPlaceholder = Of(
                "There is no real implementation, only a placeholder for the service {0}." + Environment.NewLine +
                "Please Register the implementation with the ifAlreadyRegistered.Replace parameter to fill the placeholder."),
            UnableToFindSingletonInstance = Of(
                "Expecting the instance to be stored in singleton scope, but unable to find anything here." + Environment.NewLine +
                "Likely, you've called UseInstance from the scoped container, but resolving from another container or injecting into a singleton."),
            DecoratorShouldNotBeRegisteredWithServiceKey = Of(
                "Registerring Decorator {0} with not-null service key {1} does not make sense," + Environment.NewLine +
                "because decorator may be applied to multiple service of any key." + Environment.NewLine +
                "If you wanted to apply decorator to services of specific key please use `setup: Setup.DecoratorOf(serviceKey: blah)`");

#pragma warning restore 1591 // "Missing XML-comment"

        /// <summary>Stores new error message and returns error code for it.</summary>
        /// <param name="message">Error message to store.</param> <returns>Error code for message.</returns>
        public static int Of(string message)
        {
            Messages.Add(message);
            return FirstErrorCode + Messages.Count - 1;
        }

        /// <summary>Returns the name for the provided error code.</summary>
        /// <param name="error">error code.</param> <returns>name of error, unique in scope of this <see cref="Error"/> class.</returns>
        public static string NameOf(int error)
        {
            var index = error - FirstErrorCode + 1;
            var field = typeof(Error).GetTypeInfo().DeclaredFields
                .Where(f => f.FieldType == typeof(int))
                .Where((_, i) => i == index)
                .FirstOrDefault();
            return field != null ? field.Name : null;
        }

        static Error()
        {
            Throw.GetMatchedException = ContainerException.Of;
        }
    }

    /// <summary>Checked error condition, possible error sources.</summary>
    public enum ErrorCheck
    {
        /// <summary>Unspecified, just throw.</summary>
        Unspecified,
        /// <summary>Predicate evaluated to false.</summary>
        InvalidCondition,
        /// <summary>Checked object is null.</summary>
        IsNull,
        /// <summary>Checked object is of unexpected type.</summary>
        IsNotOfType,
        /// <summary>Checked type is not assignable to expected type</summary>
        TypeIsNotOfType,
        /// <summary>Invoked operation throws, it is source of inner exception.</summary>
        OperationThrows,
    }

    /// <summary>Enables more clean error message formatting and a bit of code contracts.</summary>
    public static class Throw
    {
        private static string[] CreateDefaultMessages()
        {
            var messages = new string[(int)ErrorCheck.OperationThrows + 1];
            messages[(int)ErrorCheck.Unspecified] = "The error reason is unspecified, which is bad thing.";
            messages[(int)ErrorCheck.InvalidCondition] = "Argument {0} of type {1} has invalid condition.";
            messages[(int)ErrorCheck.IsNull] = "Argument of type {0} is null.";
            messages[(int)ErrorCheck.IsNotOfType] = "Argument {0} is not of type {1}.";
            messages[(int)ErrorCheck.TypeIsNotOfType] = "Type argument {0} is not assignable from type {1}.";
            messages[(int)ErrorCheck.OperationThrows] = "Invoked operation throws the inner exception {0}.";
            return messages;
        }

        private static readonly string[] _defaultMessages = CreateDefaultMessages();

        /// <summary>Returns the default message specified for <see cref="ErrorCheck"/> code.</summary>
        /// <param name="error">Error code to get message for.</param> <returns>String format message.</returns>
        public static string GetDefaultMessage(ErrorCheck error)
        {
            return _defaultMessages[(int)error];
        }

        /// <summary>Declares mapping between <see cref="ErrorCheck"/> type and <paramref name="error"/> code to specific <see cref="Exception"/>.</summary>
        /// <returns>Returns mapped exception.</returns>
        public delegate Exception GetMatchedExceptionHandler(ErrorCheck errorCheck, int error, object arg0, object arg1, object arg2, object arg3, Exception inner);

        /// <summary>Returns matched exception for error check and error code.</summary>
        public static GetMatchedExceptionHandler GetMatchedException = ContainerException.Of;

        /// <summary>Throws matched exception with provided error code if throw condition is true.</summary>
        public static void If(bool throwCondition, int error = -1, object arg0 = null, object arg1 = null, object arg2 = null, object arg3 = null)
        {
            if (throwCondition)
                throw GetMatchedException(ErrorCheck.InvalidCondition, error, arg0, arg1, arg2, arg3, null);
        }

        /// <summary>Throws matched exception with provided error code if throw condition is true.
        /// Otherwise returns source <paramref name="arg0"/>.</summary>
        public static T ThrowIf<T>(this T arg0, bool throwCondition, int error = -1, object arg1 = null, object arg2 = null, object arg3 = null)
        {
            if (!throwCondition) return arg0;
            throw GetMatchedException(ErrorCheck.InvalidCondition, error, arg0, arg1, arg2, arg3, null);
        }

        /// <summary>Throws exception if <paramref name="arg"/> is null, otherwise returns <paramref name="arg"/>.</summary>
        public static T ThrowIfNull<T>(this T arg, int error = -1, object arg0 = null, object arg1 = null, object arg2 = null, object arg3 = null)
            where T : class
        {
            if (arg != null) return arg;
            throw GetMatchedException(ErrorCheck.IsNull, error, arg0 ?? typeof(T), arg1, arg2, arg3, null);
        }

        /// <summary>Throws exception if <paramref name="arg0"/> is not assignable to type specified by <paramref name="arg1"/>,
        /// otherwise just returns <paramref name="arg0"/>.</summary>
        public static T ThrowIfNotOf<T>(this T arg0, Type arg1, int error = -1, object arg2 = null, object arg3 = null)
            where T : class
        {
            if (arg1.IsTypeOf(arg0)) return arg0;
            throw GetMatchedException(ErrorCheck.IsNotOfType, error, arg0, arg1, arg2, arg3, null);
        }

        /// <summary>Throws if <paramref name="arg0"/> is not assignable from <paramref name="arg1"/>.</summary>
        public static Type ThrowIfNotImplementedBy(this Type arg0, Type arg1, int error = -1, object arg2 = null, object arg3 = null)
        {
            if (arg1.IsAssignableTo(arg0)) return arg0;
            throw GetMatchedException(ErrorCheck.TypeIsNotOfType, error, arg0, arg1, arg2, arg3, null);
        }

        /// <summary>Invokes <paramref name="operation"/> and in case of <typeparamref name="TEx"/> re-throws it as inner-exception.</summary>
        public static T IfThrows<TEx, T>(Func<T> operation, bool throwCondition, int error, object arg0 = null, object arg1 = null, object arg2 = null, object arg3 = null) where TEx : Exception
        {
            try
            {
                return operation();
            }
            catch (TEx ex)
            {
                if (throwCondition)
                    throw GetMatchedException(ErrorCheck.OperationThrows, error, arg0, arg1, arg2, arg3, ex);
                return default(T);
            }
        }

        /// <summary>Just throws the exception with the <paramref name="error"/> code.</summary>
        public static object It(int error, object arg0 = null, object arg1 = null, object arg2 = null, object arg3 = null)
        {
            throw GetMatchedException(ErrorCheck.Unspecified, error, arg0, arg1, arg2, arg3, null);
        }

        /// <summary>Throws <paramref name="error"/> instead of returning value of <typeparamref name="T"/>.
        /// Supposed to be used in expression that require some return value.</summary>
        public static T For<T>(int error, object arg0 = null, object arg1 = null, object arg2 = null, object arg3 = null)
        {
            throw GetMatchedException(ErrorCheck.Unspecified, error, arg0, arg1, arg2, arg3, null);
        }
    }

    /// <summary>Called from generated code.</summary>
    public static class ThrowInGeneratedCode
    {
        /// <summary>Throws if object is null.</summary>
        /// <param name="obj">object to check.</param><param name="message">Error message.</param>
        /// <returns>object if not null.</returns>
        public static object ThrowNewErrorIfNull(this object obj, string message)
        {
            if (obj == null) Throw.It(Error.Of(message));
            return obj;
        }
    }

    /// <summary>Contains helper methods to work with Type: for instance to find Type implemented base types and interfaces, etc.</summary>
    public static class ReflectionTools
    {
        /// <summary>Flags for <see cref="GetImplementedTypes"/> method.</summary>
        [Flags]
        public enum AsImplementedType
        {
            /// <summary>Include nor object not source type.</summary>
            None = 0,
            /// <summary>Include source type to list of implemented types.</summary>
            SourceType = 1,
            /// <summary>Include <see cref="System.Object"/> type to list of implemented types.</summary>
            ObjectType = 2
        }

        /// <summary>Returns all interfaces and all base types (in that order) implemented by <paramref name="sourceType"/>.
        /// Specify <paramref name="asImplementedType"/> to include <paramref name="sourceType"/> itself as first item and
        /// <see cref="object"/> type as the last item.</summary>
        /// <param name="sourceType">Source type for discovery.</param>
        /// <param name="asImplementedType">Additional types to include into result collection.</param>
        /// <returns>Array of found types, empty if nothing found.</returns>
        public static Type[] GetImplementedTypes(this Type sourceType, AsImplementedType asImplementedType = AsImplementedType.None)
        {
            Type[] results;

            var interfaces = sourceType.GetImplementedInterfaces();
            var interfaceStartIndex = (asImplementedType & AsImplementedType.SourceType) == 0 ? 0 : 1;
            var includingObjectType = (asImplementedType & AsImplementedType.ObjectType) == 0 ? 0 : 1;
            var sourcePlusInterfaceCount = interfaceStartIndex + interfaces.Length;

            var baseType = sourceType.GetTypeInfo().BaseType;
            if (baseType == null || baseType == typeof(object))
                results = new Type[sourcePlusInterfaceCount + includingObjectType];
            else
            {
                List<Type> baseBaseTypes = null;
                for (var bb = baseType.GetTypeInfo().BaseType; bb != null && bb != typeof(object); bb = bb.GetTypeInfo().BaseType)
                    (baseBaseTypes ?? (baseBaseTypes = new List<Type>(2))).Add(bb);

                if (baseBaseTypes == null)
                    results = new Type[sourcePlusInterfaceCount + includingObjectType + 1];
                else
                {
                    results = new Type[sourcePlusInterfaceCount + baseBaseTypes.Count + includingObjectType + 1];
                    baseBaseTypes.CopyTo(results, sourcePlusInterfaceCount + 1);
                }

                results[sourcePlusInterfaceCount] = baseType;
            }

            if (interfaces.Length == 1)
                results[interfaceStartIndex] = interfaces[0];
            else if (interfaces.Length > 1)
                Array.Copy(interfaces, 0, results, interfaceStartIndex, interfaces.Length);

            if (interfaceStartIndex == 1)
                results[0] = sourceType;
            if (includingObjectType == 1)
                results[results.Length - 1] = typeof(object);

            return results;
        }

        /// <summary>Gets a collection of the interfaces implemented by the current type and its base types.</summary>
        public static Type[] GetImplementedInterfaces(this Type type) =>
            type.GetTypeInfo().ImplementedInterfaces.ToArrayOrSelf();

        /// <summary>Gets all declared and base members.</summary>
        /// <param name="type">Type to get members from.</param>
        /// <param name="includeBase">(optional) When set looks into base members.</param>
        /// <returns>All members.</returns>
        public static IEnumerable<MemberInfo> GetAllMembers(this Type type, bool includeBase = false) =>
            type.GetMembers(t =>
                t.DeclaredMethods.Cast<MemberInfo>().Concat(
                t.DeclaredProperties.Cast<MemberInfo>().Concat(
                t.DeclaredFields.Cast<MemberInfo>())),
                includeBase);

        /// <summary>Returns true if <paramref name="openGenericType"/> contains all generic parameters
        /// from <paramref name="genericParameters"/>.</summary>
        /// <param name="openGenericType">Expected to be open-generic type, throws otherwise.</param>
        /// <param name="genericParameters">Generic parameters.</param>
        /// <returns>Returns true if contains, and false otherwise.</returns>
        public static bool ContainsAllGenericTypeParameters(this Type openGenericType, Type[] genericParameters)
        {
            if (!openGenericType.IsOpenGeneric())
                return false;

            var matchedParams = new Type[genericParameters.Length];
            Array.Copy(genericParameters, matchedParams, genericParameters.Length);

            SetToNullGenericParametersReferencedInConstraints(matchedParams);
            SetToNullMatchesFoundInGenericParameters(matchedParams, openGenericType.GetGenericParamsAndArgs());

            for (var i = 0; i < matchedParams.Length; i++)
                if (matchedParams[i] != null)
                    return false;
            return true;
        }

        /// <summary>Returns true if class is compiler generated. Checking for CompilerGeneratedAttribute
        /// is not enough, because this attribute is not applied for classes generated from "async/await".</summary>
        public static bool IsCompilerGenerated(this Type type) =>
            type.FullName != null && type.FullName.Contains("<>c__DisplayClass");

        /// <summary>Returns true if type is generic.</summary>
        public static bool IsGeneric(this Type type) =>
            type.GetTypeInfo().IsGenericType;

        /// <summary>Returns true if type is generic type definition (open type).</summary>
        public static bool IsGenericDefinition(this Type type) =>
            type.GetTypeInfo().IsGenericTypeDefinition;

        /// <summary>Returns true if type is closed generic: does not have open generic parameters, only closed/concrete ones.</summary>
        public static bool IsClosedGeneric(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsGenericType && !typeInfo.ContainsGenericParameters;
        }

        /// <summary>Returns true if type if open generic: contains at list one open generic parameter. Could be
        /// generic type definition as well.</summary>
        public static bool IsOpenGeneric(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsGenericType && typeInfo.ContainsGenericParameters;
        }

        /// <summary>Returns generic type definition if type is generic and null otherwise.</summary>
        public static Type GetGenericDefinitionOrNull(this Type type) =>
            type != null && type.GetTypeInfo().IsGenericType ? type.GetGenericTypeDefinition() : null;

        /// <summary>Returns generic type parameters and arguments in order they specified. If type is not generic, returns empty array.</summary>
        public static Type[] GetGenericParamsAndArgs(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsGenericTypeDefinition
                ? typeInfo.GenericTypeParameters
                : typeInfo.GenericTypeArguments;
        }

        /// <summary>Returns array of interface and base class constraints for provider generic parameter type.</summary>
        public static Type[] GetGenericParamConstraints(this Type type) =>
            type.GetTypeInfo().GetGenericParameterConstraints();

        /// <summary>If type is array returns is element type, otherwise returns null.</summary>
        /// <param name="type">Source type.</param> <returns>Array element type or null.</returns>
        public static Type GetArrayElementTypeOrNull(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsArray ? typeInfo.GetElementType() : null;
        }

        /// <summary>Return base type or null, if not exist (the case for only for object type).</summary>
        public static Type GetBaseType(this Type type) =>
            type.GetTypeInfo().BaseType;

        /// <summary>Checks if type is public or nested public in public type.</summary>
        public static bool IsPublicOrNestedPublic(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsPublic || typeInfo.IsNestedPublic && typeInfo.DeclaringType.IsPublicOrNestedPublic();
        }

        /// <summary>Returns true if type is class.</summary>
        public static bool IsClass(this Type type) =>
            type.GetTypeInfo().IsClass;

        /// <summary>Returns true if type is value type.</summary>
        public static bool IsValueType(this Type type) =>
            type.GetTypeInfo().IsValueType;

        /// <summary>Returns true if type is interface.</summary>
        public static bool IsInterface(this Type type) =>
            type.GetTypeInfo().IsInterface;

        /// <summary>Returns true if type if abstract or interface.</summary>
        public static bool IsAbstract(this Type type) =>
            type.GetTypeInfo().IsAbstract;

        /// <summary>Returns true if type is static.</summary>
        public static bool IsStatic(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsAbstract && typeInfo.IsSealed;
        }

        /// <summary>Returns true if type is enum type.</summary>
        public static bool IsEnum(this Type type) =>
            type.GetTypeInfo().IsEnum;

        /// <summary>Returns true if instance of type is assignable to instance of <paramref name="other"/> type.</summary>
        public static bool IsAssignableTo(this Type type, Type other) =>
            type != null && other != null && other.GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());

        /// <summary>Returns true if type of <paramref name="obj"/> is assignable to source <paramref name="type"/>.</summary>
        public static bool IsTypeOf(this Type type, object obj) =>
            obj != null && obj.GetType().IsAssignableTo(type);

        /// <summary>Returns true if provided type IsPitmitive in .Net terms, or enum, or string,
        /// or array of primitives if <paramref name="orArrayOfPrimitives"/> is true.</summary>
        public static bool IsPrimitive(this Type type, bool orArrayOfPrimitives = false)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsPrimitive || typeInfo.IsEnum || type == typeof(string)
                || orArrayOfPrimitives && typeInfo.IsArray && typeInfo.GetElementType().IsPrimitive(true);
        }

        /// <summary>Returns all attributes defined on <paramref name="type"/>.</summary>
        public static Attribute[] GetAttributes(this Type type, Type attributeType = null, bool inherit = false) =>
            type.GetTypeInfo().GetCustomAttributes(attributeType ?? typeof(Attribute), inherit)
                // ReSharper disable once RedundantEnumerableCastCall
                .Cast<Attribute>() // required in .NET 4.5
                .ToArrayOrSelf();

        /// <summary>Recursive method to enumerate all input type and its base types for specific details.
        /// Details are returned by <paramref name="getMembers"/> delegate.</summary>
        public static IEnumerable<TMember> GetMembers<TMember>(this Type type,
            Func<TypeInfo, IEnumerable<TMember>> getMembers,
            bool includeBase = false)
        {
            var typeInfo = type.GetTypeInfo();
            var members = getMembers(typeInfo);
            if (!includeBase)
                return members;

            var baseType = typeInfo.BaseType;
            if (baseType == null || baseType == typeof(object))
                return members;

            var baseMembers = baseType.GetMembers(getMembers, true);
            return members.Append(baseMembers);
        }

        /// <summary>Returns all public instance constructors for the type</summary>
        public static IEnumerable<ConstructorInfo> GetPublicInstanceConstructors(this Type type) =>
            type.GetTypeInfo().DeclaredConstructors.Match(c => c.IsPublic && !c.IsStatic);

        /// <summary>Enumerates all constructors from input type.</summary>
        public static IEnumerable<ConstructorInfo> GetAllConstructors(this Type type,
            bool includeNonPublic = false, bool includeStatic = false)
        {
            var ctors = type.GetTypeInfo().DeclaredConstructors;
            if (!includeNonPublic) ctors = ctors.Where(c => c.IsPublic);
            if (!includeStatic) ctors = ctors.Where(c => !c.IsStatic);
            return ctors;
        }

        /// <summary>Searches and returns constructor by its signature.</summary>
        public static ConstructorInfo GetConstructorOrNull(this Type type, bool includeNonPublic = false, params Type[] args) =>
            type.GetAllConstructors(includeNonPublic)
                .FirstOrDefault(c => c.GetParameters().Select(p => p.ParameterType).SequenceEqual(args));

        /// <summary>Returns single constructor otherwise (if no or more than one) throws an exception</summary>
        public static ConstructorInfo Constructor(this Type type, bool includeNonPublic = false) =>
            type.GetSingleConstructorOrNull(includeNonPublic) ??
            Throw.For<ConstructorInfo>(
                Error.Of("Unable to find a single constructor in Type {0} (including non-public={1})"), type, includeNonPublic);

        /// <summary>Returns single constructor otherwise (if no or more than one) returns null.</summary>
        public static ConstructorInfo GetSingleConstructorOrNull(this Type type, bool includeNonPublic = false)
        {
            var ctors = type.GetAllConstructors(includeNonPublic).ToArrayOrSelf();
            return ctors.Length == 1 ? ctors[0] : null;
        }

        /// <summary>Returns single declared (not inherited) method by name, or null if not found.</summary>
        public static MethodInfo Method(this Type type, string name, bool includeNonPublic = false) =>
            type.GetSingleMethodOrNull(name, includeNonPublic) ??
            Throw.For<MethodInfo>(
                Error.Of("Undefined Method '{0}' in Type {1} (including non-public={2})"), name, type, includeNonPublic);

        /// <summary>Looks up for method with and specified parameter types.</summary>
        public static MethodInfo Method(this Type type, string name, params Type[] paramTypes) =>
            type.GetMethodOrNull(name, paramTypes) ??
            Throw.For<MethodInfo>(
                Error.Of("Undefined Method '{0}' in Type {1} with parameters {2}."), name, type, paramTypes);

        /// <summary>Looks up for single declared method with the specified name. Returns null if method is not found.</summary>
        public static MethodInfo GetSingleMethodOrNull(this Type type, string name, bool includeNonPublic = false)
        {
            var methods = type.GetTypeInfo().DeclaredMethods
                .Match(m => (includeNonPublic || m.IsPublic) && m.Name == name)
                .ToArrayOrSelf();
            return methods.Length == 1 ? methods[0] : null;
        }

        /// <summary>Looks up for method with and specified parameter types.</summary>
        public static MethodInfo GetMethodOrNull(this Type type, string name, params Type[] paramTypes)
        {
            var paramCount = paramTypes.Length;
            var methods = type.GetTypeInfo().DeclaredMethods.ToArrayOrSelf();
            for (var m = 0; m < methods.Length; m++)
            {
                var method = methods[m];
                if (method.Name != name)
                    continue;

                var methodParams = method.GetParameters();
                if (paramCount == methodParams.Length)
                {
                    if (paramCount == 0)
                        return method;

                    var p = 0;
                    for (; p < paramCount; ++p)
                    {
                        var methodParamType = methodParams[p].ParameterType;
                        if (methodParamType == paramTypes[p])
                            continue;

                        if (methodParamType.IsOpenGeneric() &&
                            methodParamType.GetGenericTypeDefinition() == paramTypes[p])
                            continue;
                    }

                    if (p == paramCount)
                        return method;
                }
            }

            return null;
        }

        /// <summary>Returns property by name, including inherited. Or null if not found.</summary>
        public static PropertyInfo Property(this Type type, string name, bool includeBase = false) =>
            type.GetPropertyOrNull(name, includeBase).ThrowIfNull(Error.Of("Undefined property {0} in type {1}"), name, type);

        /// <summary>Returns property by name, including inherited. Or null if not found.</summary>
        public static PropertyInfo GetPropertyOrNull(this Type type, string name, bool includeBase = false) =>
            type.GetMembers(_ => _.DeclaredProperties, includeBase: includeBase).FirstOrDefault(p => p.Name == name);

        /// <summary>Returns field by name, including inherited. Or null if not found.</summary>
        public static FieldInfo Field(this Type type, string name, bool includeBase = false) =>
            type.GetFieldOrNull(name, includeBase).ThrowIfNull(Error.Of("Undefined field {0} in type {1}"), name, type);

        /// <summary>Returns field by name, including inherited. Or null if not found.</summary>
        public static FieldInfo GetFieldOrNull(this Type type, string name, bool includeBase = false) =>
            type.GetMembers(_ => _.DeclaredFields, includeBase: includeBase).FirstOrDefault(p => p.Name == name);

        /// <summary>Returns type assembly.</summary>
        public static Assembly GetAssembly(this Type type) => type.GetTypeInfo().Assembly;

        /// <summary>Is <c>true</c> for interface declared property explicitly implemented, e.g. <c>IInterface.Prop</c></summary>
        public static bool IsExplicitlyImplemented(this PropertyInfo property) => property.Name.Contains(".");

        /// <summary>Returns true if member is static, otherwise returns false.</summary>
        public static bool IsStatic(this MemberInfo member)
        {
            var method = member as MethodInfo;
            if (method != null)
                return method.IsStatic;

            var field = member as FieldInfo;
            if (field != null)
                return field.IsStatic;

            var prop = member as PropertyInfo;
            if (prop == null || prop.IsExplicitlyImplemented())
                return false;

            var propAccessor =
                prop.GetGetMethodOrNull(includeNonPublic: true) ??
                prop.GetSetMethodOrNull(includeNonPublic: true);

            if (propAccessor == null)
                return false; // how come?

            return propAccessor.IsStatic;
        }

        /// <summary>Return either <see cref="PropertyInfo.PropertyType"/>, or <see cref="FieldInfo.FieldType"/>, 
        /// <see cref="MethodInfo.ReturnType"/>.</summary>
        public static Type GetReturnTypeOrDefault(this MemberInfo member) =>
            member is ConstructorInfo ? member.DeclaringType
                : member is MethodInfo ? ((MethodInfo)member).ReturnType
                : member is PropertyInfo ? ((PropertyInfo)member).PropertyType
                : member is FieldInfo ? ((FieldInfo)member).FieldType
                : null;

        /// <summary>Returns true if field is backing field for property.</summary>
        public static bool IsBackingField(this FieldInfo field) =>
            field.Name[0] == '<';

        /// <summary>Returns true if property is indexer: aka this[].</summary>
        public static bool IsIndexer(this PropertyInfo property) =>
            property.GetIndexParameters().Length != 0;

        /// <summary>Returns true if type is generated type of hoisted closure.</summary>
        public static bool IsClosureType(this Type type) =>
            type.Name.Contains("<>c__DisplayClass");

        /// <summary>Returns attributes defined for the member/method.</summary>
        public static IEnumerable<Attribute> GetAttributes(this MemberInfo member, Type attributeType = null, bool inherit = false) =>
            member.GetCustomAttributes(attributeType ?? typeof(Attribute), inherit).Cast<Attribute>();

        /// <summary>Returns attributes defined for parameter.</summary>
        public static IEnumerable<Attribute> GetAttributes(this ParameterInfo parameter, Type attributeType = null, bool inherit = false) =>
            parameter.GetCustomAttributes(attributeType ?? typeof(Attribute), inherit).Cast<Attribute>();

        /// <summary>Get types from assembly that are loaded successfully.
        /// Hacks to <see cref="ReflectionTypeLoadException"/> for loaded types.</summary>
        public static Type[] GetLoadedTypes(this Assembly assembly)
        {
            try
            {
                return Portable.GetAssemblyTypes(assembly).ToArrayOrSelf();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(type => type != null).ToArray();
            }
        }

        /// <summary>Creates default(T) expression for provided <paramref name="type"/>.</summary>
        public static Expr GetDefaultValueExpression(this Type type) =>
            Call(_getDefaultMethod.Value.MakeGenericMethod(type), ArrayTools.Empty<Expr>());

#region Implementation

        private static void SetToNullGenericParametersReferencedInConstraints(Type[] genericParams)
        {
            for (int i = 0; i < genericParams.Length; i++)
            {
                var genericParam = genericParams[i];
                if (genericParam == null)
                    continue;

                var genericConstraints = genericParam.GetGenericParamConstraints();
                for (var j = 0; j < genericConstraints.Length; j++)
                {
                    var genericConstraint = genericConstraints[j];
                    if (genericConstraint.IsOpenGeneric())
                    {
                        var constraintGenericParams = genericConstraint.GetGenericParamsAndArgs();
                        for (var k = 0; k < constraintGenericParams.Length; k++)
                        {
                            var constraintGenericParam = constraintGenericParams[k];
                            if (constraintGenericParam != genericParam)
                            {
                                var genericParamIndex = genericParams.IndexOf(constraintGenericParam);
                                if (genericParamIndex != -1)
                                    genericParams[genericParamIndex] = null;
                            }
                        }
                    }
                }
            }
        }

        private static void SetToNullMatchesFoundInGenericParameters(Type[] matchedParams, Type[] genericParams)
        {
            for (var i = 0; i < genericParams.Length; i++)
            {
                var genericParam = genericParams[i];
                if (genericParam.IsGenericParameter)
                {
                    var matchedIndex = matchedParams.IndexOf(genericParam);
                    if (matchedIndex != -1)
                        matchedParams[matchedIndex] = null;
                }
                else if (genericParam.IsOpenGeneric())
                    SetToNullMatchesFoundInGenericParameters(matchedParams, genericParam.GetGenericParamsAndArgs());
            }
        }

        internal static T GetDefault<T>() => default(T);

        private static readonly Lazy<MethodInfo> _getDefaultMethod = new Lazy<MethodInfo>(() =>
            typeof(ReflectionTools).Method(nameof(GetDefault), true));

#endregion
    }

    /// <summary>Provides pretty printing/debug view for number of types.</summary>
    public static class PrintTools
    {
        /// <summary>Default separator used for printing enumerable.</summary>
        public static string DefaultItemSeparator = ", " + Environment.NewLine;

        /// <summary>Prints input object by using corresponding Print methods for know types.</summary>
        /// <param name="s">Builder to append output to.</param> <param name="x">Object to print.</param>
        /// <param name="quote">(optional) Quote to use for quoting string object.</param>
        /// <param name="itemSeparator">(optional) Separator for enumerable.</param>
        /// <param name="getTypeName">(optional) Custom type printing policy.</param>
        /// <returns>String builder with appended output.</returns>
        public static StringBuilder Print(this StringBuilder s, object x,
            string quote = null, string itemSeparator = null, Func<Type, string> getTypeName = null) =>
            x == null ? s.Append("null")
                : x is string ? s.Print((string)x, quote)
                : x is Type ? s.Print((Type)x, getTypeName)
                : x.GetType().IsEnum() ? s.Print(x.GetType()).Append('.').Append(Enum.GetName(x.GetType(), x))
                : (x is IEnumerable<Type> || x is IEnumerable) &&
                    !x.GetType().IsAssignableTo(typeof(IEnumerable<>).MakeGenericType(x.GetType())) // exclude infinite recursion and StackOverflowEx
                    ? s.Print((IEnumerable)x, itemSeparator ?? DefaultItemSeparator, (_, o) => _.Print(o, quote, null, getTypeName))
                : s.Append(x);

        /// <summary>Appends string to string builder quoting with <paramref name="quote"/> if provided.</summary>
        /// <param name="s">String builder to append string to.</param> <param name="str">String to print.</param>
        /// <param name="quote">(optional) Quote to add before and after string.</param>
        /// <returns>String builder with appended string.</returns>
        public static StringBuilder Print(this StringBuilder s, string str, string quote = null) =>
            quote == null ? s.Append(str) : s.Append(quote).Append(str).Append(quote);

        /// <summary>Prints enumerable by using corresponding Print method for known item type.</summary>
        /// <param name="s">String builder to append output to.</param>
        /// <param name="items">Items to print.</param>
        /// <param name="separator">(optional) Custom separator if provided.</param>
        /// <param name="printItem">(optional) Custom item printer if provided.</param>
        /// <returns>String builder with appended output.</returns>
        public static StringBuilder Print(this StringBuilder s, IEnumerable items,
            string separator = ", ", Action<StringBuilder, object> printItem = null)
        {
            if (items == null) return s;
            printItem = printItem ?? ((_, x) => _.Print(x));
            var itemCount = 0;
            foreach (var item in items)
                printItem(itemCount++ == 0 ? s : s.Append(separator), item);
            return s;
        }

        /// <summary>Default delegate to print Type details: by default prints Type FullName and
        /// skips namespace if it start with "System."</summary>
        public static Func<Type, string> GetTypeNameDefault = t =>
#if DEBUG
            t.Name;
#else
            t.FullName != null && t.Namespace != null && !t.Namespace.StartsWith("System") ? t.FullName : t.Name;
#endif

        /// <summary>Appends type details to string builder.</summary>
        /// <param name="s">String builder to append output to.</param>
        /// <param name="type">Input type to print.</param>
        /// <param name="getTypeName">(optional) Delegate to provide custom type details.</param>
        /// <returns>String builder with appended output.</returns>
        public static StringBuilder Print(this StringBuilder s, Type type, Func<Type, string> getTypeName = null)
        {
            if (type == null) return s;

            getTypeName = getTypeName ?? GetTypeNameDefault;
            var typeName = getTypeName(type);

            var isArray = type.IsArray;
            if (isArray)
                type = type.GetElementType();

            if (!type.IsGeneric())
                return s.Append(typeName.Replace('+', '.'));

            s.Append(typeName.Substring(0, typeName.IndexOf('`')).Replace('+', '.')).Append('<');

            var genericArgs = type.GetGenericParamsAndArgs();
            if (type.IsGenericDefinition())
                s.Append(',', genericArgs.Length - 1);
            else
                s.Print(genericArgs, ", ", (_, t) => _.Print((Type)t, getTypeName));

            s.Append('>');

            if (isArray)
                s.Append("[]");

            return s;
        }
    }

    /// <summary>Ports some methods from .Net 4.0/4.5</summary>
    public static partial class Portable
    {
        // note: fallback to DefinedTypes (PCL)
        /// <summary>Portable version of Assembly.GetTypes or Assembly.DefinedTypes.</summary>
        public static readonly Func<Assembly, IEnumerable<Type>> GetAssemblyTypes = GetAssemblyTypesMethod();

        private static Func<Assembly, IEnumerable<Type>> GetAssemblyTypesMethod()
        {
            var assemblyParamExpr = Parameter(typeof(Assembly), "a");

            Expr typesExpr;

            var definedTypeInfosProperty = typeof(Assembly).GetPropertyOrNull("DefinedTypes");
            if (definedTypeInfosProperty == null)
            {
                var getTypesMethod = typeof(Assembly).Method("GetTypes");
                typesExpr = Call(assemblyParamExpr, getTypesMethod, ArrayTools.Empty<Expr>());
            }
            else
            {
                typesExpr = Property(assemblyParamExpr, definedTypeInfosProperty);
                if (typesExpr.Type == typeof(IEnumerable<TypeInfo>))
                {
                    var typeInfoParamExpr = Parameter(typeof(TypeInfo), "typeInfo");
                    var selectMethod = typeof(Enumerable).Method(nameof(Enumerable.Select), typeof(IEnumerable<>), typeof(Func<,>));
                    var asTypeMethod = typeof(TypeInfo).Method(nameof(TypeInfo.AsType));
                    typesExpr = Call(selectMethod.MakeGenericMethod(typeof(TypeInfo), typeof(Type)),
                        typesExpr,
                        Lambda<Func<TypeInfo, Type>>(
                            Call(typeInfoParamExpr, asTypeMethod, ArrayTools.Empty<Expr>()),
                            typeInfoParamExpr));
                }
            }

            return Lambda<Func<Assembly, IEnumerable<Type>>>(typesExpr, assemblyParamExpr).CompileFast();
        }

        /// <summary>Portable version of PropertyInfo.GetGetMethod.</summary>
        public static MethodInfo GetGetMethodOrNull(this PropertyInfo p, bool includeNonPublic = false) =>
            p.DeclaringType.GetSingleMethodOrNull("get_" + p.Name, includeNonPublic);

        /// <summary>Portable version of PropertyInfo.GetSetMethod.</summary>
        public static MethodInfo GetSetMethodOrNull(this PropertyInfo p, bool includeNonPublic = false) =>
            p.DeclaringType.GetSingleMethodOrNull("set_" + p.Name, includeNonPublic);

        private static readonly Lazy<Func<int>> _getEnvCurrentManagedThreadId = new Lazy<Func<int>>(() =>
        {
            var method = typeof(Environment).GetMethodOrNull("get_CurrentManagedThreadId", ArrayTools.Empty<Type>());
            if (method == null)
                return null;

            var lambdaExpr = Lambda<Func<int>>(Call(method, ArrayTools.Empty<Expr>()), ArrayTools.Empty<ParamExpr>());
            return lambdaExpr
#if FEC_EXPRESSION_INFO
                .ToLambdaExpression()
#endif
                .Compile();
        });

        /// <summary>Returns managed Thread ID either from Environment or Thread.CurrentThread whichever is available.</summary>
        public static int GetCurrentManagedThreadID()
        {
            var resultID = -1;
            GetCurrentManagedThreadID(ref resultID);
            if (resultID == -1)
                resultID = _getEnvCurrentManagedThreadId.Value();
            return resultID;
        }

        static partial void GetCurrentManagedThreadID(ref int threadID);
    }
}