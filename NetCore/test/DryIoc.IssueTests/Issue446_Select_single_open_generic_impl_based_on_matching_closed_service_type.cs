using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue446_Select_single_open_generic_impl_based_on_matching_closed_service_type
    {
        [Test]
        public void Main_test()
        {
            var container = new Container();

            container.RegisterDelegate<ServiceFactory>(r => r.Resolve);

            container.Register<IMediator, Mediator>();
            container.RegisterMany(new[] { typeof(HelloRequestHandler), typeof(GoodMorningRequestHandler) });

            container.Register<Controller>();

            var controller = container.Resolve<Controller>();

            Assert.IsNotNull(controller);

            var hello = controller.Hello("World").Result;
            var gm = controller.GoodMorning("World").Result;
        }

        public class HelloRequest : IRequest<string>
        {
            public string Name { get; set; }
        }

        public class HelloRequestHandler : IRequestHandler<HelloRequest, string>
        {
            public async  Task<string> Handle(HelloRequest request, CancellationToken cancellationToken) =>
                await Task.FromResult("Hello, " + request.Name);
        }

        public class GoodMorningRequest : IRequest<string>
        {
            public string Name { get; set; }
        }

        public class GoodMorningRequestHandler : IRequestHandler<GoodMorningRequest, string>
        {
            public async Task<string> Handle(GoodMorningRequest message)
            {
                return await Task.FromResult("Good Morning, " + message.Name);
            }

            public async Task<string> Handle(GoodMorningRequest request, CancellationToken cancellationToken) =>
                await Task.FromResult("Good Morning, " + request.Name);
        }

        public class Controller
        {
            private readonly IMediator _mediator;

            public Controller(IMediator mediator)
            {
                _mediator = mediator;
            }

            public async Task<string> Hello(string name) => 
                await _mediator.Send(new HelloRequest { Name = name });

            public async Task<string> GoodMorning(string name) => 
                await _mediator.Send(new GoodMorningRequest { Name = name });
        }
    }
}
