using System;
using System.Collections;
using System.Collections.Generic;

namespace DryIoc.Playground
{
    /// <summary>Immutable http://en.wikipedia.org/wiki/AVL_tree where actual node key is hash code of <typeparamref name="K"/>.</summary>
    public sealed class HashTree<K, V>
    {
        /// <summary>Empty tree to start with. The <see cref="Height"/> of the empty tree is 0.</summary>
        public static readonly HashTree<K, V> Empty = new HashTree<K, V>();

        /// <summary>Key of type K that should support <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode"/>.</summary>
        public readonly K Key;

        /// <summary>Value of any type V.</summary>
        public readonly V Value;

        /// <summary>Hash calculated from <see cref="Key"/> with <see cref="object.GetHashCode"/>. Hash is stored to improve speed.</summary>
        public readonly int Hash;

        /// <summary>In case of <see cref="Hash"/> conflicts for different keys contains conflicted keys with their values.</summary>
        public readonly KV<K, V>[] Conflicts;

        /// <summary>Left subtree/branch, or empty.</summary>
        public readonly HashTree<K, V> Left;

        /// <summary>Right subtree/branch, or empty.</summary>
        public readonly HashTree<K, V> Right;

        /// <summary>Height of longest subtree/branch. It is 0 for empty tree, and 1 for single node tree.</summary>
        public readonly int Height;

        /// <summary>Returns true is tree is empty.</summary>
        public bool IsEmpty { get { return Height == 0; } }

        /// <summary>Returns new tree with added key-value. If value with the same key is exist, then
        /// if <paramref name="update"/> is not specified: then existing value will be replaced by <paramref name="value"/>;
        /// if <paramref name="update"/> is specified: then update delegate will decide what value to keep.</summary>
        /// <param name="key">Key to add.</param><param name="value">Value to add.</param>
        /// <param name="update">(optional) Delegate to decide what value to keep: old or new one.</param>
        /// <returns>New tree with added or updated key-value.</returns>
        public HashTree<K, V> AddOrUpdate(K key, V value, Update<V> update = null)
        {
            return AddOrUpdate(key.GetHashCode(), key, value, update, false);
        }

        public HashTree<K, V> Update(K key, V value, Update<V> update = null)
        {
            return AddOrUpdate(key.GetHashCode(), key, value, update, true);
        }

        /// <summary>Searches for key in tree and returns the value if found, or <paramref name="defaultValue"/> otherwise.</summary>
        /// <param name="key">Key to look for.</param> <param name="defaultValue">Value to return if key is not found.</param>
        /// <returns>Found value or <paramref name="defaultValue"/>.</returns>
        public V GetValueOrDefault(K key, V defaultValue = default(V))
        {
            var t = this;
            var hash = key.GetHashCode();
            while (t.Height != 0 && t.Hash != hash)
                t = hash < t.Hash ? t.Left : t.Right;
            return t.Height != 0 && (ReferenceEquals(key, t.Key) || key.Equals(t.Key))
                ? t.Value : t.GetConflictedValueOrDefault(key, defaultValue);
        }

        /// <summary>Searches by hash directly instead of key, and return last value added for key corresponding to the hash, 
        /// or <paramref name="defaultValue"/>. In a tree single hash code could have multiple (conflicted) keys, here the rest of
        /// conflicted key values are ignored.</summary>
        /// <param name="hash">Hash to look for.</param> <param name="defaultValue">Value to return if hash is not found.</param>
        /// <returns>Found value for unique key, or for last added conflicted key.</returns>
        /// <remarks>Use the method if you know that hash is truly unique, it will perform faster than <see cref="GetValueOrDefault"/>.</remarks>
        public V GetFirstValueByHashOrDefault(int hash, V defaultValue = default(V))
        {
            var t = this;
            while (t.Height != 0 && t.Hash != hash)
                t = hash < t.Hash ? t.Left : t.Right;
            return t.Height != 0 ? t.Value : defaultValue;
        }

        /// <summary>Depth-first in-order traversal as described in http://en.wikipedia.org/wiki/Tree_traversal
        /// The only difference is using fixed size array instead of stack for speed-up (~20% faster than stack).</summary>
        /// <returns>Sequence of enumerated key value pairs.</returns>
        public IEnumerable<KV<K, V>> Enumerate()
        {
            if (Height == 0)
                yield break;

            var parents = new HashTree<K, V>[Height];

            var tree = this;
            var parentCount = -1;
            while (tree.Height != 0 || parentCount != -1)
            {
                if (tree.Height != 0)
                {
                    parents[++parentCount] = tree;
                    tree = tree.Left;
                }
                else
                {
                    tree = parents[parentCount--];
                    yield return new KV<K, V>(tree.Key, tree.Value);

                    if (tree.Conflicts != null)
                        for (var i = 0; i < tree.Conflicts.Length; i++)
                            yield return tree.Conflicts[i];

                    tree = tree.Right;
                }
            }
        }

        // TODO: Review new enumerator
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public struct Enumerator : IEnumerator<KV<K, V>>
        {
            private readonly HashTree<K, V> _tree;
            private HashTree<K, V>[] _parents;
            private int _parentCount;
            private HashTree<K, V> _t;
            private KV<K, V> _current;
            private int _currentConflictIndex;
            private KV<K, V>[] _conflicts;

            public Enumerator(HashTree<K, V> tree)
            {
                _tree = tree;
                _parents = new HashTree<K, V>[_tree.Height];
                _parentCount = -1;
                _currentConflictIndex = 0;
                _t = _tree;
                _current = default(KV<K, V>);
                _conflicts = null;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (_conflicts != null)
                {
                    if (_currentConflictIndex < _conflicts.Length)
                    {
                        _current = _conflicts[_currentConflictIndex];
                        _currentConflictIndex++;
                        return true;
                    }
                }


                while (!_t.IsEmpty || _parentCount != -1)
                {
                    if (!_t.IsEmpty)
                    {
                        _parents[++_parentCount] = _t;
                        _t = _t.Left;
                    }
                    else
                    {
                        _t = _parents[_parentCount--];
                        _current = new KV<K, V>(_t.Key, _t.Value);
                        _conflicts = _t.Conflicts;
                        _currentConflictIndex = 0;
                        _t = _t.Right;
                        return true;
                    }
                }

                return false;
            }

            public void Reset()
            {
                _parents = new HashTree<K, V>[_tree.Height];
                _parentCount = -1;
                _currentConflictIndex = 0;
                _t = _tree;
                _current = default(KV<K, V>);
                _conflicts = null;
            }

            public KV<K, V> Current
            {
                get { return _current; }
            }

            object IEnumerator.Current
            {
                get { return _current; }
            }
        }

        /// <summary>Removes or updates value for specified key, or does nothing if key is not found.
        /// Based on Eric Lippert http://blogs.msdn.com/b/ericlippert/archive/2008/01/21/immutability-in-c-part-nine-academic-plus-my-avl-tree-implementation.aspx </summary>
        /// <param name="key">Key to look for.</param> <param name="isUpdated">(optional) Delegate to update value, return true from delegate if value is updated.</param>
        /// <returns>New tree with removed or updated value.</returns>
        public HashTree<K, V> RemoveOrUpdate(K key, IsUpdated<V> isUpdated = null)
        {
            return RemoveOrUpdate(key.GetHashCode(), key, isUpdated);
        }

        #region Implementation

        private HashTree() { }

        private HashTree(int hash, K key, V value, KV<K, V>[] conficts, HashTree<K, V> left, HashTree<K, V> right)
        {
            Hash = hash;
            Key = key;
            Value = value;
            Conflicts = conficts;
            Left = left;
            Right = right;
            Height = 1 + (left.Height > right.Height ? left.Height : right.Height);
        }

        private HashTree<K, V> AddOrUpdate(int hash, K key, V value, Update<V> update, bool updateOnly)
        {
            return Height == 0 ? (updateOnly ? this : new HashTree<K, V>(hash, key, value, null, Empty, Empty))
                : (hash == Hash ? UpdateValueAndResolveConflicts(key, value, update, updateOnly)
                : (hash < Hash
                    ? With(Left.AddOrUpdate(hash, key, value, update, updateOnly), Right)
                    : With(Left, Right.AddOrUpdate(hash, key, value, update, updateOnly))).KeepBalanced());
        }

        private HashTree<K, V> UpdateValueAndResolveConflicts(K key, V value, Update<V> update, bool updateOnly)
        {
            if (ReferenceEquals(Key, key) || Key.Equals(key))
                return new HashTree<K, V>(Hash, key, update == null ? value : update(Value, value), Conflicts, Left, Right);

            if (Conflicts == null) // add only if updateOnly is false.
                return updateOnly ? this
                    : new HashTree<K, V>(Hash, Key, Value, new[] { new KV<K, V>(key, value) }, Left, Right);

            var found = Conflicts.Length - 1;
            while (found >= 0 && !Equals(Conflicts[found].Key, Key)) --found;
            if (found == -1)
            {
                if (updateOnly) return this;
                var newConflicts = new KV<K, V>[Conflicts.Length + 1];
                Array.Copy(Conflicts, 0, newConflicts, 0, Conflicts.Length);
                newConflicts[Conflicts.Length] = new KV<K, V>(key, value);
                return new HashTree<K, V>(Hash, Key, Value, newConflicts, Left, Right);
            }

            var conflicts = new KV<K, V>[Conflicts.Length];
            Array.Copy(Conflicts, 0, conflicts, 0, Conflicts.Length);
            conflicts[found] = new KV<K, V>(key, update == null ? value : update(Conflicts[found].Value, value));
            return new HashTree<K, V>(Hash, Key, Value, conflicts, Left, Right);
        }

        private V GetConflictedValueOrDefault(K key, V defaultValue)
        {
            if (Conflicts != null)
                for (var i = 0; i < Conflicts.Length; i++)
                    if (Equals(Conflicts[i].Key, key))
                        return Conflicts[i].Value;
            return defaultValue;
        }

        private HashTree<K, V> RemoveOrUpdate(int hash, K key, IsUpdated<V> isUpdated = null, bool ignoreKey = false)
        {
            if (Height == 0)
                return this;

            HashTree<K, V> result;
            if (hash == Hash) // found matched Node
            {
                if (ignoreKey || Equals(Key, key))
                {
                    if (!ignoreKey)
                    {
                        V updatedValue;
                        if (isUpdated != null && isUpdated(Value, out updatedValue))
                            return new HashTree<K, V>(Hash, Key, updatedValue, Conflicts, Left, Right);

                        if (Conflicts != null)
                        {
                            if (Conflicts.Length == 1)
                                return new HashTree<K, V>(Hash, Conflicts[0].Key, Conflicts[0].Value, null, Left, Right);
                            var shrinkedConflicts = new KV<K, V>[Conflicts.Length - 1];
                            Array.Copy(Conflicts, 1, shrinkedConflicts, 0, shrinkedConflicts.Length);
                            return new HashTree<K, V>(Hash, Conflicts[0].Key, Conflicts[0].Value, shrinkedConflicts, Left, Right);
                        }
                    }

                    if (Height == 1) // remove node
                        return Empty;

                    if (Right.IsEmpty)
                        result = Left;
                    else if (Left.IsEmpty)
                        result = Right;
                    else
                    {
                        // we have two children, so remove the next highest node and replace this node with it.
                        var successor = Right;
                        while (!successor.Left.IsEmpty) successor = successor.Left;
                        result = successor.With(Left, Right.RemoveOrUpdate(successor.Hash, default(K), ignoreKey: true));
                    }
                }
                else if (Conflicts != null)
                {
                    var index = Conflicts.Length - 1;
                    while (index >= 0 && !Equals(Conflicts[index].Key, key)) --index;
                    if (index == -1)        // key is not found in conflicts - just return
                        return this;

                    V updatedValue;
                    var conflict = Conflicts[index];
                    if (isUpdated != null && isUpdated(conflict.Value, out updatedValue))
                    {
                        var updatedConflicts = new KV<K, V>[Conflicts.Length];
                        Array.Copy(Conflicts, 0, updatedConflicts, 0, updatedConflicts.Length);
                        updatedConflicts[index] = new KV<K, V>(conflict.Key, updatedValue);
                        return new HashTree<K, V>(Hash, Key, Value, updatedConflicts, Left, Right);
                    }

                    if (Conflicts.Length == 1)
                        return new HashTree<K, V>(Hash, Key, Value, null, Left, Right);
                    var shrinkedConflicts = new KV<K, V>[Conflicts.Length - 1];
                    var newIndex = 0;
                    for (var i = 0; i < Conflicts.Length; ++i)
                        if (i != index) shrinkedConflicts[newIndex++] = Conflicts[i];
                    return new HashTree<K, V>(Hash, Key, Value, shrinkedConflicts, Left, Right);
                }
                else return this; // if key is not matching and no conflicts to lookup - just return
            }
            else if (hash < Hash)
                result = With(Left.RemoveOrUpdate(hash, key, isUpdated, ignoreKey), Right);
            else
                result = With(Left, Right.RemoveOrUpdate(hash, key, isUpdated, ignoreKey));
            return result.KeepBalanced();
        }

        private HashTree<K, V> KeepBalanced()
        {
            var delta = Left.Height - Right.Height;
            return delta >= 2 ? With(Left.Right.Height - Left.Left.Height == 1 ? Left.RotateLeft() : Left, Right).RotateRight()
                : (delta <= -2 ? With(Left, Right.Left.Height - Right.Right.Height == 1 ? Right.RotateRight() : Right).RotateLeft()
                : this);
        }

        private HashTree<K, V> RotateRight()
        {
            return Left.With(Left.Left, With(Left.Right, Right));
        }

        private HashTree<K, V> RotateLeft()
        {
            return Right.With(With(Left, Right.Left), Right.Right);
        }

        private HashTree<K, V> With(HashTree<K, V> left, HashTree<K, V> right)
        {
            return new HashTree<K, V>(Hash, Key, Value, Conflicts, left, right);
        }

        #endregion
    }
}
