using System;
using System.Runtime.Remoting.Messaging;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class ReuseInCurrentScopeTests
    {
        [Test]
        public void Can_reuse_instances_in_new_open_scope()
        {
            using (var container = new Container().OpenScope())
            {
                container.Register<Log>(Reuse.InCurrentScope);

                var outerLog = container.Resolve<Log>();
                using (var scope = container.OpenScope())
                {
                    var scopedLog1 = scope.Resolve<Log>();
                    var scopedLog2 = scope.Resolve<Log>();

                    Assert.That(scopedLog1, Is.SameAs(scopedLog2));
                    Assert.That(scopedLog1, Is.Not.SameAs(outerLog));
                }
            }
        }

        [Test]
        public void Can_reuse_instances_in_three_level_nested_scope()
        {
            using (var container = new Container().OpenScope())
            {
                container.Register<Log>(Reuse.InCurrentScope);

                var outerLog = container.Resolve<Log>();
                using (var scope = container.OpenScope())
                {
                    var scopedLog = scope.Resolve<Log>();

                    using (var deepScope = scope.OpenScope())
                    {
                        var deepLog1 = deepScope.Resolve<Log>();
                        var deepLog2 = deepScope.Resolve<Log>();

                        Assert.That(deepLog1, Is.SameAs(deepLog2));
                        Assert.That(deepLog1, Is.Not.SameAs(scopedLog));
                        Assert.That(deepLog1, Is.Not.SameAs(outerLog));
                    }

                    Assert.That(scopedLog, Is.Not.SameAs(outerLog));
                }
            }
        }

        [Test]
        public void Can_reuse_injected_dependencies_in_new_open_scope()
        {
            var container = new Container();
            container.Register<Consumer>();
            container.Register<Account>();
            container.Register<Log>(Reuse.InCurrentScope);

            using (var scoped = container.OpenScope())
            {
                var outerConsumer = scoped.Resolve<Consumer>();

                using (var nestedScoped = scoped.OpenScope())
                {
                    var scopedConsumer1 = nestedScoped.Resolve<Consumer>();
                    var scopedConsumer2 = nestedScoped.Resolve<Consumer>();

                    Assert.That(scopedConsumer1.Log, Is.SameAs(scopedConsumer2.Log));
                    Assert.That(scopedConsumer1.Log, Is.Not.SameAs(outerConsumer.Log));
                }
            }
        }

        [Test]
        public void CanNot_open_deeply_nested_scope_from_root_container()
        {
            var container = new Container();
            using (container.OpenScope())
            {
                var ex = Assert.Throws<ContainerException>(() => container.OpenScope());
                Assert.That(ex.Error, Is.EqualTo(Error.NOT_DIRECT_SCOPE_PARENT));
                Assert.That(ex.Message, Is.StringContaining("Unable to Open Scope from not a direct parent container"));
            }
        }

        [Test]
        public void Cannot_create_service_after_scope_is_disposed()
        {
            var container = new Container();
            container.Register<Log>(Reuse.InCurrentScope);

            Func<Log> getLog;
            using (var containerWithNewScope = container.OpenScope())
                getLog = containerWithNewScope.Resolve<Func<Log>>();

            Assert.That(Assert.Throws<ContainerException>(() => getLog()).Error, Is.EqualTo(Error.CONTAINER_IS_DISPOSED));
            Assert.That(Assert.Throws<ContainerException>(() => getLog()).Error, Is.EqualTo(Error.CONTAINER_IS_DISPOSED));
        }

        [Test]
        public void Scope_can_be_safely_disposed_multiple_times_It_does_NOT_throw()
        {
            var container = new Container();
            container.Register<Log>(Reuse.InCurrentScope);

            var containerWithNewScope = container.OpenScope();
            containerWithNewScope.Resolve<Log>();
            containerWithNewScope.Dispose();

            Assert.DoesNotThrow(
                containerWithNewScope.Dispose);
        }

        [Test]
        public void Calling_Func_of_scoped_service_outside_of_scope_should_Throw()
        {
            var container = new Container();
            container.Register<Log>(Reuse.InCurrentScope);

            var getLog = container.Resolve<Func<Log>>();
            using (container.OpenScope())
                Assert.That(getLog(), Is.InstanceOf<Log>());

            var ex = Assert.Throws<ContainerException>(() => getLog());
            Assert.That(ex.Error, Is.EqualTo(Error.NO_CURRENT_SCOPE));
            Assert.That(ex.Message, Is.StringContaining("No current scope available: probably you are resolving scoped service outside of scope."));
        }

        [Test]
        public void Nested_scope_disposition_should_not_affect_singleton_resolution_for_parent_container()
        {
            var container = new Container();
            container.Register<IService, Service>(Reuse.Singleton);

            IService serviceInNestedScope;
            using (container.OpenScope())
                serviceInNestedScope = container.Resolve<Func<IService>>()();

            var serviceInOuterScope = container.Resolve<Func<IService>>()();

            Assert.That(serviceInNestedScope, Is.SameAs(serviceInOuterScope));
        }

        [Test]
        public void Reuse_can_select_scope_with_specific_name()
        {
            var container = new Container(scopeContext: new ExecutionFlowScopeContext());
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
            var container = new Container(scopeContext: new ExecutionFlowScopeContext());
            container.Register<Blah>(Reuse.InCurrentNamedScope(1));

            using (var s1 = container.OpenScope())
            {
                var ex = Assert.Throws<ContainerException>(() => s1.Resolve<Blah>());
                Assert.That(ex.Error, Is.EqualTo(Error.NO_MATCHED_SCOPE_FOUND));
            }
        }

        [Test]
        public void If_no_scope_opened_And_not_matched_name_found_then_resolving_should_Throw()
        {
            var container = new Container(scopeContext: new ExecutionFlowScopeContext());
            container.Register<Blah>(Reuse.InCurrentNamedScope(1));
            
            var ex = Assert.Throws<ContainerException>(() => container.Resolve<Blah>());
            Assert.That(ex.Error, Is.EqualTo(Error.NO_CURRENT_SCOPE));
        }

        public sealed class ExecutionFlowScopeContext : IScopeContext
        {
            public static readonly object ROOT_SCOPE_NAME = typeof(ExecutionFlowScopeContext);

            public object RootScopeName { get { return ROOT_SCOPE_NAME; } }

            public IScope GetCurrentOrDefault()
            {
                var scope = (Remote<IScope>)CallContext.LogicalGetData(_key);
                return scope == null ? null : scope.Value;
            }

            public void SetCurrent(Func<IScope, IScope> update)
            {
                var oldScope = GetCurrentOrDefault();
                var newScope = update.ThrowIfNull()(oldScope);
                CallContext.LogicalSetData(_key, new Remote<IScope>(newScope));
            }

            private static readonly string _key = typeof(ExecutionFlowScopeContext).Name;
        }

        public sealed class Remote<T> : MarshalByRefObject
        {
            public readonly T Value;

            public Remote(T value)
            {
                Value = value;
            }
        }

        internal class Blah
        {
        }
    }
}