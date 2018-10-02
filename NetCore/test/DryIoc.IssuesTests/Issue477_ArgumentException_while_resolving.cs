using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue477_ArgumentException_while_resolving
    {
        [Test]
        public void Test()
        {
            var c = new Container();

            var myFoo = new Foo();
            c.RegisterDelegate<Foo>(r => myFoo);
            c.Register<IBaz, Baz>();

            c.Register<Bar>(Made.Of(() => new Bar(Arg.Of<Foo>(), Arg.Of<IBaz>())));

            var test = c.Resolve<Bar>();
        }

        public class Base
        {
        }

        class Foo : Base
        {
        }

        interface IBaz { }

        class Baz : IBaz
        {
            public Baz(Foo f) { }
        }

        class Bar
        {
            public Bar(Base b, IBaz baz) { }
        }
    }
}
