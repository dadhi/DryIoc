using System;

namespace DryIoc.AttributedRegistration
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ExportAttribute : Attribute
    {
        public string ContractName { get; set; }
        public Type ContractType { get; set; }

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

    public enum CreationPolicy { Shared, NonShared }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class PartCreationPolicyAttribute : Attribute
    {
        public CreationPolicy CreationPolicy { get; set; }

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
        public ImportAttribute() { }

        public ImportAttribute(string contractName)
        {
            ContractName = contractName;
        }

        public string ContractName { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class MetadataAttributeAttribute : Attribute { }
}