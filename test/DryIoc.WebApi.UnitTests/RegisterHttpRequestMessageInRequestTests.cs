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
            public TestRequestMessage Message { get; set; }

            public A(TestRequestMessage r)
            {
                Message = r;
            }
        }

        [Test] // todo: sometimes fails, so fix this
        public void Register_request_message_in_current_scope()
         {
            // Create container with AsyncExecutionFlowScopeContext which works across async/await boundaries.
            // In case of MVC it may be changed to HttpContextScopeContext.
            var container = new Container(scopeContext: AsyncExecutionFlowScopeContext.Default);

            container.Register<A>();

            const int parallelRequestCount = 20;
            var tasks = new Task[parallelRequestCount];
            for (var i = 0; i < parallelRequestCount; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    var message = new TestRequestMessage();
                    using (var scope = container.OpenScope())
                    {
                        // It will replace request instance inside current scope, keep all resolution cache, etc intact. It is fast.
                        //scope.RegisterInstance(message, IfAlreadyRegistered.Replace);
                        scope.Use(message);

                        var a = scope.Resolve<A>();

                        Assert.AreSame(message, a.Message);

                        await Task.Delay(5); //processing request

                        var a2 = scope.Resolve<A>();
                        Assert.AreSame(a.Message, a2.Message);
                    }
                });
            }

            Task.WaitAll(tasks);
        }
    }
}
