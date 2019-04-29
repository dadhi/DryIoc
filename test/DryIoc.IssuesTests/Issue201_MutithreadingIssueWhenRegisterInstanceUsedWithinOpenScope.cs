
using System.Threading.Tasks;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    class Issue201_MultiThreadingIssueWhenRegisterInstanceUsedWithinOpenScope
    {
        [Test]
        public void UseInstance_InThread_Replace_should_return_the_same_in_thread()
        {
            var container = new Container(scopeContext: new AsyncExecutionFlowScopeContext());
            container.Register<U>();

            const int workerCount = 4;
            const int repeatTimes = 5;

            for (var i = 0; i < repeatTimes; i++)
            {
                var tasks = new Task[workerCount];
                for (var j = 0; j < workerCount; j++)
                {
                    tasks[j] = Task.Run(async () =>
                    {
                        using (var scope = container.OpenScope(ThreadScopeContext.ScopeContextName))
                        {
                            scope.Use(new S());
                            var u = scope.Resolve<U>();

                            await Task.Delay(5).ConfigureAwait(true);

                            Assert.AreSame(u.S, scope.Resolve<U>().S);
                        }
                    });
                }

                Task.WaitAll(tasks);
            }
        }

        public class S { }

        public class U
        {
            public S S { get; private set; }

            public U(S s)
            {
                S = s;
            }
        }
    }
}
