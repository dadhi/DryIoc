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

namespace DryIoc.MefAttributedModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    
    /// <summary>Base attribute to specify type of reuse (implementing <see cref="IReuse"/>) for annotated class.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    public class ReuseAttribute : Attribute
    {
        /// <summary>Implementation of reuse. Could be null to specify transient or no reuse.</summary>
        public readonly Type ReuseType;

        /// <summary>Optional name, valid only for Current Scope Reuse.</summary>
        public readonly string ReuseName;

        /// <summary>Create attribute with specified type implementing reuse.</summary>
        /// <param name="reuseType">Could be null to specify transient or no reuse.</param>
        /// <param name="reuseName">(optional) Name is valid only for Current Scope Reuse and will be ignored by the rest of reuse types.</param>
        public ReuseAttribute(Type reuseType, string reuseName = null)
        {
            if (reuseType != null) (typeof(IReuse)).ThrowIfNotOf(reuseType);
            ReuseType = reuseType;
            ReuseName = reuseName;
        }
    }

    /// <summary>Defines the Transient reuse for exported service.</summary>
    public class TransientReuseAttribute : ReuseAttribute
    {
        /// <summary>Creates attribute by specifying null as <see cref="ReuseAttribute.ReuseType"/>.</summary>
        public TransientReuseAttribute() : base(null) { }
    }

    /// <summary>Denotes exported type with Singleton reuse.</summary>
    public class SingletonReuseAttribute : ReuseAttribute
    {
        /// <summary>Creates attribute.</summary>
        public SingletonReuseAttribute() : base(typeof(SingletonReuse)) { }
    }

    /// <summary>Denotes exported type with Current Scope Reuse.</summary>
    public class CurrentScopeReuseAttribute : ReuseAttribute
    {
        /// <summary>Creates attribute.</summary> <param name="reuseName">(optional)</param>
        public CurrentScopeReuseAttribute(string reuseName = null) : base(typeof(CurrentScopeReuse), reuseName) { }
    }

    /// <summary>Denotes exported type with Resolution Scope Reuse.</summary>
    public class ResolutionScopeReuseAttribute : ReuseAttribute
    {
        /// <summary>Creates attribute.</summary>
        public ResolutionScopeReuseAttribute() : base(typeof(ResolutionScopeReuse)) { }
    }

    /// <summary>Represents Reuse Wrappers defined for exported type.</summary>
    public class ReuseWrappersAttribute : Attribute
    {
        /// <summary>Reuse Wrapper types.</summary>
        public Type[] WrapperTypes { get; set; }

        /// <summary>Creates attribute.</summary> <param name="wrapperTypes">Reuse Wrapper types.</param>
        public ReuseWrappersAttribute(params Type[] wrapperTypes)
        {
            WrapperTypes = wrapperTypes;
        }
    }

    /// <summary>Defines export with arbitrary object key.</summary>
    [SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible"), AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class ExportWithKeyAttribute : ExportAttribute
    {
        /// <remarks>Specifies service key if <see cref="ExportAttribute.ContractName"/> is not specified.</remarks>
        public object ContractKey { get; set; }

        /// <summary>Creates attribute.</summary>
        /// <param name="contractKey">Service key object, should implement <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/></param> 
        /// <param name="contractType">Service type.</param>
        public ExportWithKeyAttribute(object contractKey, Type contractType)
            : base(contractType)
        {
            ContractKey = contractKey;
        }

        /// <summary>Creates attribute using implementation type as <see cref="ExportAttribute.ContractType"/></summary>
        /// <param name="contractKey">Service key object, should implement <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/></param> 
        public ExportWithKeyAttribute(object contractKey) : this(contractKey, null) { }
    }

    /// <summary>Specifies to export all implemented contract types automatically.</summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ExportManyAttribute : Attribute
    {
        /// <summary>Specifies service key if <see cref="ContractName"/> is not specified.</summary>
        public object ContractKey { get; set; }

        /// <summary>If specified has more priority over <see cref="ContractKey"/>.</summary>
        public string ContractName { get; set; }

        /// <summary>Excludes specified contract types.</summary>
        public Type[] Except { get; set; }

        /// <summary>Public types by default.</summary>
        public bool NonPublic { get; set; }

        /// <summary>Returns all contract types implemented by implementation type <see cref="Except"/> some.</summary>
        /// <param name="implementationType">To get contract types from.</param> <returns>Exported contract types.</returns>
        public IEnumerable<Type> GetContractTypes(Type implementationType)
        {
            var contractTypes = implementationType
                .GetImplementedTypes(ReflectionTools.IncludeImplementedType.SourceType);
            if (!NonPublic)
                contractTypes = contractTypes.Where(ReflectionTools.IsPublicOrNestedPublic).ToArray();
            return Except == null || Except.Length == 0 ? contractTypes : contractTypes.Except(Except);
        }
    }

    /// <summary>Specifies that class exporting static or instance method factories</summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class AsFactoryAttribute : Attribute { }

    /// <summary>Exports service as custom wrapper.</summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class AsWrapperAttribute : Attribute
    {
        /// <summary>For open-generic wrapper indicates wrapped argument type index.</summary>
        public int ContractTypeGenericArgIndex { get; set; }

        /// <summary>Explicitly defines wrapped type. If defined overrides <see cref="ContractTypeGenericArgIndex"/>.</summary>
        public Type WrappedContractType { get; set; }

        /// <summary>Creates attribute with <see cref="ContractTypeGenericArgIndex"/>.</summary>
        /// <param name="contractTypeGenericArgInsdex"></param>
        public AsWrapperAttribute(int contractTypeGenericArgInsdex = 0)
        {
            ContractTypeGenericArgIndex = contractTypeGenericArgInsdex.ThrowIf(contractTypeGenericArgInsdex < 0);
        }

        /// <summary>Creates attribute with <see cref="WrappedContractType"/>.</summary>
        /// <param name="wrappedContractType"></param>
        public AsWrapperAttribute(Type wrappedContractType)
        {
            WrappedContractType = wrappedContractType.ThrowIfNull();
        }
    }

    /// <summary>Specifies that exported service is decorator of services of <see cref="ExportAttribute.ContractType"/>.</summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class AsDecoratorAttribute : Attribute
    {
        /// <summary>If <see cref="ContractName"/> specified, it has more priority over <see cref="ContractKey"/>. </summary>
        public string ContractName { get; set; }

        /// <summary>Contract key of decorated type.</summary>
        public object ContractKey { get; set; }
    }

    /// <summary>Base type for exported type Setup Condition.</summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public abstract class ExportConditionAttribute : Attribute
    {
        /// <summary>Returns true to use exported service for request.</summary>
        /// <param name="request"></param> <returns>True to use exported service for request.</returns>
        public abstract bool Evaluate(Request request);
    }

    /// <summary>Imports service Only with equal <see cref="ContractKey"/>.</summary>
    [SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible"), AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
    public class ImportWithKeyAttribute : ImportAttribute
    {
        /// <summary>Arbitrary object to match with service key.</summary>
        public object ContractKey { get; set; }

        /// <summary>Creates attribute object service key.</summary> <param name="contractKey"></param>
        /// <param name="contractType">(optional) If missing then imported member type will be used as service type.</param>
        public ImportWithKeyAttribute(object contractKey, Type contractType = null)
            : base(contractType)
        {
            ContractKey = contractKey;
        }

        /// <summary>Creates attribute with string service name.</summary> <param name="contractKey"></param>
        /// <param name="contractType">(optional) If missing then imported member type will be used as service type.</param>
        public ImportWithKeyAttribute(string contractKey, Type contractType = null)
            : base(contractKey, contractType)
        {
            ContractKey = contractKey;
        }
    }

    /// <summary>Exports service with associated metadata object.</summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class | // for Export 
        AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    public class WithMetadataAttribute : Attribute
    {
        /// <summary>Metadata object</summary>
        public readonly object Metadata;

        /// <summary>Creates attribute</summary> <param name="metadata"></param>
        public WithMetadataAttribute(object metadata)
        {
            Metadata = metadata.ThrowIfNull();
        }
    }

    /// <summary>Indicate to import service and in case it is not registered, register it using provided
    /// implementation info. Useful for ad-hoc/quick-prototyping registration of types from not controlled libraries.</summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
    public class ImportExternalAttribute : Attribute
    {
        /// <summary>Implementation type of registered service.</summary>
        public Type ImplementationType { get; set; }

        /// <summary>Use specific constructor for registration.</summary>
        public Type[] ConstructorSignature { get; set; }

        /// <summary>Metadata associated with registration.</summary>
        public object Metadata { get; set; }

        /// <summary>Registering (and importing) with specified service key.</summary>
        public object ContractKey { get; set; }

        /// <summary>Registering (and importing) with specified service type.</summary>
        public Type ContractType { get; set; }

        /// <summary>Creates attributes.</summary>
        /// <param name="implementationType">(optional) Implementation type of registered service.</param>
        /// <param name="constructorSignature">(optional) Use specific constructor for registration.</param>
        /// <param name="metadata">(optional) Metadata associated with registration.</param>
        /// <param name="contractKey">(optional) Registering (and importing) with specified service key.</param>
        /// <param name="contractType">(optional) Registering (and importing) with specified service type.</param>
        public ImportExternalAttribute(Type implementationType = null, Type[] constructorSignature = null,
            object metadata = null, object contractKey = null, Type contractType = null)
        {
            ImplementationType = implementationType;
            ConstructorSignature = constructorSignature;
            Metadata = metadata;
            ContractType = contractType;
            ContractKey = contractKey;
        }
    }

    /// <summary>Specifies that exported service setup to <see cref="Setup.OpenResolutionScope"/>.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class OpenResolutionScopeAttribute : Attribute { }
}
