using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using ImTools;

namespace Playground
{
    [MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class ImMapBenchmarks
    {
        [Benchmark(Baseline = true)]
        public Zero.ImMap<string> AddOrUpdate_v1()
        {
            var map = Zero.ImMap<string>.Empty;

            for (var i = 0; i < 9; i++)
                map = map.AddOrUpdate(i, i.ToString());

            return map;
        }

        //[Benchmark(Baseline = true)]
        public ImMap<string> AddOrUpdate()
        {
            var map = ImMap<string>.Empty;

            for (var i = 0; i < 9; i++)
                map = map.AddOrUpdate2(i, i.ToString());

            return map;
        }

        [Benchmark]
        public ImMap<string> AddOrUpdate_optimized()
        {
            var map = ImMap<string>.Empty;

            for (var i = 0; i < 9; i++)
                map = map.AddOrUpdate(i, i.ToString());

            return map;
        }
    }
}

namespace Zero
{
    using System.Collections.Generic;

    /// <summary>Immutable http://en.wikipedia.org/wiki/AVL_tree with integer keys and <typeparamref name="V"/> values.</summary>
    public sealed class ImMap<V>
    {
        /// <summary>Empty tree to start with.</summary>
        public static readonly ImMap<V> Empty = new ImMap<V>();

        /// <summary>Key.</summary>
        public readonly int Key;

        /// <summary>Value.</summary>
        public readonly V Value;

        /// <summary>Left sub-tree/branch, or empty.</summary>
        public readonly ImMap<V> Left;

        /// <summary>Right sub-tree/branch, or empty.</summary>
        public readonly ImMap<V> Right;

        /// <summary>Height of longest sub-tree/branch plus 1. It is 0 for empty tree, and 1 for single node tree.</summary>
        public readonly int Height;

        /// <summary>Returns true is tree is empty.</summary>
        public bool IsEmpty => Height == 0;

        /// <summary>Returns new tree with added or updated value for specified key.</summary>
        /// <param name="key"></param> <param name="value"></param>
        /// <returns>New tree.</returns>
        public ImMap<V> AddOrUpdate(int key, V value) =>
            AddOrUpdateImpl(key, value);

        /// <summary>Returns new tree with added or updated value for specified key.</summary>
        /// <param name="key">Key</param> <param name="value">Value</param>
        /// <param name="updateValue">(optional) Delegate to calculate new value from and old and a new value.</param>
        /// <returns>New tree.</returns>
        public ImMap<V> AddOrUpdate(int key, V value, Update<V> updateValue) =>
            AddOrUpdateImpl(key, value, false, updateValue);

        /// <summary>Returns new tree with updated value for the key, Or the same tree if key was not found.</summary>
        /// <param name="key"></param> <param name="value"></param>
        /// <returns>New tree if key is found, or the same tree otherwise.</returns>
        public ImMap<V> Update(int key, V value) =>
            AddOrUpdateImpl(key, value, true, null);

        /// <summary>Get value for found key or null otherwise.</summary>
        /// <param name="key"></param> <param name="defaultValue">(optional) Value to return if key is not found.</param>
        /// <returns>Found value or <paramref name="defaultValue"/>.</returns>
        public V GetValueOrDefault(int key, V defaultValue = default(V))
        {
            var node = this;
            while (node.Height != 0 && node.Key != key)
                node = key < node.Key ? node.Left : node.Right;
            return node.Height != 0 ? node.Value : defaultValue;
        }

        /// <summary>Returns true if key is found and sets the value.</summary>
        /// <param name="key">Key to look for.</param> <param name="value">Result value</param>
        /// <returns>True if key found, false otherwise.</returns>
        public bool TryFind(int key, out V value)
        {
            var hash = key.GetHashCode();

            var node = this;
            while (node.Height != 0 && node.Key != key)
                node = hash < node.Key ? node.Left : node.Right;

            if (node.Height != 0)
            {
                value = node.Value;
                return true;
            }

            value = default(V);
            return false;
        }

        /// <summary>Returns all sub-trees enumerated from left to right.</summary> 
        /// <returns>Enumerated sub-trees or empty if tree is empty.</returns>
        public IEnumerable<ImMap<V>> Enumerate()
        {
            if (Height == 0)
                yield break;

            var parents = new ImMap<V>[Height];

            var node = this;
            var parentCount = -1;
            while (node.Height != 0 || parentCount != -1)
            {
                if (node.Height != 0)
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

        /// <summary>Removes or updates value for specified key, or does nothing if key is not found.
        /// Based on Eric Lippert http://blogs.msdn.com/b/ericlippert/archive/2008/01/21/immutability-in-c-part-nine-academic-plus-my-avl-tree-implementation.aspx </summary>
        /// <param name="key">Key to look for.</param> 
        /// <returns>New tree with removed or updated value.</returns>
        public ImMap<V> Remove(int key) =>
            RemoveImpl(key);

        /// <summary>Outputs key value pair</summary>
        public override string ToString() => IsEmpty ? "empty" : (Key + ":" + Value);

        #region Implementation

        private ImMap() { }

        private ImMap(int key, V value)
        {
            Key = key;
            Value = value;
            Left = Empty;
            Right = Empty;
            Height = 1;
        }

        private ImMap(int key, V value, ImMap<V> left, ImMap<V> right, int height)
        {
            Key = key;
            Value = value;
            Left = left;
            Right = right;
            Height = height;
        }

        private ImMap(int key, V value, ImMap<V> left, ImMap<V> right)
        {
            Key = key;
            Value = value;
            Left = left;
            Right = right;
            Height = 1 + (left.Height > right.Height ? left.Height : right.Height);
        }

        private ImMap<V> AddOrUpdateImpl(int key, V value)
        {
            return Height == 0  // add new node
                ? new ImMap<V>(key, value)
                : (key == Key // update found node
                    ? new ImMap<V>(key, value, Left, Right)
                    : (key < Key  // search for node
                        ? (Height == 1
                            ? new ImMap<V>(Key, Value, new ImMap<V>(key, value), Right, height: 2)
                            : new ImMap<V>(Key, Value, Left.AddOrUpdateImpl(key, value), Right).KeepBalance())
                        : (Height == 1
                            ? new ImMap<V>(Key, Value, Left, new ImMap<V>(key, value), height: 2)
                            : new ImMap<V>(Key, Value, Left, Right.AddOrUpdateImpl(key, value)).KeepBalance())));
        }

        private ImMap<V> AddOrUpdateImpl(int key, V value, bool updateOnly, Update<V> update)
        {
            return Height == 0 ? // tree is empty
                (updateOnly ? this : new ImMap<V>(key, value))
                : (key == Key ? // actual update
                    new ImMap<V>(key, update == null ? value : update(Value, value), Left, Right)
                    : (key < Key    // try update on left or right sub-tree
                        ? new ImMap<V>(Key, Value, Left.AddOrUpdateImpl(key, value, updateOnly, update), Right)
                        : new ImMap<V>(Key, Value, Left, Right.AddOrUpdateImpl(key, value, updateOnly, update)))
                    .KeepBalance());
        }

        private ImMap<V> KeepBalance()
        {
            var delta = Left.Height - Right.Height;
            if (delta >= 2) // left is longer by 2, rotate left
            {
                var left = Left;
                var leftLeft = left.Left;
                var leftRight = left.Right;
                if (leftRight.Height - leftLeft.Height == 1)
                {
                    // double rotation:
                    //      5     =>     5     =>     4
                    //   2     6      4     6      2     5
                    // 1   4        2   3        1   3     6
                    //    3        1
                    return new ImMap<V>(leftRight.Key, leftRight.Value,
                        left: new ImMap<V>(left.Key, left.Value,
                            left: leftLeft, right: leftRight.Left), right: new ImMap<V>(Key, Value,
                            left: leftRight.Right, right: Right));
                }

                // todo: do we need this?
                // one rotation:
                //      5     =>     2
                //   2     6      1     5
                // 1   4              4   6
                return new ImMap<V>(left.Key, left.Value,
                    left: leftLeft, right: new ImMap<V>(Key, Value,
                        left: leftRight, right: Right));
            }

            if (delta <= -2)
            {
                var right = Right;
                var rightLeft = right.Left;
                var rightRight = right.Right;
                if (rightLeft.Height - rightRight.Height == 1)
                {
                    return new ImMap<V>(rightLeft.Key, rightLeft.Value,
                        left: new ImMap<V>(Key, Value,
                            left: Left, right: rightLeft.Left), right: new ImMap<V>(right.Key, right.Value,
                            left: rightLeft.Right, right: rightRight));
                }

                return new ImMap<V>(right.Key, right.Value,
                    left: new ImMap<V>(Key, Value,
                        left: Left, right: rightLeft), right: rightRight);
            }

            return this;
        }

        private ImMap<V> RemoveImpl(int key, bool ignoreKey = false)
        {
            if (Height == 0)
                return this;

            ImMap<V> result;
            if (key == Key || ignoreKey) // found node
            {
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
                    result = new ImMap<V>(successor.Key, successor.Value,
                        Left, Right.RemoveImpl(successor.Key, ignoreKey: true));
                }
            }
            else if (key < Key)
                result = new ImMap<V>(Key, Value, Left.RemoveImpl(key), Right);
            else
                result = new ImMap<V>(Key, Value, Left, Right.RemoveImpl(key));

            return result.KeepBalance();
        }

        #endregion
    }
}
