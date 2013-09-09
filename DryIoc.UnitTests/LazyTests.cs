using System;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class LazyTests
    {
        [Test]
        public void Resolved_Lazy_should_be_LazyOfService_type()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service));

            var lazy = container.Resolve<Lazy<IService>>();

            Assert.That(lazy, Is.InstanceOf<Lazy<IService>>());
        }

        [Test]
        public void Given_registered_transient_Resolved_Lazy_should_create_new_instances()
        {
            var container = new Container();
            container.Register(typeof(ISingleton), typeof(Singleton), Reuse.Singleton);

            var one = container.Resolve<Lazy<ISingleton>>();
            var another = container.Resolve<Lazy<ISingleton>>();
            
            Assert.That(one, Is.Not.SameAs(another));
        }

        [Test]
        public void Given_registered_singleton_Resolved_Lazy_should_create_same_instances()
        {
            var container = new Container();
            container.Register(typeof(ISingleton), typeof(Singleton), Reuse.Singleton);

            var one = container.Resolve<Lazy<ISingleton>>();
            var another = container.Resolve<Lazy<ISingleton>>();

            Assert.That(one.Value, Is.SameAs(another.Value));
        }

        [Test]
        public void Given_registered_singleton_Resolving_as_Lazy_should_NOT_create_service_instance_until_Value_is_accessed()
        {
            var container = new Container();
            container.Register<ServiceWithInstanceCount>(Reuse.Singleton);
            ServiceWithInstanceCount.InstanceCount = 0;

            var lazy = container.Resolve<Lazy<ServiceWithInstanceCount>>();
            Assert.That(ServiceWithInstanceCount.InstanceCount, Is.EqualTo(0));

            Assert.That(lazy.Value, Is.InstanceOf<ServiceWithInstanceCount>());
            Assert.That(ServiceWithInstanceCount.InstanceCount, Is.EqualTo(1));
        }

        [Test]
        public void It_is_possible_to_resolve_Lazy_of_Lazy()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service));

            var lazyOfLazy = container.Resolve<Lazy<Lazy<IService>>>();

            Assert.That(lazyOfLazy, Is.InstanceOf<Lazy<Lazy<IService>>>());
            Assert.That(lazyOfLazy.Value.Value, Is.InstanceOf<Service>());
        }

        [Test]
        public void It_is_possible_to_resolve_Lazy_of_Func()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service));

            var lazyOfFunc = container.Resolve<Lazy<Func<IService>>>();

            Assert.That(lazyOfFunc, Is.InstanceOf<Lazy<Func<IService>>>());
            Assert.That(lazyOfFunc.Value(), Is.InstanceOf<Service>());
        }

        [Test]
        public void Given_registered_service_Injecting_it_as_Lazy_dependency_should_work()
        {
            var container = new Container();
            container.Register(typeof(IDependency), typeof(Dependency));
            container.Register<ServiceWithLazyDependency>();

            var instance = container.Resolve<ServiceWithLazyDependency>();

            Assert.That(instance.LazyOne, Is.Not.Null);
        }
    }
}
