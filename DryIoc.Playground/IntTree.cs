using System.Collections.Generic;

namespace DryIoc.Playground
{
    /// <summary>
    /// Immutable AVL-tree (http://en.wikipedia.org/wiki/AVL_tree) with key of type int.
    /// </summary>
    public sealed class IntTree<V>
    {
        public static readonly IntTree<V> Empty = new IntTree<V>();
        public bool IsEmpty { get { return Height == 0; } }

        public readonly int Key;
        public readonly V Value;

        public readonly int Height;
        public readonly IntTree<V> Left, Right;

        public IntTree<V> AddOrUpdate(int key, V value, UpdateMethod<V> updateValue = null)
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
        public IEnumerable<IntTree<V>> Enumerate()
        {
            if (Height == 0) yield break;
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

    public sealed class IntHashTree<K, V>
    {
        public static readonly IntHashTree<K, V> Empty = new IntHashTree<K, V>(IntTree<KV<K, V>>.Empty);
        public bool IsEmpty { get { return _root.IsEmpty; } }

        public IntHashTree<K, V> AddOrUpdate(K key, V value, UpdateMethod<V> updateValue = null)
        {
            return new IntHashTree<K, V>(
                _root.AddOrUpdate(key.GetHashCode(), new KV<K, V>(key, value), UpdateValueWithRespectToConflicts(updateValue)));
        }

        public V GetValueOrDefault(K key, V defaultValue = default(V))
        {
            var kv = _root.GetValueOrDefault(key.GetHashCode());
            return kv != null && (ReferenceEquals(key, kv.Key) || key.Equals(kv.Key))
                ? kv.Value : GetConflictedValueOrDefault(kv, key, defaultValue);
        }

        public IEnumerable<KV<K, V>> Enumerate()
        {
            if (!_root.IsEmpty)
                foreach (var t in _root.Enumerate())
                {
                    yield return t.Value;
                    if (t.Value is KVWithConflicts)
                    {
                        var conflicts = ((KVWithConflicts)t.Value).Conflicts;
                        for (var i = 0; i < conflicts.Length; ++i)
                            yield return conflicts[i];
                    }
                }
        }

        #region Implementation

        private readonly IntTree<KV<K, V>> _root;

        private IntHashTree(IntTree<KV<K, V>> root)
        {
            _root = root;
        }

        private static UpdateMethod<KV<K, V>> UpdateValueWithRespectToConflicts(UpdateMethod<V> updateValue)
        {
            return (old, newOne) =>
            {
                var conflicts = old is KVWithConflicts ? ((KVWithConflicts)old).Conflicts : null;
                if (ReferenceEquals(old.Key, newOne.Key) || old.Key.Equals(newOne.Key))
                    return conflicts == null ? UpdateValue(old, newOne, updateValue)
                         : new KVWithConflicts(UpdateValue(old, newOne, updateValue), conflicts);

                if (conflicts == null)
                    return new KVWithConflicts(old, new[] { newOne });

                var i = conflicts.Length - 1;
                while (i >= 0 && !Equals(conflicts[i].Key, newOne.Key)) --i;
                if (i != -1) newOne = UpdateValue(old, newOne, updateValue);
                return new KVWithConflicts(old, conflicts.AppendOrUpdate(newOne, i));
            };
        }

        private static KV<K, V> UpdateValue(KV<K, V> old, KV<K, V> newOne, UpdateMethod<V> updateValue)
        {
            return updateValue == null ? newOne : new KV<K, V>(old.Key, updateValue(old.Value, newOne.Value));
        }

        private static V GetConflictedValueOrDefault(KV<K, V> item, K key, V defaultValue)
        {
            var conflicts = item is KVWithConflicts ? ((KVWithConflicts)item).Conflicts : null;
            if (conflicts != null)
                for (var i = 0; i < conflicts.Length; ++i)
                    if (Equals(conflicts[i].Key, key))
                        return conflicts[i].Value;
            return defaultValue;
        }

        private sealed class KVWithConflicts : KV<K, V>
        {
            public readonly KV<K, V>[] Conflicts;

            public KVWithConflicts(KV<K, V> kv, KV<K, V>[] conflicts)
                : base(kv.Key, kv.Value)
            {
                Conflicts = conflicts;
            }
        }

        #endregion
    }
}
