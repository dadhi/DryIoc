using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ImTools;

namespace Playground
{
    public static class TreeBenchmarks
    {
        public static void GetDictVsIntHashTrie(int itemCount)
        {
            var key = itemCount;
            var value = "hey";

            var keys = Enumerable.Range(0, itemCount).ToArray();

            var dict = new Dictionary<int, string>();
            var trie = HashTrie<string>.Empty;

            var dictAddTime = DictAdd(dict, keys, key, value);
            var treeAddTime = HashTrieAdd(ref trie, keys, key, value);

            Console.WriteLine("Adding {0} items (ms):", itemCount);
            Console.WriteLine("Dict - " + dictAddTime);
            Console.WriteLine("Trie - " + treeAddTime);
            Console.WriteLine();

            var getTimes = 1 * 1000 * 1000;

            var dictGetTime = DictGet(dict, key, getTimes);
            var treeGetTime = HashTrieGet(trie, key, getTimes);

            Console.WriteLine("Getting one out of {0} items {1:N0} times (ms):", itemCount, getTimes);
            Console.WriteLine("Dict - " + dictGetTime);
            Console.WriteLine("Trie - " + treeGetTime);
        }

        public static void GetDictVsHashTrie2OfInt(int itemCount)
        {
            var key = itemCount;
            var value = "hey";

            var keys = Enumerable.Range(0, itemCount).ToArray();

            var dict = new Dictionary<int, string>();
            var trie = IntHashTrie<string>.Empty;

            var dictAddTime = DictAdd(dict, keys, key, value);
            var treeAddTime = IntHashTrieAdd(ref trie, keys, key, value);

            Console.WriteLine("Adding {0} items (ms):", itemCount);
            Console.WriteLine("Dict - " + dictAddTime);
            Console.WriteLine("Trie - " + treeAddTime);
            Console.WriteLine();

            var getTimes = 1 * 1000 * 1000;

            var dictGetTime = DictGet(dict, key, getTimes);
            var treeGetTime = IntHashTrieGet(trie, key, getTimes);

            Console.WriteLine("Getting one out of {0} items {1:N0} times (ms):", itemCount, getTimes);
            Console.WriteLine("Dict - " + dictGetTime);
            Console.WriteLine("Trie - " + treeGetTime);
        }

        public static void GetHashTreeVsHashTrie2OfInt(int itemCount)
        {
            var key = itemCount;
            var value = "hey";

            var keys = Enumerable.Range(0, itemCount).ToArray();

            var tree = ImHashMap<int, string>.Empty;
            var trie = IntHashTrie<string>.Empty;

            var treeAddTime = ImHashMapAddIntKey(ref tree, keys, key, value);
            var trieAddTime = IntHashTrieAdd(ref trie, keys, key, value);

            Console.WriteLine("Adding {0} items (ms):", itemCount);
            Console.WriteLine("Tree - " + treeAddTime);
            Console.WriteLine("Trie - " + trieAddTime);
            Console.WriteLine();

            var getTimes = 1 * 1000 * 1000;

            var treeGetTime = ImHashMapGetIntKey(tree, key, getTimes);
            var trieGetTime = IntHashTrieGet(trie, key, getTimes);

            Console.WriteLine("Getting one out of {0} items {1:N0} times (ms):", itemCount, getTimes);
            Console.WriteLine("Tree - " + treeGetTime);
            Console.WriteLine("Trie - " + trieGetTime);
        }

        public static void GetHashTreeVsHashTrie(int itemCount)
        {
            var key = typeof(TreeBenchmarks);
            var value = "hey";

            var keys = typeof(Dictionary<,>).Assembly.GetTypes().Take(itemCount).ToArray();

            var tree = ImHashMap<Type, string>.Empty;
            var trie = HashTrie<Type, string>.Empty;

            var treeAddTime = TreeAdd(ref tree, keys, key, value);
            var trieAddTime = TrieAdd(ref trie, keys, key, value);

            Console.WriteLine("Adding {0} items (ms):", itemCount);
            Console.WriteLine("Tree - " + treeAddTime);
            Console.WriteLine("Trie - " + trieAddTime);
            Console.WriteLine();

            var getTimes = 1 * 1000 * 1000;

            var treeGetTime = TreeGet(tree, key, getTimes);
            var trieGetTime = TrieGet(trie, key, getTimes);

            Console.WriteLine("Getting one out of {0} items {1:N0} times (ms):", itemCount, getTimes);
            Console.WriteLine("Tree - " + treeGetTime);
            Console.WriteLine("Trie - " + trieGetTime);
        }


        #region Dictionary

        private static long DictAdd<K, V>(Dictionary<K, V> dict, K[] keys, K key, V value)
        {
            var addSyncRoot = new object();

            var dictWatch = Stopwatch.StartNew();

            for (var i = 0; i < keys.Length; i++)
                lock (addSyncRoot)
                    dict.Add(keys[i], default(V));

            lock (addSyncRoot)
                dict.Add(key, value);

            dictWatch.Stop();
            GC.Collect();
            return dictWatch.ElapsedMilliseconds;
        }

        private static long DictGet<K, V>(Dictionary<K, V> dict, K key, int times)
        {
            V ignored;

            var getSyncRoot = new object();
            var dictWatch = Stopwatch.StartNew();

            for (var i = 0; i < times; i++)
                lock (getSyncRoot)
                    dict.TryGetValue(key, out ignored);

            dictWatch.Stop();
            GC.Collect();
            return dictWatch.ElapsedMilliseconds;
        }


        #endregion

        #region DryIoc.ImHashMap

        private static long ImHashMapAddIntKey<V>(ref ImHashMap<int, V> tree, int[] keys, int key, V value)
        {
            var ignored = default(V);
            var treeTime = Stopwatch.StartNew();

            for (var i = 0; i < keys.Length; i++)
                Interlocked.Exchange(ref tree, tree.AddOrUpdate(keys[i], ignored));

            Interlocked.Exchange(ref tree, tree.AddOrUpdate(key, value));

            treeTime.Stop();
            GC.Collect();
            return treeTime.ElapsedMilliseconds;
        }

        private static long ImHashMapGetIntKey<V>(ImHashMap<int, V> tree, int key, int times)
        {
            V ignored = default(V);
            var treeWatch = Stopwatch.StartNew();

            for (int i = 0; i < times; i++)
                ignored = tree.GetValueOrDefault(key);

            treeWatch.Stop();
            GC.KeepAlive(ignored);
            GC.Collect();
            return treeWatch.ElapsedMilliseconds;
        }

        private static long TreeAdd<V>(ref ImHashMap<Type, V> tree, Type[] keys, Type key, V value)
        {
            var ignored = default(V);
            var treeTime = Stopwatch.StartNew();

            for (var i = 0; i < keys.Length; i++)
            {
                var k = keys[i];
                Interlocked.Exchange(ref tree, tree.AddOrUpdate(k, ignored));
            }

            Interlocked.Exchange(ref tree, tree.AddOrUpdate(key, value));

            treeTime.Stop();
            GC.KeepAlive(ignored);
            GC.Collect();
            return treeTime.ElapsedMilliseconds;
        }

        private static long TreeGet<T>(ImHashMap<Type, T> tree, Type key, int times)
        {
            T ignored = default(T);

            var treeWatch = Stopwatch.StartNew();

            for (int i = 0; i < times; i++)
            {
                ignored = tree.GetValueOrDefault(key);
            }

            treeWatch.Stop();
            GC.KeepAlive(ignored);
            GC.Collect();
            return treeWatch.ElapsedMilliseconds;
        }

        #endregion

        #region HAMT

        private static long HashTrieAdd<V>(ref HashTrie<V> trie, int[] keys, int key, V value)
        {
            var ignored = default(V);
            var treeTime = Stopwatch.StartNew();

            for (var i = 0; i < keys.Length; i++)
                Interlocked.Exchange(ref trie, trie.AddOrUpdate(keys[i], ignored));

            Interlocked.Exchange(ref trie, trie.AddOrUpdate(key, value));

            treeTime.Stop();
            GC.Collect();
            return treeTime.ElapsedMilliseconds;
        }

        private static long HashTrieGet<V>(HashTrie<V> trie, int key, int times)
        {
            V ignored = default(V);
            var watch = Stopwatch.StartNew();

            for (int i = 0; i < times; i++)
                ignored = trie.GetValueOrDefault(key);

            watch.Stop();
            GC.KeepAlive(ignored);
            GC.Collect();
            return watch.ElapsedMilliseconds;
        }

        #endregion

        private static long TrieAdd<V>(ref HashTrie<Type, V> tree, Type[] keys, Type key, V value)
        {
            var ignored = default(V);
            var treeTime = Stopwatch.StartNew();

            for (var i = 0; i < keys.Length; i++)
            {
                var k = keys[i];
                Interlocked.Exchange(ref tree, tree.AddOrUpdate(k, ignored));
            }

            Interlocked.Exchange(ref tree, tree.AddOrUpdate(key, value));

            treeTime.Stop();
            GC.KeepAlive(ignored);
            GC.Collect();
            return treeTime.ElapsedMilliseconds;
        }

        private static long TrieGet<T>(HashTrie<Type, T> tree, Type key, int times)
        {
            T ignored = default(T);

            var treeWatch = Stopwatch.StartNew();

            for (int i = 0; i < times; i++)
            {
                ignored = tree.GetValueOrDefault(key);
            }

            treeWatch.Stop();
            GC.KeepAlive(ignored);
            GC.Collect();
            return treeWatch.ElapsedMilliseconds;
        }



        #region HAMT with Int Key

        private static long IntHashTrieAdd(ref IntHashTrie<string> trie, int[] keys, int key, string value)
        {
            var ignored = "ignored";
            var treeTime = Stopwatch.StartNew();

            for (var i = 0; i < keys.Length; i++)
                Interlocked.Exchange(ref trie, trie.AddOrUpdate(keys[i], ignored));

            Interlocked.Exchange(ref trie, trie.AddOrUpdate(key, value));

            treeTime.Stop();
            GC.Collect();
            return treeTime.ElapsedMilliseconds;
        }

        private static long IntHashTrieGet<V>(IntHashTrie<V> trie, int key, int times)
        {
            V ignored = default(V);
            var watch = Stopwatch.StartNew();

            for (int i = 0; i < times; i++)
                ignored = trie.GetValueOrDefault(key);

            watch.Stop();
            GC.KeepAlive(ignored);
            GC.Collect();
            return watch.ElapsedMilliseconds;
        }


        #endregion


    }
}
