using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    // "think it through"
    public class GHIssue489_Resolve_with_IfUnresolved_ReturnDefaultIfNotRegistered_throws_if_dependency_is_not_registered
    {
        public void Test()
        {
            var c = new Container();

            c.Register<A>();

            var a = c.Resolve<A>(IfUnresolved.ReturnDefaultIfNotRegistered);
            Assert.IsNull(a);
        }

        public class A
        {
            public B B;
            public A(B b) => B = b;
        }

        public class B {}
    }
}