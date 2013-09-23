using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
	[TestFixture]
	public class HashTreeTests
	{
		[Test]
		public void CanGetSuccessfully()
		{
			var m = IntTree<int>.Empty
				.AddOrUpdate(1, 11)
				.AddOrUpdate(2, 22)
				.AddOrUpdate(3, 33);

			Assert.AreEqual(11, m.GetOrDefault(1));
            Assert.AreEqual(22, m.GetOrDefault(2));
            Assert.AreEqual(33, m.GetOrDefault(3));
		}

		[Test]
		public void LLCase()
		{
			var m = IntTree<int>.Empty
				.AddOrUpdate(5, 1)
				.AddOrUpdate(4, 2)
				.AddOrUpdate(3, 3);

			Assert.AreEqual(4, m.Key);
			Assert.AreEqual(3, m.Left.Key);
			Assert.AreEqual(5, m.Right.Key);
		}

		[Test]
		public void TreeRemainsBalancedAfterUnbalancedInsertIntoBalancedTree()
		{
			var m = IntTree<int>.Empty
				.AddOrUpdate(5, 1)
				.AddOrUpdate(4, 2)
				.AddOrUpdate(3, 3)
				.AddOrUpdate(2, 4)
				.AddOrUpdate(1, 5);

			Assert.AreEqual(4, m.Key);
			Assert.AreEqual(2, m.Left.Key);
			Assert.AreEqual(1, m.Left.Left.Key);
			Assert.AreEqual(3, m.Left.Right.Key);
			Assert.AreEqual(5, m.Right.Key);
		}

		[Test]
		public void LRCase()
		{
			var m = IntTree<int>.Empty
				.AddOrUpdate(5, 1)
				.AddOrUpdate(3, 2)
				.AddOrUpdate(4, 3);

			Assert.AreEqual(4, m.Key);
			Assert.AreEqual(3, m.Left.Key);
			Assert.AreEqual(5, m.Right.Key);
		}

		[Test]
		public void RRCase()
		{
			var m = IntTree<int>.Empty
				.AddOrUpdate(3, 1)
				.AddOrUpdate(4, 2)
				.AddOrUpdate(5, 3);

			Assert.AreEqual(4, m.Key);
			Assert.AreEqual(3, m.Left.Key);
			Assert.AreEqual(5, m.Right.Key);
		}

		[Test]
		public void RLCase()
		{
			var m = IntTree<int>.Empty
				.AddOrUpdate(3, 1)
				.AddOrUpdate(5, 2)
				.AddOrUpdate(4, 3);

			Assert.AreEqual(4, m.Key);
			Assert.AreEqual(3, m.Left.Key);
			Assert.AreEqual(5, m.Right.Key);
		}

		[Test]
		public void Search_in_empty_tree_should_not_throw()
		{
			var tree = IntTree<int>.Empty;

            Assert.DoesNotThrow(
                () => tree.GetOrDefault(0));
		}

		[Test]
		public void For_two_same_added_items_height_should_be_one()
		{
			var tree = IntTree<string>
				.Empty
				.AddOrUpdate(1, "x")
				.AddOrUpdate(1, "y");

			Assert.AreEqual(1, tree.Height);
		}

	    [Test]
	    public void Enumerated_values_should_be_returned_in_sorted_order()
	    {
	        var items = Enumerable.Range(0, 10).ToArray();
	        var tree = items.Aggregate(IntTree<int>.Empty, (t, i) => t.AddOrUpdate(i, i));

	        var enumerated = tree.Select(t => t.Value).ToArray();

            CollectionAssert.AreEqual(items, enumerated);
	    }

        [Test]
        public void Can_create_tree_with_int_keys()
        {
            var tree = IntTree<string>.Empty
                .AddOrUpdate(1, "a")
                .AddOrUpdate(2, "b")
                .AddOrUpdate(3, "c");

            var value = tree.GetOrDefault(2);

            Assert.That(value, Is.EqualTo("b"));
        }

        [Test]
        public void Can_use_HashTree_to_represent_general_HashTree()
        {
            var tree = IntTree<KeyValuePair<Type, string>[]>.Empty;

            var key = typeof(HashTreeTests);
            var keyHash = key.GetHashCode();
            var value = "test";

            IntTree<KeyValuePair<Type, string>[]>.UpdateValue updateValue = (old, added) =>
            {
                var newItem = added[0];
                var oldItemCount = old.Length;
                for (var i = 0; i < oldItemCount; i++)
                {
                    if (old[i].Key == newItem.Key)
                    {
                        var updatedItems = new KeyValuePair<Type, string>[oldItemCount];
                        Array.Copy(old, updatedItems, updatedItems.Length);
                        updatedItems[i] = newItem;
                        return updatedItems;
                    }
                }

                var addedItems = new KeyValuePair<Type, string>[oldItemCount + 1];
                Array.Copy(old, addedItems, addedItems.Length);
                addedItems[oldItemCount] = newItem;
                return addedItems;
            };

            tree = tree.AddOrUpdate(keyHash, new[] { new KeyValuePair<Type, string>(key, value) }, updateValue);
            tree = tree.AddOrUpdate(keyHash, new[] { new KeyValuePair<Type, string>(key, value) }, updateValue);

            string result = null;

            var items = tree.GetOrDefault(keyHash);
            if (items != null)
            {
                var firstItem = items[0];
                if (firstItem.Key == key)
                    result = firstItem.Value;
                else if (items.Length > 1)
                {
                    for (var i = 1; i < items.Length; i++)
                    {
                        if (items[i].Key == key)
                        {
                            result = items[i].Value;
                            break;
                        }
                    }
                }
            }

            Assert.That(result, Is.EqualTo("test"));
        }

        [Test]
        public void Tree_should_support_arbitrary_keys_by_using_their_hash_code()
        {
            var tree = HashTree<Type, string>.Empty;

            var key = typeof(HashTreeTests);
            var value = "test";

            tree = tree.AddOrUpdate(key, value);

            var result = tree.GetOrDefault(key);
            Assert.That(result, Is.EqualTo("test"));
        }

	    [Test]
	    public void Tree_should_supported_hash_conflicting_keys()
	    {
	        var key1 = new HashConflictingKey<string>("a");
	        var key2 = new HashConflictingKey<string>("b");
	        var key3 = new HashConflictingKey<string>("c");
	        var tree = HashTree<HashConflictingKey<string>, int>.Empty
                .AddOrUpdate(key1, 1)
                .AddOrUpdate(key2, 2)
                .AddOrUpdate(key3, 3);

	        var value = tree.GetOrDefault(key3);

            Assert.That(value, Is.EqualTo(3));
	    }

	    public class DictVsMap
		{
			public object Bla { get; set; }

			public DictVsMap(object bla)
			{
				Bla = bla;
			}
		}
	}

	public class HashConflictingKey<T>
	{
		public T Key;

		public HashConflictingKey(T key)
		{
			Key = key;
		}

		public override int GetHashCode()
		{
			return 1;
		}

		public override bool Equals(object obj)
		{
			return Equals(Key, ((HashConflictingKey<T>)obj).Key);
		}
	}
}
