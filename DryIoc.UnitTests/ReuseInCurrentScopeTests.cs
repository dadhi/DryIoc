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

            var log = container.Resolve<Log>();
            using (var containerWithNewScope = container.OpenScope())
            {
                var logScoped1 = containerWithNewScope.Resolve<Log>();
                var logScoped2 = containerWithNewScope.Resolve<Log>();

                Assert.That(logScoped1, Is.SameAs(logScoped2));
                Assert.That(logScoped1, Is.Not.SameAs(log));
            }
        }

        [Test]
        public void Can_reuse_dependencies_in_new_open_scope()
        {
            var container = new Container();
            container.Register<Consumer>();
            container.Register<Account>(Reuse.Singleton);
            container.Register<Log>(Reuse.InCurrentScope);

            var consumer = container.Resolve<Consumer>();
            using (var containerWithNewScope = container.OpenScope())
            {
                var consumerScoped1 = containerWithNewScope.Resolve<Consumer>();
                var consumerScoped2 = containerWithNewScope.Resolve<Consumer>();

                Assert.That(consumerScoped1.Log, Is.SameAs(consumerScoped2.Log).And.Not.SameAs(consumer.Log));
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

            Assert.Throws<ContainerException>(() => getLog());
            Assert.Throws<ContainerException>(() => getLog());
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
        public void Nested_scope_disposition_should_not_affect_outer_scope_factories()
        {
            var container = new Container();
            container.Register<Log>(Reuse.InCurrentScope);

            var getLog = container.Resolve<Func<Log>>();
            using (container.OpenScope()) { }

            Assert.DoesNotThrow(() => getLog());
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
    }
}