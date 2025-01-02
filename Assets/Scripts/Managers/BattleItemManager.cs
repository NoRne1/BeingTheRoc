using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class BattleItemManager
{
    private BattleManager battleManager;

    private TownBattleInfoModel battleInfo;

    public List<string> battleItemIDs = new List<string>();
    //有行动值的battleItem
    public List<string> roundBattleItemIDs = new List<string>();
    public List<string> playerItemIDs = new List<string>();
    public List<string> enemyItemIDs = new List<string>();
    public string granaryItemID = "";
    public Dictionary<Vector2, UIBattleItem> pos_uibattleItemDic = new Dictionary<Vector2, UIBattleItem>();
    public Dictionary<string, Vector2> id_posDic = new Dictionary<string, Vector2>();

    public void AddItem(BattleItem item) 
    {
        battleItemIDs.Add(item.uuid);
        if (item.attributes.Speed > 0) { roundBattleItemIDs.Add(item.uuid); }
        if (item.type == BattleItemType.player) { playerItemIDs.Add(item.uuid); }
        if (item.type == BattleItemType.enemy) { enemyItemIDs.Add(item.uuid); }
        if (item.type == BattleItemType.granary) { granaryItemID = item.uuid; }
    }

    public void ClearItem() 
    {
        battleItemIDs.Clear();
        roundBattleItemIDs.Clear();
        playerItemIDs.Clear();
        enemyItemIDs.Clear();
        granaryItemID = "";
    }
    //战斗开始时初始化
    public void Init(List<string> characterIDs, TownBattleInfoModel battleInfo)
    {
        battleManager = BattleManager.Instance;
        this.battleInfo = battleInfo;
        float difficultyFactor = battleManager.difficultyExtraFactor + battleInfo.battleBaseDifficulty;
        ClearItem();
        id_posDic.Clear();
        pos_uibattleItemDic.Clear();

        //时间Item没有位置，所以不加入pos_uibattleItemDic
        var timeItem = new BattleItem(BattleItemType.time);
        timeItem.BattleInit();
        GlobalAccess.SaveBattleItem(timeItem);
        AddItem(timeItem);
        //granary Item速度为0，不出现在行动条上
        var granary = new BattleItem(BattleItemType.granary);
        granary.BattleInit();
        //for test
        granary.attributes.UpdateInitMaxHP((int)MathF.Min(5000, (difficultyFactor * 1000)));
        GlobalAccess.SaveBattleItem(granary);
        AddItem(granary);
        battleManager.chessboardManager.PlaceBattleItem(granary.uuid, battleManager.chessBoard.slots[battleInfo.granaryPos]);

        for (int i = 0; i < characterIDs.Count; i++)
        {
            var battleItem = NorneStore.Instance.ObservableObject<CharacterModel>(new CharacterModel(characterIDs[i])).Value.ToBattleItem();
            battleItem.BattleInit();
            GlobalAccess.SaveBattleItem(battleItem);
            AddItem(battleItem);
        }

        foreach (var pair in battleInfo.enermys)
        {
            var battleItem = pair.Value.ToBattleItem(difficultyFactor);
            battleItem.BattleInit();
            GlobalAccess.SaveBattleItem(battleItem);
            AddItem(battleItem);
            battleManager.chessboardManager.PlaceBattleItem(battleItem.uuid, battleManager.chessBoard.slots[pair.Key]);
        }
    }

    public void AddSupportEnermy(EnermyModel enermyModel)
    {
        float difficultyFactor = battleManager.difficultyExtraFactor + battleInfo.battleBaseDifficulty;
        var battleItem = enermyModel.ToBattleItem(difficultyFactor);
        battleItem.remainActingDistance = GlobalAccess.roundDistance;
        battleItem.BattleInit();
        GlobalAccess.SaveBattleItem(battleItem);
        AddItem(battleItem);
        battleManager.chessboardManager.PlaceBattleItem(battleItem.uuid, battleManager.chessboardManager.GetRandomAvailableSlot());
        battleManager.moveBarManager.RefreshMoveBar();
    }

    public void ResortBattleItems()
    {
        var tempBattleItems = roundBattleItemIDs.Select(uuid => GlobalAccess.GetBattleItem(uuid)).ToList();
        tempBattleItems.Sort((itemA, itemB) =>
        {
            return Mathf.CeilToInt(itemA.remainActingDistance / itemA.attributes.Speed)
                .CompareTo(Mathf.CeilToInt(itemB.remainActingDistance / itemB.attributes.Speed));
        });
        roundBattleItemIDs = tempBattleItems.Select(item => item.uuid).ToList();
    }

    public bool HasBattleItem(Vector2 vect)
    {
        return pos_uibattleItemDic.ContainsKey(vect);
    }

    public bool HasBattleItem(UIChessboardSlot slot)
    {
        return pos_uibattleItemDic.ContainsKey(slot.position);
    }

    public bool HasBattleItem(string uuid)
    {
        return id_posDic.ContainsKey(uuid);
    }

    public List<string> GetBattleItemsByRange(Vector2 pos, TargetRange range, BattleItemType battleItemType)
    {
        List<string> results = new List<string>();
        List<Vector2> vectList = GameUtil.Instance.GetTargetRangeList(pos, range);
        foreach (var vect in vectList)
        {
            if (pos_uibattleItemDic.ContainsKey(vect))
            {
                var battleItem = GlobalAccess.GetBattleItem(pos_uibattleItemDic[vect].itemID);
                if ((battleItemType & battleItem.type) != 0)
                {
                    results.Add(pos_uibattleItemDic[vect].itemID);
                }
            }
        }
        return results;
    }

    public Vector2 GetPosByUUid(string uuid)
    {
        var result = pos_uibattleItemDic.Where(item => item.Value.itemID == uuid).ToList();
        if (result.Count > 0)
        {
            return result.First().Key;
        } else
        {
            return Vector2.negativeInfinity;
        }
    }

    public UIBattleItem GetUIBattleItemByUUid(string uuid)
    {
        var result = pos_uibattleItemDic.Where(item => item.Value.itemID == uuid).ToList();
        if (result.Count > 0 && pos_uibattleItemDic.ContainsKey(result.First().Key))
        {
            return pos_uibattleItemDic[result.First().Key];
        } else
        {
            return null;
        }
    }

    public void CharacterDie(string uuid)
    {
        battleItemIDs.RemoveAll(tempid => {
            var item = GlobalAccess.GetBattleItem(tempid);
            return item.uuid == uuid;
        });
        roundBattleItemIDs.RemoveAll(tempid => {
            var item = GlobalAccess.GetBattleItem(tempid);
            return item.uuid == uuid;
        });
        playerItemIDs.RemoveAll(tempid => {
            var item = GlobalAccess.GetBattleItem(tempid);
            return item.uuid == uuid;
        });
        var pairs = pos_uibattleItemDic.Where(item => item.Value.itemID == uuid);
        foreach (var pair in pairs)
        {
            UnityEngine.Object.Destroy(pair.Value.gameObject);
            pos_uibattleItemDic.Remove(pair.Key);
        }
        GameManager.Instance.RemoveCharacter(uuid);
        battleManager.moveBarManager.RefreshMoveBar();
    }

    //离开战场（非死亡）
    public void CharacterLeave(string uuid)
    {
        battleItemIDs.RemoveAll(tempid => {
            var item = GlobalAccess.GetBattleItem(tempid);
            return item.uuid == uuid;
        });
        roundBattleItemIDs.RemoveAll(tempid => {
            var item = GlobalAccess.GetBattleItem(tempid);
            return item.uuid == uuid;
        });
        playerItemIDs.RemoveAll(tempid => {
            var item = GlobalAccess.GetBattleItem(tempid);
            return item.uuid == uuid;
        });
        if (id_posDic.ContainsKey(uuid))
        {
            UnityEngine.Object.Destroy(pos_uibattleItemDic[id_posDic[uuid]]);
        }
        pos_uibattleItemDic.Remove(id_posDic[uuid]);
        id_posDic.Remove(uuid);
        battleManager.moveBarManager.RefreshMoveBar();
    }

    public void BattleEnd()
    {
        foreach (var id in battleItemIDs)
        {
            var item = GlobalAccess.GetBattleItem(id);
            item.BattleEnd();
            NorneStore.Instance.Remove(item);
        }
    }
}
