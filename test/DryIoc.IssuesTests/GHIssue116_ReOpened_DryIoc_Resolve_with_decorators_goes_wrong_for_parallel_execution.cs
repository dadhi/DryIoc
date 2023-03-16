using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    // [TestFixture]
    public class GHIssue116_ReOpened_DryIoc_Resolve_with_decorators_goes_wrong_for_parallel_execution : ITest
    {
        public int Run()
        {
            DryIoc_Resolve_parallel_execution_on_repeat(10).GetAwaiter().GetResult();
            return 1;
        }

        interface IQuery<T> { }
        class Query<T> : IQuery<T> { };
        class QueryDecorator<T> : IQuery<T>
        {
            public QueryDecorator(IQuery<T> decoratee) { }
        }

        public async Task DryIoc_Resolve_parallel_execution_on_repeat(int repeatCount)
        {
            for (var i = 0; i < repeatCount; i++)
                await DryIoc_Resolve_parallel_execution();
        }

        // [Test, Repeat(10)]
        public async Task DryIoc_Resolve_parallel_execution()
        {
            var container = new Container();

            container.Register(typeof(IQuery<string>), typeof(Query<string>));
            container.Register(typeof(IQuery<string>), typeof(QueryDecorator<string>), setup: Setup.Decorator);

            IQuery<string> ResolveInScope()
            {
                using (var scope = container.OpenScope())
                {
                    return scope.Resolve<IQuery<string>>();
                }
            }

            const int tasksCount = 32;

            var tasks = new Task<IQuery<string>>[tasksCount];
            for (var i = 0; i < tasks.Length; i++)
                tasks[i] = Task.Run(() => ResolveInScope());
            
            await Task.WhenAll(tasks);

            var failed = false;
            var sb = new StringBuilder(tasks.Length);
            for (var i = 0; i < tasks.Length; i++)
            {   
                var success = tasks[i].Result is QueryDecorator<string>;
                if (!success) failed = true;
                sb.Append(success ? '_' : 'F');
            }

            Assert.IsFalse(failed, $"Some tasks are failed [{sb}]");
        }
    }
}
