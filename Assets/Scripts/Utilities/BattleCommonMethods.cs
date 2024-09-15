using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using System.Linq;

public enum KnockbackStatus
{
    toBorder = 0,
    crash = 1,
    success = 2,
    failure = 3,
}

public static class BattleCommonMethods
{
    public static void MoveChangeTarget(string targetID, int value)
    {
        var battleItem = GlobalAccess.GetBattleItem(targetID);
        battleItem.remainActingDistance = (int)(battleItem.remainActingDistance * (1 - (value / 100.0f)));
        GlobalAccess.SaveBattleItem(battleItem);
        BattleManager.Instance.moveBarManager.CalcBattleItemAndShow(0);
    }

    public static KnockbackStatus KnockbackCaster(string casterID, string targetID, int casterKnockbackDistance)
    {
        if (GlobalAccess.GetBattleItem(casterID).isConfine) { return KnockbackStatus.failure; }
        KnockbackStatus status = KnockbackStatus.success;
        Vector2 casterPos = BattleManager.Instance.battleItemManager.pos_uibattleItemDic.ToList()
            .Where(pair => pair.Value.itemID == casterID)
            .Select(pair => pair.Key).FirstOrDefault();
        Vector2 targetPos = BattleManager.Instance.battleItemManager.pos_uibattleItemDic.ToList()
            .Where(pair => pair.Value.itemID == targetID)
            .Select(pair => pair.Key).FirstOrDefault();
        int dx = (int)(casterPos.x - targetPos.x);
        int dy = (int)(casterPos.y - targetPos.y);

        // 判断击退方向是否为横向、纵向或45度斜向
        if (dx == 0 || dy == 0 || Math.Abs(dx) == Math.Abs(dy))
        {
            int newX = (int)casterPos.x;
            int newY = (int)casterPos.y;

            var uiBattleItem = BattleManager.Instance.battleItemManager.pos_uibattleItemDic[casterPos];

            for (int i = 0; i < casterKnockbackDistance; i++)
            {
                int nextX = newX + (dx != 0 ? Math.Sign(dx) : 0);
                int nextY = newY + (dy != 0 ? Math.Sign(dy) : 0);
                if (nextX < 0 || nextX >= 8 || nextY < 0 || nextY >= 8)
                {
                    // 如果下一步超出棋盘范围，截止击退
                    status = KnockbackStatus.toBorder;
                    break;
                }

                if (BattleManager.Instance.battleItemManager.pos_uibattleItemDic.Keys.Contains(new Vector2(nextX, nextY)))
                {
                    // 如果下一步与其他battleItem相撞，截止击退，并造成碰撞伤害
                    var uiBattleItem2 = BattleManager.Instance.battleItemManager.pos_uibattleItemDic[new Vector2(nextX, nextY)];
                    ProcessDirectAttack(uiBattleItem.itemID, uiBattleItem2.itemID, GlobalAccess.knockbackDirectDamage);
                    ProcessDirectAttack(uiBattleItem2.itemID, uiBattleItem.itemID, GlobalAccess.knockbackDirectDamage);
                    status = KnockbackStatus.toBorder;
                    break;
                }
                newX = nextX;
                newY = nextY;
            }
            switch (status)
            {
                case KnockbackStatus.success:
                case KnockbackStatus.toBorder:
                    uiBattleItem.transform.DOMove(new Vector2(newX, newY), 1.5f);
                    break;
                case KnockbackStatus.crash:
                    int nextX = newX + (dx != 0 ? Math.Sign(dx) : 0);
                    int nextY = newY + (dy != 0 ? Math.Sign(dy) : 0);
                    if (BattleManager.Instance.battleItemManager.pos_uibattleItemDic.Keys.Contains(new Vector2(nextX, nextY)))
                    {
                        // 创建一个序列
                        Sequence mySequence = DOTween.Sequence();
                        // 第一次移动到(nextX, nextY)
                        mySequence.Append(uiBattleItem.transform.DOMove(new Vector2(nextX, nextY), 1.5f));
                        // 添加一个缓动效果，可以使用 Ease 来控制缓动效果
                        mySequence.Append(uiBattleItem.transform.DOShakePosition(0.5f, strength: new Vector3(1, 1, 0), vibrato: 10, randomness: 90, snapping: false, fadeOut: true));
                        // 回弹到(newX, newY)
                        mySequence.Append(uiBattleItem.transform.DOMove(new Vector2(newX, newY), 0.5f).SetEase(Ease.InOutBounce));
                    }
                    else
                    {
                        Debug.Log("Invalid knockback crash, and this should not appeal!");
                    }
                    break;
                case KnockbackStatus.failure:
                    Debug.Log("Invalid knockback, and this should not appeal!");
                    break;
            }
        }
        else
        {
            Debug.Log("Invalid knockback direction. Only horizontal, vertical, or 45-degree diagonal directions are allowed.");
            status = KnockbackStatus.failure;
        }
        return status;
    }

    public static KnockbackStatus KnockbackTarget(string casterID, string targetID, int targetKnockbackDistance)
    {
        if (GlobalAccess.GetBattleItem(targetID).isConfine) { return KnockbackStatus.failure; }
        KnockbackStatus status = KnockbackStatus.success;
        Vector2 casterPos = BattleManager.Instance.battleItemManager.pos_uibattleItemDic.ToList()
            .Where(pair => pair.Value.itemID == casterID)
            .Select(pair => pair.Key).FirstOrDefault();
        Vector2 targetPos = BattleManager.Instance.battleItemManager.pos_uibattleItemDic.ToList()
            .Where(pair => pair.Value.itemID == targetID)
            .Select(pair => pair.Key).FirstOrDefault();
        int dx = (int)(targetPos.x - casterPos.x);
        int dy = (int)(targetPos.y - casterPos.y);

        // 判断击退方向是否为横向、纵向或45度斜向
        if (dx == 0 || dy == 0 || Math.Abs(dx) == Math.Abs(dy))
        {
            int newX = (int)targetPos.x;
            int newY = (int)targetPos.y;

            var uiBattleItem = BattleManager.Instance.battleItemManager.pos_uibattleItemDic[targetPos];

            for (int i = 0; i < targetKnockbackDistance; i++)
            {
                int nextX = newX + (dx != 0 ? Math.Sign(dx) : 0);
                int nextY = newY + (dy != 0 ? Math.Sign(dy) : 0);
                if (nextX < 0 || nextX >= 8 || nextY < 0 || nextY >= 8)
                {
                    // 如果下一步超出棋盘范围，截止击退
                    status = KnockbackStatus.toBorder;
                    break;
                }

                if (BattleManager.Instance.battleItemManager.pos_uibattleItemDic.Keys.Contains(new Vector2(nextX, nextY)))
                {
                    // 如果下一步与其他battleItem相撞，截止击退，并造成碰撞伤害
                    var uiBattleItem2 = BattleManager.Instance.battleItemManager.pos_uibattleItemDic[new Vector2(nextX, nextY)];
                    ProcessDirectAttack(uiBattleItem.itemID, uiBattleItem2.itemID, GlobalAccess.knockbackDirectDamage);
                    ProcessDirectAttack(uiBattleItem2.itemID, uiBattleItem.itemID, GlobalAccess.knockbackDirectDamage);
                    status = KnockbackStatus.toBorder;
                    break;
                }
                newX = nextX;
                newY = nextY;
            }
            switch (status)
            {
                case KnockbackStatus.success:
                case KnockbackStatus.toBorder:
                    uiBattleItem.transform.DOMove(new Vector2(newX, newY), 1.5f);
                    break;
                case KnockbackStatus.crash:
                    int nextX = newX + (dx != 0 ? Math.Sign(dx) : 0);
                    int nextY = newY + (dy != 0 ? Math.Sign(dy) : 0);
                    if (BattleManager.Instance.battleItemManager.pos_uibattleItemDic.Keys.Contains(new Vector2(nextX, nextY)))
                    {
                        // 创建一个序列
                        Sequence mySequence = DOTween.Sequence();
                        // 第一次移动到(nextX, nextY)
                        mySequence.Append(uiBattleItem.transform.DOMove(new Vector2(nextX, nextY), 1.5f));
                        // 添加一个缓动效果，可以使用 Ease 来控制缓动效果
                        mySequence.Append(uiBattleItem.transform.DOShakePosition(0.5f, strength: new Vector3(1, 1, 0), vibrato: 10, randomness: 90, snapping: false, fadeOut: true));
                        // 回弹到(newX, newY)
                        mySequence.Append(uiBattleItem.transform.DOMove(new Vector2(newX, newY), 0.5f).SetEase(Ease.InOutBounce));
                    } else
                    {
                        Debug.Log("Invalid knockback crash, and this should not appeal!");
                    }
                    break;
                case KnockbackStatus.failure:
                    Debug.Log("Invalid knockback, and this should not appeal!");
                    break;
            }
        }
        else
        {
            Debug.Log("Invalid knockback direction. Only horizontal, vertical, or 45-degree diagonal directions are allowed.");
            status = KnockbackStatus.failure;
        }
        return status;
    }

    //找最短路径
    public static (bool, List<Vector2>) CanMoveTo(Vector2 source, Vector2 dest, int mobility, List<Vector2> battleItemsPosList)
    {
        if (source == dest)
        {
            return (true, new List<Vector2>());
        }

        Queue<(Vector2, List<Vector2>)> queue = new Queue<(Vector2, List<Vector2>)>();
        HashSet<Vector2> visited = new HashSet<Vector2>();

        queue.Enqueue((source, new List<Vector2> { source }));
        visited.Add(source);

        Vector2[] directions = new Vector2[]
        {
            new Vector2(1, 0),
            new Vector2(-1, 0),
            new Vector2(0, 1),
            new Vector2(0, -1),
        };

        while (queue.Count > 0)
        {
            var (current, currentPath) = queue.Dequeue();

            foreach (var direction in directions)
            {
                Vector2 next = current + direction;
                if (next.x >= 0 && next.x < 8 && next.y >= 0 && next.y < 8 && !visited.Contains(next) && !battleItemsPosList.Contains(next))
                {
                    List<Vector2> newPath = new List<Vector2>(currentPath) { next };
                    if (next == dest)
                    {
                        if (newPath.Count - 1 <= mobility)
                        {
                            return (true, newPath);
                        }
                        else
                        {
                            return (false, newPath);
                        }
                    }
                    queue.Enqueue((next, newPath));
                    visited.Add(next);
                }
            }
        }

        return (false, new List<Vector2>()); // If no path is found
    }

    public static IEnumerator MoveToPosition(UIBattleItem uiBattleItem, List<Vector2> movePath)
    {
        // 移动逻辑
        Debug.Log("Moving to position: " + movePath.Last());
        BattleManager.Instance.chessboardManager.chessBoard.ResetColors();
        var path = movePath.Select(pos => BattleManager.Instance.chessboardManager.chessBoard.slots[pos].transform.position).ToList();
        Tween moveTween = MoveAlongPath(path, uiBattleItem.transform);
        if (moveTween == null)
        {
            Debug.LogError("MoveTween is null");
            yield return null;
        } else 
        {
            yield return moveTween.WaitForCompletion();
            BattleManager.Instance.chessboardManager.ShowMovePath(uiBattleItem.itemID);
            yield return null;
        }
    }

    private static Tween MoveAlongPath(List<Vector3> path, Transform targetTransform)
    {
        // 检查路径是否为空或只有一个点
        if (path == null || path.Count < 2)
        {
            Debug.LogError("Path must contain at least 2 points.");
            return null;
        }

        // 设置 DOTween 的路径
        Vector3[] waypoints = path.ToArray();

        // 计算总路径长度
        float totalDistance = 0f;
        for (int i = 1; i < waypoints.Length; i++)
        {
            totalDistance += Vector3.Distance(waypoints[i - 1], waypoints[i]);
        }

        // 设定匀速移动的时间（可以根据需求调整时间）
        float duration = totalDistance / 5f; // 这里假设速度为5单位/秒

        // 使用 DOTween 设置路径动画
        return targetTransform.DOPath(waypoints, duration, PathType.Linear, PathMode.Full3D)
            .SetEase(Ease.Linear)
            .OnComplete(() => Debug.Log("Movement along path completed."));
    }

    // 常规攻击（受属性影响，会暴击，触发特效)
    public static NormalAttackResult CalcNormalAttack(string casterID, List<string> targetIDs, StoreItemModel itemModel, int value, int attackTime = 1)
    {
        int baseAccuracy = itemModel == null ? 100 : itemModel.equipDefine.baseAccuracy;
        EquipClass equipClass = itemModel == null ?  EquipClass.none : itemModel.equipDefine.equipClass;
        var result = new NormalAttackResult(attackTime);
        var battleItemManager = BattleManager.Instance.battleItemManager;
        var self = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(casterID)).Value;
        foreach (var targetID in targetIDs)
        {
            var target = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(targetID)).Value;
            int[] Data = new int[(int)EquipClass.MAX];
            Data[(int)equipClass] = GameManager.Instance.treasureManager.equipClassEffect.ContainsKey(equipClass) ?
                GameManager.Instance.treasureManager.equipClassEffect[equipClass] : 0;
            for (int index = 0; index < attackTime; index++)
            {
                bool hitFlag = GameUtil.Instance.GetRandomRate(baseAccuracy *
                    (1 - target.attributes.Dodge / 100.0f) *
                    (1 + (self.attributes.Accuracy + Data[(int)EquipClass.arch]) / 100.0f)
                );
                bool criticalFlag = GameUtil.Instance.GetRandomRate(self.attributes.Lucky);
                int damage = (int)((value + Data[(int)EquipClass.sword])
                    * (1 + self.attributes.Strength / 100.0f) //力量乘区
                    * (1 - target.attributes.Defense / (target.attributes.Defense + 100.0f)) //防御乘区
                    * (hitFlag ? 1 : 0) //命中乘区
                    * (criticalFlag ? 2 : 1) //暴击乘区
                    * (1 - (self.attributes.Protection / 100.0f)) //减伤乘区
                    * (1 + (self.attributes.EnchanceDamage / 100.0f))); //增伤乘区

                var targetItem = battleItemManager.pos_uibattleItemDic.Values.Where((item) => item.itemID == targetID).ToList().FirstOrDefault();

                int hemato = (int)(damage * (self.attributes.Hematophagia / 100.0f));
                AttackStatus status;
                if (!battleItemManager.HasBattleItem(targetID))
                {
                    status = AttackStatus.errorTarget;
                } else if (!hitFlag)
                {
                    status = AttackStatus.miss;  
                } else if (criticalFlag)
                {
                    status = AttackStatus.critical;  
                } else
                {
                    status = AttackStatus.normal;  
                } 
                var tempResult = new AttackDisplayResult(result.attackIdentifier, index, casterID, targetID, status, damage, hemato, true, itemModel);
                result.displayResults.Add(tempResult);
            }
        }
        return result;
    }

    public static void ProcessNormalAttack(AttackDisplayResult displayResult)
    {
        var battleItemManager = BattleManager.Instance.battleItemManager;
        if (displayResult.attackStatus != AttackStatus.errorTarget)
        {
            battleItemManager.pos_uibattleItemDic[battleItemManager.id_posDic[displayResult.targetID]].Damage(displayResult);
        } 
        BattleManager.Instance.battleItemDamageSubject.OnNext(displayResult);
    }

    // 直接伤害
    public static void ProcessDirectAttack(string casterID, string targetID, int value)
    {
        var battleItemManager = BattleManager.Instance.battleItemManager;
        var target = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(targetID)).Value;
        var targetItem = battleItemManager.pos_uibattleItemDic.Values.Where((item) => item.itemID == targetID).ToList().FirstOrDefault();

        if (battleItemManager.HasBattleItem(targetID))
        {
            var tempResult = new AttackDisplayResult(GameUtil.Instance.GenerateUniqueId(), 0, casterID, targetID, AttackStatus.normal, value, 0, false, null);
            battleItemManager.pos_uibattleItemDic[battleItemManager.id_posDic[targetID]].Damage(tempResult);
            BattleManager.Instance.battleItemDamageSubject.OnNext(tempResult);
        }
    }

    // 常规回血（受属性影响，会暴击，触发特效）
    public static void ProcessNormalHealth(string selfID, List<string> targetIDs, int value)
    {
        var battleItemManager = BattleManager.Instance.battleItemManager;
        var self = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(selfID)).Value;
        List<AttackStatus> statuses = new List<AttackStatus>();
        foreach (var id in targetIDs)
        {
            var target = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(id)).Value;
            bool criticalFlag = UnityEngine.Random.Range(0, 100) < (self.attributes != null ? self.attributes.Lucky : 0);
            int healthHp = (int)(value
                * (1 + (self.attributes != null ? self.attributes.Strength : 0) / 100.0f)
                * (criticalFlag ? 1 : 2));
            if (battleItemManager.HasBattleItem(id))
            {
                battleItemManager.pos_uibattleItemDic[battleItemManager.id_posDic[id]].HPChange(healthHp, criticalFlag);
            }
        }
    }

    // 直接回血
    public static void ProcessDirectHealth(string selfID, string targetID, int value)
    {
        var battleItemManager = BattleManager.Instance.battleItemManager;
        var self = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(selfID)).Value;
        List<AttackStatus> statuses = new List<AttackStatus>();
        var target = NorneStore.Instance.ObservableObject<BattleItem>(new BattleItem(targetID)).Value;
        if (battleItemManager.HasBattleItem(targetID))
        {
            battleItemManager.pos_uibattleItemDic[battleItemManager.id_posDic[targetID]].HPChange(value, false);
        }
    }
}
