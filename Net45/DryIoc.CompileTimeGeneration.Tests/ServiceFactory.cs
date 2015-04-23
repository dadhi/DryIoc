using System;
using System.Collections.Generic;

namespace DryIoc.CompileTimeGeneration.Tests
{
    public partial class ServiceFactory : IResolverContext, IResolverContextProvider, IDisposable
    {
        public static void Register(Type serviceType, FactoryDelegate factoryDelegate)
        {
            _defaultResolutions = _defaultResolutions.AddOrUpdate(serviceType, factoryDelegate);
        }

        public static void Register(Type serviceType, object serviceKey, FactoryDelegate factoryDelegate)
        {
            var entry = _keyedResolutions.GetValueOrDefault(serviceType) as HashTree ?? HashTree.Empty;
            _keyedResolutions = _keyedResolutions.AddOrUpdate(serviceType, entry.AddOrUpdate(serviceKey, factoryDelegate));
        }

        private static HashTree _defaultResolutions = HashTree.Empty;//<Type -> FactoryDelegate>
        private static HashTree _keyedResolutions = HashTree.Empty;//<Type -> <Key -> FactoryDelegate>>

        public ServiceFactory(IScope singletonScope = null, IScope openedScope = null)
        {
            SingletonScope = singletonScope ?? new Scope();
            OpenedScope = openedScope;
            ScopeContext = new ThreadScopeContext();
        }

        public IResolverContext Resolver
        {
            get { return this; }
        }

        public void Dispose()
        {
            if (OpenedScope != null)
                OpenedScope.Dispose();
            else
            {
                SingletonScope.Dispose();
                var scopeContext = ScopeContext as IDisposable;
                if (scopeContext != null)
                    scopeContext.Dispose();
            }
        }

        public IScope SingletonScope { get; private set; }

        public IScope GetCurrentNamedScope(object name, bool throwIfNotFound)
        {
            var currentScope = ScopeContext == null ? OpenedScope : ScopeContext.GetCurrentOrDefault();
            if (currentScope == null)
            {
                if (throwIfNotFound) Throw.It(Error.NO_CURRENT_SCOPE);
                return null;
            }

            var matchingScope = GetMatchingScopeOrDefault(currentScope, name);
            if (matchingScope == null)
            {
                if (throwIfNotFound) Throw.It(Error.NO_MATCHED_SCOPE_FOUND, name);
                return null;
            }

            return matchingScope;
        }

        private static IScope GetMatchingScopeOrDefault(IScope scope, object name)
        {
            if (name != null)
                while (scope != null && !name.Equals(scope.Name))
                    scope = scope.Parent;
            return scope;
        }

        public IScope GetOrCreateResolutionScope(ref IScope scope, Type serviceType, object serviceKey)
        {
            return scope ?? (scope = new Scope(null, new KV(serviceType, serviceKey)));
        }

        public IScope GetMatchingResolutionScope(IScope scope, Type assignableFromServiceType, object serviceKey, bool outermost, bool throwIfNotFound)
        {
            var matchingScope = GetMatchingScopeOrDefault(scope, assignableFromServiceType, serviceKey, outermost);
            if (matchingScope == null)
            {
                if (throwIfNotFound) Throw.It(Error.NO_MATCHED_SCOPE_FOUND, new KV(assignableFromServiceType, serviceKey));
                return null;
            }
            return matchingScope;
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

        public object ResolveDefault(Type serviceType, IfUnresolved ifUnresolved, IScope scope)
        {
            var factoryDelegate = _defaultResolutions.GetValueOrDefault(serviceType) as FactoryDelegate;
            return factoryDelegate != null
                ? factoryDelegate(AppendableArray.Empty, this, null)
                : GetDefaultOrThrowIfUnresolved(serviceType, ifUnresolved);
        }

        public object ResolveKeyed(Type serviceType, object serviceKey, IfUnresolved ifUnresolved,
            Type requiredServiceType, IScope scope)
        {
            if (serviceKey == null && requiredServiceType == null)
                return ResolveDefault(serviceType, ifUnresolved, scope);

            serviceType = requiredServiceType ?? serviceType;
            var resolutions = _keyedResolutions.GetValueOrDefault(serviceType) as HashTree;
            if (resolutions != null)
            {
                var factoryDelegate = resolutions.GetValueOrDefault(serviceKey) as FactoryDelegate;
                if (factoryDelegate != null)
                    return factoryDelegate(AppendableArray.Empty, this, null);
            }

            return GetDefaultOrThrowIfUnresolved(serviceType, ifUnresolved);
        }

        public IEnumerable<object> ResolveMany(Type serviceType, object serviceKey, Type requiredServiceType,
            object compositeParentKey, IScope scope)
        {
            serviceType = requiredServiceType ?? serviceType;

            var resolutions = _keyedResolutions.GetValueOrDefault(serviceType) as HashTree;
            if (resolutions != null)
            {
                if (serviceKey != null)
                {
                    var factoryDelegate = resolutions.GetValueOrDefault(serviceKey) as FactoryDelegate;
                    if (factoryDelegate != null)
                        yield return factoryDelegate(AppendableArray.Empty, this, scope);
                }
                else
                {
                    foreach (var resolution in resolutions.Enumerate())
                    {
                        var factoryDelegate = (FactoryDelegate)resolution.Value;
                        yield return factoryDelegate(AppendableArray.Empty, this, scope);
                    }
                }
            }
            else
            {
                var factoryDelegate = _defaultResolutions.GetValueOrDefault(serviceType) as FactoryDelegate;
                if (factoryDelegate != null)
                    yield return factoryDelegate(AppendableArray.Empty, this, scope);
            }
        }

        private static object GetDefaultOrThrowIfUnresolved(Type serviceType, IfUnresolved ifUnresolved)
        {
            if (ifUnresolved == IfUnresolved.Throw) Throw.It(Error.UNABLE_TO_RESOLVE_SERVICE, serviceType);
            return null;
        }

        #endregion

        public IScope OpenedScope { get; private set; }
        public IScopeContext ScopeContext { get; private set; }

        public ServiceFactory OpenScope()
        {
            var newOpenedScope = new Scope(OpenedScope, null);

            // Replacing current context scope with new nested only if current is the same as nested parent, otherwise throw.
            ScopeContext.SetCurrent(scope =>
                 newOpenedScope.ThrowIf(scope != OpenedScope, Error.NOT_DIRECT_SCOPE_PARENT, OpenedScope, scope));

            return new ServiceFactory(SingletonScope, newOpenedScope);
        }

        public ServiceFactory OpenScopeWithoutContext()
        {
            var newOpenedScope = new Scope(OpenedScope, null);
            return new ServiceFactory(SingletonScope, newOpenedScope);
        }
    }

    public class KV
    {
        public readonly object Key, Value;
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

    public sealed class HashTree
    {
        public static readonly HashTree Empty = new HashTree(IntKeyTree.Empty);
        public bool IsEmpty { get { return _tree.IsEmpty; } }

        private sealed class KVWithConflicts : KV
        {
            public readonly KV[] Conflicts;
            public KVWithConflicts(KV kv, KV[] conflicts) : base(kv.Key, kv.Value) { Conflicts = conflicts; }
        }

        public HashTree AddOrUpdate(object key, object value)
        {
            return new HashTree(_tree.AddOrUpdate(key.GetHashCode(), new KV(key, value), UpdateConflictingKeyValue));
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

        public object GetValueOrDefault(object key)
        {
            var kv = _tree.GetValueOrDefault(key.GetHashCode()) as KV;
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

        private readonly IntKeyTree _tree;
        private HashTree(IntKeyTree tree) { _tree = tree; }

        #endregion
    }

    public static class Error
    {
        /// <summary>First error code to identify error range for other possible error code definitions.</summary>
        public readonly static int FIRST_ERROR_CODE = 0;

        /// <summary>List of error messages indexed with code.</summary>
        public readonly static List<string> Messages = new List<string>(100);

        public static readonly int
            UNABLE_TO_RESOLVE_SERVICE = Of(
                "Unable to resolve {0}." + Environment.NewLine +
                "Please ensure you have service registered (with proper key) - 95% of cases." + Environment.NewLine +
                "Remaining 5%: Service does not match the reuse scope, or service has wrong Setup.With(condition), or no Rules.WithUnknownServiceResolver(ForMyService)."),
            NO_CURRENT_SCOPE = Of(
                "No current scope available: probably you are registering to, or resolving from outside of scope."),
            NO_MATCHED_SCOPE_FOUND = Of(
                "Unable to find scope with matching name: {0}."),
            NOT_DIRECT_SCOPE_PARENT = Of(
                "Unable to OpenScope [{0}] because parent scope [{1}] is not current context scope [{2}]." + Environment.NewLine +
                "It is probably other scope was opened in between OR you forgot to Dispose some other scope!");

        public static int Of(string message)
        {
            Messages.Add(message);
            return FIRST_ERROR_CODE + Messages.Count - 1;
        }
    }

    public class ServiceFactoryException : InvalidOperationException
    {
        public int Error { get; private set; }

        public ServiceFactoryException(int error, string message)
            : base(message)
        {
            Error = error;
        }
    }

    public static class Throw
    {
        public static void It(int error, params object[] args)
        {
            var messageFormat = Error.Messages[error];
            var message = string.Format(messageFormat, args);
            throw new ServiceFactoryException(error, message);
        }
    }
}
