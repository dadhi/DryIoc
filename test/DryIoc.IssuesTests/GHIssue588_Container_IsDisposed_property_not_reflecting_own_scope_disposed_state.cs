using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue588_Container_IsDisposed_property_not_reflecting_own_scope_disposed_state : ITest
    {
        public int Run()
        {
            InnerScope_tracked_in_OuterScope_is_not_marked_as_disposed();
            return 1;
        }

        [Test]
        public void InnerScope_tracked_in_OuterScope_is_not_marked_as_disposed()
        {
            var container = new Container();
            var outerScope = container.OpenScope("Test", true);
            var innerScope = outerScope.OpenScope("Test2", true);

            outerScope.Dispose();

            Assert.True(innerScope.IsDisposed);
        }
    }
}
