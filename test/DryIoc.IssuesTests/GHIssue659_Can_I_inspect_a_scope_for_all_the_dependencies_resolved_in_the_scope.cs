using NUnit.Framework;

using System;
using System.Text;
using DryIoc.FastExpressionCompiler.LightExpression;
using System.Linq;
using System.Diagnostics;

// #if !NET5_0_OR_GREATER
// namespace System.Runtime.CompilerServices
// {
//     internal static class IsExternalInit { }
// }
// #endif

namespace DryIoc.IssuesTests;

[TestFixture]
public class GHIssue659_Can_I_inspect_a_scope_for_all_the_dependencies_resolved_in_the_scope : ITest
{
    public int Run()
    {
        Enumerate_services_stored_in_scope_at_some_point_of_time();

        return 1;
    }

    public record Foo(Bar bar, IResolverContext context);
    public record Bar(Bazz bazz);
    public record Bazz(Fizz fizz);
    public record Fizz(Buzz buzz);
    public record Buzz;

    [Test]
    public void Enumerate_services_stored_in_scope_at_some_point_of_time()
    {
        var c = new Container();

        c.Register<Foo>(Reuse.Scoped);
        c.Register<Bar>(Reuse.Transient);
        c.Register<Bazz>(Reuse.Scoped);
        c.Register<Fizz>(Reuse.Transient);
        c.Register<Buzz>(Reuse.Scoped);

        using var scope = c.OpenScope();
        var fooExpr = scope.Resolve<LambdaExpression, Foo>();

        var sb = new StringBuilder().Append("var @cs = ");

        fooExpr.ToCSharpString(sb,
            lineIdent: 0,
            stripNamespace: true,
            printType: StripOuterTypes,
            identSpaces: 4);

        static string StripOuterTypes(Type inputType, string repr) => repr.Substring(repr.LastIndexOf('.') + 1);

        sb.Append(';');
        Debug.WriteLine(sb.ToString());

        var foo = scope.Resolve<Foo>();

        var services = foo.context.CurrentScope.GetSnapshotOfServicesWithFactoryIDs();
        Assert.AreEqual(3, services.Length);
        CollectionAssert.AreEquivalent(new[] { "Foo", "Bazz", "Buzz" }, services.Select(s => s.Value.GetType().Name));

        foreach (var service in services)
            Debug.WriteLine($"FactoryID: {service.Key}, Service: {service.Value}");

        foreach (var regInfo in c.GetServiceRegistrations().Where(sr => services.Any(s => s.Key == sr.Factory.FactoryID)))
            Debug.WriteLine(regInfo);
    }
}
