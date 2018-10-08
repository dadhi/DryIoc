using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DryIoc.Web;
using NSubstitute;
using NUnit.Framework;

namespace DryIoc.Mvc.UnitTests
{
    [TestFixture]
    public class DryIocMvcTests
    {
        [Test]
        public void Can_enable_mvc_support_for_container_and_resolve_filter_provider()
        {
            var container = new Container().WithMvc(new[] { typeof(DryIocMvcTests).Assembly });

            var filterProvider = container.Resolve<IFilterProvider>();

            Assert.IsInstanceOf<DryIocFilterAttributeFilterProvider>(filterProvider);
        }

        [Test]
        public void Can_specify_to_throw_on_unresolved_controller()
        {
            var fakeItems = new Dictionary<object, object>();
            var root = new Container(scopeContext: new HttpContextScopeContext(() => fakeItems))
                .WithMvc(new[] { typeof(DryIocMvcTests).Assembly }, throwIfUnresolved: type => type.IsController());

            using (root.OpenScope(Reuse.WebRequestScopeName))
            {
                var ex = Assert.Throws<ContainerException>(() =>
                    DependencyResolver.Current.GetService(typeof(MissingDependencyController)));

                Assert.AreEqual(
                    Error.NameOf(Error.UnableToResolveUnknownService), 
                    Error.NameOf(ex.Error));
            }
        }

        [Test]
        public void Can_specify_scope_context_scope_error_handler()
        {
            var contextItems = new Dictionary<object, object>();
            Exception scopeContextEx = null;
            var container = new Container(
                scopeContext: new HttpContextScopeContext(ex => scopeContextEx = ex, () => contextItems))
                .WithMvc(new[] { typeof(DryIocMvcTests).Assembly });

            container.Register<Blah>(Reuse.InWebRequest);

            contextItems = null; // let's wrack the context!

            using (container.OpenScope(Reuse.WebRequestScopeName))
            {
                Assert.Throws<ContainerException>(() => container.Resolve<Blah>());
                Assert.IsInstanceOf<NullReferenceException>(scopeContextEx);
            }
        }

        [Test]
        public void Without_specyfuing_scope_context_error_handler_the_error_should_be_rethrown()
        {
            var contextItems = new Dictionary<object, object>();
            var container = new Container(
                    scopeContext: new HttpContextScopeContext(() => contextItems))
                .WithMvc(new[] { typeof(DryIocMvcTests).Assembly });

            container.Register<Blah>(Reuse.InWebRequest);

            var scope = container.OpenScope(Reuse.WebRequestScopeName);
            {
                contextItems = null; // let's wrack the context!
                Assert.Throws<ContainerException>(() => container.Resolve<Blah>());
            }
            Assert.Throws<NullReferenceException>(() => scope.Dispose());
        }

        [Test]
        public void Can_resolve_from_dependency_resolver()
        {
            var container = new Container().WithMvc(new[] { typeof(DryIocMvcTests).Assembly });

            container.Register<Blah>(Reuse.Singleton);
            container.Register<Fooh>(serviceKey: 1);
            container.Register<Fooh>(serviceKey: 2);

            var resolver = DependencyResolver.Current;

            var blah = resolver.GetService(typeof(Blah));
            Assert.IsNotNull(blah);
            Assert.AreSame(blah, resolver.GetService(typeof(Blah)));

            var foohs = resolver.GetServices(typeof(Fooh)).ToArray();
            Assert.AreEqual(2, foohs.Length);
        }

        [Test]
        public void Can_resolve_filter_provider()
        {
            var container = new Container().WithMvc(new[] { typeof(DryIocMvcTests).Assembly });
            var filterProvider = container.Resolve<IFilterProvider>();
            Assert.IsInstanceOf<DryIocFilterAttributeFilterProvider>(filterProvider);

            filterProvider.GetFilters(Substitute.For<ControllerContext>(), Substitute.For<ActionDescriptor>());
        }

        [Test]
        public void When_custom_scope_context_is_specified_then_it_should_be_preserved()
        {
            var container = new Container(scopeContext: new AsyncExecutionFlowScopeContext())
                .WithMvc(new[] { typeof(DryIocMvcTests).Assembly });

            Assert.IsInstanceOf<AsyncExecutionFlowScopeContext>(container.ScopeContext);
        }

        [Test]
        public void Correct_filter_provider_substitution()
        {
            FilterProviders.Providers.Add(new FilterAttributeFilterProvider());
            Assert.IsNotEmpty(FilterProviders.Providers.OfType<FilterAttributeFilterProvider>());

            new Container().WithMvc(new[] { typeof(DryIocMvcTests).Assembly });

            Assert.IsEmpty(FilterProviders.Providers.OfType<FilterAttributeFilterProvider>()
                .Except(FilterProviders.Providers.OfType<DryIocFilterAttributeFilterProvider>()));
            Assert.AreEqual(1, FilterProviders.Providers.OfType<DryIocFilterAttributeFilterProvider>().Count());
        }

        public class Blah { }
        public class Fooh { }

        public interface ISomeDep { }

        public class MissingDependencyController : Controller
        {
            public ISomeDep Dep { get; private set; }

            public MissingDependencyController(ISomeDep dep)
            {
                Dep = dep;
            }
        }
    }
}
