using System;

namespace Web.IocDi
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ResolveAsSelfAttribute : Attribute { }
}
