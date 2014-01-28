using System;
using System.Collections.Generic;

namespace DryIoc.Playground
{
    /// <summary>
    /// Immutable AVL-tree (http://en.wikipedia.org/wiki/AVL_tree) with node key of type int.
    /// </summary>
    public sealed class IntTree<V>
    {
        public static readonly IntTree<V> Empty = new IntTree<V>();
        public bool IsEmpty { get { return Height == 0; } }

        public readonly int Key;
        public readonly V Value;

        public readonly int Height;
        public readonly IntTree<V> Left, Right;

        public delegate V UpdateValue(V existing, V added);

        public IntTree<V> AddOrUpdate(int key, V value, UpdateValue updateValue = null)
        {
            return Height == 0 ? new IntTree<V>(key, value, Empty, Empty)
                : (key == Key ? new IntTree<V>(key, updateValue == null ? value : updateValue(Value, value), Left, Right)
                : (key < Key
                    ? With(Left.AddOrUpdate(key, value, updateValue), Right)
                    : With(Left, Right.AddOrUpdate(key, value, updateValue))).EnsureBalanced());
        }

        public V GetValueOrDefault(int key, V defaultValue = default(V))
        {
            var t = this;
            while (t.Height != 0 && t.Key != key)
                t = key < t.Key ? t.Left : t.Right;
            return t.Height != 0 ? t.Value : defaultValue;
        }

        /// <summary>Depth-first in-order traversal as described in http://en.wikipedia.org/wiki/Tree_traversal
        /// The only difference is using fixed size array instead of stack for speed-up (~20% faster than stack).</summary>
        public IEnumerable<IntTree<V>> TraverseInOrder()
        {
            var parents = new IntTree<V>[Height];
            var parentCount = -1;
            var node = this;
            while (!node.IsEmpty || parentCount != -1)
            {
                if (!node.IsEmpty)
                {
                    parents[++parentCount] = node;
                    node = node.Left;
                }
                else
                {
                    node = parents[parentCount--];
                    yield return node;
                    node = node.Right;
                }
            }
        }

        #region Implementation

        private IntTree() { }

        private IntTree(int key, V value, IntTree<V> left, IntTree<V> right)
        {
            Key = key;
            Value = value;
            Left = left;
            Right = right;
            Height = 1 + (left.Height > right.Height ? left.Height : right.Height);
        }

        private IntTree<V> EnsureBalanced()
        {
            var delta = Left.Height - Right.Height;
            return delta >= 2 ? With(Left.Right.Height - Left.Left.Height == 1 ? Left.RotateLeft() : Left, Right).RotateRight()
                : (delta <= -2 ? With(Left, Right.Left.Height - Right.Right.Height == 1 ? Right.RotateRight() : Right).RotateLeft()
                : this);
        }

        private IntTree<V> RotateRight()
        {
            return Left.With(Left.Left, With(Left.Right, Right));
        }

        private IntTree<V> RotateLeft()
        {
            return Right.With(With(Left, Right.Left), Right.Right);
        }

        private IntTree<V> With(IntTree<V> left, IntTree<V> right)
        {
            return new IntTree<V>(Key, Value, left, right);
        }

        #endregion
    }

    public sealed class HashTree<K, V>
    {
        public static readonly HashTree<K, V> Empty = new HashTree<K, V>(IntTree<DryIoc.KV<K, V>>.Empty, null);

        public static HashTree<K, V> Using(Func<V, V, V> updateValue)
        {
            return new HashTree<K, V>(IntTree<DryIoc.KV<K, V>>.Empty, updateValue);
        }

        public HashTree<K, V> AddOrUpdate(K key, V value)
        {
            return new HashTree<K, V>(
                _tree.AddOrUpdate(key.GetHashCode(), new DryIoc.KV<K, V>(key, value), UpdateConflicts), 
                _updateValue);
        }

        public V GetValueOrDefault(K key)
        {
            var item = _tree.GetValueOrDefault(key.GetHashCode());
            return item != null && (ReferenceEquals(key, item.Key) || key.Equals(item.Key)) ? item.Value : GetConflictedOrDefault(item, key);
        }

        #region Implementation

        private HashTree(IntTree<DryIoc.KV<K, V>> tree, Func<V, V, V> updateValue)
        {
            _tree = tree;
            _updateValue = updateValue;
        }

        private readonly IntTree<DryIoc.KV<K, V>> _tree;
        private readonly Func<V, V, V> _updateValue;

        private DryIoc.KV<K, V> UpdateConflicts(DryIoc.KV<K, V> existing, DryIoc.KV<K, V> added)
        {
            var conflicts = existing is KVWithConflicts ? ((KVWithConflicts)existing).Conflicts : null;
            if (ReferenceEquals(existing.Key, added.Key) || existing.Key.Equals(added.Key))
                return conflicts == null ? UpdateValue(existing, added)
                     : new KVWithConflicts(UpdateValue(existing, added), conflicts);

            if (conflicts == null)
                return new KVWithConflicts(existing, new[] { added });

            var i = conflicts.Length - 1;
            while (i >= 0 && !Equals(conflicts[i].Key, added.Key)) --i;
            if (i != -1) added = UpdateValue(existing, added);
            return new KVWithConflicts(existing, conflicts.AppendOrUpdate(added, i));
        }

        private DryIoc.KV<K, V> UpdateValue(DryIoc.KV<K, V> existing, DryIoc.KV<K, V> added)
        {
            return _updateValue == null ? added 
                : new DryIoc.KV<K, V>(existing.Key, _updateValue(existing.Value, added.Value));
        }

        private static V GetConflictedOrDefault(DryIoc.KV<K, V> item, K key)
        {
            var conflicts = item is KVWithConflicts ? ((KVWithConflicts)item).Conflicts : null;
            if (conflicts != null)
                for (var i = 0; i < conflicts.Length; i++)
                    if (Equals(conflicts[i].Key, key))
                        return conflicts[i].Value;
            return default(V);
        }

        private sealed class KVWithConflicts : DryIoc.KV<K, V>
        {
            public readonly DryIoc.KV<K, V>[] Conflicts;

            public KVWithConflicts(DryIoc.KV<K, V> kv, DryIoc.KV<K, V>[] conflicts)
                : base(kv.Key, kv.Value)
            {
                Conflicts = conflicts;
            }
        }

        #endregion
    }
}
