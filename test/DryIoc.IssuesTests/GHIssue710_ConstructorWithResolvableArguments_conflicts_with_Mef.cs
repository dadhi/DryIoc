using System;
using System.ComponentModel.Composition;
using DryIoc;
using DryIoc.MefAttributedModel;
using NUnit.Framework;

namespace DryIoc.IssuesTests;

[TestFixture]
public class GHIssue710_ConstructorWithResolvableArguments_conflicts_with_Mef : ITest
{
    public int Run()
    {
        Original_case();
        return 1;
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
        public void Print(string s) => Console.WriteLine(s);
    }

    [Test]
    public void Original_case()
    {
        var c = new Container().WithMef()
            .With(rules => rules
            .With(FactoryMethod.ConstructorWithResolvableArguments)
        );

        c.RegisterExports(typeof(Server), typeof(Printer));

        var root = c.Resolve<IServer>();
        Assert.That(root, Is.Not.Null, "Root is not resolved");

        var server = root as Server;
        Assert.That(server.Printer, Is.Not.Null, "Import is not satisfied");

        root.Hello();
    }
}
