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

        public interface IIncomingRequestInterceptor<T> { }
        private class FooInterceptor<T> : IIncomingRequestInterceptor<T> { }
        private class BarInterceptor<T> : IIncomingRequestInterceptor<T> { }
    }
}
