// TODO:
// - Add ExportFactory.
// - Recognize export of Factory implementation.

//#define MEF_IS_AVAILABLE

namespace DryIoc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
#if MEF_IS_AVAILABLE
	using System.ComponentModel.Composition;
#endif

    public static class AttributedRegistrator
    {
        public static bool SingletonByDefault = true;

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

                var setup = info.FactorySetupInfo == null
                    ? ServiceSetup.WithMetadata(metadata)
                    : info.FactorySetupInfo.CreateSetup(metadata);

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

                ExportInfo[] exports = null;
                var isSingleton = SingletonByDefault; // default is singleton
                var metadataAttributeIndex = -1;
                var factoryType = FactoryType.Service;
                FactorySetupInfo setupInfo = null;
                var multipleFactorySetupsFound = false;

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

                        if (exports == null)
                            exports = new[] { export };
                        else if (!exports.Contains(export))
                            exports = exports.AppendOrUpdate(export);
                    }
                    else if (attribute is ExportAllAttribute)
                    {
                        var exportAllAttribute = (ExportAllAttribute)attribute;
                        var exportAllInfos = exportAllAttribute.SelectServiceTypes(type)
                            .Select(t => new ExportInfo { ServiceType = t, ServiceName = exportAllAttribute.ContractName })
                            .ToArray();

                        if (exports != null)
                            for (var index = 0; index < exports.Length; index++)
                            {
                                var export = exports[index];
                                if (!exportAllInfos.Contains(export))
                                    exportAllInfos = exportAllInfos.AppendOrUpdate(export);
                            }

                        exports = exportAllInfos;
                    }
                    else if (attribute is PartCreationPolicyAttribute)
                    {
                        isSingleton = ((PartCreationPolicyAttribute)attribute).CreationPolicy == CreationPolicy.Shared;
                    }
                    else if (attribute is ExportAsGenericWrapperAttribute)
                    {
                        multipleFactorySetupsFound = setupInfo != null;
                        setupInfo = new GenericWrapperSetupInfo
                        {
                            ServiceTypeIndex = ((ExportAsGenericWrapperAttribute)attribute).ServiceTypeArgIndex
                        };
                        factoryType = FactoryType.GenericWrapper;
                    }
                    else if (attribute is ExportAsDecoratorAttribute)
                    {
                        multipleFactorySetupsFound = setupInfo != null;
                        var decoratorAttribute = ((ExportAsDecoratorAttribute)attribute);
                        setupInfo = new DecoratorSetupInfo
                        {
                            ServiceName = decoratorAttribute.ContractName,
                            CompareMetadata = decoratorAttribute.CompareMetadata,
                            Condition = decoratorAttribute.Condition
                        };
                        factoryType = FactoryType.Decorator;
                    }

                    if (Attribute.IsDefined(attribute.GetType(), typeof(MetadataAttributeAttribute), false))
                    {
                        Throw.If(metadataAttributeIndex != -1, Error.UNSUPPORTED_MULTIPLE_METADATA, type);
                        metadataAttributeIndex = attributeIndex;
                    }
                }

                Throw.If(multipleFactorySetupsFound, Error.UNSUPPORTED_MULTIPLE_SETUPS, type);

                if (metadataAttributeIndex != -1 && setupInfo is DecoratorSetupInfo)
                    ((DecoratorSetupInfo)setupInfo).ThrowIf(info => !info.CompareMetadata);

                yield return new RegistrationInfo
                {
                    Exports = exports,
                    ImplementationType = type,
                    IsSingleton = isSingleton && factoryType != FactoryType.Decorator,
                    MetadataAttributeIndex = metadataAttributeIndex,
                    FactorySetupInfo = setupInfo
                };
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

        public static void UseImportAttributes(this ResolutionRules rules)
        {
            rules.ConstructorParameters = rules.ConstructorParameters.Append(GetConstructorParameterServiceKeyOrDefault);
            rules.PropertiesAndFields = rules.PropertiesAndFields.Append(TryGetPropertyOrFieldServiceKey);
        }

        public static object GetConstructorParameterServiceKeyOrDefault(ParameterInfo parameter, Request parent, IRegistry registry)
        {
            var attributes = parameter.GetCustomAttributes(false);
            if (attributes.Length == 0)
                return null;

            object key;
            if (TryGetServiceKeyFromImportAttribute(out key, attributes) ||
                TryGetServiceKeyWithMetadataAttribute(out key, parameter.ParameterType, registry, attributes) ||
                TryGetServiceKeyFromExportOnImportAttribute(out key, parameter.ParameterType, registry, attributes))
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
                || TryGetServiceKeyFromExportOnImportAttribute(out key, member.GetMemberType(), registry, attributes);
        }

        public static bool TryGetServiceKeyFromImportAttribute(out object key, object[] attributes)
        {
            var import = GetSingleAttributeOrNull<ImportAttribute>(attributes);
            key = import == null ? null : import.ContractName;
            return import != null;
        }

        public static bool TryGetServiceKeyWithMetadataAttribute(out object key, Type contractType, IRegistry registry, object[] attributes)
        {
            key = null;
            var import = GetSingleAttributeOrNull<ImportWithMetadataAttribute>(attributes);
            if (import == null)
                return false;

            var serviceType = registry.GetWrappedServiceTypeOrSelf(contractType);
            var metadata = import.Metadata;
            key = registry.GetKeys(serviceType, factory => metadata.Equals(factory.Setup.Metadata)).FirstOrDefault()
                .ThrowIfNull(Error.UNABLE_TO_FIND_DEPENDENCY_WITH_METADATA, serviceType, metadata);
            return true;
        }

        public static bool TryGetServiceKeyFromExportOnImportAttribute(out object key, Type contractType, IRegistry registry, object[] attributes)
        {
            key = null;
            var import = GetSingleAttributeOrNull<ExportOnImport>(attributes);
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

    public static partial class Error
    {
        public static readonly string UNABLE_TO_FIND_SINGLE_CONSTRUCTOR_WITH_IMPORTING_ATTRIBUTE =
            "Unable to find single constructor with " + typeof(ImportingConstructorAttribute) + " in {0}.";

        public static readonly string UNABLE_TO_FIND_DEPENDENCY_WITH_METADATA =
            "Unable to resolve dependency {0} with metadata [{1}].";

        public static readonly string UNSUPPORTED_MULTIPLE_METADATA =
            "Multiple associated metadata found while exporting {0}. " +
            "Only single metadata is supported per implementation type, please remove the rest.";

        public static readonly string UNSUPPORTED_MULTIPLE_SETUPS =
            "Multiple factory setups found while exporting {0}. " +
            "Only single ExportAs.. attribute is supported, please remove the rest.";
    }

#pragma warning disable 659

    [Serializable]
    public sealed class RegistrationInfo
    {
        public Type ImplementationType;
        public bool IsSingleton;
        public int MetadataAttributeIndex;
        public ExportInfo[] Exports;
        public FactorySetupInfo FactorySetupInfo;

        public override bool Equals(object obj)
        {
            var other = obj as RegistrationInfo;
            return other != null
                && other.ImplementationType == ImplementationType
                && other.IsSingleton == IsSingleton
                && Equals(other.FactorySetupInfo, FactorySetupInfo)
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

    public abstract class FactorySetupInfo
    {
        public abstract FactorySetup CreateSetup(object metadata);
    }

    [Serializable]
    public class GenericWrapperSetupInfo : FactorySetupInfo
    {
        public int ServiceTypeIndex;

        public override FactorySetup CreateSetup(object metadata)
        {
            return GenericWrapperSetup.With(types => types[ServiceTypeIndex]);
        }

        public override bool Equals(object obj)
        {
            var other = obj as GenericWrapperSetupInfo;
            return other != null && other.ServiceTypeIndex == ServiceTypeIndex;
        }
    }

    [Serializable]
    public class DecoratorSetupInfo : FactorySetupInfo
    {
        public string ServiceName;
        public bool CompareMetadata;
        public Type Condition;

        public override FactorySetup CreateSetup(object metadata)
        {
            if (Condition != null)
                return DecoratorSetup.With(((IDecoratorCondition)Activator.CreateInstance(Condition)).Check);

            if (CompareMetadata || ServiceName != null)
                return DecoratorSetup.With(request =>
                    (!CompareMetadata || Equals(metadata, request.Metadata)) &&
                    (ServiceName == null || ServiceName.Equals(request.ServiceKey)));

            return DecoratorSetup.Default;
        }

        public override bool Equals(object obj)
        {
            var other = obj as DecoratorSetupInfo;
            return other != null && other.ServiceName == ServiceName && other.CompareMetadata == CompareMetadata;
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
        public bool CompareMetadata { get; set; }
        public Type Condition { get; set; }
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
    public class ExportOnImport : Attribute
    {
        public string ContractName { get; set; }

        public Type ImplementationType { get; set; }

        public CreationPolicy CreationPolicy { get; set; }

        public object Metadata { get; set; }

        public Type[] ConstructorSignature { get; set; }
    }
}