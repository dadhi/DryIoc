namespace DryIoc.WebApi.Owin.UnitTests
{
    using System.Linq;
    using System.Web.Http;
    using Microsoft.Owin.Builder;
    using NUnit.Framework;
    using Owin;

    [TestFixture]
    public class DryIocWebApiOwinTests
    {
        [Test]
        public void UseAutofacWebApiAddsDelegatingHandler()
        {
            var app = new AppBuilder();
            var config = new HttpConfiguration();

            app.UseDryIocWebApi(config);

            Assert.AreEqual(1, config.MessageHandlers.OfType<SetRequestDependencyScopeHandler>().Count());
        }

        [Test]
        public void UseAutofacWebApiWillOnlyAddDelegatingHandlerOnce()
        {
            var app = new AppBuilder();
            var config = new HttpConfiguration();

            app.UseDryIocWebApi(config);
            app.UseDryIocWebApi(config);

            Assert.AreEqual(1, config.MessageHandlers.OfType<SetRequestDependencyScopeHandler>().Count());
        }
    }
}
