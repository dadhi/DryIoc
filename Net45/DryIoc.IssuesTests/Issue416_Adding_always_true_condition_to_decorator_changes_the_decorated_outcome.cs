using System;
using System.Reflection;
using System.Threading.Tasks;
using MediatR;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue416_Adding_always_true_condition_to_decorator_changes_the_decorated_outcome
    {
        [Test]
        public void Main()
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

            c.RegisterDelegate<SingleInstanceFactory>(r => serviceType => r.Resolve(serviceType));
            c.RegisterDelegate<MultiInstanceFactory>(r => serviceType => r.ResolveMany(serviceType));

            c.Register<IActionHandler, SomeActionHandler>(serviceKey: "key1");
            c.Register<IActionHandler, SomeActionHandler2>(serviceKey: "key2");

            // specifying DecoratorWith does not work (DbContext isn't injected in the resolution scope of IAsyncNotificationHandler 
            c.Register(typeof(IAsyncRequestHandler<,>), typeof(Decorator<,>),
                       made: Parameters.Of.Type<IActionHandler>(serviceKey: "key1"),
                       setup: Setup.DecoratorWith(r => true));

            c.Register(typeof(IAsyncRequestHandler<,>), typeof(Decorator<,>),
                       made: Parameters.Of.Type<IActionHandler>(serviceKey: "key2"),
                       setup: Setup.DecoratorWith(r => true));

            // using simply setup: DryIoc.Setup.Decorator works
            //c.Register(typeof(IAsyncRequestHandler<,>), typeof(Decorator<,>),
            //           made: Parameters.Of.Type<IActionHandler>(serviceKey: "key1"),
            //           setup: Setup.Decorator);

            //c.Register(typeof(IAsyncRequestHandler<,>), typeof(Decorator<,>),
            //           made: Parameters.Of.Type<IActionHandler>(serviceKey: "key2"),
            //           setup: Setup.Decorator);

            c.Register<Command1>();
            c.Register<CommandFactory>();

            c.Register<DbContext, Model1>(Reuse.InResolutionScopeOf(typeof(IAsyncRequestHandler<,>)));
            c.Register<DbContext, Model1>(Reuse.InResolutionScopeOf(typeof(IAsyncNotificationHandler<>)));

            var mediator = c.Resolve<IMediator>();
            var x = mediator.SendAsync(new RequestCommand()).Result;

            Assert.AreEqual("success", x);
        }

        public class DbContext
        {
        }

        public class Model1 : DbContext { }

        public class RequestCommand : IAsyncRequest<string>
        {
        }

        public class Notification : IAsyncNotification
        {
        }

        public class SomeRequestHandler : IAsyncRequestHandler<RequestCommand, string>
        {
            public ICommandFactory Factory { get; private set; }

            public IMediator Mediator { get; private set; }

            public SomeRequestHandler(IMediator mediator, ICommandFactory factory)
            {
                Mediator = mediator;
                Factory = factory;
            }

            public async Task<string> Handle(RequestCommand message)
            {
                await Mediator.PublishAsync(new Notification());
                return "success";
            }
        }

        public class SomeNotificationHandler : IAsyncNotificationHandler<Notification>
        {
            public ICommandFactory Factory { get; private set; }

            public SomeNotificationHandler(ICommandFactory factory)
            {
                Factory = factory;
            }

            public Task Handle(Notification notification)
            {
                Console.WriteLine("notification called");
                return Task.FromResult(0);
            }
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

        public class Decorator<TRequest, TResponse> : IAsyncRequestHandler<TRequest, TResponse>
            where TRequest : IAsyncRequest<TResponse>
        {
            IAsyncRequestHandler<TRequest, TResponse> _decorated;

            public readonly IActionHandler ActionHandler;

            public Decorator(IActionHandler handler, IAsyncRequestHandler<TRequest, TResponse> inner)
            {
                ActionHandler = handler;
                _decorated = inner;
            }

            public Task<TResponse> Handle(TRequest req)
            {
                return _decorated.Handle(req);
            }
        }

        public interface IActionHandler
        {
            DbContext DbContext { get; }
        }

        public class SomeActionHandler : IActionHandler
        {
            public DbContext DbContext { get; private set; }

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
    }
}
