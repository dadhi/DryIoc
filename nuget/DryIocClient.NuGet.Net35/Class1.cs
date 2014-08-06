using System.ComponentModel.Composition;
using DryIoc;
using DryIoc.MefAttributedModel;
using NUnit.Framework;

namespace DryIocClient.NuGet.Net35
{
    [TestFixture]
    public class Class1
    {
        [Test]
        public void Test()
        {
            var container = new Container();
            container.RegisterExports(typeof (ExportedClass));

            container.Resolve<ExportedClass>();
        }
    }

    [Export]
    internal class ExportedClass {}
}