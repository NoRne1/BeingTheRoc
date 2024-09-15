using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public struct EnemyAIWeightFactor
{
    public float taunt;
    //攻击的收益
    public float attackProceeds;
    //攻击的收益
    public float protectProceeds;
    //受到攻击的威胁
    public float attackThreaten;
    //考虑buff流派的时候用
    // public float damageThreaten;
    public float targetHP;

    public EnemyAIWeightFactor(float taunt = 1, float attackProceeds = 1, float protectProceeds = 1, float attackThreaten = 1, float targetHP = 1)
    {
        this.taunt = taunt;
        this.attackProceeds = attackProceeds;
        this.protectProceeds = protectProceeds;
        this.attackThreaten = attackThreaten;
        this.targetHP = targetHP;
    }
}

public abstract class EnemyAI
{
    protected BattleManager battleManager = BattleManager.Instance;
    public IEnumerator TurnAction(string uuid) 
    {
        while (true) 
        {
            var selfItem = GlobalAccess.GetBattleItem(uuid);
            if (selfItem.attributes.currentEnergy > 0)
            {
                yield return battleManager.StartCoroutine(TurnOnceAction(uuid));
            }
            else 
            {
                battleManager.roundManager.roundTime.OnNext((uuid, RoundTime.end));
                break;
            }
        }
    }
    public abstract IEnumerator TurnOnceAction(string uuid);
    public List<(Vector2, BattleItem)> getBattleItems(BattleItemType type)
    {
        List<(Vector2, BattleItem)> playerItems = new List<(Vector2, BattleItem)>();
        battleManager.battleItemManager.battleItemIDs.ForEach(id =>
        {
            var battleItem = GlobalAccess.GetBattleItem(id);
            if (battleItem.battleItemType == type && battleManager.battleItemManager.id_posDic.ContainsKey(id)) 
            {
                playerItems.Add((battleManager.battleItemManager.id_posDic[id], battleItem));
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

    //attackProceeds, attackedThreaten, bestPostion
    public (float, float, Vector2) CalculateAttackProceedsAndThreaten(Vector2 attackerPos, BattleItem attacker, Vector2 targetPos, BattleItem target, StoreItemModel equip, float maxAttackThreaten, Dictionary<Vector2, int> threatenMap)
    {
        //因为是正则化到[0，1]的数据，所以1表示最大值
        float attackProceeds = 0;
        float attackedThreaten = 1;
        Vector2 bestPostion = Vector2.negativeInfinity;
        if (equip != null)
        {
            var possibleAttackPositions = GameUtil.Instance.GetTargetRangeList(targetPos, equip.equipDefine.targetRange);
            var result = GetMinEnergyAttackPositions(possibleAttackPositions, attackerPos, attacker.attributes.Mobility);
            int remainingEnergy = attacker.attributes.currentEnergy - result.Item1;
            if (remainingEnergy > 0)
            {
                int attackTakeEnergy = (remainingEnergy / equip.equipDefine.takeEnergy) * equip.equipDefine.takeEnergy;
                attackProceeds = (equip.equipDefine.attackThreaten * attackTakeEnergy * 1.0f) / (maxAttackThreaten * attacker.attributes.Energy);
                if(threatenMap.Count > 0) 
                {
                    var maxThreaten = threatenMap.Values.Max();
                    foreach (var pos in result.Item2)
                    {
                        if (threatenMap.ContainsKey(pos)) 
                        {
                            if (threatenMap[pos] * 1.0f / maxThreaten <= attackedThreaten)
                            {
                                attackedThreaten = threatenMap[pos] * 1.0f / maxThreaten;
                                bestPostion = pos;
                            }
                        } else 
                        {
                            attackedThreaten = 0;
                            bestPostion = pos;
                            break;
                        }
                    }
                } else 
                {
                    attackedThreaten = 0;
                    bestPostion = result.Item2.Count > 0 ? result.Item2.First() : Vector2.negativeInfinity;
                }
            }
        }
        return (attackProceeds, attackedThreaten, bestPostion);
    }

    //protectProceeds, attackedThreaten, bestPostion
    public (float, float, Vector2) CalculateProtectProceedsAndThreaten(Vector2 protectorPos, BattleItem protector, Vector2 targetPos, BattleItem target, StoreItemModel equip, float maxProtectAbility, Dictionary<Vector2, int> threatenMap)
    {
        //因为是正则化到[0，1]的数据，所以1表示最大值
        float protectProceeds = 0;
        float protectedThreaten = 1;
        Vector2 bestPostion = Vector2.negativeInfinity;
        if (equip != null)
        {
            var possibleProtectPositions = GameUtil.Instance.GetTargetRangeList(targetPos, equip.equipDefine.targetRange);
            var result = GetMinEnergyAttackPositions(possibleProtectPositions, protectorPos, protector.attributes.Mobility);
            int remainingEnergy = protector.attributes.Energy - result.Item1;
            if (remainingEnergy > 0)
            {
                var maxThreaten = threatenMap.Values.Max();
                foreach (var pos in result.Item2)
                {
                    if (threatenMap.ContainsKey(pos) && threatenMap[pos] * 1.0f / maxThreaten < protectedThreaten)
                    {   
                        protectedThreaten = threatenMap[pos] * 1.0f / maxThreaten;
                        bestPostion = pos;
                    }
                }
                if (bestPostion != Vector2.negativeInfinity) 
                {
                    var protectAbility = equip.equipDefine.protectAbility * remainingEnergy;
                    var truelyProceeds = threatenMap.ContainsKey(targetPos) ? Mathf.Min(protectAbility, threatenMap[targetPos]) : 0;
                    protectProceeds = truelyProceeds * 1.0f / (maxProtectAbility * protector.attributes.Energy);  
                }
            }
        }
        return (protectProceeds, protectedThreaten, bestPostion);
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
                    //不考虑该位置有没有其他物体阻挡，能不能到达，都去计算，会导致不太精确，从而造成ai有时不够聪明，恰好符合需求
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
        return threatenDict;
    }

    //单步移动（仅移动一个行动点）
    public IEnumerator MoveToPositionOneStep(string uuid, Vector2 des) 
    {
        var battleItem = GlobalAccess.GetBattleItem(uuid);
        var tempPos = battleManager.battleItemManager.GetPosByUUid(uuid);
        var result = BattleCommonMethods.CanMoveTo(tempPos, des,
        int.MaxValue, battleManager.battleItemManager.pos_uibattleItemDic.Keys.ToList());
        if (!battleItem.isConfine && battleItem.attributes.currentEnergy > 0 && result.Item1)
        {
            // 移动成功
            var actualDesPos = result.Item2.Count > battleItem.attributes.Mobility ? result.Item2[battleItem.attributes.Mobility] : des;
            Debug.Log("EnemyAI move to: " + des + ", success to: " + actualDesPos);
            yield return battleManager.StartCoroutine(battleManager.chessboardManager.ItemMove(uuid, actualDesPos, null));
        } else {
            Debug.Log("EnemyAI move failed to :" + des);
        }
    }

    public Vector2 FindBestDes(Vector2 selfPos, Vector2 targetPos, int mobility, StoreItemModel equip, Dictionary<Vector2, int> threatenMap) 
    {
        var possiblePos = GameUtil.Instance.GetTargetRangeList(targetPos, equip.equipDefine.targetRange);
        int minAccessEnergy = int.MaxValue;
        float minThreaten = int.MaxValue;
        Vector2 result = Vector2.negativeInfinity;
        foreach(var pos in possiblePos)
        {
            if (!battleManager.battleItemManager.HasBattleItem(pos)) 
            {
                var tempThreaten = threatenMap.ContainsKey(pos) ? threatenMap[pos] : 0;
                var moveResult = BattleCommonMethods.CanMoveTo(selfPos, pos, int.MaxValue, battleManager.battleItemManager.pos_uibattleItemDic.Keys.ToList());
                if (moveResult.Item1) 
                {
                    //可以到达
                    var tempAccessEnergy = Mathf.CeilToInt((moveResult.Item2.Count - 1) * 1.0f / mobility);
                    if (tempAccessEnergy < minAccessEnergy) 
                    {
                        minThreaten = tempThreaten;
                        result = pos;
                    } else if (tempAccessEnergy == minAccessEnergy)
                    {
                        if (tempThreaten < minThreaten) 
                        {
                            minThreaten = tempThreaten;
                            result = pos;
                        }
                    }
                }
            }
        }
        return result;
    }
}

public class TankAI: EnemyAI
{
    public EnemyAIWeightFactor weightFactor = new EnemyAIWeightFactor(1, 1, 1, 1);
    public override IEnumerator TurnOnceAction(string uuid)
    {
        Debug.Log("TankAI TurnOnceAction");
        var selfItem = GlobalAccess.GetBattleItem(uuid);
        var selfPos = battleManager.battleItemManager.id_posDic[uuid];
        var players = getBattleItems(BattleItemType.player);
        var enemys = getBattleItems(BattleItemType.enemy);
        //totalThreatenDict
        Dictionary<Vector2, int> totalThreatenDict = new Dictionary<Vector2, int>();
        foreach (var player in players)
        {
            Dictionary<Vector2, int> threatenDict = CalculateThreatenMap(player.Item1, player.Item2);
            totalThreatenDict = DictionaryExtensions.AddDictionaries(totalThreatenDict, threatenDict);
        }
        //totalTaunt
        float totalTaunt = 0;
        foreach (var player in players)
        {
            totalTaunt += player.Item2.attributes.Taunt;
        }

        //target & maxScore
        (Vector2, BattleItem) target = (Vector2.negativeInfinity, null);
        float maxScore = 0;
        Vector2 bestPos = Vector2.negativeInfinity;
        StoreItemModel bestEquip = null;
        var threatenEquips = selfItem.backpack.GetEquipsSortedByThreaten();
        float maxAttackThreaten = -1;
        if(threatenEquips.Count > 0) 
        {
            maxAttackThreaten = threatenEquips.FirstOrDefault().equipDefine.attackThreaten;
            foreach(var equip in threatenEquips)
            {
                foreach (var player in players)
                {
                    var attackProceedsAndThreatenResult = CalculateAttackProceedsAndThreaten(selfPos, selfItem, player.Item1, player.Item2, equip, maxAttackThreaten, totalThreatenDict);
                    float tempScore = 0;
                    if (attackProceedsAndThreatenResult.Item1 > 0) 
                    {
                        //有攻击收益才是有效的行动，否则会由其他逻辑处理
                        tempScore = player.Item2.attributes.Taunt * 1.0f / totalTaunt * weightFactor.taunt
                        + attackProceedsAndThreatenResult.Item1 * weightFactor.attackProceeds
                        + (1 - attackProceedsAndThreatenResult.Item2) * weightFactor.attackThreaten
                        + player.Item2.attributes.currentHP * 1.0f / player.Item2.attributes.MaxHP * weightFactor.targetHP;
                    }
                    if (tempScore > maxScore) 
                    {
                        maxScore = tempScore;
                        target = player;
                        bestPos = attackProceedsAndThreatenResult.Item3;
                        bestEquip = equip;
                    } 
                }
            }
        }
        
        var protectEquips = selfItem.backpack.GetEquipsSortedByProtectAbility();
        float maxProtectAbility = -1;
        if (protectEquips.Count > 0)
        {
            maxProtectAbility = protectEquips.FirstOrDefault().equipDefine.protectAbility;
            foreach(var equip in protectEquips)
            {
                foreach (var enemy in enemys)
                {
                    var protectProceedsAndThreatenResult = CalculateProtectProceedsAndThreaten(selfPos, selfItem, enemy.Item1, enemy.Item2, equip, maxProtectAbility, totalThreatenDict);
                    float tempScore = 0;
                    if (protectProceedsAndThreatenResult.Item1 > 0) 
                    {
                        //有保护收益才是有效的行动，否则会由其他逻辑处理
                        tempScore = enemy.Item2.attributes.Taunt * 1.0f / totalTaunt * weightFactor.taunt
                        + protectProceedsAndThreatenResult.Item1 * weightFactor.protectProceeds
                        + (1 - protectProceedsAndThreatenResult.Item2) * weightFactor.attackThreaten
                        + enemy.Item2.attributes.currentHP * 1.0f / enemy.Item2.attributes.MaxHP * weightFactor.targetHP;
                    }
                    if (tempScore > maxScore) 
                    {
                        maxScore = tempScore;
                        target = enemy;
                        bestPos = protectProceedsAndThreatenResult.Item3;
                        bestEquip = equip;
                    } 
                }
            }
        }
        
        if (maxScore > 0) 
        {
            if (selfPos != bestPos) 
            {
                yield return battleManager.StartCoroutine(MoveToPositionOneStep(uuid, bestPos));
            } else 
            {
                if (bestEquip != null) 
                {
                    yield return battleManager.StartCoroutine(ItemUseManager.Instance.ProcessEquipUse(uuid, bestEquip, 
                        new List<string>{target.Item2.uuid}, Vector2.negativeInfinity));
                }
            }
        } else 
        {
            //没有直接收益
            float targetMaxScore = 0;
            Vector2 moveTargetPos = Vector2.negativeInfinity;
            StoreItemModel equip = null;
            
            //找最佳目标
            if (maxAttackThreaten >= maxProtectAbility) 
            {
                foreach (var player in players)
                {
                    var tempScore = player.Item2.attributes.Taunt * 1.0f / totalTaunt * weightFactor.taunt
                            + player.Item2.attributes.currentHP * 1.0f / player.Item2.attributes.MaxHP * weightFactor.targetHP;
                    if (tempScore > targetMaxScore) 
                    {
                        targetMaxScore = tempScore;
                        moveTargetPos = player.Item1;
                        equip = threatenEquips.FirstOrDefault();
                    }
                }
            } else 
            {
                foreach (var enemy in enemys)
                {
                    float tempScore = 0;
                    if (totalThreatenDict.ContainsKey(enemy.Item1)) 
                    {
                        var leftHp = enemy.Item2.attributes.currentHP * 1.0f - totalThreatenDict[enemy.Item1];
                        tempScore = leftHp / enemy.Item2.attributes.MaxHP;
                        if (leftHp <= 0)
                        {
                            tempScore += 999;
                        }
                    } else 
                    {
                        tempScore = enemy.Item2.attributes.currentHP * 1.0f / enemy.Item2.attributes.MaxHP;
                    }
                    if (tempScore > targetMaxScore) 
                    {
                        targetMaxScore = tempScore;
                        moveTargetPos = enemy.Item1;
                        equip = protectEquips.FirstOrDefault();
                    }
                }
            }
            //向目标移动
            if (targetMaxScore > 0 && moveTargetPos != Vector2.negativeInfinity && equip != null) 
            {
                Vector2 des = FindBestDes(selfPos, moveTargetPos, selfItem.attributes.Mobility, equip, totalThreatenDict);
                if (des != Vector2.negativeInfinity) 
                {
                    yield return battleManager.StartCoroutine(MoveToPositionOneStep(uuid, des));
                }
            }
        }
        yield return null;
    }
}

public class WarriorAI : EnemyAI
{
    public override IEnumerator TurnOnceAction(string uuid)
    {
        Debug.Log("WarriorAI TurnOnceAction");
        yield return null;
    }
}

public class AssassinAI: EnemyAI
{
    public EnemyAIWeightFactor weightFactor = new EnemyAIWeightFactor(1, 1.5f, 1, 1.5f);
    public override IEnumerator TurnOnceAction(string uuid)
    {
        Debug.Log("AssassinAI TurnOnceAction");
        yield return null;
        // var selfItem = GlobalAccess.GetBattleItem(uuid);
        // var selfPos = battleManager.battleItemManager.id_posDic[uuid];
        // var players = getBattleItems(BattleItemType.player);
        // //totalThreatenDict
        // Dictionary<Vector2, int> totalThreatenDict = new Dictionary<Vector2, int>();
        // foreach (var player in players)
        // {
        //     Dictionary<Vector2, int> threatenDict = CalculateThreatenMap(player.Item1, player.Item2);
        //     DictionaryExtensions.AddDictionaries(totalThreatenDict, threatenDict);
        // }
        // //totalTaunt
        // float totalTaunt = 0;
        // foreach (var player in players)
        // {
        //     totalTaunt += player.Item2.attributes.Taunt;
        // }
        // //target & maxScore
        // (Vector2, BattleItem) target = (Vector2.negativeInfinity, null);
        // float maxScore = 0;
        // Vector2 bestPos = Vector2.negativeInfinity;
        // var threatenEquips = selfItem.backpack.GetEquipsSortedByThreaten();
        // float maxAttackThreaten = threatenEquips.FirstOrDefault().equipDefine.attackThreaten;
        // foreach (var player in players)
        // {
        //     var attackProceedsAndThreatenResult = CalculateAttackProceedsAndThreaten(selfPos, selfItem, player.Item1, player.Item2, threatenEquips.FirstOrDefault(), maxAttackThreaten, totalThreatenDict);
        //     float tempScore = player.Item2.attributes.Taunt * 1.0f / totalTaunt * weightFactor.taunt
        //         + attackProceedsAndThreatenResult.Item1 * weightFactor.attackProceeds
        //         + attackProceedsAndThreatenResult.Item2 * weightFactor.attackThreaten
        //         + player.Item2.attributes.currentHP * 1.0f / player.Item2.attributes.MaxHP * weightFactor.targetHP;
        //     if (tempScore > maxScore) 
        //     {
        //         maxScore = tempScore;
        //         target = player;
        //         bestPos = attackProceedsAndThreatenResult.Item3;
        //     } 
        // }
        // if (bestPos != Vector2.negativeInfinity)
        // {
        //     if (bestPos != selfPos)
        //     {
        //         //移动
        //         yield return battleManager.StartCoroutine(MoveToPositionOneStep(uuid, bestPos));
        //     } else 
        //     {
        //         var threatenEquips = selfItem.backpack.GetEquipsSortedByThreaten();
        //         var protectEquips = selfItem.backpack.GetEquipsSortedByProtectAbility();
        //         var currentEnergy = selfItem.attributes.currentEnergy;
        //         var currentThreaten = totalThreatenDict[bestPos];
        //         StoreItemModel bestEquip = null;
        //         int bestScore = 0; 
        //         foreach (var equip in threatenEquips)
        //         {
        //             if (currentEnergy >= equip.equipDefine.takeEnergy)
        //             {
        //                 //使用威胁最大的装备
        //                 bestScore = equip.equipDefine.attackThreaten;
        //                 bestEquip = equip;
        //                 break;
        //             }
        //         }
        //         foreach (var equip in protectEquips)
        //         {
        //             if (currentEnergy >= equip.equipDefine.takeEnergy)
        //             {
        //                 var tempScore = Mathf.Min(currentThreaten, equip.equipDefine.protectAbility);
        //                 //使用保护能力最大的装备
        //                 if (tempScore > bestScore)
        //                 {
        //                     currentThreaten -= tempScore;
        //                     bestScore = equip.equipDefine.protectAbility;
        //                     bestEquip = equip;
        //                     break;
        //                 }
        //             }
        //         }
        //         yield return battleManager.StartCoroutine(ItemUseManager.Instance.ProcessEquipUse(uuid, bestEquip, 
        //             new List<string>{target.Item2.uuid}, Vector2.negativeInfinity));
        //         //范围武器支持
        //         // battleManager.chessboardManager.EnqueueAction(new ChessboardAction(new EquipUseInfo()));
        //         // yield return battleManager.StartCoroutine(MoveToPosition(uuid, bestPos));
        //     }
        // }
    }
}

public class MagicianAI : EnemyAI
{
    public EnemyAIWeightFactor weightFactor = new EnemyAIWeightFactor(1, 1, 1, 1);
    public override IEnumerator TurnOnceAction(string uuid)
    {
        Debug.Log("MagicianAI TurnOnceAction");
        yield return null;
    }
}

public class PastorAI : EnemyAI
{
    public override IEnumerator TurnOnceAction(string uuid)
    {
        Debug.Log("PastorAI TurnOnceAction");
        yield return null;
    }
}
