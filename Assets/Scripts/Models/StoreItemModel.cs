using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;



public class StoreItemModel : StoreItemDefine
{
    public string uuid;
    public string characterID = "";
    public Vector2Int position; // 在背包中的位置
    public int rotationAngle; // 旋转角度
    private int tempRotationAngle; // 旋转角度
    public List<Vector2Int> OccupiedCells
    {
        get
        {
            if(CanEquip() && tempOccupiedCells.Count != 0)
            {
                //优先返回临时的
                return tempOccupiedCells;
            } else
            {
                return occupiedCells;
            }
        }
    }

    private List<Vector2Int> tempOccupiedCells;
    private List<Vector2Int> occupiedCells;

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
                case EquipType.Xten:
                    return new Vector2(3, 3);
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
                case EquipType.Xten:
                    return new Vector3(0, 1, 0);
                case EquipType.Xz:
                    return new Vector3(-0.5f, 1, 0);
                default:
                    return Vector3.zero;
            }
        }
    }

    public StoreItemModel(StoreItemDefine define)
    {
        uuid = GameUtil.Instance.GenerateUniqueId();
        ID = define.ID;
        type = define.type;
        equipType = define.equipType;
        invokeType = define.invokeType;
        title = define.title;
        level = define.level;
        price = define.price;
        takeEnergy = define.takeEnergy;
        iconResource = define.iconResource;
        iconResource2 = define.iconResource2;
        effect1 = define.effect1;
        effect2 = define.effect2;
        effect3 = define.effect3;
        OccupiedCellsInit();
    }

    public bool CanEquip()
    {
        return equipType != EquipType.none;
    }

    public void Equip(string characterID, Vector2Int position)
    {
        if (CanEquip())
        {
            this.characterID = characterID;
            this.position = position;
            this.rotationAngle = tempRotationAngle;
            this.occupiedCells = new List<Vector2Int>(tempOccupiedCells);
        }
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

    public void ResetRotate()
    {
        this.tempRotationAngle = rotationAngle;
        this.tempOccupiedCells = new List<Vector2Int>(occupiedCells);
    }

    public void Unequip()
    {
        this.characterID = "";
        this.position = Vector2Int.zero;
        this.rotationAngle = 0;
        OccupiedCellsInit();
    }

    public void OccupiedCellsInit()
    {
        List<Vector2Int> result = new List<Vector2Int>();
        switch (equipType)
        {
            case EquipType.none:
                occupiedCells = null;
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
            case EquipType.Xten:
                result.Add(new Vector2Int(0, 0));
                result.Add(new Vector2Int(1, 0));
                result.Add(new Vector2Int(2, 0));
                result.Add(new Vector2Int(1, -1));
                result.Add(new Vector2Int(1, 1));
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

    public void Rotate(int angle)
    {
        List<Vector2Int> points;
        if (tempOccupiedCells == null || tempOccupiedCells.Count == 0)
        {
            return;
        }
        points = new List<Vector2Int>(tempOccupiedCells);
        // 对每个点进行旋转和平移
        for (int i = 0; i < points.Count; i++)
        {
            Vector2Int point = points[i]; // 获取当前点的副本
            int newX, newY;

            // 根据给定的角度执行旋转
            switch (angle)
            {
                case 0:
                    newX = point.x;
                    newY = point.y;
                    break;
                case 90:
                    newX = -point.y;
                    newY = point.x;
                    break;
                case 180:
                    newX = -point.x;
                    newY = -point.y;
                    break;
                case 270:
                    newX = point.y;
                    newY = -point.x;
                    break;
                default:
                    throw new ArgumentException("Invalid angle. Please enter 0, 90, 180, or 270.");
            }

            // 更新点的坐标
            points[i] = new Vector2Int(newX, newY);
        }
        tempOccupiedCells = points;
        tempRotationAngle = (tempRotationAngle + 90) % 360;
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
            return result.Select(temp => { return temp + vect; }).ToList();
        }
        else
        {
            return result;
        }
    }
    //public void Rotate(int angle)
    //{
    //    if (occupiedCells == null || occupiedCells.Count == 0)
    //    {
    //        return;
    //    }
    //    List<Vector2Int> points = new List<Vector2Int>(occupiedCells);
    //    // 旋转后最小的 100*x + y 运算结果
    //    // 100*x+y是为了优先考虑行最小，然后考虑列最小，
    //    int minScore = int.MaxValue;

    //    // 记录使得最小得分的点的平移距离
    //    int translateX = 0;
    //    int translateY = 0;

    //    // 执行旋转操作并计算最小得分
    //    foreach (Vector2Int point in points)
    //    {
    //        int newX, newY;

    //        // 根据给定的角度执行旋转
    //        switch (angle)
    //        {
    //            case 0:
    //                newX = point.x;
    //                newY = point.y;
    //                break;
    //            case 90:
    //                newX = -point.y;
    //                newY = point.x;
    //                break;
    //            case 180:
    //                newX = -point.x;
    //                newY = -point.y;
    //                break;
    //            case 270:
    //                newX = point.y;
    //                newY = -point.x;
    //                break;
    //            default:
    //                throw new ArgumentException("Invalid angle. Please enter 0, 90, 180, or 270.");
    //        }

    //        // 计算旋转后的得分
    //        int score = 100 * newX + newY;

    //        // 更新最小得分和平移距离
    //        if (score < minScore)
    //        {
    //            minScore = score;
    //            translateX = newX;
    //            translateY = newY;
    //        }
    //    }

    //    // 对每个点进行旋转和平移
    //    for (int i = 0; i < points.Count; i++)
    //    {
    //        Vector2Int point = points[i]; // 获取当前点的副本
    //        int newX, newY;

    //        // 根据给定的角度执行旋转
    //        switch (angle)
    //        {
    //            case 0:
    //                newX = point.x;
    //                newY = point.y;
    //                break;
    //            case 90:
    //                newX = -point.y;
    //                newY = point.x;
    //                break;
    //            case 180:
    //                newX = -point.x;
    //                newY = -point.y;
    //                break;
    //            case 270:
    //                newX = point.y;
    //                newY = -point.x;
    //                break;
    //            default:
    //                throw new ArgumentException("Invalid angle. Please enter 0, 90, 180, or 270.");
    //        }

    //        // 对点进行平移
    //        newX -= translateX;
    //        newY -= translateY;

    //        // 更新点的坐标
    //        points[i] = new Vector2Int(newX, newY);
    //    }
    //    occupiedCells = points;
    //    rotationAngle = (rotationAngle + 90) % 360;
    //    Debug.Log("Rotate occupiedCells:" + occupiedCells[0] +" " + occupiedCells[1]);
    //    Debug.Log("Rotate rotationAngle:" + rotationAngle);
    //}
}
