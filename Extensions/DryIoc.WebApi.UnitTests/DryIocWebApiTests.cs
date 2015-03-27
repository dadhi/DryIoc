using System.Web.Http;
using NUnit.Framework;

namespace DryIoc.WebApi.UnitTests
{
    [TestFixture]
    public class DryIocWebApiTests
    {
        [Test]
        public void Enable_WebApi_support_without_exceptions()
        {
            var container = new Container();

            container.WithWebApi(new HttpConfiguration());
        }
    }
}
