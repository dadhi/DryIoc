using System;
using System.Linq.Expressions;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;
// ReSharper disable RedundantAssignment

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class LazyTests : ITest
    {
        public int Run()
        {
            Resolved_Lazy_should_be_LazyOfService_type();
            Given_registered_transient_resolved_Lazy_should_create_new_instances();
            Given_registered_singleton_resolved_Lazy_should_create_same_instances();
            Given_registered_singleton_resolving_as_Lazy_should_NOT_create_service_instance_until_Value_is_accessed();
            It_is_possible_to_resolve_Lazy_of_Lazy();
            It_is_possible_to_resolve_Lazy_of_Func();
            Given_registered_service_Injecting_it_as_Lazy_dependency_should_work();
            Lazy_is_dynamic_and_allow_recursive_dependency();
            Resolving_Func_With_Args_of_Lazy_should_throw_for_missing_dependency();
            Lazy_dependency_is_injected_as_nested_Resolve_method();
            Can_resolve_dynamic_dependency_as_Lazy();
            return 11;
        }

        [Test]
        public void Resolved_Lazy_should_be_LazyOfService_type()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service));

            var lazy = container.Resolve<Lazy<IService>>();

            Assert.That(lazy, Is.InstanceOf<Lazy<IService>>());
            Assert.That(lazy.IsValueCreated, Is.False);
        }

        [Test]
        public void Given_registered_transient_resolved_Lazy_should_create_new_instances()
        {
            var container = new Container();
            container.Register(typeof(ISingleton), typeof(Singleton), Reuse.Singleton);

            var one = container.Resolve<Lazy<ISingleton>>();
            var another = container.Resolve<Lazy<ISingleton>>();

            Assert.That(one, Is.Not.SameAs(another));
        }

        [Test]
        public void Given_registered_singleton_resolved_Lazy_should_create_same_instances()
        {
            var container = new Container();
            container.Register(typeof(ISingleton), typeof(Singleton), Reuse.Singleton);

            var one = container.Resolve<Lazy<ISingleton>>();
            var two = container.Resolve<Lazy<ISingleton>>();

            Assert.That(one.Value, Is.SameAs(two.Value));
        }

        [Test]
        public void Given_registered_singleton_resolving_as_Lazy_should_NOT_create_service_instance_until_Value_is_accessed()
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

        enum X { A, B }
        enum Y { A, B }

        void GetE<T>(T e = default(T))
        {
            if (e.GetType() == typeof(X))
                Assert.AreEqual(e, X.A);

            if (e.GetType() == typeof(Y))
                Assert.AreEqual(e, Y.A);
        }

        [Test]
        public void Lazy_is_dynamic_and_allow_recursive_dependency()
        {
            var container = new Container();
            container.Register<Me>();

            var me = container.Resolve<Me>();

            Assert.IsInstanceOf<Me>(me.Self.Value);
        }

        [Test]
        public void Resolving_Func_With_Args_of_Lazy_should_throw_for_missing_dependency()
        {
            var container = new Container();
            container.Register<IServiceWithParameterAndDependency, ServiceWithParameterAndDependency>();
            container.Register<Service>();

            var f = container.Resolve<Func<bool, Lazy<IServiceWithParameterAndDependency>>>();

            Assert.AreEqual(true, f(true).Value.Flag);
        }

        [Test]
        public void Lazy_dependency_is_injected_as_nested_Resolve_method()
        {
            var container = new Container();
            container.Register<Foo>();
            container.Register<IDependency, BarDependency>();

            var fooExpr = container.Resolve<LambdaExpression>(typeof(Foo));

            StringAssert.Contains(".Resolve", fooExpr.ToString());
        }

        internal class BarDependency : IDependency { }

        internal class Foo
        {
            public IDependency Dep { get; private set; }
            public Foo(Lazy<IDependency> dep) { Dep = dep.Value; }
        }

        internal class Me
        {
            public Lazy<Me> Self { get; private set; }

            public Me(Lazy<Me> self)
            {
                Self = self;
            }
        }

        [Test]
        public void Can_resolve_dynamic_dependency_as_Lazy()
        {
            var container = new Container();
            container.Register<Dog>();
            container.Register<Food>(setup: Setup.With(openResolutionScope: true));

            var dog = container.Resolve<Dog>();
            var food = dog.Feed();
            Assert.IsInstanceOf<Food>(food);

            var dogExpr = container.Resolve<LambdaExpression>(typeof(Dog));
            StringAssert.Contains(".Resolve", dogExpr.ToString());
        }

        internal class Dog
        {
            private readonly Lazy<Food> _food;

            public Dog(Lazy<Food> food)
            {
                _food = food;
            }

            public Food Feed()
            {
                return _food.Value;
            }
        }

        internal class Food { }
    }
}
