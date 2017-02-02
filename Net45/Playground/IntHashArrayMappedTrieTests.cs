using System;
using System.Collections.Generic;
using ImTools;

namespace Playground
{
    public sealed class IntHashTrie<V>
    {
        public static readonly IntHashTrie<V> Empty = new IntHashTrie<V>();

        public V GetValueOrDefault(int hash, V defaultValue = default(V))
        {
            var index = hash & 31;
            if ((_indexBitmap >> index & 1) == 0)
                return defaultValue;

            var node = _nodes[index];
            return !(node is HashTrieNode<V>)
                ? (hash >> 5 == 0 ? (V)node : defaultValue)
                : ((HashTrieNode<V>)node).GetValueOrDefault(hash >> 5, defaultValue);
        }

        public IntHashTrie<V> AddOrUpdate(int hash, V value, Update<V> updateValue = null)
        {
            var index = hash & 31; // index from 0 to 31
            var restOfHash = hash >> 5;

            var newNodes = new object[32];
            if (_indexBitmap == 0)
            {
                newNodes[index] = restOfHash == 0 ? (object)value : HashTrieNode<V>.Empty.AddOrUpdate(restOfHash, value);
                return new IntHashTrie<V>(1u << index, newNodes);
            }

            Array.Copy(_nodes, 0, newNodes, 0, newNodes.Length);

            if ((_indexBitmap & (1u << index)) == 0) // no nodes at the index, could be inserted.
            {
                newNodes[index] = restOfHash == 0 ? (object)value : Empty.AddOrUpdate(restOfHash, value);
                return new IntHashTrie<V>(_indexBitmap | (1u << index), newNodes);
            }

            var updatedNode = _nodes[index];
            if (updatedNode is HashTrieNode<V>)
                updatedNode = ((HashTrieNode<V>)updatedNode).AddOrUpdate(restOfHash, value, updateValue);
            else if (restOfHash != 0) // if we need to update value with node we will move value down to new node sub-nodes at index 0. 
                updatedNode = new HashTrieNode<V>(1u, updatedNode).AddOrUpdate(restOfHash, value, updateValue);
            else // here the actual update should go, cause old and new nodes contain values.
                updatedNode = updateValue == null ? value : updateValue((V)updatedNode, value);
            newNodes[index] = updatedNode;
            return new IntHashTrie<V>(_indexBitmap, newNodes);
        }

        #region Implementation

        private IntHashTrie() { }

        private IntHashTrie(uint indexBitmap, params object[] nodes)
        {
            _indexBitmap = indexBitmap;
            _nodes = nodes;
        }

        private readonly object[] _nodes; // Up to 32 nodes: HashTrieNodes or values.
        private readonly uint _indexBitmap; // Bits indicating nodes at what index are in use.

        #endregion
    }

    /// <summary>
    /// Immutable Hash Array Mapped Trie (http://en.wikipedia.org/wiki/Hash_array_mapped_trie)
    /// similar to the one described at http://lampwww.epfl.ch/papers/idealhashtrees.pdf.
    /// It is basically a http://en.wikipedia.org/wiki/Trie built on hash chunks. It provides O(1) access-time and
    /// does not require self-balancing. The maximum number of tree levels would be (32 bits of hash / 5 bits level chunk = 7).
    /// In addition it is space efficient and requires single integer (to store index bitmap) per 1 to 32 values.
    /// TODO: ? Optimize get/add speed with mutable sparse array (for insert) at root level. That safe cause bitmapIndex will Not see new inserted values.
    /// </summary>
    /// <typeparam name="V">Type of value stored in trie.</typeparam>
    public sealed class HashTrieNode<V>
    {
        public static readonly HashTrieNode<V> Empty = new HashTrieNode<V>();

        public bool IsEmpty
        {
            get { return _indexBitmap == 0; }
        }

        public HashTrieNode<V> AddOrUpdate(int hash, V value, Update<V> updateValue = null)
        {
            var index = hash & LEVEL_MASK; // index from 0 to 31
            var restOfHash = hash >> LEVEL_BITS;
            if (_indexBitmap == 0)
                return new HashTrieNode<V>(1u << index,
                    restOfHash == 0 ? (object)value : Empty.AddOrUpdate(restOfHash, value));

            var pastIndexBitmap = _indexBitmap >> index;
            if ((pastIndexBitmap & 1) == 0) // no nodes at the index, could be inserted.
            {
                var subnode = restOfHash == 0 ? (object)value : Empty.AddOrUpdate(restOfHash, value);

                var pastIndexCount = pastIndexBitmap == 0 ? 0 : GetSetBitsCount(pastIndexBitmap);
                var insertIndex = _nodes.Length - pastIndexCount;

                var nodesToInsert = new object[_nodes.Length + 1];
                if (insertIndex != 0)
                    Array.Copy(_nodes, 0, nodesToInsert, 0, insertIndex);
                nodesToInsert[insertIndex] = subnode;
                if (pastIndexCount != 0)
                    Array.Copy(_nodes, insertIndex, nodesToInsert, insertIndex + 1, pastIndexCount);

                return new HashTrieNode<V>(_indexBitmap | (1u << index), nodesToInsert);
            }

            var updateIndex = _nodes.Length == 1 ? 0
                : _nodes.Length - (pastIndexBitmap == 1 ? 1 : GetSetBitsCount(pastIndexBitmap));

            var updatedNode = _nodes[updateIndex];
            if (updatedNode is HashTrieNode<V>)
                updatedNode = ((HashTrieNode<V>)updatedNode).AddOrUpdate(restOfHash, value, updateValue);
            else if (restOfHash != 0) // if we need to update value with node we will move value down to new node sub-nodes at index 0. 
                updatedNode = new HashTrieNode<V>(1u, updatedNode).AddOrUpdate(restOfHash, value, updateValue);
            else // here the actual update should go, cause old and new nodes contain values.
                updatedNode = updateValue == null ? value : updateValue((V)updatedNode, value);

            var nodesToUpdate = new object[_nodes.Length];
            if (nodesToUpdate.Length > 1)
                Array.Copy(_nodes, 0, nodesToUpdate, 0, nodesToUpdate.Length);
            nodesToUpdate[updateIndex] = updatedNode;

            return new HashTrieNode<V>(_indexBitmap, nodesToUpdate);
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
                if (!(subnode is HashTrieNode<V>)) // reached the leaf value node
                    return hash == 0 ? (V)subnode : defaultValue;

                node = (HashTrieNode<V>)subnode;
                pastIndexBitmap = node._indexBitmap >> (hash & LEVEL_MASK);
            }

            return defaultValue;
        }

        public V GetValueOrDefault2(int hash, V defaultValue = default(V))
        {
            var index = hash & LEVEL_MASK;
            var pastIndexBitmap = _indexBitmap >> index;
            if ((pastIndexBitmap & 1) == 0)
                return defaultValue;

            var nodeOrValue = _nodes[index];
            if (!(nodeOrValue is HashTrieNode<V>))
                return hash >> LEVEL_BITS == 0 ? (V)nodeOrValue : defaultValue;

            hash >>= LEVEL_BITS;
            var node = (HashTrieNode<V>)nodeOrValue;
            pastIndexBitmap = node._indexBitmap >> (hash & LEVEL_MASK);
            while ((pastIndexBitmap & 1) == 1)
            {
                var subnode = node._nodes[
                    node._nodes.Length - (pastIndexBitmap == 1 ? 1 : GetSetBitsCount(pastIndexBitmap))];

                hash >>= LEVEL_BITS;
                if (!(subnode is HashTrieNode<V>)) // reached the leaf value node
                    return hash == 0 ? (V)subnode : defaultValue;

                node = (HashTrieNode<V>)subnode;
                pastIndexBitmap = node._indexBitmap >> (hash & LEVEL_MASK);
            }

            return defaultValue;
        }

        public IEnumerable<V> Enumerate()
        {
            for (var i = 0; i < _nodes.Length; --i)
            {
                var n = _nodes[i];
                if (n is HashTrieNode<V>)
                    foreach (var subnode in ((HashTrieNode<V>)n).Enumerate())
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

        private HashTrieNode() { }

        internal HashTrieNode(uint indexBitmap, params object[] nodes)
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
}
