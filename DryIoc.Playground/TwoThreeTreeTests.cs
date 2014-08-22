using NUnit.Framework;

namespace DryIoc.Playground
{
    public class TwoThreeTreeTests
    {
        [Test]
        public void Add_unordered_items_to_tree()
        {
            var items = new[] { 50, 60, 70, 40, 30, 20, 10, 80, 90, 100 };

            var tree = TwoThreeTree<int, string>.Empty;

            for (var i = 0; i < items.Length; i++)
                tree = tree.AddOrUpdate(items[i], i.ToString());

            // the result for keys should be:
            //              40
            //            /    \
            //          20     60,80
            //         /  \    / |   \
            //       10   30  50 70 90,100

            var root = (TwoThreeTree<int, string>.OneItemTree)tree.Root;
            Assert.That(root.Item.Key, Is.EqualTo(40));

            var rootLeft = (TwoThreeTree<int, string>.OneItemTree)root.Left;
            Assert.That(rootLeft.Item.Key, Is.EqualTo(20));

            var rootLeftLeft = (TwoThreeTree<int, string>.OneItemLeaf)rootLeft.Left;
            Assert.That(rootLeftLeft.Key, Is.EqualTo(10));

            var rootLeftRight = (TwoThreeTree<int, string>.OneItemLeaf)rootLeft.Right;
            Assert.That(rootLeftRight.Key, Is.EqualTo(30));

            var rootRight = (TwoThreeTree<int, string>.TwoItemsTree)root.Right;
            Assert.That(rootRight.LeftItem.Key, Is.EqualTo(60));
            Assert.That(rootRight.RightItem.Key, Is.EqualTo(80));

            var rootRightLeft = (TwoThreeTree<int, string>.OneItemLeaf)rootRight.Left;
            Assert.That(rootRightLeft.Key, Is.EqualTo(50));

            var rootRightMiddle = (TwoThreeTree<int, string>.OneItemLeaf)rootRight.Middle;
            Assert.That(rootRightMiddle.Key, Is.EqualTo(70));

            var rootRightRight = (TwoThreeTree<int, string>.TwoItemsLeaf)rootRight.Right;
            Assert.That(rootRightRight.LeftItem.Key, Is.EqualTo(90));
            Assert.That(rootRightRight.RightItem.Key, Is.EqualTo(100));
        }

        [Test]
        public void Get_stored_value_by_key()
        {
            var items = new[] { 50, 60, 70, 40, 30, 20, 10, 80, 90, 100 };

            var tree = TwoThreeTree<int, int>.Empty;

            for (var i = 0; i < items.Length; i++)
                tree = tree.AddOrUpdate(items[i], i);

            // the result for keys should be:
            //              40
            //            /    \
            //          20     60,80
            //         /  \    / |   \
            //       10   30  50 70 90,100

            for (var i = 0; i < items.Length; i++)
            {
                var value = tree.GetValueOrDefault(items[i], -1);
                Assert.That(value, Is.EqualTo(i));
            }
        }
    }

    public sealed class TwoThreeTree<K, V>
    {
        public static readonly TwoThreeTree<K, V> Empty = new TwoThreeTree<K, V>();

        public readonly INode Root;
        public bool IsEmpty { get { return Root == null; } }

        private TwoThreeTree(INode root = null)
        {
            Root = root;
        }

        public TwoThreeTree<K, V> AddOrUpdate(K key, V value)
        {
            return IsEmpty ? new TwoThreeTree<K, V>(new OneItemLeaf(key.GetHashCode(), key, value)) 
                : new TwoThreeTree<K, V>(Root.AddOrUpdate(new OneItemLeaf(key.GetHashCode(), key, value)));
        }

        public V GetValueOrDefault(K key, V defaultValue = default(V))
        {
            if (Root == null) return defaultValue;
            var node = Root;
            var hash = key.GetHashCode();
            while (node.Type != NodeType.OneItemLeaf)
                node = node.LookFor(hash);

            var item = (OneItemLeaf)node;
            return hash == item.Hash && (ReferenceEquals(key, item.Key) || key.Equals(item.Key))
                ? item.Value : item.GetConflictedValueOrDefault(key, defaultValue);
        }

        public enum NodeType { OneItemLeaf, TwoItemsLeaf, OneItemTree, TwoItemsTree }

        public interface INode
        {
            NodeType Type { get; }
            INode AddOrUpdate(OneItemLeaf item);
            INode LookFor(int hash);
        }

        public sealed class OneItemLeaf : INode
        {
            public NodeType Type { get { return NodeType.OneItemLeaf; } }

            public readonly int Hash;
            public readonly K Key;
            public readonly V Value;
            public readonly KV<K, V>[] Conflicts;

            public OneItemLeaf(int hash, K key, V value, KV<K, V>[] conflicts = null)
            {
                Hash = hash;
                Key = key;
                Value = value;
                Conflicts = conflicts;
            }

            public INode AddOrUpdate(OneItemLeaf item)
            {
                return item.Hash > Hash ? new TwoItemsLeaf(this, item) : new TwoItemsLeaf(item, this);
            }

            INode INode.LookFor(int _) { return this; }

            public V GetConflictedValueOrDefault(K key, V defaultValue)
            {
                throw new System.NotImplementedException();
            }
        }

        public sealed class OneItemTree : INode
        {
            public NodeType Type { get { return NodeType.OneItemTree; } }
            public readonly OneItemLeaf Item;
            public readonly INode Left, Right;

            public OneItemTree(OneItemLeaf item, INode left, INode right)
            {
                Item = item;
                Left = left;
                Right = right;
            }

            public INode AddOrUpdate(OneItemLeaf item)
            {
                if (item.Hash < Item.Hash)
                {
                    var newLeft = Left.AddOrUpdate(item);
                    if (!IsEmerged(newLeft, Left))
                        return new OneItemTree(Item, newLeft, Right);
                    var emergedLeft = (OneItemTree)newLeft;
                    return new TwoItemsTree(emergedLeft.Item, Item, emergedLeft.Left, emergedLeft.Right, Right);
                    // otherwise just reattach new left
                }
                //else if (item.Hash > LeftItem.Hash)
                {
                    var newRight = Right.AddOrUpdate(item);
                    if (!IsEmerged(newRight, Right))
                        return new OneItemTree(Item, Left, newRight);
                    var emergedRight = (OneItemTree)newRight;
                    return new TwoItemsTree(Item, emergedRight.Item, Left, emergedRight.Left, emergedRight.Right);
                }
            }

            public INode LookFor(int hash)
            {
                return hash == Item.Hash ? Item : hash < Item.Hash ? Left : Right;
            }
        }

        public class TwoItemsLeaf : INode
        {
            public virtual NodeType Type { get { return NodeType.TwoItemsLeaf; } }
            public readonly OneItemLeaf LeftItem, RightItem;

            public TwoItemsLeaf(OneItemLeaf leftItem, OneItemLeaf rightItem)
            {
                LeftItem = leftItem;
                RightItem = rightItem;
            }

            public virtual INode AddOrUpdate(OneItemLeaf item)
            {
                if (item.Hash < LeftItem.Hash)
                    return new OneItemTree(LeftItem, item, RightItem);
                // for now skip if (item.Hash == LeftItem.Hash)
                if (item.Hash < RightItem.Hash)
                    return new OneItemTree(item, LeftItem, RightItem);
                // for now skip if (item.Hash == RightItem.Hash)
                // else if (item.Hash > RightItem.Hash)
                    return new OneItemTree(RightItem, LeftItem, item);
            }

            public virtual INode LookFor(int hash)
            {
                return hash == LeftItem.Hash ? LeftItem : RightItem;
            }
        }

        public sealed class TwoItemsTree : TwoItemsLeaf
        {
            public override NodeType Type { get { return NodeType.TwoItemsTree; }}
            public readonly INode Left, Middle, Right;

            public TwoItemsTree(OneItemLeaf leftItem, OneItemLeaf rightItem, INode left, INode middle, INode right)
                : base(leftItem, rightItem)
            {
                Left = left;
                Middle = middle;
                Right = right;
            }

            public override INode AddOrUpdate(OneItemLeaf item)
            {
                if (item.Hash < LeftItem.Hash)
                {
                    var newLeft = Left.AddOrUpdate(item);
                    return !IsEmerged(newLeft, Left)
                        ? new TwoItemsTree(LeftItem, RightItem, newLeft, Middle, Right)
                        : (INode)new OneItemTree(LeftItem, newLeft, new OneItemTree(RightItem, Middle, Right));
                }
                // if (item.Hash == LeftItem.Hash)
                if (item.Hash < RightItem.Hash)
                {
                    var newMiddle = Middle.AddOrUpdate(item);
                    if (!IsEmerged(newMiddle, Middle))
                        return new TwoItemsTree(LeftItem, RightItem, Left, newMiddle, Right);
                    var emergedMiddle = (OneItemTree)newMiddle;
                    return new OneItemTree(emergedMiddle.Item,
                        new OneItemTree(LeftItem, Left, emergedMiddle.Right),
                        new OneItemTree(RightItem, emergedMiddle.Left, Right));
                }
                // for now skip if (item.Hash == RightItem.Hash)
                // else if (item.Hash > RightItem.Hash)
                {
                    var newRight = Right.AddOrUpdate(item);
                    return !IsEmerged(newRight, Right)
                        ? new TwoItemsTree(LeftItem, RightItem, Left, Middle, newRight)
                        : (INode) new OneItemTree(RightItem, new OneItemTree(LeftItem, Left, Middle), newRight);
                }
            }

            public override INode LookFor(int hash)
            {
                return hash == LeftItem.Hash ? LeftItem 
                    : hash == RightItem.Hash ? RightItem
                    : hash < LeftItem.Hash ? Left
                    : hash > RightItem.Hash ? Right
                    : Middle;
            }
        }

        private static bool IsEmerged(INode newNode, INode oldNode)
        {
            // it is always OneItemTree
            return newNode.Type == NodeType.OneItemTree && newNode.Type != oldNode.Type;
        }
    }
}
