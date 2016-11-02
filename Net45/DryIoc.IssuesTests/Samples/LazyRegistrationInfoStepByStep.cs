using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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

                if (info.FactoryMethodInfo != null && info.FactoryMethodInfo.DeclaringType == null)
                    info.FactoryMethodInfo.DeclaringType = lazyLoadedAssembly.Value.GetType(info.FactoryMethodInfo.DeclaringTypeFullName);

                return info.CreateFactory();
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
            Assert.AreEqual(2, cmds.Commands.Length);
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

                    // multiple services workaround: generate missing service keys
                    var serviceKey = export.ServiceKey;
                    if (serviceKey == null)
                        serviceKey = Guid.NewGuid().ToString();

                    regs.Add(new KeyValuePair<object, ExportedRegistrationInfo>(serviceKey, lazyRegistration));
                }
            }

            // Step 2 - Add resolution rule for creating factory on resolve.
            Rules.UnknownServiceResolver createFactoryFromAssembly = request =>
            {
                List<KeyValuePair<object, ExportedRegistrationInfo>> regs;
                if (!regInfoByServiceTypeNameIndex.TryGetValue(request.ServiceType.FullName, out regs))
                    return null;

                var regIndex = regs.FindIndex(pair => request.ServiceKey == null || Equals(pair.Key, request.ServiceKey));
                if (regIndex == -1)
                    return null;

                var info = regs[regIndex].Value;
                if (info.ImplementationType == null)
                    info.ImplementationType = lazyLoadedAssembly.Value.GetType(info.ImplementationTypeFullName);

                if (info.FactoryMethodInfo != null && info.FactoryMethodInfo.DeclaringType == null)
                    info.FactoryMethodInfo.DeclaringType = lazyLoadedAssembly.Value.GetType(info.FactoryMethodInfo.DeclaringTypeFullName);

                return info.CreateFactory();
            };

            // Step 3 - Add service type handler for resolving many factories.
            Rules.UnknownServiceHandler createFactoriesFromAssembly = serviceType =>
            {
                List<KeyValuePair<object, ExportedRegistrationInfo>> regs;
                if (!regInfoByServiceTypeNameIndex.TryGetValue(serviceType.FullName, out regs))
                    return null;

                var factories = new List<KV<object, Factory>>();
                foreach (var pair in regs)
                {
                    if (pair.Value.ImplementationType == null)
                        pair.Value.ImplementationType = lazyLoadedAssembly.Value.GetType(pair.Value.ImplementationTypeFullName);

                    if (pair.Value.FactoryMethodInfo != null && pair.Value.FactoryMethodInfo.DeclaringType == null)
                        pair.Value.FactoryMethodInfo.DeclaringType = lazyLoadedAssembly.Value.GetType(pair.Value.ImplementationTypeFullName);

                    factories.Add(new KV<object, Factory>(pair.Key, pair.Value.CreateFactory()));
                }

                return factories;
            };

            // Test that resolve works
            //========================
            var container = new Container().WithMef()
                .With(rules => rules.WithUnknownServiceResolvers(createFactoryFromAssembly))
                .With(rules => rules.WithUnknownServiceHandlers(createFactoriesFromAssembly));

            // the same resolution code as in previous test
            //========================
            var cmds = container.Resolve<CommandImporter>();
            Assert.IsNotNull(cmds.Commands);
            Assert.AreEqual(2, cmds.Commands.Length);
            Assert.AreEqual("Sample command, Another command", string.Join(", ", cmds.Commands.Select(c => c.Metadata.Name).OrderByDescending(c => c)));
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

            var lazyLoadedAssembly = new Lazy<Assembly>(() => assembly);

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

                    // multiple services workaround: generate missing service keys
                    var serviceKey = export.ServiceKey;
                    if (serviceKey == null)
                        serviceKey = Guid.NewGuid().ToString();

                    regs.Add(new KeyValuePair<object, ExportedRegistrationInfo>(serviceKey, lazyRegistration));
                }
            }

            // Step 2 - Add resolution rule for creating factory on resolve.
            Rules.UnknownServiceResolver createFactoryFromAssembly = request =>
            {
                List<KeyValuePair<object, ExportedRegistrationInfo>> regs;
                if (!regInfoByServiceTypeNameIndex.TryGetValue(request.ServiceType.FullName, out regs))
                    return null;

                var regIndex = regs.FindIndex(pair => request.ServiceKey == null || Equals(pair.Key, request.ServiceKey));
                if (regIndex == -1)
                    return null;

                var info = regs[regIndex].Value;
                var lazyFactory = new Lazy<ReflectionFactory>(() =>
                {
                    if (info.ImplementationType == null)
                        info.ImplementationType = lazyLoadedAssembly.Value.GetType(info.ImplementationTypeFullName);

                    if (info.FactoryMethodInfo != null && info.FactoryMethodInfo.DeclaringType == null)
                        info.FactoryMethodInfo.DeclaringType = lazyLoadedAssembly.Value.GetType(info.FactoryMethodInfo.DeclaringTypeFullName);

                    return info.CreateFactory();
                });

                return new LazyReflectionFactory(lazyFactory);
            };

            // Step 3 - Add service type handler for resolving many factories.
            Rules.UnknownServiceHandler createFactoriesFromAssembly = serviceType =>
            {
                List<KeyValuePair<object, ExportedRegistrationInfo>> regs;
                if (!regInfoByServiceTypeNameIndex.TryGetValue(serviceType.FullName, out regs))
                    return null;

                var factories = new List<KV<object, Factory>>();
                foreach (var pair in regs)
                {
                    var lazyFactory = new Lazy<ReflectionFactory>(() =>
                    {
                        if (pair.Value.ImplementationType == null)
                            pair.Value.ImplementationType = lazyLoadedAssembly.Value.GetType(pair.Value.ImplementationTypeFullName);

                        if (pair.Value.FactoryMethodInfo != null && pair.Value.FactoryMethodInfo.DeclaringType == null)
                            pair.Value.FactoryMethodInfo.DeclaringType = lazyLoadedAssembly.Value.GetType(pair.Value.ImplementationTypeFullName);

                        return pair.Value.CreateFactory();
                    });

                    factories.Add(new KV<object, Factory>(pair.Key, new LazyReflectionFactory(lazyFactory)));
                }

                return factories;
            };

            // Test that resolve works
            //========================
            var container = new Container().WithMef()
                .With(rules => rules.WithUnknownServiceResolvers(createFactoryFromAssembly))
                .With(rules => rules.WithUnknownServiceHandlers(createFactoriesFromAssembly));

            // the same resolution code as in previous test
            //========================
            var cmds = container.Resolve<CommandImporter>();
            Assert.IsNotNull(cmds.LazyHandler);
            Assert.IsNotNull(cmds.LazyHandler.Value);

            Assert.IsNotNull(cmds.Commands);
            Assert.AreEqual(2, cmds.Commands.Length);
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
            public Lazy<ICommand, ICommandMetadata>[] Commands { get; set; }
        }
    }

    public interface IThing { }

    public interface IFrog {}

    [ExportMany]
    class Frog : IFrog { }
}
