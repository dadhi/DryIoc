using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using ImTools;

namespace Playground
{
    public class ImHashMapBenchmarks
    {
        private static readonly Type[] _keys = typeof(Dictionary<,>).Assembly.GetTypes().Take(100).ToArray();

        [MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
        public class Populate
        {
            [Benchmark(Baseline = true)]
            public ImHashMap<Type, string> AddOrUpdate()
            {
                var map = ImHashMap<Type, string>.Empty;

                foreach (var key in _keys)
                    map = map.AddOrUpdate(key, "a", out _, out _, (type, _, v) => v);

                return map.AddOrUpdate(typeof(ImHashMapBenchmarks), "!", out _, out _, (type, _, v) => v);
            }

            [Benchmark]
            public V1.ImHashMap<Type, string> AddOrUpdate_v1()
            {
                var map = V1.ImHashMap<Type, string>.Empty;

                foreach (var key in _keys)
                    map = map.AddOrUpdate(key, "a", out _, out _, (type, _, v) => v);

                return map.AddOrUpdate(typeof(ImHashMapBenchmarks), "!", out _, out _, (type, _, v) => v);
            }

            //[Benchmark]
            public ConcurrentDictionary<Type, string> ConcurrentDict()
            {
                var map = new ConcurrentDictionary<Type, string>();

                foreach (var key in _keys)
                    map.TryAdd(key, "a");

                map.TryAdd(typeof(ImHashMapBenchmarks), "!!!");

                return map;
            }
        }

        [MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
        public class Lookup
        {
            public static ImHashMap<Type, string> AddOrUpdate()
            {
                var map = ImHashMap<Type, string>.Empty;

                foreach (var key in _keys)
                    map = map.AddOrUpdate(key, "a");

                map = map.AddOrUpdate(typeof(ImHashMapBenchmarks), "!");

                return map;
            }

            private static readonly ImHashMap<Type, string> _map = AddOrUpdate();

            public static V1.ImHashMap<Type, string> AddOrUpdate_v1()
            {
                var map = V1.ImHashMap<Type, string>.Empty;

                foreach (var key in _keys)
                    map = map.AddOrUpdate(key, "a");

                map = map.AddOrUpdate(typeof(ImHashMapBenchmarks), "!");

                return map;
            }

            private static readonly V1.ImHashMap<Type, string> _mapV1 = AddOrUpdate_v1();

            public static ConcurrentDictionary<Type, string> ConcurrentDict()
            {
                var map = new ConcurrentDictionary<Type, string>();

                foreach (var key in _keys)
                    map.TryAdd(key, "a");

                map.TryAdd(typeof(ImHashMapBenchmarks), "!!!");

                return map;
            }

            private static readonly ConcurrentDictionary<Type, string> _dict = ConcurrentDict();

            public static Type LookupKey = typeof(ImHashMapBenchmarks);

            [Benchmark(Baseline = true)]
            public string TryFind()
            {
                _map.TryFind(LookupKey, out var result);
                return result;
            }

            [Benchmark]
            public string TryFind_v1()
            {
                _mapV1.TryFind(LookupKey, out var result);
                return result;
            }

            //[Benchmark(Baseline = true)]
            public string GetValueOrDefault()
            {
                return _map.GetValueOrDefault(LookupKey);
            }

            //[Benchmark]
            public string GetValueOrDefault_v1()
            {
                return _mapV1.GetValueOrDefault(LookupKey);
            }

            //[Benchmark]
            public string ConcurrentDict_TryGet()
            {
                _dict.TryGetValue(LookupKey, out var result);
                return result;
            }
        }
    }
}

namespace V1
{
    using System.Runtime.CompilerServices; // For [MethodImpl(AggressiveInlining)]

    /// Immutable http://en.wikipedia.org/wiki/AVL_tree 
    /// where node key is the hash code of <typeparamref name="K"/>.
    public class ImHashMap<K, V>
    {
        /// Empty tree to start with. 
        public static readonly ImHashMap<K, V> Empty = new Branch();

        /// Returns true if tree is empty. Valid for a `Branch`.
        public bool IsEmpty => Height == 0;

        /// Calculated key hash.
        public readonly int Hash;

        /// Key of type K that should support <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode"/>
        public readonly K Key;

        /// Value of any type V.
        public readonly V Value;

        /// Left sub-tree/branch, or empty.
        public virtual ImHashMap<K, V> Left => Empty;

        /// Right sub-tree/branch, or empty.
        public virtual ImHashMap<K, V> Right => Empty;

        /// Height of longest sub-tree/branch plus 1. It is 0 for empty tree, and 1 for single node tree.
        public virtual int Height => 1;

        /// Conflicts
        public virtual KV<K, V>[] Conflicts => null;

        /// The branch node
        public class Branch : ImHashMap<K, V>
        {
            /// Left sub-tree/branch, or empty.
            public override ImHashMap<K, V> Left { get; }

            /// Right sub-tree/branch, or empty.
            public override ImHashMap<K, V> Right { get; }

            /// Height of longest sub-tree/branch plus 1. It is 0 for empty tree, and 1 for single node tree.
            public override int Height { get; }

            /// Empty tree constructor
            public Branch() { }

            /// Constructs the branch node
            public Branch(int hash, K key, V value, ImHashMap<K, V> left, ImHashMap<K, V> right)
                : base(hash, key, value)
            {
                Left = left;
                Right = right;
                Height = 1 + (left.Height > right.Height ? left.Height : right.Height);
            }

            /// Creates the branch node with known height
            public Branch(int hash, K key, V value, ImHashMap<K, V> left, ImHashMap<K, V> right, int height)
                : base(hash, key, value)
            {
                Left = left;
                Right = right;
                Height = height;
            }
        }

        /// Branch with the conflicts
        public sealed class ConflictsBranch : Branch
        {
            /// In case of Hash conflicts for different keys contains conflicted keys with their values.
            public override KV<K, V>[] Conflicts { get; }

            /// Creates the branch node
            internal ConflictsBranch(
                int hash, K key, V value, KV<K, V>[] conflicts,
                ImHashMap<K, V> left, ImHashMap<K, V> right) :
                base(hash, key, value, left, right)
            {
                Conflicts = conflicts;
            }

            /// Creates the branch node with known height
            internal ConflictsBranch(
                int hash, K key, V value, KV<K, V>[] conflicts,
                ImHashMap<K, V> left, ImHashMap<K, V> right, int height) :
                base(hash, key, value, left, right, height)
            {
                Conflicts = conflicts;
            }
        }

        /// <summary>Returns new tree with added key-value. 
        /// If value with the same key is exist then the value is replaced.</summary>
        /// <param name="key">Key to add.</param><param name="value">Value to add.</param>
        /// <returns>New tree with added or updated key-value.</returns>
        public ImHashMap<K, V> AddOrUpdate(K key, V value) =>
            AddOrUpdate(key.GetHashCode(), key, value);

        /// <summary>Returns new tree with added key-value. If value with the same key is exist, then
        /// if <paramref name="update"/> is not specified: then existing value will be replaced by <paramref name="value"/>;
        /// if <paramref name="update"/> is specified: then update delegate will decide what value to keep.</summary>
        /// <param name="key">Key to add.</param><param name="value">Value to add.</param>
        /// <param name="update">Update handler.</param>
        /// <returns>New tree with added or updated key-value.</returns>
        public ImHashMap<K, V> AddOrUpdate(K key, V value, Update<V> update) =>
            AddOrUpdate(key.GetHashCode(), key, value, update);

        /// Returns the previous value if updated.
        [MethodImpl((MethodImplOptions)256)]
        public ImHashMap<K, V> AddOrUpdate(K key, V value, out bool isUpdated, out V updatedOldValue, Update<K, V> update) =>
            AddOrUpdate(key.GetHashCode(), key, value, update, out isUpdated, out updatedOldValue);

        /// <summary>Looks for <paramref name="key"/> and replaces its value with new <paramref name="value"/>, or 
        /// runs custom update handler (<paramref name="update"/>) with old and new value to get the updated result.</summary>
        /// <param name="key">Key to look for.</param>
        /// <param name="value">New value to replace key value with.</param>
        /// <param name="update">(optional) Delegate for custom update logic, it gets old and new <paramref name="value"/>
        /// as inputs and should return updated value as output.</param>
        /// <returns>New tree with updated value or the SAME tree if no key found.</returns>
        public ImHashMap<K, V> Update(K key, V value, Update<V> update = null) =>
            Update(key.GetHashCode(), key, value, update);

        /// <summary>Looks for key in a tree and returns the key value if found, or <paramref name="defaultValue"/> otherwise.</summary>
        /// <param name="key">Key to look for.</param> <param name="defaultValue">(optional) Value to return if key is not found.</param>
        /// <returns>Found value or <paramref name="defaultValue"/>.</returns>
        [MethodImpl((MethodImplOptions)256)]
        public V GetValueOrDefault(K key, V defaultValue = default(V))
        {
            var t = this;
            var hash = key.GetHashCode();
            while (t.Height != 0 && t.Hash != hash)
                t = hash < t.Hash ? t.Left : t.Right;
            return t.Height != 0 && (ReferenceEquals(key, t.Key) || key.Equals(t.Key))
                ? t.Value : t.GetConflictedValueOrDefault(key, defaultValue);
        }

        /// <summary>Returns true if key is found and sets the value.</summary>
        /// <param name="key">Key to look for.</param> <param name="value">Result value</param>
        /// <returns>True if key found, false otherwise.</returns>
        [MethodImpl((MethodImplOptions)256)]
        public bool TryFind(K key, out V value)
        {
            var hash = key.GetHashCode();

            var t = this;
            while (t.Height != 0 && t.Hash != hash)
                t = hash < t.Hash ? t.Left : t.Right;

            if (t.Height != 0 && (ReferenceEquals(key, t.Key) || key.Equals(t.Key)))
            {
                value = t.Value;
                return true;
            }

            return t.TryFindConflictedValue(key, out value);
        }

        /// <summary>Depth-first in-order traversal as described in http://en.wikipedia.org/wiki/Tree_traversal
        /// The only difference is using fixed size array instead of stack for speed-up (~20% faster than stack).</summary>
        /// <returns>Sequence of enumerated key value pairs.</returns>
        public IEnumerable<KV<K, V>> Enumerate()
        {
            if (Height == 0)
                yield break;

            var parents = new ImHashMap<K, V>[Height];

            var node = this;
            var parentCount = -1;
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
                    yield return new KV<K, V>(node.Key, node.Value);

                    if (node is ConflictsBranch conflictNode)
                    {
                        var conflicts = conflictNode.Conflicts;
                        for (var i = 0; i < conflicts.Length; i++)
                            yield return conflicts[i];
                    }

                    node = node.Right;
                }
            }
        }

        /// <summary>Removes or updates value for specified key, or does nothing if key is not found.
        /// Based on Eric Lippert http://blogs.msdn.com/b/ericlippert/archive/2008/01/21/immutability-in-c-part-nine-academic-plus-my-avl-tree-implementation.aspx </summary>
        /// <param name="key">Key to look for.</param> 
        /// <returns>New tree with removed or updated value.</returns>
        public ImHashMap<K, V> Remove(K key) =>
            Remove(key.GetHashCode(), key);

        /// <summary>Outputs key value pair</summary>
        public override string ToString() => IsEmpty ? "empty" : (Key + ":" + Value);

        #region Implementation

        private ImHashMap() { }

        /// Creates the leaf node
        protected ImHashMap(int hash, K key, V value)
        {
            Hash = hash;
            Key = key;
            Value = value;
        }

        /// It is fine
        public ImHashMap<K, V> AddOrUpdate(int hash, K key, V value)
        {
            return Height == 0  // add new node
                ? new ImHashMap<K, V>(hash, key, value)
                : (hash == Hash // update found node
                    ? (ReferenceEquals(Key, key) || Key.Equals(key)
                        ? CreateBranch(hash, key, value, Conflicts, Left, Right, Height)
                        : UpdateValueAndResolveConflicts(key, value, null, false))
                    : (hash < Hash  // search for node
                        ? (Height == 1
                            ? CreateBranch(Hash, Key, Value, Conflicts, new ImHashMap<K, V>(hash, key, value), Right, height: 2)
                            : Balance(Hash, Key, Value, Conflicts, Left.AddOrUpdate(hash, key, value), Right))
                        : (Height == 1
                            ? CreateBranch(Hash, Key, Value, Conflicts, Left, new ImHashMap<K, V>(hash, key, value), height: 2)
                            : Balance(Hash, Key, Value, Conflicts, Left, Right.AddOrUpdate(hash, key, value)))));
        }

        private ImHashMap<K, V> AddOrUpdate(int hash, K key, V value, Update<V> update)
        {
            return Height == 0
                ? new ImHashMap<K, V>(hash, key, value)
                : hash == Hash // update
                ? (ReferenceEquals(Key, key) || Key.Equals(key)
                    ? CreateBranch(hash, key, update(Value, value), Conflicts, Left, Right, Height)
                    : UpdateValueAndResolveConflicts(key, value, update, false))
                : (hash < Hash
                    ? With(Left.AddOrUpdate(hash, key, value, update), Right)
                    : With(Left, Right.AddOrUpdate(hash, key, value, update)));
        }

        private ImHashMap<K, V> AddOrUpdate(
            int hash, K key, V value, Update<K, V> update, out bool updated, out V oldValue)
        {
            updated = false;
            oldValue = default(V);

            if (Height == 0)
                return new ImHashMap<K, V>(hash, key, value);

            if (hash == Hash)
            {
                if (ReferenceEquals(Key, key) || Key.Equals(key))
                {
                    var newValue = update(Key, Value, value);
                    if (ReferenceEquals(newValue, Value) || newValue?.Equals(Value) == true)
                        return this;

                    updated = true;
                    oldValue = Value;
                    return CreateBranch(hash, key, newValue, Conflicts, Left, Right, Height);
                }

                if (Conflicts == null) // add only if updateOnly is false.
                    return new ConflictsBranch(Hash, Key, Value, new[] { new KV<K, V>(key, value) }, Left, Right, Height);
                return UpdateValueAndResolveConflicts(key, value, update, false, ref updated, ref oldValue);
            }

            if (hash < Hash)
            {
                var newLeft = Left.AddOrUpdate(hash, key, value, update, out updated, out oldValue);
                return newLeft == Left ? this : Balance(Hash, Key, Value, Conflicts, newLeft, Right);
            }
            else
            {
                var newRight = Right.AddOrUpdate(hash, key, value, update, out updated, out oldValue);
                return newRight == Right ? this : Balance(Hash, Key, Value, Conflicts, Left, newRight);
            }
        }

        /// It is fine.
        public ImHashMap<K, V> Update(int hash, K key, V value, Update<V> update = null)
        {
            return Height == 0 ? this
                : hash == Hash
                    ? (ReferenceEquals(Key, key) || Key.Equals(key)
                        ? CreateBranch(hash, key, update == null ? value : update(Value, value), Conflicts, Left, Right, Height)
                        : UpdateValueAndResolveConflicts(key, value, update, true))
                    : (hash < Hash
                        ? With(Left.Update(hash, key, value, update), Right)
                        : With(Left, Right.Update(hash, key, value, update)));
        }

        private ImHashMap<K, V> UpdateValueAndResolveConflicts(K key, V value, Update<V> update, bool updateOnly)
        {
            if (Conflicts == null) // add only if updateOnly is false.
                return updateOnly ? this
                    : new ConflictsBranch(Hash, Key, Value, new[] { new KV<K, V>(key, value) }, Left, Right, Height);

            var found = Conflicts.Length - 1;
            while (found >= 0 && !Equals(Conflicts[found].Key, Key)) --found;
            if (found == -1)
            {
                if (updateOnly) return this;
                var newConflicts = new KV<K, V>[Conflicts.Length + 1];
                Array.Copy(Conflicts, 0, newConflicts, 0, Conflicts.Length);
                newConflicts[Conflicts.Length] = new KV<K, V>(key, value);
                return new ConflictsBranch(Hash, Key, Value, newConflicts, Left, Right, Height);
            }

            var conflicts = new KV<K, V>[Conflicts.Length];
            Array.Copy(Conflicts, 0, conflicts, 0, Conflicts.Length);
            conflicts[found] = new KV<K, V>(key, update == null ? value : update(Conflicts[found].Value, value));
            return new ConflictsBranch(Hash, Key, Value, conflicts, Left, Right, Height);
        }

        private ImHashMap<K, V> UpdateValueAndResolveConflicts(
            K key, V value, Update<K, V> update, bool updateOnly, ref bool updated, ref V oldValue)
        {
            if (Conflicts == null) // add only if updateOnly is false.
                return updateOnly ? this
                    : new ConflictsBranch(Hash, Key, Value, new[] { new KV<K, V>(key, value) }, Left, Right, Height);

            var found = Conflicts.Length - 1;
            while (found >= 0 && !Equals(Conflicts[found].Key, Key)) --found;
            if (found == -1)
            {
                if (updateOnly)
                    return this;
                var newConflicts = new KV<K, V>[Conflicts.Length + 1];
                Array.Copy(Conflicts, 0, newConflicts, 0, Conflicts.Length);
                newConflicts[Conflicts.Length] = new KV<K, V>(key, value);
                return new ConflictsBranch(Hash, Key, Value, newConflicts, Left, Right, Height);
            }

            var conflicts = new KV<K, V>[Conflicts.Length];
            Array.Copy(Conflicts, 0, conflicts, 0, Conflicts.Length);

            if (update == null)
                conflicts[found] = new KV<K, V>(key, value);
            else
            {
                var conflict = conflicts[found];
                var newValue = update(conflict.Key, conflict.Value, value);
                if (ReferenceEquals(newValue, conflict.Value) || newValue?.Equals(conflict.Value) == true)
                    return this;

                updated = true;
                oldValue = conflict.Value;
                conflicts[found] = new KV<K, V>(key, newValue);
            }

            return new ConflictsBranch(Hash, Key, Value, conflicts, Left, Right, Height);
        }

        /// It is fine.
        public V GetConflictedValueOrDefault(K key, V defaultValue)
        {
            if (Conflicts != null)
                for (var i = Conflicts.Length - 1; i >= 0; --i)
                    if (Equals(Conflicts[i].Key, key))
                        return Conflicts[i].Value;
            return defaultValue;
        }

        private bool TryFindConflictedValue(K key, out V value)
        {
            if (Height != 0 && Conflicts != null)
                for (var i = Conflicts.Length - 1; i >= 0; --i)
                    if (Equals(Conflicts[i].Key, key))
                    {
                        value = Conflicts[i].Value;
                        return true;
                    }

            value = default(V);
            return false;
        }

        private ImHashMap<K, V> With(ImHashMap<K, V> left, ImHashMap<K, V> right) =>
            left == Left && right == Right ? this : Balance(Hash, Key, Value, Conflicts, left, right);

        private static ImHashMap<K, V> CreateBranch(int hash, K key, V value, KV<K, V>[] conflicts,
            ImHashMap<K, V> left, ImHashMap<K, V> right)
        {
            if (conflicts == null)
            {
                if (left == Empty && left == right)
                    return new ImHashMap<K, V>(hash, key, value);
                return new Branch(hash, key, value, left, right);
            }

            return new ConflictsBranch(hash, key, value, conflicts, left, right);
        }

        private static ImHashMap<K, V> CreateBranch(int hash, K key, V value, KV<K, V>[] conflicts,
            ImHashMap<K, V> left, ImHashMap<K, V> right, int height)
        {
            if (conflicts == null)
            {
                if (height == 1)
                    return new ImHashMap<K, V>(hash, key, value);
                return new Branch(hash, key, value, left, right, height);
            }
            return new ConflictsBranch(hash, key, value, conflicts, left, right, height);
        }

        private static ImHashMap<K, V> Balance(
            int hash, K key, V value, KV<K, V>[] conflicts,
            ImHashMap<K, V> left, ImHashMap<K, V> right)
        {
            var delta = left.Height - right.Height;
            if (delta > 1) // left is longer by 2, rotate left
            {
                var leftLeft = left.Left;
                var leftRight = left.Right;
                if (leftRight.Height > leftLeft.Height)
                {
                    // double rotation:
                    //      5     =>     5     =>     4
                    //   2     6      4     6      2     5
                    // 1   4        2   3        1   3     6
                    //    3        1
                    return CreateBranch(leftRight.Hash, leftRight.Key, leftRight.Value, leftRight.Conflicts,
                        CreateBranch(left.Hash, left.Key, left.Value, left.Conflicts, leftLeft, leftRight.Left),
                        CreateBranch(hash, key, value, conflicts, leftRight.Right, right));
                }

                // one rotation:
                //      5     =>     2
                //   2     6      1     5
                // 1   4              4   6
                return CreateBranch(left.Hash, left.Key, left.Value, left.Conflicts,
                    leftLeft, CreateBranch(hash, key, value, conflicts, leftRight, right));
            }

            if (delta < -1)
            {
                var rightLeft = right.Left;
                var rightRight = right.Right;
                if (rightLeft.Height > rightRight.Height)
                {
                    return CreateBranch(rightLeft.Hash, rightLeft.Key, rightLeft.Value, rightLeft.Conflicts,
                        CreateBranch(hash, key, value, conflicts, left, rightLeft.Left),
                        CreateBranch(right.Hash, right.Key, right.Value, right.Conflicts, rightLeft.Right, rightRight));
                }

                return CreateBranch(right.Hash, right.Key, right.Value, right.Conflicts,
                    CreateBranch(hash, key, value, conflicts, left, rightLeft),
                    rightRight);
            }

            return CreateBranch(hash, key, value, conflicts, left, right);
        }

        internal ImHashMap<K, V> Remove(int hash, K key, bool ignoreKey = false)
        {
            if (Height == 0)
                return this;

            ImHashMap<K, V> result;
            if (hash == Hash) // found node
            {
                if (ignoreKey || Equals(Key, key))
                {
                    if (!ignoreKey && Conflicts != null)
                        return ReplaceRemovedWithConflicted();

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
                        result = CreateBranch(
                            successor.Hash, successor.Key, successor.Value, successor.Conflicts,
                            Left, Right.Remove(successor.Hash, default(K), ignoreKey: true));
                    }
                }
                else if (Conflicts != null)
                    return TryRemoveConflicted(key);
                else
                    return this; // if key is not matching and no conflicts to lookup - just return
            }
            else if (hash < Hash)
                result = Balance(Hash, Key, Value, Conflicts, Left.Remove(hash, key, ignoreKey), Right);
            else
                result = Balance(Hash, Key, Value, Conflicts, Left, Right.Remove(hash, key, ignoreKey));

            return result;
        }

        private ImHashMap<K, V> TryRemoveConflicted(K key)
        {
            var index = Conflicts.Length - 1;
            while (index >= 0 && !Equals(Conflicts[index].Key, key)) --index;
            if (index == -1) // key is not found in conflicts - just return
                return this;

            if (Conflicts.Length == 1)
                return new Branch(Hash, Key, Value, Left, Right, Height);
            var lessConflicts = new KV<K, V>[Conflicts.Length - 1];
            var newIndex = 0;
            for (var i = 0; i < Conflicts.Length; ++i)
                if (i != index) lessConflicts[newIndex++] = Conflicts[i];
            return new ConflictsBranch(Hash, Key, Value, lessConflicts, Left, Right, Height);
        }

        private ImHashMap<K, V> ReplaceRemovedWithConflicted()
        {
            if (Conflicts.Length == 1)
                return new Branch(Hash, Conflicts[0].Key, Conflicts[0].Value, Left, Right, Height);
            var lessConflicts = new KV<K, V>[Conflicts.Length - 1];
            Array.Copy(Conflicts, 1, lessConflicts, 0, lessConflicts.Length);
            return new ConflictsBranch(Hash, Conflicts[0].Key, Conflicts[0].Value, lessConflicts, Left, Right, Height);
        }

        #endregion
    }
}
