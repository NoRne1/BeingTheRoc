using System;
using System.Collections.Generic;
using UnityEngine;

public class Backpack : MonoBehaviour
{
    private Dictionary<Vector2Int, StoreItemModel> grid = new Dictionary<Vector2Int, StoreItemModel>();

    public Backpack(int sizeX, int sizeY)
    {
        // 初始化背包，所有位置都为空
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                grid[new Vector2Int(x, y)] = null;
            }
        }
    }

    public bool CanPlace(StoreItemModel item, Vector2Int position)
    {
        foreach (Vector2Int cell in item.OccupiedCells)
        {
            Vector2Int cellPosition = position + cell;
            if (!grid.ContainsKey(cellPosition) || grid[cellPosition] != null)
            {
                return false;
            }
        }
        return true;
    }

    public bool Place(StoreItemModel item, Vector2Int position)
    {
        if (CanPlace(item, position))
        {
            foreach (Vector2Int cell in item.OccupiedCells)
            {
                Vector2Int cellPosition = position + cell;
                grid[cellPosition] = item;
            }
            return true;
        }
        return false;
    }

    public StoreItemModel GetItem(Vector2Int position)
    {
        if (grid.ContainsKey(position))
        {
            return grid[position];
        }
        return null;
    }

    public void RemoveItem(Vector2Int position)
    {
        if (grid.ContainsKey(position))
        {
            grid[position] = null;
        }
    }
}

