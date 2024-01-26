using NUnit.Framework;

namespace DryIoc.IssuesTests;

[TestFixture]
public sealed class GHIssue623_Scoped_service_decorator : ITest
{
    public int Run()
    {
        Test_not_working();
        Test_working();
        return 2;
    }

    [Test]
    public void Test_not_working()
    {
        var c = new Container();

        c.Register<Consumer>(Reuse.Singleton);
        c.Register<ISomeDep, GlobalDep>(Reuse.Transient);

        c.Register<ISomeDep, DecoratorScoped>(
            reuse: Reuse.Scoped,
            setup: Setup.DecoratorWith(condition: req => req.CurrentScope != null));

        Assert.AreEqual(2, c.Resolve<Consumer>().Foo());
        Assert.AreEqual(2, c.Resolve<ISomeDep>().Foo()); // the code causing the error

        var t2 = c.OpenScope().Resolve<ISomeDep>();
        Assert.IsInstanceOf<DecoratorScoped>(t2);
        Assert.AreEqual(6, t2.Foo());
    }

    [Test]
    public void Test_working()
    {
        var c = new Container();
        c.Register<Consumer>(Reuse.Singleton);
        c.Register<ISomeDep, GlobalDep>(Reuse.Transient);

        c.Register<ISomeDep, DecoratorScoped>(
            reuse: Reuse.Scoped,
            setup: Setup.DecoratorWith(condition: req => req.CurrentScope != null));

        Assert.AreEqual(2, c.Resolve<Consumer>().Foo());
        // Assert.AreEqual(2, c.Resolve<ITest>().Foo());

        var t2 = c.OpenScope().Resolve<ISomeDep>();
        Assert.IsInstanceOf<DecoratorScoped>(t2); // if uncomment code above then we will get error here
        Assert.AreEqual(6, t2.Foo());
    }

    public interface ISomeDep
    {
        int Foo();
    }

    public class Consumer
    {
        public ISomeDep Dep { get; }

        public Consumer(ISomeDep dep) => Dep = dep;

        public int Foo() => Dep.Foo();
    }

    public class GlobalDep : ISomeDep
    {
        public int Foo() => 2;
    }

    public class DecoratorScoped : ISomeDep
    {
        private readonly ISomeDep _test;

        public DecoratorScoped(ISomeDep test) => _test = test;

        public int Foo() => _test.Foo() * 3;
    }
}