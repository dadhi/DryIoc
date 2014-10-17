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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace DryIoc
{
    /// <summary>
    /// IoC Container. Documentation is available at https://bitbucket.org/dadhi/dryioc.
    /// </summary>
    public sealed partial class Container : IRegistry, IDisposable
    {
        public readonly Request Root;

        /// <summary>
        /// Creates new container, optionally providing <see cref="Rules"/> to modify default container behavior.
        /// </summary>
        /// <param name="rules">
        /// (optional) Rules to modify container default resolution behavior. 
        /// If not specified, then <see cref="DryIoc.Rules.Default"/> will be used.
        /// </param>
        public Container(Rules rules = null)
            : this(rules ?? Rules.Default,
            Interlocked.Increment(ref _lastRegistryID),
            Ref.Of(HashTree<Type, object>.Empty),
            Ref.Of(HashTree<Type, Factory[]>.Empty),
            Ref.Of(GenericsSupport.GenericWrappers),
            new Scope()) { }

        /// <summary>Creates new container instance with possibility to update default rules.</summary>
        /// <param name="updateRules"> Delegate gets <see cref="DryIoc.Rules.Default"/> as parameter and may return updated rules as result.</param>
        public Container(Func<Rules, Rules> updateRules)
            : this(updateRules.ThrowIfNull().Invoke(Rules.Default) ?? Rules.Default) { }

        /// <summary>Copies all of container state except Cache and specifies new rules.</summary>
        /// <param name="newRules">New rules. Its could be based on <see cref="DryIoc.Rules.Default"/> or copied container rules.</param>
        /// <returns></returns>
        public Container WithNewRules(Rules newRules)
        {
            ThrowIfContainerDisposed();
            return new Container(newRules, _registryID, _factories, _decorators, _genericWrappers, _singletonScope, _currentScope);
        }

        /// <example>
        /// <code lang="cs"><![CDATA[
        /// using (var scoped = container.OpenScope())
        /// {
        ///     var handler = scoped.Resolve<IHandler>();
        ///     handler.Handle(data);
        /// }
        /// ]]></code>
        /// </example>
        public Container OpenScope()
        {
            ThrowIfContainerDisposed();
            return new Container(Rules, _registryID, _factories, _decorators, _genericWrappers, _singletonScope, new Scope());
        }

        /// <summary>Synonym to <see cref="OpenScope"/>.</summary>
        /// <returns>Container with new current scope.</returns>
        public Container BeginScope()
        {
            return OpenScope();
        }

        public Container CreateChildContainer()
        {
            ThrowIfContainerDisposed();
            var parentRegistry = new WeakReference(this);
            return new Container(Rules.With(childRequest =>
            {
                var childRequestWithParentRegistry = childRequest.ReplaceRegistryWith(parentRegistry);
                var factory = childRequestWithParentRegistry.Registry.ResolveFactory(childRequestWithParentRegistry);
                return factory == null ? null : new ExpressionFactory(
                    request => factory.GetExpressionOrDefault(request.ReplaceRegistryWith(parentRegistry)));
            }));
        }

        /// <summary>
        /// Returns new container with all expression, delegate, items cache stripped off.
        /// It will preserve resolved services in Singleton/Current scope.
        /// </summary>
        /// <returns>New container without cache.</returns>
        public Container WipeCache()
        {
            ThrowIfContainerDisposed();
            return new Container(Rules, _registryID, _factories, _decorators, _genericWrappers, _singletonScope, _currentScope);
        }

        /// <summary>Disposes container current scope and that means container itself.</summary>
        public void Dispose()
        {
            _currentScope.Dispose();
            if (_currentScope != _singletonScope) 
                return; // Skip the rest for opened scope, to dispose only scoped container 

            // Cleanup the state
            _factories.Swap(_ => HashTree<Type, object>.Empty);
            _decorators.Swap(_ => HashTree<Type, Factory[]>.Empty);
            _genericWrappers.Swap(_ => HashTree<Type, Factory>.Empty);

            _resolvedDefaultDelegates = HashTree<Type, FactoryDelegate>.Empty;
            _resolvedKeyedDelegates = HashTree<Type, HashTree<object, FactoryDelegate>>.Empty;
            _resolutionState.Dispose();
            
            Rules = Rules.Empty;
        }

        /// <summary>Indicates if container is disposed.</summary>
        public bool IsDisposed
        {
            get { return _currentScope.IsDisposed; }
        }

        /// <summary>Prints container registry ID to identify it among others.</summary>
        /// <returns>Printed info.</returns>
        public override string ToString()
        {
            return "{RegistryID=" + _registryID + "}";
        }

        #region IRegistrator

        public void Register(Factory factory, Type serviceType, object serviceKey, IfAlreadyRegistered ifAlreadyRegistered)
        {
            ThrowIfContainerDisposed();
            factory.ThrowIfNull().ValidateBeforeRegistration(serviceType.ThrowIfNull(), this);
            switch (factory.Setup.Type)
            {
                case FactoryType.Decorator:
                    _decorators.Swap(x => x.AddOrUpdate(serviceType, new[] { factory }, ArrayTools.Append));
                    break;
                case FactoryType.GenericWrapper:
                    _genericWrappers.Swap(x => x.AddOrUpdate(serviceType, factory));
                    break;
                default:
                    AddOrUpdateServiceFactory(factory, serviceType, serviceKey, ifAlreadyRegistered);
                    break;
            }
        }

        public bool IsRegistered(Type serviceType, object serviceKey, FactoryType factoryType, Func<Factory, bool> condition)
        {
            ThrowIfContainerDisposed();
            serviceType = serviceType.ThrowIfNull();
            switch (factoryType)
            {
                case FactoryType.GenericWrapper:
                    serviceType.ThrowIf(!serviceType.IsGeneric(), Error.IS_REGISTERED_FOR_GENERIC_WRAPPER_CALLED_WITH_NONGENERIC_SERVICE_TYPE);
                    var wrapper = _genericWrappers.Value.GetValueOrDefault(serviceType.GetGenericDefinitionOrNull());
                    return wrapper != null && (condition == null || condition(wrapper));

                case FactoryType.Decorator:
                    var decorators = _decorators.Value.GetValueOrDefault(serviceType);
                    return decorators != null && (condition == null || decorators.Any(condition));

                default:
                    return GetServiceFactoryOrDefault(serviceType, serviceKey,
                        factories => factories.Select(x => x.Value).FirstOrDefault(condition ?? (factory => true)),
                        retryForOpenGeneric: true) != null;
            }
        }

        public void Unregister(Type serviceType, object serviceKey, FactoryType factoryType, Func<Factory, bool> condition)
        {
            ThrowIfContainerDisposed();
            switch (factoryType)
            {
                case FactoryType.GenericWrapper:
                    if (condition == null)
                        _genericWrappers.Swap(_ => _.RemoveOrUpdate(serviceType));
                    else
                        _genericWrappers.Swap(_ => _.RemoveOrUpdate(serviceType,
                            (Factory oldFactory, out Factory newFactory) =>
                            {
                                newFactory = oldFactory;
                                return !condition(oldFactory);
                            }));
                    break;
                case FactoryType.Decorator:
                    if (condition == null)
                        _decorators.Swap(_ => _.RemoveOrUpdate(serviceType));
                    else
                        _decorators.Swap(_ => _.RemoveOrUpdate(serviceType, (Factory[] oldFactories, out Factory[] newFactories) =>
                        {
                            newFactories = oldFactories.Where(factory => !condition(factory)).ToArray();
                            return newFactories.Length != 0;
                        }));
                    break;
                default:
                    if (serviceKey == null && condition == null)
                        _factories.Swap(_ => _.RemoveOrUpdate(serviceType));
                    else
                        _factories.Swap(_ => _.RemoveOrUpdate(serviceType, (object oldEntry, out object newEntry) =>
                        {
                            newEntry = oldEntry; // by default hold old entry

                            if (oldEntry is Factory) // return false to remove entry
                                return serviceKey != null && !DefaultKey.Default.Equals(serviceKey) ||
                                       condition != null && !condition((Factory)oldEntry);

                            var factoriesEntry = (FactoriesEntry)oldEntry;
                            var oldFactories = factoriesEntry.Factories;
                            var newFactories = oldFactories;
                            if (serviceKey == null)
                            {   // remove all factories for which condition is true
                                foreach (var factory in newFactories.Enumerate())
                                    if (condition == null || condition(factory.Value))
                                        newFactories = newFactories.RemoveOrUpdate(factory.Key);
                            }
                            else
                            {   // remove factory with specified key if its found and condition is true
                                var factory = newFactories.GetValueOrDefault(serviceKey);
                                if (factory != null && (condition == null || condition(factory)))
                                    newFactories = newFactories.RemoveOrUpdate(serviceKey);
                            }

                            if (newFactories != oldFactories) // if we deleted something then make a cleanup
                            {
                                if (newFactories.IsEmpty)
                                    return false; // if no more remaining factories, then delete the whole entry

                                if (newFactories.Height == 1 && newFactories.Key.Equals(DefaultKey.Default))
                                    newEntry = newFactories.Value; // replace entry with single remaining default factory
                                else
                                {   // update last default key if current default key was removed
                                    var newDefaultKey = factoriesEntry.LastDefaultKey;
                                    if (newDefaultKey != null && newFactories.GetValueOrDefault(newDefaultKey) == null)
                                        newDefaultKey = newFactories.Enumerate().Select(x => x.Key).OfType<DefaultKey>()
                                            .OrderByDescending(key => key.RegistrationOrder).FirstOrDefault();
                                    newEntry = new FactoriesEntry(newDefaultKey, newFactories);
                                }
                            }

                            return true;
                        }));
                    break;
            }
        }

        #endregion

        #region IResolver

        object IResolver.ResolveDefault(Type serviceType, IfUnresolved ifUnresolved, Request parentOrDefault)
        {
            var factoryDelegate = _resolvedDefaultDelegates.GetValueOrDefault(serviceType);
            return factoryDelegate != null
                ? factoryDelegate(_resolutionState.Items, null)
                : ResolveAndCacheDefaultDelegate(serviceType, ifUnresolved, parentOrDefault);
        }

        object IResolver.ResolveKeyed(Type serviceType, object serviceKey, IfUnresolved ifUnresolved, Type requiredServiceType,
            Request parentOrDefault)
        {
            var cacheServiceKey = serviceKey;
            if (requiredServiceType != null)
            {
                var wrappedServiceType = ((IRegistry)this).GetWrappedServiceType(serviceType).ThrowIfNotOf(requiredServiceType, 
                    Error.WRAPPED_TYPE_NOT_OF_REQUIRED_SERVICE_TYPE, requiredServiceType, serviceType);

                if (serviceType == wrappedServiceType)
                    serviceType = requiredServiceType;
                else
                    cacheServiceKey = serviceKey == null ? requiredServiceType
                        : (object)new KV<Type, object>(requiredServiceType, serviceKey);
            }

            // If service key is null, then use resolve default instead of keyed.
            if (cacheServiceKey == null)
                return ((IResolver)this).ResolveDefault(serviceType, ifUnresolved, parentOrDefault);

            ThrowIfContainerDisposed();

            FactoryDelegate factoryDelegate;

            var factoryDelegates = _resolvedKeyedDelegates.GetValueOrDefault(serviceType);
            if (factoryDelegates != null &&
                (factoryDelegate = factoryDelegates.GetValueOrDefault(cacheServiceKey)) != null)
                return factoryDelegate(_resolutionState.Items, null);

            var request = (parentOrDefault ?? Root).Push(serviceType, serviceKey, ifUnresolved, requiredServiceType);

            var factory = ((IRegistry)this).ResolveFactory(request);
            factoryDelegate = factory == null ? null : factory.GetDelegateOrDefault(request);
            if (factoryDelegate == null)
                return null;

            var resultService = factoryDelegate(request.State.Items, request.ResolutionScope);

            // Safe to cache factory only after it is evaluated without errors.
            factoryDelegates = factoryDelegates ?? HashTree<object, FactoryDelegate>.Empty;
            Interlocked.Exchange(ref _resolvedKeyedDelegates, _resolvedKeyedDelegates
                .AddOrUpdate(serviceType, factoryDelegates.AddOrUpdate(cacheServiceKey, factoryDelegate)));

            return resultService;
        }

        void IResolver.ResolvePropertiesAndFields(object instance, PropertiesAndFieldsSelector selectPropertiesAndFields,
            Request parentOrDefault)
        {
            var selector = selectPropertiesAndFields
                ?? Rules.PropertiesAndFields
                ?? PropertiesAndFields.AllPublicNonPrimitive;

            var instanceType = instance.ThrowIfNull().GetType();
            var request = (parentOrDefault ?? Root).Push(instanceType).ResolveTo(new InstanceFactory(instance));

            foreach (var serviceInfo in selector(instanceType, request))
                if (serviceInfo != null)
                {
                    var value = request.Resolve(serviceInfo);
                    if (value != null)
                        serviceInfo.SetValue(instance, value);
                }
        }

        private object ResolveAndCacheDefaultDelegate(Type serviceType, IfUnresolved ifUnresolved, Request parentOrDefault)
        {
            ThrowIfContainerDisposed();

            var request = (parentOrDefault ?? Root).Push(serviceType, ifUnresolved: ifUnresolved);

            var factory = ((IRegistry)this).ResolveFactory(request);
            var factoryDelegate = factory == null ? null : factory.GetDelegateOrDefault(request);
            if (factoryDelegate == null)
                return null;

            var resultService = factoryDelegate(request.State.Items, request.ResolutionScope);

            // Safe to cache factory only after it is evaluated without errors.
            Interlocked.Exchange(ref _resolvedDefaultDelegates, _resolvedDefaultDelegates
                .AddOrUpdate(serviceType, factoryDelegate));

            return resultService;
        }

        private void ThrowIfContainerDisposed()
        {
            this.ThrowIf(IsDisposed, Error.CONTAINER_IS_DISPOSED);
        }

        #endregion

        #region IRegistry

        int IRegistry.RegistryID { get { return _registryID; } }

        public Rules Rules { get; private set; }

        IScope IRegistry.SingletonScope { get { return _singletonScope; } }
        IScope IRegistry.CurrentScope { get { return _currentScope; } }

        Factory IRegistry.ResolveFactory(Request request)
        {
            var factory = GetServiceFactoryOrDefault(request.ServiceType, request.ServiceKey, Rules.FactorySelector);
            if (factory != null && factory.ProvidesFactoryForRequest)
                factory = factory.GetFactoryForRequestOrDefault(request);

            if (factory != null)
                return factory;

            var ruleFactory = Rules.ForUnregisteredService.GetFirstNonDefault(r => r(request));
            if (ruleFactory != null)
            {
                Register(ruleFactory, request.ServiceType, request.ServiceKey, IfAlreadyRegistered.ThrowIfDuplicateKey);
                return ruleFactory;
            }

            Throw.If(request.IfUnresolved == IfUnresolved.Throw, Error.UNABLE_TO_RESOLVE_SERVICE, request);
            return null;
        }

        Factory IRegistry.GetServiceFactoryOrDefault(Type serviceType, object serviceKey)
        {
            return GetServiceFactoryOrDefault(serviceType.ThrowIfNull(), serviceKey, Rules.FactorySelector,
                retryForOpenGeneric: true);
        }

        IEnumerable<KV<object, Factory>> IRegistry.GetAllServiceFactories(Type serviceType)
        {
            var entry = _factories.Value.GetValueOrDefault(serviceType);
            if (entry == null && serviceType.IsClosedGeneric())
                entry = _factories.Value.GetValueOrDefault(serviceType.GetGenericDefinitionOrNull());

            return entry == null ? Enumerable.Empty<KV<object, Factory>>()
                : entry is Factory ? new[] { new KV<object, Factory>(DefaultKey.Default, (Factory)entry) }
                : ((FactoriesEntry)entry).Factories.Enumerate();
        }

        Expression IRegistry.GetDecoratorExpressionOrDefault(Request request)
        {
            // Stop if no decorators registered.
            var decorators = _decorators.Value;
            if (decorators.IsEmpty)
                return null;

            // Decorators for non service types are not supported.
            if (request.ResolvedFactoryType != FactoryType.Service)
                return null;

            // We are already resolving decorator for the service, so stop now.
            var parent = request.GetNonWrapperParentOrRoot();
            if (!parent.IsRoot && parent.ResolvedFactoryType == FactoryType.Decorator)
                return null;

            LambdaExpression resultFuncExpr = null;

            var serviceType = request.ServiceType;

            var implementationType = request.ImplementationType ?? request.ServiceType;
            var implementedTypes = implementationType.GetImplementedTypes(
                TypeTools.IncludeFlags.SourceType | TypeTools.IncludeFlags.ObjectType);

            // Look for Action<ImplementedType> initializer-decorator
            for (var i = 0; i < implementedTypes.Length; i++)
            {
                var implementedType = implementedTypes[i];
                var initializerActionType = typeof(Action<>).MakeGenericType(implementedType);
                var initializerFactories = decorators.GetValueOrDefault(initializerActionType);
                if (initializerFactories != null)
                {
                    var doAction = _doMethod.MakeGenericMethod(implementedType, implementationType);
                    for (var j = 0; j < initializerFactories.Length; j++)
                    {
                        var initializerFactory = initializerFactories[j];
                        if (((DecoratorSetup)initializerFactory.Setup).Condition(request))
                        {
                            var decoratorRequest = request.ReplaceServiceInfoWith(ServiceInfo.Of(initializerActionType)).ResolveTo(initializerFactory);
                            var actionExpr = initializerFactory.GetExpressionOrDefault(decoratorRequest);
                            if (actionExpr != null)
                                ComposeDecoratorFuncExpression(ref resultFuncExpr, serviceType, Expression.Call(doAction, actionExpr));
                        }
                    }
                }
            }

            // Then look for decorators registered as Func of decorated service returning decorator - Func<TService, TService>.
            var decoratorFuncType = typeof(Func<,>).MakeGenericType(serviceType, serviceType);
            var funcDecoratorFactories = decorators.GetValueOrDefault(decoratorFuncType);
            if (funcDecoratorFactories != null)
            {
                for (var i = 0; i < funcDecoratorFactories.Length; i++)
                {
                    var decoratorFactory = funcDecoratorFactories[i];
                    var decoratorRequest = request.ReplaceServiceInfoWith(ServiceInfo.Of(decoratorFuncType)).ResolveTo(decoratorFactory);
                    if (((DecoratorSetup)decoratorFactory.Setup).Condition(request))
                    {
                        var funcExpr = decoratorFactory.GetExpressionOrDefault(decoratorRequest);
                        if (funcExpr != null)
                            ComposeDecoratorFuncExpression(ref resultFuncExpr, serviceType, funcExpr);
                    }
                }
            }

            // Next look for normal decorators.
            var serviceDecorators = decorators.GetValueOrDefault(serviceType);
            var openGenericDecoratorIndex = serviceDecorators == null ? 0 : serviceDecorators.Length;
            var openGenericServiceType = request.ServiceType.GetGenericDefinitionOrNull();
            if (openGenericServiceType != null)
                serviceDecorators = serviceDecorators.Append(decorators.GetValueOrDefault(openGenericServiceType));

            Expression resultDecorator = resultFuncExpr;
            if (serviceDecorators != null)
            {
                for (var i = 0; i < serviceDecorators.Length; i++)
                {
                    var decorator = serviceDecorators[i];
                    var decoratorRequest = request.ResolveTo(decorator);
                    if (((DecoratorSetup)decorator.Setup).Condition(request))
                    {
                        // Cache closed generic registration produced by open-generic decorator.
                        if (i >= openGenericDecoratorIndex && decorator.ProvidesFactoryForRequest)
                        {
                            decorator = decorator.GetFactoryForRequestOrDefault(request);
                            Register(decorator, serviceType, null, IfAlreadyRegistered.ThrowIfDuplicateKey);
                        }

                        var decoratorExpr = request.State.GetCachedFactoryExpressionOrDefault(decorator.FactoryID);
                        if (decoratorExpr == null)
                        {
                            IList<Type> unusedFunArgs;
                            var funcExpr = decorator
                                .GetFuncWithArgsOrDefault(decoratorFuncType, decoratorRequest, out unusedFunArgs)
                                .ThrowIfNull(Error.DECORATOR_FACTORY_SHOULD_SUPPORT_FUNC_RESOLUTION, decoratorFuncType);

                            decoratorExpr = unusedFunArgs != null ? funcExpr.Body : funcExpr;
                            request.State.CacheFactoryExpression(decorator.FactoryID, decoratorExpr);
                        }

                        if (resultDecorator == null || !(decoratorExpr is LambdaExpression))
                            resultDecorator = decoratorExpr;
                        else
                        {
                            if (!(resultDecorator is LambdaExpression))
                                resultDecorator = Expression.Invoke(decoratorExpr, resultDecorator);
                            else
                            {
                                var prevDecorators = ((LambdaExpression)resultDecorator);
                                var decorateDecorator = Expression.Invoke(decoratorExpr, prevDecorators.Body);
                                resultDecorator = Expression.Lambda(decorateDecorator, prevDecorators.Parameters[0]);
                            }
                        }
                    }
                }
            }

            return resultDecorator;
        }

        Factory IRegistry.GetGenericWrapperOrDefault(Type openGenericServiceType)
        {
            return _genericWrappers.Value.GetValueOrDefault(openGenericServiceType);
        }

        Type IRegistry.GetWrappedServiceType(Type serviceType)
        {
            var itemType = serviceType.GetElementTypeOrNull();
            if (itemType != null)
                return ((IRegistry)this).GetWrappedServiceType(itemType);

            var genericType = serviceType.GetGenericDefinitionOrNull();
            if (genericType != null)
            {
                var factory = _genericWrappers.Value.GetValueOrDefault(genericType);
                if (factory == null)
                    return serviceType;

                var setup = (GenericWrapperSetup)factory.Setup;
                var wrappedServiceType = setup.SelectWrappedServiceTypeArg(serviceType.GetGenericParamsAndArgs());
                return ((IRegistry)this).GetWrappedServiceType(wrappedServiceType); // unwrap further
            }

            return serviceType;
        }

        #endregion

        #region Decorators supporting cast

        private static void ComposeDecoratorFuncExpression(ref LambdaExpression result, Type serviceType, Expression decoratorFuncExpr)
        {
            if (result == null)
            {
                var decorated = Expression.Parameter(serviceType, "decorated");
                result = Expression.Lambda(Expression.Invoke(decoratorFuncExpr, decorated), decorated);
            }
            else
            {
                var decorateDecorator = Expression.Invoke(decoratorFuncExpr, result.Body);
                result = Expression.Lambda(decorateDecorator, result.Parameters[0]);
            }
        }

        private static readonly MethodInfo _doMethod = typeof(Container).GetDeclaredMethod("DoAction");

        internal static Func<T, R> DoAction<T, R>(Action<T> action) where R : T
        {
            return x => { action(x); return (R)x; };
        }

        #endregion

        #region Factories Add/Get

        private sealed class FactoriesEntry
        {
            public readonly DefaultKey LastDefaultKey;
            public readonly HashTree<object, Factory> Factories;

            public FactoriesEntry(DefaultKey lastDefaultKey, HashTree<object, Factory> factories)
            {
                LastDefaultKey = lastDefaultKey;
                Factories = factories;
            }
        }

        private void AddOrUpdateServiceFactory(Factory factory, Type serviceType, object serviceKey, IfAlreadyRegistered ifAlreadyRegistered)
        {
            if (serviceKey == null)
            {
                _factories.Swap(x => x.AddOrUpdate(serviceType, factory, (oldValue, _) =>
                {
                    if (oldValue is Factory) // adding new default to registered default
                    {
                        switch (ifAlreadyRegistered)
                        {
                            case IfAlreadyRegistered.KeepRegistered:
                                return oldValue;
                            case IfAlreadyRegistered.UpdateRegistered:
                                return factory;
                            default:
                                return new FactoriesEntry(DefaultKey.Default.Next(), HashTree<object, Factory>.Empty
                                    .AddOrUpdate(DefaultKey.Default, (Factory)oldValue)
                                    .AddOrUpdate(DefaultKey.Default.Next(), factory));
                        }
                    }

                    // otherwise, when already have some keyed factories registered.
                    var oldEntry = ((FactoriesEntry)oldValue);
                    if (oldEntry.LastDefaultKey == null) // there was not default registration, add the first one.
                        return new FactoriesEntry(DefaultKey.Default, oldEntry.Factories.AddOrUpdate(DefaultKey.Default, factory));

                    switch (ifAlreadyRegistered)
                    {
                        case IfAlreadyRegistered.KeepRegistered:
                            return oldValue;
                        case IfAlreadyRegistered.UpdateRegistered:
                            return new FactoriesEntry(oldEntry.LastDefaultKey, oldEntry.Factories.Update(oldEntry.LastDefaultKey, factory));
                        default: // just add another default factory
                            var newDefaultKey = oldEntry.LastDefaultKey.Next();
                            return new FactoriesEntry(newDefaultKey, oldEntry.Factories.AddOrUpdate(newDefaultKey, factory));
                    }
                }));
            }
            else // for non default service key
            {
                var newEntry = new FactoriesEntry(null, HashTree<object, Factory>.Empty.AddOrUpdate(serviceKey, factory));

                _factories.Swap(x => x.AddOrUpdate(serviceType, newEntry, (oldValue, _) =>
                {
                    if (oldValue is Factory) // if registered is default, just add it to new entry
                        return new FactoriesEntry(DefaultKey.Default, newEntry.Factories.AddOrUpdate(DefaultKey.Default, (Factory)oldValue));

                    var oldEntry = ((FactoriesEntry)oldValue);
                    return new FactoriesEntry(oldEntry.LastDefaultKey, oldEntry.Factories.AddOrUpdate(serviceKey, factory, (oldFactory, __) =>
                        ifAlreadyRegistered == IfAlreadyRegistered.KeepRegistered ? oldFactory
                        : ifAlreadyRegistered == IfAlreadyRegistered.UpdateRegistered ? factory
                        : Throw.No<Factory>(Error.REGISTERING_WITH_DUPLICATE_SERVICE_KEY, serviceType, serviceKey, oldFactory)));
                }));
            }
        }

        private Factory GetServiceFactoryOrDefault(Type serviceType, object serviceKey,
            Rules.FactorySelectorRule factorySelector, bool retryForOpenGeneric = false)
        {
            var entry = _factories.Value.GetValueOrDefault(serviceType);
            if (entry == null && retryForOpenGeneric && serviceType.IsClosedGeneric())
                entry = _factories.Value.GetValueOrDefault(serviceType.GetGenericDefinitionOrNull());

            if (entry != null)
            {
                if (entry is Factory)
                {
                    if (serviceKey != null && !DefaultKey.Default.Equals(serviceKey))
                        return null;
                    var factory = (Factory)entry;
                    return factorySelector == null ? factory
                        : factorySelector(new[] { new KeyValuePair<object, Factory>(DefaultKey.Default, factory) });
                }

                var factories = ((FactoriesEntry)entry).Factories;
                if (serviceKey != null)
                {
                    var factory = factories.GetValueOrDefault(serviceKey);
                    return factorySelector == null ? factory
                        : factorySelector(new[] { new KeyValuePair<object, Factory>(serviceKey, factory) });
                }

                var defaultFactories = factories.Enumerate().Where(x => x.Key is DefaultKey).ToArray();
                if (defaultFactories.Length != 0)
                    return factorySelector != null
                        ? factorySelector(defaultFactories.Select(kv => new KeyValuePair<object, Factory>(kv.Key, kv.Value)))
                        : defaultFactories.Length == 1 ? defaultFactories[0].Value
                        : Throw.No<Factory>(Error.EXPECTED_SINGLE_DEFAULT_FACTORY, serviceType, defaultFactories);
            }

            return null;
        }

        #endregion

        #region Implementation

        private readonly int _registryID;
        private static int _lastRegistryID;

        private readonly Ref<HashTree<Type, object>> _factories; // where object is Factory or KeyedFactoriesEntry
        private readonly Ref<HashTree<Type, Factory[]>> _decorators;
        private readonly Ref<HashTree<Type, Factory>> _genericWrappers;

        private readonly Scope _singletonScope, _currentScope;

        private HashTree<Type, FactoryDelegate> _resolvedDefaultDelegates;
        private HashTree<Type, HashTree<object, FactoryDelegate>> _resolvedKeyedDelegates;
        private readonly ResolutionState _resolutionState;

        private Container(
            Rules rules, int registryID,
            Ref<HashTree<Type, object>> factories,
            Ref<HashTree<Type, Factory[]>> decorators,
            Ref<HashTree<Type, Factory>> genericWrappers,
            Scope singletonScope, Scope currentScope = null)
        {
            Rules = rules;
            _registryID = registryID;

            _factories = factories;
            _decorators = decorators;
            _genericWrappers = genericWrappers;

            _singletonScope = singletonScope;
            _currentScope = currentScope ?? singletonScope;

            _resolvedDefaultDelegates = HashTree<Type, FactoryDelegate>.Empty;
            _resolvedKeyedDelegates = HashTree<Type, HashTree<object, FactoryDelegate>>.Empty;

            _resolutionState = new ResolutionState();

            Root = Request.CreateRoot(new WeakReference(this), new WeakReference(_resolutionState));
        }

        #endregion
    }

    public sealed class DefaultKey
    {
        public static readonly DefaultKey Default = new DefaultKey(0);

        public DefaultKey Next()
        {
            return Of(RegistrationOrder + 1);
        }

        public readonly int RegistrationOrder;

        public override bool Equals(object other)
        {
            return other is DefaultKey && ((DefaultKey)other).RegistrationOrder == RegistrationOrder;
        }

        public override int GetHashCode()
        {
            return RegistrationOrder;
        }

        public override string ToString()
        {
            return "DefaultKey#" + RegistrationOrder;
        }

        #region Implementation

        private static DefaultKey[] _keyPool = { Default };

        private DefaultKey(int registrationOrder)
        {
            RegistrationOrder = registrationOrder;
        }

        private static DefaultKey Of(int registrationOrder)
        {
            if (registrationOrder < _keyPool.Length)
                return _keyPool[registrationOrder];

            var nextKey = new DefaultKey(registrationOrder);
            if (registrationOrder == _keyPool.Length)
                _keyPool = _keyPool.AppendOrUpdate(nextKey);
            return nextKey;
        }

        #endregion
    }

    /// <summary>
    /// Holds service expression cache, and state items passed to <see cref="FactoryDelegate"/> in resolution root.
    /// </summary>
    public sealed class ResolutionState : IDisposable
    {
        public static readonly ParameterExpression ItemsParamExpr = Expression.Parameter(typeof(AppendableArray<object>), "items");

        public AppendableArray<object> Items
        {
            get { return _items; }
        }

        public int GetOrAddItem(object item)
        {
            var index = -1;
            Ref.Swap(ref _items, x =>
            {
                index = x.IndexOf(item);
                if (index == -1)
                    index = (x = x.Append(item)).Length - 1;
                return x;
            });
            return index;
        }

        public Expression GetItemExpression(int itemIndex, Type itemType)
        {
            var itemExpr = _itemsExpressions.GetFirstValueByHashOrDefault(itemIndex);
            if (itemExpr == null)
            {
                var indexExpr = Expression.Constant(itemIndex, typeof(int));
                itemExpr = Expression.Convert(Expression.Call(ItemsParamExpr, _getItemMethod, indexExpr), itemType);
                Interlocked.Exchange(ref _itemsExpressions, _itemsExpressions.AddOrUpdate(itemIndex, itemExpr));
            }
            return itemExpr;
        }

        public Expression GetOrAddItemExpression(object item, Type itemType)
        {
            return GetItemExpression(GetOrAddItem(item), itemType);
        }

        public Expression GetOrAddItemExpression<T>(T item)
        {
            return GetItemExpression(GetOrAddItem(item), typeof(T));
        }

        public Expression GetCachedFactoryExpressionOrDefault(int factoryID)
        {
            return _factoryExpressions.GetFirstValueByHashOrDefault(factoryID);
        }

        public void CacheFactoryExpression(int factoryID, Expression factoryExpression)
        {
            Interlocked.Exchange(ref _factoryExpressions, _factoryExpressions.AddOrUpdate(factoryID, factoryExpression));
        }

        /// <summary>Cleans the state.</summary>
        public void Dispose()
        {
            _items = AppendableArray<object>.Empty;
            _itemsExpressions = HashTree<int, Expression>.Empty;
            _factoryExpressions = HashTree<int, Expression>.Empty;
        }

        #region Implementation

        private static readonly MethodInfo _getItemMethod = typeof(AppendableArray<object>).GetDeclaredMethod("Get");

        private AppendableArray<object> _items = AppendableArray<object>.Empty;
        private HashTree<int, Expression> _itemsExpressions = HashTree<int, Expression>.Empty;
        private HashTree<int, Expression> _factoryExpressions = HashTree<int, Expression>.Empty;

        #endregion
    }

    public sealed class AppendableArray<T>
    {
        public static readonly AppendableArray<T> Empty = new AppendableArray<T>();

        public readonly int Length;

        public AppendableArray<T> Append(T value)
        {
            return new AppendableArray<T>(Length + 1,
                _tree.AddOrUpdate(Length >> NODE_ARRAY_BIT_COUNT, new[] { value }, ArrayTools.Append));
        }

        public int IndexOf(T value)
        {
            foreach (var node in _tree.Enumerate())
            {
                var indexInNode = node.Value.IndexOf(x => ReferenceEquals(x, value) || Equals(x, value));
                if (indexInNode != -1)
                    return node.Key << NODE_ARRAY_BIT_COUNT | indexInNode;
            }

            return -1;
        }

        /// <summary>Returns item stored at specified index.
        /// Method relies on underlying array for index range checking.</summary>
        /// <param name="index">Index to look for item.</param>
        /// <returns>Found item, or <see cref="ArgumentOutOfRangeException"/> exception otherwise.</returns>
        public T Get(int index)
        {
            return _treeHasSingleNode ? _tree.Value[index]
                : _tree.GetFirstValueByHashOrDefault(index >> NODE_ARRAY_BIT_COUNT)[index & NODE_ARRAY_BIT_MASK];
        }

        #region Implementation

        // Node array length is number of items stored per tree node. 
        // When the item added to same node, array will be copied. So if array is too long performance will degrade.
        // Should be power of two: e.g. 2, 4, 8, 16, 32...
        internal const int NODE_ARRAY_LENGTH = 32;

        private const int NODE_ARRAY_BIT_MASK = NODE_ARRAY_LENGTH - 1; // for length 32 will be 11111 binary.
        private const int NODE_ARRAY_BIT_COUNT = 5;                    // number of set bits in NODE_ARRAY_BIT_MASK.

        private readonly HashTree<int, T[]> _tree;
        private readonly bool _treeHasSingleNode;

        private AppendableArray() : this(0, HashTree<int, T[]>.Empty) { }

        private AppendableArray(int length, HashTree<int, T[]> tree)
        {
            Length = length;
            _tree = tree;
            _treeHasSingleNode = length <= NODE_ARRAY_LENGTH;
        }

        #endregion
    }

    public delegate object FactoryDelegate(AppendableArray<object> items, IScope scope);

    /// <summary>Handles default conversation of expression into <see cref="FactoryDelegate"/>.</summary>
    public static partial class FactoryCompiler
    {
        public static Expression<FactoryDelegate> ToFactoryExpression(this Expression expression)
        {
            // Removing not required Convert from expression root, because CompiledFactory result still be converted at the end.
            if (expression.NodeType == ExpressionType.Convert)
                expression = ((UnaryExpression)expression).Operand;
            if (expression.Type.IsValueType())
                expression = Expression.Convert(expression, typeof(object));
            return Expression.Lambda<FactoryDelegate>(expression, ResolutionState.ItemsParamExpr, Request.ScopeParamExpr);
        }

        public static FactoryDelegate CompileToDelegate(this Expression expression, Rules rules)
        {
            var factoryExpression = expression.ToFactoryExpression();
            FactoryDelegate factoryDelegate = null;
            CompileToMethod(factoryExpression, rules, ref factoryDelegate);
            // ReSharper disable ConstantNullCoalescingCondition
            factoryDelegate = factoryDelegate ?? factoryExpression.Compile();
            // ReSharper restore ConstantNullCoalescingCondition

            //System.Runtime.CompilerServices.RuntimeHelpers.PrepareMethod(factoryDelegate.Method.MethodHandle);
            return factoryDelegate;
        }

        // Partial method definition to be implemented in .NET40 version of Container.
        // It is optional and fine to be not implemented.
        static partial void CompileToMethod(Expression<FactoryDelegate> factoryExpression, Rules rules, ref FactoryDelegate result);
    }

    /// <summary>
    /// Adds to Container support for:
    /// <list type="bullet">
    /// <item>Open-generic services</item>
    /// <item>
    /// Service generics wrappers and arrays using <see cref="Rules.ForUnregisteredService"/> extension point.
    /// Supported wrappers include: Func of <see cref="FuncTypes"/>, Lazy, Many, IEnumerable, arrays, Meta, KeyValuePair, DebugExpression.
    /// All wrapper factories are added into collection <see cref="GenericWrappers"/> and searched by <see cref="ResolveGenericsAndArrays"/>
    /// unregistered resolution rule.
    /// </item>
    /// </list>
    /// </summary>
    public static class GenericsSupport
    {
        /// <summary>Supported Func types up to 4 input parameters.</summary>
        public static readonly Type[] FuncTypes = { typeof(Func<>), typeof(Func<,>), typeof(Func<,,>), typeof(Func<,,,>), typeof(Func<,,,,>) };

        public static readonly HashTree<Type, Factory> GenericWrappers;

        static GenericsSupport()
        {
            GenericWrappers = HashTree<Type, Factory>.Empty

            .AddOrUpdate(typeof(IEnumerable<>), // support for IEnumerable and arrays
                new FactoryProvider(_ => new ExpressionFactory(GetEnumerableOrArrayExpression), GenericWrapperSetup.Default))

            .AddOrUpdate(typeof(Many<>),
                new FactoryProvider(_ => new ExpressionFactory(GetManyExpression), GenericWrapperSetup.Default))

            .AddOrUpdate(typeof(Lazy<>),
                new ReflectionFactory(typeof(Lazy<>),
                    setup: GenericWrapperSetup.With((t, _) =>
                        t.GetConstructorWithParameters(new[] { typeof(Func<>).MakeGenericType(t.GetGenericParamsAndArgs()) }))))

            .AddOrUpdate(typeof(KeyValuePair<,>),
                new FactoryProvider(GetKeyValuePairFactoryOrDefault, GenericWrapperSetup.With(t => t[1])))

            .AddOrUpdate(typeof(Meta<,>),
                new FactoryProvider(GetMetaFactoryOrDefault, GenericWrapperSetup.With(t => t[0])))

            .AddOrUpdate(typeof(DebugExpression<>),
                new FactoryProvider(_ => new ExpressionFactory(GetDebugExpression), GenericWrapperSetup.Default));

            var funcFactory = new FactoryProvider(
                _ => new ExpressionFactory(GetFuncExpression),
                GenericWrapperSetup.With(t => t[t.Length - 1]));
            foreach (var funcType in FuncTypes)
                GenericWrappers = GenericWrappers.AddOrUpdate(funcType, funcFactory);
        }

        public static readonly Rules.ResolveUnregisteredServiceRule ResolveGenericsAndArrays = request =>
        {
            // When resolving array, use the same procedure as for IEnumerable 
            var serviceTypeGenericDef = request.ServiceType.IsArray
                ? typeof(IEnumerable<>)
                : request.ServiceType.GetGenericDefinitionOrNull();

            if (serviceTypeGenericDef == null)
                return null;

            var factory =
                request.Registry.GetServiceFactoryOrDefault(serviceTypeGenericDef, request.ServiceKey) ??
                request.Registry.GetGenericWrapperOrDefault(serviceTypeGenericDef);

            if (factory != null && factory.ProvidesFactoryForRequest)
                factory = factory.GetFactoryForRequestOrDefault(request);

            return factory;
        };

        public static Expression GetEnumerableOrArrayExpression(Request request)
        {
            var collectionType = request.ServiceType;
            var itemType = collectionType.GetElementTypeOrNull() ?? collectionType.GetGenericParamsAndArgs()[0];

            var registry = request.Registry;
            var wrappedItemType = registry.GetWrappedServiceType(itemType);
            var requiredItemType = request.ServiceInfo.Details.RequiredServiceType ?? wrappedItemType;

            var items = registry.GetAllServiceFactories(requiredItemType);

            // Composite pattern support: filter out composite root from available keys.
            var parent = request.GetNonWrapperParentOrRoot();
            if (!parent.IsRoot && parent.ServiceType == requiredItemType)
            {
                var parentFactoryID = parent.ResolvedFactory.FactoryID;
                items = items.Where(x => x.Value.FactoryID != parentFactoryID);
            }

            // Return collection of single matched item if key is specified.
            if (request.ServiceKey != null)
                items = items.Where(kv => request.ServiceKey.Equals(kv.Key));

            var itemArray = items.ToArray();
            List<Expression> itemExprList = null;
            if (itemArray.Length != 0)
            {
                itemExprList = new List<Expression>(itemArray.Length);
                for (var i = 0; i < itemArray.Length; i++)
                {
                    var item = itemArray[i];
                    var itemRequest = request.Push(itemType, item.Key, IfUnresolved.ReturnDefault);
                    var itemFactory = registry.ResolveFactory(itemRequest);
                    if (itemFactory != null)
                    {
                        var itemExpr = itemFactory.GetExpressionOrDefault(itemRequest);
                        if (itemExpr != null)
                            itemExprList.Add(itemExpr);
                    }
                }
            }

            return Expression.NewArrayInit(itemType.ThrowIfNull(), itemExprList ?? Enumerable.Empty<Expression>());
        }

        public static Expression GetManyExpression(Request request)
        {
            var manyType = request.ServiceType;
            var itemType = manyType.GetGenericParamsAndArgs()[0];

            var wrappedItemType = request.Registry.GetWrappedServiceType(itemType);
            var requiredItemType = request.ServiceInfo.Details.RequiredServiceType ?? wrappedItemType;

            // Composite pattern support: filter out composite root from available keys.
            var parentFactoryID = 0;
            var parent = request.GetNonWrapperParentOrRoot();
            if (!parent.IsRoot && parent.ServiceType == requiredItemType)
                parentFactoryID = parent.ResolvedFactory.FactoryID;

            var resolveMethod = _resolveManyDynamicallyMethod.MakeGenericMethod(itemType, requiredItemType);

            var requestExpr = request.State.GetOrAddItemExpression(request);
            var resolveCallExpr = Expression.Call(resolveMethod, requestExpr, Expression.Constant(parentFactoryID));

            return Expression.New(manyType.GetFirstConstructor(), resolveCallExpr);
        }

        public static Expression GetFuncExpression(Request request)
        {
            var funcType = request.ServiceType;
            var funcTypeArgs = funcType.GetGenericParamsAndArgs();
            var serviceType = funcTypeArgs[funcTypeArgs.Length - 1];
            var serviceRequest = request.Push(serviceType);
            var serviceFactory = request.Registry.ResolveFactory(serviceRequest);
            if (serviceFactory == null)
                return null;

            if (funcTypeArgs.Length == 1)
            {
                var expr = serviceFactory.GetExpressionOrDefault(serviceRequest);
                return expr == null ? null : Expression.Lambda(funcType, expr, null);
            }

            IList<Type> unusedFuncArgs;
            var funcExpr = serviceFactory.GetFuncWithArgsOrDefault(funcType, serviceRequest, out unusedFuncArgs);
            funcExpr.ThrowIfNull(Error.UNSUPPORTED_FUNC_WITH_ARGS, serviceType, request);
            unusedFuncArgs.ThrowIf(unusedFuncArgs != null, Error.SOME_FUNC_PARAMS_ARE_UNUSED, request);
            return funcExpr;
        }

        public static Expression GetDebugExpression(Request request)
        {
            var ctor = request.ServiceType.GetFirstConstructor();
            var serviceType = request.ServiceType.GetGenericParamsAndArgs()[0];
            var serviceRequest = request.Push(serviceType);
            var factory = request.Registry.ResolveFactory(serviceRequest);
            var expr = factory == null ? null : factory.GetExpressionOrDefault(serviceRequest);
            return expr == null ? null : Expression.New(ctor, request.State.GetOrAddItemExpression(expr.ToFactoryExpression()));
        }

        public static Factory GetKeyValuePairFactoryOrDefault(Request request)
        {
            var typeArgs = request.ServiceType.GetGenericParamsAndArgs();
            var serviceKeyType = typeArgs[0];
            var serviceKey = request.ServiceKey;
            if (serviceKey == null && serviceKeyType.IsValueType() ||
                serviceKey != null && !serviceKeyType.IsTypeOf(serviceKey))
                return null;

            var serviceType = typeArgs[1];
            return new ExpressionFactory(pairRequest =>
            {
                var serviceRequest = pairRequest.Push(serviceType, serviceKey);
                var serviceFactory = request.Registry.ResolveFactory(serviceRequest);
                var serviceExpr = serviceFactory == null ? null : serviceFactory.GetExpressionOrDefault(serviceRequest);
                if (serviceExpr == null)
                    return null;
                var pairCtor = pairRequest.ServiceType.GetFirstConstructor();
                var keyExpr = pairRequest.State.GetOrAddItemExpression(serviceKey, serviceKeyType);
                var pairExpr = Expression.New(pairCtor, keyExpr, serviceExpr);
                return pairExpr;
            });
        }

        public static Factory GetMetaFactoryOrDefault(Request request)
        {
            var typeArgs = request.ServiceType.GetGenericParamsAndArgs();
            var metadataType = typeArgs[1];
            var serviceType = typeArgs[0];

            var registry = request.Registry;
            var wrappedServiceType = registry.GetWrappedServiceType(serviceType);
            var requiredServiceType = request.ServiceInfo.Details.RequiredServiceType ?? wrappedServiceType;

            object resultMetadata = null;
            var serviceKey = request.ServiceKey;
            if (serviceKey == null)
            {
                var result = registry.GetAllServiceFactories(requiredServiceType).FirstOrDefault(kv =>
                    kv.Value.Setup.Metadata != null && metadataType.IsTypeOf(kv.Value.Setup.Metadata));
                if (result != null)
                {
                    serviceKey = result.Key;
                    resultMetadata = result.Value.Setup.Metadata;
                }
            }
            else
            {
                var factory = registry.GetServiceFactoryOrDefault(requiredServiceType, serviceKey);
                if (factory != null)
                {
                    var metadata = factory.Setup.Metadata;
                    resultMetadata = metadata != null && metadataType.IsTypeOf(metadata) ? metadata : null;
                }
            }

            if (resultMetadata == null)
                return null;

            return new ExpressionFactory(req =>
            {
                var serviceRequest = req.Push(serviceType, serviceKey);
                var serviceFactory = registry.ResolveFactory(serviceRequest);
                var serviceExpr = serviceFactory == null ? null : serviceFactory.GetExpressionOrDefault(serviceRequest);
                if (serviceExpr == null)
                    return null;
                var metaCtor = req.ServiceType.GetFirstConstructor();
                var metadataExpr = req.State.GetOrAddItemExpression(resultMetadata, metadataType);
                var metaExpr = Expression.New(metaCtor, serviceExpr, metadataExpr);
                return metaExpr;
            });
        }

        #region Tools

        public static bool IsFunc(this Request request)
        {
            var serviceTypeGenericDefinition = request.ServiceType.GetGenericDefinitionOrNull();
            return serviceTypeGenericDefinition != null && FuncTypes.Contains(serviceTypeGenericDefinition);
        }

        public static bool IsFuncWithArgs(this Request request)
        {
            return request.IsFunc() && request.ServiceType.GetGenericDefinitionOrNull() != typeof(Func<>);
        }

        #endregion

        #region Implementation

        private static readonly MethodInfo _resolveManyDynamicallyMethod =
            typeof(GenericsSupport).GetDeclaredMethod("ResolveManyDynamically");

        internal static IEnumerable<TItem> ResolveManyDynamically<TItem, TWrappedItem>(Request request, int parentFactoryID)
        {
            var itemType = typeof(TItem);
            var wrappedItemType = typeof(TWrappedItem);

            var items = request.Registry.GetAllServiceFactories(wrappedItemType);
            if (parentFactoryID != -1)
                items = items.Where(kv => kv.Value.FactoryID != parentFactoryID);

            // Return collection of single matched item if key is specified.
            if (request.ServiceKey != null)
                items = items.Where(kv => request.ServiceKey.Equals(kv.Key));

            foreach (var item in items)
            {
                var service = request.ResolveKeyed(itemType, item.Key, IfUnresolved.ReturnDefault, wrappedItemType, null);
                if (service != null) // skip unresolved items
                    yield return (TItem)service;
            }
        }

        #endregion
    }

    public sealed partial class Rules
    {
        public static readonly Rules Empty = new Rules();

        /// <summary>
        /// Default rules with support for generic wrappers: IEnumerable, Many, arrays, Func, Lazy, Meta, KeyValuePair, DebugExpression.
        /// Check <see cref="GenericsSupport.ResolveGenericsAndArrays"/> for details.
        /// </summary>
        public static readonly Rules Default = Empty.With(GenericsSupport.ResolveGenericsAndArrays);

        public ConstructorSelector Constructor { get { return _injectionRules.Constructor; } }
        public ParameterSelector Parameters { get { return _injectionRules.Parameters; } }
        public PropertiesAndFieldsSelector PropertiesAndFields { get { return _injectionRules.PropertiesAndFields; } }

        /// <summary>
        /// Returns new instance of the rules with specified <see cref="InjectionRules"/>.
        /// </summary>
        /// <returns>New rules with specified <see cref="InjectionRules"/>.</returns>
        public Rules With(
            ConstructorSelector constructor = null,
            ParameterSelector parameters = null,
            PropertiesAndFieldsSelector propertiesAndFields = null)
        {
            return new Rules(this)
            {
                _injectionRules = InjectionRules.With(
                    constructor ?? _injectionRules.Constructor,
                    parameters ?? _injectionRules.Parameters,
                    propertiesAndFields ?? _injectionRules.PropertiesAndFields)
            };
        }

        public delegate Factory FactorySelectorRule(IEnumerable<KeyValuePair<object, Factory>> factories);
        public FactorySelectorRule FactorySelector { get; private set; }
        public Rules With(FactorySelectorRule rule)
        {
            return new Rules(this) { FactorySelector = rule };
        }

        public delegate Factory ResolveUnregisteredServiceRule(Request request);
        public ResolveUnregisteredServiceRule[] ForUnregisteredService { get; private set; }
        public Rules With(params ResolveUnregisteredServiceRule[] rules)
        {
            return new Rules(this) { ForUnregisteredService = rules };
        }

        #region Implementation

        private InjectionRules _injectionRules;
        private bool _compilationToDynamicAssemblyEnabled; // NOTE: used by .NET 4 and higher versions.

        private Rules()
        {
            _injectionRules = InjectionRules.Empty;
        }

        private Rules(Rules copy)
        {
            FactorySelector = copy.FactorySelector;
            ForUnregisteredService = copy.ForUnregisteredService;
            _injectionRules = copy._injectionRules;
            _compilationToDynamicAssemblyEnabled = copy._compilationToDynamicAssemblyEnabled;
        }

        #endregion
    }

    /// <summary>
    /// Rules to dictate Container or registered implementation (<see cref="ReflectionFactory"/>) how to:
    /// <list type="bullet">
    /// <item>Select constructor for creating service with <see cref="Constructor"/>.</item>
    /// <item>Specify how to resolve constructor parameters with <see cref="Parameters"/>.</item>
    /// <item>Specify what properties/fields to resolve and how with <see cref="PropertiesAndFields"/>.</item>
    /// </list>
    /// </summary>
    public class InjectionRules
    {
        /// <summary>Empty means that no rules specified.</summary>
        public static readonly InjectionRules Empty = new InjectionRules();

        public static InjectionRules With(ConstructorSelector constructor = null,
            ParameterSelector parameters = null, PropertiesAndFieldsSelector propertiesAndFields = null)
        {
            return constructor == null && parameters == null && propertiesAndFields == null
                ? Empty : new InjectionRules(constructor, parameters, propertiesAndFields);
        }

        /// <summary>
        /// Sets rule how to select constructor with simplified signature without <see cref="Request"/> and <see cref="IRegistry"/> parameters. 
        /// </summary>
        /// <param name="constructor">Rule delegate taking implementation type as input and returning selected constructor info.</param>
        /// <returns>New instance of <see cref="InjectionRules"/> with <see cref="Constructor"/> set to specified delegate.</returns>
        public InjectionRules With(Func<Type, ConstructorInfo> constructor)
        {
            return constructor == null ? this
                : new InjectionRules((type, _) => constructor(type), Parameters, PropertiesAndFields);
        }

        public ConstructorSelector Constructor { get; private set; }

        /// <summary>
        /// Specifies how constructor parameters should be resolved: 
        /// parameter service key and type, throw or return default value if parameter is unresolved.
        /// </summary>
        public ParameterSelector Parameters { get; private set; }

        /// <summary>
        /// Specifies what <see cref="ServiceInfo"/> should be used when resolving property or field.
        /// </summary>
        public PropertiesAndFieldsSelector PropertiesAndFields { get; private set; }

        #region Implementation

        private InjectionRules() { }

        private InjectionRules(
            ConstructorSelector constructor = null,
            ParameterSelector parameters = null,
            PropertiesAndFieldsSelector propertiesAndFields = null)
        {
            Constructor = constructor;
            Parameters = parameters;
            PropertiesAndFields = propertiesAndFields;
        }

        #endregion
    }

    public static class Error
    {
        public static readonly string UNABLE_TO_RESOLVE_SERVICE =
            "Unable to resolve {0}." + Environment.NewLine +
            "Please register service OR adjust container resolution rules.";

        public static readonly string UNSUPPORTED_FUNC_WITH_ARGS =
            "Unsupported resolution of {0} as function with arguments: {1}.";

        public static readonly string EXPECTED_IMPL_TYPE_ASSIGNABLE_TO_SERVICE_TYPE =
            "Expecting implementation type {0} to be assignable to service type {1} but it is not.";

        public static readonly string UNABLE_TO_REGISTER_NON_FACTORY_PROVIDER_FOR_OPEN_GENERIC_SERVICE =
            "Unable to register not a factory provider for open-generic service {0}.";

        public static readonly string UNABLE_TO_REGISTER_OPEN_GENERIC_IMPL_WITH_NON_GENERIC_SERVICE =
            "Unable to register open-generic implementation {0} with non-generic service {1}.";

        public static readonly string UNABLE_TO_REGISTER_OPEN_GENERIC_IMPL_CAUSE_SERVICE_DOES_NOT_SPECIFY_ALL_TYPE_ARGS =
            "Unable to register open-generic implementation {0} because service {1} should specify all of its type arguments, but specifies only {2}.";

        public static readonly string USUPPORTED_REGISTRATION_OF_NON_GENERIC_IMPL_TYPE_DEFINITION_BUT_WITH_GENERIC_ARGS =
            "Unsupported registration of implementation {0} which is not a generic type definition but contains generic parameters." + Environment.NewLine +
            "Consider to register generic type definition {1} instead.";

        public static readonly string USUPPORTED_REGISTRATION_OF_NON_GENERIC_SERVICE_TYPE_DEFINITION_BUT_WITH_GENERIC_ARGS =
            "Unsupported registration of service {0} which is not a generic type definition but contains generic parameters." + Environment.NewLine +
            "Consider to register generic type definition {1} instead.";

        public static readonly string EXPECTED_SINGLE_DEFAULT_FACTORY =
            "Expecting single default registration of {0} but found many:" + Environment.NewLine + "{1}." + Environment.NewLine +
            "Please identify service with key, or metadata OR set Rules to return single registered factory.";

        public static readonly string EXPECTED_NON_ABSTRACT_IMPL_TYPE =
            "Expecting not abstract and not interface implementation type, but found {0}.";

        public static readonly string NO_PUBLIC_CONSTRUCTOR_DEFINED =
            "There is no public constructor defined for {0}.";

        public static readonly string UNSPECIFIED_HOWTO_SELECT_CONSTRUCTOR_FOR_IMPLTYPE =
            "Unspecified how to select single constructor for implementation type {0} with {1} public constructors.";

        public static readonly string CONSTRUCTOR_MISSES_SOME_PARAMETERS =
            "Constructor [{0}] of {1} misses some arguments required for {2} dependency.";

        public static readonly string UNABLE_TO_SELECT_CONSTRUCTOR =
            "Unable to select single constructor from {0} available in {1}." + Environment.NewLine +
            "Please provide constructor selector when registering service.";

        public static readonly string EXPECTED_FUNC_WITH_MULTIPLE_ARGS =
            "Expecting Func with one or more arguments but found {0}.";

        public static readonly string EXPECTED_CLOSED_GENERIC_SERVICE_TYPE =
            "Expecting closed-generic service type but found {0}.";

        public static readonly string RECURSIVE_DEPENDENCY_DETECTED =
            "Recursive dependency is detected when resolving" + Environment.NewLine + "{0}.";

        public static readonly string SCOPE_IS_DISPOSED =
            "Scope is disposed and scoped instances are no longer available.";

        public static readonly string CONTAINER_IS_GARBAGE_COLLECTED =
            "Container is no longer available (has been garbage-collected).";

        public static readonly string REGISTERING_WITH_DUPLICATE_SERVICE_KEY =
            "Service {0} with the same key \"{1}\" is already registered as {2}.";

        public static readonly string GENERIC_WRAPPER_EXPECTS_SINGLE_TYPE_ARG_BY_DEFAULT =
            "Generic Wrapper is working with single service type only, but found many:" + Environment.NewLine + "{0}." + Environment.NewLine +
            "Please specify service type selector in Generic Wrapper setup upon registration.";

        public static readonly string SOME_FUNC_PARAMS_ARE_UNUSED =
            "Found some unused Func parameters:" + Environment.NewLine + "{0}" + Environment.NewLine + "when resolving {1}.";

        public static readonly string DECORATOR_FACTORY_SHOULD_SUPPORT_FUNC_RESOLUTION =
            "Decorator factory should support resolution as {0}, but it does not.";

        public static readonly string UNABLE_TO_MATCH_IMPL_BASE_TYPES_WITH_SERVICE_TYPE =
            "Unable to match service with any of open-generic implementation {0} implemented types {1} when resolving {2}.";

        public static readonly string UNABLE_TO_FIND_OPEN_GENERIC_IMPL_TYPE_ARG_IN_SERVICE =
            "Unable to find for open-generic implementation {0} the type argument {1} when resolving {2}.";

        public static readonly string IS_REGISTERED_FOR_GENERIC_WRAPPER_CALLED_WITH_NONGENERIC_SERVICE_TYPE =
            "IsRegistered for GenericWrapper called with non generic service type {0}.";

        public static readonly string UNABLE_TO_SELECT_CTOR_USING_SELECTOR =
            "Unable to get constructor of {0} using provided constructor selector.";

        public static readonly string UNABLE_TO_FIND_CTOR_WITH_ALL_RESOLVABLE_ARGS =
            "Unable to find constructor with all resolvable parameters when resolving {0}.";

        public static readonly string UNABLE_TO_FIND_MATCHING_CTOR_FOR_FUNC_WITH_ARGS =
            "Unable to find constructor with all parameters matching Func signature {0} " + Environment.NewLine +
            "and the rest of parameters resolvable from Container when resolving: {1}.";

        public static readonly string REGED_FACTORY_DLG_RESULT_NOT_OF_SERVICE_TYPE =
            "Registered factory delegate returns service {0} is not assignable to serviceType {1}.";

        public static readonly string REGED_OBJ_NOT_ASSIGNABLE_TO_SERVICE_TYPE =
            "Registered instance {0} is not assignable to serviceType {1}.";

        public static readonly string WRAPPED_TYPE_NOT_OF_REQUIRED_SERVICE_TYPE =
            "Registered service (wrapped) type {0} is not assignable to required service type {1} when resolving {2}.";

        public static readonly string INJECTED_VALUE_IS_OF_DIFFERENT_TYPE =
            "Injected value {0} is not assignable to {1} when resolving: {2}.";

        public static readonly string UNABLE_TO_FIND_SPECIFIED_WRITEABLE_PROPERTY_OR_FIELD =
            "Unable to find writable property or field \"{0}\" when resolving: {1}.";

        public static readonly string UNABLE_TO_REGISTER_ALL_FOR_ANY_IMPLEMENTED_TYPE =
            "Unable to register any of implementation {0} implemented services {1}.";

        public static readonly string PUSHING_TO_REQUEST_WITHOUT_FACTORY =
            "Pushing next info {0} to request not yet resolved to factory: {1}";

        public static readonly string TARGET_WAS_ALREADY_DISPOSED = 
            "Target of type {0} was already disposed in {1}.";

        public static readonly string CONTAINER_IS_DISPOSED = 
            "Container {0} is disposed and its operations are no longer available.";
    }

    public static class Registrator
    {
        /// <summary>
        /// Registers service of <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="serviceType">The service type to register</param>
        /// <param name="factory"><see cref="Factory"/> details object.</param>
        /// <param name="named">(optional) service key (name). Could be of any type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="ifAlreadyRegistered">(optional) policy to deal with case when service with such type and name is already registered.</param>
        public static void Register(this IRegistrator registrator, Type serviceType, Factory factory,
            object named = null, IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.ThrowIfDuplicateKey)
        {
            registrator.Register(factory, serviceType, named, ifAlreadyRegistered);
        }

        /// <summary>
        /// Registers service of <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of service.</typeparam>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="factory"><see cref="Factory"/> details object.</param>
        /// <param name="named">(optional) service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="ifAlreadyRegistered">(optional) policy to deal with case when service with such type and name is already registered.</param>
        public static void Register<TService>(this IRegistrator registrator, Factory factory,
            object named = null, IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.ThrowIfDuplicateKey)
        {
            registrator.Register(factory, typeof(TService), named, ifAlreadyRegistered);
        }

        /// <summary>
        /// Registers service <paramref name="serviceType"/> with corresponding <paramref name="implementationType"/>.
        /// </summary>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="serviceType">The service type to register.</param>
        /// <param name="implementationType">Implementation type. Concrete and open-generic class are supported.</param>
        /// <param name="reuse">
        /// (optional) <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. 
        /// Default value means no reuse, aka Transient.</param>
        /// <param name="withConstructor">(optional) strategy to select constructor when multiple available.</param>
        /// <param name="setup">(optional) Factory setup, by default is (<see cref="Setup"/>)</param>
        /// <param name="named">(optional) Service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="ifAlreadyRegistered">(optional) Policy to deal with case when service with such type and name is already registered.</param>
        public static void Register(this IRegistrator registrator, Type serviceType, Type implementationType,
            IReuse reuse = null, Func<Type, ConstructorInfo> withConstructor = null, FactorySetup setup = null,
            object named = null, IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.ThrowIfDuplicateKey)
        {
            setup = (setup ?? Setup.Default).WithConstructor(withConstructor);
            var factory = new ReflectionFactory(implementationType, reuse, setup);
            registrator.Register(factory, serviceType, named, ifAlreadyRegistered);
        }

        /// <summary>
        /// Registers service of <paramref name="implementationAndServiceType"/>. ServiceType will be the same as <paramref name="implementationAndServiceType"/>.
        /// </summary>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="implementationAndServiceType">Implementation type. Concrete and open-generic class are supported.</param>
        /// <param name="reuse">(optional) <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="withConstructor">(optional) strategy to select constructor when multiple available.</param>
        /// <param name="setup">(optional) factory setup, by default is (<see cref="Setup"/>)</param>
        /// <param name="named">(optional) service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="ifAlreadyRegistered">(optional) policy to deal with case when service with such type and name is already registered.</param>
        public static void Register(this IRegistrator registrator,
            Type implementationAndServiceType, IReuse reuse = null, Func<Type, ConstructorInfo> withConstructor = null,
            FactorySetup setup = null,
            object named = null, IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.ThrowIfDuplicateKey)
        {
            setup = (setup ?? Setup.Default).WithConstructor(withConstructor);
            var factory = new ReflectionFactory(implementationAndServiceType, reuse, setup);
            registrator.Register(factory, implementationAndServiceType, named, ifAlreadyRegistered);
        }

        /// <summary>
        /// Registers service of <typeparamref name="TService"/> type implemented by <typeparamref name="TImplementation"/> type.
        /// </summary>
        /// <typeparam name="TService">The type of service.</typeparam>
        /// <typeparam name="TImplementation">The type of service.</typeparam>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="reuse">(optional) <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="withConstructor">(optional) strategy to select constructor when multiple available.</param>
        /// <param name="setup">(optional) factory setup, by default is (<see cref="Setup"/>)</param>
        /// <param name="named">(optional) service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="ifAlreadyRegistered">(optional) policy to deal with case when service with such type and name is already registered.</param>
        public static void Register<TService, TImplementation>(this IRegistrator registrator,
            IReuse reuse = null, Func<Type, ConstructorInfo> withConstructor = null,
            FactorySetup setup = null,
            object named = null, IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.ThrowIfDuplicateKey)
            where TImplementation : TService
        {
            setup = (setup ?? Setup.Default).WithConstructor(withConstructor);
            var factory = new ReflectionFactory(typeof(TImplementation), reuse, setup);
            registrator.Register(factory, typeof(TService), named, ifAlreadyRegistered);
        }

        /// <summary>
        /// Registers implementation type <typeparamref name="TServiceAndImplementation"/> with itself as service type.
        /// </summary>
        /// <typeparam name="TServiceAndImplementation">The type of service.</typeparam>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="reuse">(optional) <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="withConstructor">(optional) strategy to select constructor when multiple available.</param>
        /// <param name="setup">(optional) Factory setup, by default is (<see cref="Setup"/>)</param>
        /// <param name="named">(optional) Service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="ifAlreadyRegistered">(optional) Policy to deal with case when service with such type and name is already registered.</param>
        public static void Register<TServiceAndImplementation>(this IRegistrator registrator,
            IReuse reuse = null, Func<Type, ConstructorInfo> withConstructor = null, FactorySetup setup = null,
            object named = null, IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.ThrowIfDuplicateKey)
        {
            setup = (setup ?? Setup.Default).WithConstructor(withConstructor);
            var factory = new ReflectionFactory(typeof(TServiceAndImplementation), reuse, setup);
            registrator.Register(factory, typeof(TServiceAndImplementation), named, ifAlreadyRegistered);
        }

        /// <summary>
        /// Returns true if type is public and not an object type. 
        /// Provides default setting for <see cref="RegisterAll"/> "types" parameter.
        /// </summary>
        /// <param name="type">Type to check.</param>
        /// <returns>True for matched type, false otherwise.</returns>
        public static bool DefaultImplementedTypesForRegisterAll(Type type)
        {
            return type.IsPublicOrNestedPublic() && type != typeof(object);
        }

        /// <summary>
        /// Registers single registration for all implemented public interfaces and base classes.
        /// </summary>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="implementationType">Service implementation type. Concrete and open-generic class are supported.</param>
        /// <param name="reuse">(optional) <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="withConstructor">(optional) strategy to select constructor when multiple available.</param>
        /// <param name="setup">(optional) factory setup, by default is (<see cref="Setup"/>)</param>
        /// <param name="types">(optional) condition to include selected types only. Default value is <see cref="DefaultImplementedTypesForRegisterAll"/></param>
        /// <param name="named">(optional) service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="ifAlreadyRegistered">(optional) policy to deal with case when service with such type and name is already registered.</param>
        public static void RegisterAll(this IRegistrator registrator, Type implementationType,
            IReuse reuse = null, Func<Type, ConstructorInfo> withConstructor = null,
            FactorySetup setup = null, Func<Type, bool> types = null,
            object named = null, IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.ThrowIfDuplicateKey)
        {
            setup = (setup ?? Setup.Default).WithConstructor(withConstructor);
            var factory = new ReflectionFactory(implementationType, reuse, setup);

            var implementedTypes = implementationType.GetImplementedTypes(TypeTools.IncludeFlags.SourceType);
            var implementedServiceTypes = implementedTypes.Where(types ?? DefaultImplementedTypesForRegisterAll);
            if (implementationType.IsGenericDefinition())
            {
                var implTypeArgs = implementationType.GetGenericParamsAndArgs();
                implementedServiceTypes = implementedServiceTypes
                    .Where(t => t.ContainsAllGenericParameters(implTypeArgs))
                    .Select(t => t.GetGenericDefinitionOrNull());
            }

            var atLeastOneRegistered = false;
            foreach (var serviceType in implementedServiceTypes)
            {
                registrator.Register(factory, serviceType, named, ifAlreadyRegistered);
                atLeastOneRegistered = true;
            }

            Throw.If(!atLeastOneRegistered, Error.UNABLE_TO_REGISTER_ALL_FOR_ANY_IMPLEMENTED_TYPE, implementationType, implementedTypes);
        }

        /// <summary>
        /// Registers single registration for all implemented public interfaces and base classes.
        /// </summary>
        /// <typeparam name="TImplementation">The type of service.</typeparam>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="reuse">(optional) <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="withConstructor">(optional) strategy to select constructor when multiple available.</param>
        /// <param name="setup">(optional) factory setup, by default is (<see cref="Setup"/>)</param>
        /// <param name="types">(optional) condition to include selected types only. Default value is <see cref="DefaultImplementedTypesForRegisterAll"/></param>
        /// <param name="named">(optional) service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="ifAlreadyRegistered">(optional) policy to deal with case when service with such type and name is already registered.</param>
        public static void RegisterAll<TImplementation>(this IRegistrator registrator,
            IReuse reuse = null, Func<Type, ConstructorInfo> withConstructor = null,
            FactorySetup setup = null, Func<Type, bool> types = null,
            object named = null, IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.ThrowIfDuplicateKey)
        {
            registrator.RegisterAll(typeof(TImplementation),
                reuse, withConstructor, setup, types, named, ifAlreadyRegistered);
        }

        /// <summary>
        /// Registers a factory delegate for creating an instance of <typeparamref name="TService"/>.
        /// Delegate can use <see cref="IResolver"/> parameter to resolve any required dependencies, e.g.:
        /// <code>RegisterDelegate&lt;ICar&gt;(r => new Car(r.Resolve&lt;IEngine&gt;()))</code>
        /// </summary>
        /// <typeparam name="TService">The type of service.</typeparam>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="factoryDelegate">The delegate used to create a instance of <typeparamref name="TService"/>.</param>
        /// <param name="reuse">(optional) <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="setup">(optional) factory setup, by default is (<see cref="Setup"/>)</param>
        /// <param name="named">(optional) service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="ifAlreadyRegistered">(optional) policy to deal with case when service with such type and name is already registered.</param>
        public static void RegisterDelegate<TService>(this IRegistrator registrator,
            Func<Request, TService> factoryDelegate, IReuse reuse = null, FactorySetup setup = null,
            object named = null, IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.ThrowIfDuplicateKey)
        {
            var factory = new DelegateFactory(r => factoryDelegate(r), reuse, setup);
            registrator.Register(factory, typeof(TService), named, ifAlreadyRegistered);
        }

        /// <summary>
        /// Registers a factory delegate for creating an instance of <paramref name="serviceType"/>.
        /// Delegate can use <see cref="IResolver"/> parameter to resolve any required dependencies, e.g.:
        /// <code>RegisterDelegate&lt;ICar&gt;(r => new Car(r.Resolve&lt;IEngine&gt;()))</code>
        /// </summary>
        /// <param name="serviceType">Service type to register.</param>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="factoryDelegate">The delegate used to create a instance of <paramref name="serviceType"/>.</param>
        /// <param name="reuse">(optional) <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="setup">(optional) factory setup, by default is (<see cref="Setup"/>)</param>
        /// <param name="named">(optional) service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="ifAlreadyRegistered">(optional) policy to deal with case when service with such type and name is already registered.</param>
        public static void RegisterDelegate(this IRegistrator registrator, Type serviceType,
            Func<Request, object> factoryDelegate, IReuse reuse = null, FactorySetup setup = null,
            object named = null, IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.ThrowIfDuplicateKey)
        {
            Func<Request, object> checkedFactory = r => factoryDelegate(r)
                .ThrowIfNotOf(serviceType, Error.REGED_FACTORY_DLG_RESULT_NOT_OF_SERVICE_TYPE, serviceType);

            var factory = new DelegateFactory(checkedFactory, reuse, setup);
            registrator.Register(factory, serviceType, named, ifAlreadyRegistered);
        }

        /// <summary>
        /// Registers a pre-created object of <typeparamref name="TService"/>.
        /// It is just a sugar on top of <see cref="RegisterDelegate{TService}"/> method.
        /// </summary>
        /// <typeparam name="TService">The type of service.</typeparam>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="instance">The pre-created instance of <typeparamref name="TService"/>.</param>
        /// <param name="reuse">(optional) <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="setup">(optional) factory setup, by default is (<see cref="Setup"/>)</param>
        /// <param name="named">(optional) service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="ifAlreadyRegistered">(optional) policy to deal with case when service with such type and name is already registered.</param>
        public static void RegisterInstance<TService>(this IRegistrator registrator, TService instance, IReuse reuse = null,
            FactorySetup setup = null, object named = null, IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.ThrowIfDuplicateKey)
        {
            var factory = reuse == null ? (Factory)new InstanceFactory(instance, setup) : new DelegateFactory(_ => instance, reuse, setup);
            registrator.Register(factory, typeof(TService), named, ifAlreadyRegistered);
        }

        /// <summary>
        /// Registers a pre-created object assignable to <paramref name="serviceType"/>. 
        /// </summary>
        /// <param name="registrator">Any <see cref="IRegistrator"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="serviceType">Service type to register.</param>
        /// <param name="instance">The pre-created instance of <paramref name="serviceType"/>.</param>
        /// <param name="reuse">(optional) <see cref="IReuse"/> implementation, e.g. <see cref="Reuse.Singleton"/>. Default value means no reuse, aka Transient.</param>
        /// <param name="setup">(optional) factory setup, by default is (<see cref="Setup"/>)</param>
        /// <param name="named">(optional) service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="ifAlreadyRegistered">(optional) policy to deal with case when service with such type and name is already registered.</param>
        public static void RegisterInstance(this IRegistrator registrator, Type serviceType, object instance, IReuse reuse = null, 
            FactorySetup setup = null, object named = null, IfAlreadyRegistered ifAlreadyRegistered = IfAlreadyRegistered.ThrowIfDuplicateKey)
        {
            var factory = reuse == null ? (Factory)new InstanceFactory(instance, setup) : new DelegateFactory(_ => instance, reuse, setup);
            registrator.Register(factory, serviceType, named, ifAlreadyRegistered);
        }

        /// <summary>
        /// Registers initializing action that will be called after service is resolved just before returning it to caller.
        /// Check example below for using initializer to automatically subscribe to singleton event aggregator.
        /// You can register multiple initializers for single service. 
        /// Or you can register initializer for <see cref="Object"/> type to be applied for all services and use <see cref="condition"/> 
        /// to filter target services.
        /// </summary>
        /// <remarks>Initializer internally implemented as decorator registered as Action delegate, so all decorators behavior is applied.</remarks>
        /// <typeparam name="TTarget">Any type implemented by requested service type including service type itself and object type.</typeparam>
        /// <param name="registrator">Usually is <see cref="Container"/> object.</param>
        /// <param name="initialize">Delegate with <typeparamref name="TTarget"/> object and 
        /// <see cref="IResolver"/> to resolve additional services required by initializer.</param>
        /// <param name="condition">(optional) Condition to select required target.</param>
        /// <example>
        /// <code lang="cs"><![CDATA[
        ///     container.Register<EventAggregator>(Reuse.Singleton);
        ///     container.Register<ISubscriber, SomeSubscriber>();
        /// 
        ///     // Registers initializer for all subscribers implementing ISubscriber.
        ///     container.RegisterInitiliazer<ISubscriber>((s, r) => r.Resolve<EventAggregator>().Subscribe(s));
        /// ]]></code>
        /// </example>
        public static void RegisterInitializer<TTarget>(this IRegistrator registrator,
            Action<TTarget, Request> initialize, Func<Request, bool> condition = null)
        {
            registrator.RegisterDelegate<Action<TTarget>>(r => target => initialize(target, r), setup: DecoratorSetup.WithCondition(condition));
        }

        /// <summary>
        /// Returns true if <paramref name="serviceType"/> is registered in container or its open generic definition is registered in container.
        /// </summary>
        /// <param name="registrator">Usually <see cref="Container"/> to explore or any other <see cref="IRegistrator"/> implementation.</param>
        /// <param name="serviceType">The type of the registered service.</param>
        /// <param name="named">(optional) service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="factoryType">(optional) factory type to lookup, <see cref="FactoryType.Service"/> by default.</param>
        /// <param name="condition">(optional) condition to specify what registered factory do you expect.</param>
        /// <returns>True if <paramref name="serviceType"/> is registered, false - otherwise.</returns>
        public static bool IsRegistered(this IRegistrator registrator, Type serviceType,
            object named = null, FactoryType factoryType = FactoryType.Service, Func<Factory, bool> condition = null)
        {
            return registrator.IsRegistered(serviceType, named, factoryType, condition);
        }

        /// <summary>
        /// Returns true if <typeparamref name="TService"/> type is registered in container or its open generic definition is registered in container.
        /// </summary>
        /// <typeparam name="TService">The type of service.</typeparam>
        /// <param name="registrator">Usually <see cref="Container"/> to explore or any other <see cref="IRegistrator"/> implementation.</param>
        /// <param name="named">(optional) service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="factoryType">(optional) factory type to lookup, <see cref="FactoryType.Service"/> by default.</param>
        /// <param name="condition">(optional) condition to specify what registered factory do you expect.</param>
        /// <returns>True if <typeparamref name="TService"/> name="serviceType"/> is registered, false - otherwise.</returns>
        public static bool IsRegistered<TService>(this IRegistrator registrator,
            object named = null, FactoryType factoryType = FactoryType.Service, Func<Factory, bool> condition = null)
        {
            return registrator.IsRegistered(typeof(TService), named, factoryType, condition);
        }

        /// <summary> Removes specified registration from container.</summary>
        /// <param name="registrator">Usually <see cref="Container"/> to explore or any other <see cref="IRegistrator"/> implementation.</param>
        /// <param name="serviceType">Type of service to remove.</param>
        /// <param name="named">(optional) service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="factoryType">(optional) factory type to lookup, <see cref="FactoryType.Service"/> by default.</param>
        /// <param name="condition">(optional) condition for Factory to be removed.</param>
        public static void Unregister(this IRegistrator registrator, Type serviceType,
            object named = null, FactoryType factoryType = FactoryType.Service, Func<Factory, bool> condition = null)
        {
            registrator.Unregister(serviceType, named, factoryType, condition);
        }

        /// <summary> Removes specified registration from container.</summary>
        /// <typeparam name="TService">The type of service to remove.</typeparam>
        /// <param name="registrator">Usually <see cref="Container"/> to explore or any other <see cref="IRegistrator"/> implementation.</param>
        /// <param name="named">(optional) service key (name). Could be of any of type with overridden <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>.</param>
        /// <param name="factoryType">(optional) factory type to lookup, <see cref="FactoryType.Service"/> by default.</param>
        /// <param name="condition">(optional) condition for Factory to be removed.</param>
        public static void Unregister<TService>(this IRegistrator registrator,
            object named = null, FactoryType factoryType = FactoryType.Service, Func<Factory, bool> condition = null)
        {
            registrator.Unregister(typeof(TService), named, factoryType, condition);
        }
    }

    public static class Resolver
    {
        /// <summary>
        /// Returns instance of <typepsaramref name="TService"/> type.
        /// </summary>
        /// <param name="serviceType">The type of the requested service.</param>
        /// <param name="resolver">Any <see cref="IResolver"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="ifUnresolved">(optional) Says how to handle unresolved service.</param>
        /// <returns>The requested service instance.</returns>
        public static object Resolve(this IResolver resolver, Type serviceType, IfUnresolved ifUnresolved = IfUnresolved.Throw)
        {
            return resolver.ResolveDefault(serviceType, ifUnresolved, null);
        }

        /// <summary>
        /// Returns instance of <typepsaramref name="TService"/> type.
        /// </summary>
        /// <typeparam name="TService">The type of the requested service.</typeparam>
        /// <param name="resolver">Any <see cref="IResolver"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="ifUnresolved">(optional) Says how to handle unresolved service.</param>
        /// <returns>The requested service instance.</returns>
        public static TService Resolve<TService>(this IResolver resolver, IfUnresolved ifUnresolved = IfUnresolved.Throw)
        {
            return (TService)resolver.ResolveDefault(typeof(TService), ifUnresolved, null);
        }

        /// <summary>
        /// Returns instance of <typeparamref name="TService"/> searching for <paramref name="requiredServiceType"/>.
        /// In case of <typeparamref name="TService"/> being generic wrapper like Func, Lazy, IEnumerable, etc., <paramref name="requiredServiceType"/>
        /// could specify wrapped service type.
        /// </summary>
        /// <typeparam name="TService">The type of the requested service.</typeparam>
        /// <param name="resolver">Any <see cref="IResolver"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="requiredServiceType">(optional) Service or wrapped type assignable to <typeparamref name="TService"/>.</param>
        /// <param name="ifUnresolved">(optional) Says how to handle unresolved service.</param>
        /// <returns>The requested service instance.</returns>
        /// <remarks>Using <paramref name="requiredServiceType"/> implicitly support Covariance for generic wrappers even in .Net 3.5.</remarks>
        /// <example>
        /// <code lang="cs"><![CDATA[
        ///     container.Register<IService, Service>();
        ///     var services = container.Resolve<IEnumerable<object>>(typeof(IService));
        /// ]]></code>
        /// </example>
        public static TService Resolve<TService>(this IResolver resolver, Type requiredServiceType, IfUnresolved ifUnresolved = IfUnresolved.Throw)
        {
            return (TService)resolver.ResolveKeyed(typeof(TService), null, ifUnresolved, requiredServiceType, null);
        }

        /// <summary>
        /// Returns instance of <paramref name="serviceType"/> searching for <paramref name="requiredServiceType"/>.
        /// In case of <paramref name="serviceType"/> being generic wrapper like Func, Lazy, IEnumerable, etc., <paramref name="requiredServiceType"/>
        /// could specify wrapped service type.
        /// </summary>
        /// <param name="serviceType">The type of the requested service.</param>
        /// <param name="resolver">Any <see cref="IResolver"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="serviceKey">Service key (any type with <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/> defined).</param>
        /// <param name="ifUnresolved">(optional) Says how to handle unresolved service.</param>
        /// <param name="requiredServiceType">(optional) Service or wrapped type assignable to <paramref name="serviceType"/>.</param>
        /// <returns>The requested service instance.</returns>
        /// <remarks>Using <paramref name="requiredServiceType"/> implicitly support Covariance for generic wrappers even in .Net 3.5.</remarks>
        /// <example>
        /// <code lang="cs"><![CDATA[
        ///     container.Register<IService, Service>();
        ///     var services = container.Resolve(typeof(Lazy<object>), "named", requiredServiceType: typeof(IService));
        /// ]]></code>
        /// </example>
        public static object Resolve(this IResolver resolver, Type serviceType, object serviceKey,
            IfUnresolved ifUnresolved = IfUnresolved.Throw, Type requiredServiceType = null)
        {
            return serviceKey == null
                ? resolver.ResolveDefault(serviceType, ifUnresolved, null)
                : resolver.ResolveKeyed(serviceType, serviceKey, ifUnresolved, requiredServiceType, null);
        }

        /// <summary>
        /// Returns instance of <typepsaramref name="TService"/> type.
        /// </summary>
        /// <typeparam name="TService">The type of the requested service.</typeparam>
        /// <param name="resolver">Any <see cref="IResolver"/> implementation, e.g. <see cref="Container"/>.</param>
        /// <param name="serviceKey">Service key (any type with <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/> defined).</param>
        /// <param name="ifUnresolved">(optional) Says how to handle unresolved service.</param>
        /// <param name="requiredServiceType">(optional) Service or wrapped type assignable to <typeparamref name="TService"/>.</param>
        /// <returns>The requested service instance.</returns>
        /// <remarks>Using <paramref name="requiredServiceType"/> implicitly support Covariance for generic wrappers even in .Net 3.5.</remarks>
        public static TService Resolve<TService>(this IResolver resolver, object serviceKey,
            IfUnresolved ifUnresolved = IfUnresolved.Throw, Type requiredServiceType = null)
        {
            return (TService)resolver.Resolve(typeof(TService), serviceKey, ifUnresolved, requiredServiceType);
        }

        /// <summary>Specifies new registered services awareness. Either dynamic or fixed view./// </summary>
        public enum ManyResult { DynamicIsBitSlower, FixedIsBitFaster }

        /// <summary>
        /// Returns all registered services instances including all keyed and default registrations.
        /// Use <paramref name="result"/> to return either all registered services at the moment of resolve (dynamic fresh view) or
        /// the same services that were returned with first <see cref="ResolveMany{TService}"/> call (fixed view).
        /// </summary>
        /// <typeparam name="TService">Return collection item type. It denotes registered service type if <paramref name="requiredServiceType"/> is not specified.</typeparam>
        /// <param name="resolver">Usually <see cref="Container"/> object.</param>
        /// <param name="requiredServiceType">(optional) Denotes registered service type. Should be assignable to <typeparamref name="TService"/>.</param>
        /// <param name="result">(optional) Specifies new registered services awareness. Aware by default.</param>
        /// <returns>Result collection of services.</returns>
        /// <remarks>The same result could be achieved by directly calling:
        /// <code lang="cs"><![CDATA[
        ///     container.Resolve<Many<IService>>();        // for dynamic result - default behavior
        ///     container.Resolve<IEnumerable<IService>>(); // for fixed result
        ///     container.Resolve<IService[]>();            // for fixed result too
        /// ]]></code>
        /// </remarks>
        public static IEnumerable<TService> ResolveMany<TService>(this IResolver resolver,
            Type requiredServiceType = null, ManyResult result = ManyResult.DynamicIsBitSlower)
        {
            return result == ManyResult.DynamicIsBitSlower
                ? resolver.Resolve<Many<TService>>(requiredServiceType).Items
                : resolver.Resolve<IEnumerable<TService>>(requiredServiceType);
        }

        /// <summary>
        /// For given instance resolves and sets properties and fields.
        /// It respects <see cref="DryIoc.Rules.PropertiesAndFields"/> rules set per container, 
        /// or if rules are not set it uses <see cref="PropertiesAndFields.AllPublicNonPrimitive"/>, 
        /// or you can specify your own rules with <paramref name="selectPropertiesAndFields"/> parameter.
        /// </summary>
        /// <typeparam name="TService">Input and returned instance type.</typeparam>
        /// <param name="resolver">Usually a container instance, cause <see cref="Container"/> implements <see cref="IResolver"/></param>
        /// <param name="instance">Service instance with properties to resolve and initialize.</param>
        /// <param name="selectPropertiesAndFields">(optional) Function to select properties and fields, overrides all other rules if specified.</param>
        /// <returns>Input instance with resolved dependencies, to enable fluent method composition.</returns>
        /// <remarks>Different Rules could be combined together using <see cref="PropertiesAndFields.OverrideWith"/> method.</remarks>        
        public static TService ResolvePropertiesAndFields<TService>(this IResolver resolver, TService instance,
            PropertiesAndFieldsSelector selectPropertiesAndFields = null)
        {
            resolver.ResolvePropertiesAndFields(instance, selectPropertiesAndFields, null);
            return instance;
        }
    }

    public interface IServiceInfo
    {
        Type ServiceType { get; }
        ServiceInfoDetails Details { get; }
        IServiceInfo Create(Type serviceType, ServiceInfoDetails details);
    }

    public class ServiceInfoDetails
    {
        public static readonly ServiceInfoDetails IfUnresolvedThrow = new ServiceInfoDetails();
        public static readonly ServiceInfoDetails IfUnresolvedReturnDefault = new IfUnresolvedReturnNull();

        public static ServiceInfoDetails Of(Type requiredServiceType = null,
            object serviceKey = null, IfUnresolved ifUnresolved = IfUnresolved.Throw)
        {
            return ifUnresolved == IfUnresolved.Throw
                ? (requiredServiceType == null
                    ? (serviceKey == null ? IfUnresolvedThrow : new WithKey(serviceKey))
                    : new WithType(requiredServiceType, serviceKey))
                : (requiredServiceType == null
                    ? (serviceKey == null ? IfUnresolvedReturnDefault : new WithKeyReturnDefault(serviceKey))
                    : new WithTypeReturnDefault(requiredServiceType, serviceKey));
        }

        public static ServiceInfoDetails Of(Func<Request, object> getValue)
        {
            return new WithValue(getValue.ThrowIfNull());
        }

        /// <summary>Service type to search in registry. Should be assignable to user requested service type.</summary>
        public virtual Type RequiredServiceType { get { return null; } }

        /// <summary>Service key provided with registration.</summary>
        public virtual object ServiceKey { get { return null; } }

        /// <summary>Policy to deal with unresolved request.</summary>
        public virtual IfUnresolved IfUnresolved { get { return IfUnresolved.Throw; } }

        /// <summary>Allows to get, or resolve value using passed <see cref="Request"/>.</summary>
        public virtual Func<Request, object> GetValue { get { return null; } }

        public override string ToString()
        {
            if (GetValue != null)
                return "{with custom value}";

            var s = new StringBuilder();
            if (RequiredServiceType != null)
                s.Append("{required: ").Print(RequiredServiceType);
            if (ServiceKey != null)
                (s.Length == 0 ? s.Append('{') : s.Append(", ")).Print(ServiceKey, "\"");
            if (IfUnresolved != IfUnresolved.Throw)
                (s.Length == 0 ? s.Append('{') : s.Append(", ")).Append("allow default");
            return (s.Length == 0 ? s : s.Append('}')).ToString();
        }

        private sealed class IfUnresolvedReturnNull : ServiceInfoDetails
        {
            public override IfUnresolved IfUnresolved { get { return IfUnresolved.ReturnDefault; } }
        }

        private class WithValue : ServiceInfoDetails
        {
            public override Func<Request, object> GetValue { get { return _getValue; } }
            public WithValue(Func<Request, object> getValue) { _getValue = getValue; }
            private readonly Func<Request, object> _getValue;
        }

        private class WithKey : ServiceInfoDetails
        {
            public override object ServiceKey { get { return _serviceKey; } }
            public WithKey(object serviceKey) { _serviceKey = serviceKey; }
            private readonly object _serviceKey;
        }

        private sealed class WithKeyReturnDefault : WithKey
        {
            public override IfUnresolved IfUnresolved { get { return IfUnresolved.ReturnDefault; } }
            public WithKeyReturnDefault(object serviceKey) : base(serviceKey) { }
        }

        private class WithType : WithKey
        {
            public override Type RequiredServiceType { get { return _requiredServiceType; } }
            public WithType(Type requiredServiceType, object serviceKey) : base(serviceKey) { _requiredServiceType = requiredServiceType; }
            private readonly Type _requiredServiceType;
        }

        private sealed class WithTypeReturnDefault : WithType
        {
            public override IfUnresolved IfUnresolved { get { return IfUnresolved.ReturnDefault; } }
            public WithTypeReturnDefault(Type requiredServiceType, object serviceKey) : base(requiredServiceType, serviceKey) { }
        }
    }

    public static class ServiceInfoTools
    {
        public static T With<T>(this T info, ServiceInfoDetails details, Request request)
            where T : IServiceInfo
        {
            if (details == null || details == info.Details)
                return info;

            var serviceType = info.ServiceType;
            var requiredServiceType = details.RequiredServiceType;
            if (requiredServiceType != null)
            {
                var wrappedServiceType = request.Registry.GetWrappedServiceType(serviceType).ThrowIfNotOf(requiredServiceType, 
                    Error.WRAPPED_TYPE_NOT_OF_REQUIRED_SERVICE_TYPE, requiredServiceType, request);
                if (wrappedServiceType == serviceType)
                {
                    serviceType = requiredServiceType;
                    details = ServiceInfoDetails.Of(null, details.ServiceKey, details.IfUnresolved);
                }
            }

            return (T)info.Create(serviceType, details);
        }

        public static IServiceInfo InheritDependencyFromOwnerInfo(this IServiceInfo dependency, IServiceInfo owner, FactoryType ownerFactoryType)
        {
            var ownerDetails = owner.Details;
            if (ownerDetails == null || ownerDetails == ServiceInfoDetails.IfUnresolvedThrow)
                return dependency;

            var dependencyDetails = dependency.Details;

            var ifUnresolved = ownerDetails.IfUnresolved == IfUnresolved.Throw
                ? dependencyDetails.IfUnresolved
                : ownerDetails.IfUnresolved;

            // Use dependency key if it's non default, otherwise and if owner is not service, the
            var serviceKey = dependencyDetails.ServiceKey == null && ownerFactoryType != FactoryType.Service
                ? ownerDetails.ServiceKey
                : dependencyDetails.ServiceKey;

            var serviceType = dependency.ServiceType;
            var requiredServiceType = dependencyDetails.RequiredServiceType;
            if (ownerDetails.RequiredServiceType != null)
            {
                requiredServiceType = null;
                if (ownerDetails.RequiredServiceType.IsAssignableTo(serviceType))
                    serviceType = ownerDetails.RequiredServiceType;
                else
                    requiredServiceType = ownerDetails.RequiredServiceType;
            }

            if (serviceType == dependency.ServiceType && serviceKey == dependencyDetails.ServiceKey &&
                ifUnresolved == dependencyDetails.IfUnresolved && requiredServiceType == dependencyDetails.RequiredServiceType)
                return dependency;

            return dependency.Create(serviceType, ServiceInfoDetails.Of(requiredServiceType, serviceKey, ifUnresolved));
        }

        public static StringBuilder Print(this StringBuilder s, IServiceInfo info)
        {
            s.Print(info.ServiceType);
            var details = info.Details.ToString();
            return details == string.Empty ? s : s.Append(' ').Append(details);
        }
    }

    public class ServiceInfo : IServiceInfo
    {
        public static ServiceInfo Of(Type serviceType,
            object serviceKey = null, IfUnresolved ifUnresolved = IfUnresolved.Throw)
        {
            serviceType.ThrowIfNull().ThrowIf(serviceType.IsOpenGeneric(), Error.EXPECTED_CLOSED_GENERIC_SERVICE_TYPE);
            return serviceKey == null && ifUnresolved == IfUnresolved.Throw
                ? new ServiceInfo(serviceType)
                : new WithDetails(serviceType, ServiceInfoDetails.Of(null, serviceKey, ifUnresolved));
        }

        public Type ServiceType { get; private set; }
        public virtual ServiceInfoDetails Details { get { return ServiceInfoDetails.IfUnresolvedThrow; } }

        public IServiceInfo Create(Type serviceType, ServiceInfoDetails details)
        {
            return details == ServiceInfoDetails.IfUnresolvedThrow
                ? new ServiceInfo(serviceType)
                : new WithDetails(serviceType, details);
        }

        public override string ToString()
        {
            return new StringBuilder().Print(this).ToString();
        }

        #region Implementation

        private ServiceInfo(Type serviceType)
        {
            ServiceType = serviceType;
        }

        private sealed class WithDetails : ServiceInfo
        {
            public override ServiceInfoDetails Details
            {
                get { return _details; }
            }

            public WithDetails(Type serviceType, ServiceInfoDetails details)
                : base(serviceType)
            {
                _details = details;
            }

            private readonly ServiceInfoDetails _details;
        }

        #endregion
    }

    public class ParameterServiceInfo : IServiceInfo
    {
        public static ParameterServiceInfo Of(ParameterInfo parameter)
        {
            return new ParameterServiceInfo(parameter.ThrowIfNull());
        }

        public virtual Type ServiceType { get { return _parameter.ParameterType; } }
        public virtual ServiceInfoDetails Details { get { return ServiceInfoDetails.IfUnresolvedThrow; } }

        public IServiceInfo Create(Type serviceType, ServiceInfoDetails details)
        {
            return serviceType == ServiceType
                ? new WithDetails(_parameter, details)
                : new TypeWithDetails(_parameter, serviceType, details);
        }

        public override string ToString()
        {
            return new StringBuilder().Print(this).Append(" as parameter ").Print(_parameter.Name, "\"").ToString();
        }

        private readonly ParameterInfo _parameter;
        public ParameterServiceInfo(ParameterInfo parameter) { _parameter = parameter; }

        private class WithDetails : ParameterServiceInfo
        {
            public override ServiceInfoDetails Details { get { return _details; } }
            public WithDetails(ParameterInfo parameter, ServiceInfoDetails details)
                : base(parameter) { _details = details; }
            private readonly ServiceInfoDetails _details;
        }

        private sealed class TypeWithDetails : WithDetails
        {
            public override Type ServiceType { get { return _serviceType; } }
            public TypeWithDetails(ParameterInfo parameter, Type serviceType, ServiceInfoDetails details)
                : base(parameter, details) { _serviceType = serviceType; }
            private readonly Type _serviceType;
        }
    }

    public abstract class PropertyOrFieldServiceInfo : IServiceInfo
    {
        public static PropertyOrFieldServiceInfo Of(MemberInfo member)
        {
            return member.ThrowIfNull() is PropertyInfo
                ? (PropertyOrFieldServiceInfo)new Property((PropertyInfo)member)
                : new Field((FieldInfo)member);
        }

        public static PropertyOrFieldServiceInfo Of(PropertyInfo property)
        {
            return new Property(property.ThrowIfNull());
        }

        public static PropertyOrFieldServiceInfo Of(FieldInfo field)
        {
            return new Field(field.ThrowIfNull());
        }

        public abstract Type ServiceType { get; }
        public virtual ServiceInfoDetails Details { get { return ServiceInfoDetails.IfUnresolvedReturnDefault; } }
        public abstract IServiceInfo Create(Type serviceType, ServiceInfoDetails details);

        public abstract MemberInfo Member { get; }
        public abstract void SetValue(object holder, object value);

        private class Property : PropertyOrFieldServiceInfo
        {
            public override Type ServiceType { get { return _property.PropertyType; } }
            public override IServiceInfo Create(Type serviceType, ServiceInfoDetails details)
            {
                return serviceType == ServiceType
                    ? new WithDetails(_property, details)
                    : new TypeWithDetails(_property, serviceType, details);
            }

            public override MemberInfo Member { get { return _property; } }
            public override void SetValue(object holder, object value)
            {
                _property.SetValue(holder, value, null);
            }

            public override string ToString()
            {
                return new StringBuilder().Print(this).Append(" as property ").Print(_property.Name, "\"").ToString();
            }

            private readonly PropertyInfo _property;
            public Property(PropertyInfo property)
            {
                _property = property;
            }

            private class WithDetails : Property
            {
                public override ServiceInfoDetails Details { get { return _details; } }
                public WithDetails(PropertyInfo property, ServiceInfoDetails details)
                    : base(property) { _details = details; }
                private readonly ServiceInfoDetails _details;
            }

            private sealed class TypeWithDetails : WithDetails
            {
                public override Type ServiceType { get { return _serviceType; } }
                public TypeWithDetails(PropertyInfo property, Type serviceType, ServiceInfoDetails details)
                    : base(property, details) { _serviceType = serviceType; }
                private readonly Type _serviceType;
            }
        }

        private class Field : PropertyOrFieldServiceInfo
        {
            public override Type ServiceType { get { return _field.FieldType; } }
            public override IServiceInfo Create(Type serviceType, ServiceInfoDetails details)
            {
                return serviceType == null
                    ? new WithDetails(_field, details)
                    : new TypeWithDetails(_field, serviceType, details);
            }

            public override MemberInfo Member { get { return _field; } }
            public override void SetValue(object holder, object value)
            {
                _field.SetValue(holder, value);
            }

            public override string ToString()
            {
                return new StringBuilder().Print(this).Append(" as field ").Print(_field.Name, "\"").ToString();
            }

            private readonly FieldInfo _field;
            public Field(FieldInfo field)
            {
                _field = field;
            }

            private class WithDetails : Field
            {
                public override ServiceInfoDetails Details { get { return _details; } }
                public WithDetails(FieldInfo field, ServiceInfoDetails details)
                    : base(field) { _details = details; }
                private readonly ServiceInfoDetails _details;
            }

            private sealed class TypeWithDetails : WithDetails
            {
                public override Type ServiceType { get { return _serviceType; } }
                public TypeWithDetails(FieldInfo field, Type serviceType, ServiceInfoDetails details)
                    : base(field, details) { _serviceType = serviceType; }
                private readonly Type _serviceType;
            }
        }
    }

    /// <summary>
    /// Contains resolution stack with information about resolved service and factory for it,
    /// Additionally request is playing role of resolution context, containing <see cref="ResolutionState"/>, and
    /// weak reference to <see cref="IRegistry"/>. That the all required information for resolving services.
    /// Request implements <see cref="IResolver"/> interface on top of provided Registry, which could be use
    /// by delegate factories.
    /// </summary>
    public sealed class Request : IResolver
    {
        /// <summary>
        /// Creates Root empty request associated with provided <paramref name="registry"/>.
        /// Every resolution will start from Root request by pushing service information into request, 
        /// and then resolving it.
        /// </summary>
        /// <param name="registry">Reference to associated registry. 
        /// Could be changed later with <see cref="ReplaceRegistryWith"/> method.
        /// </param>
        /// <param name="state">Resolution state associated with container. 
        /// It is separated from registry, because later could be replaced: for instance by Parent container registry</param>
        /// <returns>New root request.</returns>
        public static Request CreateRoot(WeakReference registry, WeakReference state)
        {
            registry.ThrowIfNull().Target.ThrowIfNotOf(typeof(IRegistry));
            state.ThrowIfNull().Target.ThrowIfNotOf(typeof(ResolutionState));
            return new Request(null, registry, state, null, null, null);
        }

        /// <summary>
        /// Indicates that request is empty initial request: there is no <see cref="ServiceInfo"/> in such a request.
        /// </summary>
        public bool IsRoot
        {
            get { return ServiceInfo == null; }
        }

        #region Resolver

        public object ResolveDefault(Type serviceType, IfUnresolved ifUnresolved, Request _)
        {
            return Registry.ResolveDefault(serviceType, ifUnresolved, this);
        }

        public object ResolveKeyed(Type serviceType, object serviceKey, IfUnresolved ifUnresolved, Type requiredServiceType, Request _)
        {
            return Registry.ResolveKeyed(serviceType, serviceKey, ifUnresolved, requiredServiceType, this);
        }

        public void ResolvePropertiesAndFields(object instance, PropertiesAndFieldsSelector selectPropertiesAndFields, Request _)
        {
            Registry.ResolvePropertiesAndFields(instance, selectPropertiesAndFields, this);
        }

        public object Resolve(IServiceInfo info)
        {
            var details = info.Details;
            return details == ServiceInfoDetails.IfUnresolvedThrow || details == ServiceInfoDetails.IfUnresolvedReturnDefault
                ? Registry.ResolveDefault(info.ServiceType, details.IfUnresolved, this)
                : Registry.ResolveKeyed(info.ServiceType, details.ServiceKey, details.IfUnresolved, details.RequiredServiceType, this);
        }

        #endregion

        #region Resolution Scope

        public static readonly ParameterExpression ScopeParamExpr = Expression.Parameter(typeof(IScope), "scope");

        public static readonly MethodInfo GetScopeMethod = typeof(Request).GetDeclaredMethod("GetScope");

        internal static IScope GetScope(ref IScope scope)
        {
            return scope = scope ?? new Scope();
        }

        public static readonly Expression ScopeExpr = Expression.Call(GetScopeMethod, ScopeParamExpr);

        public IScope ResolutionScope
        {
            get { return _scope.Value; }
        }

        public IScope GetOrCreateResolutionScope()
        {
            if (_scope.Value == null)
                _scope.Swap(scope => scope ?? new Scope());
            return _scope.Value;
        }

        #endregion

        /// <summary>
        /// Reference to resolved items and cached factory expressions. 
        /// Used to propagate the state from resolution root, probably from another container (request creator).
        /// </summary>
        public ResolutionState State
        {
            get { return (_stateWeakRef.Target as ResolutionState).ThrowIfNull(Error.CONTAINER_IS_GARBAGE_COLLECTED); } 
        }

        public readonly Request Parent;
        public readonly IServiceInfo ServiceInfo;
        public readonly Factory ResolvedFactory;

        public IRegistry Registry
        {
            get { return (_registryWeakRef.Target as IRegistry).ThrowIfNull(Error.CONTAINER_IS_GARBAGE_COLLECTED); }
        }

        public Type ServiceType { get { return ServiceInfo == null ? null : ServiceInfo.ServiceType; } }
        public object ServiceKey { get { return ServiceInfo.ThrowIfNull().Details.ServiceKey; } }
        public IfUnresolved IfUnresolved { get { return ServiceInfo.ThrowIfNull().Details.IfUnresolved; } }
        public FactoryType ResolvedFactoryType { get { return ResolvedFactory.ThrowIfNull().Setup.Type; } }

        public Type ImplementationType
        {
            get { return ResolvedFactory == null ? null : ResolvedFactory.ImplementationType; }
        }

        public Request Push(IServiceInfo info)
        {
            if (IsRoot)
                return new Request(this, _registryWeakRef, _stateWeakRef, new Ref<IScope>(), info.ThrowIfNull(), null);

            ResolvedFactory.ThrowIfNull(Error.PUSHING_TO_REQUEST_WITHOUT_FACTORY, info.ThrowIfNull(), this);
            var inheritedInfo = info.InheritDependencyFromOwnerInfo(ServiceInfo, ResolvedFactory.Setup.Type);
            return new Request(this, _registryWeakRef, _stateWeakRef, _scope, inheritedInfo, null);
        }

        public Request Push(Type serviceType,
            object serviceKey = null, IfUnresolved ifUnresolved = IfUnresolved.Throw, Type requiredServiceType = null)
        {
            var info = requiredServiceType == null
                ? DryIoc.ServiceInfo.Of(serviceType, serviceKey, ifUnresolved)
                : DryIoc.ServiceInfo.Of(serviceType).With(ServiceInfoDetails.Of(requiredServiceType, serviceKey, ifUnresolved), this);
            return Push(info);
        }

        public Request ReplaceServiceInfoWith(IServiceInfo info)
        {
            return new Request(Parent, _registryWeakRef, _stateWeakRef, _scope, info, ResolvedFactory);
        }

        /// <summary>
        /// Changes registry to provided one. 
        /// Could be used by child container, to switch child registry to parent preserving the rest of request state.
        /// </summary>
        /// <param name="registry">Reference to registry to switch to.</param>
        /// <returns>Request with replaced registry.</returns>
        public Request ReplaceRegistryWith(WeakReference registry)
        {
            registry.ThrowIfNull().Target.ThrowIfNotOf(typeof(IRegistry));
            return new Request(Parent, registry, _stateWeakRef, _scope, ServiceInfo, ResolvedFactory);
        }

        /// <summary>Returns new request with set <see cref="ResolvedFactory"/>.</summary>
        /// <param name="factory">Factory to which request is resolved.</param>
        /// <returns>New request with set factory.</returns>
        public Request ResolveTo(Factory factory)
        {
            if (IsRoot || (ResolvedFactory != null && ResolvedFactory.FactoryID == factory.FactoryID))
                return this; // resolving only once, no need to check recursion again.

            if (factory.Setup.Type == FactoryType.Service) // skip recursion check for non-services: decorators, wrappers
                for (var p = Parent; !p.IsRoot; p = p.Parent)
                    Throw.If(p.ResolvedFactory.FactoryID == factory.FactoryID, Error.RECURSIVE_DEPENDENCY_DETECTED, Print(factory.FactoryID));

            return new Request(Parent, _registryWeakRef, _stateWeakRef, _scope, ServiceInfo, factory);
        }

        /// <summary>
        /// Searches parent request stack upward and returns closest parent of <see cref="FactoryType.Service"/>.
        /// If not found returns <see cref="IsRoot"/> request.
        /// </summary>
        /// <returns>Return closest <see cref="FactoryType.Service"/> parent or root.</returns>
        public Request GetNonWrapperParentOrRoot()
        {
            var p = Parent;
            while (!p.IsRoot && p.ResolvedFactory.Setup.Type == FactoryType.GenericWrapper)
                p = p.Parent;
            return p;
        }

        /// <summary>Enumerates all request stack parents. Last returned will <see cref="IsRoot"/> empty parent.</summary>
        /// <returns>Unfolding parents.</returns>
        public IEnumerable<Request> Enumerate()
        {
            for (var r = this; !r.IsRoot; r = r.Parent)
                yield return r;
        }

        /// <summary> Prints current request info only (no parents printed) to provided builder.</summary>
        /// <param name="s">Builder to print too.</param>
        /// <returns>Builder with appended info.</returns>
        public StringBuilder PrintCurrent(StringBuilder s)
        {
            if (IsRoot)
                return s.Append("<root>");
            if (ResolvedFactory != null && ResolvedFactory.Setup.Type != FactoryType.Service)
                s.Append(Enum.GetName(typeof(FactoryType), ResolvedFactory.Setup.Type)).Append(' ');
            if (ImplementationType != null && ImplementationType != ServiceType)
                s.Print(ImplementationType).Append(": ");
            return s.Append(ServiceInfo);
        }

        /// <summary>Prints full stack of requests starting from current one using <see cref="PrintCurrent"/>.</summary>
        /// <param name="recursiveFactoryID">Flag specifying that in case of found recursion/repetition of requests, 
        /// mark repeated requests.</param>
        /// <returns>Builder with appended request stack info.</returns>
        public StringBuilder Print(int recursiveFactoryID = -1)
        {
            var s = PrintCurrent(new StringBuilder());
            if (Parent == null)
                return s;

            s = recursiveFactoryID == -1 ? s : s.Append(" <--recursive");
            return Parent.Enumerate().TakeWhile(r => !r.IsRoot).Aggregate(s, (a, r) =>
            {
                a = r.PrintCurrent(a.AppendLine().Append(" in "));
                return r.ResolvedFactory.FactoryID == recursiveFactoryID ? a.Append(" <--recursive") : a;
            });
        }

        /// <summary>Print while request stack info to string using <seealso cref="Print"/>.</summary>
        /// <returns>String with info.</returns>
        public override string ToString()
        {
            return Print().ToString();
        }

        #region Implementation

        internal Request(Request parent,
            WeakReference registryWeakRef, WeakReference resolutionStateWeakRef, 
            Ref<IScope> scope, IServiceInfo serviceInfo,
            Factory resolvedFactory)
        {
            Parent = parent;
            _registryWeakRef = registryWeakRef;
            _stateWeakRef = resolutionStateWeakRef;
            _scope = scope;
            ServiceInfo = serviceInfo;
            ResolvedFactory = resolvedFactory;
        }

        private readonly WeakReference _registryWeakRef;
        private readonly WeakReference _stateWeakRef;
        private readonly Ref<IScope> _scope;

        #endregion
    }

    /// <summary>Type of services supported by Container.</summary>
    public enum FactoryType { Service, Decorator, GenericWrapper };

    /// <summary>Specify caching policy for service expressions created by <see cref="Factory"/></summary>
    public enum FactoryCaching { DisabledForExpression, EnabledForExpression };

    /// <summary>Base class to store optional <see cref="Factory"/> settings.</summary>
    public abstract class FactorySetup
    {
        public abstract FactoryType Type { get; }
        public virtual FactoryCaching Caching { get { return FactoryCaching.DisabledForExpression; } }
        public virtual object Metadata { get { return null; } }
        public virtual IReusedObjectBehavior ReusedObjectBehavior { get { return null; } }

        public readonly InjectionRules Rules;
        protected FactorySetup(InjectionRules rules) { Rules = rules ?? InjectionRules.Empty; }
        public abstract FactorySetup WithRules(InjectionRules rules);
    }

    public static class FactorySetupTools
    {
        public static T WithConstructor<T>(this T source, Func<Type, ConstructorInfo> constructor) where T : FactorySetup
        {
            return constructor == null ? source : (T)source.WithRules(source.Rules.With(constructor));
        }
    }

    public class Setup : FactorySetup
    {
        public static readonly Setup Default = new Setup();

        public static Setup With(
            ConstructorSelector constructor = null, ParameterSelector parameters = null, PropertiesAndFieldsSelector propertiesAndFields = null,
            FactoryCaching caching = FactoryCaching.EnabledForExpression, Func<object> lazyGetMetadata = null, object metadata = null)
        {
            var setup = caching == FactoryCaching.EnabledForExpression && lazyGetMetadata == null && metadata == null
                ? Default : new Setup(InjectionRules.Empty, caching, lazyGetMetadata);
            return constructor == null && parameters == null && propertiesAndFields == null
                ? setup : (Setup)setup.WithRules(InjectionRules.With(constructor, parameters, propertiesAndFields));
        }

        public static Setup WithMetadata(object metadata = null)
        {
            return metadata == null ? Default : new Setup(metadata: metadata);
        }

        public static Setup WithMetadata(Func<object> lazyGetMetadata)
        {
            return new Setup(lazyGetMetadata: lazyGetMetadata.ThrowIfNull());
        }

        public static FactorySetup With(IReusedObjectBehavior reusedObjectBehavior)
        {
            return new Setup(reusedObjectBehavior: reusedObjectBehavior.ThrowIfNull());
        }

        public override FactorySetup WithRules(InjectionRules rules)
        {
            return new Setup(rules, Caching, _lazyGetMetadata, _metadata);
        }

        public override FactoryType Type { get { return FactoryType.Service; } }
        public override FactoryCaching Caching { get { return _caching; } }
        public override IReusedObjectBehavior ReusedObjectBehavior { get { return _reusedObjectBehavior; } }
        public override object Metadata
        {
            get { return _metadata ?? (_metadata = _lazyGetMetadata == null ? null : _lazyGetMetadata()); }
        }

        #region Implementation

        private Setup(InjectionRules rules = null,
            FactoryCaching caching = FactoryCaching.EnabledForExpression,
            Func<object> lazyGetMetadata = null, object metadata = null,
            IReusedObjectBehavior reusedObjectBehavior = null)
            : base(rules)
        {
            _caching = caching;
            _lazyGetMetadata = lazyGetMetadata;
            _metadata = metadata;
            _reusedObjectBehavior = reusedObjectBehavior;
        }

        private readonly FactoryCaching _caching;
        private readonly Func<object> _lazyGetMetadata;
        private object _metadata;
        private readonly IReusedObjectBehavior _reusedObjectBehavior;

        #endregion
    }

    public class GenericWrapperSetup : FactorySetup
    {
        public static readonly GenericWrapperSetup Default = new GenericWrapperSetup();

        public static GenericWrapperSetup With(
            ConstructorSelector constructor = null, ParameterSelector parameters = null, PropertiesAndFieldsSelector propertiesAndFields = null,
            Func<Type[], Type> selectServiceTypeArg = null)
        {
            var setup = selectServiceTypeArg == null
                ? Default : new GenericWrapperSetup(InjectionRules.Empty, selectServiceTypeArg);
            return constructor == null && parameters == null && propertiesAndFields == null
                ? setup : (GenericWrapperSetup)setup.WithRules(InjectionRules.With(constructor, parameters, propertiesAndFields));
        }

        public static GenericWrapperSetup With(Func<Type[], Type> selectServiceTypeArg)
        {
            return selectServiceTypeArg == null
                ? Default : new GenericWrapperSetup(InjectionRules.Empty, selectServiceTypeArg);
        }

        public override FactorySetup WithRules(InjectionRules rules)
        {
            return new GenericWrapperSetup(rules, SelectWrappedServiceTypeArg);
        }

        public override FactoryType Type { get { return FactoryType.GenericWrapper; } }
        public readonly Func<Type[], Type> SelectWrappedServiceTypeArg;

        #region Implementation

        private GenericWrapperSetup(InjectionRules rules = null,
            Func<Type[], Type> selectServiceTypeArg = null)
            : base(rules)
        {
            SelectWrappedServiceTypeArg = selectServiceTypeArg ?? ThrowIfNotSingleTypeArg;
        }

        private static Type ThrowIfNotSingleTypeArg(Type[] typeArgs)
        {
            return typeArgs.ThrowIf(typeArgs.Length != 1, Error.GENERIC_WRAPPER_EXPECTS_SINGLE_TYPE_ARG_BY_DEFAULT)[0];
        }

        #endregion
    }

    public class DecoratorSetup : FactorySetup
    {
        public static readonly DecoratorSetup Default = new DecoratorSetup();

        public static DecoratorSetup With(
            ConstructorSelector constructor = null, ParameterSelector parameters = null, PropertiesAndFieldsSelector propertiesAndFields = null,
            Func<Request, bool> condition = null)
        {
            var setup = condition == null
                ? Default : new DecoratorSetup(InjectionRules.Empty, condition);
            return constructor == null && parameters == null && propertiesAndFields == null
                ? setup : (DecoratorSetup)setup.WithRules(InjectionRules.With(constructor, parameters, propertiesAndFields));
        }

        public static DecoratorSetup WithCondition(Func<Request, bool> condition = null)
        {
            return condition == null ? Default : new DecoratorSetup(InjectionRules.Empty, condition);
        }

        public override FactorySetup WithRules(InjectionRules rules)
        {
            return new DecoratorSetup(rules, Condition);
        }

        public override FactoryType Type { get { return FactoryType.Decorator; } }
        public readonly Func<Request, bool> Condition;

        #region Implementation

        private DecoratorSetup(InjectionRules rules = null, Func<Request, bool> condition = null)
            : base(rules)
        {
            Condition = condition ?? (_ => true);
        }

        #endregion
    }

    /// <summary>
    /// Base class for different ways to instantiate service: 
    /// <list type="bullet">
    /// <item>Through reflection - <see cref="ReflectionFactory"/></item>
    /// <item>Using custom delegate - <see cref="DelegateFactory"/></item>
    /// <item>Using custom expression - <see cref="ExpressionFactory"/></item>
    /// <item>Just use pre-created instance - <see cref="InstanceFactory"/></item>
    /// <item>To dynamically provide factory based on Request - <see cref="FactoryProvider"/></item>
    /// </list>
    /// For all of the types Factory should provide result as <see cref="Expression"/> and <see cref="FactoryDelegate"/>.
    /// Factories are supposed to be immutable as the results Cache is handled separately by <see cref="ResolutionState"/>.
    /// Each created factory has an unique ID set in <see cref="FactoryID"/>.
    /// </summary>
    public abstract class Factory
    {
        public readonly int FactoryID;
        public readonly IReuse Reuse;

        public FactorySetup Setup
        {
            get { return _setup; }
            protected internal set { _setup = value ?? DryIoc.Setup.Default; }
        }

        public virtual Type ImplementationType { get { return null; } }

        /// <summary>
        /// Indicates that Factory is factory provider and client should call <see cref="GetFactoryForRequestOrDefault"/>
        /// to get concrete factory.
        /// </summary>
        public virtual bool ProvidesFactoryForRequest { get { return false; } }

        protected Factory(IReuse reuse = null, FactorySetup setup = null)
        {
            FactoryID = Interlocked.Increment(ref _lastFactoryID);
            Reuse = reuse;
            Setup = setup ?? DryIoc.Setup.Default;
        }

        public virtual void ValidateBeforeRegistration(Type serviceType, IRegistry registry)
        {
            Throw.If(serviceType.IsGenericDefinition() && !ProvidesFactoryForRequest,
                Error.UNABLE_TO_REGISTER_NON_FACTORY_PROVIDER_FOR_OPEN_GENERIC_SERVICE, serviceType);
        }

        public virtual Factory GetFactoryForRequestOrDefault(Request request) { return null; }

        public abstract Expression CreateExpressionOrDefault(Request request);

        public virtual LambdaExpression CreateFuncWithArgsOrDefault(Type funcType, Request request, out IList<Type> unusedFuncArgs)
        {
            unusedFuncArgs = null;
            return null;
        }

        public Expression GetExpressionOrDefault(Request request, Type wrapperType = null)
        {
            request = request.ResolveTo(this);

            var decorator = request.Registry.GetDecoratorExpressionOrDefault(request);
            var noOrFuncDecorator = decorator == null || decorator is LambdaExpression;

            if (noOrFuncDecorator && wrapperType == null &&
                Setup.Caching == FactoryCaching.EnabledForExpression)
            {
                var cachedServiceExpr = request.State.GetCachedFactoryExpressionOrDefault(FactoryID);
                if (cachedServiceExpr != null)
                    return decorator == null ? cachedServiceExpr : Expression.Invoke(decorator, cachedServiceExpr);
            }

            var serviceExpr = noOrFuncDecorator ? CreateExpressionOrDefault(request) : decorator;
            if (serviceExpr == null)
                return null;

            if (Reuse != null)
            {
                var scope = Reuse.GetScope(request);

                // When singleton scope and no Func in request chain 
                // then reused instance should can be inserted directly instead of calling Scope method.
                if (Reuse is SingletonReuse &&
                    (request.Parent.IsRoot || !request.Parent.Enumerate().Any(GenericsSupport.IsFunc)))
                    serviceExpr = GetInstantiatedScopedServiceExpression(serviceExpr, scope, request, wrapperType);
                else
                    serviceExpr = GetScopedServiceExpression(serviceExpr, scope, request, wrapperType);
            }
            
            if (noOrFuncDecorator && wrapperType == null &&
                Setup.Caching == FactoryCaching.EnabledForExpression)
                request.State.CacheFactoryExpression(FactoryID, serviceExpr);
           
            if (noOrFuncDecorator && decorator != null)
                serviceExpr = Expression.Invoke(decorator, serviceExpr);

            return serviceExpr;
        }

        public virtual LambdaExpression GetFuncWithArgsOrDefault(Type funcType, Request request, out IList<Type> unusedFuncArgs)
        {
            request = request.ResolveTo(this);
            var func = CreateFuncWithArgsOrDefault(funcType, request, out unusedFuncArgs);
            if (func == null)
                return null;

            var decorator = request.Registry.GetDecoratorExpressionOrDefault(request);
            if (decorator != null && !(decorator is LambdaExpression))
                return Expression.Lambda(funcType, decorator, func.Parameters);

            if (Reuse != null)
            {
                var scope = Reuse.GetScope(request);
                var reusedInstanceExpr = GetScopedServiceExpression(func.Body, scope, request);
                func = Expression.Lambda(funcType, reusedInstanceExpr, func.Parameters);
            }

            if (decorator != null)
                func = Expression.Lambda(funcType, Expression.Invoke(decorator, func.Body), func.Parameters);

            return func;
        }

        public virtual FactoryDelegate GetDelegateOrDefault(Request request)
        {
            var expression = GetExpressionOrDefault(request);
            return expression == null ? null : expression.CompileToDelegate(request.Registry.Rules);
        }

        public override string ToString()
        {
            var s = new StringBuilder();
            s.Append("{FactoryID=").Append(FactoryID);
            if (ImplementationType != null)
                s.Append(", ImplType=").Print(ImplementationType);
            if (Reuse != null)
                s.Append(", ReuseType=").Print(Reuse.GetType());
            if (Setup.Type != DryIoc.Setup.Default.Type)
                s.Append(", FactoryType=").Append(Setup.Type);
            return s.Append("}").ToString();
        }

        #region Implementation

        private static int _lastFactoryID;
        private FactorySetup _setup;

        private static readonly MethodInfo _scopeGetOrAddMethod = typeof(IScope).GetDeclaredMethod("GetOrAdd");

        protected Expression GetInstantiatedScopedServiceExpression(Expression serviceExpr, IScope scope, Request request, 
            Type wrapperType = null)
        {
            var factoryDelegate = serviceExpr.ToFactoryExpression().Compile();
            object reused;
            if (Setup.ReusedObjectBehavior == null)
            {
                reused = scope.GetOrAdd(FactoryID,
                    () => factoryDelegate(request.State.Items, request.ResolutionScope));
            }
            else
            {
                var behavior = Setup.ReusedObjectBehavior;
                var wrapped = scope.GetOrAdd(FactoryID,
                    () => behavior.Wrap(factoryDelegate(request.State.Items, request.ResolutionScope)));
                
                if (wrapperType != null)
                    return wrapperType != behavior.WrapperType ? null 
                        : request.State.GetOrAddItemExpression(wrapped, wrapperType);
                
                reused = behavior.Unwrap(wrapped);
            }

            return request.State.GetOrAddItemExpression(reused, serviceExpr.Type);
        }

        protected Expression GetScopedServiceExpression(Expression serviceExpr, IScope scope, Request request, Type wrapperType = null)
        {
            var scopeExpr = scope == request.ResolutionScope
                ? Request.ScopeExpr
                : request.State.GetOrAddItemExpression(scope);

            var factoryIDExpr = Expression.Constant(FactoryID);
            if (Setup.ReusedObjectBehavior == null)
                return Expression.Convert(Expression.Call(scopeExpr, _scopeGetOrAddMethod,
                    factoryIDExpr, Expression.Lambda<Func<object>>(serviceExpr, null)), serviceExpr.Type);

            var behavior = Setup.ReusedObjectBehavior;
            var behaviorExpr = request.State.GetOrAddItemExpression(behavior, typeof(IReusedObjectBehavior));

            // First transform factory function to function returning wrapped object.
            var wrappedServiceExpr = Expression.Call(behaviorExpr, "Wrap", null, serviceExpr);

            // Make call expression like the: scope.GetOrAdd(id, () => behavior.Wrap(new Service))
            var getWrappedServiceExpr = Expression.Lambda(wrappedServiceExpr, null);
            var wrappedScopedServiceExpr = Expression.Call(scopeExpr, _scopeGetOrAddMethod, factoryIDExpr, getWrappedServiceExpr);

            if (wrapperType != null)
                return wrapperType != behavior.WrapperType ? null 
                    : Expression.Convert(wrappedScopedServiceExpr, wrapperType);

            var unwrappedServiceExpr = Expression.Call(behaviorExpr, "Unwrap", null, wrappedScopedServiceExpr);
            return unwrappedServiceExpr;
        }

        #endregion
    }

    /// <summary>Thin wrapper for pre-created service object registered: more lightweight then <see cref="DelegateFactory"/>,
    /// and provides type of registered instance as <see cref="ImplementationType"/></summary>
    /// <remarks>Reuse is not applied to registered object, therefore it does not saved to any Scope, 
    /// and container is not responsible for Disposing it.</remarks>
    public sealed class InstanceFactory : Factory
    {
        public override Type ImplementationType
        {
            get { return _instance.GetType(); }
        }

        public InstanceFactory(object instance, FactorySetup setup = null)
            : base(null, setup)
        {
            _instance = instance.ThrowIfNull();
        }

        public override void ValidateBeforeRegistration(Type serviceType, IRegistry _)
        {
            _instance.ThrowIfNotOf(serviceType, Error.REGED_OBJ_NOT_ASSIGNABLE_TO_SERVICE_TYPE, serviceType);
        }

        public override Expression CreateExpressionOrDefault(Request request)
        {
            return request.State.GetOrAddItemExpression(_instance, _instance.GetType());
        }

        public override FactoryDelegate GetDelegateOrDefault(Request _)
        {
            return (state, scope) => _instance;
        }

        private readonly object _instance;
    }

    public delegate ConstructorInfo ConstructorSelector(Type implementationType, Request request);
    public delegate ParameterServiceInfo ParameterSelector(ParameterInfo parameter, Request request);
    public delegate IEnumerable<PropertyOrFieldServiceInfo> PropertiesAndFieldsSelector(Type implementationType, Request request);

    /// <summary>
    /// Contains alternative rules to select constructor in implementation type registered with <see cref="ReflectionFactory"/>
    /// </summary>
    public static partial class Constructor
    {
        /// <summary>
        /// Searches for constructor with all resolvable parameters or throws <see cref="ContainerException"/> if not found.
        /// Works both for resolving as service and as Func&lt;TArgs..., TService&gt;.
        /// </summary>
        /// <param name="implementationType">Type to instantiate with found constructor.</param>
        /// <param name="request"><see cref="Request"/> object leading to resolve this type.</param>
        /// <returns>Found constructor or throws exception.</returns>
        public static ConstructorInfo WithAllResolvableArguments(Type implementationType, Request request)
        {
            var ctors = implementationType.GetAllConstructors().ToArrayOrSelf();
            if (ctors.Length == 0)
                return null; // Delegate handling of constructor absence to caller code.

            if (ctors.Length == 1)
                return ctors[0];

            var ctorsWithMoreParamsFirst = ctors
                .Select(c => new { Ctor = c, Params = c.GetParameters() })
                .OrderByDescending(x => x.Params.Length);

            if (request.Parent.IsFuncWithArgs())
            {
                // For Func with arguments, match constructor should contain all input arguments and the rest should be resolvable.
                var funcType = request.Parent.ServiceType;
                var funcArgs = funcType.GetGenericParamsAndArgs();
                var inputArgCount = funcArgs.Length - 1;

                var matchedCtor = ctorsWithMoreParamsFirst
                    .Where(x => x.Params.Length >= inputArgCount)
                    .FirstOrDefault(x =>
                    {
                        var matchedIndecesMask = 0;
                        return x.Params.Except(
                            x.Params.Where(p =>
                            {
                                var inputArgIndex = funcArgs.IndexOf(t => t == p.ParameterType);
                                if (inputArgIndex == -1 || inputArgIndex == inputArgCount ||
                                    (matchedIndecesMask & inputArgIndex << 1) != 0) // input argument was already matched by another parameter
                                    return false;
                                matchedIndecesMask |= inputArgIndex << 1;
                                return true;
                            }))
                            .All(p => ResolveParameter(p, request.ResolvedFactory, request) != null);
                    });

                return matchedCtor.ThrowIfNull(Error.UNABLE_TO_FIND_MATCHING_CTOR_FOR_FUNC_WITH_ARGS, funcType, request).Ctor;
            }
            else
            {
                var matchedCtor = ctorsWithMoreParamsFirst.FirstOrDefault(
                    x => x.Params.All(p => ResolveParameter(p, request.ResolvedFactory, request) != null));
                return matchedCtor.ThrowIfNull(Error.UNABLE_TO_FIND_CTOR_WITH_ALL_RESOLVABLE_ARGS, request).Ctor;
            }
        }

        #region Implementation

        private static Expression ResolveParameter(ParameterInfo p, Factory factory, Request request)
        {
            var registry = request.Registry;
            var getParamInfo = registry.Rules.Parameters.OverrideWith(factory.Setup.Rules.Parameters);
            var paramInfo = getParamInfo(p, request) ?? ParameterServiceInfo.Of(p);
            var paramRequest = request.Push(paramInfo.With(ServiceInfoDetails.IfUnresolvedReturnDefault, request));
            var paramFactory = registry.ResolveFactory(paramRequest);
            return paramFactory == null ? null : paramFactory.GetExpressionOrDefault(paramRequest);
        }

        #endregion
    }

    /// <summary>DSL for specifying <see cref="ParameterSelector"/> injection rules.</summary>
    public static partial class Parameters
    {
        /// <summary>Specifies to return default details <see cref="ServiceInfoDetails.IfUnresolvedThrow"/> for all parameters.</summary>
        public static ParameterSelector All = (p, req) => null;

        /// <summary>Specifies that all parameters could be set to default if unresolved.</summary>
        public static ParameterSelector AllDefaultIfUnresolved = ((p, req) =>
            ParameterServiceInfo.Of(p).With(ServiceInfoDetails.IfUnresolvedReturnDefault, req));

        public static ParameterSelector OverrideWith(this ParameterSelector source, ParameterSelector other)
        {
            return source == null || source == All ? other ?? All
                : other == null || other == All ? source
                : (parameter, req) => other(parameter, req) ?? source(parameter, req);
        }

        public static ParameterSelector And(this ParameterSelector source, Func<ParameterInfo, bool> condition,
            Type requiredServiceType = null, object serviceKey = null, IfUnresolved ifUnresolved = IfUnresolved.Throw)
        {
            return source.WithDetails(condition, ServiceInfoDetails.Of(requiredServiceType, serviceKey, ifUnresolved));
        }

        public static ParameterSelector And(this ParameterSelector source, string name,
            Type requiredServiceType = null, object serviceKey = null, IfUnresolved ifUnresolved = IfUnresolved.Throw)
        {
            return source.And(p => p.Name.Equals(name), requiredServiceType, serviceKey, ifUnresolved);
        }

        public static ParameterSelector And(this ParameterSelector source, string name, object value)
        {
            return source.And(name, _ => value);
        }

        public static ParameterSelector And(this ParameterSelector source, string name, Func<Request, object> getValue)
        {
            return source.WithDetails(p => p.Name.Equals(name), ServiceInfoDetails.Of(getValue));
        }

        public static ParameterSelector And(this ParameterSelector source, Type type, object value)
        {
            return source.And(type, _ => value);
        }

        public static ParameterSelector And(this ParameterSelector source, Type type, Func<Request, object> getValue)
        {
            type.ThrowIfNull();
            return source.WithDetails(p => type.IsAssignableTo(p.ParameterType), ServiceInfoDetails.Of(getValue));
        }

        public static IEnumerable<Attribute> GetAttributes(this ParameterInfo paremeter, Type attributeType = null, bool inherit = false)
        {
            return paremeter.GetCustomAttributes(attributeType ?? typeof(Attribute), inherit).Cast<Attribute>();
        }

        #region Implementation

        private static ParameterSelector WithDetails(this ParameterSelector source, Func<ParameterInfo, bool> condition,
            ServiceInfoDetails details)
        {
            condition.ThrowIfNull();
            return (parameter, req) => condition(parameter)
                ? ParameterServiceInfo.Of(parameter).With(details, req)
                : source(parameter, req);
        }

        #endregion
    }

    /// <summary>DSL for specifying <see cref="PropertiesAndFieldsSelector"/> injection rules.</summary>
    public static partial class PropertiesAndFields
    {
        /// <summary>Say to not resolve any properties or fields.</summary>
        public static PropertiesAndFieldsSelector None = (type, request) => null;

        /// <summary>
        /// Public assignable instance members of any type except object, string, primitives types, and arrays of those.
        /// </summary>
        public static PropertiesAndFieldsSelector AllPublicNonPrimitive = All(Flags.PublicNonPrimitive);

        /// <summary>Flags to specify visibility of properties and fields to resolve.</summary>
        public enum Flags { PublicNonPrimitive, Public, NonPrimitive, All }

        public delegate PropertyOrFieldServiceInfo GetInfo(MemberInfo m, Request req);

        /// <summary>
        /// Generates selector property and field selector with settings specified by parameters.
        /// If all parameters are omitted the return all public not primitive members.
        /// </summary>
        /// <param name="flags">(optional) Specifies visibility of members to be resolved. Default is <see cref="Flags.PublicNonPrimitive"/>.</param>
        /// <param name="getInfoOrNull">(optional) Return service info for a member or null to skip it resolution.</param>
        /// <returns>Result selector composed using provided settings.</returns>
        public static PropertiesAndFieldsSelector All(Flags flags, GetInfo getInfoOrNull = null)
        {
            var getInfo = getInfoOrNull ?? ((m, req) => PropertyOrFieldServiceInfo.Of(m));
            return (t, req) =>
                t.GetAll(_ => _.DeclaredProperties).Where(p => p.Match(flags)).Select(m => getInfo(m, req)).Concat(
                t.GetAll(_ => _.DeclaredFields).Where(f => f.Match(flags)).Select(m => getInfo(m, req)));
        }

        /// <summary>
        /// Compose properties and fields selector using provided settings: 
        /// in particularly I can change default setting to return null if member is unresolved,
        /// and exclude properties by name, type (using <see cref="GetPropertyOrFieldType"/>), etc.
        /// </summary>
        /// <param name="ifUnresolved">(optional) Specifies for all members to throw or return default if unresolved, by default does not throw.</param>
        /// <param name="flags">(optional) Specifies visibility of members to be resolved. Default is <see cref="Flags.PublicNonPrimitive"/>.</param>
        /// <returns>Result selector composed using provided settings.</returns>
        public static PropertiesAndFieldsSelector All(IfUnresolved ifUnresolved = IfUnresolved.ReturnDefault, Flags flags = Flags.PublicNonPrimitive)
        {
            var selector = ifUnresolved == IfUnresolved.ReturnDefault
                ? (GetInfo)null
                : (m, req) => ifUnresolved == IfUnresolved.ReturnDefault
                    ? PropertyOrFieldServiceInfo.Of(m)
                    : PropertyOrFieldServiceInfo.Of(m).With(ServiceInfoDetails.IfUnresolvedThrow, req);
            return All(flags, selector);
        }

        /// <summary>
        /// Selects members provided by <paramref name="source"/> excluding members that satisfy condition <paramref name="except"/>.
        /// </summary>
        /// <param name="source">Source selection of properties and fields, 
        /// could be <see cref="None"/>, or see <see cref="AllPublicNonPrimitive"/>, 
        /// or one created by <see cref="All(DryIoc.PropertiesAndFields.Flags,DryIoc.PropertiesAndFields.GetInfo)"/></param>
        /// <param name="except">(optional) Specifies rule to exclude members, e.g. exclude all fields, or property with specific name or attribute.</param>
        /// <returns>Result selector composed using provided settings.</returns>
        public static PropertiesAndFieldsSelector Except(this PropertiesAndFieldsSelector source, Func<MemberInfo, bool> except)
        {
            except.ThrowIfNull();
            return (type, req) => source(type, req).Where(x => !except(x.Member));
        }

        public static PropertiesAndFieldsSelector OverrideWith(this PropertiesAndFieldsSelector source, PropertiesAndFieldsSelector other)
        {
            return source == null || source == None ? (other ?? None)
                 : other == null || other == None ? source
                 : (type, req) =>
                {
                    var sourceMembers = source(type, req).ToArray();
                    var otherMembers = other(type, req).ToArray();
                    return sourceMembers == null || sourceMembers.Length == 0 ? otherMembers
                        : otherMembers == null || otherMembers.Length == 0 ? sourceMembers
                        : sourceMembers
                            .Where(s => s != null && otherMembers.All(o => o == null || !s.Member.Name.Equals(o.Member.Name)))
                            .Concat(otherMembers);
                };
        }

        public static PropertiesAndFieldsSelector And(this PropertiesAndFieldsSelector source, string name,
            Type requiredServiceType = null, object serviceKey = null, IfUnresolved ifUnresolved = IfUnresolved.ReturnDefault)
        {
            return source.WithDetails(name, ServiceInfoDetails.Of(requiredServiceType, serviceKey, ifUnresolved));
        }

        public static PropertiesAndFieldsSelector And(this PropertiesAndFieldsSelector source, string name, object value)
        {
            return source.WithDetails(name, ServiceInfoDetails.Of(_ => value));
        }

        public static PropertiesAndFieldsSelector And(this PropertiesAndFieldsSelector source, string name, Func<Request, object> getValue)
        {
            return source.WithDetails(name, ServiceInfoDetails.Of(getValue));
        }

        public static PropertiesAndFieldsSelector And(this PropertiesAndFieldsSelector source, Func<MemberInfo, bool> condition,
            Type requiredServiceType = null, object serviceKey = null, IfUnresolved ifUnresolved = IfUnresolved.ReturnDefault)
        {
            return source.WithDetails(condition, ServiceInfoDetails.Of(requiredServiceType, serviceKey, ifUnresolved));
        }

        public static PropertiesAndFieldsSelector And(this PropertiesAndFieldsSelector source, Type type, Func<Request, object> getValue)
        {
            return source.WithDetails(m => type.IsAssignableTo(m.GetPropertyOrFieldType()), ServiceInfoDetails.Of(getValue));
        }

        public static PropertiesAndFieldsSelector And(this PropertiesAndFieldsSelector source, Type type, object value)
        {
            return source.And(type, _ => value);
        }

        #region Tools

        /// <summary>
        /// Return either <see cref="PropertyInfo.PropertyType"/>, or <see cref="FieldInfo.FieldType"/> 
        /// depending on actual type of the <paramref name="member"/>.
        /// </summary>
        /// <param name="member">Expecting member of type <see cref="PropertyInfo"/> or <see cref="FieldInfo"/> only.</param>
        /// <returns>Type of property of field.</returns>
        public static Type GetPropertyOrFieldType(this MemberInfo member)
        {
            return member is PropertyInfo
                ? ((PropertyInfo)member).PropertyType
                : ((FieldInfo)member).FieldType;
        }

        /// <remarks>Matches property base on visibility <see cref="Flags"/> provided.</remarks>
        public static bool Match(this PropertyInfo property, Flags flags = Flags.PublicNonPrimitive)
        {
            return property.CanWrite
                && (flags == Flags.NonPrimitive || flags == Flags.All || property.IsPublic())
                && (flags == Flags.Public || flags == Flags.All || !property.PropertyType.IsPrimitiveOrObjectOrString());
        }

        /// <remarks>Matches field base on visibility <see cref="Flags"/> provided.</remarks>
        public static bool Match(this FieldInfo field, Flags flags = Flags.PublicNonPrimitive)
        {
            return !field.IsInitOnly && !field.IsBackingField()
                && (flags == Flags.Public || flags == Flags.All || !field.FieldType.IsPrimitiveOrObjectOrString());
        }

        public static bool IsBackingField(this FieldInfo field)
        {
            return field.Name[0] == '<';
        }

        public static bool IsPublic(this PropertyInfo property)
        {
            return _getPropertySetMethodDelegate(property) != null;
        }

        public static IEnumerable<Attribute> GetAttributes(this MemberInfo member, Type attributeType = null, bool inherit = false)
        {
            return member.GetCustomAttributes(attributeType ?? typeof(Attribute), inherit).Cast<Attribute>();
        }

        #endregion

        #region Implementation

        private static PropertiesAndFieldsSelector WithDetails(this PropertiesAndFieldsSelector source,
            string name, ServiceInfoDetails details)
        {
            name.ThrowIfNull();
            return source.OverrideWith((type, request) =>
            {
                var property = type.GetPropertyOrNull(name);
                if (property != null && property.Match(Flags.All))
                    return new[] { PropertyOrFieldServiceInfo.Of(property).With(details, request) };

                var field = type.GetFieldOrNull(name);
                if (field != null && field.Match(Flags.All))
                    return new[] { PropertyOrFieldServiceInfo.Of(field).With(details, request) };

                return Throw.No<IEnumerable<PropertyOrFieldServiceInfo>>(
                    Error.UNABLE_TO_FIND_SPECIFIED_WRITEABLE_PROPERTY_OR_FIELD, name, request);
            });
        }

        private static PropertiesAndFieldsSelector WithDetails(this PropertiesAndFieldsSelector source,
            Func<MemberInfo, bool> condition, ServiceInfoDetails details)
        {
            condition.ThrowIfNull();
            return source.OverrideWith((ownerType, request) =>
                ownerType.GetAll(_ => _.DeclaredProperties).Where(p => p.Match(Flags.All) && condition(p))
                .Select(p => PropertyOrFieldServiceInfo.Of(p).With(details, request)).Concat(
                ownerType.GetAll(_ => _.DeclaredFields).Where(f => f.Match(Flags.All) && condition(f))
                .Select(f => PropertyOrFieldServiceInfo.Of(f).With(details, request))));
        }

        private static readonly Func<PropertyInfo, MethodInfo> _getPropertySetMethodDelegate =
            ExpressionTools.GetMethodDelegate<PropertyInfo, MethodInfo>("GetSetMethod");

        #endregion
    }

    public sealed class ReflectionFactory : Factory
    {
        public override Type ImplementationType { get { return _implementationType; } }
        public override bool ProvidesFactoryForRequest { get { return _implementationType.IsGenericDefinition(); } }

        public ReflectionFactory(Type implementationType, IReuse reuse = null, FactorySetup setup = null)
            : base(reuse, setup)
        {
            _implementationType = implementationType.ThrowIfNull()
                .ThrowIf(implementationType.IsAbstract(), Error.EXPECTED_NON_ABSTRACT_IMPL_TYPE);
        }

        /// <remarks>
        /// Before registering factory checks that ImplementationType is assignable Or
        /// in case of open generics, compatible with <paramref name="serviceType"/>. 
        /// Then checks that there is defined constructor selector for implementation type with multiple/no constructors.
        /// </remarks>
        public override void ValidateBeforeRegistration(Type serviceType, IRegistry registry)
        {
            base.ValidateBeforeRegistration(serviceType, registry);

            var implType = _implementationType;
            if (!implType.IsGenericDefinition())
            {
                if (implType.IsOpenGeneric())
                    Throw.Me(Error.USUPPORTED_REGISTRATION_OF_NON_GENERIC_IMPL_TYPE_DEFINITION_BUT_WITH_GENERIC_ARGS,
                        implType, implType.GetGenericDefinitionOrNull());

                if (implType != serviceType && serviceType != typeof(object) &&
                    Array.IndexOf(implType.GetImplementedTypes(), serviceType) == -1)
                    Throw.Me(Error.EXPECTED_IMPL_TYPE_ASSIGNABLE_TO_SERVICE_TYPE, implType, serviceType);
            }
            else if (implType != serviceType)
            {
                if (serviceType.IsGenericDefinition())
                {
                    var implementedTypes = implType.GetImplementedTypes();
                    var implementedOpenGenericTypes = implementedTypes.Where(t => t.GetGenericDefinitionOrNull() == serviceType);

                    var implTypeArgs = implType.GetGenericParamsAndArgs();
                    Throw.If(!implementedOpenGenericTypes.Any(t => t.ContainsAllGenericParameters(implTypeArgs)),
                        Error.UNABLE_TO_REGISTER_OPEN_GENERIC_IMPL_CAUSE_SERVICE_DOES_NOT_SPECIFY_ALL_TYPE_ARGS,
                        implType, serviceType, implementedOpenGenericTypes);
                }
                else if (implType.IsGeneric() && serviceType.IsOpenGeneric())
                    Throw.Me(Error.USUPPORTED_REGISTRATION_OF_NON_GENERIC_SERVICE_TYPE_DEFINITION_BUT_WITH_GENERIC_ARGS,
                        serviceType, serviceType.GetGenericDefinitionOrNull());
                else
                    Throw.Me(Error.UNABLE_TO_REGISTER_OPEN_GENERIC_IMPL_WITH_NON_GENERIC_SERVICE, implType, serviceType);
            }

            if (Setup.Rules.Constructor == null && registry.Rules.Constructor == null)
            {
                var publicCtorCount = implType.GetAllConstructors().Count();
                Throw.If(publicCtorCount != 1, Error.UNSPECIFIED_HOWTO_SELECT_CONSTRUCTOR_FOR_IMPLTYPE, implType, publicCtorCount);
            }
        }

        /// <summary>
        /// Given factory with open-generic implementation type creates factory with closed type with type arguments provided by service type.
        /// </summary>
        /// <param name="request"><see cref="Request"/> with service type which provides concrete type arguments.</param>
        /// <returns>Factory with the same setup and reuse but with closed concrete implementation type.</returns>
        public override Factory GetFactoryForRequestOrDefault(Request request)
        {
            var closedTypeArgs = _implementationType == request.ServiceType.GetGenericDefinitionOrNull()
                ? request.ServiceType.GetGenericParamsAndArgs()
                : GetClosedTypeArgsForGenericImplementationType(_implementationType, request);

            var closedImplType = _implementationType.MakeGenericType(closedTypeArgs);

            return new ReflectionFactory(closedImplType, Reuse, Setup);
        }

        public override Expression CreateExpressionOrDefault(Request request)
        {
            var ctor = SelectConstructor(request);
            var ctorParams = ctor.GetParameters();

            Expression[] paramExprs = null;
            if (ctorParams.Length != 0)
            {
                paramExprs = new Expression[ctorParams.Length];
                for (var i = 0; i < ctorParams.Length; i++)
                {
                    var info = GetParameterServiceInfo(ctorParams[i], request);
                    var expr = GetDependencyExpressionOrNull(info, request);
                    if (expr == null) return null;
                    paramExprs[i] = expr;
                }
            }

            var newExpr = Expression.New(ctor, paramExprs);
            return InitMembersIfRequired(newExpr, request);
        }

        public override LambdaExpression CreateFuncWithArgsOrDefault(Type funcType, Request request, out IList<Type> unusedFuncArgs)
        {
            var funcParamTypes = funcType.GetGenericParamsAndArgs();
            Throw.If(funcParamTypes.Length == 1, Error.EXPECTED_FUNC_WITH_MULTIPLE_ARGS, funcType);
            unusedFuncArgs = null;

            var ctor = SelectConstructor(request);
            var ctorParams = ctor.GetParameters();
            var paramExprs = new Expression[ctorParams.Length];
            var funcParamExprs = new ParameterExpression[funcParamTypes.Length - 1]; // (minus Func return parameter).

            for (var i = 0; i < ctorParams.Length; i++)
            {
                var ctorParam = ctorParams[i];
                for (var fp = 0; fp < funcParamTypes.Length - 1; fp++)
                {
                    var funcParamType = funcParamTypes[fp];
                    if (ctorParam.ParameterType == funcParamType &&
                        funcParamExprs[fp] == null) // Skip if Func parameter was already used for constructor.
                    {
                        paramExprs[i] = funcParamExprs[fp] = Expression.Parameter(funcParamType, ctorParam.Name);
                        break;
                    }
                }

                if (paramExprs[i] == null) // If no matching constructor parameter found in Func, resolve it from Container.
                {
                    var info = GetParameterServiceInfo(ctorParam, request);
                    var expr = GetDependencyExpressionOrNull(info, request);
                    if (expr == null) return null;
                    paramExprs[i] = expr;
                }
            }

            // Find unused Func parameters (present in Func but not in constructor) and create "_" (ignored) Parameter expressions for them.
            // In addition store unused parameter in output list for client review.
            for (var fp = 0; fp < funcParamExprs.Length; fp++)
            {
                if (funcParamExprs[fp] == null) // unused parameter
                {
                    if (unusedFuncArgs == null)
                        unusedFuncArgs = new List<Type>(2);
                    var funcParamType = funcParamTypes[fp];
                    unusedFuncArgs.Add(funcParamType);
                    funcParamExprs[fp] = Expression.Parameter(funcParamType, "_");
                }
            }

            var newExpr = Expression.New(ctor, paramExprs);
            var newExprInitialized = InitMembersIfRequired(newExpr, request);
            return Expression.Lambda(funcType, newExprInitialized, funcParamExprs);
        }

        #region Implementation

        private readonly Type _implementationType;

        private ConstructorInfo SelectConstructor(Request request)
        {
            var implType = _implementationType;
            var selector = Setup.Rules.Constructor ?? request.Registry.Rules.Constructor;
            if (selector != null)
                return selector(implType, request).ThrowIfNull(Error.UNABLE_TO_SELECT_CTOR_USING_SELECTOR, implType);

            var ctors = implType.GetAllConstructors().ToArrayOrSelf();
            Throw.If(ctors.Length == 0, Error.NO_PUBLIC_CONSTRUCTOR_DEFINED, implType);
            Throw.If(ctors.Length > 1, Error.UNABLE_TO_SELECT_CONSTRUCTOR, ctors.Length, implType);
            return ctors[0];
        }

        private Expression InitMembersIfRequired(NewExpression newService, Request request)
        {
            var getInfos = request.Registry.Rules.PropertiesAndFields.OverrideWith(Setup.Rules.PropertiesAndFields);
            var infos = getInfos(_implementationType, request);
            if (infos == null)
                return newService;

            var bindings = new List<MemberBinding>();
            foreach (var info in infos)
                if (info != null)
                {
                    var expr = GetDependencyExpressionOrNull(info, request);
                    if (expr == null) return null;
                    bindings.Add(Expression.Bind(info.Member, expr));
                }

            return bindings.Count == 0 ? (Expression)newService : Expression.MemberInit(newService, bindings);
        }

        private ParameterServiceInfo GetParameterServiceInfo(ParameterInfo parameter, Request request)
        {
            return request.Registry.Rules.Parameters.OverrideWith(Setup.Rules.Parameters)(parameter, request)
                ?? ParameterServiceInfo.Of(parameter);
        }

        private static Type[] GetClosedTypeArgsForGenericImplementationType(Type implType, Request request)
        {
            var serviceTypeArgs = request.ServiceType.GetGenericParamsAndArgs();
            var serviceTypeGenericDef = request.ServiceType.GetGenericDefinitionOrNull().ThrowIfNull();

            var openImplTypeArgs = implType.GetGenericParamsAndArgs();
            var implementedTypes = implType.GetImplementedTypes();

            Type[] resultImplTypeArgs = null;
            for (var i = 0; resultImplTypeArgs == null && i < implementedTypes.Length; i++)
            {
                var implementedType = implementedTypes[i];
                if (implementedType.IsOpenGeneric() &&
                    implementedType.GetGenericDefinitionOrNull() == serviceTypeGenericDef)
                {
                    var matchedTypeArgs = new Type[openImplTypeArgs.Length];
                    if (MatchServiceWithImplementedTypeArgs(ref matchedTypeArgs,
                        openImplTypeArgs, implementedType.GetGenericParamsAndArgs(), serviceTypeArgs))
                        resultImplTypeArgs = matchedTypeArgs;
                }
            }

            resultImplTypeArgs = resultImplTypeArgs.ThrowIfNull(
                Error.UNABLE_TO_MATCH_IMPL_BASE_TYPES_WITH_SERVICE_TYPE, implType, implementedTypes, request);

            var unmatchedArgIndex = Array.IndexOf(resultImplTypeArgs, null);
            if (unmatchedArgIndex != -1)
                Throw.Me(Error.UNABLE_TO_FIND_OPEN_GENERIC_IMPL_TYPE_ARG_IN_SERVICE,
                    implType, openImplTypeArgs[unmatchedArgIndex], request);

            return resultImplTypeArgs;
        }

        private static bool MatchServiceWithImplementedTypeArgs(ref Type[] matchedImplArgs,
            Type[] openImplementationArgs, Type[] openImplementedArgs, Type[] closedServiceArgs)
        {
            for (var i = 0; i < openImplementedArgs.Length; i++)
            {
                var openImplementedArg = openImplementedArgs[i];
                var closedServiceArg = closedServiceArgs[i];
                if (openImplementedArg.IsGenericParameter)
                {
                    var matchedIndex = openImplementationArgs.IndexOf(t => t.Name == openImplementedArg.Name);
                    if (matchedIndex != -1)
                    {
                        if (matchedImplArgs[matchedIndex] == null)
                            matchedImplArgs[matchedIndex] = closedServiceArg;
                        else if (matchedImplArgs[matchedIndex] != closedServiceArg)
                            return false; // more than one closedServiceArg is matching with single openArg
                    }
                }
                else if (openImplementedArg != closedServiceArg)
                {
                    if (!openImplementedArg.IsOpenGeneric() ||
                        openImplementedArg.GetGenericDefinitionOrNull() != closedServiceArg.GetGenericDefinitionOrNull())
                        return false; // openArg and closedArg are different types

                    if (!MatchServiceWithImplementedTypeArgs(ref matchedImplArgs, openImplementationArgs,
                        openImplementedArg.GetGenericParamsAndArgs(), closedServiceArg.GetGenericParamsAndArgs()))
                        return false; // nested match failed due either one of above reasons.
                }
            }

            return true;
        }

        private static Expression GetDependencyExpressionOrNull(IServiceInfo dependencyInfo, Request ownerRequest)
        {
            var request = ownerRequest.Push(dependencyInfo);
            var factory = dependencyInfo.Details.GetValue == null
                ? request.Registry.ResolveFactory(request)
                : new DelegateFactory(r => dependencyInfo.Details.GetValue(r)
                    .ThrowIfNotOf(dependencyInfo.ServiceType, Error.INJECTED_VALUE_IS_OF_DIFFERENT_TYPE, dependencyInfo, ownerRequest));

            var expr = factory == null ? null : factory.GetExpressionOrDefault(request);
            return expr ??
                (ownerRequest.IfUnresolved == IfUnresolved.ReturnDefault ? null
                : request.ServiceType.GetDefaultValueExpression());
        }

        #endregion
    }

    public sealed class ExpressionFactory : Factory
    {
        public ExpressionFactory(Func<Request, Expression> getServiceExpression, IReuse reuse = null, FactorySetup setup = null)
            : base(reuse, setup)
        {
            _getServiceExpression = getServiceExpression.ThrowIfNull();
        }

        public override Expression CreateExpressionOrDefault(Request request)
        {
            return _getServiceExpression(request);
        }

        private readonly Func<Request, Expression> _getServiceExpression;
    }

    /// <summary>
    /// This factory is the thin wrapper for user provided delegate 
    /// and where possible it uses delegate directly: without converting it to expression.
    /// </summary>
    public sealed class DelegateFactory : Factory
    {
        public DelegateFactory(Func<Request, object> factoryDelegate, IReuse reuse = null, FactorySetup setup = null)
            : base(reuse, setup)
        {
            _factoryDelegate = factoryDelegate.ThrowIfNull();
        }

        public override Expression CreateExpressionOrDefault(Request request)
        {
            var factoryDelegateExpr = request.State.GetOrAddItemExpression(_factoryDelegate);
            var requestExpr = request.State.GetOrAddItemExpression(request);
            return Expression.Convert(Expression.Invoke(factoryDelegateExpr, requestExpr), request.ServiceType);
        }

        public override FactoryDelegate GetDelegateOrDefault(Request request)
        {
            request = request.ResolveTo(this);

            if (request.Registry.GetDecoratorExpressionOrDefault(request) != null)
                return base.GetDelegateOrDefault(request);

            if (Reuse == null)
                return (items, _) => _factoryDelegate(request);

            var reuseScope = Reuse.GetScope(request);
            var scopeIndex = reuseScope == request.ResolutionScope ? -1 : request.State.GetOrAddItem(reuseScope);

            return (items, scope) => (scopeIndex == -1 ? scope : (Scope)items.Get(scopeIndex))
                .GetOrAdd(FactoryID, () => _factoryDelegate(request));
        }

        private readonly Func<Request, object> _factoryDelegate;
    }

    /// <summary>
    /// Creates/provides <see cref="Factory"/> based on <see cref="Request"/> for enabling context-dependent scenarios.
    /// </summary>
    public sealed class FactoryProvider : Factory
    {
        public override bool ProvidesFactoryForRequest { get { return true; } }

        public FactoryProvider(Func<Request, Factory> provideFactoryOrDefault, FactorySetup setup = null)
            : base(setup: setup)
        {
            _provideFactoryOrDefault = provideFactoryOrDefault.ThrowIfNull();
        }

        public override Factory GetFactoryForRequestOrDefault(Request request)
        {
            var factory = _provideFactoryOrDefault(request);
            if (factory != null && factory.Setup == DryIoc.Setup.Default)
                factory.Setup = Setup; // propagate provider setup if it is not specified by client.
            return factory;
        }

        public override Expression CreateExpressionOrDefault(Request request)
        {
            throw new NotSupportedException();
        }

        private readonly Func<Request, Factory> _provideFactoryOrDefault;
    }

    /// <summary>
    /// Lazy object storage that will create object with provided factory on first access, 
    /// then will be returning the same object for subsequent access.
    /// </summary>
    public interface IScope
    {
        /// <summary>Creates, stores, and returns stored object.</summary>
        /// <param name="id">Unique ID to find created object in subsequent calls.</param>
        /// <param name="factory">Delegate to create object. It will be used immediately, and reference to delegate will Not be stored.</param>
        /// <returns>Created and stored object.</returns>
        object GetOrAdd(int id, Func<object> factory);
    }

    /// <summary>
    /// <see cref="IScope"/> implementation which will dispose stored <see cref="IDisposable"/> objects on its own dispose.
    /// Locking is used internally to ensure that object factory called only once.
    /// </summary>
    public sealed class Scope : IScope, IDisposable
    {
        /// <summary>
        /// <see cref="IScope.GetOrAdd"/> for purpose.
        /// Will throw <see cref="ContainerException"/> if scope is disposed.
        /// </summary>
        /// <param name="id">Unique ID to find created object in subsequent calls.</param>
        /// <param name="factory">Delegate to create object. It will be used immediately, and reference to delegate will Not be stored.</param>
        /// <returns>Created and stored object.</returns>
        public object GetOrAdd(int id, Func<object> factory)
        {
            if (_disposed == 1)
                Throw.Me(Error.SCOPE_IS_DISPOSED);

            lock (_syncRoot)
            {
                var item = _items.GetFirstValueByHashOrDefault(id);
                if (item == null)
                    Ref.Swap(ref _items, _ => _.AddOrUpdate(id, item = factory()));
                return item;
            }
        }

        /// <summary>Disposes all stored <see cref="IDisposable"/> objects and nullifies object storage.</summary>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
                return;

            lock (_syncRoot)
            {
                if (!_items.IsEmpty)
                    foreach (var item in _items.Enumerate().Select(x => x.Value).OfType<IDisposable>())
                        item.Dispose();
                _items = null;
            }
        }

        public bool IsDisposed
        {
            get { return _disposed == 1; }
        }

        #region Implementation

        private HashTree<int, object> _items = HashTree<int, object>.Empty;
        private int _disposed;

        // Sync root is required to create object only once. The same reason as for Lazy<T>.
        private readonly object _syncRoot = new object();

        #endregion
    }

    public interface IReuse
    {
        IScope GetScope(Request request);
    }

    public sealed class SingletonReuse : IReuse
    {
        public IScope GetScope(Request request)
        {
            return request.Registry.SingletonScope;
        }
    }

    public sealed class CurrentScopeReuse : IReuse
    {
        public IScope GetScope(Request request)
        {
            return request.Registry.CurrentScope;
        }
    }

    public sealed class ResolutionScopeReuse : IReuse
    {
        public IScope GetScope(Request request)
        {
            return request.GetOrCreateResolutionScope();
        }
    }

    public static partial class Reuse
    {
        public static readonly IReuse Transient = null; // no reuse.
        public static readonly IReuse Singleton = new SingletonReuse();
        public static readonly IReuse InCurrentScope = new CurrentScopeReuse();
        public static readonly IReuse InResolutionScope = new ResolutionScopeReuse();
    }

    public interface IReusedObjectBehavior
    {
        Type WrapperType { get; }
        object Wrap(object target);
        object Unwrap(object wrapped);
    }

    public static class ReusedObjectBehavior
    {
        public static readonly IReusedObjectBehavior ExternallyDisposable = new ExternallyDisposableBehavior();

        public sealed class ExternallyDisposableBehavior : IReusedObjectBehavior
        {
            public Type WrapperType { get { return typeof(ExplicitlyDisposableWrapper); } }

            public object Wrap(object target)
            {
                return new ExplicitlyDisposableWrapper((target as IDisposable).ThrowIfNull());
            }

            public object Unwrap(object wrapped)
            {
                return (wrapped as ExplicitlyDisposableWrapper).ThrowIfNull().Target;
            }
        }
    }

    public sealed class ExplicitlyDisposableWrapper
    {
        public ExplicitlyDisposableWrapper(IDisposable target)
        {
            _target = target;
            _targetType = _target.GetType();
        }

        public IDisposable Target
        {
            get 
            { 
                Throw.If(IsDisposed, Error.TARGET_WAS_ALREADY_DISPOSED, _targetType, typeof(ExplicitlyDisposableWrapper));
                return _target;
            }
        }

        public bool IsDisposed
        {
            get { return _disposed == 1; }
        }

        public void DisposeTarget()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
                return;
            _target.Dispose();
            _target = null;
        }

        private IDisposable _target;
        private Type _targetType; 
        private int _disposed;
    }

    public sealed class ExplicitlyDisposable<T> : IDisposable
    {
        public ExplicitlyDisposable(ExplicitlyDisposableWrapper source)
        {
            _wrapper = source.ThrowIfNull().ThrowIf(!(source.Target is T));
        }

        public T Target { get{ return (T)_wrapper.Target; } }
        public bool IsDisposed { get { return _wrapper.IsDisposed; } }

        public void Dispose()
        {
            _wrapper.DisposeTarget();
        }

        private readonly ExplicitlyDisposableWrapper _wrapper;
    }

    public enum IfUnresolved { Throw, ReturnDefault }

    /// <summary>
    /// Declares minimal API for service resolution.
    /// The user friendly convenient methods are implemented as extension methods in <see cref="Resolver"/> class.
    /// </summary>
    /// <remarks>Resolve default and keyed is separated because of micro optimization for faster resolution.</remarks>
    public interface IResolver
    {
        /// <summary>Resolves service from container and returns created service object.</summary>
        /// <param name="serviceType">Service type to search and to return.</param>
        /// <param name="ifUnresolved">Says what to do if service is unresolved.</param>
        /// <param name="parentOrDefault">Parent request for dependency, or null for resolution root.</param>
        /// <returns>Created service object or default based on <paramref name="ifUnresolved"/> provided.</returns>
        object ResolveDefault(Type serviceType, IfUnresolved ifUnresolved, Request parentOrDefault);

        /// <summary>Resolves service from container and returns created service object.</summary>
        /// <param name="serviceType">Service type to search and to return.</param>
        /// <param name="serviceKey">Optional service key used for registering service.</param>
        /// <param name="ifUnresolved">Says what to do if service is unresolved.</param>
        /// <param name="requiredServiceType">Actual registered service type to use instead of <paramref name="serviceType"/>, 
        /// or wrapped type for generic wrappers.  The type should be assignable to return <paramref name="serviceType"/>.</param>
        /// <param name="parentOrDefault">Parent request for dependency, or null for resolution root.</param>
        /// <returns>Created service object or default based on <paramref name="ifUnresolved"/> provided.</returns>
        /// <remarks>
        /// This method covers all possible resolution input parameters comparing to <see cref="ResolveDefault"/>, and
        /// by specifying the same parameters as for <see cref="ResolveDefault"/> should return the same result.
        /// </remarks>
        object ResolveKeyed(Type serviceType, object serviceKey, IfUnresolved ifUnresolved, Type requiredServiceType, Request parentOrDefault);

        /// <summary>
        /// For given instance resolves and sets properties and fields.
        /// It respects <see cref="DryIoc.Rules.PropertiesAndFields"/> rules set per container, 
        /// or if rules are not set it uses default rule <see cref="PropertiesAndFields.AllPublicNonPrimitive"/>, 
        /// or you can specify your own rules with <paramref name="selectPropertiesAndFields"/> parameter.
        /// </summary>
        /// <param name="instance">Service instance with properties to resolve and initialize.</param>
        /// <param name="selectPropertiesAndFields">(optional) Function to select properties and fields, overrides all other rules if specified.</param>
        /// <param name="parentOrDefault">Parent request for dependency, or null for resolution root.</param>
        /// <remarks>Different Rules could be combined together using <see cref="PropertiesAndFields.OverrideWith"/> method.</remarks>        
        void ResolvePropertiesAndFields(object instance, PropertiesAndFieldsSelector selectPropertiesAndFields, Request parentOrDefault);
    }

    public enum IfAlreadyRegistered { ThrowIfDuplicateKey, KeepRegistered, UpdateRegistered }

    public interface IRegistrator
    {
        void Register(Factory factory, Type serviceType, object serviceKey, IfAlreadyRegistered ifAlreadyRegistered);

        bool IsRegistered(Type serviceType, object serviceKey, FactoryType factoryType, Func<Factory, bool> condition);

        void Unregister(Type serviceType, object serviceKey, FactoryType factoryType, Func<Factory, bool> condition);
    }

    public interface IRegistry : IResolver, IRegistrator
    {
        int RegistryID { get; }

        Rules Rules { get; }

        IScope CurrentScope { get; }
        IScope SingletonScope { get; }

        Factory ResolveFactory(Request request);

        Factory GetServiceFactoryOrDefault(Type serviceType, object serviceKey);
        Factory GetGenericWrapperOrDefault(Type openGenericServiceType);

        Expression GetDecoratorExpressionOrDefault(Request request);

        IEnumerable<KV<object, Factory>> GetAllServiceFactories(Type serviceType);

        /// <summary>
        /// If <paramref name="type"/> is generic type then this method checks if the type registered as generic wrapper,
        /// and recursively unwraps and returns its type argument. This type argument is the actual service type we want to find.
        /// Otherwise, method returns the input <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Type to unwrap. Method will return early if type is not generic.</param>
        /// <returns>Unwrapped service type in case it corresponds to registered generic wrapper, or input type in all other cases.</returns>
        Type GetWrappedServiceType(Type type);
    }

    public sealed class Many<TService>
    {
        public readonly IEnumerable<TService> Items;

        public Many(IEnumerable<TService> items)
        {
            Items = items.ThrowIfNull();
        }
    }

    public sealed class Meta<TService, TMetadata>
    {
        public readonly TService Value;

        public readonly TMetadata Metadata;

        public Meta(TService service, TMetadata metadata)
        {
            Value = service;
            Metadata = metadata;
        }
    }

    [DebuggerDisplay("{Expression}")]
    public sealed class DebugExpression<TService>
    {
        public readonly Expression<FactoryDelegate> Expression;

        public DebugExpression(Expression<FactoryDelegate> expression)
        {
            Expression = expression;
        }
    }

    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
    public class ContainerException : InvalidOperationException
    {
        public ContainerException(string message) : base(message) { }
    }

    /// <summary>Enables more clean exception message formatting, and exception throwing.</summary>
    public static partial class Throw
    {
        public static Func<string, Exception> GetException = message => new ContainerException(message);

        public static Func<object, string> PrintArg = x => new StringBuilder().Print(x).ToString();

        public static T ThrowIf<T>(this T arg0, bool throwCondition, string message = null, object arg1 = null, object arg2 = null)
        {
            if (!throwCondition) return arg0;
            throw GetException(message == null
                ? Format(ARGUMENT_HAS_IMVALID_CONDITION, arg0, typeof(T))
                : Format(message, arg0, arg1, arg2));
        }

        public static T ThrowIfNull<T>(this T arg, string message = null, object arg0 = null, object arg1 = null, object arg2 = null) where T : class
        {
            if (arg != null) return arg;
            throw GetException(message == null ? Format(ARGUMENT_IS_NULL, typeof(T)) : Format(message, arg0, arg1, arg2));
        }

        public static T ThrowIfNotOf<T>(this T arg0, Type type, string message = null, object arg1 = null, object arg2 = null) where T : class
        {
            if (type.IsTypeOf(arg0)) return arg0;
            throw GetException(message == null ? Format(ARG_IS_NOT_OF_TYPE, arg0, type) : Format(message, arg0, arg1, arg2));
        }

        public static Type ThrowIfNotOf(this Type arg0, Type type, string message = null, object arg1 = null, object arg2 = null)
        {
            if (type.IsAssignableTo(arg0)) return arg0;
            throw GetException(message == null ? Format(TYPE_ARG_IS_NOT_OF_TYPE, arg0, type) : Format(message, arg0, arg1, arg2));
        }

        public static void If(bool throwCondition, string message, object arg0 = null, object arg1 = null, object arg2 = null)
        {
            if (!throwCondition) return;
            throw GetException(Format(message, arg0, arg1, arg2));
        }

        public static void Me(string message, object arg0 = null, object arg1 = null, object arg2 = null)
        {
            throw GetException(Format(message, arg0, arg1, arg2));
        }

        public static T No<T>(string message, object arg0 = null, object arg1 = null, object arg2 = null)
        {
            throw GetException(Format(message, arg0, arg1, arg2));
        }

        private static string Format(this string message, object arg0 = null, object arg1 = null, object arg2 = null)
        {
            return string.Format(message, PrintArg(arg0), PrintArg(arg1), PrintArg(arg2));
        }

        private static readonly string ARGUMENT_IS_NULL = "Argument of type {0} is null.";
        private static readonly string ARGUMENT_HAS_IMVALID_CONDITION = "Argument {0} of type {1} has invalid condition.";
        private static readonly string ARG_IS_NOT_OF_TYPE = "Argument {0} is not of type {1}.";
        private static readonly string TYPE_ARG_IS_NOT_OF_TYPE = "Argument type {0} is not assignable from type {1}.";
    }

    /// <summary>
    /// Contains helper methods to work with Type: for instance to find Type implemented base types and interfaces, etc.  
    /// </summary>
    public static class TypeTools
    {
        /// <summary> Flags for <see cref="GetImplementedTypes"/> method.</summary>
        [Flags]
        public enum IncludeFlags { None = 0, SourceType = 1, ObjectType = 2 }

        /// <summary>
        /// Returns all interfaces and all base types (in that order) implemented by <paramref name="sourceType"/>.
        /// Specify <paramref name="includeFlags"/> to include <paramref name="sourceType"/> itself as first item and 
        /// <see cref="object"/> type as the last item.
        /// </summary>
        /// <param name="sourceType">Source type for discovery.</param>
        /// <param name="includeFlags">Additional types to include into result collection.</param>
        /// <returns>Collection of found types.</returns>
        public static Type[] GetImplementedTypes(this Type sourceType, IncludeFlags includeFlags = IncludeFlags.None)
        {
            Type[] results;

            var interfaces = sourceType.GetImplementedInterfaces();
            var interfaceStartIndex = (includeFlags & IncludeFlags.SourceType) == 0 ? 0 : 1;
            var includingObjectType = (includeFlags & IncludeFlags.ObjectType) == 0 ? 0 : 1;
            var sourcePlusInterfaceCount = interfaceStartIndex + interfaces.Length;

            var baseType = sourceType.GetTypeInfo().BaseType;
            if (baseType == null || baseType == typeof(object))
                results = new Type[sourcePlusInterfaceCount + includingObjectType];
            else
            {
                List<Type> baseBaseTypes = null;
                for (var bb = baseType.GetTypeInfo().BaseType; bb != null && bb != typeof(object); bb = bb.GetTypeInfo().BaseType)
                    (baseBaseTypes ?? (baseBaseTypes = new List<Type>(2))).Add(bb);

                if (baseBaseTypes == null)
                    results = new Type[sourcePlusInterfaceCount + includingObjectType + 1];
                else
                {
                    results = new Type[sourcePlusInterfaceCount + baseBaseTypes.Count + includingObjectType + 1];
                    baseBaseTypes.CopyTo(results, sourcePlusInterfaceCount + 1);
                }

                results[sourcePlusInterfaceCount] = baseType;
            }

            if (interfaces.Length == 1)
                results[interfaceStartIndex] = interfaces[0];
            else if (interfaces.Length > 1)
                Array.Copy(interfaces, 0, results, interfaceStartIndex, interfaces.Length);

            if (interfaceStartIndex == 1)
                results[0] = sourceType;
            if (includingObjectType == 1)
                results[results.Length - 1] = typeof(object);

            return results;
        }

        /// <summary>Gets a collection of the interfaces implemented by the current type and its base types.</summary>
        /// <param name="type">Source type</param>
        /// <returns>Collection of interface types.</returns>
        public static Type[] GetImplementedInterfaces(this Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces.ToArrayOrSelf();
        }

        /// <summary>
        /// Returns true if <paramref name="type"/> contains all generic parameters from <paramref name="genericParameters"/>.
        /// </summary>
        /// <param name="type">Expected to be open-generic type.</param>
        /// <param name="genericParameters">Generic parameter type to look in.</param>
        /// <returns></returns>
        public static bool ContainsAllGenericParameters(this Type type, Type[] genericParameters)
        {
            if (!type.IsOpenGeneric())
                return false;

            var paramNames = new string[genericParameters.Length];
            for (var i = 0; i < genericParameters.Length; i++)
                paramNames[i] = genericParameters[i].Name;

            NullifyNamesFoundInGenericParameters(paramNames, type.GetGenericParamsAndArgs());

            for (var i = 0; i < paramNames.Length; i++)
                if (paramNames[i] != null)
                    return false;
            return true;
        }

        public static bool IsGeneric(this Type type)
        {
            return type.GetTypeInfo().IsGenericType;
        }

        public static bool IsGenericDefinition(this Type type)
        {
            return type.GetTypeInfo().IsGenericTypeDefinition;
        }

        public static bool IsClosedGeneric(this Type type)
        {
            return type.GetTypeInfo().IsGenericType && !type.GetTypeInfo().ContainsGenericParameters;
        }

        public static bool IsOpenGeneric(this Type type)
        {
            return type.GetTypeInfo().IsGenericType && type.GetTypeInfo().ContainsGenericParameters;
        }

        public static Type GetGenericDefinitionOrNull(this Type type)
        {
            return type != null && type.GetTypeInfo().IsGenericType ? type.GetGenericTypeDefinition() : null;
        }

        public static Type[] GetGenericParamsAndArgs(this Type type)
        {
            return _getGenericArgumentsDelegate(type);
        }

        public static Type GetElementTypeOrNull(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsArray ? typeInfo.GetElementType() : null;
        }

        public static Type GetBaseType(this Type type)
        {
            return type.GetTypeInfo().BaseType;
        }

        public static bool IsPublicOrNestedPublic(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsPublic || typeInfo.IsNestedPublic && typeInfo.DeclaringType.IsPublicOrNestedPublic();
        }

        public static bool IsValueType(this Type type)
        {
            return type.GetTypeInfo().IsValueType;
        }

        public static bool IsAbstract(this Type type)
        {
            return type.GetTypeInfo().IsAbstract;
        }

        public static bool IsEnum(this Type type)
        {
            return type.GetTypeInfo().IsEnum;
        }

        public static bool IsAssignableTo(this Type type, Type other)
        {
            return type != null && other != null && other.GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());
        }

        public static bool IsTypeOf(this Type type, object obj)
        {
            return obj != null && obj.GetType().IsAssignableTo(type);
        }

        public static bool IsPrimitiveOrObjectOrString(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsPrimitive || type == typeof(string) || type == typeof(object) ||
                   typeInfo.IsArray && typeInfo.GetElementType().IsPrimitiveOrObjectOrString();
        }

        public static Attribute[] GetAttributes(this Type type, Type attributeType = null, bool inherit = false)
        {
            return type.GetTypeInfo().GetCustomAttributes(attributeType ?? typeof(Attribute), inherit)
                .Cast<Attribute>().ToArrayOrSelf();
        }

        public static IEnumerable<T> GetAll<T>(this Type type, Func<TypeInfo, IEnumerable<T>> getDeclared)
        {
            var typeInfo = type.GetTypeInfo();
            var declared = getDeclared(typeInfo);
            var baseType = typeInfo.BaseType;
            return baseType == null || baseType == typeof(object) ? declared
                : declared.Concat(baseType.GetAll(getDeclared));
        }

        public static IEnumerable<ConstructorInfo> GetAllConstructors(this Type type, bool includeNonPublic = false)
        {
            var all = type.GetTypeInfo().DeclaredConstructors;
            return includeNonPublic ? all : all.Where(c => c.IsPublic);
        }

        public static ConstructorInfo GetConstructorWithParameters(this Type type, Type[] parameters, bool includeNonPublic = false)
        {
            return type.GetAllConstructors(includeNonPublic)
                .FirstOrDefault(c => c.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameters))
                .ThrowIfNull();
        }

        public static ConstructorInfo GetFirstConstructor(this Type type, bool includeNonPublic = false)
        {
            return type.GetAllConstructors(includeNonPublic).FirstOrDefault().ThrowIfNull();
        }

        public static MethodInfo GetDeclaredMethod(this Type type, string name)
        {
            return type.GetTypeInfo().DeclaredMethods.FirstOrDefault(m => m.Name == name).ThrowIfNull();
        }

        public static MethodInfo GetDeclaredMethod(this Type type, string name, Type[] parameters)
        {
            parameters.ThrowIfNull();
            return type.GetTypeInfo().DeclaredMethods.FirstOrDefault(m =>
                m.Name == name && m.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameters))
                .ThrowIfNull();
        }

        public static PropertyInfo GetPropertyOrNull(this Type type, string name)
        {
            return type.GetAll(_ => _.DeclaredProperties).FirstOrDefault(p => p.Name == name);
        }

        public static FieldInfo GetFieldOrNull(this Type type, string name)
        {
            return type.GetAll(_ => _.DeclaredFields).FirstOrDefault(p => p.Name == name);
        }

        public static Assembly GetAssembly(this Type type) { return type.GetTypeInfo().Assembly; }

        #region Implementation

        private static readonly Func<Type, Type[]> _getGenericArgumentsDelegate =
            ExpressionTools.GetMethodDelegate<Type, Type[]>("GetGenericArguments");

        private static void NullifyNamesFoundInGenericParameters(string[] names, Type[] genericParameters)
        {
            for (var i = 0; i < genericParameters.Length; i++)
            {
                var sourceTypeArg = genericParameters[i];
                if (sourceTypeArg.IsGenericParameter)
                {
                    var matchingTargetArgIndex = Array.IndexOf(names, sourceTypeArg.Name);
                    if (matchingTargetArgIndex != -1)
                        names[matchingTargetArgIndex] = null;
                }
                else if (sourceTypeArg.IsOpenGeneric())
                    NullifyNamesFoundInGenericParameters(names, sourceTypeArg.GetGenericParamsAndArgs());
            }
        }

        #endregion
    }

    public static class ArrayTools
    {
        public static T[] ToArrayOrSelf<T>(this IEnumerable<T> source)
        {
            if (source is T[])
                return (T[])source;
            return source.ToArray();
        }

        public static T[] Append<T>(this T[] source, params T[] added)
        {
            if (added == null || added.Length == 0)
                return source;
            if (source == null || source.Length == 0)
                return added;
            var result = new T[source.Length + added.Length];
            Array.Copy(source, 0, result, 0, source.Length);
            if (added.Length == 1)
                result[source.Length] = added[0];
            else
                Array.Copy(added, 0, result, source.Length, added.Length);
            return result;
        }

        public static T[] AppendOrUpdate<T>(this T[] source, T value, int index = -1)
        {
            if (source == null || source.Length == 0)
                return new[] { value };
            var sourceLength = source.Length;
            index = index < 0 ? sourceLength : index;
            var result = new T[index < sourceLength ? sourceLength : sourceLength + 1];
            Array.Copy(source, result, sourceLength);
            result[index] = value;
            return result;
        }

        public static int IndexOf<T>(this T[] source, Func<T, bool> predicate)
        {
            if (source == null || source.Length == 0)
                return -1;
            for (var i = 0; i < source.Length; ++i)
                if (predicate(source[i]))
                    return i;
            return -1;
        }

        public static T[] RemoveAt<T>(this T[] source, int index)
        {
            if (source == null || source.Length == 0 || index < 0 || index >= source.Length)
                return source;
            if (index == 0 && source.Length == 1)
                return new T[0];
            var result = new T[source.Length - 1];
            if (index != 0)
                Array.Copy(source, 0, result, 0, index);
            if (index != result.Length)
                Array.Copy(source, index + 1, result, index, result.Length - index);
            return result;
        }

        public static T[] Remove<T>(this T[] source, T value)
        {
            return source.RemoveAt(source.IndexOf(x => Equals(x, value)));
        }

        public static R GetFirstNonDefault<T, R>(this T[] source, Func<T, R> selector)
        {
            var result = default(R);
            if (source != null && source.Length != 0)
                for (var i = 0; i < source.Length && Equals(result, default(R)); i++)
                    result = selector(source[i]);
            return result;
        }
    }

    public static class PrintTools
    {
        public readonly static string DEFAULT_ITEM_SEPARATOR = ";" + Environment.NewLine;

        public static StringBuilder Print(this StringBuilder s, object x,
            string quote = null, string itemSeparator = null, Func<Type, string> getTypeName = null)
        {
            return x == null ? s.Append("null")
                : x is string ? s.Print((string)x, quote)
                : x is Type ? s.Print((Type)x, getTypeName)
                : x is IEnumerable<Type> || x is IEnumerable
                    ? s.Print((IEnumerable)x, itemSeparator ?? DEFAULT_ITEM_SEPARATOR, (_, o) => _.Print(o, quote, null, getTypeName))
                : s.Append(x);
        }

        /// <summary>Appends string to string builder quoting with <paramref name="quote"/> if provided.</summary>
        /// <param name="s">String builder to append string to.</param>
        /// <param name="str">String to print.</param>
        /// <param name="quote">(optional) Quote to add before and after string.</param>
        /// <returns>String builder with appended string.</returns>
        public static StringBuilder Print(this StringBuilder s, string str, string quote = null)
        {
            return quote == null ? s.Append(str) : s.Append(quote).Append(str).Append(quote);
        }

        public static StringBuilder Print(this StringBuilder s, IEnumerable items,
            string separator = ", ", Action<StringBuilder, object> printItem = null)
        {
            if (items == null) return s;
            printItem = printItem ?? ((_, x) => _.Print(x));
            var itemCount = 0;
            foreach (var item in items)
                printItem(itemCount++ == 0 ? s : s.Append(separator), item);
            return s;
        }

        public static readonly Func<Type, string> GetTypeNameDefault = t =>
            t.FullName != null && t.Namespace != "System" && !t.Namespace.StartsWith("System.") ? t.FullName : t.Name;

        public static StringBuilder Print(this StringBuilder s, Type type, Func<Type, string> getTypeName = null)
        {
            if (type == null) return s;

            getTypeName = getTypeName ?? GetTypeNameDefault;
            var typeName = getTypeName(type);

            var isArray = type.IsArray;
            if (isArray)
                type = type.GetElementType();

            if (!type.IsGeneric())
                return s.Append(typeName.Replace('+', '.'));

            s.Append(typeName.Substring(0, typeName.IndexOf('`')).Replace('+', '.')).Append('<');

            var genericArgs = type.GetGenericParamsAndArgs();
            if (type.IsGenericDefinition())
                s.Append(',', genericArgs.Length - 1);
            else
                s.Print(genericArgs, ", ", (_, t) => _.Print((Type)t, getTypeName));

            s.Append('>');

            if (isArray)
                s.Append("[]");

            return s;
        }
    }

    public static class ExpressionTools
    {
        public static Func<TInstance, TReturn> GetMethodDelegate<TInstance, TReturn>(string methodName)
        {
            var methodInfo = typeof(TInstance).GetDeclaredMethod(methodName, new Type[0]);
            var thisParamExpr = Expression.Parameter(typeof(TInstance), "_");
            var methodExpr = Expression.Lambda<Func<TInstance, TReturn>>(Expression.Call(thisParamExpr, methodInfo), thisParamExpr);
            return methodExpr.Compile();
        }

        public static Expression GetDefaultValueExpression(this Type type)
        {
            return Expression.Call(_getDefaultMethod.MakeGenericMethod(type), (Expression[])null);
        }

        private static readonly MethodInfo _getDefaultMethod = typeof(ExpressionTools).GetDeclaredMethod("GetDefault");
        internal static T GetDefault<T>() { return default(T); }
    }

    public sealed class KV<K, V>
    {
        public readonly K Key;
        public readonly V Value;

        public KV(K key, V value)
        {
            Key = key;
            Value = value;
        }

        public override string ToString()
        {
            return new StringBuilder("[").Print(Key).Append(", ").Print(Value).Append("]").ToString();
        }

        public override bool Equals(object obj)
        {
            var other = obj as KV<K, V>;
            return other != null
                && (ReferenceEquals(other.Key, Key) || Equals(other.Key, Key))
                && (ReferenceEquals(other.Value, Value) || Equals(other.Value, Value));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((object)Key == null ? 0 : Key.GetHashCode() * 397)
                     ^ ((object)Value == null ? 0 : Value.GetHashCode());
            }
        }
    }

    public delegate V Update<V>(V oldValue, V newValue);
    public delegate bool ShouldUpdate<V>(V oldValue, out V updatedValue);

    /// <summary>
    /// Immutable kind of http://en.wikipedia.org/wiki/AVL_tree where actual node key is hash code of <typeparamref name="K"/>.
    /// </summary>
    public sealed class HashTree<K, V>
    {
        public static readonly HashTree<K, V> Empty = new HashTree<K, V>();

        public readonly K Key;
        public readonly V Value;

        public readonly int Hash;
        public readonly KV<K, V>[] Conflicts;
        public readonly HashTree<K, V> Left, Right;
        public readonly int Height;

        public bool IsEmpty { get { return Height == 0; } }

        public HashTree<K, V> AddOrUpdate(K key, V value, Update<V> update = null)
        {
            return AddOrUpdate(key.GetHashCode(), key, value, update);
        }

        public V GetValueOrDefault(K key, V defaultValue = default(V))
        {
            var t = this;
            var hash = key.GetHashCode();
            while (t.Height != 0 && t.Hash != hash)
                t = hash < t.Hash ? t.Left : t.Right;
            return t.Height != 0 && (ReferenceEquals(key, t.Key) || key.Equals(t.Key))
                ? t.Value : t.GetConflictedValueOrDefault(key, defaultValue);
        }

        public V GetFirstValueByHashOrDefault(int hash, V defaultValue = default(V))
        {
            var t = this;
            while (t.Height != 0 && t.Hash != hash)
                t = hash < t.Hash ? t.Left : t.Right;
            return t.Height != 0 ? t.Value : defaultValue;
        }

        /// <remarks>
        /// Depth-first in-order traversal as described in http://en.wikipedia.org/wiki/Tree_traversal
        /// The only difference is using fixed size array instead of stack for speed-up (~20% faster than stack).
        /// </remarks>
        public IEnumerable<KV<K, V>> Enumerate()
        {
            var parents = new HashTree<K, V>[Height];
            var parentCount = -1;
            var t = this;
            while (!t.IsEmpty || parentCount != -1)
            {
                if (!t.IsEmpty)
                {
                    parents[++parentCount] = t;
                    t = t.Left;
                }
                else
                {
                    t = parents[parentCount--];
                    yield return new KV<K, V>(t.Key, t.Value);
                    if (t.Conflicts != null)
                        for (var i = 0; i < t.Conflicts.Length; i++)
                            yield return t.Conflicts[i];
                    t = t.Right;
                }
            }
        }

        /// <remarks>
        /// Based on Eric Lippert http://blogs.msdn.com/b/ericlippert/archive/2008/01/21/immutability-in-c-part-nine-academic-plus-my-avl-tree-implementation.aspx
        /// </remarks>
        public HashTree<K, V> RemoveOrUpdate(K key, ShouldUpdate<V> updateInstead = null)
        {
            return RemoveOrUpdate(key.GetHashCode(), key, updateInstead);
        }

        public HashTree<K, V> Update(K key, V value)
        {
            return RemoveOrUpdate(key.GetHashCode(), key, (V _, out V newValue) =>
            {
                newValue = value;
                return true;
            });
        }

        #region Implementation

        private HashTree() { }

        private HashTree(int hash, K key, V value, KV<K, V>[] conficts, HashTree<K, V> left, HashTree<K, V> right)
        {
            Hash = hash;
            Key = key;
            Value = value;
            Conflicts = conficts;
            Left = left;
            Right = right;
            Height = 1 + (left.Height > right.Height ? left.Height : right.Height);
        }

        private HashTree<K, V> AddOrUpdate(int hash, K key, V value, Update<V> update)
        {
            return Height == 0 ? new HashTree<K, V>(hash, key, value, null, Empty, Empty)
                : (hash == Hash ? UpdateValueAndResolveConflicts(key, value, update)
                : (hash < Hash
                    ? With(Left.AddOrUpdate(hash, key, value, update), Right)
                    : With(Left, Right.AddOrUpdate(hash, key, value, update)))
                        .KeepBalanced());
        }

        private HashTree<K, V> UpdateValueAndResolveConflicts(K key, V value, Update<V> update)
        {
            if (ReferenceEquals(Key, key) || Key.Equals(key))
                return new HashTree<K, V>(Hash, key, update == null ? value : update(Value, value), Conflicts, Left, Right);

            if (Conflicts == null)
                return new HashTree<K, V>(Hash, Key, Value, new[] { new KV<K, V>(key, value) }, Left, Right);

            var i = Conflicts.Length - 1;
            while (i >= 0 && !Equals(Conflicts[i].Key, Key)) i--;
            var conflicts = new KV<K, V>[i != -1 ? Conflicts.Length : Conflicts.Length + 1];
            Array.Copy(Conflicts, 0, conflicts, 0, Conflicts.Length);
            conflicts[i != -1 ? i : Conflicts.Length] =
                new KV<K, V>(key, i != -1 && update != null ? update(Conflicts[i].Value, value) : value);
            return new HashTree<K, V>(Hash, Key, Value, conflicts, Left, Right);
        }

        private V GetConflictedValueOrDefault(K key, V defaultValue)
        {
            if (Conflicts != null)
                for (var i = 0; i < Conflicts.Length; i++)
                    if (Equals(Conflicts[i].Key, key))
                        return Conflicts[i].Value;
            return defaultValue;
        }

        private HashTree<K, V> RemoveOrUpdate(int hash, K key, ShouldUpdate<V> updateInstead = null, bool ignoreKey = false)
        {
            if (Height == 0)
                return this;

            HashTree<K, V> result;
            if (hash == Hash) // found matched Node
            {
                if (ignoreKey || Equals(Key, key))
                {
                    if (!ignoreKey)
                    {
                        V updatedValue;
                        if (updateInstead != null && updateInstead(Value, out updatedValue))
                            return new HashTree<K, V>(Hash, Key, updatedValue, Conflicts, Left, Right);

                        if (Conflicts != null)
                        {
                            if (Conflicts.Length == 1)
                                return new HashTree<K, V>(Hash, Conflicts[0].Key, Conflicts[0].Value, null, Left, Right);
                            var shrinkedConflicts = new KV<K, V>[Conflicts.Length - 1];
                            Array.Copy(Conflicts, 1, shrinkedConflicts, 0, shrinkedConflicts.Length);
                            return new HashTree<K, V>(Hash, Conflicts[0].Key, Conflicts[0].Value, shrinkedConflicts, Left, Right);
                        }
                    }

                    if (Height == 1) // remove node
                        return Empty;

                    if (Right.IsEmpty)
                        result = Left;
                    else if (Left.IsEmpty)
                        result = Right;
                    else
                    {
                        // we have two children, so remove the next highest node and replace this node with it.
                        var successor = Right;
                        while (!successor.Left.IsEmpty) successor = successor.Left;
                        result = successor.With(Left, Right.RemoveOrUpdate(successor.Hash, default(K), ignoreKey: true));
                    }
                }
                else if (Conflicts != null)
                {
                    var index = Conflicts.Length - 1;
                    while (index >= 0 && !Equals(Conflicts[index].Key, key)) --index;
                    if (index == -1)        // key is not found in conflicts - just return
                        return this;

                    V updatedValue;
                    var conflict = Conflicts[index];
                    if (updateInstead != null && updateInstead(conflict.Value, out updatedValue))
                    {
                        var updatedConflicts = new KV<K, V>[Conflicts.Length];
                        Array.Copy(Conflicts, 0, updatedConflicts, 0, updatedConflicts.Length);
                        updatedConflicts[index] = new KV<K, V>(conflict.Key, updatedValue);
                        return new HashTree<K, V>(Hash, Key, Value, updatedConflicts, Left, Right);
                    }

                    if (Conflicts.Length == 1)
                        return new HashTree<K, V>(Hash, Key, Value, null, Left, Right);
                    var shrinkedConflicts = new KV<K, V>[Conflicts.Length - 1];
                    var newIndex = 0;
                    for (var i = 0; i < Conflicts.Length; ++i)
                        if (i != index) shrinkedConflicts[newIndex++] = Conflicts[i];
                    return new HashTree<K, V>(Hash, Key, Value, shrinkedConflicts, Left, Right);
                }
                else return this; // if key is not matching and no conflicts to lookup - just return
            }
            else if (hash < Hash)
                result = With(Left.RemoveOrUpdate(hash, key, updateInstead, ignoreKey), Right);
            else
                result = With(Left, Right.RemoveOrUpdate(hash, key, updateInstead, ignoreKey));
            return result.KeepBalanced();
        }

        private HashTree<K, V> KeepBalanced()
        {
            var delta = Left.Height - Right.Height;
            return delta >= 2 ? With(Left.Right.Height - Left.Left.Height == 1 ? Left.RotateLeft() : Left, Right).RotateRight()
                : (delta <= -2 ? With(Left, Right.Left.Height - Right.Right.Height == 1 ? Right.RotateRight() : Right).RotateLeft()
                : this);
        }

        private HashTree<K, V> RotateRight()
        {
            return Left.With(Left.Left, With(Left.Right, Right));
        }

        private HashTree<K, V> RotateLeft()
        {
            return Right.With(With(Left, Right.Left), Right.Right);
        }

        private HashTree<K, V> With(HashTree<K, V> left, HashTree<K, V> right)
        {
            return new HashTree<K, V>(Hash, Key, Value, Conflicts, left, right);
        }

        #endregion
    }

    public static class Ref
    {
        public static Ref<T> Of<T>(T value = default(T)) where T : class
        {
            return new Ref<T>(value);
        }

        /// <remarks>
        /// First, it evaluates new value using <paramref name="getValue"/> function. 
        /// Second, it checks that original value is not changed. 
        /// If it is changed it will retry first step, otherwise it assigns new value and returns original (the one used for <paramref name="getValue"/>).
        /// </remarks>
        public static T Swap<T>(ref T value, Func<T, T> getValue) where T : class
        {
            var retryCount = 0;
            while (true)
            {
                var oldValue = value;
                var newValue = getValue(oldValue);
                if (Interlocked.CompareExchange(ref value, newValue, oldValue) == oldValue)
                    return oldValue;
                if (++retryCount > RETRY_COUNT_UNTIL_THROW)
                    throw new InvalidOperationException(ERROR_RETRY_COUNT_EXCEEDED);
            }
        }

        private const int RETRY_COUNT_UNTIL_THROW = 50;
        private static readonly string ERROR_RETRY_COUNT_EXCEEDED =
            "Ref retried to Update for " + RETRY_COUNT_UNTIL_THROW + " times But there is always someone else intervened.";
    }

    public sealed class Ref<T> where T : class
    {
        public T Value { get { return _value; } }

        public Ref(T initialValue = default(T))
        {
            _value = initialValue;
        }

        public T Swap(Func<T, T> getValue)
        {
            return Ref.Swap(ref _value, getValue);
        }

        private T _value;
    }
}