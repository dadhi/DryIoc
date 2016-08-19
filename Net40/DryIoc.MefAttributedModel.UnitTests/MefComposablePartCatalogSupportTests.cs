using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class MefComposablePartCatalogSupportTests
    {
        [Test]
        public void Mef_works()
        {
            var catalog = new TypeCatalog(typeof(Yes), typeof(No));

            var container = new CompositionContainer(catalog);

            var yes = container.GetExportedValue<Yes>();
            Assert.IsNotNull(yes);
        }

        [Test]
        public void DryIoc_can_consume_catalog()
        {
            var registrations = new List<ExportedRegistrationInfo>();

            ComposablePartCatalog catalog = new TypeCatalog(typeof(Yes), typeof(No), typeof(Ok));
            foreach (var part in catalog.Parts)
            {
                var creationInfoField = part.GetType().GetMembers(t => t.DeclaredFields)
                    .FirstOrDefault(f => f.Name == "_creationInfo")
                    .ThrowIfNull();
                var creationInfo = creationInfoField.GetValue(part);

                var getPartMethod = creationInfo.GetType().GetMembers(t => t.DeclaredMethods)
                    .FirstOrDefault(m => m.Name == "GetPartType")
                    .ThrowIfNull();

                var implementationType = (Type)getPartMethod.Invoke(creationInfo, ArrayTools.Empty<object>());

                ExportInfo[] exports = null;
                foreach (var exportDefinition in part.ExportDefinitions)
                {

                    //string serviceTypeFullName = null;
                    //object exportTypeObject;
                    //if (exportDefinition.Metadata.TryGetValue(CompositionConstants.ExportTypeIdentityMetadataName, out exportTypeObject))
                    //    serviceTypeFullName = (string)exportTypeObject;
                    //var contractName = exportDefinition.ContractName;
                    //var serviceKey = string.Equals(contractName, serviceTypeFullName) ? null : contractName;
                    //var export = new ExportInfo(null, serviceKey) { ServiceTypeFullName = serviceTypeFullName };

                    var serviceInfo = GetServiceorDefault(exportDefinition);
                    var export = new ExportInfo(serviceInfo.ServiceType, serviceInfo.Details.ServiceKey);

                    exports = exports.AppendOrUpdate(export);
                }

                var registration = new ExportedRegistrationInfo
                {
                    ImplementationType = implementationType,
                    Exports = exports
                };

                registrations.Add(registration);
            }

            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(registrations);

            container.Resolve<IAnswer<Ok>>();

        }

        private static IServiceInfo GetServiceorDefault(ExportDefinition ed)
        {
            var ct = FindType(ed.ContractName);
            if (ct != null)
                return ServiceInfo.Of(ct);

            var et = FindType((string)ed.Metadata[CompositionConstants.ExportTypeIdentityMetadataName]);
            if (et != null)
                return ServiceInfo.Of(et, serviceKey: ed.ContractName);

            return null;
        }

        private static readonly Type _contractNameServices = 
            typeof(ExportAttribute).Assembly.GetType("System.ComponentModel.Composition.ContractNameServices", throwOnError: true);


        private static readonly PropertyInfo _typeIdentityCacheProperty = 
            _contractNameServices.GetProperty("TypeIdentityCache", BindingFlags.GetProperty | BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly Dictionary<Type, string> _typeIdentityCache =
            (Dictionary<Type, string>)_typeIdentityCacheProperty.GetValue(null, null);

        private static Type FindType(string exportTypeIdentity)
        {
            return _typeIdentityCache.FirstOrDefault(kvp => kvp.Value == exportTypeIdentity).Key;
        }

        public interface IAnswer { }
        public interface IAnswer<T> { }

        [Export]
        public class No { }


        [Export("hey", typeof(IAnswer))]
        [Export(typeof(IAnswer<Ok>))]
        public class Ok : IAnswer, IAnswer<Ok> { }

        [Export]
        public class Yes
        {
            [ImportingConstructor]
            public Yes(No no) { }
        }
    }

}
