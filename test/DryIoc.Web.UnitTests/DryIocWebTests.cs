using System.Collections.Generic;
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
        public void Can_create_container_with_HttpContext_reuse_context()
        {
            var original = new Container();

            var fakeItems = new Dictionary<object, object>();
            var container = original.WithHttpContextScopeContext(() => fakeItems);
            
            container.Register<Me>(Reuse.InWebRequest);
            using (var c = container.OpenScope(Reuse.WebRequestScopeName))
            {
                var me = container.Resolve<Me>();
                Assert.AreSame(me, container.Resolve<Me>());

                using (c.OpenScope())
                    Assert.AreSame(me, container.Resolve<Me>());
            }

            var ex = Assert.Throws<ContainerException>(() => container.Resolve<Me>());
            Assert.AreEqual(
                Error.NameOf(Error.NoCurrentScope),
                Error.NameOf(ex.Error));
        }

        internal class Me {}

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
            var app = new TestHttpApplication(module);
            app.RaiseBeginRequest();
            app.RaiseEndRequest();
        }

        [Test]
        public void Disposing_module_does_nothing_and_does_not_throw()
        {
            var module = new DryIocHttpModule();
            var app = new TestHttpApplication(module);
            app.RaiseBeginRequest();
            app.RaiseEndRequest();
            app.Dispose();
        }
    }

    internal class TestHttpApplication : HttpApplication
    {
        public TestHttpApplication(IHttpModule httpModule)
        {
            _httpModule = httpModule.ThrowIfNull();
            _httpModule.Init(this);
        }

        public void RaiseBeginRequest()
        {
            var httpRequest = new HttpRequest(default(string), "http://ignored.net", default(string));
            var httpResponse = new HttpResponse(default(TextWriter));
            
            HttpContext.Current = new HttpContext(httpRequest, httpResponse);
            _appContextInfo.SetValue(this, HttpContext.Current);

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

        private static readonly FieldInfo 
            _appContextInfo = typeof(HttpApplication).GetField("_context", BindingFlags.Instance | BindingFlags.NonPublic).ThrowIfNull();

        private readonly IHttpModule _httpModule;
    }
}
