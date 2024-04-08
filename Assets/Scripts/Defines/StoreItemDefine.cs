using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// 定义装备类型
public enum ItemType
{
    none = -1, //不可装备
    X1_1 = 0, // 占用一个格子
    X2_1 = 1, // 占用两个相邻格子
    X3_1 = 2, //占用三个相邻格子
    X2_2 = 3, // 占用2x2的格子
    X3_2 = 4,
    X3_3 = 5,
    Xtu = 6, //土字形的四格
    Xcorner = 7, //拐角形状的三格
    Xten = 8, //十字型的五格
    Xz = 9, //z字型的4格
}

public class StoreItemDefine
{
    public int ID { get; set; }
    public ItemType type;
    public string iconResource { get; set; }
    public EquipLevel level { get; set; }
    public string title { get; set; }
    public int price { get; set; }
}

