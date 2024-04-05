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
    public List<Vector2Int> OccupiedCells { get { return occupiedCells; } }
    private List<Vector2Int> occupiedCells;

    public void OccupiedCellsInit()
    {
        List<Vector2Int> result = new List<Vector2Int>();
        switch (type)
        {
            case ItemType.none:
                occupiedCells = null;
                return;
            case ItemType.X1_1:
                result.Add(new Vector2Int(0,0));
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

    public List<Vector2Int> Rotate(int angle)
    {
        if (occupiedCells == null || occupiedCells.Count == 0)
        {
            return null;
        }
        List<Vector2Int> points = new List<Vector2Int>(occupiedCells);
        // 旋转后最小的 100*x + y 运算结果
        // 100*x+y是为了优先考虑行最小，然后考虑列最小，
        int minScore = int.MaxValue;

        // 记录使得最小得分的点的平移距离
        int translateX = 0;
        int translateY = 0;

        // 执行旋转操作并计算最小得分
        foreach (Vector2Int point in points)
        {
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

            // 计算旋转后的得分
            int score = 100 * newX + newY;

            // 更新最小得分和平移距离
            if (score < minScore)
            {
                minScore = score;
                translateX = newX;
                translateY = newY;
            }
        }

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

            // 对点进行平移
            newX -= translateX;
            newY -= translateY;

            // 更新点的坐标
            points[i] = new Vector2Int(newX, newY);
        }
        return points;
    }
}

