using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class BattleItemManager
{
    private BattleManager battleManager;

    public List<string> battleItemIDs = new List<string>();
    public List<string> PlayerItemIDs = new List<string>();
    public Dictionary<Vector2, UIBattleItem> pos_uibattleItemDic = new Dictionary<Vector2, UIBattleItem>();
    public Dictionary<string, Vector2> id_posDic = new Dictionary<string, Vector2>();

    //战斗开始时初始化
    public void Init(List<string> characterIDs, TownBattleInfoModel battleInfo)
    {
        battleManager = BattleManager.Instance;
        battleItemIDs.Clear();
        PlayerItemIDs.Clear();
        id_posDic.Clear();
        pos_uibattleItemDic.Clear();

        //时间Item没有位置，所以不加入pos_uibattleItemDic
        var timeItem = new BattleItem(BattleItemType.time);
        timeItem.BattleInit();
        GlobalAccess.SaveBattleItem(timeItem);
        battleItemIDs.Add(timeItem.uuid);

        for (int i = 0; i < characterIDs.Count; i++)
        {
            var battleItem = NorneStore.Instance.ObservableObject<CharacterModel>(new CharacterModel(characterIDs[i])).Value.ToBattleItem();
            battleItem.BattleInit();
            GlobalAccess.SaveBattleItem(battleItem);
            battleItemIDs.Add(battleItem.uuid);
            PlayerItemIDs.Add(battleItem.uuid);
        }

        foreach (var pair in battleInfo.enermys)
        {
            var battleItem = pair.Value.ToBattleItem(battleManager.difficultyExtraFactor + battleInfo.battleBaseDifficulty);
            battleItem.BattleInit();
            GlobalAccess.SaveBattleItem(battleItem);
            battleItemIDs.Add(battleItem.uuid);
            battleManager.chessboardManager.PlaceBattleItem(battleItem.uuid, battleManager.chessBoard.slots[pair.Key]);
        }

        foreach (var id in battleItemIDs)
        {
            var battleItem = GlobalAccess.GetBattleItem(id);
            foreach (var skillId in battleItem.skills)
            {
                if (DataManager.Instance.Skills.ContainsKey(skillId))
                {
                    var skill = DataManager.Instance.Skills[skillId];
                    if (skill.InvokeType == SkillInvokeType.battleStart)
                    {
                        SkillManager.Instance.InvokeSkill(id, skill.MethodName, skill.PropertyType, skill.Value);
                    }
                } else
                {
                    Debug.Log("skill: " + skillId + " not found!");
                    continue;
                }
            }
        }
    }

    public void ResortBattleItems()
    {
        var tempBattleItems = battleItemIDs.Select(uuid => GlobalAccess.GetBattleItem(uuid)).ToList();
        tempBattleItems.Sort((itemA, itemB) =>
        {
            return Mathf.CeilToInt(itemA.remainActingDistance / itemA.attributes.Speed)
                .CompareTo(Mathf.CeilToInt(itemB.remainActingDistance / itemB.attributes.Speed));
        });
        battleItemIDs = tempBattleItems.Select(item => item.uuid).ToList();
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
                if ((battleItemType & battleItem.battleItemType) != 0)
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
        PlayerItemIDs.RemoveAll(tempid => {
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
    }

    //离开战场（非死亡）
    public void CharacterLeave(string uuid)
    {
        battleItemIDs.RemoveAll(tempid => {
            var item = GlobalAccess.GetBattleItem(tempid);
            return item.uuid == uuid;
        });
        PlayerItemIDs.RemoveAll(tempid => {
            var item = GlobalAccess.GetBattleItem(tempid);
            return item.uuid == uuid;
        });
        if (id_posDic.ContainsKey(uuid))
        {
            UnityEngine.Object.Destroy(pos_uibattleItemDic[id_posDic[uuid]]);
        }
        pos_uibattleItemDic.Remove(id_posDic[uuid]);
        id_posDic.Remove(uuid);
    }
}
