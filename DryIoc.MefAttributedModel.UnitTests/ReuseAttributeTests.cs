using System.ComponentModel.Composition;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class ReuseAttributeTests
    {
        [Test]
        public void Can_specify_any_supported_Reuse_using_attribute()
        {
            var container = new Container().WithAttributedModel();
            container.RegisterExports(typeof(ServiceWithReuseAttribute));

            var service = container.Resolve<ServiceWithReuseAttribute>();
            var otherService = container.Resolve<ServiceWithReuseAttribute>();

            Assert.That(service, Is.Not.SameAs(otherService));
        }
    }

    [Export, TransientReuse]
    public class ServiceWithReuseAttribute {}
}
