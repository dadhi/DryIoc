using System.Web.Mvc;
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

            Assert.IsInstanceOf<DryIocAggregatedFilterAttributeFilterProvider>(filterProvider);
        }
    }
}
