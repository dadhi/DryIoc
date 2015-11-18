namespace System.Reflection
{
    /// <summary>Provides <see cref="GetTypeInfo"/> for the type.</summary>
    public static class TypeInfoTools
    {
        /// <summary>Wraps input type into <see cref="TypeInfo"/> structure.</summary>
        /// <param name="type">Input type.</param> <returns>Type info wrapper.</returns>
        public static TypeInfo GetTypeInfo(this Type type)
        {
            return new TypeInfo(type);
        }
    }

    /// <summary>Partial analog of TypeInfo existing in .NET 4.5 and higher.</summary>
    public struct TypeInfo
    {
        private Type _type;

        /// <summary>Creates type info by wrapping input type.</summary> <param name="type">Type to wrap.</param>
        public TypeInfo(Type type)
        {
            _type = type;
        }

        #pragma warning disable 1591 // "Missing XML-comment"

        public Type AsType() { return _type; }

        public bool IsAssignableFrom(TypeInfo from)
        {
            return _type.IsAssignableFrom(from.AsType());
        }

        #pragma warning restore 1591
    }

}
