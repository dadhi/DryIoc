using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue348_Create_a_child_container_without_WithNoMoreRegistrationAllowed_flag : ITest
    {
        public int Run()
        {
            Should_allow_registration_in_child_container();
            return 1;
        }

        [Test]
        public void Should_allow_registration_in_child_container()
        {
            var parent = new Container().WithNoMoreRegistrationAllowed();
            var child = parent.WithRegistrationsCopy(IsRegistryChangePermitted.Permitted);

            Assert.DoesNotThrow(() =>
            child.Register<Service>());

            child = child.WithNoMoreRegistrationAllowed();

            var ex = Assert.Throws<ContainerException>(() =>
            child.Register<Service>());

            Assert.AreEqual(Error.NameOf(Error.NoMoreRegistrationsAllowed), ex.ErrorName);
        }

        class Service {}
    }
}
