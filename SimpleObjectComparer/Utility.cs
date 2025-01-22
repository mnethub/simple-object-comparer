using System.Collections;
using System.Reflection;

namespace SimpleObjectComparer
{
    internal static class Utility
    {
        public static bool IsSimpleListEqual(object? oldValue, object? newValue, Dictionary<string, BaseTypeComparer> customComparers, out OldNewPair oldNewPair)
        {
            List<object?> list1 = [];
            List<object?> list2 = [];

            if (oldValue != null)
                list1 = ((IEnumerable)oldValue).Cast<object?>().ToList();

            if (newValue != null)
                list1 = ((IEnumerable)newValue).Cast<object?>().ToList();


            oldNewPair = OldNewPair.Get(list1, list2);
            return CompareSimpleLists(list1, list2, customComparers);
        }

        public static bool CompareSimpleLists<T>(List<T> list1, List<T> list2, Dictionary<string, BaseTypeComparer> customComparers)
        {
            if (list1 == null || list2 == null)
                return list1 == list2;

            if (list1.Count != list2.Count)
                return false;

            for (int i = 0; i < list1.Count; i++)
            {
                if (!EqualsWithNullCheck(list1[i], list2[i], customComparers) )
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Not applicable for reference types
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool EqualsWithNullCheck(object? obj1, object? obj2, Dictionary<string, BaseTypeComparer> _customComparers)
        {
            if (_customComparers.Count > 0)
            {
                var typeName = (obj1 ?? obj2)?.GetType()?.FullName ?? string.Empty;
                if (_customComparers.TryGetValue(typeName, out BaseTypeComparer? value))
                    return value.IsEqual(obj1, obj2);
            }

            if (obj1 == null && obj2 == null) return true;
            if (obj1 == null || obj2 == null) return false;
            return obj1.Equals(obj2);
        }

    }
}
