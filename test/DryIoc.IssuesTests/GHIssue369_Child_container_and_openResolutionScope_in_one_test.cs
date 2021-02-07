using NUnit.Framework;
using System.Linq;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue369_Child_container_and_openResolutionScope_in_one_test
    {
        public interface IActionBehaviorContext
        {
        }

        public class ActionBehaviorContext : IActionBehaviorContext
        {
        }

        public interface IActionBehavior
        {
            IActionBehaviorContext Context { get; }
        }

        public class ActionBehavior : IActionBehavior
        {
            public IActionBehaviorContext Context { get; }
            public ActionBehavior(IActionBehaviorContext ctx) => Context = ctx;
        }

        // [Test] // won't work
        public void Emulating_the_original_test()
        {
            var container = new Container();

            container.Register<IActionBehaviorContext, ActionBehaviorContext>(reuse: Reuse.Scoped);
            container.RegisterDelegate(typeof(IActionBehavior), 
                r => new ActionBehavior(r.Resolve<IActionBehaviorContext>()), 
                setup: Setup.With(openResolutionScope: true));

            using (var child = container.CreateChild(ifAlreadyRegistered:IfAlreadyRegistered.Replace))
            {
                var s1 = child.Resolve<IActionBehaviorContext>(); // WONT WORK, because the we need a scope to resolve the scoped context and we have none 
                var s2 = child.Resolve<IActionBehavior>();
 
                Assert.AreSame(s1, s2.Context); //failed. With my reasoning they should be the same!
                //factory resolve IActionBehaviorContext with help of request
            }
        }
    }
}