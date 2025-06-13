using DryIocAttributes;
using DryIoc.MefAttributedModel;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue349_MEF_ReuseAttribute_treats_ScopeName_as_string_instead_of_object_type : ITest
    {
        public int Run()
        {
            Can_provide_the_object_Scope_service_name_into_the_attribute();
            Can_provide_the_multiple_scope_names_in_the_attribute();
            return 2;
        }

        [Test]
        public void Can_provide_the_object_Scope_service_name_into_the_attribute()
        {
            var container = new Container();

            container.RegisterExportsAndTypes(typeof(Service));

            var scope = container.OpenScope(typeof(ScopeMarker));
            scope.Resolve<Service>();
        }

        [Test]
        public void Can_provide_the_multiple_scope_names_in_the_attribute()
        {
            var container = new Container();

            container.RegisterExportsAndTypes(typeof(MultiService));

            var scope = container.OpenScope(typeof(ScopeMarker));
            scope.Resolve<MultiService>();

            var scope1 = container.OpenScope("ScopeMarker");
            scope1.Resolve<MultiService>();

            var scope2 = container.OpenScope(42);
            scope1.Resolve<MultiService>();
        }

        class ScopeMarker { }

        [Reuse(ReuseType.Scoped, typeof(ScopeMarker))]
        class Service { }

        [Reuse(ReuseType.Scoped, typeof(ScopeMarker), "ScopeMarker", 42)]
        class MultiService { }
    }
}
