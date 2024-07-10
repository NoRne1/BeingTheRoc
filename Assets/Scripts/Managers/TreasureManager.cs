using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureManager
{
    private Dictionary<int, (StoreItemModel, int)> treasures;

    public bool AddTreasure(StoreItemModel item)
    {
        if (item.type != ItemType.treasure)
        {
            Debug.LogError("AddTreasure not a treasure");
            return false;
        }
        if (treasures.ContainsKey(item.ID))
        {
            var tempNum = treasures[item.ID].Item2;
            treasures[item.ID] = (treasures[item.ID].Item1, tempNum++);
        }
        else
        {
            treasures.Add(item.ID, (item, 1));
        }
        return true;
    }

    public void RemoveTreasure(int id, int num = -1)
    {
        if (treasures.ContainsKey(id))
        {
            var tempNum = treasures[id].Item2;
            if (num == -1 || tempNum <= num)
            {
                //全部移除
                treasures.Remove(id);
            }
            else
            {
                treasures[id] = (treasures[id].Item1, tempNum - num);
            }
        }
    }

    public (StoreItemModel, int) FindTreasure(int id)
    {
        if (treasures.ContainsKey(id))
        {
            return treasures[id];
        } else
        {
            return (null, 0);
        }
    }
}
