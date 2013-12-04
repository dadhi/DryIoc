using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace DryIoc.SpeedTestApp.Net40.Tests
{
    public class TestArrayCreationSpeed
    {
        public static void Compare()
        {
            var times = 1 * 1000 * 1000;

            ItemsConsumer result = null;
            Stopwatch timer;
            GC.Collect();

            timer = Stopwatch.StartNew();
            for (int t = 0; t < times; t++)
            {
                result = new ItemsConsumer(new IItem[]
                {
                    new SomeItem(), 
                    new AnotherItem(), 
                    new YetAnotherItem(),
                    new YetAnotherItem(),
                    new YetAnotherItem()
                });
            }
            timer.Stop();
            Console.WriteLine("Array initializer took {0} milliseconds to complete.", timer.ElapsedMilliseconds);

            GC.Collect();
            timer = Stopwatch.StartNew();
            for (int t = 0; t < times; t++)
            {
                result = new ItemsConsumer(GetItems());
            }
            timer.Stop();
            Console.WriteLine("Lazy Enumerable took {0} milliseconds to complete.", timer.ElapsedMilliseconds);

            GC.Collect();
            timer = Stopwatch.StartNew();
            for (int t = 0; t < times; t++)
            {
                result = new ItemsConsumer(new Chained<IItem>(
                    new SomeItem(), new Chained<IItem>(
                    new AnotherItem(), new Chained<IItem>(
                    new YetAnotherItem(), new Chained<IItem>(
                    new YetAnotherItem(), new Chained<IItem>(
                    new YetAnotherItem()))))));
            }
            timer.Stop();
            Console.WriteLine("Chained took {0} milliseconds to complete.", timer.ElapsedMilliseconds);

            GC.KeepAlive(result);

            var items = new List<IItem>();
        }

        private static IEnumerable<IItem> GetItems()
        {
            yield return new SomeItem();
            yield return new AnotherItem();
            yield return new YetAnotherItem();
            yield return new YetAnotherItem();
            yield return new YetAnotherItem();
        }
    }

    public sealed class Chained<T> : IEnumerable<T>
    {
        public readonly T Value;
        public readonly Chained<T> Next;

        public Chained(T value, Chained<T> next = null)
        {
            Value = value;
            Next = next;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new ChainedEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private sealed class ChainedEnumerator : IEnumerator<T>
        {
            private Chained<T> _current;

            public ChainedEnumerator(Chained<T> source)
            {
                _current = new Chained<T>(default(T), source);
            }

            public T Current
            {
                get { return _current.Value; }
            }

            public bool MoveNext()
            {
                return (_current = _current.Next) == null;
            }

            public void Dispose() { }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }
    }

    internal interface IItem { }

    class SomeItem : IItem { }
    class AnotherItem : IItem { }
    class YetAnotherItem : IItem { }

    class ItemsConsumer
    {
        public int Count;

        public ItemsConsumer(IEnumerable<IItem> items)
        {
            Count = items.Count();
        }
    }
}
