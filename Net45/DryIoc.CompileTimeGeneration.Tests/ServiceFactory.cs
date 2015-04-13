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

        public ServiceFactory()
        {
            SingletonScope = new Scope();
        }

        public IResolverContext Resolver
        {
            get { return this; }
        }

        public void Dispose()
        {
            SingletonScope.Dispose();
        }

        public IScope SingletonScope { get; private set; }

        public IScope GetCurrentNamedScope(object name, bool throwIfNotFound)
        {
            throw new NotImplementedException();
        }

        public IScope GetOrCreateResolutionScope(ref IScope scope, Type serviceType, object serviceKey)
        {
            return scope ?? (scope = new Scope(null, new KV<Type, object>(serviceType, serviceKey)));
        }

        public IScope GetMatchingResolutionScope(IScope scope, Type assignableFromServiceType, object serviceKey, bool outermost,
            bool throwIfNotFound)
        {
            throw new NotImplementedException();
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
            return ifUnresolved == IfUnresolved.ReturnDefault
                ? null
                : Throw.For<object>(Error.UNABLE_TO_RESOLVE_SERVICE, serviceType);
        }

        #endregion
    }

    public class KV
    {
        public object Key, Value;
        public KV(object key, object value) { Key = key; Value = value; }
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
}
