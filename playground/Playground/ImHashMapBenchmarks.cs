using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using ImTools;

namespace Playground
{
    public class ImHashMapBenchmarks
    {
        private static readonly Type[] _keys = typeof(Dictionary<,>).Assembly.GetTypes().Take(1000).ToArray();

        [MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
        public class Populate
        {
            /*
            ## 15.01.2019:

                     Method |     Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
            --------------- |---------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
                AddOrUpdate | 15.84 us | 0.1065 us | 0.0944 us |  1.00 |    0.00 |      7.3242 |           - |           - |            33.87 KB |
             AddOrUpdate_v1 | 27.00 us | 0.1792 us | 0.1588 us |  1.71 |    0.02 |      7.7515 |           - |           - |            35.77 KB |

            
            ## 16.01.2019: Total test against ImHashMap V1, System ImmutableDictionary and ConcurrentDictionary

BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.523 (1803/April2018Update/Redstone4)
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
Frequency=2156249 Hz, Resolution=463.7683 ns, Timer=TSC
.NET Core SDK=2.2.100
  [Host]     : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT
  DefaultJob : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT


         Method | Count |           Mean |         Error |        StdDev |         Median | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
--------------- |------ |---------------:|--------------:|--------------:|---------------:|------:|--------:|------------:|------------:|------------:|--------------------:|
 AddOrUpdate_v1 |    10 |       987.6 ns |      36.53 ns |      99.99 ns |       934.3 ns |  0.94 |    0.17 |      0.5589 |           - |           - |             2.58 KB |
    AddOrUpdate |    10 |     1,067.5 ns |      50.43 ns |     136.33 ns |     1,073.5 ns |  1.00 |    0.00 |      0.4044 |           - |           - |             1.87 KB |
 ConcurrentDict |    10 |     1,943.9 ns |      92.31 ns |      81.83 ns |     1,921.3 ns |  1.85 |    0.25 |      0.6371 |           - |           - |             2.95 KB |
 AddOrUpdate_v2 |    10 |     2,688.3 ns |     229.79 ns |     677.55 ns |     2,342.9 ns |  2.50 |    0.77 |      0.4349 |           - |           - |             2.02 KB |
  ImmutableDict |    10 |     5,903.1 ns |     749.28 ns |     801.72 ns |     5,607.8 ns |  5.66 |    1.06 |      0.5875 |           - |           - |             2.73 KB |
                |       |                |               |               |                |       |         |             |             |             |                     |
 ConcurrentDict |   100 |    14,476.1 ns |   1,184.05 ns |   1,215.93 ns |    14,193.3 ns |  0.48 |    0.21 |      3.6011 |      0.0305 |           - |            16.66 KB |
 AddOrUpdate_v1 |   100 |    16,999.4 ns |   1,522.75 ns |   4,441.93 ns |    14,281.8 ns |  0.59 |    0.25 |      8.4686 |           - |           - |            39.05 KB |
 AddOrUpdate_v2 |   100 |    28,695.4 ns |      41.78 ns |      32.62 ns |    28,697.6 ns |  0.94 |    0.33 |      7.7515 |           - |           - |            35.81 KB |
    AddOrUpdate |   100 |    31,854.4 ns |   2,882.03 ns |   8,497.72 ns |    36,547.9 ns |  1.00 |    0.00 |      7.3242 |           - |           - |            33.91 KB |
  ImmutableDict |   100 |    89,602.2 ns |   1,873.19 ns |   2,229.89 ns |    88,767.5 ns |  2.98 |    1.05 |      9.3994 |           - |           - |            43.68 KB |
                |       |                |               |               |                |       |         |             |             |             |                     |
 ConcurrentDict |  1000 |   219,064.7 ns |     559.14 ns |     466.91 ns |   218,894.7 ns |  0.69 |    0.01 |     49.3164 |     17.8223 |           - |           254.29 KB |
 AddOrUpdate_v1 |  1000 |   297,651.6 ns |   1,073.37 ns |     838.02 ns |   297,421.1 ns |  0.93 |    0.01 |    120.6055 |      3.4180 |           - |           556.41 KB |
    AddOrUpdate |  1000 |   319,478.3 ns |   2,768.11 ns |   2,161.16 ns |   319,079.8 ns |  1.00 |    0.00 |    113.2813 |      0.9766 |           - |           526.48 KB |
 AddOrUpdate_v2 |  1000 |   615,321.8 ns |  72,410.43 ns | 213,503.80 ns |   467,207.0 ns |  1.97 |    0.66 |    118.6523 |      0.4883 |           - |            547.3 KB |
  ImmutableDict |  1000 | 1,516,613.7 ns | 107,376.35 ns | 290,298.20 ns | 1,387,325.2 ns |  4.95 |    1.19 |    140.6250 |      1.9531 |           - |           648.02 KB |
            */

            [Params(100)]
            public int Count;

            [Benchmark(Baseline = true)]
            public ImHashMap<Type, string> AddOrUpdate()
            {
                var map = ImHashMap<Type, string>.Empty;

                foreach (var key in _keys.Take(Count))
                    map = map.AddOrUpdate(key, "a", out _, out _);

                return map.AddOrUpdate(typeof(ImHashMapBenchmarks), "!", out _, out _);
            }

            [Benchmark]
            public V1.ImHashMap<Type, string> AddOrUpdate_v1()
            {
                var map = V1.ImHashMap<Type, string>.Empty;

                foreach (var key in _keys.Take(Count))
                    map = map.AddOrUpdate(key, "a");

                return map.AddOrUpdate(typeof(ImHashMapBenchmarks), "!");
            }

            [Benchmark]
            public V2.ImHashMap<Type, string> AddOrUpdate_v2()
            {
                var map = V2.ImHashMap<Type, string>.Empty;

                foreach (var key in _keys.Take(Count))
                    map = map.AddOrUpdate(key, "a", out _, out _, (type, _, v) => v);

                return map.AddOrUpdate(typeof(ImHashMapBenchmarks), "!", out _, out _, (type, _, v) => v);
            }

            //[Benchmark]
            public ImmutableDictionary<Type, string> ImmutableDict()
            {
                var map = ImmutableDictionary<Type, string>.Empty;

                foreach (var key in _keys.Take(Count))
                    map = map.Add(key, "a");

                return map.Add(typeof(ImHashMapBenchmarks), "!");
            }

            //[Benchmark]
            public ConcurrentDictionary<Type, string> ConcurrentDict()
            {
                var map = new ConcurrentDictionary<Type, string>();

                foreach (var key in _keys.Take(Count))
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

            public static V2.ImHashMap<Type, string> AddOrUpdate_v2()
            {
                var map = V2.ImHashMap<Type, string>.Empty;

                foreach (var key in _keys)
                    map = map.AddOrUpdate(key, "a");

                map = map.AddOrUpdate(typeof(ImHashMapBenchmarks), "!");

                return map;
            }

            private static readonly V2.ImHashMap<Type, string> _mapV2 = AddOrUpdate_v2();

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

            //[Benchmark(Baseline = true)]
            public string TryFind()
            {
                _map.TryFind<Type, string>(LookupKey, out var result);
                return result;
            }

            //[Benchmark]
            public string TryFindByRef()
            {
                _map.TryFind(LookupKey, out var result);
                return result;
            }

            //[Benchmark]
            public string TryFind_v1()
            {
                _mapV1.TryFind(LookupKey, out var result);
                return result;
            }

            //[Benchmark]
            public string TryFind_v2()
            {
                _mapV2.TryFind(LookupKey, out var result);
                return result;
            }

            [Benchmark(Baseline = true)]
            public string GetValueOrDefault() => _map.GetValueOrDefault<Type, string>(LookupKey);

            [Benchmark]
            public string GetValueOrDefault_ByType() => _map.GetValueOrDefault(LookupKey);

            [Benchmark]
            public string GetValueOrDefault_v1() => _mapV1.GetValueOrDefault(LookupKey);

            //[Benchmark]
            public string GetValueOrDefault_v2() => _mapV2.GetValueOrDefault(LookupKey);

            //[Benchmark]
            public string ConcurrentDict_TryGet()
            {
                _dict.TryGetValue(LookupKey, out var result);
                return result;
            }
        }
    }
}

namespace V2
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

namespace V1
{
    using System.Runtime.CompilerServices; // For [MethodImpl(AggressiveInlining)]

    /// <summary>Immutable http://en.wikipedia.org/wiki/AVL_tree 
    /// where node key is the hash code of <typeparamref name="K"/>.</summary>
    public sealed class ImHashMap<K, V>
    {
        /// <summary>Empty tree to start with.</summary>
        public static readonly ImHashMap<K, V> Empty = new ImHashMap<K, V>();

        /// <summary>Calculated key hash.</summary>
        public int Hash => _data.Hash;

        /// <summary>Key of type K that should support <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode"/>.</summary>
        public K Key => _data.Key;

        /// <summary>Value of any type V.</summary>
        public V Value => _data.Value;

        /// <summary>In case of <see cref="Hash"/> conflicts for different keys contains conflicted keys with their values.</summary>
        public KV<K, V>[] Conflicts => _data.Conflicts;

        /// <summary>Left sub-tree/branch, or empty.</summary>
        public readonly ImHashMap<K, V> Left;

        /// <summary>Right sub-tree/branch, or empty.</summary>
        public readonly ImHashMap<K, V> Right;

        /// <summary>Height of longest sub-tree/branch plus 1. It is 0 for empty tree, and 1 for single node tree.</summary>
        public readonly int Height;

        /// <summary>Returns true if tree is empty.</summary>
        public bool IsEmpty => Height == 0;

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
            while (t.Height != 0 && t._data.Hash != hash)
                t = hash < t._data.Hash ? t.Left : t.Right;

            if (t.Height != 0 && (ReferenceEquals(key, t._data.Key) || key.Equals(t._data.Key)))
            {
                value = t._data.Value;
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

                    if (node.Conflicts != null)
                        for (var i = 0; i < node.Conflicts.Length; i++)
                            yield return node.Conflicts[i];

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

        private sealed class Data
        {
            public readonly int Hash;
            public readonly K Key;
            public readonly V Value;

            public readonly KV<K, V>[] Conflicts;

            public Data() { }

            public Data(int hash, K key, V value, KV<K, V>[] conflicts = null)
            {
                Hash = hash;
                Key = key;
                Value = value;
                Conflicts = conflicts;
            }
        }

        private readonly Data _data;

        private ImHashMap() { _data = new Data(); }

        private ImHashMap(Data data)
        {
            _data = data;
            Left = Empty;
            Right = Empty;
            Height = 1;
        }

        private ImHashMap(Data data, ImHashMap<K, V> left, ImHashMap<K, V> right)
        {
            _data = data;
            Left = left;
            Right = right;
            Height = 1 + (left.Height > right.Height ? left.Height : right.Height);
        }

        private ImHashMap(Data data, ImHashMap<K, V> left, ImHashMap<K, V> right, int height)
        {
            _data = data;
            Left = left;
            Right = right;
            Height = height;
        }

        // todo: made public for benchmarking
        /// <summary>It is fine</summary>
        public ImHashMap<K, V> AddOrUpdate(int hash, K key, V value)
        {
            return Height == 0  // add new node
                ? new ImHashMap<K, V>(new Data(hash, key, value))
                : (hash == Hash // update found node
                    ? (ReferenceEquals(Key, key) || Key.Equals(key)
                        ? new ImHashMap<K, V>(new Data(hash, key, value, Conflicts), Left, Right)
                        : UpdateValueAndResolveConflicts(key, value, null, false))
                    : (hash < Hash  // search for node
                        ? (Height == 1
                            ? new ImHashMap<K, V>(_data,
                                new ImHashMap<K, V>(new Data(hash, key, value)), Right, height: 2)
                            : new ImHashMap<K, V>(_data,
                                Left.AddOrUpdate(hash, key, value), Right).KeepBalance())
                        : (Height == 1
                            ? new ImHashMap<K, V>(_data,
                                Left, new ImHashMap<K, V>(new Data(hash, key, value)), height: 2)
                            : new ImHashMap<K, V>(_data,
                                Left, Right.AddOrUpdate(hash, key, value)).KeepBalance())));
        }

        private ImHashMap<K, V> AddOrUpdate(int hash, K key, V value, Update<V> update)
        {
            return Height == 0
                ? new ImHashMap<K, V>(new Data(hash, key, value))
                : (hash == Hash // update
                    ? (ReferenceEquals(Key, key) || Key.Equals(key)
                        ? new ImHashMap<K, V>(new Data(hash, key, update(Value, value), Conflicts), Left, Right)
                        : UpdateValueAndResolveConflicts(key, value, update, false))
                    : (hash < Hash
                        ? With(Left.AddOrUpdate(hash, key, value, update), Right)
                        : With(Left, Right.AddOrUpdate(hash, key, value, update)))
                    .KeepBalance());
        }

        // todo: made public for benchmarking
        /// <summary>It is fine</summary>
        public ImHashMap<K, V> Update(int hash, K key, V value, Update<V> update)
        {
            return Height == 0 ? this
                : (hash == Hash
                    ? (ReferenceEquals(Key, key) || Key.Equals(key)
                        ? new ImHashMap<K, V>(new Data(hash, key, update == null ? value : update(Value, value), Conflicts), Left, Right)
                        : UpdateValueAndResolveConflicts(key, value, update, true))
                    : (hash < Hash
                        ? With(Left.Update(hash, key, value, update), Right)
                        : With(Left, Right.Update(hash, key, value, update)))
                    .KeepBalance());
        }

        private ImHashMap<K, V> UpdateValueAndResolveConflicts(K key, V value, Update<V> update, bool updateOnly)
        {
            if (Conflicts == null) // add only if updateOnly is false.
                return updateOnly ? this
                    : new ImHashMap<K, V>(new Data(Hash, Key, Value, new[] { new KV<K, V>(key, value) }), Left, Right);

            var found = Conflicts.Length - 1;
            while (found >= 0 && !Equals(Conflicts[found].Key, Key)) --found;
            if (found == -1)
            {
                if (updateOnly) return this;
                var newConflicts = new KV<K, V>[Conflicts.Length + 1];
                Array.Copy(Conflicts, 0, newConflicts, 0, Conflicts.Length);
                newConflicts[Conflicts.Length] = new KV<K, V>(key, value);
                return new ImHashMap<K, V>(new Data(Hash, Key, Value, newConflicts), Left, Right);
            }

            var conflicts = new KV<K, V>[Conflicts.Length];
            Array.Copy(Conflicts, 0, conflicts, 0, Conflicts.Length);
            conflicts[found] = new KV<K, V>(key, update == null ? value : update(Conflicts[found].Value, value));
            return new ImHashMap<K, V>(new Data(Hash, Key, Value, conflicts), Left, Right);
        }

        // todo: temporary made public for benchmarking
        /// <summary>It is fine</summary>
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

        private ImHashMap<K, V> KeepBalance()
        {
            var delta = Left.Height - Right.Height;
            if (delta >= 2) // left is longer by 2, rotate left
            {
                var left = Left;
                var leftLeft = left.Left;
                var leftRight = left.Right;
                if (leftRight.Height - leftLeft.Height == 1)
                {
                    // double rotation:
                    //      5     =>     5     =>     4
                    //   2     6      4     6      2     5
                    // 1   4        2   3        1   3     6
                    //    3        1
                    return new ImHashMap<K, V>(leftRight._data,
                        left: new ImHashMap<K, V>(left._data,
                            left: leftLeft, right: leftRight.Left), right: new ImHashMap<K, V>(_data,
                            left: leftRight.Right, right: Right));
                }

                // todo: do we need this?
                // one rotation:
                //      5     =>     2
                //   2     6      1     5
                // 1   4              4   6
                return new ImHashMap<K, V>(left._data,
                    left: leftLeft, right: new ImHashMap<K, V>(_data,
                        left: leftRight, right: Right));
            }

            if (delta <= -2)
            {
                var right = Right;
                var rightLeft = right.Left;
                var rightRight = right.Right;
                if (rightLeft.Height - rightRight.Height == 1)
                {
                    return new ImHashMap<K, V>(rightLeft._data,
                        left: new ImHashMap<K, V>(_data,
                            left: Left, right: rightLeft.Left), right: new ImHashMap<K, V>(right._data,
                            left: rightLeft.Right, right: rightRight));
                }

                return new ImHashMap<K, V>(right._data,
                    left: new ImHashMap<K, V>(_data,
                        left: Left, right: rightLeft), right: rightRight);
            }

            return this;
        }

        private ImHashMap<K, V> With(ImHashMap<K, V> left, ImHashMap<K, V> right) =>
            left == Left && right == Right ? this : new ImHashMap<K, V>(_data, left, right);

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
                        result = new ImHashMap<K, V>(successor._data,
                            Left, Right.Remove(successor.Hash, default(K), ignoreKey: true));
                    }
                }
                else if (Conflicts != null)
                    return TryRemoveConflicted(key);
                else
                    return this; // if key is not matching and no conflicts to lookup - just return
            }
            else if (hash < Hash)
                result = new ImHashMap<K, V>(_data, Left.Remove(hash, key, ignoreKey), Right);
            else
                result = new ImHashMap<K, V>(_data, Left, Right.Remove(hash, key, ignoreKey));

            if (result.Height == 1)
                return result;

            return result.KeepBalance();
        }

        private ImHashMap<K, V> TryRemoveConflicted(K key)
        {
            var index = Conflicts.Length - 1;
            while (index >= 0 && !Equals(Conflicts[index].Key, key)) --index;
            if (index == -1) // key is not found in conflicts - just return
                return this;

            if (Conflicts.Length == 1)
                return new ImHashMap<K, V>(new Data(Hash, Key, Value), Left, Right);
            var shrinkedConflicts = new KV<K, V>[Conflicts.Length - 1];
            var newIndex = 0;
            for (var i = 0; i < Conflicts.Length; ++i)
                if (i != index) shrinkedConflicts[newIndex++] = Conflicts[i];
            return new ImHashMap<K, V>(new Data(Hash, Key, Value, shrinkedConflicts), Left, Right);
        }

        private ImHashMap<K, V> ReplaceRemovedWithConflicted()
        {
            if (Conflicts.Length == 1)
                return new ImHashMap<K, V>(new Data(Hash, Conflicts[0].Key, Conflicts[0].Value), Left, Right);
            var shrinkedConflicts = new KV<K, V>[Conflicts.Length - 1];
            Array.Copy(Conflicts, 1, shrinkedConflicts, 0, shrinkedConflicts.Length);
            return new ImHashMap<K, V>(new Data(Hash, Conflicts[0].Key, Conflicts[0].Value, shrinkedConflicts), Left, Right);
        }

        #endregion
    }
}
