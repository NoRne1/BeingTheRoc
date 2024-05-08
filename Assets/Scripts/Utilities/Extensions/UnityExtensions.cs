using System;
using System.Collections.Generic;

public static class UnityExtensions
{
    // 扩展方法：当对象非空时执行指定的委托
    public static void IfNotNull<T>(this T obj, Action<T> action) where T : class
    {
        if (obj != null)
        {
            action(obj);
        }
    }

    public static void IfNotNull<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, Action<KeyValuePair<TKey, TValue>> action)
    {
        if (!EqualityComparer<KeyValuePair<TKey, TValue>>.Default.Equals(pair, default))
        {
            action(pair);
        }
    }
}
