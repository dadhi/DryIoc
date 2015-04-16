using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Hosting;
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

            container.Register<Blah>(DryIoc.Reuse.Singleton);
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
            
            container.Register<Blah>(Reuse.InRequest);
            container.Register<Fooh>(Reuse.InRequest, serviceKey: 1);
            container.Register<Fooh>(Reuse.InRequest, serviceKey: 2);

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
        public void Can_begin_scope_and_resolve_controller()
        {
            var config = new HttpConfiguration();
            new Container().WithWebApi(config, new[] { typeof(MyController).Assembly });

            using (var scope = config.DependencyResolver.BeginScope())
            {
                var controller = scope.GetService(typeof(MyController));
                Assert.NotNull(controller);
                Assert.AreSame(controller, scope.GetService(typeof(MyController)));
            }
        }

        public class MyController : ApiController
        {
        }

        [Test]
        public void Can_begin_scope_and_resolve_any_service_as_fallback_rule()
        {
            var config = new HttpConfiguration();
var container = new Container(rules =>
    rules.WithUnknownServiceResolver(GetNotRegisteredServices(Assembly.GetExecutingAssembly())));

container.WithWebApi(config);

            using (var scope = config.DependencyResolver.BeginScope())
            {
                var controller = scope.GetService(typeof(MyController));
                Assert.NotNull(controller);
                Assert.AreSame(controller, scope.GetService(typeof(MyController)));
            }

            var blah = container.Resolve<Blah>();
            Assert.AreSame(blah, container.Resolve<Blah>());
        }

        private static Rules.UnknownServiceResolver GetNotRegisteredServices(params Assembly[] assemblies)
        {
            // load types once
            var implTypes = assemblies.ThrowIfNull().SelectMany(a => a.GetLoadedTypes()).ToArray();

            return request =>
            {
                var container = request.Container;
                var reuse = container.OpenedScope != null ? Reuse.InRequest : DryIoc.Reuse.Singleton;
                container.RegisterMany(implTypes, type => type.IsAssignableTo(request.ServiceType), reuse);
                return container.GetServiceFactoryOrDefault(request.ServiceType, serviceKey: null);
            };
        }

        [Test]
        public void Can_resolve_filter_provider()
        {
            var config = new HttpConfiguration();
            var container = new Container().WithWebApi(config);
            var filterProvider = container.Resolve<IFilterProvider>();
            Assert.IsInstanceOf<DryIocFilterProvider>(filterProvider);

            var dummyFilter = Substitute.For<IFilter>();

            var descriptor = Substitute.For<HttpActionDescriptor>();

            descriptor.GetFilters().ReturnsForAnyArgs(c => new Collection<IFilter>(new[] { dummyFilter }));

            var controllerDescriptor = Substitute.For<HttpControllerDescriptor>();
            controllerDescriptor.GetFilters().ReturnsForAnyArgs(c => new Collection<IFilter>());
            descriptor.ControllerDescriptor = controllerDescriptor;

            var filters = filterProvider.GetFilters(config, descriptor);

            CollectionAssert.Contains(filters.Select(f => f.Instance), dummyFilter);
        }

        [Test]
        public void Can_register_current_request_in_dependency_scope()
        {
            var scopedContainer = new Container().OpenScope();
            using (var dependencyScope = new DryIocDependencyScope(scopedContainer))
            {
                var request = new HttpRequestMessage();
                request.Properties.Add(HttpPropertyKeys.DependencyScope, dependencyScope);

                var handler = new RegisterRequestMessageHandler();
                handler.RegisterInDependencyScope(request);

                var message = dependencyScope.GetService(typeof(HttpRequestMessage));
                Assert.AreSame(request, message);
            }
        }

        public class Blah {}
        public class Fooh {}
    }
}
