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

namespace System
{
    /// <summary>Represents a 2-tuple, or pair.</summary>
    /// <typeparam name="T1">The type of the first component.</typeparam>
    /// <typeparam name="T2">The type of the second component.</typeparam>
    public class Tuple<T1, T2>
    {
        /// <summary>Gets the first component.</summary>
        public T1 Item1 { get; private set; }

        /// <summary>Gets the second component.</summary>
        public T2 Item2 { get; private set; }

        /// <summary>Initializes a new instance of the <see cref="Tuple{T1, T2}"/> class.</summary>
        /// <param name="item1">The first component.</param>
        /// <param name="item2">The second component.</param>
        public Tuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }

    /// <summary>Provides static methods for creating tuple objects.</summary>
    public static class Tuple
    {
        /// <summary>
        /// Creates a new 2-tuple, or pair.
        /// </summary>
        /// <typeparam name="T1">The type of the first component.</typeparam>
        /// <typeparam name="T2">The type of the second component.</typeparam>
        /// <param name="item1">The first component.</param>
        /// <param name="item2">The second component.</param>
        /// <returns></returns>
        public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        {
            var tuple = new Tuple<T1, T2>(item1, item2);
            return tuple;
        }
    }
}
