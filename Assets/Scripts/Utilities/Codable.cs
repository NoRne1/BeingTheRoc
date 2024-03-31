using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class ObjectExtensions
{
    public static Dictionary<string, object> ToDictionary(this object obj)
    {
        var dictionary = new Dictionary<string, object>();
        foreach (var property in obj.GetType().GetProperties())
        {
            if (property.CanRead)
            {
                dictionary[property.Name] = property.GetValue(obj);
            }
        }
        return dictionary;
    }
}

public static class DictionaryExtensions
{
    public static void Merge<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Dictionary<TKey, TValue> dict)
    {
        foreach (var kvp in dict)
        {
            dictionary[kvp.Key] = kvp.Value;
        }
    }

    public static void RecursiveMerge<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Dictionary<TKey, TValue> dict)
    {
        foreach (var kvp in dict)
        {
            if (kvp.Value is Dictionary<TKey, TValue> innerDict && dictionary.TryGetValue(kvp.Key, out TValue currentValue) && currentValue is Dictionary<TKey, TValue> selfInnerDict)
            {
                selfInnerDict.RecursiveMerge(innerDict);
            }
            else
            {
                dictionary[kvp.Key] = kvp.Value;
            }
        }
    }
}
