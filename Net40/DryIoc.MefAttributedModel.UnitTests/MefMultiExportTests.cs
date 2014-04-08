using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class MefMultiExportTests
    {
        [Import("a")]
        public Bazooka BazookaA { get; set; }

        [Import("b")]
        public Bazooka BazookaB { get; set; }

        [Test]
        public void Test()
        {
            var container = new CompositionContainer(new TypeCatalog(typeof(Bazooka)));
            container.SatisfyImportsOnce(this);

            Assert.That(BazookaA, Is.SameAs(BazookaB));
        }
    }

    [Export("a")]
    [Export("b")]
    public class Bazooka
    {
        
    }
}
