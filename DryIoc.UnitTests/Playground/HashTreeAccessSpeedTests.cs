using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DryIoc.UnitTests.Playground
{
    public sealed class Hashed<K, V>
    {
        public readonly K Key;
        public readonly V Value;
        public readonly KeyValuePair<K, V>[] Conflicts;

        public Hashed(K key, V value, KeyValuePair<K, V>[] conflicts = null)
        {
            Key = key;
            Value = value;
            Conflicts = conflicts;
        }
    }

    public static class Hashed
    {
        public static IntTree<Hashed<K, V>> AddOrUpdate<K, V>(this IntTree<Hashed<K, V>> tree, K key, V value)
        {
            return tree.AddOrUpdate(key.GetHashCode(), new Hashed<K, V>(key, value), Update);
        }

        public static V TryGet<K, V>(this IntTree<Hashed<K, V>> tree, K key)
        {
            var item = tree.GetValueOrDefault(key.GetHashCode());
            return item != null && Equals(item.Key, key) ? item.Value : TryGetConflicted(item, key);
        }

        #region Implementation

        private static Hashed<K, V> Update<K, V>(Hashed<K, V> item, Hashed<K, V> newItem)
        {
            if (Equals(item.Key, newItem.Key))
                return new Hashed<K, V>(newItem.Key, newItem.Value, item.Conflicts);

            var newConflict = new KeyValuePair<K, V>(newItem.Key, newItem.Value);
            var newConflicts = item.Conflicts == null ? new[] { newConflict }
                : item.Conflicts.AppendOrUpdate(newConflict, Array.FindIndex(item.Conflicts, x => Equals(x.Key, newItem.Key)));

            return new Hashed<K, V>(item.Key, item.Value, newConflicts);
        }

        private static V TryGetConflicted<K, V>(Hashed<K, V> item, K key)
        {
            if (item != null && item.Conflicts != null)
            {
                var conflicts = item.Conflicts;
                for (var i = 0; i < conflicts.Length; i++)
                    if (Equals(conflicts[i].Key, key))
                        return conflicts[i].Value;
            }
            return default(V);
        }

        #endregion
    }

    public sealed class HashTree2<K, V> : IEnumerable<HashTree2<K, V>>
    {
        public static readonly HashTree2<K, V> Empty = new HashTree2<K, V>(IntTree<KVStack>.Empty);

        public HashTree2<K, V> AddOrUpdate(K key, V value)
        {
            return new HashTree2<K, V>(_tree.AddOrUpdate(key.GetHashCode(), new KVStack(key, value), Update));
        }

        public V TryGet(K key, V defaultValue = default(V))
        {
            var item = _tree.GetValueOrDefault(key.GetHashCode());
            return item != null && Equals(item.Key, key) ? item.Value : TryGetStacked(item, key, defaultValue);
        }

        public IEnumerator<HashTree2<K, V>> GetEnumerator()
        {
            return _tree.TraverseInOrder().Select(t => new HashTree2<K, V>(t)).GetEnumerator();
        }

        private readonly IntTree<KVStack> _tree;

        private HashTree2(IntTree<KVStack> tree)
        {
            _tree = tree;
        }

        private static KVStack Update(KVStack item, KVStack newItem)
        {
            if (Equals(item.Key, newItem.Key))
                return item.Tail.Push(newItem);

            for (KVStack i = item.Tail, tested = KVStack.End; i != KVStack.End; i = i.Tail, tested = tested.Push(i))
            {
                if (Equals(i.Key, newItem.Key))
                {
                    var result = i.Tail.Push(newItem);
                    for (var ti = tested; ti != KVStack.End; ti = ti.Tail)
                        result = result.Push(ti);
                    return result;
                }
            }

            return item.Push(newItem);
        }

        private static V TryGetStacked(KVStack item, K key, V defaultValue)
        {
            for (var i = item; i != KVStack.End; i = i.Tail)
                if (Equals(i.Key, key))
                    return i.Value;
            return defaultValue;
        }

        private sealed class KVStack
        {
            public static readonly KVStack End = new KVStack();

            public readonly K Key;
            public readonly V Value;
            public readonly KVStack Tail;

            public KVStack(K key, V value, KVStack tail = null)
            {
                Key = key;
                Value = value;
                Tail = tail ?? End;
            }

            public KVStack Push(KVStack head)
            {
                return new KVStack(head.Key, head.Value, this);
            }

            private KVStack() { }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public sealed class HashTree3<K, V> : IEnumerable<HashTree3<K, V>>
    {
        public static readonly HashTree3<K, V> Empty = new HashTree3<K, V>(IntTree<KV>.Empty);

        public HashTree3<K, V> AddOrUpdate(K key, V value)
        {
            return new HashTree3<K, V>(_tree.AddOrUpdate(key.GetHashCode(), new KV { Key = key, Value = value }, Update));
        }

        private readonly KV _defaultItem = new KV();

        public V TryGet(K key)
        {
            var item = _tree.GetValueOrDefault(key.GetHashCode(), _defaultItem);
            return key.Equals(item.Key) ? item.Value : TryGetConflicted(item, key);
            //return item != null && key.Equals(item.Key) ? item.Value : TryGetConflicted(item, key);
        }

        public IEnumerator<HashTree3<K, V>> GetEnumerator()
        {
            return _tree.TraverseInOrder().Select(t => new HashTree3<K, V>(t)).GetEnumerator();
        }

        private readonly IntTree<KV> _tree;

        private HashTree3(IntTree<KV> tree)
        {
            _tree = tree;
        }

        private static KV Update(KV item, KV newItem)
        {
            if (Equals(item.Key, newItem.Key))
                return Cons(newItem, Tail(item));

            for (KV i = Tail(item), tested = null; i != null; i = Tail(i), tested = Cons(i, tested))
            {
                if (Equals(i.Key, newItem.Key))
                {
                    var updated = Cons(newItem, Tail(i));
                    for (var ti = tested; ti != null; ti = Tail(ti))
                        updated = Cons(ti, updated);
                    return updated;
                }
            }

            return Cons(newItem, item); // appended
        }

        private static KV Cons(KV item, KV rest)
        {
            return rest == null ? new KV { Key = item.Key, Value = item.Value }
                : new KVStack { Key = item.Key, Value = item.Value, Rest = rest };
        }

        private static KV Tail(KV item)
        {
            return item is KVStack ? ((KVStack)item).Rest : null;
        }

        private static V TryGetConflicted(KV item, K key)
        {
            for (var i = item; i != null; i = Tail(i))
                if (Equals(i.Key, key))
                    return i.Value;
            return default(V);
        }

        private class KV
        {
            public K Key;
            public V Value;
        }

        private sealed class KVStack : KV
        {
            public KV Rest;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public sealed class HashTreeV2<V> : IEnumerable<HashTreeV2<V>>
    {
        public static readonly HashTreeV2<V> Empty = new HashTreeV2<V>();
        public bool IsEmpty { get { return Height == 0; } }

        public readonly HashTreeV2<V> Left, Right;
        public readonly int Height;
        public readonly int Key;
        public readonly V Value;

        public delegate V UpdateValue(V old, V added);

        public HashTreeV2<V> AddOrUpdate(int key, V newValue, UpdateValue update = null)
        {
            return Height == 0 ? new HashTreeV2<V>(key, newValue, Empty, Empty)
                : (key == Key ? new HashTreeV2<V>(key, update == null ? newValue : update(Value, newValue), Left, Right)
                    : (key < Key
                        ? With(Left.AddOrUpdate(key, newValue), Right)
                        : With(Left, Right.AddOrUpdate(key, newValue))).EnsureBalanced());
        }

        public V TryGet(int key, V defaultValue = default(V))
        {
            var current = this;
            while (current.Height != 0 && key != current.Key)
                current = key < current.Key ? current.Left : current.Right;
            return current.Height != 0 ? current.Value : defaultValue;
        }

        public V TryGet_ORIGINAL(int key, V defaultValue = default(V))
        {
            for (var t = this; t.Height != 0; t = key < t.Key ? t.Left : t.Right)
                if (key == t.Key)
                    return t.Value;
            return defaultValue;
        }

        public IEnumerator<HashTreeV2<V>> GetEnumerator()
        {
            Stack<HashTreeV2<V>> leftNodes = null;
            for (var node = this; !node.IsEmpty || leftNodes != null; node = node.Right)
            {
                for (; !node.IsEmpty; node = node.Left)
                    leftNodes = new Stack<HashTreeV2<V>>(node, leftNodes);
                node = leftNodes.Head;
                leftNodes = leftNodes.Tail;
                yield return node;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region Implementation

        private HashTreeV2() { }

        private HashTreeV2(int key, V value, HashTreeV2<V> left, HashTreeV2<V> right)
        {
            Key = key;
            Value = value;
            Left = left;
            Right = right;
            Height = 1 + (left.Height > right.Height ? left.Height : right.Height);
        }

        private HashTreeV2<V> EnsureBalanced()
        {
            var delta = Left.Height - Right.Height;
            return delta >= 2 ? With(Left.Right.Height - Left.Left.Height == 1 ? Left.RotateLeft() : Left, Right).RotateRight()
                : (delta <= -2 ? With(Left, Right.Left.Height - Right.Right.Height == 1 ? Right.RotateRight() : Right).RotateLeft()
                : this);
        }

        private HashTreeV2<V> RotateRight()
        {
            return Left.With(Left.Left, With(Left.Right, Right));
        }

        private HashTreeV2<V> RotateLeft()
        {
            return Right.With(With(Left, Right.Left), Right.Right);
        }

        private HashTreeV2<V> With(HashTreeV2<V> left, HashTreeV2<V> right)
        {
            return new HashTreeV2<V>(Key, Value, left, right);
        }

        private sealed class Stack<T>
        {
            public readonly T Head;
            public readonly Stack<T> Tail;

            public Stack(T head, Stack<T> tail = null)
            {
                Head = head;
                Tail = tail;
            }
        }

        #endregion
    }
}
