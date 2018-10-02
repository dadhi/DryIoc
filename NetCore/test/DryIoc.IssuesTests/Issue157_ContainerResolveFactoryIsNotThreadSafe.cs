using System.Diagnostics;
using System.Threading;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue157_ContainerResolveFactoryIsNotThreadSafe
    {
        [Test]
        public void Should_not_throw_ResolveFromMultiple()
        {
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.ElapsedMilliseconds < 300)
            {
                // The issue is stochastic, meaning we might not hit it on first run
                var c = new Container();
                c.Register(typeof(C<>));
                var m = new object();

                for (var i = 0; i < 5; i++) // Create 10 threads waiting for a pulse and then resolving dependency
                    new Thread(() =>
                    {
                        lock (m) Monitor.Wait(m);
                        c.Resolve<C<int>>();
                    }).Start();

                Thread.Sleep(30); // Wait for all threads to start and go to sleep
                lock (m) Monitor.PulseAll(m); // Wake up all threads at once
            }
        }

        public class C<T> { }
    }
}
