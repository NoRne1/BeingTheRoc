using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct EnemyAIWeightFactor
{
    public float taunt;
    //攻击的收益
    public float attackProceeds;
    //受到攻击的威胁
    public float attackThreaten;
    //考虑buff流派的时候用
    // public float damageThreaten;
    public float targetHP;

    public EnemyAIWeightFactor(float taunt = 1, float attackProceeds = 1, float attackThreaten = 1, float targetHP = 1)
    {
        this.taunt = taunt;
        this.attackProceeds = attackProceeds;
        this.attackThreaten = attackThreaten;
        this.targetHP = targetHP;
    }
}

public abstract class EnemyAI
{
    protected BattleManager battleManager = BattleManager.Instance;
    public abstract void TurnAction(string uuid);
    public List<(Vector2, BattleItem)> getPlayerItems()
    {
        List<(Vector2, BattleItem)> playerItems = new List<(Vector2, BattleItem)>();
        battleManager.battleItemManager.PlayerItemIDs.ForEach(id =>
        {
            var battleItem = GlobalAccess.GetBattleItem(id);
            if (battleManager.battleItemManager.id_posDic.ContainsKey(id)) 
            {
                playerItems.Add((battleManager.battleItemManager.id_posDic[id], battleItem));
            } else 
            {
                Debug.LogError("PlayerItemIDs contains id not in id_posDic");
            }
        });
        return playerItems;
    }
    // (energy, minEnergyPoints)
    //获得到达进攻位置最小行动消耗的能量，以及该能量消耗下所有可能的进攻位置
    private (int, List<Vector2>) GetMinEnergyAttackPositions(List<Vector2> possiblePositions, Vector2 attackerPos, int mobility)
    {
        List<Vector2> minEnergyPositions = new List<Vector2>();
        int minEnergy = int.MaxValue;

        foreach (var pos in possiblePositions)
        {
            var result = BattleCommonMethods.CanMoveTo(attackerPos, pos, int.MaxValue, battleManager.battleItemManager.pos_uibattleItemDic.Keys.ToList());
            if (!result.Item1)
            {
                continue;
            }
            int distance = result.Item2.Count - 1;
            int tempEnergy = Mathf.CeilToInt(distance * 1.0f / mobility);

            if (tempEnergy < minEnergy)
            {
                minEnergy = tempEnergy;
                minEnergyPositions.Clear();
                minEnergyPositions.Add(pos);
            }
            else if (tempEnergy == minEnergy)
            {
                minEnergyPositions.Add(pos);
            }
        }

        return (minEnergy, minEnergyPositions);
    }

    public (float, float, Vector2) CalculateAttackProceedsAndThreaten(Vector2 attackerPos, BattleItem attacker, Vector2 targetPos, BattleItem taeget, Dictionary<Vector2, int> threatenMap)
    {
        //因为是正则化到[0，1]的数据，所以1表示最大值
        float attackProceeds = 0;
        float attackedThreaten = 1;
        Vector2 bestPostion = Vector2.negativeInfinity;
        StoreItemModel threatenEquip = attacker.backpack.GetMostThreatenEquip();
        if (threatenEquip != null)
        {
            var possibleAttackPositions = GameUtil.Instance.GetTargetRangeList(targetPos, threatenEquip.equipDefine.targetRange);
            var result = GetMinEnergyAttackPositions(possibleAttackPositions, attackerPos, attacker.attributes.Mobility);
            int remainingEnergy = attacker.attributes.Energy - result.Item1;
            if (remainingEnergy > 0)
            {
                attackProceeds = remainingEnergy * 1.0f / attacker.attributes.Energy;
            }
            var maxThreaten = threatenMap.Values.Max();
            foreach (var pos in result.Item2)
            {
                if (threatenMap.ContainsKey(pos) && threatenMap[pos] * 1.0f / maxThreaten < attackedThreaten)
                {   
                    attackedThreaten = threatenMap[pos] * 1.0f / maxThreaten;
                    bestPostion = pos;
                }
            }
        }
        return (attackProceeds, attackedThreaten, bestPostion);
    }

    public Dictionary<Vector2, int> CalculateThreatenMap(Vector2 pos, BattleItem player)
    {
        Dictionary<Vector2, int> threatenDict = new Dictionary<Vector2, int>();
        StoreItemModel threatenEquip = player.backpack.GetMostThreatenEquip();
        if (threatenEquip != null)
        {
            foreach (var x in Enumerable.Range(0, 8))
            {
                foreach (var y in Enumerable.Range(0, 8))
                {
                    if (!battleManager.battleItemManager.HasBattleItem(new Vector2(x, y)))
                    {
                        //该位置可以移动到
                        Vector2 tempVector = new Vector2(x, y);
                        int distance = Mathf.Abs((int)(pos.x - tempVector.x)) + Mathf.Abs((int)(pos.y - tempVector.y));
                        int remainingEnergy = player.attributes.Energy - Mathf.CeilToInt(distance * 1.0f / player.attributes.Mobility);
                        if (remainingEnergy > 0)
                        {
                            // Calculate damage for positions within attack range
                            List<Vector2> attackPositions = GameUtil.Instance.GetTargetRangeList(tempVector, threatenEquip.equipDefine.targetRange);
                            foreach (var attackPos in attackPositions)
                            {
                                int maxDamage = threatenEquip.equipDefine.attackThreaten * remainingEnergy;
                                if (threatenDict.ContainsKey(attackPos))
                                {
                                    threatenDict[attackPos] = Mathf.Max(threatenDict[attackPos], maxDamage);
                                }
                                else
                                {
                                    threatenDict[attackPos] = maxDamage;
                                }
                            }
                        }
                    }
                }
            }
        }
        return threatenDict;
    }

    public bool MoveToPosition(string uuid, Vector2 des) 
    {
        var battleItem = GlobalAccess.GetBattleItem(uuid);
        var tempPos = battleManager.battleItemManager.GetPosByUUid(uuid);
        var result = BattleCommonMethods.CanMoveTo(tempPos, des,
        battleItem.attributes.Mobility * battleItem.attributes.currentEnergy, battleManager.battleItemManager.pos_uibattleItemDic.Keys.ToList());
        if (!battleItem.isConfine && battleItem.attributes.currentEnergy > 0 && result.Item1)
        {
            // 移动成功
            Debug.Log("EnemyAI move success to :" + des);
            battleManager.battleItemManager.pos_uibattleItemDic.Add(des, battleManager.battleItemManager.pos_uibattleItemDic[tempPos]);
            battleManager.battleItemManager.pos_uibattleItemDic.Remove(tempPos);
            battleManager.battleItemManager.id_posDic.Remove(uuid);
            battleManager.battleItemManager.id_posDic.Add(uuid, des);

            battleManager.chessboardManager.EnqueueAction(new ChessboardAction(new MoveInfo(battleManager.battleItemManager.pos_uibattleItemDic[des], 
                result.Item2)));
                
            battleItem.attributes.currentEnergy -= Mathf.CeilToInt((result.Item2.Count - 1) * 1.0f / battleItem.attributes.Mobility);
            GlobalAccess.SaveBattleItem(battleItem);
            battleItem.moveSubject.OnNext(des);
            return true;
        } else {
            Debug.Log("EnemyAI move failed to :" + des);
            return false;
        }
    }
}

public class TankAI: EnemyAI
{
    public EnemyAIWeightFactor weightFactor = new EnemyAIWeightFactor(1, 1, 1, 1);
    public override void TurnAction(string uuid)
    {
        Debug.Log("TankAI TurnAction");
        battleManager.roundManager.roundTime.OnNext((uuid, RoundTime.end));
    }
}

public class WarriorAI : EnemyAI
{
    public override void TurnAction(string uuid)
    {
        Debug.Log("WarriorAI TurnAction");
        battleManager.roundManager.roundTime.OnNext((uuid, RoundTime.end));
    }
}

public class AssassinAI: EnemyAI
{
    public EnemyAIWeightFactor weightFactor = new EnemyAIWeightFactor(1, 1.5f, 1, 1.5f);
    public override void TurnAction(string uuid)
    {
        var selfItem = GlobalAccess.GetBattleItem(uuid);
        var selfPos = battleManager.battleItemManager.id_posDic[uuid];
        var players = getPlayerItems();
        //totalThreatenDict
        Dictionary<Vector2, int> totalThreatenDict = new Dictionary<Vector2, int>();
        foreach (var player in players)
        {
            Dictionary<Vector2, int> threatenDict = CalculateThreatenMap(player.Item1, player.Item2);
            DictionaryExtensions.AddDictionaries(totalThreatenDict, threatenDict);
        }
        //totalTaunt
        float totalTaunt = 0;
        foreach (var player in players)
        {
            totalTaunt += player.Item2.attributes.Taunt;
        }
        //target & maxScore
        (Vector2, BattleItem) target;
        float maxScore = 0;
        Vector2 bestPos = Vector2.negativeInfinity;
        foreach (var player in players)
        {
            var attackProceedsAndThreatenResult = CalculateAttackProceedsAndThreaten(selfPos, selfItem, player.Item1, player.Item2, totalThreatenDict);
            float tempScore = player.Item2.attributes.Taunt / totalTaunt * weightFactor.taunt
                + attackProceedsAndThreatenResult.Item1 * weightFactor.attackProceeds
                + attackProceedsAndThreatenResult.Item2 * weightFactor.attackThreaten
                + player.Item2.attributes.currentHP / player.Item2.attributes.MaxHP * weightFactor.targetHP;
            if (tempScore > maxScore) 
            {
                maxScore = tempScore;
                target = player;
                bestPos = attackProceedsAndThreatenResult.Item3;
            } 
        }
        if (bestPos != Vector2.negativeInfinity)
        {
            //移动攻击
            MoveToPosition(uuid, bestPos);
            var threatenEquips = selfItem.backpack.GetEquipsSortedByThreaten();
            var protectEquips = selfItem.backpack.GetEquipsSortedByProtectAbility();
            var currentEnergy = selfItem.attributes.currentEnergy;
            var currentThreaten = totalThreatenDict[bestPos];
            while (currentEnergy > 0) 
            {
                StoreItemModel bestEquip = null;
                int bestScore = 0; 
                foreach (var equip in threatenEquips)
                {
                    if (currentEnergy >= equip.equipDefine.takeEnergy)
                    {
                        //使用威胁最大的装备
                        bestScore = equip.equipDefine.attackThreaten;
                        bestEquip = equip;
                        break;
                    }
                }
                foreach (var equip in protectEquips)
                {
                    if (currentEnergy >= equip.equipDefine.takeEnergy)
                    {
                        var tempScore = Mathf.Min(currentThreaten, equip.equipDefine.protectAbility);
                        //使用保护能力最大的装备
                        if (tempScore > bestScore)
                        {
                            currentThreaten -= tempScore;
                            bestScore = equip.equipDefine.protectAbility;
                            bestEquip = equip;
                            break;
                        }
                    }
                }
                //范围武器支持
                // battleManager.chessboardManager.EnqueueAction(new ChessboardAction(new EquipUseInfo()));
                currentEnergy -= bestEquip.equipDefine.takeEnergy;
            }
        }
        battleManager.chessboardManager.EnqueueAction(new ChessboardAction(new EndRoundInfo(uuid)));
    }
}

public class MagicianAI : EnemyAI
{
    public EnemyAIWeightFactor weightFactor = new EnemyAIWeightFactor(1, 1, 1, 1);
    public override void TurnAction(string uuid)
    {
        Debug.Log("MagicianAI TurnAction");
        battleManager.roundManager.roundTime.OnNext((uuid, RoundTime.end));
    }
}

public class PastorAI : EnemyAI
{
    public override void TurnAction(string uuid)
    {
        Debug.Log("PastorAI TurnAction");
        battleManager.roundManager.roundTime.OnNext((uuid, RoundTime.end));
    }
}
