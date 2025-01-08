using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public static class ListExtensions
{
    private static System.Random random = new System.Random();

    // 扩展方法：从列表中返回一个随机元素
    public static T RandomItem<T>(this List<T> list)
    {
        if (list == null || list.Count == 0)
        {
            throw new System.ArgumentException("The list cannot be null or empty");
        }
        
        int randomIndex = random.Next(0, list.Count); // 获取一个随机索引
        return list[randomIndex]; // 返回随机元素
    }
}