using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue289_Think_how_to_make_Use_to_directly_replace_scoped_service_without_special_asResolutionCall_setup
    {
        [Test]
        public void Test()
        {
            var c = new Container(scopeContext: new AsyncExecutionFlowScopeContext());

            c.Register<Parent>(Reuse.Scoped);

            c.Register(Made.Of(() => ChildFactory()), Reuse.Scoped);
            var childFactory = ((IContainer)c).ResolveFactory(Request.Create(c, ServiceInfo.Of<Child>()));

            // instance is resolved in outer scope
            using (var _ = c.OpenScope())
            {
                var outerParentInstance = c.Resolve<Parent>();
                Assert.IsNotNull(outerParentInstance);

                var outerChildInstance = c.Resolve<Child>();
                Assert.AreSame(outerParentInstance.Child, outerChildInstance);

                using (var __ = c.OpenScope())
                {
                    // inject instance
                    var inner = c.Resolve<IResolverContext>();
                    inner.CurrentScope.SetOrAdd(childFactory.FactoryID, outerChildInstance);

                    // direct type resolution works
                    var innerChildInstance = c.Resolve<Child>();
                    Assert.AreSame(outerChildInstance, innerChildInstance);

                    // recursive type resolution skips the instance and calls the factory even though the instance is placed to the current scope
                    var innerParentInstance = c.Resolve<Parent>();
                    Assert.AreSame(outerChildInstance, innerParentInstance.Child);
                }
            }
        }

        public class Child
        {
            public int X { get; set; }
        }

        public class Parent
        {
            public readonly Child Child;
            public Parent(Child child) => Child = child;
        }

        public static Child ChildFactory() => new Child();
    }
}
