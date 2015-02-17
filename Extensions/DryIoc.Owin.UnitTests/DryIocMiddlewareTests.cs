using System.Threading.Tasks;
using Owin;
using Microsoft.Owin;
using Microsoft.Owin.Testing;
using NUnit.Framework;

namespace DryIoc.Owin.UnitTests
{
    [TestFixture]
    public class DryIocMiddlewareTests
    {
        [Test]
        public async void Scoped_container_is_used_in_pipeline()
        {
            var container = new Container();

            using (var server = TestServer.Create(app =>
            {
                app.UseDryIocMiddleware(container);
                app.Run(context => context.Response.WriteAsync("Hey, DryIoc!"));
            }))
            {
                var response = await server.HttpClient.GetAsync("/");
                StringAssert.Contains("Hey, DryIoc!", response.Content.ReadAsStringAsync().Result);
            }
        }

        [Test]
        public async void Registered_test_middleware_is_used_in_pipeline()
        {
            var container = new Container();

            container.Register<TestMiddleware>();
            container.RegisterInstance(new Greeting { Message = "Hey, DryIoc!" });

            using (var server = TestServer.Create(app => app.UseDryIocMiddleware(container)))
            {
                var response = await server.HttpClient.GetAsync("/");
                StringAssert.Contains("Hey, DryIoc!", response.Content.ReadAsStringAsync().Result);
            }
        }

        [Test]
        public async void Should_ignore_unresolved_middleware_due_missing_dependency()
        {
            var container = new Container();
            container.Register<TestMiddleware>();
            // Greeting dependency does not registered

            using (var server = TestServer.Create(app => app.UseDryIocMiddleware(container)))
            {
                var response = await server.HttpClient.GetAsync("/");
                Assert.IsEmpty(response.Content.ReadAsStringAsync().Result);
            }
        }

        [Test]
        public async void Can_register_to_context_request()
        {
            var container = new Container();

            container.Register<TestMiddleware>();

            using (var server = TestServer.Create(app => app.UseDryIocMiddleware(container,
                r => r.RegisterInstance(new Greeting { Message = "Hey, DryIoc!" }, WebReuse.InRequest, IfAlreadyRegistered.Replace))))
            {
                var response = await server.HttpClient.GetAsync("/");
                StringAssert.Contains("Hey, DryIoc!", response.Content.ReadAsStringAsync().Result);
            }
        }

        internal class TestMiddleware : OwinMiddleware
        {
            public Greeting Greeting { get; private set; }

            public TestMiddleware(OwinMiddleware next, Greeting greeting) : base(next)
            {
                Greeting = greeting;
            }

            public override Task Invoke(IOwinContext context)
            {
                context.Response.WriteAsync(Greeting.Message);
                return Next.Invoke(context);
            }
        }

        internal class Greeting
        {
            public string Message;
        }
    }
}
