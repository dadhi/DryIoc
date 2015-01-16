using System.IO;
using System.Reflection;
using System.Web;
using NUnit.Framework;

namespace DryIoc.Web.UnitTests
{
    [TestFixture]
    public class DryIocWebTests
    {
        [Test]
        public void Can_init_module_initializer_without_errors()
        {
            DryIocHttpModuleInitializer.Initialize();
        }

        [Test]
        public void Can_http_module_without_errors()
        {
            IHttpModule module = new DryIocHttpModule();
            module.Init(new HttpApplication());
        }

        [Test]
        public void Should_put_new_scope_into_context_on_begin_request()
        {
            var module = new DryIocHttpModule();
            var app = new FakeHttpApplication(module);
            app.RaiseBeginRequest();
            app.RaiseEndRequest();
        }
    }

    public class FakeHttpApplication : HttpApplication
    {
        public FakeHttpApplication(IHttpModule httpModule)
        {
            httpModule.ThrowIfNull().Init(this);
            _httpModule = httpModule;
        }

        public void RaiseBeginRequest()
        {
            var httpRequest = new HttpRequest(default(string), "http://ignored.net", default(string));
            var httpResponse = new HttpResponse(default(TextWriter));
            HttpContext.Current = new HttpContext(httpRequest, httpResponse);

            Events[_eventBeginRequest].ThrowIfNull().DynamicInvoke(this, null);
        }

        public void RaiseEndRequest()
        {
            Events[_eventEndRequestKey].ThrowIfNull().DynamicInvoke(this, null);
            HttpContext.Current = null;
        }

        public override void Dispose()
        {
            _httpModule.Dispose();
            base.Dispose();
        }

        private static readonly object 
            _eventBeginRequest = typeof(HttpApplication).GetField("EventBeginRequest", BindingFlags.NonPublic | BindingFlags.Static).ThrowIfNull().GetValue(null),
            _eventEndRequestKey = typeof(HttpApplication).GetField("EventEndRequest", BindingFlags.NonPublic | BindingFlags.Static).ThrowIfNull().GetValue(null);

        private readonly IHttpModule _httpModule;
    }
}
