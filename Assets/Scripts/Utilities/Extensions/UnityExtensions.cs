using System;

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
}
