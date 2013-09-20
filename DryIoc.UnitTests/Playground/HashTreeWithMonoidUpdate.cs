using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.UnitTests.Playground
{
    [TestFixture]
    public class HashTreeOfArrayTests
    {
        [Test]
        public void Test()
        {
            var tree = HashTreeOfCompositeValue<int, string[]>.Create(ComposeArrays)
                .AddOrUpdate(0, new[] {"a"})
                .AddOrUpdate(0, new[] {"a"})
                .AddOrUpdate(0, new[] {"a"})
                .AddOrUpdate(0, new[] {"a"});

        }

        public static T[] ComposeArrays<T>(T[] old, T[] added)
        {
            var result = new T[old.Length + added.Length];
            Array.Copy(old, 0, result, 0, old.Length);
            if (added.Length == 1) // usual case.
                result[old.Length] = added[0];
            else
                Array.Copy(added, 0, result, old.Length, added.Length);
            return result;
        }
    }

    public class HashTreeOfCompositeValue<K, V> : IEnumerable<HashTreeOfCompositeValue<K, V>>
    {
        public static HashTreeOfCompositeValue<K, V> Create(Func<V, V, V> composeValue = null)
        {
            return composeValue == null ? _emptyDefault
                : new HashTreeOfCompositeValue<K, V>(HashTree<KV>.Empty, composeValue);
        }

        public HashTreeOfCompositeValue<K, V> AddOrUpdate(K key, V value)
        {
            return new HashTreeOfCompositeValue<K, V>(
                _tree.AddOrUpdate(key.GetHashCode(), new KV { Key = key, Value = value }, Update),
                _composeValue);
        }

        public V TryGet(K key)
        {
            var item = _tree.TryGet(key.GetHashCode());
            return item != null && (ReferenceEquals(key, item.Key) || key.Equals(item.Key)) 
                ? item.Value : TryGetConflicted(item, key);
        }

        public IEnumerator<HashTreeOfCompositeValue<K, V>> GetEnumerator()
        {
            foreach (var node in _tree)
                yield return new HashTreeOfCompositeValue<K, V>(node, _composeValue);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static readonly HashTreeOfCompositeValue<K, V> _emptyDefault = new HashTreeOfCompositeValue<K, V>(HashTree<KV>.Empty);

        private readonly HashTree<KV> _tree;

        private readonly Func<V, V, V> _composeValue;

        private HashTreeOfCompositeValue(HashTree<KV> tree, Func<V, V, V> composeValue = null)
        {
            _tree = tree;
            _composeValue = composeValue;
        }

        private KV Compose(KV old, KV added)
        {
            return _composeValue == null ? added
                : new KV { Key = old.Key, Value = _composeValue(old.Value, added.Value) };
        }

        private KV Update(KV old, KV added)
        {
            var conflicts = old is KVWithConflicts ? ((KVWithConflicts)old).Conflicts : null;

            if (ReferenceEquals(old.Key, added.Key) || old.Key.Equals(added.Key))
            {
                added = Compose(old, added);
                return conflicts == null ? added
                    : new KVWithConflicts { Key = added.Key, Value = added.Value, Conflicts = conflicts };
            }

            KV[] newConflicts;
            if (conflicts == null)
                newConflicts = new[] { added };
            else
            {
                var i = conflicts.Length - 1;
                while (i >= 0 && !Equals(conflicts[i].Key, added.Key)) --i;
                if (i != -1) added = Compose(conflicts[i], added);
                newConflicts = conflicts.AddOrUpdateCopy(added, i);
            }

            return new KVWithConflicts { Key = old.Key, Value = old.Value, Conflicts = newConflicts };
        }

        private static V TryGetConflicted(KV item, K key)
        {
            var conflicts = item is KVWithConflicts ? ((KVWithConflicts)item).Conflicts : null;
            if (conflicts != null)
                for (var i = 0; i < conflicts.Length; i++)
                    if (Equals(conflicts[i].Key, key))
                        return conflicts[i].Value;
            return default(V);
        }

        private class KV
        {
            public K Key;
            public V Value;
        }

        private sealed class KVWithConflicts : KV
        {
            public KV[] Conflicts;
        }
    }
}
