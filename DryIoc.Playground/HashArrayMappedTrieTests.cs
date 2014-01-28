using System;
using NUnit.Framework;

namespace DryIoc.Playground
{
    [TestFixture]
    //[Ignore]
    public class HashArrayMappedTrieTests
    {
        [Test]
        public void Test_how_int_hash_code_is_working_in_edge_cases()
        {
            // Hash code for negative is still negative and equal to key
            var x = -7;
            Assert.AreEqual(-7, x.GetHashCode());

            // Conversation of negative to UINT does not throw.
            var ux = (uint)x;
            var uux = unchecked((uint)x);
        }

        [Test]
        public void Create_trie_and_add_value_to_it()
        {
            var trie = Hamt<string>.Empty;
            trie = trie.AddOrUpdate(705, "a");
            trie = trie.AddOrUpdate(706, "b");
            trie = trie.AddOrUpdate(750, "c");
            trie = trie.AddOrUpdate(705, "A");
            trie = trie.AddOrUpdate(0, "0x");
            trie = trie.AddOrUpdate(5, "5x");
            trie = trie.AddOrUpdate(555555555, "55x");
            trie = trie.AddOrUpdate(750, "C");
            
            Assert.AreEqual("C", trie.GetValueOrDefault(750));
        }
    }

    public sealed class Hamt<V>
    {
        public static Hamt<V> Empty = new Hamt<V>();

        private readonly HamtNode<V> _root;

        public Hamt(HamtNode<V> root = null)
        {
            _root = root ?? HamtNode<V>.Empty;
        }

        public V GetValueOrDefault(int key, V defaultValue = default (V))
        {
            var node = _root;
            var hash = (uint)key;
            while (hash != 0)
            {


                hash >>= 5;
            }

            return defaultValue;
        }

        public Hamt<V> AddOrUpdate(int key, V value)
        {
            return new Hamt<V>(_root.AddOrUpdate((uint)key, value));
        }
    }

    public class HamtNode<V>
    {
        public static readonly HamtNode<V> Empty = new HamtNode<V>();

        public readonly object[] Nodes;   // up to 32 nodes: sub nodes or values.
        public readonly uint IndexBitmap; // bits indicating what nodes are in use.


        public HamtNode(object[] nodes, uint indexBitmap)
        {
            Nodes = nodes;
            IndexBitmap = indexBitmap;
        }

        public HamtNode<V> AddOrUpdate(uint hash, V value)
        {
            var index = (int)(hash & LEVEL_MASK); // index from 0 to 31

            // get value or node
            var restOfHash = hash >> 5;
            var valueOrNode = restOfHash == 0 ? (object)value : Empty.AddOrUpdate(restOfHash, value);

            // insert or update node
            var indexBit = (uint)1 << index;
            if (Nodes == null)
                return new HamtNode<V>(new[] { valueOrNode }, indexBit);

            // find real index where to insert into new nodes
            var pastIndexBitmap = IndexBitmap >> index;
            var pastIndexNodeCount = GetSetBitsCount(pastIndexBitmap);
            var realIndex = Nodes.Length - pastIndexNodeCount;

            // insert:
            if ((IndexBitmap & indexBit) == 0)
            {
                // otherwise copy old nodes with extra room for new node
                var nodesToInsert = new object[Nodes.Length + 1];

                // Copy up to index, set node to index, and copy past of index nodes.
                if (realIndex != 0)
                    Array.Copy(Nodes, 0, nodesToInsert, 0, realIndex);
                nodesToInsert[realIndex] = valueOrNode;
                if (realIndex != Nodes.Length)
                    Array.Copy(Nodes, realIndex, nodesToInsert, realIndex + 1, pastIndexNodeCount);

                return new HamtNode<V>(nodesToInsert, IndexBitmap | indexBit);
            }

            // update:
            // copy nodes and replace value at index
            var nodesToUpdate = new object[Nodes.Length];
            if (nodesToUpdate.Length > 1)
                Array.Copy(Nodes, 0, nodesToUpdate, 0, nodesToUpdate.Length);
            nodesToUpdate[realIndex] = valueOrNode;

            return new HamtNode<V>(nodesToUpdate, IndexBitmap);
        }

        private HamtNode() { }

        private const uint LEVEL_MASK = 0x1F; // 11111

        private static uint GetSetBitsCount(uint i)
        {
            i = i - ((i >> 1) & 0x55555555);
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
        }
    }

    public static class Bits
    {
        private const int BITS_NUMBER = 32;

        public static string PrintBits(this uint key)
        {
            var bits = new char[BITS_NUMBER];
            var test = key;
            for (var i = BITS_NUMBER - 1; i >= 0; i--)
            {
                bits[i] = (test & 1) == 1 ? '1' : '0';
                test >>= 1;
            }

            return new string(bits);
        }
    }
}
