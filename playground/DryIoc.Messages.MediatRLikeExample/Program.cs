using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DryIoc.Messages.MediatRLikeExample
{
    [TestFixture]
    public class Program
    {
        public static Task Main() => new Program().Run();

        [Test]
        public Task Run()
        {
            var container = new Container(rules => rules.WithTrackingDisposableTransients());
            var writer = new WrappingWriter(Console.Out);

            BuildMediator(container, writer);

            return Runner.Run(container, writer, "DryIoc.Messages");
        }

        private static void BuildMediator(IRegistrator container, TextWriter writer)
        {
            container.RegisterInstance(writer);
            container.RegisterMany(new[] { typeof(Program).GetAssembly() }, Registrator.Interfaces);

            container.Register(typeof(IMessageHandler<,>), typeof(MiddlewareMessageHandler<,>), setup: Setup.Decorator);
            container.Register(typeof(BroadcastMessageHandler<>));
        }
    }

    public class Jing : IMessage
    {
        public string Message { get; set; }
    }

    public class JingHandler : AsyncMessageHandler<Jing, EmptyResponse>
    {
        private readonly TextWriter _writer;

        public JingHandler(TextWriter writer) => _writer = writer;

        protected override Task<EmptyResponse> Handle(Jing request, CancellationToken cancellationToken) => 
            _writer.WriteLineAsync($"--- Handled Jing: {request.Message}, no Jong").ToEmptyResponse();
    }

    public class Ping : IMessage<Pong>
    {
        public string Message { get; set; }
    }

    public class Pong
    {
        public string Message { get; set; }
    }

    public class Pinged : IMessage
    {
    }

    public class Ponged : IMessage
    {
    }

    public class PingedHandler : IMessageHandler<Pinged>
    {
        private readonly TextWriter _writer;

        public PingedHandler(TextWriter writer) => _writer = writer;

        public Task<EmptyResponse> Handle(Pinged notification, CancellationToken cancellationToken) => 
            _writer.WriteLineAsync("Got pinged async.").ToEmptyResponse();
    }

    public class PongedHandler : IMessageHandler<Ponged>
    {
        private readonly TextWriter _writer;

        public PongedHandler(TextWriter writer) => _writer = writer;

        public Task<EmptyResponse> Handle(Ponged notification, CancellationToken cancellationToken) => 
            _writer.WriteLineAsync("Got ponged async.").ToEmptyResponse();
    }

    public class ConstrainedPingedHandler<TNotification> : IMessageHandler<TNotification>
        where TNotification : Pinged
    {
        private readonly TextWriter _writer;
        public ConstrainedPingedHandler(TextWriter writer) => _writer = writer;

        public Task<EmptyResponse> Handle(TNotification notification, CancellationToken cancellationToken) => 
            _writer.WriteLineAsync("Got pinged constrained async.").ToEmptyResponse();
    }

    public class PingedAlsoHandler : IMessageHandler<Pinged>
    {
        private readonly TextWriter _writer;

        public PingedAlsoHandler(TextWriter writer) => 
            _writer = writer;

        public Task<EmptyResponse> Handle(Pinged notification, CancellationToken cancellationToken) => 
            _writer.WriteLineAsync("Got pinged also async.").ToEmptyResponse();
    }

    public class PingHandler : IMessageHandler<Ping, Pong>
    {
        private readonly TextWriter _writer;

        public PingHandler(TextWriter writer) => _writer = writer;

        public async Task<Pong> Handle(Ping request, CancellationToken cancellationToken)
        {
            await _writer.WriteLineAsync($"--- Handled Ping: {request.Message}");
            return new Pong { Message = request.Message + " Pong" };
        }
    }

    public class GenericHandler : IMessageHandler<IMessage>
    {
        private readonly TextWriter _writer;
        public GenericHandler(TextWriter writer) => _writer = writer;

        public Task<EmptyResponse> Handle(IMessage notification, CancellationToken cancellationToken) =>
            _writer.WriteLineAsync("Got notified.").ToEmptyResponse();
    }

    public class GenericRequestPreProcessor<TRequest, TResponse> : IMessageMiddleware<TRequest, TResponse>
        where TRequest : IMessage<TResponse>
    {
        private readonly TextWriter _writer;

        public GenericRequestPreProcessor(TextWriter writer) => _writer = writer;

        public int RelativeOrder => -1;
        public async Task<TResponse> Handle(TRequest message, CancellationToken cancellationToken, Func<Task<TResponse>> nextMiddleware)
        {
            await _writer.WriteLineAsync("- Starting Up");
            return await nextMiddleware();
        }
    }

    public class GenericPipelineBehavior<TRequest, TResponse> : IMessageMiddleware<TRequest, TResponse>
    {
        private readonly TextWriter _writer;
        public GenericPipelineBehavior(TextWriter writer) => _writer = writer;

        public int RelativeOrder => 3;

        public async Task<TResponse> Handle(TRequest message, CancellationToken cancellationToken, Func<Task<TResponse>> nextMiddleware)
        {
            await _writer.WriteLineAsync("-- Handling Request");
            var response = await nextMiddleware();
            await _writer.WriteLineAsync("-- Finished Request");
            return response;
        }
    }

    public class GenericRequestPostProcessor<TRequest, TResponse> : IMessageMiddleware<TRequest, TResponse>
    {
        private readonly TextWriter _writer;
        public GenericRequestPostProcessor(TextWriter writer) => _writer = writer;

        public int RelativeOrder => 2;

        public async Task<TResponse> Handle(TRequest message, CancellationToken cancellationToken, Func<Task<TResponse>> nextMiddleware)
        {
            var result = await nextMiddleware();
            await _writer.WriteLineAsync("- All Done");
            return result;
        }
    }

    public class ConstrainedRequestPostProcessor<TRequest, TResponse> : IMessageMiddleware<TRequest, TResponse>
        where TRequest : Ping
    {
        private readonly TextWriter _writer;
        public ConstrainedRequestPostProcessor(TextWriter writer) => _writer = writer;

        public int RelativeOrder => 1;

        public async Task<TResponse> Handle(TRequest message, CancellationToken cancellationToken, Func<Task<TResponse>> nextMiddleware)
        {
            var result = await nextMiddleware();
            await _writer.WriteLineAsync("- All Done with Ping");
            return result;
        }
    }

    public static class Runner
    {
        public static async Task Run(IResolver resolver, WrappingWriter writer, string projectName)
        {
            await writer.WriteLineAsync("===============");
            await writer.WriteLineAsync(projectName);
            await writer.WriteLineAsync("===============");

            var ct = new CancellationToken();

            await writer.WriteLineAsync("Sending Ping...");
            var pingPong = resolver.Resolve<IMessageHandler<Ping, Pong>>();
            var pong = await pingPong.Handle(new Ping { Message = "Ping" }, ct);
            await writer.WriteLineAsync("Received: " + pong.Message);

            await writer.WriteLineAsync("Publishing Pinged...");
            var pinged = resolver.Resolve<BroadcastMessageHandler<Pinged>>();
            await pinged.Handle(new Pinged(), ct);

            await writer.WriteLineAsync("Publishing Ponged...");
            var failedPong = false;
            try
            {
                var ponged = resolver.Resolve<BroadcastMessageHandler<Ponged>>();
                await ponged.Handle(new Ponged(), ct);
            }
            catch (Exception e)
            {
                failedPong = true;
                await writer.WriteLineAsync(e.ToString());
            }

            bool failedJing = false;
            await writer.WriteLineAsync("Sending Jing...");
            try
            {
                var jing = resolver.Resolve<IMessageHandler<Jing, EmptyResponse>>();
                await jing.Handle(new Jing { Message = "Jing" }, ct);
            }
            catch (Exception e)
            {
                failedJing = true;
                await writer.WriteLineAsync(e.ToString());
            }

            await writer.WriteLineAsync("---------------");
            var contents = writer.Contents;
            var order = new[] {
                contents.IndexOf("- Starting Up", StringComparison.OrdinalIgnoreCase),
                contents.IndexOf("-- Handling Request", StringComparison.OrdinalIgnoreCase),
                contents.IndexOf("--- Handled Ping", StringComparison.OrdinalIgnoreCase),
                contents.IndexOf("-- Finished Request", StringComparison.OrdinalIgnoreCase),
                contents.IndexOf("- All Done", StringComparison.OrdinalIgnoreCase),
                contents.IndexOf("- All Done with Ping", StringComparison.OrdinalIgnoreCase),
            };

            var results = new RunResults
            {
                RequestHandlers = contents.Contains("--- Handled Ping:"),
                VoidRequestsHandlers = contents.Contains("--- Handled Jing:"),
                PipelineBehaviors = contents.Contains("-- Handling Request"),
                RequestPreProcessors = contents.Contains("- Starting Up"),
                RequestPostProcessors = contents.Contains("- All Done"),
                ConstrainedGenericBehaviors = contents.Contains("- All Done with Ping") && !failedJing,
                OrderedPipelineBehaviors = order.SequenceEqual(order.OrderBy(i => i)),
                NotificationHandler = contents.Contains("Got pinged async"),
                MultipleNotificationHandlers = contents.Contains("Got pinged async") && contents.Contains("Got pinged also async"),
                ConstrainedGenericNotificationHandler = contents.Contains("Got pinged constrained async") && !failedPong,
                CovariantNotificationHandler = contents.Contains("Got notified")
            };

            await writer.WriteLineAsync($"Request Handler...................{(results.RequestHandlers ? "Y" : "N")}");
            await writer.WriteLineAsync($"Void Request Handler..............{(results.VoidRequestsHandlers ? "Y" : "N")}");
            await writer.WriteLineAsync($"Pipeline Behavior.................{(results.PipelineBehaviors ? "Y" : "N")}");
            await writer.WriteLineAsync($"Pre-Processor.....................{(results.RequestPreProcessors ? "Y" : "N")}");
            await writer.WriteLineAsync($"Post-Processor....................{(results.RequestPostProcessors ? "Y" : "N")}");
            await writer.WriteLineAsync($"Constrained Post-Processor........{(results.ConstrainedGenericBehaviors ? "Y" : "N")}");
            await writer.WriteLineAsync($"Ordered Behaviors.................{(results.OrderedPipelineBehaviors ? "Y" : "N")}");
            await writer.WriteLineAsync($"Notification Handler..............{(results.NotificationHandler ? "Y" : "N")}");
            await writer.WriteLineAsync($"Notification Handlers.............{(results.MultipleNotificationHandlers ? "Y" : "N")}");
            await writer.WriteLineAsync($"Constrained Notification Handler..{(results.ConstrainedGenericNotificationHandler ? "Y" : "N")}");
            await writer.WriteLineAsync($"Covariant Notification Handler....{(results.CovariantNotificationHandler ? "Y" : "N")}");
        }
    }

    public class RunResults
    {
        public bool RequestHandlers { get; set; }
        public bool VoidRequestsHandlers { get; set; }
        public bool PipelineBehaviors { get; set; }
        public bool RequestPreProcessors { get; set; }
        public bool RequestPostProcessors { get; set; }
        public bool OrderedPipelineBehaviors { get; set; }
        public bool ConstrainedGenericBehaviors { get; set; }
        public bool NotificationHandler { get; set; }
        public bool MultipleNotificationHandlers { get; set; }
        public bool CovariantNotificationHandler { get; set; }
        public bool ConstrainedGenericNotificationHandler { get; set; }
    }

    public class WrappingWriter : TextWriter
    {
        private readonly TextWriter _innerWriter;
        private readonly StringBuilder _stringWriter = new StringBuilder();

        public WrappingWriter(TextWriter innerWriter)
        {
            _innerWriter = innerWriter;
        }

        public override void Write(char value)
        {
            _stringWriter.Append(value);
            _innerWriter.Write(value);
        }

        public override Task WriteLineAsync(string value)
        {
            _stringWriter.AppendLine(value);
            return _innerWriter.WriteLineAsync(value);
        }

        public override Encoding Encoding => _innerWriter.Encoding;

        public string Contents => _stringWriter.ToString();
    }
}