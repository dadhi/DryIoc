using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;

namespace DryIoc.CommonServiceLocator
{
    public class DryIocServiceLocator : ServiceLocatorImplBase
    {
        public readonly Container Container;

        public DryIocServiceLocator(Container container)
        {
            if (container == null) throw new ArgumentNullException("container");
            Container = container;
        }

        protected override object DoGetInstance(Type serviceType, string key)
        {
            if (serviceType == null) throw new ArgumentNullException("serviceType");
            return Container.Resolve(serviceType, key);
        }

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            if (serviceType == null) throw new ArgumentNullException("serviceType");
            return Container.ResolveMany<object>(serviceType);
        }
    }
}
