using System.Collections.Generic;
using UniRx;
using UnityEngine;
using System.Linq;
using static UnityEditor.Progress;

public class Backpack
{
    public Subject<bool> characterUpdate;
    public List<StoreItemModel> equips = new List<StoreItemModel>();
    public Dictionary<Vector2Int, StoreItemModel> grid = new Dictionary<Vector2Int, StoreItemModel>();
    
    public Backpack(int sizeX, int sizeY, Subject<bool> subject)
    {
        // 初始化背包，所有位置都为空
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                grid[new Vector2Int(x, y)] = null;
            }
        }
        characterUpdate = subject;
    }

    public bool CanPlace(StoreItemModel item, Vector2Int position)
    {
        return this.CanPlace(item, position, grid);
    }

    public bool CanPlace(StoreItemModel item, Vector2Int position, Dictionary<Vector2Int, StoreItemModel> grid)
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
            equips.Add(item);
            item.EquipPosition(position);
            characterUpdate.OnNext(true);
            return true;
        }
        return false;
    }

    //没写完
    public bool MoveTo(StoreItemModel item, Vector2Int position)
    {
        Dictionary<Vector2Int, StoreItemModel> copiedGrid = grid.ToDictionary(entry => entry.Key, entry => entry.Value);
        RemoveItemsByUUID(item.uuid, copiedGrid);
        if (CanPlace(item, position, copiedGrid))
        {
            RemoveItemsByUUID(item.uuid);
            foreach (Vector2Int cell in item.OccupiedCells)
            {
                Vector2Int cellPosition = position + cell;
                grid[cellPosition] = item;
            }

            //RemoveItemsByUUID中删除了装备，需要加回来
            equips.Add(item);
            item.EquipPosition(position);
            characterUpdate.OnNext(true);
            //只需要更新装备位置，旋转自动更新
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

    public void RemoveItemsByUUID(string uuid)
    {
        equips.RemoveAll(item => item.uuid == uuid);
        if (this.RemoveItemsByUUID(uuid, grid))
        {
            characterUpdate.OnNext(true);
        }
    }

    public bool RemoveItemsByUUID(string uuid, Dictionary<Vector2Int, StoreItemModel> grid)
    {
        // 使用 LINQ 筛选出字典中 uuid 相等的键值对，并转换为列表
        var itemsToRemove = grid.Where(kv => kv.Value?.uuid == uuid).ToList();

        // 遍历列表，从字典中删除这些键值对
        foreach (var itemToRemove in itemsToRemove)
        {
            grid[itemToRemove.Key] = null;
        }

        return itemsToRemove.Count> 0;
    }
}

