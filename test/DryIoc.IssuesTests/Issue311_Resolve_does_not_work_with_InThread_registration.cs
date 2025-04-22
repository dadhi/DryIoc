using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue311_Resolve_does_not_work_with_InThread_registration : ITest
    {
        public int Run()
        {
            Test().GetAwaiter().GetResult();
#if NET5_0_OR_GREATER
            Test_DisposeAsync().GetAwaiter().GetResult();
            return 2;
#else
            return 1;
#endif
        }

        [Test]
        public async Task Test()
        {
            using var c = new Container(scopeContext: new AutomaticThreadLocalScopeContext());

            c.Register<A>(Reuse.InThread);

            var a = c.Resolve<A>();
            Assert.AreSame(a, c.Resolve<A>());

            await Task.Run(() =>
            {
                Assert.AreNotSame(a, c.Resolve<A>());
            });
        }

#if NET5_0_OR_GREATER
        [Test]
        public async Task Test_DisposeAsync()
        {
            await using var c = new Container(scopeContext: new AutomaticThreadLocalScopeContext());

            c.Register<A>(Reuse.InThread);

            var a = c.Resolve<A>();
            Assert.AreSame(a, c.Resolve<A>());

            await Task.Run(() =>
            {
                Assert.AreNotSame(a, c.Resolve<A>());
            });
        }
#endif

        public class A { }
    }

    public class AutomaticThreadLocalScopeContext : IScopeContext
    {
        readonly ThreadLocal<IScope> _threadScope =
            new ThreadLocal<IScope>(() => Scope.Of(ThreadScopeContext.ScopeContextName), trackAllValues: true);

        public IScope GetCurrentOrDefault() => _threadScope.Value;

        public IScope SetCurrent(SetCurrentScopeHandler setCurrentScope) =>
            throw new NotSupportedException("Setting the new scope is not supported, because the thread-local scope is created automatically.");

        public void Dispose()
        {
            if (_threadScope.IsValueCreated)
                for (var i = 0; i < _threadScope.Values.Count; i++)
                {
                    var scope = _threadScope.Values[i];
                    scope.Dispose();
                }
        }

#if NET5_0_OR_GREATER
        public ValueTask DisposeAsync()
        {
            if (_threadScope.IsValueCreated)
            {
                var scopes = _threadScope.Values;
                for (var i = 0; i < scopes.Count; i++)
                {
                    var scope = scopes[i];
                    var currDisposing = scope.DisposeAsync();
                    if (!currDisposing.IsCompleted)
                        return ScopeContextTools.DisposeScopesRestAsync(currDisposing, null, scopes, i + 1);
                    if (currDisposing.IsFaulted)
                        currDisposing.GetAwaiter().GetResult();
                }
            }
            return default;
        }
#endif
    }
}
