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

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace DryIocAttributes
{
    /// <summary>List of supported DryIoc reuse types.</summary>
    public enum ReuseType
    {
        /// <summary>Means no reuse.</summary>
        Transient, 
        /// <summary>subj.</summary>
        Singleton,
        /// <summary>subj.</summary>
        CurrentScope,
        /// <summary>subj.</summary>
        ResolutionScope
    }

    /// <summary>Base attribute to specify type of reuse for annotated class.</summary>
    [AttributeUsage(AttributeTargets.Class 
        | AttributeTargets.Method 
        | AttributeTargets.Field 
        | AttributeTargets.Property 
        | AttributeTargets.Parameter, 
        Inherited = false)]
    public class ReuseAttribute : Attribute
    {
        /// <summary>Implementation of reuse. Could be null to specify transient or no reuse.</summary>
        public readonly ReuseType ReuseType;

        /// <summary>Optional name, valid only for Current Scope Reuse.</summary>
        public readonly string ScopeName;

        /// <summary>Create attribute with specified type implementing reuse.</summary>
        /// <param name="reuseType">Supported reuse type.</param>
        /// <param name="scopeName">(optional) Name is valid only for Current Scope Reuse and will be ignored by the rest of reuse types.</param>
        public ReuseAttribute(ReuseType reuseType, string scopeName = null)
        {
            ReuseType = reuseType;
            ScopeName = scopeName;
        }
    }

    /// <summary>Defines the Transient reuse for exported service.</summary>
    public class TransientReuseAttribute : ReuseAttribute
    {
        /// <summary>Creates attribute by specifying null as <see cref="ReuseAttribute.ReuseType"/>.</summary>
        public TransientReuseAttribute() : base(ReuseType.Transient) { }
    }

    /// <summary>Denotes exported type with Singleton reuse.</summary>
    public class SingletonReuseAttribute : ReuseAttribute
    {
        /// <summary>Creates attribute.</summary>
        public SingletonReuseAttribute() : base(ReuseType.Singleton) { }
    }

    /// <summary>Denotes exported type with Current Scope Reuse.</summary>
    public class CurrentScopeReuseAttribute : ReuseAttribute
    {
        /// <summary>Creates attribute.</summary> <param name="scopeName">(optional)</param>
        public CurrentScopeReuseAttribute(string scopeName = null) : base(ReuseType.CurrentScope, scopeName) { }
    }

    /// <summary>Marks exported type with Reuse.InWebRequest. 
    /// Basically it is CurrentScopeReuse with predefined name Reuse.WebRequestScopeName.</summary>
    public class WebRequestReuseAttribute : CurrentScopeReuseAttribute
    {
        /// <summary>Default web reuse scope name. Just a convention supported by DryIoc.</summary>
        public static readonly string WebRequestScopeName = "WebRequestScopeName";

        /// <summary>Creates attribute.</summary>
        public WebRequestReuseAttribute() : base(WebRequestScopeName) { }
    }

    /// <summary>Marks exported type with Reuse.InThread. 
    /// Basically it is CurrentScopeReuse with predefined name ThreadScopeContext.ScopeContextName.</summary>
    public class ThreadReuseAttribute : CurrentScopeReuseAttribute
    {
        /// <summary>Name for root scope in thread context. Just a convention supported by DryIoc.</summary>
        public static readonly string ScopeContextName = "ThreadScopeContext";

        /// <summary>Creates attribute.</summary>
        public ThreadReuseAttribute() : base(ScopeContextName) { }
    }

    /// <summary>Denotes exported type with Resolution Scope Reuse.</summary>
    public class ResolutionScopeReuseAttribute : ReuseAttribute
    {
        /// <summary>Creates attribute.</summary>
        public ResolutionScopeReuseAttribute() : base(ReuseType.ResolutionScope) { }
    }

    /// <summary>Marks exported reused service to be stored as WeakReference</summary>
    public class WeaklyReferencedAttribute : Attribute {}

    /// <summary>Marks exported reused service to be Not disposed together with scope.</summary>
    public class PreventDisposalAttribute : Attribute { }

    /// <summary>Allows disposable transient to be exported.</summary>
    public class AllowsDisposableTransientAttribute : Attribute { }

    /// <summary>Defines export with arbitrary object key.</summary>
    [SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible",
        Justification = "Not available in PCL.")]
    [AttributeUsage(AttributeTargets.Class 
        | AttributeTargets.Method 
        | AttributeTargets.Property 
        | AttributeTargets.Field, 
        AllowMultiple = true, Inherited = false)]
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
    [AttributeUsage(AttributeTargets.Class 
        | AttributeTargets.Method 
        | AttributeTargets.Property 
        | AttributeTargets.Field, 
        Inherited = false)]
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
    }

    /// <summary>Specifies that class exporting static or instance method factories</summary>
    [AttributeUsage(AttributeTargets.Class 
        | AttributeTargets.Method 
        | AttributeTargets.Property 
        | AttributeTargets.Field, 
        Inherited = false)]
    public class AsFactoryAttribute : Attribute { }

    /// <summary>Exports service as custom wrapper.</summary>
    [AttributeUsage(AttributeTargets.Class 
        | AttributeTargets.Method 
        | AttributeTargets.Property 
        | AttributeTargets.Field, 
        Inherited = false)]
    public class AsWrapperAttribute : Attribute
    {
        /// <summary>For open-generic wrapper indicates wrapped argument type index.</summary>
        public int WrappedServiceTypeArgIndex { get; set; }

        /// <summary>Per name.</summary>
        public bool AlwaysWrapsRequiredServiceType { get; set; }

        /// <summary>Creates attribute with <see cref="WrappedServiceTypeArgIndex"/>.</summary>
        /// <param name="wrappedServiceTypeArgIndex">(optional) To use single generic type arg.</param>
        /// <param name="alwaysWrapsRequiredServiceType">(optional) Required for generic wrapper to ignore its type arguments.</param>
        public AsWrapperAttribute(int wrappedServiceTypeArgIndex = -1, bool alwaysWrapsRequiredServiceType = false)
        {
            WrappedServiceTypeArgIndex = wrappedServiceTypeArgIndex;
            AlwaysWrapsRequiredServiceType = alwaysWrapsRequiredServiceType;
        }
    }

    /// <summary>Specifies that exported service is decorator of services of <see cref="ExportAttribute.ContractType"/>.</summary>
    [AttributeUsage(AttributeTargets.Class 
        | AttributeTargets.Method 
        | AttributeTargets.Property 
        | AttributeTargets.Field, 
        Inherited = false)]
    public class AsDecoratorAttribute : Attribute
    {
        /// <summary>If <see cref="ContractName"/> specified, it has more priority over <see cref="ContractKey"/>. </summary>
        public string ContractName { get; set; }

        /// <summary>Contract key of decorated type.</summary>
        public object ContractKey { get; set; }

        /// <summary>Controls the order that decorators are registered in the container when multiple decorators are used for a single type.</summary>
        public int RegistrationOrder { get; set; }
    }

    /// <summary>Type of services supported by Container.</summary>
    public enum RegistrationType
    {
        /// <summary>(default) Defines normal service factory</summary>
        Service,
        /// <summary>Defines decorator factory</summary>
        Decorator,
        /// <summary>Defines wrapper factory.</summary>
        Wrapper
    };

    /// <summary>Dependency request path information.</summary>
    public sealed class RequestInfo
    {
        /// <summary>Represents empty info (indicated by null <see cref="ServiceType"/>).</summary>
        public static readonly RequestInfo Empty = new RequestInfo(null, null, null, RegistrationType.Service, null, 0, null);

        /// <summary>Returns true for an empty request.</summary>
        public bool IsEmpty { get { return ServiceType == null; } }

        /// <summary>Returns true if request is the first in a chain.</summary>
        public bool IsResolutionRoot { get { return !IsEmpty && Parent.IsEmpty; } }

        /// <summary>Parent request or null for root resolution request.</summary>
        public readonly RequestInfo Parent;

        /// <summary>Asked service type.</summary>
        public readonly Type ServiceType;

        /// <summary>Required service type if specified.</summary>
        public readonly Type RequiredServiceType;

        /// <summary>Optional service key.</summary>
        public readonly object ServiceKey;

        /// <summary>False for Decorators and Wrappers.</summary>
        public readonly RegistrationType FactoryType;

        /// <summary>Implementation type.</summary>
        public readonly Type ImplementationType;

        /// <summary>Relative number representing reuse lifespan.</summary>
        public readonly int ReuseLifespan;

        /// <summary>Simplified version of Push with most common properties.</summary>
        /// <param name="serviceType"></param> <param name="implementationType"></param>
        /// <param name="reuseLifespan"></param> <returns>Created info chain to current (parent) info.</returns>
        public RequestInfo Push(Type serviceType, Type implementationType, int reuseLifespan)
        {
            return Push(serviceType, null, null, RegistrationType.Service, implementationType, reuseLifespan);
        }

        /// <summary>Creates info by supplying all the properties and chaining it with current (parent) info.</summary>
        /// <param name="serviceType"></param> <param name="requiredServiceType"></param>
        /// <param name="serviceKey"></param> <param name="factoryType"></param>
        /// <param name="implementationType"></param> <param name="reuseLifespan"></param>
        /// <returns>Created info chain to current (parent) info.</returns>
        public RequestInfo Push(Type serviceType, Type requiredServiceType, object serviceKey,
            RegistrationType factoryType, Type implementationType, int reuseLifespan)
        {
            return new RequestInfo(serviceType, requiredServiceType, serviceKey, factoryType, implementationType, reuseLifespan, this);
        }

        private RequestInfo(
            Type serviceType, Type requiredServiceType, object serviceKey,
            RegistrationType factoryType, Type implementationType, int reuseLifespan,
            RequestInfo parent)
        {
            Parent = parent;

            // Service info:
            ServiceType = serviceType;
            RequiredServiceType = requiredServiceType;
            ServiceKey = serviceKey;

            // Implementation info:
            FactoryType = factoryType;
            ImplementationType = implementationType;
            ReuseLifespan = reuseLifespan;
        }

        /// <summary>Returns all request until the root - parent is null.</summary>
        /// <returns>Requests from the last to first.</returns>
        public IEnumerable<RequestInfo> Enumerate()
        {
            for (var i = this; !i.IsEmpty; i = i.Parent)
                yield return i;
        }

        /// <summary>Prints request with all its parents to string.</summary> <returns>The string.</returns>
        public override string ToString()
        {
            if (IsEmpty)
                return "{{empty}}";

            var s = new StringBuilder();

            if (FactoryType != RegistrationType.Service)
                s.Append(FactoryType.ToString().ToLower()).Append(' ');
            if (ImplementationType != null && ImplementationType != ServiceType)
                s.Append(ImplementationType).Append(": ");

            s.Append(ServiceType);

            if (RequiredServiceType != null)
                s.Append(" with RequiredServiceType=").Append(RequiredServiceType);

            if (ServiceKey != null)
                s.Append(" with ServiceKey=").Append('{').Append(ServiceKey).Append('}');

            if (ReuseLifespan != 0)
                s.Append(" with ReuseLifespan=").Append(ReuseLifespan);

            if (!Parent.IsEmpty)
                s.AppendLine().Append("  in ").Append(Parent);

            return s.ToString();
        }

        /// <summary>Returns true if request info and passed object are equal, and their parents recursively are equal.</summary>
        /// <param name="obj"></param> <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as RequestInfo);
        }

        /// <summary>Returns true if request info and passed info are equal, and their parents recursively are equal.</summary>
        /// <param name="other"></param> <returns></returns>
        public bool Equals(RequestInfo other)
        {
            return other != null && EqualsWithoutParent(other)
                && (Parent == null && other.Parent == null
                || (Parent != null && Parent.EqualsWithoutParent(other.Parent)));
        }

        /// <summary>Compares infos regarding properies but not their parents.</summary>
        /// <param name="other">Info to compare for equality.</param> <returns></returns>
        public bool EqualsWithoutParent(RequestInfo other)
        {
            return other.ServiceType == ServiceType
                && other.RequiredServiceType == RequiredServiceType
                && other.ServiceKey == ServiceKey

                && other.FactoryType == FactoryType
                && other.ImplementationType == ImplementationType
                && other.ReuseLifespan == ReuseLifespan;
        }

        /// <summary>Returns hash code combined from info fields plus its parent.</summary>
        /// <returns>Combined hash code.</returns>
        public override int GetHashCode()
        {
            var hash = 0;
            for (var i = this; i != Empty; i = i.Parent)
            {
                var currentHash = i.ServiceType.GetHashCode();
                if (i.RequiredServiceType != null)
                    currentHash = CombineHashCodes(currentHash, i.RequiredServiceType.GetHashCode());

                if (i.ServiceKey != null)
                    currentHash = CombineHashCodes(currentHash, i.ServiceKey.GetHashCode());

                if (i.FactoryType != RegistrationType.Service)
                    currentHash = CombineHashCodes(currentHash, i.FactoryType.GetHashCode());

                if (i.ImplementationType != null && i.ImplementationType != i.ServiceType)
                    currentHash = CombineHashCodes(currentHash, i.ImplementationType.GetHashCode());

                if (i.ReuseLifespan != 0)
                    currentHash = CombineHashCodes(currentHash, i.ReuseLifespan);

                hash = hash == 0 ? currentHash : CombineHashCodes(hash, currentHash);
            }
            return hash;
        }

        // Inspired by System.Tuple.CombineHashCodes
        private static int CombineHashCodes(int h1, int h2)
        {
            unchecked
            {
                return (h1 << 5) + h1 ^ h2;
            }
        }
    }

    /// <summary>Base type for exported type Setup Condition.</summary>
    [AttributeUsage(AttributeTargets.Class 
        | AttributeTargets.Method 
        | AttributeTargets.Property 
        | AttributeTargets.Field, 
        Inherited = false)]
    public abstract class ExportConditionAttribute : Attribute
    {
        /// <summary>Returns true to use exported service for request.</summary>
        /// <param name="request"></param> <returns>True to use exported service for request.</returns>
        public abstract bool Evaluate(RequestInfo request);
    }

    /// <summary>Imports service Only with equal <see cref="ContractKey"/>.</summary>
    [SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible", 
        Justification = "Not available in PCL.")] 
    [AttributeUsage(AttributeTargets.Parameter 
        | AttributeTargets.Field 
        | AttributeTargets.Property)]
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
    [AttributeUsage(AttributeTargets.Class 
        | AttributeTargets.Method
        | AttributeTargets.Parameter 
        | AttributeTargets.Field 
        | AttributeTargets.Property, 
        Inherited = false)]
    public class WithMetadataAttribute : Attribute
    {
        /// <summary>Metadata object</summary>
        public readonly object Metadata;

        /// <summary>Creates attribute</summary> <param name="metadata"></param>
        public WithMetadataAttribute(object metadata)
        {
            Metadata = metadata;
        }
    }

    /// <summary>Indicate to import service and in case it is not registered, register it using provided
    /// implementation info. Useful for ad-hoc/quick-prototyping registration of types from not controlled libraries.</summary>
    [AttributeUsage(AttributeTargets.Parameter 
        | AttributeTargets.Field 
        | AttributeTargets.Property)]
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

    /// <summary>Exported type should open resolution scope when injected.</summary>
    [AttributeUsage(AttributeTargets.Class
        | AttributeTargets.Interface
        | AttributeTargets.Method 
        | AttributeTargets.Property 
        | AttributeTargets.Field)]
    public class OpenResolutionScopeAttribute : Attribute { }

    /// <summary>Marker for resolution root exports.</summary>
    [AttributeUsage(AttributeTargets.Class
        | AttributeTargets.Interface
        | AttributeTargets.Method
        | AttributeTargets.Property
        | AttributeTargets.Field)]
    public class AsResolutionCall : Attribute { }

    /// <summary>Marker for resolution root exports.</summary>
    [AttributeUsage(AttributeTargets.Class
        | AttributeTargets.Interface
        | AttributeTargets.Method
        | AttributeTargets.Property
        | AttributeTargets.Field)]
    public class AsResolutionRoot : Attribute { }

}
