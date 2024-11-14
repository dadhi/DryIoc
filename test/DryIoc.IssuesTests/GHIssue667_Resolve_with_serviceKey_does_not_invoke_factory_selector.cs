using NUnit.Framework;

using System;
using System.Text;
using DryIoc.FastExpressionCompiler.LightExpression;
using System.Linq;
using System.Diagnostics;
using DryIoc.ImTools;

namespace DryIoc.IssuesTests;

[TestFixture]
public class GHIssue667_Resolve_with_serviceKey_does_not_invoke_factory_selector : ITest
{
    public int Run()
    {
        // Original_case(); //todo: @fixme @wip
        return 1;
    }

    public interface IFoo { }
    public class Foo : IFoo { }

    [Test]
    public void Original_case()
    {
        var count = 0;
        var container = new Container(rules =>
            rules.WithFactorySelector(
                (request, single, many) =>
                {
                    ++count;
                    return single
                        ?? many.FindFirst(request.ServiceKey, static (sk, f) => f.Key.Equals(sk))?.Value;
                }));

        container.Register<IFoo, Foo>(); // Default
        container.Register<IFoo, Foo>(serviceKey: "my"); // Keyed
        _ = container.Resolve<IFoo>(); // Custom factory selector invoked
        Assert.AreEqual(1, count);

        _ = container.Resolve<IFoo>("my"); // Custom factory selector NOT invoked
        Assert.AreEqual(2, count);
    }
}
