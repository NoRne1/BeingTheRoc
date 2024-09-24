using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UniRx;
using UnityEngine;
using System.Linq;

public class TreasureManager
{
    private DisposablePool normalDisposablePool;
    private DisposablePool battleDisposablePool;
    private Timer timer;
    //item.ID not item.subID
    private Dictionary<int, (StoreItemModel, int)> treasures = new Dictionary<int, (StoreItemModel, int)>();
    public Dictionary<EquipClass, int> equipClassEffect = new Dictionary<EquipClass, int>();

    public Subject<Unit> treasuresUpdate = new Subject<Unit>();

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

    public void BattleStart()
    {
        var list = treasures.Values.Where(pair => pair.Item1.treasureDefine.invokeType == TreasureInvokeType.battleStart).ToList();
        foreach(var pair in list)
        {
            InvokeTreasureEffect(pair.Item1.ID);
        }
    }

    public void BattleEnd()
    {
        battleDisposablePool.CleanDisposables();
        timer.Clean();
        equipClassEffect.Clear();
    }


    public void InvokeTreasureEffect(int id)
    {
        if (treasures.ContainsKey(id))
        {
            MethodInfo method;
            object[] parameters;
            switch (treasures[id].Item1.treasureDefine.invokeType)
            {
                case TreasureInvokeType.normalUse:
                    if (!BattleManager.Instance.isInBattle)
                    {
                        Debug.Log("Treasure " + treasures[id].Item1.title + " effect invoked");
                        method = typeof(TreasureManager).GetMethod(treasures[id].Item1.treasureDefine.methodName,
                            BindingFlags.NonPublic | BindingFlags.Instance);
                        parameters = new object[] { treasures[id].Item1, treasures[id].Item2, treasures[id].Item1.treasureDefine.value };
                        method?.Invoke(this, parameters);
                    }
                    break;
                case TreasureInvokeType.battleUse:
                    if (BattleManager.Instance.isInBattle)
                    {
                        Debug.Log("Treasure " + treasures[id].Item1.title + " effect invoked");
                        method = typeof(TreasureManager).GetMethod(treasures[id].Item1.treasureDefine.methodName,
                            BindingFlags.NonPublic | BindingFlags.Instance);
                        parameters = new object[] { treasures[id].Item1, treasures[id].Item2, treasures[id].Item1.treasureDefine.value };
                        method?.Invoke(this, parameters);
                    }
                    break;
                case TreasureInvokeType.battleStart:
                    if (!BattleManager.Instance.isInBattle)
                    {
                        Debug.Log("Treasure " + treasures[id].Item1.title + " effect invoked");
                        method = typeof(TreasureManager).GetMethod(treasures[id].Item1.treasureDefine.methodName,
                            BindingFlags.NonPublic | BindingFlags.Instance);
                        parameters = new object[] { treasures[id].Item1, treasures[id].Item2, treasures[id].Item1.treasureDefine.value };
                        method?.Invoke(this, parameters);
                    }
                    break;
                default:
                    Debug.LogError("Invoke unknown invokeType Effect");
                    break;
            }
        } else
        {
            Debug.LogWarning("Invoke not contained Effect");
        }
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
            treasures[item.ID] = (treasures[item.ID].Item1, ++tempNum);
        }
        else
        {
            treasures.Add(item.ID, (item, 1));
        }
        treasuresUpdate.OnNext(Unit.Default);
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
            treasuresUpdate.OnNext(Unit.Default);
        }
    }

    public List<(StoreItemModel, int)> GetTreasuresList()
    {
        return treasures.Values.ToList();
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


    private void KingArrow(StoreItemModel item, int num, int value)
    {
        if (!equipClassEffect.ContainsKey(EquipClass.arch))
        {
            equipClassEffect.Add(EquipClass.arch, num * value);
        }
    }

    private void TreasureBowl(StoreItemModel item, int num, int value)
    {
        if (GameUtil.Instance.GetRandomRate_affected(value))
        {
            //存储失败，储蓄罐破裂
            GameManager.Instance.CoinChanged(item.treasureDefine.counter * 2);
            RemoveTreasure(item.ID);
        } else
        {
            GameManager.Instance.CoinChanged(-100);
            item.treasureDefine.counter += 100;
        }
    }

    private void ShieldToken(StoreItemModel item, int num, int value)
    {
        battleDisposablePool.SaveDisposable(item.uuid + "ShieldToken", BattleManager.Instance.roundManager.roundTime.AsObservable().TakeUntilDestroy(GameManager.Instance)
            .Where(round => {
                    var battleItem = GlobalAccess.GetBattleItem(round.Item1);
                    return round.Item2 == RoundTime.begin && BattleManager.Instance.roundManager.extraRound == 0 &&
                    battleItem.type == BattleItemType.player;
                })
            .Subscribe(round =>
            {
                var battleItem = GlobalAccess.GetBattleItem(round.Item1);
                battleItem.attributes.currentShield += value * num;
                GlobalAccess.SaveBattleItem(battleItem);
            }));
    }

    private void SwordStone(StoreItemModel item, int num, int value)
    {
        if (!equipClassEffect.ContainsKey(EquipClass.sword))
        {
            equipClassEffect.Add(EquipClass.sword, num * value);
        }
    }
}
