using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class AsyncExecutionFlowScopeContextTests
    {
        [Test]
        public void Reuse_can_select_scope_with_specific_name()
        {
            var container = new Container(scopeContext: new AsyncExecutionFlowScopeContext());
            container.Register<Blah>(Reuse.InCurrentNamedScope(1));

            using (var s1 = container.OpenScope(1))
            {
                var blah1 = s1.Resolve<Blah>();

                using (var s2 = s1.OpenScope(2))
                {
                    var blah2 = s2.Resolve<Blah>();
                    Assert.AreSame(blah1, blah2);
                }

                Assert.AreSame(blah1, s1.Resolve<Blah>());
            }
        }

        [Test]
        public void If_not_matched_name_found_then_current_scope_reuse_should_Throw()
        {
            var container = new Container(scopeContext: new AsyncExecutionFlowScopeContext());
            container.Register<Blah>(Reuse.InCurrentNamedScope(1));

            using (var s1 = container.OpenScope())
            {
                var ex = Assert.Throws<ContainerException>(() => s1.Resolve<Blah>());
                Assert.That(ex.Error, Is.EqualTo(Error.UnableToResolveFromRegisteredServices));
            }
        }

        [Test]
        public void If_no_scope_opened_and_no_matched_name_found_then_resolving_should_Throw()
        {
            var container = new Container(scopeContext: new AsyncExecutionFlowScopeContext());
            container.Register<Blah>(Reuse.InCurrentNamedScope(1));

            var ex = Assert.Throws<ContainerException>(() => container.Resolve<Blah>());
            Assert.That(ex.Error, Is.EqualTo(Error.UnableToResolveFromRegisteredServices));
        }

        [Test]
        public void I_can_use_execution_flow_context()
        {
            var container = new Container(scopeContext: new AsyncExecutionFlowScopeContext());

            container.Register<SomeRoot>(Reuse.InCurrentScope);
            container.Register<SomeDep>(Reuse.InCurrentScope);

            SomeDep outerDep;
            using (var scoped = container.OpenScope())
            {
                outerDep = scoped.Resolve<SomeRoot>().Dep;
                Assert.That(outerDep, Is.SameAs(scoped.Resolve<SomeRoot>().Dep));
            }

            using (var scoped = container.OpenScope())
            {
                Assert.That(scoped.Resolve<SomeRoot>().Dep,
                    Is.SameAs(scoped.Resolve<SomeRoot>().Dep));
                Assert.That(outerDep, Is.Not.SameAs(scoped.Resolve<SomeRoot>().Dep));
            }

            Assert.That(outerDep.IsDisposed, Is.True);
        }

        [Test]
        public async Task Scoped_service_should_Not_propagate_over_async_boundary_with_exec_flow_context()
        {
            var c = new Container(scopeContext: new AsyncExecutionFlowScopeContext());
            c.Register<Blah>(Reuse.InCurrentScope);

            using (var b = c.OpenScope())
            {
                var yGoodness = b.Resolve<Blah>();
                await Task.Delay(50).ConfigureAwait(false);
                Assert.AreSame(yGoodness, b.Resolve<Blah>());
                await Task.Delay(50).ConfigureAwait(true);
                Assert.AreSame(yGoodness, b.Resolve<Blah>());
            }
        }

        [Test]
        public async void Scoped_service_should_Not_propagate_over_async_boundary_with_thread_context()
        {
            using (var container = new Container(scopeContext: new ThreadScopeContext()))
            {
                container.Register<Blah>(Reuse.InCurrentScope);

                using (var scope = container.OpenScope())
                {
                    scope.Resolve<Blah>();

                    await Task.Delay(100).ConfigureAwait(false);

                    var ex = Assert.Throws<ContainerException>(() => scope.Resolve<Blah>());
                    Assert.AreEqual(Error.NoCurrentScope, ex.Error);
                }
            }
        }

        [Test]
        public async void Given_thread_context_the_ScopedOrSingleton_service_may_be_resolved_outside_of_context()
        {
            using (var container = new Container(scopeContext: new ThreadScopeContext()))
            {
                container.Register<Blah>(Reuse.ScopedOrSingleton);

                using (var scope = container.OpenScope())
                {
                    var scoped = scope.Resolve<Blah>();

                    await Task.Delay(100).ConfigureAwait(false);

                    var singleton = scope.Resolve<Blah>();
                    Assert.AreNotSame(scoped, singleton);
                }
            }
        }

        [Test]
        public void Undisposed_scope_from_context_should_be_disposed_by_container_disposal()
        {
            using (var container = new Container(scopeContext: new ThreadScopeContext()))
            {
                container.OpenScope();
            }
        }

        [Test]
        public void Different_containers_should_not_conflict_with_ambient_scope()
        {
            var container1 = new Container(scopeContext: new AsyncExecutionFlowScopeContext());
            var container2 = new Container(scopeContext: new AsyncExecutionFlowScopeContext());

            using (container1.OpenScope())
                Assert.DoesNotThrow(() => 
                   container2.OpenScope());
        }

        internal class Blah
        {
        }

        internal class SomeDep : IDisposable
        {
            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        internal class SomeRoot
        {
            public SomeDep Dep { get; private set; }
            public SomeRoot(SomeDep dep)
            {
                Dep = dep;
            }
        }
    }
}
