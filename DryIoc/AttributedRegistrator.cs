// TODO:
// - Add ExportAsDecorator/GenericWrapper.
// + Aggregate rules that using attributes into one and to GetCustomAttributes only once and improve speed.
// + Split WithMetadata to Export and Import.
// + Add ImportUsing(ImplType, Reuse, Name, Metadata, ConstructorSignature)
// + Add ExportAll(Except=new Type[] { ... }).
// + Add Import(ContractName=...) support on constructor parameters.

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

        public static void RegisterExports(this IRegistrator registrator, params Assembly[] assemblies)
        {
            registrator.RegisterExports(Scan(assemblies));
        }

        public static void RegisterExports(this IRegistrator registrator, IEnumerable<RegistrationInfo> infos)
        {
            foreach (var info in infos)
            {
                var setup = FactorySetup.Service.Default;
                if (info.FactoryType == FactoryType.Service)
                {
                    var metadata = info.MetadataAttributeIndex == -1 ? null : FindMetadata(info.ImplementationType, info.MetadataAttributeIndex);
                    setup = Factory.WithMetadata(metadata);
                }
                else if (info.FactoryType == FactoryType.GenericWrapper)
                {
                    setup = Factory.GenericWrapper();
                }

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

        public static IEnumerable<RegistrationInfo> Scan(IEnumerable<Assembly> assemblies)
        {
            var implementationTypes = assemblies.SelectMany(a => a.GetTypes()).Where(TypeIsForScan);

            foreach (var implementationType in implementationTypes)
            {
                var attributes = implementationType.GetCustomAttributes(false);

                ExportInfo[] exports = null;
                var isSingleton = SingletonByDefault; // default is singleton
                var metadataAttributeIndex = -1;
                var factoryType = FactoryType.Service;

                for (var attributeIndex = 0; attributeIndex < attributes.Length; attributeIndex++)
                {
                    var attribute = attributes[attributeIndex];
                    if (attribute is ExportAttribute)
                    {
                        var exportAttribute = (ExportAttribute)attribute;
                        var export = new ExportInfo
                        {
                            ServiceType = exportAttribute.ContractType ?? implementationType,
                            ServiceName = exportAttribute.ContractName
                        };

                        if (exports == null)
                        {
                            exports = new[] { export };
                        }
                        else if (!exports.Contains(export))
                        {
                            exports = exports.AddOrUpdateCopy(export);
                        }
                    }
                    else if (attribute is ExportPublicTypesAttribute)
                    {
                        var autoExportAttribute = (ExportPublicTypesAttribute)attribute;
                        var autoExports = autoExportAttribute.SelectServiceTypes(implementationType)
                            .Select(type => new ExportInfo { ServiceType = type })
                            .ToArray();

                        if (exports != null)
                        {
                            for (var index = 0; index < exports.Length; index++)
                            {
                                var export = exports[index];
                                if (!autoExports.Contains(export))
                                    autoExports = autoExports.AddOrUpdateCopy(export);
                            }
                        }

                        exports = autoExports;

                    }
                    else if (attribute is PartCreationPolicyAttribute)
                    {
                        isSingleton = ((PartCreationPolicyAttribute)attribute).CreationPolicy == CreationPolicy.Shared;
                    }
                    else if (attribute is ExportAsGenericWrapperAttribute)
                    {
                        factoryType = FactoryType.GenericWrapper;
                    }

                    if (Attribute.IsDefined(attribute.GetType(), typeof(MetadataAttributeAttribute), false))
                    {
                        metadataAttributeIndex = attributeIndex;
                    }
                }

                yield return new RegistrationInfo
                {
                    Exports = exports,
                    ImplementationType = implementationType,
                    IsSingleton = isSingleton,
                    MetadataAttributeIndex = metadataAttributeIndex,
                    FactoryType = factoryType
                };
            }
        }

        #region Tools

        public static bool TypeIsForScan(Type type)
        {
            return type.IsClass && !type.IsAbstract &&
                (Attribute.IsDefined(type, typeof(ExportAttribute), false) ||
                Attribute.IsDefined(type, typeof(ExportPublicTypesAttribute), false));
        }

        public static ConstructorInfo FindSingleImportingConstructor(Type type)
        {
            return type.GetConstructors()
                .SingleOrDefault(x => Attribute.IsDefined(x, typeof(ImportingConstructorAttribute)))
                .ThrowIfNull(UNABLE_TO_FIND_SINGLE_CONSTRUCTOR_WITH_IMPORTING_ATTRIBUTE, type);
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

        public static object TryGetKeyFromImportAttribute(ParameterInfo parameter, Request _, IRegistry __)
        {
            var imports = parameter.GetCustomAttributes(typeof(ImportAttribute), false);
            return imports.Length == 1 ? ((ImportAttribute)imports[0]).ContractName : null;
        }

        public static object TryGetKeyWithMetadataAttribute(ParameterInfo parameter, Request parent, IRegistry registry)
        {
            var attributes = parameter.GetCustomAttributes(typeof(ImportWithMetadataAttribute), false);
            if (attributes.Length == 0)
                return null;

            var attribute = (ImportWithMetadataAttribute)attributes[0];
            var serviceType = registry.GetWrappedServiceTypeOrSelf(parameter.ParameterType);
            var serviceKey = registry.GetKeys(serviceType, f => attribute.Metadata.Equals(f.Setup.Metadata)).FirstOrDefault();

            return serviceKey.ThrowIfNull(UNABLE_TO_FIND_DEPENDENCY_WITH_METADATA, serviceType, attribute.Metadata);
        }

        public static object TryImportUsing(ParameterInfo param, Request parent, IRegistry registry)
        {
            var imports = param.GetCustomAttributes(typeof(ImportUsing), false);
            if (imports.Length == 0)
                return null;

            var import = (ImportUsing)imports[0];
            var serviceType = registry.GetWrappedServiceTypeOrSelf(param.ParameterType);
            var serviceName = import.ContractName;
            if (registry.IsRegistered(serviceType, serviceName))
                return null;

            var implementationType = import.ImplementationType ?? serviceType;
            var reuse = import.CreationPolicy == CreationPolicy.Shared ? Reuse.Singleton : null;
            SelectConstructor withConstructor = t => t.GetConstructor(import.ConstructorSignature);
            var setup = Factory.WithMetadata(import.Metadata);

            registry.Register(serviceType, implementationType, reuse, withConstructor, setup, serviceName);
            return serviceName;
        }

        public static bool ImportPropertyOrField(out object key, MemberInfo propertyOrField, Request parent, IRegistry _)
        {
            key = null;
            var imports = propertyOrField.GetCustomAttributes(typeof(ImportAttribute), false);
            if (imports.Length == 0)
                return false;
            key = ((ImportAttribute)imports[0]).ContractName;
            return true;
        }

        #endregion

        #region Implementation

        private static readonly string UNABLE_TO_FIND_SINGLE_CONSTRUCTOR_WITH_IMPORTING_ATTRIBUTE =
            "Unable to find single constructor with " + typeof(ImportingConstructorAttribute) + " in {0}.";

        private static string UNABLE_TO_FIND_DEPENDENCY_WITH_METADATA =
            "Unable to resolve dependency {0} with metadata [{1}]";

        #endregion
    }

#pragma warning disable 659
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
    public sealed class RegistrationInfo
    {
        public ExportInfo[] Exports;

        public Type ImplementationType;

        public bool IsSingleton;

        public int MetadataAttributeIndex;

        public FactoryType FactoryType;

        public override bool Equals(object obj)
        {
            var other = obj as RegistrationInfo;
            if (other == null)
                return false;

            if (Exports.Length != other.Exports.Length)
                return false;

            for (var i = 0; i < Exports.Length; i++)
                if (!Exports[i].Equals(other.Exports[i]))
                    return false;

            return ImplementationType == other.ImplementationType
                && IsSingleton == other.IsSingleton
                && MetadataAttributeIndex == other.MetadataAttributeIndex;
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
        public ExportWithMetadataAttribute(object metadata)
        {
            Metadata = metadata.ThrowIfNull();
        }

        public readonly object Metadata;
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ExportPublicTypesAttribute : Attribute
    {
        public static Func<Type, bool> ExportedTypes = Container.PublicTypes;

        public Type[] Except { get; set; }

        public string ContractName { get; set; }

        public IEnumerable<Type> SelectServiceTypes(Type targetType)
        {
            var serviceTypes = targetType.GetSelfAndImplemented().Where(ExportedTypes);
            return Except == null || Except.Length == 0 ? serviceTypes : serviceTypes.Except(Except);
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ExportAsGenericWrapperAttribute : Attribute { }

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
    public class ImportUsing : Attribute
    {
        public string ContractName { get; set; }

        public Type ImplementationType { get; set; }

        public CreationPolicy CreationPolicy { get; set; }

        public object Metadata { get; set; }

        public Type[] ConstructorSignature { get; set; }
    }
}