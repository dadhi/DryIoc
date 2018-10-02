using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue339_GenericDecoratorWithConstraints
    {
        [Test]
        public void Test()
        {
            var container = new Container();

            container.Register(typeof(IAsyncRequestHandler<,>), typeof(Decorator<,>),
                setup: Setup.Decorator);

            container.Register<IAsyncRequestHandler<GetStringRequest, string>, GetStringRequestHandler>();
            container.Register<IActionHandler, TransactionActionHandler>();

            var result = container.Resolve<IAsyncRequestHandler<GetStringRequest, string>>();

            Assert.IsInstanceOf<Decorator<GetStringRequest, string>>(result);
            Assert.IsInstanceOf<TransactionActionHandler>(((Decorator<GetStringRequest, string>)result).Handler);
            Assert.IsInstanceOf<GetStringRequestHandler>(((Decorator<GetStringRequest, string>)result).Inner);
        }

        // API

        public interface IAsyncRequest<out TResponse> { }

        public interface IAsyncRequestHandler<in TRequest, out TResponse>
            where TRequest : IAsyncRequest<TResponse> { }

        public interface IActionHandler { }


        // Samples

        public interface IBusinessLayerCommand { }

        public class GetStringRequest : IAsyncRequest<string>, IBusinessLayerCommand { }

        public class GetStringRequestHandler : IAsyncRequestHandler<GetStringRequest, string> { }

        public class TransactionActionHandler : IActionHandler { }

        public class Decorator<TRequest, TResponse> : IAsyncRequestHandler<TRequest, TResponse>
            where TRequest : IAsyncRequest<TResponse>, IBusinessLayerCommand
        {
            public Decorator(IActionHandler handler, IAsyncRequestHandler<TRequest, TResponse> inner)
            {
                Handler = handler;
                Inner = inner;
            }

            public readonly IActionHandler Handler;
            public readonly IAsyncRequestHandler<TRequest, TResponse> Inner;
        }
    }
}
