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

            Startup.RegisterServices(container);

            // Fake some services
            container.RegisterMany<GetNewsStub>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            var testContainer = container.WithWebApi(new HttpConfiguration(),
                new[] { typeof(NewsController).Assembly },
                throwIfUnresolved: t => t.IsController());

            using (var scope = testContainer.OpenScope(Reuse.WebRequestScopeName))
            {
                var news = scope.Resolve<NewsController>();
                var newsItems = news.GetNewsItems();

                Assert.AreEqual("fake1", newsItems[0]);
            }
        }

        internal class GetNewsStub : IGetNews
        {
            public string[] News()
            {
                return new[] {"fake1", "fake2"};
            }
        }
    }
}
