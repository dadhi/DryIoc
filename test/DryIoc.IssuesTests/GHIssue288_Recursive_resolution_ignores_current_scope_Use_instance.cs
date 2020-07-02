using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue288_Recursive_resolution_ignores_current_scope_Use_instance
    {
        [Test]
        public void Test()
        {
            var c = new Container(scopeContext: new AsyncExecutionFlowScopeContext());

            c.Register<Child>(Made.Of(() => ChildFactory()), Reuse.Scoped,
                // NOTE: this is important to make the dependency 'dynamic' and be replaceable even if parent expression is cached
                // see https://github.com/dadhi/DryIoc/blob/master/docs/DryIoc.Docs/RulesAndDefaultConventions.md#injecting-dependency-asresolutioncall
                Setup.With(asResolutionCall: true));
            
            c.Register<Parent>(Reuse.Scoped);

            // instance is resolved in outer scope
            using (var outer = c.OpenScope())
            {
                var outerParentInstance = c.Resolve<Parent>();
                Assert.IsNotNull(outerParentInstance);
                
                var outerChildInstance = c.Resolve<Child>();
                Assert.AreSame(outerParentInstance.Child, outerChildInstance);

                using (var _ = c.OpenScope())
                {
                    // inject instance
                    var inner = c.Resolve<IResolverContext>();
                    inner.Use(outerChildInstance);

                    // direct type resolution works
                    var innerChildInstance = c.Resolve<Child>();
                    Assert.AreSame(outerChildInstance, innerChildInstance);

                    // recursive type resolution skips the instance and calls the factory even though the instance is placed to the current scope
                    var innerParentInstance = c.Resolve<Parent>();
                    Assert.AreSame(innerChildInstance, innerParentInstance.Child);
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
