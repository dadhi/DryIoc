using System;

namespace Web.IocDi
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class ResolveAsAttribute : Attribute
    {
        public ResolveAsAttribute(Type serviceType) { ServiceType = serviceType; }
        public Type ServiceType { get; private set; }
    }
}
