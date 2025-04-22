using System;
using System.Threading;
using System.Threading.Tasks;

namespace DryIoc.IssuesTests
{
    // Not an actual test but example of the console app with Console events (!!!) and usage of the custom `StaticScopeContext`
    public class GHIssue583_Scope_is_lost_on_CTRL_C
    {
        public int Run()
        {
            Test_AsyncExecutionFlowScopeContext().GetAwaiter().GetResult();
            return 1;
        }

        public async Task Test_AsyncExecutionFlowScopeContext()
        {
            IContainer container = new Container(scopeContext: new StaticScopeContext());

            CancellationTokenSource cts = new();

            using (IResolverContext scope = container.OpenScope("MyScope", trackInParent: true))
            {
                Console.CancelKeyPress += (sender, args) =>
                {
                    Console.WriteLine("Canceling");
                    Console.WriteLine(scope.CurrentScope?.ToString() ?? "No scope");

                    cts.Cancel();
                    args.Cancel = true;
                };

                while (!cts.IsCancellationRequested)
                {
                    Console.WriteLine(scope.CurrentScope.ToString());

                    await Task.Delay(30).ConfigureAwait(false);
                }
            }

            Console.WriteLine("Scope disposed");
        }
    }

    internal sealed class StaticScopeContext : IScopeContext
    {
        private static IScope scope;

        public IScope GetCurrentOrDefault() => scope;

        public IScope SetCurrent(SetCurrentScopeHandler changeCurrentScope) => scope = changeCurrentScope(GetCurrentOrDefault());
        public void Dispose()
        {
        }

#if NET5_0_OR_GREATER
        public ValueTask DisposeAsync() => default;
#endif
    }
}
