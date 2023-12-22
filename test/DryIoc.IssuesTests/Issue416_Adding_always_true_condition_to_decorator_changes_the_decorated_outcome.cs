using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue416_Adding_always_true_condition_to_decorator_changes_the_decorated_outcome : ITest
    {
        public int Run()
        {
            Resolve_can_resolve_decorator_as_resolution_call_together_with_resolution_scope_reuse();
            ResolveMany_can_resolve_decorator_with_dependency();
            Minimal_test();
            Test();
            return 4;
        }

        [Test]
        public void Resolve_can_resolve_decorator_as_resolution_call_together_with_resolution_scope_reuse()
        {
            var container = new Container();

            container.Register<B>(Reuse.ScopedTo<IA>());
            container.Register<IA, A>();
            container.Register<IA, D>(setup: Setup.DecoratorWith(_ => true, openResolutionScope: true));

            var a = container.Resolve(typeof(IA));

            Assert.IsInstanceOf<D>(a);
            Assert.AreSame(((D)a).B, ((D)a).Bb);
        }

        [Test]
        public void ResolveMany_can_resolve_decorator_with_dependency()
        {
            var container = new Container();

            container.Register<B>(Reuse.ScopedTo<IA>());
            container.Register<IA, A>();
            container.Register<IA, D>(setup: Setup.DecoratorWith(_ => true, openResolutionScope: true));

            var a = container.ResolveMany(typeof(IA)).First();

            Assert.IsInstanceOf<D>(a);
            Assert.AreSame(((D)a).B, ((D)a).Bb);
        }

        public class B {}

        public interface IA
        {
            B B { get; }
        }

        public class A : IA
        {
            public B B { get; private set; }

            public A(B b)
            {
                B = b;
            }
        }

        public class D : IA
        {
            public IA A { get; private set; }

            public B B { get { return A.B; } }
            public B Bb { get; private set; }

            public D(IA a, B b)
            {
                A = a;
                Bb = b;
            }
        }

        [Test]
        public void Minimal_test()
        {
            var container = new Container();

            container.Register<Aa>(setup: Setup.With(openResolutionScope: true));
            container.Register<Bb>(setup: Setup.With(openResolutionScope: true));
            container.Register<Dd>();
            container.Register<IXx, Xx>(Reuse.ScopedTo<Aa>());
            container.Register<IXx, Yy>(Reuse.ScopedTo<Bb>());

            var a = container.Resolve<Aa>();
            Assert.IsInstanceOf<Xx>(a.D.Xx);

            var b = container.Resolve<Bb>();
            Assert.IsInstanceOf<Yy>(b.D.Xx);
        }

        public class Aa
        {
            public Dd D { get; private set; }

            public Aa(Dd d)
            {
                D = d;
            }
        }

        public class Bb
        {
            public Dd D { get; private set; }

            public Bb(Dd d)
            {
                D = d;
            }
        }

        public class Dd
        {
            public IXx Xx { get; set; }

            public Dd(IXx xx)
            {
                Xx = xx;
            }
        }

        public interface IXx { }
        public class Xx : IXx { }
        public class Yy : IXx { }

        [Test]
        public void Test()
        {
            var c = new Container(r => r
                .WithoutThrowOnRegisteringDisposableTransient()
                .With(FactoryMethod.ConstructorWithResolvableArguments));

            c.RegisterMany(new[] {
                typeof(IMediator).GetAssembly(),
                typeof(SomeRequestHandler).GetAssembly() },
                    type => type.GetTypeInfo().IsInterface
                    // exclude action handler so we can register by key
                    && !typeof(IActionHandler).IsAssignableFrom(type));

            c.RegisterDelegate<ServiceFactory>(r => r.Resolve);

            c.Register<IActionHandler, SomeActionHandler>(serviceKey: "key1");
            c.Register<IActionHandler, SomeActionHandler2>(serviceKey: "key2");
            c.Register<IActionHandler, SomeActionHandler3>(serviceKey: "key3");
            c.Register<IActionHandler, SomeActionHandler4>(serviceKey: "key4");

            c.Register(typeof(IRequestHandler<,>), typeof(Decorator<,>),
                       made: Parameters.Of.Type<IActionHandler>(serviceKey: "key1"),
                       setup: Setup.DecoratorWith(r => true, openResolutionScope: true));

            c.Register(typeof(IRequestHandler<,>), typeof(Decorator<,>),
                       made: Parameters.Of.Type<IActionHandler>(serviceKey: "key2"),
                       setup: Setup.DecoratorWith(r => true, openResolutionScope: true));

            c.Register(typeof(IRequestHandler<,>), typeof(Decorator<,>),
                       made: Parameters.Of.Type<IActionHandler>(serviceKey: "key3"),
                       setup: Setup.DecoratorWith(r => true, openResolutionScope: true));

            c.Register(typeof(IRequestHandler<,>), typeof(Decorator<,>),
                       made: Parameters.Of.Type<IActionHandler>(serviceKey: "key4"),
                       setup: Setup.DecoratorWith(r => true, openResolutionScope: true));

            c.Register<Command1>();
            c.Register<CommandFactory>();

            c.Register<DbContext, Model1>(Reuse.ScopedToService(typeof(IRequestHandler<,>)));
            c.Register<DbContext, Model1>(Reuse.ScopedToService(typeof(INotificationHandler<>)));

            var mediator = c.Resolve<IMediator>();

            var x = mediator.Send(new RequestCommand()).Result;

            Assert.AreEqual("success", x);
        }

        public class DbContext
        {
        }

        public class Model1 : DbContext { }

        public class RequestCommand : IRequest<string> { }

        public class Notification : INotification { }

        public class SomeRequestHandler : IRequestHandler<RequestCommand, string>
        {
            public ICommandFactory Factory { get; private set; }

            public IMediator Mediator { get; private set; }

            public SomeRequestHandler(IMediator mediator, ICommandFactory factory)
            {
                Mediator = mediator;
                Factory = factory;
            }

            public async Task<string> Handle(RequestCommand request, CancellationToken cancellationToken)
            {
                await Mediator.Publish(new Notification());
                return "success";
            }
        }

        public class SomeNotificationHandler : INotificationHandler<Notification>
        {
            public ICommandFactory Factory { get; }

            public SomeNotificationHandler(ICommandFactory factory)
            {
                Factory = factory;
            }

            public Task Handle(Notification notification, CancellationToken cancellationToken) => 
                Task.FromResult(0);
        }

        public class Command1
        {
            public Command1(DbContext ctx)
            {
                _ctx = ctx;
            }

            public DbContext _ctx;
        }

        public interface ICommandFactory
        {
        }

        public class CommandFactory : ICommandFactory
        {
            public CommandFactory(Command1 command)
            {
                _command = command;
            }

            public Command1 _command;
        }

        public class Decorator<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
            where TRequest : IRequest<TResponse>
        {
            IRequestHandler<TRequest, TResponse> _decorated;

            public readonly IActionHandler ActionHandler;

            public Decorator(IActionHandler handler, IRequestHandler<TRequest, TResponse> inner)
            {
                ActionHandler = handler;
                _decorated = inner;
            }

            public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken) =>
                _decorated.Handle(request, cancellationToken);
        }

        public interface IActionHandler
        {
        }

        public class SomeActionHandler : IActionHandler
        {
            public DbContext DbContext { get; }

            public SomeActionHandler(DbContext context)
            {
                DbContext = context;
            }
        }

        public class SomeActionHandler2 : IActionHandler
        {
            public DbContext DbContext { get; private set; }

            public SomeActionHandler2(DbContext context)
            {
                DbContext = context;
            }
        }

        public class SomeActionHandler3 : IActionHandler
        {
        }

        public class SomeActionHandler4 : IActionHandler
        {
        }
    }
}
