/*
The MIT License (MIT)

Copyright (c) 2013 Maksim Volkau

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

namespace System.ComponentModel.Composition
{
    /// <summary>Specifies to register annotated type in container. 
    /// Or you could annotate Method with this attribute, and the method will be used as Factory Method for registration.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, 
        AllowMultiple = true, Inherited = false)]
    public class ExportAttribute : Attribute
    {
        /// <summary>Optional contract name to identify registered service when you are importing it.</summary>
        public string ContractName { get; private set; }

        /// <summary>Optional service type to register with, if omitted then annotated implementation type will be used as service type.
        /// If specified it should be assignable from annotated type.</summary>
        public Type ContractType { get; private set; }

        /// <summary>Creates default attribute without <see cref="ContractName"/> and with annotated type as <see cref="ContractType"/>.</summary>
        public ExportAttribute() { }

        /// <summary>Creates Export with specified <see cref="ContractType"/>, which should be assignable from annotated type.</summary>
        /// <param name="contractType">Contract type.</param>
        public ExportAttribute(Type contractType)
            : this(null, contractType) { }

        /// <summary>Creates Export with specified <see cref="ContractName"/> to identify service when imported.</summary>
        /// <param name="contractName">Contract name.</param>
        public ExportAttribute(string contractName)
            : this(contractName, null) { }

        /// <summary>Creates Export with both <see cref="ContractName"/> and <see cref="ContractType"/> specified.</summary>
        /// <param name="contractName"><see cref="ContractName"/></param>
        /// <param name="contractType"><see cref="ContractType"/></param>
        public ExportAttribute(string contractName, Type contractType)
        {
            ContractType = contractType;
            ContractName = contractName;
        }
    }

    /// <summary>Specifies that all types inherited from annotated type should be exported (see <see cref="ExportAttribute"/>) 
    /// with these settings.</summary>
    [AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    public class InheritedExportAttribute : ExportAttribute
    {
        /// <summary>Create default attribute without <see cref="ExportAttribute.ContractName"/> 
        /// and with annotated type as <see cref="ExportAttribute.ContractType"/>.</summary>
        public InheritedExportAttribute() { }

        /// <summary>Creates Export with specified <see cref="ExportAttribute.ContractType"/>, which should be assignable from annotated type.</summary>
        /// <param name="contractType">Contract type.</param>
        public InheritedExportAttribute(Type contractType)
            : base(contractType) { }

        /// <summary>Creates Export with specified <see cref="ExportAttribute.ContractName"/> to identify service when imported.</summary>
        /// <param name="contractName">Contract name.</param>
        public InheritedExportAttribute(string contractName)
            : base(contractName) { }

        /// <summary>Creates Export with both <see cref="ExportAttribute.ContractName"/> and <see cref="ExportAttribute.ContractType"/> specified.</summary>
        /// <param name="contractName"><see cref="ExportAttribute.ContractName"/></param>
        /// <param name="contractType"><see cref="ExportAttribute.ContractType"/></param>
        public InheritedExportAttribute(string contractName, Type contractType)
            : base(contractName, contractType) { }
    }

    /// <summary>Prevents annotated type from being exported, if its base type was marked with <see cref="InheritedExportAttribute"/>.</summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class PartNotDiscoverableAttribute : Attribute { }

    /// <summary>Specifies that exported type instance should be NonShared (created in each Import - aka Transient) 
    /// or Shared (created once and then reused in each Import - aka Singleton).</summary>
    public enum CreationPolicy
    {
        /// <summary>Default for compatibility with MEF .NET 4.5, in DryIoc means the same as <see cref="Shared"/>.</summary>
        Any,
        /// <summary>(default) Exported type instance will be created once and then reused in each Import - aka Singleton</summary>
        Shared,
        /// <summary>Exported type instance will be created in each Import - aka Transient.</summary>
        NonShared
    }

    /// <summary>Specifies <see cref="CreationPolicy"/> fro Exported type.</summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class PartCreationPolicyAttribute : Attribute
    {
        /// <summary>Creation policy, Shared by default.</summary>
        public CreationPolicy CreationPolicy { get; private set; }

        /// <summary>Creates attribute.</summary> <param name="policy"><see cref="CreationPolicy"/></param>
        public PartCreationPolicyAttribute(CreationPolicy policy)
        {
            CreationPolicy = policy;
        }
    }

    /// <summary>Specifies that annotated constructor should be used for creating Exported type.</summary>
    [AttributeUsage(AttributeTargets.Constructor)]
    public class ImportingConstructorAttribute : Attribute { }

    /// <summary>Specifies that for parameter, property or field should be injected with service instance identified by
    /// attribute settings. Import is not required for constructor parameters in Exported type: it is needed only
    /// if you want specify <see cref="ContractType"/> type different from parameter type.
    /// For properties and fields attribute is required, otherwise they won't be injected.</summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class ImportAttribute : Attribute
    {
        /// <summary>Optional, identifies Exported service with equal <see cref="ExportAttribute.ContractName"/> to be injected.</summary>
        public string ContractName { get; set; }

        /// <summary>Optional, identifies Exported service with equal <see cref="ExportAttribute.ContractType"/> to be injected.
        /// The specified type should be assignable to annotated parameter, property or field type.</summary>
        public Type ContractType { get; set; }

        /// <summary>Allow default value for the member if corresponding Exported service was found. 
        /// If not specified, then instead of default value Exception will be thrown.</summary>
        public bool AllowDefault { get; set; }

        /// <summary>Creates default attribute. Required for property or field to be imported. May be omitted for parameters.</summary>
        public ImportAttribute() { }

        /// <summary>Import with matched <see cref="ExportAttribute.ContractType"/>. Type should assignable to annotated member.</summary>
        /// <param name="contractType"></param>
        public ImportAttribute(Type contractType)
            : this(null, contractType)
        {
        }

        /// <summary>Import with matched <see cref="ExportAttribute.ContractName"/>.</summary>
        /// <param name="contractName"></param>
        public ImportAttribute(string contractName)
            : this(contractName, null)
        {
        }

        /// <summary>Import with both matched <see cref="ExportAttribute.ContractName"/> and <see cref="ExportAttribute.ContractType"/>.</summary>
        /// <param name="contractName"></param> <param name="contractType"></param>
        public ImportAttribute(string contractName, Type contractType)
        {
            ContractName = contractName;
            ContractType = contractType;
        }
    }

    /// <summary>Specifies that annotated attribute should be used as metadata object associated with Export.
    /// You can create your won custom Export attribute with metadata by deriving from <see cref="ExportAttribute"/> and
    /// annotating new attribute with <see cref="MetadataAttributeAttribute"/>.</summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MetadataAttributeAttribute : Attribute { }
}