using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

// 定义装备类型
public enum ItemType
{
    equip = 0,
    expendable = 1,
    treasure = 2,
    economicGoods = 3,
    special = 4,
}

//public enum ItemInvokeType
//{
//    none = -1,
//    bagUse = 0, // 使用生效（背包中使用，或treasure0点击生效）
//    equip = 1, // 装备生效
//    instant = 2, // 获得立即生效
//    equipUse = 3, // 装备后使用直接生效
//    equipTarget = 4, // 装备后使用并选择目标生效
//    battleStart = 5, // 战斗开始时生效
//}

public enum EffectType
{
    none = -1,
    property = 0, // 属性改变
    buff = 1, // buff
    skill = 2, // skill
    attack = 3,
}

public enum EffectInvokeType
{
    none = -1,
    useInstant = 0, // 使用立即生效
    damage = 1, // 造成伤害生效
    toDeath = 2, //造成死亡生效
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
    Health = 9,
    Exp = 10,
    Shield = 11,
    Protection = 12,
    EnchanceDamage = 13,
    Hematophagia = 14,
    DistanceDamage = 15,
    AgainstDamage = 16,
}

public enum TargetRange
{
    none = -1,
    around_8 = 0,
    archer = 1,
    archer_long = 2,
    range_1 = 3,
    range_2 = 4,
    range_3 = 5,
    line = 6,
}

public class Effect
{
    public EffectType? effectType;
    public EffectInvokeType? invokeType;
    public PropertyType? propertyType;
    public int invokeNum;
    public string methodName;
    public int? value;

    public int Value {
        get { return value ?? 0; }
        set { this.value = value; }
    }

    public Effect Copy()
    {
        Effect copy = (Effect)this.MemberwiseClone();

        return copy;
    }
}

public class StoreItemDefine
{
    public int ID { get; set; }
    public ItemType type { get; set; }
    public int subID { get; set; }
    public string title { get; set; }
    public GeneralLevel level { get; set; }
    public int price { get; set; }
    public string iconResource { get; set; }
    public string desc { get; set; }
    public int ExtraEntry1 { get; set; }
    public int ExtraEntry2 { get; set; }
    public int ExtraEntry3 { get; set; }           

    public bool CanEquip()
    {
        return type == ItemType.equip;
    }
}

