using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue196_Private_and_public_Constructors_in_generic_classes : ITest
    {
        public int Run()
        {
            Test();
            return 1;
        }

        class TestClass<T>
        {
            public TestClass()
            {
            }

            private TestClass(int bar)
            {
            }
        }
        
        [Test]
        public void Test()
        {
            var container = new Container();
            container.Register(typeof(TestClass<>));
            container.Resolve<TestClass<int>>();
        }
    }
}
