using System.Collections.Generic;
using UniRx;
using UnityEngine;
using System.Linq;
using static UnityEditor.Progress;

public class Backpack
{
    public string characterID;
    //装备变化（换位不算）
    public Subject<Unit> equipUpdate = new Subject<Unit>();
    //背包变化（换位也算）
    public Subject<Unit> fatherUpdate;
    public List<StoreItemModel> equips = new List<StoreItemModel>();
    public Dictionary<Vector2Int, StoreItemModel> grid = new Dictionary<Vector2Int, StoreItemModel>();
    
    //just for deepcopy
    public Backpack()
    {}
    public Backpack(string characterID, int sizeX, int sizeY, Subject<Unit> subject)
    {
        this.characterID = characterID;
        // 初始化背包，所有位置都为空
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                grid[new Vector2Int(x, y)] = null;
            }
        }
        fatherUpdate = subject;
    }

    public StoreItemModel GetMostThreatenEquip() 
    {
        StoreItemModel mostThreatenEquip = null;
        int maxThreaten = 0;
        foreach (var equip in equips)
        {
            if (equip.equipCanUse && equip.equipModel.attackThreaten > maxThreaten)
            {
                mostThreatenEquip = equip;
                maxThreaten = equip.equipModel.attackThreaten;
            }
        }
        return mostThreatenEquip;   
    }

    public List<StoreItemModel> GetEquipsSortedByThreaten()
    {
        return equips
            .Where(equip => equip.equipCanUse && equip.equipModel.attackThreaten > 0)
            .OrderByDescending(equip => equip.equipModel.attackThreaten)
            .ToList();
    }

    public StoreItemModel GetMostProtectAbilityEquip() 
    {
        StoreItemModel mostProtectAbilityEquip = null;
        int maxProtectAbility = 0;
        foreach (var equip in equips)
        {
            if (equip.equipCanUse && equip.equipModel.protectAbility > maxProtectAbility)
            {
                mostProtectAbilityEquip = equip;
                maxProtectAbility = equip.equipModel.protectAbility;
            }
        }
        return mostProtectAbilityEquip;   
    }

    public List<StoreItemModel> GetEquipsSortedByProtectAbility()
    {
        return equips
            .Where(equip => equip.equipCanUse && equip.equipModel.protectAbility > 0)
            .OrderByDescending(equip => equip.equipModel.protectAbility)
            .ToList();
    }

    public bool CanPlace(StoreItemModel item, Vector2Int position)
    {
        return this.CanPlace(item, position, grid);
    }

    public bool CanPlace(StoreItemModel item, Vector2Int position, Dictionary<Vector2Int, StoreItemModel> grid)
    {
        foreach (Vector2Int cell in item.equipModel.OccupiedCells)
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
            foreach (Vector2Int cell in item.equipModel.OccupiedCells)
            {
                Vector2Int cellPosition = position + cell;
                grid[cellPosition] = item;
            }
            equips.Add(item);
            item.Equip(characterID, position);
            fatherUpdate.OnNext(Unit.Default);
            equipUpdate.OnNext(Unit.Default);
            return true;
        }
        return false;
    }

    public bool MoveTo(StoreItemModel item, Vector2Int position)
    {
        Dictionary<Vector2Int, StoreItemModel> copiedGrid = grid.ToDictionary(entry => entry.Key, entry => entry.Value);
        RemoveItemsByUUID(item.uuid, copiedGrid);
        if (CanPlace(item, position, copiedGrid))
        {
            RemoveItemsByUUID(item.uuid);
            foreach (Vector2Int cell in item.equipModel.OccupiedCells)
            {
                Vector2Int cellPosition = position + cell;
                grid[cellPosition] = item;
            }

            //RemoveItemsByUUID中删除了装备，需要加回来
            equips.Add(item);
            item.Equip(characterID, position);
            fatherUpdate.OnNext(Unit.Default);
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
            fatherUpdate.OnNext(Unit.Default);
            equipUpdate.OnNext(Unit.Default);
        }
    }

    //删除格子中的占用
    private bool RemoveItemsByUUID(string uuid, Dictionary<Vector2Int, StoreItemModel> grid)
    {
        // 使用 LINQ 筛选出字典中 uuid 相等的键值对，并转换为列表
        var itemsToRemove = grid.Where(kv => kv.Value?.uuid == uuid).ToList();

        // 遍历列表，从字典中删除这些键值对
        foreach (var itemToRemove in itemsToRemove)
        {
            grid[itemToRemove.Key] = null;
        }

        return itemsToRemove.Count > 0;
    }
}

