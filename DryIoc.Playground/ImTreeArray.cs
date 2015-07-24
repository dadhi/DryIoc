using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace DryIoc.Playground
{
    /// <summary>Immutable array based on wide hash tree, where each node is sub-array with predefined size: 32 is by default.
    /// Array supports only append, no remove.</summary>
    public class ImTreeArray
    {
        /// <summary>Node array size. When the item added to same node, array will be copied. 
        /// So if array is too big performance will degrade. Should be power of two: e.g. 2, 4, 8, 16, 32...</summary>
        public const int NODE_ARRAY_SIZE = 32;

        /// <summary>Empty/default value to start from.</summary>
        public static readonly ImTreeArray Empty = new ImTreeArray(0);

        /// <summary>Number of items in array.</summary>
        public readonly int Length;

        /// <summary>Appends value and returns new array.</summary>
        /// <param name="value">Value to append.</param> <returns>New array.</returns>
        public virtual ImTreeArray Append(object value)
        {
            return Length < NODE_ARRAY_SIZE
                ? new ImTreeArray(Length + 1, _items.AppendOrUpdate(value))
                : new Tree(Length, ImTreeMapIntToObj.Empty.AddOrUpdate(0, _items)).Append(value);
        }

        /// <summary>Returns item stored at specified index. Method relies on underlying array for index range checking.</summary>
        /// <param name="index">Index to look for item.</param> <returns>Found item.</returns>
        /// <exception cref="ArgumentOutOfRangeException">from underlying node array.</exception>
        public virtual object Get(int index)
        {
            return _items[index];
        }

        /// <summary>Returns index of first equal value in array if found, or -1 otherwise.</summary>
        /// <param name="value">Value to look for.</param> <returns>Index of first equal value, or -1 otherwise.</returns>
        public virtual int IndexOf(object value)
        {
            if (_items == null || _items.Length == 0)
                return -1;

            for (var i = 0; i < _items.Length; ++i)
            {
                var item = _items[i];
                if (ReferenceEquals(item, value) || Equals(item, value))
                    return i;
            }
            return -1;
        }

        #region Implementation

        private readonly object[] _items;

        private ImTreeArray(int length, object[] items = null)
        {
            Length = length;
            _items = items;
        }

        private sealed class Tree : ImTreeArray
        {
            private const int NODE_ARRAY_BIT_MASK = NODE_ARRAY_SIZE - 1; // for length 32 will be 11111 binary.
            private const int NODE_ARRAY_BIT_COUNT = 5;                  // number of set bits in NODE_ARRAY_BIT_MASK.

            public override ImTreeArray Append(object value)
            {
                var key = Length >> NODE_ARRAY_BIT_COUNT;
                var nodeItems = _tree.GetValueOrDefault(key) as object[];
                return new Tree(Length + 1, _tree.AddOrUpdate(key, nodeItems.AppendOrUpdate(value)));
            }

            public override object Get(int index)
            {
                return ((object[])_tree.GetValueOrDefault(index >> NODE_ARRAY_BIT_COUNT))[index & NODE_ARRAY_BIT_MASK];
            }

            public override int IndexOf(object value)
            {
                foreach (var node in _tree.Enumerate())
                {
                    var nodeItems = (object[])node.Value;
                    if (!nodeItems.IsNullOrEmpty())
                    {
                        for (var i = 0; i < nodeItems.Length; ++i)
                        {
                            var item = nodeItems[i];
                            if (ReferenceEquals(item, value) || Equals(item, value))
                                return node.Key << NODE_ARRAY_BIT_COUNT | i;
                        }
                    }
                }

                return -1;
            }

            public Tree(int length, ImTreeMapIntToObj tree)
                : base(length)
            {
                _tree = tree;
            }

            private readonly ImTreeMapIntToObj _tree;
        }

        #endregion
    }

    [TestFixture]
    public class ImTreeArrayTests
    {
        [Test]
        public void Append_to_end()
        {
            var store = ImTreeArray.Empty;
            store = store
                .Append("a")
                .Append("b")
                .Append("c")
                .Append("d");

            Assert.AreEqual("d", store.Get(3));
            Assert.AreEqual("c", store.Get(2));
            Assert.AreEqual("b", store.Get(1));
            Assert.AreEqual("a", store.Get(0));
        }

        [Test]
        public void Indexed_store_get_or_add()
        {
            var store = ImTreeArray.Empty;

            store = store
                .Append("a")
                .Append("b")
                .Append("c")
                .Append("d");

            var i = store.Length - 1;

            Assert.AreEqual("d", store.Get(i));
        }

        [Test]
        public void IndexOf_with_empty_store()
        {
            var store = ImTreeArray.Empty;

            Assert.AreEqual(-1, store.IndexOf("a"));
        }

        [Test]
        public void IndexOf_non_existing_item()
        {
            var store = ImTreeArray.Empty;

            store = store.Append("a");

            Assert.AreEqual(-1, store.IndexOf("b"));
        }

        [Test]
        public void IndexOf_existing_item()
        {
            var store = ImTreeArray.Empty;

            store = store
                .Append("a")
                .Append("b")
                .Append("c");

            Assert.AreEqual(1, store.IndexOf("b"));
        }

        [Test]
        public void Append_for_full_node_and_get_node_last_item()
        {
            var nodeArrayLength = ImTreeArray.NODE_ARRAY_SIZE;
            var array = ImTreeArray.Empty;
            for (var i = 0; i <= nodeArrayLength; i++)
                array = array.Append(i);

            var item = array.Get(nodeArrayLength);

            Assert.That(item, Is.EqualTo(nodeArrayLength));
        }

        /// <remarks>Issue #17 AppendableArray stops to work over 64 elements. (dev. branch)</remarks>
        [Test]
        public void Append_and_get_items_in_multiple_node_array()
        {
            var list = new List<Foo>();
            var array = ImTreeArray.Empty;

            for (var index = 0; index < 129; ++index)
            {
                var item = new Foo { Index = index };

                list.Add(item);
                array = array.Append(item);
            }

            for (var index = 0; index < list.Count; ++index)
            {
                var listItem = list[index];
                var arrayItem = array.Get(index);

                Assert.AreEqual(index, listItem.Index);
                Assert.AreEqual(index, ((Foo)arrayItem).Index);
            }
        }

        class Foo
        {
            public int Index;
        }
    }
}
