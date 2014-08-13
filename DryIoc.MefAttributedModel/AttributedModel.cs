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

        public static ResolutionRules WithAttributedModel(this ResolutionRules rules)
        {
            return rules
                .WithConstructorSelector(SelectImportingConstructor) // hello, Max!!! we are Martians.
                .With(GetConstructorParameterServiceInfo)
                .WithPropertyAndFieldSelector(SelectPropertiesAndFieldsWithImportAttribute);
        }

        public static Container WithAttributedModel(this Container container)
        {
            return container.WithNewRules(container.ResolutionRules.WithAttributedModel());
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

                if (export.ServiceType.IsGenericType &&
                    export.ServiceType.GetGenericTypeDefinition() == typeof(IFactory<>))
                    RegisterFactory(registrator, info.ImplementationType, export);
            }
        }

        public static IEnumerable<RegistrationInfo> Scan(IEnumerable<Assembly> assemblies)
        {
            return assemblies.SelectMany(a => a.GetTypes())
                .Select(GetRegistrationInfoOrDefault)
                .Where(info => info != null);
        }

        public static RegistrationInfo GetRegistrationInfoOrDefault(Type implementationType)
        {
            return !implementationType.IsClass || implementationType.IsAbstract ? null
                : GetRegistrationInfoOrDefault(implementationType, GetAllExportRelatedAttributes(implementationType));
        }

        public static RegistrationInfo GetRegistrationInfoOrDefault(Type implementationType, object[] attributes)
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

                    if (implementationType.IsGenericTypeDefinition)
                    {
                        var implTypeArgs = implementationType.GetGenericArguments();
                        allContractTypes = allContractTypes.Where(t =>
                            t.IsGenericType && t.ContainsGenericParameters && t.ContainsAllGenericParameters(implTypeArgs))
                            .Select(t => t.GetGenericTypeDefinition());
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
                else if (attribute is ExportAsGenericWrapperAttribute)
                {
                    Throw.If(info.FactoryType != FactoryType.Service, Error.UNSUPPORTED_MULTIPLE_FACTORY_TYPES, implementationType);
                    info.FactoryType = FactoryType.GenericWrapper;
                    var genericWrapperAttribute = ((ExportAsGenericWrapperAttribute)attribute);
                    info.GenericWrapper = new GenericWrapperInfo
                    {
                        ServiceTypeIndex = genericWrapperAttribute.ContractTypeArgIndex
                    };
                }
                else if (attribute is ExportAsDecoratorAttribute)
                {
                    Throw.If(info.FactoryType != FactoryType.Service, Error.UNSUPPORTED_MULTIPLE_FACTORY_TYPES, implementationType);
                    var decorator = ((ExportAsDecoratorAttribute)attribute);
                    info.FactoryType = FactoryType.Decorator;
                    info.Decorator = new DecoratorInfo(decorator.ConditionType, decorator.ContractName ?? decorator.ContractKey);
                }

                if (Attribute.IsDefined(attribute.GetType(), typeof(MetadataAttributeAttribute), true))
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

            var info = GetRegistrationInfoOrDefault(serviceType, attributes);
            if (info == null)
                return;

            // Result expression is {container.Resolve<IFactory<TService>>(factoryName).Create()} 
            Func<Request, IRegistry, Expression> factoryCreateExpr = (request, registry) =>
                Expression.Call(
                    Expression.Call(_resolveMethod.MakeGenericMethod(factoryExport.ServiceType),
                        request.State.GetItemExpression(registry),
                        Expression.Constant(factoryExport.ServiceKeyInfo.Key, typeof(string)),
                        Expression.Constant(IfUnresolved.Throw, typeof(IfUnresolved))),
                    _factoryMethodName, null);

            var factory = new ExpressionFactory(factoryCreateExpr,
                AttributedModel.GetReuseByType(info.ReuseType), info.GetSetup(attributes));

            for (var i = 0; i < info.Exports.Length; i++)
            {
                var export = info.Exports[i];
                registrator.Register(factory,
                    export.ServiceType, export.ServiceKeyInfo.Key, IfAlreadyRegistered.ThrowIfDuplicateKey);
            }
        }

        #endregion

        #region Tools

        public static ConstructorInfo SelectImportingConstructor(Type implementationType, Request req, IRegistry reg)
        {
            var constructors = implementationType.GetConstructors();
            return constructors.Length == 1 ? constructors[0]
                : constructors.SingleOrDefault(x => Attribute.IsDefined(x, typeof(ImportingConstructorAttribute)))
                    .ThrowIfNull(Error.UNABLE_TO_FIND_SINGLE_CONSTRUCTOR_WITH_IMPORTING_ATTRIBUTE, implementationType);
        }

        public static IReuse GetReuseByType(Type reuseType)
        {
            if (reuseType == null)
                return null;

            IReuse reuse;
            if (!AttributedModel.SupportedReuseTypes.TryGetValue(reuseType, out reuse))
                throw Error.UNSUPPORTED_REUSE_TYPE.Of(reuseType);
            return reuse;
        }

        #endregion

        #region Rules

        public static CustomServiceInfo GetConstructorParameterServiceInfo(ParameterInfo parameter, Request request, IRegistry registry)
        {
            return GetCustomServiceInfo(parameter.ParameterType, parameter.GetCustomAttributes(false), request, registry);
        }

        public static IEnumerable<ServiceInfo> SelectPropertiesAndFieldsWithImportAttribute(Type type, Request request, IRegistry registry)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var properties = type.GetProperties(flags).Where(p => p.GetSetMethod() != null)
                .Select(p => GetPropertyServiceInfo(p, request, registry));
            
            var fields = type.GetFields(flags).Where(f => !f.IsInitOnly)
                .Select(p => GetFieldServiceInfo(p, request, registry));

            return properties.Concat(fields).Where(info => info != null);
        }

        public static ServiceInfo GetPropertyServiceInfo(PropertyInfo property, Request parent, IRegistry registry)
        {
            var customInfo = GetCustomServiceInfo(property.PropertyType, property.GetCustomAttributes(false), parent, registry);
            return customInfo == null ? null : ServiceInfo.Of(property).WithCustom(customInfo, registry);
        }

        public static ServiceInfo GetFieldServiceInfo(FieldInfo field, Request parent, IRegistry registry)
        {
            var customInfo = GetCustomServiceInfo(field.FieldType, field.GetCustomAttributes(false), parent, registry);
            return customInfo == null ? null : ServiceInfo.Of(field).WithCustom(customInfo, registry);
        }

        public static CustomServiceInfo GetCustomServiceInfo(Type serviceType, object[] attributes, Request parent, IRegistry registry)
        {
            if (attributes.Length == 0)
                return null;

            var import = GetSingleAttributeOrDefault<ImportAttribute>(attributes);
            if (import != null)
            {
                var serviceKey = import.ContractName ??
                    (import is ImportWithKeyAttribute ? ((ImportWithKeyAttribute)import).ContractKey : null);
                return CustomServiceInfo.Of(import.ContractType, serviceKey);
            }

            var info = GetServiceInfoWithMetadataAttribute(serviceType, attributes, parent, registry) 
                ?? GetServiceInfoFromImportExternalAttribute(serviceType, registry, attributes);
            return info;
        }

        public static CustomServiceInfo GetServiceInfoWithMetadataAttribute(Type serviceType, object[] attributes, Request parent, IRegistry registry)
        {
            var import = GetSingleAttributeOrDefault<ImportWithMetadataAttribute>(attributes);
            if (import == null)
                return null;

            serviceType = registry.UnwrapServiceType(serviceType);
            var metadata = import.Metadata;
            var factory = registry.GetAllFactories(serviceType)
                .FirstOrDefault(f => metadata.Equals(f.Value.Setup.Metadata))
                .ThrowIfNull(Error.UNABLE_TO_FIND_DEPENDENCY_WITH_METADATA, serviceType, metadata, parent);

            return CustomServiceInfo.Of(serviceType, factory.Key);
        }

        public static CustomServiceInfo GetServiceInfoFromImportExternalAttribute(Type serviceType, IRegistry registry, object[] attributes)
        {
            var exportAttr = GetSingleAttributeOrDefault<ImportExternalAttribute>(attributes);
            if (exportAttr == null)
                return null;

            serviceType = exportAttr.ContractType ?? registry.UnwrapServiceType(serviceType);
            var serviceKey = exportAttr.ContractKey;

            if (!registry.IsRegistered(serviceType, serviceKey))
            {
                var reuseAttr = GetSingleAttributeOrDefault<ReuseAttribute>(attributes);
                var reuse = GetReuseByType(reuseAttr == null ? DefaultReuseType : reuseAttr.ReuseType);

                var implementationType = exportAttr.ImplementationType ?? serviceType;

                var getConstructor = exportAttr.WithConstructor != null
                    ? (ConstructorSelector)((t, _, __) => t.GetConstructor(exportAttr.WithConstructor)) : null;

                registry.Register(serviceType,
                    implementationType, reuse, getConstructor, ServiceSetup.WithMetadata(exportAttr.Metadata),
                    serviceKey, IfAlreadyRegistered.KeepRegistered);
            }

            return CustomServiceInfo.Of(serviceType, serviceKey);
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

        public static readonly string EXPORT_IS_REQUIRED =
            "At least one Export attributed should be defined for {0}.";

        public static readonly string EXPORT_ALL_EXPORTS_EMPTY_LIST_OF_TYPES =
            "Unable to get contract types for implementation {0} cause all of its implemented types where filtered out: {1}";

        public static readonly string UNSUPPORTED_REUSE_TYPE =
            "Attributed model does not support reuse type {0}. ";
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
            if (type.IsEnum)
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
        public GenericWrapperInfo GenericWrapper;
        public DecoratorInfo Decorator;

        public Factory CreateFactory()
        {
            return new ReflectionFactory(ImplementationType, AttributedModel.GetReuseByType(ReuseType), setup: GetSetup());
        }

        public IReuse GetReuse()
        {
            return ReuseType == null ? null : AttributedModel.SupportedReuseTypes[ReuseType];
        }

        public FactorySetup GetSetup(object[] attributes = null)
        {
            if (FactoryType == FactoryType.GenericWrapper)
                return GenericWrapper == null ? GenericWrapperSetup.Default : GenericWrapper.GetSetup();

            if (FactoryType == FactoryType.Decorator)
                return Decorator == null ? DecoratorSetup.Default
                    : HasMetadataAttribute ? Decorator.GetSetup(() => GetMetadata(attributes))
                    : Decorator.GetSetup();

            if (HasMetadataAttribute)
                return ServiceSetup.WithMetadata(() => GetMetadata(attributes));

            return ServiceSetup.Default;
        }

        public override bool Equals(object obj)
        {
            var other = obj as RegistrationInfo;
            return other != null
                && other.ImplementationType == ImplementationType
                && other.ReuseType == ReuseType
                && other.FactoryType == FactoryType
                && Equals(other.GenericWrapper, GenericWrapper)
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
            if (GenericWrapper != null) code.Append(@",
    GenericWrapper = new GenericWrapperInfo { ServiceTypeIndex = ").Append(GenericWrapper.ServiceTypeIndex).Append(@" }");
            if (Decorator != null)
            {
                code.Append(@",
"); Decorator.AppendAsCode(code);
            }
            code.Append(@"
}");
            return code;
        }

        private object GetMetadata(object[] attributes = null)
        {
            attributes = attributes ?? ImplementationType.GetCustomAttributes(false);
            var metadataAttr = attributes.FirstOrDefault(
                a => Attribute.IsDefined(a.GetType(), typeof(MetadataAttributeAttribute), true));

            if (metadataAttr is ExportWithMetadataAttribute)
                return ((ExportWithMetadataAttribute)metadataAttr).Metadata;

            return metadataAttr;
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

    public sealed class GenericWrapperInfo
    {
        public int ServiceTypeIndex;

        public GenericWrapperSetup GetSetup()
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
                return DecoratorSetup.With(((IDecoratorCondition)Activator.CreateInstance(ConditionType)).Check);

            if (ServiceKeyInfo != ServiceKeyInfo.Default || getMetadata != null)
                return DecoratorSetup.With(request =>
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
            : this(null, contractType) {}
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
            ContractType = contractType;
            ImplementationType = implementationType;
            WithConstructor = withConstructor;
            Metadata = metadata;
            ContractKey = contractKey;
        }
    }

    public interface IFactory<T>
    {
        T Create();
    }

    #endregion
}