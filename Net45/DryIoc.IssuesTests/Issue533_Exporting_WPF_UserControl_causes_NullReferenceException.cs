using System.ComponentModel.Composition;
using DryIoc.MefAttributedModel;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue533_Exporting_WPF_UserControl_causes_NullReferenceException
    {
        [Test]
        public void Test()
        {
            var c = new Container().WithMef();

            c.RegisterExports(typeof(Explicit));
            c.UseInstance("prop");

            var e = c.Resolve<IExplicit>();
            Assert.AreEqual("prop", e.X);
        }

        public interface IExplicit
        {
            string X { get; set; }
        }

        [Export(typeof(IExplicit))]
        public class Explicit : IExplicit
        {
            string IExplicit.X { get; set; }

            [Import]
            public string X
            {
                get { return ((IExplicit)this).X; }
                set { ((IExplicit)this).X = value; }
            }
        }
    }
}
