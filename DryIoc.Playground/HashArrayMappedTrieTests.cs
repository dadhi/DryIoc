using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace DryIoc.Playground
{
    [TestFixture]
    //[Ignore]
    public class HashArrayMappedTrieTests
    {
        [Test]
        public void Test_how_int_hash_code_is_working_at_edge_cases()
        {
            // Hash code for negative is still negative and equal to key
            Assert.AreEqual(-7, -7.GetHashCode());
        }

        [Test]
        public void Create_trie_and_add_value_to_it()
        {
            var trie = HashTrie<string>.Empty;
            trie = trie.AddOrUpdate(705, "a");
            trie = trie.AddOrUpdate(706, "b");
            trie = trie.AddOrUpdate(750, "c");
            trie = trie.AddOrUpdate(705, "A");
            trie = trie.AddOrUpdate(0, "0x");
            trie = trie.AddOrUpdate(5, "5x");
            trie = trie.AddOrUpdate(555555555, "55x");
            trie = trie.AddOrUpdate(750, "C");

            Assert.AreEqual(null, trie.GetValueOrDefault(13));
            Assert.AreEqual("C", trie.GetValueOrDefault(750));
            Assert.AreEqual("0x", trie.GetValueOrDefault(0));
            Assert.AreEqual("A", trie.GetValueOrDefault(705));
            Assert.AreEqual("55x", trie.GetValueOrDefault(555555555));
            Assert.AreEqual(null, trie.GetValueOrDefault(-1));
        }

        [Test]
        public void Store_value_with_0_hash_then_with_0_plus_some_hash()
        {
            var trie = HashTrie<string>.Empty;
            trie = trie.AddOrUpdate(0, "a");
            trie = trie.AddOrUpdate(64, "b");

            Assert.AreEqual("b", trie.GetValueOrDefault(64));
            Assert.AreEqual("a", trie.GetValueOrDefault(0));
        }

        [Test]
        public void Store_value_with_0_plus_some_hash_then_with_0_hash()
        {
            var trie = HashTrie<string>.Empty;
            trie = trie.AddOrUpdate(64, "a");
            trie = trie.AddOrUpdate(0, "b");

            Assert.AreEqual("a", trie.GetValueOrDefault(64));
            Assert.AreEqual("b", trie.GetValueOrDefault(0));
        }

        [Test]
        public void Update_values_with_0_hash()
        {
            var trie = HashTrie<string>.Empty;
            trie = trie.AddOrUpdate(0, "a");
            trie = trie.AddOrUpdate(0, "b");

            Assert.AreEqual("b", trie.GetValueOrDefault(0));
        }
    }

    public delegate T UpdateMethod<T>(T old, T newOne);

    /// <summary>
    /// Immutable Hash Array Mapped Trie (http://en.wikipedia.org/wiki/Hash_array_mapped_trie)
    /// similar to the one described at http://lampwww.epfl.ch/papers/idealhashtrees.pdf.
    /// It is basically a http://en.wikipedia.org/wiki/Trie built on hash chunks. It provides O(1) access-time and
    /// does not require self-balancing. The maximum number of tree levels would be (32 bits of hash / 5 bits level chunk = 7).
    /// In addition it is space efficient and requires single integer (to store index bitmap) per 1 to 32 values.
    /// TODO: ? Optimize get/add speed with mutable sparse array (for insert) at root level. That safe cause bitmapIndex will Not see new inserted values.
    /// </summary>
    /// <typeparam name="V">Type of value stored in trie.</typeparam>
    public sealed class HashTrie<V>
    {
        public static readonly HashTrie<V> Empty = new HashTrie<V>();

        public bool IsEmpty
        {
            get { return _indexBitmap == 0; }
        }

        public V GetValueOrDefault(int hash, V defaultValue = default(V))
        {
            var node = this;
            var pastIndexBitmap = node._indexBitmap >> (hash & LEVEL_MASK);
            while ((pastIndexBitmap & 1) == 1)
            {
                var subnode = node._nodes[
                    node._nodes.Length - (pastIndexBitmap == 1 ? 1 : GetSetBitsCount(pastIndexBitmap))];

                hash >>= LEVEL_BITS;
                if (!(subnode is HashTrie<V>)) // is leaf value node
                    return hash == 0 ? (V)subnode : defaultValue;

                node = (HashTrie<V>)subnode;
                pastIndexBitmap = node._indexBitmap >> (hash & LEVEL_MASK);
            }

            return defaultValue;
        }

        public HashTrie<V> AddOrUpdate(int hash, V value, UpdateMethod<V> updateValue = null)
        {
            var index = hash & LEVEL_MASK; // index from 0 to 31
            var restOfHash = hash >> LEVEL_BITS;
            if (_indexBitmap == 0)
                return new HashTrie<V>(1u << index, restOfHash == 0 ? (object)value : Empty.AddOrUpdate(restOfHash, value));

            var nodeCount = _nodes.Length;

            var pastIndexBitmap = _indexBitmap >> index;
            if ((pastIndexBitmap & 1) == 0) // no nodes at the index, could be inserted.
            {
                var subnode = restOfHash == 0 ? (object)value : Empty.AddOrUpdate(restOfHash, value);

                var pastIndexCount = pastIndexBitmap == 0 ? 0 : GetSetBitsCount(pastIndexBitmap);
                var insertIndex = nodeCount - pastIndexCount;

                var nodesToInsert = new object[nodeCount + 1];
                if (insertIndex != 0)
                    Array.Copy(_nodes, 0, nodesToInsert, 0, insertIndex);
                nodesToInsert[insertIndex] = subnode;
                if (pastIndexCount != 0)
                    Array.Copy(_nodes, insertIndex, nodesToInsert, insertIndex + 1, pastIndexCount);

                return new HashTrie<V>(_indexBitmap | (1u << index), nodesToInsert);
            }

            var updateIndex = nodeCount == 1 ? 0
                : nodeCount - (pastIndexBitmap == 1 ? 1 : GetSetBitsCount(pastIndexBitmap));

            var updatedNode = _nodes[updateIndex];
            if (updatedNode is HashTrie<V>)
                updatedNode = ((HashTrie<V>)updatedNode).AddOrUpdate(restOfHash, value, updateValue);
            else                     // if node to update is some value
                if (restOfHash != 0) // if we need to update value with node we will move value down to new node sub-nodes at index 0. 
                    updatedNode = new HashTrie<V>(1u, updatedNode).AddOrUpdate(restOfHash, value, updateValue);
                else // here the actual update should go, cause old and new nodes contain values.
                    updatedNode = updateValue == null ? value : updateValue((V)updatedNode, value);

            var nodesToUpdate = new object[nodeCount];
            if (nodesToUpdate.Length > 1)
                Array.Copy(_nodes, 0, nodesToUpdate, 0, nodesToUpdate.Length);
            nodesToUpdate[updateIndex] = updatedNode;

            return new HashTrie<V>(_indexBitmap, nodesToUpdate);
        }

        public IEnumerable<V> TraverseInKeyOrder()
        {
            for (var i = 0; i < _nodes.Length; --i)
            {
                var n = _nodes[i];
                if (n is HashTrie<V>)
                    foreach (var subnode in ((HashTrie<V>)n).TraverseInKeyOrder())
                        yield return subnode;
                else
                    yield return (V)n;
            }
        }

        #region Implementation

        private const int LEVEL_MASK = 31;  // Hash mask to find hash part on each trie level.
        private const int LEVEL_BITS = 5;   // Number of bits from hash corresponding to one level.

        private readonly object[] _nodes;   // Up to 32 nodes: sub nodes or values.
        private readonly uint _indexBitmap; // Bits indicating nodes at what index are in use.

        private HashTrie() { }

        private HashTrie(uint indexBitmap, params object[] nodes)
        {
            _nodes = nodes;
            _indexBitmap = indexBitmap;
        }

        // Variable-precision SWAR algorithm http://playingwithpointers.com/swar.html
        // Fastest compared to the rest (but did not check pre-computed WORD counts): http://gurmeet.net/puzzles/fast-bit-counting-routines/
        private static uint GetSetBitsCount(uint n)
        {
            n = n - ((n >> 1) & 0x55555555);
            n = (n & 0x33333333) + ((n >> 2) & 0x33333333);
            return (((n + (n >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
        }

        #endregion
    }

    public sealed class HashTrie<K, V>
    {
        public static readonly HashTrie<K, V> Empty = new HashTrie<K, V>(HashTrie<DryIoc.KV<K, V>>.Empty, null);

        public HashTrie<K, V> AddOrUpdate(K key, V value)
        {
            return new HashTrie<K, V>(
                _root.AddOrUpdate(key.GetHashCode(), new DryIoc.KV<K, V>(key, value), UpdateConflicts),
                _updateValue);
        }

        private readonly HashTrie<DryIoc.KV<K, V>> _root;
        private readonly Func<V, V, V> _updateValue;

        private HashTrie(HashTrie<DryIoc.KV<K, V>> root, Func<V, V, V> updateValue)
        {
            _root = root;
            _updateValue = updateValue;
        }

        private DryIoc.KV<K, V> UpdateConflicts(DryIoc.KV<K, V> old, DryIoc.KV<K, V> added)
        {
            var conflicts = old is KVWithConflicts ? ((KVWithConflicts)old).Conflicts : null;
            if (ReferenceEquals(old.Key, added.Key) || old.Key.Equals(added.Key))
                return conflicts == null ? UpdateValue(old, added)
                     : new KVWithConflicts(UpdateValue(old, added), conflicts);

            if (conflicts == null)
                return new KVWithConflicts(old, new[] { added });

            var i = conflicts.Length - 1;
            while (i >= 0 && !Equals(conflicts[i].Key, added.Key)) --i;
            if (i != -1) added = UpdateValue(old, added);
            return new KVWithConflicts(old, conflicts.AppendOrUpdate(added, i));
        }

        private DryIoc.KV<K, V> UpdateValue(DryIoc.KV<K, V> existing, DryIoc.KV<K, V> added)
        {
            return _updateValue == null ? added
                : new DryIoc.KV<K, V>(existing.Key, _updateValue(existing.Value, added.Value));
        }

        private sealed class KVWithConflicts : DryIoc.KV<K, V>
        {
            public readonly DryIoc.KV<K, V>[] Conflicts;

            public KVWithConflicts(DryIoc.KV<K, V> kv, DryIoc.KV<K, V>[] conflicts)
                : base(kv.Key, kv.Value)
            {
                Conflicts = conflicts;
            }
        }
    }
}
