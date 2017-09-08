using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue512_InResolutionScopeOf_is_not_working
    {
        [Test]
        public void Test_with_default_container_rules()
        {
            var container = new Container();

            container.Register<TestScope1>();
            container.Register<TestScope2>();
            container.Register<Test1>(Reuse.InResolutionScopeOf<TestScope1>());
            container.Register<Test1>(Reuse.InResolutionScopeOf<TestScope2>());

            var t1 = container.Resolve<TestScope1>();
            var t2 = container.Resolve<TestScope2>();

            Assert.AreNotSame(t1.Test1, t2.Test1);
        }

        [Test]
        public void Test_with_MS_DI_container_rules()
        {
            var container = new Container(rules => rules
                .WithFactorySelector(Rules.SelectLastRegisteredFactory()));

            container.Register<TestScope1>();
            container.Register<TestScope2>();
            container.Register<Test1>(Reuse.InResolutionScopeOf<TestScope1>());
            container.Register<Test1>(Reuse.InResolutionScopeOf<TestScope2>());

            var t1 = container.Resolve<TestScope1>();
            var t2 = container.Resolve<TestScope2>();

            Assert.AreNotSame(t1.Test1, t2.Test1);
        }

        public class TestScope1
        {
            public Test1 Test1 { get; }

            public TestScope1(Test1 test1)
            {
                Test1 = test1;
            }
        }

        public class TestScope2
        {
            public Test1 Test1 { get; }

            public TestScope2(Test1 test1)
            {
                Test1 = test1;
            }
        }

        public class Test1 { }
    }
}
