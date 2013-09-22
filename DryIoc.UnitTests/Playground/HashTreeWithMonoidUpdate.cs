using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace DryIoc.UnitTests.Playground
{
    [TestFixture]
    public class HashTreeOfArrayTests
    {
        [Test]
        public void Can_combine_arrays_into_one_AND_get_it_by_key()
        {
            var tree = new HashTreeX<int, string[]>(ConcatArrays);
            var list = new List<string[]>(100);
            for (var i = 0; i < 100; i++)
            {
                tree.Add(i, new[] { i.ToString() });
                list.Add(new[] { i.ToString() });
            }

            Assert.That(tree.Select(kv => kv.Value), Is.EqualTo(list));
        }

        [Test]
        public void Can_concat_arrays()
        {
            var tree = new HashTreeX<int, string[]>(ConcatArrays)
            {
                {0, new[] {"a"}},
                {0, new[] {"b"}},
                {0, new[] {"c"}},
            };

            Assert.That(tree.TryGet(0), Is.EqualTo(new[] { "a", "b", "c" }));
        }

        private static V[] ConcatArrays<V>(V[] old, V[] added)
        {
            var result = new V[old.Length + added.Length];
            Array.Copy(old, 0, result, 0, old.Length);
            if (added.Length == 1) // usual case.
                result[old.Length] = added[0];
            else
                Array.Copy(added, 0, result, old.Length, added.Length);
            return result;
        }
    }

    public sealed class HashTreeX<K, V> : IEnumerable<KV<K, V>>
    {
        public HashTreeX(Func<V, V, V> updateValue = null) : this(HashTree<KV<K, V>>.Empty, updateValue) { }

        public HashTreeX(HashTree<KV<K, V>> tree, Func<V, V, V> updateValue = null)
        {
            _tree = tree;
            _updateValue = updateValue;
        }

        public void Add(K key, V value)
        {
            Interlocked.Exchange(ref _tree, _tree.AddOrUpdate(key.GetHashCode(), new KV<K, V>(key, value), Update));
        }

        public V TryGet(K key)
        {
            var item = _tree.TryGet(key.GetHashCode());
            return item != null && (ReferenceEquals(key, item.Key) || key.Equals(item.Key)) ? item.Value : TryGetConflicted(item, key);
        }

        public IEnumerator<KV<K, V>> GetEnumerator()
        {
            foreach (var node in _tree)
            {
                yield return node.Value;
                if (node.Value is KVWithConflicts)
                {
                    var conflicts = ((KVWithConflicts)node.Value).Conflicts;
                    for (var i = 0; i < conflicts.Length; i++)
                        yield return conflicts[i];
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region Implementation

        private volatile HashTree<KV<K, V>> _tree;
        private readonly Func<V, V, V> _updateValue;

        private KV<K, V> Update(KV<K, V> old, KV<K, V> added)
        {
            if (!(old is KVWithConflicts))
                return ReferenceEquals(old.Key, added.Key) || old.Key.Equals(added.Key)
                    ? UpdateValue(old, added) : new KVWithConflicts(old, new[] { added });

            var conflicts = ((KVWithConflicts)old).Conflicts;
            if (ReferenceEquals(old.Key, added.Key) || old.Key.Equals(added.Key))
                return new KVWithConflicts(UpdateValue(old, added), conflicts);

            var i = conflicts.Length - 1;
            while (i >= 0 && !Equals(conflicts[i].Key, added.Key)) --i;
            if (i != -1) added = UpdateValue(old, added);
            return new KVWithConflicts(old, conflicts.AddOrUpdateCopy(added, i));
        }

        private KV<K, V> UpdateValue(KV<K, V> old, KV<K, V> added)
        {
            return _updateValue == null ? added : new KV<K, V>(old.Key, _updateValue(old.Value, added.Value));
        }

        private static V TryGetConflicted(KV<K, V> item, K key)
        {
            var conflicts = item is KVWithConflicts ? ((KVWithConflicts)item).Conflicts : null;
            if (conflicts != null)
                for (var i = 0; i < conflicts.Length; i++)
                    if (Equals(conflicts[i].Key, key))
                        return conflicts[i].Value;
            return default(V);
        }

        private sealed class KVWithConflicts : KV<K, V>
        {
            public readonly KV<K, V>[] Conflicts;

            public KVWithConflicts(KV<K, V> kv, KV<K, V>[] conflicts)
                : base(kv.Key, kv.Value)
            {
                Conflicts = conflicts;
            }
        }

        #endregion
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
}
