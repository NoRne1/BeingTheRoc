using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    // 扩展方法：通过索引访问字典值
    public static TValue GetValueByIndex<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, int index)
    {
        if (dictionary == null)
        {
            Debug.LogWarning("Dictionary is null.");
            return default(TValue);
        }

        // 获取字典中的键值对数量
        int count = dictionary.Count;

        // 检查索引是否有效
        if (index >= 0 && index < count)
        {
            // 直接按索引访问字典中的值
            return dictionary.ElementAt(index).Value;
        }
        else
        {
            // 索引超出范围，返回默认值
            Debug.LogWarning("Index out of range for dictionary.");
            return default(TValue);
        }
    }

    public static Dictionary<Vector2, int> AddDictionaries(Dictionary<Vector2, int> dict1, Dictionary<Vector2, int> dict2)
    {
        Dictionary<Vector2, int> result = new Dictionary<Vector2, int>(dict1);

        foreach (var kvp in dict2)
        {
            if (result.ContainsKey(kvp.Key))
            {
                result[kvp.Key] += kvp.Value;
            }
            else
            {
                result[kvp.Key] = kvp.Value;
            }
        }

        return result;
    }

    public static Dictionary<Vector2, int> SubtractDictionaries(Dictionary<Vector2, int> dict1, Dictionary<Vector2, int> dict2)
    {
        Dictionary<Vector2, int> result = new Dictionary<Vector2, int>(dict1);

        foreach (var kvp in dict2)
        {
            if (result.ContainsKey(kvp.Key))
            {
                result[kvp.Key] -= kvp.Value;
            }
            else
            {
                result[kvp.Key] = -kvp.Value;
            }
        }

        return result;
    }
}
