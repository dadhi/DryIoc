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

        [Test]
        public void If_reuse_type_does_not_implement_IReuse_it_should_Throw()
        {
            var container = new Container().WithAttributedModel();
            Assert.Throws<ContainerException>(() => 
                container.RegisterExports(typeof(ServiceWithBadReuseAttribute)));
        }
    }

    [Export, TransientReuse]
    public class ServiceWithReuseAttribute {}

    [Export, Reuse(typeof(string))]
    public class ServiceWithBadReuseAttribute { }
}
