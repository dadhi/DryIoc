using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue518_Select_default_then_resolvable_constructor : ITest
    {
        public int Run()
        {
            Example();
            return 1;
        }

        [Test]
        public void Example()
        {
            var c = new Container();

            c.Register<A1>(made: Made.Of(
                req => FactoryMethod.DefaultConstructor().Invoke(req) 
                    ?? FactoryMethod.ConstructorWithResolvableArguments(req)));

            c.Register<A2>(made: Made.Of(
                req => FactoryMethod.DefaultConstructor().Invoke(req) 
                    ?? FactoryMethod.ConstructorWithResolvableArguments(req)));

            c.Register<B>();

            var a1 = c.Resolve<A1>();
            Assert.IsNotNull(a1);

            var a2 = c.Resolve<A2>();
            Assert.IsNotNull(a2.B);
        }

        class A1 
        {
        }

        class A2
        {
            public readonly B B;
            public A2(B b) => B = b;
        }

        class B {}
    }
}