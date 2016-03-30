using System;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Owin.Diagnostics;
using Microsoft.Owin.Hosting;
using Owin;
using DryIoc.WebApi.Owin;

namespace DryIoc.WebApi.Owin.Sample
{
    public class Program
    {
        public static void Main()
        {
            var url = "http://localhost:8065";

            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine("Owin host started, any key to exit");
                Console.ReadKey();
            }
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "default",
                routeTemplate: "{controller}"
               );

            var di = new Container();

            // NOTE: Registers ISession provider to work with injected Request
            di.Register<ISession>(Made.Of(() => GetSession(Arg.Of<HttpRequestMessage>())));

            di.WithWebApi(config);

            app.UseWebApi(config);
            app.UseErrorPage(ErrorPageOptions.ShowAll);
        }

        public static ISession GetSession(HttpRequestMessage request)
        {
            // TODO: This is just a sample. Insert whatever session management logic you need.
            var session = new Session();
            return session;
        }
    }

    public interface ISession
    {
        string Token { get; }
    }

    public class Session : ISession
    {
        string m_token;

        public Session()
        {
            Console.WriteLine("Session()");
        }

        public string Token
        {
            get { return m_token ?? (m_token = Guid.NewGuid().ToString()); }
        }
    }

    [RoutePrefix("api")]
    public class RootController : ApiController
    {
        readonly ISession m_session;

        public RootController(ISession session_)
        {
            m_session = session_;
        }

        [Route()]
        public IHttpActionResult GetApiRoot()
        {
            return Json(
                new
                {
                    type = "root",
                    token = m_session.Token
                });
        }
    }
}