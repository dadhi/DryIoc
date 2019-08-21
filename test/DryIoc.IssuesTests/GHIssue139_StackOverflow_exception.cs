using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue139_StackOverflow_exception
    {
        [Test]
        public void Singleton_decorator_should_be_correctly_applied_to_different_implementations()
        {
            var c = new Container();

            c.Register<A>();
            c.Register<B>();

            c.Register<object>(
                made: Made.Of(GetType().SingleMethod(nameof(DecorateS))), 
                setup: Setup.DecoratorOf<S>(),
                reuse: Reuse.Singleton);

            var a = c.Resolve<A>();
            Assert.AreEqual(1, a.CallCount);
            Assert.AreEqual(1, a.B.CallCount);
        }

        public static T DecorateS<T>(T t) where T : S
        {
            t.CallCount += 1;
            return t;
        }

        public interface S
        {
            int CallCount { get; set; }
        }

        public class A : S
        {
            public B B { get; }

            public A(B b) => 
                B = b;

            public int CallCount { get; set; }
        }

        public class B : S
        {
            public int CallCount { get; set; }
        }
    }
}
