using System;
using System.Collections.Generic;
using UnityEngine;



public class StoreItemModel : StoreItemDefine
{
    public string uuid;
    public Vector3 recordPosition;
    public Vector2Int position; // 在背包中的位置
    public int rotationAngle; // 旋转角度

    public List<Vector2Int> OccupiedCells
    {
        get
        {
            return occupiedCells;
        }
    }
    private List<Vector2Int> occupiedCells;

    public Vector2 occupiedRect
    {
        get
        {
            switch (type)
            {
                case ItemType.none:
                    return Vector2.zero;
                case ItemType.X1_1:
                    return new Vector2(180, 180);
                case ItemType.X2_1:
                    return new Vector2(180, 360);
                case ItemType.X3_1:
                    return new Vector2(180, 540);
                case ItemType.X2_2:
                    return new Vector2(360, 360);
                case ItemType.X3_2:
                    return new Vector2(360, 540);
                case ItemType.X3_3:
                    return new Vector2(540, 540);
                case ItemType.Xtu:
                    return new Vector2(360, 540);
                case ItemType.Xcorner:
                    return new Vector2(360, 360);
                case ItemType.Xten:
                    return new Vector2(360, 360);
                case ItemType.Xz:
                    return new Vector2(540, 360);
                default:
                    return Vector2.zero;
            }
        }
    }

    public StoreItemModel(StoreItemDefine define)
    {
        uuid = GameUtil.Instance.GenerateUniqueId();
        ID = define.ID;
        type = define.type;
        iconResource = define.iconResource;
        level = define.level;
        title = define.title;
        price = define.price;
        OccupiedCellsInit();
    }

    public void EquipPosition(Vector2Int position)
    {
        if (type != ItemType.none)
        {
            this.position = position;
        }
    }

    //public void EquipRotation(int rotationAngle)
    //{
    //    if (type != ItemType.none)
    //    {
    //        this.rotationAngle = rotationAngle;
    //    }
    //}

    public void unEquip()
    {
        this.recordPosition = Vector3.zero;
        this.position = Vector2Int.zero;
        this.rotationAngle = 0;
        OccupiedCellsInit();
    }

    public void OccupiedCellsInit()
    {
        List<Vector2Int> result = new List<Vector2Int>();
        switch (type)
        {
            case ItemType.none:
                occupiedCells = null;
                return;
            case ItemType.X1_1:
                result.Add(new Vector2Int(0, 0));
                break;
            case ItemType.X2_1:
                result.Add(new Vector2Int(0, 0));
                result.Add(new Vector2Int(1, 0));
                break;
            case ItemType.X3_1:
                result.Add(new Vector2Int(0, 0));
                result.Add(new Vector2Int(1, 0));
                result.Add(new Vector2Int(2, 0));
                break;
            case ItemType.X2_2:
                result.Add(new Vector2Int(0, 0));
                result.Add(new Vector2Int(1, 0));
                result.Add(new Vector2Int(0, 1));
                result.Add(new Vector2Int(1, 1));
                break;
            case ItemType.X3_2:
                result.Add(new Vector2Int(0, 0));
                result.Add(new Vector2Int(1, 0));
                result.Add(new Vector2Int(2, 0));
                result.Add(new Vector2Int(0, 1));
                result.Add(new Vector2Int(1, 1));
                result.Add(new Vector2Int(2, 1));
                break;
            case ItemType.X3_3:
                result.Add(new Vector2Int(0, 0));
                result.Add(new Vector2Int(1, 0));
                result.Add(new Vector2Int(2, 0));
                result.Add(new Vector2Int(0, 1));
                result.Add(new Vector2Int(1, 1));
                result.Add(new Vector2Int(2, 1));
                result.Add(new Vector2Int(0, 2));
                result.Add(new Vector2Int(1, 2));
                result.Add(new Vector2Int(2, 2));
                break;
            case ItemType.Xtu:
                result.Add(new Vector2Int(0, 0));
                result.Add(new Vector2Int(0, 1));
                result.Add(new Vector2Int(0, 2));
                result.Add(new Vector2Int(1, 1));
                break;
            case ItemType.Xcorner:
                result.Add(new Vector2Int(0, 0));
                result.Add(new Vector2Int(0, 1));
                result.Add(new Vector2Int(1, 0));
                break;
            case ItemType.Xten:
                result.Add(new Vector2Int(0, 0));
                result.Add(new Vector2Int(1, 0));
                result.Add(new Vector2Int(2, 0));
                result.Add(new Vector2Int(1, -1));
                result.Add(new Vector2Int(1, 1));
                break;
            case ItemType.Xz:
                result.Add(new Vector2Int(0, 0));
                result.Add(new Vector2Int(1, 0));
                result.Add(new Vector2Int(1, 1));
                result.Add(new Vector2Int(2, 1));
                break;
        }
        occupiedCells = result;
    }
    public void Rotate(int angle)
    {
        if (occupiedCells == null || occupiedCells.Count == 0)
        {
            return;
        }
        List<Vector2Int> points = new List<Vector2Int>(occupiedCells);
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
        occupiedCells = points;
        rotationAngle = (rotationAngle + 90) % 360;
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
