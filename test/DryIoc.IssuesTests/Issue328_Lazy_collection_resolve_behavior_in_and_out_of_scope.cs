using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue328_Lazy_collection_resolve_behavior_in_and_out_of_scope : ITest
    {
        public int Run()
        {
            Cache_should_not_affect_results_for_lazy_enumerable();
            return 1;
        }

        [Test]
        public void Cache_should_not_affect_results_for_lazy_enumerable()
        {
            var container = new Container(rules => rules.WithResolveIEnumerableAsLazyEnumerable());

            container.Register<IAction, ActionOne>(Reuse.InCurrentNamedScope("A"));
            container.Register<IAction, ActionTwo>(Reuse.InCurrentNamedScope("B"));

            using (var scopeA = container.OpenScope("A"))
            {
                var scopedActions = scopeA.Resolve<IEnumerable<IAction>>().ToArray();
                Assert.AreEqual(1, scopedActions.Length);
                Assert.IsInstanceOf<ActionOne>(scopedActions[0]);
            }

            var actions = container.Resolve<IEnumerable<IAction>>().ToArray();
            Assert.AreEqual(0, actions.Length);
        }

        public interface IAction
        {
        }

        public class ActionOne : IAction
        {
        }

        public class ActionTwo : IAction
        {
        }
    }
}
