using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue382_Different_instances_of_interface_with_Reuse_InCurrentNamedScope
    {
        [Test]
        public void Test()
        {
            var container = new Container();
            container.Register<IAction, ActionOne>(Reuse.InCurrentNamedScope("1"), setup: Setup.With(asResolutionCall: true));
            container.Register<IAction, ActionTwo>(Reuse.InCurrentNamedScope("2"), setup: Setup.With(asResolutionCall: true));
            container.Register<Service>();

            using (var scopeOne = container.OpenScope("1"))
            {
                scopeOne.Resolve<Service>();
            }

            using (var scopeTwo = container.OpenScope("2"))
            {
                scopeTwo.Resolve<Service>(); 
            }
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

        public class Service
        {
            public readonly IAction Action;

            public Service(IAction action)
            {
                Action = action;
            }
        }
    }
}
