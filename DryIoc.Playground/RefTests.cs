using System;
using System.Threading;
using NUnit.Framework;

namespace DryIoc.Playground
{
    [TestFixture]
    [Ignore]
    public class RefTests
    {
        [Test]
        public void Test()
        {
        }

        //private const int s_spinCount = 4000;

        //public void Wait()
        //{
        //    var s = new SpinWait();
        //    while (m_remain > 0)
        //    {
        //        if (s.Spin() >= s_spinCount) 
        //            m_event.WaitOne();
        //    }
        //}
    }

    public struct SpinWait
    {
        private int _count;
        private static readonly bool _isSingleProc = Environment.ProcessorCount == 1;
        private const int YIELD_FREQUENCY = 4000;
        private const int YIELD_ONE_FREQUENCY = 3 * YIELD_FREQUENCY;

        public int Spin()
        {
            var oldCount = _count;

            // On a single-CPU machine, we ensure our counter is always 
            // a multiple of YIELD_FREQUENCY, so we yield every time. 
            // Else, we just increment by one. 
            _count += _isSingleProc ? YIELD_FREQUENCY : 1;

            // If not a multiple of YIELD_FREQUENCY spin (w/ back-off). 
            var countModFrequency = _count % YIELD_FREQUENCY;
            if (countModFrequency > 0)
                Thread.SpinWait((int)(1 + countModFrequency * 0.05f));
            else
                Thread.Sleep(_count <= YIELD_ONE_FREQUENCY ? 0 : 1);

            return oldCount;
        }

        private void Yield()
        {
            Thread.Sleep(_count < YIELD_ONE_FREQUENCY ? 0 : 1);
        }
    }

    public sealed class Ref<T> where T : class
    {
        public T Value { get { return _value; } }

        public T Swap(T value)
        {
            return Interlocked.Exchange(ref _value, value);
        }

        private T _value;
    }
}



