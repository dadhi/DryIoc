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
            DryIoc_Resolve_parallel_execution_on_repeat(64).GetAwaiter().GetResult();
            // DryIoc_Resolve_parallel_execution_with_compile_service_expression(64).GetAwaiter().GetResult();
            return 2;
        }

        interface IQuery<T> { }
        class Query<T> : IQuery<T> { };
        class QueryDecorator<T> : IQuery<T>
        {
            public readonly IQuery<T> Decoratee;
            public QueryDecorator(IQuery<T> decoratee) => Decoratee = decoratee;
        }

        public async Task DryIoc_Resolve_parallel_execution_on_repeat(int repeatCount)
        {
            for (var i = 0; i < repeatCount; i++)
                await DryIoc_Resolve_parallel_execution(i);
        }

        // [Test, Repeat(10)]
        public async Task DryIoc_Resolve_parallel_execution(int iter)
        {
            var container = new Container();

            container.Register(typeof(IQuery<string>), typeof(Query<string>));            
            container.Register(typeof(IQuery<string>), typeof(QueryDecorator<string>), setup: Setup.Decorator);

            const int tasksCount = 32;

            var tasks = new Task<IQuery<string>>[tasksCount];
            for (var i = 0; i < tasks.Length; i++)
                tasks[i] = Task.Run(() => container.Resolve<IQuery<string>>());
            
            await Task.WhenAll(tasks);

            var failed = false;
            var sb = new StringBuilder(tasks.Length);
            for (var i = 0; i < tasks.Length; i++)
            {   
                var result = tasks[i].Result;
                var decorator = result as QueryDecorator<string>;
                var success = decorator != null && decorator.Decoratee is QueryDecorator<string> == false;
                failed |= !success;
                sb.Append(success ? '_' : decorator == null ? 'F' : 'f');
            }

            Assert.IsFalse(failed, $"Some of {tasks.Length} tasks are failed [{sb}] on iteration {iter}");
        }

        // [Test, Repeat(10)]
        public async Task DryIoc_Resolve_parallel_execution_with_compile_service_expression(int iter)
        {
            var container = new Container(Rules.Default.WithoutInterpretationForTheFirstResolution()); // todo: @fixme check

            container.Register(typeof(IQuery<string>), typeof(Query<string>));            
            container.Register(typeof(IQuery<string>), typeof(QueryDecorator<string>), setup: Setup.Decorator);

            const int tasksCount = 32;

            var tasks = new Task<IQuery<string>>[tasksCount];
            for (var i = 0; i < tasks.Length; i++)
                tasks[i] = Task.Run(() => container.Resolve<IQuery<string>>());
            
            await Task.WhenAll(tasks);

            var failed = false;
            var sb = new StringBuilder(tasks.Length);
            for (var i = 0; i < tasks.Length; i++)
            {   
                var result = tasks[i].Result;
                var decorator = result as QueryDecorator<string>;
                var success = decorator != null && decorator.Decoratee is QueryDecorator<string> == false;
                failed |= !success;
                sb.Append(success ? '_' : decorator == null ? 'F' : 'f');
            }

            Assert.IsFalse(failed, $"Some of {tasks.Length} tasks are failed [{sb}] on iteration {iter}");
        }
    }
}
