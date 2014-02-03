using System;
using System.Diagnostics;

namespace DryIoc.Playground
{
    public static class BitOpVsIsOpSpeedTests
    {
        public static void Compare()
        {
            var times = 10 * 1000 * 1000;

            object item = new NotAValue();
            var index = 9;
            var bitmap = 1u << index; 

            var result = false;

            GC.Collect();
            var watch = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                result = item is NotAValue;
            }
            watch.Stop();
            var isOp = watch.ElapsedMilliseconds;

            GC.Collect();
            watch = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                result = ((bitmap >> index) & 1) == 1;
            }
            watch.Stop();
            var bitOp = watch.ElapsedMilliseconds;

            GC.KeepAlive(result);
            Console.WriteLine("Is op: " + isOp);
            Console.WriteLine("Bit op: " + bitOp);
        }
    }

    public sealed class NotAValue {}
}
