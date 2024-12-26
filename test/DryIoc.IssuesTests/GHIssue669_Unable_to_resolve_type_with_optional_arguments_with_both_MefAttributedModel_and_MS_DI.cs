using NUnit.Framework;
using DryIoc.MefAttributedModel;
using DryIoc.Microsoft.DependencyInjection;

namespace DryIoc.IssuesTests;

[TestFixture]
public class GHIssue669_Unable_to_resolve_type_with_optional_arguments_with_both_MefAttributedModel_and_MS_DI : ITest
{
    public int Run()
    {
        Case_with_MEF_only();
        Case_with_MS_DI_only();
        Original_case();

        return 3;
    }

    public class Bar { }

    public class Foo
    {
        public Foo(Bar bar = null) { }
    }

    [Test]
    public void Original_case()
    {
        var container = new Container(DryIocAdapter.WithMicrosoftDependencyInjectionRules(Rules.Default.WithMefAttributedModel()));

        container.Register<Foo>();

        var foo = container.Resolve<Foo>();

        Assert.IsNotNull(foo);
    }

    [Test]
    public void Case_with_MEF_only()
    {
        var container = new Container(Rules.Default.WithMefAttributedModel());

        container.Register<Foo>();

        var foo = container.Resolve<Foo>();

        Assert.IsNotNull(foo);
    }

    [Test]
    public void Case_with_MS_DI_only()
    {
        var container = new Container(DryIocAdapter.MicrosoftDependencyInjectionRules);

        container.Register<Foo>();

        var foo = container.Resolve<Foo>();

        Assert.IsNotNull(foo);
    }
}
