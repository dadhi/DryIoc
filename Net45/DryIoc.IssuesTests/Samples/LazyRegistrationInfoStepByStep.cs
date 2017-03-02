using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using DryIoc.MefAttributedModel;
using DryIocAttributes;
using NUnit.Framework;
using ImTools;

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

                return regs[regIndex].Value.CreateFactory(typeName => lazyLoadedAssembly.Value.GetType(typeName));
            };

            // Test that resolve works
            //========================
            var container = new Container(rules => rules.WithUnknownServiceResolvers(createFactoryFromAssembly));
            var thing = container.Resolve<IThing>();
            Assert.IsNotNull(thing);
        }

        [Test]
        public void NonLazy_import_of_commands()
        {
            // ordinary registration
            var container = new Container().WithMef();
            container.RegisterExports(new[] { typeof(LazyRegistrationInfoStepByStep).Assembly });

            // check that importing commands actually works
            var cmds = container.Resolve<CommandImporter>();
            Assert.IsNotNull(cmds.Commands);
            Assert.AreEqual(2, cmds.Commands.Count());
            Assert.AreEqual("Sample command, Another command", string.Join(", ", cmds.Commands.Select(c => c.Metadata.Name).OrderByDescending(c => c)));
        }

        [Test]
        public void Lazy_import_of_commands()
        {
            // the same registration code as in the lazy sample
            //========================
            var assembly = typeof(LazyRegistrationInfoStepByStep).Assembly;

            // Step 1 - Scan assembly and find exported type, create DTOs for them.
            var registrations = AttributedModel.Scan(new[] { assembly });

            // Step 2 - Make DTOs lazy.
            var lazyRegistrations = registrations.Select(info => info.MakeLazy());

            // In run-time deserialize registrations and register them as rule for unresolved services
            //=========================================================================================

            var lazyLoadedAssembly = new Lazy<Assembly>(() => assembly);

            // Step 1 - Create Index for fast search by ExportInfo.ServiceTypeFullName.
            var registrationsByServiceTypeName = CollectRegistrationsByServiceTypeName(lazyRegistrations);

            Rules.DynamicRegistrationProvider dynamicRegistrations = (_, serviceType, serviceKey) =>
            {
                List<KeyValuePair<object, ExportedRegistrationInfo>> serviceTypeRegistrations;
                if (!registrationsByServiceTypeName.TryGetValue(serviceType.FullName, out serviceTypeRegistrations))
                    return null;

                var factories = new List<KV<object, Factory>>();
                foreach (var r in serviceTypeRegistrations)
                    factories.Add(KV.Of<object, Factory>(r.Key, r.Value.CreateFactory(lazyLoadedAssembly)));

                return factories;
            };

            // Test that resolve works
            //========================
            var container = new Container().WithMef()
                .With(rules => rules.WithDynamicRegistrations(dynamicRegistrations));

            var cmds = container.Resolve<CommandImporter>();

            Assert.IsNotNull(cmds.Commands);
            Assert.AreEqual(2, cmds.Commands.Count());
            Assert.AreEqual("Sample command, Another command", 
                string.Join(", ", cmds.Commands.Select(c => c.Metadata.Name).OrderByDescending(c => c)));
        }

        private static Dictionary<string, List<KeyValuePair<object, ExportedRegistrationInfo>>> 
            CollectRegistrationsByServiceTypeName(IEnumerable<ExportedRegistrationInfo> lazyRegistrations)
        {
            var registrationByServiceTypeName = new Dictionary<string, List<KeyValuePair<object, ExportedRegistrationInfo>>>();
            foreach (var lazyRegistration in lazyRegistrations)
            {
                var exports = lazyRegistration.Exports;
                for (var i = 0; i < exports.Length; i++)
                {
                    var export = exports[i];
                    var serviceTypeFullName = export.ServiceTypeFullName;

                    List<KeyValuePair<object, ExportedRegistrationInfo>> regs;
                    if (!registrationByServiceTypeName.TryGetValue(serviceTypeFullName, out regs))
                        registrationByServiceTypeName.Add(serviceTypeFullName,
                            regs = new List<KeyValuePair<object, ExportedRegistrationInfo>>());

                    regs.Add(new KeyValuePair<object, ExportedRegistrationInfo>(export.ServiceKey, lazyRegistration));
                }
            }

            return registrationByServiceTypeName;
        }

        [Test]
        public void Lazy_import_of_commands_using_LazyFactory()
        {
            // the same registration code as in the lazy sample
            //========================
            var assembly = typeof(LazyRegistrationInfoStepByStep).Assembly;

            // Step 1 - Scan assembly and find exported type, create DTOs for them.
            var registrations = AttributedModel.Scan(new[] { assembly });

            // Step 2 - Make DTOs lazy.
            var lazyRegistrations = registrations.Select(info => info.MakeLazy())
                .ToArray(); // NOTE: This is required to materialized DTOs to be seriliazed.

            // In run-time deserialize registrations and register them as rule for unresolved services
            //=========================================================================================

            var assemblyLoaded = false;
            var lazyLoadedAssembly = new Lazy<Assembly>(() =>
            {
                assemblyLoaded = true;
                return assembly;
            });

            // Step 1 - Create Index for fast search by ExportInfo.ServiceTypeFullName.
            var registrationsByServiceTypeName = CollectRegistrationsByServiceTypeName(lazyRegistrations);

            Rules.DynamicRegistrationProvider dynamicRegistrations = (_, serviceType, serviceKey) =>
            {
                if (serviceType == typeof(CommandImporter))
                    return null; // todo: exclude with IfAlreadyRegistered when implemented

                List<KeyValuePair<object, ExportedRegistrationInfo>> serviceTypeRegistrations;
                if (!registrationsByServiceTypeName.TryGetValue(serviceType.FullName, out serviceTypeRegistrations))
                    return null;

                var factories = new List<KV<object, Factory>>();
                foreach (var r in serviceTypeRegistrations)
                    factories.Add(KV.Of<object, Factory>(r.Key, r.Value.CreateFactory(lazyLoadedAssembly)));

                return factories;
            };

            // Test that resolve works
            //========================
            var container = new Container().WithMef()
                .With(rules => rules.WithDynamicRegistrations(dynamicRegistrations));

            // make sure that CommandImporter itself is available without loading the lazy assembly
            container.RegisterExports(typeof(CommandImporter));

            // the same resolution code as in previous test
            //========================
            var cmds = container.Resolve<CommandImporter>();
            Assert.IsFalse(assemblyLoaded);

            Assert.IsNotNull(cmds.LazyHandler);
            Assert.IsNotNull(cmds.LazyHandler.Value);
            Assert.IsTrue(assemblyLoaded);

            Assert.IsNotNull(cmds.Commands);
            Assert.AreEqual(2, cmds.Commands.Count());
            Assert.AreEqual("Sample command, Another command", string.Join(", ", cmds.Commands.Select(c => c.Metadata.Name).OrderByDescending(c => c)));
        }

        public interface ICommandMetadata { string Name { get; } }

        [MetadataAttribute]
        public class CommandAttribute : Attribute, ICommandMetadata
        {
            public CommandAttribute(string name) { Name = name; }
            public string Name { get; }
        }

        public interface ICommand { }

        public interface IHandler<T> where T : class { }

        [Export(typeof(IHandler<object>))]
        public class ObjectHandler : IHandler<object> { }

        [Export(typeof(ICommand)), Command("Sample command")]
        public class SampleCommand : ICommand { }

        [Export(typeof(ICommand)), Command("Another command")]
        public class AnotherCommand : ICommand { }

        [Export]
        public class CommandImporter
        {
            [Import(AllowDefault = true)]
            public Lazy<IHandler<object>> LazyHandler { get; set; }

            [ImportMany]
            public IEnumerable<Lazy<ICommand, ICommandMetadata>> Commands { get; set; }
        }

        [Test]//, Ignore("fails to distinguish between imported Actions")]
        public void Lazy_import_of_Actions()
        {
            // the same registration code as in the lazy sample
            //========================
            var assembly = typeof(LazyRegistrationInfoStepByStep).Assembly;

            // Step 1 - Scan assembly and find exported type, create DTOs for them.
            var registrations = AttributedModel.Scan(new[] { assembly });

            // Step 2 - Make DTOs lazy.
            var serviceKeyStore = new ServiceKeyStore();
            var lazyRegistrations = registrations
                .Select(info => info.MakeLazy().EnsureUniqueExportServiceKeys(serviceKeyStore))
                .ToArray();  // this is required to materialized DTOs to be seriliazed.

            // In run-time deserialize registrations and register them as rule for unresolved services
            //=========================================================================================

            var assemblyLoaded = false;
            var lazyLoadedAssembly = new Lazy<Assembly>(() =>
            {
                assemblyLoaded = true;
                return assembly;
            });

            // Step 1 - Create Index for fast search by ExportInfo.ServiceTypeFullName.
            var registrationsByServiceTypeName = CollectRegistrationsByServiceTypeName(lazyRegistrations);

            Rules.DynamicRegistrationProvider dynamicRegistrations = (_, serviceType, serviceKey) =>
            {
                if (serviceType == typeof(ActionImporter))
                    return null;

                List<KeyValuePair<object, ExportedRegistrationInfo>> serviceTypeRegistrations;
                if (!registrationsByServiceTypeName.TryGetValue(serviceType.FullName, out serviceTypeRegistrations))
                    return null;

                var factories = new List<KV<object, Factory>>();
                foreach (var r in serviceTypeRegistrations)
                    factories.Add(KV.Of<object, Factory>(r.Key, r.Value.CreateFactory(lazyLoadedAssembly)));

                return factories;
            };

            // Test that resolve works fine with the non-lazy scenario
            //========================
            var cnt = new Container().WithMef();
            cnt.RegisterExports(typeof(ActionExporter), typeof(ActionImporter));

            // validate imported metadata
            var importer = cnt.Resolve<ActionImporter>();
            Assert.AreEqual(2, importer.Actions.Length);
            Assert.AreEqual("One, Two", string.Join(", ", importer.Actions.Select(a => a.Metadata["Name"].ToString()).OrderBy(n => n)));

            // validate imported actions
            var action1 = importer.Actions.First(m => m.Metadata["Name"].Equals("One")).Value;
            Assert.DoesNotThrow(() => action1());
            var action2 = importer.Actions.First(m => m.Metadata["Name"].Equals("Two")).Value;
            Assert.Throws<NotImplementedException>(() => action2());

            // Test that resolve works with the lazy scenario
            //========================
            var container = new Container().WithMef()
              .With(rules => rules.WithDynamicRegistrations(dynamicRegistrations));

            // make sure that ActionImporter itself is available without loading the lazy assembly
            container.RegisterExports(typeof(ActionImporter));
            importer = container.Resolve<ActionImporter>();
            Assert.IsFalse(assemblyLoaded);

            // validate imported metadata
            Assert.IsNotNull(importer.Actions);
            Assert.AreEqual(2, importer.Actions.Length);

            // todo: fails here with "One, One" instead of "One, Two"
            Assert.AreEqual("One, Two", string.Join(", ", importer.Actions.Select(a => a.Metadata["Name"].ToString()).OrderBy(n => n)));
            Assert.IsFalse(assemblyLoaded);

            // validate imported actions
            action1 = importer.Actions.First(m => m.Metadata["Name"].Equals("One")).Value;
            Assert.IsTrue(assemblyLoaded);
            Assert.DoesNotThrow(() => action1());

            action2 = importer.Actions.First(m => m.Metadata["Name"].Equals("Two")).Value;
            Assert.Throws<NotImplementedException>(() => action2());
        }

        [Export]
        public class ActionImporter
        {
            public const string ContractName = "ActionImporter";

            [ImportMany(ContractName)]
            public Lazy<Action, IDictionary<string, object>>[] Actions { get; set; }
        }

        public class ActionExporter
        {
            [Export(ActionImporter.ContractName), ExportMetadata("Name", "One")]
            public void Method1()
            {
            }

            [Export(ActionImporter.ContractName), ExportMetadata("Name", "Two")]
            public void Method2()
            {
                throw new NotImplementedException();
            }
        }
    }

    public interface IThing { }

    public interface IFrog {}

    [ExportMany]
    class Frog : IFrog { }
}
