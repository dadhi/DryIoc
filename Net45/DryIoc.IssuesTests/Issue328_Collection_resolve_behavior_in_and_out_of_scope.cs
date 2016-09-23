using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue328_Collection_resolve_behavior_in_and_out_of_scope
    {
        [Test, Ignore("fails")]
        public void Cache_should_not_affect_results_for_lazy_enumerable()
        {
            var container = new Container(rules => rules.WithResolveIEnumerableAsLazyEnumerable());

            container.Register<IAction, ActionOne>(Reuse.InCurrentNamedScope("A"));
            container.Register<IAction, ActionTwo>(Reuse.InCurrentNamedScope("B"));

            using (var aScope = container.OpenScope("A"))
            {
                var actionsA = aScope.Resolve<IEnumerable<IAction>>().ToArray();
            }

            var actions = container.Resolve<IEnumerable<IAction>>().ToArray();
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
