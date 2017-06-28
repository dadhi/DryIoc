/*
The MIT License (MIT)

Copyright (c) 2013-2016 Maksim Volkau

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
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using DryIocAttributes;
    using ImTools;

    /// <summary>Implements MEF Attributed Programming Model.
    /// Documentation is available at https://bitbucket.org/dadhi/dryioc/wiki/MefAttributedModel. </summary>
    public static class AttributedModel
    {
        /// <summary>Maps the supported reuse types to respective DryIoc reuse.</summary>
        public static readonly ImTreeMap<ReuseType, Func<object, IReuse>> SupportedReuseTypes =
            ImTreeMap<ReuseType, Func<object, IReuse>>.Empty
            .AddOrUpdate(ReuseType.Transient, _ => Reuse.Transient)
            .AddOrUpdate(ReuseType.Singleton, _ => Reuse.Singleton)
            .AddOrUpdate(ReuseType.CurrentScope, Reuse.InCurrentNamedScope)
            .AddOrUpdate(ReuseType.ResolutionScope, _ => Reuse.InResolutionScope)
            .AddOrUpdate(ReuseType.ScopedOrSingleton, _ => Reuse.ScopedOrSingleton);

        /// <summary>Updates the source rules to provide full MEF compatibility.</summary>
        /// <param name="rules">Source rules.</param> <returns>New rules.</returns>
        public static Rules WithMefRules(this Rules rules)
        {
            var importsMadeOf = Made.Of(
                request => GetImportingConstructor(request, rules.FactoryMethod),
                GetImportedParameter, _getImportedPropertiesAndFields);

            return rules.With(importsMadeOf)
                .WithDefaultReuseInsteadOfTransient(Reuse.Singleton)
                .WithTrackingDisposableTransients();
        }

        /// <summary>Add to container rules with <see cref="WithMefRules"/> to provide the full MEF compatibility.
        /// In addition registers the MEF specific wrappers, and adds support for <see cref="IPartImportsSatisfiedNotification"/>.</summary>
        /// <param name="container">Source container.</param> <returns>New container.</returns>
        public static IContainer WithMef(this IContainer container)
        {
            return container
                .With(WithMefRules)
                .WithImportsSatisfiedNotification()
                .WithMefSpecificWrappers()
                .WithMultipleSameContractNamesSupport();
        }

        /// <summary>The basic rules to support MEF/DryIoc Attributes for
        /// specifying service construction via <see cref="ImportingConstructorAttribute"/>,
        /// and for specifying injected dependencies via Import attributes.</summary>
        /// <param name="rules">Original container rules.</param><returns>New rules.</returns>
        public static Rules WithMefAttributedModel(this Rules rules)
        {
            var importsMadeOf = Made.Of(
                request => GetImportingConstructor(request, rules.FactoryMethod),
                GetImportedParameter, _getImportedPropertiesAndFields);

            // hello, Max!!! we are Martians.
            return rules.With(importsMadeOf)
                .WithDefaultReuseInsteadOfTransient(Reuse.Singleton);
        }

        /// <summary>Applies the <see cref="WithMefAttributedModel(DryIoc.Rules)"/> to the container.</summary>
        /// <param name="container">source container</param><returns>New container with applied rules.</returns>
        public static IContainer WithMefAttributedModel(this IContainer container)
        {
            return container.With(WithMefRules);
        }

        #region IPartImportsSatisfiedNotification support

        /// <summary>Registers <see cref="IPartImportsSatisfiedNotification"/> calling decorator into container.
        /// It is not directly related to MEF Exports and Imports, and may be used for notifying the injection
        /// is completed for normal DryIoc registrations.</summary>
        /// <param name="container">Container to support.</param>
        /// <returns>The container with made registration.</returns>
        public static IContainer WithImportsSatisfiedNotification(this IContainer container)
        {
            container.Register<object>(
                made: _importsSatisfiedNotificationFactoryMethod,
                setup: _importsSatisfiedNotificationDecoratorSetup);
            return container;
        }

        internal static TService NotifyImportsSatisfied<TService>(TService service)
        {
            var notification = service as IPartImportsSatisfiedNotification;
            if (notification != null)
                notification.OnImportsSatisfied();

            return service;
        }

        private static readonly Made _importsSatisfiedNotificationFactoryMethod = Made.Of(
            typeof(AttributedModel).GetSingleMethodOrNull("NotifyImportsSatisfied", includeNonPublic: true));

        private static readonly Setup _importsSatisfiedNotificationDecoratorSetup = Setup.DecoratorWith(
            request => request.GetKnownImplementationOrServiceType().IsAssignableTo(typeof(IPartImportsSatisfiedNotification)),
            useDecorateeReuse: true);

        #endregion

        #region ExportFactory<T>, ExportFactory<T, TMetadata> and Lazy<T, TMetadata> support

        /// <summary>Registers MEF-specific wrappers into the container.</summary>
        /// <remarks>MEF-specific wrappers are: <see cref="ExportFactory{T}"/>,
        /// <see cref="ExportFactory{T, TMetadata}"/> and <see cref="Lazy{T, TMetadata}"/>.</remarks>
        /// <param name="container">Container to support.</param>
        /// <returns>The container with registration.</returns>
        public static IContainer WithMefSpecificWrappers(this IContainer container)
        {
            container.Register(typeof(ExportFactory<>),
                made: _createExportFactoryMethod,
                setup: Setup.Wrapper);

            container.Register(typeof(ExportFactory<,>),
                made: _createExportFactoryWithMetadataMethod,
                setup: Setup.WrapperWith(0));

            container.Register(typeof(Lazy<,>),
                made: _createLazyWithMetadataMethod,
                setup: Setup.WrapperWith(0));

            var lazyFactory = new ExpressionFactory(r =>
                WrappersSupport.GetLazyExpressionOrDefault(r, nullWrapperForUnresolvedService: true),
                setup: Setup.Wrapper);
            container.Register(typeof(Lazy<>), lazyFactory, IfAlreadyRegistered.Replace);

            return container;
        }

        /// <summary>Proxy for the tuple parameter to <see cref="ExportFactory{T}"/>.
        /// Required to cover for missing Tuple in .NET 4.0 and lower.
        /// Provides implicit conversion in both <see cref="KeyValuePair{TKey,TValue}"/> and <see cref="Tuple{T1,T2}"/>.</summary>
        /// <typeparam name="TPart">Type of created part.</typeparam>
        public sealed class PartAndDisposeActionPair<TPart>
        {
            /// <summary>Conversion operator.</summary> <param name="source">to be converted</param>
            public static implicit operator KeyValuePair<TPart, Action>(PartAndDisposeActionPair<TPart> source)
            {
                return new KeyValuePair<TPart, Action>(source.Part, source.DisposeAction);
            }

            /// <summary>Conversion operator.</summary> <param name="source">to be converted</param>
            public static implicit operator Tuple<TPart, Action>(PartAndDisposeActionPair<TPart> source)
            {
                return Tuple.Create(source.Part, source.DisposeAction);
            }

            /// <summary>Created export part.</summary>
            public readonly TPart Part;

            /// <summary>Action to dispose the created part and its dependencies</summary>
            public readonly Action DisposeAction;

            /// <summary>Creates a proxy by wrapping the Part and Dispose action.</summary>
            /// <param name="part"></param> <param name="disposeAction"></param>
            public PartAndDisposeActionPair(TPart part, Action disposeAction)
            {
                Part = part;
                DisposeAction = disposeAction;
            }
        }

        /// <summary>Creates the <see cref="ExportFactory{T}"/>.</summary>
        /// <typeparam name="T">The type of the exported service</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="ifUnresolved"><see cref="IfUnresolved"/> behavior specification.</param>
        internal static ExportFactory<T> CreateExportFactory<T>(IContainer container, DryIoc.IfUnresolved ifUnresolved)
        {
            // check if the service is resolvable
            var func = container.Resolve<Func<T>>(ifUnresolved: ifUnresolved);
            if (func == null)
            {
                return null;
            }

            return new ExportFactory<T>(() =>
            {
                var scope = container.With(r => r
                    .WithDefaultReuseInsteadOfTransient(Reuse.InCurrentScope))
                    .OpenScope();
                try
                {
                    var it = scope.Resolve<T>();
                    return new PartAndDisposeActionPair<T>(it, scope.Dispose);
                }
                catch
                {
                    scope.Dispose();
                    throw;
                }
            });
        }

        private static readonly Made _createExportFactoryMethod = Made.Of(
            typeof(AttributedModel).GetSingleMethodOrNull("CreateExportFactory", includeNonPublic: true),
            parameters: Parameters.Of.Type(request => request.IfUnresolved));

        /// <summary>Creates the <see cref="ExportFactory{T, TMetadata}"/>.</summary>
        /// <typeparam name="T">The type of the exported service.</typeparam>
        /// <typeparam name="TMetadata">The type of the metadata.</typeparam>
        /// <param name="metaFactory">The factory with the service metadata.</param>
        /// <param name="container">The container.</param>
        internal static ExportFactory<T, TMetadata> CreateExportFactoryWithMetadata<T, TMetadata>(Meta<KeyValuePair<object, Func<T>>, TMetadata> metaFactory, IContainer container)
        {
            return new ExportFactory<T, TMetadata>(() =>
            {
                var scope = container.With(r => r
                    .WithDefaultReuseInsteadOfTransient(Reuse.InCurrentScope))
                    .OpenScope();
                try
                {
                    var result = scope.Resolve<T>(serviceKey: metaFactory.Value.Key);
                    return new PartAndDisposeActionPair<T>(result, scope.Dispose);
                }
                catch
                {
                    scope.Dispose();
                    throw;
                }
            }, metaFactory.Metadata);
        }

        private static readonly Made _createExportFactoryWithMetadataMethod = Made.Of(
            typeof(AttributedModel).GetSingleMethodOrNull("CreateExportFactoryWithMetadata", includeNonPublic: true));

        /// <summary>Creates the <see cref="Lazy{T, TMetadata}"/>.</summary>
        /// <typeparam name="T">The type of the exported service.</typeparam>
        /// <typeparam name="TMetadata">The type of the metadata.</typeparam>
        /// <param name="metaFactory">The factory with the service metadata.</param>
        /// <returns></returns>
        internal static Lazy<T, TMetadata> CreateLazyWithMetadata<T, TMetadata>(Meta<Lazy<T>, TMetadata> metaFactory)
        {
            return metaFactory == null || metaFactory.Value == null ? null :
                new Lazy<T, TMetadata>(() => metaFactory.Value.Value, metaFactory.Metadata);
        }

        private static readonly Made _createLazyWithMetadataMethod = Made.Of(
            typeof(AttributedModel).GetSingleMethodOrNull("CreateLazyWithMetadata", includeNonPublic: true));

        #endregion

        #region Support for multiple same (non-unique) contract names for the same exported type

        /// <summary>Add support for using the same contract name for the same multiple exported types.</summary>
        /// <param name="container">Source container.</param> <returns>New container.</returns>
        public static IContainer WithMultipleSameContractNamesSupport(this IContainer container)
        {
            // map to convert the non-unique keys into an unique ones: ContractName/Key -> { ContractType, count }[]
            container.UseInstance(new ServiceKeyStore());

            // decorator to filter in a presence of multiple same keys
            // note: it explicitly set to Transient to produce new results for new filtered collection,
            // otherwise it may be set to Singleton by container wide rules and always produce the results for the first resolved collection
            container.Register(typeof(IEnumerable<>), Reuse.Transient, _filterCollectionByMultiKey, Setup.Decorator);

            return container;
        }

        private static readonly Made _filterCollectionByMultiKey = Made.Of(
            typeof(AttributedModel).GetSingleMethodOrNull("FilterCollectionByMultiKey", includeNonPublic: true),
            parameters: Parameters.Of.Type(request => request.ServiceKey));

        internal static IEnumerable<T> FilterCollectionByMultiKey<T>(IEnumerable<KeyValuePair<object, T>> source, object serviceKey)
        {
            return serviceKey == null
                ? source.Select(it => it.Value)
                : source.Where(it =>
                {
                    if (it.Key is DefaultKey)
                        return false;
                    if (serviceKey.Equals(it.Key))
                        return true;
                    var multiKey = it.Key as KV<object, int>;
                    return multiKey != null && serviceKey.Equals(multiKey.Key);
                })
                .Select(it => it.Value);
        }

        #endregion

        /// <summary>Registers implementation type(s) with provided registrator/container. Expects that
        /// implementation type are annotated with <see cref="ExportAttribute"/>, or <see cref="ExportManyAttribute"/>.</summary>
        /// <param name="registrator">Container to register types into.</param>
        /// <param name="types">Provides types to peek exported implementation types from.</param>
        public static void RegisterExports(this IRegistrator registrator, IEnumerable<Type> types)
        {
            var exportedRegistrationInfos = types.ThrowIfNull().SelectMany(GetExportedRegistrations);
            registrator.RegisterExports(exportedRegistrationInfos);
        }

        /// <summary>Registers implementation type(s) with provided registrator/container. Expects that
        /// implementation type are annotated with <see cref="ExportAttribute"/>, or <see cref="ExportManyAttribute"/>.</summary>
        /// <param name="registrator">Container to register types into.</param>
        /// <param name="types">Implementation types to register.</param>
        public static void RegisterExports(this IRegistrator registrator, params Type[] types)
        {
            registrator.RegisterExports((IEnumerable<Type>)types);
        }

        /// <summary>First scans (<see cref="Scan"/>) provided assemblies to find types annotated with
        /// <see cref="ExportAttribute"/>, or <see cref="ExportManyAttribute"/>.
        /// Then registers found types into registrator/container.</summary>
        /// <param name="registrator">Container to register into</param>
        /// <param name="assemblies">Provides assemblies to scan for exported implementation types.</param>
        /// <remarks>In case of <see cref="ReflectionTypeLoadException"/> try get type with <see cref="ReflectionTools.GetLoadedTypes"/>.</remarks>
        public static void RegisterExports(this IRegistrator registrator, IEnumerable<Assembly> assemblies)
        {
            registrator.RegisterExports(Scan(assemblies));
        }

        /// <summary>Registers new factories into registrator/container based on provided registration info's, which
        /// is serializable DTO for registration.</summary>
        /// <param name="registrator">Container to register into.</param>
        /// <param name="registrations">Registrations to register.</param>
        public static void RegisterExports(this IRegistrator registrator, IEnumerable<ExportedRegistrationInfo> registrations)
        {
            var serviceKeyStore = new Lazy<ServiceKeyStore>(() =>
                ((IResolver)registrator).Resolve<ServiceKeyStore>(DryIoc.IfUnresolved.ReturnDefault));

            foreach (var info in registrations)
                RegisterInfo(registrator, info, serviceKeyStore);
        }

        /// <summary>Helper to apply laziness to provided registrations.</summary>
        /// <param name="registrations">The registrations to transform to lazy from</param>
        /// <returns>Transformed registrations.</returns>
        public static IEnumerable<ExportedRegistrationInfo> MakeLazyAndEnsureUniqueServiceKeys(
            this IEnumerable<ExportedRegistrationInfo> registrations)
        {
            var serviceKeyStore = new ServiceKeyStore();
            return registrations.Select(info => info.MakeLazy().EnsureUniqueExportServiceKeys(serviceKeyStore));
        }

        /// <summary>Registers factories into registrator/container based on single provided info, which could
        /// contain multiple exported services with single implementation.</summary>
        /// <param name="registrator">Container to register into.</param>
        /// <param name="info">Registration information provided.</param>
        /// <param name="serviceKeyStore">Multi key contract name store.</param>
        public static void RegisterInfo(this IRegistrator registrator, ExportedRegistrationInfo info, Lazy<ServiceKeyStore> serviceKeyStore = null)
        {
            serviceKeyStore = serviceKeyStore ?? new Lazy<ServiceKeyStore>(() =>
                ((IResolver)registrator).Resolve<ServiceKeyStore>(DryIoc.IfUnresolved.ReturnDefault));

            // factory is used for all exports of implementation
            var factory = info.CreateFactory();

            var exports = info.Exports;
            for (var i = 0; i < exports.Length; i++)
            {
                var export = exports[i];

                var serviceType = export.ServiceType;
                var serviceKey = export.ServiceKey;
                if (serviceKey != null)
                {
                    var store = serviceKeyStore.Value;
                    if (store != null)
                        serviceKey = store.EnsureUniqueServiceKey(serviceType, serviceKey);
                }

                registrator.Register(factory, serviceType, serviceKey, export.IfAlreadyRegistered,
                    isStaticallyChecked: true); // may be set to true, cause we're reflecting from the compiler checked code
            }
        }

        /// <summary>Scans assemblies to find concrete type annotated with <see cref="ExportAttribute"/>, or <see cref="ExportManyAttribute"/>
        /// attributes, and create serializable DTO with all information required for registering of exported types.</summary>
        /// <param name="assemblies">Assemblies to scan.</param>
        /// <returns>Lazy collection of registration info DTOs.</returns>
        public static IEnumerable<ExportedRegistrationInfo> Scan(IEnumerable<Assembly> assemblies)
        {
            return assemblies
                .Distinct()
                .SelectMany(Portable.GetAssemblyTypes)
                .SelectMany(GetExportedRegistrations);
        }

        /// <summary>Creates registration info DTOs for provided type and/or for exported members.
        /// If no exports found, the method returns empty enumerable.</summary>
        /// <param name="type">Type to convert into registration infos.</param>
        /// <returns>Created DTOs.</returns>
        public static IEnumerable<ExportedRegistrationInfo> GetExportedRegistrations(Type type)
        {
            if (!CanBeExported(type))
                yield break;

            ExportedRegistrationInfo typeRegistrationInfo = null;

            // Export does not make sense for static or abstract type
            // because the instance of such type can't be created (resolved).
            if (!type.IsStatic() && !type.IsAbstract())
            {
                var typeAttributes = GetAllExportAttributes(type);
                if (IsExportDefined(typeAttributes))
                {
                    typeRegistrationInfo = GetRegistrationInfoOrDefault(type, typeAttributes);
                    if (typeRegistrationInfo != null)
                        yield return typeRegistrationInfo;
                }
            }

            var members = type.GetAllMembers(includeBase: true);
            foreach (var member in members)
            {
                var memberAttributes = member.GetAttributes().ToArrayOrSelf();
                if (!IsExportDefined(memberAttributes))
                    continue;

                var memberReturnType = member.GetReturnTypeOrDefault();
                var memberRegistrationInfo = GetRegistrationInfoOrDefault(memberReturnType, memberAttributes)
                    .ThrowIfNull();

                var factoryMethod = new FactoryMethodInfo
                {
                    DeclaringType = type,
                    MemberName = member.Name
                };

                if (!member.IsStatic())
                {
                    // if no export for instance factory, then add one
                    if (typeRegistrationInfo == null)
                    {
                        // todo: Review need for factory service key
                        // - May be export factory AsWrapper to hide from collection resolution
                        // - Use an unique (GUID) service key
                        var factoryKey = Constants.InstanceFactory;

                        var factoryTypeAttributes = new Attribute[] { new ExportAttribute(factoryKey) };
                        typeRegistrationInfo = GetRegistrationInfoOrDefault(type, factoryTypeAttributes).ThrowIfNull();
                        yield return typeRegistrationInfo;
                    }

                    // note: the first export is used for instance factory, the rest is ignored
                    factoryMethod.InstanceFactory = typeRegistrationInfo.Exports[0];
                }

                var method = member as MethodInfo;
                if (method != null)
                {
                    factoryMethod.MethodParameterTypeFullNamesOrNames = method.GetParameters()
                        .Select(p => p.ParameterType.FullName ?? p.ParameterType.Name)
                        .ToArrayOrSelf();

                    // the only possibility (for now) for registering completely generic T service
                    // is registering it as an Object
                    if (memberReturnType.IsGenericParameter &&
                        memberRegistrationInfo.FactoryType == DryIoc.FactoryType.Decorator)
                    {
                        var exports = memberRegistrationInfo.Exports;
                        for (var i = 0; i < exports.Length; ++i)
                            exports[i].ServiceType = typeof(object);
                    }
                }

                memberRegistrationInfo.FactoryMethodInfo = factoryMethod;

                // If member reuse is not provided get it from the declaring type (fix for #355)
                if (memberRegistrationInfo.Reuse == null)
                {
                    if (typeRegistrationInfo != null)
                        memberRegistrationInfo.Reuse = typeRegistrationInfo.Reuse;
                    else
                    {
                        var creationPolicyAttrs = type.GetAttributes(typeof(PartCreationPolicyAttribute), inherit: true);
                        if (creationPolicyAttrs.Length != 0)
                            memberRegistrationInfo.Reuse = GetReuseInfo((PartCreationPolicyAttribute)creationPolicyAttrs[0]);
                    }
                }

                yield return memberRegistrationInfo;
            }
        }

        /// <summary>Creates and index by service type name.
        /// Then returns the factory provider which uses index for fast registration discovery.</summary>
        /// <param name="lazyRegistrations">Registrations with <see cref="ExportedRegistrationInfo.IsLazy"/> set to true.
        /// Consider to call <see cref="MakeLazyAndEnsureUniqueServiceKeys"/> on registrations before passing them here.</param>
        /// <param name="getAssembly">Assembly to load type by name from. NOTE: The assembly will be loaded only once!</param>
        /// <param name="ifAlreadyRegistered">(optional) Keep existing registrations by default.</param>
        /// <param name="otherServiceExports">(optional) Index to share with other providers,
        /// if not specified - each provider will use its own. The index maps the full service name
        /// from <paramref name="lazyRegistrations"/> to its registration and (optional) service key pairs.</param>
        /// <returns><see cref="Rules.DynamicRegistrationProvider"/></returns>
        public static Rules.DynamicRegistrationProvider GetLazyTypeRegistrationProvider(
            this IEnumerable<ExportedRegistrationInfo> lazyRegistrations, Func<Assembly> getAssembly,
            IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.Keep,
            IDictionary<string, IList<KeyValuePair<object, ExportedRegistrationInfo>>> otherServiceExports = null)
        {
            var assembly = new Lazy<Assembly>(getAssembly);
            return lazyRegistrations.GetLazyTypeRegistrationProvider(
                t => assembly.Value.GetType(t), ifAlreadyRegistered, otherServiceExports);
        }

        /// <summary>Creates and index by service type name.
        /// Then returns the factory provider which uses index for fast registration discovery.</summary>
        /// <param name="lazyRegistrations">Registrations with <see cref="ExportedRegistrationInfo.IsLazy"/> set to true.
        /// Consider to call <see cref="MakeLazyAndEnsureUniqueServiceKeys"/> on registrations before passing them here.</param>
        /// <param name="typeProvider">Required for Lazy registration info to create actual Type from type name.</param>
        /// <param name="ifAlreadyRegistered">(optional) Keep existing registrations by default.</param>
        /// <param name="otherServiceExports">(optional) Index to share with other providers,
        /// if not specified - each provider will use its own. The index maps the full service name
        /// from <paramref name="lazyRegistrations"/> to its registration and (optional) service key pairs.</param>
        /// <returns><see cref="Rules.DynamicRegistrationProvider"/></returns>
        public static Rules.DynamicRegistrationProvider GetLazyTypeRegistrationProvider(
            this IEnumerable<ExportedRegistrationInfo> lazyRegistrations,
            Func<string, Type> typeProvider,
            IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.Keep,
            IDictionary<string, IList<KeyValuePair<object, ExportedRegistrationInfo>>> otherServiceExports = null)
        {
            otherServiceExports = otherServiceExports
                ?? new Dictionary<string, IList<KeyValuePair<object, ExportedRegistrationInfo>>>();

            foreach (var reg in lazyRegistrations)
            {
                var exports = reg.Exports;
                for (var i = 0; i < exports.Length; i++)
                {
                    var e = exports[i];
                    IList<KeyValuePair<object, ExportedRegistrationInfo>> expRegs;
                    if (!otherServiceExports.TryGetValue(e.ServiceTypeFullName, out expRegs))
                        otherServiceExports[e.ServiceTypeFullName] =
                            expRegs = new List<KeyValuePair<object, ExportedRegistrationInfo>>();
                    expRegs.Add(new KeyValuePair<object, ExportedRegistrationInfo>(e.ServiceKey, reg));
                }
            }

            return (serviceType, serviceKey) =>
            {
                IList<KeyValuePair<object, ExportedRegistrationInfo>> regs;
                return otherServiceExports.TryGetValue(serviceType.FullName, out regs)
                    ? regs.Map(r => new DynamicRegistration(r.Value.CreateFactory(typeProvider), ifAlreadyRegistered, r.Key))
                    : null;
            };
        }

        private static bool CanBeExported(Type type)
        {
            return type.IsClass() && !type.IsCompilerGenerated();
        }

        private static ReuseInfo GetReuseInfo(PartCreationPolicyAttribute attribute)
        {
            var reuseType = attribute.CreationPolicy == CreationPolicy.NonShared
                ? ReuseType.Transient
                : ReuseType.Singleton;
            return new ReuseInfo { ReuseType = reuseType };
        }

        /// <summary>Converts reuse info into pre-defined (<see cref="SupportedReuseTypes"/>) or custom reuse object.</summary>
        /// <param name="reuseInfo">Reuse type to find in supported.</param>
        /// <returns>DryIoc reuse object.</returns>
        public static IReuse GetReuse(ReuseInfo reuseInfo)
        {
            if (reuseInfo == null)
                return null; // unspecified reuse, decided by container rules

            if (reuseInfo.CustomReuseType != null)
                return reuseInfo.ScopeName == null
                    ? (IReuse)Activator.CreateInstance(reuseInfo.CustomReuseType)
                    : (IReuse)Activator.CreateInstance(reuseInfo.CustomReuseType, reuseInfo.ScopeName);

            return SupportedReuseTypes.GetValueOrDefault(reuseInfo.ReuseType)
                .ThrowIfNull(Error.UnsupportedReuseType, reuseInfo.ReuseType)
                .Invoke(reuseInfo.ScopeName);
        }

        #region Rules

        private static FactoryMethod GetImportingConstructor(Request request, FactoryMethodSelector fallbackSelector = null)
        {
            var implType = request.ImplementationType;
            var ctors = implType.GetPublicInstanceConstructors().ToArrayOrSelf();
            var ctor = ctors.Length == 1 ? ctors[0]
                : ctors.SingleOrDefault(it => it.GetAttributes(typeof(ImportingConstructorAttribute)).Any());

            if (ctor == null)
            {
                // next try to fallback defined constructor, it may be defined as ConstructorWithResolvableArguments
                if (fallbackSelector != null)
                {
                    var fallbackCtor = fallbackSelector(request);
                    if (fallbackCtor != null)
                        return fallbackCtor;
                }

                // at the end try default constructor
                ctor = ctors.SingleOrDefault(it => it.GetParameters().Length == 0);
            }

            ctor.ThrowIfNull(Error.NoSingleCtorWithImportingAttr, implType, request);
            return FactoryMethod.Of(ctor);
        }

        private static Func<ParameterInfo, ParameterServiceInfo> GetImportedParameter(Request request)
        {
            return parameter =>
            {
                var serviceInfo = ParameterServiceInfo.Of(parameter);
                var attrs = parameter.GetAttributes().ToArray();
                return attrs.Length == 0 ? serviceInfo :
                    serviceInfo.WithDetails(GetFirstImportDetailsOrNull(parameter.ParameterType, attrs, request), request);
            };
        }

        private static readonly PropertiesAndFieldsSelector _getImportedPropertiesAndFields =
            PropertiesAndFields.All(withInfo: GetImportedPropertiesAndFieldsOnly);

        private static PropertyOrFieldServiceInfo GetImportedPropertiesAndFieldsOnly(MemberInfo member, Request request)
        {
            var attributes = member.GetAttributes().ToArrayOrSelf();
            var details = attributes.Length == 0 ? null
                : GetFirstImportDetailsOrNull(member.GetReturnTypeOrDefault(), attributes, request);
            return details == null ? null : PropertyOrFieldServiceInfo.Of(member).WithDetails(details, request);
        }

        private static ServiceDetails GetFirstImportDetailsOrNull(Type type, Attribute[] attributes, Request request)
        {
            return GetImportDetails(type, attributes, request)
                ?? GetImportExternalDetails(type, attributes, request);
        }

        private static ServiceDetails GetImportDetails(Type type, Attribute[] attributes, Request request)
        {
            object serviceKey;
            Type requiredServiceType;
            var ifUnresolved = DryIoc.IfUnresolved.Throw;

            var metadata = GetRequiredMetadata(attributes);

            var import = GetSingleAttributeOrDefault<ImportAttribute>(attributes);
            if (import == null)
            {
                var importMany = GetSingleAttributeOrDefault<ImportManyAttribute>(attributes);
                if (importMany == null)
                    return null;

                serviceKey = importMany.ContractName;
                requiredServiceType = importMany.ContractType;
            }
            else
            {
                serviceKey = import.ContractName;
                if (serviceKey == null)
                {
                    var importEx = import as ImportExAttribute;
                    if (importEx != null)
                        serviceKey = importEx.ContractKey;
                }

                requiredServiceType = import.ContractType;
                if (import.AllowDefault)
                    ifUnresolved = DryIoc.IfUnresolved.ReturnDefault;
            }

            // handle not-found, probably base or object type imports
            if (requiredServiceType == null && serviceKey != null)
                requiredServiceType = FindRequiredServiceTypeByServiceKey(type, request, serviceKey);

            return ServiceDetails.Of(requiredServiceType, serviceKey, ifUnresolved, null, metadata.Key, metadata.Value);
        }

        private static Type FindRequiredServiceTypeByServiceKey(Type type, Request request, object serviceKey)
        {
            var contractNameStore = request.Container.Resolve<ServiceKeyStore>(DryIoc.IfUnresolved.ReturnDefault);
            if (contractNameStore == null)
                return null;

            // may be null if service key is registered at all Or registered not through MEF
            var serviceTypes = contractNameStore.GetServiceTypesOrDefault(serviceKey);
            if (serviceTypes == null)
                return null;

            // required when importing the wrappers
            var unwrappedType = request.Container.GetWrappedType(type, null);

            // first filter out non compatible / assignable types
            if (serviceTypes.Length != 1)
            {
                serviceTypes = serviceTypes.Match(t => t.Key.IsAssignableTo(unwrappedType));
                if (serviceTypes.Length > 1)
                    Throw.It(Error.UnableToSelectFromMultipleTypes, serviceTypes, KV.Of(serviceKey, type));
            }

            if (serviceTypes.Length == 1)
            {
                var exportedType = serviceTypes[0].Key;
                if (exportedType.IsAssignableTo(unwrappedType))
                    return exportedType;
            }
            else if (serviceTypes.Length > 1)
            {
                // todo: multiple required types are not supported at the moment
                Throw.It(DryIoc.Error.Of("Multiple required types are not supported at the moment: {0}"), serviceTypes);
            }

            return null;
        }

        internal static KeyValuePair<string, object> ComposeServiceKeyMetadata(object serviceKey, Type serviceType)
        {
            return new KeyValuePair<string, object>(
                string.Concat(serviceKey.GetHashCode(), ":", serviceType.FullName),
                serviceKey);
        }

        private static KeyValuePair<string, object> GetRequiredMetadata(Attribute[] attributes)
        {
            var withMetadataAttr = GetSingleAttributeOrDefault<WithMetadataAttribute>(attributes);
            var metadata = withMetadataAttr == null ? null : withMetadataAttr.Metadata;
            var metadataKey = withMetadataAttr == null ? null
                : withMetadataAttr.MetadataKey == null ? Constants.ExportMetadataDefaultKey
                : withMetadataAttr.MetadataKey;

            return new KeyValuePair<string, object>(metadataKey, metadata);
        }

        private static ServiceDetails GetImportExternalDetails(Type serviceType, Attribute[] attributes, Request request)
        {
            var import = GetSingleAttributeOrDefault<ImportExternalAttribute>(attributes);
            if (import == null)
                return null;

            var container = request.Container;
            serviceType = import.ContractType ?? container.GetWrappedType(serviceType, request.RequiredServiceType);
            var serviceKey = import.ContractKey;

            IDictionary<string, object> setupMetadata = null;

            // will be used for resolution and for registration setup, if service is not registered
            var metadataValue = import.Metadata;
            var metadataKey = import.MetadataKey;
            if (metadataKey == null && metadataValue != null) // set default key if absent
                metadataKey = Constants.ExportMetadataDefaultKey;

            if (metadataKey != null || metadataValue != null)
                setupMetadata = new Dictionary<string, object> { { metadataKey, metadataValue } };

            if (serviceKey != null)
            {
                var serviceKeyMetadata = ComposeServiceKeyMetadata(serviceKey, serviceType);

                setupMetadata = setupMetadata ?? new Dictionary<string, object>();
                setupMetadata.Add(serviceKeyMetadata.Key, serviceKeyMetadata.Value);

                if (metadataValue == null && metadataKey == null)
                {
                    metadataKey = serviceKeyMetadata.Key;
                    metadataValue = serviceKeyMetadata.Value;
                }
            }

            if (!container.IsRegistered(serviceType, serviceKey))
            {
                var implementationType = import.ImplementationType ?? serviceType;

                var reuseAttr = GetSingleAttributeOrDefault<ReuseAttribute>(attributes);
                var reuse = reuseAttr == null ? null
                    : GetReuse(new ReuseInfo { ReuseType = reuseAttr.ReuseType, ScopeName = reuseAttr.ScopeName });

                var impl = import.ConstructorSignature == null ? null
                    : Made.Of(t => t.GetConstructorOrNull(args: import.ConstructorSignature));

                container.Register(serviceType, implementationType, reuse, impl,
                    Setup.With(setupMetadata), IfAlreadyRegistered.Keep, serviceKey);
            }

            // the default because we intentionally register the service and expect it to be available
            var ifUnresolved = DryIoc.IfUnresolved.Throw;

            return ServiceDetails.Of(serviceType, serviceKey, ifUnresolved, null, metadataKey, metadataValue);
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

        private static ExportedRegistrationInfo GetRegistrationInfoOrDefault(Type type, Attribute[] attributes)
        {
            if (type.IsOpenGeneric())
                type = type.GetGenericTypeDefinition();

            var info = new ExportedRegistrationInfo { ImplementationType = type, Reuse = null };

            for (var attrIndex = 0; attrIndex < attributes.Length; attrIndex++)
            {
                var attribute = attributes[attrIndex];
                if (attribute is ExportExAttribute)
                {
                    info.Exports = GetExportsFromExportExAttribute((ExportExAttribute)attribute, info, type);
                }
                else if (attribute is ExportManyAttribute)
                {
                    info.Exports = GetExportsFromExportManyAttribute((ExportManyAttribute)attribute, info, type);
                }
                else if (attribute is ExportAttribute)
                {
                    info.Exports = GetExportsFromExportAttribute((ExportAttribute)attribute, info, type);
                }
                else if (attribute is PartCreationPolicyAttribute)
                {
                    info.Reuse = GetReuseInfo((PartCreationPolicyAttribute)attribute);
                }
                else if (attribute is ReuseAttribute)
                {
                    var resueAttr = (ReuseAttribute)attribute;
                    info.Reuse = resueAttr.CustomReuseType == null
                        ? new ReuseInfo { ReuseType = resueAttr.ReuseType, ScopeName = resueAttr.ScopeName }
                        : new ReuseInfo { CustomReuseType = resueAttr.CustomReuseType, ScopeName = resueAttr.ScopeName };
                }
                else if (attribute is OpenResolutionScopeAttribute)
                {
                    info.OpenResolutionScope = true;
                }
                else if (attribute is AsResolutionCallAttribute)
                {
                    info.AsResolutionCall = true;
                }
                else if (attribute is AsResolutionRootAttribute)
                {
                    info.AsResolutionRoot = true;
                }
                else if (attribute is WeaklyReferencedAttribute)
                {
                    info.WeaklyReferenced = true;
                }
                else if (attribute is PreventDisposalAttribute)
                {
                    info.PreventDisposal = true;
                }
                else if (attribute is AllowDisposableTransientAttribute)
                {
                    info.AllowDisposableTransient = true;
                }
                else if (attribute is TrackDisposableTransientAttribute)
                {
                    info.TrackDisposableTransient = true;
                }
                else if (attribute is UseParentReuseAttribute)
                {
                    info.UseParentReuse = true;
                }
                else if (attribute is AsWrapperAttribute)
                {
                    PopulateWrapperInfoFromAttribute(info, (AsWrapperAttribute)attribute, type);
                }
                else if (attribute is AsDecoratorAttribute)
                {
                    PopulateDecoratorInfoFromAttribute(info, (AsDecoratorAttribute)attribute, type);
                }
                else if (attribute is ExportConditionAttribute)
                {
                    info.ConditionType = attribute.GetType();
                }

                if (attribute is ExportAttribute || attribute is WithMetadataAttribute ||
                    attribute.GetType().GetAttributes(typeof(MetadataAttributeAttribute), true).Any())
                {
                    info.HasMetadataAttribute = true;
                }
            }

            if (info.HasMetadataAttribute)
                info.InitExportedMetadata(attributes);

            info.Exports.ThrowIfNull(Error.NoExport, type);
            return info;
        }

        private static bool IsExportDefined(Attribute[] attributes)
        {
            return attributes.Length != 0
                && attributes.IndexOf(a => a is ExportAttribute || a is ExportManyAttribute) != -1
                && attributes.IndexOf(a => a is PartNotDiscoverableAttribute) == -1;
        }

        private static ExportInfo[] GetExportsFromExportAttribute(ExportAttribute attribute,
            ExportedRegistrationInfo info, Type implementationType)
        {
            var export = new ExportInfo(attribute.ContractType ?? implementationType,
                attribute.ContractName ??
#pragma warning disable 618 // ExportWithKeyAttribute is Obsolete.
                (attribute is ExportWithKeyAttribute ? ((ExportWithKeyAttribute)attribute).ContractKey : null));
#pragma warning restore 618

            // Overrides the existing export with new one (will override export from Export Many)
            return info.Exports.AppendOrUpdate(export, info.Exports.IndexOf(export));
        }

        private static ExportInfo[] GetExportsFromExportExAttribute(ExportExAttribute attribute,
            ExportedRegistrationInfo info, Type implementationType)
        {
            var export = new ExportInfo(
                attribute.ContractType ?? implementationType,
                attribute.ContractKey,
                GetIfAlreadyRegistered(attribute.IfAlreadyExported));

            // Overrides the existing export with new one (will override export from Export Many)
            return info.Exports.AppendOrUpdate(export, info.Exports.IndexOf(export));
        }

        private static IfAlreadyRegistered GetIfAlreadyRegistered(IfAlreadyExported ifAlreadyExported)
        {
            switch (ifAlreadyExported)
            {
                case IfAlreadyExported.Throw: return IfAlreadyRegistered.Throw;
                case IfAlreadyExported.Keep: return IfAlreadyRegistered.Keep;
                case IfAlreadyExported.Replace: return IfAlreadyRegistered.Replace;
                case IfAlreadyExported.AppendNewImplementation: return IfAlreadyRegistered.AppendNewImplementation;
                default: return IfAlreadyRegistered.AppendNotKeyed;
            }
        }

        private static ExportInfo[] GetExportsFromExportManyAttribute(ExportManyAttribute attribute,
            ExportedRegistrationInfo info, Type implementationType)
        {
            var contractTypes = implementationType.GetRegisterManyImplementedServiceTypes(attribute.NonPublic);
            if (!attribute.Except.IsNullOrEmpty())
                contractTypes = contractTypes.Except(attribute.Except).ToArrayOrSelf();

            var manyExports = contractTypes
                .Map(contractType => new ExportInfo(contractType,
                    attribute.ContractName ?? attribute.ContractKey, GetIfAlreadyRegistered(attribute.IfAlreadyExported)));

            Throw.If(manyExports.Length == 0, Error.ExportManyDoesNotExportAnyType, implementationType, contractTypes);

            // Filters exports that were already made, because ExportMany has less priority than Export(Ex)
            var currentExports = info.Exports;
            if (currentExports.IsNullOrEmpty())
            {
                currentExports = manyExports;
            }
            else
            {
                for (var i = 0; i < manyExports.Length; i++)
                {
                    var manyExport = manyExports[i];
                    if (!currentExports.Contains(manyExport))
                        currentExports = currentExports.AppendOrUpdate(manyExport);
                }
            }

            return currentExports;
        }

        private static void PopulateWrapperInfoFromAttribute(ExportedRegistrationInfo resultInfo, AsWrapperAttribute attribute,
            Type implementationType)
        {
            Throw.If(resultInfo.FactoryType != DryIoc.FactoryType.Service, Error.UnsupportedMultipleFactoryTypes, implementationType);
            resultInfo.FactoryType = DryIoc.FactoryType.Wrapper;
            resultInfo.Wrapper = new WrapperInfo
            {
                WrappedServiceTypeArgIndex = attribute.WrappedServiceTypeArgIndex,
                AlwaysWrapsRequiredServiceType = attribute.AlwaysWrapsRequiredServiceType
            };
        }

        private static void PopulateDecoratorInfoFromAttribute(ExportedRegistrationInfo resultInfo, AsDecoratorAttribute attribute,
            Type implementationType)
        {
            Throw.If(resultInfo.FactoryType != DryIoc.FactoryType.Service, Error.UnsupportedMultipleFactoryTypes, implementationType);
            resultInfo.FactoryType = DryIoc.FactoryType.Decorator;
            var decoratedServiceKey = attribute.ContractName ?? attribute.ContractKey;
            resultInfo.Decorator = new DecoratorInfo
            {
                DecoratedServiceKey = decoratedServiceKey,
                Order = attribute.Order,
                UseDecorateeReuse = attribute.UseDecorateeReuse
            };
        }

        private static Attribute[] GetAllExportAttributes(Type type)
        {
            var attributes = type.GetAttributes();

            for (var baseType = type.GetBaseType();
                baseType != typeof(object) && baseType != null;
                baseType = baseType.GetBaseType())
                attributes = attributes.Append(GetInheritedExportAttributes(baseType));

            var interfaces = type.GetImplementedInterfaces();
            if (interfaces.Length != 0)
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

        #endregion
    }

    /// <summary>Enables de-duplication of service key by putting key into the pair with index. </summary>
    public sealed class ServiceKeyStore
    {
        // Mapping of ServiceKey/ContractName to { ContractType, count }[]
        private readonly Ref<ImTreeMap<object, KV<Type, int>[]>>
            _store = Ref.Of(ImTreeMap<object, KV<Type, int>[]>.Empty);

        /// <summary>Stores the key with respective type,
        /// incrementing type count for multiple registrations with same key  and type.</summary>
        /// <param name="serviceType">Type</param> <param name="serviceKey">Key</param>
        /// <returns>The key combined with index, if the key has same type more than once,
        /// otherwise (for single or nu types) returns passed key as-is..</returns>
        public object EnsureUniqueServiceKey(Type serviceType, object serviceKey)
        {
            _store.Swap(it => it
                .AddOrUpdate(serviceKey, new[] { KV.Of(serviceType, 1) }, (types, newTypes) =>
                  {
                      var newType = newTypes[0].Key;
                      var typeAndCountIndex = types.IndexOf(t => t.Key == newType);
                      if (typeAndCountIndex != -1)
                      {
                          var typeAndCount = types[typeAndCountIndex];

                      // Change the serviceKey only when multiple same types are registered with the same key
                      serviceKey = KV.Of(serviceKey, typeAndCount.Value);

                          typeAndCount = typeAndCount.WithValue(typeAndCount.Value + 1);
                          return types.AppendOrUpdate(typeAndCount, typeAndCountIndex);
                      }

                      return types.Append(newTypes);
                  }));

            return serviceKey;
        }

        /// <summary>Retrieves types and their count used with specified <paramref name="serviceKey"/>.</summary>
        /// <param name="serviceKey">Service key to get info.</param>
        /// <returns>Types and their count for the specified key, if key is not stored - returns null.</returns>
        public KV<Type, int>[] GetServiceTypesOrDefault(object serviceKey)
        {
            return _store.Value.GetValueOrDefault(serviceKey);
        }
    }

    /// <summary>Names used by Attributed Model to mark the special exports.</summary>
    public static class Constants
    {
        /// <summary>Predefined key in metadata dictionary for metadata provided as single object (not dictionary).</summary>
        public static readonly string ExportMetadataDefaultKey = "@ExportMetadataDefaultKey";

        /// <summary>Marks the Export generated for type which export its instance members,
        /// but should not be resolved as-self by default.</summary>
        public static readonly string InstanceFactory = "@InstanceFactory";
    }

    /// <summary>Defines error codes and messages for <see cref="AttributedModelException"/>.</summary>
    public static class Error
    {
        /// <summary>Error messages for corresponding codes.</summary>
        public static readonly IList<string> Messages = new List<string>(20);

        /// <summary>Codes are starting from this value.</summary>
        public static readonly int FirstErrorCode = DryIoc.Error.FirstErrorCode + DryIoc.Error.Messages.Count;

#pragma warning disable 1591 // Missing XML-comment
        public static readonly int
            NoSingleCtorWithImportingAttr = Of(
                "Unable to find single constructor: nor marked with " + typeof(ImportingConstructorAttribute) +
                " nor default constructor in {0} when resolving: {1}"),
            UnsupportedMultipleFactoryTypes = Of(
                "Found multiple factory types associated with exported {0}. Only single ExportAs.. attribute is supported, please remove the rest."),
            DuplicateMetadataKey = Of(
                "Duplicate metadata key {0} for the already defined {1}."),
            NoExport = Of(
                "At least one Export attributed should be defined for {0}."),
            ExportManyDoesNotExportAnyType = Of(
                "Unable to get contract types for implementation {0} because all of its implemented types where filtered out: {1}"),
            UnsupportedReuseType = Of(
                "Attributed model does not support reuse type {0}."),
            UnsupportedReuseWrapperType = Of(
                "Attributed model does not support reuse wrapper type {0}."),
            UnableToSelectFromMultipleTypes = Of(
                "Unable to select from multiple exported types {0} for the import {1}");

#pragma warning restore 1591

        /// <summary>Returns message by provided error code.</summary>
        /// <param name="error">Code starting from <see cref="FirstErrorCode"/></param> <returns>String message.</returns>
        public static string GetMessage(int error)
        {
            return Messages[error - FirstErrorCode];
        }

        /// <summary>Returns the ID of error message.</summary> <param name="message"></param> <returns></returns>
        public static int Of(string message)
        {
            Messages.Add(message);
            return FirstErrorCode + Messages.Count - 1;
        }

        #region Implementation

        static Error()
        {
            Throw.GetMatchedException = GetAttributedModelOrContainerException;
        }

        private static Exception GetAttributedModelOrContainerException(ErrorCheck check, int error, object arg0, object arg1, object arg2, object arg3, Exception inner)
        {
            return FirstErrorCode <= error && error < FirstErrorCode + Messages.Count
                ? AttributedModelException.Of(check, error, arg0, arg1, arg2, arg3, inner)
                : ContainerException.Of(check, error, arg0, arg1, arg2, arg3, inner);
        }

        #endregion
    }

    /// <summary>Specific exception type to be thrown by MefAttributedModel extension. Check <see cref="Error"/> for possible error cases.</summary>
    public class AttributedModelException : ContainerException
    {
        /// <summary>Creates exception by wrapping <paramref name="errorCode"/> and with message corresponding to code.</summary>
        /// <param name="errorCheck">Type of check.</param> <param name="errorCode">Error code to wrap, <see cref="Error"/> for codes defined.</param>
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

        private AttributedModelException(int error, string message) : base(error, message) { }

        private AttributedModelException(int error, string message, Exception innerException) : base(error, message, innerException) { }
    }

    /// <summary>Converts provided literal into valid C# code. Used for generating registration code
    /// from <see cref="ExportedRegistrationInfo"/> DTOs.</summary>
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
            return x == null ? code.Append("null") : code.Append('"').Append(x.Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n")).Append('"');
        }

        /// <summary>Prints valid c# Type literal: typeof(Namespace.Type).</summary>
        /// <param name="code">Code to print to.</param> <param name="x">Value to print.</param> <returns>Code with appended literal.</returns>
        public static StringBuilder AppendType(this StringBuilder code, Type x)
        {
            return x == null ? code.Append("null") : code.Append("typeof(")
                .Print(x, t => t == typeof(void) ? "void" : t.FullName ?? t.Name)
                .Append(')');
        }

        /// <summary>Prints valid c# Enum literal: Enum.Value.</summary>
        /// <param name="code">Code to print to.</param>
        /// <param name="enumType">Enum type of the value.</param>
        /// <param name="x">Value to print.</param> <returns>Code with appended literal.</returns>
        public static StringBuilder AppendEnum(this StringBuilder code, Type enumType, object x)
        {
            if (enumType.IsNullable())
            {
                if (x == null)
                    return code.Print("null");

                enumType = enumType.GetTypeInfo().GenericTypeArguments.Single();
            }

            return code.Print(enumType, t => t.FullName ?? t.Name).Append('.').Append(Enum.GetName(enumType, x));
        }

        /// <summary>Prints the <see cref="Dictionary{TKey, TValue}"/> where keys are strings.</summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="code">The code.</param>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="appendValue">The callback to append a value (optional).</param>
        public static StringBuilder AppendDictionary<TValue>(this StringBuilder code, IDictionary<string, TValue> dictionary, Func<StringBuilder, string, TValue, StringBuilder> appendValue = null)
        {
            if (appendValue == null)
                appendValue = (sb, key, value) => sb.AppendCode(value);

            code.Append("new System.Collections.Generic.Dictionary<string, ");
            code.Print(typeof(TValue));
            code.AppendLine("> {");
            foreach (var pair in dictionary.OrderBy(p => p.Key).ThenBy(p => p.Key.Length))
            {
                code.Append("            { ");
                code.AppendString(pair.Key);
                code.Append(", ");
                code = appendValue(code, pair.Key, pair.Value);
                code.AppendLine(" },");
            }

            code.Append("        }");
            return code;
        }

        /// <summary>Prints the <see cref="Dictionary{TKey, TValue}"/> where keys and values are strings.</summary>
        /// <param name="code">The code.</param>
        /// <param name="dictionary">The dictionary.</param>
        public static StringBuilder AppendDictionary(this StringBuilder code, IDictionary<string, string> dictionary)
        {
            return code.AppendDictionary(dictionary, (c, k, v) => c.AppendString(v));
        }

        /// <summary>Determines whether the type is null-able.</summary>
        /// <param name="type">The type to check.</param>
        public static bool IsNullable(this Type type)
        {
            return type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>Prints code items.</summary>
        /// <param name="code">Code to print to.</param>
        /// <param name="items">Items to print.</param> <returns>Code with appended items.</returns>
        public static StringBuilder AppendMany<T>(this StringBuilder code, IEnumerable<T> items)
        {
            code.Append("new ").Print(typeof(T)).Print("[] {");
            int count = 0;
            foreach (var item in items)
            {
                if (count++ != 0)
                    code.Append(", ");
                code.AppendCode(item);
            }
            return code.Append("}");
        }

        /// <summary>Prints valid c# literal depending of <paramref name="x"/> type.</summary>
        /// <param name="code">Code to print to.</param> <param name="x">Value to print.</param>
        /// <param name="ifNotRecognized">(optional) Delegate to print unrecognized value.</param>
        /// <returns>Code with appended item.</returns>
        public static StringBuilder AppendCode(this StringBuilder code, object x, Action<StringBuilder, object> ifNotRecognized = null)
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

    // todo: v3: combine the Boolean fields into one with bit flags
    /// <summary>Serializable DTO of all registration information.</summary>
    public sealed class ExportedRegistrationInfo
    {
        /// <summary>All exports defined for implementation type (registration).</summary>
        public ExportInfo[] Exports;

        /// <summary>Concrete type on what exports are defined: exported type.</summary>
        /// <remarks>May be null if <see cref="ImplementationTypeFullName"/> specified.</remarks>
        public Type ImplementationType;

        /// <summary>Full name of exported type. Enables type lazy-loading scenario.</summary>
        public string ImplementationTypeFullName;

        /// <summary>Indicate the lazy info with the type defined by its name instead of Runtime Type.</summary>
        public bool IsLazy { get { return ImplementationTypeFullName != null; } }

        /// <summary>Specifies the reuse information</summary>
        public ReuseInfo Reuse;

        /// <summary>Corresponds to <see cref="Setup.OpenResolutionScope"/>.</summary>
        public bool OpenResolutionScope;

        /// <summary>Corresponds to <see cref="Setup.AsResolutionCall"/>.</summary>
        public bool AsResolutionCall;

        /// <summary>Corresponds to <see cref="Setup.AsResolutionRoot"/>.</summary>
        public bool AsResolutionRoot;

        /// <summary>Specifies to prevent disposal of reused instance if it is disposable</summary>
        public bool PreventDisposal;

        /// <summary>Specifies to store reused instance as WeakReference.</summary>
        public bool WeaklyReferenced;

        /// <summary>Allows registering transient disposable. But the disposal is up to you.</summary>
        public bool AllowDisposableTransient;

        /// <summary>Turns On tracking of disposable transient dependency in parent scope or in open scope if resolved directly.</summary>
        public bool TrackDisposableTransient;

        /// <summary>Instructs to use parent reuse. Applied only if Reuse is not specified.</summary>
        public bool UseParentReuse;

        /// <summary>True if exported type has metadata.</summary>
        public bool HasMetadataAttribute;

        /// <summary>Gets or sets the metadata.</summary>
        public IDictionary<string, object> Metadata;

        /// <summary>Factory type to specify <see cref="Setup"/>.</summary>
        public DryIoc.FactoryType FactoryType;

        /// <summary>Type consisting of single method compatible with <see cref="Setup.Condition"/> type.</summary>
        public Type ConditionType;

        /// <summary>Not null if exported with <see cref="AsDecoratorAttribute"/>, contains info about decorator.</summary>
        public DecoratorInfo Decorator;

        /// <summary>Not null if exported with <see cref="AsWrapperAttribute"/>, contains info about wrapper.</summary>
        public WrapperInfo Wrapper;

        /// <summary>Not null for exported members.</summary>
        public FactoryMethodInfo FactoryMethodInfo;

        /// <summary>Returns new info with type representation as type full name string, instead of
        /// actual type.</summary> <returns>New lazy ExportInfo for not lazy this, otherwise - this one.</returns>
        public ExportedRegistrationInfo MakeLazy()
        {
            if (IsLazy) return this;
            var newInfo = (ExportedRegistrationInfo)MemberwiseClone();
            newInfo.ImplementationTypeFullName = ImplementationType.FullName;
            newInfo.ImplementationType = null;
            var exports = newInfo.Exports;
            for (var i = 0; i < exports.Length; i++)
                exports[i] = exports[i].MakeLazy();
            if (newInfo.FactoryMethodInfo != null)
                newInfo.FactoryMethodInfo = newInfo.FactoryMethodInfo.MakeLazy();
            return newInfo;
        }

        /// <summary>De-duplicates service keys in export via tracking they uniqueness in passed store.
        /// The result key would be a pair of original key and index. If key is already unique it will be returned as-is.</summary>
        /// <param name="keyStore">Place to track and check the key uniqueness.</param>
        /// <returns>Modifies this, and return this just for fluency.</returns>
        public ExportedRegistrationInfo EnsureUniqueExportServiceKeys(ServiceKeyStore keyStore)
        {
            for (var i = 0; i < Exports.Length; i++)
            {
                var e = Exports[i];
                if (e.ServiceKey != null)
                    e.ServiceKey = keyStore.EnsureUniqueServiceKey(e.ServiceType, e.ServiceKey);
            }

            return this;
        }

        /// <summary>Creates factory from registration info.</summary>
        /// <param name="typeProvider">(optional) Required for Lazy registration info to create actual Type from type name.</param>
        /// <returns>Created factory.</returns>
        public ReflectionFactory CreateFactory(Func<string, Type> typeProvider = null)
        {
            if (!IsLazy)
                return new ReflectionFactory(ImplementationType, GetReuse(), GetMade(), GetSetup());

            typeProvider = typeProvider.ThrowIfNull();
            var made = GetMade(typeProvider);
            var setup = GetSetup(made);
            return new ReflectionFactory(() => typeProvider(ImplementationTypeFullName), GetReuse(), made, setup);
        }

        private Made GetMade(Func<string, Type> typeProvider = null)
        {
            if (FactoryMethodInfo == null)
                return Made.Default;
            return FactoryMethodInfo.CreateMade(typeProvider);
        }

        /// <summary>Gets the <see cref="IReuse"/> instance.</summary>
        public IReuse GetReuse()
        {
            return AttributedModel.GetReuse(Reuse);
        }

        /// <summary>Create factory setup from registration DTO.</summary>
        /// <param name="made">(optional) Used for collecting metadata from factory method attributes if any.</param>
        /// <returns>Created factory setup.</returns>
        public Setup GetSetup(Made made = null)
        {
            if (FactoryType == DryIoc.FactoryType.Wrapper)
                return Wrapper == null ? Setup.Wrapper : Wrapper.GetSetup();

            var condition = ConditionType == null
                ? (Func<Request, bool>)null
                : r => ((ExportConditionAttribute)Activator.CreateInstance(ConditionType))
                    .Evaluate(ConvertRequestInfo(r.RequestInfo));

            if (FactoryType == DryIoc.FactoryType.Decorator)
                return Decorator == null ? Setup.Decorator : Decorator.GetSetup(condition);

            object metadata = Metadata;
            if (metadata == null && HasMetadataAttribute)
                metadata = IsLazy
                    ? new Func<object>(() => CollectExportedMetadata(CollectMetadataAttributes(made)))
                    : (object)CollectExportedMetadata(CollectMetadataAttributes(made));

            return Setup.With(metadata, condition,
                OpenResolutionScope, AsResolutionCall, AsResolutionRoot,
                PreventDisposal, WeaklyReferenced,
                AllowDisposableTransient, TrackDisposableTransient,
                UseParentReuse);
        }

        private IEnumerable<Attribute> CollectMetadataAttributes(Made made)
        {
            if (ImplementationType == null)
                return ArrayTools.Empty<Attribute>();

            IEnumerable<Attribute> metaAttrs = ImplementationType.GetAttributes();
            if (made != null && made.FactoryMethodKnownResultType != null)
            {
                var member = made.FactoryMethod(request: null).ConstructorOrMethodOrMember;
                if (member != null)
                    metaAttrs = metaAttrs.Concat(member.GetAttributes());
            }
            return metaAttrs;
        }

        private static DryIocAttributes.RequestInfo ConvertRequestInfo(DryIoc.RequestInfo source)
        {
            if (source.IsEmpty)
                return RequestInfo.Empty;

            var factoryType =
                source.FactoryType == DryIoc.FactoryType.Decorator ? DryIocAttributes.FactoryType.Decorator :
                source.FactoryType == DryIoc.FactoryType.Wrapper ? DryIocAttributes.FactoryType.Wrapper :
                DryIocAttributes.FactoryType.Service;

            var ifUnresolved =
                source.IfUnresolved == DryIoc.IfUnresolved.Throw ? IfUnresolved.Throw : IfUnresolved.ReturnDefault;

            return ConvertRequestInfo(source.ParentOrWrapper).Push(
                source.ServiceType,
                source.RequiredServiceType,
                source.ServiceKey,
                source.MetadataKey, source.Metadata,
                ifUnresolved,
                source.FactoryID,
                factoryType,
                source.ImplementationType,
                source.ReuseLifespan);
        }

        /// <summary>Compares with another info for equality.</summary>
        /// <param name="obj">Other info to compare.</param> <returns>True if equal.</returns>
        public override bool Equals(object obj)
        {
            var other = obj as ExportedRegistrationInfo;
            return other != null
                && other.ImplementationType == ImplementationType
                && Equals(other.Reuse, Reuse)
                && other.FactoryType == FactoryType
                && Equals(other.Wrapper, Wrapper)
                && Equals(other.Decorator, Decorator)
                && other.Exports.SequenceEqual(Exports);
        }

        /// <summary>Generate valid c# code for instantiating of info from its state. Supposed be used in compile-time scenarios.</summary>
        /// <param name="code">Code to append "new RegistrationInfo(...)" to.</param>
        /// <returns>Code with "new info".</returns>
        public StringBuilder ToCode(StringBuilder code = null)
        {
            code = code ?? new StringBuilder();
            code.Append(@"
    new ExportedRegistrationInfo {
        ImplementationType = ").AppendType(ImplementationType).Append(@",
        Exports = new[] {
        "); for (var i = 0; i < Exports.Length; i++)
                code = Exports[i].ToCode(code.Append("    ")).Append(@",
        "); code.Append("}");
            if (Reuse != null) Reuse.ToCode(code.Append(@",
        Reuse = ")); code.Append(@",
        OpenResolutionScope = ").AppendBool(OpenResolutionScope).Append(@",
        AsResolutionCall = ").AppendBool(AsResolutionCall).Append(@",
        AsResolutionRoot = ").AppendBool(AsResolutionRoot).Append(@",
        PreventDisposal = ").AppendBool(PreventDisposal).Append(@",
        WeaklyReferenced = ").AppendBool(WeaklyReferenced).Append(@",
        AllowDisposableTransient = ").AppendBool(AllowDisposableTransient).Append(@",
        TrackDisposableTransient = ").AppendBool(TrackDisposableTransient).Append(@",
        UseParentReuse = ").AppendBool(UseParentReuse).Append(@",
        HasMetadataAttribute = ").AppendBool(HasMetadataAttribute).Append(@",
        FactoryType = ").AppendEnum(typeof(DryIoc.FactoryType), FactoryType).Append(@",
        ConditionType = ").AppendType(ConditionType);
            if (Metadata != null) code.Append(@",
        Metadata = ").AppendDictionary(Metadata, MetadataItemToCode);
            if (Wrapper != null) Wrapper.ToCode(code.Append(@",
        Wrapper = "));
            if (Decorator != null) Decorator.ToCode(code.Append(@",
        Decorator = "));
            if (FactoryMethodInfo != null) FactoryMethodInfo.ToCode(code.Append(@",
        FactoryMethodInfo = ")); code.Append(@"
    }");
            return code;
        }

        private StringBuilder MetadataItemToCode(StringBuilder code, string key, object value)
        {
            object metadataCode;
            if (Metadata != null &&
                Metadata.TryGetValue(key + ToCodeKeySuffix, out metadataCode))
                return code.Append(metadataCode);

            return code.AppendCode(value);
        }

        /// <summary>Collects the metadata as <see cref="Dictionary{TKey, TValue}"/>.</summary>
        /// <param name="attributes"></param>
        public void InitExportedMetadata(Attribute[] attributes)
        {
            Metadata = CollectExportedMetadata(attributes);
            if (Metadata != null)
                CollectAttributeConstructorsCode(Metadata);
        }

        /// <summary>Metadata key suffix for the C# representation of the custom attribute constructors.</summary>
        public const string ToCodeKeySuffix = ".ToCode()";

        private void CollectAttributeConstructorsCode(IDictionary<string, object> metadata)
        {
            var attributes = CustomAttributeData.GetCustomAttributes(ImplementationType)
                .Select(item => new
                {
                // ReSharper disable PossibleNullReferenceException
                // ReSharper disable AssignNullToNotNullAttribute
                Key = item.Constructor.DeclaringType.FullName,
                    Value = string.Format("new {0}({1})",
                        item.Constructor.DeclaringType.FullName,
                        string.Join(", ", item.ConstructorArguments.Map(a => a.ToString()).ToArrayOrSelf())) +
                        (item.NamedArguments.Any() ?
                            " { " + string.Join(", ", item.NamedArguments.Map(na => na.MemberInfo.Name + " = " + na.TypedValue).ToArrayOrSelf()) + " }" :
                            string.Empty)
                // ReSharper restore AssignNullToNotNullAttribute
                // ReSharper restore PossibleNullReferenceException
            })
                .OrderBy(item => item.Key);

            foreach (var attr in attributes)
                if (metadata.ContainsKey(attr.Key))
                    metadata[attr.Key + ToCodeKeySuffix] = attr.Value;
        }

        private static IDictionary<string, object> CollectExportedMetadata(IEnumerable<Attribute> attributes)
        {
            Dictionary<string, object> metaDict = null;

            var metadataAttributes = attributes
                .Where(a => a is ExportMetadataAttribute || a is WithMetadataAttribute
                    || a.GetType().GetAttributes(typeof(MetadataAttributeAttribute), true).Any())
                .OrderBy(a => a.GetType().FullName);

            foreach (var metaAttr in metadataAttributes)
            {
                string metaKey;
                object metaValue = metaAttr;
                var addProperties = false;

                if (metaAttr is ExportMetadataAttribute)
                {
                    var exportMetaAttr = (ExportMetadataAttribute)metaAttr;
                    metaKey = exportMetaAttr.Name; // note: defaults to string.Empty
                    metaValue = exportMetaAttr.Value;
                }
                else if (metaAttr is WithMetadataAttribute)
                {
                    var withMetadataAttr = (WithMetadataAttribute)metaAttr;
                    metaKey = withMetadataAttr.MetadataKey ?? Constants.ExportMetadataDefaultKey;
                    metaValue = withMetadataAttr.Metadata;
                }
                else
                {
                    // index custom metadata attributes with their type name
                    metaKey = metaAttr.GetType().FullName;
                    addProperties = true;
                }

                if (metaDict != null && metaDict.ContainsKey(metaKey))
                    Throw.It(Error.DuplicateMetadataKey, metaKey, metaDict);

                metaDict = metaDict ?? new Dictionary<string, object>();
                metaDict.Add(metaKey, metaValue);

                if (addProperties)
                {
                    var metaTypes = new List<TypeInfo>();
                    var metaType = metaAttr.GetType();
                    while (metaType != null && metaType != typeof(Attribute) && metaType != typeof(ExportAttribute))
                    {
                        var metaTypeInfo = metaType.GetTypeInfo();
                        metaTypes.Add(metaTypeInfo);
                        metaType = metaTypeInfo.BaseType;
                    }

                    var properties = metaTypes.SelectMany(t => t.DeclaredProperties);
                    foreach (var property in properties)
                    {
                        metaKey = property.Name;
                        metaValue = property.GetValue(metaAttr, new object[0]);

                        if (metaDict.ContainsKey(metaKey))
                            Throw.It(Error.DuplicateMetadataKey, metaKey, metaDict);

                        metaDict.Add(metaKey, metaValue);
                    }
                }
            }

            return metaDict;
        }
    }

    /// <summary>Serializable info about exported member, aka factory method in DryIoc.</summary>
    public sealed class FactoryMethodInfo
    {
        /// <summary>The type declaring the member.</summary>
        public Type DeclaringType;

        /// <summary>The declaring type name.</summary>
        public string DeclaringTypeFullName;

        /// <summary>Member defining the Export.</summary>
        public string MemberName;

        /// <summary>Parameter type full names (and names for generic parameters) to identify the method overload.</summary>
        public string[] MethodParameterTypeFullNamesOrNames;

        /// <summary>(optional) Not null for exported instance member which requires factory object, null for static members.</summary>
        public ExportInfo InstanceFactory;

        /// <summary>Indicate the lazy info with the type defined by its name instead of Runtime Type.</summary>
        public bool IsLazy { get { return DeclaringTypeFullName != null; } }

        /// <summary>Returns new export info with type representation as type full name string, instead of
        /// actual type.</summary> <returns>New lazy ExportInfo for not lazy this, otherwise - this one.</returns>
        public FactoryMethodInfo MakeLazy()
        {
            if (IsLazy) return this;
            var info = (FactoryMethodInfo)MemberwiseClone();
            info.DeclaringTypeFullName = DeclaringType.FullName;
            info.DeclaringType = null;
            if (info.InstanceFactory != null)
                info.InstanceFactory = info.InstanceFactory.MakeLazy();
            return info;
        }

        /// <summary>Constructs Made out of info properties.</summary>
        /// <returns>New made</returns>
        public Made CreateMade(Func<string, Type> typeProvider = null)
        {
            if (!IsLazy)
                return Made.Of(GetMember(DeclaringType),
                    InstanceFactory == null ? null : ServiceInfo.Of(
                        InstanceFactory.ServiceType, DryIoc.IfUnresolved.ReturnDefault, InstanceFactory.ServiceKey));

            typeProvider = typeProvider.ThrowIfNull();
            return Made.Of(_ => FactoryMethod.Of(
                GetMember(typeProvider(DeclaringTypeFullName)),
                InstanceFactory == null ? null : ServiceInfo.Of(
                    InstanceFactory.ServiceType ?? typeProvider(InstanceFactory.ServiceTypeFullName),
                    DryIoc.IfUnresolved.ReturnDefault, InstanceFactory.ServiceKey)));
        }

        private MemberInfo GetMember(Type declaringType)
        {
            return declaringType
                .GetAllMembers(includeBase: true)
                .FirstOrDefault(m =>
                {
                    if (m.Name != MemberName)
                        return false;

                    var method = m as MethodInfo;
                    if (method == null) // return early if it is property or field, no need to compare method signature
                        return true;

                    var parameters = method.GetParameters();
                    var factoryMethodParameterTypeNames = MethodParameterTypeFullNamesOrNames ?? ArrayTools.Empty<string>();
                    if (parameters.Length != factoryMethodParameterTypeNames.Length)
                        return false;

                    return parameters.Length == 0
                        || parameters.Select(p => p.ParameterType.FullName ?? p.ParameterType.Name)
                            .SequenceEqual(factoryMethodParameterTypeNames);
                })
                .ThrowIfNull();
        }

        /// <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />.</summary>
        /// <returns>true if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />; otherwise, false.</returns>
        /// <param name="obj">The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Object" />.</param>
        public override bool Equals(object obj)
        {
            var other = obj as FactoryMethodInfo;
            return other != null
                && other.DeclaringType == DeclaringType
                && other.MemberName == MemberName
                && (other.MethodParameterTypeFullNamesOrNames == null && MethodParameterTypeFullNamesOrNames == null ||
                    other.MethodParameterTypeFullNamesOrNames != null && MethodParameterTypeFullNamesOrNames != null &&
                    other.MethodParameterTypeFullNamesOrNames.SequenceEqual(MethodParameterTypeFullNamesOrNames))
                && Equals(other.InstanceFactory, InstanceFactory);
        }

        /// <summary>Generates valid c# code to re-create the info.</summary>
        /// <param name="code">Code to append generated code to.</param>
        /// <returns>Code with appended generated info.</returns>
        public StringBuilder ToCode(StringBuilder code)
        {
            code.Append(@"new FactoryMethodInfo {
            DeclaringType = ").AppendType(DeclaringType).Append(@",
            MemberName = ").AppendString(MemberName);
            if (!MethodParameterTypeFullNamesOrNames.IsNullOrEmpty()) code.Append(@",
            MethodParameterTypeFullNamesOrNames = ").AppendMany(MethodParameterTypeFullNamesOrNames);
            if (InstanceFactory != null) InstanceFactory.ToCode(code.Append(@",
            InstanceFactory = "));
            return code.Append(@"
        }");
        }
    }

    /// <summary>Specifies the standard and custom reuse info.</summary>
    public sealed class ReuseInfo
    {
        /// <summary>One of <see cref="AttributedModel.SupportedReuseTypes"/>.</summary>
        public ReuseType ReuseType;

        /// <summary>Name of the scope to pass to reuse factory from <see cref="AttributedModel.SupportedReuseTypes"/>.</summary>
        public string ScopeName;

        /// <summary>Custom reuse type, overrides the <see cref="ReuseType"/>.</summary>
        public Type CustomReuseType;

        /// <summary>Compares with another info for equality.</summary>
        /// <param name="obj">Other info to compare.</param> <returns>True if equal.</returns>
        public override bool Equals(object obj)
        {
            var other = obj as ReuseInfo;
            return other != null
                && other.ReuseType == ReuseType
                && other.ScopeName == ScopeName
                && other.CustomReuseType == CustomReuseType;
        }

        /// <summary>Converts info to the C# code representation.</summary>
        /// <param name="code">Code to append to.</param> <returns>Code with appended info.</returns>
        public StringBuilder ToCode(StringBuilder code)
        {
            code = CustomReuseType == null
                ? code.Append("new ReuseInfo { ReuseType = ").AppendEnum(typeof(ReuseType), ReuseType)
                : code.Append("new ReuseInfo { CustomReuseType = ").AppendType(CustomReuseType);

            if (ScopeName != null)
                code = code.Append(", ScopeName = ").AppendString(ScopeName);

            return code.Append(" }");
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

        /// <summary>Wrapped contract name or service key.</summary>
        public object ServiceKey;

        /// <summary>If already registered option to pass to container registration.</summary>
        public IfAlreadyRegistered IfAlreadyRegistered;

        /// <summary>Indicate the lazy info with type defined by its name instead of Runtime Type.</summary>
        public bool IsLazy { get { return ServiceTypeFullName != null; } }

        /// <summary>Default constructor is usually required by de-serializer.</summary>
        public ExportInfo() { }

        /// <summary>Creates exported info out of type and optional key.</summary>
        /// <param name="serviceType">Contract type to store.</param>
        /// <param name="serviceKey">(optional) ContractName string or service key.</param>
        /// <param name="ifAlreadyRegistered">(optional) Handles the case when the same export is already registered.</param>
        public ExportInfo(Type serviceType, object serviceKey = null,
            IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.AppendNotKeyed)
        {
            ServiceType = serviceType;
            ServiceKey = serviceKey;
            IfAlreadyRegistered = ifAlreadyRegistered;
        }

        /// <summary>Creates exported info out of type and optional key.</summary>
        /// <param name="serviceTypeFullName">Contract type name to store.</param>
        /// <param name="serviceKey">(optional) ContractName string or service key.</param>
        /// <param name="ifAlreadyRegistered">(optional) Handles the case when the same export is already registered.</param>
        public ExportInfo(string serviceTypeFullName, object serviceKey = null,
            IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.AppendNotKeyed)
        {
            ServiceTypeFullName = serviceTypeFullName;
            ServiceKey = serviceKey;
            IfAlreadyRegistered = ifAlreadyRegistered;
        }

        /// <summary>Compares with another info for equality.</summary>
        /// <param name="obj">Other info to compare.</param> <returns>True if equal.</returns>
        public override bool Equals(object obj)
        {
            var other = obj as ExportInfo;
            return other != null
                && other.ServiceType == ServiceType
                && Equals(other.ServiceKey, ServiceKey)
                && other.IfAlreadyRegistered == IfAlreadyRegistered;
        }

        /// <summary>Generates valid c# code to re-create the info.</summary>
        /// <param name="code">Code to append generated code to.</param>
        /// <returns>Code with appended generated info.</returns>
        public StringBuilder ToCode(StringBuilder code = null)
        {
            return (code ?? new StringBuilder())
                .Append("new ExportInfo(").AppendType(ServiceType).Append(", ")
                .AppendCode(ServiceKey).Append(", ")
                .AppendEnum(typeof(IfAlreadyRegistered), IfAlreadyRegistered)
                .Append(")");
        }

        /// <summary>Returns new export info with type representation as type full name string, instead of
        /// actual type.</summary> <returns>New lazy ExportInfo for not lazy this, otherwise - this one.</returns>
        public ExportInfo MakeLazy()
        {
            if (IsLazy) return this;
            var info = (ExportInfo)MemberwiseClone();
            info.ServiceTypeFullName = ServiceType.FullName;
            info.ServiceType = null;
            return info;
        }
    }

    /// <summary>Defines wrapper setup in serializable way.</summary>
    public sealed class WrapperInfo
    {
        /// <summary>Index of wrapped type argument in open-generic wrapper.</summary>
        public int WrappedServiceTypeArgIndex;

        /// <summary>Per name.</summary>
        public bool AlwaysWrapsRequiredServiceType;

        /// <summary>Creates Wrapper setup from this info.</summary> <returns>Setup.</returns>
        public Setup GetSetup()
        {
            return Setup.WrapperWith(WrappedServiceTypeArgIndex, AlwaysWrapsRequiredServiceType);
        }

        /// <summary>Used to compare wrappers info for equality.</summary> <param name="obj">Other info to compare.</param>
        /// <returns>True if equal</returns>
        public override bool Equals(object obj)
        {
            var other = obj as WrapperInfo;
            return other != null
                && other.WrappedServiceTypeArgIndex == WrappedServiceTypeArgIndex
                && other.AlwaysWrapsRequiredServiceType == AlwaysWrapsRequiredServiceType;
        }

        /// <summary>Converts info to valid C# code to be used in generation scenario.</summary>
        /// <param name="code">Code to append to.</param> <returns>Code with appended info code.</returns>
        public StringBuilder ToCode(StringBuilder code = null)
        {
            return (code ?? new StringBuilder())
                .Append("new WrapperInfo { WrappedServiceTypeArgIndex = ")
                .AppendCode(WrappedServiceTypeArgIndex).Append(", AlwaysWrapsRequiredServiceType = ")
                .AppendBool(AlwaysWrapsRequiredServiceType).Append(" }");
        }
    }

    /// <summary>Provides serializable info about Decorator setup.</summary>
    public sealed class DecoratorInfo
    {
        /// <summary>Decorated service key.</summary>
        public object DecoratedServiceKey;

        /// <summary>Controls the order that decorators are registered in the container when multiple decorators are used for a single type.</summary>
        public int Order;

        /// <summary>Instructs to use decorated service reuse. Decorated service may be decorator itself.</summary>
        public bool UseDecorateeReuse;

        /// <summary>Converts info to corresponding decorator setup.</summary>
        /// <param name="condition">(optional) <see cref="Setup.Condition"/>.</param>
        /// <returns>Decorator setup.</returns>
        public Setup GetSetup(Func<Request, bool> condition = null)
        {
            if (DecoratedServiceKey == null && condition == null && Order == 0 && !UseDecorateeReuse)
                return Setup.Decorator;

            return Setup.DecoratorWith(r =>
                (DecoratedServiceKey == null || Equals(DecoratedServiceKey, r.ServiceKey)) &&
                (condition == null || condition(r)),
                Order, UseDecorateeReuse);
        }

        /// <summary>Compares this info to other info for equality.</summary> <param name="obj">Other info to compare.</param>
        /// <returns>true if equal.</returns>
        public override bool Equals(object obj)
        {
            var other = obj as DecoratorInfo;
            return other != null && Equals(other.DecoratedServiceKey, DecoratedServiceKey);
        }

        /// <summary>Converts info to valid C# code to be used in generation scenario.</summary>
        /// <param name="code">Code to append to.</param> <returns>Code with appended info code.</returns>
        public StringBuilder ToCode(StringBuilder code)
        {
            return code.Append("new DecoratorInfo { DecoratedServiceKey = ")
                .AppendCode(DecoratedServiceKey).Append(", Order = ").AppendCode(Order)
                .Append(", UseDecorateeReuse = ").AppendBool(UseDecorateeReuse).Append(" }");
        }
    }
#pragma warning restore 659
    #endregion
}