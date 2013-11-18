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
        public void Tree_should_support_arbitrary_keys_by_using_their_hash_code()
        {
            var tree = HashTree<Type, string>.Empty;

            var key = typeof(HashTreeTests);
            var value = "test";

            tree = tree.AddOrUpdate(key, value);

            var result = tree.GetValueOrDefault(key);
            Assert.That(result, Is.EqualTo("test"));
        }

        [Test]
        public void When_adding_value_with_hash_conflicted_key_Then_I_should_be_able_to_get_it_back()
        {
            var key1 = new HashConflictingKey<string>("a");
            var key2 = new HashConflictingKey<string>("b");
            var key3 = new HashConflictingKey<string>("c");
            var tree = HashTree<HashConflictingKey<string>, int>.Empty
                .AddOrUpdate(key1, 1)
                .AddOrUpdate(key2, 2)
                .AddOrUpdate(key3, 3);

            var value = tree.GetValueOrDefault(key3);

            Assert.That(value, Is.EqualTo(3));
        }

        [Test]
        public void When_adding_couple_of_values_with_hash_conflicted_key_Then_I_should_be_able_to_get_them_back()
        {
            var key1 = new HashConflictingKey<string>("a");
            var key2 = new HashConflictingKey<string>("b");
            var key3 = new HashConflictingKey<string>("c");
            var tree = HashTree<HashConflictingKey<string>, int>.Empty
                .AddOrUpdate(key1, 1)
                .AddOrUpdate(key2, 2)
                .AddOrUpdate(key3, 3);

            var values = tree.TraverseInOrder().ToDictionary(kv => kv.Key.Key, kv => kv.Value);

            Assert.That(values, Is.EqualTo(new Dictionary<string, int>
            {
                { "a", 1 }, 
                { "b", 2 }, 
                { "c", 3 }, 
            }));
        }

        [Test]
        public void Test_that_all_added_values_are_accessible()
        {
            var t = HashTree<int, int>.Empty
                .AddOrUpdate(1, 11)
                .AddOrUpdate(2, 22)
                .AddOrUpdate(3, 33);

            Assert.AreEqual(11, t.GetValueOrDefault(1));
            Assert.AreEqual(22, t.GetValueOrDefault(2));
            Assert.AreEqual(33, t.GetValueOrDefault(3));
        }

        [Test]
        public void Test_balance_ensured_for_left_left_tree()
        {
            var t = HashTree<int, int>.Empty
                .AddOrUpdate(5, 1)
                .AddOrUpdate(4, 2)
                .AddOrUpdate(3, 3);

            //     5   =>    4
            //   4         3   5
            // 3
            Assert.AreEqual(4, t.Key);
            Assert.AreEqual(3, t.Left.Key);
            Assert.AreEqual(5, t.Right.Key);
        }

        [Test]
        public void Test_balance_preserved_when_add_to_balanced_tree()
        {
            var t = HashTree<int, int>.Empty
                .AddOrUpdate(5, 1)
                .AddOrUpdate(4, 2)
                .AddOrUpdate(3, 3)
                // add to that
                .AddOrUpdate(2, 4)
                .AddOrUpdate(1, 5);

            //       4    =>     4
            //     3   5      2     5
            //   2          1   3
            // 1
            Assert.AreEqual(4, t.Key);
            Assert.AreEqual(2, t.Left.Key);
            Assert.AreEqual(1, t.Left.Left.Key);
            Assert.AreEqual(3, t.Left.Right.Key);
            Assert.AreEqual(5, t.Right.Key);
        }

        [Test]
        public void Test_balance_ensured_for_left_right_tree()
        {
            var t = HashTree<int, int>.Empty
                .AddOrUpdate(5, 1)
                .AddOrUpdate(3, 2)
                .AddOrUpdate(4, 3);

            //     5  =>    5   =>   4 
            //  3         4        3   5
            //    4     3  
            Assert.AreEqual(4, t.Key);
            Assert.AreEqual(3, t.Left.Key);
            Assert.AreEqual(5, t.Right.Key);
        }

        [Test]
        public void Test_balance_ensured_for_right_right_tree()
        {
            var t = HashTree<int, int>.Empty
                .AddOrUpdate(3, 1)
                .AddOrUpdate(4, 2)
                .AddOrUpdate(5, 3);

            // 3      =>     4
            //   4         3   5
            //     5
            Assert.AreEqual(4, t.Key);
            Assert.AreEqual(3, t.Left.Key);
            Assert.AreEqual(5, t.Right.Key);
        }

        [Test]
        public void Test_balance_ensured_for_right_left_tree()
        {
            var t = HashTree<int, int>.Empty
                .AddOrUpdate(3, 1)
                .AddOrUpdate(5, 2)
                .AddOrUpdate(4, 3);

            // 3      =>   3     =>    4
            //    5          4       3   5
            //  4              5
            Assert.AreEqual(4, t.Key);
            Assert.AreEqual(3, t.Left.Key);
            Assert.AreEqual(5, t.Right.Key);
        }

        [Test]
        public void Search_in_empty_tree_should_not_throw()
        {
            var tree = HashTree<int, int>.Empty;

            Assert.DoesNotThrow(
                () => tree.GetValueOrDefault(0));
        }

        [Test]
        public void For_two_same_added_items_height_should_be_one()
        {
            var tree = HashTree<int, string>.Empty
                .AddOrUpdate(1, "x")
                .AddOrUpdate(1, "y");

            Assert.AreEqual(1, tree.Height);
        }

        [Test]
        public void Enumerated_values_should_be_returned_in_sorted_order()
        {
            var items = Enumerable.Range(0, 10).ToArray();
            var tree = items.Aggregate(HashTree<int, int>.Empty, (t, i) => t.AddOrUpdate(i, i));

            var enumerated = tree.TraverseInOrder().Select(t => t.Value).ToArray();

            CollectionAssert.AreEqual(items, enumerated);
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
