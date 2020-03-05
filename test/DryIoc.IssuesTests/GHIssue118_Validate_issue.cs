using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue118_Validate_issue
    {
        [Test]
        public void Test()
        {
            using (var container = new Container())
            {
                var outerScoped = Reuse.ScopedTo("Outer");
                container.Register<SomethingDefinedInOuterScope>(outerScoped);
                
                var innerScoped = Reuse.ScopedTo("Inner", false, outerScoped.Lifespan - 1);
                container.Register<SomethingDefinedInInnerScope>(innerScoped);
                container.Validate();

                using (var outerScopedContainer = container.OpenScope(outerScoped.Name))
                {
                    using (var innerScopedContainer = outerScopedContainer.OpenScope(innerScoped.Name))
                    {
                        var ex = Assert.Throws<ContainerException>(() => 
                            innerScopedContainer.Resolve<SomethingDefinedInOuterScope>());
                    }
                }
            }
        }

        class SomethingDefinedInOuterScope
        {
            public SomethingDefinedInOuterScope(SomethingDefinedInInnerScope dependency) // we were expecting DryIoC to not allow that, it's wrong since the dependency has a tighter lifetime
            {
            }
        }

        class SomethingDefinedInInnerScope
        {
        }
    }
}
