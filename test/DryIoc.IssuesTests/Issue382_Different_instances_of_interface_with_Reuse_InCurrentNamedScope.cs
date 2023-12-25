using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue382_Different_instances_of_interface_with_Reuse_InCurrentNamedScope : ITest
    {
        public int Run()
        {
            Should_work_as_is();
            Should_work_with_asResolutionCall();
            return 2;
        }

        [Test]
        public void Should_work_as_is()
        {
            var container = new Container();
            container.Register<IAction, ActionOne>(Reuse.InCurrentNamedScope("1"));
            container.Register<IAction, ActionTwo>(Reuse.InCurrentNamedScope("2"));
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

        [Test]
        public void Should_work_with_asResolutionCall()
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
