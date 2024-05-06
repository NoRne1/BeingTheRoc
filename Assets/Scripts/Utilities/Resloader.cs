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

    public static Sprite LoadSprite(string path)
    {
        return Resloader.Load<Sprite>(ConstValue.spritePath + path);
    }
}