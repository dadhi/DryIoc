using System.ComponentModel.Composition;
using DryIoc.MefAttributedModel;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue451_Should_forbid_static_imports
    {
        [Test]
        public void Should_ignore_static_import()
        {
            var container = new Container().WithMef();

            container.RegisterInstance(42);

            container.RegisterExports(typeof(A));

            var a = container.Resolve<A>();

            Assert.AreEqual(42, a.X);
            Assert.AreEqual(default(int), A.Y); // static property is ignored
        }

        [Export]
        class A
        {
            [Import]
            public int X { get; set; }

            [Import]
            public static int Y { get; set; }
        }
    }
}
