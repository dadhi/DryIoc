using System;

namespace DryIoc.UnitTests.Playground
{
    public sealed class IntNTree<V>
    {
        public const int N = 5;

        public static readonly IntNTree<V> Empty = new IntNTree<V>();

        public readonly IntNTree<V> Left, Right;
        public readonly int Height;

        public delegate V UpdateValue(V existing, V added);

        public V GetValueOrDefault(int key, V defaultValue = default(V))
        {
            var node = this;
            while (node.Height != 0)
                if (key < _leftKey)
                    node = node.Left;
                else if (key > _rightKey)
                    node = node.Right;
                else
                    return node.GetItemValueOrDefault(key, defaultValue);
            return defaultValue;
        }

        public IntNTree<V> AddOrUpdate(int key, V value, UpdateValue updateValue = null)
        {
            var newItem = new KV<int, V>(key, value);
            return Height == 0 ? new IntNTree<V>(new[] { newItem }, Empty, Empty) : AddOrUpdate(newItem, updateValue);
        }

        private readonly KV<int, V>[] _items;
        private readonly int _leftKey;
        private readonly int _rightKey;

        private IntNTree() { }

        private IntNTree(KV<int, V>[] items, IntNTree<V> left, IntNTree<V> right)
        {
            _items = items;
            _leftKey = _items[0].Key;
            _rightKey = _items[_items.Length - 1].Key;

            Left = left;
            Right = right;
            Height = 1 + (left.Height > right.Height ? left.Height : right.Height);
        }

        internal V GetItemValueOrDefault(int key, V defaultValue = default(V))
        {
            var items = _items;
            for (var i = 0; i < items.Length; i++)
                if (key == items[i].Key)
                    return items[i].Value;
            return defaultValue;
        }

        private IntNTree<V> AddOrUpdate(KV<int, V> item, UpdateValue updateValue)
        {
            if (item.Key < _leftKey)
            {
                if (_items.Length < N)
                {
                    var newItems = new KV<int, V>[_items.Length + 1];
                    newItems[0] = item;
                    Array.Copy(_items, 0, newItems, 1, _items.Length);
                    return new IntNTree<V>(newItems, Left, Right);
                }

                return With(Left.AddOrUpdate(item, updateValue), Right).EnsureBalanced();
            }

            if (item.Key > _rightKey)
            {
                if (_items.Length < N)
                {
                    var newItems = new KV<int, V>[_items.Length + 1];
                    Array.Copy(_items, 0, newItems, 0, _items.Length);
                    newItems[_items.Length] = item;
                    return new IntNTree<V>(newItems, Left, Right);
                }

                return With(Left, Right.AddOrUpdate(item, updateValue)).EnsureBalanced();
            }

            var i = 0;
            while (item.Key > _items[i].Key) i++;

            if (item.Key == _items[i].Key) // Update value
            {
                var newItems = new KV<int, V>[_items.Length];
                Array.Copy(_items, 0, newItems, 0, _items.Length);
                newItems[i] = updateValue == null ? item : new KV<int, V>(item.Key, updateValue(_items[i].Value, item.Value));
                return new IntNTree<V>(newItems, Left, Right);
            }

            if (_items.Length < N) // Insert value into current node
            {
                var newItems = new KV<int, V>[_items.Length + 1];
                Array.Copy(_items, 0, newItems, 0, i);
                newItems[i] = item;
                Array.Copy(_items, i, newItems, i + 1, _items.Length - i);
                return new IntNTree<V>(newItems, Left, Right);
            }

            var items = new KV<int, V>[_items.Length];

            // Drop left item to the left node if it has room for insert, otherwise drop right item to right node.
            if (Left._items.Length < N)
            {
                // values: drop 1, 2, insert 3, 4, 6, 9
                // indexes:     0, 1, insert 1, 2, 3, 4 
                // i == 2
                Array.Copy(_items, 1, items, 0, i - 1);
                items[i - 1] = item;
                Array.Copy(_items, i, items, i, _items.Length - i);
                return new IntNTree<V>(items, Left.AddOrUpdate(_items[0], updateValue), Right);
            }

            // values:  copy 1, 2, insert 3, copy 4, 6, drop 9
            // indexes: copy 0, 1, insert 1, copy 2, 3, drop 4 
            // i == 2
            Array.Copy(_items, 0, items, 0, i);
            items[i] = item;
            Array.Copy(_items, i, items, i + 1, _items.Length - i - 1);
            return new IntNTree<V>(items, Left, Right.AddOrUpdate(_items[0], updateValue))
                .EnsureBalanced(); // Ensure that is balanced cause we are not checking for Right room
        }

        private IntNTree<V> With(IntNTree<V> left, IntNTree<V> right)
        {
            return new IntNTree<V>(_items, left, right);
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