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

    [AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
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

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class PartNotDiscoverableAttribute : Attribute { }

    public enum CreationPolicy { Shared, NonShared }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class PartCreationPolicyAttribute : Attribute
    {
        public CreationPolicy CreationPolicy { get; private set; }

        public PartCreationPolicyAttribute(CreationPolicy policy)
        {
            CreationPolicy = policy;
        }
    }

    [AttributeUsage(AttributeTargets.Constructor)]
    public class ImportingConstructorAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class ImportAttribute : Attribute
    {
        public string ContractName { get; set; }
        public Type ContractType { get; set; }
        public bool AllowDefault { get; set; }

        public ImportAttribute() { }

        public ImportAttribute(Type contractType)
            : this(null, contractType)
        {
        }

        public ImportAttribute(string contractName)
            : this(contractName, null)
        {
        }

        public ImportAttribute(string contractName, Type contractType)
        {
            ContractName = contractName;
            ContractType = contractType;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class MetadataAttributeAttribute : Attribute { }
}