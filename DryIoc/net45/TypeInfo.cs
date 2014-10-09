using System.Collections.Generic;

namespace System.Reflection
{
    public static class TypeInfoTools
    {
        public static TypeInfo GetTypeInfo(this Type type)
        {
            return new TypeInfo(type);
        }
    }

    public struct TypeInfo
    {
        public TypeInfo(Type type)
        {
            _type = type;
        }

        public Type AsType() { return _type; }

        public IEnumerable<ConstructorInfo> DeclaredConstructors
        {
            get { return _type.GetConstructors(ALL_DECLARED ^ BindingFlags.Static); }
        }

        public IEnumerable<MethodInfo> DeclaredMethods
        {
            get { return _type.GetMethods(ALL_DECLARED); }
        }

        public IEnumerable<FieldInfo> DeclaredFields
        {
            get { return _type.GetFields(ALL_DECLARED); }
        }

        public IEnumerable<PropertyInfo> DeclaredProperties
        {
            get { return _type.GetProperties(ALL_DECLARED); }
        }

        public IEnumerable<Type> ImplementedInterfaces { get { return _type.GetInterfaces(); } }
        public Type BaseType { get { return _type.BaseType; } }
        public bool IsGenericType { get { return _type.IsGenericType; } }
        public bool IsGenericTypeDefinition { get { return _type.IsGenericTypeDefinition; } }
        public bool ContainsGenericParameters { get { return _type.ContainsGenericParameters; } }
        public bool IsValueType { get { return _type.IsValueType; } }
        public bool IsPrimitive { get { return _type.IsPrimitive; } }
        public bool IsArray { get { return _type.IsArray; } }
        public bool IsPublic { get { return _type.IsPublic; } }
        public bool IsNestedPublic { get { return _type.IsNestedPublic; } }
        public Type DeclaringType { get { return _type.DeclaringType; } }
        public bool IsAbstract { get { return _type.IsAbstract;  } }

        public Type GetElementType() { return _type.GetElementType(); }

        public bool IsAssignableFrom(TypeInfo typeInfo) { return _type.IsAssignableFrom(typeInfo.AsType()); }

        private readonly Type _type;

        private const BindingFlags ALL_DECLARED =
            BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.DeclaredOnly;
    }
}