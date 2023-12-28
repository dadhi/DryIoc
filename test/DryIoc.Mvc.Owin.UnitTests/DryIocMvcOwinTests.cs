using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Testing;
using NUnit.Framework;
using DryIoc.Owin;
using DryIoc.Web;
using DryIoc.ImTools;

namespace DryIoc.Mvc.Owin.UnitTests
{
    [TestFixture]
    public class DryIocMvcOwinTests : ITest
    {
        public int Run()
        {
            Configure_container_with_MVC_and_use_OWIN_middleware().GetAwaiter().GetResult();
            Registering_greeting_with_middleware_configuration().GetAwaiter().GetResult();
            return 2;
        }

        [Test]
        public async Task Configure_container_with_MVC_and_use_OWIN_middleware()
        {
            IContainer container = new Container();
            container.Register<TestGreetingMiddleware>();
            container.Register(Made.Of(() => new Greeting { Message = "Hey, DryIoc!" }), Reuse.InWebRequest);

            var contextItems = new Dictionary<object, object>();
            HttpContextScopeContext.GetContextItems = () => contextItems;

            using (var server = TestServer.Create(app =>
            {
                container = container.WithMvc(ArrayTools.Empty<Assembly>());
                app.UseDryIocOwinMiddleware(container);
            }))
            {
                var response = await server.HttpClient.GetAsync("/");
                StringAssert.Contains("Hey, DryIoc!", response.Content.ReadAsStringAsync().Result);
            }
        }

        [Test]
        public async Task Registering_greeting_with_middleware_configuration()
        {
            IContainer container = new Container();
            container.Register<TestGreetingMiddleware>();

            var contextItems = new Dictionary<object, object>();
            HttpContextScopeContext.GetContextItems = () => contextItems;

            using (var server = TestServer.Create(app =>
            {
                app.UseDryIocOwinMiddleware(
                    container.WithMvc(ArrayTools.Empty<Assembly>()),
                    scope => scope.Use(new Greeting { Message = "Hey, DryIoc!" }));
            }))
            {
                var response = await server.HttpClient.GetAsync("/");
                StringAssert.Contains("Hey, DryIoc!", response.Content.ReadAsStringAsync().Result);
            }
        }

        internal class TestGreetingMiddleware : OwinMiddleware
        {
            public Greeting Greeting { get; private set; }

            public TestGreetingMiddleware(OwinMiddleware next, Greeting greeting)
                : base(next)
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
