using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue_UsingAsyncMethodAsMadeOf : ITest
    {
        public int Run()
        {
            Async_Made_Of().GetAwaiter().GetResult();
            return 1;
        }

        [Test]
        public async Task Async_Made_Of()
        {
            var container = new Container(rules => 
                rules.WithoutThrowOnRegisteringDisposableTransient());
            container.Register(Made.Of(() => GetA()));

            var a = await container.ResolveAsync<A>();

            Assert.IsNotNull(a);
        }

        public static async Task<A> GetA()
        {
            await Task.Delay(TimeSpan.FromMilliseconds(30));
            return await Task.FromResult(new A());
        }

        public class A { }
    }

    public static class ResolveAsyncExt
    {
        public static async Task<T> ResolveAsync<T>(this IContainer container)
        {
            return await container.Resolve<Task<T>>();
        }
    }
}
