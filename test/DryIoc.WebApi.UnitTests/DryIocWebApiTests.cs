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

            container.Register<Blah>(Reuse.Singleton);
            container.Register<Fooh>(serviceKey: 1);
            container.Register<Fooh>(serviceKey: 2);

            var resolver = config.DependencyResolver;

            var blah = resolver.GetService(typeof(Blah));
            Assert.IsNotNull(blah);
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

            container.Register<Blah>(Reuse.InWebRequest);
            container.Register<Fooh>(Reuse.InWebRequest, serviceKey: 1);
            container.Register<Fooh>(Reuse.InWebRequest, serviceKey: 2);

            using (var scope = config.DependencyResolver.BeginScope())
            {
                var blah = scope.GetService(typeof(Blah));
                Assert.IsNotNull(blah);
                Assert.AreSame(blah, scope.GetService(typeof(Blah)));

                var foohs = scope.GetServices(typeof(Fooh)).ToArray();
                Assert.AreEqual(2, foohs.Length);
            }
        }

        [Test]
        public void Can_begin_scope_and_resolve_controller_without_specifying_assemblies()
        {
            var config = new HttpConfiguration();
            new Container().WithWebApi(config);

            using (var scope = config.DependencyResolver.BeginScope())
            {
                var controller = scope.GetService(typeof(MyController));
                Assert.IsNotNull(controller);
                Assert.AreSame(controller, scope.GetService(typeof(MyController)));
            }
        }

        [Test]
        public void Can_begin_scope_and_resolve_controller_specifying_assemblies()
        {
            var config = new HttpConfiguration();
            new Container().WithWebApi(config, new[] { Assembly.GetExecutingAssembly() });

            using (var scope = config.DependencyResolver.BeginScope())
            {
                var controller = scope.GetService(typeof(MyController));
                Assert.IsNotNull(controller);
                Assert.AreSame(controller, scope.GetService(typeof(MyController)));
            }
        }

        [Test]
        public void Can_verify_if_no_controllers_were_registered()
        {
            var config = new HttpConfiguration();
            var container = new Container().WithWebApi(config, new[] { Assembly.GetExecutingAssembly() });
            var errors = container.Validate();

            Assert.AreEqual(
                typeof(MissingDependencyController).GetImplementedServiceTypes().Length,
                errors.Length);
        }

        [Test]
        public void Can_specify_to_throw_on_unresolved_controller()
        {
            var config = new HttpConfiguration();
            new Container().WithWebApi(config, throwIfUnresolved: type => type.IsController());

            using (var scope = config.DependencyResolver.BeginScope())
            {
                var ex = Assert.Throws<ContainerException>(() =>
                    scope.GetService(typeof(MissingDependencyController)));

                Assert.AreEqual(Error.UnableToResolveUnknownService, ex.Error);
            }
        }

        [Test]
        public void IsController_will_not_recognize_type_without_controller_suffix() => 
            Assert.IsFalse(typeof(ControllerWithWrongName).IsController());

        [Test]
        public void Controller_with_property_injection()
        {
            var config = new HttpConfiguration();
            var c = new Container()
                .With(rules => rules.With(propertiesAndFields: PropertiesAndFields.Auto))
                .WithWebApi(config, throwIfUnresolved: type => type.IsController());

            c.Register<A>(Reuse.Singleton);

            using (var scope = config.DependencyResolver.BeginScope())
            {
                var propController = (PropController)scope.GetService(typeof(PropController));
                Assert.IsNotNull(propController.A);
            }
        }


        public class PropController : ApiController
        {
            public A A { get; set; }
        }

        public class A { }

        public class MyController : ApiController { }

        public interface ISomeDep { }

        public class MissingDependencyController : ApiController
        {
            public ISomeDep Dep { get; private set; }

            public MissingDependencyController(ISomeDep dep)
            {
                Dep = dep;
            }
        }

        public class ControllerWithWrongName : ApiController
        {
        }

        [Test]
        public void Can_begin_scope_and_resolve_any_service_as_fallback_rule()
        {
            var config = new HttpConfiguration();
            var container = new Container(rules => rules.WithDefaultReuse(Reuse.ScopedOrSingleton))
                .WithAutoFallbackDynamicRegistrations(config.Services.GetAssembliesResolver().GetAssemblies());

            container.WithWebApi(config);

            using (var scope = config.DependencyResolver.BeginScope())
            {
                var controller = scope.GetService(typeof(MyController));
                Assert.IsNotNull(controller);
                Assert.AreSame(controller, scope.GetService(typeof(MyController)));
            }

            var blah = container.Resolve<Blah>();
            Assert.AreSame(blah, container.Resolve<Blah>());
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
            var scopedContainer = new Container().OpenScope(Reuse.WebRequestScopeName);
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

        [Test]
        public void When_custom_scope_context_is_specified_then_it_should_be_preserved()
        {
            var config = new HttpConfiguration();
            var container = new Container(scopeContext: new DummyContext())
                .WithWebApi(config);

            Assert.IsInstanceOf<DummyContext>(container.ScopeContext);
        }

        [Test]
        public void One_and_only_one_filter_provider_registered()
        {
            var config = new HttpConfiguration();
            var container = new Container().WithWebApi(config);
            var services = config.Services.GetFilterProviders().OfType<DryIocFilterProvider>();
            if (services.Count() > 1) Assert.Fail("More than one provider registered");
            else if (services.Count() == 0) Assert.Fail("No provider registered.");
        }

        public class Blah { }

        public class Fooh { }

        public class DummyContext : IScopeContext
        {
            public string RootScopeName => string.Empty;

            public void Dispose() { }

            public IScope GetCurrentOrDefault() => new Scope();

            public IScope SetCurrent(SetCurrentScopeHandler setCurrentScope) => setCurrentScope(new Scope());
        }
    }
}
