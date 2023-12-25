using DryIoc.Microsoft.DependencyInjection;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue387_Nested_container_returns_a_new_instance_for_singletons : ITest
    {
        public int Run()
        {
            Test1();
            return 1;
        }

        [Test]
        public void Test1()
        {
            var c = new Container(DryIocAdapter.MicrosoftDependencyInjectionRules);

            c.Register<IMemoryCache, MemCache>(Reuse.Singleton);

            IMemoryCache cache1 = null;

            // cache1 = c.Resolve<IMemoryCache>();

            using (var nested = c.CreateChild())
            {
                using (var scope = nested.OpenScope())
                {
                    cache1 = scope.Resolve<IMemoryCache>();
                    var cache2 = scope.Resolve<IMemoryCache>();

                    Assert.AreSame(cache2, cache1);
                }

                var cache3 = nested.Resolve<IMemoryCache>();
                Assert.AreSame(cache3, cache1);
            }

            // Uncomment the line above for this assert to pass.
            // var cache4 = c.Resolve<IMemoryCache>();
            // Assert.AreSame(cache4, cache1);
        }

        interface IMemoryCache {}
        class MemCache : IMemoryCache {}
    }
}