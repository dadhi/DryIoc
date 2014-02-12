using System;
using System.Diagnostics;

namespace DryIoc.Playground.Performance
{
    public static class CompareTryGetVsGetOrDefault
    {
        public static void Compare()
        {
            var times = 5 * 1000 * 1000;

            var value = new object();
            var holder = new Holder(value);
            var type = typeof(object);
            object result = null;
            Stopwatch timer;
            GC.Collect();

            timer = Stopwatch.StartNew();
            for (int t = 0; t < times; t++)
            {
                object r;
                if (holder.TryGetValue(out r, type))
                    result = r;
            }
            timer.Stop();
            Console.WriteLine("TryGetValue took {0} milliseconds to complete.", timer.ElapsedMilliseconds);
            GC.Collect();

            timer = Stopwatch.StartNew();
            for (int t = 0; t < times; t++)
            {
                result = holder.GetValueOrDefault(type) ?? value;
            }
            timer.Stop();
            Console.WriteLine("GetValueOrDefault took {0} milliseconds to complete.", timer.ElapsedMilliseconds);
            GC.Collect();

            GC.KeepAlive(result);
        }
    }

    class Holder
    {
        private readonly object _value;

        public Holder(object value)
        {
            _value = value;
        }

        public bool TryGetValue(out object value, Type key)
        {
            if (key == typeof(object))
            {
                value = _value;
                return true;
            }

            value = null;
            return false;
        }

        public object GetValueOrDefault(Type key)
        {
            return key == typeof(object) ? _value : null;
        }
    }
}
