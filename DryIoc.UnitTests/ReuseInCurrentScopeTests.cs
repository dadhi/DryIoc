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
            container.Register<Log>(Reuse.InCurrentScope);

            var outerLog = container.Resolve<Log>();
            using (var containerWithNewScope = container.CreateNewScope())
            {
                var scopedLog1 = containerWithNewScope.Resolve<Log>();
                var scopedLog2 = containerWithNewScope.Resolve<Log>();

                Assert.That(scopedLog1, Is.SameAs(scopedLog2));
                Assert.That(scopedLog1, Is.Not.SameAs(outerLog));
            }
        }

        [Test]
        public void Can_reuse_dependencies_in_new_open_scope()
        {
            var container = new Container();
            container.Register<Consumer>();
            container.Register<Account>(Reuse.Singleton);
            container.Register<Log>(Reuse.InCurrentScope);

            var outerConsumer = container.Resolve<Consumer>();
            using (var scope = container.CreateNewScope())
            {
                var scopedConsumer1 = scope.Resolve<Consumer>();
                var scopedConsumer2 = scope.Resolve<Consumer>();

                Assert.That(scopedConsumer1.Log, Is.SameAs(scopedConsumer2.Log));
                Assert.That(scopedConsumer1.Log, Is.Not.SameAs(outerConsumer.Log));
            }
        }

        [Test]
        public void Cannot_create_service_after_scope_is_disposed()
        {
            var container = new Container();
            container.Register<Log>(Reuse.InCurrentScope);

            Func<Log> getLog;
            using (var containerWithNewScope = container.CreateNewScope())
                getLog = containerWithNewScope.Resolve<Func<Log>>();

            Assert.Throws<ContainerException>(() => getLog());
            Assert.Throws<ContainerException>(() => getLog());
        }

        [Test]
        public void Scope_can_be_safely_disposed_multiple_times_It_does_NOT_throw()
        {
            var container = new Container();
            container.Register<Log>(Reuse.InCurrentScope);

            var containerWithNewScope = container.CreateNewScope();
            containerWithNewScope.Resolve<Log>();
            containerWithNewScope.Dispose();

            Assert.DoesNotThrow(
                containerWithNewScope.Dispose);
        }

        [Test]
        public void Nested_scope_disposition_should_not_affect_outer_scope_factories()
        {
            var container = new Container();
            container.Register<Log>(Reuse.InCurrentScope);

            var getLog = container.Resolve<Func<Log>>();
            using (container.CreateNewScope()) { }

            Assert.DoesNotThrow(() => getLog());
        }

        [Test]
        public void Nested_scope_disposition_should_not_affect_singleton_resolution_for_parent_container()
        {
            var container = new Container();
            container.Register<IService, Service>(Reuse.Singleton);

            IService serviceInNestedScope;
            using (container.CreateNewScope())
                serviceInNestedScope = container.Resolve<Func<IService>>()();

            var serviceInOuterScope = container.Resolve<Func<IService>>()();

            Assert.That(serviceInNestedScope, Is.SameAs(serviceInOuterScope));
        }
    }
}