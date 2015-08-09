using System.Threading.Tasks;
using NUnit.Framework;

namespace DryIoc.WebApi.UnitTests
{
    [TestFixture]
    public class RegisterHttpRequestMessageInRequestTests
    {
        public class TestRequestMessage {}

        public class A
        {
            public TestRequestMessage Request { get; set; }

            public A(TestRequestMessage r)
            {
                Request = r;
            }
        }

        [Test]
        public void Register_request_message_in_current_scope()
        {
            // Create container with AsyncExecutionFlowScopeContext which works across async/await boundaries.
            // In case of MVC it may be changed to HttpContextScopeContext.
            // If not specified container will use ThreadScopeContext.
            var container = new Container( 
                scopeContext: new AsyncExecutionFlowScopeContext());

            container.Register<A>();

            // Register Null request in parent container in order to swap to actual request in current scope.
            // When resolving A container will find registered request dependency and cache access to it for fast performance.

            const int parallelRequestCount = 20;
            var tasks = new Task[parallelRequestCount];
            for (var i = 0; i < parallelRequestCount; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    var message = new TestRequestMessage();
                    using (var scope = container.OpenScope())
                    {
                        // Resolve request as early registered ReuseSwapable.
                        // and swap its current value (null) with your request.
                        // It will replace request instance inside current scope, keep all resolution cache, etc intact. It is fast.
                        scope.RegisterInstance(message, Reuse.InCurrentScope, IfAlreadyRegistered.Replace);

                        var a = scope.Resolve<A>();
                        await Task.Delay(5);//processing request
                        Assert.NotNull(a.Request);
                        Assert.AreSame(a.Request, scope.Resolve<A>().Request);
                    }
                });
            }

            Task.WaitAll(tasks);
        }
    }
}
