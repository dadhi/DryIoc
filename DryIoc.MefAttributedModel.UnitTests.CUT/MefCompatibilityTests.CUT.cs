using System;
using System.ComponentModel.Composition;

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
}
