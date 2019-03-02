using System;
using System.Linq;
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

            Assert.AreEqual(Error.ResolvingOpenGenericServiceTypeIsNotPossible, ex.Error);
        }

        [Test]
        public void Resolving_open_generic_with_recursive_dependency_of_the_same_type_should_throw()
        {
            var container = new Container();
            container.Register(typeof(GenericOne<>));

            var ex = Assert.Throws<ContainerException>(
                () => container.Resolve<GenericOne<string>>());

            Assert.AreEqual(Error.RecursiveDependencyDetected, ex.Error);
        }

        [Test]
        public void Possible_to_select_constructor_for_open_generic_imlementation()
        {
            var container = new Container();
            container.Register(typeof(LazyOne<>),
                made: Made.Of(t => t.Constructors()
                    .FirstOrDefault(c => c.GetParameters().Any(p => p.ParameterType.GetGenericDefinitionOrNull() == typeof(Func<>)))));
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
        public void Resolve_many_should_filter_out_implementations_with_notfit_constraints()
        {
            var container = new Container();
            container.Register(typeof(IGeneric<>), typeof(GenericWithConstraint<>));
            container.Register(typeof(IGeneric<>), typeof(AnotherGenericWithConstraint<>));
            container.Register<Blah>();

            var services = container.Resolve<IGeneric<Blah>[]>();

            Assert.That(services.Length, Is.EqualTo(1));
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
        public void Should_resolve_with_multiple_level_nested_and_reordered_type_arguments()
        {
            var container = new Container();
            container.Register(typeof(IDouble<,>), typeof(DoubleMultiNested<,>));

            var service = container.Resolve<IDouble<int, Nested<IDouble<Nested<string>, int>>>>();

            Assert.That(service, Is.InstanceOf<DoubleMultiNested<string, int>>());
        }

        [Test]
        public void Should_Throw_when_service_has_multiple_type_args_for_single_implementation_type_parameter()
        {
            var container = new Container();
            container.Register(typeof(IDouble<,>), typeof(DoubleMultiNested<,>));

            Assert.Throws<ContainerException>(() =>
                // should be INT instead of last BOOL
                container.Resolve<IDouble<int, Nested<IDouble<Nested<string>, bool>>>>());
        }

        [Test]
        public void Should_Throw_when_service_has_multiple_type_args_for_single_implementation_type_parameter2()
        {
            var container = new Container();
            container.Register(typeof(IFizz<,>), typeof(BuzzDiffArgCount<,>));

            Assert.Throws<ContainerException>(() =>
                // should be INT instead of last BOOL
                container.Resolve<IFizz<Wrap<string, int>, bool>>());
        }

        [Test]
        public void Should_Throw_when_registering_implementation_with_service_without_some_type_args_specified()
        {
            var container = new Container();

            Assert.Throws<ContainerException>(() =>
                container.Register(typeof(Banana<>), typeof(BananaSplit<,>)));
        }

        [Test]
        public void Should_Throw_container_exception_for_service_type_with_mismatched_type_arguments()
        {
            var container = new Container();
            container.Register(typeof(IDouble<,>), typeof(DoubleNested<,>));

            Assert.Throws<ContainerException>(() =>
                container.Resolve<IDouble<int, string>>());

            container.Resolve<IDouble<Nested<int>, string>>();
        }

        [Test]
        public void Registering_open_generic_implementation_with_non_generic_service_should_Throw()
        {
            var container = new Container();

            Assert.Throws<ContainerException>(() =>
                container.Register(typeof(IDisposable), typeof(IceCreamSource<>)));
        }

        [Test]
        public void Registering_open_generic_implementation_with_matched_closed_generic_service_should_be_OK()
        {
            var container = new Container();

            container.Register(typeof(IceCream<int>), typeof(IceCreamSource<>));

            Assert.IsInstanceOf<IceCreamSource<int>>(container.Resolve<IceCream<int>>());
        }

        [Test]
        public void Registering_open_generic_implementation_with_not_implemented_closed_generic_service_should_Throw()
        {
            var container = new Container();

            Assert.Throws<ContainerException>(() =>
                container.Register(typeof(Banana<int>), typeof(IceCreamSource<>)));
        }

        [Test]
        public void Registering_all_of_implemented_services_should_register_only_those_containing_all_impl_generic_args()
        {
            var container = new Container();
            container.RegisterMany(new[] { typeof(IceCreamSource<>) }, Reuse.Singleton);

            container.Resolve<IceCreamSource<bool>>();
            container.Resolve<IceCream<bool>>();

            var ex = Assert.Throws<ContainerException>(() =>
            container.Resolve<ISomeIface>());

            Assert.AreEqual(Error.UnableToResolveUnknownService, ex.Error);
        }

        [Test]
        public void Given_open_generic_singleton_registered_with_register_many_then_resolving_as_close_generic_should_fail()
        {
            var container = new Container();
            container.RegisterMany(new[] { typeof(IceCreamSource<>) }, Reuse.Singleton);

            var disposable = container.Resolve<LazyEnumerable<ISomeIface>>().ToArray();

            Assert.That(disposable.Length, Is.EqualTo(0));
        }

        [Test]
        public void Registering_generic_but_not_closed_implementation_should_Throw()
        {
            var container = new Container();
            var genericButNotClosedType = typeof(Closed<>).GetBaseType();

            Assert.Throws<ContainerException>(() =>
                container.Register(genericButNotClosedType));
        }

        [Test]
        public void Registering_generic_but_not_closed_service_should_Throw()
        {
            var container = new Container();

            Assert.Throws<ContainerException>(() =>
                container.Register(typeof(Closed<>).GetBaseType(), typeof(Closed<>)));
        }

        [Test] 
        public void Resolving_array_of_generic_implementations_should_select_only_matched_types()
        {
            var container = new Container();
            container.Register(typeof(IBlah<,>), typeof(Blah<,>));
            container.Register(typeof(IBlah<,>), typeof(AnotherBlah<>));

            var services = container.Resolve<IBlah<int, bool>[]>();

            Assert.That(services.Length, Is.EqualTo(1));
            Assert.That(services[0], Is.InstanceOf<Blah<int, bool>>());
        }

        internal interface IBlah<T0, T1> { }
        internal class Blah<T0, T1> : IBlah<T0, T1> { }
        internal class AnotherBlah<T> : IBlah<string, T> { }

        [Test]
        public void Can_match_generic_parameter_from_constraint()
        {
            var container = new Container();
            container.Register(typeof(ICommandHandler<>), typeof(UpdateCommandHandler<,>));

            var handler = container.Resolve<ICommandHandler<UpdateCommand<SpecialEntity>>>();

            Assert.IsInstanceOf<UpdateCommandHandler<SpecialEntity, UpdateCommand<SpecialEntity>>>(handler);
        }

        internal interface ICommandHandler<TCmd> { }
        internal class SpecialEntity { }
        internal class UpdateCommand<TEntity> { }

        internal class UpdateCommandHandler<TEntity, TCommand> : ICommandHandler<TCommand>
            where TEntity : SpecialEntity
            where TCommand : UpdateCommand<TEntity>
        { }

        [Test]
        public void Should_throw_if_generic_service_is_not_implemented_in_impl_type()
        {
            var container = new Container();

            var ex = Assert.Throws<ContainerException>(() => 
            container.Register(typeof(X<>), typeof(Y<>)));

            Assert.AreEqual(ex.Error, Error.RegisteringImplementationNotAssignableToServiceType);
        }

        [Test]
        public void Should_handle_recurred_constraint_pattern()
        {
            var container = new Container();
            container.Register(typeof(Y<>), typeof(X<>));

            var yz = container.Resolve<Y<Z>>();
        
            Assert.IsInstanceOf<X<Z>>(yz);
        }

        internal interface IRecurrable<T> { }
        internal class Y<T> { }
        internal class Z : IRecurrable<Z> { }
        internal class X<T> : Y<T> where T : IRecurrable<T> { }

        [Test]
        public void Resgitration_throws_if_service_does_not_provide_all_generic_parameters()
        {
            var container = new Container();
            
            var ex = Assert.Throws<ContainerException>(() => 
            container.Register(typeof(ICommandHandler<>), typeof(ReplayCommandHandler<,>)));

            Assert.AreEqual(ex.Error, Error.RegisteringOpenGenericServiceWithMissingTypeArgs);
        }

        internal class ReplayCommand<T> { }
        internal class ReplayCommandHandler<TEntity, TCommand> : ICommandHandler<TCommand>
            where TCommand : ReplayCommand<SpecialEntity>
        { }

        [Test]
        public void Can_register_open_generic_returned_by_factory_method()
        {
            var container = new Container();

            container.Register(typeof(IA<>), made: Made.Of(GetType().SingleMethod(nameof(GetAOf), true)));

            container.Resolve<IA<B>>();
        }

        public class B { }

        internal interface IA<T> where T : new()
        {
            T Value { get; }
        }

        internal class A<T> : IA<T> where T : new()
        {
            public T Value { get { return new T(); } }
        }

        internal static A<T> GetAOf<T>() where T : new()
        {
            return new A<T>();
        }

        [Test]
        public void Selecting_constructor_in_open_generic_class_should_work()
        {
            var container = new Container();

            container.Register(typeof(Zzz<>), 
                made: Made.Of(typeof(Zzz<>).Constructor(typeof(string))));

            container.RegisterInstance<string>("x");

            container.Resolve<Zzz<string>>();
        }

        public class Zzz<T>
        {
            public Zzz(int a) {}
            public Zzz(string b) {}
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

        internal interface IGeneric<T>
        {
            T Service { get; }
        }

        internal class GenericWithConstraint<T> : IGeneric<T>
            where T : IService, new()
        {
            public T Service { get; private set; }

            public GenericWithConstraint(T service)
            {
                Service = service;
            }
        }

        internal class AnotherGenericWithConstraint<T> : IGeneric<T>
        {
            public T Service { get; private set; }

            public AnotherGenericWithConstraint(T service)
            {
                Service = service;
            }
        }

        internal class Blah { }

        public interface IDouble<T1, T2> { }

        public class Double<T1, T2> : IDouble<T2, T1> { }

        public class DoubleNested<T1, T2> : IDouble<Nested<T2>, T1> { }

        public class DoubleMultiNested<T1, T2> : IDouble<T2, Nested<IDouble<Nested<T1>, T2>>> { }

        public class Nested<T> { }

        public class BananaSplit<T1, T2> : Banana<T1>, IceCream<T2> { }

        public class Banana<T> { }

        public interface IceCream<T> { }

        public interface ISomeIface { }

        public class IceCreamSource<T> : IceCream<T>, ISomeIface
        {
            public void Dispose()
            {
            }
        }

        public class Open<T> { }

        public class Closed<T> : Open<T> { }

        public class Wrap<T> { }

        public class Wrap<T1, T2> { }

        public interface IFizz<T1, T2> { }

        public class BuzzDiffArgCount<T1, T2> : IFizz<Wrap<T2>, T1> { }

        #endregion
    }
}
