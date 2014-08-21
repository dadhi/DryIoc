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
            Assert.That(root.LeftItem.Key, Is.EqualTo(40));

            var rootLeft = (TwoThreeTree<int, string>.OneItemTree)root.Left;
            Assert.That(rootLeft.LeftItem.Key, Is.EqualTo(20));

            var rootLeftLeft = (TwoThreeTree<int, string>.OneItemLeaf)rootLeft.Left;
            Assert.That(rootLeftLeft.LeftItem.Key, Is.EqualTo(10));

            var rootLeftRight = (TwoThreeTree<int, string>.OneItemLeaf)rootLeft.Right;
            Assert.That(rootLeftRight.LeftItem.Key, Is.EqualTo(30));

            var rootRight = (TwoThreeTree<int, string>.TwoItemsTree)root.Right;
            Assert.That(rootRight.LeftItem.Key, Is.EqualTo(60));
            Assert.That(rootRight.RightItem.Key, Is.EqualTo(80));

            var rootRightLeft = (TwoThreeTree<int, string>.OneItemLeaf)rootRight.Left;
            Assert.That(rootRightLeft.LeftItem.Key, Is.EqualTo(50));

            var rootRightMiddle = (TwoThreeTree<int, string>.OneItemLeaf)rootRight.Middle;
            Assert.That(rootRightMiddle.LeftItem.Key, Is.EqualTo(70));

            var rootRightRight = (TwoThreeTree<int, string>.TwoItemsLeaf)rootRight.Right;
            Assert.That(rootRightRight.LeftItem.Key, Is.EqualTo(90));
            Assert.That(rootRightRight.RightItem.Key, Is.EqualTo(100));
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
            return IsEmpty 
                ? new TwoThreeTree<K, V>(new OneItemLeaf(new Item(key.GetHashCode(), key, value)))
                : new TwoThreeTree<K, V>(Root.AddOrUpdate(new Item(key.GetHashCode(), key, value)));
        }

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

        public enum NodeType { OneItemLeaf, TwoItemsLeaf, OneItemTree, TwoItemsTree }

        public interface INode
        {
            NodeType Type { get; }
            INode AddOrUpdate(Item item);
        }

        public class OneItemLeaf : INode
        {
            public virtual NodeType Type { get { return NodeType.OneItemLeaf; } }
            public readonly Item LeftItem;

            public OneItemLeaf(Item leftItem)
            {
                LeftItem = leftItem;
            }

            public virtual INode AddOrUpdate(Item item)
            {
                return item.Hash > LeftItem.Hash ? new TwoItemsLeaf(LeftItem, item) : new TwoItemsLeaf(item, LeftItem);
            }
        }

        public sealed class OneItemTree : OneItemLeaf
        {
            public override NodeType Type { get { return NodeType.OneItemTree; } }
            public readonly INode Left, Right;

            public OneItemTree(Item item, INode left, INode right)
                : base(item)
            {
                Left = left;
                Right = right;
            }

            public override INode AddOrUpdate(Item item)
            {
                if (item.Hash < LeftItem.Hash)
                {
                    var newLeft = Left.AddOrUpdate(item);
                    if (!IsEmerged(newLeft, Left)) 
                        return new OneItemTree(LeftItem, newLeft, Right);
                    var emergedLeft = (OneItemTree)newLeft;
                    return new TwoItemsTree(emergedLeft.LeftItem, LeftItem, emergedLeft.Left, emergedLeft.Right, Right);
                    // otherwise just reattach new left
                }
                //else if (item.Hash > LeftItem.Hash)
                {
                    var newRight = Right.AddOrUpdate(item);
                    if (!IsEmerged(newRight, Right)) 
                        return new OneItemTree(LeftItem, Left, newRight);
                    var emergedRight = (OneItemTree)newRight;
                    return new TwoItemsTree(LeftItem, emergedRight.LeftItem, Left, emergedRight.Left, emergedRight.Right);
                }
            }
        }

        public class TwoItemsLeaf : OneItemLeaf
        {
            public override NodeType Type { get { return NodeType.TwoItemsLeaf; } }
            public readonly Item RightItem;

            public TwoItemsLeaf(Item leftItem, Item rightItem) : base(leftItem)
            {
                RightItem = rightItem;
            }

            public override INode AddOrUpdate(Item item)
            {
                if (item.Hash < LeftItem.Hash)
                    return new OneItemTree(LeftItem, new OneItemLeaf(item), new OneItemLeaf(RightItem));
                // for now skip if (item.Hash == LeftItem.Hash)
                if (item.Hash < RightItem.Hash)
                    return new OneItemTree(item, new OneItemLeaf(LeftItem), new OneItemLeaf(RightItem));
                // for now skip if (item.Hash == RightItem.Hash)
                // else if (item.Hash > RightItem.Hash)
                    return new OneItemTree(RightItem, new OneItemLeaf(LeftItem), new OneItemLeaf(item));
            }
        }

        public sealed class TwoItemsTree : TwoItemsLeaf
        {
            public override NodeType Type { get { return NodeType.TwoItemsTree; }}
            public readonly INode Left, Middle, Right;

            public TwoItemsTree(Item leftItem, Item rightItem, INode left, INode middle, INode right) : base(leftItem, rightItem)
            {
                Left = left;
                Middle = middle;
                Right = right;
            }

            public override INode AddOrUpdate(Item item)
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
                    return new OneItemTree(emergedMiddle.LeftItem,
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
        }

        private static bool IsEmerged(INode newNode, INode oldNode)
        {
            // it is always OneItemTree
            return newNode.Type == NodeType.OneItemTree && newNode.Type != oldNode.Type;
        }
    }
}
