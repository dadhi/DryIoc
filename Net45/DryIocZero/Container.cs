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

namespace DryIocZero
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;

    using DryIoc;

    /// <summary>Minimal container to register service factory delegates and then resolve service from them.</summary>
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Does not contain any unmanaged resources.")]
    public sealed partial class Container : IFactoryDelegateRegistrator, IResolverContext, IResolver, IScopeAccess, IDisposable
    {
        /// <summary>Creates container.</summary>
        /// <param name="scopeContext">(optional) Ambient scope context.</param>
        public Container(IScopeContext scopeContext = null)
            : this(Ref.Of(ImTreeMap<Type, FactoryDelegate>.Empty), 
                  Ref.Of(ImTreeMap<Type, ImTreeMap<object, FactoryDelegate>>.Empty), 
                  new SingletonScope(), scopeContext, null, 0) { }

        /// <summary>Full constructor - all state included.</summary>
        /// <param name="defaultFactories"></param>
        /// <param name="keyedFactories"></param>
        /// <param name="singletonScope"></param>
        /// <param name="scopeContext">Ambient scope context.</param>
        /// <param name="openedScope">Container bound opened scope.</param>
        /// <param name="disposed"></param>
        public Container(Ref<ImTreeMap<Type, FactoryDelegate>> defaultFactories, 
            Ref<ImTreeMap<Type, ImTreeMap<object, FactoryDelegate>>> keyedFactories, 
            IScope singletonScope, IScopeContext scopeContext, IScope openedScope, 
            int disposed)
        {
            _defaultFactories = defaultFactories;
            _keyedFactories = keyedFactories;

            SingletonScope = singletonScope;
            ScopeContext = scopeContext;
            _openedScope = openedScope;

            _disposed = disposed;
        }

        /// <summary>Provides access to resolver.</summary>
        public IResolver Resolver { get { return this; } }

        /// <summary>Scopes access.</summary>
        public IScopeAccess Scopes { get { return this; } }

        #region IResolver

        partial void ResolveGenerated(ref object service, Type serviceType, IScope scope);

        /// <summary>Directly uses generated factories to resolve service. Or returns default if service does not have generated factory.</summary>
        /// <param name="serviceType">Service type to lookup generated factory.</param> <returns>Created service or null.</returns>
        [SuppressMessage("ReSharper", "InvocationIsSkipped", Justification = "Per design")]
        [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull", Justification = "Per design")]
        public object ResolveGeneratedOrGetDefault(Type serviceType)
        {
            object service = null;
            ResolveGenerated(ref service, serviceType, null);
            return service;
        }

        /// <summary>Resolves service from container and returns created service object.</summary>
        /// <param name="serviceType">Service type to search and to return.</param>
        /// <param name="ifUnresolvedReturnDefault">Says what to do if service is unresolved.</param>
        /// <returns>Created service object or default based on <paramref name="ifUnresolvedReturnDefault"/> provided.</returns>
        [SuppressMessage("ReSharper", "InvocationIsSkipped", Justification = "Per design")]
        [SuppressMessage("ReSharper", "ConstantNullCoalescingCondition", Justification = "Per design")]
        public object Resolve(Type serviceType, bool ifUnresolvedReturnDefault)
        {
            object service = null;
            if (_defaultFactories.Value.IsEmpty)
                ResolveGenerated(ref service, serviceType, null);
            return service ?? ResolveDefaultFromRuntimeRegistrationsFirst(serviceType, ifUnresolvedReturnDefault, null);
        }

        private object ResolveDefaultFromRuntimeRegistrationsFirst(Type serviceType, bool ifUnresolvedReturnDefault, IScope scope)
        {
            object service = null;
            var factories = _defaultFactories.Value;
            var factory = factories.GetValueOrDefault(serviceType);
            if (factory == null)
                ResolveGenerated(ref service, serviceType, scope);
            else
                service = ((FactoryDelegate)factory)(this, scope);
            return service ?? Throw.If(!ifUnresolvedReturnDefault,
                Error.UnableToResolveDefaultService, serviceType, factories.IsEmpty ? string.Empty : "non-");
        }

        partial void ResolveGenerated(ref object service, Type serviceType, object serviceKey, Type requiredServiceType, RequestInfo preRequestParent, IScope scope);

        /// <summary>Directly uses generated factories to resolve service. Or returns default if service does not have generated factory.</summary>
        /// <param name="serviceType">Service type to lookup generated factory.</param> <param name="serviceKey">Service key to locate factory.</param>
        /// <returns>Created service or null.</returns>
        [SuppressMessage("ReSharper", "InvocationIsSkipped", Justification = "Per design")]
        [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull", Justification = "Per design")]
        public object ResolveGeneratedOrGetDefault(Type serviceType, object serviceKey)
        {
            object service = null;
            ResolveGenerated(ref service, serviceType, serviceKey, null, null, null);
            return service;
        }

        /// <summary>Resolves service from container and returns created service object.</summary>
        /// <param name="serviceType">Service type to search and to return.</param>
        /// <param name="serviceKey">Optional service key used for registering service.</param>
        /// <param name="ifUnresolvedReturnDefault">Says what to do if service is unresolved.</param>
        /// <param name="requiredServiceType">Actual registered service type to use instead of <paramref name="serviceType"/>, 
        ///     or wrapped type for generic wrappers.  The type should be assignable to return <paramref name="serviceType"/>.</param>
        /// <param name="preResolveParent">Dependency resolution path info.</param>
        /// <param name="scope">Propagated resolution scope.</param>
        /// <returns>Created service object or default based on <paramref name="ifUnresolvedReturnDefault"/> provided.</returns>
        /// <remarks>
        /// This method covers all possible resolution input parameters comparing to <see cref="Resolve(System.Type,bool)"/>, and
        /// by specifying the same parameters as for <see cref="Resolve(System.Type,bool)"/> should return the same result.
        /// </remarks>
        [SuppressMessage("ReSharper", "InvocationIsSkipped", Justification = "Per design")]
        [SuppressMessage("ReSharper", "ConstantNullCoalescingCondition", Justification = "Per design")]
        public object Resolve(Type serviceType, object serviceKey, 
            bool ifUnresolvedReturnDefault = false, Type requiredServiceType = null, 
            RequestInfo preResolveParent = null, IScope scope = null)
        {
            object service = null;
            if (_keyedFactories.Value.IsEmpty)
            {
                if (serviceKey == null && requiredServiceType == null && preResolveParent == null)
                    ResolveGenerated(ref service, serviceType, scope);
                else
                    ResolveGenerated(ref service, serviceType, serviceKey, requiredServiceType, preResolveParent, scope);
            }
            return service ?? ResolveFromRuntimeRegistrationsFirst(serviceType, serviceKey, ifUnresolvedReturnDefault, requiredServiceType, preResolveParent, scope);
        }

        [SuppressMessage("ReSharper", "InvocationIsSkipped", Justification = "Per design")]
        [SuppressMessage("ReSharper", "ConstantNullCoalescingCondition", Justification = "Per design")]
        private object ResolveFromRuntimeRegistrationsFirst(Type serviceType, object serviceKey,
            bool ifUnresolvedReturnDefault, Type requiredServiceType, RequestInfo preResolveParent, IScope scope)
        {
            serviceType = requiredServiceType ?? serviceType;
            preResolveParent = preResolveParent ?? RequestInfo.Empty;

            if (serviceKey == null)
                return ResolveDefaultFromRuntimeRegistrationsFirst(serviceType, ifUnresolvedReturnDefault, scope);

            var factories = _keyedFactories.Value.GetValueOrDefault(serviceType);
            if (factories != null)
            {
                var factory = factories.GetValueOrDefault(serviceKey);
                if (factory != null)
                    return factory(this, scope);
            }

            // If not resolved from runtime registration then try resolve generated
            object service = null;
            ResolveGenerated(ref service, serviceType, serviceKey, requiredServiceType, preResolveParent, scope);
            return service ?? Throw.If(!ifUnresolvedReturnDefault,
                Error.UnableToResolveKeyedService, serviceType, serviceKey, factories == null ? string.Empty : "non-");
        }

        partial void ResolveManyGenerated(ref IEnumerable<KV<object, FactoryDelegate>> services, Type serviceType);

        /// <summary>Resolves many generated only services. Ignores runtime registrations.</summary>
        /// <param name="serviceType">Service type.</param>
        /// <returns>Collection of service key - service pairs.</returns>
        public IEnumerable<KV<object, FactoryDelegate>> ResolveManyGeneratedOrGetEmpty(Type serviceType)
        {
            var manyGenerated = Enumerable.Empty<KV<object, FactoryDelegate>>();
            ResolveManyGenerated(ref manyGenerated, serviceType);
            return manyGenerated;
        }

        /// <summary>Resolves all services registered for specified <paramref name="serviceType"/>, or if not found returns
        /// empty enumerable. If <paramref name="serviceType"/> specified then returns only (single) service registered with
        /// this type. Excludes for result composite parent identified by <paramref name="compositeParentKey"/>.</summary>
        /// <param name="serviceType">Return type of an service item.</param>
        /// <param name="serviceKey">(optional) Resolve only single service registered with the key.</param>
        /// <param name="requiredServiceType">(optional) Actual registered service to search for.</param>
        /// <param name="compositeParentKey">(optional) Parent service key to exclude to support Composite pattern.</param>
        /// <param name="compositeParentRequiredType">(optional) Parent required service type to identify composite, together with key.</param>
        /// <param name="preResolveParent">(optional) Dependency resolution path info prior to resolve.</param>
        /// <param name="scope">propagated resolution scope, may be null.</param>
        /// <returns>Enumerable of found services or empty. Does Not throw if no service found.</returns>
        public IEnumerable<object> ResolveMany(Type serviceType, 
            object serviceKey = null, Type requiredServiceType = null, object compositeParentKey = null, 
            Type compositeParentRequiredType = null, RequestInfo preResolveParent = null, IScope scope = null)
        {
            serviceType = requiredServiceType ?? serviceType;

            var manyGeneratedFactories = Enumerable.Empty<KV<object, FactoryDelegate>>();
            ResolveManyGenerated(ref manyGeneratedFactories, serviceType);
            if (compositeParentKey != null)
                manyGeneratedFactories = manyGeneratedFactories.Where(kv => !compositeParentKey.Equals(kv.Key));

            foreach (var generated in manyGeneratedFactories)
                yield return ((FactoryDelegate)generated.Value)(this, scope);

            var factories = _keyedFactories.Value.GetValueOrDefault(serviceType);
            if (factories != null)
            {
                if (serviceKey != null)
                {
                    var factoryDelegate = factories.GetValueOrDefault(serviceKey);
                    if (factoryDelegate != null)
                        yield return factoryDelegate(this, scope);
                }
                else
                {
                    foreach (var resolution in factories.Enumerate())
                        if (compositeParentKey == null || !compositeParentKey.Equals(resolution.Key))
                            yield return ((FactoryDelegate)resolution.Value)(this, scope);
                }
            }
            else
            {
                var factoryDelegate = _defaultFactories.Value.GetValueOrDefault(serviceType) as FactoryDelegate;
                if (factoryDelegate != null)
                    yield return factoryDelegate(this, scope);
            }
        }

        #endregion

        #region IFactoryDelegateRegistrator

        /// <summary>Registers factory delegate with corresponding service type.</summary>
        /// <param name="serviceType">Type</param> <param name="factoryDelegate">Delegate</param>
        public void Register(Type serviceType, FactoryDelegate factoryDelegate)
        {
            ThrowIfContainerDisposed();
            _defaultFactories.Swap(_ => _.AddOrUpdate(serviceType, factoryDelegate));
        }

        /// <summary>Registers factory delegate with corresponding service type and service key.</summary>
        /// <param name="serviceType">Type</param> <param name="serviceKey">Key</param> <param name="factoryDelegate">Delegate</param>
        public void Register(Type serviceType, object serviceKey, FactoryDelegate factoryDelegate)
        {
            if (serviceKey == null)
            {
                Register(serviceType, factoryDelegate);
                return;
            }
            ThrowIfContainerDisposed();
            _keyedFactories.Swap(_ =>
            {
                var entry = _.GetValueOrDefault(serviceType) ?? ImTreeMap<object, FactoryDelegate>.Empty;
                return _.AddOrUpdate(serviceType, entry.AddOrUpdate(serviceKey, factoryDelegate));
            });
        }

        private Ref<ImTreeMap<Type, FactoryDelegate>> _defaultFactories;
        private Ref<ImTreeMap<Type, ImTreeMap<object, FactoryDelegate>>> _keyedFactories;

        #endregion

        #region IScopeAccess

        /// <summary>Scope containing container singletons.</summary>
        public IScope SingletonScope { get; private set; }

        /// <summary>Current scope.</summary>
        public IScope GetCurrentScope()
        {
            return GetCurrentNamedScope(null, false);
        }

        /// <summary>Scope context or null of not necessary.</summary>
        public IScopeContext ScopeContext { get; private set; }

        /// <summary>Creates new container with new current ambient scope.</summary>
        /// <param name="name">(optional) Scope name.</param>
        /// <returns>New container.</returns>
        public Container OpenScope(object name = null)
        {
            ThrowIfContainerDisposed();

            var nestedOpenedScope = new Scope(_openedScope, name);

            // Replacing current context scope with new nested only if current is the same as nested parent, otherwise throw.
            if (ScopeContext != null)
                ScopeContext.SetCurrent(scope =>
                {
                    Throw.If(scope != _openedScope, Error.NotDirectScopeParent, _openedScope, scope);
                    return nestedOpenedScope;
                });

            return new Container(_defaultFactories, _keyedFactories, 
                SingletonScope, ScopeContext, nestedOpenedScope, _disposed);
        }

        private readonly IScope _openedScope;

        /// <summary>Returns current scope matching the <paramref name="name"/>. 
        /// If name is null then current scope is returned, or if there is no current scope then exception thrown.</summary>
        /// <param name="name">May be null</param> <returns>Found scope or throws exception.</returns>
        /// <param name="throwIfNotFound">Says to throw if no scope found.</param>
        /// <exception cref="ContainerException"> with code <see cref="Error.NoMatchedScopeFound"/>.</exception>
        public IScope GetCurrentNamedScope(object name, bool throwIfNotFound)
        {
            var currentScope = ScopeContext == null ? _openedScope : ScopeContext.GetCurrentOrDefault();
            if (currentScope == null)
                return (IScope)Throw.If(throwIfNotFound, Error.NoCurrentScope);

            var matchingScope = GetMatchingScopeOrDefault(currentScope, name);
            if (matchingScope == null)
                return (IScope)Throw.If(throwIfNotFound, Error.NoMatchedScopeFound, name);

            return matchingScope;
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
        public IScope GetOrCreateResolutionScope(ref IScope scope, Type serviceType, object serviceKey)
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
        /// <returns>Matching scope or throws exception.</returns>
        public IScope GetMatchingResolutionScope(IScope scope, Type assignableFromServiceType, object serviceKey,
            bool outermost, bool throwIfNotFound)
        {
            var matchingScope = GetMatchingScopeOrDefault(scope, assignableFromServiceType, serviceKey, outermost);
            return matchingScope ?? (IScope)Throw.If(throwIfNotFound, 
                    Error.NoMatchedScopeFound,new KV<Type, object>(assignableFromServiceType, serviceKey));
        }

        private static IScope GetMatchingScopeOrDefault(IScope scope, Type assignableFromServiceType, object serviceKey,
            bool outermost)
        {
            if (assignableFromServiceType == null && serviceKey == null)
                return scope;

            IScope matchedScope = null;
            while (scope != null)
            {
                var name = scope.Name as KV<Type, object>;
                if (name != null &&
                    (assignableFromServiceType == null
                    || name.Key != null && assignableFromServiceType.GetTypeInfo().IsAssignableFrom(name.Key.GetTypeInfo())) 
                    && (serviceKey == null || serviceKey.Equals(name.Value)))
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

        #region IDisposable

        /// <summary>Disposes opened scope or root container including: Singletons, ScopeContext, Make default and keyed factories empty.</summary>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly",
            Justification = "Does not container any unmanaged resources.")]
        public void Dispose()
        {
            if (_openedScope != null)
            {
                _openedScope.Dispose();
                if (ScopeContext != null)
                    ScopeContext.SetCurrent(scope => scope == _openedScope ? scope.Parent : scope);
            }
            else
            {
                if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1) return;
                _defaultFactories = Ref.Of(ImTreeMap<Type, FactoryDelegate>.Empty);
                _keyedFactories = Ref.Of(ImTreeMap<Type, ImTreeMap<object, FactoryDelegate>>.Empty);
                SingletonScope.Dispose();
                if (ScopeContext != null)
                    ScopeContext.Dispose();
            }
        }

        private int _disposed;

        private void ThrowIfContainerDisposed()
        {
            Throw.If(_disposed == 1, Error.ContainerIsDisposed);
        }

        #endregion
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
        /// <param name="id">To set value at.</param> <param name="item">Value to set.</param>
        void SetOrAdd(int id, object item);

        /// <summary>Creates id/index for new item to be stored in scope. 
        /// If separate index is not supported then just returns back passed <paramref name="externalId"/>.</summary>
        /// <param name="externalId">Id to be mapped to new item id/index</param> 
        /// <returns>New it/index or just passed <paramref name="externalId"/></returns>
        int GetScopedItemIdOrSelf(int externalId);
    }

    /// <summary>Declares minimal API for service resolution.
    /// The user friendly convenient methods are implemented as extension methods in <see cref="Resolver"/> class.</summary>
    /// <remarks>Resolve default and keyed is separated because of micro optimization for faster resolution.</remarks>
    public interface IResolver
    {
        /// <summary>Resolves default (non-keyed) service from container and returns created service object.</summary>
        /// <param name="serviceType">Service type to search and to return.</param>
        /// <param name="ifUnresolvedReturnDefault">Says what to do if service is unresolved.</param>
        /// <returns>Created service object or default based on <paramref name="ifUnresolvedReturnDefault"/> provided.</returns>
        object Resolve(Type serviceType, bool ifUnresolvedReturnDefault);

        /// <summary>Resolves service from container and returns created service object.</summary>
        /// <param name="serviceType">Service type to search and to return.</param>
        /// <param name="serviceKey">Optional service key used for registering service.</param>
        /// <param name="ifUnresolvedReturnDefault">Says what to do if service is unresolved.</param>
        /// <param name="requiredServiceType">Actual registered service type to use instead of <paramref name="serviceType"/>, 
        ///     or wrapped type for generic wrappers.  The type should be assignable to return <paramref name="serviceType"/>.</param>
        /// <param name="preResolveParent">Dependency resolution path info.</param>
        /// <param name="scope">Propagated resolution scope.</param>
        /// <returns>Created service object or default based on <paramref name="ifUnresolvedReturnDefault"/> provided.</returns>
        /// <remarks>
        /// This method covers all possible resolution input parameters comparing to <see cref="Resolve(System.Type,bool)"/>, and
        /// by specifying the same parameters as for <see cref="Resolve(System.Type,bool)"/> should return the same result.
        /// </remarks>
        object Resolve(Type serviceType, object serviceKey, bool ifUnresolvedReturnDefault, Type requiredServiceType, 
            RequestInfo preResolveParent, IScope scope);

        /// <summary>Resolves all services registered for specified <paramref name="serviceType"/>, or if not found returns
        /// empty enumerable. If <paramref name="serviceType"/> specified then returns only (single) service registered with
        /// this type. Excludes for result composite parent identified by <paramref name="compositeParentKey"/>.</summary>
        /// <param name="serviceType">Return type of an service item.</param>
        /// <param name="serviceKey">(optional) Resolve only single service registered with the key.</param>
        /// <param name="requiredServiceType">(optional) Actual registered service to search for.</param>
        /// <param name="compositeParentKey">(optional) Parent service key to exclude to support Composite pattern.</param>
        /// <param name="compositeParentRequiredType">(optional) Parent required service type to identify composite, together with key.</param>
        /// <param name="preResolveParent">(optional) Dependency resolution path info prior to resolve.</param>
        /// <param name="scope">propagated resolution scope, may be null.</param>
        /// <returns>Enumerable of found services or empty. Does Not throw if no service found.</returns>
        IEnumerable<object> ResolveMany(Type serviceType, object serviceKey, Type requiredServiceType, object compositeParentKey, Type compositeParentRequiredType, 
            RequestInfo preResolveParent, IScope scope);
    }

    /// <summary>Provides access to scopes.</summary>
    public interface IScopeAccess
    {
        /// <summary>Scope containing container singletons.</summary>
        IScope SingletonScope { get; }

        /// <summary>Current scope.</summary>
        IScope GetCurrentScope();

        /// <summary>Gets current scope matching the <paramref name="name"/>. 
        /// If name is null then current scope is returned, or if there is no current scope then exception thrown.</summary>
        /// <param name="name">May be null</param> <returns>Found scope or throws exception.</returns>
        /// <param name="throwIfNotFound">Says to throw if no scope found.</param>
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
        IScope GetMatchingResolutionScope(IScope scope, Type assignableFromServiceType, object serviceKey, bool outermost, bool throwIfNotFound);
    }

    /// <summary>Returns reference to actual resolver implementation. 
    /// Minimizes dependency to Factory Delegate on container.</summary>
    public interface IResolverContext
    {
        /// <summary>Provides access to resolver implementation.</summary>
        IResolver Resolver { get; }

        /// <summary>Scopes access.</summary>
        IScopeAccess Scopes { get; }
    }

    /// <summary>Service factory delegate which accepts resolver and resolution scope as parameters and should return service object.
    /// It is stateless because does not include state parameter as <c>DryIoc.FactoryDelegate</c>.</summary>
    /// <param name="r">Provides access to <see cref="IResolver"/> to enable dynamic dependency resolution inside factory delegate.</param>
    /// <param name="scope">Resolution scope parameter. May be null to enable on demand scope creation inside factory delegate.</param>
    /// <returns>Created service object.</returns>
    public delegate object FactoryDelegate(IResolverContext r, IScope scope);

    /// <summary>Provides methods to register default or keyed factory delegates.</summary>
    public interface IFactoryDelegateRegistrator
    {
        /// <summary>Registers factory delegate with corresponding service type.</summary>
        /// <param name="serviceType">Type</param> <param name="factoryDelegate">Delegate</param>
        void Register(Type serviceType, FactoryDelegate factoryDelegate);

        /// <summary>Registers factory delegate with corresponding service type and service key.</summary>
        /// <param name="serviceType">Type</param> <param name="serviceKey">Key</param> <param name="factoryDelegate">Delegate</param>
        void Register(Type serviceType, object serviceKey, FactoryDelegate factoryDelegate);
    }

    /// <summary>Delegate to get new scope from old/existing current scope.</summary>
    /// <param name="oldScope">Old/existing scope to change.</param>
    /// <returns>New scope or old if do not want to change current scope.</returns>
    public delegate IScope SetCurrentScopeHandler(IScope oldScope);

    /// <summary>Provides ambient current scope and optionally scope storage for container, 
    /// examples are HttpContext storage, Execution context, Thread local.</summary>
    public interface IScopeContext : IDisposable
    {
        /// <summary>Name associated with context root scope - so the reuse may find scope context.</summary>
        string RootScopeName { get; }

        /// <summary>Returns current scope or null if no ambient scope available at the moment.</summary>
        /// <returns>Current scope or null.</returns>
        IScope GetCurrentOrDefault();

        /// <summary>Changes current scope using provided delegate. Delegate receives current scope as input and
        /// should return new current scope.</summary>
        /// <param name="setCurrentScope">Delegate to change the scope.</param>
        /// <remarks>Important: <paramref name="setCurrentScope"/> may be called multiple times in concurrent environment.
        /// Make it predictable by removing any side effects.</remarks>
        /// <returns>New current scope. So it is convenient to use method in <c>using (var newScope = ctx.SetCurrent(...))</c>.</returns>
        IScope SetCurrent(SetCurrentScopeHandler setCurrentScope);
    }

    /// <summary>Convenience extensions for registrations on top of delegate registrator.</summary>
    public static class Registrator
    {
        /// <summary>Registers user provided delegate to create the service</summary>
        /// <param name="registrator">Registrator to register with.</param>
        /// <param name="serviceType">Service type.</param>
        /// <param name="factoryDelegate">Delegate to produce service instance.</param>
        /// <param name="serviceKey">(optional) Service key.</param>
        public static void RegisterDelegate<TService>(this IFactoryDelegateRegistrator registrator,
            Type serviceType, Func<IResolver, TService> factoryDelegate, object serviceKey = null)
        {
            registrator.Register(typeof(TService), serviceKey, (context, scope) => factoryDelegate(context.Resolver));
        }


        /// <summary>Registers user provided delegate to create the service</summary>
        /// <typeparam name="TService">Service type.</typeparam>
        ///  <param name="registrator">Registrator to register with.</param>
        /// <param name="factoryDelegate">Delegate to produce service instance.</param>
        /// <param name="serviceKey">(optional) Service key.</param>
        public static void RegisterDelegate<TService>(this IFactoryDelegateRegistrator registrator, 
            Func<IResolver, TService> factoryDelegate, object serviceKey = null)
        {
            registrator.RegisterDelegate(typeof(TService), factoryDelegate, serviceKey);
        }

        /// <summary>Registers passed service instance.</summary>
        /// <typeparam name="TService">Service type, may be different from instance type.</typeparam>
        /// <param name="registrator">Registrator to register with.</param>
        /// <param name="instance">Externally managed service instance.</param>
        /// <param name="serviceKey">(optional) Service key.</param>
        public static void RegisterInstance<TService>(this IFactoryDelegateRegistrator registrator, TService instance, object serviceKey = null)
        {
            registrator.Register(typeof(TService), serviceKey, (context, scope) => instance);
        }
    }

    /// <summary>Sugar to allow more simple resolve API</summary>
    public static class Resolver
    {
        /// <summary>Resolves non-keyed service of <typeparamref name="TService"/> type.</summary>
        /// <typeparam name="TService">Type of service to resolve.</typeparam> <param name="resolver"></param>
        /// <param name="requiredServiceType">(optional) Required service type.</param>
        /// <param name="ifUnresolvedReturnDefault">Says what to do if service is unresolved.</param>
        /// <param name="serviceKey">(optional) Service key.</param>
        /// <returns>Service object or throws exception.</returns>
        public static TService Resolve<TService>(this IResolver resolver, 
            Type requiredServiceType = null, bool ifUnresolvedReturnDefault = false, object serviceKey = null)
        {
            return (TService)(requiredServiceType == null && serviceKey == null
                ? resolver.Resolve(typeof(TService), ifUnresolvedReturnDefault)
                : resolver.Resolve(typeof(TService), serviceKey, ifUnresolvedReturnDefault, requiredServiceType, 
                    RequestInfo.Empty, null));
        }
    }

    /// <summary>Scope implementation which will dispose stored <see cref="IDisposable"/> items on its own dispose.
    /// Locking is used internally to ensure that object factory called only once.</summary>
    public sealed class Scope : IScope
    {
        /// <summary>Parent scope in scope stack. Null for root scope.</summary>
        public IScope Parent { get; private set; }

        /// <summary>Optional name object associated with scope.</summary>
        public object Name { get; private set; }

        /// <summary>Create scope with optional parent and name.</summary>
        /// <param name="parent">Parent in scope stack.</param> <param name="name">Associated name object.</param>
        public Scope(IScope parent = null, object name = null)
        {
            Parent = parent;
            Name = name;
            _items = ImTreeMapIntToObj.Empty;
        }

        /// <summary>Just returns back <paramref name="externalId"/> without any changes.</summary>
        /// <param name="externalId">Id will be returned back.</param> <returns><paramref name="externalId"/>.</returns>
        public int GetScopedItemIdOrSelf(int externalId)
        {
            return externalId;
        }

        /// <summary><see cref="IScope.GetOrAdd"/> for description.
        /// Will throw <see cref="ContainerException"/> if scope is disposed.</summary>
        /// <param name="id">Unique ID to find created object in subsequent calls.</param>
        /// <param name="createValue">Delegate to create object. It will be used immediately, and reference to delegate will Not be stored.</param>
        /// <returns>Created and stored object.</returns>
        /// <exception cref="ContainerException">if scope is disposed.</exception>
        public object GetOrAdd(int id, CreateScopedValue createValue)
        {
            return _items.GetValueOrDefault(id) ?? TryGetOrAdd(id, createValue);
        }

        private object TryGetOrAdd(int id, CreateScopedValue createValue)
        {
            Throw.If(_disposed == 1, Error.ScopeIsDisposed);

            if (id == -1) // disposable transient
            {
                var dt = createValue();
                Ref.Swap(ref _disposableTransients, dts => dts.AppendOrUpdate(dt));
                return dt;
            }

            object item;
            lock (_locker)
            {
                item = _items.GetValueOrDefault(id);
                if (item != null)
                    return item;
                item = createValue();
            }

            var items = _items;
            var newItems = items.AddOrUpdate(id, item);
            // if _items were not changed so far then use them, otherwise (if changed) do ref swap;
            if (Interlocked.CompareExchange(ref _items, newItems, items) != items)
                Ref.Swap(ref _items, _ => _.AddOrUpdate(id, item));
            return item;
        }

        /// <summary>Sets (replaces) value at specified id, or adds value if no existing id found.</summary>
        /// <param name="id">To set value at.</param> <param name="item">Value to set.</param>
        public void SetOrAdd(int id, object item)
        {
            Throw.If(_disposed == 1, Error.ScopeIsDisposed);
            Ref.Swap(ref _items, items => items.AddOrUpdate(id, item));
        }

        /// <summary>Disposes all stored <see cref="IDisposable"/> objects and nullifies object storage.</summary>
        /// <remarks>If item disposal throws exception, then it won't be propagated outside, 
        /// so the rest of the items could be disposed.</remarks>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                return;
            var items = _items;
            if (!items.IsEmpty)
            {
                // dispose in backward registration order
                var itemList = items.Enumerate().ToArray();
                for (var i = itemList.Length - 1; i >= 0; --i)
                    DisposeItem(itemList[i].Value);
            }
            _items = ImTreeMapIntToObj.Empty;

            var disposableTransients = _disposableTransients;
            if (!disposableTransients.IsNullOrEmpty())
                for (var i = 0; i < disposableTransients.Length; i++)
                    DisposeItem(_disposableTransients[i]);
            _disposableTransients = null;
        }

        /// <summary>Prints scope info (name and parent) to string for debug purposes.</summary> 
        /// <returns>String representation.</returns>
        public override string ToString()
        {
            return "{" +
                (Name != null ? "Name=" + Name + ", " : string.Empty) +
                (Parent == null ? "Parent=null" : "Parent=" + Parent)
                + "}";
        }

        #region Implementation

        private ImTreeMapIntToObj _items;
        private object[] _disposableTransients;
        private int _disposed;

        // Sync root is required to create object only once. The same reason as for Lazy<T>.
        private readonly object _locker = new object();

        internal static void DisposeItem(object item)
        {
            var disposable = item as IDisposable;
            if (disposable == null)
            {
                // Unwrap WeakReference if item wrapped in it.
                var weakRefItem = item as WeakReference;
                if (weakRefItem != null)
                    disposable = weakRefItem.Target as IDisposable;
            }

            if (disposable != null)
            {
                try { disposable.Dispose(); }
                catch (Exception)
                {
                    // NOTE Ignoring disposing exception, they not so important for program to proceed.
                }
            }
        }

        #endregion
    }

    /// <summary>Different from <see cref="Scope"/> so that uses single array of items for fast access.
    /// The array structure is:
    /// items[0] is reserved for storing object[][] buckets.
    /// items[1-BucketSize] are used for storing actual singletons up to (BucketSize-1) index
    /// Buckets structure is variable number of object[BucketSize] buckets used to storing items with index >= BucketSize.
    /// The structure allows very fast access to up to <see cref="BucketSize"/> singletons - it just array access: items[itemIndex]
    /// For further indexes it is a fast O(1) access: ((object[][])items[i])[i / BucketSize - 1][i % BucketSize]
    /// </summary>
    public sealed class SingletonScope : IScope
    {
        /// <summary>Parent scope in scope stack. Null for root scope.</summary>
        public IScope Parent { get; private set; }

        /// <summary>Optional name object associated with scope.</summary>
        public object Name { get; private set; }

        /// <summary>Amount of items in item array.</summary>
        public static readonly int BucketSize = 32;

        /// <summary>Creates scope.</summary>
        /// <param name="parent">Parent in scope stack.</param> <param name="name">Associated name object.</param>
        public SingletonScope(IScope parent = null, object name = null)
        {
            Parent = parent;
            Name = name;
            Items = new object[BucketSize];
            _factoryIdToIndexMap = ImTreeMapIntToObj.Empty;
            _lastItemIndex = 0;
        }

        /// <summary>Adds mapping between provide id and index for new stored item. Returns index.</summary>
        /// <param name="externalId">External id mapped to internal index.</param>
        /// <returns>Already mapped index, or newly created.</returns>
        public int GetScopedItemIdOrSelf(int externalId)
        {
            var index = _factoryIdToIndexMap.GetValueOrDefault(externalId);
            if (index != null)
                return (int)index;

            Ref.Swap(ref _factoryIdToIndexMap, map =>
            {
                index = map.GetValueOrDefault(externalId);
                return index != null ? map
                    : map.AddOrUpdate(externalId, index = Interlocked.Increment(ref _lastItemIndex));
            });

            return (int)index;
        }

        /// <summary><see cref="IScope.GetOrAdd"/> for description.
        /// Will throw <see cref="ContainerException"/> if scope is disposed.</summary>
        /// <param name="id">Unique ID to find created object in subsequent calls.</param>
        /// <param name="createValue">Delegate to create object. It will be used immediately, and reference to delegate will Not be stored.</param>
        /// <returns>Created and stored object.</returns>
        /// <exception cref="ContainerException">if scope is disposed.</exception>
        public object GetOrAdd(int id, CreateScopedValue createValue)
        {
            return id < BucketSize && id >= 0 // it could be -1 for disposable transients
                ? (Items[id] ?? GetOrAddItem(Items, id, createValue))
                : GetOrAddItem(id, createValue);
        }

        /// <summary>Sets (replaces) value at specified id, or adds value if no existing id found.</summary>
        /// <param name="id">To set value at.</param> <param name="item">Value to set.</param>
        public void SetOrAdd(int id, object item)
        {
            Throw.If(_disposed == 1, Error.ScopeIsDisposed);
            if (id < BucketSize)
                Items[id] = item;
            else
            {
                var bucket = GetOrAddBucket(id);
                var indexInBucket = id % BucketSize;
                bucket[indexInBucket] = item;
            }
        }

        /// <summary>Adds external non-service item into singleton collection. 
        /// The item may not have corresponding external item ID.</summary>
        /// <param name="item">External item to add, this may be metadata, service key, etc.</param>
        /// <returns>Index of added or already added item.</returns>
        internal int GetOrAdd(object item)
        {
            var index = IndexOf(item);
            if (index == -1)
            {
                index = Interlocked.Increment(ref _lastItemIndex);
                SetOrAdd(index, item);
            }
            return index;
        }

        /// <summary>Disposes all stored <see cref="IDisposable"/> objects and nullifies object storage.</summary>
        /// <remarks>If item disposal throws exception, then it won't be propagated outside, so the rest of the items could be disposed.</remarks>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                return;

            var factoryIdToIndexMap = _factoryIdToIndexMap;
            if (!factoryIdToIndexMap.IsEmpty)
            {
                var ids = factoryIdToIndexMap.Enumerate().ToArray();
                for (var i = ids.Length - 1; i >= 0; --i)
                    Scope.DisposeItem(GetItemOrDefault((int)ids[i].Value));
            }
            _factoryIdToIndexMap = ImTreeMapIntToObj.Empty;

            var disposableTransients = _disposableTransients;
            if (!disposableTransients.IsNullOrEmpty())
                for (var i = 0; i < disposableTransients.Length; i++)
                    Scope.DisposeItem(_disposableTransients[i]);
            _disposableTransients = null;

            Items = ArrayTools.Empty<object>();
        }

        #region Implementation

        private static readonly object[] _lockers =
        {
            new object(), new object(), new object(), new object(),
            new object(), new object(), new object(), new object()
        };

        private ImTreeMapIntToObj _factoryIdToIndexMap;
        private int _lastItemIndex;
        private int _disposed;

        /// <summary>value at 0 index is reserved for [][] structure to accommodate more values</summary>
        internal object[] Items;

        private object[] _disposableTransients;

        private object GetOrAddItem(int index, CreateScopedValue createValue)
        {
            if (index == -1) // disposable transient
            {
                var item = createValue();
                Ref.Swap(ref _disposableTransients, items => items.AppendOrUpdate(item));
                return item;
            }

            var bucket = GetOrAddBucket(index);
            index = index % BucketSize;
            return GetOrAddItem(bucket, index, createValue);
        }

        private static object GetOrAddItem(object[] bucket, int index, CreateScopedValue createValue)
        {
            var value = bucket[index];
            if (value != null)
                return value;

            var locker = _lockers[index % _lockers.Length];
            lock (locker)
            {
                value = bucket[index];
                if (value == null)
                    bucket[index] = value = createValue();
            }

            return value;
        }

        private object GetItemOrDefault(int index)
        {
            if (index < BucketSize)
                return Items[index];

            var bucketIndex = index / BucketSize - 1;
            var buckets = Items[0] as object[][];
            if (buckets != null && buckets.Length > bucketIndex)
            {
                var bucket = buckets[bucketIndex];
                if (bucket != null)
                    return bucket[index % BucketSize];
            }

            return null;
        }

        // find if bucket already created starting from 0
        // if not - create new buckets array and copy old buckets into it
        private object[] GetOrAddBucket(int index)
        {
            var bucketIndex = index / BucketSize - 1;
            var buckets = Items[0] as object[][];
            if (buckets == null ||
                buckets.Length < bucketIndex + 1 ||
                buckets[bucketIndex] == null)
            {
                Ref.Swap(ref Items[0], value =>
                {
                    if (value == null)
                    {
                        var newBuckets = new object[bucketIndex + 1][];
                        newBuckets[bucketIndex] = new object[BucketSize];
                        return newBuckets;
                    }

                    var oldBuckets = (object[][])value;
                    if (oldBuckets.Length < bucketIndex + 1)
                    {
                        var newBuckets = new object[bucketIndex + 1][];
                        Array.Copy(oldBuckets, 0, newBuckets, 0, oldBuckets.Length);
                        newBuckets[bucketIndex] = new object[BucketSize];
                        return newBuckets;
                    }

                    if (oldBuckets[bucketIndex] == null)
                        oldBuckets[bucketIndex] = new object[BucketSize];

                    return value;
                });
            }

            var bucket = ((object[][])Items[0])[bucketIndex];
            return bucket;
        }

        private int IndexOf(object item)
        {
            var index = Items.IndexOf(item);
            if (index != -1)
                return index;

            // look in other buckets
            var bucketsObj = Items[0];
            if (bucketsObj != null)
            {
                var buckets = (object[][])bucketsObj;
                for (var i = 0; i < buckets.Length; i++)
                {
                    var b = buckets[i];
                    if (b != null)
                    {
                        index = b.IndexOf(item);
                        if (index != -1)
                            return (i + 1) * BucketSize + index;
                    }
                }
            }

            return -1;
        }

        #endregion
    }

    /// <summary>List of error codes and messages.</summary>
    public static class Error
    {
        /// <summary>First error code to identify error range for other possible error code definitions.</summary>
        public readonly static int FirstErrorCode = 0;

        /// <summary>List of error messages indexed with code.</summary>
        public readonly static List<string> Messages = new List<string>(100);

#pragma warning disable 1591 // "Missing XML-comment"
        public static readonly int
            UnableToResolveDefaultService = Of(
                "Unable to resolve {0} from {1}empty runtime registrations and from generated factory delegates."),
            UnableToResolveKeyedService = Of(
                "Unable to resolve {0} with key [{1}] from {2}empty runtime registrations and from generated factory delegates."),
            NoCurrentScope = Of(
                "No current scope available: probably you are registering to, or resolving from outside of scope."),
            NoMatchedScopeFound = Of(
                "Unable to find scope with matching name: {0}."),
            NotDirectScopeParent = Of(
                "Unable to OpenScope [{0}] because parent scope [{1}] is not current context scope [{2}]." + Environment.NewLine +
                "It is probably other scope was opened in between OR you forgot to Dispose some other scope!"),
            ContainerIsDisposed = Of(
                "Container is disposed and its operations are no longer available."),
            ScopeIsDisposed = Of(
                "Scope is disposed and scoped instances are no longer available.");
#pragma warning restore 1591 // "Missing XML-comment"

        /// <summary>Generates new code for message.</summary>
        /// <param name="message">Message.</param> <returns>Code.</returns>
        public static int Of(string message)
        {
            Messages.Add(message);
            return FirstErrorCode + Messages.Count - 1;
        }
    }

    /// <summary>Zero container exception.</summary>
    [SuppressMessage("Microsoft.Usage",
        "CA2237:MarkISerializableTypesWithSerializable",
        Justification = "Not available in PCL")]
    public class ContainerException : InvalidOperationException
    {
        /// <summary>Error code.</summary>
        public int Error { get; private set; }

        /// <summary>Creates exception.</summary>
        /// <param name="error">Code.</param> <param name="message">Message.</param>
        public ContainerException(int error, string message)
            : base(message)
        {
            Error = error;
        }
    }

    /// <summary>Simplifies throwing exceptions.</summary>
    public static class Throw
    {
        /// <summary>Just throws exception with specified error code.</summary>
        /// <param name="error">Code.</param> <param name="args">Arguments for error message.</param>
        public static object It(int error, params object[] args)
        {
            var messageFormat = Error.Messages[error];
            var message = string.Format(messageFormat, args);
            throw new ContainerException(error, message);
        }

        /// <summary>Throws is condition is true.</summary>
        /// <param name="condition">Condition.</param>
        /// <param name="error">Code.</param> <param name="args">Arguments for error message.</param>
        /// <returns>Returns null if condition is false.</returns>
        public static object If(bool condition, int error, params object[] args)
        {
            if (condition) It(error, args);
            return null;
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

    /// <summary>Type of services supported by Container.</summary>
    public enum FactoryType
    {
        /// <summary>(default) Defines normal service factory</summary>
        Service,
        /// <summary>Defines decorator factory</summary>
        Decorator,
        /// <summary>Defines wrapper factory.</summary>
        Wrapper
    }

    /// <summary>Reuse goal is to locate or create scope where reused objects will be stored.</summary>
    /// <remarks><see cref="IReuse"/> implementors supposed to be stateless, and provide scope location behavior only.
    /// The reused service instances should be stored in scope(s).</remarks>
    public interface IReuse
    {
        /// <summary>Relative to other reuses lifespan value.</summary>
        int Lifespan { get; }
    }

    /// <summary>Specifies pre-defined reuse behaviors supported by container: 
    /// used when registering services into container with <see cref="Registrator"/> methods.</summary>
    public static class Reuse
    {
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
    }

    /// <summary>Returns container bound scope for storing singleton objects.</summary>
    public sealed class SingletonReuse : IReuse
    {
        /// <summary>Relative to other reuses lifespan value.</summary>
        public int Lifespan { get { return 1000; } }
    }

    /// <summary>Returns container bound current scope created by <see cref="Container.OpenScope"/> method.</summary>
    /// <remarks>It is the same as Singleton scope if container was not created by <see cref="Container.OpenScope"/>.</remarks>
    public sealed class CurrentScopeReuse : IReuse
    {
        /// <summary>Name to find current scope or parent with equal name.</summary>
        public readonly object Name;

        /// <summary>Relative to other reuses lifespan value.</summary>
        public int Lifespan { get { return 100; } }

        /// <summary>Creates reuse optionally specifying its name.</summary> 
        /// <param name="name">(optional) Used to find matching current scope or parent.</param>
        public CurrentScopeReuse(object name = null)
        {
            Name = name;
        }
    }

    /// <summary>Represents services created once per resolution root (when some of Resolve methods called).</summary>
    /// <remarks>Scope is created only if accessed to not waste memory.</remarks>
    public sealed class ResolutionScopeReuse : IReuse
    {
        /// <summary>Relative to other reuses lifespan value.</summary>
        public int Lifespan { get { return 0; } }

        /// <summary>Indicates consumer with assignable service type that defines resolution scope.</summary>
        public readonly Type AssignableFromServiceType;

        /// <summary>Indicates service key of the consumer that defines resolution scope.</summary>
        public readonly object ServiceKey;

        /// <summary>When set indicates to find the outermost matching consumer with resolution scope,
        /// otherwise nearest consumer scope will be used.</summary>
        public readonly bool Outermost;

        /// <summary>Creates new resolution scope reuse with specified type and key.</summary>
        /// <param name="assignableFromServiceType">(optional)</param> <param name="serviceKey">(optional)</param>
        /// <param name="outermost">(optional)</param>
        public ResolutionScopeReuse(Type assignableFromServiceType = null, object serviceKey = null, bool outermost = false)
        {
            AssignableFromServiceType = assignableFromServiceType;
            ServiceKey = serviceKey;
            Outermost = outermost;
        }
    }

    /// <summary>Dependency request path information.</summary>
    public sealed class RequestInfo
    {
        /// <summary>Represents empty info (indicated by null <see cref="ServiceType"/>).</summary>
        public static readonly RequestInfo Empty = new RequestInfo(null, null, null, -1, FactoryType.Service, null, null, null);

        /// <summary>Returns true for an empty request.</summary>
        public bool IsEmpty { get { return ServiceType == null; } }

        /// <summary>Returns true if request is the first in a chain.</summary>
        public bool IsResolutionRoot { get { return !IsEmpty && ParentOrWrapper.IsEmpty; } }

        /// <summary>Parent request or null for root resolution request.</summary>
        public readonly RequestInfo ParentOrWrapper;

        /// <summary>Returns service parent skipping wrapper if any. To get immediate parent us <see cref="ParentOrWrapper"/>.</summary>
        public RequestInfo Parent
        {
            get
            {
                return IsEmpty ? Empty : ParentOrWrapper.FirstOrEmpty(p => p.FactoryType != FactoryType.Wrapper);
            }
        }

        /// <summary>Gets first request info starting with itself which satisfies the condition, or empty otherwise.</summary>
        /// <param name="condition">Condition to stop on. Should not be null.</param>
        /// <returns>Request info of found parent.</returns>
        public RequestInfo FirstOrEmpty(Func<RequestInfo, bool> condition)
        {
            var r = this;
            while (!r.IsEmpty && !condition(r))
                r = r.ParentOrWrapper;
            return r;
        }

        /// <summary>Asked service type.</summary>
        public readonly Type ServiceType;

        /// <summary>Required service type if specified.</summary>
        public readonly Type RequiredServiceType;

        /// <summary>Optional service key.</summary>
        public readonly object ServiceKey;

        /// <summary>Resolved factory ID, used to identify applied decorator.</summary>
        public readonly int FactoryID;

        /// <summary>False for Decorators and Wrappers.</summary>
        public readonly FactoryType FactoryType;

        /// <summary>Implementation type.</summary>
        public readonly Type ImplementationType;

        /// <summary>Relative number representing reuse lifespan.</summary>
        public int ReuseLifespan { get { return Reuse == null ? 0 : Reuse.Lifespan; } }

        /// <summary>Service reuse.</summary>
        public readonly IReuse Reuse;

        /// <summary>Simplified version of Push with most common properties.</summary>
        /// <param name="serviceType"></param> <param name="factoryID"></param> <param name="implementationType"></param>
        /// <param name="reuse"></param> <returns>Created info chain to current (parent) info.</returns>
        public RequestInfo Push(Type serviceType, int factoryID, Type implementationType, IReuse reuse)
        {
            return Push(serviceType, null, null, factoryID, FactoryType.Service, implementationType, reuse);
        }

        /// <summary>Creates info by supplying all the properties and chaining it with current (parent) info.</summary>
        /// <param name="serviceType"></param> <param name="requiredServiceType"></param>
        /// <param name="serviceKey"></param> <param name="factoryType"></param> <param name="factoryID"></param>
        /// <param name="implementationType"></param> <param name="reuse"></param>
        /// <returns>Created info chain to current (parent) info.</returns>
        public RequestInfo Push(Type serviceType, Type requiredServiceType, object serviceKey,
            int factoryID, FactoryType factoryType, Type implementationType, IReuse reuse)
        {
            return new RequestInfo(serviceType, requiredServiceType, serviceKey,
                factoryID, factoryType, implementationType, reuse, this);
        }

        private RequestInfo(
            Type serviceType, Type requiredServiceType, object serviceKey,
            int factoryID, FactoryType factoryType, Type implementationType, IReuse reuse,
            RequestInfo parentOrWrapper)
        {
            ParentOrWrapper = parentOrWrapper;

            // Service info:
            ServiceType = serviceType;
            RequiredServiceType = requiredServiceType;
            ServiceKey = serviceKey;

            // Implementation info:
            FactoryID = factoryID;
            FactoryType = factoryType;
            ImplementationType = implementationType;
            Reuse = reuse;
        }

        /// <summary>Returns all request until the root - parent is null.</summary>
        /// <returns>Requests from the last to first.</returns>
        public IEnumerable<RequestInfo> Enumerate()
        {
            for (var i = this; !i.IsEmpty; i = i.ParentOrWrapper)
                yield return i;
        }

        /// <summary>Prints request with all its parents to string.</summary> <returns>The string.</returns>
        public override string ToString()
        {
            if (IsEmpty)
                return "{empty}";

            var s = new StringBuilder();

            if (FactoryID != 0)
                s.Append('#').Append(FactoryID).Append(' ');

            if (FactoryType != FactoryType.Service)
                s.Append(FactoryType.ToString().ToLower()).Append(' ');
            if (ImplementationType != null && ImplementationType != ServiceType)
                s.Append(ImplementationType).Append(": ");

            s.Append(ServiceType);

            if (RequiredServiceType != null)
                s.Append(" with RequiredServiceType=").Append(RequiredServiceType);

            if (ServiceKey != null)
                s.Append(" with ServiceKey=").Append('{').Append(ServiceKey).Append('}');

            if (ReuseLifespan != 0)
                s.Append(" with ReuseLifespan=").Append(ReuseLifespan);

            if (!ParentOrWrapper.IsEmpty)
                s.AppendLine().Append("  in ").Append(ParentOrWrapper);

            return s.ToString();
        }

        /// <summary>Returns true if request info and passed object are equal, and their parents recursively are equal.</summary>
        /// <param name="obj"></param> <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as RequestInfo);
        }

        /// <summary>Returns true if request info and passed info are equal, and their parents recursively are equal.</summary>
        /// <param name="other"></param> <returns></returns>
        public bool Equals(RequestInfo other)
        {
            return other != null && EqualsWithoutParent(other)
                && (ParentOrWrapper == null && other.ParentOrWrapper == null
                || (ParentOrWrapper != null && ParentOrWrapper.EqualsWithoutParent(other.ParentOrWrapper)));
        }

        /// <summary>Compares with other info taking into account the properties but not the parents and their properties.</summary>
        /// <param name="other">Info to compare for equality.</param> <returns></returns>
        public bool EqualsWithoutParent(RequestInfo other)
        {
            return other.ServiceType == ServiceType
                && other.RequiredServiceType == RequiredServiceType
                && other.ServiceKey == ServiceKey

                && other.FactoryType == FactoryType
                && other.ImplementationType == ImplementationType
                && other.ReuseLifespan == ReuseLifespan;
        }

        /// <summary>Returns hash code combined from info fields plus its parent.</summary>
        /// <returns>Combined hash code.</returns>
        public override int GetHashCode()
        {
            var hash = 0;
            for (var i = this; !i.IsEmpty; i = i.ParentOrWrapper)
            {
                var currentHash = i.ServiceType.GetHashCode();
                if (i.RequiredServiceType != null)
                    currentHash = CombineHashCodes(currentHash, i.RequiredServiceType.GetHashCode());

                if (i.ServiceKey != null)
                    currentHash = CombineHashCodes(currentHash, i.ServiceKey.GetHashCode());

                if (i.FactoryType != FactoryType.Service)
                    currentHash = CombineHashCodes(currentHash, i.FactoryType.GetHashCode());

                if (i.ImplementationType != null && i.ImplementationType != i.ServiceType)
                    currentHash = CombineHashCodes(currentHash, i.ImplementationType.GetHashCode());

                if (i.ReuseLifespan != 0)
                    currentHash = CombineHashCodes(currentHash, i.ReuseLifespan);

                hash = hash == 0 ? currentHash : CombineHashCodes(hash, currentHash);
            }
            return hash;
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

    /// <summary>Used to represent multiple default service keys. 
    /// Exposes <see cref="RegistrationOrder"/> to determine order of service added.</summary>
    public sealed class DefaultKey
    {
        /// <summary>Default value.</summary>
        public static readonly DefaultKey Value = new DefaultKey(0);

        /// <summary>Allows to determine service registration order.</summary>
        public readonly int RegistrationOrder;

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

        /// <summary>Compares keys based on registration order.</summary>
        /// <param name="key">Key to compare with.</param>
        /// <returns>True if keys have the same order.</returns>
        public override bool Equals(object key)
        {
            var defaultKey = key as DefaultKey;
            return key == null || defaultKey != null && defaultKey.RegistrationOrder == RegistrationOrder;
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

    /// <summary>Custom exclude from test code coverage attribute for portability.</summary>
    public sealed class ExcludeFromCodeCoverageAttribute : Attribute
    {
        /// <summary>Optional reason of why the marked code is excluded from coverage.</summary>
        public readonly string Reason;

        /// <summary>Creates attribute with optional reason message.</summary> <param name="reason"></param>
        public ExcludeFromCodeCoverageAttribute(string reason = null)
        {
            Reason = reason;
        }
    }
}
