using System;
using System.Web.Http;
using Microsoft.Owin.Hosting;

namespace DryIoc.WebApi.Owin.Sample
{
    public class Program
    {
        public static void Main()
        {
            var url = "http://localhost:8065";

            using (WebApp.Start<Startup>(url))
            {
                Console.WriteLine("Owin host is started at {0} \npress any key to exit", url);
                Console.ReadKey();
            }
        }
    }

    public interface ISession
    {
        string Token { get; }
    }

    public class Session : ISession
    {
        string _token;

        public Session()
        {
            Console.WriteLine("Session()");
        }

        public string Token
        {
            get { return _token ?? (_token = Guid.NewGuid().ToString()); }
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