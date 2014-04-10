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

namespace DryIoc.MefAttributedModel
{
    /// <summary>
    /// Implements MEF Attributed Programming Model. Documentation is available at https://bitbucket.org/dadhi/dryioc/wiki/MefAttributedModel.
    /// TODO:
    /// - add: ImportAttribute.ContractType and AllowDefault support.
    /// </summary>
    public static class AttributedModel
    {
        // NOTE: Default reuse policy is Singleton, the same as in MEF.
        public static CreationPolicy DefaultCreationPolicy = CreationPolicy.Shared;

        public static Container WithAttributedModel(this Container container)
        {
            container.ResolutionRules.Swap(rules => rules
                .With(rules.ToResolveConstructorParameterServiceKey.Append(GetConstructorParameterServiceKeyOrDefault))
                .With(rules.ToResolvePropertyOrFieldWithServiceKey.Append(TryGetPropertyOrFieldServiceKey)));
            return container;
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
                RegisterExport(registrator, info);
        }

        public static void RegisterExport(this IRegistrator registrator, RegistrationInfo info)
        {
            var factory = new ReflectionFactory(info.Type, info.GetReuse(), FindSingleImportingConstructor, info.GetSetup());

            var exports = info.Exports;
            for (var i = 0; i < exports.Length; i++)
            {
                var export = exports[i];
                registrator.Register(factory, export.ServiceType, export.ServiceKeyInfo.Key, IfAlreadyRegistered.ThrowIfDuplicateKey);

                if (export.ServiceType.IsGenericType &&
                    export.ServiceType.GetGenericTypeDefinition() == typeof(IFactory<>))
                    RegisterFactory(registrator, info.Type, export);
            }
        }

        public static IEnumerable<RegistrationInfo> Scan(IEnumerable<Assembly> assemblies)
        {
            return assemblies.SelectMany(a => a.GetTypes())
                .Select(GetRegistrationInfoOrDefault)
                .Where(info => info != null);
        }

        public static RegistrationInfo GetRegistrationInfoOrDefault(Type type)
        {
            return !type.IsClass || type.IsAbstract ? null
                : GetRegistrationInfoOrDefault(type, GetAllExportRelatedAttributes(type));
        }

        public static RegistrationInfo GetRegistrationInfoOrDefault(Type type, object[] attributes)
        {
            if (attributes.Length == 0 ||
                attributes.IndexOf(a => a is ExportAttribute || a is ExportAllAttribute) == -1 ||
                attributes.IndexOf(a => a is PartNotDiscoverableAttribute) != -1)
                return null;

            var info = new RegistrationInfo { Type = type };

            for (var attributeIndex = 0; attributeIndex < attributes.Length; attributeIndex++)
            {
                var attribute = attributes[attributeIndex];
                if (attribute is ExportAttribute)
                {
                    var exportAttribute = (ExportAttribute)attribute;
                    var export = new ExportInfo(exportAttribute.ContractType ?? type,
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
                    var allContractTypes = exportAllAttribute.GetAllContractTypes(type);

                    if (type.IsGenericTypeDefinition)
                    {
                        var implTypeArgs = type.GetGenericArguments();
                        allContractTypes = allContractTypes.Where(t =>
                            t.IsGenericType && t.ContainsGenericParameters && t.ContainsAllGenericParameters(implTypeArgs))
                            .Select(t => t.GetGenericTypeDefinition());
                    }

                    var exportAllInfos = allContractTypes
                        .Select(t => new ExportInfo(t, exportAllAttribute.ContractName ?? exportAllAttribute.ContractKey))
                        .ToArray();

                    Throw.If(exportAllInfos.Length == 0, Error.EXPORT_ALL_EXPORTS_EMPTY_LIST_OF_TYPES,
                        type, allContractTypes);

                    if (info.Exports != null)
                        for (var index = 0; index < info.Exports.Length; index++)
                            if (!exportAllInfos.Contains(info.Exports[index])) // filtering out identical exports
                                exportAllInfos = exportAllInfos.AppendOrUpdate(info.Exports[index]);

                    info.Exports = exportAllInfos;
                }
                else if (attribute is PartCreationPolicyAttribute)
                {
                    info.IsSingleton = ((PartCreationPolicyAttribute)attribute).CreationPolicy == CreationPolicy.Shared;
                }
                else if (attribute is CreationPolicyAttribute)
                {
                    info.IsSingleton = ((CreationPolicyAttribute)attribute).CreationPolicy == CreationPolicy.Shared;
                }
                else if (attribute is ExportAsGenericWrapperAttribute)
                {
                    Throw.If(info.FactoryType != FactoryType.Service, Error.UNSUPPORTED_MULTIPLE_FACTORY_TYPES, type);
                    info.FactoryType = FactoryType.GenericWrapper;
                    var genericWrapperAttribute = ((ExportAsGenericWrapperAttribute)attribute);
                    info.GenericWrapper = new GenericWrapperInfo
                    {
                        ServiceTypeIndex = genericWrapperAttribute.ContractTypeArgIndex
                    };
                }
                else if (attribute is ExportAsDecoratorAttribute)
                {
                    Throw.If(info.FactoryType != FactoryType.Service, Error.UNSUPPORTED_MULTIPLE_FACTORY_TYPES, type);
                    var decorator = ((ExportAsDecoratorAttribute)attribute);
                    info.FactoryType = FactoryType.Decorator;
                    info.Decorator = new DecoratorInfo(
                        decorator.ShouldCompareMetadata,
                        decorator.ConditionType,
                        decorator.ContractName ?? decorator.ContractKey);
                }

                if (Attribute.IsDefined(attribute.GetType(), typeof(MetadataAttributeAttribute), true))
                {
                    Throw.If(info.MetadataAttributeIndex != -1, Error.UNSUPPORTED_MULTIPLE_METADATA, type);
                    info.MetadataAttributeIndex = attributeIndex;
                }
            }

            if (info.FactoryType == FactoryType.Decorator)
            {
                Throw.If(info.Decorator.ShouldCompareMetadata && info.MetadataAttributeIndex == -1,
                    Error.METADATA_FOR_DECORATOR_IS_NOT_FOUND, type);
                info.IsSingleton = false;
            }

            info.Exports.ThrowIfNull(Error.EXPORT_IS_REQUIRED, type);
            return info;
        }

        #region Implementation

        private static object[] GetAllExportRelatedAttributes(Type type)
        {
            var attributes = type.GetCustomAttributes(false);

            for (var baseType = type.BaseType;
                baseType != typeof(object) && baseType != null;
                baseType = baseType.BaseType)
                attributes = attributes.Append(GetInheritedExportAttributes(baseType));

            var interfaces = type.GetInterfaces();
            for (var i = 0; i < interfaces.Length; i++)
                attributes = attributes.Append(GetInheritedExportAttributes(interfaces[i]));

            return attributes;
        }

        private static object[] GetInheritedExportAttributes(Type type)
        {
            var exports = type.GetCustomAttributes(typeof(InheritedExportAttribute), false);
            for (var i = 0; i < exports.Length; i++)
            {
                var export = (InheritedExportAttribute)exports[i];
                if (export.ContractType == null)
                    exports[i] = new InheritedExportAttribute(export.ContractName, type);
            }
            return exports;
        }

        private static readonly MethodInfo _resolveMethod =
            typeof(Resolver).GetMethod("Resolve", new[] { typeof(IResolver), typeof(string), typeof(IfUnresolved) });

        private static readonly string _factoryMethodName = "Create";
        private static readonly string _dotFactoryMethodName = "." + _factoryMethodName;

        private static void RegisterFactory(IRegistrator registrator, Type factoryType, ExportInfo factoryExport)
        {
            var serviceType = factoryExport.ServiceType.GetGenericArguments()[0];
            var allMethods = factoryType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var factoryMethod = allMethods.FirstOrDefault(m =>
                (m.Name == _factoryMethodName || m.Name.EndsWith(_dotFactoryMethodName))
                && m.GetParameters().Length == 0 && m.ReturnType == serviceType)
                .ThrowIfNull();

            var attributes = factoryMethod.GetCustomAttributes(false);

            var exportInfo = GetRegistrationInfoOrDefault(serviceType, attributes);
            if (exportInfo == null)
                return;

            // Result expression is {container.Resolve<IFactory<TService>>(factoryName).Create()} 
            Func<Request, IRegistry, Expression> factoryCreateExpr = (request, registry) =>
                Expression.Call(
                    Expression.Call(_resolveMethod.MakeGenericMethod(factoryExport.ServiceType),
                        request.ResolvedExpressions.ToExpression(registry),
                        Expression.Constant(factoryExport.ServiceKeyInfo.Key, typeof(string)),
                        Expression.Constant(IfUnresolved.Throw, typeof(IfUnresolved))),
                    _factoryMethodName, null);

            var factory = new DelegateFactory(factoryCreateExpr,
                exportInfo.GetReuse(), exportInfo.GetSetup(attributes));

            for (var i = 0; i < exportInfo.Exports.Length; i++)
            {
                var export = exportInfo.Exports[i];
                registrator.Register(factory, export.ServiceType, export.ServiceKeyInfo.Key, IfAlreadyRegistered.ThrowIfDuplicateKey);
            }
        }

        #endregion

        #region Tools

        public static ConstructorInfo FindSingleImportingConstructor(Type type)
        {
            var constructors = type.GetConstructors();
            return constructors.Length == 1 ? constructors[0]
                : constructors.SingleOrDefault(x => Attribute.IsDefined(x, typeof(ImportingConstructorAttribute)))
                    .ThrowIfNull(Error.UNABLE_TO_FIND_SINGLE_CONSTRUCTOR_WITH_IMPORTING_ATTRIBUTE, type);
        }

        #endregion

        #region Rules

        public static object GetConstructorParameterServiceKeyOrDefault(ParameterInfo parameter, Request parent, IRegistry registry)
        {
            var attributes = parameter.GetCustomAttributes(false);
            if (attributes.Length == 0)
                return null;

            object key;
            if (TryGetServiceKeyFromImportAttribute(out key, attributes) ||
                TryGetServiceKeyWithMetadataAttribute(out key, parameter.ParameterType, parent, registry, attributes) ||
                TryGetServiceKeyFromImportOrExportAttribute(out key, parameter.ParameterType, registry, attributes))
                return key;
            return null;
        }

        public static bool TryGetPropertyOrFieldServiceKey(out object key, MemberInfo member, Request _, IRegistry registry)
        {
            key = null;
            var attributes = member.GetCustomAttributes(false);
            if (attributes.Length == 0)
                return false;

            return TryGetServiceKeyFromImportAttribute(out key, attributes) ||
                TryGetServiceKeyFromImportOrExportAttribute(out key,
                    member is PropertyInfo ? ((PropertyInfo)member).PropertyType : ((FieldInfo)member).FieldType,
                    registry, attributes);
        }

        public static bool TryGetServiceKeyFromImportAttribute(out object key, object[] attributes)
        {
            var import = GetSingleAttributeOrDefault<ImportAttribute>(attributes);
            key = import == null ? null
                : import.ContractName ??
                (import is ImportWithKeyAttribute ? ((ImportWithKeyAttribute)import).ContractKey : null);
            return import != null;
        }

        public static bool TryGetServiceKeyWithMetadataAttribute(out object key, Type contractType, Request parent, IRegistry registry, object[] attributes)
        {
            key = null;
            var import = GetSingleAttributeOrDefault<ImportWithMetadataAttribute>(attributes);
            if (import == null)
                return false;

            var serviceType = registry.GetWrappedServiceTypeOrSelf(contractType);
            var metadata = import.Metadata;

            var item = registry.GetAllFactories(serviceType).FirstOrDefault(kv => metadata.Equals(kv.Value.Setup.Metadata))
                .ThrowIfNull(Error.UNABLE_TO_FIND_DEPENDENCY_WITH_METADATA, serviceType, metadata, parent);
            key = item.Key;
            return true;
        }

        public static bool TryGetServiceKeyFromImportOrExportAttribute(out object key, Type contractType, IRegistry registry, object[] attributes)
        {
            key = null;
            var import = GetSingleAttributeOrDefault<ExportOnceAttribute>(attributes);
            if (import == null)
                return false;

            var serviceType = registry.GetWrappedServiceTypeOrSelf(contractType);
            var serviceName = import.ContractName;
            if (!registry.IsRegistered(serviceType, serviceName))
            {
                var implementationType = import.ImplementationType ?? serviceType;
                var reuse = import.CreationPolicy == CreationPolicy.Shared ? Reuse.Singleton : null;
                var getConstructor = import.ConstructorSignature != null
                    ? new GetConstructor(t => t.GetConstructor(import.ConstructorSignature)) : null;
                var setup = ServiceSetup.WithMetadata(import.Metadata);
                registry.Register(serviceType, implementationType, reuse, getConstructor, setup, serviceName);
            }

            key = serviceName;
            return true;
        }

        private static TAttribute GetSingleAttributeOrDefault<TAttribute>(object[] attributes) where TAttribute : Attribute
        {
            TAttribute attr = null;
            for (var i = 0; i < attributes.Length && attr == null; i++)
                attr = attributes[i] as TAttribute;
            return attr;
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

        public static readonly string METADATA_FOR_DECORATOR_IS_NOT_FOUND =
            "Exported Decorator should compare metadata BUT metadata is not found for {0}.";

        public static readonly string EXPORT_IS_REQUIRED =
            "At least one Export attributed should be defined for {0}.";

        public static readonly string EXPORT_ALL_EXPORTS_EMPTY_LIST_OF_TYPES =
            "Unable to get contract types for implementation {0} cause all of its implemented types where filtered out: {1}";
    }

    #region Registration Info DTOs
#pragma warning disable 659

    public sealed class RegistrationInfo
    {
        public Type Type;
        public ExportInfo[] Exports;
        public bool IsSingleton = AttributedModel.DefaultCreationPolicy == CreationPolicy.Shared;
        public int MetadataAttributeIndex = -1;
        public FactoryType FactoryType;
        public GenericWrapperInfo GenericWrapper;
        public DecoratorInfo Decorator;

        public IReuse GetReuse()
        {
            return IsSingleton ? Reuse.Singleton : Reuse.Transient;
        }

        public object GetMetadata(object[] attributes = null)
        {
            attributes = attributes ?? Type.GetCustomAttributes(false);
            var metadataAttribute = MetadataAttributeIndex == -1 ? null
                : attributes.ThrowIf(attributes.Length == 0)[MetadataAttributeIndex];
            var metadata = !(metadataAttribute is ExportWithMetadataAttribute) ? metadataAttribute
                : ((ExportWithMetadataAttribute)metadataAttribute).Metadata;
            return metadata;
        }

        public FactorySetup GetSetup(object[] attributes = null)
        {
            if (FactoryType == FactoryType.GenericWrapper)
                return GenericWrapper == null ? GenericWrapperSetup.Default : GenericWrapper.CreateSetup();

            if (FactoryType == FactoryType.Decorator)
                return Decorator == null ? DecoratorSetup.Default : Decorator.CreateSetup(GetMetadata(attributes));

            return ServiceSetup.WithMetadata(GetMetadata(attributes));
        }

        public override bool Equals(object obj)
        {
            var other = obj as RegistrationInfo;
            return other != null
                && other.Type == Type
                && other.IsSingleton == IsSingleton
                && other.FactoryType == FactoryType
                && Equals(other.GenericWrapper, GenericWrapper)
                && Equals(other.Decorator, Decorator)
                && other.Exports.SequenceEqual(Exports);
        }

        public StringBuilder AppendCode(StringBuilder code = null)
        {
            code = code ?? new StringBuilder();
            code.Append(
@"new RegistrationInfo {
    Type = ").AppendType(Type).Append(@",
    Exports = new[] {
        "); for (var i = 0; i < Exports.Length; i++)
                code = Exports[i].AppendCode(code).Append(@",
        "); code.Append(@"},
    IsSingleton = ").AppendBool(IsSingleton).Append(@",
    MetadataAttributeIndex = ").Append(MetadataAttributeIndex).Append(@",
    FactoryType = ").AppendEnum(typeof(FactoryType), FactoryType);
            if (GenericWrapper != null) code.Append(@",
    GenericWrapper = new GenericWrapperInfo { ServiceTypeIndex = ").Append(GenericWrapper.ServiceTypeIndex).Append(@" }");
            if (Decorator != null)
            {
                code.Append(@",
"); Decorator.AppendCode(code);
            }
            code.Append(@"
}");
            return code;
        }
    }

    public static class PrintCode
    {
        public static StringBuilder AppendBool(this StringBuilder code, bool x)
        {
            return code.Append(x ? "true" : "false");
        }

        public static StringBuilder AppendString(this StringBuilder code, string x)
        {
            return code.Append(x == null ? "null" : ("\"" + x + "\""));
        }

        public static StringBuilder AppendType(this StringBuilder code, Type x)
        {
            return code.Append(x == null ? "null" : "typeof(" + x.Print() + ")");
        }

        public static StringBuilder AppendEnum(this StringBuilder code, Type enumType, object enumValue)
        {
            return code.Append(enumType.Print() + "." + Enum.GetName(enumType, enumValue));
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
            if (type.IsEnum)
                return code.AppendEnum(type, x);

            if (ifNotRecognized != null)
                ifNotRecognized(code, x);
            else
                code.Append(x);

            return code;
        }
    }

    public sealed class ExportInfo
    {
        public ExportInfo() { }

        public ExportInfo(Type serviceType, object serviceKey = null)
        {
            ServiceType = serviceType;
            ServiceKeyInfo = ServiceKeyInfo.Of(serviceKey);
        }

        public Type ServiceType;
        public ServiceKeyInfo ServiceKeyInfo = ServiceKeyInfo.Default;

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

    public sealed class GenericWrapperInfo
    {
        public int ServiceTypeIndex;

        public GenericWrapperSetup CreateSetup()
        {
            return GenericWrapperSetup.With(SelectServiceType);
        }

        public override bool Equals(object obj)
        {
            var other = obj as GenericWrapperInfo;
            return other != null && other.ServiceTypeIndex == ServiceTypeIndex;
        }

        private Type SelectServiceType(Type[] typeArgs)
        {
            return typeArgs[ServiceTypeIndex];
        }
    }

    public sealed class DecoratorInfo
    {
        public DecoratorInfo() { }

        public DecoratorInfo(bool shouldCompareMetadata = false, Type conditionType = null, object serviceKey = null)
        {
            ShouldCompareMetadata = shouldCompareMetadata;
            ConditionType = conditionType;
            ServiceKeyInfo = ServiceKeyInfo.Of(serviceKey);
        }

        public bool ShouldCompareMetadata;
        public Type ConditionType;
        public ServiceKeyInfo ServiceKeyInfo = ServiceKeyInfo.Default;

        public DecoratorSetup CreateSetup(object metadata)
        {
            if (ConditionType != null)
                return DecoratorSetup.With(((IDecoratorCondition)Activator.CreateInstance(ConditionType)).Check);

            if (ShouldCompareMetadata || ServiceKeyInfo != null)
                return DecoratorSetup.With(request =>
                    (!ShouldCompareMetadata || Equals(metadata, request.ResolvedFactory.Setup.Metadata)) &&
                    (ServiceKeyInfo.Key == null || Equals(ServiceKeyInfo.Key, request.ServiceKey)));

            return DecoratorSetup.Default;
        }

        public override bool Equals(object obj)
        {
            var other = obj as DecoratorInfo;
            return other != null
                && other.ShouldCompareMetadata == ShouldCompareMetadata
                && other.ConditionType == ConditionType
                && Equals(other.ServiceKeyInfo.Key, ServiceKeyInfo.Key);
        }

        public StringBuilder AppendCode(StringBuilder code = null)
        {
            return (code ?? new StringBuilder())
                .Append(@"Decorator = new DecoratorInfo(")
                .AppendBool(ShouldCompareMetadata).Append(", ")
                .AppendType(ConditionType).Append(", ")
                .AppendObject(ServiceKeyInfo.Key).Append(")");
        }
    }

    public class ServiceKeyInfo
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

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ExportWithMetadataAttribute : Attribute
    {
        public object Metadata { get; set; }

        public ExportWithMetadataAttribute(object metadata)
        {
            Metadata = metadata.ThrowIfNull();
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
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
        public static Func<Type, bool> ExportedTypes = Registrator.RegisterAllDefaultTypes;

        /// <remarks>Specifies service key if <see cref="ContractName"/> is not specified.</remarks>
        public object ContractKey { get; set; }

        /// <remarks>If specified has more priority over <see cref="ContractKey"/>.</remarks>
        public string ContractName { get; set; }

        public Type[] Except { get; set; }

        public IEnumerable<Type> GetAllContractTypes(Type implementationType)
        {
            var contractTypes = implementationType.GetImplementedTypes(TypeTools.IncludeItself.AsFirst)
                .Where(ExportedTypes);
            return Except == null || Except.Length == 0 ? contractTypes : contractTypes.Except(Except);
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ExportAsGenericWrapperAttribute : Attribute
    {
        public int ContractTypeArgIndex { get; set; }

        public ExportAsGenericWrapperAttribute(int contractTypeArgIndex = 0)
        {
            ContractTypeArgIndex = contractTypeArgIndex.ThrowIf(contractTypeArgIndex < 0);
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ExportAsDecoratorAttribute : Attribute
    {
        /// <remarks>If specified has more priority over <see cref="ContractKey"/>.</remarks>
        public string ContractName { get; set; }

        public object ContractKey { get; set; }
        public bool ShouldCompareMetadata { get; set; }
        public Type ConditionType { get; set; }
    }

    public interface IDecoratorCondition
    {
        bool Check(Request request);
    }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ImportWithKeyAttribute : ImportAttribute
    {
        public object ContractKey { get; set; }

        public ImportWithKeyAttribute(object contractKey, Type contractType)
            : base(contractType)
        {
            ContractKey = contractKey;
        }

        public ImportWithKeyAttribute(object contractKey) : this(contractKey, null) { }
    }

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ImportWithMetadataAttribute : Attribute
    {
        public ImportWithMetadataAttribute(object metadata)
        {
            Metadata = metadata.ThrowIfNull();
        }

        public readonly object Metadata;
    }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ExportOnceAttribute : Attribute
    {
        public string ContractName { get; set; }

        public Type ImplementationType { get; set; }

        public CreationPolicy CreationPolicy { get; set; }

        public object Metadata { get; set; }

        public Type[] ConstructorSignature { get; set; }
    }

    public interface IFactory<T>
    {
        T Create();
    }

    /// <summary>
    /// You may use this attribute to specify CreationPolicy for <see cref="IFactory{T}"/> Create method.
    /// Or in place of <see cref="PartCreationPolicyAttribute"/> for exported classes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CreationPolicyAttribute : Attribute
    {
        public CreationPolicy CreationPolicy { get; private set; }

        public CreationPolicyAttribute(CreationPolicy policy)
        {
            CreationPolicy = policy;
        }
    }

    #endregion
}