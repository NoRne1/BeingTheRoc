using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

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
    Xtu = 6, //土字形的四格
    Xcorner = 7, //拐角形状的三格
    Xz = 9, //z字型的4格
}

public enum InvokeType
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
    HP = 9,
    Exp = 10,
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
    public TargetRange targetRange { get; set; }
    public string title { get; set; }
    public GenerlLevel level { get; set; }
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
            case InvokeType.bagUse:
            case InvokeType.equipTarget:
            case InvokeType.equipUse:
                return true;
            case InvokeType.none:
            case InvokeType.equip:
            case InvokeType.instant:
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

    public List<Vector2> GetTargetRangeList(Vector2 vect)
    {
        List<Vector2> result;
        bool flag = true;
        switch (targetRange)
        {
            case TargetRange.range_1:
                result = new List<Vector2>()
                {
                    new Vector2(0, 1),
                    new Vector2(0, -1),
                    new Vector2(1, 0),
                    new Vector2(-1, 0),
                };
                break;
            case TargetRange.range_2:
                result = new List<Vector2>()
                {
                    new Vector2(0, 1),
                    new Vector2(0, -1),
                    new Vector2(1, 0),
                    new Vector2(-1, 0),
                    new Vector2(-2, 0),
                    new Vector2(2, 0),
                    new Vector2(0, 2),
                    new Vector2(0, -2),
                    new Vector2(1, 1),
                    new Vector2(1, -1),
                    new Vector2(-1, 1),
                    new Vector2(-1, -1),
                };
                break;
            case TargetRange.range_3:
                result = new List<Vector2>()
                {
                    new Vector2(0, 1),
                    new Vector2(0, -1),
                    new Vector2(1, 0),
                    new Vector2(-1, 0),
                    new Vector2(-2, 0),
                    new Vector2(2, 0),
                    new Vector2(0, 2),
                    new Vector2(0, -2),
                    new Vector2(-3, 0),
                    new Vector2(3, 0),
                    new Vector2(0, 3),
                    new Vector2(0, -3),
                    new Vector2(1, 1),
                    new Vector2(1, -1),
                    new Vector2(-1, 1),
                    new Vector2(-1, -1),
                    new Vector2(2, 1),
                    new Vector2(2, -1),
                    new Vector2(-2, 1),
                    new Vector2(-2, -1),
                    new Vector2(1, 2),
                    new Vector2(1, -2),
                    new Vector2(-1, 2),
                    new Vector2(-1, -2),
                };
                break;
            case TargetRange.archer:
                result = new List<Vector2>()
                {
                    new Vector2(-2, 0),
                    new Vector2(2, 0),
                    new Vector2(0, 2),
                    new Vector2(0, -2),
                    new Vector2(1, 1),
                    new Vector2(1, -1),
                    new Vector2(-1, 1),
                    new Vector2(-1, -1),
                };
                break;
            case TargetRange.archer_long:
                result = new List<Vector2>()
                {
                    new Vector2(-3, 0),
                    new Vector2(3, 0),
                    new Vector2(0, 3),
                    new Vector2(0, -3),
                    new Vector2(2, 1),
                    new Vector2(2, -1),
                    new Vector2(-2, 1),
                    new Vector2(-2, -1),
                    new Vector2(1, 2),
                    new Vector2(1, -2),
                    new Vector2(-1, 2),
                    new Vector2(-1, -2),
                };
                break;
            case TargetRange.around_8:
                result = new List<Vector2>()
                {
                    new Vector2(0, 1),
                    new Vector2(0, -1),
                    new Vector2(1, 0),
                    new Vector2(-1, 0),
                    new Vector2(1, 1),
                    new Vector2(1, -1),
                    new Vector2(-1, 1),
                    new Vector2(-1, -1),
                };
                break;
            case TargetRange.line:
                int boardSize = 8;
                List<Vector2> tempList = new List<Vector2>();
                // 获取同行的坐标
                for (int x = 0; x < boardSize; x++)
                {
                    if (x != vect.x) // 排除传入坐标本身
                    {
                        tempList.Add(new Vector2(x, vect.y));
                    }
                }

                // 获取同列的坐标
                for (int y = 0; y < boardSize; y++)
                {
                    if (y != vect.y && y != vect.y) // 排除传入坐标本身
                    {
                        tempList.Add(new Vector2(vect.x, y));
                    }
                }
                result = tempList;
                flag = false;
                break;
            case TargetRange.none:
            default:
                result = new List<Vector2>();
                break;
        }
        if (flag)
        {
            return result.Select(temp => { return temp + vect; })
                .Where(temp => { return GameUtil.Instance.InChessBoard(temp); }).ToList();
        }
        else
        {
            return result;
        }
    }
}

