using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace SimpleObjectComparer
{
    public class Comparer : IComparer
    {
        private readonly ConcurrentDictionary<string, PropertyInfo[]> _propertyInfoCache = [];
        private readonly CompareOptions _compareOptions;
        private readonly Dictionary<string, BaseTypeComparer> _customComparers; // For performance

        public Comparer()
        {
            _compareOptions = new CompareOptions();
            _customComparers = [];
        }
        public Comparer(CompareOptions compareOptions)
        {
            _compareOptions = compareOptions;
            _customComparers = [];
            if (compareOptions?.CustomTypeComparers != null)
            {
                foreach(var c in compareOptions.CustomTypeComparers)
                    _customComparers[c.ObjectType.FullName ?? "noname"] = c;
            }
        }


        public Delta Compare<T>(T oldObject, T newObject)
        {
            var type1 = oldObject?.GetType();
            var type2 = newObject?.GetType();

            if (type1 != null && type2 != null && type1 != type2)
                throw new InvalidOperationException("Cannot compare objects of different types");
            else if (type1 == null && type2 == null)
                return new Delta(typeof(object));

            Type type = type1 ?? type2 ?? typeof(object);

            if (!type.IsComplexType())
                throw new InvalidOperationException($"Cannot compare objects of type {type.FullName} ");

            var delta = new Delta(type);

            // Set Added and Deleted flags
            if (oldObject == null || newObject == null)
            {
                delta.IsAdded = oldObject == null;
                delta.IsDeleted = newObject == null;
                //return delta;
            }

            //Get properties
            var propertyInfos = GetPropertyInfos(type);

            foreach (var propertyInfo in propertyInfos)
            {
                if (Attribute.IsDefined(propertyInfo, typeof(CompareIgnoreAttribute)))
                {
                    delta.IgnoredFields.Add(propertyInfo.Name);
                    continue;
                }

                var oldValue = propertyInfo.GetValueWithNull(oldObject);
                var newValue = propertyInfo.GetValueWithNull(newObject);

                if (propertyInfo.PropertyType.IsSimpleType())
                {
                    if (!Utility.EqualsWithNullCheck(oldValue, newValue, _customComparers))
                        delta.SimpleTypes.Add(propertyInfo.Name, OldNewPair.Get(oldValue, newValue));
                }
                else if (propertyInfo.PropertyType.IsComplexType())
                {
                    var delta1 = Compare(oldValue, newValue);
                    if (delta1.IsAdded || delta1.IsDeleted || delta1.IsModified)
                        delta.ComplexTypes.Add(propertyInfo.Name, delta1);

                }
                else if (propertyInfo.PropertyType.IsCollectionOrArray())
                {
                    var underlyingType = propertyInfo.PropertyType.GetUnderlyingType();
                    if (underlyingType != null && underlyingType.IsSimpleType())
                    {
                        //Simple list
                        if (!Utility.IsSimpleListEqual(oldValue, newValue, _customComparers, out OldNewPair listTuple))
                        {
                            delta.SimpleListTypes.Add(propertyInfo.Name, listTuple);
                        }
                    }
                    else if (underlyingType != null && underlyingType.IsComplexType())
                    {
                        //Complex list
                        var deltas = GetComplexListDeltas(underlyingType, oldValue, newValue);
                        if (deltas != null && deltas.Count > 0)
                        {
                            delta.ComplexListTypes.Add(propertyInfo.Name, deltas);
                        }
                    }
                    else
                        delta.UnsupportedFields.Add(propertyInfo.Name);
                }
                else
                {
                    delta.UnsupportedFields.Add(propertyInfo.Name);
                }
            }

            delta.IsModified = !delta.IsAdded && !delta.IsDeleted && (delta.SimpleTypes.Count > 0 || delta.ComplexTypes.Count > 0 || delta.SimpleListTypes.Count > 0 || delta.ComplexListTypes.Count > 0);
            return delta;
        }

        private List<Delta> GetComplexListDeltas(Type underlyingType, object? oldValue, object? newValue)
        {
            if (oldValue == null && newValue == null)
                return [];

            List<object?> list1 = [];
            List<object?> list2 = [];

            if (oldValue != null)
                list1 = ((IEnumerable)oldValue).Cast<object?>().ToList();

            if (newValue != null)
                list2 = ((IEnumerable)newValue).Cast<object?>().ToList();


            var keyPropertyInfos = GetKeyPropertyInfos(underlyingType);
            if (keyPropertyInfos.Count == 0)
                return OneToOneComplexObjCompare(list1, list2);
            else
                return KeyBasedComplexObjCompare(list1, list2, keyPropertyInfos);
        }

        private List<Delta> KeyBasedComplexObjCompare(List<object?> list1, List<object?> list2, List<PropertyInfo> keyPropertyInfos)
        {
            //TODO Consider Parallel foreach

            var deltas = new List<Delta>();
            //Remove nulls for simplification
            list1.RemoveAll(item => item == null);
            list2.RemoveAll(item => item == null);

            var list1Remove = new HashSet<object?>();
            var list2Remove = new HashSet<object?>();

            foreach (var oldItem in list1)
            {
                var newItem = list2.FirstOrDefault(n => keyPropertyInfos.All(k => Utility.EqualsWithNullCheck(k.GetValueWithNull(n), k.GetValueWithNull(oldItem),_customComparers)));

                if (newItem != null)
                {
                    var delta = Compare(oldItem, newItem);
                    delta.Keys = GetKeys(oldItem, keyPropertyInfos);
                    if (delta.IsAdded || delta.IsDeleted || delta.IsModified)
                        deltas.Add(delta);

                    list1Remove.Add(oldItem);
                    list2Remove.Add(newItem);
                }
            }
            list1.RemoveAll(item => list1Remove.Contains(item));
            list2.RemoveAll(item => list2Remove.Contains(item));

            foreach (var deletedItem in list1)
            {
                var delta = Compare(deletedItem, null);
                delta.Keys = GetKeys(deletedItem, keyPropertyInfos);
                if (delta.IsAdded || delta.IsDeleted || delta.IsModified)
                    deltas.Add(delta);
            }

            foreach (var addedItem in list2)
            {
                var delta = Compare(null, addedItem);
                delta.Keys = GetKeys(addedItem, keyPropertyInfos);
                if (delta.IsAdded || delta.IsDeleted || delta.IsModified)
                    deltas.Add(delta);
            }

            return deltas;
        }

        private List<Delta> OneToOneComplexObjCompare(List<object?> list1, List<object?> list2)
        {
            var deltas = new List<Delta>();
            var p1 = 0; var p2 = 0;

            while (p1 < list1.Count && p2 < list2.Count)
            {
                var delta1 = Compare(list1[p1], list2[p2]);
                if (delta1.IsModified || delta1.IsAdded || delta1.IsDeleted)
                    deltas.Add(delta1);
                p1++;
                p2++;
            }
            while (p1 < list1.Count)
            {
                var delta1 = Compare(list1[p1], null);
                if (delta1.IsModified || delta1.IsAdded || delta1.IsDeleted)
                    deltas.Add(delta1);
                p1++;
            }
            while (p2 < list2.Count)
            {
                var delta1 = Compare(null, list2[p2]);
                if (delta1.IsModified || delta1.IsAdded || delta1.IsDeleted)
                    deltas.Add(delta1);
                p2++;
            }
            return deltas;
        }

        private static Dictionary<string, object?> GetKeys(object? obj, List<PropertyInfo> keyPropertyInfos)
        {
            var keys = new Dictionary<string, object?>();

            foreach (var pi in keyPropertyInfos)
                keys.Add(pi.Name, pi.GetValueWithNull(obj));

            return keys;
        }
        private List<PropertyInfo> GetKeyPropertyInfos(Type type)
        {
            var propertyInfos = GetPropertyInfos(type);

            var keyPropertyInfos = propertyInfos.Where(pi => Attribute.IsDefined(pi, typeof(ComparekeyAttribute))).ToList();

            foreach (PropertyInfo pi in keyPropertyInfos)
            {
                if (!pi.PropertyType.IsSimpleType())
                    throw new InvalidOperationException($"Key attribute is not supported for property {pi.Name} of type: {pi.PropertyType.FullName}");
            }

            return keyPropertyInfos;

        }

        private PropertyInfo[] GetPropertyInfos(Type type)
        {
            if (_compareOptions.NoCache)
                return type.GetProperties();

            PropertyInfo[] propertyInfo;
            if (type.FullName != null && _propertyInfoCache.TryGetValue(type.FullName, out PropertyInfo[]? value))
                propertyInfo = value;
            else
            {
                propertyInfo = type.GetProperties();
                _propertyInfoCache[type.FullName ?? "noname"] = propertyInfo;
            }
            return propertyInfo;
        }
    }
}
