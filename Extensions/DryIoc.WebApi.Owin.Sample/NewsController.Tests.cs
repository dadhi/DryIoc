using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using NUnit.Framework;

namespace DryIoc.WebApi.Owin.Sample
{
    [TestFixture]
    public class NewsControllerTest
    {
        [Test]
        public void Can_get_fake_news()
        {
            var container = new Container();

            // todo: Replace with face services
            Startup.RegisterServices(container);

            var testContainer = container.WithWebApi(new HttpConfiguration(), 
                new[] { typeof(NewsController).Assembly },
                throwIfUnresolved: t => t.IsController());

            using (var scope = testContainer.OpenScope(Reuse.WebRequestScopeName))
            {
                var news = scope.Resolve<NewsController>();
            }
        }
    }
}
