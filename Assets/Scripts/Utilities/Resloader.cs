using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Resloader
{
    //加载资源
    public static T Load<T>(string path) where T : UnityEngine.Object
    {
        return Resources.Load<T>(path) as T;
    }

    public static Sprite LoadSprite(string path, string folder)
    {
        return Resloader.Load<Sprite>(ConstValue.spritePath + folder + path);
    }

    public static List<Sprite> LoadAllSprite(string folder)
    {
        // 使用 Resources.LoadAll 来加载指定文件夹中的所有 Sprite 类型的资源
        Sprite[] sprites = Resources.LoadAll<Sprite>(ConstValue.spritePath + folder);
        
        // 将加载的 Sprite 数组转换为 List<Sprite> 并返回
        return new List<Sprite>(sprites);
    }
}