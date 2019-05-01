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
                container.Register<SomethingDefinedInOuterScope>(Reuse.ScopedTo("Outer"));
                container.Register<SomethingDefinedInInnerScope>(Reuse.ScopedTo("Inner"));
                container.Validate();

                using (var outerScopedContainer = container.OpenScope("Outer"))
                {
                    using (var innerScopedContainer = outerScopedContainer.OpenScope("Inner"))
                    {
                        var item = innerScopedContainer.Resolve<SomethingDefinedInOuterScope>();
                        Assert.IsNotNull(item);
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
