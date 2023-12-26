using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue434_ReturnDefaultIfNotRegistered_is_not_respected_between_scopes : ITest
    {
        public int Run()
        {
            Test_Use();
            return 1;
        }

        [Test]
        public void Test_Use()
        {
            var container = new Container();

            using (var scope = container.OpenScope())
            {
                var foo = scope.Resolve<Foo>(IfUnresolved.ReturnDefaultIfNotRegistered); // Does not throw
                scope.Use<Foo>(new Foo()); // Do a registration within the scope
            }

            using (var scope = container.OpenScope())
            {
                var foo = scope.Resolve<Foo>(IfUnresolved.ReturnDefaultIfNotRegistered);
                Assert.IsNull(foo);

                var ex = Assert.Throws<ContainerException>(() => 
                    scope.Resolve<Foo>());
                Assert.AreEqual(Error.NameOf(Error.UnableToResolveUnknownService), ex.ErrorName);

                foo = scope.Resolve<Foo>(IfUnresolved.ReturnDefault);
                Assert.IsNull(foo);
            }
        }

        class Foo
        {
        }
    }
}
