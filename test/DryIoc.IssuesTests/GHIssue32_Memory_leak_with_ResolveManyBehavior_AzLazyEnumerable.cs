using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue32_Memory_leak_with_ResolveManyBehavior_AzLazyEnumerable
    {
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

        [Test]
        public void Test_memory_allocations()
        {
            var c = new Container();
            c.Register(typeof(IIncomingRequestInterceptor<>), typeof(FooInterceptor<>));
            c.Register(typeof(IIncomingRequestInterceptor<>), typeof(BarInterceptor<>));

            var interceptorType = typeof(IIncomingRequestInterceptor<>).MakeGenericType(typeof(int));

            var interceptors1 = c.ResolveMany(interceptorType).ToList();
            var interceptors2 = c.ResolveMany(interceptorType).ToList();

            var interceptors3 = c.ResolveMany(interceptorType).ToList();
            var interceptors4 = c.ResolveMany(interceptorType).ToList();

            // store into field to prevent collection
            Interceptors = new List<object> { interceptors1, interceptors2, interceptors3, interceptors4 };
        }

        public List<object> Interceptors;
        public interface IIncomingRequestInterceptor<T> { }
        private class FooInterceptor<T> : IIncomingRequestInterceptor<T> { }
        private class BarInterceptor<T> : IIncomingRequestInterceptor<T> { }
    }
}
