using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;


namespace DryIoc.SpeedTestApp.Net40.Tests
{
    public class TestArrayCreationSpeed
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
    }

    internal class ItemEnumerable : IEnumerable<IItem> 
    {
        public IEnumerator<IItem> GetEnumerator()
        {
            return new ItemEnumenrator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal class ItemEnumenrator : IEnumerator<IItem>
        {
            private int _state;
            
            public IItem Current { get; private set; }

            public bool MoveNext()
            {
                switch (_state)
                {
                    case 0: 
                        Current = new SomeItem(); ++_state;
                        break;
                    case 1:
                        Current = new AnotherItem(); ++_state;
                        break;
                    case 2:
                        Current = new YetAnotherItem(); ++_state;
                        break;
                    case 3:
                        Current = new YetAnotherItem(); ++_state;
                        break;
                    case 4:
                        Current = new YetAnotherItem(); ++_state;
                        break;
                    default:
                        ++_state;
                        break;
                }

                return _state != 6;
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
