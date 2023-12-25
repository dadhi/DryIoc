using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue387_ArgumentException_with_initiliazer : ITest
    {
        public int Run()
        {
            Initializers_for_multiple_interfaces_single_impl_should_select_the_correct_one_for_the_resolved_iface();
            Initializers_for_multiple_interfaces_single_impl_should_select_the_correct_one_for_the_other_resolved_iface();
            Initializers_for_multiple_interfaces_single_impl_should_select_all_for_the_resolved_impl();
            return 3;
        }

        [Test]
        public void Initializers_for_multiple_interfaces_single_impl_should_select_the_correct_one_for_the_resolved_iface()
        {
            var container = new Container();

            container.RegisterInitializer<ITest>((test, resolver) => test.TestMe());
            container.RegisterInitializer<ITest2>((test, resolver) => test.TestMe2());
            container.RegisterMany<Test>();

            var result = container.Resolve<ITest>();

            Assert.AreEqual("TestMe;", ((Test)result).TestLog);
        }

        [Test]
        public void Initializers_for_multiple_interfaces_single_impl_should_select_the_correct_one_for_the_other_resolved_iface()
        {
            var container = new Container();

            container.RegisterInitializer<ITest>((test, resolver) => test.TestMe());
            container.RegisterInitializer<ITest2>((test, resolver) => test.TestMe2());
            container.RegisterMany<Test>();

            var result = container.Resolve<ITest2>();

            Assert.AreEqual("TestMe2;", ((Test)result).TestLog);
        }

        [Test]
        public void Initializers_for_multiple_interfaces_single_impl_should_select_all_for_the_resolved_impl()
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
