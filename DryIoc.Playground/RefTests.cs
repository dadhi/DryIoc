using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace DryIoc.Playground
{
    [TestFixture]
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
        public static Ref<T> Of<T>(T value)
        {
            return new Ref<T>(value);
        }
    }

    public sealed class Ref<T>
    {
        public const int NUMBER_OF_RETRIES = 10;
        public const int NUMBER_OF_AWAITS = 100;

        public T Value { get { return _value; } }

        public Ref(T initialValue)
        {
            _value = initialValue;
        }

        public T Update(Func<T, T> update, Func<T, T> makeSnapshot = null)
        {
            makeSnapshot = makeSnapshot ?? UseOriginalValue;

            var retryCount = 0;
            while (retryCount++ < NUMBER_OF_RETRIES)
            {
                var version = _version; // remember snapshot version locally
                var snapshotValue = makeSnapshot(_value);
                var newValue = update(snapshotValue);

                // Await here for finished commit, spin maybe?
                // Why not Thread.Sleep(0): http://joeduffyblog.com/2006/08/22/priorityinduced-starvation-why-sleep1-is-better-than-sleep0-and-the-windows-balance-set-manager/
                var awaitsCount = 0;
                while (_isCommitInProgress == 1 && awaitsCount++ < NUMBER_OF_AWAITS)
                    Thread.Sleep(1);

                // If still/already in progress then retry. Otherwise mark that current code we is committing.
                if (Interlocked.CompareExchange(ref _isCommitInProgress, 1, 0) == 1)
                    continue;

                try
                {
                    // If some other code did not change original value (and version) - means it is consistent, 
                    // then commit update and increment version to signal the change to other code.
                    if (version == _version)
                    {
                        _value = newValue;
                        Interlocked.Increment(ref _version);
                        return snapshotValue; // return snapshot used for update
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref _isCommitInProgress, 0);
                }
            }

            throw new InvalidOperationException("Retried " + NUMBER_OF_RETRIES + " times But there is always someone else intervened.");
        }

        #region Implementation

        private T _value;
        private int _version;
        private int _isCommitInProgress;

        private static T UseOriginalValue(T value)
        {
            return value;
        }

        #endregion
    }
}



