/*
The MIT License (MIT)

Copyright (c) 2013 Maksim Volkau

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System.Collections;
using System.Text;

namespace System
{
    /// <summary>Func with 5 input parameters.</summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    /// <param name="arg3"></param>
    /// <param name="arg4"></param>
    /// <param name="arg5"></param>
    /// <returns></returns>
    public delegate TResult Func<T1, T2, T3, T4, T5, TResult>(
        T1 arg1,
        T2 arg2,
        T3 arg3,
        T4 arg4,
        T5 arg5);

    /// <summary>Func with 6 input parameters.</summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    /// <param name="arg3"></param>
    /// <param name="arg4"></param>
    /// <param name="arg5"></param>
    /// <param name="arg6"></param>
    /// <returns></returns>
    public delegate TResult Func<T1, T2, T3, T4, T5, T6, TResult>(
        T1 arg1,
        T2 arg2,
        T3 arg3,
        T4 arg4,
        T5 arg5,
        T6 arg6);

    /// <summary>Func with 7 input parameters.</summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <typeparam name="T5"></typeparam>
    /// <typeparam name="T6"></typeparam>
    /// <typeparam name="T7"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    /// <param name="arg3"></param>
    /// <param name="arg4"></param>
    /// <param name="arg5"></param>
    /// <param name="arg6"></param>
    /// <param name="arg7"></param>
    /// <returns></returns>
    public delegate TResult Func<T1, T2, T3, T4, T5, T6, T7, TResult>(
        T1 arg1,
        T2 arg2,
        T3 arg3,
        T4 arg4,
        T5 arg5,
        T6 arg6,
        T7 arg7);

    /// <summary>Wrapper for value computation required on-demand. Since computed the same value will be returned over and over again.</summary>
    /// <typeparam name="T">Type of value.</typeparam>
    public sealed class Lazy<T>
    {
        /// <summary>Creates lazy object with passed value computation delegate.</summary>
        /// <param name="valueFactory">Value computation. Will be stored until computation is done.</param>
        /// <exception cref="ArgumentNullException">Throws for null computation.</exception>
        public Lazy(Func<T> valueFactory)
        {
            if (valueFactory == null) throw new ArgumentNullException("valueFactory");
            _valueFactory = valueFactory;
        }

        /// <summary>Indicates if value is computed already, or not.</summary>
        public bool IsValueCreated { get; private set; }

        /// <summary>Computes value if it was not before, and returns it. 
        /// Value is guaranteed to be computed only once despite possible thread contention.</summary>
        /// <exception cref="InvalidOperationException">Throws if value computation is recursive.</exception>
        public T Value
        {
            get { return IsValueCreated ? _value : Create(); }
        }

        #region Implementation

        private Func<T> _valueFactory;
        private T _value;
        private readonly object _valueCreationLock = new object();

        private T Create()
        {
            lock (_valueCreationLock)
            {
                if (!IsValueCreated)
                {
                    if (_valueFactory == null) throw new InvalidOperationException("The initialization function tries to access Value on this instance.");
                    var factory = _valueFactory;
                    _valueFactory = null;
                    _value = factory();
                    IsValueCreated = true;
                }
            }

            return _value;
        }

        #endregion
    }

    /// <summary>Common properties of all tuples.</summary>
    public interface ITuple
    {
        /// <summary>Gets number of components in tuple.</summary>
        int Size { get; }

        /// <summary>Appends string representation of tuple components to passed string builder.</summary>
        /// <param name="sb">String builder to append to.</param> <returns>String builder with appended tuple.</returns>
        string ToString(StringBuilder sb);

        /// <summary> Returns the hash code for tuple object combining component's hash codes. </summary>
        /// <param name="comparer"></param> <returns>Hash code.</returns>
        int GetHashCode(IEqualityComparer comparer);
    }

    /// <summary>Contains utility methods for creating and working with tuple.</summary>
    public static class Tuple
    {
        /// <summary>Creates a new 2-tuple, or pair. </summary>
        ///  <returns> A 2-tuple whose value is (<paramref name="item1"/>, <paramref name="item2"/>). </returns>
        /// <param name="item1">The value of the first component of the tuple.</param><param name="item2">The value of the second component of the tuple.</param><typeparam name="T1">The type of the first component of the tuple.</typeparam><typeparam name="T2">The type of the second component of the tuple.</typeparam>
        public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        {
            return new Tuple<T1, T2>(item1, item2);
        }
    }

    /// <summary>Represents a 2-tuple, or pair. </summary>
    /// <typeparam name="T1">The type of the tuple's first component.</typeparam><typeparam name="T2">The type of the tuple's second component.</typeparam><filterpriority>2</filterpriority>
    public sealed class Tuple<T1, T2> : ITuple
    {
        private readonly T1 _item1;
        private readonly T2 _item2;

        /// <summary> Gets the value of the first component.</summary>
        public T1 Item1 { get { return _item1; } }

        /// <summary> Gets the value of the current second component. </summary>
        public T2 Item2 { get { return _item2; } }

        int ITuple.Size { get { return 2; } }

        /// <summary>Initializes a new instance of the <see cref="T:System.Tuple`2"/> class.</summary>
        /// <param name="item1">The value of the tuple's first component.</param><param name="item2">The value of the tuple's second component.</param>
        public Tuple(T1 item1, T2 item2)
        {
            _item1 = item1;
            _item2 = item2;
        }

        /// <summary> Returns a value that indicates whether the current <see cref="T:System.Tuple`2"/> object is equal to a specified object.</summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns> true if the current instance is equal to the specified object; otherwise, false. </returns>
        public override bool Equals(object obj)
        {
            var other = obj as Tuple<T1, T2>;
            return other != null && Equals(other.Item1, Item1) && Equals(other.Item2, Item2);
        }

        /// <summary> Returns the hash code for the current <see cref="T:System.Tuple`2"/> object. </summary>
        /// <returns> A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            var h1 = _item1 == null ? 0 : _item1.GetHashCode();
            var h2 = _item2 == null ? 0 : _item2.GetHashCode();
            return (h1 << 5) + h1 ^ h2;
        }

        int ITuple.GetHashCode(IEqualityComparer ignored)
        {
            return GetHashCode();
        }

        /// <summary> Returns a string that represents the value of this <see cref="T:System.Tuple`2"/> instance. </summary>
        /// <returns> The string representation of this <see cref="T:System.Tuple`2"/> object. </returns>
        public override string ToString()
        {
            return ((ITuple)this).ToString(new StringBuilder().Append("("));
        }

        string ITuple.ToString(StringBuilder sb)
        {
            return sb.Append(_item1).Append(", ").Append(_item2).Append(")").ToString();
        }
    }
}