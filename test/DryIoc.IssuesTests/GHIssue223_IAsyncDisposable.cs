#if NET5_0_OR_GREATER
using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue223_IAsyncDisposable : ITest
    {
        public int Run()
        {
            DisposeAsync_of_disposable_then_async_disposable_of_order_one_then_unordered_async_disposable().GetAwaiter().GetResult();
            DisposeAsync_of_disposable_then_slow_async_disposable_of_one_order_then_another_async_disposable_of_different_order().GetAwaiter().GetResult();
            DisposeAsync_in_reverse_order_of_resolution_with_multiple_slow_disposable().GetAwaiter().GetResult();
            DisposeAsync_the_slow_async_disposable().GetAwaiter().GetResult();
            DisposeAsync_in_reverse_order_of_resolution_for_unordered_registrations().GetAwaiter().GetResult();
            DisposeAsync_for_the_ordered_Async_implementor().GetAwaiter().GetResult();
            DisposeAsync_for_Async_implementor().GetAwaiter().GetResult();
            DisposeAsync_for_both_Sync_and_Async_implementor().GetAwaiter().GetResult();

            return 8;
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
        public async Task DisposeAsync_the_slow_async_disposable()
        {
            var c = new Container();

            c.RegisterDelegate<Action<object>, SlowAsyncDisposable>(
                static act => new SlowAsyncDisposable(act), Reuse.Scoped);

            object justAsync = null;
            await using (var scope = c.OpenScope())
            {
                scope.Use<Action<object>>(a => { justAsync = a; });
                _ = scope.Resolve<SlowAsyncDisposable>();
            }
            Assert.IsInstanceOf<SlowAsyncDisposable>(justAsync);
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
            object disposed1st = null;
            object disposed2nd = null;
            await using (var scope = c.OpenScope())
            {
                scope.Use<Action<object>>(a =>
                {
                    if (i == 0) disposed1st = a;
                    else if (i == 1) disposed2nd = a;
                    ++i;
                });

                _ = scope.Resolve<BothDisposableAndAsyncDisposable>();
                _ = scope.Resolve<SomeAsyncDisposable>();
            }

            Assert.IsInstanceOf<BothDisposableAndAsyncDisposable>(disposed1st);
            Assert.IsInstanceOf<SomeAsyncDisposable>(disposed2nd);
        }

        [Test]
        public async Task DisposeAsync_in_reverse_order_of_resolution_for_unordered_registrations()
        {
            var c = new Container();

            c.RegisterDelegate<Action<object>, SomeAsyncDisposable>(
                static act => new SomeAsyncDisposable(act), Reuse.Scoped);
            c.RegisterDelegate<Action<object>, BothDisposableAndAsyncDisposable>(
                static act => new BothDisposableAndAsyncDisposable(act), Reuse.Scoped);

            var i = 0;
            object disposed1st = null;
            object disposed2nd = null;
            await using (var scope = c.OpenScope())
            {
                scope.Use<Action<object>>(a =>
                {
                    if (i == 0) disposed1st = a;
                    else if (i == 1) disposed2nd = a;
                    ++i;
                });

                _ = scope.Resolve<BothDisposableAndAsyncDisposable>();
                _ = scope.Resolve<SomeAsyncDisposable>();
            }
            Assert.IsInstanceOf<SomeAsyncDisposable>(disposed1st);
            Assert.IsInstanceOf<BothDisposableAndAsyncDisposable>(disposed2nd);
        }

        [Test]
        public async Task DisposeAsync_in_reverse_order_of_resolution_with_multiple_slow_disposable()
        {
            var c = new Container();

            c.RegisterDelegate<Action<object>, SomeDisposable>(
                static act => new SomeDisposable(act), Reuse.Scoped);

            c.RegisterDelegate<Action<object>, SlowAsyncDisposable>(
                static act => new SlowAsyncDisposable(act), Reuse.Scoped, setup: Setup.With(disposalOrder: 2));

            c.RegisterDelegate<Action<object>, AnotherSlowAsyncDisposable>(
                static act => new AnotherSlowAsyncDisposable(act), Reuse.Scoped, setup: Setup.With(disposalOrder: -1));

            var i = 0;
            object disposed1st = null;
            object disposed2nd = null;
            object disposed3rd = null;
            await using (var scope = c.OpenScope())
            {
                scope.Use<Action<object>>(a =>
                {
                    if (i == 0) disposed1st = a;
                    else if (i == 1) disposed2nd = a;
                    else if (i == 2) disposed3rd = a;
                    ++i;
                });

                _ = scope.Resolve<SomeDisposable>();
                _ = scope.Resolve<SlowAsyncDisposable>();
                _ = scope.Resolve<AnotherSlowAsyncDisposable>();
            }

            Assert.IsInstanceOf<AnotherSlowAsyncDisposable>(disposed1st);
            Assert.IsInstanceOf<SomeDisposable>(disposed2nd);
            Assert.IsInstanceOf<SlowAsyncDisposable>(disposed3rd);
        }

        [Test]
        public async Task DisposeAsync_of_disposable_then_slow_async_disposable_of_one_order_then_another_async_disposable_of_different_order()
        {
            var c = new Container();

            c.RegisterDelegate<Action<object>, SomeDisposable>(
                static act => new SomeDisposable(act), Reuse.Scoped);

            c.RegisterDelegate<Action<object>, SlowAsyncDisposable>(
                static act => new SlowAsyncDisposable(act), Reuse.Scoped);

            c.RegisterDelegate<Action<object>, AnotherSlowAsyncDisposable>(
                static act => new AnotherSlowAsyncDisposable(act), Reuse.Scoped, setup: Setup.With(disposalOrder: 1));

            var i = 0;
            object disposed1st = null;
            object disposed2nd = null;
            object disposed3rd = null;
            await using (var scope = c.OpenScope())
            {
                scope.Use<Action<object>>(a =>
                {
                    if (i == 0) disposed1st = a;
                    else if (i == 1) disposed2nd = a;
                    else if (i == 2) disposed3rd = a;
                    ++i;
                });

                _ = scope.Resolve<SomeDisposable>();
                _ = scope.Resolve<SlowAsyncDisposable>();
                _ = scope.Resolve<AnotherSlowAsyncDisposable>();
            }

            Assert.IsInstanceOf<SlowAsyncDisposable>(disposed1st);
            Assert.IsInstanceOf<SomeDisposable>(disposed2nd);
            Assert.IsInstanceOf<AnotherSlowAsyncDisposable>(disposed3rd);
        }

        [Test]
        public async Task DisposeAsync_of_disposable_then_async_disposable_of_order_one_then_unordered_async_disposable()
        {
            var c = new Container();

            c.RegisterDelegate<Action<object>, SomeDisposable>(
                static act => new SomeDisposable(act), Reuse.Scoped, setup: Setup.With(disposalOrder: 1));

            c.RegisterDelegate<Action<object>, SomeAsyncDisposable>(
                static act => new SomeAsyncDisposable(act), Reuse.Scoped, setup: Setup.With(disposalOrder: 1));

            c.RegisterDelegate<Action<object>, BothDisposableAndAsyncDisposable>(
                static act => new BothDisposableAndAsyncDisposable(act), Reuse.Scoped);

            var i = 0;
            object disposed1st = null;
            object disposed2nd = null;
            object disposed3rd = null;
            await using (var scope = c.OpenScope())
            {
                scope.Use<Action<object>>(a =>
                {
                    if (i == 0) disposed1st = a;
                    else if (i == 1) disposed2nd = a;
                    else if (i == 2) disposed3rd = a;
                    ++i;
                });

                _ = scope.Resolve<SomeDisposable>();
                _ = scope.Resolve<SomeAsyncDisposable>();
                _ = scope.Resolve<BothDisposableAndAsyncDisposable>();
            }

            Assert.IsInstanceOf<BothDisposableAndAsyncDisposable>(disposed1st);
            Assert.IsInstanceOf<SomeAsyncDisposable>(disposed2nd);
            Assert.IsInstanceOf<SomeDisposable>(disposed3rd);
        }

        public class SomeDisposable : IDisposable
        {
            private readonly Action<object> _disposeAction;

            public SomeDisposable(Action<object> disposeAction) => _disposeAction = disposeAction;

            public void Dispose() =>
                _disposeAction(this);
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

        public class SlowAsyncDisposable : IAsyncDisposable
        {
            private readonly Action<object> _disposeAction;

            public SlowAsyncDisposable(Action<object> disposeAction) => _disposeAction = disposeAction;

            public async ValueTask DisposeAsync()
            {
                await Task.Delay(25);
                _disposeAction(this);
            }
        }

        public class AnotherSlowAsyncDisposable : IAsyncDisposable
        {
            private readonly Action<object> _disposeAction;

            public AnotherSlowAsyncDisposable(Action<object> disposeAction) => _disposeAction = disposeAction;

            public async ValueTask DisposeAsync()
            {
                await Task.Delay(25);
                _disposeAction(this);
            }
        }
    }
}
#endif