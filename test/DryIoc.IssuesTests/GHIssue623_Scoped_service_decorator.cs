using NUnit.Framework;

namespace DryIoc.IssuesTests;

[TestFixture]
public sealed class GHIssue623_Scoped_service_decorator : ITest
{
    public int Run()
    {
        // Test_not_working(); // todo: @fixme, avoid resolution root caching for the conditional singleton decorator with failed condition (at least) 
        Test_working();
        return 2;
    }

    [Test]
    public void Test_not_working()
    {
        var c = new Container();

        c.Register<GlobalService>(Reuse.Singleton);
        c.Register<ITest, TestGlobal>(Reuse.Transient);

        c.Register<ITest, TestDecoratorScoped>(
            reuse: Reuse.Scoped,
            setup: Setup.DecoratorWith(condition: req => req.CurrentScope != null));

        Assert.AreEqual(2, c.Resolve<GlobalService>().Foo());
        Assert.AreEqual(2, c.Resolve<ITest>().Foo()); // the code causing the error

        var t2 = c.OpenScope().Resolve<ITest>();
        Assert.IsInstanceOf<TestDecoratorScoped>(t2);
        Assert.AreEqual(6, t2.Foo());
    }

    [Test]
    public void Test_working()
    {
        var c = new Container();
        c.Register<GlobalService>(Reuse.Singleton);
        c.Register<ITest, TestGlobal>(Reuse.Transient);

        c.Register<ITest, TestDecoratorScoped>(
            reuse: Reuse.Scoped,
            setup: Setup.DecoratorWith(condition: req => req.CurrentScope != null));

        Assert.AreEqual(2, c.Resolve<GlobalService>().Foo());
        // Assert.AreEqual(2, c.Resolve<ITest>().Foo());

        var t2 = c.OpenScope().Resolve<ITest>();
        Assert.IsInstanceOf<TestDecoratorScoped>(t2); // if uncomment code above then we will get error here
        Assert.AreEqual(6, t2.Foo());
    }

    public interface ITest
    {
        int Foo();
    }

    public class GlobalService
    {
        public ITest T { get; }

        public GlobalService(ITest test) => T = test;

        public int Foo() => T.Foo();
    }

    public class TestGlobal : ITest
    {
        public int Foo() => 2;
    }

    public class TestDecoratorScoped : ITest
    {
        private readonly ITest _test;

        public TestDecoratorScoped(ITest test) => _test = test;

        public int Foo() => _test.Foo() * 3;
    }
}