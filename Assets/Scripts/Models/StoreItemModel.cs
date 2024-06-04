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

    public StoreItemModel(StoreItemDefine define)
    {
        uuid = GameUtil.Instance.GenerateUniqueId();
        ID = define.ID;
        type = define.type;
        equipType = define.equipType;
        invokeType = define.invokeType;
        targetRange = define.targetRange;
        title = define.title;
        level = define.level;
        price = define.price;
        takeEnergy = define.takeEnergy;
        iconResource = define.iconResource;
        iconResource2 = define.iconResource2;
        effect1 = define.effect1;
        effect2 = define.effect2;
        effect3 = define.effect3;
        desc = define.desc;
        ExtraEntry1 = define.ExtraEntry1;
        ExtraEntry2 = define.ExtraEntry2;
        ExtraEntry3 = define.ExtraEntry3;
        OccupiedCellsInit();
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
        tempRotationAngle = (tempRotationAngle + angle) % 360;
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
