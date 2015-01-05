using System.Threading.Tasks;
using Xunit;

namespace DryIoc.IssuesTests
{
    public class Issue72_SupportAllPossibleFlavorsOfChildNestedContainer
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

        [Fact]
        public void ScopeTest()
        {
            var container = new Container(scopeContext: new ExecutionFlowScopeContext());

            container.Register<A>();
            container.RegisterInstance(default(SomeRequest),
                Reuse.InCurrentScope, Setup.Default.WithReuseWrappers(typeof(ReusedRef)));

            var request1 = Task.Run(async () =>
            {
                var request = new SomeRequest();
                var scope = container.OpenScope();
                scope.Resolve<ReusedRef<SomeRequest>>().Swap(_ => request);

                await Task.Delay(5);//processing request
                Assert.Same(request, scope.Resolve<A>().Request);
            });

            var request2 = Task.Run(async () =>
            {
                var request = new SomeRequest();
                var scope = container.OpenScope();
                scope.Resolve<ReusedRef<SomeRequest>>().Swap(_ => request);
                await Task.Delay(2);//processing request
                Assert.Same(request, scope.Resolve<A>().Request);
            });

            Task.WaitAll(request1, request2);
        }
    }
}
