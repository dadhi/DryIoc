using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Testing;
using NUnit.Framework;
using Owin;

namespace DryIoc.Owin.UnitTests
{
    [TestFixture]
    public class TestLazyInjectionInOwinMiddleware
    {
        static Container container;
        public class Startup
        {
            public void Configuration(IAppBuilder app)
            {
                app.UseDryIocOwinMiddleware(container);
            }
        }

        [SetUp]
        public void Init()
        {
            var scopeContext = new AsyncExecutionFlowScopeContext();
            container = new Container(scopeContext: scopeContext);
            container.Register<MyService>();
            container.Register<MyOwinMiddleware>();
        }

        [Test]
        public async void TestLazyWithOwin()
        {
            using (var server = TestServer.Create<Startup>())
            {
                System.Net.Http.HttpResponseMessage response = await server.HttpClient.GetAsync("dummy");
                Assert.IsTrue(MyOwinMiddleware.IsCalled);
            }
        }

        public class MyService
        {
        }

        public class MyOwinMiddleware : OwinMiddleware
        {
            public static bool IsCalled { get; private set; }

            public MyOwinMiddleware(OwinMiddleware next, Lazy<MyService> service)
                : base(next)
            {
            }
            public override Task Invoke(IOwinContext context)
            {
                IsCalled = true;
                return Next.Invoke(context);
            }
        }
    }
}
