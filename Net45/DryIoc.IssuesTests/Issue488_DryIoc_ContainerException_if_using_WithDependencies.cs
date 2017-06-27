using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue488_DryIoc_ContainerException_if_using_WithDependencies
    {
        [Test]
        public void Can_combine_WithDependency_parameter_spec_and_registration_parameter_spec()
        {
            var container = new Container();

            container.Register<TestParameter>(
                made: Parameters.Of.Type<string>(serviceKey: "someSetting"),
                reuse: Reuse.Singleton);

            container.Register<TestThing>();
            container.UseInstance("in scope", serviceKey: "someSetting");

            var z = new TestThing();
            var t = container.WithDependencies(Parameters.Of.Type(_ => z))
                .Resolve<TestParameter>();

            Assert.AreSame(z, t.Thing);
            Assert.AreEqual("in scope", t.Setting);
        }

        public class TestThing { }

        public class TestParameter
        {
            public TestThing Thing { get; private set; }
            public string Setting { get; private set; }

            public TestParameter(TestThing thing, string setting)
            {
                Thing = thing;
                Setting = setting;
            }
        }

        [Test]
        public void Can_combine_WithDependency_with_optional_parameter()
        {
            var container = new Container();

            container.Register<TestParameter2>(
                made: Parameters.Of.Type<string>(serviceKey: "someSetting"),
                reuse: Reuse.Singleton);

            container.UseInstance("in scope", serviceKey: "someSetting");

            var t = container.WithDependencies(Parameters.Of.Type(_ => "42"))
                .Resolve<TestParameter2>();

            Assert.AreEqual("42", t.Test);
        }

        public class TestParameter2
        {
            public string Test { get; set; }

            public TestParameter2(string t = "Hello World")
            {
                Test = t;
            }
        }
    }
}
