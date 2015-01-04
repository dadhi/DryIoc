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

namespace DryIoc
{
    using System.Threading;

    public static partial class Portable
    {
        static partial void GetCurrentManagedThreadID(ref int threadID)
        {
            threadID = Thread.CurrentThread.ManagedThreadId;
        }
    }
}

namespace System.Reflection
{
    using Collections.Generic;
    using Linq;

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

        public Assembly Assembly { get { return _type.Assembly; } }

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

        public IEnumerable<Attribute> GetCustomAttributes(Type attributeType, bool inherit)
        {
            return _type.GetCustomAttributes(attributeType, inherit).Cast<Attribute>();
        }

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
        public bool IsAbstract { get { return _type.IsAbstract; } }
        public bool IsEnum { get { return _type.IsEnum; } }

        public Type GetElementType() { return _type.GetElementType(); }

        public bool IsAssignableFrom(TypeInfo typeInfo) { return _type.IsAssignableFrom(typeInfo.AsType()); }

        private readonly Type _type;

        private const BindingFlags ALL_DECLARED =
            BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.DeclaredOnly;
    }
}
