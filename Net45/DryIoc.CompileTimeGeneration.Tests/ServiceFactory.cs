using System;
using System.Collections.Generic;

namespace DryIoc.CompileTimeGeneration.Tests
{
    public partial class ServiceFactory : IResolverContext, IResolverContextProvider, IDisposable
    {
        public static HashTree<Type, FactoryDelegate>
            DefaultResolutions = HashTree<Type, FactoryDelegate>.Empty;

        public static HashTree<Type, HashTree<object, FactoryDelegate>>
            KeyedResolutions = HashTree<Type, HashTree<object, FactoryDelegate>>.Empty;

        public ServiceFactory()
        {
            SingletonScope = new Scope();
        }

        public IResolverContext Resolver
        {
            get { return this; }
        }

        public void Dispose()
        {
            SingletonScope.Dispose();
        }

        public IScope SingletonScope { get; private set; }

        public IScope GetCurrentNamedScope(object name, bool throwIfNotFound)
        {
            throw new NotImplementedException();
        }

        public IScope GetOrCreateResolutionScope(ref IScope scope, Type serviceType, object serviceKey)
        {
            return scope ?? (scope = new Scope(null, new KV<Type, object>(serviceType, serviceKey)));
        }

        public IScope GetMatchingResolutionScope(IScope scope, Type assignableFromServiceType, object serviceKey, bool outermost,
            bool throwIfNotFound)
        {
            throw new NotImplementedException();
        }

        #region IResolver

        public object ResolveDefault(Type serviceType, IfUnresolved ifUnresolved, IScope scope)
        {
            var factoryDelegate = DefaultResolutions.GetValueOrDefault(serviceType);
            return factoryDelegate != null
                ? factoryDelegate(AppendableArray.Empty, this, null)
                : GetDefaultOrThrowIfUnresolved(serviceType, ifUnresolved);
        }

        public object ResolveKeyed(Type serviceType, object serviceKey, IfUnresolved ifUnresolved,
            Type requiredServiceType, IScope scope)
        {
            if (serviceKey == null && requiredServiceType == null)
                return ResolveDefault(serviceType, ifUnresolved, scope);

            serviceType = requiredServiceType ?? serviceType;
            var resolutions = KeyedResolutions.GetValueOrDefault(serviceType);
            if (resolutions != null)
            {
                var factoryDelegate = resolutions.GetValueOrDefault(serviceKey);
                if (factoryDelegate != null)
                    return factoryDelegate(AppendableArray.Empty, this, null);
            }

            return GetDefaultOrThrowIfUnresolved(serviceType, ifUnresolved);
        }

        public IEnumerable<object> ResolveMany(Type serviceType, object serviceKey, Type requiredServiceType,
            object compositeParentKey, IScope scope)
        {
            serviceType = requiredServiceType ?? serviceType;

            var resolutions = KeyedResolutions.GetValueOrDefault(serviceType);
            if (resolutions != null)
            {
                if (serviceKey != null)
                {
                    var factoryDelegate = resolutions.GetValueOrDefault(serviceKey);
                    if (factoryDelegate != null)
                        yield return factoryDelegate(AppendableArray.Empty, this, scope);
                }
                else
                {
                    foreach (var resolution in resolutions.Enumerate())
                    {
                        var factoryDelegate = resolution.Value;
                        yield return factoryDelegate(AppendableArray.Empty, this, scope);
                    }
                }
            }
            else
            {
                var factoryDelegate = DefaultResolutions.GetValueOrDefault(serviceType);
                if (factoryDelegate != null)
                    yield return factoryDelegate(AppendableArray.Empty, this, scope);
            }
        }

        private static object GetDefaultOrThrowIfUnresolved(Type serviceType, IfUnresolved ifUnresolved)
        {
            return ifUnresolved == IfUnresolved.ReturnDefault
                ? null
                : Throw.For<object>(Error.UNABLE_TO_RESOLVE_SERVICE, serviceType);
        }

        #endregion
    }
}
