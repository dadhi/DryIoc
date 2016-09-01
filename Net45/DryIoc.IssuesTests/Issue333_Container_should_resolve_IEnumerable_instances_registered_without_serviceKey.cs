using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue333_Container_should_resolve_IEnumerable_instances_registered_without_serviceKey
    {
        [Test]
        public void Can_globally_filter_keyed_services_from_collection()
        {
            var container = new Container();

            container.Register(typeof(IEnumerable<>),
                made: Made.Of(typeof(Decorators).GetSingleMethodOrNull(nameof(Decorators.SkipKeyedServices))),
                setup: Setup.Decorator);

            container.Register<IAction, ActionOne>();
            container.Register<IAction, ActionTwo>(serviceKey: nameof(ActionTwo));

            var actions = container.Resolve<IEnumerable<IAction>>();
            Assert.IsInstanceOf<ActionOne>(actions.Single());
        }

        [Test]
        public void Can_globally_filter_keyed_services_and_the_keyed_dependencies_from_collection()
        {
            var container = new Container();

            container.Register(typeof(IEnumerable<>),
                made: Made.Of(typeof(Decorators).GetSingleMethodOrNull(nameof(Decorators.SkipKeyedServices))),
                setup: Setup.Decorator);

            container.Register<IActionUser, ActionUserOne>(serviceKey: nameof(ActionUserOne));
            container.Register<IActionUser, ActionUserTwo>();

            container.Register<IAction, ActionOne>();
            container.Register<IAction, ActionTwo>(serviceKey: nameof(ActionTwo));

            var actionUsers = container.Resolve<IEnumerable<IActionUser>>();
            var actionUser = actionUsers.Single();
            Assert.IsInstanceOf<ActionUserTwo>(actionUser);

            var action = ((ActionUserTwo)actionUser).DefaultActions.Single();
            Assert.IsInstanceOf<ActionOne>(action);
        }

        public static class Decorators
        {
            public static IEnumerable<T> SkipKeyedServices<T>(IEnumerable<KeyValuePair<DefaultKey, T>> services)
            {
                return services.Select(it => it.Value);
            }
        }

        public interface IAction { }
        public class ActionOne : IAction { }
        public class ActionTwo : IAction { }

        public interface IActionUser { }

        public class ActionUserOne : IActionUser
        {
            public IEnumerable<IAction> DefaultActions { get; private set; }

            public ActionUserOne(IEnumerable<IAction> defaultActions)
            {
                DefaultActions = defaultActions;
            }
        }

        public class ActionUserTwo : IActionUser
        {
            public IEnumerable<IAction> DefaultActions { get; private set; }

            public ActionUserTwo(IEnumerable<IAction> defaultActions)
            {
                DefaultActions = defaultActions;
            }
        }
    }
}
