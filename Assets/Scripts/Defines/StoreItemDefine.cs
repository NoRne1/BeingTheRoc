using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

// 定义物品类型
public enum ItemType
{
    equip = 0,
    expendable = 1,
    treasure = 2,
    economicGoods = 3,
    special = 4,
    food = 5,
}

public enum SellType
{
    none = 0,
    shop = 1,
    restaurant = 2,
}

public enum BattleEffect 
{   
    DashToTarget = 0, // 突进到目标
    Backward = 1, // 后退
    Knockback = 2, // 击退
    ReturnEnergy = 3, // 返还能量
}

public enum EffectType
{
    none = -1,
    property = 0, // 属性改变
    battleEffect = 1, // 突进击退等特殊词条效果
    buff = 2, // buff
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
    HealthPercent = 17,
    hungry = 18,
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
    public int invokeTime;
    public string methodName;
    public int? value;

    public int Value {
        get { return value ?? 0; }
        set { this.value = value; }
    }

    public Effect(){}
    public Effect(EffectType effectType, EffectInvokeType invokeType, PropertyType propertyType, int invokeTime, string methodName, int value)
    {
        this.effectType = effectType;
        this.invokeType = invokeType;
        this.propertyType = propertyType;
        this.invokeTime = invokeTime;
        this.methodName = methodName;
        this.value = value;
    }
}

public class StoreItemDefine
{
    public int ID { get; set; }
    public ItemType type { get; set; }
    public int subID { get; set; }
    public SellType sellType { get; set; }
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

    public bool CanBagUse()
    {
        return type == ItemType.expendable || type == ItemType.food;
    }
}

