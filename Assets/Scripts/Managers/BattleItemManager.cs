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
    public string playerGranaryID = "";
    public string enemyGranaryID = "";
    public Dictionary<Vector2, UIBattleItem> pos_uibattleItemDic = new Dictionary<Vector2, UIBattleItem>();
    public Dictionary<string, Vector2> id_posDic = new Dictionary<string, Vector2>();

    public BuffDefine currentWeatherBuff;

    public void AddItem(BattleItem item) 
    {
        battleItemIDs.Add(item.uuid);
        if (item.attributes.Speed > 0) { roundBattleItemIDs.Add(item.uuid); }
        if (item.isPlayer) { playerItemIDs.Add(item.uuid); }
        if (item.isEnemy) { enemyItemIDs.Add(item.uuid); }
        if (item.type == BattleItemType.granary && item.side == BattleItemSide.player) { playerGranaryID = item.uuid; }
        if (item.type == BattleItemType.granary && item.side == BattleItemSide.enemy) { enemyGranaryID = item.uuid; }
    }

    public void ClearItem() 
    {
        battleItemIDs.Clear();
        roundBattleItemIDs.Clear();
        playerItemIDs.Clear();
        enemyItemIDs.Clear();
        enemyGranaryID = "";
    }
    //战斗开始时初始化
    public void Init(int granaryOpacity, List<string> characterIDs, TownBattleInfoModel battleInfo)
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
        var playerGranary = new BattleItem(BattleItemType.granary);
        playerGranary.side = BattleItemSide.player;
        playerGranary.BattleInit();
        playerGranary.attributes.UpdateInitMaxHP(granaryOpacity);
        GlobalAccess.SaveBattleItem(playerGranary);
        AddItem(playerGranary);

        var enemyGranary = new BattleItem(BattleItemType.granary);
        enemyGranary.side = BattleItemSide.enemy;
        enemyGranary.BattleInit();
        //for test
        enemyGranary.attributes.UpdateInitMaxHP((int)MathF.Min(5000, (difficultyFactor * 1000)));
        GlobalAccess.SaveBattleItem(enemyGranary);
        AddItem(enemyGranary);
        battleManager.chessboardManager.PlaceBattleItem(enemyGranary.uuid, battleManager.chessBoard.slots[battleInfo.granaryPos]);

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

        RefreshWeatherEffect();
    }

    public void AddSupportEnermy(EnermyModel enermyModel)
    {
        float difficultyFactor = battleManager.difficultyExtraFactor + battleInfo.battleBaseDifficulty;
        var battleItem = enermyModel.ToBattleItem(difficultyFactor);
        battleItem.remainActingDistance = GlobalAccess.roundDistance;
        battleItem.BattleInit();
        ProcessWeatherBuff(battleItem.uuid);
        GlobalAccess.SaveBattleItem(battleItem);
        AddItem(battleItem);
        battleManager.chessboardManager.PlaceBattleItem(battleItem.uuid, battleManager.chessboardManager.GetRandomAvailableSlot());
        battleManager.moveBarManager.RefreshMoveBar();
    }

    public void AddQuitTimeItem()
    {
        var quitTimeItem = new BattleItem(BattleItemType.quitTime);
        quitTimeItem.BattleInit();
        quitTimeItem.remainActingDistance = GlobalAccess.roundDistance;
        GlobalAccess.SaveBattleItem(quitTimeItem);
        AddItem(quitTimeItem);
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

    //新一天开始时触发
    public void RefreshWeatherEffect()
    {     
        if (GameManager.Instance.currentWeather.effect.effectType == WeatherEffectType.battleBuff)
        {
            currentWeatherBuff = GameUtil.Instance.DeepCopy(DataManager.Instance.BuffDefines[GameManager.Instance.currentWeather.effect.buffID]);
            currentWeatherBuff.Duration = -1;
        } else {
            currentWeatherBuff = null;
        }
        foreach(var battleItemID in playerItemIDs)
        {
            ProcessWeatherBuff(battleItemID);
        }
        foreach(var battleItemID in enemyItemIDs)
        {
            ProcessWeatherBuff(battleItemID);
        }
    }

    //包含了删除旧的和添加新的
    public void ProcessWeatherBuff(string battleItemID)
    {
        var item = GlobalAccess.GetBattleItem(battleItemID);
        BuffModel buffModel = new BuffModel(currentWeatherBuff, battleItemID, "-1", BuffType.weather);
        buffModel.DecreaseTime = BuffDecreaseTime.none;
        //先清除旧的
        item.buffCenter.RemoveBuff(BuffType.weather);
        if (currentWeatherBuff != null)
        {
            //不需要SaveBattleItem,因为AddBuff触发battleItemUpdate
            item.buffCenter.AddBuff(buffModel);
        }
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
