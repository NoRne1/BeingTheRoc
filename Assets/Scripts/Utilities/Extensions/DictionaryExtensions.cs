using System.Collections.Generic;
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
        // 将字典的键值对转换为列表
        List<KeyValuePair<TKey, TValue>> list = new List<KeyValuePair<TKey, TValue>>(dictionary);

        // 检查索引是否有效
        if (index >= 0 && index < list.Count)
        {
            // 返回指定索引处的值
            return list[index].Value;
        }
        else
        {
            // 索引超出范围，返回默认值（这里可以根据需求修改）
            Debug.LogWarning("Index out of range for dictionary.");
            return default(TValue);
        }
    }
}
