using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public static class BattleCommonMethods
{
    public static void MoveChangeTarget(string targetID, int value)
    {
        var battleItem = GlobalAccess.GetBattleItem(targetID);
        battleItem.remainActingDistance = (int)(battleItem.remainActingDistance * (1 - (value / 100.0f)));
        GlobalAccess.SaveBattleItem(battleItem);
        BattleManager.Instance.CalcBattleItemAndShow(0);
    }

    public static Vector2 GetKnockbackCoordinates(Vector2 casterPos, Vector2 targetPos, int knockbackDistance)
    {
        int dx = (int)(targetPos.x - casterPos.x);
        int dy = (int)(targetPos.y - casterPos.y);

        // 判断击退方向是否为横向、纵向或45度斜向
        if (dx == 0 || dy == 0 || Math.Abs(dx) == Math.Abs(dy))
        {
            int newX = (int)targetPos.x;
            int newY = (int)targetPos.y;

            for (int i = 0; i < knockbackDistance; i++)
            {
                int nextX = newX + (dx != 0 ? Math.Sign(dx) : 0);
                int nextY = newY + (dy != 0 ? Math.Sign(dy) : 0);

                if (nextX < 0 || nextX >= 8 || nextY < 0 || nextY >= 8)
                {
                    // 如果下一步超出棋盘范围，返回当前坐标
                    return new Vector2(newX, newY);
                }

                newX = nextX;
                newY = nextY;
            }

            return new Vector2(newX, newY);
        }
        else
        {
            throw new ArgumentException("Invalid knockback direction. Only horizontal, vertical, or 45-degree diagonal directions are allowed.");
        }
    }

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

    public static void MoveAlongPath(List<Vector3> path, Transform targetTransform)
    {
        // 检查路径是否为空或只有一个点
        if (path == null || path.Count < 2)
        {
            Debug.LogError("Path must contain at least 2 points.");
            return;
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
        targetTransform.DOPath(waypoints, duration, PathType.Linear, PathMode.Full3D)
            .SetEase(Ease.Linear)
            .OnComplete(() => Debug.Log("Movement along path completed."));
    }
}
