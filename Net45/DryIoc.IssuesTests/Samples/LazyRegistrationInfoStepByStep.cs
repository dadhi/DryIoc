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

            var assembly = Assembly.LoadFrom(assemblyFile);
            var registrations = AttributedModel.Scan(new[] { assembly });

            // create serializable registrations
            var lazyRegistrations = registrations.MakeLazyAndEnsureUniqueServiceKeys();

            // load the registrations and provide a way dynamically register them in container
            var dynamicRegistrations = lazyRegistrations.GetLazyTypeRegistrationProvider(() => assembly);

            var container = new Container().WithMef()
                .With(rules => rules.WithDynamicRegistrations(dynamicRegistrations));

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
            var assembly = typeof(LazyRegistrationInfoStepByStep).Assembly;
            var registrations = AttributedModel.Scan(new[] { assembly });
            var lazyRegistrations = registrations.MakeLazyAndEnsureUniqueServiceKeys();
            var dynamicRegistrations = lazyRegistrations.GetLazyTypeRegistrationProvider(() => assembly);

            var container = new Container().WithMef()
                .With(rules => rules.WithDynamicRegistrations(dynamicRegistrations));

            var cmds = container.Resolve<CommandImporter>();

            Assert.IsNotNull(cmds.Commands);
            Assert.AreEqual(2, cmds.Commands.Count());
            Assert.AreEqual("Sample command, Another command",
                string.Join(", ", cmds.Commands.Select(c => c.Metadata.Name).OrderByDescending(c => c)));
        }

        [Test]
        public void Lazy_import_of_commands_using_LazyFactory()
        {
            var assembly = typeof(LazyRegistrationInfoStepByStep).Assembly;
            var registrations = AttributedModel.Scan(new[] { assembly });
            var lazyRegistrations = registrations.MakeLazyAndEnsureUniqueServiceKeys();

            var assemblyLoaded = false;
            var dynamicRegistrations = lazyRegistrations.GetLazyTypeRegistrationProvider(() =>
            {
                assemblyLoaded = true;
                return assembly;
            });

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

        [Test]
        public void Lazy_import_of_commands_using_custom_DynamicRegistrationProvider()
        {
            var assembly = typeof(LazyRegistrationInfoStepByStep).Assembly;
            var registrations = AttributedModel.Scan(new[] { assembly });
            var lazyRegistrations = registrations.MakeLazyAndEnsureUniqueServiceKeys();

            var assemblyLoaded = false;
            Func<string, Type> typeProvider = typeName =>
            {
                assemblyLoaded = true;
                return assembly.GetType(typeName);
            };

            var commandRegistrations = lazyRegistrations
                .Where(r => r.ImplementationTypeFullName.EndsWith("Command"))
                .Select(r => new DynamicRegistration(r.CreateFactory(typeProvider)))
                .ToArray();

            Rules.DynamicRegistrationProvider getDynamicRegistrations = (type, key) =>
            {
                if (type == typeof(ICommand))
                {
                    return commandRegistrations;
                }

                return null;
            };

            // Test that resolve works
            //========================
            var container = new Container().WithMef()
                .With(rules => rules.WithDynamicRegistrations(getDynamicRegistrations));

            // make sure that CommandImporter itself is available without loading the lazy assembly
            container.RegisterExports(typeof(CommandImporter));
            container.RegisterExports(typeof(ObjectHandler));

            // the same resolution code as in previous test
            //========================
            var cmds = container.Resolve<CommandImporter>();
            Assert.IsFalse(assemblyLoaded);

            Assert.IsNotNull(cmds.LazyHandler);
            Assert.IsNotNull(cmds.LazyHandler.Value);
            Assert.IsFalse(assemblyLoaded);

            Assert.IsNotNull(cmds.Commands);
            Assert.AreEqual(2, cmds.Commands.Count());
            Assert.AreEqual("Sample command, Another command", string.Join(", ", cmds.Commands.Select(c => c.Metadata.Name).OrderByDescending(c => c)));
            Assert.IsFalse(assemblyLoaded);

            var command = cmds.Commands.First().Value;
            Assert.IsNotNull(command);
            Assert.IsTrue(assemblyLoaded);
        }

        [Test]
        public void Lazy_import_of_commands_using_multiple_dynamic_registrations_of_the_same_service()
        {
            var assembly = typeof(LazyRegistrationInfoStepByStep).Assembly;
            var registrations = AttributedModel.Scan(new[] { assembly });
            var lazyRegistrations = registrations.MakeLazyAndEnsureUniqueServiceKeys();
            var assemblyLoaded = false;

            // use shared service exports to compose multiple providers
            var serviceExports = new Dictionary<string, IList<KeyValuePair<object, ExportedRegistrationInfo>>>();

            // create a separate DynamicRegistrationProvider for each lazy registration
            // to simulate that each ICommand is located in a different assembly
            var dynamicRegistrations = lazyRegistrations
                .Select(r => new[] { r }
                .GetLazyTypeRegistrationProvider(
                    otherServiceExports: serviceExports,
                    typeProvider: t =>
                    {
                        assemblyLoaded = true;
                        return assembly.GetType(t);
                    }))
                .ToArray();

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
            Assert.AreEqual(2, cmds.Commands.Count()); // fails: only one command is imported
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

        [Test, Ignore("Fails with StackOverflowException at line #302")]
        public void Lazy_import_should_detect_circular_dependencies()
        {
            // ordinary registration
            var nonLazyContainer = new Container().WithMef();
            nonLazyContainer.RegisterExports(new[] { typeof(LazyRegistrationInfoStepByStep).Assembly });

            // check that importing as non-lazy actually detects the circular dependency
            Assert.Throws<ContainerException>(() =>
            {
              var cmds = nonLazyContainer.Resolve<CircularDependencyRoot>();
              Assert.IsNotNull(cmds.Service);
            });

            // register dynamically
            var assembly = typeof(LazyRegistrationInfoStepByStep).Assembly;
            var registrations = AttributedModel.Scan(new[] { assembly });
            var lazyRegistrations = registrations.MakeLazyAndEnsureUniqueServiceKeys();

            // use shared service exports to compose multiple providers
            var serviceExports = new Dictionary<string, IList<KeyValuePair<object, ExportedRegistrationInfo>>>();

            // create a separate DynamicRegistrationProvider for each lazy registration
            // to simulate that each ICommand is located in a different assembly
            var dynamicRegistrations = lazyRegistrations
                .Select(r => new[] { r }
                .GetLazyTypeRegistrationProvider(
                    otherServiceExports: serviceExports,
                    typeProvider: t =>
                    {
                        return assembly.GetType(t);
                    }))
                .ToArray();

            // Test that dynamic resolution also detects the circular dependency
            //==================================================================
            var container = new Container().WithMef()
                .With(rules => rules.WithDynamicRegistrations(dynamicRegistrations));

            // make sure that CircularDependencyRoot itself is available without loading the lazy assembly
            container.RegisterExports(typeof(CircularDependencyRoot));
            Assert.Throws<ContainerException>(() =>
            {
                container.Resolve<CircularDependencyRoot>();
            });
        }

        [Export]
        public class CircularDependencyRoot
        {
            [Import]
            public IFirstLevelDependency Service { get; set; }
        }

        public interface IFirstLevelDependency { }

        [Export(typeof(IFirstLevelDependency))]
        public class FirstLevelDependency : IFirstLevelDependency
        {
            [Import]
            public ISecondLevelDependency Service { get; set; }
        }

        public interface ISecondLevelDependency { }

        [Export(typeof(ISecondLevelDependency))]
        public class SecondLevelDependency : ISecondLevelDependency
        {
            [Import]
            public IFirstLevelDependency Service { get; set; }
        }

        [Test]
        public void Lazy_import_of_Actions()
        {
            var assembly = typeof(LazyRegistrationInfoStepByStep).Assembly;
            var registrations = AttributedModel.Scan(new[] { assembly });
            var lazyRegistrations = registrations.MakeLazyAndEnsureUniqueServiceKeys();

            var assemblyLoaded = false;
            var dynamicRegistrations = lazyRegistrations.GetLazyTypeRegistrationProvider(() =>
            {
                assemblyLoaded = true;
                return assembly;
            });

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

    public interface IFrog { }

    [ExportMany]
    class Frog : IFrog { }
}
