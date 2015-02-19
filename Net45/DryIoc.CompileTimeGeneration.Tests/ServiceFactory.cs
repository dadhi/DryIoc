using System;
using System.Collections.Generic;

namespace DryIoc.CompileTimeGeneration.Tests
{
    public partial class ServiceFactory : IResolverWithScopes, IResolverProvider, IDisposable
    {
        public IScope SingletonScope { get; private set; }

        public IScope CurrentScope
        {
            get { throw new NotImplementedException(); }
        }

        public ServiceFactory()
        {
            SingletonScope = new Scope();
        }

        public void Dispose()
        {
            SingletonScope.Dispose();
        }

        public IResolverWithScopes Resolver
        {
            get { return this; }
        }

        public object ResolveDefault(Type serviceType, IfUnresolved ifUnresolved)
        {
            var factoryDelegate = DefaultResolutions.GetValueOrDefault(serviceType);
            return factoryDelegate == null 
                ? GetDefaultOrThrowIfUnresolved(serviceType, ifUnresolved)
                : factoryDelegate(AppendableArray.Empty, this, null);
        }

        public object ResolveKeyed(Type serviceType, object serviceKey, IfUnresolved ifUnresolved, Type requiredServiceType)
        {
            if (serviceKey == null && requiredServiceType == null)
                return ResolveDefault(serviceType, ifUnresolved);

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

        public IEnumerable<object> ResolveMany(Type serviceType, object serviceKey, Type requiredServiceType, object compositeParentKey)
        {
            serviceType = requiredServiceType ?? serviceType;

            var resolutions = KeyedResolutions.GetValueOrDefault(serviceType);
            if (resolutions != null)
            {
                if (serviceKey != null)
                {
                    var factoryDelegate = resolutions.GetValueOrDefault(serviceKey);
                    if (factoryDelegate != null)
                        yield return factoryDelegate(AppendableArray.Empty, this, null);
                }
                else
                {
                    foreach (var resolution in resolutions.Enumerate())
                    {
                        var factoryDelegate = resolution.Value;
                        yield return factoryDelegate(AppendableArray.Empty, this, null);
                    }
                }
            }
            else
            {
                var factoryDelegate = DefaultResolutions.GetValueOrDefault(serviceType);
                if (factoryDelegate != null)
                    yield return factoryDelegate(AppendableArray.Empty, this, null);
            }
        }

        public object ResolvePropertiesAndFields(object instance, PropertiesAndFieldsSelector selectPropertiesAndFields)
        {
            throw new NotImplementedException();
        }

        private static object GetDefaultOrThrowIfUnresolved(Type serviceType, IfUnresolved ifUnresolved)
        {
            return ifUnresolved == IfUnresolved.ReturnDefault ? null
                : Throw.Instead<object>(Error.UNABLE_TO_RESOLVE_SERVICE, serviceType);
        }
    }
}
