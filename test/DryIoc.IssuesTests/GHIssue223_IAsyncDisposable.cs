#if NET5_0_OR_GREATER
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
            DisposeAsync_for_the_ordered_Async_implementor().GetAwaiter().GetResult();
            DisposeAsync_for_Async_implementor().GetAwaiter().GetResult();
            DisposeAsync_for_both_Sync_and_Async_implementor().GetAwaiter().GetResult();

            return 2;
        }

        [Test]
        public async Task DisposeAsync_for_both_Sync_and_Async_implementor()
        {
            var c = new Container();

            c.RegisterDelegate<Action<object>, BothDisposableAndAsyncDisposable>(
                static act => new BothDisposableAndAsyncDisposable(act), Reuse.Scoped);

            object asyncDisp = null;
            await using (var scope = c.OpenScope())
            {
                scope.Use<Action<object>>(a => { asyncDisp = a; });
                _ = scope.Resolve<BothDisposableAndAsyncDisposable>();
            }
            Assert.IsInstanceOf<BothDisposableAndAsyncDisposable>(asyncDisp);

            object syncDisp = null;
            using (var scope = c.OpenScope())
            {
                scope.Use<Action<object>>(a => { syncDisp = a; });
                _ = scope.Resolve<BothDisposableAndAsyncDisposable>();
            }
            Assert.IsInstanceOf<BothDisposableAndAsyncDisposable>(syncDisp);
        }

        [Test]
        public async Task DisposeAsync_for_Async_implementor()
        {
            var c = new Container();

            c.RegisterDelegate<Action<object>, SomeAsyncDisposable>(
                static act => new SomeAsyncDisposable(act), Reuse.Scoped);

            object justAsync = null;
            await using (var scope = c.OpenScope())
            {
                scope.Use<Action<object>>(a => { justAsync = a; });
                _ = scope.Resolve<SomeAsyncDisposable>();
            }
            Assert.IsInstanceOf<SomeAsyncDisposable>(justAsync);
        }

        [Test]
        public async Task DisposeAsync_for_the_ordered_Async_implementor()
        {
            var c = new Container();

            c.RegisterDelegate<Action<object>, SomeAsyncDisposable>(
                static act => new SomeAsyncDisposable(act), Reuse.Scoped, setup: Setup.With(disposalOrder: 2));
            c.RegisterDelegate<Action<object>, BothDisposableAndAsyncDisposable>(
                static act => new BothDisposableAndAsyncDisposable(act), Reuse.Scoped, setup: Setup.With(disposalOrder: 1));

            var i = 0;
            object disposedFirst = null;
            object disposedSecond = null;
            await using (var scope = c.OpenScope())
            {
                scope.Use<Action<object>>(a =>
                {
                    if (i == 0) disposedFirst = a;
                    else if (i == 1) disposedSecond = a;
                    ++i;
                });

                _ = scope.Resolve<BothDisposableAndAsyncDisposable>();
                _ = scope.Resolve<SomeAsyncDisposable>();
            }
            Assert.IsInstanceOf<BothDisposableAndAsyncDisposable>(disposedFirst);
            Assert.IsInstanceOf<SomeAsyncDisposable>(disposedSecond);
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
                await Task.Delay(30);
                onDisposed(this);
            }
        }

        public class SomeDisposable : IDisposable
        {
            private readonly Action<object> _disposeAction;

            public SomeDisposable(Action<object> disposeAction) => _disposeAction = disposeAction;

            public void Dispose()
            {
                _disposeAction(this);
            }
        }

        public class BothDisposableAndAsyncDisposable : IDisposable, IAsyncDisposable
        {
            private readonly Action<object> _disposeAction;

            public BothDisposableAndAsyncDisposable(Action<object> disposeAction) => _disposeAction = disposeAction;

            public void Dispose() =>
                _disposeAction(this);

            public ValueTask DisposeAsync()
            {
                _disposeAction(this);
                return default;
            }
        }

        public class SomeAsyncDisposable : IAsyncDisposable
        {
            private readonly Action<object> _disposeAction;

            public SomeAsyncDisposable(Action<object> disposeAction) => _disposeAction = disposeAction;

            public ValueTask DisposeAsync()
            {
                _disposeAction(this);
                return default;
            }
        }
    }
}
#endif