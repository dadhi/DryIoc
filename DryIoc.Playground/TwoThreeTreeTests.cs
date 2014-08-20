using NUnit.Framework;

namespace DryIoc.Playground
{
    public class TwoThreeTreeTests
    {
        [Test]
        public void Add_items_to_tree()
        {
            var items = new[] { 50, 60, 70, 40, 30, 20, 10, 80, 90, 100 };

            var tree = TwoThreeTree<int, string>.Empty;

            tree = tree.AddOrUpdate(items[1], "60");
            tree = tree.AddOrUpdate(items[0], "50");
        }
    }

    public sealed class TwoThreeTree<K, V>
    {
        public static readonly TwoThreeTree<K, V> Empty = new TwoThreeTree<K, V>();

        public sealed class Item
        {
            public readonly int Hash;
            public readonly K Key;
            public readonly V Value;
            public readonly KV<K, V>[] Conflicts;

            public Item(int hash, K key, V value, KV<K, V>[] conflicts = null)
            {
                Hash = hash;
                Key = key;
                Value = value;
                Conflicts = conflicts;
            }
        }

        public readonly Item LeftItem, RightItem;
        public readonly TwoThreeTree<K, V> LeftTree, Middle, Right;

        public TwoThreeTree<K, V> AddOrUpdate(K key, V value)
        {
            return AddOrUpdate(key.GetHashCode(), key, value);
        }

        #region Implementation

        private TwoThreeTree() { }

        private TwoThreeTree(Item leftItem, Item rightItem,
            TwoThreeTree<K, V> leftTree, TwoThreeTree<K, V> middle, TwoThreeTree<K, V> right)
        {
            LeftItem = leftItem;
            RightItem = rightItem;
            LeftTree = leftTree;
            Middle = middle;
            Right = right;
        }

        private TwoThreeTree<K, V> AddOrUpdate(int hash, K key, V value)
        {
            var newItem = new Item(hash, key, value);

            if (this == Empty)
                return new TwoThreeTree<K, V>(newItem, null, Empty, Empty, Empty);

            // The non=empty tree always has at least LeftItem
            if (hash < LeftItem.Hash) // insert into left tree
            {
                if (LeftTree == Empty) // 2 ways: insert in current node if we have space OR split node and move middle item up
                {
                    if (RightItem == null) // move left item to right and middle tree to the right
                    {
                        return new TwoThreeTree<K, V>(newItem, LeftItem, null, null, Middle);
                    }
                    
                    // split node and pop up the middle item to parent

                }
            }

            return Empty;
        }

        #endregion
    }
}
