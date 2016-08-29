using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue267_False_alarm_about_recursive_dependency_when_using_decorator
    {
        [Test]
        public void Test()
        {
            var ctr = new Container(Rules.Default.With(propertiesAndFields: PropertiesAndFields.Auto));

            ctr.Register<A>();
            ctr.Register<B>();
            ctr.Register<A>(
                Made.Of(() => Decorate(Arg.Of<A>(), Arg.Of<Func<B>>())),
                setup: Setup.Decorator);

            ctr.Resolve<A>();
        }

        static A Decorate(A a, Func<B> b) { return a; }

        class A {}

        class B
        {
            public A A { get; set; }
        }
    }
}
