using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.UnitTests.Performance
{
    public class HashTreeEnumerationSpeedTests
    {
        public static void CompareListVsHashTree()
        {
            var times = 1 * 1000 * 1000;
            var itemCount = 20;
            Console.WriteLine("Enumerating {0} items {1:n0} times in for cycle: ", itemCount, times);

            var list = new List<string>(itemCount);
            var tree = HashTree<string>.Empty;

            foreach (var i in Enumerable.Range(0, itemCount))
            {
                list.Add(i.ToString());
                tree = tree.AddOrUpdate(i, i.ToString());
            }

            Stopwatch timer;
            var result = string.Empty;
            GC.Collect();


            timer = Stopwatch.StartNew();
            for (int t = 0; t < times; t++)
            {
                foreach (var i in list)
                {
                    result = i;
                }
            }
            timer.Stop();
            Console.WriteLine("List enumeration took {0} milliseconds to complete.", timer.ElapsedMilliseconds);
            GC.Collect();


            timer = Stopwatch.StartNew();
            for (int t = 0; t < times; t++)
            {
                foreach (var i in tree)
                {
                    result = i.Value;
                }
            }
            timer.Stop();
            Console.WriteLine("Tree enumeration took {0} milliseconds to complete.", timer.ElapsedMilliseconds);
            GC.Collect();

            GC.KeepAlive(result);
        }
    }
}