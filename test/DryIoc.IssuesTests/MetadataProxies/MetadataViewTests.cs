using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DryIoc.MefAttributedModel;
using NUnit.Framework;

namespace DryIoc.IssuesTests.MetadataProxies
{
    [TestFixture]
    public class MetadataViewTests
    {
        [Test]
        public void Untyped_metadata_works_out_of_the_box()
        {
            var cont = new Container().WithMef();
            cont.RegisterExports(new[] { Assembly.GetExecutingAssembly() });

            var importer = cont.Resolve<UntypedPluginImporter>();
            Assert.AreEqual(2, importer.Plugins.Length);
            Assert.AreEqual("PdfExport, RtfExport", string.Join(", ", importer.Plugins.Select(p => p.Metadata["PluginName"]).OrderBy(m => m)));
        }

        [Test]
        public void Typed_metadata_view_cannot_be_resolved_by_default()
        {
            var cont = new Container().WithMef();
            cont.RegisterExports(new[] { Assembly.GetExecutingAssembly() });

            var importer = cont.Resolve<TypedPluginImporter>();
            Assert.AreEqual(0, importer.Plugins.Length);
        }

        [Test]
        public void Untyped_metadata_works_fine_with_new_Lazy_wrapper()
        {
            var cont = new Container().WithMef().WithTypedMetadataViewGenerator();
            cont.RegisterExports(new[] { Assembly.GetExecutingAssembly() });

            var importer = cont.Resolve<UntypedPluginImporter>();
            Assert.AreEqual(2, importer.Plugins.Length);
            Assert.AreEqual("PdfExport, RtfExport", string.Join(", ", importer.Plugins.Select(p => p.Metadata["PluginName"]).OrderBy(m => m)));
        }

        [Test]
        public void Typed_metadata_view_now_also_resolves()
        {
            var cont = new Container().WithMef().WithTypedMetadataViewGenerator();
            cont.RegisterExports(new[] { Assembly.GetExecutingAssembly() });

            var importer = cont.Resolve<TypedPluginImporter>();
            Assert.AreEqual(2, importer.Plugins.Length);
            Assert.AreEqual("PdfExport, RtfExport", string.Join(", ", importer.Plugins.Select(p => p.Metadata.PluginName).OrderBy(m => m)));
        }
    }

    [Export]
    public class UntypedPluginImporter
    {
        [ImportMany]
        public Lazy<IPlugin, IDictionary<string, object>>[] Plugins { get; set; }
    }

    [Export]
    public class TypedPluginImporter
    {
        [ImportMany]
        public Lazy<IPlugin, IPluginMetadata>[] Plugins { get; set; }
    }

    public interface IPlugin { }

    public interface IPluginMetadata { string PluginName { get; } }

    [Export(typeof(IPlugin)), ExportMetadata("PluginName", "PdfExport")]
    internal class FilePluginPdf : IPlugin { }

    [Export(typeof(IPlugin)), ExportMetadata("PluginName", "RtfExport")]
    internal class FilePluginRtf : IPlugin { }
}
