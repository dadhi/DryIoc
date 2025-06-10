using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DryIoc.IssuesTests;

[TestFixture]
public class GHIssue685_Creating_scopes_via_funcs_is_not_threadsafe_and_fails_sporadically_with_NullRef_exception : ITest
{
    public int Run()
    {
        for (var i = 0; i < 10; ++i)
            Original_case();
        return 1;
    }

    public class OutsideScopeDependency { }

    public class MyScoped { }

    public class Root
    {
        private readonly Func<MyScoped> _createScope;
        public Root(Func<MyScoped> createScope, OutsideScopeDependency outsideScopeDependency) => _createScope = createScope;
        public void CreateScope() => _createScope();
    }

    private static void CreateScopeWithRandomDelay(Root root)
    {
        Thread.Sleep(new Random().Next(0, 2));
        root.CreateScope();
    }

    [Test]
    public void Original_case()
    {
        var c = new Container(rules => rules.WithFuncAndLazyWithoutRegistration());

        c.Register<Root>(Reuse.Transient);
        c.Register<OutsideScopeDependency>(Reuse.Singleton);
        c.Register<MyScoped>(Reuse.Scoped, setup: Setup.With(openResolutionScope: true));

        var root = c.Resolve<Root>();

        var concurrentActions = Enumerable.Range(0, 10)
            .Select<int, Action>(_ => () => CreateScopeWithRandomDelay(root))
            .ToArray();

        Parallel.Invoke(concurrentActions);
    }
}
