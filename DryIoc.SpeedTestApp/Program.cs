using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DryIoc.Playground;
using DryIoc.Playground.Performance;

namespace DryIoc.SpeedTestApp
{
	class Program
	{
		static void Main()
		{
            //Thread.CurrentThread.Priority = ThreadPriority.Highest;
            //CompareIlEmitDynamicMethodVsExpressionCompileSpeed();
            //CompareBitOpVsIsOpSpeed();
            //CompareDirectVsIndirectArrayAccessSpeed();
            CompareTreeGet();
            //CompareClosureFieldAccess();
            //DoCompareTryGetVsGetOrDefault();
		    //CompareHashTreeEnumeration();
		    //CompareMethodArgumentPassing();
		    //CompareTypesForEquality(typeof(string));
			Console.ReadKey();
		}

        private static void CompareTreeGet()
        {
            GetHashTreeVs23Tree(20);
            Console.WriteLine();
            GetHashTreeVs23Tree(2000);
            //GetDictVsHashTrieOfInt(25);
            //GetHashTreeVsHashTrie(itemCount: 20);
            //GetDictVsHashTrie2OfInt(25);
            //GetHashTreeVsHashTrie2OfInt(25);
            //GetHashTreeVsIndexedStore(25);
        }

        private static void CompareDirectVsIndirectArrayAccessSpeed()
        {
            DirectVsIndirectArrayAccessSpeedTests.Compare();
            Console.WriteLine();
            DirectVsIndirectArrayAccessSpeedTests.Compare();
        }

        private static void CompareIlEmitDynamicMethodVsExpressionCompileSpeed()
        {
            //IlEmitDynamicMethodVsExpressionCompile.Compare();
            IlEmitDynamicMethodVsExpressionCompile.CompareResultDelegates();
            Console.WriteLine();
            IlEmitDynamicMethodVsExpressionCompile.CompareResultDelegates();
        }

        private static void CompareBitOpVsIsOpSpeed()
        {
            BitOpVsIsOpSpeedTests.Compare();
            Console.WriteLine();
            BitOpVsIsOpSpeedTests.Compare();
        }

        private static void DoCompareTryGetVsGetOrDefault()
        {
            CompareTryGetVsGetOrDefault.Compare();
            Console.WriteLine();
            CompareTryGetVsGetOrDefault.Compare();
        }

	    private static void CompareMethodArgumentPassing()
	    {
            MethodArgumentsPassingSpeedComparison.Compare();
	        Console.WriteLine();
	        MethodArgumentsPassingSpeedComparison.Compare();
	    }

	    private static void CompareHashTreeEnumeration()
	    {
	        HashTreeEnumerationSpeedTests.CompareListVsHashTree();
            Console.WriteLine();
	        HashTreeEnumerationSpeedTests.CompareListVsHashTree();
	    }

        private static void CompareClosureFieldAccess()
        {
            ClosureFieldsAccessSpeed.Test();
            Console.WriteLine();
            ClosureFieldsAccessSpeed.Test();
        }

	    private static void CompareTypesForEquality(Type actual)
	    {
            var times = 5 * 1000 * 1000;
	        
            var expected = typeof (string);
	        var result = false;
            Stopwatch time;

	        time = Stopwatch.StartNew();
	        for (int i = 0; i < times; i++)
            {
                result = expected == actual;
            }
            time.Stop();
            Console.WriteLine("type1 == type2 took {0} milliseconds to complete.", time.ElapsedMilliseconds);
            GC.Collect();


            time = Stopwatch.StartNew();
            for (int i = 0; i < times; i++)
            {
                result = expected.Equals(actual);
            }
            time.Stop();
            Console.WriteLine("type1.Equals(type2) took {0} milliseconds to complete.", time.ElapsedMilliseconds);
            GC.Collect();


            time = Stopwatch.StartNew();
            for (int i = 0; i < times; i++)
	        {
                result = Equals(expected, actual);
	        }
            time.Stop();
            Console.WriteLine("Equals(type1, type2) took {0} milliseconds to complete.", time.ElapsedMilliseconds);
            GC.Collect();

            time = Stopwatch.StartNew();
            for (int i = 0; i < times; i++)
            {
                result = ReferenceEquals(expected, actual) || expected.Equals(actual);
            }
            time.Stop();
            Console.WriteLine("ReferenceEquals(type1, type2) || type1.Equals(type2) took {0} milliseconds to complete.", time.ElapsedMilliseconds);
            GC.Collect();

            //var expectedHandle = expected.TypeHandle;
            //var actualHandle = actual.TypeHandle;
            //time = Stopwatch.StartNew();
            //for (int i = 0; i < times; i++)
            //{
            //    result = expectedHandle.Equals(actualHandle);
            //}
            //time.Stop();
            //Console.WriteLine("TypeHandle.Equals(TypeHandle) took {0} milliseconds to complete.", time.ElapsedMilliseconds);
            //GC.Collect();
            GC.KeepAlive(result);
	    }

        public static void GetHashTreeVs23Tree(int itemCount)
        {
            var key = typeof(IntTreeTests.DictVsMap);
            var value = "hey";

            var keys = typeof(Dictionary<,>).Assembly.GetTypes().Take(itemCount).ToArray();

            var tree = HashTree<Type, string>.Empty;
            var tree23 = TwoThreeTree<Type, string>.Empty;

            var treeAddTime = TreeAdd(ref tree, keys, key, value);
            var trieAddTime = Tree23Add(ref tree23, keys, key, value);

            Console.WriteLine("Adding {0} items (ms):", itemCount);
            Console.WriteLine("Tree - " + treeAddTime);
            Console.WriteLine("2-3 - " + trieAddTime);
            Console.WriteLine();

            var getTimes = 1 * 1000 * 1000;

            var treeGetTime = TreeGet(tree, key, getTimes);
            var tree23GetTime = Tree23Get(tree23, key, getTimes);

            Console.WriteLine("Getting one out of {0} items {1:N0} times (ms):", itemCount, getTimes);
            Console.WriteLine("Tree - " + treeGetTime);
            Console.WriteLine("2-3 - " + tree23GetTime);
        }

        public static void GetHashTreeVsHashTrie(int itemCount)
        {
            var key = typeof(IntTreeTests.DictVsMap);
            var value = "hey";

            var keys = typeof(Dictionary<,>).Assembly.GetTypes().Take(itemCount).ToArray();

            var tree = HashTree<Type, string>.Empty;
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

        public static void GetAvlTreeVsHashTree(int itemCount)
        {
            var key = typeof(IntTreeTests.DictVsMap);
            var value = "hey";

            var keys = typeof(Dictionary<,>).Assembly.GetTypes().Take(itemCount).ToArray();

            var avlTree = Playground.HashTree<Type, string>.Empty;
            var hashTree = HashTree<Type, string>.Empty;

            var avlTreeAddTime = HashTree4Add(ref avlTree, keys, key, value);
            var hashTreeAddTime = TreeAdd(ref hashTree, keys, key, value);

            Console.WriteLine("Adding {0} items (ms):", itemCount);
            Console.WriteLine("AvlTree - " + avlTreeAddTime);
            Console.WriteLine("HashTree - " + hashTreeAddTime);
            Console.WriteLine();

            var getTimes = 1 * 1000 * 1000;

            var avlTreeGetTime = HashTree4Get(avlTree, key, getTimes);
            var hashTreeGetTime = TreeGet(hashTree, key, getTimes);

            Console.WriteLine("Getting one out of {0} items {1:N0} times (ms):", itemCount, getTimes);
            Console.WriteLine("AvlTree - " + avlTreeGetTime);
            Console.WriteLine("HashTree - " + hashTreeGetTime);
        }

        public static void GetHashTree2vs1OfInt(int itemCount)
        {
            var key = itemCount;
            var value = "hey";

            var keys = Enumerable.Range(0, itemCount).ToArray();

            var v2 = HashTreeV2<string>.Empty;
            var v1 = Playground.HashTree<int, string>.Empty;

            var v2add = IntTreeV2Add(ref v2, keys, key, value);
            var v1add = IntTreeAdd(ref v1, keys, key, value);

            Console.WriteLine("Adding {0} items (ms):", itemCount);
            Console.WriteLine("v2 - " + v2add);
            Console.WriteLine("v1 - " + v1add);
            Console.WriteLine();

            var getTimes = 1 * 1000 * 1000;

            var v2get = IntTreeV2Get(v2, key, getTimes);
            var v1get = IntTreeGet(v1, key, getTimes);

            Console.WriteLine("Getting one out of {0} items {1:N0} times (ms):", itemCount, getTimes);
            Console.WriteLine("v2 - " + v2get);
            Console.WriteLine("v1 - " + v1get);
        }

        public static void GetHashTreeXVsHashTree(int itemCount)
        {
            var key = typeof(IntTreeTests.DictVsMap);
            var value = "hey";

            var keys = typeof(Dictionary<,>).Assembly.GetTypes().Take(itemCount).ToArray();

            var typeTree = HashTreeX<Type, string>.Using();
            var hashTree = Playground.HashTree<Type, string>.Empty;

            var typeTreeAddTime = HashTreeXAdd(ref typeTree, keys, key, value);
            var hashTreeAddTime = HashTree4Add(ref hashTree, keys, key, value);

            Console.WriteLine("Adding {0} items (ms):", itemCount);
            Console.WriteLine("HashTreeX - " + typeTreeAddTime);
            Console.WriteLine("HashTree - " + hashTreeAddTime);
            Console.WriteLine();

            var getTimes = 1 * 1000 * 1000;

            var typeTreeGetTime = HashTreeXGet(typeTree, key, getTimes);
            var hashTreeGetTime = HashTree4Get(hashTree, key, getTimes);

            Console.WriteLine("Getting one out of {0} items {1:N0} times (ms):", itemCount, getTimes);
            Console.WriteLine("HashTreeX - " + typeTreeGetTime);
            Console.WriteLine("HashTree - " + hashTreeGetTime);
        }

        public static void GetDictVsHashTree2OfType(int itemCount)
        {
            var key = typeof(IntTreeTests.DictVsMap);
            var value = "hey";

            var keys = typeof(Dictionary<,>).Assembly.GetTypes().Take(itemCount).ToArray();

            var dict = new Dictionary<Type, string>();
            var tree = Playground.HashTree<Type, string>.Empty;

            var dictAddTime = DictAdd(dict, keys, key, value);
            var treeAddTime = HashTree4Add(ref tree, keys, key, value);

            Console.WriteLine("Adding {0} items (ms):", itemCount);
            Console.WriteLine("Dict - " + dictAddTime);
            Console.WriteLine("Tree - " + treeAddTime);
            Console.WriteLine();

            var getTimes = 1 * 1000 * 1000;

            var dictGetTime = DictGet(dict, key, getTimes);
            var treeGetTime = HashTree4Get(tree, key, getTimes);

            Console.WriteLine("Getting one out of {0} items {1:N0} times (ms):", itemCount, getTimes);
            Console.WriteLine("Dict - " + dictGetTime);
            Console.WriteLine("Tree - " + treeGetTime);
        }

        public static void GetIntTreeVsIntNTree(int itemCount)
        {
            var key = itemCount - 1;
            var value = "hey";

            var keys = Enumerable.Range(0, itemCount).ToArray();

            var tree = Playground.HashTree<int, string>.Empty;
            var ntree = IntNTree<string>.Empty;

            var treeAddTime = IntTreeAdd(ref tree, keys, key, value);
            var ntreeAddTime = IntNTreeAdd(ref ntree, keys, key, value);

            Console.WriteLine("Adding {0} items (ms):", itemCount);
            Console.WriteLine("Tree - " + treeAddTime);
            Console.WriteLine("NTree - " + ntreeAddTime);
            Console.WriteLine();

            var getTimes = 1 * 1000 * 1000;

            var treeGetTime = IntTreeGet(tree, key, getTimes);
            var ntreeGetTime = IntNTreeGet(ntree, key, getTimes);

            Console.WriteLine("Getting one out of {0} items {1:N0} times (ms):", itemCount, getTimes);
            Console.WriteLine("Tree - " + treeGetTime);
            Console.WriteLine("NTree - " + ntreeGetTime);
        }

        public static void GetDictVsHashTreeOfInt(int itemCount)
        {
            var key = itemCount;
            var value = "hey";

            var keys = Enumerable.Range(0, itemCount).ToArray();

            var dict = new Dictionary<int, string>();
            var tree = Playground.HashTree<int, string>.Empty;

            var dictAddTime = DictAdd(dict, keys, key, value);
            var treeAddTime = IntTreeAdd(ref tree, keys, key, value);

            Console.WriteLine("Adding {0} items (ms):", itemCount);
            Console.WriteLine("Dict - " + dictAddTime);
            Console.WriteLine("Tree - " + treeAddTime);
            Console.WriteLine();

            var getTimes = 1 * 1000 * 1000;

            var dictGetTime = DictGet(dict, key, getTimes);
            var treeGetTime = IntTreeGet(tree, key, getTimes);

            Console.WriteLine("Getting one out of {0} items {1:N0} times (ms):", itemCount, getTimes);
            Console.WriteLine("Dict - " + dictGetTime);
            Console.WriteLine("Tree - " + treeGetTime);
        }

        public static void GetDictVsHashTrieOfInt(int itemCount)
        {
            var key = itemCount;
            var value = "hey";

            var keys = Enumerable.Range(0, itemCount).ToArray();

            var dict = new Dictionary<int, string>();
            var trie = HashTrie<string>.Empty;

            var dictAddTime = DictAdd(dict, keys, key, value);
            var treeAddTime = IntTrieAdd(ref trie, keys, key, value);

            Console.WriteLine("Adding {0} items (ms):", itemCount);
            Console.WriteLine("Dict - " + dictAddTime);
            Console.WriteLine("Trie - " + treeAddTime);
            Console.WriteLine();

            var getTimes = 1 * 1000 * 1000;

            var dictGetTime = DictGet(dict, key, getTimes);
            var treeGetTime = IntTrieGet(trie, key, getTimes);

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
			var trie = HashTrie2<string>.Empty;

			var dictAddTime = DictAdd(dict, keys, key, value);
			var treeAddTime = IntTrieAdd(ref trie, keys, key, value);

			Console.WriteLine("Adding {0} items (ms):", itemCount);
			Console.WriteLine("Dict - " + dictAddTime);
			Console.WriteLine("Trie - " + treeAddTime);
			Console.WriteLine();

			var getTimes = 1 * 1000 * 1000;

			var dictGetTime = DictGet(dict, key, getTimes);
			var treeGetTime = IntTrieGet(trie, key, getTimes);

			Console.WriteLine("Getting one out of {0} items {1:N0} times (ms):", itemCount, getTimes);
			Console.WriteLine("Dict - " + dictGetTime);
			Console.WriteLine("Trie - " + treeGetTime);
		}

        public static void GetHashTreeVsHashTrie2OfInt(int itemCount)
        {
            var key = itemCount;
            var value = "hey";

            var keys = Enumerable.Range(0, itemCount).ToArray();

            var tree = Playground.HashTree<int, string>.Empty;
            var trie = HashTrie2<string>.Empty;

            var treeAddTime = IntTreeAdd(ref tree, keys, key, value);
            var trieAddTime = IntTrieAdd(ref trie, keys, key, value);

            Console.WriteLine("Adding {0} items (ms):", itemCount);
            Console.WriteLine("Tree - " + treeAddTime);
            Console.WriteLine("Trie - " + trieAddTime);
            Console.WriteLine();

            var getTimes = 1 * 1000 * 1000;

            var treeGetTime = IntTreeGet(tree, key, getTimes);
            var trieGetTime = IntTrieGet(trie, key, getTimes);

            Console.WriteLine("Getting one out of {0} items {1:N0} times (ms):", itemCount, getTimes);
            Console.WriteLine("Tree - " + treeGetTime);
            Console.WriteLine("Trie - " + trieGetTime);
        }

        public static void GetHashTreeVsIndexedStore(int itemCount)
        {
            var key = itemCount;
            var value = "hey";

            var keys = Enumerable.Range(0, itemCount).ToArray();

            var tree = HashTree<string>.Empty;
            var store = AppendableArray<string>.Empty;

            var treeAddTime = IntAdd(ref tree, keys, key, value);
            var trieAddTime = IntAdd(ref store, keys, value);

            Console.WriteLine("Adding {0} items (ms):", itemCount);
            Console.WriteLine("Tree - " + treeAddTime);
            Console.WriteLine("Store - " + trieAddTime);
            Console.WriteLine();

            var getTimes = 1 * 1000 * 1000;

            var treeGetTime = IntGet(tree, key, getTimes);
            var trieGetTime = IntGet(store, key, getTimes);

            Console.WriteLine("Getting one out of {0} items {1:N0} times (ms):", itemCount, getTimes);
            Console.WriteLine("Tree - " + treeGetTime);
            Console.WriteLine("Store - " + trieGetTime);
        }

		public static void GetDictVsHashTreeOfType(int itemCount)
		{
			var key = typeof(IntTreeTests.DictVsMap);
			var value = "hey";

			var keys = typeof(Dictionary<,>).Assembly.GetTypes().Take(itemCount).ToArray();

			var dict = new Dictionary<Type, string>();
			var tree = Playground.HashTree<Type, string>.Empty;

			var dictAddTime = DictAdd(dict, keys, key, value);
			var treeAddTime = HashTreeAdd(ref tree, keys, key, value);

			Console.WriteLine("Adding {0} items (ms):", itemCount);
			Console.WriteLine("Dict - " + dictAddTime);
			Console.WriteLine("Tree - " + treeAddTime);
			Console.WriteLine();

			var getTimes = 1 * 1000 * 1000;

			var dictGetTime = DictGet(dict, key, getTimes);
			var treeGetTime = HashTreeGet(tree, key, getTimes);

			Console.WriteLine("Getting one out of {0} items {1:N0} times (ms):", itemCount, getTimes);
			Console.WriteLine("Dict - " + dictGetTime);
			Console.WriteLine("Tree - " + treeGetTime);
		}

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

        private static long IntNTreeAdd<V>(ref IntNTree<V> tree, int[] keys, int key, V value)
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

	    private static long IntTreeAdd<V>(ref Playground.HashTree<int, V> tree, int[] keys, int key, V value)
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

        private static long IntTrieAdd<V>(ref HashTrie<V> trie, int[] keys, int key, V value)
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

        private static long IntTrieAdd(ref HashTrie2<string> trie, int[] keys, int key, string value)
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

        private static long IntAdd(ref AppendableArray<string> store, int[] keys, string value)
        {
            const string ignored = "";
            var treeTime = Stopwatch.StartNew();

            for (var i = 0; i < keys.Length; i++)
                Interlocked.Exchange(ref store, store.AppendOrUpdate(ignored + i));

            Interlocked.Exchange(ref store, store.AppendOrUpdate(value));

            treeTime.Stop();
            GC.Collect();
            return treeTime.ElapsedMilliseconds;
        }

        private static long IntAdd(ref HashTree<string> tree, int[] keys, int key, string value)
        {
            var ignored = "ignored";
            var treeTime = Stopwatch.StartNew();

            for (var i = 0; i < keys.Length; i++)
                Interlocked.Exchange(ref tree, tree.AddOrUpdate(keys[i], ignored));

            Interlocked.Exchange(ref tree, tree.AddOrUpdate(key, value));

            treeTime.Stop();
            GC.Collect();
            return treeTime.ElapsedMilliseconds;
        }

        private static long IntTreeV2Add<V>(ref HashTreeV2<V> tree, int[] keys, int key, V value)
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

        private static long IntNTreeGet<V>(IntNTree<V> tree, int key, int times)
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

		private static long IntTreeGet<V>(Playground.HashTree<int, V> tree, int key, int times)
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

        private static long IntGet<V>(HashTree< V> tree, int key, int times)
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

        private static long IntGet(AppendableArray<string> store, int key, int times)
        {
            object ignored = "ignored";
            var treeWatch = Stopwatch.StartNew();

            for (int i = 0; i < times; i++)
                ignored = store.Get(key);

            treeWatch.Stop();
            GC.KeepAlive(ignored);
            GC.Collect();
            return treeWatch.ElapsedMilliseconds;
        }

        private static long IntTrieGet<V>(HashTrie<V> trie, int key, int times)
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

        private static long IntTrieGet<V>(HashTrie2<V> trie, int key, int times)
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

        private static long IntTreeV2Get<V>(HashTreeV2<V> tree, int key, int times)
        {
            V ignored = default(V);
            var treeWatch = Stopwatch.StartNew();

            for (int i = 0; i < times; i++)
                ignored = tree.TryGet(key);

            treeWatch.Stop();
            GC.KeepAlive(ignored);
            GC.Collect();
            return treeWatch.ElapsedMilliseconds;
        }

        private static long HashTree4Add<V>(ref Playground.HashTree<Type, V> tree, Type[] keys, Type key, V value)
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

        private static long TreeAdd<V>(ref HashTree<Type, V> tree, Type[] keys, Type key, V value)
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

        private static long Tree23Add<V>(ref TwoThreeTree<Type, V> tree, Type[] keys, Type key, V value)
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

        private static long HashTreeXAdd<V>(ref HashTreeX<Type, V> tree, Type[] keys, Type key, V value)
        {
            var ignored = default(V);
            var treeTime = Stopwatch.StartNew();

            for (var i = 0; i < keys.Length; i++)
            {
                var k = keys[i];
                tree.AddOrUpdate(k, ignored);
            }

            tree.AddOrUpdate(key, value);

            treeTime.Stop();
            GC.KeepAlive(ignored);
            GC.Collect();
            return treeTime.ElapsedMilliseconds;
        }

	    private static long HashTreeAdd<V>(ref Playground.HashTree<Type, V> tree, Type[] keys, Type key, V value)
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

        private static long HashTree4Get<T>(Playground.HashTree<Type, T> tree, Type key, int times)
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

        private static long TreeGet<T>(HashTree<Type, T> tree, Type key, int times)
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

        private static long Tree23Get<T>(TwoThreeTree<Type, T> tree, Type key, int times)
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
        
        private static long HashTreeXGet<T>(HashTreeX<Type, T> tree, Type key, int times)
        {
            T ignored = default(T);

            var treeWatch = Stopwatch.StartNew();

            for (int i = 0; i < times; i++)
            {
                ignored = tree.TryGet(key);
            }

            treeWatch.Stop();
            GC.KeepAlive(ignored);
            GC.Collect();
            return treeWatch.ElapsedMilliseconds;
        }

	    private static long HashTreeGet<T>(Playground.HashTree<Type, T> tree, Type key, int times)
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
	}
}
