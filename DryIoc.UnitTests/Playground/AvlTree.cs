using System;
using System.Collections.Generic;

namespace DryIoc.UnitTests.Playground
{
    public sealed class AvlTree<K, V>
    {
        public static readonly AvlTree<K, V> Empty = new AvlTree<K, V>();
        public bool IsEmpty { get { return Height == 0; } }

        public readonly int Hash;
        public readonly K Key;
        public readonly V Value;
        public readonly KV<K, V>[] Conflicts;
        public readonly int Height;
        public readonly AvlTree<K, V> Left, Right;

        public delegate V UpdateValue(V current, V added);

        public AvlTree<K, V> AddOrUpdate(K key, V value, UpdateValue updateValue = null)
        {
            return AddOrUpdate(key.GetHashCode(), key, value, updateValue ?? ReplaceValue);
        }

        public V GetValueOrDefault(K key, V defaultValue = default(V))
        {
            var hash = key.GetHashCode();
            for (var t = this; t.Height != 0; t = hash < t.Hash ? t.Left : t.Right)
                if (hash == t.Hash)
                    return ReferenceEquals(key, t.Key) || key.Equals(t.Key) ? t.Value : t.GetConflictedOrDefault(key, defaultValue);
            return defaultValue;
        }

        /// <summary>Depth-first in-order traversal as described in http://en.wikipedia.org/wiki/Tree_traversal
        /// The only difference is using fixed size array instead of stack for speed-up (~20% faster than stack).</summary>
        public IEnumerable<KV<K, V>> TraverseInOrder()
        {
            var parents = new AvlTree<K, V>[Height];
            var parentCount = -1;
            var t = this;
            while (!t.IsEmpty || parentCount != -1)
            {
                if (!t.IsEmpty)
                {
                    parents[++parentCount] = t;
                    t = t.Left;
                }
                else
                {
                    t = parents[parentCount--];
                    yield return new KV<K, V>(t.Key, t.Value);
                    if (t.Conflicts != null)
                        for (var i = 0; i < t.Conflicts.Length; i++)
                            yield return t.Conflicts[i];
                    t = t.Right;
                }
            }
        }

        #region Implementation

        private AvlTree() { }

        private AvlTree(int hash, K key, V value, KV<K, V>[] conficts, AvlTree<K, V> left, AvlTree<K, V> right)
        {
            Hash = hash;
            Key = key;
            Value = value;
            Conflicts = conficts;
            Left = left;
            Right = right;
            Height = 1 + (left.Height > right.Height ? left.Height : right.Height);
        }

        private static V ReplaceValue(V _, V added)
        {
            return added;
        }

        private AvlTree<K, V> AddOrUpdate(int hash, K key, V value, UpdateValue updateValue)
        {
            return Height == 0 ? new AvlTree<K, V>(hash, key, value, null, Empty, Empty)
                : (hash == Hash ? ResolveConflicts(key, value, updateValue)
                : (hash < Hash
                    ? With(Left.AddOrUpdate(hash, key, value, updateValue), Right)
                    : With(Left, Right.AddOrUpdate(hash, key, value, updateValue)))
                        .EnsureBalanced());
        }

        private V GetConflictedOrDefault(K key, V defaultValue)
        {
            if (Conflicts != null)
                for (var i = 0; i < Conflicts.Length; i++)
                    if (Equals(Conflicts[i].Key, key))
                        return Conflicts[i].Value;
            return defaultValue;
        }

        private AvlTree<K, V> ResolveConflicts(K key, V value, UpdateValue updateValue)
        {
            if (ReferenceEquals(Key, key) || Key.Equals(key))
                return new AvlTree<K, V>(Hash, key, updateValue(Value, value), Conflicts, Left, Right);

            if (Conflicts == null)
                return new AvlTree<K, V>(Hash, Key, Value, new[] { new KV<K, V>(key, value) }, Left, Right);

            var i = Conflicts.Length - 1;
            while (i >= 0 && !Equals(Conflicts[i].Key, Key)) i--;
            var conflicts = new KV<K, V>[i != -1 ? Conflicts.Length : Conflicts.Length + 1];
            Array.Copy(Conflicts, 0, conflicts, 0, Conflicts.Length);
            conflicts[i != -1 ? i : Conflicts.Length] = new KV<K, V>(key, i != -1 ? updateValue(Conflicts[i].Value, value) : value);
            return new AvlTree<K, V>(Hash, Key, Value, conflicts, Left, Right);
        }

        private AvlTree<K, V> EnsureBalanced()
        {
            var delta = Left.Height - Right.Height;
            return delta >= 2 ? With(Left.Right.Height - Left.Left.Height == 1 ? Left.RotateLeft() : Left, Right).RotateRight()
                : (delta <= -2 ? With(Left, Right.Left.Height - Right.Right.Height == 1 ? Right.RotateRight() : Right).RotateLeft()
                : this);
        }

        private AvlTree<K, V> RotateRight()
        {
            return Left.With(Left.Left, With(Left.Right, Right));
        }

        private AvlTree<K, V> RotateLeft()
        {
            return Right.With(With(Left, Right.Left), Right.Right);
        }

        private AvlTree<K, V> With(AvlTree<K, V> left, AvlTree<K, V> right)
        {
            return new AvlTree<K, V>(Hash, Key, Value, Conflicts, left, right);
        }

        #endregion
    }
}