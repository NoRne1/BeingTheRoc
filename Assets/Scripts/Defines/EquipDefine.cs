using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

public enum EquipClass
{
    none = -1,
    sword = 0,
    arch = 1,
    shield = 2,
    other = 3,
    MAX
}

public enum EquipInvokeType
{
    none = -1,
    use = 0, // 使用直接生效
    targetItem = 1, // 使用并选择目标后生效
    targetPos = 2, // 使用并选择地点后生效
}

public class EquipDefine
{
    public int ID { get; set; }
    public FiveElementsType fiveElementsType { get; set; }
    public EquipType equipType { get; set; }
    public EquipInvokeType invokeType { get; set; }
    public EquipClass equipClass { get; set; }
    public bool isExpendable { get; set; }
    public TargetRange targetRange { get; set; }
    public int takeEnergy { get; set; }
    public string equipResource { get; set; }
    // just for attack
    public Effect effect1 { get; set; }
    public Effect effect2 { get; set; }
    public Effect effect3 { get; set; }
    //每一点能量能够造成的威胁值
    public int attackThreaten { get; set; }
    //每一点能量能够造成的保护值
    public int protectAbility { get; set; }

    public List<Vector2Int> tempOccupiedCells;
    public List<Vector2Int> occupiedCells;
    public List<Vector2Int> OccupiedCells
    {
        get
        {
            if (tempOccupiedCells != null && tempOccupiedCells.Count != 0)
            {
                //优先返回临时的
                return tempOccupiedCells;
            }
            else
            {
                if (occupiedCells == null)
                {
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
    //这个属性是用来处理记录格子位置和实际摆放中心点的偏移，记录是装备最左上的格子
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
