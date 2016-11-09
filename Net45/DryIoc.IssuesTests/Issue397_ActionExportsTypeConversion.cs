using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Threading;
using DryIoc.MefAttributedModel;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue397_ActionExportsTypeConversion
    {
        public Issue397_ActionExportsTypeConversion()
        {
            // use English exception messages
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en");
        }

        [Test, Ignore("InvalidOperationException")]
        public void Exported_actions_can_be_resolved_as_array()
        {
            var cnt = new Container().WithMef();
            cnt.RegisterExports(typeof(Exporter), typeof(ArrayImporter));

            // System.InvalidOperationException : No coercion operator is defined between types 'System.Object' and 'System.Void'.
            var importer = cnt.Resolve<ArrayImporter>();
            Assert.IsNotNull(importer.ImportedMethods);
            Assert.AreEqual(2, importer.ImportedMethods.Length);

            var method1 = importer.ImportedMethods.First();
            Assert.IsNotNull(method1);
            Assert.DoesNotThrow(() => method1());

            var method2 = importer.ImportedMethods.Last();
            Assert.IsNotNull(method2);
            Assert.DoesNotThrow(() => method2());
        }

        [Test, Ignore("InvalidOperationException")]
        public void Exported_actions_can_be_resolved_as_IEnumerable()
        {
            var cnt = new Container().WithMef();
            cnt.RegisterExports(typeof(Exporter), typeof(EnumerableImporter));

            // System.InvalidOperationException : No coercion operator is defined between types 'System.Object' and 'System.Void'.
            var importer = cnt.Resolve<EnumerableImporter>();
            Assert.IsNotNull(importer.ImportedMethods);
            Assert.AreEqual(2, importer.ImportedMethods.Count());

            var method1 = importer.ImportedMethods.First();
            Assert.IsNotNull(method1);
            Assert.DoesNotThrow(() => method1());

            var method2 = importer.ImportedMethods.Last();
            Assert.IsNotNull(method2);
            Assert.DoesNotThrow(() => method2());
        }

        [Test, Ignore("InvalidOperationException")]
        public void Exported_actions_can_be_resolved_as_IEnumerable_of_Lazy()
        {
            var cnt = new Container().WithMef();
            cnt.RegisterExports(typeof(Exporter), typeof(EnumerableOfLazyImporter));

            // resolution works fine
            var importer = cnt.Resolve<EnumerableOfLazyImporter>();
            Assert.IsNotNull(importer.ImportedMethods);
            Assert.AreEqual(2, importer.ImportedMethods.Count());

            // System.InvalidOperationException : No coercion operator is defined between types 'System.Object' and 'System.Void'.
            var method1 = importer.ImportedMethods.First().Value;
            Assert.IsNotNull(method1);
            Assert.DoesNotThrow(() => method1());

            var method2 = importer.ImportedMethods.Last().Value;
            Assert.IsNotNull(method2);
            Assert.DoesNotThrow(() => method2());
        }

        [Test, Ignore("InvalidOperationException")]
        public void Exported_actions_can_be_resolved_as_IEnumerable_of_Lazy_with_metadata()
        {
            var cnt = new Container().WithMef();
            cnt.RegisterExports(typeof(Exporter), typeof(EnumerableOfLazyWithMetadataImporter));

            // System.InvalidOperationException : No coercion operator is defined between types 'System.Void' and 'System.Object'.
            var importer = cnt.Resolve<EnumerableOfLazyWithMetadataImporter>();
            Assert.IsNotNull(importer.ImportedMethods);
            Assert.AreEqual(2, importer.ImportedMethods.Count());

            var method1 = importer.ImportedMethods.First().Value;
            Assert.IsNotNull(method1);
            Assert.DoesNotThrow(() => method1());

            var method2 = importer.ImportedMethods.Last().Value;
            Assert.IsNotNull(method2);
            Assert.DoesNotThrow(() => method2());
        }

        [Test, Ignore("ArgumentException")]
        public void Exported_actions_can_be_resolved_as_IEnumerable_of_Lazy_within_scope()
        {
            var cnt = new Container().WithMef().With(r => r.WithImplicitRootOpenScope().WithDefaultReuseInsteadOfTransient(Reuse.InCurrentScope));
            cnt.RegisterExports(typeof(Exporter), typeof(EnumerableOfLazyImporter));

            // resolution works fine
            var importer = cnt.Resolve<EnumerableOfLazyImporter>();
            Assert.IsNotNull(importer.ImportedMethods);
            Assert.AreEqual(2, importer.ImportedMethods.Count());

            // System.ArgumentException : Expression of type 'System.Void' cannot be used for return type 'System.Object'
            var method1 = importer.ImportedMethods.First().Value;
            Assert.IsNotNull(method1);
            Assert.DoesNotThrow(() => method1());

            var method2 = importer.ImportedMethods.Last().Value;
            Assert.IsNotNull(method2);
            Assert.DoesNotThrow(() => method2());
        }

        [Test, Ignore("ArgumentException")]
        public void Exported_actions_can_be_resolved_as_IEnumerable_of_Lazy_with_metadata_within_scope()
        {
            var cnt = new Container().WithMef().With(r => r.WithImplicitRootOpenScope().WithDefaultReuseInsteadOfTransient(Reuse.InCurrentScope));
            cnt.RegisterExports(typeof(Exporter), typeof(EnumerableOfLazyWithMetadataImporter));

            // resolution works fine
            var importer = cnt.Resolve<EnumerableOfLazyWithMetadataImporter>();
            Assert.IsNotNull(importer.ImportedMethods);
            Assert.AreEqual(2, importer.ImportedMethods.Count());

            // System.ArgumentException : Expression of type 'System.Void' cannot be used for return type 'System.Object'
            var method1 = importer.ImportedMethods.First().Value;
            Assert.IsNotNull(method1);
            Assert.DoesNotThrow(() => method1());

            var method2 = importer.ImportedMethods.Last().Value;
            Assert.IsNotNull(method2);
            Assert.DoesNotThrow(() => method2());
        }

        private const string ContractName = "Issue397";

        private class Exporter
        {
            [Export(ContractName), ExportMetadata("MethodName", "Method one")]
            public void Method1()
            {
            }

            [Export(ContractName), ExportMetadata("MethodName", "Method two")]
            public void Method2()
            {
            }
        }

        [Export]
        private class ArrayImporter
        {
            [ImportMany(ContractName)]
            public Action[] ImportedMethods { get; set; }
        }

        [Export]
        private class EnumerableImporter
        {
            [ImportMany(ContractName)]
            public IEnumerable<Action> ImportedMethods { get; set; }
        }

        [Export]
        private class EnumerableOfLazyImporter
        {
            [ImportMany(ContractName)]
            public IEnumerable<Lazy<Action>> ImportedMethods { get; set; }
        }

        [Export]
        private class EnumerableOfLazyWithMetadataImporter
        {
            [ImportMany(ContractName)]
            public IEnumerable<Lazy<Action, IDictionary<string, object>>> ImportedMethods { get; set; }
        }
    }
}
