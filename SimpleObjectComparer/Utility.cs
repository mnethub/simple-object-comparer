using System.Collections;
using System.Reflection;

namespace SimpleObjectComparer
{
    internal static class Utility
    {
        public static bool IsSimpleListEqual(IEnumerable? oldValue, IEnumerable? newValue,Type underlyingType, Dictionary<string, BaseTypeComparer> customComparers, out OldNewPair oldNewPair)
        {
            oldNewPair = OldNewPair.Get(newValue, newValue);

            var customComparer = GetCustomComparer(underlyingType, customComparers);
            return CompareSimpleLists(oldValue, newValue, customComparer);
        }

        public static bool CompareSimpleLists(IEnumerable? enumerable1, IEnumerable? enumerable2,  BaseTypeComparer? customComparer)
        {

            if (enumerable1 == null || enumerable2 == null)
                return enumerable1 == enumerable2;

            if (enumerable1.Count != enumerable2.Count)
                return false;

            var enumerator1 = enumerable1.GetEnumerator();
            var enumerator2 = enumerable2.GetEnumerator();

            while(enumerator1.MoveNext() && enumerator2.MoveNext())
            {
                if (!EqualsWithNullCheck(enumerator1.Current, enumerator2.Current, customComparer))
                    return false;
            }

            return true;
        }

        public static BaseTypeComparer? GetCustomComparer(Type? type, Dictionary<string, BaseTypeComparer> customComparers)
        {
            if(customComparers ==  null) 
                return null;
            customComparers.TryGetValue(type?.FullName??string.Empty, out BaseTypeComparer? value);
            return value;
        }

        /// <summary>
        /// Not applicable for reference types
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool EqualsWithNullCheck(object? obj1, object? obj2, Dictionary<string, BaseTypeComparer> customComparers)
        {
            return EqualsWithNullCheck(obj1, obj2, GetCustomComparer((obj1 ?? obj2)?.GetType(), customComparers));
        }

        /// <summary>
        /// Not applicable for reference types
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool EqualsWithNullCheck(object? obj1, object? obj2, BaseTypeComparer? customComparer)
        {
            if (customComparer != null)
                return customComparer.IsEqual(obj1, obj2);

            if (obj1 == null && obj2 == null) return true;
            if (obj1 == null || obj2 == null) return false;
            return obj1.Equals(obj2);
        }

    }
}
