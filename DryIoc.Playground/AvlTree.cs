using System;
using System.Collections.Generic;

namespace DryIoc.Playground
{
    /// <summary>
    /// Immutable kind of http://en.wikipedia.org/wiki/AVL_tree where actual node key is hash code of <typeparamref name="K"/>.
    /// </summary>
    public sealed class AvlTree<K, V>
    {
        public static readonly AvlTree<K, V> Empty = new AvlTree<K, V>();

        public readonly K Key;
        public readonly V Value;

        public readonly int Hash;
        public readonly KV<K, V>[] Conflicts;
        public readonly AvlTree<K, V> Left, Right;
        public readonly int Height;

        public bool IsEmpty { get { return Height == 0; } }

        public delegate V UpdateValue(V oldValue, V value);

        public AvlTree<K, V> AddOrUpdate(K key, V value, UpdateValue updateValue = null)
        {
            return AddOrUpdate(key.GetHashCode(), key, value, updateValue);
        }

        public V GetFirstValueOfHashOrDefault(K key, V defaultValue = default(V))
        {
            var t = this;
            var hash = key.GetHashCode();
            while (t.Height != 0 && t.Hash != hash)
                t = hash < t.Hash ? t.Left : t.Right;
            return t.Height != 0 && (ReferenceEquals(key, t.Key) || key.Equals(t.Key)) ? t.Value
                : t.GetConflictedValueOrDefault(key, defaultValue);
        }

        public V GetValueOrDefault(int uniqueHash, V defaultValue = default(V))
        {
            var t = this;
            while (t.Height != 0 && t.Hash != uniqueHash)
                t = uniqueHash < t.Hash ? t.Left : t.Right;
            return t.Height != 0 ? t.Value : defaultValue;
        }

        /// <summary>
        /// Depth-first in-order traversal as described in http://en.wikipedia.org/wiki/Tree_traversal
        /// The only difference is using fixed size array instead of stack for speed-up (~20% faster than stack).
        /// </summary>
        public IEnumerable<KV<K, V>> Enumerate()
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

        /// <summary>
        /// Based on Eric Lippert's http://blogs.msdn.com/b/ericlippert/archive/2008/01/21/immutability-in-c-part-nine-academic-plus-my-avl-tree-implementation.aspx
        /// </summary>
        public AvlTree<K, V> Remove(K key)
        {
            return Remove(key.GetHashCode(), key);
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

        private AvlTree<K, V> AddOrUpdate(int hash, K key, V value, UpdateValue updateValue)
        {
            return Height == 0 ? new AvlTree<K, V>(hash, key, value, null, Empty, Empty)
                : (hash == Hash ? UpdateValueAndResolveConflicts(key, value, updateValue)
                : (hash < Hash
                    ? With(Left.AddOrUpdate(hash, key, value, updateValue), Right)
                    : With(Left, Right.AddOrUpdate(hash, key, value, updateValue)))
                        .KeepBalanced());
        }

        private AvlTree<K, V> UpdateValueAndResolveConflicts(K key, V value, UpdateValue updateValue)
        {
            if (ReferenceEquals(Key, key) || Key.Equals(key))
                return new AvlTree<K, V>(Hash, key, updateValue == null ? value : updateValue(Value, value), Conflicts, Left, Right);

            if (Conflicts == null)
                return new AvlTree<K, V>(Hash, Key, Value, new[] { new KV<K, V>(key, value) }, Left, Right);

            var i = Conflicts.Length - 1;
            while (i >= 0 && !Equals(Conflicts[i].Key, Key)) --i;
            var conflicts = new KV<K, V>[i != -1 ? Conflicts.Length : Conflicts.Length + 1];
            Array.Copy(Conflicts, 0, conflicts, 0, Conflicts.Length);
            conflicts[i != -1 ? i : Conflicts.Length] =
                new KV<K, V>(key, i != -1 && updateValue != null ? updateValue(Conflicts[i].Value, value) : value);
            return new AvlTree<K, V>(Hash, Key, Value, conflicts, Left, Right);
        }

        private V GetConflictedValueOrDefault(K key, V defaultValue)
        {
            if (Conflicts != null)
                for (var i = 0; i < Conflicts.Length; i++)
                    if (Equals(Conflicts[i].Key, key))
                        return Conflicts[i].Value;
            return defaultValue;
        }

        private AvlTree<K, V> Remove(int hash, K key)
        {
            AvlTree<K, V> result;
            if (hash == Hash)
            {
                if ((ReferenceEquals(key, Key) || Equals(key, Key)) && Conflicts == null)
                {
                    // We have a match. If this is a leaf, just remove it by returning Empty.  
                    // If we have only one child, replace the node with the child.
                    if (Height == 1) // leaf
                        result = Empty;
                    else if (Right.IsEmpty)
                        result = Left;
                    else if (Left.IsEmpty)
                        result = Right;
                    else
                    {
                        // We have two children. Remove the next-highest node and replace this node with it.
                        var successor = Right;
                        while (!successor.Left.IsEmpty) successor = successor.Left;
                        result = successor.With(Left, Right.Remove(successor.Hash, key));
                    }
                }
                else if (Conflicts == null) // Means that keys are different and no conflicts - do not remove - just return current node as is.
                {
                    return this;
                }
                else if (Equals(key, Key))
                {
                    if (Conflicts.Length == 1)
                        return new AvlTree<K, V>(Hash, Conflicts[0].Key, Conflicts[0].Value, null, Left, Right);

                    var newConflicts = new KV<K, V>[Conflicts.Length - 1];
                    Array.Copy(Conflicts, 1, newConflicts, 0, newConflicts.Length);
                    return new AvlTree<K, V>(Hash, Conflicts[0].Key, Conflicts[0].Value, newConflicts, Left, Right);
                }
                else
                {
                    var index = Conflicts.Length - 1;
                    while (index >= 0 && !Equals(Conflicts[index].Key, Key)) --index;
                    if (index == -1)
                        return this; // key is not found in Conflicts - return node as is.

                    if (Conflicts.Length == 1)
                        return new AvlTree<K, V>(Hash, Key, Value, null, Left, Right);

                    var newConflicts = new KV<K, V>[Conflicts.Length - 1];
                    var newIndex = 0;
                    for (var i = 0; i < Conflicts.Length; ++i)
                        if (i != index) newConflicts[newIndex++] = Conflicts[i];
                    return new AvlTree<K, V>(Hash, Key, Value, newConflicts, Left, Right);
                }
            }
            else if (hash < Hash)
                result = With(Left.Remove(hash, key), Right);
            else
                result = With(Left, Right.Remove(hash, key));
            return result.KeepBalanced();
        }

        private AvlTree<K, V> KeepBalanced()
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