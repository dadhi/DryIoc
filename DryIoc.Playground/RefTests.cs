using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace DryIoc.Playground
{
    [TestFixture]
    //[Ignore]
    public class RefTests
    {
        [Test]
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
                    itemsRef.Update(xs => xs.Reverse().ToArray());
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

    public static class Ref
    {
        public static Ref<T> Of<T>(T value) where T : class
        {
            return new Ref<T>(value);
        }
    }

    public sealed class Ref<T> where T : class
    {
        public T Value { get { return _value; } }

        public Ref(T initialValue = default(T))
        {
            _value = initialValue;
        }

        public T Update(Func<T, T> update)
        {
            var retryCount = 0;
            while (true)
            {
                var oldValue = _value;
                var newValue = update(oldValue);
                if (Interlocked.CompareExchange(ref _value, newValue, oldValue) == oldValue)
                    return oldValue;
                if (++retryCount > RETRY_COUNT_UNTIL_THROW)
                    throw new InvalidOperationException(ERROR_EXCEEDED_RETRY_COUNT);
            }
        }

        private T _value;

        private const int RETRY_COUNT_UNTIL_THROW = 10;
        private static readonly string ERROR_EXCEEDED_RETRY_COUNT =
            "Ref retried to Update for " + RETRY_COUNT_UNTIL_THROW + " times But there is always someone else intervened.";
    }
}
