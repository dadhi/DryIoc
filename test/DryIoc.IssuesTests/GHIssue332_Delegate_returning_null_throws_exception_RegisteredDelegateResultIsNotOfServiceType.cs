using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue332_Delegate_returning_null_throws_exception_RegisteredDelegateResultIsNotOfServiceType : ITest
    {
        public int Run()
        {
            Test();
            return 1;
        }

        [Test]
        public void Test() 
        {
            var container = new Container();
            container.RegisterDelegate(typeof(A), x => null);
            var a = container.Resolve<A>();
            Assert.IsNull(a);
        }

        class A {}
    }
}