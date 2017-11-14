using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue287_Add_IfUnresolved_ReturnDefaultIfNotRegistered_policy
    {
        class A
        {
            public B B { get; set; }
        }

        class B
        {
            public B(C c) { }
        }

        class C { }

        [Test]
        public void The_property_injection_uses_IfUnresolved_ReturnDefaultIfUnregistered_policy()
        {
            var container = new Container();
            container.Register<A>(made: PropertiesAndFields.Auto); // enable property injection for A
            container.Register<B>();
            // Skipping registration for C

            // Throws an exception because B is registered but cannot be resolved due its missing dependency C
            Assert.Throws<ContainerException>(() =>
                container.Resolve<A>());
        }
    }
}
