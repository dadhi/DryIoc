using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using NSubstitute;
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

        [Test]
        public void Can_resolve_from_dependency_resolver()
        {
            var config = new HttpConfiguration();
            var container = new Container().WithWebApi(config);

            container.Register<Blah>(Reuse.Singleton);
            container.Register<Fooh>(serviceKey: 1);
            container.Register<Fooh>(serviceKey: 2);

            var resolver = config.DependencyResolver;

            var blah = resolver.GetService(typeof(Blah));
            Assert.NotNull(blah);
            Assert.AreSame(blah, resolver.GetService(typeof(Blah)));

            var foohs = resolver.GetServices(typeof(Fooh)).ToArray();
            Assert.AreEqual(2, foohs.Length);

            resolver.Dispose();
        }

        [Test]
        public void Can_begin_scope_and_resolved_scoped_service()
        {
            var config = new HttpConfiguration();
            var container = new Container().WithWebApi(config);
            
            container.Register<Blah>(ReuseInWeb.Request);
            container.Register<Fooh>(ReuseInWeb.Request, serviceKey: 1);
            container.Register<Fooh>(ReuseInWeb.Request, serviceKey: 2);

            using (var scope = config.DependencyResolver.BeginScope())
            {
                var blah = scope.GetService(typeof(Blah));
                Assert.NotNull(blah);
                Assert.AreSame(blah, scope.GetService(typeof(Blah)));

                var foohs = scope.GetServices(typeof(Fooh)).ToArray();
                Assert.AreEqual(2, foohs.Length);
            }
        }

        [Test]
        public void Can_resolve_filter_provider()
        {
            var config = new HttpConfiguration();
            var container = new Container().WithWebApi(config);
            var filterProvider = container.Resolve<IFilterProvider>();
            Assert.IsInstanceOf<DryIocAggregatedFilterProvider>(filterProvider);

            var descriptor = Substitute.For<HttpActionDescriptor>();
            descriptor.GetFilters().ReturnsForAnyArgs(c => new Collection<IFilter>());

            var controllerDescriptor = Substitute.For<HttpControllerDescriptor>();
            controllerDescriptor.GetFilters().ReturnsForAnyArgs(c => new Collection<IFilter>());
            descriptor.ControllerDescriptor = controllerDescriptor;

            filterProvider.GetFilters(config, descriptor);
        }

        public class Blah {}
        public class Fooh {}
    }
}
