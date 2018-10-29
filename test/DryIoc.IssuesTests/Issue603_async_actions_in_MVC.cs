using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue603_async_actions_in_MVC
    {
        [Test]
        public async Task Test()
        {
            var container = new Container(scopeContext: new AsyncExecutionFlowScopeContext());
            container.Register<MyService>(Reuse.Scoped);

            var scope = container.OpenScope();
            var service1 = scope.Resolve<MyService>();
            var service2 = await Hello(scope);

            Assert.AreSame(service1, service2);
        }

        public async Task<MyService> Hello(IResolverContext container)
        {
            await Task.Delay(5).ConfigureAwait(false);
            var task = Task.Run(() => container.Resolve<MyService>());
            return await task;
        }

        public class MyService {}
    }
}
