using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DryIoc.AttributedRegistration.UnitTests.CUT
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ExportWithMetadataAttribute : Attribute
    {
        public object Metadata { get; set; }

        public ExportWithMetadataAttribute(object metadata)
        {
            Metadata = metadata;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ExportAllAttribute : Attribute
    {
        public static Func<Type, bool> ExportedTypes = t => (t.IsPublic || t.IsNestedPublic) && t != typeof(object);

        public Type[] Except { get; set; }
        public string ContractName { get; set; }

        public IEnumerable<Type> SelectServiceTypes(Type targetType)
        {
            var serviceTypes = targetType.EnumerateSelfAndImplementedTypes().Where(ExportedTypes);
            return Except == null || Except.Length == 0 ? serviceTypes : serviceTypes.Except(Except);
        }
    }

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class ImportWithMetadataAttribute : Attribute
    {
        public ImportWithMetadataAttribute(object metadata)
        {
            Metadata = metadata;
        }

        public readonly object Metadata;
    }

    public static class Sugar
    {
        public static string Print(object x)
        {
            return x is string ? (string)x
                : (x is Type ? ((Type)x).Print()
                : (x is IEnumerable<Type> ? ((IEnumerable)x).Print(";" + Environment.NewLine)
                : (x is IEnumerable ? ((IEnumerable)x).Print()
                : (string.Empty + x))));
        }

        public static string Print(this IEnumerable items, string separator = ", ", Func<object, string> printItem = null)
        {
            if (items == null) return null;
            printItem = printItem ?? Print;
            var builder = new StringBuilder();
            foreach (var item in items)
                (builder.Length == 0 ? builder : builder.Append(separator)).Append(printItem(item));
            return builder.ToString();
        }

        public static string Print(this Type type, Func<Type, string> print = null /* prints Type.FullName by default */)
        {
            if (type == null) return null;
            var name = print == null ? type.FullName : print(type);
            if (type.IsGenericType) // for generic types
            {
                var genericArgs = type.GetGenericArguments();
                var genericArgsString = type.IsGenericTypeDefinition
                    ? new string(',', genericArgs.Length - 1)
                    : string.Join(", ", genericArgs.Select(x => x.Print(print)).ToArray());
                name = name.Substring(0, name.IndexOf('`')) + "<" + genericArgsString + ">";
            }
            return name.Replace('+', '.'); // for nested classes
        }

        public static Type[] EnumerateSelfAndImplementedTypes(this Type type)
        {
            Type[] results;

            var interfaces = type.GetInterfaces();
            var selfPlusInterfaceCount = 1 + interfaces.Length;

            var baseType = type.BaseType;
            if (baseType == null || baseType == typeof(object))
                results = new Type[selfPlusInterfaceCount];
            else
            {
                List<Type> baseBaseTypes = null;
                for (var bb = baseType.BaseType; bb != null && bb != typeof(object); bb = bb.BaseType)
                    (baseBaseTypes ?? (baseBaseTypes = new List<Type>(2))).Add(bb);

                if (baseBaseTypes == null)
                    results = new Type[selfPlusInterfaceCount + 1];
                else
                {
                    results = new Type[selfPlusInterfaceCount + 1 + baseBaseTypes.Count];
                    baseBaseTypes.CopyTo(results, selfPlusInterfaceCount + 1);
                }

                results[selfPlusInterfaceCount] = baseType;
            }

            results[0] = type;

            if (selfPlusInterfaceCount == 2)
                results[1] = interfaces[0];
            else if (selfPlusInterfaceCount > 2)
                Array.Copy(interfaces, 0, results, 1, interfaces.Length);

            if (results.Length > 1 && type.IsGenericTypeDefinition)
            {
                for (var i = 1; i < results.Length; i++)
                {
                    var interfaceOrBase = results[i];
                    if (interfaceOrBase.IsGenericType && !interfaceOrBase.IsGenericTypeDefinition &&
                        interfaceOrBase.ContainsGenericParameters)
                        results[i] = interfaceOrBase.GetGenericTypeDefinition();
                }
            }

            return results;
        }

        public static Type GetMemberType(this MemberInfo member)
        {
            var mt = member.MemberType;
            //mt.ThrowIf(mt != MemberTypes.Field && mt != MemberTypes.Property);
            return mt == MemberTypes.Field ? ((FieldInfo)member).FieldType : ((PropertyInfo)member).PropertyType;
        }

        public static V GetOrAdd<K, V>(this IDictionary<K, V> source, K key, Func<K, V> valueFactory)
        {
            V value;
            if (!source.TryGetValue(key, out value))
                source.Add(key, value = valueFactory(key));
            return value;
        }

        public static T[] Append<T>(this T[] source, params T[] added)
        {
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
            var sourceLength = source.Length;
            index = index < 0 ? sourceLength : index;
            var result = new T[index < sourceLength ? sourceLength : sourceLength + 1];
            Array.Copy(source, result, sourceLength);
            result[index] = value;
            return result;
        }
    }



}
