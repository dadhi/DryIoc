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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace DryIoc.MefAttributedModel
{
    /// <summary>
    /// Implements MEF Attributed Programming Model. Documentation is available at https://bitbucket.org/dadhi/dryioc/wiki/MefAttributedModel.
    /// </summary>
    public static class AttributedModel
    {
        ///<remarks>Default reuse policy is Singleton, the same as in MEF.</remarks>
        public static Type DefaultReuseType = typeof(SingletonReuse);

        public readonly static Dictionary<Type, IReuse> SupportedReuseTypes = new Dictionary<Type, IReuse>
        {
            { typeof(SingletonReuse), Reuse.Singleton},
            { typeof(CurrentScopeReuse), Reuse.InCurrentScope },
            { typeof(ResolutionScopeReuse), Reuse.InResolutionScope }
        };

        public static Rules WithAttributedModel(this Rules rules)
        {
            // hello, Max!!! we are Martians.
            return rules.With(GetImportingConstructor, GetImportedParameter, GetImportedPropertiesAndFields);
        }

        public static Container WithAttributedModel(this Container container)
        {
            return container.WithNewRules(container.Rules.WithAttributedModel());
        }

        public static void RegisterExports(this IRegistrator registrator, params Type[] types)
        {
            registrator.RegisterExports(types
                .Select(GetRegistrationInfoOrDefault).Where(info => info != null));
        }

        public static void RegisterExports(this IRegistrator registrator, params Assembly[] assemblies)
        {
            registrator.RegisterExports(Scan(assemblies));
        }

        public static void RegisterExports(this IRegistrator registrator, IEnumerable<RegistrationInfo> infos)
        {
            foreach (var info in infos)
                RegisterInfo(registrator, info);
        }

        public static void RegisterInfo(this IRegistrator registrator, RegistrationInfo info)
        {
            var factory = info.CreateFactory();

            for (var i = 0; i < info.Exports.Length; i++)
            {
                var export = info.Exports[i];

                registrator.Register(factory, export.ServiceType,
                    export.ServiceKeyInfo.Key, IfAlreadyRegistered.ThrowIfDuplicateKey);

                if (export.ServiceType.GetGenericDefinitionOrNull() == typeof(IFactory<>))
                    RegisterFactory(registrator, info.ImplementationType, export);
            }
        }

        public static IEnumerable<RegistrationInfo> Scan(IEnumerable<Assembly> assemblies)
        {
            return assemblies.SelectMany(_getAssemblyTypes)
                .Select(GetRegistrationInfoOrDefault)
                .Where(info => info != null);
        }

        public static RegistrationInfo GetRegistrationInfoOrDefault(Type implementationType)
        {
            return implementationType.IsValueType() || implementationType.IsAbstract() ? null
                : GetRegistrationInfoOrDefault(implementationType, GetAllExportRelatedAttributes(implementationType));
        }

        public static RegistrationInfo GetRegistrationInfoOrDefault(Type implementationType, Attribute[] attributes)
        {
            if (attributes.Length == 0 ||
                attributes.IndexOf(a => a is ExportAttribute || a is ExportAllAttribute) == -1 ||
                attributes.IndexOf(a => a is PartNotDiscoverableAttribute) != -1)
                return null;

            var info = new RegistrationInfo { ImplementationType = implementationType, ReuseType = DefaultReuseType };

            for (var attributeIndex = 0; attributeIndex < attributes.Length; attributeIndex++)
            {
                var attribute = attributes[attributeIndex];
                if (attribute is ExportAttribute)
                {
                    var exportAttribute = (ExportAttribute)attribute;
                    var export = new ExportInfo(exportAttribute.ContractType ?? implementationType,
                        exportAttribute.ContractName ??
                        (attribute is ExportWithKeyAttribute ? ((ExportWithKeyAttribute)attribute).ContractKey : null));

                    if (info.Exports == null)
                        info.Exports = new[] { export };
                    else if (!info.Exports.Contains(export))
                        info.Exports = info.Exports.AppendOrUpdate(export);
                }
                else if (attribute is ExportAllAttribute)
                {
                    var exportAllAttribute = (ExportAllAttribute)attribute;
                    var allContractTypes = exportAllAttribute.GetAllContractTypes(implementationType);

                    if (implementationType.IsGenericDefinition())
                    {
                        var implTypeArgs = implementationType.GetGenericParamsAndArgs();
                        allContractTypes = allContractTypes
                            .Where(t => t.ContainsAllGenericParameters(implTypeArgs))
                            .Select(t => t.GetGenericDefinitionOrNull());
                    }

                    var exportAllInfos = allContractTypes
                        .Select(t => new ExportInfo(t, exportAllAttribute.ContractName ?? exportAllAttribute.ContractKey))
                        .ToArray();

                    Throw.If(exportAllInfos.Length == 0, Error.EXPORT_ALL_EXPORTS_EMPTY_LIST_OF_TYPES,
                        implementationType, allContractTypes);

                    if (info.Exports != null)
                        for (var index = 0; index < info.Exports.Length; index++)
                            if (!exportAllInfos.Contains(info.Exports[index])) // filtering out identical exports
                                exportAllInfos = exportAllInfos.AppendOrUpdate(info.Exports[index]);

                    info.Exports = exportAllInfos;
                }
                else if (attribute is PartCreationPolicyAttribute)
                {
                    var creationPolicy = ((PartCreationPolicyAttribute)attribute).CreationPolicy;
                    info.ReuseType = creationPolicy == CreationPolicy.Shared ? typeof(SingletonReuse) : null;
                }
                else if (attribute is ReuseAttribute)
                {
                    info.ReuseType = ((ReuseAttribute)attribute).ReuseType;
                }
                else if (attribute is AsWrapperAttribute)
                {
                    Throw.If(info.FactoryType != FactoryType.Service, Error.UNSUPPORTED_MULTIPLE_FACTORY_TYPES, implementationType);
                    info.FactoryType = FactoryType.Wrapper;
                    var genericWrapperAttribute = ((AsWrapperAttribute)attribute);
                    info.Wrapper = new WrapperInfo
                    {
                        WrappedServiceType = genericWrapperAttribute.WrappedContractType,
                        WrappedServiceTypeGenericArgIndex = genericWrapperAttribute.ContractTypeGenericArgIndex
                    };
                }
                else if (attribute is AsDecoratorAttribute)
                {
                    Throw.If(info.FactoryType != FactoryType.Service, Error.UNSUPPORTED_MULTIPLE_FACTORY_TYPES, implementationType);
                    var decorator = ((AsDecoratorAttribute)attribute);
                    info.FactoryType = FactoryType.Decorator;
                    info.Decorator = new DecoratorInfo(decorator.ConditionType, decorator.ContractName ?? decorator.ContractKey);
                }

                if (attribute.GetType().GetAttributes(typeof(MetadataAttributeAttribute), true).Any())
                {
                    Throw.If(info.HasMetadataAttribute, Error.UNSUPPORTED_MULTIPLE_METADATA, implementationType);
                    info.HasMetadataAttribute = true;
                }
            }

            if (info.FactoryType == FactoryType.Decorator)
                info.ReuseType = null;

            info.Exports.ThrowIfNull(Error.EXPORT_IS_REQUIRED, implementationType);
            return info;
        }

        #region Tools

        public static ConstructorInfo GetImportingConstructor(Type implementationType, Request request)
        {
            var constructors = implementationType.GetAllConstructors().ToArrayOrSelf();
            return constructors.Length == 1 ? constructors[0]
                : constructors.SingleOrDefault(x => x.GetAttributes(typeof(ImportingConstructorAttribute)).Any())
                    .ThrowIfNull(Error.UNABLE_TO_FIND_SINGLE_CONSTRUCTOR_WITH_IMPORTING_ATTRIBUTE, implementationType);
        }

        public static IReuse GetReuseByType(Type reuseType)
        {
            if (reuseType == null)
                return null;
            IReuse reuse;
            var supported = SupportedReuseTypes.TryGetValue(reuseType, out reuse);
            reuseType.ThrowIf(!supported, Error.UNSUPPORTED_REUSE_TYPE);
            return reuse;
        }

        #endregion

        #region Rules

        public static ParameterServiceInfo GetImportedParameter(ParameterInfo parameter, Request request)
        {
            var serviceInfo = ParameterServiceInfo.Of(parameter);
            var attrs = parameter.GetAttributes().ToArray();
            return attrs.Length == 0 ? serviceInfo
                : serviceInfo.WithDetails(GetFirstImportDetailsOrNull(parameter.ParameterType, attrs, request), request);
        }

        public static readonly PropertiesAndFieldsSelector GetImportedPropertiesAndFields =
            PropertiesAndFields.All(PropertiesAndFields.Include.All, GetImportedPropertiesAndFieldsOnly);

        private static PropertyOrFieldServiceInfo GetImportedPropertiesAndFieldsOnly(MemberInfo member, Request request)
        {
            var attributes = member.GetAttributes().ToArray();
            var details = attributes.Length == 0 ? null 
                : GetFirstImportDetailsOrNull(member.GetPropertyOrFieldType(), attributes, request);
            return details == null ? null : PropertyOrFieldServiceInfo.Of(member).WithDetails(details, request);
        }

        public static ServiceInfoDetails GetFirstImportDetailsOrNull(Type type, Attribute[] attributes, Request request)
        {
            return GetImportDetails(type, attributes, request) ?? GetImportExternalDetails(type, attributes, request);
        }

        public static ServiceInfoDetails GetImportDetails(Type reflectedType, Attribute[] attributes, Request request)
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

        public static object GetServiceKeyWithMetadataAttribute(Type reflectedType, Attribute[] attributes, Request request)
        {
            var meta = GetSingleAttributeOrDefault<WithMetadataAttribute>(attributes);
            if (meta == null)
                return null;

            var registry = request.Registry;
            reflectedType = registry.GetWrappedServiceType(reflectedType);
            var metadata = meta.Metadata;
            var factory = registry.GetAllServiceFactories(reflectedType)
                .FirstOrDefault(f => metadata.Equals(f.Value.Setup.Metadata))
                .ThrowIfNull(Error.UNABLE_TO_FIND_DEPENDENCY_WITH_METADATA, reflectedType, metadata, request);

            return factory.Key;
        }

        public static ServiceInfoDetails GetImportExternalDetails(Type serviceType, Attribute[] attributes, Request request)
        {
            var import = GetSingleAttributeOrDefault<ImportExternalAttribute>(attributes);
            if (import == null)
                return null;

            var registry = request.Registry;
            serviceType = import.ContractType ?? registry.GetWrappedServiceType(serviceType);
            var serviceKey = import.ContractKey;

            if (!registry.IsRegistered(serviceType, serviceKey))
            {
                var implementationType = import.ImplementationType ?? serviceType;

                var reuseAttr = GetSingleAttributeOrDefault<ReuseAttribute>(attributes);
                var reuse = GetReuseByType(reuseAttr == null ? DefaultReuseType : reuseAttr.ReuseType);

                var withConstructor = import.WithConstructor == null ? null
                    : (ConstructorSelector)((t, _) => t.GetConstructorOrNull(args: import.WithConstructor));

                registry.Register(serviceType, implementationType,
                    reuse, null, Setup.With(withConstructor, metadata: import.Metadata), 
                    serviceKey, IfAlreadyRegistered.KeepRegistered);
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

        private static readonly MethodInfo _resolveMethod = typeof(Resolver)
            .GetDeclaredMethod("Resolve", new[] { typeof(IResolver), typeof(object), typeof(IfUnresolved), typeof(Type) });

        private static readonly Func<Assembly, IEnumerable<Type>> _getAssemblyTypes =
            ExpressionTools.GetMethodDelegate<Assembly, IEnumerable<Type>>("GetTypes").ThrowIfNull();

        private const string _factoryMethodName = "Create";
        private const string _dotFactoryMethodName = "." + _factoryMethodName;

        private static void RegisterFactory(IRegistrator registrator, Type factoryType, ExportInfo factoryExport)
        {
            var serviceType = factoryExport.ServiceType.GetGenericParamsAndArgs()[0];
            var allMethods = factoryType.GetAll(_ => _.DeclaredMethods);
            var factoryMethod = allMethods.FirstOrDefault(m =>
                (m.Name == _factoryMethodName || m.Name.EndsWith(_dotFactoryMethodName))
                && m.GetParameters().Length == 0 && m.ReturnType == serviceType)
                .ThrowIfNull();

            var attributes = factoryMethod.GetCustomAttributes(false).Cast<Attribute>().ToArray();

            var info = GetRegistrationInfoOrDefault(serviceType, attributes);
            if (info == null)
                return;

            // Result expression is {container.Resolve<IFactory<TService>>(factoryName).Create()} 
            Func<Request, Expression> factoryCreateExpr = request =>
                Expression.Call(
                    Expression.Call(_resolveMethod.MakeGenericMethod(factoryExport.ServiceType),
                        request.State.GetOrAddItemExpression(request),
                        Expression.Constant(factoryExport.ServiceKeyInfo.Key, typeof(string)),
                        Expression.Constant(IfUnresolved.Throw, typeof(IfUnresolved)),
                        Expression.Constant(null, typeof(Type))),
                    _factoryMethodName, null);

            var factory = new ExpressionFactory(factoryCreateExpr, GetReuseByType(info.ReuseType), info.GetSetup(attributes));

            for (var i = 0; i < info.Exports.Length; i++)
            {
                var exp = info.Exports[i];
                registrator.Register(factory, exp.ServiceType, exp.ServiceKeyInfo.Key, IfAlreadyRegistered.ThrowIfDuplicateKey);
            }
        }

        #endregion
    }

    public static class Error
    {
        public static readonly string UNABLE_TO_FIND_SINGLE_CONSTRUCTOR_WITH_IMPORTING_ATTRIBUTE =
            "Unable to find single constructor with " + typeof(ImportingConstructorAttribute) + " in {0}.";

        public static readonly string UNABLE_TO_FIND_DEPENDENCY_WITH_METADATA =
            "Unable to resolve dependency {0} with metadata [{1}] in {2}";

        public static readonly string UNSUPPORTED_MULTIPLE_METADATA =
            "Multiple associated metadata found while exporting {0}." + Environment.NewLine +
            "Only single metadata is supported per implementation type, please remove the rest.";

        public static readonly string UNSUPPORTED_MULTIPLE_FACTORY_TYPES =
            "Found multiple factory types associated with exported {0}. " +
            "Only single ExportAs.. attribute is supported, please remove the rest.";

        public static readonly string EXPORT_IS_REQUIRED =
            "At least one Export attributed should be defined for {0}.";

        public static readonly string EXPORT_ALL_EXPORTS_EMPTY_LIST_OF_TYPES =
            "Unable to get contract types for implementation {0} cause all of its implemented types where filtered out: {1}";

        public static readonly string UNSUPPORTED_REUSE_TYPE =
            "Attributed model does not support reuse type {0}.";

        public static readonly string EXPORTED_NONGENERIC_WRAPPER_NO_WRAPPED_TYPE = 
            "Exported non-generic wrapper type {0} requires wrapped service type to be specified, but it is null, " +
            "and instead generic argument index is set to {1}.";

        public static readonly string EXPORTED_GENERIC_WRAPPER_BAD_ARG_INDEX = 
            "Exported generic wrapper type {0} specifies generic argument index {1} outside of argument list size.";
    }

    public static class PrintCode
    {
        public static StringBuilder AppendBool(this StringBuilder code, bool x)
        {
            return code.Append(x ? "true" : "false");
        }

        public static StringBuilder AppendString(this StringBuilder code, string x)
        {
            return x == null ? code.Append("null") : code.Append('"').Append(x).Append('"');
        }

        public static StringBuilder AppendType(this StringBuilder code, Type x)
        {
            return x == null ? code.Append("null") : code.Append("typeof(").Print(x).Append(')');
        }

        public static StringBuilder AppendEnum(this StringBuilder code, Type enumType, object enumValue)
        {
            return code.Print(enumType).Append('.').Append(Enum.GetName(enumType, enumValue));
        }

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

    public sealed class RegistrationInfo
    {
        public ExportInfo[] Exports;

        public Type ImplementationType;
        public string ImplementationTypeFullName;

        public Type ReuseType;
        public bool HasMetadataAttribute;
        public FactoryType FactoryType;
        public DecoratorInfo Decorator;
        public WrapperInfo Wrapper;

        public Factory CreateFactory()
        {
            return new ReflectionFactory(ImplementationType, AttributedModel.GetReuseByType(ReuseType), GetSetup());
        }

        public IReuse GetReuse()
        {
            return ReuseType == null ? null : AttributedModel.SupportedReuseTypes[ReuseType];
        }

        public FactorySetup GetSetup(Attribute[] attributes = null)
        {
            if (FactoryType == FactoryType.Wrapper)
                return Wrapper == null ? WrapperSetup.Default : Wrapper.GetSetup();

            if (FactoryType == FactoryType.Decorator)
                return Decorator == null ? DecoratorSetup.Default
                    : HasMetadataAttribute ? Decorator.GetSetup(() => GetMetadata(attributes))
                    : Decorator.GetSetup();

            if (HasMetadataAttribute)
                return Setup.With(lazyMetadata: () => GetMetadata(attributes));

            return Setup.Default;
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

    public sealed class ExportInfo
    {
        public Type ServiceType;
        public string ServiceTypeFullName;

        public ServiceKeyInfo ServiceKeyInfo = ServiceKeyInfo.Default;

        public ExportInfo() { } // Default constructor is usually required by deserializer.

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

        public StringBuilder AppendCode(StringBuilder code = null)
        {
            return (code ?? new StringBuilder())
                .Append(@"new ExportInfo(").AppendType(ServiceType).Append(@", ")
                .AppendObject(ServiceKeyInfo.Key).Append(@")");
        }
    }

    /// <summary>Describes <see cref="WrapperSetup"/> in serializable way.</summary>
    public sealed class WrapperInfo
    {
        public Type WrappedServiceType;
        public int WrappedServiceTypeGenericArgIndex;

        public WrapperSetup GetSetup()
        {
            return WrapperSetup.With(getWrappedServiceType: GetWrappedServiceType);
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
                Error.EXPORTED_NONGENERIC_WRAPPER_NO_WRAPPED_TYPE, WrappedServiceTypeGenericArgIndex);

            var typeArgs = wrapperType.GetGenericParamsAndArgs();
            wrapperType.ThrowIf(WrappedServiceTypeGenericArgIndex > typeArgs.Length - 1,
                Error.EXPORTED_GENERIC_WRAPPER_BAD_ARG_INDEX, WrappedServiceTypeGenericArgIndex);

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

        public DecoratorSetup GetSetup(Func<object> getMetadata = null)
        {
            if (ConditionType != null)
                return DecoratorSetup.WithCondition(((IDecoratorCondition)Activator.CreateInstance(ConditionType)).CanApply);

            if (ServiceKeyInfo != ServiceKeyInfo.Default || getMetadata != null)
                return DecoratorSetup.WithCondition(request =>
                    (ServiceKeyInfo.Key == null || Equals(ServiceKeyInfo.Key, request.ServiceKey)) &&
                    (getMetadata == null || Equals(getMetadata(), request.ResolvedFactory.Setup.Metadata)));

            return DecoratorSetup.Default;
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

    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property,
        AllowMultiple = false, Inherited = false)]
    public class ReuseAttribute : Attribute
    {
        public readonly Type ReuseType;

        public ReuseAttribute(Type reuseType)
        {
            ReuseType = reuseType;
        }
    }

    public class TransientReuseAttribute : ReuseAttribute
    {
        public TransientReuseAttribute() : base(null) { }
    }

    public class SingletonReuseAttribute : ReuseAttribute
    {
        public SingletonReuseAttribute() : base(typeof(SingletonReuse)) { }
    }

    public class CurrentScopeReuseAttribute : ReuseAttribute
    {
        public CurrentScopeReuseAttribute() : base(typeof(CurrentScopeReuse)) { }
    }

    public class ResolutionScopeReuseAttribute : ReuseAttribute
    {
        public ResolutionScopeReuseAttribute() : base(typeof(ResolutionScopeReuse)) { }
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

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ExportAllAttribute : Attribute
    {
        public static Func<Type, bool> ExportedTypes = Registrator.DefaultImplementedTypesForRegisterAll;

        /// <remarks>Specifies service key if <see cref="ContractName"/> is not specified.</remarks>
        public object ContractKey { get; set; }

        /// <remarks>If specified has more priority over <see cref="ContractKey"/>.</remarks>
        public string ContractName { get; set; }

        public Type[] Except { get; set; }

        public IEnumerable<Type> GetAllContractTypes(Type implementationType)
        {
            var contractTypes = implementationType.GetImplementedTypes(TypeTools.IncludeFlags.SourceType).Where(ExportedTypes);
            return Except == null || Except.Length == 0 ? contractTypes : contractTypes.Except(Except);
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
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

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class AsDecoratorAttribute : Attribute
    {
        /// <remarks> If <see cref="ContractName"/> specified, it has more priority over <see cref="ContractKey"/>. </remarks>
        public string ContractName { get; set; }
        public object ContractKey { get; set; }
        public Type ConditionType { get; set; }
    }

    public interface IDecoratorCondition
    {
        bool CanApply(Request request);
    }

    [SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible"), AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
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
        AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, // for Import
        AllowMultiple = false, Inherited = false)]
    public class WithMetadataAttribute : Attribute
    {
        public WithMetadataAttribute(object metadata)
        {
            Metadata = metadata.ThrowIfNull();
        }

        public readonly object Metadata;
    }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property,
        AllowMultiple = false, Inherited = false)]
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

    public interface IFactory<T>
    {
        T Create();
    }

    #endregion
}