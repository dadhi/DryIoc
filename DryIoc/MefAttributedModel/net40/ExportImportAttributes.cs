namespace System.ComponentModel.Composition
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class ExportAttribute : Attribute
    {
        public string ContractName { get; private set; }
        public Type ContractType { get; private set; }

        public ExportAttribute() { }

        public ExportAttribute(Type contractType)
            : this(null, contractType) { }

        public ExportAttribute(string contractName)
            : this(contractName, null) { }

        public ExportAttribute(string contractName, Type contractType)
        {
            ContractType = contractType;
            ContractName = contractName;
        }
    }

    [AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
    public class InheritedExportAttribute : ExportAttribute
    {
        public InheritedExportAttribute() { }

        public InheritedExportAttribute(Type contractType)
            : base(contractType) { }

        public InheritedExportAttribute(string contractName)
            : base(contractName) { }

        public InheritedExportAttribute(string contractName, Type contractType)
            : base(contractName, contractType) { }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class PartNotDiscoverableAttribute : Attribute { }

    public enum CreationPolicy { Shared, NonShared }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class PartCreationPolicyAttribute : Attribute
    {
        public CreationPolicy CreationPolicy { get; private set; }

        public PartCreationPolicyAttribute(CreationPolicy policy)
        {
            CreationPolicy = policy;
        }
    }

    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    public class ImportingConstructorAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class ImportAttribute : Attribute
    {
        public string ContractName { get; private set; }

        public ImportAttribute() { }

        public ImportAttribute(string contractName)
        {
            ContractName = contractName;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class MetadataAttributeAttribute : Attribute { }
}