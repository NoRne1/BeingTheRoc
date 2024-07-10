using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class TreasureManager
{
    private DisposablePool normalDisposablePool;
    private DisposablePool battleDisposablePool;
    private Timer timer;
    private Dictionary<int, (StoreItemModel, int)> treasures;

    public TreasureManager()
    {
        normalDisposablePool = new DisposablePool();
        battleDisposablePool = new DisposablePool();
        timer = new Timer();
        BattleManager.Instance.roundManager.roundTime.AsObservable().TakeUntilDestroy(GameManager.Instance)
            .Where(round => round.Item2 == RoundTime.begin && BattleManager.Instance.roundManager.extraRound == 0)
            .Subscribe(round =>
            {
                timer.NextRound(round.Item1);
            });
    }

    ~TreasureManager()
    {
        normalDisposablePool.CleanDisposables();
        battleDisposablePool.CleanDisposables();
    }

    public void BattleEnd()
    {
        battleDisposablePool.CleanDisposables();
        timer.Clean();
    }

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
