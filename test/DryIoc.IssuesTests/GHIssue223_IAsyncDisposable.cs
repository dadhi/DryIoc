#if NET6_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue223_IAsyncDisposable : ITest
    {
        public int Run()
        {
            // ShouldDisposeAsyncDisposable().GetAwaiter().GetResult();
            return 1;
        }

        // [Test] // @wip
        public async Task ShouldDisposeAsyncDisposable()
        {
            var container = new Container();
            List<object> disposedObjects = new();

            container.RegisterDelegate<AsyncDisposable>(_ => new AsyncDisposable(disposedObject => disposedObjects.Add(disposedObject)), Reuse.Scoped);

            AsyncDisposable asyncDisposable = null;
            await using (var scope = container.OpenScope())
            {
                asyncDisposable = container.Resolve<AsyncDisposable>();
            }

            Assert.Contains(asyncDisposable, disposedObjects);
        }

        // [Test]
        // public async Task ShouldDisposeSlowAsyncDisposable()
        // {
        //     var container = new Container();
        //     List<object> disposedObjects = new();
        //     container.RegisterDelegate<SlowAsyncDisposable>(sf => new SlowAsyncDisposable(disposedObject => disposedObjects.Add(disposedObject)), Reuse.Scoped);

        //     SlowAsyncDisposable asyncDisposable = null;
        //     await using (var scope = container.BeginScope())
        //     {
        //         asyncDisposable = container.GetInstance<SlowAsyncDisposable>();
        //     }

        //     Assert.Contains(asyncDisposable, disposedObjects);
        // }

        // [Test]
        // public async Task ShouldDisposeInCorrectOrder()
        // {
        //     var container = new Container();
        //     List<object> disposedObjects = new();
        //     container.RegisterDelegate<AsyncDisposable>(sf => new AsyncDisposable(disposedObject => disposedObjects.Add(disposedObject)), Reuse.Scoped);
        //     container.RegisterDelegate<SlowAsyncDisposable>(sf => new SlowAsyncDisposable(disposedObject => disposedObjects.Add(disposedObject)), Reuse.Scoped);
        //     container.RegisterDelegate<Disposable>(sf => new Disposable(disposedObject => disposedObjects.Add(disposedObject)), Reuse.Scoped);

        //     AsyncDisposable asyncDisposable = null;
        //     SlowAsyncDisposable slowAsyncDisposable = null;
        //     Disposable disposable = null;
        //     await using (var scope = container.BeginScope())
        //     {
        //         disposable = container.GetInstance<Disposable>();
        //         asyncDisposable = container.GetInstance<AsyncDisposable>();
        //         slowAsyncDisposable = container.GetInstance<SlowAsyncDisposable>();
        //     }

        //     Assert.Same(disposedObjects[0], slowAsyncDisposable);
        //     Assert.Same(disposedObjects[1], asyncDisposable);
        //     Assert.Same(disposedObjects[2], disposable);
        // }

        // [Test]
        // public async Task ShouldDisposeDisposable()
        // {
        //     var container = new Container();
        //     List<object> disposedObjects = new();

        //     container.RegisterDelegate<Disposable>(sf => new Disposable(disposedObject => disposedObjects.Add(disposedObject)), Reuse.Scoped);
        //     Disposable disposable = null;
        //     await using (var scope = container.BeginScope())
        //     {
        //         disposable = container.GetInstance<Disposable>();
        //     }

        //     Assert.Contains(disposable, disposedObjects);
        // }

        // [Test]
        // public void ShouldThrowWhenAsyncDisposableIsDisposedInSynchronousScope()
        // {
        //     var container = new Container();
        //     container.RegisterDelegate<AsyncDisposable>(sf => new AsyncDisposable(_ => { }), Reuse.Scoped);

        //     AsyncDisposable asyncDisposable = null;
        //     var scope = container.BeginScope();
        //     asyncDisposable = container.GetInstance<AsyncDisposable>();

        //     Assert.Throws<InvalidOperationException>(() => scope.Dispose());
        // }

        public class SlowAsyncDisposable : IAsyncDisposable
        {
            private readonly Action<object> onDisposed;

            public SlowAsyncDisposable(Action<object> onDisposed)
            {
                this.onDisposed = onDisposed;
            }
            public async ValueTask DisposeAsync()
            {
                await Task.Delay(100);
                onDisposed(this);
            }
        }

        public class AsyncDisposable : IAsyncDisposable
        {
            private readonly Action<object> onDisposed;

            public AsyncDisposable(Action<object> onDisposed)
            {
                this.onDisposed = onDisposed;
            }
            public ValueTask DisposeAsync()
            {
                onDisposed(this);
                return ValueTask.CompletedTask;
            }
        }

        public class Disposable : IDisposable
        {
            private readonly Action<object> onDisposed;

            public Disposable(Action<object> onDisposed)
            {
                this.onDisposed = onDisposed;
            }

            public void Dispose()
            {
                onDisposed(this);
            }
        }
    }
}
#endif