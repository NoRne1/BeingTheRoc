using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// 定义装备类型
public enum ItemType
{
    equip = 0,
    charm = 1,
    treasure = 2,
    potion = 3,
    economicGoods = 4,
    special = 5,
}

public enum EquipType
{
    none = -1,
    X1_1 = 0, // 占用一个格子
    X1_2 = 1, // 占用两个相邻格子
    X2_1 = 2, // 占用两个相邻格子
    X3_1 = 3, //占用三个相邻格子
    X2_2 = 4, // 占用2x2的格子
    X3_2 = 5,
    X3_3 = 6,
    Xtu = 7, //土字形的四格
    Xcorner = 8, //拐角形状的三格
    Xten = 9, //十字型的五格
    Xz = 10, //z字型的4格
}

public enum InvokeType
{
    none = -1,
    use = 0, // 使用生效
    equip = 1, // 装备生效
    instant = 2, // 立即执行
}

public enum EffectType
{
    none = -1,
    property = 0, // 属性改变
    health = 1, // 生命值变化
    buff = 2, // buff
    skill = 3, // skill
}

public enum PropertyType
{
    none = -1,
    MaxHP = 0, 
    Strength = 1, 
    Defense = 2,
    Dodge = 3,
    Accuracy = 4,
    Speed = 5,
    Mobility = 6,
    Energy = 7,
    Lucky = 8,
}

public class Effect
{
    public EffectType? effectType;
    public PropertyType? propertyType;
    public string methodName;
    public int? value;

    public int Value { get { return value ?? 0; } }
}

public class StoreItemDefine
{
    public int ID { get; set; }
    public ItemType type { get; set; }
    public EquipType equipType { get; set; }
    public InvokeType invokeType { get; set; }
    public string title { get; set; }
    public EquipLevel level { get; set; }
    public int price { get; set; }
    public int takeEnergy { get; set; }
    public string iconResource { get; set; }
    public string iconResource2 { get; set; }
    public Effect effect1 { get; set; }
    public Effect effect2 { get; set; }
    public Effect effect3 { get; set; }
}

