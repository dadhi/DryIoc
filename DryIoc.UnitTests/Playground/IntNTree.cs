using System;

namespace DryIoc.UnitTests.Playground
{
    public sealed class IntNTree<V>
    {
        public static int N = 3;

        public static readonly IntNTree<V> Empty = new IntNTree<V>();

        public readonly IntNTree<V> Left, Right;
        public readonly int Height;

        public delegate V UpdateValue(V existing, V added);

        public V GetValueOrDefault(int key, V defaultValue = default(V))
        {
            var node = this;
            while (node.Height != 0)
                if (key < node.LeftKey)
                    node = node.Left;
                else if (key > node.RightKey)
                    node = node.Right;
                else
                {
                    var items = node.Items;
                    if (key == node.LeftKey)
                        return items[0].Value;
                    
                    if (key == node.RightKey)
                        return items[items.Length - 1].Value;
                    
                    if (items.Length > 2)
                        for (var i = 1; i < items.Length - 1; i++)
                            if (key == items[i].Key)
                                return items[i].Value;
                    break;
                }
           
            return defaultValue;
        }

        public IntNTree<V> AddOrUpdate(int key, V value, UpdateValue updateValue = null)
        {
            return AddOrUpdate(new KV<int, V>(key, value), updateValue);
        }

        internal readonly KV<int, V>[] Items;
        internal readonly int LeftKey;
        internal readonly int RightKey;

        private IntNTree() { }

        private IntNTree(KV<int, V>[] items, IntNTree<V> left, IntNTree<V> right)
        {
            Items = items;
            LeftKey = Items[0].Key;
            RightKey = Items[Items.Length - 1].Key;

            Left = left;
            Right = right;
            Height = 1 + (left.Height > right.Height ? left.Height : right.Height);
        }

        private IntNTree<V> AddOrUpdate(KV<int, V> item, UpdateValue updateValue)
        {
            if (Height == 0)
                return new IntNTree<V>(new[] { item }, Empty, Empty);

            if (item.Key < LeftKey)
            {
                if (Items.Length < N)
                {
                    var newItems = new KV<int, V>[Items.Length + 1];
                    newItems[0] = item;
                    Array.Copy(Items, 0, newItems, 1, Items.Length);
                    return new IntNTree<V>(newItems, Left, Right);
                }

                return With(Left.AddOrUpdate(item, updateValue), Right).EnsureBalanced();
            }

            if (item.Key > RightKey)
            {
                if (Items.Length < N)
                {
                    var newItems = new KV<int, V>[Items.Length + 1];
                    Array.Copy(Items, 0, newItems, 0, Items.Length);
                    newItems[Items.Length] = item;
                    return new IntNTree<V>(newItems, Left, Right);
                }

                return With(Left, Right.AddOrUpdate(item, updateValue)).EnsureBalanced();
            }

            var i = 0;
            while (item.Key > Items[i].Key) i++;

            if (item.Key == Items[i].Key) // Update value
            {
                var newItems = new KV<int, V>[Items.Length];
                Array.Copy(Items, 0, newItems, 0, Items.Length);
                newItems[i] = updateValue == null ? item : new KV<int, V>(item.Key, updateValue(Items[i].Value, item.Value));
                return new IntNTree<V>(newItems, Left, Right);
            }

            if (Items.Length < N) // Insert value into current node
            {
                var newItems = new KV<int, V>[Items.Length + 1];
                Array.Copy(Items, 0, newItems, 0, i);
                newItems[i] = item;
                Array.Copy(Items, i, newItems, i + 1, Items.Length - i);
                return new IntNTree<V>(newItems, Left, Right);
            }

            var items = new KV<int, V>[Items.Length];

            // Drop left item to the left node if it has room for insert, otherwise drop right item to right node.
            if (Left.Height == 0 || Left.Items.Length < N)
            {
                // values: drop 1, 2, insert 3, 4, 6, 9
                // indexes:     0, 1, insert 1, 2, 3, 4 
                // i == 2
                Array.Copy(Items, 1, items, 0, i - 1);
                items[i - 1] = item;
                Array.Copy(Items, i, items, i, Items.Length - i);
                return new IntNTree<V>(items, Left.AddOrUpdate(Items[0], updateValue), Right).EnsureBalanced();
            }

            // values:  copy 1, 2, insert 3, copy 4, 6, drop 9
            // indexes: copy 0, 1, insert 1, copy 2, 3, drop 4 
            // i == 2
            Array.Copy(Items, 0, items, 0, i);
            items[i] = item;
            Array.Copy(Items, i, items, i + 1, Items.Length - i - 1);
            return new IntNTree<V>(items, Left, Right.AddOrUpdate(Items[Items.Length - 1], updateValue)).EnsureBalanced();
        }

        private IntNTree<V> With(IntNTree<V> left, IntNTree<V> right)
        {
            return new IntNTree<V>(Items, left, right);
        }

        private IntNTree<V> EnsureBalanced()
        {
            var delta = Left.Height - Right.Height;
            return delta >= 2 ? With(Left.Right.Height - Left.Left.Height == 1 ? Left.RotateLeft() : Left, Right).RotateRight()
                : (delta <= -2 ? With(Left, Right.Left.Height - Right.Right.Height == 1 ? Right.RotateRight() : Right).RotateLeft()
                : this);
        }

        private IntNTree<V> RotateRight()
        {
            return Left.With(Left.Left, With(Left.Right, Right));
        }

        private IntNTree<V> RotateLeft()
        {
            return Right.With(With(Left, Right.Left), Right.Right);
        }
    }
}