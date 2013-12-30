using System;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class OpenGenericsTests
    {
        [Test]
        public void Resolving_non_registered_generic_should_throw()
        {
            var container = new Container();

            container.Register(typeof(IService<string>), typeof(Service<string>));

            Assert.Throws<ContainerException>(() =>
                container.Resolve<IService<int>>());
        }

        [Test]
        public void Resolving_generic_should_return_registered_open_generic_impelementation()
        {
            var container = new Container();
            container.Register(typeof(IService<>), typeof(Service<>));

            var service = container.Resolve<IService<int>>();

            Assert.That(service, Is.InstanceOf<Service<int>>());
        }

        [Test]
        public void Resolving_transient_open_generic_implementation_should_work()
        {
            var container = new Container();
            container.Register(typeof(IService<>), typeof(Service<>));

            var one = container.Resolve(typeof(IService<int>));
            var another = container.Resolve(typeof(IService<int>));

            Assert.That(one, Is.Not.SameAs(another));
        }

        [Test]
        public void Resolving_generic_with_generic_arg_as_dependency_should_work()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service));
            container.Register(typeof(IService<>), typeof(ServiceWithGenericDependency<>));

            var service = (ServiceWithGenericDependency<IService>)container.Resolve(typeof(IService<IService>));

            Assert.That(service.Dependency, Is.InstanceOf<Service>());
        }

        [Test]
        public void Given_open_generic_registered_as_singleton_Resolving_two_closed_generics_should_return_the_same_instance()
        {
            var container = new Container();
            container.Register(typeof(IService<>), typeof(Service<>), Reuse.Singleton);

            var one = container.Resolve(typeof(IService<int>));
            var another = container.Resolve(typeof(IService<int>));

            Assert.AreSame(one, another);
        }

        [Test]
        public void Given_open_generic_registered_as_singleton_Resolving_two_closed_generics_of_different_type_should_not_throw()
        {
            var container = new Container();
            container.Register(typeof(IService<>), typeof(Service<>), Reuse.Singleton);

            var one = container.Resolve(typeof(IService<int>));
            var another = container.Resolve(typeof(IService<string>));

            Assert.AreNotSame(one, another);
        }

        [Test]
        public void Resolving_generic_with_concrete_implementation_should_work()
        {
            var container = new Container();
            container.Register(typeof(IService<string>), typeof(ClosedGenericClass));

            var service = container.Resolve(typeof(IService<string>));

            Assert.That(service, Is.InstanceOf<IService<string>>());
        }

        [Test]
        public void Resolving_open_generic_service_type_should_throw()
        {
            var container = new Container();
            container.Register(typeof(Service<>));

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve(typeof(Service<>)));

            Assert.That(ex.Message, Is.StringContaining("Service<>"));
        }

        [Test]
        public void Given_open_generic_registered_with_recursive_dependency_on_same_type_closed_generic_Resolving_it_should_throw()
        {
            var container = new Container();
            container.Register(typeof(GenericOne<>));

            Assert.Throws<ContainerException>(
                () => container.Resolve<GenericOne<string>>());
        }

        [Test]
        public void Possible_to_select_constructor_for_open_generic_imlementation()
        {
            var container = new Container();
            container.Register(typeof(LazyOne<>),
                withConstructor: t => t.GetConstructor(new[] { typeof(Func<>).MakeGenericType(t.GetGenericArguments()) }));
            container.Register<Service>();

            var service = container.Resolve<LazyOne<Service>>();

            Assert.That(service.LazyValue, Is.InstanceOf<Func<Service>>());
        }

        [Test]
        public void Possible_to_resolve_open_generic_with_constraints()
        {
            var container = new Container();
            container.Register(typeof(GenericWithConstraint<>));
            container.Register<Service>();

            var service = container.Resolve<GenericWithConstraint<Service>>();

            Assert.That(service.Service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Should_handle_reordered_type_arguments_for_closed_generics()
        {
            var container = new Container();
            container.Register(typeof(IDouble<int, string>), typeof(Double<string, int>));

            var service = container.Resolve<IDouble<int, string>>();

            Assert.That(service, Is.InstanceOf<Double<string, int>>());
        }

        [Test]
        public void Should_resolve_with_reordered_type_arguments()
        {
            var container = new Container();
            container.Register(typeof(IDouble<,>), typeof(Double<,>));

            var service = container.Resolve<IDouble<int, string>>();

            Assert.That(service, Is.InstanceOf<Double<string, int>>());
        }

        [Test]
        public void Should_resolve_with_nested_type_arguments()
        {
            var container = new Container();
            container.Register(typeof(IDouble<,>), typeof(DoubleNested<,>));

            var service = container.Resolve<IDouble<Nested<int>, string>>();

            Assert.That(service, Is.InstanceOf<DoubleNested<string, int>>());
        }

        [Test]
        public void Should_Throw_container_exception_for_service_type_with_mismatched_type_arguments()
        {
            var container = new Container();
            container.Register(typeof(IDouble<,>), typeof(DoubleNested<,>));

            container.Resolve<IDouble<int, string>>();
        }

        [Test]
        public void Should_Throw_when_registering_service_with_not_all_type_args_required_by_implementation()
        {
            var container = new Container();

            Assert.Throws<ContainerException>(() => 
                container.Register(typeof(Banana<>), typeof(BananaSplit<,>)));
        }
    }

    #region CUT

    public class LazyOne<T>
    {
        public Func<T> LazyValue { get; set; }
        public T Value { get; set; }

        public LazyOne(T initValue)
        {
            Value = initValue;
        }

        public LazyOne(Func<T> lazyValue)
        {
            LazyValue = lazyValue;
        }
    }

    public class GenericWithConstraint<T> where T : IService, new()
    {
        public T Service { get; set; }

        public GenericWithConstraint(T service)
        {
            Service = service;
        }
    }

    public interface IDouble<T1, T2> { }

    public class Double<T1, T2> : IDouble<T2, T1> { }

    public class DoubleNested<T1, T2> : IDouble<Nested<T2>, T1> {}

    public class Nested<T> {}

    public class BananaSplit<T1, T2> : Banana<T1>, IceCream<T2> {}

    public class Banana<T> {}

    public interface IceCream<T> { }

    #endregion
}
