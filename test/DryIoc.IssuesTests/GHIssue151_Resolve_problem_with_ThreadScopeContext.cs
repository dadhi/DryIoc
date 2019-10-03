using System;
using System.Collections.Generic;
using System.Threading;
using DryIoc;
using NUnit.Framework;

namespace DryIocTest
{
    class A
    {
        public readonly int Id = Thread.CurrentThread.ManagedThreadId;
    }

    class B
    {
        private readonly A _a;

        public B(A a)
        {
            _a = a;
        }

        public bool IsSameThread() =>
            Thread.CurrentThread.ManagedThreadId == _a.Id;
    }

    [TestFixture]
    public class GHIssue151_Resolve_problem_with_ThreadScopeContext
    {

        public class Count
        {
            public int Value;
        }

        [Test]
        public void Simplest_Test()
        {
            var container = new Container(scopeContext: new ThreadScopeContext());
            container.Register<B>();

            using (var scope1 = container.OpenScope())
            {
                scope1.Use(new A());
                var b = container.Resolve<B>();
                if (!b.IsSameThread())
                    Assert.Fail("It is not the same thread");
            }

            using (var scope2 = container.OpenScope())
            {
                scope2.Use(new A());
                var b = container.Resolve<B>();
                if (!b.IsSameThread())
                    Assert.Fail("It is not the same thread");
            }
        }

        [Test][Ignore("todo: fix me")]
        public void Test()
        {
            var container = new Container(scopeContext: new ThreadScopeContext());
            container.Register<B>();

            var count = new Count();

            var threads = new List<Thread>();
            for (var i = 0; i < 5; i++)
            {
                var thread = new Thread(_ => Work(container, count));
                threads.Add(thread);
                thread.Start();
            }

            foreach (var thread in threads)
                thread.Join();
            
            Assert.AreEqual(5, count.Value);
        }

        private void Work(IContainer container, Count count)
        {
            using (var scope1 = container.OpenScope())
            {
                scope1.Use(new A());
                TestDryIoc(container, count);
            }

            using (var scope2 = container.OpenScope())
            {
                scope2.Use(new A());
                TestDryIoc(container, count);
            }
        }

        private void TestDryIoc(IContainer container, Count count)
        {
            var c = Interlocked.Increment(ref count.Value);
            try
            {
                var b = container.Resolve<B>();
                if (!b.IsSameThread())
                    Assert.Fail($"Invalid object on count {c}");
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
    }
}
