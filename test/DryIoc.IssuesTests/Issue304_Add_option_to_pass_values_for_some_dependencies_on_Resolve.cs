using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue304_Add_option_to_pass_values_for_some_dependencies_on_Resolve : ITest
    {
        public int Run()
        {
            Test_parameters_as_container_WithDependencies();
            Test_parameters_as_direct_resolve();
            Test_properties();
            return 3;
        }

        [Test]
        public void Test_parameters_as_container_WithDependencies()
        {
            var c = new Container();
            c.Register<A>();

            var a = c.WithDependencies(Parameters.Of.Type(_ => "Nya!")).Resolve<A>();

            Assert.AreEqual("Nya!", a.Message);
        }

        [Test]
        public void Test_parameters_as_direct_resolve()
        {
            var c = new Container();
            c.Register<A>();

            var a = c.Resolve<A>(new object[] { "Nya!" });

            Assert.AreEqual("Nya!", a.Message);
        }

        [Test]
        public void Test_properties()
        {
            var c = new Container();
            c.Register<A>();

            var a = c.WithDependencies(
                Parameters.IfUnresolvedReturnDefault,
                PropertiesAndFields.Of.Name(nameof(A.Message), _ => "Nya!"))
                .Resolve<A>();

            Assert.AreEqual("Nya!", a.Message);
        }

        public class A
        {
            public string Message { get; set; }

            public A(string message)
            {
                Message = message;
            }
        }
    }
}
