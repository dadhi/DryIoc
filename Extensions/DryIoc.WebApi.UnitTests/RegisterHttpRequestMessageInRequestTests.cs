using System.Threading.Tasks;
using NUnit.Framework;

namespace DryIoc.WebApi.UnitTests
{
    [TestFixture]
    public class RegisterHttpRequestMessageInRequestTests
    {
        public class SomeRequest {}

        public class A
        {
            public SomeRequest Request { get; set; }

            public A(SomeRequest r)
            {
                Request = r;
            }
        }

        [Test]
        public void ScopeTest()
        {
            var container = new Container(scopeContext: new ExecutionFlowScopeContext());

            container.Register<A>();
            container.RegisterInstance(default(SomeRequest),
                WebReuse.InRequest, Setup.With(reuseWrappers: typeof(ReuseSwapable)));

            var request1 = Task.Run(async () =>
            {
                var request = new SomeRequest();
                var scope = container.OpenScope();
                scope.Resolve<ReuseSwapable>(typeof(SomeRequest)).Swap(request);

                await Task.Delay(5);//processing request
                Assert.AreSame(request, scope.Resolve<A>().Request);
            });

            var request2 = Task.Run(async () =>
            {
                var request = new SomeRequest();
                var scope = container.OpenScope();
                scope.Resolve<ReuseSwapable>(typeof(SomeRequest)).Swap(request);
                await Task.Delay(2);//processing request
                Assert.AreSame(request, scope.Resolve<A>().Request);
            });

            Task.WaitAll(request1, request2);
        }
    }
}
