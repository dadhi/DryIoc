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
        public async void Check_that_scoped_container_is_used_in_pipeline()
        {
            var container = new Container();
            container.Register<OwinMiddleware, TestMiddleware>();

            using (var server = TestServer.Create(app =>
            {
                app.UseDryIocMiddleware(container);
                app.Run(context =>
                {
                    return context.Response.WriteAsync("Hey, DryIoc!");
                });
            }))
            {

                var response = await server.HttpClient.GetAsync("/");
                StringAssert.Contains("Hey, DryIoc!", response.Content.ReadAsStringAsync().Result);
            }
        }

        public class TestMiddleware : OwinMiddleware
        {
            public TestMiddleware(OwinMiddleware next) : base(next) {}

            public override Task Invoke(IOwinContext context)
            {
                context.Response.WriteAsync("Hey, DryIoc!");
                return Next.Invoke(context);
            }
        }

        internal class X { }
    }
}
