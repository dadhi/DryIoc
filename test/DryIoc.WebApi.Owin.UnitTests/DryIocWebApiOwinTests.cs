namespace DryIoc.WebApi.Owin.UnitTests
{
    using System.Linq;
    using System.Web.Http;
    using Microsoft.Owin.Builder;
    using NUnit.Framework;
    using Owin;

    [TestFixture]
    public class DryIocWebApiOwinTests : ITest
    {
        public int Run()
        {
            Ensure_that_we_added_delegating_handler();
            Ensure_that_we_added_delegating_handler_once();
            return 2;
        }

        [Test]
        public void Ensure_that_we_added_delegating_handler()
        {
            var app = new AppBuilder();
            var config = new HttpConfiguration();

            app.UseDryIocWebApi(config);

            Assert.AreEqual(1, config.MessageHandlers.OfType<SetRequestDependencyScopeHandler>().Count());
        }

        [Test]
        public void Ensure_that_we_added_delegating_handler_once()
        {
            var app = new AppBuilder();
            var config = new HttpConfiguration();

            app.UseDryIocWebApi(config);
            app.UseDryIocWebApi(config);

            Assert.AreEqual(1, config.MessageHandlers.OfType<SetRequestDependencyScopeHandler>().Count());
        }
    }
}
