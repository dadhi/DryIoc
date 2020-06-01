using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue283_Open_generic_decorator_is_not_applied_to_RegisterInstance
    {
        [Test]
        public void Test()
        {
            var c = new Container();

            c.RegisterInstance(new A<string>());

            c.Register(typeof(A<>), made: Made.Of(GetType().GetMethod(nameof(Apply))), setup: Setup.Decorator);

            var a = c.Resolve<A<string>>();
            Assert.IsTrue(a.Decorated);
        }

        public class A<T>
        {
            public bool Decorated;
        }

        public static A<T> Apply<T>(A<T> a)
        {
            a.Decorated = true;
            return a;
        }
    }
}
