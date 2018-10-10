using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace ImTools.UnitTests
{
    [TestFixture]
    public class RefTests
    {
        [Explicit("Multi-threaded non deterministic test")]
        public void Consistently_updated_by_multiple_threads()
        {
            const int itemCount = 10;
            var items = new int[itemCount];
            for (var i = 0; i < itemCount; i++)
                items[i] = i;

            var itemsRef = Ref.Of(items.ToArray());

            const int threadCount = 10;
            var latch = new CountdownLatch(threadCount);

            for (var i = threadCount - 1; i >= 0; i--)
            {
                var delay = i * 10;
                var thread = new Thread(() =>
                {
                    Thread.Sleep(delay);
                    itemsRef.Swap(xs => xs.Reverse().ToArray());
                    latch.Signal();
                }) { IsBackground = true };
                thread.Start();
            }

            latch.Wait();

            CollectionAssert.AreEqual(items, itemsRef.Value);
        }

        private sealed class CountdownLatch
        {
            public CountdownLatch(int count)
            {
                _remain = count;
                _event = new ManualResetEvent(false);
            }

            public void Signal()
            {
                // The last thread to signal also sets the event. 
                if (Interlocked.Decrement(ref _remain) == 0)
                    _event.Set();
            }

            public void Wait()
            {
                _event.WaitOne();
            }

            private int _remain;
            private readonly EventWaitHandle _event;
        }
    }
}
