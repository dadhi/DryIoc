using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.SpeedTestApp.Net40.Tests
{
    public class TestImmutableCollectionSpeeds
    {
        [Test]
        [Ignore]
        public static void TestAccess()
        {
            var count = 100 * 1000;
            var result = 0;
            var list = new List<int>(Enumerable.Range(0, count));

            GC.Collect();
            var listsp = Stopwatch.StartNew();

            for (int i = 0; i < list.Count; i++)
            {
                result = list[i];
            }

            Console.WriteLine(listsp.Elapsed);

            var ilist = HashTree<int, int>.Empty;
            list.ForEach(i => ilist = ilist.AddOrUpdate(i, i));
            GC.Collect();
            listsp.Restart();

            for (int i = 0; i < list.Count; i++)
            {
               result = ilist.GetValueOrDefault(i);
            }
            
            Console.WriteLine(listsp.Elapsed);
            
            GC.KeepAlive(result);
        }
    }
}
