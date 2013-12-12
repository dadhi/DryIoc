using System.ComponentModel.Composition;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class DelegateFactoryTests
    {
        [Test]
        public void Could_export_delegate_factory_with_custom_interface()
        {
            var container = new Container(AttributedModel.DefaultSetup);
            container.RegisterExports(typeof(OrangeFactory));

            container.Resolve<OrangeFactory>();
            var orange = container.Resolve<Orange>();
        }
    }

    [Export(typeof(Orange)), ExportAsFactory]
    class OrangeFactory : IFactory<Orange>
    {
        public Orange Create()
        {
            return new Orange();
        }
    }

    class Orange {}
}
