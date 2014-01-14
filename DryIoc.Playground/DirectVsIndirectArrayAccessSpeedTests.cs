using System;
using System.Diagnostics;

namespace DryIoc.Playground
{
    public class DirectVsIndirectArrayAccessSpeedTests
    {
        public static void Compare()
        {
            var times = 10 * 1000000;
            var itemCount = 20;
            
            var arr = new object[itemCount];
            for (int i = 0; i < itemCount; i++)
                arr[i] = i;

            var arrWrapper = new ArrayWrapper(arr);

            object result = null;
            
            GC.Collect();
            var watch = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                result = arr[13];
            }
            watch.Stop();
            var direct = watch.ElapsedMilliseconds;

            GC.Collect();
            watch = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                result = arrWrapper.Get(13);
            }
            watch.Stop();
            var indirect = watch.ElapsedMilliseconds;

            GC.KeepAlive(result);
            Console.WriteLine("direct: " + direct);
            Console.WriteLine("indirect: " + indirect);
        }
    }

    sealed class ArrayWrapper
    {
        private object[] _arr;

        public ArrayWrapper(object[] arr)
        {
            _arr = arr;
        }

        public object Get(int i)
        {
            return _arr[i];
        }
    }
}
