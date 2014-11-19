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
    /// <summary>Implements MEF Attributed Programming Model. 
    /// Documentation is available at https://bitbucket.org/dadhi/dryioc/wiki/MefAttributedModel. </summary>
    public static class AttributedModel
    {
        ///<summary>Default reuse policy is Singleton, the same as in MEF.</summary>
        public static Type DefaultReuseType = typeof(SingletonReuse);

        /// <summary>Map of supported reuse types: so the reuse type specified by <see cref="ReuseAttribute"/> 
        /// could be mapped to corresponding <see cref="Reuse"/> members.</summary>
        public static readonly HashTree<Type, IReuse> SupportedReuseTypes = HashTree<Type, IReuse>.Empty
            .AddOrUpdate(typeof(SingletonReuse), Reuse.Singleton)
            .AddOrUpdate(typeof(CurrentScopeReuse), Reuse.InCurrentScope)
            .AddOrUpdate(typeof(ResolutionScopeReuse), Reuse.InResolutionScope);

        /// <summary>Map of supported reuse wrapper types, so that specified in <see cref="ReuseWrappersAttribute"/> type
        /// could be mapped to corresponding provider in <see cref="ReuseWrapper"/></summary>
        public static readonly HashTree<Type, IReuseWrapper> SupportedReuseWrapperTypes = HashTree<Type, IReuseWrapper>.Empty
            .AddOrUpdate(typeof(WeakReference), ReuseWrapper.WeakReference)
            .AddOrUpdate(typeof(ExplicitlyDisposable), ReuseWrapper.ExplicitlyDisposable)
            .AddOrUpdate(typeof(Disposable), ReuseWrapper.Disposable)
            .AddOrUpdate(typeof(Ref<object>), ReuseWrapper.Ref);

        /// <summary>Returns new rules with attributed model importing rules appended.</summary>
        /// <param name="rules">Source rules to append importing rules to.</param>
        /// <returns>New rules with attributed model rules.</returns>
        public static Rules WithAttributedModel(this Rules rules)
        {
            // hello, Max!!! we are Martians.
            return rules.With(GetImportingConstructor, GetImportedParameter, GetImportedPropertiesAndFields);
        }

        /// <summary>Appends attributed model rules to passed container using <see cref="WithAttributedModel(DryIoc.Rules)"/>.</summary>
        /// <param name="container">Source container to apply attributed model importing rules to.</param>
        /// <returns>Returns new container with new rules.</returns>
        public static Container WithAttributedModel(this Container container)
        {
            return container.WithNewRules(container.Rules.WithAttributedModel());
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
                    export.ServiceKeyInfo.Key, IfAlreadyRegistered.ThrowIfDuplicateKey);

                if (registrationInfo.IsFactory)
                    RegisterFactoryMethods(registrator, registrationInfo.ImplementationType);
            }
        }

        /// <summary>Scans assemblies to find concrete type annotated with <see cref="ExportAttribute"/>, or <see cref="ExportAllAttribute"/>
        /// attributes, and create serializable DTO with all information required for registering of exported types.</summary>
        /// <param name="assemblies">Assemblies to scan.</param>
        /// <returns>Lazy collection of registration info DTOs.</returns>
        public static IEnumerable<RegistrationInfo> Scan(IEnumerable<Assembly> assemblies)
        {
            return assemblies.SelectMany(_getAssemblyTypes)
                .Select(GetRegistrationInfoOrDefault)
                .Where(info => info != null);
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
            return !DefineExport(attributes) ? null : GetRegistrationInfoOrDefault(implementationType, attributes);
        }

        #region Tools

        /// <summary>Returns reuse object by mapping provided type to <see cref="SupportedReuseTypes"/>.
        /// Returns null (transient or no reuse) if null provided reuse type.</summary>
        /// <param name="reuseType">Reuse type to find in supported.</param>
        /// <returns>Supported reuse object.</returns>
        public static IReuse GetReuseByType(Type reuseType)
        {
            return reuseType == null ? null
                : SupportedReuseTypes.GetValueOrDefault(reuseType).ThrowIfNull(Error.UNSUPPORTED_REUSE_TYPE, reuseType);
        }

        /// <summary>Returns reuse wrapper object: corresponding member of <see cref="ReuseWrapper"/>.
        /// Uses <see cref="SupportedReuseWrapperTypes"/> mapping to find it.</summary>
        /// <param name="reuseWrapperType">Reuse wrapper type to find mapping for.</param>
        /// <returns>Supported reuse wrapper object.</returns>
        public static IReuseWrapper GetReuseWrapperByType(Type reuseWrapperType)
        {
            return SupportedReuseWrapperTypes.GetValueOrDefault(reuseWrapperType)
                .ThrowIfNull(Error.UNSUPPORTED_REUSE_WRAPPER_TYPE, reuseWrapperType);
        }

        #endregion

        #region Rules

        private static ConstructorInfo GetImportingConstructor(Request request)
        {
            var implementationType = request.ImplementationType;
            var constructors = implementationType.GetAllConstructors().ToArrayOrSelf();
            return constructors.Length == 1 
                ? constructors[0]
                : constructors.SingleOrDefault(x => x.GetAttributes(typeof(ImportingConstructorAttribute)).Any())
                    .ThrowIfNull(Error.UNABLE_TO_FIND_SINGLE_CONSTRUCTOR_WITH_IMPORTING_ATTRIBUTE, implementationType);
        }

        private static ParameterServiceInfo GetImportedParameter(ParameterInfo parameter, Request request)
        {
            var serviceInfo = ParameterServiceInfo.Of(parameter);
            var attrs = parameter.GetAttributes().ToArray();
            return attrs.Length == 0 ? serviceInfo
                : serviceInfo.WithDetails(GetFirstImportDetailsOrNull(parameter.ParameterType, attrs, request), request);
        }

        private static readonly PropertiesAndFieldsSelector GetImportedPropertiesAndFields =
            PropertiesAndFields.All(PropertiesAndFields.Include.All, GetImportedPropertiesAndFieldsOnly);

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

            var registry = request.Registry;
            reflectedType = registry.GetWrappedServiceType(reflectedType);
            var metadata = meta.Metadata;
            var factory = registry.GetAllServiceFactories(reflectedType)
                .FirstOrDefault(f => metadata.Equals(f.Value.Setup.Metadata))
                .ThrowIfNull(Error.UNABLE_TO_FIND_DEPENDENCY_WITH_METADATA, reflectedType, metadata, request);

            return factory.Key;
        }

        private static ServiceInfoDetails GetImportExternalDetails(Type serviceType, Attribute[] attributes, Request request)
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
                    : (ConstructorSelector)(r => r.ImplementationType.GetConstructorOrNull(args: import.WithConstructor));

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
                    info.ReuseType = ((ReuseAttribute)attribute).ReuseType;
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

            info.Exports.ThrowIfNull(Error.EXPORT_IS_REQUIRED, implementationType);
            return info;
        }

        private static bool DefineExport(Attribute[] attributes)
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
            
            Throw.If(exports.Length == 0, Error.EXPORT_ALL_EXPORTS_EMPTY_LIST_OF_TYPES,
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

        private static readonly MethodInfo _resolveMethod = typeof(Resolver)
            .GetDeclaredMethod("Resolve", new[] { typeof(IResolver), typeof(object), typeof(IfUnresolved), typeof(Type) });

        private static readonly Func<Assembly, IEnumerable<Type>> _getAssemblyTypes =
            ExpressionTools.GetMethodDelegate<Assembly, IEnumerable<Type>>("GetTypes");

        private const string FACTORY_METHOD_NAME = "Create";
        private const string DOT_FACTORY_METHOD_NAME = "." + FACTORY_METHOD_NAME;

        private static void RegisterFactoryMethods(IRegistrator registrator, Type factoryType)
        {
            //var serviceType = factoryExport.ServiceType.GetGenericParamsAndArgs()[0];
            var methods = factoryType.GetAll(_ => _.DeclaredMethods);
            foreach (var method in methods)
            {
                var attributes = method.GetAttributes().ToArrayOrSelf();
                if (!DefineExport(attributes))
                    continue;

                var info = GetRegistrationInfoOrDefault(method.ReturnType, attributes);
                if (info == null)
                    continue;

                //// Result expression is {container.Resolve<IFactory<TService>>(factoryName).Create()} 
                //Func<Request, Expression> factoryCreateExpr = request =>
                //    Expression.Call(
                //        Expression.Call(_resolveMethod.MakeGenericMethod(factoryExport.ServiceType),
                //            request.State.GetOrAddItemExpression(request),
                //            Expression.Constant(factoryExport.ServiceKeyInfo.Key, typeof(string)),
                //            Expression.Constant(IfUnresolved.Throw, typeof(IfUnresolved)),
                //            Expression.Constant(null, typeof(Type))),
                //        FACTORY_METHOD_NAME, null);

                //var factory = new ExpressionFactory(factoryCreateExpr, GetReuseByType(info.ReuseType), info.GetSetup(attributes));

                //for (var i = 0; i < info.Exports.Length; i++)
                //{
                //    var exp = info.Exports[i];
                //    registrator.Register(factory, exp.ServiceType, exp.ServiceKeyInfo.Key, IfAlreadyRegistered.ThrowIfDuplicateKey);
                //}
            }
        }

        private static void RegisterFactory(IRegistrator registrator, Type factoryType, ExportInfo factoryExport)
        {
            var serviceType = factoryExport.ServiceType.GetGenericParamsAndArgs()[0];
            var allMethods = factoryType.GetAll(_ => _.DeclaredMethods);
            var factoryMethod = allMethods.FirstOrDefault(m =>
                (m.Name == FACTORY_METHOD_NAME || m.Name.EndsWith(DOT_FACTORY_METHOD_NAME))
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
                    FACTORY_METHOD_NAME, null);

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

        public static readonly string UNSUPPORTED_REUSE_WRAPPER_TYPE =
            "Attributed model does not support reuse wrapper type {0}.";

        public static readonly string EXPORTED_NONGENERIC_WRAPPER_NO_WRAPPED_TYPE =
            "Exported non-generic wrapper type {0} requires wrapped service type to be specified, but it is null, " +
            "and instead generic argument index is set to {1}.";

        public static readonly string EXPORTED_GENERIC_WRAPPER_BAD_ARG_INDEX =
            "Exported generic wrapper type {0} specifies generic argument index {1} outside of argument list size.";
    }

    /// <summary>Converts provided literal into valid C# code. Used for generating registration code 
    /// from <see cref="RegistrationInfo"/> DTOs.</summary>
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

    /// <summary>Serializable DTO of all registration information.</summary>
    public sealed class RegistrationInfo
    {
        public ExportInfo[] Exports;

        public Type ImplementationType;
        public string ImplementationTypeFullName;

        public Type ReuseType;
        public Type[] ReuseWrapperTypes;

        public bool HasMetadataAttribute;
        public FactoryType FactoryType;
        public DecoratorInfo Decorator;
        public WrapperInfo Wrapper;
        public bool IsFactory;

        public Factory CreateFactory()
        {
            return new ReflectionFactory(ImplementationType, AttributedModel.GetReuseByType(ReuseType), GetSetup());
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

            IReuseWrapper[] reuseWrappers = null;
            if (ReuseWrapperTypes != null && ReuseWrapperTypes.Length != 0)
                reuseWrappers = ReuseWrapperTypes.Select(AttributedModel.GetReuseWrapperByType).ToArray();

            return Setup.With(reuseWrappers: reuseWrappers, lazyMetadata: lazyMetadata);
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

    /// <summary>Defines DTO for exported service type and key.</summary>
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

    /// <summary>Describes <see cref="SetupWrapper"/> in serializable way.</summary>
    public sealed class WrapperInfo
    {
        public Type WrappedServiceType;
        public int WrappedServiceTypeGenericArgIndex;

        public SetupWrapper GetSetup()
        {
            return SetupWrapper.With(getWrappedServiceType: GetWrappedServiceType);
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

        public SetupDecorator GetSetup(Func<object> lazyMetadata = null)
        {
            if (ConditionType != null)
                return SetupDecorator.WithCondition(((IDecoratorCondition)Activator.CreateInstance(ConditionType)).CanApply);

            if (ServiceKeyInfo != ServiceKeyInfo.Default || lazyMetadata != null)
                return SetupDecorator.WithCondition(request =>
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

        /// <summary>Create attribute with specified type implementing <see cref="IReuse"/>.</summary>
        /// <param name="reuseType">Could be null to specify transient or no reuse.</param>
        public ReuseAttribute(Type reuseType)
        {
            if (reuseType != null)
                (typeof(IReuse)).ThrowIfNotSubtypeOf(reuseType);
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

    public interface IFactory<T>
    {
        T Create();
    }

    #endregion
}