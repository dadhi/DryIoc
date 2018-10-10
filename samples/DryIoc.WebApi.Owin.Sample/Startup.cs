using System.Net.Http;
using System.Web.Http;
using DryIoc.WebApi.Owin.Sample.Services;
using Microsoft.Owin.Diagnostics;
using Owin;

namespace DryIoc.WebApi.Owin.Sample
{
    public class Startup
    {
        public IContainer Container { get; private set; }

        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "default",
                routeTemplate: "api/{controller}");

            var container = new Container();
            RegisterServices(container);

            // Important to use container returned from call for any further operation.
            // If you won't do anything with it, then it is fine not to save returned container 
            // (it still be hold by WebApi dependency resolver)
            Container = container.WithWebApi(config, throwIfUnresolved: t => t.IsController());

            app.UseWebApi(config);
            app.UseErrorPage(ErrorPageOptions.ShowAll);
        }

        public static void RegisterServices(IRegistrator registrator)
        {
            // NOTE: Registers ISession provider to work with injected Request
            registrator.Register(Made.Of(() => GetSession(Arg.Of<HttpRequestMessage>())));

            registrator.RegisterMany<ConsoleLogger>();
            registrator.RegisterMany<NewsProvider>();
        }

        public static ISession GetSession(HttpRequestMessage request)
        {
            // TODO: This is just a sample. Insert whatever session management logic you need.
            var session = new Session();
            return session;
        }
    }
}