using System;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue116_ReOpened_DryIoc_Resolve_with_decorators_goes_wrong_for_parallel_execution : ITest
    {
        const int IterCount = 64;
        const int TaskCount = 64;

        public int Run()
        {
            DryIoc_Resolve_parallel_execution_on_repeat();
            DryIoc_Resolve_parallel_execution_with_compile_service_expression_on_repeat();
            return 2;
        }

        interface IQuery<T> { }
        class Query<T> : IQuery<T> { };
        class QueryDecorator<T> : IQuery<T>
        {
            public readonly IQuery<T> Decoratee;
            public QueryDecorator(IQuery<T> decoratee) => Decoratee = decoratee;
        }

        public void DryIoc_Resolve_parallel_execution_on_repeat()
        {
            for (var i = 0; i < IterCount; i++)
                DryIoc_Resolve_parallel_execution();
        }

        public void DryIoc_Resolve_parallel_execution_with_compile_service_expression_on_repeat()
        {
            for (var i = 0; i < IterCount; i++)
                DryIoc_Resolve_parallel_execution_with_compile_service_expression();
        }

        [Test]
        public void DryIoc_Resolve_parallel_execution()
        {
            var container = new Container();

            container.Register(typeof(IQuery<string>), typeof(Query<string>));
            container.Register(typeof(IQuery<string>), typeof(QueryDecorator<string>), setup: Setup.Decorator);

            var tasks = new Task<IQuery<string>>[TaskCount];
            for (var i = 0; i < tasks.Length; i++)
                tasks[i] = Task.Run(() => container.Resolve<IQuery<string>>());

            Task.WaitAll(tasks);

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

            Assert.IsFalse(failed, $"Some of {tasks.Length} tasks are failed [{sb}]");
        }

        [Test]
        public void DryIoc_Resolve_parallel_execution_with_compile_service_expression()
        {
            var container = new Container(Rules.Default.WithoutInterpretationForTheFirstResolution());

            // check that open-generics work as well
            container.Register(typeof(IQuery<>), typeof(Query<>));
            container.Register(typeof(IQuery<>), typeof(QueryDecorator<>), setup: Setup.Decorator);

            var tasks = new Task<IQuery<string>>[TaskCount];
            for (var i = 0; i < tasks.Length; i++)
                tasks[i] = Task.Run(() => container.Resolve<IQuery<string>>());

            Task.WaitAll(tasks);

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

            Assert.IsFalse(failed, $"Some of {tasks.Length} tasks are failed [{sb}]");
        }
    }

    class SingleThreadSynchronizationContext : SynchronizationContext, IDisposable
    {
        private readonly Thread _thread;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly BlockingCollection<(SendOrPostCallback, object)> _tasks;

        public SingleThreadSynchronizationContext()
        {
            _cancellationTokenSource = new();
            _tasks = new();
            _thread = new Thread(static state =>
            {
                var ctx = (SingleThreadSynchronizationContext)state;
                SynchronizationContext.SetSynchronizationContext(ctx);
                try
                {
                    while (!ctx._cancellationTokenSource.IsCancellationRequested)
                    {
                        var (post, a) = ctx._tasks.Take(ctx._cancellationTokenSource.Token);
                        post(a);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Ignore the cancellation exception
                }
            });
            _thread.Start(this);
        }

        public override void Post(SendOrPostCallback d, object state) => _tasks.Add((d, state));

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _thread.Join();
            _tasks.Dispose();
            _cancellationTokenSource.Dispose();
        }
    }
}
