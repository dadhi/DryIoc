/*
The MIT License (MIT)

Copyright (c) 2013 Maksim Volkau

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included AddOrUpdateServiceFactory
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
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;
    using System.Threading;

    /// <summary>IoC Container. Documentation is available at https://bitbucket.org/dadhi/dryioc. </summary>
    public sealed partial class Container : IContainer
    {
        /// <summary>Creates new container, optionally providing <see cref="Rules"/> to modify default container behavior.</summary>
        /// <param name="rules">(optional) Rules to modify container default resolution behavior. 
        /// If not specified, then <see cref="DryIoc.Rules.Default"/> will be used.</param>
        /// <param name="scopeContext">(optional) Scope context to use for <see cref="Reuse.InCurrentScope"/>, default is <see cref="ThreadScopeContext"/>.</param>
        public Container(Rules rules = null, IScopeContext scopeContext = null)
            : this(rules ?? Rules.Default, Ref.Of(Registry.Default), new Scope(), scopeContext ?? GetDefaultScopeContext()) { }

        /// <summary>Creates new container with configured rules.</summary>
        /// <param name="configure">Delegate gets <see cref="DryIoc.Rules.Default"/> as input and may return configured rules.</param>
        /// <param name="scopeContext">(optional) Scope context to use for <see cref="Reuse.InCurrentScope"/>, default is <see cref="ThreadScopeContext"/>.</param>
        public Container(Func<Rules, Rules> configure, IScopeContext scopeContext = null)
            : this(configure.ThrowIfNull()(Rules.Default) ?? Rules.Default, scopeContext) { }

        /// <summary>Shares all of container state except Cache and specifies new rules.</summary>
        /// <param name="configure">(optional) Configure rules, if not specified then uses Rules from current container.</param> 
        /// <param name="scopeContext">(optional) New scope context, if not specified then uses context from current container.</param>
        /// <returns>New container.</returns>
        public IContainer With(Func<Rules, Rules> configure = null, IScopeContext scopeContext = null)
        {
            ThrowIfContainerDisposed();
            var rules = configure == null ? Rules : configure(Rules);
            scopeContext = scopeContext ?? _scopeContext ?? GetDefaultScopeContext();
            if (rules == Rules && scopeContext == _scopeContext)
                return this;
            var registryWithoutCache = Ref.Of(_registry.Value.WithoutCache());
            return new Container(rules, registryWithoutCache, _singletonScope, scopeContext, _openedScope, _disposed);
        }

        /// <summary>Returns new container with all expression, delegate, items cache removed/reset.
        /// It will preserve resolved services in Singleton/Current scope.</summary>
        /// <returns>New container with empty cache.</returns>
        public IContainer WithoutCache()
        {
            ThrowIfContainerDisposed();
            var registryWithoutCache = Ref.Of(_registry.Value.WithoutCache());
            return new Container(Rules, registryWithoutCache, _singletonScope, _scopeContext, _openedScope, _disposed);
        }

        /// <summary>Creates new container with state shared with original except singletons and cache.
        /// Dropping cache is required because singletons are cached in resolution state.</summary>
        /// <returns>New container with empty Singleton Scope.</returns>
        public IContainer WithoutSingletonsAndCache()
        {
            ThrowIfContainerDisposed();
            var registryWithoutCache = Ref.Of(_registry.Value.WithoutCache());
            var newSingletons = new Scope();
            return new Container(Rules, registryWithoutCache, newSingletons, _scopeContext, _openedScope, _disposed);
        }

        /// <summary>Shares all parts with original container But copies registration, so the new registration
        /// won't be visible in original. Registrations include decorators and wrappers as well.</summary>
        /// <param name="preserveCache">(optional) If set preserves cache if you know what to do.</param>
        /// <returns>New container with copy of all registrations.</returns>
        public IContainer WithRegistrationsCopy(bool preserveCache = false)
        {
            ThrowIfContainerDisposed();
            var newRegistry = preserveCache ? _registry.Copy() : Ref.Of(_registry.Value.WithoutCache());
            return new Container(Rules, newRegistry, _singletonScope, _scopeContext, _openedScope, _disposed);
        }

        /// <summary>Container opened scope. May or may not be equal to Current Scope.</summary>
        public IScope OpenedScope { get { return _openedScope; } }

        /// <summary>Returns scope context associated with container.</summary>
        public IScopeContext ScopeContext { get { return _scopeContext; } }

        /// <summary>Creates new container with new opened scope and set this scope as current in ambient scope context.</summary>
        /// <param name="scopeName">(optional) Name for opened scope to allow reuse to identify the scope.</param>
        /// <param name="configure">(optional) Configure rules, if not specified then uses Rules from current container.</param> 
        /// <returns>New container with different current scope and optionally Rules.</returns>
        /// <example><code lang="cs"><![CDATA[
        /// using (var scoped = container.OpenScope())
        /// {
        ///     var handler = scoped.Resolve<IHandler>();
        ///     handler.Handle(data);
        /// }
        /// ]]></code></example>
        /// <remarks>Be sure to Dispose returned scope, because if not - ambient context will keep scope with it's items
        /// introducing memory leaks and likely preventing to open other scopes.</remarks>
        public IContainer OpenScope(object scopeName = null, Func<Rules, Rules> configure = null)
        {
            ThrowIfContainerDisposed();

            scopeName = scopeName ?? (_openedScope == null ? _scopeContext.RootScopeName : null);
            var newOpenedScope = new Scope(_openedScope, scopeName);

            // Replacing current context scope with new nested only if current is the same as nested parent, otherwise throw.
            _scopeContext.SetCurrent(scope =>
                 newOpenedScope.ThrowIf(scope != _openedScope, Error.NOT_DIRECT_SCOPE_PARENT, _openedScope, scope));

            var rules = configure == null ? Rules : configure(Rules);
            return new Container(rules, _registry, _singletonScope, _scopeContext, newOpenedScope, _disposed);
        }

        /// <summary>Creates scoped container with scope bound to container itself, and not some ambient context.
        /// Current container scope will become parent for new scope.</summary>
        /// <param name="scopeName">(optional) Scope name.</param>
        /// <returns>New container with all state shared except new created scope and context.</returns>
        public IContainer OpenScopeWithoutContext(object scopeName = null)
        {
            ThrowIfContainerDisposed();

            scopeName = scopeName ?? (_openedScope == null ? NO_CONTEXT_ROOT_SCOPE_NAME : null);
            var newOpenedScope = new Scope(_openedScope, scopeName);

            return new Container(Rules, _registry, _singletonScope, /*no context*/null, newOpenedScope, _disposed);
        }

        /// <summary>Provide root scope name for <see cref="OpenScopeWithoutContext"/></summary>
        public static readonly object NO_CONTEXT_ROOT_SCOPE_NAME = typeof(IContainer);

        /// <summary>Creates container (facade) that fallbacks to this container for unresolved services.
        /// Facade is the new empty container, with the same rules and scope context as current container. 
        /// It could be used for instance to create Test facade over original container with replacing some services with test ones.</summary>
        /// <remarks>Singletons from container are not reused by facade - when you resolve singleton directly from parent and then ask for it from child, it will return another object.
        /// To achieve that you may use <see cref="IContainer.OpenScope"/> with <see cref="Reuse.InCurrentScope"/>.</remarks>
        /// <returns>New facade container.</returns>
        public IContainer CreateFacade()
        {
            ThrowIfContainerDisposed();
            return new Container(Rules.WithUnknownServiceResolver(FallbackToContainers(this)), _scopeContext);
        }

        /// <summary>The rule to fallback to multiple containers in order to resolve service unresolved in rule owner.</summary>
        /// <param name="containers">One or many container to resolve from if service is not resolved from original container.</param>
        /// <returns>New rule with fallback to resolve service from specified containers.</returns>
        /// <remarks>There are two options to detach containers link: 1st - save original container before linking to containers and use it; 
        /// 2nd - save rule returned by this method and then remove saved rule from container using <see cref="DryIoc.Rules.WithoutUnknownServiceResolver"/>.</remarks>
        public static Rules.UnknownServiceResolver FallbackToContainers(params IContainer[] containers)
        {
            var containerWeakRefs = containers
                .ThrowIf(ArrayTools.IsNullOrEmpty)
                .Select(p => p.ContainerWeakRef).ToArray();

            return request =>
            {
                Factory factory = null;
                for (var i = 0; i < containerWeakRefs.Length && factory == null; i++)
                {
                    var containerWeakRef = containerWeakRefs[i];
                    var containerRequest = request.SwitchContainer(containerWeakRef);

                    // Enable continue to next parent if factory is not found in first parent by
                    // updating IfUnresolved policy to ReturnDefault.
                    if (containerRequest.IfUnresolved != IfUnresolved.ReturnDefault)
                        containerRequest = containerRequest.UpdateServiceInfo(info => // NOTE Smells.
                            ServiceInfo.Of(info.ServiceType, IfUnresolved.ReturnDefault).InheritInfo(info));

                    var container = containerWeakRef.GetTarget();
                    factory = container.ResolveFactory(containerRequest);
                }

                return factory == null ? null : 
                    new ExpressionFactory(r => factory.GetExpressionOrDefault(r));
            };
        }

        /// <summary>Disposes container current scope and that means container itself.</summary>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
                return;

            if (_openedScope != null) // for container created with Open(Bound)Scope
            {
                if (_scopeContext != null)
                {
                    // try to revert context to parent scope, otherwise if context and opened scope not in sync - do nothing
                    var openedScope = _openedScope;
                    _scopeContext.SetCurrent(scope => scope == openedScope ? scope.Parent : scope);
                }

                _openedScope.Dispose();
            }
            else // for Container created with constructor.
            {
                if (_scopeContext is IDisposable)
                    ((IDisposable)_scopeContext).Dispose();

                _singletonScope.Dispose();

                _registry.Swap(_ => Registry.Empty);

                Rules = Rules.Empty;
            }
        }

        #region Static state

        internal static readonly ParameterExpression StateParamExpr = 
            Expression.Parameter(typeof(AppendableArray), "state");

        internal static readonly ParameterExpression ResolverContextProviderParamExpr = 
            Expression.Parameter(typeof(IResolverContextProvider), "r");
        
        internal static readonly Expression ResolverContextExpr = 
            Expression.Property(ResolverContextProviderParamExpr, "Resolver");

        internal static readonly Expression ResolverExpr =
            Expression.Convert(ResolverContextExpr, typeof(IResolver));

        internal static readonly ParameterExpression ResolutionScopeParamExpr = 
            Expression.Parameter(typeof(IScope), "scope");

        internal static Expression GetResolutionScopeExpression(Request request)
        {
            if (request.Scope != null)
                return ResolutionScopeParamExpr;

            var parent = request.Enumerate().Last();
            var registeredServiceType = request.Container.UnwrapServiceType(parent.RequiredServiceType ?? parent.ServiceType);
            var parentServiceTypeExpr = request.Container.GetOrAddStateItemExpression(registeredServiceType, typeof(Type));
            var parentServiceKeyExpr = Expression.Convert(request.Container.GetOrAddStateItemExpression(parent.ServiceKey), typeof(object));
            return Expression.Call(ResolverContextExpr, "GetOrCreateResolutionScope", null, ResolutionScopeParamExpr, parentServiceTypeExpr, parentServiceKeyExpr);
        }

        #endregion

        #region IRegistrator

        /// <summary>Returns all registered service factories with their Type and optional Key.</summary>
        /// <returns>Existing registrations.</returns>
        /// <remarks>Decorator and Wrapper types are not included.</remarks>
        public IEnumerable<ServiceTypeKeyFactory> GetServiceRegistrations()
        {
            return _registry.Value.GetServiceRegistrations();
        }

        /// <summary>Stores factory into container using <paramref name="serviceType"/> and <paramref name="serviceKey"/> as key
        /// for later lookup.</summary>
        /// <param name="factory">Any subtypes of <see cref="Factory"/>.</param>
        /// <param name="serviceType">Type of service to resolve later.</param>
        /// <param name="serviceKey">(optional) Service key of any type with <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>
        /// implemented.</param>
        /// <param name="ifAlreadyRegistered">(optional) Says how to handle existing registration with the same 
        /// <paramref name="serviceType"/> and <paramref name="serviceKey"/>.</param>
        /// <returns>True if factory was added to registry, false otherwise. 
        /// False may be in case of <see cref="IfAlreadyRegistered.Keep"/> setting and already existing factory.</returns>
        public bool Register(Factory factory, Type serviceType, object serviceKey, IfAlreadyRegistered ifAlreadyRegistered)
        {
            ThrowIfContainerDisposed();
            factory.ThrowIfNull().CheckBeforeRegistration(this, serviceType.ThrowIfNull(), serviceKey);

            var isRegistered = false;
            _registry.Swap(registry =>
            {
                var newRegistry = registry.Register(factory, serviceType, ifAlreadyRegistered, serviceKey);
                isRegistered = newRegistry != registry;
                return newRegistry;
            });
            return isRegistered;
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
            return _registry.Value.IsRegistered(serviceType.ThrowIfNull(), serviceKey, factoryType, condition);
        }

        /// <summary>Removes specified factory from registry. 
        /// Factory is removed only from registry, if there is relevant cache, it will be kept.
        /// Use <see cref="WithoutCache"/> to remove all the cache.</summary>
        /// <param name="serviceType">Service type to look for.</param>
        /// <param name="serviceKey">Service key to look for.</param>
        /// <param name="factoryType">Expected factory type.</param>
        /// <param name="condition">Expected factory condition.</param>
        public void Unregister(Type serviceType, object serviceKey, FactoryType factoryType, Func<Factory, bool> condition)
        {
            ThrowIfContainerDisposed();
            _registry.Swap(r => r.Unregister(factoryType, serviceType, serviceKey, condition));
        }

        #endregion

        #region IResolver

        object IResolver.ResolveDefault(Type serviceType, IfUnresolved ifUnresolved, IScope scope)
        {
            var registry = _registry.Value;
            var factoryDelegate = registry.DefaultFactoryDelegateCache.Value.GetValueOrDefault(serviceType);
            return factoryDelegate != null
                ? factoryDelegate(registry.ResolutionStateCache.Value, _containerWeakRef, scope)
                : ResolveAndCacheDefaultDelegate(serviceType, ifUnresolved, scope);
        }

        private object ResolveAndCacheDefaultDelegate(Type serviceType, IfUnresolved ifUnresolved, IScope scope)
        {
            ThrowIfContainerDisposed();

            var request = _emptyRequest.Push(serviceType, ifUnresolved: ifUnresolved, scope: scope);
            var factory = ((IContainer)this).ResolveFactory(request);

            // The situation is possible for multiple default services registered.
            if (request.ServiceKey != null)
                return ((IResolver)this).ResolveKeyed(serviceType, request.ServiceKey, ifUnresolved, null, scope);

            var factoryDelegate = factory == null ? null : factory.GetDelegateOrDefault(request);
            if (factoryDelegate == null)
                return null;

            var registry = _registry.Value;
            var resultService = factoryDelegate(registry.ResolutionStateCache.Value, _containerWeakRef, scope);

            if (factory.Setup.CacheFactoryExpression)
                registry.DefaultFactoryDelegateCache.Swap(_ => _.AddOrUpdate(serviceType, factoryDelegate));

            return resultService;
        }

        object IResolver.ResolveKeyed(Type serviceType, object serviceKey, 
            IfUnresolved ifUnresolved, Type requiredServiceType, IScope scope)
        {
            var cacheServiceKey = serviceKey;
            if (requiredServiceType != null && 
                requiredServiceType != serviceType)
            {
                var wrappedServiceType = ((IContainer)this).UnwrapServiceType(serviceType)
                    .ThrowIfNotOf(requiredServiceType, Error.WRAPPED_NOT_ASSIGNABLE_FROM_REQUIRED_TYPE, serviceType);

                if (serviceType == wrappedServiceType)
                    serviceType = requiredServiceType;
                else
                    cacheServiceKey = serviceKey == null ? requiredServiceType
                        : (object)new KV<Type, object>(requiredServiceType, serviceKey);
            }

            if (scope != null)
                scope = new Scope(scope, new KV<Type, object>(serviceType, serviceKey));

            // If service key is null, then use resolve default instead of keyed.
            if (cacheServiceKey == null)
                return ((IResolver)this).ResolveDefault(serviceType, ifUnresolved, scope);

            var registry = _registry.Value;

            var cacheKey = new KV<Type, object>(serviceType, cacheServiceKey);
            var factoryDelegate = registry.KeyedFactoryDelegateCache.Value.GetValueOrDefault(cacheKey);
            if (factoryDelegate != null)
                return factoryDelegate(registry.ResolutionStateCache.Value, _containerWeakRef, scope);

            ThrowIfContainerDisposed();

            var request = _emptyRequest.Push(serviceType, serviceKey, ifUnresolved, requiredServiceType, scope);

            var factory = ((IContainer)this).ResolveFactory(request);
            factoryDelegate = factory == null ? null : factory.GetDelegateOrDefault(request);
            if (factoryDelegate == null)
                return null;

            var resultService = factoryDelegate(request.Container.ResolutionStateCache, _containerWeakRef, scope);

            // Cache factory only after it is invoked without errors to prevent not-working entries in cache.
            if (factory.Setup.CacheFactoryExpression)
                registry.KeyedFactoryDelegateCache.Swap(_ => _.AddOrUpdate(cacheKey, factoryDelegate));

            return resultService;
        }

        IEnumerable<object> IResolver.ResolveMany(Type serviceType, object serviceKey, Type requiredServiceType, object compositeParentKey, IScope scope)
        {
            var container = ((IContainer)this);
            var itemServiceType = requiredServiceType ?? serviceType;
            var items = container.GetAllServiceFactories(itemServiceType);
            var itemsWithVariance = !itemServiceType.IsGeneric() ? null :
                container.GetServiceRegistrations().Where(x =>
                    itemServiceType != x.ServiceType && x.ServiceType.IsClosedGeneric() &&
                    itemServiceType.GetGenericTypeDefinition() == x.ServiceType.GetGenericTypeDefinition() &&
                    x.ServiceType.IsAssignableTo(itemServiceType));

            if (serviceKey != null) // include only single item matching key.
            {
                items = items.Where(x => serviceKey.Equals(x.Key));
                if (itemsWithVariance != null)
                    itemsWithVariance = itemsWithVariance.Where(x => serviceKey.Equals(x.OptionalServiceKey));
            }

            if (compositeParentKey != null) // exclude composite parent from items
            {
                items = items.Where(x => !compositeParentKey.Equals(x.Key));
                if (itemsWithVariance != null)
                    itemsWithVariance = itemsWithVariance.Where(x => !compositeParentKey.Equals(x.OptionalServiceKey));
            }

            foreach (var item in items)
            {
                var service = ((IResolver)this).ResolveKeyed(serviceType, item.Key, IfUnresolved.ReturnDefault, requiredServiceType, scope);
                if (service != null) // skip unresolved items
                    yield return service;
            }

            if (itemsWithVariance != null)
                foreach (var item in itemsWithVariance)
                {
                    var service = ((IResolver)this).ResolveKeyed(serviceType, item.OptionalServiceKey, IfUnresolved.ReturnDefault, item.ServiceType, scope);
                    if (service != null) // skip unresolved items
                        yield return service;
                }
        }

        private void ThrowIfContainerDisposed()
        {
            Throw.If(IsDisposed, Error.CONTAINER_IS_DISPOSED);
        }

        #endregion

        #region IResolverContext

        /// <summary>Scope associated with container.</summary>
        IScope IResolverContext.SingletonScope
        {
            get { return _singletonScope; }
        }

        /// <summary>Gets current scope matching the <paramref name="name"/>. 
        /// If name is null then current scope is returned, or if there is no current scope then exception thrown.</summary>
        /// <param name="name">May be null</param> <param name="throwIfNotFound">Says to throw if no scope found.</param>
        /// <returns>Found scope or throws exception.</returns>
        /// <exception cref="ContainerException"> with code <see cref="Error.NO_MATCHED_SCOPE_FOUND"/>.</exception>
        IScope IResolverContext.GetCurrentNamedScope(object name, bool throwIfNotFound)
        {
            var currentScope = _scopeContext == null ? _openedScope : _scopeContext.GetCurrentOrDefault();
            return currentScope == null 
                ? (throwIfNotFound ? Throw.For<IScope>(Error.NO_CURRENT_SCOPE) : null)
                : GetMatchingScopeOrDefault(currentScope, name)
                ?? (throwIfNotFound ? Throw.For<IScope>(Error.NO_MATCHED_SCOPE_FOUND, name) : null);
        }

        private static IScope GetMatchingScopeOrDefault(IScope scope, object name)
        {
            if (name != null)
                while (scope != null && !name.Equals(scope.Name))
                    scope = scope.Parent;
            return scope;
        }

        /// <summary>Check if scope is not null, then just returns it, otherwise will create and return it.</summary>
        /// <param name="scope">May be null scope.</param>
        /// <param name="serviceType">Marking scope with resolved service type.</param> 
        /// <param name="serviceKey">Marking scope with resolved service key.</param>
        /// <returns>Input <paramref name="scope"/> ensuring it is not null.</returns>
        IScope IResolverContext.GetOrCreateResolutionScope(ref IScope scope, Type serviceType, object serviceKey)
        {
            return scope ?? (scope = new Scope(null, new KV<Type, object>(serviceType, serviceKey)));
        }

        /// <summary>If both <paramref name="assignableFromServiceType"/> and <paramref name="serviceKey"/> are null, 
        /// then returns input <paramref name="scope"/>.
        /// Otherwise searches scope hierarchy to find first scope with: Type assignable <paramref name="assignableFromServiceType"/> and 
        /// Key equal to <paramref name="serviceKey"/>.</summary>
        /// <param name="scope">Scope to start matching with Type and Key specified.</param>
        /// <param name="assignableFromServiceType">Type to match.</param> <param name="serviceKey">Key to match.</param>
        /// <param name="outermost">If true - commands to look for outermost match instead of nearest.</param>
        /// <param name="throwIfNotFound">Says to throw if no scope found.</param>
        /// <returns>Matching scope or throws <see cref="ContainerException"/>.</returns>
        IScope IResolverContext.GetMatchingResolutionScope(IScope scope, Type assignableFromServiceType, object serviceKey,
            bool outermost, bool throwIfNotFound)
        {
            return GetMatchingScopeOrDefault(scope, assignableFromServiceType, serviceKey, outermost)
                ?? (!throwIfNotFound ? null : Throw.For<IScope>(Error.NO_MATCHED_SCOPE_FOUND,
                new KV<Type, object>(assignableFromServiceType, serviceKey)));
        }

        private static IScope GetMatchingScopeOrDefault(IScope scope, Type assignableFromServiceType, object serviceKey, bool outermost)
        {
            if (assignableFromServiceType == null && serviceKey == null)
                return scope;

            IScope matchedScope = null;
            while (scope != null)
            {
                var name = scope.Name as KV<Type, object>;
                if (name != null &&
                    (assignableFromServiceType == null || name.Key.IsAssignableTo(assignableFromServiceType)) &&
                    (serviceKey == null || serviceKey.Equals(name.Value)))
                {
                    matchedScope = scope;
                    if (!outermost) // break on first found match.
                        break;
                }
                scope = scope.Parent;
            }

            return matchedScope;
        }       

        #endregion

        #region IContainer

        /// <summary>The rules object defines policies per container for registration and resolution.</summary>
        public Rules Rules { get; private set; }

        /// <summary>Indicates that container is disposed.</summary>
        public bool IsDisposed
        {
            get { return _disposed == 1; }
        }

        /// <summary>Empty request bound to container. All other requests are created by pushing to empty request.</summary>
        Request IContainer.EmptyRequest
        {
            get { return _emptyRequest; }
        }

        /// <summary>Self weak reference, with readable message when container is GCed/Disposed.</summary>
        ContainerWeakRef IContainer.ContainerWeakRef
        {
            get { return _containerWeakRef; }
        }

        Factory IContainer.ResolveFactory(Request request)
        {
            var factory = GetServiceFactoryOrDefault(request, Rules.FactorySelector);
            if (factory != null && factory.Provider != null && // handle provider: open-generic, etc.
                (factory = factory.Provider.ProvideConcreteFactory(request)) != null)
                Register(factory, request.ServiceType, request.ServiceKey, IfAlreadyRegistered.Replace);

            var unknownServiceResolvers = Rules.UnknownServiceResolvers;
            if (factory == null && !unknownServiceResolvers.IsNullOrEmpty())
                for (var i = 0; factory == null && i < unknownServiceResolvers.Length; i++)
                    factory = unknownServiceResolvers[i](request);

            if (factory == null)
                Throw.If(request.IfUnresolved == IfUnresolved.Throw, Error.UNABLE_TO_RESOLVE_SERVICE, request);

            return factory;
        }

        Factory IContainer.GetServiceFactoryOrDefault(Request request)
        {
            return GetServiceFactoryOrDefault(request, Rules.FactorySelector);
        }

        IEnumerable<KV<object, Factory>> IContainer.GetAllServiceFactories(Type serviceType)
        {
            var registry = _registry.Value;

            var entry = registry.Services.GetValueOrDefault(serviceType);
            if (entry == null && serviceType.IsClosedGeneric())
                entry = registry.Services.GetValueOrDefault(serviceType.GetGenericDefinitionOrNull());

            return entry == null ? Enumerable.Empty<KV<object, Factory>>()
                : entry is Factory ? new[] { new KV<object, Factory>(DefaultKey.Value, (Factory)entry) }
                : ((FactoriesEntry)entry).Factories.Enumerate();
        }

        Expression IContainer.GetDecoratorExpressionOrDefault(Request request)
        {
            var registry = _registry.Value;

            // Stop if no decorators registered.
            var decorators = registry.Decorators;
            if (decorators.IsEmpty)
                return null;

            // Decorators for non service types are not supported.
            var factoryType = request.ResolvedFactory.FactoryType;
            if (factoryType != FactoryType.Service)
                return null;

            // We are already resolving decorator for the service, so stop now.
            var parent = request.ParentNonWrapper();
            if (!parent.IsEmpty && parent.ResolvedFactory.FactoryType == FactoryType.Decorator)
                return null;

            var serviceType = request.ServiceType;
            var decoratorFuncType = typeof(Func<,>).MakeGenericType(serviceType, serviceType);

            // First look for Func decorators Func<TService,TService> and initializers Action<TService>.
            var funcDecoratorExpr = GetFuncDecoratorExpressionOrDefault(decoratorFuncType, decorators, request);

            // Next look for normal decorators.
            var serviceDecorators = decorators.GetValueOrDefault(serviceType);
            var openGenericDecoratorIndex = serviceDecorators == null ? 0 : serviceDecorators.Length;
            var openGenericServiceType = request.ServiceType.GetGenericDefinitionOrNull();
            if (openGenericServiceType != null)
                serviceDecorators = serviceDecorators.Append(decorators.GetValueOrDefault(openGenericServiceType));

            Expression resultDecorator = funcDecoratorExpr;
            if (serviceDecorators != null)
            {
                for (var i = 0; i < serviceDecorators.Length; i++)
                {
                    var decorator = serviceDecorators[i];
                    var decoratorRequest = request.ResolveWithFactory(decorator);
                    var decoratorCondition = decorator.Setup.Condition;
                    if (decoratorCondition == null || decoratorCondition(request))
                    {
                        // Cache closed generic registration produced by open-generic decorator.
                        if (i >= openGenericDecoratorIndex && decorator.Provider != null)
                        {
                            decorator = decorator.Provider.ProvideConcreteFactory(request);
                            Register(decorator, serviceType, null, IfAlreadyRegistered.AppendNotKeyed);
                        }

                        var decoratorExpr = GetCachedFactoryExpressionOrDefault(decorator.FactoryID);
                        if (decoratorExpr == null)
                        {
                            decoratorRequest = decoratorRequest.WithFuncArgs(decoratorFuncType,
                                funcArgPrefix: i.ToString()); // use prefix to generate non-conflicting Func argument names

                            decoratorExpr = decorator.GetExpressionOrDefault(decoratorRequest)
                                .ThrowIfNull(Error.CANT_CREATE_DECORATOR_EXPR, decoratorRequest);

                            var decoratedArgWasUsed = decoratorRequest.FuncArgs.Key[0];
                            decoratorExpr = !decoratedArgWasUsed ? decoratorExpr // case of replacing decorator.
                                : Expression.Lambda(decoratorFuncType, decoratorExpr, decoratorRequest.FuncArgs.Value);

                            CacheFactoryExpression(decorator.FactoryID, decoratorExpr);
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

        Factory IContainer.GetWrapperFactoryOrDefault(Type serviceType)
        {
            return _registry.Value.GetWrapperOrDefault(serviceType);
        }

        Type IContainer.UnwrapServiceType(Type serviceType)
        {
            var itemType = serviceType.GetElementTypeOrNull();
            if (itemType != null)
                return ((IContainer)this).UnwrapServiceType(itemType);

            var factory = ((IContainer)this).GetWrapperFactoryOrDefault(serviceType);
            if (factory == null)
                return serviceType;

            var wrapperSetup = (Setup.WrapperSetup)factory.Setup;
            var wrappedServiceType = wrapperSetup.UnwrapServiceType(serviceType);

            // Unwrap further recursively.
            return ((IContainer)this).UnwrapServiceType(wrappedServiceType);
        }

        /// <summary>For given instance resolves and sets properties and fields.</summary>
        /// <param name="instance">Service instance with properties to resolve and initialize.</param>
        /// <param name="propertiesAndFields">(optional) Function to select properties and fields, overrides all other rules if specified.</param>
        /// <returns>Instance with assigned properties and fields.</returns>
        /// <remarks>Different Rules could be combined together using <see cref="PropertiesAndFields.And"/> method.</remarks>        
        public object InjectPropertiesAndFields(object instance, PropertiesAndFieldsSelector propertiesAndFields)
        {
            propertiesAndFields = propertiesAndFields ?? Rules.PropertiesAndFields ?? PropertiesAndFields.Auto;

            var instanceType = instance.ThrowIfNull().GetType();

            var request = _emptyRequest.Push(instanceType)
                .ResolveWithFactory(new ReflectionFactory(instanceType));

            foreach (var serviceInfo in propertiesAndFields(request))
                if (serviceInfo != null)
                {
                    var details = serviceInfo.Details;
                    var value = request.Container.Resolve(serviceInfo.ServiceType,
                        details.ServiceKey, details.IfUnresolved, details.RequiredServiceType);
                    if (value != null)
                        serviceInfo.SetValue(instance, value);
                }

            return instance;
        }

        /// <summary>Adds factory expression to cache identified by factory ID (<see cref="Factory.FactoryID"/>).</summary>
        /// <param name="factoryID">Key in cache.</param>
        /// <param name="factoryExpression">Value to cache.</param>
        public void CacheFactoryExpression(int factoryID, Expression factoryExpression)
        {
            _registry.Value.FactoryExpressionCache.Swap(_ => _.AddOrUpdate(factoryID, factoryExpression));
        }

        /// <summary>Searches and returns cached factory expression, or null if not found.</summary>
        /// <param name="factoryID">Factory ID to lookup by.</param> <returns>Found expression or null.</returns>
        public Expression GetCachedFactoryExpressionOrDefault(int factoryID)
        {
            return _registry.Value.FactoryExpressionCache.Value.GetValueOrDefault(factoryID) as Expression;
        }

        /// <summary>State item objects which may include: singleton instances for fast access, reuses, reuse wrappers, factory delegates, etc.</summary>
        public AppendableArray ResolutionStateCache
        {
            get { return _registry.Value.ResolutionStateCache.Value; }
        }

        /// <summary>Adds item if it is not already added to state, returns added or existing item index.</summary>
        /// <param name="item">Item to find in existing items with <see cref="object.Equals(object, object)"/> or add if not found.</param>
        /// <returns>Index of found or added item.</returns>
        public int GetOrAddStateItem(object item)
        {
            var index = -1;
            _registry.Value.ResolutionStateCache.Swap(state =>
            {
                index = state.IndexOf(item);
                if (index == -1)
                    index = (state = state.Append(item)).Length - 1;
                return state;
            });
            return index;
        }

        /// <summary>If possible wraps added item in <see cref="ConstantExpression"/> (possible for primitive type, Type, strings), 
        /// otherwise invokes <see cref="GetOrAddStateItem"/> and wraps access to added item (by returned index) into expression: state => state.Get(index).</summary>
        /// <param name="item">Item to wrap or to add.</param> <param name="itemType">(optional) Specific type of item, otherwise item <see cref="object.GetType()"/>.</param>
        /// <param name="throwIfStateRequired">(optional) Enable filtering of stateful items.</param>
        /// <returns>Returns constant or state access expression for added items.</returns>
        public Expression GetOrAddStateItemExpression(object item, Type itemType = null, bool throwIfStateRequired = false)
        {
            if (item == null)
                return itemType == null ? (Expression)Expression.Constant(null)
                    : Expression.Convert(Expression.Constant(null), itemType);

            itemType = itemType ?? item.GetType();

            if (Rules.ItemToExpressionConverter != null)
            {
                var expression = Rules.ItemToExpressionConverter(item, itemType);
                if (expression != null)
                    return expression;
            }

            if (throwIfStateRequired)
                Throw.It(Error.STATE_IS_REQUIRED_TO_USE_ITEM, item);

            var itemIndex = GetOrAddStateItem(item);
            var indexExpr = Expression.Constant(itemIndex, typeof(int));
            var getItemByIndexExpr = Expression.Call(StateParamExpr, _getItemMethod, indexExpr);
            return Expression.Convert(getItemByIndexExpr, itemType);
        }

        private static readonly MethodInfo _getItemMethod = typeof(AppendableArray).GetSingleDeclaredMethodOrNull("Get");

        #endregion

        #region Decorators support

        private static LambdaExpression GetFuncDecoratorExpressionOrDefault(Type decoratorFuncType,
            HashTree<Type, Factory[]> decorators, Request request)
        {
            LambdaExpression funcDecoratorExpr = null;

            var serviceType = request.ServiceType;

            // Look first for Action<ImplementedType> initializer-decorator
            var implementationType = request.ImplementationType ?? serviceType;
            var implementedTypes = implementationType.GetImplementedTypes(
                ReflectionTools.IncludeImplementedType.SourceType | ReflectionTools.IncludeImplementedType.ObjectType);

            for (var i = 0; i < implementedTypes.Length; i++)
            {
                var implementedType = implementedTypes[i];
                var initializerActionType = typeof(Action<>).MakeGenericType(implementedType);
                var initializerFactories = decorators.GetValueOrDefault(initializerActionType);
                if (initializerFactories != null)
                {
                    var doAction = _doMethod.MakeGenericMethod(implementedType, implementationType);
                    for (var j = 0; j < initializerFactories.Length; j++)
                    {
                        var initializerFactory = initializerFactories[j];
                        var condition = initializerFactory.Setup.Condition;
                        if (condition == null || condition(request))
                        {
                            var decoratorRequest =
                                request.UpdateServiceInfo(_ => ServiceInfo.Of(initializerActionType))
                                    .ResolveWithFactory(initializerFactory);
                            var actionExpr = initializerFactory.GetExpressionOrDefault(decoratorRequest);
                            if (actionExpr != null)
                                ComposeDecoratorFuncExpression(ref funcDecoratorExpr, serviceType,
                                    Expression.Call(doAction, actionExpr));
                        }
                    }
                }
            }

            // Then look for decorators registered as Func of decorated service returning decorator - Func<TService, TService>.
            var funcDecoratorFactories = decorators.GetValueOrDefault(decoratorFuncType);
            if (funcDecoratorFactories != null)
            {
                for (var i = 0; i < funcDecoratorFactories.Length; i++)
                {
                    var decoratorFactory = funcDecoratorFactories[i];
                    var decoratorRequest = request
                        .UpdateServiceInfo(_ => ServiceInfo.Of(decoratorFuncType))
                        .ResolveWithFactory(decoratorFactory);

                    var condition = decoratorFactory.Setup.Condition;
                    if (condition == null || condition(request))
                    {
                        var funcExpr = decoratorFactory.GetExpressionOrDefault(decoratorRequest);
                        if (funcExpr != null)
                            ComposeDecoratorFuncExpression(ref funcDecoratorExpr, serviceType, funcExpr);
                    }
                }
            }

            return funcDecoratorExpr;
        }

        private static void ComposeDecoratorFuncExpression(ref LambdaExpression result, Type serviceType, Expression decoratorFuncExpr)
        {
            if (result == null)
            {
                var decorated = Expression.Parameter(serviceType, "decorated" + serviceType.Name);
                result = Expression.Lambda(Expression.Invoke(decoratorFuncExpr, decorated), decorated);
            }
            else
            {
                var decorateDecorator = Expression.Invoke(decoratorFuncExpr, result.Body);
                result = Expression.Lambda(decorateDecorator, result.Parameters[0]);
            }
        }

        private static readonly MethodInfo _doMethod = typeof(Container).GetSingleDeclaredMethodOrNull("DoAction");
        internal static Func<T, R> DoAction<T, R>(Action<T> action) where R : T
        {
            return x => { action(x); return (R)x; };
        }

        #endregion

        #region Factories Add/Get

        private sealed class FactoriesEntry
        {
            public readonly DefaultKey LastDefaultKey;
            public readonly HashTree<object, Factory> Factories;

            public FactoriesEntry(DefaultKey lastDefaultKey, HashTree<object, Factory> factories)
            {
                LastDefaultKey = lastDefaultKey;
                Factories = factories;
            }
        }

        private Factory GetServiceFactoryOrDefault(Request request, Rules.FactorySelectorRule factorySelector)
        {
            var serviceType = request.ServiceType;
            var serviceKey = request.ServiceKey;
            var services = _registry.Value.Services;

            var entry = services.GetValueOrDefault(serviceType);
            if (serviceType.IsGeneric()) // keep entry or find one for open-generic
            {
                var serviceOpenGenericType = serviceType.GetGenericDefinitionOrNull();
                if (entry == null) // just find new one for open-generic
                    entry = services.GetValueOrDefault(serviceOpenGenericType);
                else if (serviceKey != null && (
                    entry is Factory && !DefaultKey.Value.Equals(serviceKey) ||
                    entry is FactoriesEntry && ((FactoriesEntry)entry).Factories.GetValueOrDefault(serviceKey) == null))
                {
                    var genericEntry = services.GetValueOrDefault(serviceOpenGenericType);
                    if (genericEntry != null)
                        entry = genericEntry;
                }
            }

            if (entry == null) // no entry - no factories: return earlier
                return null;

            if (factorySelector != null) // handle selector
            {
                var allFactories = entry is Factory
                    ? new[] { new KeyValuePair<object, Factory>(DefaultKey.Value, (Factory)entry) }
                    : ((FactoriesEntry)entry).Factories.Enumerate()
                        .Where(f => f.Value.Setup.Condition == null || f.Value.Setup.Condition(request))
                        .Select(f => new KeyValuePair<object, Factory>(f.Key, f.Value))
                        .ToArray();
                return factorySelector(request, allFactories);
            }

            var factory = entry as Factory;
            if (factory != null)
                return serviceKey != null && !DefaultKey.Value.Equals(serviceKey) ? null 
                    : factory.Setup.Condition != null && !factory.Setup.Condition(request) ? null
                    : factory;

            var factories = ((FactoriesEntry)entry).Factories;
            if (serviceKey != null)
            {
                factory = factories.GetValueOrDefault(serviceKey);
                return factory != null && 
                    factory.Setup.Condition != null && !factory.Setup.Condition(request) 
                    ? null : factory;
            }

            var defaultFactories = factories.Enumerate()
                .Where(x => x.Key is DefaultKey &&
                    (x.Value.Setup.Condition == null || x.Value.Setup.Condition(request)))
                    .ToArray();

            if (defaultFactories.Length != 1)
            {
                Throw.If(defaultFactories.Length > 1 && request.IfUnresolved == IfUnresolved.Throw, 
                    Error.EXPECTED_SINGLE_DEFAULT_FACTORY, serviceType, defaultFactories);
                return null;
            }

            var defaultFactory = defaultFactories[0];

            // NOTE: For resolution root sets correct default key to be used in delegate cache.
            if (request.Parent.IsEmpty)
                request.ChangeServiceKey(defaultFactory.Key);
            return defaultFactory.Value;
        }

        #endregion

        #region Implementation

        private int _disposed;

        private readonly Ref<Registry> _registry;

        private readonly ContainerWeakRef _containerWeakRef;
        private readonly Request _emptyRequest;

        private readonly IScope _singletonScope;
        private readonly IScope _openedScope;
        private readonly IScopeContext _scopeContext;

        private sealed class Registry
        {
            public static readonly Registry Empty = new Registry();
            public static readonly Registry Default = new Registry(WrappersSupport.Wrappers);

            // Factories:
            public readonly HashTree<Type, object> Services;
            public readonly HashTree<Type, Factory[]> Decorators;
            public readonly HashTree<Type, Factory> Wrappers;

            // Cache:
            public readonly Ref<HashTree<Type, FactoryDelegate>> DefaultFactoryDelegateCache;
            public readonly Ref<HashTree<KV<Type, object>, FactoryDelegate>> KeyedFactoryDelegateCache;
            public readonly Ref<IntKeyTree> FactoryExpressionCache;
            public readonly Ref<AppendableArray> ResolutionStateCache;

            public Registry WithoutCache()
            {
                return new Registry(Services, Decorators, Wrappers,
                    Ref.Of(HashTree<Type, FactoryDelegate>.Empty), Ref.Of(HashTree<KV<Type, object>, FactoryDelegate>.Empty),
                    Ref.Of(IntKeyTree.Empty), Ref.Of(AppendableArray.Empty));
            }

            private Registry(HashTree<Type, Factory> wrapperFactories = null)
                : this(HashTree<Type, object>.Empty, 
                    HashTree<Type, Factory[]>.Empty,
                    wrapperFactories ?? HashTree<Type, Factory>.Empty,
                    Ref.Of(HashTree<Type, FactoryDelegate>.Empty),
                    Ref.Of(HashTree<KV<Type, object>, FactoryDelegate>.Empty),
                    Ref.Of(IntKeyTree.Empty),
                    Ref.Of(AppendableArray.Empty)) { }

            private Registry(
                HashTree<Type, object> services,
                HashTree<Type, Factory[]> decorators,
                HashTree<Type, Factory> wrappers,
                Ref<HashTree<Type, FactoryDelegate>> defaultFactoryDelegateCache,
                Ref<HashTree<KV<Type, object>, FactoryDelegate>> keyedFactoryDelegateCache,
                Ref<IntKeyTree> factoryExpressionCache,
                Ref<AppendableArray> resolutionStateCache)
            {
                Services = services;
                Decorators = decorators;
                Wrappers = wrappers;
                DefaultFactoryDelegateCache = defaultFactoryDelegateCache;
                KeyedFactoryDelegateCache = keyedFactoryDelegateCache;
                FactoryExpressionCache = factoryExpressionCache;
                ResolutionStateCache = resolutionStateCache;
            }

            private Registry WithServices(HashTree<Type, object> services)
            {
                return services == Services ? this :
                    new Registry(services, Decorators, Wrappers,
                        DefaultFactoryDelegateCache.Copy(), KeyedFactoryDelegateCache.Copy(),
                        FactoryExpressionCache.Copy(), ResolutionStateCache.Copy());
            }

            private Registry WithDecorators(HashTree<Type, Factory[]> decorators)
            {
                return decorators == Decorators ? this :
                    new Registry(Services, decorators, Wrappers,
                        DefaultFactoryDelegateCache.Copy(), KeyedFactoryDelegateCache.Copy(),
                        FactoryExpressionCache.Copy(), ResolutionStateCache.Copy());
            }

            private Registry WithWrappers(HashTree<Type, Factory> wrappers)
            {
                return wrappers == Wrappers ? this :
                    new Registry(Services, Decorators, wrappers,
                        DefaultFactoryDelegateCache.Copy(), KeyedFactoryDelegateCache.Copy(),
                        FactoryExpressionCache.Copy(), ResolutionStateCache.Copy());
            }

            public IEnumerable<ServiceTypeKeyFactory> GetServiceRegistrations()
            {
                foreach (var factoryEntry in Services.Enumerate())
                {
                    var serviceType = factoryEntry.Key;
                    var factory = factoryEntry.Value as Factory;
                    if (factory != null)
                        yield return new ServiceTypeKeyFactory { ServiceType = serviceType, Factory = factory };
                    else
                    {
                        var factories = ((FactoriesEntry)factoryEntry.Value).Factories;
                        foreach (var f in factories.Enumerate())
                            yield return new ServiceTypeKeyFactory { ServiceType = serviceType, OptionalServiceKey = f.Key, Factory = f.Value };
                    }
                }
            }

            public Registry Register(Factory factory, Type serviceType, IfAlreadyRegistered ifAlreadyRegistered, object serviceKey)
            {
                switch (factory.FactoryType)
                {
                    case FactoryType.Decorator:
                        return WithDecorators(Decorators.AddOrUpdate(serviceType, new[] { factory }, ArrayTools.Append));

                    case FactoryType.Wrapper:
                        return WithWrappers(Wrappers.AddOrUpdate(serviceType, factory));

                    default:
                        return WithService(factory, serviceType, serviceKey, ifAlreadyRegistered);
                }
            }

            public bool IsRegistered(Type serviceType, object serviceKey, FactoryType factoryType, Func<Factory, bool> condition)
            {
                serviceType = serviceType.ThrowIfNull();
                switch (factoryType)
                {
                    case FactoryType.Wrapper:
                        var wrapper = GetWrapperOrDefault(serviceType);
                        return wrapper != null && (condition == null || condition(wrapper));

                    case FactoryType.Decorator:
                        var decorators = Decorators.GetValueOrDefault(serviceType);
                        return decorators != null && decorators.Length != 0
                               && (condition == null || decorators.Any(condition));

                    default:
                        var entry = Services.GetValueOrDefault(serviceType);
                        if (entry == null)
                            return false;

                        var factory = entry as Factory;
                        if (factory != null)
                            return (serviceKey == null || DefaultKey.Value.Equals(serviceKey))
                                   && (condition == null || condition(factory));

                        var factories = ((FactoriesEntry)entry).Factories;
                        if (serviceKey == null)
                            return condition == null || factories.Enumerate().Any(f => condition(f.Value));

                        factory = factories.GetValueOrDefault(serviceKey);
                        return factory != null && (condition == null || condition(factory));
                }
            }

            public Factory GetWrapperOrDefault(Type serviceType)
            {
                Factory factory = null;
                if (serviceType.IsGeneric())
                    factory = Wrappers.GetValueOrDefault(serviceType.GetGenericDefinitionOrNull());
                if (factory == null && !serviceType.IsGenericDefinition())
                    factory = Wrappers.GetValueOrDefault(serviceType);
                return factory;
            }

            private Registry WithService(Factory factory, Type serviceType, object serviceKey, IfAlreadyRegistered ifAlreadyRegistered)
            {
                Factory replacedFactory = null;
                HashTree<Type, object> services;
                if (serviceKey == null)
                {
                    services = Services.AddOrUpdate(serviceType, factory, (oldEntry, newFactory) =>
                    {
                        if (oldEntry == null)
                            return newFactory;

                        var oldFactories = oldEntry as FactoriesEntry;
                        if (oldFactories != null && oldFactories.LastDefaultKey == null) // no default registered yet
                            return new FactoriesEntry(DefaultKey.Value,
                                oldFactories.Factories.AddOrUpdate(DefaultKey.Value, (Factory)newFactory));

                        var oldFactory = oldFactories == null ? (Factory)oldEntry : null;
                        switch (ifAlreadyRegistered)
                        {
                            case IfAlreadyRegistered.Throw:
                                oldFactory = oldFactory ?? oldFactories.Factories.GetValueOrDefault(oldFactories.LastDefaultKey);
                                return Throw.For<object>(Error.UNABLE_TO_REGISTER_DUPLICATE_DEFAULT, serviceType, oldFactory);

                            case IfAlreadyRegistered.Keep:
                                return oldEntry;

                            case IfAlreadyRegistered.Replace:
                                replacedFactory = oldFactory ?? oldFactories.Factories.GetValueOrDefault(oldFactories.LastDefaultKey);
                                if (replacedFactory != null)
                                    ((Factory)newFactory).FactoryID = replacedFactory.FactoryID;
                                return oldFactories == null
                                    ? newFactory
                                    : new FactoriesEntry(oldFactories.LastDefaultKey,
                                        oldFactories.Factories.AddOrUpdate(oldFactories.LastDefaultKey, (Factory)newFactory));

                            default:
                                if (oldFactories == null)
                                    return new FactoriesEntry(DefaultKey.Value.Next(),
                                        HashTree<object, Factory>.Empty
                                            .AddOrUpdate(DefaultKey.Value, (Factory)oldEntry)
                                            .AddOrUpdate(DefaultKey.Value.Next(), (Factory)newFactory));

                                var nextKey = oldFactories.LastDefaultKey.Next();
                                return new FactoriesEntry(nextKey,
                                    oldFactories.Factories.AddOrUpdate(nextKey, (Factory)newFactory));
                        }
                    });
                }
                else
                {
                    var factories = new FactoriesEntry(null, HashTree<object, Factory>.Empty.AddOrUpdate(serviceKey, factory));
                    services = Services.AddOrUpdate(serviceType, factories, (oldEntry, newEntry) =>
                    {
                        if (oldEntry == null)
                            return newEntry;

                        if (oldEntry is Factory) // if registered is default, just add it to new entry
                            return new FactoriesEntry(DefaultKey.Value,
                                ((FactoriesEntry)newEntry).Factories.AddOrUpdate(DefaultKey.Value, (Factory)oldEntry));

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
                                        newFactory.FactoryID = replacedFactory.FactoryID;
                                        return newFactory;

                                    //case IfAlreadyRegistered.Throw:
                                    //case IfAlreadyRegistered.AppendDefault:
                                    default:
                                        return Throw.For<Factory>(Error.UNABLE_TO_REGISTER_DUPLICATE_KEY,
                                            serviceType, serviceKey, oldFactory);
                                }
                            }));
                    });
                }

                var registry = WithServices(services);

                return registry == this ? this
                    : replacedFactory == null ? registry
                        : RemoveFactoryCache(registry, replacedFactory, serviceType, serviceKey);
            }

            public Registry Unregister(FactoryType factoryType, Type serviceType, object serviceKey, Func<Factory, bool> condition)
            {
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

                        return removedWrapper == null ? this
                            : RemoveFactoryCache(registry, removedWrapper, serviceType);

                    case FactoryType.Decorator:
                        Factory[] removedDecorators = null;
                        registry = WithDecorators(Decorators.Update(serviceType, null, (factories, _null) =>
                        {
                            var remaining = condition == null ? null : factories.Where(f => !condition(f)).ToArray();
                            removedDecorators = remaining == null || remaining.Length == 0 ? factories : factories.Except(remaining).ToArray();
                            return remaining;
                        }));

                        if (removedDecorators.IsNullOrEmpty())
                            return this;

                        foreach (var removedDecorator in removedDecorators)
                            registry = RemoveFactoryCache(registry, removedDecorator, serviceType);
                        return registry;

                    default:
                        return UnregisterServiceFactory(serviceType, serviceKey, condition);
                }
            }

            private Registry UnregisterServiceFactory(Type serviceType, object serviceKey = null, Func<Factory, bool> condition = null)
            {
                object removed = null; // Factory or FactoriesEntry or Factory[]

                HashTree<Type, object> services;

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
                        var remainingFactories = HashTree<object, Factory>.Empty;
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
                                    : HashTree<object, Factory>.Empty;
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
                    return RemoveFactoryCache(registry, (Factory)removed, serviceType, serviceKey);

                var removedFactories = removed as Factory[]
                    ?? ((FactoriesEntry)removed).Factories.Enumerate().Select(f => f.Value).ToArray();

                foreach (var removedFactory in removedFactories)
                    registry = RemoveFactoryCache(registry, removedFactory, serviceType, serviceKey);

                return registry;
            }

            private static Registry RemoveFactoryCache(Registry registry, Factory factory, Type serviceType, object serviceKey = null)
            {
                registry.FactoryExpressionCache.Swap(_ => _.Update(factory.FactoryID, null));
                if (serviceKey == null)
                    registry.DefaultFactoryDelegateCache.Swap(_ => _.Update(serviceType, null));
                else
                    registry.KeyedFactoryDelegateCache.Swap(_ => _.Update(new KV<Type, object>(serviceType, serviceKey), null));

                if (factory.Provider != null)
                    foreach (var f in factory.Provider.ProvidedFactoriesServiceTypeKey)
                        registry = registry.Unregister(factory.FactoryType, f.Key, f.Value, null);

                return registry;
            }
        }

        private Container(Rules rules, Ref<Registry> registry, IScope singletonScope, IScopeContext scopeContext, 
            IScope openedScope = null, int disposed = 0)
        {
            Rules = rules;

            _registry = registry;
            _disposed = disposed;

            _singletonScope = singletonScope;
            _scopeContext = scopeContext;
            _openedScope = openedScope;

            _containerWeakRef = new ContainerWeakRef(this);
            _emptyRequest = Request.CreateEmpty(_containerWeakRef);
        }

        static IScopeContext GetDefaultScopeContext()
        {
            IScopeContext context = null;
            GetDefaultScopeContext(ref context);
            // ReSharper disable once ConstantNullCoalescingCondition
            return context ?? new ThreadScopeContext();
        }

        static partial void GetDefaultScopeContext(ref IScopeContext resultContext);

        #endregion
    }

    /// <summary>Extension methods for automating common use-cases.</summary>
    public static class ContainerTools
    {
        /// <summary>Adds rule to register unknown service when it is resolved.</summary>
        /// <param name="container">Container to add rule to.</param>
        /// <param name="implTypes">Provider of implementation types.</param>
        /// <param name="changeDefaultReuse">(optional) Delegate to change auto-detected (Singleton or Current) scope reuse to another reuse.</param>
        /// <returns>Container with new rule.</returns>
        /// <remarks>Types provider will be asked on each rule evaluation.</remarks>
        public static IContainer WithAutoFallbackResolution(this IContainer container, IEnumerable<Type> implTypes,
            Func<IReuse, Request, IReuse> changeDefaultReuse = null)
        {
            return container.ThrowIfNull().With(rules =>
                rules.WithUnknownServiceResolver(AutoRegisterUnknownServiceRule(implTypes, changeDefaultReuse)));
        }

        /// <summary>Adds rule to register unknown service when it is resolved.</summary>
        /// <param name="container">Container to add rule to.</param>
        /// <param name="implTypeAssemblies">Provides assembly with implementation types.</param>
        /// <param name="changeDefaultReuse">(optional) Delegate to change auto-detected (Singleton or Current) scope reuse to another reuse.</param>
        /// <returns>Container with new rule.</returns>
        /// <remarks>Implementation types will be requested from assemblies only once, in this method call.</remarks>
        public static IContainer WithAutoFallbackResolution(this IContainer container, IEnumerable<Assembly> implTypeAssemblies,
            Func<IReuse, Request, IReuse> changeDefaultReuse = null)
        {
            var types = implTypeAssemblies.ThrowIfNull().SelectMany(a => a.GetLoadedTypes()).ToArray();
            return container.WithAutoFallbackResolution(types);
        }

        /// <summary>Fallback rule to automatically register requested service with Reuse based on resolution source.</summary>
        /// <param name="implTypes">Assemblies to look for implementation types.</param>
        /// <param name="changeDefaultReuse">(optional) Delegate to change auto-detected (Singleton or Current) scope reuse to another reuse.</param>
        /// <returns>Rule.</returns>
        public static Rules.UnknownServiceResolver AutoRegisterUnknownServiceRule(IEnumerable<Type> implTypes, 
            Func<IReuse, Request, IReuse> changeDefaultReuse = null)
        {
            return request =>
            {
                var container = request.Container;
                var reuse = container.OpenedScope != null
                    ? Reuse.InCurrentNamedScope(container.OpenedScope.Name)
                    : Reuse.Singleton;
                if (changeDefaultReuse != null)
                    reuse = changeDefaultReuse(reuse, request);
                container.RegisterMany(implTypes, type => type.IsAssignableTo(request.ServiceType), reuse);
                var factory = container.GetServiceFactoryOrDefault(request);
                return factory;
            };
        }
    }

    /// <summary>Used to represent multiple default service keys. 
    /// Exposes <see cref="RegistrationOrder"/> to determine order of service added.</summary>
    public sealed class DefaultKey
    {
        /// <summary>Default value.</summary>
        public static readonly DefaultKey Value = new DefaultKey(0);

        /// <summary>Create new default key with specified registration order.</summary>
        /// <param name="registrationOrder"></param> <returns>New default key.</returns>
        public static DefaultKey Of(int registrationOrder)
        {
            if (registrationOrder < _keyPool.Length)
                return _keyPool[registrationOrder];

            var nextKey = new DefaultKey(registrationOrder);
            if (registrationOrder == _keyPool.Length)
                _keyPool = _keyPool.AppendOrUpdate(nextKey);
            return nextKey;
        }

        /// <summary>Returns next default key with increased <see cref="RegistrationOrder"/>.</summary>
        /// <returns>New key.</returns>
        public DefaultKey Next()
        {
            return Of(RegistrationOrder + 1);
        }

        /// <summary>Allows to determine service registration order.</summary>
        public readonly int RegistrationOrder;

        /// <summary>Compares keys based on registration order.</summary>
        /// <param name="key">Key to compare with.</param>
        /// <returns>True if keys have the same order.</returns>
        public override bool Equals(object key)
        {
            return key == null
                || key is DefaultKey && ((DefaultKey)key).RegistrationOrder == RegistrationOrder;
        }

        /// <summary>Returns registration order as hash.</summary> <returns>Hash code.</returns>
        public override int GetHashCode()
        {
            return RegistrationOrder;
        }

        /// <summary>Prints registration order to string.</summary> <returns>Printed string.</returns>
        public override string ToString()
        {
            return "DefaultKey.Of(" + RegistrationOrder + ")";
        }

        #region Implementation

        private static DefaultKey[] _keyPool = { Value };

        private DefaultKey(int registrationOrder)
        {
            RegistrationOrder = registrationOrder;
        }

        #endregion
    }

    /// <summary>Immutable array based on wide hash tree, where each node is sub-array with predefined size: 32 is by default.
    /// Array supports only append, no remove.</summary>
    public class AppendableArray
    {
        /// <summary>Node array size. When the item added to same node, array will be copied. 
        /// So if array is too big performance will degrade. Should be power of two: e.g. 2, 4, 8, 16, 32...</summary>
        public const int NODE_ARRAY_SIZE = 32;

        /// <summary>Empty/default value to start from.</summary>
        public static readonly AppendableArray Empty = new AppendableArray(0);

        /// <summary>Number of items in array.</summary>
        public readonly int Length;

        /// <summary>Appends value and returns new array.</summary>
        /// <param name="value">Value to append.</param> <returns>New array.</returns>
        public virtual AppendableArray Append(object value)
        {
            return Length < NODE_ARRAY_SIZE
                ? new AppendableArray(Length + 1, _items.AppendOrUpdate(value))
                : new AppendableArrayTree(Length, IntKeyTree.Empty.AddOrUpdate(0, _items)).Append(value);
        }

        /// <summary>Returns item stored at specified index. Method relies on underlying array for index range checking.</summary>
        /// <param name="index">Index to look for item.</param> <returns>Found item.</returns>
        /// <exception cref="ArgumentOutOfRangeException">from underlying node array.</exception>
        public virtual object Get(int index)
        {
            return _items[index];
        }

        /// <summary>Returns index of first equal value in array if found, or -1 otherwise.</summary>
        /// <param name="value">Value to look for.</param> <returns>Index of first equal value, or -1 otherwise.</returns>
        public virtual int IndexOf(object value)
        {
            if (_items == null || _items.Length == 0)
                return -1;

            for (var i = 0; i < _items.Length; ++i)
            {
                var item = _items[i];
                if (ReferenceEquals(item, value) || Equals(item, value))
                    return i;
            }
            return -1;
        }

        #region Implementation

        private readonly object[] _items;

        private AppendableArray(int length, object[] items = null)
        {
            Length = length;
            _items = items;
        }

        private sealed class AppendableArrayTree : AppendableArray
        {
            private const int NODE_ARRAY_BIT_MASK = NODE_ARRAY_SIZE - 1; // for length 32 will be 11111 binary.
            private const int NODE_ARRAY_BIT_COUNT = 5;                  // number of set bits in NODE_ARRAY_BIT_MASK.

            public override AppendableArray Append(object value)
            {
                var key = Length >> NODE_ARRAY_BIT_COUNT;
                var nodeItems = _tree.GetValueOrDefault(key) as object[];
                return new AppendableArrayTree(Length + 1, _tree.AddOrUpdate(key, nodeItems.AppendOrUpdate(value)));
            }

            public override object Get(int index)
            {
                return ((object[])_tree.GetValueOrDefault(index >> NODE_ARRAY_BIT_COUNT))[index & NODE_ARRAY_BIT_MASK];
            }

            public override int IndexOf(object value)
            {
                foreach (var node in _tree.Enumerate())
                {
                    var nodeItems = (object[])node.Value;
                    if (!nodeItems.IsNullOrEmpty())
                    {
                        for (var i = 0; i < nodeItems.Length; ++i)
                        {
                            var item = nodeItems[i];
                            if (ReferenceEquals(item, value) || Equals(item, value))
                                return node.Key << NODE_ARRAY_BIT_COUNT | i;
                        }
                    }
                }

                return -1;
            }

            public AppendableArrayTree(int length, IntKeyTree tree)
                : base(length)
            {
                _tree = tree;
            }

            private readonly IntKeyTree _tree;
        }

        #endregion
    }

    /// <summary>Returns reference to actual resolver implementation. 
    /// Minimizes <see cref="FactoryDelegate"/> dependency on container.</summary>
    public interface IResolverContextProvider
    {
        /// <summary>Provides access to resolver implementation.</summary>
        IResolverContext Resolver { get; }
    }

    /// <summary>Wraps <see cref="IContainer"/> WeakReference with more specialized exceptions on access to GCed or disposed container.</summary>
    public sealed class ContainerWeakRef : IResolverContextProvider
    {
        /// <summary>Provides access to resolver implementation.</summary>
        public IResolverContext Resolver
        {
            get { return GetTarget(); }
        }

        /// <summary>Retrieves container instance if it is not GCed or disposed</summary>
        public IContainer GetTarget()
        {
            var container = _weakref.Target as IContainer;
            return container
                .ThrowIfNull(Error.CONTAINER_IS_GARBAGE_COLLECTED)
                // ReSharper disable once PossibleNullReferenceException
                .ThrowIf(container.IsDisposed, Error.CONTAINER_IS_DISPOSED);
        }

        /// <summary>Creates weak reference wrapper over passed container object.</summary> <param name="container">Object to wrap.</param>
        public ContainerWeakRef(IContainer container) { _weakref = new WeakReference(container); }
        private readonly WeakReference _weakref;
    }

    /// <summary>The delegate type which is actually used to create service instance by container.
    /// Delegate instance required to be static with all information supplied by <paramref name="state"/> and <paramref name="scope"/>
    /// parameters. The requirement is due to enable compilation to DynamicMethod in DynamicAssembly, and also to simplify
    /// state management and minimizes memory leaks.</summary>
    /// <param name="state">All the state items available in resolution root.</param>
    /// <param name="r">Provides access to <see cref="IResolver"/> implementation to enable nested/dynamic resolve inside:
    /// registered delegate factory, <see cref="Lazy{T}"/>, and <see cref="LazyEnumerable{TService}"/>.</param>
    /// <param name="scope">Resolution root scope: initially passed value will be null, but then the actual will be created on demand.</param>
    /// <returns>Created service object.</returns>
    public delegate object FactoryDelegate(AppendableArray state, IResolverContextProvider r, IScope scope);

    /// <summary>Handles default conversation of expression into <see cref="FactoryDelegate"/>.</summary>
    public static partial class FactoryCompiler
    {
        /// <summary>Wraps service creation expression (body) into <see cref="FactoryDelegate"/> and returns result lambda expression.</summary>
        /// <param name="expression">Service expression (body) to wrap.</param> <returns>Created lambda expression.</returns>
        public static Expression<FactoryDelegate> WrapInFactoryExpression(this Expression expression)
        {
            // Removing not required Convert from expression root, because CompiledFactory result still be converted at the end.
            if (expression.NodeType == ExpressionType.Convert)
                expression = ((UnaryExpression)expression).Operand;
            if (expression.Type.IsValueType())
                expression = Expression.Convert(expression, typeof(object));
            return Expression.Lambda<FactoryDelegate>(expression,
                Container.StateParamExpr, Container.ResolverContextProviderParamExpr, Container.ResolutionScopeParamExpr);
        }

        /// <summary>First wraps the input service creation expression into lambda expression and
        /// then compiles lambda expression to actual <see cref="FactoryDelegate"/> used for service resolution.
        /// By default it is using Expression.Compile but if corresponding rule specified (available on .Net 4.0 and higher),
        /// it will compile to DymanicMethod/Assembly.</summary>
        /// <param name="expression">Service expression (body) to wrap.</param>
        /// <param name="rules">Specify requirement to compile expression to DynamicAssembly (available on .Net 4.0 and higher).</param>
        /// <returns>Compiled factory delegate to use for service resolution.</returns>
        public static FactoryDelegate CompileToDelegate(this Expression expression, Rules rules)
        {
            var factoryExpression = expression.WrapInFactoryExpression();
            FactoryDelegate factoryDelegate = null;
            CompileToMethod(factoryExpression, rules, ref factoryDelegate);
            // ReSharper disable ConstantNullCoalescingCondition
            factoryDelegate = factoryDelegate ?? factoryExpression.Compile();
            // ReSharper restore ConstantNullCoalescingCondition

            //System.Runtime.CompilerServices.RuntimeHelpers.PrepareMethod(factoryDelegate.Method.MethodHandle);
            return factoryDelegate;
        }

        // Partial method definition to be implemented in .NET40 version of Container.
        // It is optional and fine to be not implemented.
        static partial void CompileToMethod(Expression<FactoryDelegate> factoryExpression, Rules rules, ref FactoryDelegate result);
    }

    /// <summary>Adds to Container support for:
    /// <list type="bullet">
    /// <item>Open-generic services</item>
    /// <item>Service generics wrappers and arrays using <see cref="Rules.UnknownServiceResolvers"/> extension point.
    /// Supported wrappers include: Func of <see cref="FuncTypes"/>, Lazy, Many, IEnumerable, arrays, Meta, KeyValuePair, DebugExpression.
    /// All wrapper factories are added into collection <see cref="Wrappers"/> and searched by <see cref="ResolveWrappers"/>
    /// unregistered resolution rule.</item>
    /// </list></summary>
    public static class WrappersSupport
    {
        /// <summary>Supported Func types up to 4 input parameters.</summary>
        public static readonly Type[] FuncTypes = { typeof(Func<>), typeof(Func<,>), typeof(Func<,,>), typeof(Func<,,,>), typeof(Func<,,,,>) };

        /// <summary>Registered wrappers by their concrete or generic definition service type.</summary>
        public static readonly HashTree<Type, Factory> Wrappers;

        static WrappersSupport()
        {
            Wrappers = HashTree<Type, Factory>.Empty;

            // Register array and its collection/list interfaces.
            var arrayExpr = new ExpressionFactory(GetArrayExpression, setup: Setup.Wrapper);
            var arrayInterfaces = typeof(object[]).GetImplementedInterfaces()
                .Where(t => t.IsGeneric()).Select(t => t.GetGenericDefinitionOrNull());
            foreach (var arrayInterface in arrayInterfaces)
                Wrappers = Wrappers.AddOrUpdate(arrayInterface, arrayExpr);

            Wrappers = Wrappers.AddOrUpdate(typeof(LazyEnumerable<>),
                new ExpressionFactory(GetLazyEnumerableExpressionOrDefault, setup: Setup.Wrapper));

            Wrappers = Wrappers.AddOrUpdate(typeof(Lazy<>),
                new ExpressionFactory(GetLazyExpressionOrDefault, setup: Setup.Wrapper));

            Wrappers = Wrappers.AddOrUpdate(typeof(KeyValuePair<,>),
                new ExpressionFactory(GetKeyValuePairExpressionOrDefault,
                    setup: Setup.WrapperWith(t => t.GetGenericParamsAndArgs()[1])));

            Wrappers = Wrappers.AddOrUpdate(typeof(Meta<,>),
                new ExpressionFactory(GetMetaExpressionOrDefault,
                    setup: Setup.WrapperWith(t => t.GetGenericParamsAndArgs()[0])));

            Wrappers = Wrappers.AddOrUpdate(typeof(CaptureResolutionScope<>),
                new ExpressionFactory(GetCaptureResolutionScopeExpressionOrDefault, setup: Setup.Wrapper));

            Wrappers = Wrappers.AddOrUpdate(typeof(FactoryExpression<>),
                new ExpressionFactory(GetFactoryExpression, setup: Setup.Wrapper));

            Wrappers = Wrappers.AddOrUpdate(typeof(Func<>),
                new ExpressionFactory(GetFuncExpression, setup: Setup.Wrapper));

            for (var i = 0; i < FuncTypes.Length; i++)
                Wrappers = Wrappers.AddOrUpdate(FuncTypes[i],
                    new ExpressionFactory(GetFuncExpression,
                        setup: Setup.WrapperWith(t => t.GetGenericParamsAndArgs().Last())));

            // Reuse wrappers
            Wrappers = Wrappers
                .AddOrUpdate(typeof(ReuseHiddenDisposable),
                    new ExpressionFactory(GetReusedObjectWrapperExpressionOrDefault,
                        setup: Setup.WrapperWith(t => typeof(object), ReuseWrapperFactory.HiddenDisposable)))

                .AddOrUpdate(typeof(ReuseWeakReference),
                    new ExpressionFactory(GetReusedObjectWrapperExpressionOrDefault,
                        setup: Setup.WrapperWith(_ => typeof(object), ReuseWrapperFactory.WeakReference)))

                .AddOrUpdate(typeof(ReuseSwapable),
                    new ExpressionFactory(GetReusedObjectWrapperExpressionOrDefault,
                        setup: Setup.WrapperWith(t => typeof(object), ReuseWrapperFactory.Swapable)))

                .AddOrUpdate(typeof(ReuseRecyclable),
                    new ExpressionFactory(GetReusedObjectWrapperExpressionOrDefault,
                        setup: Setup.WrapperWith(t => typeof(object), ReuseWrapperFactory.Recyclable)));
        }

        /// <summary>Unregistered/fallback wrapper resolution rule.</summary>
        public static readonly Rules.UnknownServiceResolver ResolveWrappers = request =>
        {
            var serviceType = request.ServiceType;
            var itemType = serviceType.GetElementTypeOrNull();
            if (itemType != null)
                serviceType = typeof(IEnumerable<>).MakeGenericType(itemType);

            var factory = request.Container.GetWrapperFactoryOrDefault(serviceType);
            if (factory != null && factory.Provider != null)
                factory = factory.Provider.ProvideConcreteFactory(request);

            return factory;
        };

        /// <summary>Checks if request has parent with service type of Func with arguments. 
        /// Often required to check in lazy scenarios.</summary>
        /// <param name="request">Request too check.</param>
        /// <returns>True if has Func parent.</returns>
        public static bool IsNestedInFuncWithArgs(this Request request)
        {
            return !request.Parent.IsEmpty && request.Parent.Enumerate()
                .TakeWhile(r => r.ResolvedFactory.FactoryType == FactoryType.Wrapper)
                .Any(r => r.ServiceType.IsFuncWithArgs());
        }

        private static Expression GetArrayExpression(Request request)
        {
            var collectionType = request.ServiceType;
            var itemType = collectionType.GetElementTypeOrNull() ?? collectionType.GetGenericParamsAndArgs()[0];

            var container = request.Container;
            var requiredItemType = container.UnwrapServiceType(request.RequiredServiceType ?? itemType);

            var items = container.GetAllServiceFactories(requiredItemType);
            var itemsWithVariance = !requiredItemType.IsGeneric() ? null :
                // Check generic type with compatible variance, 
                // e.g. for IHandler<in E> - IHandler<A> is compatible with IHandler<B> if B : A.
                container.GetServiceRegistrations().Where(x => 
                    requiredItemType != x.ServiceType && x.ServiceType.IsClosedGeneric() &&
                    requiredItemType.GetGenericTypeDefinition() == x.ServiceType.GetGenericTypeDefinition() &&
                    x.ServiceType.IsAssignableTo(requiredItemType));

            // Composite pattern support: filter out composite root from available keys.
            var parent = request.ParentNonWrapper();
            if (!parent.IsEmpty && parent.ServiceType == requiredItemType)
            {
                var parentFactoryID = parent.ResolvedFactory.FactoryID;
                items = items.Where(x => x.Value.FactoryID != parentFactoryID);
                if (itemsWithVariance != null)
                    itemsWithVariance = itemsWithVariance.Where(x => x.Factory.FactoryID != parentFactoryID);
            }

            // Return collection of single matched item if key is specified.
            if (request.ServiceKey != null)
            {
                items = items.Where(x => request.ServiceKey.Equals(x.Key));
                if (itemsWithVariance != null)
                    itemsWithVariance = itemsWithVariance.Where(x => request.ServiceKey.Equals(x.OptionalServiceKey));
            }

            var allItems = items.Select(kv => new ServiceTypeKeyFactory { 
                ServiceType = requiredItemType, OptionalServiceKey = kv.Key, Factory = kv.Value });
            if (itemsWithVariance != null)
                allItems = allItems.Concat(itemsWithVariance);

            var itemArray = allItems.ToArray();
            List<Expression> itemExprList = null;
            if (itemArray.Length != 0)
            {
                itemExprList = new List<Expression>(itemArray.Length);
                for (var i = 0; i < itemArray.Length; i++)
                {
                    var item = itemArray[i];
                    var requiredServiceType = requiredItemType != item.ServiceType ? item.ServiceType : null;
                    var itemRequest = request.Push(itemType, item.OptionalServiceKey, IfUnresolved.ReturnDefault, requiredServiceType);
                    var itemFactory = container.ResolveFactory(itemRequest);
                    if (itemFactory != null)
                    {
                        var itemExpr = itemFactory.GetExpressionOrDefault(itemRequest);
                        if (itemExpr != null)
                            itemExprList.Add(itemExpr);
                    }
                }
            }

            return Expression.NewArrayInit(itemType.ThrowIfNull(), itemExprList ?? Enumerable.Empty<Expression>());
        }

        private static readonly MethodInfo _resolveManyMethod =
            typeof(IResolver).GetSingleDeclaredMethodOrNull("ResolveMany").ThrowIfNull();

        private static Expression GetLazyEnumerableExpressionOrDefault(Request request)
        {
            if (IsNestedInFuncWithArgs(request))
                return null;

            var wrapperType = request.ServiceType;
            var itemServiceType = wrapperType.GetGenericParamsAndArgs()[0];
            var itemRequiredServiceType = request.Container.UnwrapServiceType(request.RequiredServiceType ?? itemServiceType);

            // Composite pattern support: find composite parent key to exclude from result.
            object compositeParentKey = null;
            var parent = request.ParentNonWrapper();
            if (!parent.IsEmpty && parent.ServiceType == itemRequiredServiceType)
                compositeParentKey = parent.ServiceKey;

            var callResolveManyExpr = Expression.Call(Container.ResolverContextExpr, _resolveManyMethod,
                Expression.Constant(itemServiceType),
                request.Container.GetOrAddStateItemExpression(request.ServiceKey),
                Expression.Constant(itemRequiredServiceType),
                request.Container.GetOrAddStateItemExpression(compositeParentKey),
                Container.GetResolutionScopeExpression(request));

            if (itemServiceType != typeof(object)) // cast to object is not required cause Resolve already return IEnumerable<object>
                callResolveManyExpr = Expression.Call(typeof(Enumerable), "Cast", new[] { itemServiceType }, callResolveManyExpr);

            return Expression.New(wrapperType.GetSingleConstructorOrNull().ThrowIfNull(), callResolveManyExpr);
        }

        // Result: r => new Lazy<TService>(() => r.Resolver.Resolve<TService>(key, ifUnresolved, requiredType));
        private static Expression GetLazyExpressionOrDefault(Request request)
        {
            if (IsNestedInFuncWithArgs(request))
                return null;

            var wrapperType = request.ServiceType;
            var serviceType = wrapperType.GetGenericParamsAndArgs()[0];
            var serviceExpr = Resolver.CreateResolutionExpression(request, serviceType);
            var factoryExpr = Expression.Lambda(serviceExpr, null);
            var wrapperCtor = wrapperType.GetConstructorOrNull(args: typeof(Func<>).MakeGenericType(serviceType));
            return Expression.New(wrapperCtor, factoryExpr);
        }

        private static Expression GetFuncExpression(Request request)
        {
            var funcType = request.ServiceType;
            var funcArgs = funcType.GetGenericParamsAndArgs();
            var serviceType = funcArgs[funcArgs.Length - 1];

            ParameterExpression[] funcArgExprs = null;
            if (funcArgs.Length > 1)
            {
                request = request.WithFuncArgs(funcType);
                funcArgExprs = request.FuncArgs.Value;
            }

            var serviceRequest = request.Push(serviceType);
            var serviceFactory = request.Container.ResolveFactory(serviceRequest);
            var serviceExpr = serviceFactory == null ? null : serviceFactory.GetExpressionOrDefault(serviceRequest);
            return serviceExpr == null ? null : Expression.Lambda(funcType, serviceExpr, funcArgExprs);
        }

        private static Expression GetFactoryExpression(Request request)
        {
            var ctor = request.ServiceType.GetSingleConstructorOrNull().ThrowIfNull();
            var serviceType = request.ServiceType.GetGenericParamsAndArgs()[0];
            var serviceRequest = request.Push(serviceType);
            var factory = request.Container.ResolveFactory(serviceRequest);
            var expr = factory == null ? null : factory.GetExpressionOrDefault(serviceRequest);
            return expr == null ? null : Expression.New(ctor, request.Container.GetOrAddStateItemExpression(expr.WrapInFactoryExpression()));
        }

        private static Expression GetKeyValuePairExpressionOrDefault(Request request)
        {
            var typeArgs = request.ServiceType.GetGenericParamsAndArgs();
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

            var pairCtor = request.ServiceType.GetSingleConstructorOrNull().ThrowIfNull();
            var keyExpr = request.Container.GetOrAddStateItemExpression(serviceKey, serviceKeyType);
            var pairExpr = Expression.New(pairCtor, keyExpr, serviceExpr);
            return pairExpr;
        }

        /// <remarks>If service key is not specified in request then it will search for all
        /// registered factory with the same metadata type, despite keys.</remarks>
        private static Expression GetMetaExpressionOrDefault(Request request)
        {
            var typeArgs = request.ServiceType.GetGenericParamsAndArgs();
            var metadataType = typeArgs[1];
            var serviceType = typeArgs[0];

            var container = request.Container;
            var requiredServiceType = container.UnwrapServiceType(request.RequiredServiceType ?? serviceType);
            var serviceKey = request.ServiceKey;

            var result = container.GetAllServiceFactories(requiredServiceType)
                .FirstOrDefault(f => (serviceKey == null || f.Key.Equals(serviceKey))
                    && f.Value.Setup.Metadata != null && metadataType.IsTypeOf(f.Value.Setup.Metadata));

            if (result == null)
                return null;

            serviceKey = result.Key;

            var serviceRequest = request.Push(serviceType, serviceKey);
            var serviceFactory = container.ResolveFactory(serviceRequest);
            var serviceExpr = serviceFactory == null ? null : serviceFactory.GetExpressionOrDefault(serviceRequest);
            if (serviceExpr == null)
                return null;

            var metaCtor = request.ServiceType.GetSingleConstructorOrNull().ThrowIfNull();
            var metadataExpr = request.Container.GetOrAddStateItemExpression(result.Value.Setup.Metadata, metadataType);
            var metaExpr = Expression.New(metaCtor, serviceExpr, metadataExpr);
            return metaExpr;
        }

        private static Expression GetCaptureResolutionScopeExpressionOrDefault(Request request)
        {
            var wrapperType = request.ServiceType;
            var wrapperCtor = wrapperType.GetSingleConstructorOrNull();

            var serviceType = wrapperType.GetGenericParamsAndArgs()[0];
            var serviceRequest = request.Push(serviceType, request.ServiceKey);
            var serviceFactory = request.Container.ResolveFactory(serviceRequest);
            var serviceExpr = serviceFactory == null ? null : serviceFactory.GetExpressionOrDefault(serviceRequest);
            return serviceExpr == null ? null :
                Expression.New(wrapperCtor, serviceExpr, Container.GetResolutionScopeExpression(request));
        }

        private static Expression GetReusedObjectWrapperExpressionOrDefault(Request request)
        {
            var wrapperType = request.ServiceType;
            var serviceType = request.Container.UnwrapServiceType(request.RequiredServiceType ?? wrapperType);
            var serviceRequest = request.Push(serviceType);
            var serviceFactory = request.Container.ResolveFactory(serviceRequest);
            if (serviceFactory == null)
                return null;

            var reuse = request.Container.Rules.ReuseMapping == null 
                ? serviceFactory.Reuse
                : request.Container.Rules.ReuseMapping(serviceFactory.Reuse, serviceRequest);

            if (reuse != null && serviceFactory.Setup.ReuseWrappers.IndexOf(wrapperType) != -1)
                return serviceFactory.GetExpressionOrDefault(serviceRequest, wrapperType);
            Throw.If(request.IfUnresolved == IfUnresolved.Throw,
                Error.UNABLE_TO_RESOLVE_REUSE_WRAPPER, wrapperType, serviceRequest);
            return null;
        }

        /// <summary>Returns true if type is supported <see cref="FuncTypes"/>, and false otherwise.</summary>
        /// <param name="type">Type to check.</param><returns>True for func type, false otherwise.</returns>
        public static bool IsFunc(this Type type)
        {
            var genericDefinition = type.GetGenericDefinitionOrNull();
            return genericDefinition != null && FuncTypes.Contains(genericDefinition);
        }

        /// <summary>Returns true if type is func with 1 or more input arguments.</summary>
        /// <param name="type">Type to check.</param><returns>True for func type, false otherwise.</returns>
        public static bool IsFuncWithArgs(this Type type)
        {
            return type.IsFunc() && type.GetGenericDefinitionOrNull() != typeof(Func<>);
        }
    }

    /// <summary> Defines resolution/registration rules associated with Container instance. They may be different for different containers.</summary>
    public sealed partial class Rules
    {
        /// <summary>No rules specified.</summary>
        /// <remarks>Rules <see cref="UnknownServiceResolvers"/> are empty too.</remarks>
        public static readonly Rules Empty = new Rules();

        /// <summary>Default rules with support for generic wrappers: IEnumerable, Many, arrays, Func, Lazy, Meta, KeyValuePair, DebugExpression.
        /// Check <see cref="WrappersSupport.ResolveWrappers"/> for details.</summary>
        public static readonly Rules Default = Empty
            .WithUnknownServiceResolver(WrappersSupport.ResolveWrappers)
            .WithItemToExpressionConverter(PrimitivesToExpressionConverter);

        /// <summary>Shorthand to <see cref="Made.FactoryMethod"/></summary>
        public FactoryMethodSelector FactoryMethod { get { return _made.FactoryMethod; } }

        /// <summary>Shorthand to <see cref="Made.Parameters"/></summary>
        public ParameterSelector Parameters { get { return _made.Parameters; } }

        /// <summary>Shorthand to <see cref="Made.PropertiesAndFields"/></summary>
        public PropertiesAndFieldsSelector PropertiesAndFields { get { return _made.PropertiesAndFields; } }

        /// <summary>Returns new instance of the rules with specified <see cref="Made"/>.</summary>
        /// <returns>New rules with specified <see cref="Made"/>.</returns>
        public Rules With(
            FactoryMethodSelector factoryMethod = null,
            ParameterSelector parameters = null,
            PropertiesAndFieldsSelector propertiesAndFields = null)
        {
            var newRules = (Rules)MemberwiseClone();
            newRules._made = Made.Of(
                factoryMethod ?? newRules._made.FactoryMethod,
                parameters ?? newRules._made.Parameters,
                propertiesAndFields ?? newRules._made.PropertiesAndFields);
            return newRules;
        }

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
        /// <param name="rule">Selectors to set, could be null to use default approach.</param> <returns>New rules.</returns>
        public Rules WithFactorySelector(FactorySelectorRule rule)
        {
            var newRules = (Rules)MemberwiseClone();
            newRules.FactorySelector = rule;
            return newRules;
        }

        //we are watching you...public static
        /// <summary>Say to prefer specified service key (if found) over default key.
        /// Help to override default registrations in Open Scope scenarios: I may register service with key and resolve it as default in current scope.</summary>
        /// <param name="serviceKey">Service key to look for instead default.</param>
        /// <returns>Found factory or null.</returns>
        public static FactorySelectorRule PreferKeyOverDefault(object serviceKey)
        {
            return (request, factories) => request.ServiceKey != null
                // if service key is not default, then look for it
                ? factories.FirstOrDefault(f => f.Key.Equals(request.ServiceKey)).Value
                // otherwise look for specified service key, and if no found look for default.
                : factories.FirstOrDefault(f => f.Key.Equals(serviceKey)).Value
                ?? factories.FirstOrDefault(f => f.Key.Equals(null)).Value;
        }

        /// <summary>Defines delegate to return factory for request not resolved by registered factories or prior rules.
        /// Applied in specified array order until return not null <see cref="Factory"/>.</summary> 
        /// <param name="request">Request to return factory for</param> <returns>Factory to resolve request, or null if unable to resolve.</returns>
        public delegate Factory UnknownServiceResolver(Request request);

        /// <summary>Gets rules for resolving not-registered services. Null by default.</summary>
        public UnknownServiceResolver[] UnknownServiceResolvers { get; private set; }

        /// <summary>Appends resolver to current unknown service resolvers.</summary>
        /// <param name="rule">Rule to append.</param> <returns>New Rules.</returns>
        public Rules WithUnknownServiceResolver(UnknownServiceResolver rule)
        {
            var newRules = (Rules)MemberwiseClone();
            newRules.UnknownServiceResolvers = newRules.UnknownServiceResolvers.AppendOrUpdate(rule);
            return newRules;
        }

        /// <summary>Removes specified resolver from unknown service resolvers, and returns new Rules.
        /// If no resolver was found then <see cref="UnknownServiceResolvers"/> will stay the same instance, 
        /// so it could be check for remove success or fail.</summary>
        /// <param name="rule">Rule tor remove.</param> <returns>New rules.</returns>
        public Rules WithoutUnknownServiceResolver(UnknownServiceResolver rule)
        {
            var newRules = (Rules)MemberwiseClone();
            newRules.UnknownServiceResolvers = newRules.UnknownServiceResolvers.Remove(rule);
            return newRules;
        }

        /// <summary>Turns on/off exception throwing when dependency has shorter reuse lifespan than its parent.</summary>
        public bool ThrowIfDependencyHasShorterReuseLifespan { get; private set; }

        /// <summary>Returns new rules with <see cref="ThrowIfDependencyHasShorterReuseLifespan"/> set to specified value.</summary>
        /// <returns>New rules with new setting value.</returns>
        public Rules WithoutThrowIfDependencyHasShorterReuseLifespan()
        {
            var newRules = (Rules)MemberwiseClone();
            newRules.ThrowIfDependencyHasShorterReuseLifespan = false;
            return newRules;
        }

        /// <summary>Defines mapping from registered reuse to what will be actually used.</summary>
        /// <param name="reuse">Service registered reuse</param> <param name="request">Context.</param> <returns>Mapped result reuse to use.</returns>
        public delegate IReuse ReuseMappingRule(IReuse reuse, Request request);

        /// <summary>Gets rule to retrieve actual reuse from registered one. May be null, so the registered reuse will be used.
        /// Could be used to specify different reuse container wide, for instance <see cref="Reuse.Singleton"/> instead of <see cref="Reuse.Transient"/>.</summary>
        public ReuseMappingRule ReuseMapping { get; private set; }

        /// <summary>Sets the <see cref="ReuseMapping"/> rule.</summary> <param name="rule">Rule to set, may be null.</param> <returns>New rules.</returns>
        public Rules WithReuseMapping(ReuseMappingRule rule)
        {
            var newRules = (Rules)MemberwiseClone();
            newRules.ReuseMapping = rule;
            return newRules;
        }

        /// <summary>Allow to instantiate singletons during resolution (but not inside of Func). Instantiated singletons
        /// will be copied to <see cref="IContainer.ResolutionStateCache"/> for faster access.</summary>
        public bool SingletonOptimization { get; private set; }

        /// <summary>Disables <see cref="SingletonOptimization"/></summary>
        /// <returns>New rules with singleton optimization turned off.</returns>
        public Rules WithoutSingletonOptimization()
        {
            var newRules = (Rules)MemberwiseClone();
            newRules.SingletonOptimization = false;
            return newRules;
        }

        /// <summary>Given item object and its type should return item "pure" expression presentation, 
        /// without side-effects or external dependencies. 
        /// e.g. for string "blah" <code lang="cs"><![CDATA[]]>Expression.Constant("blah", typeof(string))</code>.
        /// If unable to convert should return null.</summary>
        /// <param name="item">Item object. Item is not null.</param> 
        /// <param name="itemType">Item type. Item type is not null.</param>
        /// <returns>Expression or null.</returns>
        public delegate Expression ItemToExpressionConverterRule(object item, Type itemType);

        /// <summary>Mapping between Type and its ToExpression converter delegate.</summary>
        public ItemToExpressionConverterRule ItemToExpressionConverter { get; private set; }

        /// <summary>Overrides previous rule. You may return null from new rule to fallback to old one.</summary>
        /// <param name="itemToExpressionOrDefault">Converts item to expression or returns null to fallback to old rule.</param>
        /// <returns>New rules</returns>
        public Rules WithItemToExpressionConverter(ItemToExpressionConverterRule itemToExpressionOrDefault)
        {
            var newRules = (Rules)MemberwiseClone();
            var currentRule = newRules.ItemToExpressionConverter;
            newRules.ItemToExpressionConverter = currentRule == null 
                ? itemToExpressionOrDefault.ThrowIfNull()
                : (item, itemType) => itemToExpressionOrDefault(item, itemType) ?? currentRule(item, itemType);
            return newRules;
        }

        /// <summary>Flag acting in implicit <see cref="Setup.Condition"/> for service registered with not null <see cref="IReuse"/>.
        /// Condition skips resolution if no matching scope found.</summary>
        public bool ImplicitSetupConditionToCheckReuseMatchingScope { get; private set; }

        /// <summary>Removes <see cref="ImplicitSetupConditionToCheckReuseMatchingScope"/></summary>
        /// <returns>New rules.</returns>
        public Rules WithoutImplicitSetupConditionToCheckReuseMatchingScope()
        {
            var newRules = (Rules)MemberwiseClone();
            newRules.ImplicitSetupConditionToCheckReuseMatchingScope = false;
            return newRules;
        }

        #region Implementation

        private Made _made;
#pragma warning disable 169
        private bool _factoryDelegateCompilationToDynamicAssembly; // NOTE: used by .NET 4 and higher versions.
#pragma warning restore 169

        private Rules()
        {
            _made = Made.Default;
            ThrowIfDependencyHasShorterReuseLifespan = true;
            ImplicitSetupConditionToCheckReuseMatchingScope = true;
            SingletonOptimization = true;
        }

        private static Expression PrimitivesToExpressionConverter(object item, Type itemType)
        {
            return itemType == typeof(DefaultKey)
                    ? (Expression)Expression.Call(itemType, "Of", null,
                        Expression.Constant(((DefaultKey)item).RegistrationOrder))
                : (itemType.IsPrimitive() || itemType == typeof(Type)
                    ? Expression.Constant(item, itemType)
                    : null);
        }

        #endregion
    }

    /// <summary>Wraps constructor or factory method optionally with factory instance to create service.</summary>
    public sealed class FactoryMethod
    {
        /// <summary>Constructor or method to use for service creation.</summary>
        public readonly MemberInfo ConstructorOrMethodOrMember;

        /// <summary>Factory info to resolve if factory method is instance member.</summary>
        public readonly ServiceInfo FactoryInfo;

        /// <summary>Wraps method and factory instance.</summary>
        /// <param name="ctorOrMethodOrMember">Constructor, static or instance method, property or field.</param>
        /// <param name="factoryInfo">Factory info to resolve in case of instance <paramref name="ctorOrMethodOrMember"/>.</param>
        /// <returns>New factory method wrapper.</returns>
        public static FactoryMethod Of(MemberInfo ctorOrMethodOrMember, ServiceInfo factoryInfo = null)
        {
            return new FactoryMethod(ctorOrMethodOrMember.ThrowIfNull(), factoryInfo);
        }

        /// <summary>Converts method to selector when selector parameter is not required.</summary>
        /// <param name="method">Method to convert.</param> <returns>Result selector.</returns>
        public static implicit operator FactoryMethodSelector(FactoryMethod method)
        {
            return _ => method;
        }

        /// <summary>Pretty prints wrapped method.</summary> <returns>Printed string.</returns>
        public override string ToString()
        {
            return new StringBuilder().Print(ConstructorOrMethodOrMember.DeclaringType)
                .Append("::").Append(ConstructorOrMethodOrMember).ToString();
        }

        /// <summary>Searches for constructor with all resolvable parameters or throws <see cref="ContainerException"/> if not found.
        /// Works both for resolving as service and as Func&lt;TArgs..., TService&gt;.</summary>
        public static FactoryMethodSelector ConstructorWithResolvableArguments = request =>
        {
            var implementationType = request.ImplementationType.ThrowIfNull();
            var ctors = implementationType.GetAllConstructors().ToArrayOrSelf();
            if (ctors.Length == 0)
                return null; // Delegate handling of constructor absence to caller code.
            if (ctors.Length == 1)
                return Of(ctors[0]);

            var ctorsWithMoreParamsFirst = ctors
                .Select(c => new { Ctor = c, Params = c.GetParameters() })
                .OrderByDescending(x => x.Params.Length);

            var factory = (request.ResolvedFactory as ReflectionFactory).ThrowIfNull();
            var parameterSelector = request.Container.Rules.Parameters.And(factory.Made.Parameters)(request);

            if (request.IsNestedInFuncWithArgs())
            {
                // For Func with arguments, match constructor should contain all input arguments and the rest should be resolvable.
                var funcType = request.Parent.ServiceType;
                var funcArgs = funcType.GetGenericParamsAndArgs();
                var inputArgCount = funcArgs.Length - 1;

                var matchedCtor = ctorsWithMoreParamsFirst
                    .Where(x => x.Params.Length >= inputArgCount)
                    .FirstOrDefault(x =>
                    {
                        var matchedIndecesMask = 0;
                        return x.Params.Except(
                            x.Params.Where(p =>
                            {
                                var inputArgIndex = funcArgs.IndexOf(p.ParameterType);
                                if (inputArgIndex == -1 || inputArgIndex == inputArgCount ||
                                    (matchedIndecesMask & inputArgIndex << 1) != 0)
                                    // input argument was already matched by another parameter
                                    return false;
                                matchedIndecesMask |= inputArgIndex << 1;
                                return true;
                            })).All(p => ResolveParameter(p, parameterSelector, request) != null);
                    });

                var ctor = matchedCtor.ThrowIfNull(Error.UNABLE_TO_FIND_MATCHING_CTOR_FOR_FUNC_WITH_ARGS, funcType, request).Ctor;
                return Of(ctor);
            }
            else
            {
                var matchedCtor = ctorsWithMoreParamsFirst.FirstOrDefault(x =>
                    x.Params.All(p => ResolveParameter(p, parameterSelector, request) != null));
                var ctor = matchedCtor.ThrowIfNull(Error.UNABLE_TO_FIND_CTOR_WITH_ALL_RESOLVABLE_ARGS, request).Ctor;
                return Of(ctor);
            }
        };

        private static Expression ResolveParameter(ParameterInfo parameter, 
            Func<ParameterInfo, ParameterServiceInfo> parameterSelector, Request request)
        {
            var parameterServiceInfo = parameterSelector(parameter) ?? ParameterServiceInfo.Of(parameter);
            var parameterRequest = request.Push(parameterServiceInfo.WithDetails(ServiceDetails.IfUnresolvedReturnDefault, request));
            var parameterFactory = request.Container.ResolveFactory(parameterRequest);
            return parameterFactory == null ? null : parameterFactory.GetExpressionOrDefault(parameterRequest);
        }

        private FactoryMethod(MemberInfo constructorOrMethodOrMember, ServiceInfo factoryInfo = null)
        {
            ConstructorOrMethodOrMember = constructorOrMethodOrMember;
            FactoryInfo = factoryInfo;
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

        /// <summary>Return type of strongly-typed expression.</summary>
        public Type ExpressionReturnType { get; private set; }

        /// <summary>Specifies how constructor parameters should be resolved: 
        /// parameter service key and type, throw or return default value if parameter is unresolved.</summary>
        public ParameterSelector Parameters { get; private set; }

        /// <summary>Specifies what <see cref="ServiceInfo"/> should be used when resolving property or field.</summary>
        public PropertiesAndFieldsSelector PropertiesAndFields { get; private set; }

        /// <summary>Container will use some sensible defaults for service creation.</summary>
        public static readonly Made Default = new Made();

        /// <summary>Creates rules with only <see cref="FactoryMethod"/> specified.</summary>
        /// <param name="factoryMethod">To use.</param> <returns>New rules.</returns>
        public static implicit operator Made(FactoryMethodSelector factoryMethod)
        {
            return Of(factoryMethod);
        }

        /// <summary>Creates rules with only <see cref="FactoryMethod"/> specified.</summary>
        /// <param name="factoryMethod">To return from <see cref="FactoryMethod"/>.</param> <returns>New rules.</returns>
        public static implicit operator Made(FactoryMethod factoryMethod)
        {
            return Of(_ => factoryMethod);
        }

        /// <summary>Creates rules with only <see cref="FactoryMethod"/> specified.</summary>
        /// <param name="factoryMethod">To create <see cref="DryIoc.FactoryMethod"/> and return it from <see cref="FactoryMethod"/>.</param> 
        /// <returns>New rules.</returns>
        public static implicit operator Made(MethodInfo factoryMethod)
        {
            return Of(_ => DryIoc.FactoryMethod.Of(factoryMethod));
        }

        /// <summary>Creates rules with only <see cref="Parameters"/> specified.</summary>
        /// <param name="parameters">To use.</param> <returns>New rules.</returns>
        public static implicit operator Made(ParameterSelector parameters)
        {
            return Of(parameters: parameters);
        }

        /// <summary>Creates rules with only <see cref="PropertiesAndFields"/> specified.</summary>
        /// <param name="propertiesAndFields">To use.</param> <returns>New rules.</returns>
        public static implicit operator Made(PropertiesAndFieldsSelector propertiesAndFields)
        {
            return Of(propertiesAndFields: propertiesAndFields);
        }

        /// <summary>Specifies injections rules for Constructor, Parameters, Properties and Fields. If no rules specified returns <see cref="Default"/> rules.</summary>
        /// <param name="factoryMethod">(optional)</param> <param name="parameters">(optional)</param> <param name="propertiesAndFields">(optional)</param>
        /// <returns>New injection rules or <see cref="Default"/>.</returns>
        public static Made Of(FactoryMethodSelector factoryMethod = null, ParameterSelector parameters = null, 
            PropertiesAndFieldsSelector propertiesAndFields = null)
        {
            return factoryMethod == null && parameters == null && propertiesAndFields == null ? Default
                : new Made(null/*ExpressionReturnType*/, factoryMethod, parameters, propertiesAndFields);
        }

        /// <summary>Defines how to select constructor from implementation type.</summary>
        /// <param name="getConstructor">Delegate taking implementation type as input and returning selected constructor info.</param>
        /// <param name="parameters">(optional)</param> <param name="propertiesAndFields">(optional)</param>
        /// <returns>New instance of <see cref="Made"/> with <see cref="FactoryMethod"/> set to specified delegate.</returns>
        public static Made Of(Func<Type, ConstructorInfo> getConstructor, ParameterSelector parameters = null, 
            PropertiesAndFieldsSelector propertiesAndFields = null)
        {
            return Of(r => DryIoc.FactoryMethod.Of(getConstructor(r.ImplementationType)), parameters, propertiesAndFields);
        }

        /// <summary>Defines factory method using expression of constructor call (with properties), or static method call.</summary>
        /// <typeparam name="T">Type with constructor or static method.</typeparam>
        /// <param name="ctorOrMethodOrMethodExpr">Expression tree with call to constructor with properties: 
        /// <code lang="cs"><![CDATA[() => new Car(Arg.Of<IEngine>()) { Color = Arg.Of<Color>("CarColor") }]]></code>
        /// or static method call <code lang="cs"><![CDATA[() => Car.Create(Arg.Of<IEngine>())]]></code></param>
        /// <returns>New Made specification.</returns>
        public static Made Of<T>(Expression<Func<T>> ctorOrMethodOrMethodExpr)
        {
            return FromExpression(typeof(T), ctorOrMethodOrMethodExpr);
        }

        /// <summary>Defines creation info from factory method call expression (without using strings).
        /// You can supply any/default arguments to factory method, they won't be used, it is only to find the <see cref="MethodInfo"/>.</summary>
        /// <typeparam name="TFactory">Factory type.</typeparam> <typeparam name="T">Factory product type.</typeparam>
        /// <param name="getFactoryInfo">Returns or resolves factory instance.</param> <param name="factoryMethodCallExpr">Get a method call expression.</param>
        /// <returns>New Made specification.</returns>
        public static Made Of<TFactory, T>(
            Func<Request, ServiceInfo.Typed<TFactory>> getFactoryInfo, 
            Expression<Func<TFactory, T>> factoryMethodCallExpr)
            where TFactory : class
        {
            getFactoryInfo.ThrowIfNull();
            return FromExpression(typeof(T), factoryMethodCallExpr,
                r => getFactoryInfo(r).ThrowIfNull() /* cannot convert to method group because of lack of covariance support in .Net 3.5*/);
        }

        #region Implementation

        private Made(Type expressionReturnType = null,
            FactoryMethodSelector factoryMethod = null, ParameterSelector parameters = null,
            PropertiesAndFieldsSelector propertiesAndFields = null)
        {
            ExpressionReturnType = expressionReturnType;
            FactoryMethod = factoryMethod;
            Parameters = parameters;
            PropertiesAndFields = propertiesAndFields;
        }

        private static Made FromExpression(Type madeOfType,
            LambdaExpression constructorOrMethodCallExpr, Func<Request, ServiceInfo> getFactoryInfo = null)
        {
            var callExpr = constructorOrMethodCallExpr.ThrowIfNull().Body;

            MemberInfo ctorOrMethodOrMember;
            IList<Expression> argumentExprs = null;
            IList<MemberBinding> memberBindingExprs = null;
            ParameterInfo[] parameters = null;
            if (callExpr.NodeType == ExpressionType.New || callExpr.NodeType == ExpressionType.MemberInit)
            {
                var newExpr = callExpr as NewExpression ?? ((MemberInitExpression)callExpr).NewExpression;
                ctorOrMethodOrMember = newExpr.Constructor;
                parameters = newExpr.Constructor.GetParameters();
                argumentExprs = newExpr.Arguments;
                if (callExpr is MemberInitExpression)
                    memberBindingExprs = ((MemberInitExpression)callExpr).Bindings;
            }
            else if (callExpr.NodeType == ExpressionType.Call)
            {
                var methodCallExpr = ((MethodCallExpression)callExpr);
                ctorOrMethodOrMember = methodCallExpr.Method;
                parameters = methodCallExpr.Method.GetParameters();
                argumentExprs = methodCallExpr.Arguments;
            }
            else if (callExpr.NodeType == ExpressionType.MemberAccess)
            {
                var member = ((MemberExpression)callExpr).Member;
                Throw.If(!(member is PropertyInfo) && !(member is FieldInfo),
                    Error.UNEXPECTED_FACTORY_MEMBER_EXPRESSION, member);
                ctorOrMethodOrMember = member;
            }
            else return Throw.For<Made>(Error.UNEXPECTED_EXPRESSION_TO_MAKE_SERVICE, callExpr);

            FactoryMethodSelector factoryMethod = request =>
                DryIoc.FactoryMethod.Of(ctorOrMethodOrMember, getFactoryInfo == null ? null : getFactoryInfo(request));

            var parameterSelector = parameters.IsNullOrEmpty() ? null : GetParameters(argumentExprs, parameters);

            var propertiesAndFieldsSelector = memberBindingExprs == null || memberBindingExprs.Count == 0 ? null
                : GetPropertiesAndFields(memberBindingExprs);

            return new Made(madeOfType, factoryMethod, parameterSelector, propertiesAndFieldsSelector);
        }

        private static ParameterSelector GetParameters(IList<Expression> argExprs, ParameterInfo[] parameterInfos)
        {
            var parameters = DryIoc.Parameters.Of;
            for (var i = 0; i < argExprs.Count; i++)
            {
                var parameter = parameterInfos[i];
                var methodCallExpr = argExprs[i] as MethodCallExpression;
                if (methodCallExpr != null)
                {
                    Throw.If(methodCallExpr.Method.DeclaringType != typeof(Arg),
                        Error.UNEXPECTED_EXPRESSION_INSTEAD_OF_ARG_METHOD, methodCallExpr);

                    var requiredServiceType = methodCallExpr.Method.GetGenericArguments()[0];
                    if (requiredServiceType == parameter.ParameterType)
                        requiredServiceType = null;

                    var serviceKey = default(object);
                    var ifUnresolved = IfUnresolved.Throw;

                    var argExpr = methodCallExpr.Arguments;
                    for (var j = 0; j < argExpr.Count; j++)
                    {
                        var argValueExpr = GetArgConstantExpressionOrDefault(argExpr[j]);
                        if (argValueExpr != null)
                        {
                            if (argValueExpr.Type == typeof(IfUnresolved))
                                ifUnresolved = (IfUnresolved)argValueExpr.Value;
                            else // service key
                                serviceKey = argValueExpr.Value;
                        }
                    }

                    var defaultValue = ifUnresolved == IfUnresolved.ReturnDefault && parameter.IsOptional ? parameter.DefaultValue : null;
                    parameters = parameters.Details((r, p) => !p.Equals(parameter) ? null : 
                        ServiceDetails.Of(requiredServiceType, serviceKey, ifUnresolved, defaultValue));
                }
                else
                {
                    GetArgConstantExpressionOrDefault(argExprs[i])
                        .ThrowIfNull(Error.UNEXPECTED_EXPRESSION_INSTEAD_OF_CONSTANT, argExprs[i]);
                }
            }
            return parameters;
        }

        private static PropertiesAndFieldsSelector GetPropertiesAndFields(IList<MemberBinding> memberBindings)
        {
            var propertiesAndFields = DryIoc.PropertiesAndFields.Of;
            for (var i = 0; i < memberBindings.Count; i++)
            {
                var memberAssignment = (memberBindings[i] as MemberAssignment).ThrowIfNull();
                var member = memberAssignment.Member;

                var methodCallExpr = memberAssignment.Expression as MethodCallExpression;
                if (methodCallExpr != null)
                {
                    Throw.If(methodCallExpr.Method.DeclaringType != typeof(Arg),
                        Error.UNEXPECTED_EXPRESSION_INSTEAD_OF_ARG_METHOD, methodCallExpr);

                    var requiredServiceType = methodCallExpr.Method.GetGenericArguments()[0];
                    if (requiredServiceType == member.GetReturnTypeOrDefault())
                        requiredServiceType = null;

                    var serviceKey = default(object);
                    var ifUnresolved = IfUnresolved.ReturnDefault;

                    var argExpr = methodCallExpr.Arguments;
                    for (var j = 0; j < argExpr.Count; j++)
                    {
                        var argValueExpr = GetArgConstantExpressionOrDefault(argExpr[j]);
                        if (argValueExpr != null)
                        {
                            if (argValueExpr.Type == typeof(IfUnresolved))
                                ifUnresolved = (IfUnresolved)argValueExpr.Value;
                            else // service key
                                serviceKey = argValueExpr.Value;
                        }
                    }

                    propertiesAndFields = propertiesAndFields.And(r => new[]
                    {
                        PropertyOrFieldServiceInfo.Of(member).WithDetails(
                            ServiceDetails.Of(requiredServiceType, serviceKey, ifUnresolved), r)
                    });
                }
                else
                {
                    GetArgConstantExpressionOrDefault(memberAssignment.Expression)
                        .ThrowIfNull(Error.UNEXPECTED_EXPRESSION_INSTEAD_OF_CONSTANT, memberAssignment.Expression);
                    propertiesAndFields = propertiesAndFields.And(r => new[]
                    {
                        PropertyOrFieldServiceInfo.Of(member)
                    });
                }
            }
            return propertiesAndFields;
        }

        private static ConstantExpression GetArgConstantExpressionOrDefault(Expression arg)
        {
            var valueExpr = arg as ConstantExpression;
            if (valueExpr != null)
                return valueExpr;
            var convert = arg as UnaryExpression; // e.g. (object)SomeEnum.Value
            if (convert != null && convert.NodeType == ExpressionType.Convert)
                valueExpr = convert.Operand as ConstantExpression;
            return valueExpr;
        }

        #endregion
    }

    /// <summary>Class for defining parameters/properties/fields service info in <see cref="Made"/> expressions.
    /// Its methods are NOT actually called, they just used to reflect service info from call expression.</summary>
    public static class Arg
    {
        /// <summary>Specifies required service type of parameter or member. If required type is the same as parameter/member type,
        /// the method is just a placeholder to help detect constructor or factory method, and does not have additional meaning.</summary>
        /// <typeparam name="TRequired">Required service type if different from parameter/member type.</typeparam>
        /// <returns>Returns some (ignored) value.</returns>
        public static TRequired Of<TRequired>() { return default(TRequired); }

        /// <summary>Specifies required service type of parameter or member. Plus specifies if-unresolved policy.</summary>
        /// <typeparam name="TRequired">Required service type if different from parameter/member type.</typeparam>
        /// <param name="ifUnresolved">Defines to throw or to return default if unresolved.</param>
        /// <returns>Returns some (ignored) value.</returns>
        public static TRequired Of<TRequired>(IfUnresolved ifUnresolved) { return default(TRequired); }

        /// <summary>Specifies required service type of parameter or member. Plus specifies service key.</summary>
        /// <typeparam name="TRequired">Required service type if different from parameter/member type.</typeparam>
        /// <param name="serviceKey">Service key object.</param>
        /// <returns>Returns some (ignored) value.</returns>
        public static TRequired Of<TRequired>(object serviceKey) { return default(TRequired); }

        /// <summary>Specifies required service type of parameter or member. Plus specifies if-unresolved policy. Plus specifies service key.</summary>
        /// <typeparam name="TRequired">Required service type if different from parameter/member type.</typeparam>
        /// <param name="ifUnresolved">Defines to throw or to return default if unresolved.</param>
        /// <param name="serviceKey">Service key object.</param>
        /// <returns>Returns some (ignored) value.</returns>
        public static TRequired Of<TRequired>(IfUnresolved ifUnresolved, object serviceKey) { return default(TRequired); }
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
            IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.AppendNotKeyed, 
            object serviceKey = null)
        {
            registrator.Register(factory, serviceType, serviceKey, ifAlreadyRegistered);
        }

        /// <summary>Registers service <paramref name="serviceType"/> with corresponding <paramref name="implementationType"/>.</summary>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="serviceType">The service type to register.</param>
        /// <param name="implementationType">Implementation type. Concrete and open-generic class are supported.</param>
        /// <param name="reuse">(optional) <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. 
        ///     Default value means no reuse, aka Transient.</param>
        /// <param name="with">(optional) specifies <see cref="Made"/>.</param>
        /// <param name="setup">(optional) Factory setup, by default is (<see cref="Setup.Default"/>)</param>
        /// <param name="ifAlreadyRegistered">(optional) Policy to deal with case when service with such type and name is already registered.</param>
        /// <param name="serviceKey">(optional) Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        public static void Register(this IRegistrator registrator, Type serviceType, Type implementationType, 
            IReuse reuse = null, Made with = null, Setup setup = null, 
            IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.AppendNotKeyed, 
            object serviceKey = null)
        {
            var factory = new ReflectionFactory(implementationType, reuse, with, setup);
            registrator.Register(factory, serviceType, serviceKey, ifAlreadyRegistered);
        }

        /// <summary>Registers service of <paramref name="implementationType"/>. ServiceType will be the same as <paramref name="implementationType"/>.</summary>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="implementationType">Implementation type. Concrete and open-generic class are supported.</param>
        /// <param name="reuse">(optional) <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="with">(optional) specifies <see cref="Made"/>.</param>
        /// <param name="setup">(optional) factory setup, by default is (<see cref="Setup.Default"/>)</param>
        /// <param name="ifAlreadyRegistered">(optional) policy to deal with case when service with such type and name is already registered.</param>
        /// <param name="serviceKey">(optional) Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        public static void Register(this IRegistrator registrator, Type implementationType, 
            IReuse reuse = null, Made with = null, Setup setup = null, 
            IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.AppendNotKeyed, 
            object serviceKey = null)
        {
            var factory = new ReflectionFactory(implementationType, reuse, with, setup);
            registrator.Register(factory, implementationType, serviceKey, ifAlreadyRegistered);
        }

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
            IReuse reuse = null, Made made = null, Setup setup = null, 
            IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.AppendNotKeyed, 
            object serviceKey = null)
            where TImplementation : TService
        {
            var factory = new ReflectionFactory(typeof(TImplementation), reuse, made, setup);
            registrator.Register(factory, typeof(TService), serviceKey, ifAlreadyRegistered);
        }

        /// <summary>Registers implementation type <typeparamref name="TImplementation"/> with itself as service type.</summary>
        /// <typeparam name="TImplementation">The type of service.</typeparam>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="reuse">(optional) <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="made">(optional) specifies <see cref="Made"/>.</param>
        /// <param name="setup">(optional) Factory setup, by default is (<see cref="Setup.Default"/>)</param>
        /// <param name="ifAlreadyRegistered">(optional) Policy to deal with case when service with such type and name is already registered.</param>
        /// <param name="serviceKey">(optional) Service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        public static void Register<TImplementation>(this IRegistrator registrator,
            IReuse reuse = null, Made made = null, Setup setup = null, 
            IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.AppendNotKeyed, 
            object serviceKey = null)
        {
            var factory = new ReflectionFactory(typeof(TImplementation), reuse, made, setup);
            registrator.Register(factory, typeof(TImplementation), serviceKey, ifAlreadyRegistered);
        }

        /// <summary>Action that could be used by User to customize register many default behavior.</summary>
        /// <param name="r">Registrator provided to do any arbitrary registration User wants.</param>
        /// <param name="serviceTypes">Valid service type that could be used with <paramref name="implType"/>.</param>
        /// <param name="implType">Concrete or open-generic implementation type.</param>
        public delegate void RegisterManyAction(IRegistrator r, Type[] serviceTypes, Type implType);

        /// <summary>Registers many service types with the same implementation.</summary>
        /// <param name="registrator">Registrator/Container</param>
        /// <param name="serviceTypes">1 or more service types.</param> 
        /// <param name="implType">Should implement service types. Will throw if not.</param>
        /// <param name="reuse">(optional)</param> <param name="made">(optional) How to create implementation instance.</param>
        /// <param name="setup">(optional)</param> <param name="ifAlreadyRegistered">(optional) By default <see cref="IfAlreadyRegistered.AppendNotKeyed"/></param>
        /// <param name="serviceKey">(optional)</param>
        public static void RegisterMany(this IRegistrator registrator, Type[] serviceTypes, Type implType,
            IReuse reuse = null, Made made = null, Setup setup = null,
            IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.AppendNotKeyed,
            object serviceKey = null)
        {
            var factory = new ReflectionFactory(implType, reuse, made, setup);
            if (serviceTypes.Length == 1)
                registrator.Register(serviceTypes[0], factory, ifAlreadyRegistered, serviceKey);
            else for (var i = 0; i < serviceTypes.Length; i++)
                registrator.Register(serviceTypes[i], factory, ifAlreadyRegistered, serviceKey);
        }

        /// <summary>Registers many implementations with their auto-figured service types.</summary>
        /// <param name="registrator">Registrator/Container to register with.</param>
        /// <param name="implTypes">Implementation type provider.</param>
        /// <param name="action">(optional) User specified registration action: 
        /// may be used to filter registrations or specify non-default registration options, e.g. Reuse or ServiceKey, etc.</param>
        /// <param name="nonPublicServiceTypes">(optional) Include non public service types.</param>
        public static void RegisterMany(this IRegistrator registrator,
            IEnumerable<Type> implTypes, RegisterManyAction action, 
            bool nonPublicServiceTypes = false)
        {
            foreach (var implType in implTypes)
            {
                if (implType.IsAbstract() || implType.IsCompilerGenerated())
                    continue;

                var serviceTypes = implType.GetImplementedTypes(ReflectionTools.IncludeImplementedType.SourceType);
                var selectedServiceTypes = nonPublicServiceTypes
                    ? serviceTypes
                    : serviceTypes.Where(ReflectionTools.IsPublicOrNestedPublic);

                if (implType.IsGenericDefinition())
                {
                    var implTypeArgs = implType.GetGenericParamsAndArgs();
                    selectedServiceTypes = selectedServiceTypes
                        .Where(t => t.ContainsAllGenericTypeParameters(implTypeArgs))
                        .Select(t => t.GetGenericDefinitionOrNull());
                }

                serviceTypes = selectedServiceTypes.ToArrayOrSelf();
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
        /// <param name="serviceTypeCondition">(optional) Condition to select only specific service type to register.</param>
        /// <param name="reuse">(optional) Reuse to apply to all service registrations.</param>
        /// <param name="made">(optional) Allow to select constructor/method to create service, specify how to inject its parameters and properties/fields.</param>
        /// <param name="setup">(optional) Factory setup, by default is <see cref="Setup.Default"/>, check <see cref="Setup"/> class for available setups.</param>
        /// <param name="ifAlreadyRegistered">(optional) Policy to deal with existing service registrations.</param>
        /// <param name="nonPublicServiceTypes">(optional) Include non public service types.</param>
        /// <param name="serviceKey">(optional) service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        public static void RegisterMany(this IRegistrator registrator, 
            IEnumerable<Type> implTypes, Func<Type, bool> serviceTypeCondition = null,
            IReuse reuse = null, Made made = null, Setup setup = null, 
            IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.AppendNotKeyed,
            bool nonPublicServiceTypes = false, object serviceKey = null)
        {
            registrator.RegisterMany(implTypes, (r, serviceTypes, implType) =>
            {
                if (serviceTypeCondition != null)
                    serviceTypes = serviceTypes.Where(serviceTypeCondition).ToArrayOrSelf();
                if (serviceTypes.Length != 0)
                    r.RegisterMany(serviceTypes, implType, reuse, made, setup, ifAlreadyRegistered, serviceKey);
            },
            nonPublicServiceTypes);
        }

        /// <summary>Registers single registration for all implemented public interfaces and base classes.</summary>
        /// <typeparam name="TImplementation">The type of service.</typeparam>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="reuse">(optional) <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="made">(optional) Allow to select constructor/method to create service, specify how to inject its parameters and properties/fields.</param>
        /// <param name="setup">(optional) Factory setup, by default is <see cref="Setup.Default"/>, check <see cref="Setup"/> class for available setups.</param>
        /// <param name="ifAlreadyRegistered">(optional) policy to deal with case when service with such type and name is already registered.</param>
        /// <param name="nonPublicServiceTypes">(optional) Include non public service types.</param>
        /// <param name="serviceKey">(optional) service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        public static void RegisterMany<TImplementation>(this IRegistrator registrator,
            IReuse reuse = null, Made made = null, Setup setup = null,
            IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.AppendNotKeyed,
            bool nonPublicServiceTypes = false, object serviceKey = null)
        {
            registrator.RegisterMany(new[] { typeof(TImplementation) }, (r, serviceTypes, implType) => 
                r.RegisterMany(serviceTypes, implType, reuse, made, setup, ifAlreadyRegistered, serviceKey),
                nonPublicServiceTypes);
        }

        /// <summary>Registers many implementations with their auto-figured service types.</summary>
        /// <param name="registrator">Registrator/Container to register with.</param>
        /// <param name="implTypeAssemblies">Assemblies with implementation/service types to register.</param>
        /// <param name="action">(optional) User specified registration action: 
        /// may be used to filter registrations or specify non-default registration options, e.g. Reuse or ServiceKey, etc.</param>
        /// <param name="nonPublicServiceTypes">(optional) Include non public service types.</param>
        public static void RegisterMany(this IRegistrator registrator,
            IEnumerable<Assembly> implTypeAssemblies, RegisterManyAction action = null, bool nonPublicServiceTypes = false)
        {
            var implTypes = implTypeAssemblies.ThrowIfNull().SelectMany(Portable.GetTypesFromAssembly);
            registrator.RegisterMany(implTypes, action, nonPublicServiceTypes);
        }

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
            IReuse reuse = null, Made made = null, Setup setup = null, 
            IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.AppendNotKeyed,
            bool nonPublicServiceTypes = false, object serviceKey = null)
        {
            registrator.RegisterMany(implTypeAssemblies.ThrowIfNull().SelectMany(Portable.GetTypesFromAssembly), 
                serviceTypeCondition, reuse, made, setup, ifAlreadyRegistered, nonPublicServiceTypes, serviceKey);
        }

        /// <summary>Registers a factory delegate for creating an instance of <typeparamref name="TService"/>.
        /// Delegate can use <see cref="IResolver"/> parameter to resolve any required dependencies, e.g.:
        /// <code lang="cs"><![CDATA[container.RegisterDelegate<ICar>(r => new Car(r.Resolve<IEngine>()))]]></code></summary>
        /// <typeparam name="TService">The type of service.</typeparam>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="factoryDelegate">The delegate used to create a instance of <typeparamref name="TService"/>.</param>
        /// <param name="reuse">(optional) <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="setup">(optional) factory setup, by default is (<see cref="Setup.Default"/>)</param>
        /// <param name="ifAlreadyRegistered">(optional) policy to deal with case when service with such type and name is already registered.</param>
        /// <param name="serviceKey">(optional). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <remarks>The method should be used as the last resort only! Though powerful it is easy to get memory leaks
        /// (due variables captured in delegate closure) and impossible to use in generation scenarios.
        /// Consider using FactoryMethod instead: 
        /// <code lang="cs"><![CDATA[container.Register<ICar>(with: Method.Of(() => new Car(Arg.Of<IEngine>())))]]></code>.</remarks>
        public static void RegisterDelegate<TService>(this IRegistrator registrator, Func<IResolver, TService> factoryDelegate, 
            IReuse reuse = null, Setup setup = null, IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.AppendNotKeyed, 
            object serviceKey = null)
        {
            var factory = new DelegateFactory(r => factoryDelegate(r), reuse, setup);
            registrator.Register(factory, typeof(TService), serviceKey, ifAlreadyRegistered);
        }

        /// <summary>Registers a factory delegate for creating an instance of <paramref name="serviceType"/>.
        /// Delegate can use <see cref="IResolver"/> parameter to resolve any required dependencies, e.g.:
        /// <code lang="cs"><![CDATA[container.RegisterDelegate<ICar>(r => new Car(r.Resolve<IEngine>()))]]></code></summary>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="serviceType">Service type to register.</param>
        /// <param name="factoryDelegate">The delegate used to create a instance of <paramref name="serviceType"/>.</param>
        /// <param name="reuse">(optional) <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="setup">(optional) factory setup, by default is (<see cref="Setup.Default"/>)</param>
        /// <param name="ifAlreadyRegistered">(optional) policy to deal with case when service with such type and name is already registered.</param>
        /// <param name="serviceKey">(optional) Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        public static void RegisterDelegate(this IRegistrator registrator, Type serviceType, Func<IResolver, object> factoryDelegate, 
            IReuse reuse = null, Setup setup = null, IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.AppendNotKeyed, 
            object serviceKey = null)
        {
            Func<IResolver, object> checkedDelegate = r => factoryDelegate(r)
                .ThrowIfNotOf(serviceType, Error.REGED_FACTORY_DLG_RESULT_NOT_OF_SERVICE_TYPE, r);
            var factory = new DelegateFactory(checkedDelegate, reuse, setup);
            registrator.Register(factory, serviceType, serviceKey, ifAlreadyRegistered);
        }

        /// <summary>Registers decorator function that gets decorated value as input and return decorator.</summary>
        /// <typeparam name="TService">Registered service type to decorate.</typeparam>
        /// <param name="registrator">Registrator/Container.</param>
        /// <param name="getDecorator">Delegate returning decorating function.</param>
        /// <param name="condition">(optional) Condition for decorator application.</param>
        public static void RegisterDelegateDecorator<TService>(this IRegistrator registrator,
            Func<IResolver, Func<TService, TService>> getDecorator, Func<Request, bool> condition = null)
        {
            registrator.RegisterDelegate(getDecorator, setup: Setup.DecoratorWith(condition));
        }

        /// <summary>Registers a pre-created object assignable to <paramref name="serviceType"/>. </summary>
        /// <param name="container">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="serviceType">Service type to register.</param>
        /// <param name="instance">The pre-created instance of <paramref name="serviceType"/>.</param>
        /// <param name="reuse">(optional) <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="ifAlreadyRegistered">(optional) policy to deal with case when service with such type and name is already registered.</param>
        /// <param name="serviceKey">(optional) service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        public static void RegisterInstance(this IContainer container, Type serviceType, object instance,
            IReuse reuse = null, IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.AppendNotKeyed,
            object serviceKey = null)
        {
            if (instance != null)
                instance.ThrowIfNotOf(serviceType, Error.REGED_OBJ_NOT_ASSIGNABLE_TO_SERVICE_TYPE);

            if (reuse == null || reuse is ResolutionScopeReuse)
            {
                var instanceType = instance == null ? null : instance.GetType();
                var factory = new DelegateFactory(_ => instance, implementationType: instanceType);
                container.Register(factory, serviceType, serviceKey, ifAlreadyRegistered);
                return;
            }

            // Try get existing factory.
            if (ifAlreadyRegistered == IfAlreadyRegistered.Replace)
            {
                var request = container.EmptyRequest.Push(serviceType, serviceKey);
                var registeredFactory = container.GetServiceFactoryOrDefault(request);

                // If existing factory is the same kind: reuse and setup-wise, then we can just replace value in scope.
                if (registeredFactory != null &&
                    registeredFactory.Reuse == reuse &&
                    registeredFactory.Setup == Setup.Default)
                {
                    reuse.GetScopeOrDefault(container.EmptyRequest)
                        .ThrowIfNull(Error.NO_SCOPE_WHEN_REGISTERING_INSTANCE, instance, reuse)
                        .SetOrAdd(registeredFactory.FactoryID, instance);
                    return;
                }
            }

            // Create factory to locate instance in scope.
            var locatorFactory = new ExpressionFactory(GetThrowInstanceNoLongerAvailable, reuse);
            if (container.Register(locatorFactory, serviceType, serviceKey, ifAlreadyRegistered))
                reuse.GetScopeOrDefault(container.EmptyRequest)
                    .ThrowIfNull(Error.NO_SCOPE_WHEN_REGISTERING_INSTANCE, instance, reuse)
                    .SetOrAdd(locatorFactory.FactoryID, instance);
        }

        private static Expression GetThrowInstanceNoLongerAvailable(Request r)
        {
            return Expression.Call(typeof(Throw), "For", new[] { r.ServiceType },
                Expression.Constant(Error.UNABLE_TO_RESOLVE_SERVICE), Expression.Constant(r.ServiceType),
                Expression.Constant(null), Expression.Constant(null), Expression.Constant(null));
        }

        /// <summary>Registers a pre-created object of <typeparamref name="TService"/>.
        /// It is just a sugar on top of <see cref="RegisterDelegate{TService}"/> method.</summary>
        /// <typeparam name="TService">The type of service.</typeparam>
        /// <param name="container">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="instance">The pre-created instance of <typeparamref name="TService"/>.</param>
        /// <param name="reuse">(optional) <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="ifAlreadyRegistered">(optional) policy to deal with case when service with such type and name is already registered.</param>
        /// <param name="serviceKey">(optional) Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        public static void RegisterInstance<TService>(this IContainer container, TService instance,
            IReuse reuse = null, IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.AppendNotKeyed,
            object serviceKey = null)
        {
            container.RegisterInstance(typeof(TService), instance, reuse, ifAlreadyRegistered, serviceKey);
        }

        /// <summary>Registers initializing action that will be called after service is resolved just before returning it to caller.
        /// Check example below for using initializer to automatically subscribe to singleton event aggregator.
        /// You can register multiple initializers for single service. 
        /// Or you can register initializer for <see cref="Object"/> type to be applied for all services and use <paramref name="condition"/> 
        /// to filter target services.</summary>
        /// <remarks>Initializer internally implemented as decorator registered as Action delegate, so all decorators behavior is applied.</remarks>
        /// <typeparam name="TTarget">Any type implemented by requested service type including service type itself and object type.</typeparam>
        /// <param name="registrator">Usually is <see cref="Container"/> object.</param>
        /// <param name="initialize">Delegate with <typeparamref name="TTarget"/> object and 
        /// <see cref="IResolver"/> to resolve additional services required by initializer.</param>
        /// <param name="condition">(optional) Condition to select required target.</param>
        /// <example><code lang="cs"><![CDATA[
        ///     container.Register<EventAggregator>(Reuse.Singleton);
        ///     container.Register<ISubscriber, SomeSubscriber>();
        /// 
        ///     // Registers initializer for all subscribers implementing ISubscriber.
        ///     container.RegisterInitiliazer<ISubscriber>((s, r) => r.Resolve<EventAggregator>().Subscribe(s));
        /// ]]></code></example>
        public static void RegisterInitializer<TTarget>(this IRegistrator registrator,
            Action<TTarget, IResolver> initialize, Func<Request, bool> condition = null)
        {
            registrator.RegisterDelegate<Action<TTarget>>(r => target => initialize(target, r),
                setup: Setup.DecoratorWith(condition));
        }

        /// <summary>Returns true if <paramref name="serviceType"/> is registered in container or its open generic definition is registered in container.</summary>
        /// <param name="registrator">Usually <see cref="Container"/> to explore or any other <see cref="IRegistrator"/> implementation.</param>
        /// <param name="serviceType">The type of the registered service.</param>
        /// <param name="serviceKey">(optional) Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="factoryType">(optional) factory type to lookup, <see cref="FactoryType.Service"/> by default.</param>
        /// <param name="condition">(optional) condition to specify what registered factory do you expect.</param>
        /// <returns>True if <paramref name="serviceType"/> is registered, false - otherwise.</returns>
        public static bool IsRegistered(this IRegistrator registrator, Type serviceType,
            object serviceKey = null, FactoryType factoryType = FactoryType.Service, Func<Factory, bool> condition = null)
        {
            return registrator.IsRegistered(serviceType, serviceKey, factoryType, condition);
        }

        /// <summary>Returns true if <typeparamref name="TService"/> type is registered in container or its open generic definition is registered in container.</summary>
        /// <typeparam name="TService">The type of service.</typeparam>
        /// <param name="registrator">Usually <see cref="Container"/> to explore or any other <see cref="IRegistrator"/> implementation.</param>
        /// <param name="serviceKey">(optional) Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="factoryType">(optional) factory type to lookup, <see cref="FactoryType.Service"/> by default.</param>
        /// <param name="condition">(optional) condition to specify what registered factory do you expect.</param>
        /// <returns>True if <typeparamref name="TService"/> name="serviceType"/> is registered, false - otherwise.</returns>
        public static bool IsRegistered<TService>(this IRegistrator registrator,
            object serviceKey = null, FactoryType factoryType = FactoryType.Service, Func<Factory, bool> condition = null)
        {
            return registrator.IsRegistered(typeof(TService), serviceKey, factoryType, condition);
        }

        /// <summary>Removes specified registration from container.</summary>
        /// <param name="registrator">Usually <see cref="Container"/> to explore or any other <see cref="IRegistrator"/> implementation.</param>
        /// <param name="serviceType">Type of service to remove.</param>
        /// <param name="serviceKey">(optional) Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="factoryType">(optional) Factory type to lookup, <see cref="FactoryType.Service"/> by default.</param>
        /// <param name="condition">(optional) Condition for Factory to be removed.</param>
        public static void Unregister(this IRegistrator registrator, Type serviceType,
            object serviceKey = null, FactoryType factoryType = FactoryType.Service, Func<Factory, bool> condition = null)
        {
            registrator.Unregister(serviceType, serviceKey, factoryType, condition);
        }

        /// <summary>Removes specified registration from container.</summary>
        /// <typeparam name="TService">The type of service to remove.</typeparam>
        /// <param name="registrator">Usually <see cref="Container"/> or any other <see cref="IRegistrator"/> implementation.</param>
        /// <param name="serviceKey">(optional) Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="factoryType">(optional) Factory type to lookup, <see cref="FactoryType.Service"/> by default.</param>
        /// <param name="condition">(optional) Condition for Factory to be removed.</param>
        public static void Unregister<TService>(this IRegistrator registrator,
            object serviceKey = null, FactoryType factoryType = FactoryType.Service, Func<Factory, bool> condition = null)
        {
            registrator.Unregister(typeof(TService), serviceKey, factoryType, condition);
        }
    }

    /// <summary>Defines convenient extension methods for <see cref="IResolver"/>.</summary>
    public static class Resolver
    {
        /// <summary>Returns instance of <typepsaramref name="TService"/> type.</summary>
        /// <param name="serviceType">The type of the requested service.</param>
        /// <param name="resolver">Any <see cref="IResolver"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="ifUnresolved">(optional) Says how to handle unresolved service.</param>
        /// <returns>The requested service instance.</returns>
        public static object Resolve(this IResolver resolver, Type serviceType, IfUnresolved ifUnresolved = IfUnresolved.Throw)
        {
            return resolver.ResolveDefault(serviceType, ifUnresolved, null);
        }

        /// <summary>Returns instance of <typepsaramref name="TService"/> type.</summary>
        /// <typeparam name="TService">The type of the requested service.</typeparam>
        /// <param name="resolver">Any <see cref="IResolver"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="ifUnresolved">(optional) Says how to handle unresolved service.</param>
        /// <returns>The requested service instance.</returns>
        public static TService Resolve<TService>(this IResolver resolver, IfUnresolved ifUnresolved = IfUnresolved.Throw)
        {
            return (TService)resolver.ResolveDefault(typeof(TService), ifUnresolved, null);
        }

        /// <summary>Returns instance of <typeparamref name="TService"/> searching for <paramref name="requiredServiceType"/>.
        /// In case of <typeparamref name="TService"/> being generic wrapper like Func, Lazy, IEnumerable, etc., <paramref name="requiredServiceType"/>
        /// could specify wrapped service type.</summary>
        /// <typeparam name="TService">The type of the requested service.</typeparam>
        /// <param name="resolver">Any <see cref="IResolver"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="requiredServiceType">(optional) Service or wrapped type assignable to <typeparamref name="TService"/>.</param>
        /// <param name="ifUnresolved">(optional) Says how to handle unresolved service.</param>
        /// <returns>The requested service instance.</returns>
        /// <remarks>Using <paramref name="requiredServiceType"/> implicitly support Covariance for generic wrappers even in .Net 3.5.</remarks>
        /// <example><code lang="cs"><![CDATA[
        ///     container.Register<IService, Service>();
        ///     var services = container.Resolve<IEnumerable<object>>(typeof(IService));
        /// ]]></code></example>
        public static TService Resolve<TService>(this IResolver resolver, Type requiredServiceType, IfUnresolved ifUnresolved = IfUnresolved.Throw)
        {
            return (TService)resolver.ResolveKeyed(typeof(TService), null, ifUnresolved, requiredServiceType, null);
        }

        /// <summary>Returns instance of <paramref name="serviceType"/> searching for <paramref name="requiredServiceType"/>.
        /// In case of <paramref name="serviceType"/> being generic wrapper like Func, Lazy, IEnumerable, etc., <paramref name="requiredServiceType"/>
        /// could specify wrapped service type.</summary>
        /// <param name="serviceType">The type of the requested service.</param>
        /// <param name="resolver">Any <see cref="IResolver"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="serviceKey">Service key (any type with <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/> defined).</param>
        /// <param name="ifUnresolved">(optional) Says how to handle unresolved service.</param>
        /// <param name="requiredServiceType">(optional) Service or wrapped type assignable to <paramref name="serviceType"/>.</param>
        /// <returns>The requested service instance.</returns>
        /// <remarks>Using <paramref name="requiredServiceType"/> implicitly support Covariance for generic wrappers even in .Net 3.5.</remarks>
        /// <example><code lang="cs"><![CDATA[
        ///     container.Register<IService, Service>();
        ///     var services = container.Resolve(typeof(Lazy<object>), "someKey", requiredServiceType: typeof(IService));
        /// ]]></code></example>
        public static object Resolve(this IResolver resolver, Type serviceType, object serviceKey,
            IfUnresolved ifUnresolved = IfUnresolved.Throw, Type requiredServiceType = null)
        {
            return serviceKey == null && requiredServiceType == null
                ? resolver.ResolveDefault(serviceType, ifUnresolved, null)
                : resolver.ResolveKeyed(serviceType, serviceKey, ifUnresolved, requiredServiceType, null);
        }

        /// <summary>Returns instance of <typepsaramref name="TService"/> type.</summary>
        /// <typeparam name="TService">The type of the requested service.</typeparam>
        /// <param name="resolver">Any <see cref="IResolver"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="serviceKey">Service key (any type with <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/> defined).</param>
        /// <param name="ifUnresolved">(optional) Says how to handle unresolved service.</param>
        /// <param name="requiredServiceType">(optional) Service or wrapped type assignable to <typeparamref name="TService"/>.</param>
        /// <returns>The requested service instance.</returns>
        /// <remarks>Using <paramref name="requiredServiceType"/> implicitly support Covariance for generic wrappers even in .Net 3.5.</remarks>
        public static TService Resolve<TService>(this IResolver resolver, object serviceKey,
            IfUnresolved ifUnresolved = IfUnresolved.Throw, Type requiredServiceType = null)
        {
            return (TService)resolver.Resolve(typeof(TService), serviceKey, ifUnresolved, requiredServiceType);
        }

        /// <summary>Returns all registered services instances including all keyed and default registrations.
        /// Use <paramref name="behavior"/> to return either all registered services at the moment of resolve (dynamic fresh view) or
        /// the same services that were returned with first <see cref="ResolveMany{TService}"/> call (fixed view).</summary>
        /// <typeparam name="TService">Return collection item type. It denotes registered service type if <paramref name="requiredServiceType"/> is not specified.</typeparam>
        /// <param name="resolver">Usually <see cref="Container"/> object.</param>
        /// <param name="requiredServiceType">(optional) Denotes registered service type. Should be assignable to <typeparamref name="TService"/>.</param>
        /// <param name="behavior">(optional) Specifies new registered services awareness. Aware by default.</param>
        /// <returns>Result collection of services.</returns>
        /// <remarks>The same result could be achieved by directly calling:
        /// <code lang="cs"><![CDATA[
        ///     container.Resolve<LazyEnumerable<IService>>();  // for dynamic result - default behavior
        ///     container.Resolve<IService[]>();                // for fixed array
        ///     container.Resolve<IEnumerable<IService>>();     // same as fixed array
        /// ]]></code>
        /// </remarks>
        public static IEnumerable<TService> ResolveMany<TService>(this IResolver resolver,
            Type requiredServiceType = null, ResolveManyBehavior behavior = ResolveManyBehavior.AsLazyEnumerable)
        {
            return behavior == ResolveManyBehavior.AsLazyEnumerable
                ? resolver.ResolveMany(typeof(TService), null, requiredServiceType, null, null).Cast<TService>()
                : resolver.Resolve<IEnumerable<TService>>(requiredServiceType);
        }

        /// <summary>For given instance resolves and sets properties and fields.
        /// It respects <see cref="DryIoc.Rules.PropertiesAndFields"/> rules set per container, 
        /// or if rules are not set it uses <see cref="PropertiesAndFields.Auto"/>, 
        /// or you can specify your own rules with <paramref name="propertiesAndFields"/> parameter.</summary>
        /// <typeparam name="TService">Input and returned instance type.</typeparam>Service (wrapped)
        /// <param name="container">Usually a container instance, cause <see cref="Container"/> implements <see cref="IResolver"/></param>
        /// <param name="instance">Service instance with properties to resolve and initialize.</param>
        /// <param name="propertiesAndFields">(optional) Function to select properties and fields, overrides all other rules if specified.</param>
        /// <returns>Input instance with resolved dependencies, to enable fluent method composition.</returns>
        /// <remarks>Different Rules could be combined together using <see cref="PropertiesAndFields.And"/> method.</remarks>        
        public static TService InjectPropertiesAndFields<TService>(this IContainer container,
            TService instance, PropertiesAndFieldsSelector propertiesAndFields = null)
        {
            return (TService)container.InjectPropertiesAndFields(instance, propertiesAndFields);
        }

        /// <summary>Creates service using container for injecting parameters without registering anything.</summary>
        /// <param name="container">Container to use for type creation and injecting its dependencies.</param>
        /// <param name="concreteType">Type to instantiate.</param>
        /// <param name="made">(optional) Injection rules to select constructor/factory method, inject parameters, properties and fields.</param>
        /// <returns>Object instantiated by constructor or object returned by factory method.</returns>
        public static object New(this IContainer container, Type concreteType, Made made = null)
        {
            concreteType.ThrowIfNull().ThrowIf(concreteType.IsOpenGeneric(), Error.UNABLE_TO_NEW_OPEN_GENERIC);
            var factory = new ReflectionFactory(concreteType, null, made, Setup.With(cacheFactoryExpression: false));
            factory.CheckBeforeRegistration(container, concreteType, null);
            var request = container.EmptyRequest.Push(ServiceInfo.Of(concreteType)).ResolveWithFactory(factory);
            var factoryDelegate = factory.GetDelegateOrDefault(request);
            var service = factoryDelegate(container.ResolutionStateCache, container.ContainerWeakRef, null);
            return service;
        }

        /// <summary>Creates service using container for injecting parameters without registering anything.</summary>
        /// <typeparam name="T">Type to instantiate.</typeparam>
        /// <param name="container">Container to use for type creation and injecting its dependencies.</param>
        /// <param name="made">(optional) Injection rules to select constructor/factory method, inject parameters, properties and fields.</param>
        /// <returns>Object instantiated by constructor or object returned by factory method.</returns>
        public static T New<T>(this IContainer container, Made made = null)
        {
            return (T)container.New(typeof(T), made);
        }

        internal static Expression CreateResolutionExpression(Request request, Type serviceType = null)
        {
            serviceType = serviceType ?? request.ServiceType;
            var serviceTypeExpr = request.Container.GetOrAddStateItemExpression(serviceType, typeof(Type));
            var ifUnresolvedExpr = Expression.Constant(request.IfUnresolved);
            var requiredServiceTypeExpr = request.Container.GetOrAddStateItemExpression(request.RequiredServiceType, typeof(Type));
            var serviceKeyExpr = Expression.Convert(request.Container.GetOrAddStateItemExpression(request.ServiceKey), typeof(object));

            var getOrNewCallExpr = Container.GetResolutionScopeExpression(request);

            var resolveExpr = Expression.Call(Container.ResolverExpr, "ResolveKeyed", null,
                serviceTypeExpr, serviceKeyExpr, ifUnresolvedExpr, requiredServiceTypeExpr,
                getOrNewCallExpr);

            return Expression.Convert(resolveExpr, serviceType);
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
        public static readonly ServiceDetails Default = new ServiceDetails();

        /// <summary>The same as <see cref="Default"/> with only difference <see cref="IfUnresolved"/> set to <see cref="DryIoc.IfUnresolved.ReturnDefault"/>.</summary>
        public static readonly ServiceDetails IfUnresolvedReturnDefault = new WithIfUnresolvedReturnDefault();

        /// <summary>Creates new DTO out of provided settings, or returns default if all settings have default value.</summary>
        /// <param name="requiredServiceType">Registered service type to search for.</param>
        /// <param name="serviceKey">Service key.</param> <param name="ifUnresolved">If unresolved policy.</param>
        /// <param name="defaultValue">Custom default value, if specified it will automatically set <paramref name="ifUnresolved"/> to <see cref="DryIoc.IfUnresolved.ReturnDefault"/>.</param>
        /// <returns>Created details DTO.</returns>
        public static ServiceDetails Of(Type requiredServiceType = null,
            object serviceKey = null, IfUnresolved ifUnresolved = IfUnresolved.Throw, 
            object defaultValue = null)
        {
            return ifUnresolved == IfUnresolved.Throw && defaultValue == null
                ? (requiredServiceType == null
                    ? (serviceKey == null ? Default : new WithKey(serviceKey))
                    : new WithType(requiredServiceType, serviceKey))
                : (requiredServiceType == null
                    ? (serviceKey == null && defaultValue == null
                        ? IfUnresolvedReturnDefault
                        : new WithKeyReturnDefault(serviceKey, defaultValue))
                    : new WithTypeReturnDefault(requiredServiceType, serviceKey, defaultValue));
        }

        /// <summary>Sets custom value for service. This setting is orthogonal to the rest.</summary>
        /// <param name="value">Custom value.</param> <returns>Details with custom value.</returns>
        public static ServiceDetails Of(object value)
        {
            return new WithCustomValue(value);
        }

        /// <summary>Service type to search in registry. Should be assignable to user requested service type.</summary>
        public virtual Type RequiredServiceType { get { return null; } }

        /// <summary>Service key provided with registration.</summary>
        public virtual object ServiceKey { get { return null; } }

        /// <summary>Policy to deal with unresolved request.</summary>
        public virtual IfUnresolved IfUnresolved { get { return IfUnresolved.Throw; } }

        /// <summary>Value to use in case <see cref="IfUnresolved"/> is set to <see cref="DryIoc.IfUnresolved.ReturnDefault"/>.</summary>
        public virtual object DefaultValue { get { return null; } }

        /// <summary>Custom value specified for dependency.</summary>
        public virtual object CustomValue { get { return null; } }

        /// <summary>Pretty prints service details to string for debugging and errors.</summary> <returns>Details string.</returns>
        public override string ToString()
        {
            if (CustomValue != null)
                return "{with custom value: " + CustomValue + "}";

            var s = new StringBuilder();
            if (RequiredServiceType != null)
                s.Append("{required: ").Print(RequiredServiceType);
            if (ServiceKey != null)
                (s.Length == 0 ? s.Append('{') : s.Append(", ")).Print(ServiceKey, "\"");
            if (IfUnresolved != IfUnresolved.Throw)
                (s.Length == 0 ? s.Append('{') : s.Append(", ")).Append("allow default");
            return (s.Length == 0 ? s : s.Append('}')).ToString();
        }

        #region Implementation

        private sealed class WithIfUnresolvedReturnDefault : ServiceDetails
        {
            public override IfUnresolved IfUnresolved { get { return IfUnresolved.ReturnDefault; } }
        }

        private class WithCustomValue : ServiceDetails
        {
            public override object CustomValue { get { return _value; } }
            public WithCustomValue(object value) { _value = value; }
            private readonly object _value;
        }

        private class WithKey : ServiceDetails
        {
            public override object ServiceKey { get { return _serviceKey; } }
            public WithKey(object serviceKey) { _serviceKey = serviceKey; }
            private readonly object _serviceKey;
        }

        private sealed class WithKeyReturnDefault : WithKey
        {
            public override IfUnresolved IfUnresolved { get { return IfUnresolved.ReturnDefault; } }
            public override object DefaultValue { get { return _defaultValue; } }
            public WithKeyReturnDefault(object serviceKey, object defaultValue)
                : base(serviceKey) { _defaultValue = defaultValue; }
            private readonly object _defaultValue;
        }

        private class WithType : WithKey
        {
            public override Type RequiredServiceType { get { return _requiredServiceType; } }
            public WithType(Type requiredServiceType, object serviceKey)
                : base(serviceKey) { _requiredServiceType = requiredServiceType; }
            private readonly Type _requiredServiceType;
        }

        private sealed class WithTypeReturnDefault : WithType
        {
            public override IfUnresolved IfUnresolved { get { return IfUnresolved.ReturnDefault; } }
            public override object DefaultValue { get { return _defaultValue; } }
            public WithTypeReturnDefault(Type requiredServiceType, object serviceKey, object defaultValue)
                : base(requiredServiceType, serviceKey) { _defaultValue = defaultValue; }
            private readonly object _defaultValue;
        }

        #endregion
    }

    /// <summary>Contains tools for combining or propagating of <see cref="IServiceInfo"/> independent of its concrete implementations.</summary>
    public static class ServiceInfoTools
    {
        /// <summary>Combines service info with details: the main task is to combine service and required service type.</summary>
        /// <typeparam name="T">Type of <see cref="IServiceInfo"/>.</typeparam>
        /// <param name="info">Source info.</param> <param name="details">Details to combine with info.</param> 
        /// <param name="request">Owner request.</param> <returns>Original source or new combined info.</returns>
        public static T WithDetails<T>(this T info, ServiceDetails details, Request request)
            where T : IServiceInfo
        {
            var serviceType = info.ServiceType;
            var wrappedServiceType = request.Container.UnwrapServiceType(serviceType);
            var requiredServiceType = details == null ? null : details.RequiredServiceType;
            if (requiredServiceType != null)
            {
                var wrappedRequiredServiceType = request.Container.UnwrapServiceType(requiredServiceType);
                wrappedServiceType.ThrowIfNotOf(wrappedRequiredServiceType, Error.WRAPPED_NOT_ASSIGNABLE_FROM_REQUIRED_TYPE, request);

                // Replace serviceType with Required if they are assignable
                if (requiredServiceType.IsAssignableTo(serviceType))
                {
                    serviceType = requiredServiceType; // override service type with required one
                    details = ServiceDetails.Of(null, details.ServiceKey, details.IfUnresolved);
                }
            }

            return serviceType == info.ServiceType && (details == null || details == info.Details)
                ? info // if service type unchanged and details absent, or details are the same return original info.
                : (T)info.Create(serviceType, details); // otherwise: create new.
        }

        /// <summary>Enables propagation/inheritance of info between dependency and its owner: 
        /// for instance <see cref="ServiceDetails.RequiredServiceType"/> for wrappers.</summary>
        /// <param name="dependency">Dependency info.</param>
        /// <param name="owner">Dependency holder/owner info.</param>
        /// <param name="shouldInheritServiceKey">(optional) Self-explanatory. Usually set to true for wrapper and decorator info.</param>
        /// <returns>Either input dependency info, or new info with properties inherited from the owner.</returns>
        public static IServiceInfo InheritInfo(this IServiceInfo dependency, IServiceInfo owner, bool shouldInheritServiceKey = false)
        {
            var ownerDetails = owner.Details;
            if (ownerDetails == null || ownerDetails == ServiceDetails.Default)
                return dependency;

            var dependencyDetails = dependency.Details;

            var ifUnresolved = ownerDetails.IfUnresolved == IfUnresolved.Throw
                ? dependencyDetails.IfUnresolved
                : ownerDetails.IfUnresolved;

            // Use dependency key if it's non default, otherwise and if owner is not service, the
            var serviceKey = dependencyDetails.ServiceKey == null && shouldInheritServiceKey
                ? ownerDetails.ServiceKey
                : dependencyDetails.ServiceKey;

            var serviceType = dependency.ServiceType;
            var requiredServiceType = dependencyDetails.RequiredServiceType;
            if (ownerDetails.RequiredServiceType != null)
            {
                requiredServiceType = null;
                if (ownerDetails.RequiredServiceType.IsAssignableTo(serviceType))
                    serviceType = ownerDetails.RequiredServiceType;
                else
                    requiredServiceType = ownerDetails.RequiredServiceType;
            }

            if (serviceType == dependency.ServiceType && serviceKey == dependencyDetails.ServiceKey &&
                ifUnresolved == dependencyDetails.IfUnresolved && requiredServiceType == dependencyDetails.RequiredServiceType)
                return dependency;

            return dependency.Create(serviceType, ServiceDetails.Of(requiredServiceType, serviceKey, ifUnresolved));
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
        /// <summary>Creates info out of provided settings</summary>
        /// <param name="serviceType">Service type</param>
        /// <param name="ifUnresolved">(optional) If unresolved policy. Set to Throw if not specified.</param>
        /// <param name="serviceKey">(optional) Service key.</param>
        /// <returns>Created info.</returns>
        public static ServiceInfo Of(Type serviceType, IfUnresolved ifUnresolved = IfUnresolved.Throw, object serviceKey = null)
        {
            serviceType.ThrowIfNull().ThrowIf(serviceType.IsOpenGeneric(), Error.EXPECTED_CLOSED_GENERIC_SERVICE_TYPE);
            return serviceKey == null && ifUnresolved == IfUnresolved.Throw
                ? new ServiceInfo(serviceType)
                : new WithDetails(serviceType, ServiceDetails.Of(null, serviceKey, ifUnresolved));
        }

        /// <summary>Creates service info using typed <typeparamref name="TService"/>.</summary>
        /// <typeparam name="TService">Service type.</typeparam>
        /// <param name="ifUnresolved">(optional)</param> <param name="serviceKey">(optional)</param>
        /// <returns>Created info.</returns>
        public static Typed<TService> Of<TService>(IfUnresolved ifUnresolved = IfUnresolved.Throw, object serviceKey = null)
        {
            return serviceKey == null && ifUnresolved == IfUnresolved.Throw
                ? new Typed<TService>()
                : new TypedWithDetails<TService>(ServiceDetails.Of(null, serviceKey, ifUnresolved));
        }

        /// <summary>Strongly-typed version of Service Info.</summary> <typeparam name="TService">Service type.</typeparam>
        public class Typed<TService> : ServiceInfo
        {
            /// <summary>Creates service info object.</summary>
            public Typed() : base(typeof(TService)) { }
        }

        /// <summary>Type of service to create. Indicates registered service in registry.</summary>
        public Type ServiceType { get; private set; }

        /// <summary>Additional settings. If not specified uses <see cref="ServiceDetails.Default"/>.</summary>
        public virtual ServiceDetails Details { get { return ServiceDetails.Default; } }

        /// <summary>Creates info from service type and details.</summary>
        /// <param name="serviceType">Required service type.</param> <param name="details">Optional details.</param> <returns>Create info.</returns>
        public IServiceInfo Create(Type serviceType, ServiceDetails details)
        {
            return details == ServiceDetails.Default
                ? new ServiceInfo(serviceType)
                : new WithDetails(serviceType, details);
        }

        /// <summary>Prints info to string using <see cref="ServiceInfoTools.Print"/>.</summary> <returns>Printed string.</returns>
        public override string ToString()
        {
            return new StringBuilder().Print(this).ToString();
        }

        #region Implementation

        private ServiceInfo(Type serviceType)
        {
            ServiceType = serviceType;
        }

        private class WithDetails : ServiceInfo
        {
            public override ServiceDetails Details { get { return _details; } }
            public WithDetails(Type serviceType, ServiceDetails details) : base(serviceType) { _details = details; }
            private readonly ServiceDetails _details;
        }

        private class TypedWithDetails<TService> : Typed<TService>
        {
            public override ServiceDetails Details { get { return _details; } }
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

            return !isOptional ? new ParameterServiceInfo(parameter)
                : new WithDetails(parameter, !hasDefaultValue
                    ? ServiceDetails.IfUnresolvedReturnDefault
                    : ServiceDetails.Of(ifUnresolved: IfUnresolved.ReturnDefault, defaultValue: defaultValue));
        }

        /// <summary>Service type specified by <see cref="ParameterInfo.ParameterType"/>.</summary>
        public virtual Type ServiceType { get { return _parameter.ParameterType; } }

        /// <summary>Optional service details.</summary>
        public virtual ServiceDetails Details { get { return ServiceDetails.Default; } }

        /// <summary>Creates info from service type and details.</summary>
        /// <param name="serviceType">Required service type.</param> <param name="details">Optional details.</param> <returns>Create info.</returns>
        public IServiceInfo Create(Type serviceType, ServiceDetails details)
        {
            return serviceType == ServiceType
                ? new WithDetails(_parameter, details)
                : new TypeWithDetails(_parameter, serviceType, details);
        }

        /// <summary>Prints info to string using <see cref="ServiceInfoTools.Print"/>.</summary> <returns>Printed string.</returns>
        public override string ToString()
        {
            return new StringBuilder().Print(this).Append(" as parameter ").Print(_parameter.Name, "\"").ToString();
        }

        #region Implementation

        private readonly ParameterInfo _parameter;

        private ParameterServiceInfo(ParameterInfo parameter) { _parameter = parameter; }

        private class WithDetails : ParameterServiceInfo
        {
            public override ServiceDetails Details { get { return _details; } }
            public WithDetails(ParameterInfo parameter, ServiceDetails details)
                : base(parameter) { _details = details; }
            private readonly ServiceDetails _details;
        }

        private sealed class TypeWithDetails : WithDetails
        {
            public override Type ServiceType { get { return _serviceType; } }
            public TypeWithDetails(ParameterInfo parameter, Type serviceType, ServiceDetails details)
                : base(parameter, details) { _serviceType = serviceType; }
            private readonly Type _serviceType;
        }

        #endregion
    }

    /// <summary>Base class for property and field dependency info.</summary>
    public abstract class PropertyOrFieldServiceInfo : IServiceInfo
    {
        /// <summary>Create member info out of provide property or field.</summary>
        /// <param name="member">Member is either property or field.</param> <returns>Created info.</returns>
        public static PropertyOrFieldServiceInfo Of(MemberInfo member)
        {
            return member.ThrowIfNull() is PropertyInfo ? (PropertyOrFieldServiceInfo)
                new Property((PropertyInfo)member) : new Field((FieldInfo)member);
        }

        /// <summary>The required service type. It will be either <see cref="FieldInfo.FieldType"/> or <see cref="PropertyInfo.PropertyType"/>.</summary>
        public abstract Type ServiceType { get; }

        /// <summary>Optional details: service key, if-unresolved policy, required service type.</summary>
        public virtual ServiceDetails Details { get { return ServiceDetails.IfUnresolvedReturnDefault; } }

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
                    : base(property) { _details = details; }
                private readonly ServiceDetails _details;
            }

            private sealed class TypeWithDetails : WithDetails
            {
                public override Type ServiceType { get { return _serviceType; } }
                public TypeWithDetails(PropertyInfo property, Type serviceType, ServiceDetails details)
                    : base(property, details) { _serviceType = serviceType; }
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
                    : base(field) { _details = details; }
                private readonly ServiceDetails _details;
            }

            private sealed class TypeWithDetails : WithDetails
            {
                public override Type ServiceType { get { return _serviceType; } }
                public TypeWithDetails(FieldInfo field, Type serviceType, ServiceDetails details)
                    : base(field, details) { _serviceType = serviceType; }
                private readonly Type _serviceType;
            }
        }

        #endregion
    }

    /// <summary>Contains resolution stack with information about resolved service and factory for it,
    /// Additionally request contain weak reference to <see cref="IContainer"/>. That the all required information for resolving services.
    /// Request implements <see cref="IResolver"/> interface on top of provided container, which could be use by delegate factories.</summary>
    public sealed class Request
    {
        /// <summary>Creates empty request associated with provided <paramref name="container"/>.
        /// Every resolution will start from this request by pushing service information into, and then resolving it.</summary>
        /// <param name="container">Reference to container issued the request. Could be changed later with <see cref="SwitchContainer"/> method.</param>
        /// <returns>New empty request.</returns>
        public static Request CreateEmpty(ContainerWeakRef container)
        {
            return new Request(null, container, null, null);
        }

        /// <summary>Indicates that request is empty initial request: there is no <see cref="ServiceInfo"/> in such a request.</summary>
        public bool IsEmpty { get { return ServiceInfo == null; } }

        /// <summary>Previous request in dependency chain. It <see cref="IsEmpty"/> for resolution root.</summary>
        public readonly Request Parent;

        /// <summary>Requested service id info and commanded resolution behavior.</summary>
        public IServiceInfo ServiceInfo { get; private set; }

        /// <summary>Factory found in container to resolve this request.</summary>
        public readonly Factory ResolvedFactory;

        /// <summary>List of specified arguments to use instead of resolving them.</summary>
        public readonly KV<bool[], ParameterExpression[]> FuncArgs;

        /// <summary>Weak reference to container.</summary>
        public readonly ContainerWeakRef ContainerWeakRef;

        /// <summary>Provides access to container currently bound to request. 
        /// By default it is container initiated request by calling resolve method,
        /// but could be changed along the way: for instance when resolving from parent container.</summary>
        public IContainer Container { get { return ContainerWeakRef.GetTarget(); } }

        /// <summary>Shortcut access to <see cref="IServiceInfo.ServiceType"/>.</summary>
        public Type ServiceType { get { return ServiceInfo == null ? null : ServiceInfo.ServiceType; } }

        /// <summary>Shortcut access to <see cref="ServiceDetails.ServiceKey"/>.</summary>
        public object ServiceKey { get { return ServiceInfo.ThrowIfNull().Details.ServiceKey; } }

        /// <summary>Shortcut access to <see cref="ServiceDetails.IfUnresolved"/>.</summary>
        public IfUnresolved IfUnresolved { get { return ServiceInfo.ThrowIfNull().Details.IfUnresolved; } }

        /// <summary>Shortcut access to <see cref="ServiceDetails.RequiredServiceType"/>.</summary>
        public Type RequiredServiceType { get { return ServiceInfo.ThrowIfNull().Details.RequiredServiceType; } }

        /// <summary>Implementation type of factory, if request was <see cref="ResolveWithFactory"/> factory, or null otherwise.</summary>
        public Type ImplementationType { get { return ResolvedFactory == null ? null : ResolvedFactory.ImplementationType; } }

        /// <summary>Resolution scope.</summary>
        public readonly IScope Scope;

        /// <summary>Returns true if request originated from first Resolve call.</summary>
        public bool IsResolutionRoot { get { return Scope == null; }  }

        /// <summary>Creates new request with provided info, and attaches current request as new request parent.</summary>
        /// <param name="info">Info about service to resolve.</param> <param name="scope">(optional) Resolution scope.</param>
        /// <returns>New request for provided info.</returns>
        /// <remarks>Current request should be resolved to factory (<see cref="ResolveWithFactory"/>), before pushing info into it.</remarks>
        public Request Push(IServiceInfo info, IScope scope = null)
        {
            if (IsEmpty)
                return new Request(this, ContainerWeakRef, info.ThrowIfNull(), null, null, 
                    scope /* input scope provided only for first request when Resolve called */);

            ResolvedFactory.ThrowIfNull(Error.PUSHING_TO_REQUEST_WITHOUT_FACTORY, info.ThrowIfNull(), this);
            var inheritedInfo = info.InheritInfo(ServiceInfo, ResolvedFactory.Setup.FactoryType != FactoryType.Service);
            return new Request(this, ContainerWeakRef, inheritedInfo, null, FuncArgs, 
                Scope /* then scope is copied into dependency requests */);
        }

        /// <summary>Composes service description into <see cref="IServiceInfo"/> and calls Push.</summary>
        /// <param name="serviceType">Service type to resolve.</param>
        /// <param name="serviceKey">(optional) Service key to resolve.</param>
        /// <param name="ifUnresolved">(optional) Instructs how to handle unresolved service.</param>
        /// <param name="requiredServiceType">(optional) Registered/unwrapped service type to find.</param>
        /// <param name="scope">(optional) Resolution scope.</param>
        /// <returns>New request with provided info.</returns>
        public Request Push(Type serviceType,
            object serviceKey = null, IfUnresolved ifUnresolved = IfUnresolved.Throw, Type requiredServiceType = null,
            IScope scope = null)
        {
            var details = ServiceDetails.Of(requiredServiceType, serviceKey, ifUnresolved);
            return Push(DryIoc.ServiceInfo.Of(serviceType).WithDetails(details, this), scope ?? Scope);
        }

        /// <summary>Allow to switch current service info to new one: for instance it is used be decorators.</summary>
        /// <param name="getInfo">Gets new info to switch to.</param>
        /// <returns>New request with new info but the rest intact: e.g. <see cref="ResolvedFactory"/>.</returns>
        public Request UpdateServiceInfo(Func<IServiceInfo, IServiceInfo> getInfo)
        {
            return new Request(Parent, ContainerWeakRef, getInfo(ServiceInfo), ResolvedFactory, FuncArgs, Scope);
        }

        /// <summary>Sets service key to passed value. Required for multiple default services to change null key to
        /// actual <see cref="DefaultKey"/></summary>
        /// <param name="serviceKey">Key to set.</param>
        public void ChangeServiceKey(object serviceKey) // NOTE: May be removed in future versions. 
        {
            var details = ServiceInfo.Details;
            ServiceInfo = ServiceInfo.Create(ServiceInfo.ServiceType,
                ServiceDetails.Of(details.RequiredServiceType, serviceKey, details.IfUnresolved, details.DefaultValue));
        }

        /// <summary>Returns new request with parameter expressions created for <paramref name="funcType"/> input arguments.
        /// The expression is set to <see cref="FuncArgs"/> request field to use for <see cref="WrappersSupport.FuncTypes"/>
        /// resolution.</summary>
        /// <param name="funcType">Func type to get input arguments from.</param>
        /// <param name="funcArgPrefix">(optional) Unique prefix to help generate non-conflicting argument names.</param>
        /// <returns>New request with <see cref="FuncArgs"/> field set.</returns>
        public Request WithFuncArgs(Type funcType, string funcArgPrefix = null)
        {
            var funcArgs = funcType.ThrowIf(!funcType.IsFuncWithArgs()).GetGenericParamsAndArgs();
            var funcArgExprs = new ParameterExpression[funcArgs.Length - 1];

            for (var i = 0; i < funcArgExprs.Length; ++i)
            {
                var funcArg = funcArgs[i];
                var prefix = funcArgPrefix == null ? "_" : "_" + funcArgPrefix + "_";
                var funcArgName = prefix + funcArg.Name + i; // Valid non conflicting argument names for code generation
                funcArgExprs[i] = Expression.Parameter(funcArg, funcArgName);
            }

            var funcArgsUsage = new bool[funcArgExprs.Length];
            var funcArgsUsageAndExpr = new KV<bool[], ParameterExpression[]>(funcArgsUsage, funcArgExprs);
            return new Request(Parent, ContainerWeakRef, ServiceInfo, ResolvedFactory, funcArgsUsageAndExpr, Scope);
        }

        /// <summary>Changes container to passed one. Could be used by child container, 
        /// to switch child container to parent preserving the rest of request state.</summary>
        /// <param name="containerWeakRef">Reference to container to switch to.</param>
        /// <returns>Request with replaced container.</returns>
        public Request SwitchContainer(ContainerWeakRef containerWeakRef)
        {
            return new Request(Parent, containerWeakRef, ServiceInfo, ResolvedFactory, FuncArgs, Scope);
        }

        /// <summary>Returns new request with set <see cref="ResolvedFactory"/>.</summary>
        /// <param name="factory">Factory to which request is resolved.</param>
        /// <returns>New request with set factory.</returns>
        public Request ResolveWithFactory(Factory factory)
        {
            if (IsEmpty || (ResolvedFactory != null && ResolvedFactory.FactoryID == factory.FactoryID))
                return this; // resolving only once, no need to check recursion again.

            if (factory.FactoryType == FactoryType.Service)
                for (var p = Parent; !p.IsEmpty; p = p.Parent)
                    Throw.If(p.ResolvedFactory.FactoryID == factory.FactoryID,
                        Error.RECURSIVE_DEPENDENCY_DETECTED, Print(factory.FactoryID));

            return new Request(Parent, ContainerWeakRef, ServiceInfo, factory, FuncArgs, Scope);
        }

        /// <summary>Searches parent request stack upward and returns closest parent of <see cref="FactoryType.Service"/>.
        /// If not found returns <see cref="IsEmpty"/> request.</summary> <returns>Found parent or <see cref="IsEmpty"/> request.</returns>
        public Request ParentNonWrapper()
        {
            var p = Parent;
            while (!p.IsEmpty && p.ResolvedFactory.FactoryType == FactoryType.Wrapper)
                p = p.Parent;
            return p;
        }

        /// <summary>Searches parent request stack upward and returns closest parent of <see cref="FactoryType.Service"/>.
        /// If not found returns <see cref="IsEmpty"/> request.</summary>
        /// <param name="condition">Condition, e.g. to find root request condition may be: <code lang="cs"><![CDATA[p => p.Parent.IsEmpty]]></code></param>
        /// <returns>Found parent or empty request.</returns>
        public Request ParentNonWrapper(Func<Request, bool> condition)
        {
            var p = Parent;
            while (!p.IsEmpty && (p.ResolvedFactory.FactoryType == FactoryType.Wrapper || !condition(p)))
                p = p.Parent;
            return p;
        }

        /// <summary>Enumerates all request stack parents. Last returned will <see cref="IsEmpty"/> empty parent.</summary>
        /// <returns>Unfolding parents.</returns>
        public IEnumerable<Request> Enumerate()
        {
            for (var r = this; !r.IsEmpty; r = r.Parent)
                yield return r;
        }

        /// <summary>Prints current request info only (no parents printed) to provided builder.</summary>
        /// <param name="s">Builder to print too.</param>
        /// <returns>(optional) Builder to appended info to, or new builder if not specified.</returns>
        public StringBuilder PrintCurrent(StringBuilder s = null)
        {
            s = s ?? new StringBuilder();
            if (IsEmpty) return s.Append("<empty>");
            if (ResolvedFactory != null && ResolvedFactory.FactoryType != FactoryType.Service)
                s.Append(ResolvedFactory.FactoryType.ToString().ToLower()).Append(' ');
            if (ImplementationType != null && ImplementationType != ServiceType)
                s.Print(ImplementationType).Append(": ");
            return s.Append(ServiceInfo);
        }

        /// <summary>Prints full stack of requests starting from current one using <see cref="PrintCurrent"/>.</summary>
        /// <param name="recursiveFactoryID">Flag specifying that in case of found recursion/repetition of requests, 
        /// mark repeated requests.</param>
        /// <returns>Builder with appended request stack info.</returns>
        public StringBuilder Print(int recursiveFactoryID = -1)
        {
            var s = PrintCurrent(new StringBuilder());
            if (Parent == null)
                return s;

            s = recursiveFactoryID == -1 ? s : s.Append(" <--recursive");
            return Parent.Enumerate().Aggregate(s, (a, r) =>
            {
                a = r.PrintCurrent(a.AppendLine().Append(" in "));
                return r.ResolvedFactory.FactoryID == recursiveFactoryID ? a.Append(" <--recursive") : a;
            });
        }

        /// <summary>Print while request stack info to string using <seealso cref="Print"/>.</summary>
        /// <returns>String with info.</returns>
        public override string ToString()
        {
            return Print().ToString();
        }

        #region Implementation

        internal Request(Request parent, ContainerWeakRef containerWeakRef, IServiceInfo serviceInfo, Factory resolvedFactory,
            KV<bool[], ParameterExpression[]> funcArgs = null, IScope scope = null)
        {
            Parent = parent;
            ContainerWeakRef = containerWeakRef;
            ServiceInfo = serviceInfo;
            ResolvedFactory = resolvedFactory;
            FuncArgs = funcArgs;
            Scope = scope;
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
        public virtual Func<Request, bool> Condition { get; private set; }

        /// <summary>Set to true allows to cache and use cached factored service expression.</summary>
        public virtual bool CacheFactoryExpression { get { return false; } }

        /// <summary>Arbitrary metadata object associated with Factory/Implementation.</summary>
        public virtual object Metadata { get { return null; } }
        
        /// <summary>Indicates that injected expression should be: 
        /// <code lang="cs"><![CDATA[r.Resolver.Resolve<IDependency>(...)]]></code>
        /// instead of: <code lang="cs"><![CDATA[new Dependency(...)]]></code>.</summary>
        public virtual bool OpenResolutionScope { get { return false; } }

        /// <summary>Specifies how to wrap the reused/shared instance to apply additional behavior, e.g. <see cref="WeakReference"/>, 
        /// or disable disposing with <see cref="ReuseHiddenDisposable"/>, etc.</summary>
        public virtual Type[] ReuseWrappers { get { return null; } }

        /// <summary>Adds new condition to setup using logical AND with old condition.</summary>
        /// <param name="condition">Condition based on service request.</param> <returns>Setup with condition.</returns>
        public Setup WithCondition(Func<Request, bool> condition)
        {
            condition.ThrowIfNull();
            var oldCondition = Condition;
            var setup = (Setup)MemberwiseClone();
            setup.Condition = oldCondition == null ? condition 
                : (request => condition(request) && oldCondition(request));
            return setup;
        }

        /// <summary>Default setup for service factories.</summary>
        public static readonly Setup Default = new ServiceSetup();

        /// <summary>Constructs setup object out of specified settings. If all settings are default then <see cref="Setup.Default"/> setup will be returned.</summary>
        /// <param name="cacheFactoryExpression">(optional)</param> <param name="lazyMetadata">(optional)</param> 
        /// <param name="metadata">(optional) Overrides <paramref name="lazyMetadata"/></param> <param name="condition">(optional)</param>
        /// <param name="openResolutionScope">(optional) If true dependency expression will be "r.Resolve(...)" instead of inline expression.</param>
        /// <param name="reuseWrappers">(optional) Multiple reuse wrappers.</param>       
        /// <returns>New setup object or <see cref="Setup.Default"/>.</returns>
        public static Setup With(bool cacheFactoryExpression = true,
            Func<object> lazyMetadata = null, object metadata = null,
            Func<Request, bool> condition = null, bool openResolutionScope = false,
            params Type[] reuseWrappers)
        {
            if (cacheFactoryExpression && lazyMetadata == null && metadata == null &&
                condition == null && openResolutionScope == false && reuseWrappers == null)
                return Default;

            if (reuseWrappers != null && reuseWrappers.Length != 0)
                for (var i = 0; i < reuseWrappers.Length; ++i)
                    typeof(IReuseWrapper).ThrowIfNotOf(reuseWrappers[i], Error.REG_REUSED_OBJ_WRAPPER_IS_NOT_IREUSED, i, reuseWrappers);

            return new ServiceSetup(cacheFactoryExpression, lazyMetadata, metadata, condition, openResolutionScope, reuseWrappers);
        }

        /// <summary>Default setup which will look for wrapped service type as single generic parameter.</summary>
        public static readonly Setup Wrapper = new WrapperSetup();

        /// <summary>Creates setup with all settings specified. If all is omitted: then <see cref="Setup.Wrapper"/> will be used.</summary>
        /// <param name="unwrapServiceType">Wrapped service selector rule.</param> <param name="reuseWrapperFactory"></param>
        /// <returns>New setup with non-default settings or <see cref="Setup.Wrapper"/> otherwise.</returns>
        public static Setup WrapperWith(Func<Type, Type> unwrapServiceType = null, IReuseWrapperFactory reuseWrapperFactory = null)
        {
            return unwrapServiceType == null && reuseWrapperFactory == null
                ? Wrapper : new WrapperSetup(unwrapServiceType, reuseWrapperFactory);
        }

        /// <summary>Default decorator setup: decorator is applied to service type it registered with.</summary>
        public static readonly Setup Decorator = new DecoratorSetup();

        /// <summary>Creates setup with optional condition.</summary>
        /// <param name="condition">(optional)</param> <returns>New setup with condition or <see cref="Setup.Decorator"/>.</returns>
        public static Setup DecoratorWith(Func<Request, bool> condition = null)
        {
            return condition == null ? Decorator : new DecoratorSetup(condition);
        }

        private sealed class ServiceSetup : Setup
        {
            public override FactoryType FactoryType { get { return FactoryType.Service; } }
            
            public override bool CacheFactoryExpression { get { return _cacheFactoryExpression; } }

            public override object Metadata { get { return _metadata ?? (_metadata = _lazyMetadata == null ? null : _lazyMetadata()); }}

            public override bool OpenResolutionScope { get { return _openResolutionScope; } }

            public override Type[] ReuseWrappers { get { return _reuseWrappers; } }

            public ServiceSetup(bool cacheFactoryExpression = true,
                Func<object> lazyMetadata = null, object metadata = null,
                Func<Request, bool> condition = null, bool openResolutionScope = false,
                Type[] reuseWrappers = null)
            {
                _cacheFactoryExpression = cacheFactoryExpression;
                _lazyMetadata = lazyMetadata;
                _metadata = metadata;
                Condition = condition;
                _openResolutionScope = openResolutionScope;
                _reuseWrappers = reuseWrappers;
            }

            private readonly bool _cacheFactoryExpression;
            private readonly Func<object> _lazyMetadata;
            private object _metadata;
            private readonly bool _openResolutionScope;
            private readonly Type[] _reuseWrappers;
        }

        /// <summary>Setup for <see cref="DryIoc.FactoryType.Wrapper"/> factory.</summary>
        public sealed class WrapperSetup : Setup
        {
            /// <summary>Returns <see cref="DryIoc.FactoryType.Wrapper"/> type.</summary>
            public override FactoryType FactoryType { get { return FactoryType.Wrapper; } }

            /// <summary>(optional) Tool for wrapping and unwrapping reused object.</summary>
            public readonly IReuseWrapperFactory ReuseWrapperFactory;

            /// <summary>Delegate to get wrapped type from provided wrapper type. 
            /// If wrapper is generic, then wrapped type is usually a generic parameter.</summary>
            public readonly Func<Type, Type> UnwrapServiceType;

            /// <summary>Constructs wrapper setup from optional wrapped type selector and reuse wrapper factory.</summary>
            /// <param name="getWrappedServiceType">(optional)</param> <param name="reuseWrapperFactory">(optional)</param>
            public WrapperSetup(Func<Type, Type> getWrappedServiceType = null, IReuseWrapperFactory reuseWrapperFactory = null)
            {
                UnwrapServiceType = getWrappedServiceType ?? GetSingleGenericArgByDefault;
                ReuseWrapperFactory = reuseWrapperFactory;
            }

            private static Type GetSingleGenericArgByDefault(Type wrapperType)
            {
                wrapperType.ThrowIf(!wrapperType.IsClosedGeneric(), Error.NO_WRAPPED_TYPE_FOR_NON_GENERIC_WRAPPER);
                var typeArgs = wrapperType.GetGenericParamsAndArgs();
                Throw.If(typeArgs.Length != 1, Error.WRAPPER_CAN_WRAP_SINGLE_SERVICE_ONLY, wrapperType);
                return typeArgs[0];
            }
        }

        private sealed class DecoratorSetup : Setup
        {
            public override FactoryType FactoryType { get { return FactoryType.Decorator; } }

            public DecoratorSetup(Func<Request, bool> condition = null)
            {
                Condition = condition;
            }
        }
    }

    /// <summary>Facility for creating concrete factories from some template/prototype. Example: 
    /// creating closed-generic type reflection factory from registered open-generic prototype factory.</summary>
    public interface IConcreteFactoryProvider
    {
        /// <summary>Returns factories created by <see cref="ProvideConcreteFactory"/> so far.</summary>
        IEnumerable<KV<Type, object>> ProvidedFactoriesServiceTypeKey { get; }

        /// <summary>Method applied for factory provider, returns new factory per request.</summary>
        /// <param name="request">Request to resolve.</param> <returns>Returns new factory per request.</returns>
        Factory ProvideConcreteFactory(Request request);
    }

    /// <summary>Base class for different ways to instantiate service: 
    /// <list type="bullet">
    /// <item>Through reflection - <see cref="ReflectionFactory"/></item>
    /// <item>Using custom delegate - <see cref="DelegateFactory"/></item>
    /// <item>Using custom expression - <see cref="ExpressionFactory"/></item>
    /// </list>
    /// For all of the types Factory should provide result as <see cref="Expression"/> and <see cref="FactoryDelegate"/>.
    /// Factories are supposed to be immutable and stateless.
    /// Each created factory has an unique ID set in <see cref="FactoryID"/>.</summary>
    public abstract class Factory
    {
        /// <summary>Unique factory id generated from static seed.</summary>
        public int FactoryID { get; internal set; }

        /// <summary>Reuse policy for factory created services.</summary>
        public readonly IReuse Reuse;

        /// <summary>Setup may contain different/non-default factory settings.</summary>
        public Setup Setup
        {
            get { return _setup; }
            protected internal set { _setup = value ?? Setup.Default; }
        }

        /// <summary>Shortcut for <see cref="DryIoc.Setup.FactoryType"/>.</summary>
        public FactoryType FactoryType
        {
            get { return Setup.FactoryType; }
        }

        /// <summary>Non-abstract closed implementation type. May be null if not known beforehand, e.g. in <see cref="DelegateFactory"/>.</summary>
        public virtual Type ImplementationType { get { return null; } }

        /// <summary>Indicates that Factory is factory provider and 
        /// consumer should call <see cref="IConcreteFactoryProvider.ProvideConcreteFactory"/>  to get concrete factory.</summary>
        public virtual IConcreteFactoryProvider Provider { get { return null; } }

        /// <summary>Initializes reuse and setup. Sets the <see cref="FactoryID"/></summary>
        /// <param name="reuse">(optional)</param>
        /// <param name="setup">(optional)</param>
        protected Factory(IReuse reuse = null, Setup setup = null)
        {
            FactoryID = Interlocked.Increment(ref _lastFactoryID);
            Reuse = reuse;
            Setup = setup ?? Setup.Default;

            if (reuse != null)
                Setup = Setup.WithCondition(HasMatchingReuseScope);
        }

        private bool HasMatchingReuseScope(Request request)
        {
            var rules = request.Container.Rules;
            if (!rules.ImplicitSetupConditionToCheckReuseMatchingScope)
                return true;

            var reuseMapping = rules.ReuseMapping;
            var reuse = reuseMapping == null ? Reuse : reuseMapping(Reuse, request);

            if (reuse is ResolutionScopeReuse)
                return reuse.GetScopeOrDefault(request) != null;

            if (reuse is CurrentScopeReuse)
                return request.Parent.Enumerate().Any(r => r.ServiceType.IsFunc())
                    || reuse.GetScopeOrDefault(request) != null;

            return true;
        }

        /// <summary>Validates that factory is OK for registered service type.</summary>
        /// <param name="container">Container to register factory in.</param>
        /// <param name="serviceType">Service type to register factory for.</param>
        /// <param name="serviceKey">Service key to register factory with.</param>
        public virtual void CheckBeforeRegistration(IContainer container, Type serviceType, object serviceKey)
        {
            Throw.If(serviceType.IsGenericDefinition() && Provider == null,
                Error.REGISTERING_OPEN_GENERIC_REQUIRES_FACTORY_PROVIDER, serviceType);
        }

        /// <summary>The main factory method to create service expression, e.g. "new Client(new Service())".
        /// If <paramref name="request"/> has <see cref="Request.FuncArgs"/> specified, they could be used in expression.</summary>
        /// <param name="request">Service request.</param>
        /// <returns>Created expression.</returns>
        public abstract Expression CreateExpressionOrDefault(Request request);

        /// <summary>Returns service expression: either by creating it with <see cref="CreateExpressionOrDefault"/> or taking expression from cache.
        /// Before returning method may transform the expression  by applying <see cref="Reuse"/>, or/and decorators if found any.
        /// If <paramref name="reuseWrapperType"/> specified: result expression may be of required wrapper type.</summary>
        /// <param name="request">Request for service.</param>
        /// <param name="reuseWrapperType">(optional) Reuse wrapper type of expression to return.</param>
        /// <returns>Service expression.</returns>
        public virtual Expression GetExpressionOrDefault(Request request, Type reuseWrapperType = null)
        {
            // return r.Resolver.Resolve<DependencyServiceType>(...) instead of actual new or method call expression.
            if (Setup.OpenResolutionScope && !request.ParentNonWrapper().IsEmpty)
                return Resolver.CreateResolutionExpression(request);

            request = request.ResolveWithFactory(this);

            var reuseMappingRule = request.Container.Rules.ReuseMapping;
            var reuse = reuseMappingRule == null ? Reuse : reuseMappingRule(Reuse, request);
            ThrowIfReuseHasShorterLifespanThanParent(reuse, request);

            var decorator = request.Container.GetDecoratorExpressionOrDefault(request);
            var noOrFuncDecorator = decorator == null || decorator is LambdaExpression;

            var isCacheable = Setup.CacheFactoryExpression && noOrFuncDecorator
                && request.FuncArgs == null && reuseWrapperType == null;
            if (isCacheable)
            {
                var cachedServiceExpr = request.Container.GetCachedFactoryExpressionOrDefault(FactoryID);
                if (cachedServiceExpr != null)
                    return decorator == null ? cachedServiceExpr : Expression.Invoke(decorator, cachedServiceExpr);
            }

            var serviceExpr = noOrFuncDecorator ? CreateExpressionOrDefault(request) : decorator;
            if (serviceExpr != null && reuse != null)
            {
                // When singleton scope, and no Func in request chain, and no renewable wrapper used,
                // then reused instance could be directly inserted into delegate instead of lazy requested from Scope.
                var canBeInstantiated = reuse is SingletonReuse && request.Container.Rules.SingletonOptimization
                    && (request.Parent.IsEmpty || !request.Parent.Enumerate().Any(r => r.ServiceType.IsFunc()))
                    && Setup.ReuseWrappers.IndexOf(t => t.IsAssignableTo(typeof(IRecyclable))) == -1;

                serviceExpr = canBeInstantiated
                    ? GetInstantiatedSingletonExpressionOrDefault(serviceExpr, request, reuseWrapperType)
                    : GetScopedExpressionOrDefault(serviceExpr, reuse, request, reuseWrapperType);
            }

            if (serviceExpr != null)
            {
                if (isCacheable)
                    request.Container.CacheFactoryExpression(FactoryID, serviceExpr);

                if (noOrFuncDecorator && decorator != null)
                    serviceExpr = Expression.Invoke(decorator, serviceExpr);
            }

            if (serviceExpr == null)
                Throw.If(request.IfUnresolved == IfUnresolved.Throw, Error.UNABLE_TO_RESOLVE_SERVICE, request);

            return serviceExpr;
        }

        /// <summary>Check method name for explanation XD.</summary> <param name="reuse">Reuse to check.</param> <param name="request">Request to resolve.</param>
        protected static void ThrowIfReuseHasShorterLifespanThanParent(IReuse reuse, Request request)
        {
            if (reuse != null && !request.Parent.IsEmpty &&
                request.Container.Rules.ThrowIfDependencyHasShorterReuseLifespan)
            {
                var parentReuse = request.Parent.ResolvedFactory.Reuse;
                if (parentReuse != null)
                    Throw.If(reuse.Lifespan < parentReuse.Lifespan,
                        Error.DEPENDENCY_HAS_SHORTER_REUSE_LIFESPAN, request.PrintCurrent(), request.Parent, reuse, parentReuse);
            }
        }

        /// <summary>Creates factory delegate from service expression and returns it. By default uses <see cref="FactoryCompiler"/>
        /// to compile delegate from expression but could be overridden by concrete factory type: e.g. <see cref="DelegateFactory"/></summary>
        /// <param name="request">Service request.</param>
        /// <returns>Factory delegate created from service expression.</returns>
        public virtual FactoryDelegate GetDelegateOrDefault(Request request)
        {
            var expression = GetExpressionOrDefault(request);
            return expression == null ? null : expression.CompileToDelegate(request.Container.Rules);
        }

        /// <summary>Returns nice string representation of factory.</summary>
        /// <returns>String representation.</returns>
        public override string ToString()
        {
            var s = new StringBuilder();
            s.Append("{FactoryID=").Append(FactoryID);
            if (ImplementationType != null)
                s.Append(", ImplType=").Print(ImplementationType);
            if (Reuse != null)
                s.Append(", ReuseType=").Print(Reuse.GetType());
            if (Setup.FactoryType != Setup.Default.FactoryType)
                s.Append(", FactoryType=").Append(Setup.FactoryType);
            return s.Append("}").ToString();
        }

        #region Implementation

        private static int _lastFactoryID;
        private Setup _setup;

        private Expression GetScopedExpressionOrDefault(Expression serviceExpr, IReuse reuse, Request request, Type requiredWrapperType = null)
        {
            var getScopeExpr = reuse.GetScopeExpression(request);

            var serviceType = serviceExpr.Type;
            var factoryIDExpr = Expression.Constant(FactoryID);

            var wrapperTypes = Setup.ReuseWrappers;

            if (wrapperTypes.IsNullOrEmpty())
                return Expression.Convert(Expression.Call(getScopeExpr, "GetOrAdd", null,
                    factoryIDExpr, Expression.Lambda<CreateScopedValue>(serviceExpr, null)), serviceType);

            // First wrap serviceExpr with wrapper Wrap method.
            var wrappers = new IReuseWrapperFactory[wrapperTypes.Length];
            for (var i = 0; i < wrapperTypes.Length; ++i)
            {
                var wrapperType = wrapperTypes[i];
                var wrapperFactory = request.Container.GetWrapperFactoryOrDefault(wrapperType).ThrowIfNull();
                var wrapper = ((Setup.WrapperSetup)wrapperFactory.Setup).ReuseWrapperFactory;

                serviceExpr = Expression.Call(
                    request.Container.GetOrAddStateItemExpression(wrapper, typeof(IReuseWrapperFactory)),
                    "Wrap", null, serviceExpr);

                wrappers[i] = wrapper; // save wrapper for later unwrap
            }

            // Makes call like this: scope.GetOrAdd(id, () => wrapper1.Wrap(wrapper0.Wrap(new Service)))
            var getScopedServiceExpr = Expression.Call(getScopeExpr, "GetOrAdd", null, 
                factoryIDExpr, Expression.Lambda<CreateScopedValue>(serviceExpr, null));

            // Unwrap wrapped service in backward order like this: wrapper0.Unwrap(wrapper1.Unwrap(scope.GetOrAdd(...)))
            for (var i = wrapperTypes.Length - 1; i >= 0; --i)
            {
                var wrapperType = wrapperTypes[i];

                // Stop on required wrapper type, if provided.
                if (requiredWrapperType != null && requiredWrapperType == wrapperType)
                    return Expression.Convert(getScopedServiceExpr, requiredWrapperType);

                var wrapperExpr = request.Container.GetOrAddStateItemExpression(wrappers[i], typeof(IReuseWrapperFactory));
                getScopedServiceExpr = Expression.Call(wrapperExpr, "Unwrap", null, getScopedServiceExpr);
            }

            return requiredWrapperType != null ? null
                : Expression.Convert(getScopedServiceExpr, serviceType);
        }

        private Expression GetInstantiatedSingletonExpressionOrDefault(Expression serviceExpr, Request request, Type requiredWrapperType = null)
        {
            var factoryDelegate = serviceExpr.CompileToDelegate(request.Container.Rules);
            var scope = request.Container.SingletonScope;

            var wrapperTypes = Setup.ReuseWrappers;
            var serviceType = serviceExpr.Type;
            if (wrapperTypes == null || wrapperTypes.Length == 0)
                return request.Container.GetOrAddStateItemExpression(
                    scope.GetOrAdd(FactoryID, () => factoryDelegate(request.Container.ResolutionStateCache, request.ContainerWeakRef, null)),
                    serviceType);

            var wrappers = new IReuseWrapperFactory[wrapperTypes.Length];
            for (var i = 0; i < wrapperTypes.Length; ++i)
            {
                var wrapperType = wrapperTypes[i];
                var wrapperFactory = request.Container.GetWrapperFactoryOrDefault(wrapperType).ThrowIfNull();
                var wrapper = ((Setup.WrapperSetup)wrapperFactory.Setup).ReuseWrapperFactory;
                var serviceFactory = factoryDelegate;
                factoryDelegate = (st, cs, rs) => wrapper.Wrap(serviceFactory(st, cs, rs));
                wrappers[i] = wrapper;
            }

            var wrappedService = scope.GetOrAdd(FactoryID,
                () => factoryDelegate(request.Container.ResolutionStateCache, request.ContainerWeakRef, null));

            for (var i = wrapperTypes.Length - 1; i >= 0; --i)
            {
                var wrapperType = wrapperTypes[i];
                if (requiredWrapperType == wrapperType)
                    return request.Container.GetOrAddStateItemExpression(wrappedService, requiredWrapperType);
                wrappedService = wrappers[i].Unwrap(wrappedService);
            }

            return requiredWrapperType != null ? null
                : request.Container.GetOrAddStateItemExpression(wrappedService, serviceType);
        }

        #endregion
    }

    /// <summary>Declares delegate to get single factory method or constructor for resolved request.</summary>
    /// <param name="request">Request to resolve.</param>
    /// <returns>Factory method wrapper over constructor or method.</returns>
    public delegate FactoryMethod FactoryMethodSelector(Request request);

    /// <summary>Specifies how to get parameter info for injected parameter and resolved request</summary>
    /// <remarks>Request is for parameter method owner not for parameter itself.</remarks>
    /// <param name="request">Request for parameter method/constructor owner.</param>
    /// <returns>Service info describing how to inject parameter.</returns>
    public delegate Func<ParameterInfo, ParameterServiceInfo> ParameterSelector(Request request);

    /// <summary>Specifies what properties or fields to inject and how.</summary>
    /// <param name="request">Request for property/field owner.</param>
    /// <returns>Corresponding service info for each property/field to be injected.</returns>
    public delegate IEnumerable<PropertyOrFieldServiceInfo> PropertiesAndFieldsSelector(Request request);

    /// <summary>DSL for specifying <see cref="ParameterSelector"/> injection rules.</summary>
    public static partial class Parameters
    {
        /// <summary>Specifies to return default details <see cref="ServiceDetails.Default"/> for all parameters.</summary>
        public static ParameterSelector Of = request => ParameterServiceInfo.Of;

        /// <summary>Combines source selector with other. Other will override the source.</summary>
        /// <param name="source">Source selector.</param> <param name="other">Specific other selector to add.</param>
        /// <returns>Combined result selector.</returns>
        public static ParameterSelector And(this ParameterSelector source, ParameterSelector other)
        {
            return source == null || source == Of ? other ?? Of
                : other == null || other == Of ? source
                : request => other(request) ?? source(request);
        }

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
                return details == null ? source(request)(parameter) 
                    : ParameterServiceInfo.Of(parameter).WithDetails(details, request);
            };
        }

        /// <summary>Adds to <paramref name="source"/> selector service info for parameter identified by <paramref name="name"/>.</summary>
        /// <param name="source">Original parameters rules.</param> <param name="name">Name to identify parameter.</param>
        /// <param name="requiredServiceType">(optional)</param> <param name="serviceKey">(optional)</param>
        /// <param name="ifUnresolved">(optional) By default throws exception if unresolved.</param>
        /// <param name="defaultValue">(optional) Specifies default value to use when unresolved.</param>
        /// <returns>New parameters rules.</returns>
        public static ParameterSelector Name(this ParameterSelector source, string name,
            Type requiredServiceType = null, object serviceKey = null, IfUnresolved ifUnresolved = IfUnresolved.Throw, object defaultValue = null)
        {
            return source.Details((r, p) => !p.Name.Equals(name) ? null 
                : ServiceDetails.Of(requiredServiceType, serviceKey, ifUnresolved, defaultValue));
        }

        /// <summary>Specify parameter by name and set custom value to it.</summary>
        /// <param name="source">Original parameters rules.</param> <param name="name">Parameter name.</param>
        /// <param name="getCustomValue">Custom value provider.</param>
        /// <returns>New parameters rules.</returns>
        public static ParameterSelector Name(this ParameterSelector source, string name, Func<Request, object> getCustomValue)
        {
            return source.Details((r, p) => p.Name.Equals(name) ? ServiceDetails.Of(getCustomValue(r)) : null);
        }

        /// <summary>Adds to <paramref name="source"/> selector service info for parameter identified by type <typeparamref name="T"/>.</summary>
        /// <typeparam name="T">Type of parameter.</typeparam> <param name="source">Source selector.</param> 
        /// <param name="requiredServiceType">(optional)</param> <param name="serviceKey">(optional)</param>
        /// <param name="ifUnresolved">(optional) By default throws exception if unresolved.</param>
        /// <param name="defaultValue">(optional) Specifies default value to use when unresolved.</param>
        /// <returns>Combined selector.</returns>
        public static ParameterSelector Type<T>(this ParameterSelector source,
            Type requiredServiceType = null, object serviceKey = null, IfUnresolved ifUnresolved = IfUnresolved.Throw, object defaultValue = null)
        {
            return source.Details((r, p) => !typeof(T).IsAssignableTo(p.ParameterType) ? null
                : ServiceDetails.Of(requiredServiceType, serviceKey, ifUnresolved, defaultValue));
        }
    }

    /// <summary>DSL for specifying <see cref="PropertiesAndFieldsSelector"/> injection rules.</summary>
    public static partial class PropertiesAndFields
    {
        /// <summary>Say to not resolve any properties or fields.</summary>
        public static PropertiesAndFieldsSelector Of = request => null;

        /// <summary>Public assignable instance members of any type except object, string, primitives types, and arrays of those.</summary>
        public static PropertiesAndFieldsSelector Auto = All(false, false);

        /// <summary>Should return service info for input member (property or field).</summary>
        /// <param name="member">Input member.</param> <param name="request">Request to provide context.</param> <returns>Service info.</returns>
        public delegate PropertyOrFieldServiceInfo GetInfo(MemberInfo member, Request request);

        /// <summary>Generates selector property and field selector with settings specified by parameters.
        /// If all parameters are omitted the return all public not primitive members.</summary>
        /// <param name="withNonPublic">(optional) Specifies to include non public members. Will include by default.</param>
        /// <param name="withPrimitive">(optional) Specifies to include members of primitive types. Will include by default.</param>
        /// <param name="withFields">(optional) Specifies to include fields as well as properties. Will include by default.</param>
        /// <param name="ifUnresolved">(optional) Defines ifUnresolved behavior for resolved members.</param>
        /// <param name="withInfo">(optional) Return service info for a member or null to skip member resolution.</param>
        /// <returns>Result selector composed using provided settings.</returns>
        public static PropertiesAndFieldsSelector All(
            bool withNonPublic = true, bool withPrimitive = true, bool withFields = true,
            IfUnresolved ifUnresolved = IfUnresolved.ReturnDefault,
            GetInfo withInfo = null)
        {
            GetInfo getInfo = (m, r) => withInfo != null ? withInfo(m, r) :
                  PropertyOrFieldServiceInfo.Of(m).WithDetails(ServiceDetails.Of(ifUnresolved: ifUnresolved), r);
            return r =>
            {
                var properties = r.ImplementationType.GetAll(_ => _.DeclaredProperties)
                    .Where(p => p.Match(withNonPublic, withPrimitive))
                    .Select(m => getInfo(m, r));
                return !withFields ? properties :
                    properties.Concat(r.ImplementationType.GetAll(_ => _.DeclaredFields)
                    .Where(f => f.Match(withNonPublic, withPrimitive))
                    .Select(m => getInfo(m, r)));
            };
        }

        /// <summary>Combines source properties and fields with other. Other will override the source condition.</summary>
        /// <param name="source">Source selector.</param> <param name="other">Specific other selector to add.</param>
        /// <returns>Combined result selector.</returns>
        public static PropertiesAndFieldsSelector And(this PropertiesAndFieldsSelector source, PropertiesAndFieldsSelector other)
        {
            return source == null || source == Of ? (other ?? Of)
                : other == null || other == Of ? source
                : r =>
                {
                    var sourceMembers = source(r).ToArrayOrSelf();
                    var otherMembers = other(r).ToArrayOrSelf();
                    return sourceMembers == null || sourceMembers.Length == 0 ? otherMembers
                        : otherMembers == null || otherMembers.Length == 0 ? sourceMembers
                        : sourceMembers
                            .Where(info => info != null && otherMembers.All(o => o == null || !info.Member.Name.Equals(o.Member.Name)))
                            .Concat(otherMembers);
                };
        }

        /// <summary>Specifies service details (key, if-unresolved policy, required type) for property/field with the name.</summary>
        /// <param name="source">Original member selector.</param> <param name="name">Member name.</param> <param name="getDetails">Details.</param>
        /// <returns>new selector.</returns>
        public static PropertiesAndFieldsSelector Details(this PropertiesAndFieldsSelector source, string name, Func<Request, ServiceDetails> getDetails)
        {
            name.ThrowIfNull();
            getDetails.ThrowIfNull();
            return source.And(request =>
            {
                var implementationType = request.ImplementationType;

                var property = implementationType.GetPropertyOrNull(name);
                if (property != null && property.Match(true, true))
                {
                    var details = getDetails(request);
                    return details == null ? null
                        : new[] { PropertyOrFieldServiceInfo.Of(property).WithDetails(details, request) };
                }

                var field = implementationType.GetFieldOrNull(name);
                if (field != null && field.Match(true, true))
                {
                    var details = getDetails(request);
                    return details == null ? null 
                        : new[] { PropertyOrFieldServiceInfo.Of(field).WithDetails(details, request) };
                }
                    
                return Throw.For<IEnumerable<PropertyOrFieldServiceInfo>>(Error.NOT_FOUND_SPECIFIED_WRITEABLE_PROPERTY_OR_FIELD, name, request);
            });
        }

        /// <summary>Adds to <paramref name="source"/> selector service info for property/field identified by <paramref name="name"/>.</summary>
        /// <param name="source">Source selector.</param> <param name="name">Name to identify member.</param>
        /// <param name="requiredServiceType">(optional)</param> <param name="serviceKey">(optional)</param>
        /// <param name="ifUnresolved">(optional) By default returns default value if unresolved.</param>
        /// <param name="defaultValue">(optional) Specifies default value to use when unresolved.</param>
        /// <returns>Combined selector.</returns>
        public static PropertiesAndFieldsSelector Name(this PropertiesAndFieldsSelector source, string name,
            Type requiredServiceType = null, object serviceKey = null, IfUnresolved ifUnresolved = IfUnresolved.ReturnDefault, object defaultValue = null)
        {
            return source.Details(name, r => ServiceDetails.Of(requiredServiceType, serviceKey, ifUnresolved, defaultValue));
        }

        /// <summary>Specifies custom value for property/field with specific name.</summary>
        /// <param name="source">Original property/field list.</param>
        /// <param name="name">Target member name.</param> <param name="getCustomValue">Custom value provider.</param>
        /// <returns>Return new combined selector.</returns>
        public static PropertiesAndFieldsSelector Name(this PropertiesAndFieldsSelector source, string name, Func<Request, object> getCustomValue)
        {
            return source.Details(name, r => ServiceDetails.Of(getCustomValue(r)));
        }

        /// <summary>Returns true if property matches flags provided.</summary>
        /// <param name="property">Property to match</param>
        /// <param name="withNonPublic">Says to include non public properties.</param>
        /// <param name="withPrimitive">Says to include properties of primitive type.</param>
        /// <returns>True if property is matched and false otherwise.</returns>
        public static bool Match(this PropertyInfo property, bool withNonPublic = false, bool withPrimitive = false)
        {
            return property.CanWrite && !property.IsIndexer() // first checks that property is assignable in general and not indexer
                && (withNonPublic || property.IsPublic())
                && (withPrimitive || !property.PropertyType.IsPrimitive(orArrayOfPrimitives: true));
        }

        /// <summary>Returns true if field matches flags provided.</summary>
        /// <param name="field">Field to match.</param>
        /// <param name="withNonPublic">Says to include non public fields.</param>
        /// <param name="withPrimitive">Says to include fields of primitive type.</param>
        /// <returns>True if property is matched and false otherwise.</returns>
        public static bool Match(this FieldInfo field, bool withNonPublic = false, bool withPrimitive = false)
        {
            return !field.IsInitOnly && !field.IsBackingField()
                && (withNonPublic || field.IsPublic)
                && (withPrimitive || !field.FieldType.IsPrimitive(orArrayOfPrimitives: true));
        }
    }

    /// <summary>Reflects on <see cref="ImplementationType"/> constructor parameters and members,
    /// creates expression for each reflected dependency, and composes result service expression.</summary>
    public sealed class ReflectionFactory : Factory
    {
        /// <summary>Non-abstract service implementation type. May be open generic.</summary>
        public override Type ImplementationType { get { return _implementationType; } }

        /// <summary>Provides closed-generic factory for registered open-generic variant.</summary>
        public override IConcreteFactoryProvider Provider { get { return _provider; } }

        /// <summary>Injection rules set for Constructor, Parameters, Properties and Fields.</summary>
        public readonly Made Made;

        /// <summary>Creates factory providing implementation type, optional reuse and setup.</summary>
        /// <param name="implementationType">Non-abstract close or open generic type.</param>
        /// <param name="reuse">(optional)</param> <param name="made">(optional)</param> <param name="setup">(optional)</param>
        public ReflectionFactory(Type implementationType, IReuse reuse = null, Made made = null, Setup setup = null)
            : base(reuse, setup)
        {
            _implementationType = implementationType;
            Made = made ?? Made.Default;

            if (Made.FactoryMethod == null)
                implementationType.ThrowIf(implementationType.IsAbstract(), Error.EXPECTED_NON_ABSTRACT_IMPL_TYPE);

            if (implementationType != null && implementationType.IsGenericDefinition())
                _provider = new CloseGenericFactoryProvider(this);
        }

        /// <summary>Before registering factory checks that ImplementationType is assignable, Or
        /// in case of open generics, compatible with <paramref name="serviceType"/>. 
        /// Then checks that there is defined constructor selector for implementation type with multiple/no constructors.</summary>
        /// <param name="container">Container to register factory in.</param>
        /// <param name="serviceType">Service type to register factory with.</param>
        /// <param name="serviceKey">(ignored)</param>
        public override void CheckBeforeRegistration(IContainer container, Type serviceType, object serviceKey)
        {
            base.CheckBeforeRegistration(container, serviceType, serviceKey);
            if (_implementationType == null)
                return;

            var implType = _implementationType;
            if (!implType.IsGenericDefinition())
            {
                if (implType.IsOpenGeneric())
                    Throw.It(Error.REGISTERING_NOT_A_GENERIC_TYPEDEF_IMPL_TYPE,
                        implType, implType.GetGenericDefinitionOrNull());

                if (implType != serviceType && serviceType != typeof(object) &&
                    Array.IndexOf(implType.GetImplementedTypes(), serviceType) == -1)
                    Throw.It(Error.IMPLEMENTATION_NOT_ASSIGNABLE_TO_SERVICE_TYPE, implType, serviceType);

                if (Made.FactoryMethod != null && Made.ExpressionReturnType != null)
                    implType.ThrowIfNotOf(Made.ExpressionReturnType, Error.MADE_OF_TYPE_NOT_ASSIGNABLE_TO_IMPLEMENTATION_TYPE);
            }
            else if (implType != serviceType)
            {
                if (serviceType.IsGenericDefinition())
                {
                    var implTypeParams = implType.GetGenericParamsAndArgs();
                    var implementedTypes = implType.GetImplementedTypes();

                    var implementedTypeFound = false;
                    var containsAllTypeParams = false;
                    for (var i = 0; !containsAllTypeParams && i < implementedTypes.Length; ++i)
                    {
                        var implementedType = implementedTypes[i];
                        implementedTypeFound = implementedType.GetGenericDefinitionOrNull() == serviceType;
                        containsAllTypeParams = implementedTypeFound &&
                            implementedType.ContainsAllGenericTypeParameters(implTypeParams);
                    }

                    if (!implementedTypeFound)
                        Throw.It(Error.IMPLEMENTATION_NOT_ASSIGNABLE_TO_SERVICE_TYPE, implType, serviceType);

                    if (!containsAllTypeParams)
                        Throw.It(Error.REGISTERING_OPEN_GENERIC_SERVICE_WITH_MISSING_TYPE_ARGS,
                            implType, serviceType, implementedTypes.Where(t => t.GetGenericDefinitionOrNull() == serviceType));
                }
                else if (implType.IsGeneric() && serviceType.IsOpenGeneric())
                    Throw.It(Error.REGISTERING_NOT_A_GENERIC_TYPEDEF_SERVICE_TYPE,
                        serviceType, serviceType.GetGenericDefinitionOrNull());
                else
                    Throw.It(Error.REGISTERING_OPEN_GENERIC_IMPL_WITH_NON_GENERIC_SERVICE, implType, serviceType);
            }

            if (Made.FactoryMethod == null && container.Rules.FactoryMethod == null)
            {
                var publicCounstructorCount = implType.GetAllConstructors().Count();
                Throw.If(publicCounstructorCount != 1, 
                    Error.NO_DEFINED_METHOD_TO_SELECT_FROM_MULTIPLE_CONSTRUCTORS, implType, publicCounstructorCount);
            }
        }

        /// <summary>Creates service expression, so for registered implementation type "Service", 
        /// you will get "new Service()". If there is <see cref="Reuse"/> specified, then expression will
        /// contain call to <see cref="Scope"/> returned by reuse.</summary>
        /// <param name="request">Request for service to resolve.</param> <returns>Created expression.</returns>
        public override Expression CreateExpressionOrDefault(Request request)
        {
            var factoryMethod = GetFactoryMethod(request);

            // If factory method is instance method, then resolve factory instance first.
            Expression factoryExpr = null;
            if (factoryMethod.FactoryInfo != null)
            {
                var factoryRequest = request.Push(factoryMethod.FactoryInfo);
                var factoryFactory = factoryRequest.Container.ResolveFactory(factoryRequest);
                factoryExpr = factoryFactory == null ? null : factoryFactory.GetExpressionOrDefault(factoryRequest);
                if (factoryExpr == null)
                    return null;
            }

            Expression[] paramExprs = null;
            var constructorOrMethod = factoryMethod.ConstructorOrMethodOrMember as MethodBase;
            if (constructorOrMethod != null)
            {
                var parameters = constructorOrMethod.GetParameters();
                if (parameters.Length != 0)
                {
                    paramExprs = new Expression[parameters.Length];

                    var parameterSelector = request.Container.Rules.Parameters.And(Made.Parameters)(request);

                    var funcArgs = request.FuncArgs;
                    var funcArgsUsedMask = 0;

                    for (var i = 0; i < parameters.Length; i++)
                    {
                        var param = parameters[i];
                        Expression paramExpr = null;

                        if (funcArgs != null)
                        {
                            for (var fa = 0; fa < funcArgs.Value.Length && paramExpr == null; ++fa)
                            {
                                var funcArg = funcArgs.Value[fa];
                                if ((funcArgsUsedMask & 1 << fa) == 0 &&                  // not yet used func argument
                                    funcArg.Type.IsAssignableTo(param.ParameterType)) // and it assignable to parameter
                                {
                                    paramExpr = funcArg;
                                    funcArgsUsedMask |= 1 << fa;  // mark that argument was used
                                    funcArgs.Key[fa] = true;      // mark that argument was used globally for Func<..> resolver.
                                }
                            }
                        }

                        // If parameter expression still null (no Func argument to substitute), try to resolve it
                        if (paramExpr == null)
                        {

                            var paramInfo = parameterSelector(param) ?? ParameterServiceInfo.Of(param);
                            var paramRequest = request.Push(paramInfo);

                            var customValue = paramInfo.Details.CustomValue;
                            if (customValue != null)
                            {
                                customValue.ThrowIfNotOf(paramRequest.ServiceType, Error.INJECTED_CUSTOM_VALUE_IS_OF_DIFFERENT_TYPE, paramRequest);
                                paramExpr = paramRequest.Container.GetOrAddStateItemExpression(customValue, throwIfStateRequired: true);
                            }
                            else
                            {
                                var paramFactory = paramRequest.Container.ResolveFactory(paramRequest);
                                paramExpr = paramFactory == null ? null : paramFactory.GetExpressionOrDefault(paramRequest);
                                if (paramExpr == null)
                                {
                                    if (request.IfUnresolved == IfUnresolved.ReturnDefault)
                                        return null;

                                    var defaultValue = paramInfo.Details.DefaultValue;
                                    paramExpr = defaultValue != null
                                        ? paramRequest.Container.GetOrAddStateItemExpression(defaultValue)
                                        : paramRequest.ServiceType.GetDefaultValueExpression();
                                }
                            }
                        }

                        paramExprs[i] = paramExpr;
                    }
                }
            }

            return CreateServiceExpression(factoryMethod.ConstructorOrMethodOrMember, factoryExpr, paramExprs, request);
        }

        private Expression CreateServiceExpression(MemberInfo ctorOrMethodOrMember, Expression factoryExpr, Expression[] paramExprs, Request request)
        {
            if (ctorOrMethodOrMember is ConstructorInfo)
                return InitPropertiesAndFields(Expression.New((ConstructorInfo)ctorOrMethodOrMember, paramExprs), request);

            if (ctorOrMethodOrMember is MethodInfo)
                return Expression.Call(factoryExpr, (MethodInfo)ctorOrMethodOrMember, paramExprs);

            if (ctorOrMethodOrMember is PropertyInfo)
                return Expression.Property(factoryExpr, (PropertyInfo)ctorOrMethodOrMember);

            return Expression.Field(factoryExpr, (FieldInfo)ctorOrMethodOrMember);
        }

        #region Implementation

        private readonly Type _implementationType;
        private readonly CloseGenericFactoryProvider _provider;

        private sealed class CloseGenericFactoryProvider : IConcreteFactoryProvider
        {
            public IEnumerable<KV<Type, object>> ProvidedFactoriesServiceTypeKey
            {
                get
                {
                    return _providedFactories.Value.IsEmpty
                        ? Enumerable.Empty<KV<Type, object>>()
                        : _providedFactories.Value.Enumerate().Select(_ => _.Value);
                }
            }

            public CloseGenericFactoryProvider(ReflectionFactory factory) { _factory = factory; }

            public Factory ProvideConcreteFactory(Request request)
            {
                var serviceType = request.ServiceType;
                var implType = _factory._implementationType;
                var closedTypeArgs = implType == serviceType.GetGenericDefinitionOrNull()
                    ? serviceType.GetGenericParamsAndArgs()
                    : GetClosedTypeArgsOrNullForOpenGenericType(implType, request);
                if (closedTypeArgs == null)
                    return null;

                Type closedImplType;
                if (request.IfUnresolved == IfUnresolved.ReturnDefault)
                {
                    try { closedImplType = implType.MakeGenericType(closedTypeArgs); }
                    catch { return null; }
                }
                else
                {
                    closedImplType = Throw.IfThrows<ArgumentException, Type>(
                       () => implType.MakeGenericType(closedTypeArgs),
                       Error.NO_MATCHED_GENERIC_PARAM_CONSTRAINTS, implType, request);
                }

                var factory = new ReflectionFactory(closedImplType, _factory.Reuse, _factory.Made, _factory.Setup);
                _providedFactories.Swap(_ => _.AddOrUpdate(factory.FactoryID,
                    new KV<Type, object>(serviceType, request.ServiceKey)));
                return factory;
            }

            private readonly ReflectionFactory _factory;

            private readonly Ref<HashTree<int, KV<Type, object>>>
                _providedFactories = Ref.Of(HashTree<int, KV<Type, object>>.Empty);
        }

        private FactoryMethod GetFactoryMethod(Request request)
        {
            var implType = _implementationType;
            var factoryMethodSelector = Made.FactoryMethod ?? request.Container.Rules.FactoryMethod;
            if (factoryMethodSelector != null)
            {
                var factoryMethod = factoryMethodSelector(request);
                if (factoryMethod != null && !(factoryMethod.ConstructorOrMethodOrMember is ConstructorInfo))
                {
                    var member = factoryMethod.ConstructorOrMethodOrMember;
                    var isStaticMember =
                        member is MethodInfo ? ((MethodInfo)member).IsStatic : 
                        member is PropertyInfo ? Portable.GetPropertyGetMethod((PropertyInfo)member).IsStatic :
                        ((FieldInfo)member).IsStatic;

                    Throw.If(isStaticMember && factoryMethod.FactoryInfo != null,
                            Error.FACTORY_OBJ_PROVIDED_BUT_METHOD_IS_STATIC, factoryMethod.FactoryInfo, factoryMethod,
                            request);

                    Throw.If(!isStaticMember && factoryMethod.FactoryInfo == null,
                        Error.FACTORY_OBJ_IS_NULL_IN_FACTORY_METHOD, factoryMethod, request);

                    request.ServiceType.ThrowIfNotOf(member.GetReturnTypeOrDefault(),
                        Error.SERVICE_IS_NOT_ASSIGNABLE_FROM_FACTORY_METHOD, factoryMethod, request);
                }

                return factoryMethod.ThrowIfNull(Error.UNABLE_TO_GET_CONSTRUCTOR_FROM_SELECTOR, implType);
            }

            var ctors = implType.GetAllConstructors().ToArrayOrSelf();
            Throw.If(ctors.Length == 0, Error.NO_PUBLIC_CONSTRUCTOR_DEFINED, implType);
            Throw.If(ctors.Length > 1, Error.UNABLE_TO_SELECT_CONSTRUCTOR, ctors.Length, implType);
            return FactoryMethod.Of(ctors[0]);
        }

        private Expression InitPropertiesAndFields(NewExpression newServiceExpr, Request request)
        {
            var members = request.Container.Rules.PropertiesAndFields.And(Made.PropertiesAndFields)(request);
            if (members == null)
                return newServiceExpr;

            var bindings = new List<MemberBinding>();
            foreach (var member in members)
                if (member != null)
                {
                    var memberRequest = request.Push(member);
                    Expression memberExpr;

                    var customValue = member.Details.CustomValue;
                    if (customValue != null)
                    {
                        customValue.ThrowIfNotOf(memberRequest.ServiceType, Error.INJECTED_CUSTOM_VALUE_IS_OF_DIFFERENT_TYPE, memberRequest);
                        memberExpr = memberRequest.Container.GetOrAddStateItemExpression(customValue, throwIfStateRequired: true);
                    }
                    else
                    {
                        var memberFactory = memberRequest.Container.ResolveFactory(memberRequest);
                        memberExpr = memberFactory == null ? null : memberFactory.GetExpressionOrDefault(memberRequest);
                        if (memberExpr == null && request.IfUnresolved == IfUnresolved.ReturnDefault)
                            return null;
                    }

                    if (memberExpr != null)
                        bindings.Add(Expression.Bind(member.Member, memberExpr));
                }

            return bindings.Count == 0 ? (Expression)newServiceExpr : Expression.MemberInit(newServiceExpr, bindings); 
        }

        private static Type[] GetClosedTypeArgsOrNullForOpenGenericType(Type implType, Request request)
        {
            var serviceType = request.ServiceType;
            var serviceTypeArgs = serviceType.GetGenericParamsAndArgs();
            var serviceOpenGenericType = serviceType.GetGenericDefinitionOrNull().ThrowIfNull();

            var implTypeParams = implType.GetGenericParamsAndArgs();
            var implTypeArgs = new Type[implTypeParams.Length];

            var implementedTypes = implType.GetImplementedTypes();

            var matchFound = false;
            for (var i = 0; !matchFound && i < implementedTypes.Length; ++i)
            {
                var implementedType = implementedTypes[i];
                if (implementedType.IsOpenGeneric() &&
                    implementedType.GetGenericDefinitionOrNull() == serviceOpenGenericType)
                {
                    matchFound = MatchServiceWithImplementedTypeParams(
                        implTypeArgs, implTypeParams, implementedType.GetGenericParamsAndArgs(), serviceTypeArgs);
                }
            }

            if (!matchFound)
                return request.IfUnresolved == IfUnresolved.ReturnDefault ? null
                    : Throw.For<Type[]>(Error.NO_MATCHED_IMPLEMENTED_TYPES_WITH_SERVICE_TYPE,
                        implType, implementedTypes, request);

            // check constraints
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
                        implTypeParamConstraint.IsOpenGeneric() && implTypeArg.IsGeneric())
                    {
                        constraintMatchFound = MatchServiceWithImplementedTypeParams(
                            implTypeArgs, implTypeParams,
                            implTypeParamConstraint.GetGenericParamsAndArgs(),
                            implTypeArg.GetGenericParamsAndArgs());
                    }
                }
            }

            var notMatchedIndex = Array.IndexOf(implTypeArgs, null);
            if (notMatchedIndex != -1)
                return request.IfUnresolved == IfUnresolved.ReturnDefault ? null
                    : Throw.For<Type[]>(Error.NOT_FOUND_OPEN_GENERIC_IMPL_TYPE_ARG_IN_SERVICE,
                        implType, implTypeParams[notMatchedIndex], request);

            return implTypeArgs;
        }

        private static bool MatchServiceWithImplementedTypeParams(
            Type[] matchedImplArgs, Type[] implParams, Type[] implementedParams, Type[] serviceArgs)
        {
            for (var i = 0; i < implementedParams.Length; i++)
            {
                var serviceArg = serviceArgs[i];
                var implementedParam = implementedParams[i];
                if (implementedParam.IsGenericParameter)
                {
                    var paramIndex = implParams.IndexOf(implementedParam);
                    if (paramIndex != -1)
                    {
                        if (matchedImplArgs[paramIndex] == null)
                            matchedImplArgs[paramIndex] = serviceArg;
                        else if (matchedImplArgs[paramIndex] != serviceArg)
                            return false; // more than one service type arg is matching with single impl type param
                    }
                }
                else if (implementedParam != serviceArg)
                {
                    if (!implementedParam.IsOpenGeneric() ||
                        implementedParam.GetGenericDefinitionOrNull() != serviceArg.GetGenericDefinitionOrNull())
                        return false; // type param and arg are of different types

                    if (!MatchServiceWithImplementedTypeParams(matchedImplArgs, implParams,
                        implementedParam.GetGenericParamsAndArgs(), serviceArg.GetGenericParamsAndArgs()))
                        return false; // nested match failed due either one of above reasons.
                }
            }

            return true;
        }

        #endregion
    }

    /// <summary>Creates service expression using client provided expression factory delegate.</summary>
    public sealed class ExpressionFactory : Factory
    {
        /// <summary>Wraps provided delegate into factory.</summary>
        /// <param name="getServiceExpression">Delegate that will be used internally to create service expression.</param>
        /// <param name="reuse">(optional) Reuse.</param> <param name="setup">(optional) Setup.</param>
        public ExpressionFactory(Func<Request, Expression> getServiceExpression, IReuse reuse = null, Setup setup = null)
            : base(reuse, setup)
        {
            _getServiceExpression = getServiceExpression.ThrowIfNull();
        }

        /// <summary>Creates service expression using wrapped delegate.</summary>
        /// <param name="request">Request to resolve.</param> <returns>Expression returned by stored delegate.</returns>
        public override Expression CreateExpressionOrDefault(Request request)
        {
            return _getServiceExpression(request);
        }

        private readonly Func<Request, Expression> _getServiceExpression;
    }

    /// <summary>This factory is the thin wrapper for user provided delegate 
    /// and where possible it uses delegate directly: without converting it to expression.</summary>
    public sealed class DelegateFactory : Factory
    {
        /// <summary>Non-abstract closed implementation type.</summary>
        public override Type ImplementationType { get { return _implementationType; } }

        /// <summary>Creates factory by providing:</summary>
        /// <param name="factoryDelegate">User specified service creation delegate.</param>
        /// <param name="reuse">(optional) Reuse behavior for created service.</param>
        /// <param name="setup">(optional) Additional settings.</param>
        /// <param name="implementationType">(optional) Implementation type if known, e.g. when registering existing instance.</param>
        public DelegateFactory(Func<IResolver, object> factoryDelegate, 
            IReuse reuse = null, Setup setup = null, Type implementationType = null)
            : base(reuse, setup)
        {
            _factoryDelegate = factoryDelegate.ThrowIfNull();
            _implementationType = implementationType;
        }

        /// <summary>Create expression by wrapping call to stored delegate with provided request.</summary>
        /// <param name="request">Request to resolve. It will be stored in resolution state to be passed to delegate on actual resolve.</param>
        /// <returns>Created delegate call expression.</returns>
        public override Expression CreateExpressionOrDefault(Request request)
        {
            var factoryDelegateExpr = request.Container.GetOrAddStateItemExpression(_factoryDelegate);
            return Expression.Convert(Expression.Invoke(factoryDelegateExpr, Container.ResolverContextExpr), request.ServiceType);
        }

        /// <summary>If possible returns delegate directly, without creating expression trees, just wrapped in <see cref="FactoryDelegate"/>.
        /// If decorator found for request then factory fall-backs to expression creation.</summary>
        /// <param name="request">Request to resolve.</param> 
        /// <returns>Factory delegate directly calling wrapped delegate, or invoking expression if decorated.</returns>
        public override FactoryDelegate GetDelegateOrDefault(Request request)
        {
            request = request.ResolveWithFactory(this);

            if (request.Container.GetDecoratorExpressionOrDefault(request) != null)
                return base.GetDelegateOrDefault(request); // via expression creation

            var rules = request.Container.Rules;
            var reuse = rules.ReuseMapping == null ? Reuse : rules.ReuseMapping(Reuse, request);
            ThrowIfReuseHasShorterLifespanThanParent(reuse, request);

            if (reuse != null)
                return base.GetDelegateOrDefault(request); // use expression creation

            return (state, r, scope) => _factoryDelegate(r.Resolver);
        }

        private readonly Func<IResolver, object> _factoryDelegate;
        private readonly Type _implementationType;
    }

    /// <summary>Should return value stored in scope.</summary>
    public delegate object CreateScopedValue();

    /// <summary>Lazy object storage that will create object with provided factory on first access, 
    /// then will be returning the same object for subsequent access.</summary>
    public interface IScope : IDisposable
    {
        /// <summary>Parent scope in scope stack. Null for root scope.</summary>
        IScope Parent { get; }

        /// <summary>Optional name object associated with scope.</summary>
        object Name { get; }

        /// <summary>Creates, stores, and returns stored object.</summary>
        /// <param name="id">Unique ID to find created object in subsequent calls.</param>
        /// <param name="createValue">Delegate to create object. It will be used immediately, and reference to delegate will not be stored.</param>
        /// <returns>Created and stored object.</returns>
        /// <remarks>Scope does not store <paramref name="createValue"/> (no memory leak here), 
        /// it stores only result of <paramref name="createValue"/> call.</remarks>
        object GetOrAdd(int id, CreateScopedValue createValue);

        /// <summary>Sets (replaces) value at specified id, or adds value if no existing id found.</summary>
        /// <param name="id">To set value at.</param> <param name="value">Value to set.</param>
        void SetOrAdd(int id, object value);
    }

    /// <summary><see cref="IScope"/> implementation which will dispose stored <see cref="IDisposable"/> items on its own dispose.
    /// Locking is used internally to ensure that object factory called only once.</summary>
    public sealed class Scope : IScope
    {
        /// <summary>Parent scope in scope stack. Null for root scope.</summary>
        public IScope Parent { get; private set; }

        /// <summary>Optional name object associated with scope.</summary>
        public object Name { get; private set; }

        /// <summary>Returns true if scope disposed.</summary>
        public bool IsDisposed
        {
            get { return _disposed == 1; }
        }

        /// <summary>Creates root scope without name.</summary>
        public Scope() { }

        /// <summary>Create scope with optional parent and name.</summary>
        /// <param name="parent">Parent in scope stack.</param> <param name="name">Associated name object.</param>
        public Scope(IScope parent, object name)
        {
            Parent = parent;
            Name = name;
        }

        /// <summary><see cref="IScope.GetOrAdd"/> for description.
        /// Will throw <see cref="ContainerException"/> if scope is disposed.</summary>
        /// <param name="id">Unique ID to find created object in subsequent calls.</param>
        /// <param name="createValue">Delegate to create object. It will be used immediately, and reference to delegate will Not be stored.</param>
        /// <returns>Created and stored object.</returns>
        /// <exception cref="ContainerException">if scope is disposed.</exception>
        public object GetOrAdd(int id, CreateScopedValue createValue)
        {
            if (_disposed == 1)
                Throw.It(Error.SCOPE_IS_DISPOSED);

            lock (_syncRoot)
            {
                var item = _items.GetValueOrDefault(id);
                if (item == null ||
                    item is IRecyclable && ((IRecyclable)item).IsRecycled)
                {
                    if (item != null)
                        DisposeItem(item);

                    Throw.If(_disposed == 1, Error.SCOPE_IS_DISPOSED);

                    item = createValue();
                    Ref.Swap(ref _items, items => items.AddOrUpdate(id, item));
                }
                return item;
            }
        }

        /// <summary>Sets (replaces) value at specified id, or adds value if no existing id found.</summary>
        /// <param name="id">To set value at.</param> <param name="value">Value to set.</param>
        public void SetOrAdd(int id, object value)
        {
            Throw.If(_disposed == 1, Error.SCOPE_IS_DISPOSED);
            Ref.Swap(ref _items, items => items.AddOrUpdate(id, value));
        }

        /// <summary>Disposes all stored <see cref="IDisposable"/> objects and nullifies object storage.</summary>
        /// <remarks>If item disposal throws exception, then it won't be propagated outside, so the rest of the items could be disposed.</remarks>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
                return;
            if (!_items.IsEmpty)
                foreach (var item in _items.Enumerate().Select(x => x.Value).Where(x => x is IDisposable || x is IReuseWrapper))
                    DisposeItem(item);
            _items = IntKeyTree.Empty;
        }

        /// <summary>Prints scope info (name and parent) to string for debug purposes.</summary> <returns>String representation.</returns>
        public override string ToString()
        {
            return "named: " +
                (Name == null ? "no" : Name.ToString()) +
                (Parent == null ? "" : " (parent " + Parent + ")");
        }

        #region Implementation

        private IntKeyTree _items = IntKeyTree.Empty;
        private int _disposed;

        // Sync root is required to create object only once. The same reason as for Lazy<T>.
        private readonly object _syncRoot = new object();

        private void DisposeItem(object item)
        {
            try
            {
                var disposable = item as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
                else
                {
                    var reused = item as IReuseWrapper;
                    while (reused != null && !(reused is IHideDisposableFromContainer)
                           && reused.Target != null && (disposable = reused.Target as IDisposable) == null)
                        reused = reused.Target as IReuseWrapper;
                    if (disposable != null)
                        disposable.Dispose();
                }
            }
            catch (Exception)
            {
                // NOTE Ignoring disposing exception, they not so important for program to proceed.
            }
        }

        #endregion
    }

    /// <summary>Provides ambient current scope and optionally scope storage for container, 
    /// examples are HttpContext storage, Execution context, Thread local.</summary>
    public interface IScopeContext
    {
        /// <summary>Name associated with context root scope - so the reuse may find scope context.</summary>
        object RootScopeName { get; }

        /// <summary>Returns current scope or null if no ambient scope available at the moment.</summary>
        /// <returns>Current scope or null.</returns>
        IScope GetCurrentOrDefault();

        /// <summary>Changes current scope using provided delegate. Delegate receives current scope as input and
        /// should return new current scope.</summary>
        /// <param name="getNewCurrentScope">Delegate to change the scope.</param>
        /// <remarks>Important: <paramref name="getNewCurrentScope"/> may be called multiple times in concurrent environment.
        /// Make it predictable by removing any side effects.</remarks>
        /// <returns>New current scope. So it is convenient to use method in "using (var newScope = ctx.SetCurrent(...))".</returns>
        IScope SetCurrent(Func<IScope, IScope> getNewCurrentScope);
    }

    /// <summary>Tracks one current scope per thread, so the current scope in different tread would be different or null,
    /// if not yet tracked. Context actually stores scope references internally, so it should be disposed to free them.</summary>
    public sealed class ThreadScopeContext : IScopeContext, IDisposable
    {
        /// <summary>Provides static access to <see cref="RootScopeName"/>. It is OK because its constant.</summary>
        public static readonly object ROOT_SCOPE_NAME = typeof(ThreadScopeContext);

        /// <summary>Key to identify context.</summary>
        public object RootScopeName { get { return ROOT_SCOPE_NAME; } }

        /// <summary>Returns current scope in calling Thread or null, if no scope tracked.</summary>
        /// <returns>Found scope or null.</returns>
        public IScope GetCurrentOrDefault()
        {
            return _scopes.GetValueOrDefault(Portable.GetCurrentManagedThreadID()) as IScope;
        }

        /// <summary>Change current scope for the calling Thread.</summary>
        /// <param name="getNewCurrentScope">Delegate to change the scope given current one (or null).</param>
        /// <remarks>Important: <paramref name="getNewCurrentScope"/> may be called multiple times in concurrent environment.
        /// Make it predictable by removing any side effects.</remarks>
        public IScope SetCurrent(Func<IScope, IScope> getNewCurrentScope)
        {
            var threadId = Portable.GetCurrentManagedThreadID();
            IScope newScope = null;
            Ref.Swap(ref _scopes, scopes =>
                scopes.AddOrUpdate(threadId, newScope = getNewCurrentScope(scopes.GetValueOrDefault(threadId) as IScope)));
            return newScope;
        }

        /// <summary>Disposed all stored/tracked scopes and empties internal scope storage.</summary>
        public void Dispose()
        {
            Ref.Swap(ref _scopes, scopes =>
            {
                if (!scopes.IsEmpty)
                    foreach (var scope in scopes.Enumerate().Where(scope => scope.Value is IDisposable))
                        ((IDisposable)scope.Value).Dispose();
                return IntKeyTree.Empty;
            });
        }

        private IntKeyTree _scopes = IntKeyTree.Empty;
    }

    /// <summary>Reuse goal is to locate or create scope where reused objects will be stored.</summary>
    /// <remarks><see cref="IReuse"/> implementors supposed to be stateless, and provide scope location behavior only.
    /// The reused service instances should be stored in scope(s).</remarks>
    public interface IReuse
    {
        /// <summary>Relative to other reuses lifespan value.</summary>
        int Lifespan { get; }

        /// <summary>Locates or creates scope to store reused service objects.</summary>
        /// <param name="request">Request to get context information or for example store something in resolution state.</param>
        /// <returns>Located scope.</returns>
        IScope GetScopeOrDefault(Request request);

        /// <summary>Supposed to create in-line expression with the same code as body of <see cref="GetScopeOrDefault"/> method.</summary>
        /// <param name="request">Request to get context information or for example store something in resolution state.</param>
        /// <returns>Expression of type <see cref="IScope"/>.</returns>
        /// <remarks>Result expression should be static: should Not create closure on any objects. 
        /// If you require to reference some item from outside, put it into <see cref="IContainer.ResolutionStateCache"/>.</remarks>
        Expression GetScopeExpression(Request request);
    }

    /// <summary>Returns container bound scope for storing singleton objects.</summary>
    public sealed class SingletonReuse : IReuse
    {
        /// <summary>Related lifespan value associated with reuse.</summary>
        public static readonly int LIFESPAN = 1000;

        /// <summary>Relative to other reuses lifespan value.</summary>
        public int Lifespan { get { return LIFESPAN; } }

        /// <summary>Returns container bound Singleton scope.</summary>
        /// <param name="request">Request to get context information or for example store something in resolution state.</param>
        /// <returns>Container singleton scope.</returns>
        public IScope GetScopeOrDefault(Request request)
        {
            return request.Container.SingletonScope;
        }

        /// <summary>Returns expression directly accessing <see cref="IResolverContext.SingletonScope"/>.</summary>
        /// <param name="request">Request to get context information or for example store something in resolution state.</param>
        /// <returns>Singleton scope property expression.</returns>
        public Expression GetScopeExpression(Request request)
        {
            return Expression.Property(Container.ResolverContextExpr, "SingletonScope");
        }

        /// <summary>Pretty print reuse name and lifespan</summary> <returns>Printed string.</returns>
        public override string ToString() { return GetType().Name + ":" + Lifespan; }
    }

    /// <summary>Returns container bound current scope created by <see cref="Container.OpenScope"/> method.</summary>
    /// <remarks>It is the same as Singleton scope if container was not created by <see cref="Container.OpenScope"/>.</remarks>
    public sealed class CurrentScopeReuse : IReuse
    {
        /// <summary>Related lifespan value associated with reuse.</summary>
        public static readonly int LIFESPAN = 100;

        /// <summary>Name to find current scope or parent with equal name.</summary>
        public readonly object Name;

        /// <summary>Relative to other reuses lifespan value.</summary>
        public int Lifespan { get { return LIFESPAN; } }

        /// <summary>Creates reuse optionally specifying its name.</summary> 
        /// <param name="name">(optional) Used to find matching current scope or parent.</param>
        public CurrentScopeReuse(object name = null)
        {
            Name = name;
        }

        /// <summary>Returns container current scope or if <see cref="Name"/> specified: current scope or its parent with corresponding name.</summary>
        /// <param name="request">Request to get context information or for example store something in resolution state.</param>
        /// <returns>Found current scope or its parent.</returns>
        /// <exception cref="ContainerException">with the code <see cref="Error.NO_MATCHED_SCOPE_FOUND"/> if <see cref="Name"/> specified but
        /// no matching scope or its parent found.</exception>
        public IScope GetScopeOrDefault(Request request)
        {
            return request.Container.GetCurrentNamedScope(Name, false);
        }

        /// <summary>Returns <see cref="IResolverContext.GetCurrentNamedScope"/> method call expression.</summary>
        /// <param name="request">Request to get context information or for example store something in resolution state.</param>
        /// <returns>Method call expression returning matched current scope.</returns>
        public Expression GetScopeExpression(Request request)
        {
            var nameExpr = request.Container.GetOrAddStateItemExpression(Name, typeof(object));
            return Expression.Call(Container.ResolverContextExpr, "GetCurrentNamedScope", null, 
                nameExpr, Expression.Constant(true));
        }

        /// <summary>Pretty prints reuse to string.</summary> <returns>Reuse string.</returns>
        public override string ToString()
        {
            var s = GetType().Name + ":" + Lifespan;
            if (Name != null) s += new StringBuilder(", Name:").Print(Name);
            return s;
        }
    }

    /// <summary>Represents services created once per resolution root (when some of Resolve methods called).</summary>
    /// <remarks>Scope is created only if accessed to not waste memory.</remarks>
    public sealed class ResolutionScopeReuse : IReuse
    {
        /// <summary>Related lifespan value associated with reuse.</summary>
        public static readonly int LIFESPAN = 10;

        /// <summary>Relative to other reuses lifespan value.</summary>
        public int Lifespan { get { return LIFESPAN; } }

        /// <summary>Creates new resolution scope reuse with specified type and key.</summary>
        /// <param name="assignableFromServiceType">(optional)</param> <param name="serviceKey">(optional)</param>
        /// <param name="outermost">(optional)</param>
        public ResolutionScopeReuse(Type assignableFromServiceType = null, object serviceKey = null, bool outermost = false)
        {
            _assignableFromServiceType = assignableFromServiceType;
            _serviceKey = serviceKey;
            _outermost = outermost;
        }

        /// <summary>Creates or returns already created resolution root scope.</summary>
        /// <param name="request">Request to get context information or for example store something in resolution state.</param>
        /// <returns>Created or existing scope.</returns>
        public IScope GetScopeOrDefault(Request request)
        {
            var scope = request.Scope;
            if (scope == null)
            {
                var parent = request.Enumerate().Last();
                request.Container.GetOrCreateResolutionScope(ref scope, parent.ServiceType, parent.ServiceKey);
            }

            return request.Container.GetMatchingResolutionScope(scope, _assignableFromServiceType, _serviceKey, _outermost, false);
        }

        /// <summary>Returns <see cref="IResolverContext.GetMatchingResolutionScope"/> method call expression.</summary>
        /// <param name="request">Request to get context information or for example store something in resolution state.</param>
        /// <returns>Method call expression returning existing or newly created resolution scope.</returns>
        public Expression GetScopeExpression(Request request)
        {
            return Expression.Call(Container.ResolverContextExpr, "GetMatchingResolutionScope", null,
                Container.GetResolutionScopeExpression(request),
                Expression.Constant(_assignableFromServiceType, typeof(Type)),
                request.Container.GetOrAddStateItemExpression(_serviceKey, typeof(object)),
                Expression.Constant(_outermost, typeof(bool)),
                Expression.Constant(true, typeof(bool)));
        }

        /// <summary>Pretty print reuse name and lifespan</summary> <returns>Printed string.</returns>
        public override string ToString() { return GetType().Name + ":" + Lifespan; }

        private readonly Type _assignableFromServiceType;
        private readonly object _serviceKey;
        private readonly bool _outermost;
    }

    /// <summary>Specifies pre-defined reuse behaviors supported by container: 
    /// used when registering services into container with <see cref="Registrator"/> methods.</summary>
    public static partial class Reuse
    {
        /// <summary>Synonym for absence of reuse.</summary>
        public static readonly IReuse Transient = null; // no reuse.

        /// <summary>Specifies to store single service instance per <see cref="Container"/>.</summary>
        public static readonly IReuse Singleton = new SingletonReuse();

        /// <summary>Specifies to store single service instance per resolution root created by <see cref="Resolver"/> methods.</summary>
        public static readonly IReuse InResolutionScope = new ResolutionScopeReuse();

        /// <summary>Specifies to store single service instance per current/open scope created with <see cref="Container.OpenScope"/>.</summary>
        public static readonly IReuse InCurrentScope = new CurrentScopeReuse();

        /// <summary>Returns current scope reuse with specific name to match with scope.
        /// If name is not specified then function returns <see cref="InCurrentScope"/>.</summary>
        /// <param name="name">(optional) Name to match with scope.</param>
        /// <returns>Created current scope reuse.</returns>
        public static IReuse InCurrentNamedScope(object name = null)
        {
            return name == null ? InCurrentScope : new CurrentScopeReuse(name);
        }

        /// <summary>Creates reuse to search for <paramref name="assignableFromServiceType"/> and <paramref name="serviceKey"/>
        /// in existing resolution scope hierarchy. If parameters are not specified or null, then <see cref="InResolutionScope"/> will be returned.</summary>
        /// <param name="assignableFromServiceType">(optional) To search for scope with service type assignable to type specified in parameter.</param>
        /// <param name="serviceKey">(optional) Search for specified key.</param>
        /// <param name="outermost">If true - commands to look for outermost match instead of nearest.</param>
        /// <returns>New reuse with specified parameters or <see cref="InResolutionScope"/> if nothing specified.</returns>
        public static IReuse InResolutionScopeOf(Type assignableFromServiceType = null, object serviceKey = null, bool outermost = false)
        {
            return assignableFromServiceType == null && serviceKey == null ? InResolutionScope
                : new ResolutionScopeReuse(assignableFromServiceType, serviceKey, outermost);
        }

        /// <summary>Creates reuse to search for <typeparamref name="TAssignableFromServiceType"/> and <paramref name="serviceKey"/>
        /// in existing resolution scope hierarchy.</summary>
        /// <typeparam name="TAssignableFromServiceType">To search for scope with service type assignable to type specified in parameter.</typeparam>
        /// <param name="serviceKey">(optional) Search for specified key.</param>
        /// <param name="outermost">If true - commands to look for outermost match instead of nearest.</param>
        /// <returns>New reuse with specified parameters.</returns>
        public static IReuse InResolutionScopeOf<TAssignableFromServiceType>(object serviceKey = null, bool outermost = false)
        {
            return InResolutionScopeOf(typeof(TAssignableFromServiceType), serviceKey, outermost);
        }

        /// <summary>Ensuring single service instance per Thread.</summary>
        public static readonly IReuse InThread = InCurrentNamedScope(ThreadScopeContext.ROOT_SCOPE_NAME);

        /// <summary>Special name that by convention recognized by <see cref="InWebRequest"/>.</summary>
        public static readonly string WebRequestScopeName = "WebRequestScopeName";

        /// <summary>Web request is just convention for reuse in <see cref="InCurrentNamedScope"/> with special name <see cref="WebRequestScopeName"/>.</summary>
        public static readonly IReuse InWebRequest = InCurrentNamedScope(WebRequestScopeName);
    }

    /// <summary>Creates <see cref="IReuseWrapper"/> for target and unwraps matching wrapper.</summary>
    public interface IReuseWrapperFactory
    {
        /// <summary>Wraps target value into new wrapper.</summary>
        /// <param name="target">Input value. May be other wrapper.</param> <returns>New wrapper.</returns>
        object Wrap(object target);

        /// <summary>Unwraps wrapper of supported/matched wrapper type. Otherwise throws.</summary>
        /// <param name="wrapper">Wrapper to unwrap.</param> <returns>Unwrapped value. May be nested wrapper.</returns>
        object Unwrap(object wrapper);
    }

    /// <summary>Listing and implementations of out-of-the-box supported <see cref="IReuseWrapper"/> factories.</summary>
    public static class ReuseWrapperFactory
    {
        /// <summary>Factory for <see cref="ReuseHiddenDisposable"/>.</summary>
        public static readonly IReuseWrapperFactory HiddenDisposable = new HiddenDisposableFactory();

        /// <summary>Factory for <see cref="ReuseWeakReference"/>.</summary>
        public static readonly IReuseWrapperFactory WeakReference = new WeakReferenceFactory();

        /// <summary>Factory for <see cref="ReuseSwapable"/>.</summary>
        public static readonly IReuseWrapperFactory Swapable = new SwapableFactory();

        /// <summary>Factory for <see cref="ReuseRecyclable"/>.</summary>
        public static readonly IReuseWrapperFactory Recyclable = new RecyclableFactory();

        #region Implementation

        private sealed class HiddenDisposableFactory : IReuseWrapperFactory
        {
            public object Wrap(object target)
            {
                return new ReuseHiddenDisposable((target as IDisposable).ThrowIfNull());
            }

            public object Unwrap(object wrapper)
            {
                return (wrapper as ReuseHiddenDisposable).ThrowIfNull().Target;
            }
        }

        private sealed class WeakReferenceFactory : IReuseWrapperFactory
        {
            public object Wrap(object target)
            {
                return new ReuseWeakReference(target);
            }

            public object Unwrap(object wrapper)
            {
                return (wrapper as ReuseWeakReference).ThrowIfNull().Target.ThrowIfNull(Error.WEAKREF_REUSE_WRAPPER_GCED);
            }
        }

        private sealed class SwapableFactory : IReuseWrapperFactory
        {
            public object Wrap(object target)
            {
                return new ReuseSwapable(target);
            }

            public object Unwrap(object wrapper)
            {
                return (wrapper as ReuseSwapable).ThrowIfNull().Target;
            }
        }

        private sealed class RecyclableFactory : IReuseWrapperFactory
        {
            public object Wrap(object target)
            {
                return new ReuseRecyclable(target);
            }

            public object Unwrap(object wrapper)
            {
                var recyclable = (wrapper as ReuseRecyclable).ThrowIfNull();
                Throw.If(recyclable.IsRecycled, Error.RECYCLABLE_REUSE_WRAPPER_IS_RECYCLED);
                return recyclable.Target;
            }
        }

        #endregion
    }

    /// <summary>Defines reused object wrapper.</summary>
    public interface IReuseWrapper
    {
        /// <summary>Wrapped value.</summary>
        object Target { get; }
    }

    /// <summary>Provides strongly-typed access to wrapped target.</summary>
    public static class ReuseWrapper
    {
        /// <summary>Type of <see cref="ReuseHiddenDisposable"/> added for intellisense discoverability.</summary>
        public static readonly Type HiddenDisposable = typeof(ReuseHiddenDisposable);
        /// <summary>Type of <see cref="ReuseWeakReference"/> added for intellisense discoverability.</summary>
        public static readonly Type WeakReference = typeof(ReuseWeakReference);
        /// <summary>Type of <see cref="ReuseRecyclable"/> added for intellisense discoverability.</summary>
        public static readonly Type Recyclable = typeof(ReuseRecyclable);
        /// <summary>Type of <see cref="ReuseSwapable"/> added for intellisense discoverability.</summary>
        public static readonly Type Swapable = typeof(ReuseSwapable);

        /// <summary>Unwraps input until target of <typeparamref name="T"/> is found. Returns found target, otherwise returns null.</summary>
        /// <typeparam name="T">Target to stop search on.</typeparam>
        /// <param name="reuseWrapper">Source reused wrapper to get target from.</param>
        public static T TargetOrDefault<T>(this IReuseWrapper reuseWrapper) where T : class
        {
            var target = reuseWrapper.ThrowIfNull().Target;
            while (!(target is T) && (target is IReuseWrapper))
                target = ((IReuseWrapper)target).Target;
            return target as T;
        }
    }

    /// <summary>Marker interface used by Scope to skip dispose for reused disposable object.</summary>
    public interface IHideDisposableFromContainer { }

    /// <summary>Wraps reused service object to prevent container to dispose service object. Intended to work only with <see cref="IDisposable"/> target.</summary>
    public class ReuseHiddenDisposable : IReuseWrapper, IHideDisposableFromContainer
    {
        /// <summary>Constructs wrapper by wrapping input target.</summary>
        /// <param name="target">Disposable target.</param>
        public ReuseHiddenDisposable(IDisposable target)
        {
            _target = target;
            _targetType = target.GetType();
        }

        /// <summary>Wrapped value.</summary>
        public object Target
        {
            get
            {
                Throw.If(IsDisposed, Error.TARGET_WAS_ALREADY_DISPOSED, _targetType, typeof(ReuseHiddenDisposable));
                return _target;
            }
        }

        /// <summary>True if target was disposed.</summary>
        public bool IsDisposed
        {
            get { return _disposed == 1; }
        }

        /// <summary>Dispose target and mark wrapper as disposed.</summary>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
                return;
            _target.Dispose();
            _target = null;
        }

        #region Implementation

        private int _disposed;
        private IDisposable _target;
        private readonly Type _targetType;

        #endregion
    }

    /// <summary>Wraps reused object as <see cref="WeakReference"/>. Allow wrapped object to be garbage collected.</summary>
    public class ReuseWeakReference : IReuseWrapper
    {
        /// <summary>Provides access to <see cref="WeakReference"/> members.</summary>
        public readonly WeakReference Ref;

        /// <summary>Wrapped value, delegates to <see cref="WeakReference.Target"/></summary>
        public object Target { get { return Ref.Target; } }

        /// <summary>Wraps input target into weak reference</summary> <param name="value">Value to wrap.</param>
        public ReuseWeakReference(object value)
        {
            Ref = new WeakReference(value);
        }
    }

    /// <summary>Wraps reused value ref box with ability to Swap it new value. Similar to <see cref="Ref{T}"/>.</summary>
    public sealed class ReuseSwapable : IReuseWrapper
    {
        /// <summary>Wrapped value.</summary>
        public object Target { get { return _value; } }

        /// <summary>Constructs ref wrapper.</summary> <param name="value">Wrapped value.</param>
        public ReuseSwapable(object value)
        {
            _value = value;
        }

        /// <summary>Exchanges currently hold object with <paramref name="getValue"/> result.</summary>
        /// <param name="getValue">Delegate to produce new object value from current one passed as parameter.</param>
        /// <returns>Returns old object value the same way as <see cref="Interlocked.Exchange(ref int,int)"/></returns>
        /// <remarks>Important: <paramref name="getValue"/> delegate may be called multiple times with new value each time, 
        /// if it was changed in meantime by other concurrently running code.</remarks>
        public object Swap(Func<object, object> getValue)
        {
            return Ref.Swap(ref _value, getValue);
        }

        /// <summary>Simplified version of Swap ignoring old value.</summary> <param name="newValue">New value.</param> <returns>Old value.</returns>
        public object Swap(object newValue)
        {
            return Interlocked.Exchange(ref _value, newValue);
        }

        private object _value;
    }

    /// <summary>If recycled set to True, that command Scope to create and return new value on next access.</summary>
    public interface IRecyclable
    {
        /// <summary>Indicates that value should be recycled.</summary>
        bool IsRecycled { get; }

        /// <summary>Commands to recycle value.</summary>
        void Recycle();
    }

    /// <summary>Wraps value with ability to be recycled, so next access to recycle value with create new value from Container.</summary>
    public class ReuseRecyclable : IReuseWrapper, IRecyclable
    {
        /// <summary>Wraps input value</summary> <param name="value"></param>
        public ReuseRecyclable(object value)
        {
            _value = value;
        }

        /// <summary>Returns wrapped value.</summary>
        public object Target
        {
            get { return _value; }
        }

        /// <summary>Indicates that value should be recycled.</summary>
        public bool IsRecycled { get; private set; }

        /// <summary>Commands to recycle value.</summary>
        public void Recycle()
        {
            IsRecycled = true;
        }

        private readonly object _value;
    }

    /// <summary>Specifies what to return when <see cref="IResolver"/> unable to resolve service.</summary>
    public enum IfUnresolved
    {
        /// <summary>Specifies to throw <see cref="ContainerException"/> if no service found.</summary>
        Throw,
        /// <summary>Specifies to return default value instead of throwing error.</summary>
        ReturnDefault
    }

    /// <summary>Declares minimal API for service resolution.
    /// The user friendly convenient methods are implemented as extension methods in <see cref="Resolver"/> class.</summary>
    /// <remarks>Resolve default and keyed is separated because of micro optimization for faster resolution.</remarks>
    public interface IResolver
    {
        /// <summary>Resolves service from container and returns created service object.</summary>
        /// <param name="serviceType">Service type to search and to return.</param>
        /// <param name="ifUnresolved">Says what to do if service is unresolved.</param>
        /// <param name="scope">Propagated resolution scope.</param>
        /// <returns>Created service object or default based on <paramref name="ifUnresolved"/> provided.</returns>
        object ResolveDefault(Type serviceType, IfUnresolved ifUnresolved, IScope scope);

        /// <summary>Resolves service from container and returns created service object.</summary>
        /// <param name="serviceType">Service type to search and to return.</param>
        /// <param name="serviceKey">Optional service key used for registering service.</param>
        /// <param name="ifUnresolved">Says what to do if service is unresolved.</param>
        /// <param name="requiredServiceType">Actual registered service type to use instead of <paramref name="serviceType"/>, 
        ///     or wrapped type for generic wrappers.  The type should be assignable to return <paramref name="serviceType"/>.</param>
        /// <param name="scope">Propagated resolution scope.</param>
        /// <returns>Created service object or default based on <paramref name="ifUnresolved"/> provided.</returns>
        /// <remarks>
        /// This method covers all possible resolution input parameters comparing to <see cref="ResolveDefault"/>, and
        /// by specifying the same parameters as for <see cref="ResolveDefault"/> should return the same result.
        /// </remarks>
        object ResolveKeyed(Type serviceType, object serviceKey, IfUnresolved ifUnresolved, Type requiredServiceType, IScope scope);

        /// <summary>Resolve all services registered for specified <paramref name="serviceType"/>, or if not found returns
        /// empty enumerable. If <paramref name="serviceType"/> specified then returns only (single) service registered with
        /// this type. Excludes for result composite parent identified by <paramref name="compositeParentKey"/>.</summary>
        /// <param name="serviceType">Return type of an service item.</param>
        /// <param name="serviceKey">(optional) Resolve only single service registered with the key.</param>
        /// <param name="requiredServiceType">(optional) Actual registered service to search for.</param>
        /// <param name="compositeParentKey">(optional) Parent service key to exclude to support Composite pattern.</param>
        /// <param name="scope">propagated resolution scope, may be null.</param>
        /// <returns>Enumerable of found services or empty. Does Not throw if no service found.</returns>
        IEnumerable<object> ResolveMany(Type serviceType, object serviceKey, Type requiredServiceType, object compositeParentKey, IScope scope);
    }

    /// <summary>Specifies options to handle situation when registering some service already present in the registry.</summary>
    public enum IfAlreadyRegistered
    {
        /// <summary>Appends new default registration or throws registration with the same key.</summary>
        AppendNotKeyed,
        /// <summary>Throws if default or registration with the same key is already exist.</summary>
        Throw,
        /// <summary>Keeps old default or keyed registration ignoring new registration: ensures Register-Once semantics.</summary>
        Keep,
        /// <summary>Replaces old registration with new one.</summary>
        Replace
    }

    /// <summary>Define registered service structure.</summary>
    [DebuggerDisplay("{ServiceType}; {OptionalServiceKey}; {Factory}")]
    public struct ServiceTypeKeyFactory
    {
        /// <summary>Required service type.</summary>
        public Type ServiceType;

        /// <summary>Optional service key, may be an instance of <see cref="DefaultKey"/> for multiple default registrations.</summary>
        public object OptionalServiceKey;

        /// <summary>Registered factory.</summary>
        public Factory Factory;
    }

    /// <summary>Defines operations that for changing registry, and checking if something exist in registry.</summary>
    public interface IRegistrator
    {
        /// <summary>Returns all registered service factories with their Type and optional Key.</summary>
        /// <returns>Existing registrations.</returns>
        /// <remarks>Decorator and Wrapper types are not included.</remarks>
        IEnumerable<ServiceTypeKeyFactory> GetServiceRegistrations();

        /// <summary>Registers factory in registry with specified service type and key for lookup.</summary>
        /// <param name="factory">To register.</param>
        /// <param name="serviceType">Service type as unique key in registry for lookup.</param>
        /// <param name="serviceKey">Service key as complementary lookup for the same service type.</param>
        /// <param name="ifAlreadyRegistered">Policy how to deal with already registered factory with same service type and key.</param>
        /// <returns>True if factory was added to registry, false otherwise. 
        /// False may be in case of <see cref="IfAlreadyRegistered.Keep"/> setting and already existing factory.</returns>
        bool Register(Factory factory, Type serviceType, object serviceKey, IfAlreadyRegistered ifAlreadyRegistered);

        /// <summary>Returns true if expected factory is registered with specified service key and type.</summary>
        /// <param name="serviceType">Type to lookup.</param>
        /// <param name="serviceKey">Key to lookup for the same type.</param>
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

    /// <summary>Provides access to both resolver and scopes to <see cref="FactoryDelegate"/>.</summary>
    public interface IResolverContext : IResolver
    {
        /// <summary>Scope associated with container.</summary>
        IScope SingletonScope { get; }

        /// <summary>Gets current scope matching the <paramref name="name"/>. 
        /// If name is null then current scope is returned, or if there is no current scope then exception thrown.</summary>
        /// <param name="name">May be null</param> <returns>Found scope or throws exception.</returns>
        /// <param name="throwIfNotFound">Says to throw if no scope found.</param>
        /// <exception cref="ContainerException"> with code <see cref="Error.NO_MATCHED_SCOPE_FOUND"/>.</exception>
        IScope GetCurrentNamedScope(object name, bool throwIfNotFound);

        /// <summary>Check if scope is not null, then just returns it, otherwise will create and return it.</summary>
        /// <param name="scope">May be null scope.</param>
        /// <param name="serviceType">Marking scope with resolved service type.</param> 
        /// <param name="serviceKey">Marking scope with resolved service key.</param>
        /// <returns>Input <paramref name="scope"/> ensuring it is not null.</returns>
        IScope GetOrCreateResolutionScope(ref IScope scope, Type serviceType, object serviceKey);

        /// <summary>If both <paramref name="assignableFromServiceType"/> and <paramref name="serviceKey"/> are null, 
        /// then returns input <paramref name="scope"/>.
        /// Otherwise searches scope hierarchy to find first scope with: Type assignable <paramref name="assignableFromServiceType"/> and 
        /// Key equal to <paramref name="serviceKey"/>.</summary>
        /// <param name="scope">Scope to start matching with Type and Key specified.</param>
        /// <param name="assignableFromServiceType">Type to match.</param> <param name="serviceKey">Key to match.</param>
        /// <param name="outermost">If true - commands to look for outermost match instead of nearest.</param>
        /// <param name="throwIfNotFound">Says to throw if no scope found.</param>
        /// <returns>Matching scope or throws <see cref="ContainerException"/>.</returns>
        IScope GetMatchingResolutionScope(IScope scope, Type assignableFromServiceType, object serviceKey, bool outermost, bool throwIfNotFound);
    }

    /// <summary>Exposes operations required for internal registry access. 
    /// That's why most of them are implemented explicitly by <see cref="Container"/>.</summary>
    public interface IContainer : IRegistrator, IResolver, IResolverContext, IDisposable
    {
        /// <summary>Returns true if container is disposed.</summary>
        bool IsDisposed { get; }

        /// <summary>Self weak reference, with readable message when container is GCed/Disposed.</summary>
        ContainerWeakRef ContainerWeakRef { get; }

        /// <summary>Rules for defining resolution/registration behavior throughout container.</summary>
        Rules Rules { get; }

        /// <summary>Empty request bound to container. All other requests are created by pushing to empty request.</summary>
        Request EmptyRequest { get; }

        /// <summary>State item objects which may include: singleton instances for fast access, reuses, reuse wrappers, factory delegates, etc.</summary>
        AppendableArray ResolutionStateCache { get; }

        /// <summary>Copies all of container state except Cache and specifies new rules.</summary>
        /// <param name="configure">(optional) Configure rules, if not specified then uses Rules from current container.</param> 
        /// <param name="scopeContext">(optional) New scope context, if not specified then uses context from current container.</param>
        /// <returns>New container.</returns>
        IContainer With(Func<Rules, Rules> configure = null, IScopeContext scopeContext = null);

        /// <summary>Returns new container with all expression, delegate, items cache removed/reset.
        /// It will preserve resolved services in Singleton/Current scope.</summary>
        /// <returns>New container with empty cache.</returns>
        IContainer WithoutCache();

        /// <summary>Creates new container with whole state shared with original except singletons.</summary>
        /// <returns>New container with empty Singleton Scope.</returns>
        IContainer WithoutSingletonsAndCache();

        /// <summary>Shares all parts with original container But copies registration, so the new registration
        /// won't be visible in original. Registrations include decorators and wrappers as well.</summary>
        /// <param name="preserveCache">(optional) If set preserves cache if you know what to do.</param>
        /// <returns>New container with copy of all registrations.</returns>
        IContainer WithRegistrationsCopy(bool preserveCache = false);

        /// <summary>Container opened scope. May or may not be equal to Current Scope.</summary>
        IScope OpenedScope { get; }

        /// <summary>Returns scope context associated with container.</summary>
        IScopeContext ScopeContext { get; }

        /// <summary>Creates new container with new opened scope and set this scope as current in ambient scope context.</summary>
        /// <param name="name">(optional) Name for opened scope to allow reuse to identify the scope.</param>
        /// <param name="configure">(optional) Configure rules, if not specified then uses Rules from current container.</param> 
        /// <returns>New container with different current scope.</returns>
        /// <example><code lang="cs"><![CDATA[
        /// using (var scoped = container.OpenScope())
        /// {
        ///     var handler = scoped.Resolve<IHandler>();
        ///     handler.Handle(data);
        /// }
        /// ]]></code></example>
        IContainer OpenScope(object name = null, Func<Rules, Rules> configure = null);

        /// <summary>Creates scoped container with scope bound to container itself, and not some ambient context.
        /// Current container scope will become parent for new scope.</summary>
        /// <param name="scopeName">(optional) Scope name.</param>
        /// <returns>New container with all state shared except new created scope and context.</returns>
        IContainer OpenScopeWithoutContext(object scopeName = null);

        /// <summary>Creates container (facade) that fallbacks to this container for unresolved services.
        /// Facade shares rules with this container, everything else is its own. 
        /// It could be used for instance to create Test facade over original container with replacing some services with test ones.</summary>
        /// <remarks>Singletons from container are not reused by facade, to achieve that rather use <see cref="OpenScope"/> with <see cref="Reuse.InCurrentScope"/>.</remarks>
        /// <returns>New facade container.</returns>
        IContainer CreateFacade();

        /// <summary>Searches for requested factory in registry, and then using <see cref="DryIoc.Rules.UnknownServiceResolvers"/>.</summary>
        /// <param name="request">Factory request.</param>
        /// <returns>Found factory, otherwise null if <see cref="Request.IfUnresolved"/> is set to <see cref="IfUnresolved.ReturnDefault"/>.</returns>
        Factory ResolveFactory(Request request);

        /// <summary>Searches for registered service factory and returns it, or null if not found.</summary>
        /// <param name="request">Factory request.</param>
        /// <returns>Found registered factory or null.</returns>
        Factory GetServiceFactoryOrDefault(Request request);

        /// <summary>Finds all registered default and keyed service factories and returns them.
        /// It skips decorators and wrappers.</summary>
        /// <param name="serviceType"></param>
        /// <returns>Enumerable of found pairs.</returns>
        /// <remarks>Returned Key item should not be null - it should be <see cref="DefaultKey.Value"/>.</remarks>
        IEnumerable<KV<object, Factory>> GetAllServiceFactories(Type serviceType);

        /// <summary>Searches for registered wrapper factory and returns it, or null if not found.</summary>
        /// <param name="serviceType">Service type to look for.</param> <returns>Found wrapper factory or null.</returns>
        Factory GetWrapperFactoryOrDefault(Type serviceType);

        /// <summary>Creates decorator expression: it could be either Func{TService,TService}, 
        /// or service expression for replacing decorators.</summary>
        /// <param name="request">Decorated service request.</param>
        /// <returns>Decorator expression.</returns>
        Expression GetDecoratorExpressionOrDefault(Request request);

        /// <summary>For given instance resolves and sets properties and fields.</summary>
        /// <param name="instance">Service instance with properties to resolve and initialize.</param>
        /// <param name="propertiesAndFields">(optional) Function to select properties and fields, overrides all other rules if specified.</param>
        /// <returns>Instance with assigned properties and fields.</returns>
        /// <remarks>Different Rules could be combined together using <see cref="PropertiesAndFields.And"/> method.</remarks>     
        object InjectPropertiesAndFields(object instance, PropertiesAndFieldsSelector propertiesAndFields);

        /// <summary>If <paramref name="type"/> is generic type then this method checks if the type registered as generic wrapper,
        /// and recursively unwraps and returns its type argument. This type argument is the actual service type we want to find.
        /// Otherwise, method returns the input <paramref name="type"/>.</summary>
        /// <param name="type">Type to unwrap. Method will return early if type is not generic.</param>
        /// <returns>Unwrapped service type in case it corresponds to registered generic wrapper, or input type in all other cases.</returns>
        Type UnwrapServiceType(Type type);

        /// <summary>Adds factory expression to cache identified by factory ID (<see cref="Factory.FactoryID"/>).</summary>
        /// <param name="factoryID">Key in cache.</param>
        /// <param name="factoryExpression">Value to cache.</param>
        void CacheFactoryExpression(int factoryID, Expression factoryExpression);

        /// <summary>Searches and returns cached factory expression, or null if not found.</summary>
        /// <param name="factoryID">Factory ID to lookup by.</param> <returns>Found expression or null.</returns>
        Expression GetCachedFactoryExpressionOrDefault(int factoryID);

        /// <summary>If possible wraps added item in <see cref="ConstantExpression"/> (possible for primitive type, Type, strings), 
        /// otherwise invokes <see cref="Container.GetOrAddStateItem"/> and wraps access to added item (by returned index) into expression: state => state.Get(index).</summary>
        /// <param name="item">Item to wrap or to add.</param> <param name="itemType">(optional) Specific type of item, otherwise item <see cref="object.GetType()"/>.</param>
        /// <param name="throwIfStateRequired">(optional) Enable filtering of stateful items.</param>
        /// <returns>Returns constant or state access expression for added items.</returns>
        Expression GetOrAddStateItemExpression(object item, Type itemType = null, bool throwIfStateRequired = false);

        /// <summary>Adds item if it is not already added to state, returns added or existing item index.</summary>
        /// <param name="item">Item to find in existing items with <see cref="object.Equals(object, object)"/> or add if not found.</param>
        /// <returns>Index of found or added item.</returns>
        int GetOrAddStateItem(object item);
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

        /// <summary>Return items enumerator.</summary> <returns>items enumerator.</returns>
        public IEnumerator<TService> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
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

    /// <summary>Used to wrap resolution scope together with directly resolved service value.
    /// Disposing wrapper means disposing service (if disposable) and disposing scope (all reused disposable dependencies.)</summary>
    /// <typeparam name="T">Type of resolved service.</typeparam>
    public sealed class CaptureResolutionScope<T> : IDisposable
    {
        /// <summary>Resolved service.</summary>
        public T Value { get; private set; }

        /// <summary>Exposes resolution scope. The supported operation for it is <see cref="IDisposable.Dispose"/>.
        /// So you can dispose scope separately from resolved service.</summary>
        public IDisposable Scope { get; private set; }

        /// <summary>Creates wrapper</summary>
        /// <param name="value">Resolved service.</param> <param name="scope">Resolution root scope.</param>
        public CaptureResolutionScope(T value, IScope scope)
        {
            Value = value;
            Scope = scope as IDisposable;
        }

        /// <summary>Disposes both resolved service (if disposable) and then disposes resolution scope.</summary>
        public void Dispose()
        {
            var disposable = Value as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
                Value = default(T);
            }

            if (Scope != null)
            {
                Scope.Dispose();
                Scope = null;
            }
        }
    }

    /// <summary>Wraps factory expression created by container internally. May be used for debugging.</summary>
    /// <typeparam name="TService">Service type to resolve.</typeparam>
    [DebuggerDisplay("{Value}")]
    public sealed class FactoryExpression<TService>
    {
        /// <summary>Factory expression that Container compiles to delegate.</summary>
        public readonly Expression<FactoryDelegate> Value;

        /// <summary>Creates wrapper.</summary> <param name="value">Wrapped expression.</param>
        public FactoryExpression(Expression<FactoryDelegate> value)
        {
            Value = value;
        }
    }

    /// <summary>Exception that container throws in case of error. Dedicated exception type simplifies
    /// filtering or catching container relevant exceptions from client code.</summary>
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
    public class ContainerException : InvalidOperationException
    {
        /// <summary>Error code of exception, possible values are listed in <see cref="Error"/> class.</summary>
        public readonly int Error;

        /// <summary>Creates exception by wrapping <paramref name="errorCode"/> and its message,
        /// optionally with <paramref name="inner"/> exception.</summary>
        /// <param name="errorCheck">Type of check</param>
        /// <param name="errorCode">Error code, check <see cref="Error"/> for possible values.</param>
        /// <param name="arg0">(optional) Arguments for formatted message.</param> <param name="arg1"></param> <param name="arg2"></param> <param name="arg3"></param>
        /// <param name="inner">(optional) Inner exception.</param>
        /// <returns>Created exception.</returns>
        public static ContainerException Of(ErrorCheck errorCheck, int errorCode,
            object arg0, object arg1 = null, object arg2 = null, object arg3 = null,
            Exception inner = null)
        {
            string message = null;
            if (errorCode != -1)
                message = string.Format(DryIoc.Error.Messages[errorCode], Print(arg0), Print(arg1), Print(arg2), Print(arg3));
            else
            {
                switch (errorCheck) // handle error check when error code is unspecified.
                {
                    case ErrorCheck.InvalidCondition:
                        errorCode = DryIoc.Error.INVALID_CONDITION;
                        message = string.Format(DryIoc.Error.Messages[errorCode], Print(arg0), Print(arg0.GetType()));
                        break;
                    case ErrorCheck.IsNull:
                        errorCode = DryIoc.Error.IS_NULL;
                        message = string.Format(DryIoc.Error.Messages[errorCode], Print(arg0));
                        break;
                    case ErrorCheck.IsNotOfType:
                        errorCode = DryIoc.Error.IS_NOT_OF_TYPE;
                        message = string.Format(DryIoc.Error.Messages[errorCode], Print(arg0), Print(arg1));
                        break;
                    case ErrorCheck.TypeIsNotOfType:
                        errorCode = DryIoc.Error.TYPE_IS_NOT_OF_TYPE;
                        message = string.Format(DryIoc.Error.Messages[errorCode], Print(arg0), Print(arg1));
                        break;
                }
            }

            return inner == null
                ? new ContainerException(errorCode, message)
                : new ContainerException(errorCode, message, inner);
        }

        /// <summary>Creates exception with message describing cause and context of error.</summary>
        /// <param name="error"></param>
        /// <param name="message">Error message.</param>
        protected ContainerException(int error, string message)
            : base(message)
        {
            Error = error;
        }

        /// <summary>Creates exception with message describing cause and context of error,
        /// and leading/system exception causing it.</summary>
        /// <param name="error"></param>
        /// <param name="message">Error message.</param>
        /// <param name="innerException">Underlying system/leading exception.</param>
        protected ContainerException(int error, string message, Exception innerException)
            : base(message, innerException)
        {
            Error = error;
        }

        /// <summary>Prints argument for formatted message.</summary> <param name="arg">To print.</param> <returns>Printed string.</returns>
        protected static string Print(object arg)
        {
            return new StringBuilder().Print(arg).ToString();
        }
    }

    /// <summary>Defines error codes and error messages for all DryIoc exceptions (DryIoc extensions may define their own.)</summary>
    public static class Error
    {
        /// <summary>First error code to identify error range for other possible error code definitions.</summary>
        public readonly static int FIRST_ERROR_CODE = 0;

        /// <summary>List of error messages indexed with code.</summary>
        public readonly static List<string> Messages = new List<string>(100);

#pragma warning disable 1591 // "Missing XML-comment"
        public static readonly int
            INVALID_CONDITION = Of("Argument {0} of type {1} has invalid condition."),
            IS_NULL = Of("Argument of type {0} is null."),
            IS_NOT_OF_TYPE = Of("Argument {0} is not of type {1}."),
            TYPE_IS_NOT_OF_TYPE = Of("Type argument {0} is not assignable from type {1}."),

            UNABLE_TO_RESOLVE_SERVICE = Of(
                "Unable to resolve {0}." + Environment.NewLine +
                "Please ensure you have service registered (with proper key) - 95% of cases." + Environment.NewLine +
                "Remaining 5%: Service does not match the reuse scope, or service has wrong Setup.With(condition), or no Rules.WithUnknownServiceResolver(ForMyService)."),
            EXPECTED_SINGLE_DEFAULT_FACTORY = Of(
                "Expecting single default registration of {0} but found many:" + Environment.NewLine + "{1}." + Environment.NewLine +
                "Please identify service with key, or metadata, or use Rules.WithFactorySelector to specify single registered factory."),
            IMPLEMENTATION_NOT_ASSIGNABLE_TO_SERVICE_TYPE = Of(
                "Implementation type {0} should be assignable to service type {1} but it is not."),
            MADE_OF_TYPE_NOT_ASSIGNABLE_TO_IMPLEMENTATION_TYPE = Of(
                "Factory method made-of-type {1} should be assignable to implementation type {0} but it is not."),
            REGISTERING_OPEN_GENERIC_REQUIRES_FACTORY_PROVIDER = Of(
                "Unable to register not a factory provider for open-generic service {0}."),
            REGISTERING_OPEN_GENERIC_IMPL_WITH_NON_GENERIC_SERVICE = Of(
                "Unable to register open-generic implementation {0} with non-generic service {1}."),
            REGISTERING_OPEN_GENERIC_SERVICE_WITH_MISSING_TYPE_ARGS = Of(
                "Unable to register open-generic implementation {0} because service {1} should specify all type arguments, but specifies only {2}."),
            REGISTERING_NOT_A_GENERIC_TYPEDEF_IMPL_TYPE = Of(
                "Unsupported registration of implementation {0} which is not a generic type definition but contains generic parameters." + Environment.NewLine +
                "Consider to register generic type definition {1} instead."),
            REGISTERING_NOT_A_GENERIC_TYPEDEF_SERVICE_TYPE = Of(
                "Unsupported registration of service {0} which is not a generic type definition but contains generic parameters." + Environment.NewLine + 
                "Consider to register generic type definition {1} instead."),
            EXPECTED_NON_ABSTRACT_IMPL_TYPE = Of(
                "Expecting not abstract and not interface implementation type, but found {0}."),
            NO_PUBLIC_CONSTRUCTOR_DEFINED = Of(
                "There is no public constructor defined for {0}."),
            NO_DEFINED_METHOD_TO_SELECT_FROM_MULTIPLE_CONSTRUCTORS = Of(
                "Unspecified how to select single constructor for implementation type {0} with {1} public constructors."),
            NO_MATCHED_IMPLEMENTED_TYPES_WITH_SERVICE_TYPE = Of(
                "Unable to match service with open-generic {0} implementing {1} when resolving {2}."),
            CTOR_IS_MISSING_SOME_PARAMETERS = Of(
                "Constructor [{0}] of {1} misses some arguments required for {2} dependency."),
            UNABLE_TO_SELECT_CONSTRUCTOR = Of(
                "Unable to select single constructor from {0} available in {1}." + Environment.NewLine
                + "Please provide constructor selector when registering service."),
            EXPECTED_FUNC_WITH_MULTIPLE_ARGS = Of(
                "Expecting Func with one or more arguments but found {0}."),
            EXPECTED_CLOSED_GENERIC_SERVICE_TYPE = Of(
                "Expecting closed-generic service type but found {0}."),
            RECURSIVE_DEPENDENCY_DETECTED = Of(
                "Recursive dependency is detected when resolving" + Environment.NewLine + "{0}."),
            SCOPE_IS_DISPOSED = Of(
                "Scope is disposed and scoped instances are no longer available."),
            WRAPPER_CAN_WRAP_SINGLE_SERVICE_ONLY = Of(
                "Wrapper {0} can wrap single service type only, but found many. You should specify service type selector in wrapper setup."),
            NOT_FOUND_OPEN_GENERIC_IMPL_TYPE_ARG_IN_SERVICE = Of(
                "Unable to find for open-generic implementation {0} the type argument {1} when resolving {2}."),
            UNABLE_TO_GET_CONSTRUCTOR_FROM_SELECTOR = Of(
                "Unable to get constructor of {0} using provided constructor selector."),
            UNABLE_TO_FIND_CTOR_WITH_ALL_RESOLVABLE_ARGS = Of(
                "Unable to find constructor with all resolvable parameters when resolving {0}."),
            UNABLE_TO_FIND_MATCHING_CTOR_FOR_FUNC_WITH_ARGS = Of(
                "Unable to find constructor with all parameters matching Func signature {0} " + Environment.NewLine
                + "and the rest of parameters resolvable from Container when resolving: {1}."),
            REGED_FACTORY_DLG_RESULT_NOT_OF_SERVICE_TYPE = Of(
                "Registered factory delegate returns service {0} is not assignable to {2}."),
            REGED_OBJ_NOT_ASSIGNABLE_TO_SERVICE_TYPE = Of(
                "Registered instance {0} is not assignable to serviceType {1}."),
            NOT_FOUND_SPECIFIED_WRITEABLE_PROPERTY_OR_FIELD = Of(
                "Unable to find writable property or field \"{0}\" when resolving: {1}."),
            PUSHING_TO_REQUEST_WITHOUT_FACTORY = Of(
                "Pushing next info {0} to request not yet resolved to factory: {1}"),
            TARGET_WAS_ALREADY_DISPOSED = Of(
                "Target {0} was already disposed in {1} wrapper."),
            NO_MATCHED_GENERIC_PARAM_CONSTRAINTS = Of(
                "Service type does not match registered open-generic implementation constraints {0} when resolving {1}."),
            NO_WRAPPED_TYPE_FOR_NON_GENERIC_WRAPPER = Of(
                "Non-generic wrapper {0} should specify wrapped service selector when registered."),
            DEPENDENCY_HAS_SHORTER_REUSE_LIFESPAN = Of(
                "Dependency {0} has shorter Reuse lifespan than its parent: {1}." + Environment.NewLine +
                "{2} lifetime is shorter than {3}." + Environment.NewLine +
                "You may turn Off this error with new Container(rules=>rules.EnableThrowIfDepenedencyHasShorterReuseLifespan(false))."),
            WEAKREF_REUSE_WRAPPER_GCED = Of(
                "Service with WeakReference reuse wrapper is garbage collected now, and no longer available."),
            SERVICE_IS_NOT_ASSIGNABLE_FROM_FACTORY_METHOD = Of(
                "Service of {0} is not assignable from factory method {2} when resolving: {3}."),
            FACTORY_OBJ_IS_NULL_IN_FACTORY_METHOD = Of(
                "Unable to use null factory object with factory method {0} when resolving: {1}."),
            FACTORY_OBJ_PROVIDED_BUT_METHOD_IS_STATIC = Of(
                "Factory instance provided {0} But factory method is static {1} when resolving: {2}."),
            NO_OPEN_THREAD_SCOPE = Of(
                "Unable to find open thread scope in {0}. Please OpenScope with {0} to make sure thread reuse work."),
            CONTAINER_IS_GARBAGE_COLLECTED = Of(
                "Container is no longer available (has been garbage-collected)."),
            CANT_CREATE_DECORATOR_EXPR = Of(
                "Unable to create decorator expression for: {0}."),
            UNABLE_TO_REGISTER_DUPLICATE_DEFAULT = Of(
                "Service {0} without key is already registered as {2}."),
            UNABLE_TO_REGISTER_DUPLICATE_KEY = Of(
                "Service {0} with the same key \"{1}\" is already registered as {2}."),
            NO_CURRENT_SCOPE = Of(
                "No current scope available: probably you are registering to, or resolving from outside of scope."),
            CONTAINER_IS_DISPOSED = Of(
                "Container is disposed and its operations are no longer available."),
            NOT_DIRECT_SCOPE_PARENT = Of(
                "Unable to OpenScope [{0}] because parent scope [{1}] is not current context scope [{2}]." + Environment.NewLine +
                "It is probably other scope was opened in between OR you forgot to Dispose some other scope!"),
            UNABLE_TO_RESOLVE_REUSE_WRAPPER = Of(
                "Unable to resolve reuse wrapper {0} for: {1}"),
            WRAPPED_NOT_ASSIGNABLE_FROM_REQUIRED_TYPE = Of(
                "Service (wrapped) type {0} is not assignable from required service type {1} when resolving {2}."),
            NO_MATCHED_SCOPE_FOUND = Of(
                "Unable to find scope with matching name: {0}."),
            UNABLE_TO_NEW_OPEN_GENERIC = Of(
                "Unable to New not concrete/open-generic type {0}."),
            REG_REUSED_OBJ_WRAPPER_IS_NOT_IREUSED = Of(
                "Registered reuse wrapper {1} at index {2} of {3} does not implement expected {0} interface."),
            RECYCLABLE_REUSE_WRAPPER_IS_RECYCLED = Of(
                "Recyclable wrapper is recycled."),
            NO_SCOPE_WHEN_REGISTERING_INSTANCE = Of(
                "No scope is available when registering instance [{0}] with [{1}]." + Environment.NewLine + 
                "You can register delegate returning instance instead, if scope will be available at resolution."),
            UNEXPECTED_EXPRESSION_TO_MAKE_SERVICE = Of(
                "Only expression of method call, property getter, or new statement (with optional property initializer) is supported, but found: {0}."),
            UNEXPECTED_FACTORY_MEMBER_EXPRESSION = Of(
                "Expected property getter, but found {0}."),
            UNEXPECTED_EXPRESSION_INSTEAD_OF_ARG_METHOD = Of(
                "Expected DryIoc.Arg method call to specify parameter/property/field, but found: {0}."),
            UNEXPECTED_EXPRESSION_INSTEAD_OF_CONSTANT = Of(
                "Expected constant expression to specify parameter/property/field, but found something else: {0}."),
            INJECTED_CUSTOM_VALUE_IS_OF_DIFFERENT_TYPE = Of(
                "Injected value {0} is not assignable to {2}."),
            STATE_IS_REQUIRED_TO_USE_ITEM = Of(
                "State is required to use (probably to inject) item {0}.");
#pragma warning restore 1591 // "Missing XML-comment"

        /// <summary>Stores new error message and returns error code for it.</summary>
        /// <param name="message">Error message to store.</param> <returns>Error code for message.</returns>
        public static int Of(string message)
        {
            Messages.Add(message);
            return FIRST_ERROR_CODE + Messages.Count - 1;
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
        /// <summary>Invoked operation throw, it is source of inner exception.</summary>
        OperationThrows,
    }

    /// <summary>Enables more clean error message formatting and a bit of code contracts.</summary>
    public static partial class Throw
    {
        /// <summary>Declares mapping between <see cref="ErrorCheck"/> type and <paramref name="error"/> code to specific <see cref="Exception"/>.</summary>
        /// <returns>Returns mapped exception.</returns>
        public delegate Exception GetMatchedExceptionHandler(ErrorCheck errorCheck, int error, object arg0, object arg1, object arg2, object arg3, Exception inner);

        /// <summary>Returns matched exception (to check type and error code). By default return <see cref="ContainerException"/>.</summary>
        public static GetMatchedExceptionHandler GetMatchedException = ContainerException.Of;

        /// <summary>Throws matched exception if throw condition is true.</summary>
        /// <param name="throwCondition">Condition to be evaluated, throws if result is true, otherwise - does nothing.</param>
        /// <param name="error">Error code to match to exception thrown.</param>
        /// <param name="arg0">Arguments to formatted message.</param> <param name="arg1"></param> <param name="arg2"></param> <param name="arg3"></param>
        public static void If(bool throwCondition, int error = -1, object arg0 = null, object arg1 = null, object arg2 = null, object arg3 = null)
        {
            if (!throwCondition) return;
            throw GetMatchedException(ErrorCheck.InvalidCondition, error, arg0, arg1, arg2, arg3, null);
        }

        /// <summary>Throws matched exception if throw condition is true. Otherwise return source <paramref name="arg0"/>.</summary>
        /// <typeparam name="T">Type of source <paramref name="arg0"/>.</typeparam>
        /// <param name="arg0">In case of exception <paramref name="arg0"/> will be used as first argument in formatted message.</param>
        /// <param name="throwCondition">Condition to be evaluated, throws if result is true, otherwise - does nothing.</param>
        /// <param name="error">Error code to match to exception thrown.</param>
        /// <param name="arg1">Rest of arguments to formatted message.</param> <param name="arg2"></param> <param name="arg3"></param>
        /// <returns><paramref name="arg0"/> if throw condition is false.</returns>
        public static T ThrowIf<T>(this T arg0, bool throwCondition, int error = -1, object arg1 = null, object arg2 = null, object arg3 = null)
        {
            if (!throwCondition) return arg0;
            throw GetMatchedException(ErrorCheck.InvalidCondition, error, arg0, arg1, arg2, arg3, null);
        }

        /// <summary>Throws matched exception if throw condition is true. Passes <paramref name="arg0"/> to condition. 
        /// Enables fluent syntax at cast of delegate creation. Otherwise return source <paramref name="arg0"/>.</summary>
        /// <typeparam name="T">Type of source <paramref name="arg0"/>.</typeparam>
        /// <param name="arg0">In case of exception <paramref name="arg0"/> will be used as first argument in formatted message.</param>
        /// <param name="throwCondition">Condition to be evaluated, throws if result is true, otherwise - does nothing.</param>
        /// <param name="error">Error code to match to exception thrown.</param>
        /// <param name="arg1">Rest of arguments to formatted message.</param> <param name="arg2"></param> <param name="arg3"></param>
        /// <returns><paramref name="arg0"/> if throw condition is false.</returns>
        public static T ThrowIf<T>(this T arg0, Func<T, bool> throwCondition, int error = -1, object arg1 = null, object arg2 = null, object arg3 = null)
        {
            if (!throwCondition(arg0)) return arg0;
            throw GetMatchedException(ErrorCheck.InvalidCondition, error, arg0, arg1, arg2, arg3, null);
        }

        /// <summary>Throws exception if <paramref name="arg"/> is null, otherwise returns <paramref name="arg"/>.</summary>
        /// <param name="arg">Argument to check for null.</param>
        /// <param name="error">Error code.</param>
        /// <param name="arg0"></param> <param name="arg1"></param> <param name="arg2"></param> <param name="arg3"></param>
        /// <typeparam name="T">Type of argument to check and return.</typeparam>
        /// <returns><paramref name="arg"/> if it is not null.</returns>
        public static T ThrowIfNull<T>(this T arg, int error = -1, object arg0 = null, object arg1 = null, object arg2 = null, object arg3 = null)
            where T : class
        {
            if (arg != null) return arg;
            throw GetMatchedException(ErrorCheck.IsNull, error, arg0 ?? typeof(T), arg1, arg2, arg3, null);
        }

        /// <summary>Throws exception if <paramref name="arg0"/> is not assignable to type specified by <paramref name="arg1"/>,
        /// otherwise just returns <paramref name="arg0"/>.</summary>
        /// <typeparam name="T">Type of argument to check and return if no error.</typeparam>
        /// <param name="arg0">Instance to check if it is assignable to type <paramref name="arg1"/>.</param>
        /// <param name="arg1">Type to check <paramref name="arg0"/> against.</param>
        /// <param name="error">Error code</param>
        /// <param name="arg2"></param> <param name="arg3"></param>
        /// <returns><paramref name="arg0"/> if it assignable to <paramref name="arg1"/>.</returns>
        public static T ThrowIfNotOf<T>(this T arg0, Type arg1, int error = -1, object arg2 = null, object arg3 = null)
            where T : class
        {
            if (arg1.IsTypeOf(arg0)) return arg0;
            throw GetMatchedException(ErrorCheck.IsNotOfType, error, arg0, arg1, arg2, arg3, null);
        }

        /// <summary>Throws if <paramref name="arg0"/> is not assignable from <paramref name="arg1"/>.</summary>
        /// <param name="arg0"></param> <param name="arg1"></param> 
        /// <param name="error">Error code</param>
        ///  <param name="arg2"></param> <param name="arg3"></param>
        /// <returns><paramref name="arg0"/> if no exception.</returns>
        public static Type ThrowIfNotOf(this Type arg0, Type arg1, int error = -1, object arg2 = null, object arg3 = null)
        {
            if (arg1.IsAssignableTo(arg0)) return arg0;
            throw GetMatchedException(ErrorCheck.TypeIsNotOfType, error, arg0, arg1, arg2, arg3, null);
        }

        /// <summary>Invokes <paramref name="operation"/> and in case of <typeparamref name="TEx"/> re-throws it as inner-exception.</summary>
        /// <typeparam name="TEx">Exception to check and handle, and then wrap as inner-exception.</typeparam>
        /// <typeparam name="T">Result of <paramref name="operation"/>.</typeparam>
        /// <param name="operation">To invoke</param> <param name="error">Error code</param>
        /// <param name="arg0"></param> <param name="arg1"></param> <param name="arg2"></param> <param name="arg3"></param>
        /// <returns>Result of <paramref name="operation"/> if no exception.</returns>
        public static T IfThrows<TEx, T>(Func<T> operation, int error, object arg0 = null, object arg1 = null,
            object arg2 = null, object arg3 = null) where TEx : Exception
        {
            try { return operation(); }
            catch (TEx ex) { throw GetMatchedException(ErrorCheck.OperationThrows, error, arg0, arg1, arg2, arg3, ex); }
        }

        /// <summary>Just throws the exception with the <paramref name="error"/> code.</summary>
        /// <param name="error">Error code.</param>
        /// <param name="arg0"></param> <param name="arg1"></param> <param name="arg2"></param> <param name="arg3"></param>
        public static void It(int error, object arg0 = null, object arg1 = null, object arg2 = null, object arg3 = null)
        {
            throw GetMatchedException(ErrorCheck.Unspecified, error, arg0, arg1, arg2, arg3, null);
        }

        /// <summary>Throws <paramref name="error"/> instead of returning value of <typeparamref name="T"/>. 
        /// Supposed to be used in expression that require some return value.</summary>
        /// <typeparam name="T"></typeparam> <param name="error"></param>
        /// <param name="arg0"></param> <param name="arg1"></param> <param name="arg2"></param> <param name="arg3"></param>
        /// <returns>Does not return, throws instead.</returns>
        public static T For<T>(int error, object arg0 = null, object arg1 = null, object arg2 = null, object arg3 = null)
        {
            throw GetMatchedException(ErrorCheck.Unspecified, error, arg0, arg1, arg2, arg3, null);
        }
    }

    /// <summary>Contains helper methods to work with Type: for instance to find Type implemented base types and interfaces, etc.</summary>
    public static class ReflectionTools
    {
        /// <summary>Flags for <see cref="GetImplementedTypes"/> method.</summary>
        [Flags]
        public enum IncludeImplementedType
        {
            /// <summary>Include nor object not source type.</summary>
            None = 0, 
            /// <summary>Include source type to list of implemented types.</summary>
            SourceType = 1,
            /// <summary>Include <see cref="System.Object"/> type to list of implemented types.</summary>
            ObjectType = 2
        }

        /// <summary>Returns all interfaces and all base types (in that order) implemented by <paramref name="sourceType"/>.
        /// Specify <paramref name="includeImplementedType"/> to include <paramref name="sourceType"/> itself as first item and 
        /// <see cref="object"/> type as the last item.</summary>
        /// <param name="sourceType">Source type for discovery.</param>
        /// <param name="includeImplementedType">Additional types to include into result collection.</param>
        /// <returns>Collection of found types.</returns>
        public static Type[] GetImplementedTypes(this Type sourceType, IncludeImplementedType includeImplementedType = IncludeImplementedType.None)
        {
            Type[] results;

            var interfaces = sourceType.GetImplementedInterfaces();
            var interfaceStartIndex = (includeImplementedType & IncludeImplementedType.SourceType) == 0 ? 0 : 1;
            var includingObjectType = (includeImplementedType & IncludeImplementedType.ObjectType) == 0 ? 0 : 1;
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
        /// <param name="type">Source type</param>
        /// <returns>Collection of interface types.</returns>
        public static Type[] GetImplementedInterfaces(this Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces.ToArrayOrSelf();
        }

        /// <summary>Returns true if <paramref name="type"/> contains all generic parameters from <paramref name="genericParams"/>.</summary>
        /// <param name="type">Expected to be open-generic type.</param>
        /// <param name="genericParams">Generic parameters.</param>
        /// <returns>Returns true if contains and false otherwise.</returns>
        public static bool ContainsAllGenericTypeParameters(this Type type, Type[] genericParams)
        {
            if (!type.IsOpenGeneric())
                return false;

            // NOTE: may be replaced with more lightweight Bits flags.
            var matchedParams = new Type[genericParams.Length];
            Array.Copy(genericParams, matchedParams, genericParams.Length);

            SetToNullGenericParametersReferencedInConstraints(matchedParams);
            SetToNullMatchesFoundInGenericParameters(matchedParams, type.GetGenericParamsAndArgs());

            for (var i = 0; i < matchedParams.Length; i++)
                if (matchedParams[i] != null)
                    return false;
            return true;
        }

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

        /// <summary>Returns true if class is compiler generated. Checking for CompilerGeneratedAttribute
        /// is not enough, because this attribute is not applied for classes generated from "async/await".</summary>
        /// <param name="type">Type to check.</param> <returns>Returns true if type is compiler generated.</returns>
        public static bool IsCompilerGenerated(this Type type)
        {
            return type.FullName != null && type.FullName.Contains("<>c__DisplayClass");
        }

        /// <summary>Returns true if type is generic.</summary><param name="type">Type to check.</param> <returns>True if type generic.</returns>
        public static bool IsGeneric(this Type type)
        {
            return type.GetTypeInfo().IsGenericType;
        }

        /// <summary>Returns true if type is generic type definition (open type).</summary><param name="type">Type to check.</param>
        /// <returns>True if type is open type: generic type definition.</returns>
        public static bool IsGenericDefinition(this Type type)
        {
            return type.GetTypeInfo().IsGenericTypeDefinition;
        }

        /// <summary>Returns true if type is closed generic: does not have open generic parameters, only closed/concrete ones.</summary>
        /// <param name="type">Type to check</param> <returns>True if closed generic.</returns>
        public static bool IsClosedGeneric(this Type type)
        {
            return type.GetTypeInfo().IsGenericType && !type.GetTypeInfo().ContainsGenericParameters;
        }

        /// <summary>Returns true if type if open generic: contains at list one open generic parameter. Could be
        /// generic type definition as well.</summary>
        /// <param name="type">Type to check.</param> <returns>True if open generic.</returns>
        public static bool IsOpenGeneric(this Type type)
        {
            return type.GetTypeInfo().IsGenericType && type.GetTypeInfo().ContainsGenericParameters;
        }

        /// <summary>Returns generic type definition if type is generic and null otherwise.</summary>
        /// <param name="type">Source type, could be null.</param> <returns>Generic type definition.</returns>
        public static Type GetGenericDefinitionOrNull(this Type type)
        {
            return type != null && type.GetTypeInfo().IsGenericType ? type.GetGenericTypeDefinition() : null;
        }

        /// <summary>Returns generic type parameters and arguments in order they specified. If type is not generic, returns empty array.</summary>
        /// <param name="type">Source type.</param> <returns>Array of generic type arguments (closed/concrete types) and parameters (open).</returns>
        public static Type[] GetGenericParamsAndArgs(this Type type)
        {
            return Portable.GetGenericArguments(type);
        }

        /// <summary>Returns array of interface and base class constraints for provider generic parameter type.</summary>
        /// <param name="type">Generic parameter type.</param>
        /// <returns>Array of interface and base class constraints.</returns>
        public static Type[] GetGenericParamConstraints(this Type type)
        {
            return type.GetTypeInfo().GetGenericParameterConstraints();
        }

        /// <summary>If type is array returns is element type, otherwise returns null.</summary>
        /// <param name="type">Source type.</param> <returns>Array element type or null.</returns>
        public static Type GetElementTypeOrNull(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsArray ? typeInfo.GetElementType() : null;
        }

        /// <summary>Return base type or null, if not exist (the case for only for object type).</summary> 
        /// <param name="type">Source type.</param> <returns>Base type or null for object.</returns>
        public static Type GetBaseType(this Type type)
        {
            return type.GetTypeInfo().BaseType;
        }

        /// <summary>Checks if type is public or nested public in public type.</summary>
        /// <param name="type">Type to check.</param> <returns>Return true if check succeeded.</returns>
        public static bool IsPublicOrNestedPublic(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsPublic || typeInfo.IsNestedPublic && typeInfo.DeclaringType.IsPublicOrNestedPublic();
        }

        /// <summary>Returns true if type is value type.</summary>
        /// <param name="type">Type to check.</param> <returns>Check result.</returns>
        public static bool IsValueType(this Type type)
        {
            return type.GetTypeInfo().IsValueType;
        }

        /// <summary>Returns true if type if abstract or interface.</summary>
        /// <param name="type">Type to check.</param> <returns>Check result.</returns>
        public static bool IsAbstract(this Type type)
        {
            return type.GetTypeInfo().IsAbstract;
        }

        /// <summary>Returns true if type is static.</summary>
        /// <param name="type">Type</param> <returns>True is static.</returns>
        public static bool IsStatic(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsAbstract && typeInfo.IsSealed;
        }

        /// <summary>Returns true if type is enum type.</summary>
        /// <param name="type">Type to check.</param> <returns>Check result.</returns>
        public static bool IsEnum(this Type type)
        {
            return type.GetTypeInfo().IsEnum;
        }

        /// <summary>Returns true if instance of type is assignable to instance of <paramref name="other"/> type.</summary>
        /// <param name="type">Type to check, could be null.</param> 
        /// <param name="other">Other type to check, could be null.</param>
        /// <returns>Check result.</returns>
        public static bool IsAssignableTo(this Type type, Type other)
        {
            return type != null && other != null && other.GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());
        }

        /// <summary>Returns true if type of <paramref name="obj"/> is assignable to source <paramref name="type"/>.</summary>
        /// <param name="type">Is type of object.</param> <param name="obj">Object to check.</param>
        /// <returns>Check result.</returns>
        public static bool IsTypeOf(this Type type, object obj)
        {
            return obj != null && obj.GetType().IsAssignableTo(type);
        }

        /// <summary>Returns true if provided type IsPitmitive in .Net terms, or enum, or string
        /// , or array of primitives if <paramref name="orArrayOfPrimitives"/> is true.</summary>
        /// <param name="type">Type to check.</param> 
        /// <param name="orArrayOfPrimitives">Says to return true for array or primitives recursively.</param>
        /// <returns>Check result.</returns>
        public static bool IsPrimitive(this Type type, bool orArrayOfPrimitives = false)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsPrimitive || typeInfo.IsEnum || type == typeof(string)
                || orArrayOfPrimitives && typeInfo.IsArray && typeInfo.GetElementType().IsPrimitive(true);
        }

        /// <summary>Returns all attributes defined on <paramref name="type"/>.</summary>
        /// <param name="type">Type to get attributes for.</param>
        /// <param name="attributeType">(optional) Check only for that attribute type, otherwise for any attribute.</param>
        /// <param name="inherit">(optional) Additionally check for attributes inherited from base type.</param>
        /// <returns>Sequence of found attributes or empty.</returns>
        public static Attribute[] GetAttributes(this Type type, Type attributeType = null, bool inherit = false)
        {
            return type.GetTypeInfo().GetCustomAttributes(attributeType ?? typeof(Attribute), inherit)
                // ReSharper disable once RedundantEnumerableCastCall
                .Cast<Attribute>() // required in .NET 4.5
                .ToArrayOrSelf();
        }

        /// <summary>Recursive method to enumerate all input type and its base types for specific details.
        /// Details are returned by <paramref name="getDeclared"/> delegate.</summary>
        /// <typeparam name="T">Details type: properties, fields, methods, etc.</typeparam>
        /// <param name="type">Input type.</param> <param name="getDeclared">Get declared type details.</param>
        /// <returns>Enumerated details info objects.</returns>
        public static IEnumerable<T> GetAll<T>(this Type type, Func<TypeInfo, IEnumerable<T>> getDeclared)
        {
            var typeInfo = type.GetTypeInfo();
            var declared = getDeclared(typeInfo);
            var baseType = typeInfo.BaseType;
            return baseType == null || baseType == typeof(object) ? declared
                : declared.Concat(baseType.GetAll(getDeclared));
        }

        /// <summary>Enumerates all constructors from input type.</summary>
        /// <param name="type">Input type.</param>
        /// <param name="includeNonPublic">(optional) If set include non-public constructors into result.</param>
        /// <param name="includeStatic">(optional) Turned off by default.</param>
        /// <returns>Enumerated constructors.</returns>
        public static IEnumerable<ConstructorInfo> GetAllConstructors(this Type type, 
            bool includeNonPublic = false, bool includeStatic = false)
        {
            var ctors = type.GetTypeInfo().DeclaredConstructors;
            if (!includeNonPublic) ctors = ctors.Where(c => c.IsPublic);
            if (!includeStatic) ctors = ctors.Where(c => !c.IsStatic);
            return ctors;
        }

        /// <summary>Searches and returns constructor by its signature.</summary>
        /// <param name="type">Input type.</param>
        /// <param name="includeNonPublic">(optional) If set include non-public constructors into result.</param>
        /// <param name="args">Signature - constructor argument types.</param>
        /// <returns>Found constructor or null.</returns>
        public static ConstructorInfo GetConstructorOrNull(this Type type, bool includeNonPublic = false, params Type[] args)
        {
            return type.GetAllConstructors(includeNonPublic)
                .FirstOrDefault(c => c.GetParameters().Select(p => p.ParameterType).SequenceEqual(args));
        }

        /// <summary>Returns single constructor, otherwise if no or more than one: returns false.</summary>
        /// <param name="type">Type to inspect.</param>
        /// <param name="includeNonPublic">If set, counts non-public constructors.</param>
        /// <returns>Single constructor or null.</returns>
        public static ConstructorInfo GetSingleConstructorOrNull(this Type type, bool includeNonPublic = false)
        {
            var ctors = type.GetAllConstructors(includeNonPublic).ToArrayOrSelf();
            return ctors.Length == 1 ? ctors[0] : null;
        }

        /// <summary>Returns single declared (not inherited) method by name, or null if not found.</summary>
        /// <param name="type">Input type</param> <param name="name">Method name to look for.</param>
        /// <returns>Found method or null.</returns>
        public static MethodInfo GetSingleDeclaredMethodOrNull(this Type type, string name)
        {
            var methods = type.GetTypeInfo().DeclaredMethods.Where(m => m.Name == name).ToArrayOrSelf();
            return methods.Length == 1 ? methods[0] : null;
        }

        /// <summary>Returns declared (not inherited) method by name and argument types, or null if not found.</summary>
        /// <param name="type">Input type</param> <param name="name">Method name to look for.</param>
        /// <param name="args">Argument types</param> <returns>Found method or null.</returns>
        public static MethodInfo GetDeclaredMethodOrNull(this Type type, string name, params Type[] args)
        {
            return type.GetTypeInfo().DeclaredMethods.FirstOrDefault(m => 
                m.Name == name && args.SequenceEqual(m.GetParameters().Select(p => p.ParameterType)));
        }

        /// <summary>Returns property by name, including inherited. Or null if not found.</summary>
        /// <param name="type">Input type.</param> <param name="name">Property name to look for.</param>
        /// <returns>Found property or null.</returns>
        public static PropertyInfo GetPropertyOrNull(this Type type, string name)
        {
            return type.GetAll(_ => _.DeclaredProperties).FirstOrDefault(p => p.Name == name);
        }

        /// <summary>Returns field by name, including inherited. Or null if not found.</summary>
        /// <param name="type">Input type.</param> <param name="name">Field name to look for.</param>
        /// <returns>Found field or null.</returns>
        public static FieldInfo GetFieldOrNull(this Type type, string name)
        {
            return type.GetAll(_ => _.DeclaredFields).FirstOrDefault(p => p.Name == name);
        }

        /// <summary>Returns type assembly.</summary> <param name="type">Input type</param> <returns>Type assembly.</returns>
        public static Assembly GetAssembly(this Type type) { return type.GetTypeInfo().Assembly; }

        /// <summary>Returns true if member is static, otherwise returns false.</summary>
        /// <param name="member">Member to check.</param> <returns>True if static.</returns>
        public static bool IsStatic(this MemberInfo member)
        {
            var isStatic =
                member is MethodInfo ? ((MethodInfo)member).IsStatic : 
                member is PropertyInfo ? Portable.GetPropertyGetMethod((PropertyInfo)member).IsStatic :
                ((FieldInfo)member).IsStatic;
            return isStatic;
        }

        /// <summary>Return either <see cref="PropertyInfo.PropertyType"/>, or <see cref="FieldInfo.FieldType"/>, <see cref="MethodInfo.ReturnType"/>.
        /// Otherwise returns null.</summary>
        /// <param name="member">Expecting member of type <see cref="PropertyInfo"/> or <see cref="FieldInfo"/> only.</param>
        /// <returns>Type of property of field.</returns>
        public static Type GetReturnTypeOrDefault(this MemberInfo member)
        {
            return member is MethodInfo ? ((MethodInfo)member).ReturnType
                : member is PropertyInfo ? ((PropertyInfo)member).PropertyType 
                : member is FieldInfo ? ((FieldInfo)member).FieldType
                : null;
        }

        /// <summary>Returns true if field is backing field for property.</summary>
        /// <param name="field">Field to check.</param> <returns>Returns true if field is backing property.</returns>
        public static bool IsBackingField(this FieldInfo field)
        {
            return field.Name[0] == '<';
        }

        /// <summary>Returns true if property is public.</summary>
        /// <param name="property">Property check.</param> <returns>Returns result of check.</returns>
        public static bool IsPublic(this PropertyInfo property)
        {
            return Portable.GetPropertySetMethod(property) != null;
        }

        /// <summary>Returns true if property is indexer: aka this[].</summary>
        /// <param name="property">Property to check</param><returns>True if indexer.</returns>
        public static bool IsIndexer(this PropertyInfo property)
        {
            return property.GetIndexParameters().Length != 0;
        }

        /// <summary>Returns attributes defined for the member/method.</summary>
        /// <param name="member">Member to check.</param> <param name="attributeType">(optional) Specific attribute type to return, any attribute otherwise.</param>
        /// <param name="inherit">Check for inherited member attributes.</param> <returns>Found attributes or empty.</returns>
        public static IEnumerable<Attribute> GetAttributes(this MemberInfo member, Type attributeType = null, bool inherit = false)
        {
            return member.GetCustomAttributes(attributeType ?? typeof(Attribute), inherit).Cast<Attribute>();
        }

        /// <summary>Returns attributes defined for parameter.</summary>
        ///  <param name="parameter">Target parameter.</param> 
        /// <param name="attributeType">(optional) Specific attribute type to return, any attribute otherwise.</param>
        /// <param name="inherit">Check for inherited attributes.</param> <returns>Found attributes or empty.</returns>
        public static IEnumerable<Attribute> GetAttributes(this ParameterInfo parameter, Type attributeType = null, bool inherit = false)
        {
            return parameter.GetCustomAttributes(attributeType ?? typeof(Attribute), inherit).Cast<Attribute>();
        }

        /// <summary>Get types from assembly that are loaded successfully. 
        /// Hacks to <see cref="ReflectionTypeLoadException"/> for loaded types.</summary>
        /// <param name="assembly">Assembly to get types from.</param>
        /// <returns>Array of loaded types.</returns>
        public static Type[] GetLoadedTypes(this Assembly assembly)
        {
            Type[] types;
            try
            {
                types = Portable.GetTypesFromAssembly(assembly).ToArrayOrSelf();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(type => type != null).ToArrayOrSelf();
            }
            return types;
        }

        #region Implementation

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

        #endregion
    }

    /// <summary>Methods to work with immutable arrays, and general array sugar.</summary>
    public static class ArrayTools
    {
        /// <summary>Returns true if array is null or have no items.</summary> <typeparam name="T">Type of array item.</typeparam>
        /// <param name="source">Source array to check.</param> <returns>True if null or has no items, false otherwise.</returns>
        public static bool IsNullOrEmpty<T>(this T[] source)
        {
            return source == null || source.Length == 0;
        }

        /// <summary>Returns source enumerable if it is array, otherwise converts source to array.</summary>
        /// <typeparam name="T">Array item type.</typeparam>
        /// <param name="source">Source enumerable.</param>
        /// <returns>Source enumerable or its array copy.</returns>
        public static T[] ToArrayOrSelf<T>(this IEnumerable<T> source)
        {
            return source is T[] ? (T[])source : source.ToArray();
        }

        /// <summary>Returns new array consisting from all items from source array then all items from added array.
        /// If source is null or empty, then added array will be returned.
        /// If added is null or empty, then source will be returned.</summary>
        /// <typeparam name="T">Array item type.</typeparam>
        /// <param name="source">Array with leading items.</param>
        /// <param name="added">Array with following items.</param>
        /// <returns>New array with items of source and added arrays.</returns>
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

        /// <summary>Returns new array with <paramref name="value"/> appended, 
        /// or <paramref name="value"/> at <paramref name="index"/>, if specified.
        /// If source array could be null or empty, then single value item array will be created despite any index.</summary>
        /// <typeparam name="T">Array item type.</typeparam>
        /// <param name="source">Array to append value to.</param>
        /// <param name="value">Value to append.</param>
        /// <param name="index">(optional) Index of value to update.</param>
        /// <returns>New array with appended or updated value.</returns>
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

        /// <summary>Calls predicate on each item in <paramref name="source"/> array until predicate returns true,
        /// then method will return this item index, or if predicate returns false for each item, method will return -1.</summary>
        /// <typeparam name="T">Type of array items.</typeparam>
        /// <param name="source">Source array: if null or empty, then method will return -1.</param>
        /// <param name="predicate">Delegate to evaluate on each array item until delegate returns true.</param>
        /// <returns>Index of item for which predicate returns true, or -1 otherwise.</returns>
        public static int IndexOf<T>(this T[] source, Func<T, bool> predicate)
        {
            if (source != null && source.Length != 0)
                for (var i = 0; i < source.Length; ++i)
                    if (predicate(source[i]))
                        return i;
            return -1;
        }

        /// <summary>Looks up for item in source array equal to provided value, and returns its index, or -1 if not found.</summary>
        /// <typeparam name="T">Type of array items.</typeparam>
        /// <param name="source">Source array: if null or empty, then method will return -1.</param>
        /// <param name="value">Value to look up.</param>
        /// <returns>Index of item equal to value, or -1 item is not found.</returns>
        public static int IndexOf<T>(this T[] source, T value)
        {
            if (source != null && source.Length != 0)
                for (var i = 0; i < source.Length; ++i)
                    if (Equals(source[i], value))
                        return i;
            return -1;
        }

        /// <summary>Produces new array without item at specified <paramref name="index"/>. 
        /// Will return <paramref name="source"/> array if index is out of bounds, or source is null/empty.</summary>
        /// <typeparam name="T">Type of array item.</typeparam>
        /// <param name="source">Input array.</param> <param name="index">Index if item to remove.</param>
        /// <returns>New array with removed item at index, or input source array if index is not in array.</returns>
        public static T[] RemoveAt<T>(this T[] source, int index)
        {
            if (source == null || source.Length == 0 || index < 0 || index >= source.Length)
                return source;
            if (index == 0 && source.Length == 1)
                return new T[0];
            var result = new T[source.Length - 1];
            if (index != 0)
                Array.Copy(source, 0, result, 0, index);
            if (index != result.Length)
                Array.Copy(source, index + 1, result, index, result.Length - index);
            return result;
        }

        /// <summary>Looks for item in array using equality comparison, and returns new array with found item remove, or original array if not item found.</summary>
        /// <typeparam name="T">Type of array item.</typeparam>
        /// <param name="source">Input array.</param> <param name="value">Value to find and remove.</param>
        /// <returns>New array with value removed or original array if value is not found.</returns>
        public static T[] Remove<T>(this T[] source, T value)
        {
            return source.RemoveAt(source.IndexOf(value));
        }
    }

    /// <summary>Provides pretty printing/debug view for number of types.</summary>
    public static class PrintTools
    {
        /// <summary>Default separator used for printing enumerable.</summary>
        public readonly static string DEFAULT_ITEM_SEPARATOR = ";" + Environment.NewLine;

        /// <summary>Prints input object by using corresponding Print methods for know types.</summary>
        /// <param name="s">Builder to append output to.</param>
        /// <param name="x">Object to print.</param>
        /// <param name="quote">(optional) Quote to use for quoting string object.</param>
        /// <param name="itemSeparator">(optional) Separator for enumerable.</param>
        /// <param name="getTypeName">(optional) Custom type printing policy.</param>
        /// <returns>String builder with appended output.</returns>
        public static StringBuilder Print(this StringBuilder s, object x,
            string quote = null, string itemSeparator = null, Func<Type, string> getTypeName = null)
        {
            return x == null ? s.Append("null")
                : x is string ? s.Print((string)x, quote)
                : x is Type ? s.Print((Type)x, getTypeName)
                : x is IEnumerable<Type> || x is IEnumerable
                    ? s.Print((IEnumerable)x, itemSeparator ?? DEFAULT_ITEM_SEPARATOR, (_, o) => _.Print(o, quote, null, getTypeName))
                : s.Append(x);
        }

        /// <summary>Appends string to string builder quoting with <paramref name="quote"/> if provided.</summary>
        /// <param name="s">String builder to append string to.</param>
        /// <param name="str">String to print.</param>
        /// <param name="quote">(optional) Quote to add before and after string.</param>
        /// <returns>String builder with appended string.</returns>
        public static StringBuilder Print(this StringBuilder s, string str, string quote = null)
        {
            return quote == null ? s.Append(str) : s.Append(quote).Append(str).Append(quote);
        }

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

        /// <summary>Default delegate to print Type details: by default print <see cref="Type.FullName"/> and
        /// spare namespace if it start with "System."</summary>
        public static readonly Func<Type, string> GetTypeNameDefault = t =>
            t.FullName != null && t.Namespace != null && !t.Namespace.StartsWith("System") ? t.FullName : t.Name;

        /// <summary>Appends <see cref="Type"/> object details to string builder.</summary>
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
        /// <summary>Portable version of Assembly.GetTypes.</summary>
        public static readonly Func<Assembly, IEnumerable<Type>> GetTypesFromAssembly =
            ExpressionTools.GetMethodDelegateOrNull<Assembly, IEnumerable<Type>>("GetTypes").ThrowIfNull();

        /// <summary>Portable version of PropertyInfo.GetGetMethod.</summary>
        public static readonly Func<PropertyInfo, MethodInfo> GetPropertyGetMethod =
            ExpressionTools.GetMethodDelegateOrNull<PropertyInfo, MethodInfo>("GetGetMethod").ThrowIfNull();

        /// <summary>Portable version of PropertyInfo.GetSetMethod.</summary>
        public static readonly Func<PropertyInfo, MethodInfo> GetPropertySetMethod =
            ExpressionTools.GetMethodDelegateOrNull<PropertyInfo, MethodInfo>("GetSetMethod").ThrowIfNull();

        /// <summary>Portable version of Type.GetGenericArguments.</summary>
        public static readonly Func<Type, Type[]> GetGenericArguments =
            ExpressionTools.GetMethodDelegateOrNull<Type, Type[]>("GetGenericArguments").ThrowIfNull();

        /// <summary>Returns managed Thread ID either from Environment or Thread.CurrentThread whichever is available.</summary>
        /// <returns>Managed Thread ID.</returns>
        public static int GetCurrentManagedThreadID()
        {
            var resultID = -1;
            GetCurrentManagedThreadID(ref resultID);
            if (resultID == -1)
                resultID = _getEnvCurrentManagedThreadId();
            return resultID;
        }

        static partial void GetCurrentManagedThreadID(ref int threadID);

        private static readonly MethodInfo _getEnvCurrentManagedThreadIdMethod =
            typeof(Environment).GetDeclaredMethodOrNull("get_CurrentManagedThreadId");

        private static readonly Func<int> _getEnvCurrentManagedThreadId = _getEnvCurrentManagedThreadIdMethod == null ? null
            : Expression.Lambda<Func<int>>(Expression.Call(_getEnvCurrentManagedThreadIdMethod), null).Compile();
    }

    /// <summary>Tools for expressions, that are not supported out-of-box.</summary>
    public static class ExpressionTools
    {
        /// <summary>Extracts method info from method call expression.
        /// It is allow to use type-safe method declaration instead of string method name.</summary>
        /// <param name="methodCall">Lambda wrapping method call.</param>
        /// <returns>Found method info or null if lambda body is not method call.</returns>
        public static MethodInfo GetCalledMethodOrNull(LambdaExpression methodCall)
        {
            var callExpr = methodCall.Body as MethodCallExpression;
            return callExpr == null ? null : callExpr.Method;
        }


        /// <summary>Extracts member info from property or field getter. Enables type-safe property declarations without using strings.</summary>
        /// <typeparam name="T">Type of member holder.</typeparam>
        /// <param name="getter">Expected to contain member access: t => t.MyProperty.</param>
        /// <returns>Extracted member info or null if getter does not contain member access.</returns>
        public static MemberInfo GetAccessedMemberOrNull<T>(Expression<Func<T, object>> getter)
        {
            var body = getter.Body;
            var member = body as MemberExpression ?? ((UnaryExpression)body).Operand as MemberExpression;
            return member == null ? null : member.Member;
        }

        /// <summary>Creates and returns delegate calling method without parameters.</summary>
        /// <typeparam name="TOwner">Method owner type.</typeparam>
        /// <typeparam name="TReturn">Method return type.</typeparam>
        /// <param name="methodName">Method name to find.</param>
        /// <returns>Created delegate or null, if no method with such name is found.</returns>
        public static Func<TOwner, TReturn> GetMethodDelegateOrNull<TOwner, TReturn>(string methodName)
        {
            var methodInfo = typeof(TOwner).GetDeclaredMethodOrNull(methodName);
            if (methodInfo == null) return null;
            var thisExpr = Expression.Parameter(typeof(TOwner), "_");
            var methodExpr = Expression.Lambda<Func<TOwner, TReturn>>(Expression.Call(thisExpr, methodInfo), thisExpr);
            return methodExpr.Compile();
        }

        /// <summary>Creates default(T) expression for provided <paramref name="type"/>.</summary>
        /// <param name="type">Type to get default value of.</param>
        /// <returns>Default value expression.</returns>
        public static Expression GetDefaultValueExpression(this Type type)
        {
            return Expression.Call(_getDefaultMethod.MakeGenericMethod(type), (Expression[])null);
        }

        private static readonly MethodInfo _getDefaultMethod = typeof(ExpressionTools).GetDeclaredMethodOrNull("GetDefault");
        internal static T GetDefault<T>() { return default(T); }
    }

    /// <summary>Immutable Key-Value. It is reference type (could be check for null), 
    /// which is different from System value type <see cref="KeyValuePair{TKey,TValue}"/>.
    /// In addition provides <see cref="Equals"/> and <see cref="GetHashCode"/> implementations.</summary>
    /// <typeparam name="K">Type of Key.</typeparam><typeparam name="V">Type of Value.</typeparam>
    public sealed class KV<K, V>
    {
        /// <summary>Key.</summary>
        public readonly K Key;

        /// <summary>Value.</summary>
        public readonly V Value;

        /// <summary>Creates Key-Value object by providing key and value. Does Not check either one for null.</summary>
        /// <param name="key">key.</param><param name="value">value.</param>
        public KV(K key, V value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>Creates nice string view.</summary><returns>String representation.</returns>
        public override string ToString()
        {
            return new StringBuilder("[").Print(Key).Append(", ").Print(Value).Append("]").ToString();
        }

        /// <summary>Returns true if both key and value are equal to corresponding key-value of other object.</summary>
        /// <param name="obj">Object to check equality with.</param> <returns>True if equal.</returns>
        public override bool Equals(object obj)
        {
            var other = obj as KV<K, V>;
            return other != null
                && (ReferenceEquals(other.Key, Key) || Equals(other.Key, Key))
                && (ReferenceEquals(other.Value, Value) || Equals(other.Value, Value));
        }

        /// <summary>Combines key and value hash code. R# generated default implementation.</summary>
        /// <returns>Combined hash code for key-value.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((object)Key == null ? 0 : Key.GetHashCode() * 397)
                     ^ ((object)Value == null ? 0 : Value.GetHashCode());
            }
        }
    }

    /// <summary>Delegate for changing value from old one to some new based on provided new value.</summary>
    /// <typeparam name="V">Type of values.</typeparam>
    /// <param name="oldValue">Existing value.</param>
    /// <param name="newValue">New value passed to Update.. method.</param>
    /// <returns>Changed value.</returns>
    public delegate V Update<V>(V oldValue, V newValue);

    /// <summary>Simple immutable AVL tree with integer keys and object values.</summary>
    public sealed class IntKeyTree
    {
        /// <summary>Empty tree to start with. The <see cref="Height"/> of the empty tree is 0.</summary>
        public static readonly IntKeyTree Empty = new IntKeyTree();

        /// <summary>Key.</summary>
        public readonly int Key;

        /// <summary>Value.</summary>
        public readonly object Value;

        /// <summary>Left subtree/branch, or empty.</summary>
        public readonly IntKeyTree Left;

        /// <summary>Right subtree/branch, or empty.</summary>
        public readonly IntKeyTree Right;

        /// <summary>Height of longest subtree/branch. It is 0 for empty tree, and 1 for single node tree.</summary>
        public readonly int Height;

        /// <summary>Returns true is tree is empty.</summary>
        public bool IsEmpty { get { return Height == 0; } }

        /// <summary>Returns new tree with added or updated value for specified key.</summary>
        /// <param name="key"></param> <param name="value"></param>
        /// <returns>New tree.</returns>
        public IntKeyTree AddOrUpdate(int key, object value)
        {
            return AddOrUpdate(key, value, false, null);
        }

        /// <summary>Delegate to get updated value based on its old and new value.</summary>
        /// <param name="oldValue">old</param> <param name="newValue">new</param> <returns>result</returns>
        public delegate object UpdateValue(object oldValue, object newValue);

        /// <summary>Returns new tree with added or updated value for specified key.</summary>
        /// <param name="key"></param> <param name="value"></param>
        /// <param name="updateValue">Delegate to get updated value based on its old and new value.</param>
        /// <returns>New tree.</returns>
        public IntKeyTree AddOrUpdate(int key, object value, UpdateValue updateValue)
        {
            return AddOrUpdate(key, value, false, updateValue);
        }

        /// <summary>Returns new tree with updated value for the key, Or the same tree if key was not found.</summary>
        /// <param name="key"></param> <param name="value"></param>
        /// <returns>New tree if key is found, or the same tree otherwise.</returns>
        public IntKeyTree Update(int key, object value)
        {
            return AddOrUpdate(key, value, true, null);
        }

        /// <summary>Get value for found key or null otherwise.</summary>
        /// <param name="key"></param> <returns>Found value or null.</returns>
        public object GetValueOrDefault(int key)
        {
            var tree = this;
            while (tree.Height != 0 && tree.Key != key)
                tree = key < tree.Key ? tree.Left : tree.Right;
            return tree.Height != 0 ? tree.Value : null;
        }

        /// <summary>Returns all sub-trees enumerated from left to right.</summary> 
        /// <returns>Enumerated sub-trees or empty if tree is empty.</returns>
        public IEnumerable<IntKeyTree> Enumerate()
        {
            if (Height == 0)
                yield break;

            var parents = new IntKeyTree[Height];

            var tree = this;
            var parentCount = -1;
            while (tree.Height != 0 || parentCount != -1)
            {
                if (tree.Height != 0)
                {
                    parents[++parentCount] = tree;
                    tree = tree.Left;
                }
                else
                {
                    tree = parents[parentCount--];
                    yield return tree;
                    tree = tree.Right;
                }
            }
        }

        #region Implementation

        private IntKeyTree() { }

        private IntKeyTree(int key, object value, IntKeyTree left, IntKeyTree right)
        {
            Key = key;
            Value = value;
            Left = left;
            Right = right;
            Height = 1 + (left.Height > right.Height ? left.Height : right.Height);
        }

        // If keys is not found and updateOnly is true, it should return current tree without changes.
        private IntKeyTree AddOrUpdate(int key, object value, bool updateOnly, UpdateValue update)
        {
            return Height == 0 ? // tree is empty
                    (updateOnly ? this : new IntKeyTree(key, value, Empty, Empty))
                : (key == Key ? // actual update
                    new IntKeyTree(key, update == null ? value : update(Value, value), Left, Right)
                : (key < Key    // try update on left or right sub-tree
                    ? With(Left.AddOrUpdate(key, value, updateOnly, update), Right)
                    : With(Left, Right.AddOrUpdate(key, value, updateOnly, update))).KeepBalanced());
        }

        private IntKeyTree KeepBalanced()
        {
            var delta = Left.Height - Right.Height;
            return delta >= 2 ? With(Left.Right.Height - Left.Left.Height == 1 ? Left.RotateLeft() : Left, Right).RotateRight()
                : (delta <= -2 ? With(Left, Right.Left.Height - Right.Right.Height == 1 ? Right.RotateRight() : Right).RotateLeft()
                : this);
        }

        private IntKeyTree RotateRight()
        {
            return Left.With(Left.Left, With(Left.Right, Right));
        }

        private IntKeyTree RotateLeft()
        {
            return Right.With(With(Left, Right.Left), Right.Right);
        }

        private IntKeyTree With(IntKeyTree left, IntKeyTree right)
        {
            return left == Left && right == Right ? this : new IntKeyTree(Key, Value, left, right);
        }

        #endregion
    }

    /// <summary>Immutable http://en.wikipedia.org/wiki/AVL_tree where actual node key is hash code of <typeparamref name="K"/>.</summary>
    public sealed class HashTree<K, V>
    {
        /// <summary>Empty tree to start with. The <see cref="Height"/> of the empty tree is 0.</summary>
        public static readonly HashTree<K, V> Empty = new HashTree<K, V>();

        /// <summary>Key of type K that should support <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode"/>.</summary>
        public readonly K Key;

        /// <summary>Value of any type V.</summary>
        public readonly V Value;

        /// <summary>Hash calculated from <see cref="Key"/> with <see cref="object.GetHashCode"/>. Hash is stored to improve speed.</summary>
        public readonly int Hash;

        /// <summary>In case of <see cref="Hash"/> conflicts for different keys contains conflicted keys with their values.</summary>
        public readonly KV<K, V>[] Conflicts;

        /// <summary>Left subtree/branch, or empty.</summary>
        public readonly HashTree<K, V> Left;

        /// <summary>Right subtree/branch, or empty.</summary>
        public readonly HashTree<K, V> Right;

        /// <summary>Height of longest subtree/branch. It is 0 for empty tree, and 1 for single node tree.</summary>
        public readonly int Height;

        /// <summary>Returns true is tree is empty.</summary>
        public bool IsEmpty { get { return Height == 0; } }

        /// <summary>Returns new tree with added key-value. If value with the same key is exist, then
        /// if <paramref name="update"/> is not specified: then existing value will be replaced by <paramref name="value"/>;
        /// if <paramref name="update"/> is specified: then update delegate will decide what value to keep.</summary>
        /// <param name="key">Key to add.</param><param name="value">Value to add.</param>
        /// <param name="update">(optional) Delegate to decide what value to keep: old or new one.</param>
        /// <returns>New tree with added or updated key-value.</returns>
        public HashTree<K, V> AddOrUpdate(K key, V value, Update<V> update = null)
        {
            return AddOrUpdate(key.GetHashCode(), key, value, update, updateOnly: false);
        }

        /// <summary>Looks for <paramref name="key"/> and replaces its value with new <paramref name="value"/>, or 
        /// it may use <paramref name="update"/> for more complex update logic. Returns new tree with updated value,
        /// or the SAME tree if key is not found.</summary>
        /// <param name="key">Key to look for.</param>
        /// <param name="value">New value to replace key value with.</param>
        /// <param name="update">(optional) Delegate for custom update logic, it gets old and new <paramref name="value"/>
        /// as inputs and should return updated value as output.</param>
        /// <returns>New tree with updated value or the SAME tree if no key found.</returns>
        public HashTree<K, V> Update(K key, V value, Update<V> update = null)
        {
            return AddOrUpdate(key.GetHashCode(), key, value, update, updateOnly: true);
        }

        /// <summary>Searches for key in tree and returns the value if found, or <paramref name="defaultValue"/> otherwise.</summary>
        /// <param name="key">Key to look for.</param> <param name="defaultValue">Value to return if key is not found.</param>
        /// <returns>Found value or <paramref name="defaultValue"/>.</returns>
        public V GetValueOrDefault(K key, V defaultValue = default(V))
        {
            var t = this;
            var hash = key.GetHashCode();
            while (t.Height != 0 && t.Hash != hash)
                t = hash < t.Hash ? t.Left : t.Right;
            return t.Height != 0 && (ReferenceEquals(key, t.Key) || key.Equals(t.Key))
                ? t.Value : t.GetConflictedValueOrDefault(key, defaultValue);
        }

        /// <summary>Depth-first in-order traversal as described in http://en.wikipedia.org/wiki/Tree_traversal
        /// The only difference is using fixed size array instead of stack for speed-up (~20% faster than stack).</summary>
        /// <returns>Sequence of enumerated key value pairs.</returns>
        public IEnumerable<KV<K, V>> Enumerate()
        {
            if (Height == 0)
                yield break;

            var parents = new HashTree<K, V>[Height];

            var tree = this;
            var parentCount = -1;
            while (tree.Height != 0 || parentCount != -1)
            {
                if (tree.Height != 0)
                {
                    parents[++parentCount] = tree;
                    tree = tree.Left;
                }
                else
                {
                    tree = parents[parentCount--];
                    yield return new KV<K, V>(tree.Key, tree.Value);

                    if (tree.Conflicts != null)
                        for (var i = 0; i < tree.Conflicts.Length; i++)
                            yield return tree.Conflicts[i];

                    tree = tree.Right;
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

        private HashTree<K, V> AddOrUpdate(int hash, K key, V value, Update<V> update, bool updateOnly)
        {
            return Height == 0 ? (updateOnly ? this : new HashTree<K, V>(hash, key, value, null, Empty, Empty))
                : (hash == Hash ? UpdateValueAndResolveConflicts(key, value, update, updateOnly)
                : (hash < Hash
                    ? With(Left.AddOrUpdate(hash, key, value, update, updateOnly), Right)
                    : With(Left, Right.AddOrUpdate(hash, key, value, update, updateOnly))).KeepBalanced());
        }

        private HashTree<K, V> UpdateValueAndResolveConflicts(K key, V value, Update<V> update, bool updateOnly)
        {
            if (ReferenceEquals(Key, key) || Key.Equals(key))
                return new HashTree<K, V>(Hash, key, update == null ? value : update(Value, value), Conflicts, Left, Right);

            if (Conflicts == null) // add only if updateOnly is false.
                return updateOnly ? this
                    : new HashTree<K, V>(Hash, Key, Value, new[] { new KV<K, V>(key, value) }, Left, Right);

            var found = Conflicts.Length - 1;
            while (found >= 0 && !Equals(Conflicts[found].Key, Key)) --found;
            if (found == -1)
            {
                if (updateOnly) return this;
                var newConflicts = new KV<K, V>[Conflicts.Length + 1];
                Array.Copy(Conflicts, 0, newConflicts, 0, Conflicts.Length);
                newConflicts[Conflicts.Length] = new KV<K, V>(key, value);
                return new HashTree<K, V>(Hash, Key, Value, newConflicts, Left, Right);
            }

            var conflicts = new KV<K, V>[Conflicts.Length];
            Array.Copy(Conflicts, 0, conflicts, 0, Conflicts.Length);
            conflicts[found] = new KV<K, V>(key, update == null ? value : update(Conflicts[found].Value, value));
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
            return left == Left && right == Right ? this : new HashTree<K, V>(Hash, Key, Value, Conflicts, left, right);
        }

        #endregion
    }

    /// <summary>Provides optimistic-concurrency consistent <see cref="Swap{T}"/> operation.</summary>
    public static class Ref
    {
        /// <summary>Factory for <see cref="Ref{T}"/> with type of value inference.</summary>
        /// <typeparam name="T">Type of value to wrap.</typeparam>
        /// <param name="value">Initial value to wrap.</param>
        /// <returns>New ref.</returns>
        public static Ref<T> Of<T>(T value) where T : class
        {
            return new Ref<T>(value);
        }

        /// <summary>Creates new ref to original ref value.</summary> <typeparam name="T">Type of ref value.</typeparam>
        /// <param name="original">Original ref.</param> <returns>New ref to original value.</returns>
        public static Ref<T> Copy<T>(this Ref<T> original) where T : class
        {
            return Of(original.Value);
        }

        /// <summary>First, it evaluates new value using <paramref name="getNewValue"/> function. 
        /// Second, it checks that original value is not changed. 
        /// If it is changed it will retry first step, otherwise it assigns new value and returns original (the one used for <paramref name="getNewValue"/>).</summary>
        /// <typeparam name="T">Type of value to swap.</typeparam>
        /// <param name="value">Reference to change to new value</param>
        /// <param name="getNewValue">Delegate to get value from old one.</param>
        /// <returns>Old/original value. By analogy with <see cref="Interlocked.Exchange(ref int,int)"/>.</returns>
        /// <remarks>Important: <paramref name="getNewValue"/> May be called multiple times to retry update with value concurrently changed by other code.</remarks>
        public static T Swap<T>(ref T value, Func<T, T> getNewValue) where T : class
        {
            var retryCount = 0;
            while (true)
            {
                var oldValue = value;
                var newValue = getNewValue(oldValue);
                if (Interlocked.CompareExchange(ref value, newValue, oldValue) == oldValue)
                    return oldValue;
                if (++retryCount > RETRY_COUNT_UNTIL_THROW)
                    throw new InvalidOperationException(ERROR_RETRY_COUNT_EXCEEDED);
            }
        }

        private const int RETRY_COUNT_UNTIL_THROW = 50;
        private static readonly string ERROR_RETRY_COUNT_EXCEEDED =
            "Ref retried to Update for " + RETRY_COUNT_UNTIL_THROW + " times But there is always someone else intervened.";
    }

    /// <summary>Wrapper that provides optimistic-concurrency Swap operation implemented using <see cref="Ref.Swap{T}"/>.</summary>
    /// <typeparam name="T">Type of object to wrap.</typeparam>
    public sealed class Ref<T> where T : class
    {
        /// <summary>Gets the wrapped value.</summary>
        public T Value { get { return _value; } }

        /// <summary>Creates ref to object, optionally with initial value provided.</summary>
        /// <param name="initialValue">Initial object value.</param>
        public Ref(T initialValue = default(T))
        {
            _value = initialValue;
        }

        /// <summary>Exchanges currently hold object with <paramref name="getNewValue"/> result: see <see cref="Ref.Swap{T}"/> for details.</summary>
        /// <param name="getNewValue">Delegate to produce new object value from current one passed as parameter.</param>
        /// <returns>Returns old object value the same way as <see cref="Interlocked.Exchange(ref int,int)"/></returns>
        /// <remarks>Important: <paramref name="getNewValue"/> May be called multiple times to retry update with value concurrently changed by other code.</remarks>
        public T Swap(Func<T, T> getNewValue)
        {
            return Ref.Swap(ref _value, getNewValue);
        }

        /// <summary>Simplified version of Swap ignoring old value.</summary>
        /// <param name="newValue">New value to set</param> <returns>Old value.</returns>
        public T Swap(T newValue)
        {
            return Interlocked.Exchange(ref _value, newValue);
        }

        private T _value;
    }
}