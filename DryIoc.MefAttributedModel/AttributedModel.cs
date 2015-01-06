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
    using System.Reflection;
    using System.Text;
    
    /// <summary>Implements MEF Attributed Programming Model. 
    /// Documentation is available at https://bitbucket.org/dadhi/dryioc/wiki/MefAttributedModel. </summary>
    public static class AttributedModel
    {
        ///<summary>Default reuse policy is Singleton, the same as in MEF.</summary>
        public static Type DefaultReuseType = typeof(SingletonReuse);

        /// <summary>Map of supported reuse types: so the reuse type specified by <see cref="ReuseAttribute"/> 
        /// could be mapped to corresponding <see cref="Reuse"/> members.</summary>
        public static readonly HashTree<Type, Func<string, IReuse>> SupportedReuseTypes = 
            HashTree<Type, Func<string, IReuse>>.Empty
            .AddOrUpdate(typeof(SingletonReuse), _ => Reuse.Singleton)
            .AddOrUpdate(typeof(CurrentScopeReuse), Reuse.InCurrentNamedScope)
            .AddOrUpdate(typeof(ResolutionScopeReuse), _ => Reuse.InResolutionScope);

        /// <summary>Returns new rules with attributed model importing rules appended.</summary>
        /// <param name="rules">Source rules to append importing rules to.</param>
        /// <returns>New rules with attributed model rules.</returns>
        public static Rules WithMefAttributedModel(this Rules rules)
        {
            // hello, Max!!! we are Martians.
            return rules.With(GetImportingConstructor, GetImportedParameter, _getImportedPropertiesAndFields);
        }

        /// <summary>Appends attributed model rules to passed container.</summary>
        /// <param name="container">Source container to apply attributed model importing rules to.</param>
        /// <returns>Returns new container with new rules.</returns>
        public static IContainer WithMefAttributedModel(this IContainer container)
        {
            return container.With(rules => rules.WithMefAttributedModel());
        }

        /// <summary>Registers implementation type(s) with provided registrator/container. Expects that
        /// implementation type are annotated with <see cref="ExportAttribute"/>, or <see cref="ExportAllAttribute"/>.</summary>
        /// <param name="registrator">Container to register types into.</param>
        /// <param name="types">Implementation types to register.</param>
        public static void RegisterExports(this IRegistrator registrator, params Type[] types)
        {
            registrator.RegisterExports(types
                .Select(GetRegistrationInfoOrDefault).Where(info => info != null));
        }
        
        /// <summary>First scans (<see cref="Scan"/>) provided assemblies to find types annotated with
        /// <see cref="ExportAttribute"/>, or <see cref="ExportAllAttribute"/>.
        /// Then registers found types into registrator/container.</summary>
        /// <param name="registrator">Container to register into</param>
        /// <param name="assemblies">Assemblies to scan for implementation types.</param>
        public static void RegisterExports(this IRegistrator registrator, params Assembly[] assemblies)
        {
            registrator.RegisterExports(Scan(assemblies));
        }

        /// <summary>Registers new factories into registrator/container based on provided registration infos, which
        /// is serializable DTO for registration.</summary>
        /// <param name="registrator">Container to register into.</param>
        /// <param name="infos">Registrations to register.</param>
        public static void RegisterExports(this IRegistrator registrator, IEnumerable<RegistrationInfo> infos)
        {
            foreach (var info in infos)
                RegisterInfo(registrator, info);
        }

        /// <summary>Registers factories into registrator/container based on single provided info, which could
        /// contain multiple exported services with single implementation.</summary>
        /// <param name="registrator">Container to register into.</param>
        /// <param name="registrationInfo">Registration information provided.</param>
        public static void RegisterInfo(this IRegistrator registrator, RegistrationInfo registrationInfo)
        {
            var factory = registrationInfo.CreateFactory();

            for (var i = 0; i < registrationInfo.Exports.Length; i++)
            {
                var export = registrationInfo.Exports[i];
                registrator.Register(factory, export.ServiceType,
                    export.ServiceKeyInfo.Key, IfAlreadyRegistered.AppendDefault);
            }

            if (registrationInfo.IsFactory)
                RegisterFactoryMethods(registrator, registrationInfo);
        }

        /// <summary>Scans assemblies to find concrete type annotated with <see cref="ExportAttribute"/>, or <see cref="ExportAllAttribute"/>
        /// attributes, and create serializable DTO with all information required for registering of exported types.</summary>
        /// <param name="assemblies">Assemblies to scan.</param>
        /// <returns>Lazy collection of registration info DTOs.</returns>
        public static IEnumerable<RegistrationInfo> Scan(IEnumerable<Assembly> assemblies)
        {
            return assemblies.SelectMany(Portable.GetTypesFromAssembly).Select(GetRegistrationInfoOrDefault).Where(x => x != null);
        }

        /// <summary>Creates registration info DTO for provided type. To find this info checks type attributes:
        /// <see cref="ExportAttribute"/>, or <see cref="ExportAllAttribute"/>.
        /// If type is not concrete or is value type, then return null.</summary>
        /// <param name="implementationType">Type to convert into registration info.</param>
        /// <returns>Created DTO.</returns>
        public static RegistrationInfo GetRegistrationInfoOrDefault(Type implementationType)
        {
            if (implementationType.IsValueType() || implementationType.IsAbstract()) 
                return null;

            var attributes = GetAllExportRelatedAttributes(implementationType);
            return !IsExportDefined(attributes) ? null : GetRegistrationInfoOrDefault(implementationType, attributes);
        }

        #region Tools

        /// <summary>Returns reuse object by mapping provided type to <see cref="SupportedReuseTypes"/>.
        /// Returns null (transient or no reuse) if null provided reuse type.</summary>
        /// <param name="reuseType">Reuse type to find in supported.</param>
        /// <param name="reuseName">(optional) Reuse name to match with scope name.</param>
        /// <returns>Supported reuse object.</returns>
        public static IReuse GetReuse(Type reuseType, string reuseName = null)
        {
            return reuseType == null ? null
                : SupportedReuseTypes.GetValueOrDefault(reuseType)
                .ThrowIfNull(Error.UNSUPPORTED_REUSE_TYPE, reuseType)
                .Invoke(reuseName);
        }

        #endregion

        #region Rules

        private static FactoryMethod GetImportingConstructor(Request request)
        {
            var implementationType = request.ImplementationType;
            var constructors = implementationType.GetAllConstructors().ToArrayOrSelf();
            return constructors.Length == 1 
                ? constructors[0]
                : constructors.SingleOrDefault(x => x.GetAttributes(typeof(ImportingConstructorAttribute)).Any())
                    .ThrowIfNull(Error.NO_SINGLE_CTOR_WITH_IMPORTING_ATTR, implementationType);
        }

        private static ParameterServiceInfo GetImportedParameter(ParameterInfo parameter, Request request)
        {
            var serviceInfo = ParameterServiceInfo.Of(parameter);
            var attrs = parameter.GetAttributes().ToArray();
            return attrs.Length == 0 ? serviceInfo
                : serviceInfo.WithDetails(GetFirstImportDetailsOrNull(parameter.ParameterType, attrs, request), request);
        }

        private static readonly PropertiesAndFieldsSelector _getImportedPropertiesAndFields =
            PropertiesAndFields.All(withInfo: GetImportedPropertiesAndFieldsOnly);

        private static PropertyOrFieldServiceInfo GetImportedPropertiesAndFieldsOnly(MemberInfo member, Request request)
        {
            var attributes = member.GetAttributes().ToArray();
            var details = attributes.Length == 0 ? null
                : GetFirstImportDetailsOrNull(member.GetPropertyOrFieldType(), attributes, request);
            return details == null ? null : PropertyOrFieldServiceInfo.Of(member).WithDetails(details, request);
        }

        private static ServiceInfoDetails GetFirstImportDetailsOrNull(Type type, Attribute[] attributes, Request request)
        {
            return GetImportDetails(type, attributes, request) ?? GetImportExternalDetails(type, attributes, request);
        }

        private static ServiceInfoDetails GetImportDetails(Type reflectedType, Attribute[] attributes, Request request)
        {
            var import = GetSingleAttributeOrDefault<ImportAttribute>(attributes);
            if (import == null)
                return null;

            var serviceKey = import.ContractName
                ?? (import is ImportWithKeyAttribute ? ((ImportWithKeyAttribute)import).ContractKey : null)
                ?? GetServiceKeyWithMetadataAttribute(reflectedType, attributes, request);

            var ifUnresolved = import.AllowDefault ? IfUnresolved.ReturnDefault : IfUnresolved.Throw;

            return ServiceInfoDetails.Of(import.ContractType, serviceKey, ifUnresolved);
        }

        private static object GetServiceKeyWithMetadataAttribute(Type reflectedType, Attribute[] attributes, Request request)
        {
            var meta = GetSingleAttributeOrDefault<WithMetadataAttribute>(attributes);
            if (meta == null)
                return null;

            var registry = request.Container;
            reflectedType = registry.UnwrapServiceType(reflectedType);
            var metadata = meta.Metadata;
            var factory = registry.GetAllServiceFactories(reflectedType)
                .FirstOrDefault(f => metadata.Equals(f.Value.Setup.Metadata))
                .ThrowIfNull(Error.NOT_FIND_DEPENDENCY_WITH_METADATA, reflectedType, metadata, request);

            return factory.Key;
        }

        private static ServiceInfoDetails GetImportExternalDetails(Type serviceType, Attribute[] attributes, Request request)
        {
            var import = GetSingleAttributeOrDefault<ImportExternalAttribute>(attributes);
            if (import == null)
                return null;

            var registry = request.Container;
            serviceType = import.ContractType ?? registry.UnwrapServiceType(serviceType);
            var serviceKey = import.ContractKey;

            if (!registry.IsRegistered(serviceType, serviceKey))
            {
                var implementationType = import.ImplementationType ?? serviceType;

                var reuseAttr = GetSingleAttributeOrDefault<ReuseAttribute>(attributes);
                var reuseType = reuseAttr == null ? DefaultReuseType : reuseAttr.ReuseType;
                var reuseName = reuseAttr == null ? null : reuseAttr.ReuseName;
                var reuse = GetReuse(reuseType, reuseName);

                var withConstructor = import.WithConstructor == null ? null
                    : (Func<Type, ConstructorInfo>)(t => t.GetConstructorOrNull(args: import.WithConstructor));

                registry.Register(serviceType, implementationType,
                    reuse, withConstructor, null, Setup.With(metadata: import.Metadata),
                    serviceKey, IfAlreadyRegistered.KeepIt);
            }

            return ServiceInfoDetails.Of(serviceType, serviceKey);
        }

        private static TAttribute GetSingleAttributeOrDefault<TAttribute>(Attribute[] attributes) where TAttribute : Attribute
        {
            TAttribute attr = null;
            for (var i = 0; i < attributes.Length && attr == null; i++)
                attr = attributes[i] as TAttribute;
            return attr;
        }

        #endregion

        #region Implementation

        private static RegistrationInfo GetRegistrationInfoOrDefault(Type implementationType, Attribute[] attributes)
        {
            var info = new RegistrationInfo { ImplementationType = implementationType, ReuseType = DefaultReuseType };

            for (var attributeIndex = 0; attributeIndex < attributes.Length; attributeIndex++)
            {
                var attribute = attributes[attributeIndex];
                if (attribute is ExportAttribute)
                {
                    info.Exports = GetExportsFromExportAttribute((ExportAttribute)attribute, info, implementationType);
                }
                else if (attribute is ExportAllAttribute)
                {
                    info.Exports = GetExportsFromExportAllAttribute((ExportAllAttribute)attribute, info, implementationType);
                }
                else if (attribute is PartCreationPolicyAttribute)
                {
                    var creationPolicy = ((PartCreationPolicyAttribute)attribute).CreationPolicy;
                    info.ReuseType = creationPolicy == CreationPolicy.Shared ? typeof(SingletonReuse) : null;
                }
                else if (attribute is ReuseAttribute)
                {
                    var reuseAttribute = ((ReuseAttribute)attribute);
                    info.ReuseType = reuseAttribute.ReuseType;
                    info.ReuseName = reuseAttribute.ReuseName;
                }
                else if (attribute is ReuseWrappersAttribute)
                {
                    info.ReuseWrapperTypes = ((ReuseWrappersAttribute)attribute).WrapperTypes;
                }
                else if (attribute is AsWrapperAttribute)
                {
                    PopulateWrapperInfoFromAttribute(info, (AsWrapperAttribute)attribute, implementationType);
                }
                else if (attribute is AsDecoratorAttribute)
                {
                    PopulateDecoratorInfoFromAttribute(info, (AsDecoratorAttribute)attribute, implementationType);
                }
                else if (attribute is AsFactoryAttribute)
                {
                    info.IsFactory = true;
                }

                if (attribute.GetType().GetAttributes(typeof(MetadataAttributeAttribute), true).Any())
                {
                    Throw.If(info.HasMetadataAttribute, Error.UNSUPPORTED_MULTIPLE_METADATA, implementationType);
                    info.HasMetadataAttribute = true;
                }
            }

            if (info.FactoryType == FactoryType.Decorator)
                info.ReuseType = null;

            info.Exports.ThrowIfNull(Error.NO_EXPORT, implementationType);
            return info;
        }

        private static bool IsExportDefined(Attribute[] attributes)
        {
            return attributes.Length != 0 
                && attributes.IndexOf(a => a is ExportAttribute || a is ExportAllAttribute) != -1 
                && attributes.IndexOf(a => a is PartNotDiscoverableAttribute) == -1;
        }

        private static ExportInfo[] GetExportsFromExportAttribute(ExportAttribute attribute,
            RegistrationInfo currentInfo, Type implementationType)
        {
            var export = new ExportInfo(attribute.ContractType ?? implementationType,
                attribute.ContractName ??
                (attribute is ExportWithKeyAttribute ? ((ExportWithKeyAttribute)attribute).ContractKey : null));

            var currentExports = currentInfo.Exports;
            var exports = currentExports == null ? new[] { export }
                : currentExports.Contains(export) ? currentExports 
                : currentExports.AppendOrUpdate(export);
            return exports;
        }

        private static ExportInfo[] GetExportsFromExportAllAttribute(ExportAllAttribute attribute,
            RegistrationInfo currentInfo, Type implementationType)
        {
            var allContractTypes = attribute.GetContractTypes(implementationType);

            if (implementationType.IsGenericDefinition())
            {
                var implTypeArgs = implementationType.GetGenericParamsAndArgs();
                allContractTypes = allContractTypes
                    .Where(t => t.ContainsAllGenericParameters(implTypeArgs))
                    .Select(t => t.GetGenericDefinitionOrNull());
            }

            var exports = allContractTypes
                .Select(t => new ExportInfo(t, attribute.ContractName ?? attribute.ContractKey))
                .ToArray();
            
            Throw.If(exports.Length == 0, Error.NO_TYPES_IN_EXPORT_ALL,
                implementationType, allContractTypes);

            var currentExports = currentInfo.Exports;
            if (currentExports != null)
                for (var index = 0; index < currentExports.Length; index++)
                    if (!exports.Contains(currentExports[index])) // filtering out identical exports
                        exports = exports.AppendOrUpdate(currentExports[index]);

            return exports;
        }

        private static void PopulateWrapperInfoFromAttribute(RegistrationInfo resultInfo, AsWrapperAttribute attribute,
            Type implementationType)
        {
            Throw.If(resultInfo.FactoryType != FactoryType.Service, Error.UNSUPPORTED_MULTIPLE_FACTORY_TYPES, implementationType);
            resultInfo.FactoryType = FactoryType.Wrapper;
            resultInfo.Wrapper = new WrapperInfo
            {
                WrappedServiceType = attribute.WrappedContractType,
                WrappedServiceTypeGenericArgIndex = attribute.ContractTypeGenericArgIndex
            };
        }

        private static void PopulateDecoratorInfoFromAttribute(RegistrationInfo resultInfo, AsDecoratorAttribute attribute,
            Type implementationType)
        {
            Throw.If(resultInfo.FactoryType != FactoryType.Service, Error.UNSUPPORTED_MULTIPLE_FACTORY_TYPES, implementationType);
            resultInfo.FactoryType = FactoryType.Decorator;
            resultInfo.Decorator = new DecoratorInfo(attribute.ConditionType, attribute.ContractName ?? attribute.ContractKey);
        }

        private static Attribute[] GetAllExportRelatedAttributes(Type type)
        {
            var attributes = type.GetAttributes();

            for (var baseType = type.GetBaseType();
                baseType != typeof(object) && baseType != null;
                baseType = baseType.GetBaseType())
                attributes = attributes.Append(GetInheritedExportAttributes(baseType));

            var interfaces = type.GetImplementedInterfaces();
            for (var i = 0; i < interfaces.Length; i++)
                attributes = attributes.Append(GetInheritedExportAttributes(interfaces[i]));

            return attributes;
        }

        private static Attribute[] GetInheritedExportAttributes(Type type)
        {
            var exports = type.GetAttributes(typeof(InheritedExportAttribute));
            for (var i = 0; i < exports.Length; i++)
            {
                var export = (InheritedExportAttribute)exports[i];
                if (export.ContractType == null)
                    exports[i] = new InheritedExportAttribute(export.ContractName, type);
            }
            return exports;
        }

        private static void RegisterFactoryMethods(IRegistrator registrator, RegistrationInfo factoryInfo)
        {
            var methods = factoryInfo.ImplementationType.GetAll(_ => _.DeclaredMethods);
            foreach (var method in methods)
            {
                var attributes = method.GetAttributes().ToArrayOrSelf();
                if (!IsExportDefined(attributes))
                    continue;

                var serviceInfo = GetRegistrationInfoOrDefault(method.ReturnType, attributes).ThrowIfNull();

                var factoryMethod = method;
                var factoryExport = factoryInfo.Exports[0];
                var rules = InjectionRules.With(r => FactoryMethod.Of(factoryMethod, 
                    r.Resolve(factoryExport.ServiceType, factoryExport.ServiceKeyInfo.Key, IfUnresolved.ReturnDefault)));
                
                var serviceFactory = serviceInfo.CreateFactory(rules);

                var serviceExports = serviceInfo.Exports;
                for (var i = 0; i < serviceExports.Length; i++)
                {
                    var export = serviceExports[i];
                    registrator.Register(serviceFactory, export.ServiceType, export.ServiceKeyInfo.Key, IfAlreadyRegistered.AppendDefault);
                }
            }
        }

        #endregion
    }

    /// <summary>Defines error codes and messages for <see cref="AttributedModelException"/>.</summary>
    public static class Error
    {
        /// <summary>Error messages for corresponding codes.</summary>
        public readonly static IList<string> Messages = new List<string>(20);

        /// <summary>Codes are starting from this value.</summary>
        public readonly static int FIRST_ERROR_CODE = 100;

#pragma warning disable 1591 // Missing XML-comment
        public static readonly int
            NO_SINGLE_CTOR_WITH_IMPORTING_ATTR  = Of("Unable to find single constructor with " + typeof(ImportingConstructorAttribute) + " in {0}."),
            NOT_FIND_DEPENDENCY_WITH_METADATA   = Of("Unable to resolve dependency {0} with metadata [{1}] in {2}"),
            UNSUPPORTED_MULTIPLE_METADATA       = Of("Multiple associated metadata found while exporting {0}." + Environment.NewLine 
                                                    + "Only single metadata is supported per implementation type, please remove the rest."),
            UNSUPPORTED_MULTIPLE_FACTORY_TYPES  = Of("Found multiple factory types associated with exported {0}. Only single ExportAs.. attribute is supported, please remove the rest."),
            NO_EXPORT                           = Of("At least one Export attributed should be defined for {0}."),
            NO_TYPES_IN_EXPORT_ALL              = Of("Unable to get contract types for implementation {0} because all of its implemented types where filtered out: {1}"),
            UNSUPPORTED_REUSE_TYPE              = Of("Attributed model does not support reuse type {0}."),
            UNSUPPORTED_REUSE_WRAPPER_TYPE      = Of("Attributed model does not support reuse wrapper type {0}."),
            NO_WRAPPED_TYPE_EXPORTED_WRAPPER    = Of("Exported non-generic wrapper type {0} requires wrapped service type to be specified, but it is null "
                                                    + "and instead generic argument index is set to {1}."),
            WRAPPED_ARG_INDEX_OUT_OF_BOUNDS     = Of("Exported generic wrapper type {0} specifies generic argument index {1} outside of argument list size.");
#pragma warning restore 1591

        /// <summary>Returns message by provided error code.</summary>
        /// <param name="error">Code starting from <see cref="FIRST_ERROR_CODE"/></param> <returns>String message.</returns>
        public static string GetMessage(int error)
        {
            return Messages[error - FIRST_ERROR_CODE];
        }

        #region Implementation

        static Error()
        {
            var original = Throw.GetMatchedException;
            Throw.GetMatchedException = (check, error, arg0, arg1, arg2, arg3, inner) =>
                0 <= error - FIRST_ERROR_CODE && error - FIRST_ERROR_CODE < Messages.Count
                    ? AttributedModelException.Of(check, error, arg0, arg1, arg2, arg3, inner)
                    : original(check, error, arg0, arg1, arg2, arg3, inner);
        }

        private static int Of(string message)
        {
            Messages.Add(message);
            return FIRST_ERROR_CODE + Messages.Count - 1;
        }

        #endregion
    }

    /// <summary>Specific exception type to be thrown by MefAttributedModel extension. Check <see cref="Error"/> for possible error cases.</summary>
    public class AttributedModelException : ContainerException
    {
        /// <summary>Creates exception by wrapping <paramref name="errorCode"/> and with message corresponding to code.</summary>
        /// <param name="errorCheck">Type of check was done.</param> <param name="errorCode">Error code to wrap, <see cref="Error"/> for codes defined.</param>
        /// <param name="arg0">(optional) Arguments for formatted message.</param> <param name="arg1"></param> <param name="arg2"></param> <param name="arg3"></param> 
        /// <param name="inner">(optional) Inner exception to wrap.</param>
        /// <returns>Create exception object.</returns>
        public new static AttributedModelException Of(ErrorCheck errorCheck, int errorCode,
            object arg0, object arg1 = null, object arg2 = null, object arg3 = null,
            Exception inner = null)
        {
            var message = string.Format(MefAttributedModel.Error.GetMessage(errorCode), Print(arg0), Print(arg1), Print(arg2), Print(arg3));
            return inner == null
                ? new AttributedModelException(errorCode, message)
                : new AttributedModelException(errorCode, message, inner);
        }

        private AttributedModelException(int error, string message) : base(error, message) {}

        private AttributedModelException(int error, string message, Exception innerException) : base(error, message, innerException) {}
    }

    /// <summary>Converts provided literal into valid C# code. Used for generating registration code 
    /// from <see cref="RegistrationInfo"/> DTOs.</summary>
    public static class PrintCode
    {
        /// <summary>Prints valid c# Boolean literal: true/false.</summary>
        /// <param name="code">Code to print to.</param> <param name="x">Value to print.</param> <returns>Code with appended literal.</returns>
        public static StringBuilder AppendBool(this StringBuilder code, bool x)
        {
            return code.Append(x ? "true" : "false");
        }

        /// <summary>Prints valid c# string constant.</summary>
        /// <param name="code">Code to print to.</param> <param name="x">Value to print.</param> <returns>Code with appended literal.</returns>
        public static StringBuilder AppendString(this StringBuilder code, string x)
        {
            return x == null ? code.Append("null") : code.Append('"').Append(x).Append('"');
        }

        /// <summary>Prints valid c# Type literal: typeof(Namespace.Type).</summary>
        /// <param name="code">Code to print to.</param> <param name="x">Value to print.</param> <returns>Code with appended literal.</returns>
        public static StringBuilder AppendType(this StringBuilder code, Type x)
        {
            return x == null ? code.Append("null") : code.Append("typeof(").Print(x).Append(')');
        }

        /// <summary>Prints valid c# Enum literal: Enum.Value.</summary>
        /// <param name="code">Code to print to.</param>
        /// <param name="enumType">Enum type of the value.</param>
        /// <param name="x">Value to print.</param> <returns>Code with appended literal.</returns>
        public static StringBuilder AppendEnum(this StringBuilder code, Type enumType, object x)
        {
            return code.Print(enumType).Append('.').Append(Enum.GetName(enumType, x));
        }

        /// <summary>Prints valid c# literal depending of <paramref name="x"/> type.</summary>
        /// <param name="code">Code to print to.</param> <param name="x">Value to print.</param>
        /// <param name="ifNotRecognized">(optional) Delegate to print unrecognized value.</param>
        /// <returns>Code with appended literal.</returns>
        public static StringBuilder AppendObject(this StringBuilder code, object x, Action<StringBuilder, object> ifNotRecognized = null)
        {
            if (x == null)
                return code.Append("null");
            if (x is bool)
                return code.AppendBool((bool)x);
            if (x is string)
                return code.AppendString((string)x);
            if (x is Type)
                return code.AppendType((Type)x);

            var type = x.GetType();
            if (type.IsEnum())
                return code.AppendEnum(type, x);

            if (ifNotRecognized != null)
                ifNotRecognized(code, x);
            else
                code.Append(x);

            return code;
        }
    }

    #region Registration Info DTOs
#pragma warning disable 659

    /// <summary>Serializable DTO of all registration information.</summary>
    public sealed class RegistrationInfo
    {
        /// <summary>All exports defined for implementation type (registration).</summary>
        public ExportInfo[] Exports;

        /// <summary>Concrete type on what exports are defined: exported type.</summary>
        /// <remarks>May be null if <see cref="ImplementationTypeFullName"/> specified.</remarks>
        public Type ImplementationType;

        /// <summary>Full name of exported type. Enables type lazy-loading scenario.</summary>
        public string ImplementationTypeFullName;

        /// <summary>One of <see cref="AttributedModel.SupportedReuseTypes"/>.</summary>
        public Type ReuseType;

        /// <summary>Name to pass to reuse factory from <see cref="AttributedModel.SupportedReuseTypes"/>.</summary>
        public string ReuseName;

        /// <summary>Reuse wrappers defined for exported type.</summary>
        public Type[] ReuseWrapperTypes;

        /// <summary>True if exported type has metadata.</summary>
        public bool HasMetadataAttribute;

        /// <summary>Factory type to specify <see cref="FactorySetup"/>.</summary>
        public FactoryType FactoryType;

        /// <summary>Not null if exported with <see cref="AsDecoratorAttribute"/>, contains info to <see cref="SetupDecorator"/>.</summary>
        public DecoratorInfo Decorator;

        /// <summary>Not null if exported with <see cref="AsWrapperAttribute"/>, contains info to <see cref="SetupWrapper"/>.</summary>
        public WrapperInfo Wrapper;
        
        /// <summary>True if exported with <see cref="AsFactoryAttribute"/>.</summary>
        public bool IsFactory;

        /// <summary>Creates factory out of registration info.</summary>
        /// <param name="rules">(optional) Injection rules. Used if registration <see cref="IsFactory"/> to specify factory methods.</param>
        /// <returns>Created factory.</returns>
        public Factory CreateFactory(InjectionRules rules = null)
        {
            var reuse = AttributedModel.GetReuse(ReuseType, ReuseName);
            return new ReflectionFactory(ImplementationType, reuse, rules, GetSetup());
        }

        /// <summary>Create factory setup from DTO data.</summary>
        /// <param name="attributes">Implementation type attributes provided to get optional metadata.</param>
        /// <returns>Created factory setup.</returns>
        public FactorySetup GetSetup(Attribute[] attributes = null)
        {
            if (FactoryType == FactoryType.Wrapper)
                return Wrapper == null ? SetupWrapper.Default : Wrapper.GetSetup();

            var lazyMetadata = HasMetadataAttribute ? (Func<object>)(() => GetMetadata(attributes)) : null;

            if (FactoryType == FactoryType.Decorator)
                return Decorator == null ? SetupDecorator.Default : Decorator.GetSetup(lazyMetadata);

            var setup = lazyMetadata == null ? Setup.Default : Setup.With(lazyMetadata: lazyMetadata);

            return ReuseWrapperTypes == null || ReuseWrapperTypes.Length == 0
                ? setup
                : setup.WithReuseWrappers(ReuseWrapperTypes);
        }

        public override bool Equals(object obj)
        {
            var other = obj as RegistrationInfo;
            return other != null
                && other.ImplementationType == ImplementationType
                && other.ReuseType == ReuseType
                && other.FactoryType == FactoryType
                && Equals(other.Wrapper, Wrapper)
                && Equals(other.Decorator, Decorator)
                && other.Exports.SequenceEqual(Exports);
        }

        /// <summary>Generate valid c# code for instantiating of info from its state. Supposed be used in compile-time scenarios.</summary>
        /// <param name="code">Code to append "new RegistrationInfo(...)" to.</param>
        /// <returns>Code with "new info".</returns>
        public StringBuilder AppendAsCode(StringBuilder code = null)
        {
            code = code ?? new StringBuilder();
            code.Append(
@"new RegistrationInfo {
    ImplementationType = ").AppendType(ImplementationType).Append(@",
    Exports = new[] {
        "); for (var i = 0; i < Exports.Length; i++)
                code = Exports[i].AppendCode(code).Append(@",
        "); code.Append(@"},
    ReuseType = ").AppendType(ReuseType).Append(@",
    HasMetadataAttribute = ").AppendBool(HasMetadataAttribute).Append(@",
    FactoryType = ").AppendEnum(typeof(FactoryType), FactoryType);
            if (Wrapper != null) code.Append(@",
    Wrapper = new WrapperInfo { WrappedServiceTypeGenericArgIndex = ")
                .Append(Wrapper.WrappedServiceTypeGenericArgIndex)
                .Append(", WrappedServiceType = ")
                .AppendType(Wrapper.WrappedServiceType).Append(@" }");
            if (Decorator != null)
            {
                code.Append(@",
"); Decorator.AppendAsCode(code);
            }
            code.Append(@"
}");
            return code;
        }

        private object GetMetadata(Attribute[] attributes = null)
        {
            attributes = attributes ?? ImplementationType.GetAttributes();
            var metadataAttr = attributes.FirstOrDefault(
                a => a.GetType().GetAttributes(typeof(MetadataAttributeAttribute), true).Any());

            return metadataAttr is WithMetadataAttribute
                ? ((WithMetadataAttribute)metadataAttr).Metadata
                : metadataAttr;
        }
    }

    /// <summary>Defines DTO for exported service type and key.</summary>
    public sealed class ExportInfo
    {
        /// <summary>Contract type.</summary>
        /// <remarks>may be null if <see cref="ServiceTypeFullName"/> specified.</remarks>
        public Type ServiceType;
 
        /// <summary>Full contract type name. Supposed to be used in lazy-loading scenario.</summary>
        public string ServiceTypeFullName;

        /// <summary>Wrapped contract name or service key. It is wrapped in order to be serializable.</summary>
        public ServiceKeyInfo ServiceKeyInfo = ServiceKeyInfo.Default;

        /// <summary>Default constructor is usually required by deserializer.</summary>
        public ExportInfo() { } 

        /// <summary>Creates exported info out of type and optional key.</summary>
        /// <param name="serviceType">Contract type to store.</param>
        /// <param name="serviceKey">(optional) ContractName string or service key.</param>
        public ExportInfo(Type serviceType, object serviceKey = null)
        {
            ServiceType = serviceType;
            ServiceKeyInfo = ServiceKeyInfo.Of(serviceKey);
        }

        public override bool Equals(object obj)
        {
            var other = obj as ExportInfo;
            return other != null
                && other.ServiceType == ServiceType
                && Equals(other.ServiceKeyInfo.Key, ServiceKeyInfo.Key);
        }

        /// <summary>Generates valid c# code to "new <see cref="ExportInfo"/>() { ... };" from its state.</summary>
        /// <param name="code">Code to append generated code to.</param>
        /// <returns>Code with appended generated info.</returns>
        public StringBuilder AppendCode(StringBuilder code = null)
        {
            return (code ?? new StringBuilder())
                .Append(@"new ExportInfo(").AppendType(ServiceType).Append(@", ")
                .AppendObject(ServiceKeyInfo.Key).Append(@")");
        }
    }

    /// <summary>Describes <see cref="SetupWrapper"/> in serializable way.</summary>
    public sealed class WrapperInfo
    {
        public Type WrappedServiceType;

        public int WrappedServiceTypeGenericArgIndex;

        public SetupWrapper GetSetup()
        {
            return SetupWrapper.With(unwrapServiceType: GetWrappedServiceType);
        }

        public override bool Equals(object obj)
        {
            var other = obj as WrapperInfo;
            return other != null && other.WrappedServiceTypeGenericArgIndex == WrappedServiceTypeGenericArgIndex;
        }

        private Type GetWrappedServiceType(Type wrapperType)
        {
            if (WrappedServiceType != null)
                return WrappedServiceType;

            wrapperType.ThrowIf(!wrapperType.IsClosedGeneric(),
                Error.NO_WRAPPED_TYPE_EXPORTED_WRAPPER, WrappedServiceTypeGenericArgIndex);

            var typeArgs = wrapperType.GetGenericParamsAndArgs();
            wrapperType.ThrowIf(WrappedServiceTypeGenericArgIndex > typeArgs.Length - 1,
                Error.WRAPPED_ARG_INDEX_OUT_OF_BOUNDS, WrappedServiceTypeGenericArgIndex);

            return typeArgs[WrappedServiceTypeGenericArgIndex];
        }
    }

    public sealed class DecoratorInfo
    {
        public DecoratorInfo() { }

        public DecoratorInfo(Type conditionType = null, object serviceKey = null)
        {
            ConditionType = conditionType;
            ServiceKeyInfo = ServiceKeyInfo.Of(serviceKey);
        }

        public Type ConditionType;
        public ServiceKeyInfo ServiceKeyInfo = ServiceKeyInfo.Default;

        public SetupDecorator GetSetup(Func<object> lazyMetadata = null)
        {
            if (ConditionType != null)
                return SetupDecorator.With(((IDecoratorCondition)Activator.CreateInstance(ConditionType)).CanApply);

            if (ServiceKeyInfo != ServiceKeyInfo.Default || lazyMetadata != null)
                return SetupDecorator.With(request =>
                    (ServiceKeyInfo.Key == null || Equals(ServiceKeyInfo.Key, request.ServiceKey)) &&
                    (lazyMetadata == null || Equals(lazyMetadata(), request.ResolvedFactory.Setup.Metadata)));

            return SetupDecorator.Default;
        }

        public override bool Equals(object obj)
        {
            var other = obj as DecoratorInfo;
            return other != null
                && other.ConditionType == ConditionType
                && Equals(other.ServiceKeyInfo.Key, ServiceKeyInfo.Key);
        }

        public StringBuilder AppendAsCode(StringBuilder code = null)
        {
            return (code ?? new StringBuilder())
                .Append(@"Decorator = new DecoratorInfo(")
                .AppendType(ConditionType).Append(", ")
                .AppendObject(ServiceKeyInfo.Key).Append(")");
        }
    }

    /// <summary>
    ///  TODO: Do we really need it? Or could just throw on no primitive type.
    /// </summary>
    public sealed class ServiceKeyInfo
    {
        public static readonly ServiceKeyInfo Default = new ServiceKeyInfo();

        public static ServiceKeyInfo Of(object key)
        {
            return key == null ? Default : new ServiceKeyInfo { Key = key };
        }

        public object Key;
    }

#pragma warning restore 659
    #endregion

    #region Additional Export/Import attributes

    /// <summary>Base attribute to specify type of reuse (implementing <see cref="IReuse"/>) for annotated class.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    public class ReuseAttribute : Attribute
    {
        /// <summary>Implementation of <see cref="IReuse"/>. Could be null to specify transient or no reuse.</summary>
        public readonly Type ReuseType;
        
        /// <summary>Optional name, valid only for <see cref="CurrentScopeReuse"/>.</summary>
        public readonly string ReuseName;

        /// <summary>Create attribute with specified type implementing <see cref="IReuse"/>.</summary>
        /// <param name="reuseType">Could be null to specify transient or no reuse.</param>
        /// <param name="reuseName">(optional) Name is valid only for <see cref="CurrentScopeReuse"/> and will be ignored by the rest of reuse types.</param>
        public ReuseAttribute(Type reuseType, string reuseName = null)
        {
            if (reuseType != null) (typeof(IReuse)).ThrowIfNotOf(reuseType);
            ReuseType = reuseType;
            ReuseName = reuseName;
        }
    }

    /// <summary>Defines the <see cref="Reuse.Transient"/> for exported service.</summary>
    public class TransientReuseAttribute : ReuseAttribute
    {
        /// <summary>Creates attribute by specifying null as <see cref="ReuseAttribute.ReuseType"/>.</summary>
        public TransientReuseAttribute() : base(null) { }
    }

    /// <summary>Denotes exported type with <see cref="Reuse.Singleton"/>.</summary>
    public class SingletonReuseAttribute : ReuseAttribute
    {
        /// <summary>Creates attribute.</summary>
        public SingletonReuseAttribute() : base(typeof(SingletonReuse)) { }
    }

    /// <summary>Denotes exported type with <see cref="Reuse.InCurrentScope"/> or 
    /// <see cref="Reuse.InCurrentNamedScope"/> if name is specified.</summary>
    public class CurrentScopeReuseAttribute : ReuseAttribute
    {
        /// <summary>Creates attribute.</summary> <param name="reuseName">(optional)</param>
        public CurrentScopeReuseAttribute(string reuseName = null) : base(typeof(CurrentScopeReuse), reuseName) {}
    }

    /// <summary>Denotes exported type with <see cref="Reuse.InResolutionScope"/>.</summary>
    public class ResolutionScopeReuseAttribute : ReuseAttribute
    {
        /// <summary>Creates attribute.</summary>
        public ResolutionScopeReuseAttribute() : base(typeof(ResolutionScopeReuse)) { }
    }

    public class ReuseWrappersAttribute : Attribute
    {
        public Type[] WrapperTypes { get; set; }

        public ReuseWrappersAttribute(params Type[] wrapperTypes)
        {
            WrapperTypes = wrapperTypes;
        }
    }

    [SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible"), AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class ExportWithKeyAttribute : ExportAttribute
    {
        /// <remarks>Specifies service key if <see cref="ExportAttribute.ContractName"/> is not specified.</remarks>
        public object ContractKey { get; set; }

        public ExportWithKeyAttribute(object contractKey, Type contractType)
            : base(contractType)
        {
            ContractKey = contractKey;
        }

        public ExportWithKeyAttribute(object contractKey) : this(contractKey, null) { }
    }

    /// <summary>Specifies to export all implemented contract types automatically.</summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ExportAllAttribute : Attribute
    {
        /// <summary>Default rule to check that type could be exported.</summary>
        public static Func<Type, bool> ExportedContractTypes = Registrator.DefaultServiceTypesForRegisterAll;

        /// <summary>Specifies service key if <see cref="ContractName"/> is not specified.</summary>
        public object ContractKey { get; set; }

        /// <summary>If specified has more priority over <see cref="ContractKey"/>.</summary>
        public string ContractName { get; set; }

        /// <summary>Excludes specified contract types.</summary>
        public Type[] Except { get; set; }

        /// <summary>Returns all contract types implemented by implementation type,
        /// that adhere to <see cref="ExportedContractTypes"/> rule, <see cref="Except"/> some specified.</summary>
        /// <param name="implementationType">To get contract types from.</param> <returns>Exported contract types.</returns>
        public IEnumerable<Type> GetContractTypes(Type implementationType)
        {
            var contractTypes = implementationType.GetImplementedTypes(TypeTools.IncludeFlags.SourceType).Where(ExportedContractTypes);
            return Except == null || Except.Length == 0 ? contractTypes : contractTypes.Except(Except);
        }
    }

    /// <summary>Specifies that class exporting static or instance method factories</summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class AsFactoryAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class AsWrapperAttribute : Attribute
    {
        public int ContractTypeGenericArgIndex { get; set; }
        public Type WrappedContractType { get; set; }

        public AsWrapperAttribute(int contractTypeGenericArgInsdex = 0)
        {
            ContractTypeGenericArgIndex = contractTypeGenericArgInsdex.ThrowIf(contractTypeGenericArgInsdex < 0);
        }

        public AsWrapperAttribute(Type wrappedContractType)
        {
            WrappedContractType = wrappedContractType.ThrowIfNull();
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class AsDecoratorAttribute : Attribute
    {
        /// <summary>If <see cref="ContractName"/> specified, it has more priority over <see cref="ContractKey"/>. </summary>
        public string ContractName { get; set; }

        public object ContractKey { get; set; }
        public Type ConditionType { get; set; }
    }

    public interface IDecoratorCondition
    {
        bool CanApply(Request request);
    }

    [SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible"), AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
    public class ImportWithKeyAttribute : ImportAttribute
    {
        public object ContractKey { get; set; }

        public ImportWithKeyAttribute(object contractKey, Type contractType = null)
            : base(contractType)
        {
            ContractKey = contractKey;
        }

        public ImportWithKeyAttribute(string contractKey, Type contractType = null)
            : base(contractKey, contractType)
        {
            ContractKey = contractKey;
        }

        public ImportWithKeyAttribute(Type contractType)
            : this(null, contractType) { }
    }

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class | // for Export 
        AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    public class WithMetadataAttribute : Attribute
    {
        public WithMetadataAttribute(object metadata)
        {
            Metadata = metadata.ThrowIfNull();
        }

        public readonly object Metadata;
    }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
    public class ImportExternalAttribute : Attribute
    {
        public Type ImplementationType { get; set; }
        public Type[] WithConstructor { get; set; }
        public object Metadata { get; set; }
        public object ContractKey { get; set; }
        public Type ContractType { get; set; }

        public ImportExternalAttribute(Type implementationType = null, Type[] withConstructor = null, object metadata = null,
            object contractKey = null, Type contractType = null)
        {
            ImplementationType = implementationType;
            WithConstructor = withConstructor;
            Metadata = metadata;
            ContractType = contractType;
            ContractKey = contractKey;
        }
    }

    #endregion
}