using NUnit.Framework;
using System;

namespace DryIoc.IssuesTests;

[TestFixture]
public class GHIssue672_Wrong_decorator_parameter_with_custom_args : ITest
{
    public int Run()
    {
        // Original_case();
        Case_without_args();
        return 2;
    }

    public abstract class A { }
    public class B : A { }

    private static object DecoratorA(object t) => t;

    [Test]
    public void Original_case()
    {
        var now = DateTime.Now;

        var container = new Container();

        container.Register<object, B>(serviceKey: "xyz");

        var decorateMethod = typeof(GHIssue672_Wrong_decorator_parameter_with_custom_args).SingleMethod(nameof(DecoratorA), true);

        container.Register<object>(made: Made.Of(_ => decorateMethod), setup: Setup.DecoratorOf<A>());

        var res = container.Resolve<object>("xyz", args: new object[] { now });

        // Assert.IsInstanceOf<B>(res); // todo: @fixme
    }

    [Test]
    public void Case_without_args()
    {
        var container = new Container();

        container.Register<object, B>(serviceKey: "xyz");

        var decorateMethod = typeof(GHIssue672_Wrong_decorator_parameter_with_custom_args).SingleMethod(nameof(DecoratorA), true);

        container.Register<object>(made: Made.Of(_ => decorateMethod), setup: Setup.DecoratorOf<A>());

        var res = container.Resolve<object>("xyz");

        Assert.IsInstanceOf<B>(res);
    }
}
