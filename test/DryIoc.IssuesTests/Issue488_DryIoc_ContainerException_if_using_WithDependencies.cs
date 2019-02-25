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
            container.RegisterInstance("in scope", serviceKey: "someSetting");

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

            container.RegisterInstance("in scope", serviceKey: "someSetting");

            var t = container.WithDependencies(Parameters.Of.Type(_ => "42"))
                .Resolve<TestParameter2>();

            Assert.AreEqual("42", t.Test);
        }

        public class TestParameter2
        {
            public string Test { get; private set; }

            public TestParameter2(string t = "Hello World")
            {
                Test = t;
            }
        }

        [Test]
        public void Can_combine_WithDependency_property_spec_and_registration_property_spec()
        {
            var container = new Container();

            container.Register<TestParameter3>(
                made: PropertiesAndFields.Of.Name(nameof(TestParameter3.Test), serviceKey: "someSetting"),
                reuse: Reuse.Singleton);

            container.RegisterInstance("instance", serviceKey: "someSetting");

            var thing = new TestThing();

            var t = container
                .WithDependencies(
                    propertiesAndFields: PropertiesAndFields.Of.Name(nameof(TestParameter3.Thing), _ => thing))
                .Resolve<TestParameter3>();

            Assert.AreSame(thing, t.Thing);
            Assert.AreEqual("instance", t.Test);
        }

        public class TestParameter3
        {
            public string Test { get; set; }
            public TestThing Thing { get; set; }
        }
    }
}
