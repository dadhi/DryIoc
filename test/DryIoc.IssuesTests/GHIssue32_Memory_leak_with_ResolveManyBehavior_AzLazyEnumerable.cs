using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.dotMemoryUnit;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue32_Memory_leak_with_ResolveManyBehavior_AzLazyEnumerable : ITest
    {
        public int Run()
        {
            Test();
            Test_memory_allocations();
            return 2;
        }

        [Test]
        public void Test()
        {
            var c = new Container();
            c.Register(typeof(IIncomingRequestInterceptor<>), typeof(FooInterceptor<>));
            c.Register(typeof(IIncomingRequestInterceptor<>), typeof(BarInterceptor<>));

            var interceptorType = typeof(IIncomingRequestInterceptor<>).MakeGenericType(typeof(int));

            var interceptors1 = c.ResolveMany(interceptorType, behavior: ResolveManyBehavior.AsLazyEnumerable).ToList();
            var interceptors2 = c.ResolveMany(interceptorType, behavior: ResolveManyBehavior.AsLazyEnumerable).ToList();

            Assert.AreEqual(2, interceptors1.Count);
            Assert.AreEqual(2, interceptors2.Count);
        }

        [Conditional("DEBUG")]
        void Print(object x) => Console.WriteLine(x);

        [DotMemoryUnit(CollectAllocations = true, FailIfRunWithoutSupport = false)]
        [Test]
        public void Test_memory_allocations()
        {
            var c = new Container();
            c.Register(typeof(IIncomingRequestInterceptor<>), typeof(FooInterceptor<>));
            c.Register(typeof(IIncomingRequestInterceptor<>), typeof(BarInterceptor<>));

            var memoryCheckPoint = dotMemory.Check();

            var interceptorType = typeof(IIncomingRequestInterceptor<>).MakeGenericType(typeof(int));

            var interceptors1 = c.ResolveMany(interceptorType).ToList();
            var interceptors2 = c.ResolveMany(interceptorType).ToList();

            dotMemory.Check(memory =>
                Assert.That(memory.GetDifference(memoryCheckPoint)
                    .GetNewObjects(where => where.Type.Like("DryIoc.FactoryDelegate")).ObjectsCount, Is.EqualTo(2)));

            dotMemory.Check(memory =>
            {
                Print("## Check 1");

                var newObjects = memory.GetDifference(memoryCheckPoint)
                    .GetNewObjects(x => x.Namespace.Like("DryIoc"))
                    .GroupByType();

                foreach (var info in newObjects)
                    Print(info);
                Print("");
            });


            var memoryCheckPoint2 = dotMemory.Check();

            var interceptors3 = c.ResolveMany(interceptorType).ToList();
            var interceptors4 = c.ResolveMany(interceptorType).ToList();

            // no new factory delegates should be created, everything is taken from cache
            dotMemory.Check(memory =>
                Assert.That(memory.GetDifference(memoryCheckPoint2)
                    .GetNewObjects(where => where.Type.Like("DryIoc.FactoryDelegate")).ObjectsCount, Is.EqualTo(0)));

            dotMemory.Check(memory =>
            {
                Print("## Check 2");

                var newObjects = memory.GetDifference(memoryCheckPoint2)
                    .GetNewObjects(x => x.Namespace.Like("DryIoc"))
                    .GroupByType();

                foreach (var info in newObjects)
                    Print(info);
                Print("");
            });

            // store into field to prevent collection
            Interceptors = new List<object> { interceptors1, interceptors2, interceptors3, interceptors4 };
        }

        public List<object> Interceptors;
        public interface IIncomingRequestInterceptor<T> { }
        private class FooInterceptor<T> : IIncomingRequestInterceptor<T> { }
        private class BarInterceptor<T> : IIncomingRequestInterceptor<T> { }
    }
}
