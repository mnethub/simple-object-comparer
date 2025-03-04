using System.Collections;
using System.Reflection;

namespace SimpleObjectComparer
{
    internal static class Extensions
    {
        public static bool IsSimpleType(this Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            return type.IsValueType || type == typeof(string);
        }

        public static bool IsComplexType(this Type type)
        {
            return type.IsClass &&
               !typeof(IEnumerable).IsAssignableFrom(type) &&
               !type.IsArray &&
               type != typeof(string) &&
               !typeof(Delegate).IsAssignableFrom(type);
        }

        public static bool IsCollectionOrArray(this Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type) || type.IsArray;
        }

        public static Type? GetUnderlyingType(this Type collectionType)
        {
            if (collectionType.IsGenericType)
            {
                var genericTypeDefinition = collectionType.GetGenericTypeDefinition();
                if (genericTypeDefinition == typeof(IEnumerable<>) ||
                    collectionType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
                {
                    return collectionType.GetGenericArguments()[0];
                }
            }
            else if (collectionType.IsArray)
                return collectionType.GetElementType();

            return null;
        }

        public static object? GetValueWithNull(this PropertyInfo? propertyInfo, object? value)
        {
            if (value == null)
                return null;

            return propertyInfo?.GetValue(value);
        }
    }

    public static class DeltaExtentions
    {
        public static HashSet<string> GetAllUniqueSimpleTypeKeys(this Delta delta)
        {
            var uniqueKeys = new HashSet<string>(delta.SimpleTypes.Keys);

            foreach (var innerDelta in delta.ComplexTypes.Values)
            {
                uniqueKeys.UnionWith(innerDelta.GetAllUniqueSimpleTypeKeys());
            }

            foreach (var deltaList in delta.ComplexListTypes.Values)
            {
                foreach (var innerDelta in deltaList)
                {
                    uniqueKeys.UnionWith(innerDelta.GetAllUniqueSimpleTypeKeys());
                }
            }

            return uniqueKeys;
        }

    }
}
