// TODO:
// - Add ExportFactory.
// - Move tests to separate CUT.dll to test Import attributes.
// - Recognize export of Factory implementation.

//#define MEF_IS_AVAILABLE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DryIoc.AttributedRegistration
{
#if MEF_IS_AVAILABLE
	using System.ComponentModel.Composition;
#endif

    public static class AttributedRegistrator
    {
        public static Action<IRegistry> DefaultSetup = SetupContainerToUseImportAttributes;

        public static void SetupContainerToUseImportAttributes(IRegistry source)
        {
            Container.DefaultSetup(source);
            source.ResolutionRules.UseImportExportAttributes();
        }

        public static void UseImportExportAttributes(this ResolutionRules rules)
        {
            rules.ConstructorParameters = rules.ConstructorParameters.Append(GetConstructorParameterServiceKeyOrDefault);
            rules.PropertiesAndFields = rules.PropertiesAndFields.Append(TryGetPropertyOrFieldServiceKey);
        }

        public static readonly bool ExportedServiceIsSingletonByDefault = true;

        public static void RegisterExported(this IRegistrator registrator, params Type[] types)
        {
            registrator.RegisterExported(ScanTypes(types));
        }

        public static void RegisterExported(this IRegistrator registrator, params Assembly[] assemblies)
        {
            registrator.RegisterExported(ScanAssemblies(assemblies));
        }

        public static void RegisterExported(this IRegistrator registrator, IEnumerable<RegistrationInfo> infos)
        {
            foreach (var info in infos)
            {
                object metadata = null;
                if (info.MetadataAttributeIndex != -1)
                    metadata = FindMetadata(info.ImplementationType, info.MetadataAttributeIndex);

                var setup = info.CreateSetup(metadata);
                var reuse = info.IsSingleton ? Reuse.Singleton : Reuse.Transient;
                var factory = new ReflectionFactory(info.ImplementationType, reuse, FindSingleImportingConstructor, setup);

                var exports = info.Exports;
                for (var i = 0; i < exports.Length; i++)
                {
                    var export = exports[i];
                    registrator.Register(factory, export.ServiceType, export.ServiceName);
                }
            }
        }

        public static IEnumerable<RegistrationInfo> ScanAssemblies(IEnumerable<Assembly> assemblies)
        {
            return ScanTypes(assemblies.SelectMany(a => a.GetTypes()));
        }

        public static IEnumerable<RegistrationInfo> ScanTypes(IEnumerable<Type> types)
        {
            foreach (var type in types.Where(TypeCouldBeExported))
            {
                var attributes = type.GetCustomAttributes(false);

                var info = new RegistrationInfo { ImplementationType = type };

                for (var attributeIndex = 0; attributeIndex < attributes.Length; attributeIndex++)
                {
                    var attribute = attributes[attributeIndex];
                    if (attribute is ExportAttribute)
                    {
                        var exportAttribute = (ExportAttribute)attribute;
                        var export = new ExportInfo
                        {
                            ServiceType = exportAttribute.ContractType ?? type,
                            ServiceName = exportAttribute.ContractName
                        };

                        if (info.Exports == null)
                            info.Exports = new[] { export };
                        else if (!info.Exports.Contains(export))
                            info.Exports = info.Exports.AppendOrUpdate(export);
                    }
                    else if (attribute is ExportAllAttribute)
                    {
                        var exportAllAttribute = (ExportAllAttribute)attribute;
                        var exportAllInfos = exportAllAttribute.SelectServiceTypes(type)
                            .Select(t => new ExportInfo { ServiceType = t, ServiceName = exportAllAttribute.ContractName })
                            .ToArray();

                        if (info.Exports != null) // eliminating identical exports
                            for (var index = 0; index < info.Exports.Length; index++)
                                if (!exportAllInfos.Contains(info.Exports[index]))
                                    exportAllInfos = exportAllInfos.AppendOrUpdate(info.Exports[index]);

                        info.Exports = exportAllInfos;
                    }
                    else if (attribute is PartCreationPolicyAttribute)
                    {
                        info.IsSingleton = ((PartCreationPolicyAttribute)attribute).CreationPolicy == CreationPolicy.Shared;
                    }
                    else if (attribute is ExportAsGenericWrapperAttribute)
                    {
                        Throw.If(info.FactoryType != FactoryType.Service, Error.UNSUPPORTED_MULTIPLE_FACTORY_TYPES, type);
                        info.FactoryType = FactoryType.GenericWrapper;
                        var genericWrapperAttribute = ((ExportAsGenericWrapperAttribute)attribute);
                        info.GenericWrapper = new GenericWrapperInfo { ServiceTypeIndex = genericWrapperAttribute.ServiceTypeArgIndex };
                    }
                    else if (attribute is ExportAsDecoratorAttribute)
                    {
                        Throw.If(info.FactoryType != FactoryType.Service, Error.UNSUPPORTED_MULTIPLE_FACTORY_TYPES, type);
                        var decorator = ((ExportAsDecoratorAttribute)attribute);
                        info.FactoryType = FactoryType.Decorator;
                        info.Decorator = new DecoratorInfo
                        {
                            ServiceName = decorator.ContractName,
                            ShouldCompareMetadata = decorator.ShouldCompareMetadata,
                            ConditionType = decorator.ConditionType
                        };
                    }

                    if (Attribute.IsDefined(attribute.GetType(), typeof(MetadataAttributeAttribute), false))
                    {
                        Throw.If(info.MetadataAttributeIndex != -1, Error.UNSUPPORTED_MULTIPLE_METADATA, type);
                        info.MetadataAttributeIndex = attributeIndex;
                    }
                }

                if (info.FactoryType == FactoryType.Decorator)
                {
                    Throw.If(info.Decorator.ShouldCompareMetadata && info.MetadataAttributeIndex == -1, Error.METADATA_FOR_DECORATOR_IS_NOT_FOUND, type);
                    info.IsSingleton = false; // Decorator could not be reused based on the definition, so we are ignoring reuse setting.
                }

                info.Exports.ThrowIfNull(Error.EXPORT_IS_REQUIRED, type);
                yield return info;
            }
        }

        #region Tools

        public static bool TypeCouldBeExported(Type type)
        {
            return type.IsClass && !type.IsAbstract &&
                (Attribute.IsDefined(type, typeof(ExportAttribute), false) ||
                Attribute.IsDefined(type, typeof(ExportAllAttribute), false));
        }

        public static ConstructorInfo FindSingleImportingConstructor(Type type)
        {
            return type.GetConstructors()
                .SingleOrDefault(x => Attribute.IsDefined(x, typeof(ImportingConstructorAttribute)))
                .ThrowIfNull(Error.UNABLE_TO_FIND_SINGLE_CONSTRUCTOR_WITH_IMPORTING_ATTRIBUTE, type);
        }

        public static object FindMetadata(Type type, int metadataAttributeIndex)
        {
            var attributes = type.GetCustomAttributes(false);
            var metadataAttribute = attributes[metadataAttributeIndex];
            var withMetadataAttribute = metadataAttribute as ExportWithMetadataAttribute;
            return withMetadataAttribute != null ? withMetadataAttribute.Metadata : metadataAttribute;
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

            return TryGetServiceKeyFromImportAttribute(out key, attributes)
                || TryGetServiceKeyFromImportOrExportAttribute(out key, member.GetMemberType(), registry, attributes);
        }

        public static bool TryGetServiceKeyFromImportAttribute(out object key, object[] attributes)
        {
            var import = GetSingleAttributeOrNull<ImportAttribute>(attributes);
            key = import == null ? null : import.ContractName;
            return import != null;
        }

        public static bool TryGetServiceKeyWithMetadataAttribute(out object key, Type contractType, Request parent, IRegistry registry, object[] attributes)
        {
            key = null;
            var import = GetSingleAttributeOrNull<ImportWithMetadataAttribute>(attributes);
            if (import == null)
                return false;

            var serviceType = registry.GetWrappedServiceTypeOrSelf(contractType);
            var metadata = import.Metadata;
            key = registry.GetKeys(serviceType, factory => metadata.Equals(factory.Setup.Metadata)).FirstOrDefault()
                .ThrowIfNull(Error.UNABLE_TO_FIND_DEPENDENCY_WITH_METADATA, serviceType, metadata, parent);
            return true;
        }

        public static bool TryGetServiceKeyFromImportOrExportAttribute(out object key, Type contractType, IRegistry registry, object[] attributes)
        {
            key = null;
            var import = GetSingleAttributeOrNull<ImportOrExportAttribute>(attributes);
            if (import == null)
                return false;

            var serviceType = registry.GetWrappedServiceTypeOrSelf(contractType);
            var serviceName = import.ContractName;
            if (!registry.IsRegistered(serviceType, serviceName))
            {
                var implementationType = import.ImplementationType ?? serviceType;
                var reuse = import.CreationPolicy == CreationPolicy.Shared ? Reuse.Singleton : null;
                SelectConstructor withConstructor = t => t.GetConstructor(import.ConstructorSignature);
                var setup = ServiceSetup.WithMetadata(import.Metadata);
                registry.Register(serviceType, implementationType, reuse, withConstructor, setup, serviceName);
            }

            key = serviceName;
            return true;
        }

        private static TAttribute GetSingleAttributeOrNull<TAttribute>(object[] attributes) where TAttribute : Attribute
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
            "Multiple associated metadata found while exporting {0}. " +
            "Only single metadata is supported per implementation type, please remove the rest.";

        public static readonly string UNSUPPORTED_MULTIPLE_FACTORY_TYPES =
            "Found multiple factory types associated with exported {0}. " +
            "Only single ExportAs.. attribute is supported, please remove the rest.";

        public static readonly string METADATA_FOR_DECORATOR_IS_NOT_FOUND =
            "Exported Decorator should compare metadata BUT metadata is not found for {0}.";

        public static readonly string EXPORT_IS_REQUIRED =
            "At least one Export attributed should be defined for {0}.";
    }

#pragma warning disable 659

    [Serializable]
    public sealed class RegistrationInfo
    {
        public Type ImplementationType;
        public ExportInfo[] Exports;
        public bool IsSingleton = AttributedRegistrator.ExportedServiceIsSingletonByDefault;
        public int MetadataAttributeIndex = -1;

        public FactoryType FactoryType;
        public GenericWrapperInfo GenericWrapper;
        public DecoratorInfo Decorator;

        public FactorySetup CreateSetup(object metadata)
        {
            if (FactoryType == FactoryType.GenericWrapper)
                return GenericWrapper == null ? GenericWrapperSetup.Default : GenericWrapper.CreateSetup();

            if (FactoryType == FactoryType.Decorator)
                return Decorator == null ? DecoratorSetup.Default : Decorator.CreateSetup(metadata);

            return ServiceSetup.WithMetadata(metadata);
        }

        public override bool Equals(object obj)
        {
            var other = obj as RegistrationInfo;
            return other != null
                && other.ImplementationType == ImplementationType
                && other.IsSingleton == IsSingleton
                && other.FactoryType == FactoryType
                && Equals(other.GenericWrapper, GenericWrapper)
                && Equals(other.Decorator, Decorator)
                && other.Exports.SequenceEqual(Exports);
        }
    }

    [Serializable]
    public sealed class ExportInfo
    {
        public Type ServiceType;
        public string ServiceName;

        public override bool Equals(object obj)
        {
            var other = obj as ExportInfo;
            return other != null && other.ServiceType == ServiceType && other.ServiceName == ServiceName;
        }
    }

    [Serializable]
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

    [Serializable]
    public sealed class DecoratorInfo
    {
        public string ServiceName;
        public bool ShouldCompareMetadata;
        public Type ConditionType;

        public DecoratorSetup CreateSetup(object metadata)
        {
            if (ConditionType != null)
                return DecoratorSetup.With(((IDecoratorCondition)Activator.CreateInstance(ConditionType)).Check);

            if (ShouldCompareMetadata || ServiceName != null)
                return DecoratorSetup.With(request =>
                    (!ShouldCompareMetadata || Equals(metadata, request.Metadata)) &&
                    (ServiceName == null || ServiceName.Equals(request.ServiceKey)));

            return DecoratorSetup.Default;
        }

        public override bool Equals(object obj)
        {
            var other = obj as DecoratorInfo;
            return other != null
                && other.ServiceName == ServiceName
                && other.ShouldCompareMetadata == ShouldCompareMetadata
                && other.ConditionType == ConditionType;
        }
    }

#pragma warning restore 659

#if !MEF_IS_AVAILABLE
    #region Defining MEF attributes
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ExportAttribute : Attribute
    {
        public string ContractName { get; set; }

        public Type ContractType { get; set; }

        public ExportAttribute()
        {
        }

        public ExportAttribute(Type contractType)
            : this(null, contractType)
        {
        }

        public ExportAttribute(string contractName)
            : this(contractName, null)
        {
        }

        public ExportAttribute(string contractName, Type contractType)
        {
            ContractType = contractType;
            ContractName = contractName;
        }
    }

    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    public class ImportingConstructorAttribute : Attribute
    {
    }

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
    public class PartCreationPolicyAttribute : Attribute
    {
        public CreationPolicy CreationPolicy { get; set; }

        public PartCreationPolicyAttribute(CreationPolicy policy)
        {
            CreationPolicy = policy;
        }
    }

    public enum CreationPolicy
    {
        Shared,
        NonShared
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class MetadataAttributeAttribute : Attribute
    {
    }
    #endregion
#endif

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ExportWithMetadataAttribute : Attribute
    {
        public object Metadata { get; set; }

        public ExportWithMetadataAttribute(object metadata)
        {
            Metadata = metadata.ThrowIfNull();
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ExportAllAttribute : Attribute
    {
        public static Func<Type, bool> ExportedTypes = Registrator.PublicTypes;

        public Type[] Except { get; set; }
        public string ContractName { get; set; }

        public IEnumerable<Type> SelectServiceTypes(Type targetType)
        {
            var serviceTypes = targetType.GetSelfAndImplemented().Where(ExportedTypes);
            return Except == null || Except.Length == 0 ? serviceTypes : serviceTypes.Except(Except);
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ExportAsGenericWrapperAttribute : Attribute
    {
        public int ServiceTypeArgIndex { get; set; }

        public ExportAsGenericWrapperAttribute(int serviceTypeArgumentIndex = 0)
        {
            ServiceTypeArgIndex = serviceTypeArgumentIndex.ThrowIf(serviceTypeArgumentIndex < 0);
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ExportAsDecoratorAttribute : Attribute
    {
        public string ContractName { get; set; }
        public bool ShouldCompareMetadata { get; set; }
        public Type ConditionType { get; set; }
    }

    public interface IDecoratorCondition
    {
        bool Check(Request request);
    }

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class ImportWithMetadataAttribute : Attribute
    {
        public ImportWithMetadataAttribute(object metadata)
        {
            Metadata = metadata.ThrowIfNull();
        }

        public readonly object Metadata;
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class ImportOrExportAttribute : Attribute
    {
        public string ContractName { get; set; }

        public Type ImplementationType { get; set; }

        public CreationPolicy CreationPolicy { get; set; }

        public object Metadata { get; set; }

        public Type[] ConstructorSignature { get; set; }
    }
}