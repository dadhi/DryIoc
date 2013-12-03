using System;
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
