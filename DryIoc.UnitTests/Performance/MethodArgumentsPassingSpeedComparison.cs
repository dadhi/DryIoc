using System;
using System.Diagnostics;

namespace DryIoc.UnitTests.Performance
{
    public static class MethodArgumentsPassingSpeedComparison
    {
        public static object GetResult(string param)
        {
            return "Hey " + param;
        }

        public static void OutResult(string param, out object result)
        {
            result = "Hey " + param;
        }

        public static void Compare()
        {
            var times = 1 * 1000 * 1000;

            object result = null;
            Stopwatch timer;
            GC.Collect();


            timer = Stopwatch.StartNew();
            for (int t = 0; t < times; t++)
            {
                result = GetResult("blah!");
            }
            timer.Stop();
            Console.WriteLine("GetResult took {0} milliseconds to complete.", timer.ElapsedMilliseconds);
            GC.Collect();


            timer = Stopwatch.StartNew();
            for (int t = 0; t < times; t++)
            {
                OutResult("blah!", out result);
            }
            timer.Stop();
            Console.WriteLine("OutResult took {0} milliseconds to complete.", timer.ElapsedMilliseconds);
            GC.Collect();

            GC.KeepAlive(result);
        }
    }
}
