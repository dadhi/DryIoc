using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue387_ArgumentException_with_initiliazer
    {
        [Test]
        public void Multiple_initializers_should_work()
        {
            var container = new Container();

            container.RegisterInitializer<ITest>((test, resolver) => test.TestMe());
            container.RegisterInitializer<ITest2>((test, resolver) => test.TestMe2());
            container.RegisterMany<Test>();

            var result = container.Resolve<Test>();
            Assert.AreEqual("TestMe;TestMe2;", result.TestLog);
        }

        public interface ITest
        {
            void TestMe();
        }

        public interface ITest2
        {
            void TestMe2();
        }

        public class Test : ITest, ITest2
        {
            public string TestLog = string.Empty;

            public void TestMe()
            {
                TestLog += "TestMe;";
            }

            public void TestMe2()
            {
                TestLog += "TestMe2;";
            }
        }
    }
}
