using System.Linq;
using System.Web.Mvc;
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
        public void Can_resolve_from_dependency_resolver()
        {
            var container = new Container().WithMvc(new[] { typeof(DryIocMvcTests).Assembly });

            container.Register<Blah>(Reuse.Singleton);
            container.Register<Fooh>(serviceKey: 1);
            container.Register<Fooh>(serviceKey: 2);

            var resolver = DependencyResolver.Current;

            var blah = resolver.GetService(typeof(Blah));
            Assert.NotNull(blah);
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

        public class Blah { }
        public class Fooh { }
    }
}
