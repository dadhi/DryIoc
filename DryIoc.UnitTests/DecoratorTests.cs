using System;
using NUnit.Framework;

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
            container.Register<IOperation, MeasureExecutionTimeOperationDecorator>(setup: DecoratorSetup.With());

            var decorator = container.Resolve<IOperation>();

            Assert.That(decorator, Is.InstanceOf<MeasureExecutionTimeOperationDecorator>());
        }

        [Test]
        public void Should_resolve_decorator_of_decorator()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, MeasureExecutionTimeOperationDecorator>(setup: DecoratorSetup.With());
            container.Register<IOperation, RetryOperationDecorator>(setup: DecoratorSetup.With());

            var decorator = (RetryOperationDecorator)container.Resolve<IOperation>();

            Assert.That(decorator.Decorated, Is.InstanceOf<MeasureExecutionTimeOperationDecorator>());
        }

        [Test]
        public void Should_resolve_decorator_for_named_service()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, AnotherOperation>(named: "Another");
            container.Register<IOperation, RetryOperationDecorator>(setup: DecoratorSetup.With());

            var decorator = (RetryOperationDecorator)container.Resolve<IOperation>("Another");

            Assert.That(decorator.Decorated, Is.InstanceOf<AnotherOperation>());
        }

        [Test]
        public void Should_NOT_cache_decorator_so_it_could_decorated_another_service()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>(named: "Some");
            container.Register<IOperation, AnotherOperation>(named: "Another");
            container.Register<IOperation, RetryOperationDecorator>(setup: DecoratorSetup.With());

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
            container.Register(typeof(IOperation<>), typeof(MeasureExecutionTimeOperationDecorator<>), setup: DecoratorSetup.With());

            var decorator = container.Resolve<IOperation<string>>();

            Assert.That(decorator, Is.InstanceOf<MeasureExecutionTimeOperationDecorator<string>>());
        }

        [Test]
        public void Should_resolve_closed_service_with_open_generic_decorator()
        {
            var container = new Container();
            container.Register<IOperation<int>, SomeOperation<int>>();
            container.Register(typeof(IOperation<>), typeof(MeasureExecutionTimeOperationDecorator<>), setup: DecoratorSetup.With());

            var operation = container.Resolve<IOperation<int>>();

            Assert.That(operation, Is.InstanceOf<MeasureExecutionTimeOperationDecorator<int>>());
        }

        [Test]
        public void Should_resolve_generic_decorator_of_decorator()
        {
            var container = new Container();
            container.Register(typeof(IOperation<>), typeof(SomeOperation<>));
            container.Register(typeof(IOperation<>), typeof(MeasureExecutionTimeOperationDecorator<>), setup: DecoratorSetup.With());
            container.Register(typeof(IOperation<>), typeof(RetryOperationDecorator<>), setup: DecoratorSetup.With());

            var decorator = (RetryOperationDecorator<int>)container.Resolve<IOperation<int>>();

            Assert.That(decorator.Decorated, Is.InstanceOf<MeasureExecutionTimeOperationDecorator<int>>());
        }

        [Test]
        public void Should_resolve_generic_decorator_of_closed_decorator_of_generic_service()
        {
            var container = new Container();
            container.Register(typeof(IOperation<>), typeof(SomeOperation<>));
            container.Register(typeof(IOperation<int>), typeof(MeasureExecutionTimeOperationDecorator<int>), setup: DecoratorSetup.With());
            container.Register(typeof(IOperation<>), typeof(RetryOperationDecorator<>), setup: DecoratorSetup.With());

            var decorator = (RetryOperationDecorator<int>)container.Resolve<IOperation<int>>();

            Assert.That(decorator.Decorated, Is.InstanceOf<MeasureExecutionTimeOperationDecorator<int>>());
        }

        [Test]
        public void Resolve_could_NOT_select_closed_over_generic_decorator_cause_their_are_not_related()
        {
            var container = new Container();
            container.Register(typeof(IOperation<>), typeof(SomeOperation<>));
            container.Register(typeof(IOperation<>), typeof(MeasureExecutionTimeOperationDecorator<>), setup: DecoratorSetup.With());
            container.Register(typeof(IOperation<int>), typeof(MeasureExecutionTimeOperationDecorator<int>), setup: DecoratorSetup.With());

            var decorator = (MeasureExecutionTimeOperationDecorator<int>)container.Resolve<IOperation<int>>();

            Assert.That(decorator.Decorated, Is.InstanceOf<MeasureExecutionTimeOperationDecorator<int>>());
        }

        [Test]
        public void Should_resolve_decorator_array()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, AnotherOperation>();
            container.Register<IOperation, RetryOperationDecorator>(setup: DecoratorSetup.With());

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
            container.Register<IOperation, RetryOperationDecorator>(setup: DecoratorSetup.With());

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
            container.Register<IOperation, AnotherOperation>(setup: DecoratorSetup.With());

            var operation = container.Resolve<IOperation>();
            //var operationExpr = container.Resolve<Container.DebugExpression<IOperation>>();

            Assert.That(operation, Is.InstanceOf<AnotherOperation>());
        }

        [Test]
        public void Should_support_decorator_of_decorator_without_decorated_service_argument_in_constructor()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, AnotherOperation>(setup: DecoratorSetup.With());
            container.Register<IOperation, RetryOperationDecorator>(setup: DecoratorSetup.With());

            var operation = container.Resolve<IOperation>();
            //var operationExpr = container.Resolve<DebugExpression<IOperation>>();

            Assert.That(operation, Is.InstanceOf<RetryOperationDecorator>());
            Assert.That(((RetryOperationDecorator)operation).Decorated, Is.InstanceOf<AnotherOperation>());
        }

        [Test]
        public void Should_support_decorating_of_Lazy_service()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, LazyDecorator>(setup: DecoratorSetup.With());

            var operation = container.Resolve<IOperation>();

            Assert.That(operation, Is.InstanceOf<LazyDecorator>());
        }

        [Test]
        public void Should_support_decorating_of_Lazy_named_service()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>(named: "some");
            container.Register<IOperation, LazyDecorator>(setup: DecoratorSetup.With());

            var operation = container.Resolve<IOperation>("some");

            Assert.That(operation, Is.InstanceOf<LazyDecorator>());
        }

        [Test]
        public void Should_apply_decorator_When_resolving_Func_of_decorated_service()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, MeasureExecutionTimeOperationDecorator>(setup: DecoratorSetup.With());

            var operation = container.Resolve<Func<IOperation>>();

            Assert.That(operation(), Is.InstanceOf<MeasureExecutionTimeOperationDecorator>());
        }

        [Test]
        public void Should_propagate_metadata_to_Meta_wrapper()
        {
            var container = new Container();
            container.Register(typeof(IOperation<>), typeof(SomeOperation<>), setup: ServiceSetup.WithMetadata("blah"));
            container.Register(typeof(IOperation<>), typeof(MeasureExecutionTimeOperationDecorator<>), setup: DecoratorSetup.Default);
            container.RegisterAll(typeof(OperationUser<>));

            var user = container.Resolve<OperationUser<object>>();

            Assert.That(user.GetOperation.Metadata, Is.EqualTo("blah"));
            Assert.That(user.GetOperation.Value(), Is.InstanceOf<MeasureExecutionTimeOperationDecorator<object>>());
        }

        [Test]
        public void Possible_to_register_decorator_as_delegate_of_decorated_service()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.RegisterInstance<Func<IOperation, IOperation>>(
                op => new MeasureExecutionTimeOperationDecorator(op), 
                DecoratorSetup.Default);

            var operation = container.Resolve<IOperation>();

            Assert.That(operation, Is.InstanceOf<MeasureExecutionTimeOperationDecorator>());
        }

        [Test]
        public void Possible_to_register_decorator_as_delegate_of_decorated_service_with_additional_dependencies_resolved_from_Container()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IMeasurer, Measurer>();
            container.RegisterDelegate<Func<IOperation, IOperation>>(
                r => decorated => MeasureExecutionTimeOperationDecorator.MeasureWith(decorated, r.Resolve<IMeasurer>()),
                setup: DecoratorSetup.Default);

            var operation = container.Resolve<IOperation>();

            Assert.That(operation, Is.InstanceOf<MeasureExecutionTimeOperationDecorator>());
        }

        [Test]
        public void Should_support_decorator_of_service_registered_with_delegate()
        {
            var container = new Container();
            container.RegisterDelegate<IOperation>(_ => new SomeOperation());
            container.Register<IOperation, MeasureExecutionTimeOperationDecorator>(setup: DecoratorSetup.With());

            var operation = container.Resolve<IOperation>();

            Assert.That(operation, Is.InstanceOf<MeasureExecutionTimeOperationDecorator>());
        }

        [Test]
        public void Should_support_decorator_of_decorator_registered_with_delegates()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.RegisterInstance<Func<IOperation, IOperation>>(op => new RetryOperationDecorator(op), DecoratorSetup.With());
            container.RegisterInstance<Func<IOperation, IOperation>>(op => new MeasureExecutionTimeOperationDecorator(op), DecoratorSetup.With());

            var operation = container.Resolve<IOperation>();

            Assert.That(operation, Is.InstanceOf<MeasureExecutionTimeOperationDecorator>());
            Assert.That(((MeasureExecutionTimeOperationDecorator)operation).Decorated, Is.InstanceOf<RetryOperationDecorator>());
        }

        [Test]
        public void When_registering_one_decorator_as_delegate_and_another_as_service_Then_registered_with_delegate_takes_precedence()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, RetryOperationDecorator>(setup: DecoratorSetup.With());
            container.RegisterInstance<Func<IOperation, IOperation>>(
                op => new MeasureExecutionTimeOperationDecorator(op), DecoratorSetup.With());

            var operation = container.Resolve<IOperation>();

            Assert.That(operation, Is.InstanceOf<RetryOperationDecorator>());
            Assert.That(((RetryOperationDecorator)operation).Decorated, Is.InstanceOf<MeasureExecutionTimeOperationDecorator>());
        }

        [Test]
        public void Should_support_resolving_Func_with_parameters_of_decorated_service()
        {
            var container = new Container();
            container.Register<IOperation, ParameterizedOperation>();
            container.Register<IOperation, RetryOperationDecorator>(setup: DecoratorSetup.Default);

            var operation = container.Resolve<Func<object, IOperation>>();

            Assert.That(operation("blah"), Is.InstanceOf<RetryOperationDecorator>());
        }

        [Test]
        public void Should_support_resolving_Func_with_parameters_without_decorated_service_argument_in_constructor()
        {
            var container = new Container();
            container.Register<IOperation, ParameterizedOperation>();
            container.Register<IOperation, AnotherOperation>(setup: DecoratorSetup.Default);

            var operation = container.Resolve<Func<object, IOperation>>();

            Assert.That(operation("blah"), Is.InstanceOf<AnotherOperation>());
        }

        [Test]
        public void Should_allow_Register_and_Resolve_of_two_decorators_of_the_same_type()
        {
            var container = new Container();
            container.RegisterDelegate<IOperation>(_ => new SomeOperation());
            container.Register<IOperation, MeasureExecutionTimeOperationDecorator>(setup: DecoratorSetup.Default);
            container.Register<IOperation, MeasureExecutionTimeOperationDecorator>(setup: DecoratorSetup.Default);

            var operation = container.Resolve<IOperation>();

            Assert.That(operation, Is.InstanceOf<MeasureExecutionTimeOperationDecorator>());
            Assert.That(((MeasureExecutionTimeOperationDecorator)operation).Decorated, Is.InstanceOf<MeasureExecutionTimeOperationDecorator>());
        }

        [Test]
        public void Should_support_multiple_decorator_in_object_graph()
        {
            var container = new Container();
            container.Register(typeof(IOperation<>), typeof(SomeOperation<>), setup: ServiceSetup.WithMetadata("some"));
            container.Register(typeof(IOperation<>), typeof(RetryOperationDecorator<>), setup: DecoratorSetup.Default);

            container.Register<IOperationUser<int>, OperationUser<int>>();
            container.Register(typeof(IOperationUser<>), typeof(LogUserOps<>), setup: DecoratorSetup.Default);

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
            container.Register<IOperation, FuncWithArgDecorator>(setup: DecoratorSetup.With());

            var func = container.Resolve<Func<object, IOperation>>();
            var operation = func("hey");
            Assert.That(operation, Is.InstanceOf<FuncWithArgDecorator>());

            var decoratedFunc = ((FuncWithArgDecorator)operation).DecoratedFunc("hey");
            Assert.That(decoratedFunc, Is.InstanceOf<ParameterizedOperation>());
        }
    }

    #region CUT

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
