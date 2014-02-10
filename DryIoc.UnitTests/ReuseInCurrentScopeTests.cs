using System;
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
            var container = new Container();
            container.Register<Log>(Reuse.InScope);

            var outerLog = container.Resolve<Log>();
            using (var scope = container.CreateScope())
            {
                var scopedLog1 = scope.Resolve<Log>();
                var scopedLog2 = scope.Resolve<Log>();

                Assert.That(scopedLog1, Is.SameAs(scopedLog2));
                Assert.That(scopedLog1, Is.Not.SameAs(outerLog));
            }
        }

        [Test]
        public void Can_reuse_instances_in_two_level_nested_scope()
        {
            var container = new Container();
            container.Register<Log>(Reuse.InScope);

            var outerLog = container.Resolve<Log>();
            using (var scope = container.CreateScope())
            {
                var scopedLog = scope.Resolve<Log>();

                using (var deepScope = scope.CreateScope())
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

        [Test]
        public void Can_reuse_injected_dependencies_in_new_open_scope()
        {
            var container = new Container();
            container.Register<Consumer>();
            container.Register<Account>(Reuse.Singleton);
            container.Register<Log>(Reuse.InScope);

            var outerConsumer = container.Resolve<Consumer>();
            using (var scope = container.CreateScope())
            {
                var scopedConsumer1 = scope.Resolve<Consumer>();
                var scopedConsumer2 = scope.Resolve<Consumer>();

                Assert.That(scopedConsumer1.Log, Is.SameAs(scopedConsumer2.Log));
                Assert.That(scopedConsumer1.Log, Is.Not.SameAs(outerConsumer.Log));
            }
        }

        [Test]
        public void Can_reuse_injected_dependencies_in_two_level_nested_scope()
        {
            var container = new Container();
            container.Register<Consumer>();
            container.Register<Account>(Reuse.Singleton);
            container.Register<Log>(Reuse.InScope);

            var outerConsumer = container.Resolve<Consumer>();
            using (var scope = container.CreateScope())
            {
                var scopedConsumer = scope.Resolve<Consumer>();

                using (var deepScope = scope.CreateScope())
                {
                    var deepConsumer1 = deepScope.Resolve<Consumer>();
                    var deepConsumer2 = deepScope.Resolve<Consumer>();

                    Assert.That(deepConsumer1.Log, Is.SameAs(deepConsumer2.Log));
                    Assert.That(deepConsumer1.Log, Is.Not.SameAs(scopedConsumer.Log));
                    Assert.That(deepConsumer1.Log, Is.Not.SameAs(outerConsumer.Log));
                }

                Assert.That(scopedConsumer.Log, Is.Not.SameAs(outerConsumer.Log));
            }
        }

        [Test]
        public void Cannot_create_service_after_scope_is_disposed()
        {
            var container = new Container();
            container.Register<Log>(Reuse.InScope);

            Func<Log> getLog;
            using (var containerWithNewScope = container.CreateScope())
                getLog = containerWithNewScope.Resolve<Func<Log>>();

            Assert.Throws<ContainerException>(() => getLog());
            Assert.Throws<ContainerException>(() => getLog());
        }

        [Test]
        public void Scope_can_be_safely_disposed_multiple_times_It_does_NOT_throw()
        {
            var container = new Container();
            container.Register<Log>(Reuse.InScope);

            var containerWithNewScope = container.CreateScope();
            containerWithNewScope.Resolve<Log>();
            containerWithNewScope.Dispose();

            Assert.DoesNotThrow(
                containerWithNewScope.Dispose);
        }

        [Test]
        public void Nested_scope_disposition_should_not_affect_outer_scope_factories()
        {
            var container = new Container();
            container.Register<Log>(Reuse.InScope);

            var getLog = container.Resolve<Func<Log>>();
            using (container.CreateScope()) { }

            Assert.DoesNotThrow(() => getLog());
        }

        [Test]
        public void Nested_scope_disposition_should_not_affect_singleton_resolution_for_parent_container()
        {
            var container = new Container();
            container.Register<IService, Service>(Reuse.Singleton);

            IService serviceInNestedScope;
            using (container.CreateScope())
                serviceInNestedScope = container.Resolve<Func<IService>>()();

            var serviceInOuterScope = container.Resolve<Func<IService>>()();

            Assert.That(serviceInNestedScope, Is.SameAs(serviceInOuterScope));
        }
    }
}