using System;
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

            container.RegisterDelegate<SingleInstanceFactory>(r =>
            {
                return serviceType => r.Resolve(serviceType);
            });

            container.RegisterDelegate<MultiInstanceFactory>(r =>
            {
                return serviceType => r.ResolveMany(serviceType);
            });

            container.Register<IMediator, Mediator>();
            container.RegisterMany(new[] { typeof(HelloRequestHandler<>), typeof(GoodMorningRequestHandler<>) });

            //container.RegisterMany(new[] { typeof(Controller).GetAssembly() },
            //    type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IAsyncRequestHandler<,>));

            container.Register<Controller>();

            var controller = container.Resolve<Controller>();

            Assert.IsNotNull(controller);

            var hello = controller.Hello("World").Result;
            var gm = controller.GoodMorning("World").Result;
        }

        public class HelloRequest<T> : IAsyncRequest<string> where T : class
        {
            public string Name { get; set; }
        }

        public class HelloRequestHandler<T> : IAsyncRequestHandler<HelloRequest<T>, string> where T : class
        {
            public async Task<string> Handle(HelloRequest<T> message)
            {
                return await Task.FromResult("Hello, " + message.Name);
            }
        }

        public class GoodMorningRequest<T> : IAsyncRequest<string> where T : class
        {
            public string Name { get; set; }
        }

        public class GoodMorningRequestHandler<T> : IAsyncRequestHandler<GoodMorningRequest<T>, string> where T : class
        {
            public async Task<string> Handle(GoodMorningRequest<T> message)
            {
                return await Task.FromResult("Good Morning, " + message.Name);
            }
        }

        public class Controller
        {
            private readonly IMediator _mediator;

            public Controller(IMediator mediator)
            {
                _mediator = mediator;
            }

            public async Task<string> Hello(string name)
            {
                return await _mediator.SendAsync(new HelloRequest<EventArgs> { Name = name });
            }

            public async Task<string> GoodMorning(string name)
            {
                return await _mediator.SendAsync(new GoodMorningRequest<Exception> { Name = name });
            }
        }
    }
}
