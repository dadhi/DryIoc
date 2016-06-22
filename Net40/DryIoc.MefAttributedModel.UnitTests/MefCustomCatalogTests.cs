using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class MefCustomCatalogTests
    {
        private ComposablePartCatalog CustomCatalog { get; } = new CustomCatalog(typeof(IDummyService));

        private ComposablePartCatalog AssemblyCatalog { get; } = new AssemblyCatalog(typeof(IDummyService).Assembly);

        [Test]
        public void Can_get_export_using_custom_catalog()
        {
            var container = new CompositionContainer(CustomCatalog);
            var service = container.GetExport<IDummyService>();

            Assert.NotNull(service);
            Assert.NotNull(service.Value);
            Assert.AreEqual(default(int), service.Value.GetValue());
            Assert.AreEqual(default(bool), service.Value.GetFlag());
            Assert.AreEqual(UnitTests.CustomCatalog.Signature, service.Value.GetString());
        }

        [Test]
        public void Can_get_export_from_aggregate_catalog_having_included_custom_catalog()
        {
            var catalog = new AggregateCatalog(CustomCatalog, AssemblyCatalog);
            var container = new CompositionContainer(catalog);
            var consumer = container.GetExport<IDummyServiceConsumer>();

            Assert.NotNull(consumer);
            Assert.NotNull(consumer.Value);
            Assert.NotNull(consumer.Value.Single);
            Assert.NotNull(consumer.Value.Multiple);
            Assert.AreEqual(1, consumer.Value.Multiple.Length);

            Assert.AreEqual(default(int), consumer.Value.Single.GetValue());
            Assert.AreEqual(default(bool), consumer.Value.Single.GetFlag());
            Assert.AreEqual(UnitTests.CustomCatalog.Signature, consumer.Value.Single.GetString());
        }
    }

    /// <summary>Custom ComposablePartCatalog for MEF-related unit tests.</summary>
    /// <remarks>Produces dummy proxy objects implementing the given interfaces.</remarks>
    public class CustomCatalog : ComposablePartCatalog
    {
        public const string Signature = "Hello, this is a runtime-generated type.";

        public CustomCatalog(params Type[] interfaces)
        {
            InnerParts = interfaces.Select(i => new CustomComposablePartDefinition(i)).ToArray();
        }

        public override IQueryable<ComposablePartDefinition> Parts
        {
            get { return InnerParts.AsQueryable(); }
        }

        private ComposablePartDefinition[] InnerParts { get; }

        private class CustomComposablePart : ComposablePart
        {
            public CustomComposablePart(CustomComposablePartDefinition definition)
            {
                Definition = definition;
            }

            public CustomComposablePartDefinition Definition { get; }

            public override IEnumerable<ExportDefinition> ExportDefinitions
            {
                get { return Definition.ExportDefinitions; }
            }

            public override IEnumerable<ImportDefinition> ImportDefinitions
            {
                get { return Definition.ImportDefinitions; }
            }

            public override object GetExportedValue(ExportDefinition definition)
            {
                return new CustomComposablePartProxy(Definition.ExportedInterface).GetTransparentProxy();
            }

            public override void SetImport(ImportDefinition definition, IEnumerable<Export> exports)
            {
                throw new NotSupportedException("CustomCatalog supports exports only.");
            }
        }

        private class CustomComposablePartDefinition : ComposablePartDefinition
        {
            public CustomComposablePartDefinition(Type exportedInterface)
            {
                ExportedInterface = exportedInterface;
            }

            public Type ExportedInterface { get; }

            public override IEnumerable<ExportDefinition> ExportDefinitions
            {
                get
                {
                    var metadata = new Dictionary<string, object>
                    {
                        { "ExportedInterface", ExportedInterface },
                        { "EnvironmentVersion", Environment.Version },
                        { CompositionConstants.ExportTypeIdentityMetadataName, ExportedInterface.FullName },
                    };

                    var export = new ExportDefinition(ExportedInterface.FullName, metadata);
                    return new[] { export };
                }
            }

            public override IEnumerable<ImportDefinition> ImportDefinitions
            {
                get { return new ImportDefinition[0]; } // no imports
            }

            public override ComposablePart CreatePart()
            {
                return new CustomComposablePart(this);
            }
        }

        private class CustomComposablePartProxy : RealProxy
        {
            public CustomComposablePartProxy(Type @interface) : base(@interface)
            {
            }

            public override IMessage Invoke(IMessage msg)
            {
                var methodCall = msg as IMethodCallMessage;
                var returnType = (methodCall?.MethodBase as MethodInfo)?.ReturnType;
                if (returnType == typeof(string))
                {
                    return new ReturnMessage(Signature, new object[0], 0, methodCall?.LogicalCallContext, methodCall);
                }
                else if (returnType != null)
                {
                    var returnValue = Activator.CreateInstance(returnType);
                    return new ReturnMessage(returnValue, new object[0], 0, methodCall?.LogicalCallContext, methodCall);
                }

                return new ReturnMessage(new Exception("Invalid method call message"), methodCall);
            }
        }
    }
}
