using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue311_Resolve_does_not_work_with_InThread_registration
    {
        [Test]
        public async Task Test()
        {
            var c = new Container(scopeContext: new AutomaticThreadLocalScopeContext());

            c.Register<A>(Reuse.InThread);

            var a = c.Resolve<A>();
            Assert.AreSame(a, c.Resolve<A>());

            await Task.Run(() =>
            {
                Assert.AreNotSame(a, c.Resolve<A>());
            });

            c.Dispose();
        }

        public class A { }
    }

    public class AutomaticThreadLocalScopeContext : IScopeContext
    {
        readonly ThreadLocal<IScope> _threadScope = 
            new ThreadLocal<IScope>(() => new Scope(name: ThreadScopeContext.ScopeContextName), trackAllValues: true);

        public IScope GetCurrentOrDefault()
        {
            return _threadScope.Value;
        }

        public IScope SetCurrent(SetCurrentScopeHandler setCurrentScope)
        {
            throw new NotSupportedException("Setting the new scope is not supported, because the thread-local scope is created automatically.");
        }

        public void Dispose()
        {
            if (_threadScope.IsValueCreated)
                foreach (var scope in _threadScope.Values)
                    scope.Dispose();
        }
    }
}
