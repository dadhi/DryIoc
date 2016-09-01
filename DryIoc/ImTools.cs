/*
The MIT License (MIT)

Copyright (c) 2016 Maksim Volkau

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included AddOrUpdateServiceFactory
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

namespace DryIoc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;

    /// <summary>Methods to work with immutable arrays, and general array sugar.</summary>
    public static class ArrayTools
    {
        /// <summary>Returns true if array is null or have no items.</summary> <typeparam name="T">Type of array item.</typeparam>
        /// <param name="source">Source array to check.</param> <returns>True if null or has no items, false otherwise.</returns>
        public static bool IsNullOrEmpty<T>(this T[] source)
        {
            return source == null || source.Length == 0;
        }

        /// <summary>Returns empty array instead of null, or source array otherwise.</summary> <typeparam name="T">Type of array item.</typeparam>
        /// <param name="source">Source array.</param> <returns>Empty array or source.</returns>
        public static T[] EmptyIfNull<T>(this T[] source)
        {
            return source ?? Empty<T>();
        }

        /// <summary>Returns source enumerable if it is array, otherwise converts source to array.</summary>
        /// <typeparam name="T">Array item type.</typeparam>
        /// <param name="source">Source enumerable.</param>
        /// <returns>Source enumerable or its array copy.</returns>
        public static T[] ToArrayOrSelf<T>(this IEnumerable<T> source)
        {
            return source is T[] ? (T[])source : source.ToArray();
        }

        /// <summary>Returns new array consisting from all items from source array then all items from added array.
        /// If source is null or empty, then added array will be returned.
        /// If added is null or empty, then source will be returned.</summary>
        /// <typeparam name="T">Array item type.</typeparam>
        /// <param name="source">Array with leading items.</param>
        /// <param name="added">Array with following items.</param>
        /// <returns>New array with items of source and added arrays.</returns>
        public static T[] Append<T>(this T[] source, params T[] added)
        {
            if (added == null || added.Length == 0)
                return source;
            if (source == null || source.Length == 0)
                return added;
            var result = new T[source.Length + added.Length];
            Array.Copy(source, 0, result, 0, source.Length);
            if (added.Length == 1)
                result[source.Length] = added[0];
            else
                Array.Copy(added, 0, result, source.Length, added.Length);
            return result;
        }

        /// <summary>Returns new array with <paramref name="value"/> appended, 
        /// or <paramref name="value"/> at <paramref name="index"/>, if specified.
        /// If source array could be null or empty, then single value item array will be created despite any index.</summary>
        /// <typeparam name="T">Array item type.</typeparam>
        /// <param name="source">Array to append value to.</param>
        /// <param name="value">Value to append.</param>
        /// <param name="index">(optional) Index of value to update.</param>
        /// <returns>New array with appended or updated value.</returns>
        public static T[] AppendOrUpdate<T>(this T[] source, T value, int index = -1)
        {
            if (source == null || source.Length == 0)
                return new[] { value };
            var sourceLength = source.Length;
            index = index < 0 ? sourceLength : index;
            var result = new T[index < sourceLength ? sourceLength : sourceLength + 1];
            Array.Copy(source, result, sourceLength);
            result[index] = value;
            return result;
        }

        /// <summary>Calls predicate on each item in <paramref name="source"/> array until predicate returns true,
        /// then method will return this item index, or if predicate returns false for each item, method will return -1.</summary>
        /// <typeparam name="T">Type of array items.</typeparam>
        /// <param name="source">Source array: if null or empty, then method will return -1.</param>
        /// <param name="predicate">Delegate to evaluate on each array item until delegate returns true.</param>
        /// <returns>Index of item for which predicate returns true, or -1 otherwise.</returns>
        public static int IndexOf<T>(this T[] source, Func<T, bool> predicate)
        {
            if (source != null && source.Length != 0)
                for (var i = 0; i < source.Length; ++i)
                    if (predicate(source[i]))
                        return i;
            return -1;
        }

        /// <summary>Looks up for item in source array equal to provided value, and returns its index, or -1 if not found.</summary>
        /// <typeparam name="T">Type of array items.</typeparam>
        /// <param name="source">Source array: if null or empty, then method will return -1.</param>
        /// <param name="value">Value to look up.</param>
        /// <returns>Index of item equal to value, or -1 item is not found.</returns>
        public static int IndexOf<T>(this T[] source, T value)
        {
            if (source != null && source.Length != 0)
                for (var i = 0; i < source.Length; ++i)
                {
                    var item = source[i];
                    if (ReferenceEquals(item, value) || Equals(item, value))
                        return i;
                }
            return -1;
        }

        /// <summary>Produces new array without item at specified <paramref name="index"/>. 
        /// Will return <paramref name="source"/> array if index is out of bounds, or source is null/empty.</summary>
        /// <typeparam name="T">Type of array item.</typeparam>
        /// <param name="source">Input array.</param> <param name="index">Index if item to remove.</param>
        /// <returns>New array with removed item at index, or input source array if index is not in array.</returns>
        public static T[] RemoveAt<T>(this T[] source, int index)
        {
            if (source == null || source.Length == 0 || index < 0 || index >= source.Length)
                return source;
            if (index == 0 && source.Length == 1)
                return new T[0];
            var result = new T[source.Length - 1];
            if (index != 0)
                Array.Copy(source, 0, result, 0, index);
            if (index != result.Length)
                Array.Copy(source, index + 1, result, index, result.Length - index);
            return result;
        }

        /// <summary>Looks for item in array using equality comparison, and returns new array with found item remove, or original array if not item found.</summary>
        /// <typeparam name="T">Type of array item.</typeparam>
        /// <param name="source">Input array.</param> <param name="value">Value to find and remove.</param>
        /// <returns>New array with value removed or original array if value is not found.</returns>
        public static T[] Remove<T>(this T[] source, T value)
        {
            return source.RemoveAt(source.IndexOf(value));
        }

        /// <summary>Returns singleton empty array of provided type.</summary> 
        /// <typeparam name="T">Array item type.</typeparam> <returns>Empty array.</returns>
        public static T[] Empty<T>()
        {
            return EmptyArray<T>.Value;
        }

        private static class EmptyArray<T>
        {
            public static readonly T[] Value = new T[0];
        }
    }

    /// <summary>Wrapper that provides optimistic-concurrency Swap operation implemented using <see cref="Ref.Swap{T}"/>.</summary>
    /// <typeparam name="T">Type of object to wrap.</typeparam>
    public sealed class Ref<T> where T : class
    {
        /// <summary>Gets the wrapped value.</summary>
        public T Value { get { return _value; } }

        /// <summary>Creates ref to object, optionally with initial value provided.</summary>
        /// <param name="initialValue">(optional) Initial value.</param>
        public Ref(T initialValue = default(T))
        {
            _value = initialValue;
        }

        /// <summary>Exchanges currently hold object with <paramref name="getNewValue"/> - see <see cref="Ref.Swap{T}"/> for details.</summary>
        /// <param name="getNewValue">Delegate to produce new object value from current one passed as parameter.</param>
        /// <returns>Returns old object value the same way as <see cref="Interlocked.Exchange(ref int,int)"/></returns>
        /// <remarks>Important: <paramref name="getNewValue"/> May be called multiple times to retry update with value concurrently changed by other code.</remarks>
        public T Swap(Func<T, T> getNewValue)
        {
            return Ref.Swap(ref _value, getNewValue);
        }

        /// <summary>Just sets new value ignoring any intermingled changes.</summary>
        /// <param name="newValue"></param> <returns>old value</returns>
        public T Swap(T newValue)
        {
            return Interlocked.Exchange(ref _value, newValue);
        }

        /// <summary>Compares current Referred value with <paramref name="currentValue"/> and if equal replaces current with <paramref name="newValue"/></summary>
        /// <param name="currentValue"></param> <param name="newValue"></param>
        /// <returns>True if current value was replaced with new value, and false if current value is outdated (already changed by other party).</returns>
        /// <example><c>[!CDATA[
        /// var value = SomeRef.Value;
        /// if (!SomeRef.TrySwapIfStillCurrent(value, Update(value))
        ///     SomeRef.Swap(v => Update(v)); // fallback to normal Swap with delegate allocation
        /// ]]</c></example>
        public bool TrySwapIfStillCurrent(T currentValue, T newValue)
        {
            return Interlocked.CompareExchange(ref _value, newValue, currentValue) == currentValue;
        }

        private T _value;
    }

    /// <summary>Provides optimistic-concurrency consistent <see cref="Swap{T}"/> operation.</summary>
    public static class Ref
    {
        /// <summary>Factory for <see cref="Ref{T}"/> with type of value inference.</summary>
        /// <typeparam name="T">Type of value to wrap.</typeparam>
        /// <param name="value">Initial value to wrap.</param>
        /// <returns>New ref.</returns>
        public static Ref<T> Of<T>(T value) where T : class
        {
            return new Ref<T>(value);
        }

        /// <summary>Creates new ref to the value of original ref.</summary> <typeparam name="T">Ref value type.</typeparam>
        /// <param name="original">Original ref.</param> <returns>New ref to original value.</returns>
        public static Ref<T> NewRef<T>(this Ref<T> original) where T : class
        {
            return Of(original.Value);
        }

        /// <summary>First, it evaluates new value using <paramref name="getNewValue"/> function. 
        /// Second, it checks that original value is not changed. 
        /// If it is changed it will retry first step, otherwise it assigns new value and returns original (the one used for <paramref name="getNewValue"/>).</summary>
        /// <typeparam name="T">Type of value to swap.</typeparam>
        /// <param name="value">Reference to change to new value</param>
        /// <param name="getNewValue">Delegate to get value from old one.</param>
        /// <returns>Old/original value. By analogy with <see cref="Interlocked.Exchange(ref int,int)"/>.</returns>
        /// <remarks>Important: <paramref name="getNewValue"/> May be called multiple times to retry update with value concurrently changed by other code.</remarks>
        public static T Swap<T>(ref T value, Func<T, T> getNewValue) where T : class
        {
            var retryCount = 0;
            while (true)
            {
                var oldValue = value;
                var newValue = getNewValue(oldValue);
                if (Interlocked.CompareExchange(ref value, newValue, oldValue) == oldValue)
                    return oldValue;
                if (++retryCount > RETRY_COUNT_UNTIL_THROW)
                    throw new InvalidOperationException(_errorRetryCountExceeded);
            }
        }

        private const int RETRY_COUNT_UNTIL_THROW = 50;
        private static readonly string _errorRetryCountExceeded =
            "Ref retried to Update for " + RETRY_COUNT_UNTIL_THROW + " times But there is always someone else intervened.";
    }

    /// <summary>Immutable Key-Value pair. It is reference type (could be check for null), 
    /// which is different from System value type <see cref="KeyValuePair{TKey,TValue}"/>.
    /// In addition provides <see cref="Equals"/> and <see cref="GetHashCode"/> implementations.</summary>
    /// <typeparam name="K">Type of Key.</typeparam><typeparam name="V">Type of Value.</typeparam>
    public class KV<K, V>
    {
        /// <summary>Key.</summary>
        public readonly K Key;

        /// <summary>Value.</summary>
        public readonly V Value;

        /// <summary>Creates Key-Value object by providing key and value. Does Not check either one for null.</summary>
        /// <param name="key">key.</param><param name="value">value.</param>
        public KV(K key, V value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>Creates nice string view.</summary><returns>String representation.</returns>
        public override string ToString()
        {
            var s = new StringBuilder('{');
            if (Key != null)
                s.Append(Key);
            s.Append(',');
            if (Value != null)
                s.Append(Value);
            s.Append('}');
            return s.ToString();
        }

        /// <summary>Returns true if both key and value are equal to corresponding key-value of other object.</summary>
        /// <param name="obj">Object to check equality with.</param> <returns>True if equal.</returns>
        public override bool Equals(object obj)
        {
            var other = obj as KV<K, V>;
            return other != null
                   && (ReferenceEquals(other.Key, Key) || Equals(other.Key, Key))
                   && (ReferenceEquals(other.Value, Value) || Equals(other.Value, Value));
        }

        /// <summary>Combines key and value hash code. R# generated default implementation.</summary>
        /// <returns>Combined hash code for key-value.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((object)Key == null ? 0 : Key.GetHashCode() * 397)
                       ^ ((object)Value == null ? 0 : Value.GetHashCode());
            }
        }
    }

    /// <summary>Delegate for changing value from old one to some new based on provided new value.</summary>
    /// <typeparam name="V">Type of values.</typeparam>
    /// <param name="oldValue">Existing value.</param>
    /// <param name="newValue">New value passed to Update.. method.</param>
    /// <returns>Changed value.</returns>
    public delegate V Update<V>(V oldValue, V newValue);

    // todo: V3: Rename to ImTree
    /// <summary>Simple immutable AVL tree with integer keys and object values.</summary>
    public sealed class ImTreeMapIntToObj
    {
        /// <summary>Empty tree to start with.</summary>
        public static readonly ImTreeMapIntToObj Empty = new ImTreeMapIntToObj();

        /// <summary>Key.</summary>
        public readonly int Key;

        /// <summary>Value.</summary>
        public readonly object Value;

        /// <summary>Left sub-tree/branch, or empty.</summary>
        public readonly ImTreeMapIntToObj Left;

        /// <summary>Right sub-tree/branch, or empty.</summary>
        public readonly ImTreeMapIntToObj Right;

        /// <summary>Height of longest sub-tree/branch plus 1. It is 0 for empty tree, and 1 for single node tree.</summary>
        public readonly int Height;

        /// <summary>Returns true is tree is empty.</summary>
        public bool IsEmpty { get { return Height == 0; } }

        /// <summary>Returns new tree with added or updated value for specified key.</summary>
        /// <param name="key"></param> <param name="value"></param>
        /// <returns>New tree.</returns>
        public ImTreeMapIntToObj AddOrUpdate(int key, object value)
        {
            return AddOrUpdate(key, value, false, null);
        }

        /// <summary>Delegate to calculate new value from and old and a new value.</summary>
        /// <param name="oldValue">Old</param> <param name="newValue">New</param> <returns>Calculated result.</returns>
        public delegate object UpdateValue(object oldValue, object newValue);

        /// <summary>Returns new tree with added or updated value for specified key.</summary>
        /// <param name="key">Key</param> <param name="value">Value</param>
        /// <param name="updateValue">(optional) Delegate to calculate new value from and old and a new value.</param>
        /// <returns>New tree.</returns>
        public ImTreeMapIntToObj AddOrUpdate(int key, object value, UpdateValue updateValue)
        {
            return AddOrUpdate(key, value, false, updateValue);
        }

        /// <summary>Returns new tree with updated value for the key, Or the same tree if key was not found.</summary>
        /// <param name="key"></param> <param name="value"></param>
        /// <returns>New tree if key is found, or the same tree otherwise.</returns>
        public ImTreeMapIntToObj Update(int key, object value)
        {
            return AddOrUpdate(key, value, true, null);
        }

        /// <summary>Get value for found key or null otherwise.</summary>
        /// <param name="key"></param> <returns>Found value or null.</returns>
        public object GetValueOrDefault(int key)
        {
            var tree = this;
            while (tree.Height != 0 && tree.Key != key)
                tree = key < tree.Key ? tree.Left : tree.Right;
            return tree.Height != 0 ? tree.Value : null;
        }

        /// <summary>Returns all sub-trees enumerated from left to right.</summary> 
        /// <returns>Enumerated sub-trees or empty if tree is empty.</returns>
        public IEnumerable<ImTreeMapIntToObj> Enumerate()
        {
            if (Height == 0)
                yield break;

            var parents = new ImTreeMapIntToObj[Height];

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
                    yield return tree;
                    tree = tree.Right;
                }
            }
        }

        #region Implementation

        private ImTreeMapIntToObj() { }

        private ImTreeMapIntToObj(int key, object value, ImTreeMapIntToObj left, ImTreeMapIntToObj right)
        {
            Key = key;
            Value = value;
            Left = left;
            Right = right;
            Height = 1 + (left.Height > right.Height ? left.Height : right.Height);
        }

        private ImTreeMapIntToObj AddOrUpdate(int key, object value, bool updateOnly, UpdateValue update)
        {
            return Height == 0 ? // tree is empty
                (updateOnly ? this : new ImTreeMapIntToObj(key, value, Empty, Empty))
                : (key == Key ? // actual update
                    new ImTreeMapIntToObj(key, update == null ? value : update(Value, value), Left, Right)
                    : (key < Key    // try update on left or right sub-tree
                        ? With(Left.AddOrUpdate(key, value, updateOnly, update), Right)
                        : With(Left, Right.AddOrUpdate(key, value, updateOnly, update))).KeepBalanced());
        }

        private ImTreeMapIntToObj KeepBalanced()
        {
            var delta = Left.Height - Right.Height;
            return delta >= 2 ? With(Left.Right.Height - Left.Left.Height == 1 ? Left.RotateLeft() : Left, Right).RotateRight()
                : (delta <= -2 ? With(Left, Right.Left.Height - Right.Right.Height == 1 ? Right.RotateRight() : Right).RotateLeft()
                    : this);
        }

        private ImTreeMapIntToObj RotateRight()
        {
            return Left.With(Left.Left, With(Left.Right, Right));
        }

        private ImTreeMapIntToObj RotateLeft()
        {
            return Right.With(With(Left, Right.Left), Right.Right);
        }

        private ImTreeMapIntToObj With(ImTreeMapIntToObj left, ImTreeMapIntToObj right)
        {
            return left == Left && right == Right ? this : new ImTreeMapIntToObj(Key, Value, left, right);
        }

        #endregion
    }

    // todo: V3: Rename to ImHashTree
    /// <summary>Immutable http://en.wikipedia.org/wiki/AVL_tree where actual node key is hash code of <typeparamref name="K"/>.</summary>
    public sealed class ImTreeMap<K, V>
    {
        /// <summary>Empty tree to start with.</summary>
        public static readonly ImTreeMap<K, V> Empty = new ImTreeMap<K, V>();

        /// <summary>Key of type K that should support <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode"/>.</summary>
        public readonly K Key;

        /// <summary>Value of any type V.</summary>
        public readonly V Value;

        /// <summary>Calculated key hash.</summary>
        public readonly int Hash;

        /// <summary>In case of <see cref="Hash"/> conflicts for different keys contains conflicted keys with their values.</summary>
        public readonly KV<K, V>[] Conflicts;

        /// <summary>Left sub-tree/branch, or empty.</summary>
        public readonly ImTreeMap<K, V> Left;

        /// <summary>Right sub-tree/branch, or empty.</summary>
        public readonly ImTreeMap<K, V> Right;

        /// <summary>Height of longest sub-tree/branch plus 1. It is 0 for empty tree, and 1 for single node tree.</summary>
        public readonly int Height;

        /// <summary>Returns true if tree is empty.</summary>
        public bool IsEmpty { get { return Height == 0; } }

        /// <summary>Returns new tree with added key-value. If value with the same key is exist, then
        /// if <paramref name="update"/> is not specified: then existing value will be replaced by <paramref name="value"/>;
        /// if <paramref name="update"/> is specified: then update delegate will decide what value to keep.</summary>
        /// <param name="key">Key to add.</param><param name="value">Value to add.</param>
        /// <param name="update">(optional) Delegate to decide what value to keep: old or new one.</param>
        /// <returns>New tree with added or updated key-value.</returns>
        public ImTreeMap<K, V> AddOrUpdate(K key, V value, Update<V> update = null)
        {
            return AddOrUpdate(key.GetHashCode(), key, value, update, updateOnly: false);
        }

        /// <summary>Looks for <paramref name="key"/> and replaces its value with new <paramref name="value"/>, or 
        /// runs custom update handler (<paramref name="update"/>) with old and new value to get the updated result.</summary>
        /// <param name="key">Key to look for.</param>
        /// <param name="value">New value to replace key value with.</param>
        /// <param name="update">(optional) Delegate for custom update logic, it gets old and new <paramref name="value"/>
        /// as inputs and should return updated value as output.</param>
        /// <returns>New tree with updated value or the SAME tree if no key found.</returns>
        public ImTreeMap<K, V> Update(K key, V value, Update<V> update = null)
        {
            return AddOrUpdate(key.GetHashCode(), key, value, update, updateOnly: true);
        }

        /// <summary>Looks for key in a tree and returns the key value if found, or <paramref name="defaultValue"/> otherwise.</summary>
        /// <param name="key">Key to look for.</param> <param name="defaultValue">(optional) Value to return if key is not found.</param>
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

        /// <summary>Depth-first in-order traversal as described in http://en.wikipedia.org/wiki/Tree_traversal
        /// The only difference is using fixed size array instead of stack for speed-up (~20% faster than stack).</summary>
        /// <returns>Sequence of enumerated key value pairs.</returns>
        public IEnumerable<KV<K, V>> Enumerate()
        {
            if (Height == 0)
                yield break;

            var parents = new ImTreeMap<K, V>[Height];

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

        #region Implementation

        private ImTreeMap() { }

        private ImTreeMap(int hash, K key, V value, KV<K, V>[] conficts, ImTreeMap<K, V> left, ImTreeMap<K, V> right)
        {
            Hash = hash;
            Key = key;
            Value = value;
            Conflicts = conficts;
            Left = left;
            Right = right;
            Height = 1 + (left.Height > right.Height ? left.Height : right.Height);
        }

        private ImTreeMap<K, V> AddOrUpdate(int hash, K key, V value, Update<V> update, bool updateOnly)
        {
            return Height == 0 ? (updateOnly ? this : new ImTreeMap<K, V>(hash, key, value, null, Empty, Empty))
                : (hash == Hash ? UpdateValueAndResolveConflicts(key, value, update, updateOnly)
                    : (hash < Hash
                        ? With(Left.AddOrUpdate(hash, key, value, update, updateOnly), Right)
                        : With(Left, Right.AddOrUpdate(hash, key, value, update, updateOnly))).KeepBalanced());
        }

        private ImTreeMap<K, V> UpdateValueAndResolveConflicts(K key, V value, Update<V> update, bool updateOnly)
        {
            if (ReferenceEquals(Key, key) || Key.Equals(key))
                return new ImTreeMap<K, V>(Hash, key, update == null ? value : update(Value, value), Conflicts, Left, Right);

            if (Conflicts == null) // add only if updateOnly is false.
                return updateOnly ? this
                    : new ImTreeMap<K, V>(Hash, Key, Value, new[] { new KV<K, V>(key, value) }, Left, Right);

            var found = Conflicts.Length - 1;
            while (found >= 0 && !Equals(Conflicts[found].Key, Key)) --found;
            if (found == -1)
            {
                if (updateOnly) return this;
                var newConflicts = new KV<K, V>[Conflicts.Length + 1];
                Array.Copy(Conflicts, 0, newConflicts, 0, Conflicts.Length);
                newConflicts[Conflicts.Length] = new KV<K, V>(key, value);
                return new ImTreeMap<K, V>(Hash, Key, Value, newConflicts, Left, Right);
            }

            var conflicts = new KV<K, V>[Conflicts.Length];
            Array.Copy(Conflicts, 0, conflicts, 0, Conflicts.Length);
            conflicts[found] = new KV<K, V>(key, update == null ? value : update(Conflicts[found].Value, value));
            return new ImTreeMap<K, V>(Hash, Key, Value, conflicts, Left, Right);
        }

        private V GetConflictedValueOrDefault(K key, V defaultValue)
        {
            if (Conflicts != null)
                for (var i = 0; i < Conflicts.Length; i++)
                    if (Equals(Conflicts[i].Key, key))
                        return Conflicts[i].Value;
            return defaultValue;
        }

        private ImTreeMap<K, V> KeepBalanced()
        {
            var delta = Left.Height - Right.Height;
            return delta >= 2 ? With(Left.Right.Height - Left.Left.Height == 1 ? Left.RotateLeft() : Left, Right).RotateRight()
                : (delta <= -2 ? With(Left, Right.Left.Height - Right.Right.Height == 1 ? Right.RotateRight() : Right).RotateLeft()
                    : this);
        }

        private ImTreeMap<K, V> RotateRight()
        {
            return Left.With(Left.Left, With(Left.Right, Right));
        }

        private ImTreeMap<K, V> RotateLeft()
        {
            return Right.With(With(Left, Right.Left), Right.Right);
        }

        private ImTreeMap<K, V> With(ImTreeMap<K, V> left, ImTreeMap<K, V> right)
        {
            return left == Left && right == Right ? this : new ImTreeMap<K, V>(Hash, Key, Value, Conflicts, left, right);
        }

        #endregion
    }
}