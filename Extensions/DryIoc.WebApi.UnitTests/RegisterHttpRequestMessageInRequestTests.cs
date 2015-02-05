using System.Threading.Tasks;
using NUnit.Framework;

namespace DryIoc.WebApi.UnitTests
{
    [TestFixture]
    public class RegisterHttpRequestMessageInRequestTests
    {
        public class MessageRequest {}

        public class A
        {
            public MessageRequest Request { get; set; }

            public A(MessageRequest r)
            {
                Request = r;
            }
        }

        [Test]
        public void ScopeTest()
        {
            // Create container with AsyncExecutionFlowScopeContext which works across Async/Await boundaries.
            // In case of MVC it may be changed to HttpContextScopeContext.
            // If not specified container will use ThreadScopeContext.
            var container = new Container( 
                scopeContext: new AsyncExecutionFlowScopeContext());

            container.Register<A>();

            // Register Null request in parent container in order to swap to actual request in current scope.
            // When resolving A container will find registered request dependency and cache access to it for fast performance.

            var task1 = Task.Run(async () =>
            {
                var request = new MessageRequest();
                using (var scope = container.OpenScope())
                {
                    // Resolve request as early registered ReuseSwapable.
                    // and swap its current value (null) with your request.
                    // It will replace request instance inside current scope, keep all resolution cache, etc intact. It is fast.
                    scope.RegisterInstance(request, Reuse.InCurrentScope);
                    //scope.Resolve<ReuseSwapable>(typeof(MessageRequest)).Swap(request);

                    var a = scope.Resolve<A>();
                    await Task.Delay(5);//processing request
                    Assert.AreSame(a.Request, scope.Resolve<A>().Request);
                }
            });

            var task2 = Task.Run(async () =>
            {
                var request = new MessageRequest();
                using (var scope = container.OpenScope())
                {
                    scope.RegisterInstance(request, Reuse.InCurrentScope);

                    var a = scope.Resolve<A>();
                    await Task.Delay(2);//processing request
                    Assert.AreSame(a.Request, scope.Resolve<A>().Request);
                }
            });

            Task.WaitAll(task1, task2);
        }
    }
}
