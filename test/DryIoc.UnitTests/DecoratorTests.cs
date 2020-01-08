using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using ImTools;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class DecoratorTests
    {
        [Test]
        public void Should_resolve_decorator()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, MeasureExecutionTimeOperationDecorator>(setup: Setup.Decorator);

            var decorator = container.Resolve<IOperation>();

            Assert.IsInstanceOf<MeasureExecutionTimeOperationDecorator>(decorator);
        }

        [Test]
        public void Should_resolve_decorator_of_decorator()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, MeasureExecutionTimeOperationDecorator>(setup: Setup.Decorator);
            container.Register<IOperation, RetryOperationDecorator>(setup: Setup.Decorator);

            var decorator = (RetryOperationDecorator)container.Resolve<IOperation>();

            Assert.IsInstanceOf<MeasureExecutionTimeOperationDecorator>(decorator.Decorated);
        }

        [Test]
        public void Should_resolve_decorator_for_named_service()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, AnotherOperation>(serviceKey: "Another");
            container.Register<IOperation, RetryOperationDecorator>(setup: Setup.Decorator);

            var decorator = (RetryOperationDecorator)container.Resolve<IOperation>("Another");

            Assert.IsInstanceOf<AnotherOperation>(decorator.Decorated);
        }

        [Test]
        public void Should_NOT_cache_decorator_so_it_could_decorated_another_service()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>(serviceKey: "Some");
            container.Register<IOperation, AnotherOperation>(serviceKey: "Another");
            container.Register<IOperation, RetryOperationDecorator>(setup: Setup.Decorator);

            var some = (RetryOperationDecorator)container.Resolve<IOperation>("Some");
            var another = (RetryOperationDecorator)container.Resolve<IOperation>("Another");

            Assert.That(some.Decorated, Is.InstanceOf<SomeOperation>());
            Assert.That(another.Decorated, Is.InstanceOf<AnotherOperation>());
        }

        [Test]
        public void Should_resolve_generic_decorator()
        {
            var container = new Container();
            container.Register(typeof(IOperation<>), typeof(SomeOperation<>));
            container.Register(typeof(IOperation<>), typeof(MeasureExecutionTimeOperationDecorator<>), setup: Setup.Decorator);

            var decorator = container.Resolve<IOperation<string>>();

            Assert.That(decorator, Is.InstanceOf<MeasureExecutionTimeOperationDecorator<string>>());
        }

        [Test]
        public void Should_resolve_closed_service_with_open_generic_decorator()
        {
            var container = new Container();
            container.Register<IOperation<int>, SomeOperation<int>>();
            container.Register(typeof(IOperation<>), typeof(MeasureExecutionTimeOperationDecorator<>), setup: Setup.Decorator);

            var operation = container.Resolve<IOperation<int>>();

            Assert.That(operation, Is.InstanceOf<MeasureExecutionTimeOperationDecorator<int>>());
        }

        [Test]
        public void Should_resolve_generic_decorator_of_decorator()
        {
            var container = new Container();
            container.Register(typeof(IOperation<>), typeof(SomeOperation<>));
            container.Register(typeof(IOperation<>), typeof(MeasureExecutionTimeOperationDecorator<>), setup: Setup.Decorator);
            container.Register(typeof(IOperation<>), typeof(RetryOperationDecorator<>), setup: Setup.Decorator);

            var decorator = (RetryOperationDecorator<int>)container.Resolve<IOperation<int>>();

            Assert.That(decorator.Decorated, Is.InstanceOf<MeasureExecutionTimeOperationDecorator<int>>());
        }

        [Test]
        public void Should_resolve_generic_decorator_of_closed_decorator_of_generic_service()
        {
            var container = new Container();
            container.Register(typeof(IOperation<>), typeof(SomeOperation<>));
            container.Register(typeof(IOperation<int>), typeof(MeasureExecutionTimeOperationDecorator<int>), setup: Setup.Decorator);
            container.Register(typeof(IOperation<>), typeof(RetryOperationDecorator<>), setup: Setup.Decorator);

            var decorator = (RetryOperationDecorator<int>)container.Resolve<IOperation<int>>();

            Assert.That(decorator.Decorated, Is.InstanceOf<MeasureExecutionTimeOperationDecorator<int>>());
        }

        [Test]
        public void Resolve_could_NOT_select_closed_over_generic_decorator_cause_their_are_not_related()
        {
            var container = new Container();
            container.Register(typeof(IOperation<>), typeof(SomeOperation<>));
            container.Register(typeof(IOperation<>), typeof(MeasureExecutionTimeOperationDecorator<>), setup: Setup.Decorator);
            container.Register(typeof(IOperation<int>), typeof(MeasureExecutionTimeOperationDecorator<int>), setup: Setup.Decorator);

            var decorator = (MeasureExecutionTimeOperationDecorator<int>)container.Resolve<IOperation<int>>();

            Assert.That(decorator.Decorated, Is.InstanceOf<MeasureExecutionTimeOperationDecorator<int>>());
        }

        [Test]
        public void Should_resolve_decorator_array()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, AnotherOperation>();
            container.Register<IOperation, RetryOperationDecorator>(setup: Setup.Decorator);

            var ops = container.Resolve<IOperation[]>();

            Assert.That(ops[0], Is.InstanceOf<RetryOperationDecorator>());
            Assert.That(((RetryOperationDecorator)ops[0]).Decorated, Is.InstanceOf<SomeOperation>());
            Assert.That(ops[1], Is.InstanceOf<RetryOperationDecorator>());
            Assert.That(((RetryOperationDecorator)ops[1]).Decorated, Is.InstanceOf<AnotherOperation>());
        }

        [Test]
        public void Should_resolve_wrappers_of_decorator_array()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, AnotherOperation>();
            container.Register<IOperation, RetryOperationDecorator>(setup: Setup.Decorator);

            var ops = container.Resolve<Lazy<IOperation>[]>();

            Assert.That(ops[0].Value, Is.InstanceOf<RetryOperationDecorator>());
            Assert.That(((RetryOperationDecorator)ops[0].Value).Decorated, Is.InstanceOf<SomeOperation>());
            Assert.That(ops[1].Value, Is.InstanceOf<RetryOperationDecorator>());
            Assert.That(((RetryOperationDecorator)ops[1].Value).Decorated, Is.InstanceOf<AnotherOperation>());
        }

        [Test]
        public void Should_support_decorator_implementation_without_decorated_service_argument_in_constructor()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, AnotherOperation>(setup: Setup.Decorator);

            var operation = container.Resolve<IOperation>();
            //var operationExpr = container.Resolve<Container.DebugExpression<IOperation>>();

            Assert.That(operation, Is.InstanceOf<AnotherOperation>());
        }

        [Test]
        public void Replacing_decorator_reuse_may_different_from_decorated_service()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>(Reuse.Singleton);
            container.Register<IOperation, AnotherOperation>(setup: Setup.Decorator);

            var first = container.Resolve<IOperation>();
            var second = container.Resolve<IOperation>();

            Assert.That(first, Is.InstanceOf<AnotherOperation>());
            Assert.That(first, Is.Not.SameAs(second));
        }

        [Test]
        public void Replacing_decorator_may_be_non_transient()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>(Reuse.Singleton);
            container.Register<IOperation, AnotherOperation>(Reuse.Singleton, setup: Setup.Decorator);

            var first = container.Resolve<IOperation>();
            var second = container.Resolve<IOperation>();

            Assert.That(first, Is.InstanceOf<AnotherOperation>());
            Assert.That(first, Is.SameAs(second));
        }

        [Test]
        public void Normal_decorator_may_be_non_transient()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, RetryOperationDecorator>(Reuse.Singleton, setup: Setup.Decorator);

            var operation = container.Resolve<IOperation>();

            Assert.That(operation, Is.InstanceOf<RetryOperationDecorator>());
            Assert.That(((RetryOperationDecorator)operation).Decorated, Is.InstanceOf<SomeOperation>());
        }

        [Test]
        public void Should_support_decorator_of_decorator_without_decorated_service_argument_in_constructor()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, AnotherOperation>(setup: Setup.Decorator);
            container.Register<IOperation, RetryOperationDecorator>(setup: Setup.Decorator);

            var operation = container.Resolve<IOperation>();

            Assert.That(operation, Is.InstanceOf<RetryOperationDecorator>());
            Assert.That(((RetryOperationDecorator)operation).Decorated, Is.InstanceOf<AnotherOperation>());
        }

        [Test]
        public void Should_support_decorating_of_Lazy_service()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, LazyDecorator>(setup: Setup.Decorator);

            var operation = container.Resolve<IOperation>();

            Assert.That(operation, Is.InstanceOf<LazyDecorator>());
        }

        [Test]
        public void Should_support_decorating_of_Lazy_named_service()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>(serviceKey: "some");
            container.Register<IOperation, LazyDecorator>(setup: Setup.Decorator);

            var operation = container.Resolve<IOperation>("some");

            Assert.That(operation, Is.InstanceOf<LazyDecorator>());
        }

        [Test]
        public void Should_apply_decorator_When_resolving_Func_of_decorated_service()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, MeasureExecutionTimeOperationDecorator>(setup: Setup.Decorator);

            var operation = container.Resolve<Func<IOperation>>();

            Assert.That(operation(), Is.InstanceOf<MeasureExecutionTimeOperationDecorator>());
        }

        [Test]
        public void Should_propagate_metadata_to_Meta_wrapper()
        {
            var container = new Container();
            container.Register(typeof(IOperation<>), typeof(SomeOperation<>), setup: Setup.With(metadataOrFuncOfMetadata: "blah"));
            container.Register(typeof(IOperation<>), typeof(MeasureExecutionTimeOperationDecorator<>), setup: Setup.Decorator);
            container.RegisterMany(new[] { typeof(OperationUser<>) });

            var user = container.Resolve<OperationUser<object>>();

            Assert.AreEqual("blah", user.GetOperation.Metadata);
            Assert.IsInstanceOf<MeasureExecutionTimeOperationDecorator<object>>(user.GetOperation.Value());
        }

        [Test]
        public void Possible_to_register_decorator_as_delegate_of_decorated_service()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.RegisterDelegateDecorator<IOperation>(_ => op => new MeasureExecutionTimeOperationDecorator(op));

            var operation = container.Resolve<IOperation>();

            Assert.That(operation, Is.InstanceOf<MeasureExecutionTimeOperationDecorator>());
        }

        [Test]
        public void Possible_to_register_decorator_as_delegate_of_decorated_service_with_additional_dependencies_resolved_from_Container()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IMeasurer, Measurer>();
            container.RegisterDelegateDecorator<IOperation>(r =>
                op => MeasureExecutionTimeOperationDecorator.MeasureWith(op, r.Resolve<IMeasurer>()));

            var operation = container.Resolve<IOperation>();

            Assert.That(operation, Is.InstanceOf<MeasureExecutionTimeOperationDecorator>());
        }

        [Test]
        public void Should_support_decorator_of_service_registered_with_delegate()
        {
            var container = new Container();
            container.RegisterDelegate<IOperation>(_ => new SomeOperation());
            container.Register<IOperation, MeasureExecutionTimeOperationDecorator>(setup: Setup.Decorator);

            var operation = container.Resolve<IOperation>();

            Assert.That(operation, Is.InstanceOf<MeasureExecutionTimeOperationDecorator>());
        }

        [Test]
        public void Should_support_decorator_of_decorator_registered_with_delegates()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.RegisterDelegateDecorator<IOperation>(r => op => new RetryOperationDecorator(op));
            container.RegisterDelegateDecorator<IOperation>(r => op => new MeasureExecutionTimeOperationDecorator(op));

            var operation = container.Resolve<IOperation>();

            Assert.That(operation, Is.InstanceOf<MeasureExecutionTimeOperationDecorator>());
            Assert.That(((MeasureExecutionTimeOperationDecorator)operation).Decorated, Is.InstanceOf<RetryOperationDecorator>());
        }

        [Test]
        public void When_mixing_Type_and_Delegate_decorators_the_registration_order_is_preserved()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, RetryOperationDecorator>(setup: Setup.Decorator);
            container.RegisterDelegateDecorator<IOperation>(r => op => new MeasureExecutionTimeOperationDecorator(op));
            container.Register<IOperation, AsyncOperationDecorator>(setup: Setup.Decorator);

            var operation = container.Resolve<IOperation>();

            Assert.IsInstanceOf<AsyncOperationDecorator>(operation);

            var decorated1 = ((AsyncOperationDecorator)operation).Decorated();
            Assert.IsInstanceOf<MeasureExecutionTimeOperationDecorator>(decorated1);

            var decorated2 = ((MeasureExecutionTimeOperationDecorator)decorated1).Decorated;
            Assert.IsInstanceOf<RetryOperationDecorator>(decorated2);
        }

        [Test]
        public void I_can_ensure_Decorator_order_with_Order_option()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, RetryOperationDecorator>(setup: Setup.DecoratorWith(order: 3));
            container.RegisterDelegateDecorator<IOperation>(r => op => new MeasureExecutionTimeOperationDecorator(op));
            container.Register<IOperation, AsyncOperationDecorator>(setup: Setup.DecoratorWith(order: 1));

            var operation = container.Resolve<IOperation>();

            Assert.IsInstanceOf<RetryOperationDecorator>(operation);

            var decorated1 = ((RetryOperationDecorator)operation).Decorated;
            Assert.IsInstanceOf<AsyncOperationDecorator>(decorated1);

            var decorated2 = ((AsyncOperationDecorator)decorated1).Decorated();
            Assert.IsInstanceOf<MeasureExecutionTimeOperationDecorator>(decorated2);
        }


        [Test]
        public void Delegate_decorator_will_use_decoratee_reuse()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>(Reuse.Singleton);
            container.RegisterDelegateDecorator<IOperation>(r =>
                op => new MeasureExecutionTimeOperationDecorator(op));

            var operation = container.Resolve<IOperation>();

            Assert.IsInstanceOf<MeasureExecutionTimeOperationDecorator>(operation);
            Assert.AreSame(operation, container.Resolve<IOperation>());
        }

        [Test]
        public void Should_support_resolving_Func_with_parameters_of_decorated_service()
        {
            var container = new Container();
            container.Register<IOperation, ParameterizedOperation>();
            container.Register<IOperation, RetryOperationDecorator>(setup: Setup.Decorator);

            var operation = container.Resolve<Func<object, IOperation>>();

            Assert.That(operation("blah"), Is.InstanceOf<RetryOperationDecorator>());
        }

        [Test]
        public void Should_support_resolving_Func_with_parameters_without_decorated_service_argument_in_constructor()
        {
            var container = new Container();
            container.Register<IOperation, ParameterizedOperation>();
            container.Register<IOperation, AnotherOperation>(setup: Setup.Decorator);

            var operation = container.Resolve<Func<object, IOperation>>();

            Assert.That(operation("blah"), Is.InstanceOf<AnotherOperation>());
        }

        [Test]
        public void Should_allow_Register_and_Resolve_of_two_decorators_of_the_same_type()
        {
            var container = new Container();
            container.RegisterDelegate<IOperation>(_ => new SomeOperation());
            container.Register<IOperation, MeasureExecutionTimeOperationDecorator>(setup: Setup.Decorator);
            container.Register<IOperation, MeasureExecutionTimeOperationDecorator>(setup: Setup.Decorator);

            var operation = container.Resolve<IOperation>();

            Assert.That(operation, Is.InstanceOf<MeasureExecutionTimeOperationDecorator>());
            Assert.That(((MeasureExecutionTimeOperationDecorator)operation).Decorated, Is.InstanceOf<MeasureExecutionTimeOperationDecorator>());
        }

        [Test]
        public void Should_support_multiple_decorator_in_object_graph()
        {
            var container = new Container();
            container.Register(typeof(IOperation<>), typeof(SomeOperation<>), setup: Setup.With(metadataOrFuncOfMetadata: "some"));
            container.Register(typeof(IOperation<>), typeof(RetryOperationDecorator<>), setup: Setup.Decorator);

            container.Register<IOperationUser<int>, OperationUser<int>>();
            container.Register(typeof(IOperationUser<>), typeof(LogUserOps<>), setup: Setup.Decorator);

            var user = container.Resolve<IOperationUser<int>>();
            Assert.That(user, Is.InstanceOf<LogUserOps<int>>());
            Assert.That(((LogUserOps<int>)user).Decorated, Is.InstanceOf<OperationUser<int>>());

            var operation = user.GetOperation.Value();
            Assert.That(operation, Is.InstanceOf<RetryOperationDecorator<int>>());
            Assert.That(((RetryOperationDecorator<int>)operation).Decorated, Is.InstanceOf<SomeOperation<int>>());
        }

        [Test]
        public void Should_support_decorator_of_Func_with_parameters()
        {
            var container = new Container();
            container.Register<IOperation, ParameterizedOperation>();
            container.Register<IOperation, FuncWithArgDecorator>(setup: Setup.Decorator);

            var func = container.Resolve<Func<object, IOperation>>();
            var operation = func("hey");
            Assert.That(operation, Is.InstanceOf<FuncWithArgDecorator>());

            var decoratedFunc = ((FuncWithArgDecorator)operation).DecoratedFunc("hey");
            Assert.That(decoratedFunc, Is.InstanceOf<ParameterizedOperation>());
        }

        [Test]
        public void May_decorate_func_of_service()
        {
            var container = new Container();

            container.Register<IOperation, SomeOperation>(Reuse.Singleton);
            container.Register<IOperation, AsyncOperationDecorator>(setup: Setup.Decorator);

            var a = container.Resolve<IOperation>();
            Assert.IsInstanceOf<AsyncOperationDecorator>(a);

            var decorated = ((AsyncOperationDecorator)a).Decorated();
            Assert.IsInstanceOf<SomeOperation>(decorated);
        }

        [Test]
        public void May_next_func_decorator_inside_other_decorator()
        {
            var container = new Container();

            container.Register<IOperation, SomeOperation>(Reuse.Singleton);
            container.Register<IOperation, AsyncOperationDecorator>(Reuse.Singleton, setup: Setup.Decorator);
            container.Register<IOperation, RetryOperationDecorator>(Reuse.Singleton, setup: Setup.Decorator);

            var a = container.Resolve<IOperation>();
            Assert.IsInstanceOf<RetryOperationDecorator>(a);

            var nestedDecorator = ((RetryOperationDecorator)a).Decorated;
            Assert.IsInstanceOf<AsyncOperationDecorator>(nestedDecorator);

            var getOp = ((AsyncOperationDecorator)nestedDecorator).Decorated;

            var decorated = getOp();
            Assert.IsInstanceOf<SomeOperation>(decorated);
            Assert.AreSame(decorated, getOp());
        }

        [Test]
        public void Removing_decorator_before_chaining_it_with_lazy_decorator()
        {
            var container = new Container();

            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, RetryOperationDecorator>(setup: Setup.Decorator);
            container.Register<IOperation, LazyDecorator>(setup: Setup.Decorator);
            container.Register<IOperation, AsyncOperationDecorator>(setup: Setup.Decorator);

            var op = container.Resolve<IOperation>();
            op = ((AsyncOperationDecorator)op).Decorated();
            Assert.IsInstanceOf<LazyDecorator>(op);

            container.Unregister<IOperation>(factoryType: FactoryType.Decorator,
                condition: factory => factory.ImplementationType == typeof(LazyDecorator));

            op = ((LazyDecorator)op).Decorated.Value;
            Assert.IsInstanceOf<RetryOperationDecorator>(op);
        }

        [Test]
        public void Can_decorate_service_type_when_required_type_is_different()
        {
            var container = new Container();
            container.Register<IBird, TalkingBirdDecorator>(setup: Setup.Decorator);
            container.Register<Duck>();

            var bird = container.Resolve<IBird>(typeof(Duck));

            Assert.IsInstanceOf<TalkingBirdDecorator>(bird);
        }

        [Test]
        public void Can_register_custom_Disposer_as_decorator()
        {
            var container = new Container();
            container.Register<Foo>(Reuse.Singleton);

            container.RegisterDelegate(
                _ => new Disposer<Foo>(f => f.IsReleased = true),
                setup: Setup.With(useParentReuse: true));

            container.Register(Made.Of(
                r => ServiceInfo.Of<Disposer<Foo>>(),
                f => f.TrackForDispose(Arg.Of<Foo>())),
                setup: Setup.DecoratorWith(useDecorateeReuse: true));

            var foo = container.Resolve<Foo>();

            container.Dispose();
            Assert.IsTrue(foo.IsReleased);
        }

        [Test]
        public void Can_register_custom_Disposer_via_specific_register_method()
        {
            var container = new Container();
            container.Register<Foo>(Reuse.Singleton);
            container.RegisterDisposer<Foo>(f => f.IsReleased = true);

            var foo = container.Resolve<Foo>();

            container.Dispose();
            Assert.IsTrue(foo.IsReleased);
        }

        [Test]
        public void Can_register_custom_Disposer_via_specific_register_method_with_condition()
        {
            var container = new Container();
            container.Register<Foo>(Reuse.Singleton, serviceKey: "blah");
            container.RegisterDisposer<Foo>(f => f.IsReleased = true, r => "blah".Equals(r.ServiceKey));

            var foo = container.Resolve<Foo>("blah");

            container.Dispose();
            Assert.IsTrue(foo.IsReleased);
        }

        [Test]
        public void Can_register_2_custom_Disposers()
        {
            var container = new Container();
            container.Register<Foo>(Reuse.Singleton);
            var released = false;
            container.RegisterDisposer<Foo>(f =>
            {
                if (f.IsReleased)
                    released = true;
                f.IsReleased = true;
            });
            container.RegisterDisposer<Foo>(f =>
            {
                if (f.IsReleased)
                    released = true;
                f.IsReleased = true;
            });

            var foo = container.Resolve<Foo>();

            container.Dispose();
            Assert.IsTrue(foo.IsReleased);
            Assert.IsTrue(released);
        }

        [Test]
        public void Can_register_2_custom_Disposers_for_keyed_service()
        {
            var container = new Container();
            container.Register<Foo>(Reuse.Singleton, serviceKey: 1);
            var released = false;
            container.RegisterDisposer<Foo>(f =>
            {
                if (f.IsReleased)
                    released = true;
                f.IsReleased = true;
            });
            container.RegisterDisposer<Foo>(f =>
            {
                if (f.IsReleased)
                    released = true;
                f.IsReleased = true;
            });

            var foo = container.Resolve<Foo>(1);

            container.Dispose();
            Assert.IsTrue(foo.IsReleased);
            Assert.IsTrue(released);
        }

        [Test]
        public void Decorator_created_by_factory_should_be_compasable_with_other_decorator()
        {
            var container = new Container();
            container.Register<A>();

            container.Register<FB>();
            container.Register(
                Made.Of(r => ServiceInfo.Of<FB>(),
                f => f.Decorate(Arg.Of<A>())),
                setup: Setup.Decorator);

            container.Register<A, C>(setup: Setup.Decorator);

            var a = container.Resolve<A>();
            Assert.IsInstanceOf<C>(a);

            var c = (C)a;
            Assert.IsInstanceOf<B>(c.A);
        }

        [Test]
        public void Can_register_decorator_of_any_T_As_object()
        {
            var container = new Container();

            container.Register<S>();
            container.Register<object>(made: Made.Of(r =>
                GetType().SingleMethod(nameof(PutMessage)).MakeGenericMethod(r.ServiceType)),
                setup: Setup.Decorator);

            var s = container.Resolve<S>();
            Assert.AreEqual("Ok", s.Message);
        }

        [Test]
        public void If_decorator_of_any_T_has_not_compatible_decoratee_type_It_should_throw()
        {
            var container = new Container();

            container.Register<S>();

            container.Register<object>(made: Made.Of(r =>
                GetType().SingleMethod(nameof(PutMessage2)).MakeGenericMethod(r.ServiceType)),
                setup: Setup.Decorator);

            Assert.Throws<ArgumentException>(() =>
            container.Resolve<S>());
        }

        [Test]
        public void If_decorator_of_any_T_returns_unexpected_decorator_type_It_should_throw()
        {
            var container = new Container();

            container.Register<S>();

            container.Register<object>(made: Made.Of(r =>
                GetType().SingleMethod(nameof(PutMessage3)).MakeGenericMethod(r.ServiceType)),
                setup: Setup.Decorator);

            var ex = Assert.Throws<ContainerException>(() =>
            container.Resolve<S>());

            Assert.AreEqual(Error.ServiceIsNotAssignableFromFactoryMethod, ex.Error);
        }

        [Test]
        public void Can_register_decorator_of_any_T_As_object_and_specified_order_of_application()
        {
            var container = new Container();

            container.Register<S>();
            container.Register<S, SS>(setup: Setup.Decorator);
            container.Register<object>(
                made: Made.Of(GetType().SingleMethod(nameof(PutMessage))),
                setup: Setup.DecoratorWith(order: -1));

            var s = container.Resolve<S>();
            Assert.AreEqual("OkNot", s.Message);
        }

        [Test]
        public void I_can_register_decorator_with_key_to_identify_decoratee()
        {
            var container = new Container();

            container.Register<S>(serviceKey: "a");
            container.Register<S, SS>(setup: Setup.Decorator);

            var s = container.Resolve<S>(serviceKey: "a");
            Assert.AreEqual("Not", s.Message);
        }

        [Test]
        public void Can_register_decorator_of_T()
        {
            var container = new Container();

            container.Register<X>();
            container.Register<object>(
                made: Made.Of(r => typeof(DecoratorFactory)
                    .SingleMethod(nameof(DecoratorFactory.Decorate))
                    .MakeGenericMethod(r.ServiceType)),
                setup: Setup.Decorator);

            var x = container.Resolve<X>();
            Assert.IsTrue(x.IsStarted);
        }

        public interface IStartable
        {
            void Start();
        }

        public class X : IStartable
        {
            public bool IsStarted { get; private set; }

            public void Start()
            {
                IsStarted = true;
            }
        }

        public static class DecoratorFactory
        {
            public static T Decorate<T>(T service) where T : IStartable
            {
                service.Start();
                return service;
            }
        }

        public class S
        {
            public string Message = "";
        }

        public class SS : S
        {
            public SS(S s)
            {
                Message = s.Message + "Not";
            }
        }

        public static T PutMessage<T>(T t)
        {
            var s = t as S;
            if (s != null)
                s.Message += "Ok";
            return t;
        }

        public static T PutMessage2<T>(T t) where T : IDisposable
        {
            var s = t as S;
            if (s != null)
                s.Message = "Ok";
            return t;
        }

        public static string PutMessage3<T>(T t)
        {
            var s = t as S;
            if (s != null)
                s.Message = "Ok";
            return "nope";
        }

        public class A { }

        public class FB : A
        {
            public A Decorate(A a)
            {
                return new B(a);
            }
        }

        class B : A
        {
            public A A { get; private set; }

            public B(A a)
            {
                A = a;
            }
        }

        class C : A
        {
            public A A { get; private set; }

            public C(A a)
            {
                A = a;
            }
        }


        public class Foo
        {
            public bool IsReleased { get; set; }
        }

        public sealed class Disposer<T> : IDisposable
        {
            private readonly Action<T> _dispose;
            private int _state;
            private const int Tracked = 1, Disposed = 2;
            private T _item;

            public Disposer(Action<T> dispose)
            {
                _dispose = dispose.ThrowIfNull();
            }

            public T TrackForDispose(T item)
            {
                if (Interlocked.CompareExchange(ref _state, Tracked, 0) != 0)
                    Throw.It(Error.DisposerTrackForDisposeError, _state == Tracked ? " tracked" : "disposed");
                _item = item;
                return item;
            }

            public void Dispose()
            {
                if (Interlocked.CompareExchange(ref _state, Disposed, Tracked) != Tracked)
                    return;
                var item = _item;
                if (item != null)
                {
                    _dispose(item);
                    _item = default(T);
                }
            }
        }

        public interface IBird { }

        public class Duck : IBird { }

        public class TalkingBirdDecorator : IBird
        {
            public IBird Decoratee { get; private set; }

            public TalkingBirdDecorator(IBird bird)
            {
                Decoratee = bird;
            }
        }

        [Test]
        public void Can_decorate_enumerable_and_alter_the_service_key_filtering()
        {
            var container = new Container();

            container.Register(typeof(IEnumerable<>), setup: Setup.Decorator,
                made: Made.Of(
                    FactoryMethod.Of<DecoratorTests>("FilterCollectionByMultiKeys"),
                    Parameters.Of.Type(r => r.ServiceKey)));

            container.Register<Aaaa>(made: Parameters.Of.Name("bs", serviceKey: "a"));
            container.Register<Bbbb>();
            container.Register<Bbbb>(serviceKey: "a");
            container.Register<Bbbb>(serviceKey: KV.Of<object, int>("a", 1));

            var aaa = container.Resolve<Aaaa>();

            Assert.AreEqual(2, aaa.Bs.Count());
        }

        [Test]
        public void Can_decorate_ienumerable_and_alter_the_service_key_filtering_and_works_with_nested_wrappers()
        {
            var container = new Container();

            container.Register(typeof(IEnumerable<>), setup: Setup.Decorator,
                made: Made.Of(
                    FactoryMethod.Of<DecoratorTests>("FilterCollectionByMultiKeys"),
                    Parameters.Of.Type(r => r.ServiceKey)));

            container.Register<AaaaFunc>(made: Parameters.Of.Name("bs", serviceKey: "a", requiredServiceType: typeof(Bbbb)));
            container.Register<Bbbb>();
            container.Register<Bbbb>(serviceKey: "a");
            container.Register<Bbbb>(serviceKey: KV.Of<object, int>("a", 1));

            var aaa = container.Resolve<AaaaFunc>();

            Assert.AreEqual(2, aaa.Bs.Count());
        }

        [Test]
        public void Can_decorate_array_and_alter_the_service_key_filtering()
        {
            var container = new Container();

            container.Register(typeof(IEnumerable<>), setup: Setup.Decorator,
                made: Made.Of(
                    FactoryMethod.Of<DecoratorTests>("FilterCollectionByMultiKeys"),
                    Parameters.Of.Type(r => r.ServiceKey)));

            container.Register<AaaaArray>(made: Parameters.Of.Name("bs", serviceKey: "a"));
            container.Register<Bbbb>();
            container.Register<Bbbb>(serviceKey: "a");
            container.Register<Bbbb>(serviceKey: KV.Of<object, int>("a", 1));

            var aaa = container.Resolve<AaaaArray>();

            Assert.AreEqual(2, aaa.Bs.Length);
        }

        [Test]
        public void Can_use_different_reuses_for_decorators_based_on_different_decoratee_reuse_in_collection()
        {
            var container = new Container();

            container.Register<BB>();

            container.Register<IAx, A1>(Reuse.Singleton);
            container.Register<IAx, A2>(Reuse.Transient);

            container.Register<IAx, AD>(setup: Setup.DecoratorWith(useDecorateeReuse: true));

            var aax1 = container.Resolve<BB>().Aax;
            Assert.AreNotSame(aax1[0], aax1[1]);

            var aax2 = container.Resolve<BB>().Aax;
            Assert.AreNotSame(aax2[0], aax2[1]);

            Assert.AreSame(aax1[0], aax2[0]);
            Assert.AreNotSame(aax1[1], aax2[1]);
        }

        public interface IAx { }
        public class A1 : IAx { }
        public class A2 : IAx { }
        public class AD : IAx { }

        public class BB
        {
            public IAx[] Aax { get; private set; }

            public BB(IAx[] aax)
            {
                Aax = aax;
            }
        }

        #region CUT

        public static IEnumerable<T> FilterCollectionByMultiKeys<T>(IEnumerable<KeyValuePair<object, T>> source, object serviceKey)
        {
            return serviceKey == null
                ? source.Select(it => it.Value)
                : source.Where(it =>
                    {
                        if (it.Key is DefaultKey)
                            return false;
                        return serviceKey.Equals(it.Key)
                               || it.Key is KV<object, int> && serviceKey.Equals(((KV<object, int>)it.Key).Key);
                    })
                    .Select(it => it.Value);
        }

        public class Bbbb { }

        public class Aaaa
        {
            public readonly IEnumerable<Bbbb> Bs;

            public Aaaa(IEnumerable<Bbbb> bs)
            {
                Bs = bs;
            }
        }

        public class AaaaFunc
        {
            public readonly IEnumerable<Bbbb> Bs;

            public AaaaFunc(IEnumerable<Func<object>> bs)
            {
                Bs = bs.Select(f => f()).Cast<Bbbb>().ToArray();
            }
        }

        public class AaaaArray
        {
            public readonly Bbbb[] Bs;

            public AaaaArray(Bbbb[] bs)
            {
                Bs = bs;
            }
        }

        public interface IOperationUser<T>
        {
            Meta<Func<IOperation<T>>, string> GetOperation { get; }
        }

        public class LogUserOps<T> : IOperationUser<T>
        {
            public readonly IOperationUser<T> Decorated;
            public Meta<Func<IOperation<T>>, string> GetOperation { get { return Decorated.GetOperation; } }

            public LogUserOps(IOperationUser<T> decorated)
            {
                Decorated = decorated;
            }
        }

        public class OperationUser<T> : IOperationUser<T>
        {
            public Meta<Func<IOperation<T>>, string> GetOperation { get; set; }

            public OperationUser(Meta<Func<IOperation<T>>, string> getOperation)
            {
                GetOperation = getOperation;
            }
        }

        public interface IOperation
        {
        }

        public class SomeOperation : IOperation
        {
        }

        public class AnotherOperation : IOperation
        {
        }

        public class ParameterizedOperation : IOperation
        {
            public object Param { get; set; }

            public ParameterizedOperation(object param)
            {
                Param = param;
            }
        }

        public class MeasureExecutionTimeOperationDecorator : IOperation
        {
            public IOperation Decorated;

            public MeasureExecutionTimeOperationDecorator(IOperation operation)
            {
                Decorated = operation;
            }

            public static IOperation MeasureWith(IOperation operation, IMeasurer measurer)
            {
                return new MeasureExecutionTimeOperationDecorator(operation) { Measurer = measurer };
            }

            public IMeasurer Measurer { get; set; }
        }

        public interface IMeasurer
        {
        }

        public class Measurer : IMeasurer
        {
        }

        public class RetryOperationDecorator : IOperation
        {
            public IOperation Decorated;

            public RetryOperationDecorator(IOperation operation)
            {
                Decorated = operation;
            }
        }

        public class AsyncOperationDecorator : IOperation
        {
            public readonly Func<IOperation> Decorated;

            public AsyncOperationDecorator(Func<IOperation> a)
            {
                Decorated = a;
            }
        }

        public interface IOperation<T>
        {
        }

        public class SomeOperation<T> : IOperation<T>
        {
        }

        public class RetryOperationDecorator<T> : IOperation<T>
        {
            public IOperation<T> Decorated;

            public RetryOperationDecorator(IOperation<T> operation)
            {
                Decorated = operation;
            }
        }

        public class MeasureExecutionTimeOperationDecorator<T> : IOperation<T>
        {
            public IOperation<T> Decorated;

            public MeasureExecutionTimeOperationDecorator(IOperation<T> operation)
            {
                Decorated = operation;
            }
        }

        public class LazyDecorator : IOperation
        {
            public Lazy<IOperation> Decorated;

            public LazyDecorator(Lazy<IOperation> decorated)
            {
                Decorated = decorated;
            }
        }

        public class FuncWithArgDecorator : IOperation
        {
            public Func<object, IOperation> DecoratedFunc;

            public FuncWithArgDecorator(Func<object, IOperation> decoratedFunc)
            {
                DecoratedFunc = decoratedFunc;
            }
        }

        #endregion
    }
}
