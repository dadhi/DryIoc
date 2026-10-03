using System;
using System.ComponentModel.Composition;
using DryIoc.MefAttributedModel;
using NUnit.Framework;

namespace DryIoc.IssuesTests;

[TestFixture]
public class GHIssue710_ConstructorWithResolvableArguments_conflicts_with_Mef : ITest
{
    public int Run()
    {
        Original_case();
        WithOverride_case();
        return 2;
    }

    public interface IServer
    {
        void Hello();
    }

    public interface IPrinter
    {
        void Print(string s);
    }

    [Export(typeof(IServer))]
    public class Server : IServer
    {
        [Import]
        internal IPrinter Printer { get; set; }

        public void Hello() => Printer.Print("Hello!");
    }

    [Export(typeof(IPrinter))]
    public class Printer : IPrinter
    {
        public void Print(string s) {}
    }

    [Test]
    public void Original_case()
    {
        var c = new Container()
            .WithMef()
            .With(rules => rules.With(FactoryMethod.ConstructorWithResolvableArguments)); // resets previous property injection rules to null disabling property injection

        c.RegisterExports(typeof(Server), typeof(Printer));

        var root = c.Resolve<IServer>();
        Assert.IsNotNull(root, "Root is not resolved");

        var server = root as Server;
        Assert.IsNull(server.Printer, "Import is not satisfied");
    }

    [Test]
    public void WithOverride_case()
    {
        var c = new Container()
            .WithMef()
            .With(rules => rules.WithOverride(FactoryMethod.ConstructorWithResolvableArguments)); // combines with previous property injection rules

        c.RegisterExports(typeof(Server), typeof(Printer));

        var root = c.Resolve<IServer>();
        Assert.IsNotNull(root, "Root is not resolved");

        var server = root as Server;
        Assert.IsNotNull(server.Printer, "Import is not satisfied");

        root.Hello();
    }
}
