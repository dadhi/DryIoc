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
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DryIoc.Zero
{
    /// <summary>Service factory delegate which accepts resolver and resolution scope as parameters and should return service object.
    /// It is stateless because does not include state parameter as <c>DryIoc.FactoryDelegate</c>.</summary>
    /// <param name="r">Provides access to <see cref="IResolver"/> to enable dynamic dependency resolution inside factory delegate.</param>
    /// <param name="scope">Resolution scope parameter. May be null to enable on demand scope creation inside factory delegate.</param>
    /// <returns>Created service object.</returns>
    public delegate object StatelessFactoryDelegate(IResolverContext r, IScope scope);

    /// <summary>Provides methods to register default or keyed factory delegates.</summary>
    public interface IFactoryDelegateRegistrator
    {
        /// <summary>Registers factory delegate with corresponding service type.</summary>
        /// <param name="serviceType">Type</param> <param name="factoryDelegate">Delegate</param>
        void Register(Type serviceType, StatelessFactoryDelegate factoryDelegate);

        /// <summary>Registers factory delegate with corresponding service type and service key.</summary>
        /// <param name="serviceType">Type</param> <param name="serviceKey">Key</param> <param name="factoryDelegate">Delegate</param>
        void Register(Type serviceType, object serviceKey, StatelessFactoryDelegate factoryDelegate);
    }

    /// <summary>Minimal container which allow to register service factory delegates and then resolve service from them.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly",
        Justification = "Does not contain any unmanaged resources.")]
    public partial class ZeroContainer : IFactoryDelegateRegistrator, IResolverContext, IResolver, IScopeAccess, IDisposable
    {
        /// <summary>Creates container.</summary>
        /// <param name="scopeContext">(optional) Scope context, by default <see cref="ThreadScopeContext"/>.</param>
        public ZeroContainer(IScopeContext scopeContext = null)
            : this(Ref.Of(ImTreeMap.Empty), Ref.Of(ImTreeMap.Empty), new SingletonScope(), scopeContext, null, 0) { }
        
        private ZeroContainer(Ref<ImTreeMap> defaultFactories, Ref<ImTreeMap> keyedFactories, IScope singletonScope, 
            IScopeContext scopeContext, IScope openedScope, int disposed)
        {
            _defaultFactories = defaultFactories;
            _keyedFactories = keyedFactories;

            SingletonScope = singletonScope;
            ScopeContext = scopeContext;
            OpenedScope = openedScope;

            _disposed = disposed;
        }

        /// <summary>Opened scope or null in root container.</summary>
        public IScope OpenedScope { get; private set; }

        /// <summary>Scope context or null of not necessary.</summary>
        public IScopeContext ScopeContext { get; private set; }

        /// <summary>Creates new container with new current ambient scope.</summary>
        /// <returns>New container.</returns>
        public ZeroContainer OpenScope()
        {
            ThrowIfContainerDisposed();

            var nestedOpenedScope = new Scope(OpenedScope);

            // Replacing current context scope with new nested only if current is the same as nested parent, otherwise throw.
            if (ScopeContext != null)
                ScopeContext.SetCurrent(scope =>
                     nestedOpenedScope.ThrowIf(scope != OpenedScope, Error.NotDirectScopeParent, OpenedScope, scope));

            return new ZeroContainer(_defaultFactories, _keyedFactories, SingletonScope, ScopeContext, 
                nestedOpenedScope, _disposed);
        }

        /// <summary>Creates new container with new opened scope independent from context.</summary>
        /// <returns>New container.</returns>
        public ZeroContainer OpenScopeWithoutContext()
        {
            ThrowIfContainerDisposed();
            var newOpenedScope = new Scope(OpenedScope);
            return new ZeroContainer(_defaultFactories, _keyedFactories, SingletonScope, ScopeContext, newOpenedScope, 
                _disposed);
        }

        #region IFactoryDelegateRegistrator

        /// <summary>Registers factory delegate with corresponding service type.</summary>
        /// <param name="serviceType">Type</param> <param name="factoryDelegate">Delegate</param>
        public void Register(Type serviceType, StatelessFactoryDelegate factoryDelegate)
        {
            ThrowIfContainerDisposed();
            _defaultFactories.Swap(_ => _.AddOrUpdate(serviceType, factoryDelegate));
        }

        /// <summary>Registers factory delegate with corresponding service type and service key.</summary>
        /// <param name="serviceType">Type</param> <param name="serviceKey">Key</param> <param name="factoryDelegate">Delegate</param>
        public void Register(Type serviceType, object serviceKey, StatelessFactoryDelegate factoryDelegate)
        {
            ThrowIfContainerDisposed();
            _keyedFactories.Swap(_ =>
            {
                var entry = _.GetValueOrDefault(serviceType) as ImTreeMap ?? ImTreeMap.Empty;
                return _.AddOrUpdate(serviceType, entry.AddOrUpdate(serviceKey, factoryDelegate));
            });
        }

        private Ref<ImTreeMap> _defaultFactories; //<Type -> FactoryDelegate>
        private Ref<ImTreeMap> _keyedFactories; //<Type -> <Key -> FactoryDelegate>>

        #endregion

        /// <summary>Provides access to resolver.</summary>
        public IResolver Resolver
        {
            get { return this; }
        }

        /// <summary>Scopes access</summary>
        public IScopeAccess Scopes
        {
            get { return this; }
        }

        /// <summary>Disposes opened scope or root container including: Singletons, ScopeContext, Make default and keyed factories empty.</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly",
            Justification = "Does not container any unmanaged resources.")]
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1) return;

            if (OpenedScope != null)
                OpenedScope.Dispose();
            else
            {
                SingletonScope.Dispose();
                var scopeContext = ScopeContext as IDisposable;
                if (scopeContext != null)
                    scopeContext.Dispose();
                _defaultFactories = Ref.Of(ImTreeMap.Empty);
                _keyedFactories = Ref.Of(ImTreeMap.Empty);
            }
        }

        public bool IsDisposed
        {
            get { return _disposed == 1; }
        }

        private int _disposed;
        private void ThrowIfContainerDisposed()
        {
            Throw.If(_disposed == 1, Error.ContainerIsDisposed);
        }
        
        /// <summary>Scope containing container singletons.</summary>
        public IScope SingletonScope { get; private set; }

        /// <summary>Current scope.</summary>
        public IScope GetCurrentScope()
        {
            return GetCurrentNamedScope(null, false);
        }

        /// <summary>Returns current scope matching the <paramref name="name"/>. 
        /// If name is null then current scope is returned, or if there is no current scope then exception thrown.</summary>
        /// <param name="name">May be null</param> <returns>Found scope or throws exception.</returns>
        /// <param name="throwIfNotFound">Says to throw if no scope found.</param>
        /// <exception cref="ZeroContainerException"> with code <see cref="Error.NoMatchedScopeFound"/>.</exception>
        public IScope GetCurrentNamedScope(object name, bool throwIfNotFound)
        {
            var currentScope = ScopeContext == null ? OpenedScope : ScopeContext.GetCurrentOrDefault();
            
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
            return scope ?? (scope = new Scope(null, new KV(serviceType, serviceKey)));
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
        public IScope GetMatchingResolutionScope(IScope scope, Type assignableFromServiceType, object serviceKey, bool outermost, bool throwIfNotFound)
        {
            var matchingScope = GetMatchingScopeOrDefault(scope, assignableFromServiceType, serviceKey, outermost);
            return matchingScope
                ?? (IScope)Throw.If(throwIfNotFound, Error.NoMatchedScopeFound, new KV(assignableFromServiceType, serviceKey));
        }

        private static IScope GetMatchingScopeOrDefault(IScope scope, Type assignableFromServiceType, object serviceKey, bool outermost)
        {
            if (assignableFromServiceType == null && serviceKey == null)
                return scope;

            IScope matchedScope = null;
            while (scope != null)
            {
                var name = scope.Name as KV;
                if (name != null &&
                    (assignableFromServiceType == null || assignableFromServiceType.IsInstanceOfType(name.Key) &&
                    (serviceKey == null || serviceKey.Equals(name.Value))))
                {
                    matchedScope = scope;
                    if (!outermost) // break on first found match.
                        break;
                }
                scope = scope.Parent;
            }

            return matchedScope;
        }       

        #region IResolver

        /// <summary>Resolves service from container and returns created service object.</summary>
        /// <param name="serviceType">Service type to search and to return.</param>
        /// <param name="ifUnresolved">Says what to do if service is unresolved.</param>
        /// <param name="scope">Propagated resolution scope.</param>
        /// <returns>Created service object or default based on <paramref name="ifUnresolved"/> provided.</returns>
        public object ResolveDefault(Type serviceType, IfUnresolved ifUnresolved, IScope scope)
        {
            var result = _defaultFactories.Value.IsEmpty 
                ? ResolveGenerated(serviceType, scope) 
                : ResolveRegisteredFirst(serviceType, scope);
            return result ?? GetDefaultOrThrowIfUnresolved(serviceType, ifUnresolved);
        }

        private object ResolveRegisteredFirst(Type serviceType, IScope scope)
        {
            var factoryDelegate = _defaultFactories.Value.GetValueOrDefault(serviceType) as StatelessFactoryDelegate;
            return factoryDelegate != null ? factoryDelegate(this, scope) : ResolveGenerated(serviceType, scope);
        }

        /// <summary>Resolves keyed service from container and returns created service object.</summary>
        /// <param name="serviceType">Service type to search and to return.</param>
        /// <param name="serviceKey">Optional service key used for registering service.</param>
        /// <param name="ifUnresolved">Says what to do if service is unresolved.</param>
        /// <param name="requiredServiceType">Actual registered service type to use instead of <paramref name="serviceType"/>, 
        ///     or wrapped type for generic wrappers.  The type should be assignable to return <paramref name="serviceType"/>.</param>
        /// <param name="scope">Propagated resolution scope.</param>
        /// <returns>Created service object or default based on <paramref name="ifUnresolved"/> provided.</returns>
        /// <remarks>
        /// This method covers all possible resolution input parameters comparing to <see cref="IResolver.ResolveDefault"/>, and
        /// by specifying the same parameters as for <see cref="IResolver.ResolveDefault"/> should return the same result.
        /// </remarks>
        public object ResolveKeyed(Type serviceType, object serviceKey, IfUnresolved ifUnresolved, Type requiredServiceType, IScope scope)
        {
            if (serviceKey == null && requiredServiceType == null)
                return ResolveDefault(serviceType, ifUnresolved, scope);

            serviceType = requiredServiceType ?? serviceType;

            var keyedFactories = _keyedFactories.Value;
            if (!keyedFactories.IsEmpty)
            {
                var factories = keyedFactories.GetValueOrDefault(serviceType) as ImTreeMap;
                var factoryDelegate = factories == null ? null
                    : factories.GetValueOrDefault(serviceKey) as StatelessFactoryDelegate;
                if (factoryDelegate != null) 
                    return factoryDelegate(this, scope);                
            }

            return ResolveGenerated(serviceType, serviceKey, scope)
                   ?? GetDefaultOrThrowIfUnresolved(serviceType, ifUnresolved);
        }

        /// <summary>Resolves all services registered for specified <paramref name="serviceType"/>, or if not found returns
        /// empty enumerable. If <paramref name="serviceType"/> specified then returns only (single) service registered with
        /// this type. Excludes for result composite parent identified by <paramref name="compositeParentKey"/>.</summary>
        /// <param name="serviceType">Return type of an service item.</param>
        /// <param name="serviceKey">(optional) Resolve only single service registered with the key.</param>
        /// <param name="requiredServiceType">(optional) Actual registered service to search for.</param>
        /// <param name="compositeParentKey">(optional) Parent service key to exclude to support Composite pattern.</param>
        /// <param name="scope">propagated resolution scope, may be null.</param>
        /// <returns>Enumerable of found services or empty. Does Not throw if no service found.</returns>
        public IEnumerable<object> ResolveMany(Type serviceType, object serviceKey, Type requiredServiceType, object compositeParentKey, IScope scope)
        {
            serviceType = requiredServiceType ?? serviceType;
            
            var manyGenerated = ResolveManyGenerated(serviceType);
            if (compositeParentKey != null)
                manyGenerated = manyGenerated.Where(kv => !compositeParentKey.Equals(kv.Key));

            foreach (var generated in manyGenerated)
                yield return ((StatelessFactoryDelegate)generated.Value)(this, scope);

            var factories = _keyedFactories.Value.GetValueOrDefault(serviceType) as ImTreeMap;
            if (factories != null)
            {
                if (serviceKey != null)
                {
                    var factoryDelegate = factories.GetValueOrDefault(serviceKey) as StatelessFactoryDelegate;
                    if (factoryDelegate != null)
                        yield return factoryDelegate(this, scope);
                }
                else
                {
                    foreach (var resolution in factories.Enumerate())
                        if (compositeParentKey == null || !compositeParentKey.Equals(resolution.Key))
                            yield return ((StatelessFactoryDelegate)resolution.Value)(this, scope);
                }
            }
            else
            {
                var factoryDelegate = _defaultFactories.Value.GetValueOrDefault(serviceType) as StatelessFactoryDelegate;
                if (factoryDelegate != null)
                    yield return factoryDelegate(this, scope);
            }
        }

        private static object GetDefaultOrThrowIfUnresolved(Type serviceType, IfUnresolved ifUnresolved)
        {
            return Throw.If(ifUnresolved == IfUnresolved.Throw, Error.UnableToResolveService, serviceType);
        }

        #endregion
    }

    /// <summary>Key Value objects pair.</summary>
    public class KV
    {
        /// <summary>Key object.</summary>
        public readonly object Key;

        /// <summary>Value object.</summary>
        public readonly object Value;

        /// <summary>Creates pair.</summary> <param name="key"></param> <param name="value"></param>
        public KV(object key, object value) { Key = key; Value = value; }

        /// <summary>Returns true if both key and value are equal to corresponding key-value of other object.</summary>
        /// <param name="obj">Object to check equality with.</param> <returns>True if equal.</returns>
        public override bool Equals(object obj)
        {
            var other = obj as KV;
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
                return (Key == null ? 0 : Key.GetHashCode() * 397)
                     ^ (Value == null ? 0 : Value.GetHashCode());
            }
        }

        /// <summary>Creates nice string view for Key and Value.</summary><returns>String representation.</returns>
        public override string ToString()
        {
            return "[" + (Key ?? "null") + ", " + (Value ?? "null") + "]";
        }
    }

    /// <summary>Simple immutable AVL tree with integer keys and object values.</summary>
    public sealed class ImTreeMap
    {
        /// <summary>Represents Empty tree.</summary>
        public static readonly ImTreeMap Empty = new ImTreeMap(ImTreeMapIntToObj.Empty);

        /// <summary>Returns true if tree is empty.</summary>
        public bool IsEmpty { get { return _tree.IsEmpty; } }

        private sealed class KVWithConflicts : KV
        {
            public readonly KV[] Conflicts;
            public KVWithConflicts(KV kv, KV[] conflicts) : base(kv.Key, kv.Value) { Conflicts = conflicts; }
        }

        /// <summary>Creates new tree with added or updated value for corresponding key.</summary>
        /// <param name="key">Key.</param> <param name="value">Value.</param> <returns>New tree.</returns>
        public ImTreeMap AddOrUpdate(object key, object value)
        {
            return new ImTreeMap(_tree.AddOrUpdate(key.GetHashCode(), new KV(key, value), UpdateConflictingKeyValue));
        }

        private static object UpdateConflictingKeyValue(object entryOld, object entryNew)
        {
            var kvOld = (KV)entryOld;
            var kvNew = (KV)entryNew;

            var conflicts = kvOld is KVWithConflicts ? ((KVWithConflicts)kvOld).Conflicts : null;

            // if equal just replace with keeping conflicts intact.
            if (ReferenceEquals(kvOld.Key, kvNew.Key) || kvOld.Key.Equals(kvNew.Key))
                return conflicts == null ? kvNew : new KVWithConflicts(kvNew, conflicts);

            // if keys are not equal but hash is the same:
            // - if no previous conflicts then add new value to conflict with old one. 
            if (conflicts == null)
                return new KVWithConflicts(kvOld, new[] { kvNew });

            // - if some conflicts exist find key in conflict.
            var i = conflicts.Length - 1;
            while (i >= 0 && !Equals(conflicts[i].Key, kvNew.Key)) --i;

            var newConflicts = new KV[i != -1 ? conflicts.Length : conflicts.Length + 1];
            Array.Copy(conflicts, 0, newConflicts, 0, conflicts.Length);
            newConflicts[i != -1 ? i : conflicts.Length] = kvNew;

            return new KVWithConflicts(kvOld, newConflicts);
        }

        /// <summary>Looks for value added with key or will return null if key is not found.</summary>
        /// <param name="key">Key</param> <returns>Found value or null if not found.</returns>
        public object GetValueOrDefault(object key)
        {
            var kv = _tree.Height == 0 ? null : _tree.GetValueOrDefault(key.GetHashCode()) as KV;
            return kv != null && (ReferenceEquals(key, kv.Key) || key.Equals(kv.Key))
                ? kv.Value : GetConflictedValueOrDefault(kv, key);
        }

        private static object GetConflictedValueOrDefault(KV kv, object key)
        {
            var conflicts = kv is KVWithConflicts ? ((KVWithConflicts)kv).Conflicts : null;
            if (conflicts != null)
                for (var i = 0; i < conflicts.Length; ++i)
                    if (Equals(conflicts[i].Key, key))
                        return conflicts[i].Value;
            return null;
        }

        /// <summary>Returns all sub-trees enumerated from left to right.</summary> 
        /// <returns>Enumerated pairs.</returns>
        public IEnumerable<KV> Enumerate()
        {
            if (!_tree.IsEmpty)
                foreach (var t in _tree.Enumerate())
                {
                    yield return (KV)t.Value;
                    if (t.Value is KVWithConflicts)
                    {
                        var conflicts = ((KVWithConflicts)t.Value).Conflicts;
                        for (var i = 0; i < conflicts.Length; ++i)
                            yield return conflicts[i];
                    }
                }
        }

        #region Implementation

        private readonly ImTreeMapIntToObj _tree;
        private ImTreeMap(ImTreeMapIntToObj tree) { _tree = tree; }

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
            UnableToResolveService = Of(
                "Unable to resolve {0}." + Environment.NewLine +
                "Please ensure you have service registered (with proper key) - 95% of cases." + Environment.NewLine +
                "Remaining 5%: Service does not match the reuse scope, or service has wrong Setup.With(condition), or no Rules.WithUnknownServiceResolver(ForMyService)."),
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable",
        Justification = "Not available in PCL.")]
    public class ZeroContainerException : InvalidOperationException
    {
        /// <summary>Error code.</summary>
        public int Error { get; private set; }

        /// <summary>Creates exception.</summary>
        /// <param name="error">Code.</param> <param name="message">Message.</param>
        public ZeroContainerException(int error, string message)
            : base(message)
        {
            Error = error;
        }
    }

    internal static class Throw
    {
        public static void It(int error, params object[] args)
        {
            var messageFormat = Error.Messages[error];
            var message = string.Format(messageFormat, args);
            throw new ZeroContainerException(error, message);
        }

        public static object If(bool condition, int error, params object[] args)
        {
            if (condition) It(error, args);
            return null;
        }
    }
}
