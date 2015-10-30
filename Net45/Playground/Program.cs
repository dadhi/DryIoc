using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Playground
{
    class Program
    {
        static void Main()
        {
            var result = ForeachOfArrayVsCustomEnumerable();
            Console.WriteLine(result);

            Console.ReadKey();
        }

        private static string ForeachOfArrayVsCustomEnumerable()
        {
            var array = new[] { "a", "b", "c", "d", "e" };
            var result  = " ";

            const int times = 5000000;

            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                foreach (var item in array)
                {
                    result = item;
                }
            }
            stopwatch.Stop();
            Console.WriteLine("Array: " + stopwatch.ElapsedMilliseconds);

            var items = Items.Of("a", Items.Of("a", Items.Of("c", Items.Of("d", Items.Of("e", null)))));

            stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                foreach (var item in items)
                {
                    result = item;
                }
            }
            stopwatch.Stop();
            Console.WriteLine("Array: " + stopwatch.ElapsedMilliseconds);

            return result;
        }
    }

    public static class Items
    {
        public static Items<T> Of<T>(T item, Items<T> next)
        {
            return new Items<T>(item, next);
        }
    }

    public sealed class Items<T> : IEnumerable<T>
    {
        public readonly T Item;
        public readonly Items<T> Next;
        private Enumerator _enumerator;

        public Items(T item, Items<T> next)
        {
            Item = item;
            Next = next;
            _enumerator = new Enumerator(this);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _enumerator;
        }

        private sealed class Enumerator : IEnumerator<T>
        {
            private Items<T> _items;

            public T Current
            {
                get { return _items.Item; }
            }

            public Enumerator(Items<T> items)
            {
                _items = items;
            }

            public bool MoveNext()
            {
                return (_items = _items.Next) != null;
            }

            public void Reset()
            {
            }

            public void Dispose()
            {
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
