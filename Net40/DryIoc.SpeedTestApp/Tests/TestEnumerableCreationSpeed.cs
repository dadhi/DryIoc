using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.SpeedTestApp.Tests
{
    public class TestEnumerableCreationSpeed
    {
        [Test]
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
                result = new ItemsConsumer(new ItemEnumerable());
            }
            timer.Stop();
            Console.WriteLine("Generated ItemEnumerable took {0} milliseconds to complete.", timer.ElapsedMilliseconds);

            GC.Collect();
            timer = Stopwatch.StartNew();
            for (int t = 0; t < times; t++)
            {
                result = new ItemsConsumer(new ItemCollection<IItem>(CreateItem, 5));
            }
            timer.Stop();
            Console.WriteLine("Generated ItemCollection took {0} milliseconds to complete.", timer.ElapsedMilliseconds);

            GC.Collect();
            timer = Stopwatch.StartNew();
            for (int t = 0; t < times; t++)
            {
                result = new ItemsConsumer(new FixedSizeArrayEnumerable<IItem>(() => new IItem[]
                {
                    new SomeItem(), 
                    new AnotherItem(), 
                    new YetAnotherItem(),
                    new YetAnotherItem(),
                    new YetAnotherItem()
                },
                5));
            }
            timer.Stop();
            Console.WriteLine("ArrayEnumerable took {0} milliseconds to complete.", timer.ElapsedMilliseconds);

            GC.KeepAlive(result);
        }

        private static IEnumerable<IItem> GetItems()
        {
            yield return new SomeItem();
            yield return new AnotherItem();
            yield return new YetAnotherItem();
            yield return new YetAnotherItem();
            yield return new YetAnotherItem();
        }

        private static IItem CreateItem(int index)
        {
            IItem current = null;
            switch (index)
            {
                case 0:
                    current = new SomeItem();
                    break;
                case 1:
                    current = new AnotherItem();
                    break;
                case 2:
                    current = new YetAnotherItem();
                    break;
                case 3:
                    current = new YetAnotherItem();
                    break;
                case 4:
                    current = new YetAnotherItem();
                    break;
            }

            return current;
        }
    }

    public sealed class FixedSizeArrayEnumerable<T> : ICollection<T>
    {
        private readonly Lazy<T[]> _items;

        public FixedSizeArrayEnumerable(Func<T[]> getItems, int size)
        {
            _items = new Lazy<T[]>(getItems);
            Count = size;
        }

        public bool IsReadOnly { get { return true; } }

        public void Add(T item) { throw new NotSupportedException(); }
        public void Clear() { throw new NotSupportedException(); }
        public bool Remove(T item) { throw new NotSupportedException(); }

        public int Count { get; private set; }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_items.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Contains(T item)
        {
            return _items.Value.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _items.Value.CopyTo(array, arrayIndex);
        }
    }

    internal sealed class ItemCollection<T> : ICollection<T>
    {
        private readonly Func<int, T> _getItem;

        public ItemCollection(Func<int, T> getItem, int count)
        {
            _getItem = getItem;
            Count = count;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new InternalEnumerator(_getItem, Count);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal sealed class InternalEnumerator : IEnumerator<T>
        {
            private readonly Func<int, T> _getItem;
            private readonly int _count;
            private int _index;

            public InternalEnumerator(Func<int, T> getItem, int count)
            {
                _getItem = getItem;
                _count = count;
            }

            public T Current { get; private set; }

            public bool MoveNext()
            {
                if (_index < _count)
                {
                    Current = _getItem(_index);
                    ++_index;
                    return true;
                }

                return false;
            }

            public void Reset()
            {
                _index = -1;
            }

            public void Dispose() { }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }

        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public int Count { get; private set; }

        public bool IsReadOnly { get { return true; } }
    }

    internal sealed class ItemEnumerable : IEnumerable<IItem>
    {
        public IEnumerator<IItem> GetEnumerator()
        {
            return new ItemEnumenrator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal sealed class ItemEnumenrator : IEnumerator<IItem>
        {
            private int _state;
            private IItem _current;

            public IItem Current
            {
                get { return _current; }
            }

            //public bool MoveNext()
            //{
            //    _current = MoveNext(_state++);
            //    return _current != null;
            //}

            public bool MoveNext()
            {
                switch (_state)
                {
                    case 0:
                        _current = new SomeItem();
                        break;
                    case 1:
                        _current = new AnotherItem();
                        break;
                    case 2:
                        _current = new YetAnotherItem();
                        break;
                    case 3:
                        _current = new YetAnotherItem();
                        break;
                    case 4:
                        _current = new YetAnotherItem();
                        break;
                }

                return ++_state != 6;
            }

            private static IItem MoveNext(int state)
            {
                IItem current = null;
                switch (state)
                {
                    case 0:
                        current = new SomeItem();
                        break;
                    case 1:
                        current = new AnotherItem();
                        break;
                    case 2:
                        current = new YetAnotherItem();
                        break;
                    case 3:
                        current = new YetAnotherItem();
                        break;
                    case 4:
                        current = new YetAnotherItem();
                        break;
                }

                return current;
            }

            public void Reset()
            {
                _state = 0;
            }

            public void Dispose()
            {
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
