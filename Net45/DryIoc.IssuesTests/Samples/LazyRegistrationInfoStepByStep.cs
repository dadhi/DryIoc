using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DryIoc.MefAttributedModel;
using DryIocAttributes;
using NUnit.Framework;

namespace DryIoc.IssuesTests.Samples
{
    public interface IAddin
    {
    }

    [TestFixture]
    public class LazyRegistrationInfoStepByStep
    {
        [Test]
        public void Can_get_registration_info_with_implementation_type_replaced_by_its_full_name()
        {
            var registrationInfo = AttributedModel.GetExportedRegistrations(typeof(Frog)).Single().MakeLazy();

            Assert.IsNotNull(registrationInfo.ImplementationTypeFullName);
            Assert.IsNotNull(registrationInfo.Exports[0].ServiceTypeFullName);

            var asm = Assembly.LoadFrom("DryIoc.IssuesTests.dll");
            var type = asm.GetType(registrationInfo.ImplementationTypeFullName);
            Assert.IsNotNull(type);
        }

        [Test]
        public void Register_interface_with_implementation_as_unregistered_type_resolution_rule()
        {
            const string assemblyFile = "DryIoc.Samples.CUT.dll";

            // In compile time prepare registrations for Serialization
            //========================================================

            var assembly = Assembly.LoadFrom(assemblyFile);

            // Step 1 - Scan assembly and find exported type, create DTOs for them.
            var registrations = AttributedModel.Scan(new[] { assembly });

            // Step 2 - Make DTOs lazy.
            var lazyRegistrations = registrations.Select(info => info.MakeLazy());

            // In run-time deserialize registrations and register them as rule for unresolved services
            //=========================================================================================

            var lazyLoadedAssembly = new Lazy<Assembly>(() => Assembly.LoadFrom(assemblyFile));

            // Step 1 - Create Index for fast search by ExportInfo.ServiceTypeFullName.
            var regInfoByServiceTypeNameIndex = new Dictionary<string, List<KeyValuePair<object, ExportedRegistrationInfo>>>();
            foreach (var lazyRegistration in lazyRegistrations)
            {
                var exports = lazyRegistration.Exports;
                for (var i = 0; i < exports.Length; i++)
                {
                    var export = exports[i];
                    var serviceTypeFullName = export.ServiceTypeFullName;

                    List<KeyValuePair<object, ExportedRegistrationInfo>> regs;
                    if (!regInfoByServiceTypeNameIndex.TryGetValue(serviceTypeFullName, out regs))
                        regInfoByServiceTypeNameIndex.Add(serviceTypeFullName, 
                            regs = new List<KeyValuePair<object, ExportedRegistrationInfo>>());
                    regs.Add(new KeyValuePair<object, ExportedRegistrationInfo>(export.ServiceKey, lazyRegistration));
                }
            }

            // Step 2 - Add resolution rule for creating factory on resolve.
            Rules.UnknownServiceResolver createFactoryFromAssembly = request =>
            {
                List<KeyValuePair<object, ExportedRegistrationInfo>> regs;
                if (!regInfoByServiceTypeNameIndex.TryGetValue(request.ServiceType.FullName, out regs))
                    return null;

                var regIndex = regs.FindIndex(pair => Equals(pair.Key, request.ServiceKey));
                if (regIndex == -1)
                    return null;

                var info = regs[regIndex].Value;
                if (info.ImplementationType == null)
                    info.ImplementationType = lazyLoadedAssembly.Value.GetType(info.ImplementationTypeFullName);

                return info.CreateFactory();
            };

            // Test that resolve works
            //========================
            var container = new Container(rules => rules.WithUnknownServiceResolvers(createFactoryFromAssembly));
            var thing = container.Resolve<IThing>();
            Assert.IsNotNull(thing);
        }
    }

    public interface IThing { }

    public interface IFrog {}

    [ExportMany]
    class Frog : IFrog { }
}
