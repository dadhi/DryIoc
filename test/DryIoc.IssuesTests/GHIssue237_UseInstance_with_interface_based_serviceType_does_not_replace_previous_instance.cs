using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue237_UseInstance_with_interface_based_serviceType_does_not_replace_previous_instance : ITest
    {
        public int Run()
        {
            Second_Use_should_replace_the_first_one_in_scope();
            return 1;
        }

        [Test]
        public void Second_Use_should_replace_the_first_one_in_scope()
        {
            var container = new Container();

            container.Register<IUser, User>(Reuse.Singleton); // making it singleton to ensure the same instance returned
            Assert.IsTrue(container.IsRegistered<IUser>());

            var origUser = container.Resolve<IUser>();
            using (var scope = container.OpenScope())
            {
                var newUser = new User { UniqueId = "42" };

                scope.Use<IUser>(newUser); // IMPORTANT: to specify the interface type as type argument
                Assert.AreSame(newUser, scope.Resolve<IUser>());

                scope.Use<IUser>(origUser);
                Assert.AreSame(origUser, scope.Resolve<IUser>());

                // replace the original one again
                scope.Use<IUser>(newUser);
                Assert.AreSame(newUser, scope.Resolve<IUser>());
            }

            // still the original one despite the manipulations in scope
            Assert.AreSame(origUser, container.Resolve<IUser>());
        }

        interface IUser
        {
            string UniqueId { get; set; }
        }

        class User : IUser
        {
            public string UniqueId { get; set; }
        }
    }
}