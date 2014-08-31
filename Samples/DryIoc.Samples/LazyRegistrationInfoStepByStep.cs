using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DryIoc.MefAttributedModel;
using NUnit.Framework;

namespace DryIoc.Samples
{
    [TestFixture]
    public class LazyRegistrationInfoStepByStep
    {
        [Test]
        public void Can_get_registration_info_with_implementation_type_replaced_by_its_full_name()
        {
            var registrationInfo = AttributedModel.GetRegistrationInfoOrDefault(typeof(Frog));
            registrationInfo.ImplementationTypeFullName = registrationInfo.ImplementationType.FullName;
            registrationInfo.ImplementationType = null;

            for (var i = 0; i < registrationInfo.Exports.Length; i++)
            {
                var export = registrationInfo.Exports[i];
                export.ServiceTypeFullName = export.ServiceType.FullName;
                export.ServiceType = null;
            }

            Assert.That(registrationInfo.ImplementationTypeFullName, Is.Not.Null);
            Assert.That(registrationInfo.Exports[0].ServiceTypeFullName, Is.Not.Null);

            var asm = Assembly.LoadFrom("DryIoc.Samples.dll");
            var type = asm.GetType(registrationInfo.ImplementationTypeFullName);
            Assert.That(type, Is.Not.Null);
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
            var lazyRegistrations = registrations.Select(info =>
            {
                info.ImplementationTypeFullName = info.ImplementationType.FullName;
                info.ImplementationType = null; // replace implementation type with its name

                for (var i = 0; i < info.Exports.Length; i++)
                {
                    var export = info.Exports[i];
                    export.ServiceTypeFullName = export.ServiceType.FullName;
                    export.ServiceType = null; // replace service type with its name
                }

                return info;
            });

            // In run time De-serialize registrations and register them as rule for unresolved services
            //=========================================================================================

            var lazyLoadedAssembly = new Lazy<Assembly>(() => Assembly.LoadFrom(assemblyFile));

            // Step 1 - Create Index for fast search by ExportInfo.ServiceTypeFullName.
            var lazyRegIndex = new Dictionary<string, List<KeyValuePair<object, RegistrationInfo>>>();
            foreach (var lazyRegistration in lazyRegistrations)
            {
                var exports = lazyRegistration.Exports;
                for (var i = 0; i < exports.Length; i++)
                {
                    var export = exports[i];
                    var serviceTypeFullName = export.ServiceTypeFullName;

                    List<KeyValuePair<object, RegistrationInfo>> regs;
                    if (!lazyRegIndex.TryGetValue(serviceTypeFullName, out regs))
                        lazyRegIndex.Add(serviceTypeFullName, regs = new List<KeyValuePair<object, RegistrationInfo>>());
                    regs.Add(new KeyValuePair<object, RegistrationInfo>(export.ServiceKeyInfo.Key, lazyRegistration));
                }
            }

            // Step 2 - Add resolution rule for creating factory on resolve.
            var rules = ResolutionRules.Default.With((request, registry) =>
            {
                List<KeyValuePair<object, RegistrationInfo>> regs;
                if (!lazyRegIndex.TryGetValue(request.ServiceType.FullName, out regs))
                    return null;

                var regIndex = regs.FindIndex(pair => Equals(pair.Key, request.ServiceKey));
                if (regIndex == -1)
                    return null;

                var info = regs[regIndex].Value;
                if (info.ImplementationType == null)
                    info.ImplementationType = lazyLoadedAssembly.Value.GetType(info.ImplementationTypeFullName);

                return info.CreateFactory();
            });

            // Test that resolve works
            //========================
            var container = new Container(rules);
            var thing = container.Resolve<IThing>();
            Assert.NotNull(thing);
        }
    }

    public interface IThing { }

    public interface IFrog {}

    [ExportAll]
    class Frog : IFrog { }
}
