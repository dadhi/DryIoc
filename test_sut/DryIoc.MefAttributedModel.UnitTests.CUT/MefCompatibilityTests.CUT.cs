using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using DryIocAttributes;

namespace DryIoc.MefAttributedModel.UnitTests.CUT
{
    public interface ILogTableManager
    {
        string TableName { get; }
    }

    // no Export attribute here
    public class LogTableManager : ILogTableManager
    {
        public const string FactoryMethodExportName = "LogTableManagerFactory";

        // constructor is private,
        // service is exposed via its factory method
        private LogTableManager(string schemaName)
        {
            TableName = $"{schemaName}.LOG_ENTRIES";
        }

        public string TableName { get; private set; }

        [Export]
        [Export(FactoryMethodExportName)]
        public static ILogTableManager Create(string schemaName)
        {
            return new LogTableManager(schemaName);
        }
    }

    [Export]
    public class LogTableManagerConsumer1
    {
        [Import]
        private Func<string, ILogTableManager> GetLogTableManager { get; set; }

        private ILogTableManager logTableManager;

        public ILogTableManager LogTableManager
        {
            get
            {
                return logTableManager ?? (logTableManager = GetLogTableManager("SCHEMA1"));
            }
        }
    }

    [Export]
    public class LogTableManagerConsumer2
    {
        [Import(CUT.LogTableManager.FactoryMethodExportName)]
        private Func<string, ILogTableManager> GetLogTableManager { get; set; }

        private ILogTableManager logTableManager;

        public ILogTableManager LogTableManager
        {
            get
            {
                return logTableManager ?? (logTableManager = GetLogTableManager("SCHEMA2"));
            }
        }
    }

    // no Export attribute here
    public class Constants
    {
        public const string SettingExportKey = "ExportedSetting";

        [Export(SettingExportKey)]
        private string ExportedValue => "Constants.ExportedValue";
    }

    public class Abc { }

    public class Provider
    {
        [Export]
        private Abc ExportedValue => new Abc();
    }


    [Export]
    public class SettingImportHelper
    {
        [ImportMany(Constants.SettingExportKey)]
        public string[] ImportedValues { get; private set; }
    }

    // no Export attribute here
    internal class SettingProvider1
    {
        [Export(Constants.SettingExportKey)]
        private string ExportedValue { get; } = "SettingProvider1.ExportedValue";
    }

    // no Export attribute here
    internal class SettingProvider2
    {
        [Export(Constants.SettingExportKey)]
        protected string ExportedValue { get; } = "SettingProvider2.ExportedValue";
    }

    // no Export attribute here
    internal class SettingProvider3
    {
        [Export(Constants.SettingExportKey)]
        public string ExportedValue { get; private set; } = "SettingProvider3.ExportedValue";
    }

    public interface IVersionedProtocol { string Version { get; } }

    internal class GenericProtocol : IVersionedProtocol
    {
        public GenericProtocol(string version) { Version = version; }
        public string Version { get; }
    }

    internal class ExportEarlyProtocolVersions
    {
        [Export] public IVersionedProtocol V1 => new GenericProtocol("1.0");
        [Export] public IVersionedProtocol V2 => new GenericProtocol("2.0");
        [Export] public IVersionedProtocol V3 => new GenericProtocol("3.0");
    }

    [Export(typeof(IVersionedProtocol))]
    internal class ModernProtocolImplementation : IVersionedProtocol { public string Version => "4.0"; }

    [Export]
    public class ImportAllProtocolVersions
    {
        [ImportMany(typeof(IVersionedProtocol))]
        public IVersionedProtocol[] Protocols { get; set; }
    }

    public interface IUntypedService { string Version { get; } }

    [Export("ArbitraryKey")]
    public class UntypedService : IUntypedService
    {
        public string Version { get { return "123.4567"; } }
    }

    //[Export("ArbitraryKey")]
    //public class AnotherUntypedService : IUntypedService
    //{
    //    public string Version { get { return "42.31415"; } }
    //}

    [Export]
    public class ImportUntypedService
    {
        [Import("ArbitraryKey")]
        public object UntypedService { get; set; }
    }

    [Export]
    public class ImportManyUntypedServices
    {
        [ImportMany("ArbitraryKey")]
        public object[] UntypedServices { get; set; }
    }

    public interface IDisposableScopedService : IDisposable { bool IsDisposed { get; } }

    [Export(typeof(IDisposableScopedService))]
    internal class MyScopedService : IDisposableScopedService
    {
        public void Dispose() { IsDisposed = true; }

        public bool IsDisposed { get; private set; }
    }

    public interface IDisposableSingletonService : IDisposable { bool IsDisposed { get; } }

    [Export(typeof(IDisposableSingletonService)), PartCreationPolicy(CreationPolicy.Shared)]
    internal class MySingletonService : IDisposableSingletonService
    {
        public void Dispose() { IsDisposed = true; }

        public bool IsDisposed { get; private set; }
    }

    public interface IServiceWithTwoConstructors
    {
        bool DefaultConstructorIsUsed { get; }
    }

    [Export(typeof(IServiceWithTwoConstructors))]
    internal class ServiceWithTwoConstructors : IServiceWithTwoConstructors
    {
        public ServiceWithTwoConstructors()
        {
            DefaultConstructorIsUsed = true;
        }

        public ServiceWithTwoConstructors(string name) { }

        public bool DefaultConstructorIsUsed { get; private set; }
    }

    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class NonSharedDependency : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    [Export, PartCreationPolicy(CreationPolicy.Shared)]
    public class SharedDependency : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class NonSharedService : IDisposable
    {
        [Import]
        public NonSharedDependency NonSharedDependency { get; set; }

        [Import]
        public SharedDependency SharedDependency { get; set; }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    [Export]
    public class UsesExportFactoryOfNonSharedService
    {
        [Import]
        public ExportFactory<NonSharedService> Factory { get; set; }
    }

    [Export, PartCreationPolicy(CreationPolicy.Shared)]
    public class SharedService : IDisposable
    {
        [Import]
        public NonSharedDependency NonSharedDependency { get; set; }

        [Import]
        public SharedDependency SharedDependency { get; set; }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    [Export]
    public class UsesExportFactoryOfSharedService
    {
        [Import]
        public ExportFactory<SharedService> Factory { get; set; }
    }

    [Export]
    public class UnspecifiedCreationPolicyService : IDisposable
    {
        [Import]
        public NonSharedDependency NonSharedDependency { get; set; }

        [Import]
        public SharedDependency SharedDependency { get; set; }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    [Export]
    public class UsesExportFactoryOfUnspecifiedCreationPolicyService
    {
        [Import]
        public ExportFactory<UnspecifiedCreationPolicyService> Factory { get; set; }
    }

    public interface ILazyMetadata { string Name { get; } }

    [MetadataAttribute, AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class LazyMetadataAttribute : Attribute, ILazyMetadata
    {
        public LazyMetadataAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public interface ILazyNamedService { bool IsDisposed { get; } }

    [Export(typeof(ILazyNamedService)), LazyMetadata("One")]
    public class LazyNamedService1 : ILazyNamedService, IDisposable
    {
        public void Dispose() { IsDisposed = true; }
        public bool IsDisposed { get; private set; }
    }

    [Export(typeof(ILazyNamedService)), LazyMetadata("Two"), PartCreationPolicy(CreationPolicy.NonShared)]
    public class LazyNamedService2 : ILazyNamedService, IDisposable
    {
        [Import]
        public NonSharedDependency NonSharedDependency { get; set; }

        public void Dispose() { IsDisposed = true; }
        public bool IsDisposed { get; private set; }
    }

    [Export]
    public class ImportLazyNamedServices
    {
        [ImportMany]
        public IEnumerable<Lazy<ILazyNamedService, ILazyMetadata>> LazyNamedServices { get; set; }
    }

    [Export]
    public class ImportsNamedServiceExportFactories
    {
        [ImportMany]
        public IEnumerable<ExportFactory<ILazyNamedService, ILazyMetadata>> NamedServiceFactories { get; set; }
    }

    public interface INonExistingService { }

    [Export]
    public class NonExistingServiceOptionalImports
    {
        [Import(AllowDefault = true)]
        public INonExistingService NonExistingService { get; set; }

        [Import(AllowDefault = true)]
        public Lazy<INonExistingService> LazyNonExistingService { get; set; }

        [Import(AllowDefault = true)]
        public ExportFactory<INonExistingService> NonExistingServiceFactory { get; set; }

        [Import(AllowDefault = true)]
        public Lazy<INonExistingService, ILazyMetadata> LazyNonExistingServiceWithMetadata { get; set; }

        [Import(AllowDefault = true)]
        public ExportFactory<INonExistingService, ILazyMetadata> NonExistingServiceFactoryWithMetadata { get; set; }
    }

    [Export]
    public class NonExistingServiceRequiredImport
    {
        [Import(AllowDefault = false)]
        public INonExistingService NonExistingService { get; set; }
    }

    [Export]
    public class NonExistingServiceRequiredLazyImport
    {
        [Import(AllowDefault = false)]
        public Lazy<INonExistingService> NonExistingService { get; set; }
    }

    [Export]
    public class NonExistingServiceRequiredExportFactoryImport
    {
        [Import(AllowDefault = false)]
        public ExportFactory<INonExistingService> NonExistingService { get; set; }
    }

    [Export]
    public class NonExistingServiceRequiredLazyWithMetadataImport
    {
        [Import(AllowDefault = false)]
        public Lazy<INonExistingService, ILazyMetadata> NonExistingService { get; set; }
    }

    [Export]
    public class NonExistingServiceRequiredExportFactoryWithMetadataImport
    {
        [Import(AllowDefault = false)]
        public ExportFactory<INonExistingService, ILazyMetadata> NonExistingService { get; set; }
    }

    public interface IScriptMetadata { long ScriptID { get; } }

    public interface IScriptWithCategoryMetadata : IScriptMetadata { string CategoryName { get; } }

    [MetadataAttribute, AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ScriptMetadataAttribute : Attribute, IScriptMetadata
    {
        public ScriptMetadataAttribute(long scriptId)
        {
            ScriptID = scriptId;
        }

        public long ScriptID { get; }
    }

    [MetadataAttribute, AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ScriptWithCategoryMetadataAttribute : ScriptMetadataAttribute, IScriptWithCategoryMetadata
    {
        public ScriptWithCategoryMetadataAttribute(long scriptId, string categoryName)
            : base(scriptId)
        {
            CategoryName = categoryName;
        }

        public string CategoryName { get; }
    }

    [Export, LazyMetadata("MultipleMetadata"), ScriptMetadata(123)]
    public class MultipleMetadataAttributes
    {
    }

    [Export]
    public class ImportStuffWithMultipleMetadataAttributes
    {
        [ImportMany]
        public Lazy<MultipleMetadataAttributes, IScriptMetadata>[] Scripts { get; set; }

        [ImportMany]
        public Lazy<MultipleMetadataAttributes, ILazyMetadata>[] NamedServices { get; set; }
    }

    [Export, ScriptWithCategoryMetadata(123, "Category"), WithMetadata("DryIocMetadata", "AlsoSupported")]
    public class InheritedMetadataAttributes
    {
    }

    [Export]
    public class ImportUntypedInheritedMetadata
    {
        [ImportMany]
        public Lazy<InheritedMetadataAttributes, IDictionary<string, object>>[] UntypedMetadataServices { get; set; }

        [ImportMany]
        public Lazy<InheritedMetadataAttributes, IScriptWithCategoryMetadata>[] TypedMetadataServices { get; set; }
    }

    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class NonSharedWithImportSatisfiedNotification : IPartImportsSatisfiedNotification
    {
        public int ImportsSatisfied { get; private set; }

        public void OnImportsSatisfied()
        {
            ImportsSatisfied++;
        }
    }

    [Export, PartCreationPolicy(CreationPolicy.Shared)]
    public class SharedWithImportSatisfiedNotification : IPartImportsSatisfiedNotification
    {
        public int ImportsSatisfied { get; private set; }

        public void OnImportsSatisfied()
        {
            ImportsSatisfied++;
        }
    }

    public class MemberExportWithMetadataExample
    {
        [Export("UnitTestExample"), ExportMetadata("Title", "Sample")]
        public static void TestMethodExample()
        {
        }
    }

    [Export]
    public class UsesMemberExportWithMetadataExample
    {
        [Import("UnitTestExample")]
        public Lazy<Action, IDictionary<string, object>> ImportedTestMethodExample { get; set; }
    }
}
