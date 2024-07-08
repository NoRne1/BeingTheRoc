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

public enum EquipType
{
    none = -1,
    X1_1 = 0, // 占用一个格子
    X1_2 = 1, // 占用两个相邻格子
    X2_1 = 2, // 占用两个相邻格子
    X3_1 = 3, //占用三个相邻格子
    X2_2 = 4, // 占用2x2的格子
    X3_2 = 5,
    Xtu = 6, // 土字形的四格
    Xcorner = 7, //拐角形状的三格
    Xz = 9, //z字型的4格
}

public enum ItemInvokeType
{
    none = -1,
    bagUse = 0, // 背包中使用生效
    equip = 1, // 装备生效
    instant = 2, // 获得立即生效
    equipUse = 3, //装备后使用直接生效
    equipTarget = 4, //装备后使用并选择目标生效
}

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
    public string methodName;
    public int? value;

    public int Value {
        get { return value ?? 0; }
        set { this.value = value; }
    }
}

public class StoreItemDefine
{
    public int ID { get; set; }
    public ItemType type { get; set; }
    public EquipType equipType { get; set; }
    public ItemInvokeType invokeType { get; set; }
    public TargetRange targetRange { get; set; }
    public string title { get; set; }
    public GeneralLevel level { get; set; }
    public int price { get; set; }
    public int takeEnergy { get; set; }
    public string iconResource { get; set; }
    public string iconResource2 { get; set; }
    public string desc { get; set; }
    public Effect effect1 { get; set; }
    public Effect effect2 { get; set; }
    public Effect effect3 { get; set; }
    public int ExtraEntry1 { get; set; }
    public int ExtraEntry2 { get; set; }
    public int ExtraEntry3 { get; set; }
    public AttributeData attr = new AttributeData();

    public List<Vector2Int> tempOccupiedCells;
    public List<Vector2Int> occupiedCells;
    public List<Vector2Int> OccupiedCells
    {
        get
        {
            if (CanEquip() && tempOccupiedCells != null && tempOccupiedCells.Count != 0)
            {
                //优先返回临时的
                return tempOccupiedCells;
            }
            else
            {   if (occupiedCells == null) {
                    OccupiedCellsInit();
                }
                return occupiedCells;
            }
        }
    }

    public Vector2 occupiedRect
    {
        get
        {
            switch (equipType)
            {
                case EquipType.X1_1:
                    return new Vector2(1, 1);
                case EquipType.X1_2:
                    return new Vector2(2, 1);
                case EquipType.X2_1:
                    return new Vector2(1, 2);
                case EquipType.X3_1:
                    return new Vector2(1, 3);
                case EquipType.X2_2:
                    return new Vector2(2, 2);
                case EquipType.X3_2:
                    return new Vector2(2, 3);
                case EquipType.Xtu:
                    return new Vector2(3, 2);
                case EquipType.Xcorner:
                    return new Vector2(2, 2);
                case EquipType.Xz:
                    return new Vector2(2, 3);
                default:
                    return Vector2.zero;
            }
        }
    }

    public Vector3 originOffset
    {
        get
        {
            switch (equipType)
            {
                case EquipType.X1_1:
                    return new Vector3(0, 0, 0);
                case EquipType.X1_2:
                    return new Vector3(-0.5f, 0, 0);
                case EquipType.X2_1:
                    return new Vector3(0, 0.5f, 0);
                case EquipType.X3_1:
                    return new Vector3(0, 1, 0);
                case EquipType.X2_2:
                    return new Vector3(-0.5f, 0.5f, 0);
                case EquipType.X3_2:
                    return new Vector3(-0.5f, 1, 0);
                case EquipType.Xtu:
                    return new Vector3(-1, 0.5f, 0);
                case EquipType.Xcorner:
                    return new Vector3(-0.5f, 0.5f, 0);
                case EquipType.Xz:
                    return new Vector3(-0.5f, 1, 0);
                default:
                    return Vector3.zero;
            }
        }
    }

    public bool CanEquip()
    {
        return equipType != EquipType.none;
    }

    public bool CanUse()
    {
        switch (invokeType)
        {
            case ItemInvokeType.bagUse:
            case ItemInvokeType.equipTarget:
            case ItemInvokeType.equipUse:
                return true;
            case ItemInvokeType.none:
            case ItemInvokeType.equip:
            case ItemInvokeType.instant:
            default:
                return false;
        }
    }

    public void OccupiedCellsInit()
    {
        List<Vector2Int> result = new List<Vector2Int>();
        switch (equipType)
        {
            case EquipType.none:
                occupiedCells = null;
                tempOccupiedCells = null;
                return;
            case EquipType.X1_1:
                result.Add(new Vector2Int(0, 0));
                break;
            case EquipType.X1_2:
                result.Add(new Vector2Int(0, 0));
                result.Add(new Vector2Int(0, 1));
                break;
            case EquipType.X2_1:
                result.Add(new Vector2Int(0, 0));
                result.Add(new Vector2Int(1, 0));
                break;
            case EquipType.X3_1:
                result.Add(new Vector2Int(0, 0));
                result.Add(new Vector2Int(1, 0));
                result.Add(new Vector2Int(2, 0));
                break;
            case EquipType.X2_2:
                result.Add(new Vector2Int(0, 0));
                result.Add(new Vector2Int(1, 0));
                result.Add(new Vector2Int(0, 1));
                result.Add(new Vector2Int(1, 1));
                break;
            case EquipType.X3_2:
                result.Add(new Vector2Int(0, 0));
                result.Add(new Vector2Int(1, 0));
                result.Add(new Vector2Int(2, 0));
                result.Add(new Vector2Int(0, 1));
                result.Add(new Vector2Int(1, 1));
                result.Add(new Vector2Int(2, 1));
                break;
            case EquipType.Xtu:
                result.Add(new Vector2Int(0, 0));
                result.Add(new Vector2Int(0, 1));
                result.Add(new Vector2Int(0, 2));
                result.Add(new Vector2Int(1, 1));
                break;
            case EquipType.Xcorner:
                result.Add(new Vector2Int(0, 0));
                result.Add(new Vector2Int(0, 1));
                result.Add(new Vector2Int(1, 0));
                break;
            case EquipType.Xz:
                result.Add(new Vector2Int(0, 0));
                result.Add(new Vector2Int(1, 0));
                result.Add(new Vector2Int(1, 1));
                result.Add(new Vector2Int(2, 1));
                break;
        }
        occupiedCells = result;
        tempOccupiedCells = new List<Vector2Int>(result);
    }

    public String GetTargetRangeResource()
    {
        switch (targetRange)
        {
            case TargetRange.none:
                return null;
            case TargetRange.around_8:
                return "equip_attack_range_0";
            case TargetRange.archer:
                return "equip_attack_range_1";
            case TargetRange.archer_long:
                return "equip_attack_range_2";
            case TargetRange.range_1:
                return "equip_attack_range_3";
            case TargetRange.range_2:
                return "equip_attack_range_4";
            case TargetRange.range_3:
                return "equip_attack_range_5";
            case TargetRange.line:
                return "equip_attack_range_6";
            default:
                return null;
        }
    }
}

