using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue429_Resolve_instance_from_named_scope_with_Func
    {
        [Test]
        public void Original_test()
        {
            var container = new Container();

            container.Register<Service>();

            container.Register<IAction, ActionOne>(Reuse.InCurrentNamedScope("1"));
            container.Register<IAction, ActionTwo>(Reuse.InCurrentNamedScope("2"));

            using (var scope = container.OpenScope("1"))
            {
                var factory = scope.Resolve<Func<int, Service>>();
                var service = factory(1);
            }
        }

        [Test]
        public void Ok_to_resolve_one_scoped_service_wrapped_in_func_before_the_scope_is_opened()
        {
            var container = new Container(scopeContext: new ThreadScopeContext());

            container.Register<Service>();

            container.Register<IAction, ActionOne>(Reuse.InCurrentNamedScope("1"));
            var factory = container.Resolve<Func<int, Service>>();

            using (var scope = container.OpenScope("1"))
            {
                var service = factory(1);
            }
        }

        [Test]
        public void Unable_to_resolve_one_scoped_service_wrapped_in_func_before_the_not_matched_scope_is_opened()
        {
            var container = new Container(scopeContext: new ThreadScopeContext());

            container.Register<Service>();

            container.Register<IAction, ActionOne>(Reuse.InCurrentNamedScope("1"));
            var factory = container.Resolve<Func<int, Service>>();

            using (container.OpenScope("2"))
            {
                Assert.Throws<ContainerException>(() =>
                    factory(1));
            }
        }

        [Test]
        public void Unable_to_select_from_two_services_Resolved_in_func_before_scope_is_opened()
        {
            var container = new Container(scopeContext: new ThreadScopeContext());

            container.Register<Service>();

            container.Register<IAction, ActionOne>(Reuse.InCurrentNamedScope("1"));
            container.Register<IAction, ActionOne>(Reuse.InCurrentNamedScope("2"));

            Assert.Throws<ContainerException>(() =>
                container.Resolve<Func<int, Service>>());
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
            private readonly IAction _action;

            public Service(IAction action, int i)
            {
                _action = action;
            }
        }
    }
}
