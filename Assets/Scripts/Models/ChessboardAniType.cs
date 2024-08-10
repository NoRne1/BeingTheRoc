using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public enum ChessboardActionType
{
    Move,
    NormalItemUse,
    EquipUse,
    EndRound,
}

public class EndRoundInfo {
    public string uuid;
    public EndRoundInfo(string uuid) {
        this.uuid = uuid;
    }
}

public class MoveInfo {
    public List<Vector2> movePath;
    public UIBattleItem uiBattleItem;
    public MoveInfo(UIBattleItem uiBattleItem, List<Vector2> movePath) {
        this.movePath = movePath;
        this.uiBattleItem = uiBattleItem;
    }
}

public class NormalItemUseInfo {
    public StoreItemModel item;
    public UIBattleItem uiBattleItem;
    public NormalItemUseInfo(UIBattleItem uiBattleItem, StoreItemModel item) {
        this.item = item;
        this.uiBattleItem = uiBattleItem;
    }
}

public class EquipUseInfo {
    public StoreItemModel item;
    public UIBattleItem uiBattleItem;
    public List<string> targetIDs;
    public Vector2 targetPos;
    public EquipUseInfo(UIBattleItem uiBattleItem, StoreItemModel item, List<string> targetIDs, Vector2 targetPos) {
        this.item = item;
        this.uiBattleItem = uiBattleItem;
        this.targetIDs = targetIDs;
        this.targetPos = targetPos;
    }
}

public class ChessboardAction
{
    public ChessboardActionType type { get; private set; }
    public MoveInfo moveInfo { get; private set; }
    public NormalItemUseInfo normalItemUseInfo { get; private set; }
    public EquipUseInfo equipUseInfo { get; private set; }
    public EndRoundInfo endRoundInfo { get; private set; }

    // public ChessboardAction(ChessboardActionType type)
    // {
    //     switch(type)
    //     {
    //         case ChessboardActionType.Move:
    //             Debug.LogError("MoveInfo must be provided when creating ChessboardAction with type Move");
    //             break;
    //         case ChessboardActionType.NormalItemUse:
    //             Debug.LogError("NormalItemUseInfo must be provided when creating ChessboardAction with type NormalItemUse");
    //             break;
    //         case ChessboardActionType.EquipUse:
    //             Debug.LogError("EquipUseInfo must be provided when creating ChessboardAction with type EquipUse");
    //             break;
    //         case ChessboardActionType.EndRound:
    //             this.type = type;
    //             break;
    //     }
    // }

    public ChessboardAction(MoveInfo moveInfo)
    {
        this.type = ChessboardActionType.Move;
        this.moveInfo = moveInfo;
    }

    public ChessboardAction(NormalItemUseInfo normalItemUseInfo)
    {
        this.type = ChessboardActionType.NormalItemUse;
        this.normalItemUseInfo = normalItemUseInfo;
    }

    public ChessboardAction(EquipUseInfo equipUseInfo)
    {
        this.type = ChessboardActionType.EquipUse;
        this.equipUseInfo = equipUseInfo;
    }

    public ChessboardAction(EndRoundInfo endRoundInfo)
    {
        this.type = ChessboardActionType.EndRound;
        this.endRoundInfo = endRoundInfo;
    }
}

public static class ChessboardActionExecutor
{
    public static IEnumerator ExecuteAction(ChessboardAction action)
    {
        switch (action.type)
        {
            case ChessboardActionType.Move:
                yield return BattleManager.Instance.StartCoroutine(MoveToPosition(action.moveInfo));
                break;
            case ChessboardActionType.NormalItemUse:
                yield return BattleManager.Instance.StartCoroutine(UseNormalItem(action.normalItemUseInfo));
                break;
            case ChessboardActionType.EquipUse:
                yield return BattleManager.Instance.StartCoroutine(UseEquip(action.equipUseInfo));
                break;
            case ChessboardActionType.EndRound:
                EndRound(action.endRoundInfo.uuid);
                yield return null;
                break;
            default:
                Debug.LogError("Unknown ChessboardAniType");
                break;
        }
    }
    public static void EndRound(string uuid)
    {
        BattleManager.Instance.roundManager.roundTime.OnNext((uuid, RoundTime.end));
    }
    public static IEnumerator MoveToPosition(MoveInfo moveInfo)
    {
        // 移动逻辑
        Debug.Log("Moving to position: " + moveInfo.movePath.Last());
        BattleManager.Instance.chessboardManager.chessBoard.ResetColors();
        var path = moveInfo.movePath.Select(pos => BattleManager.Instance.chessboardManager.chessBoard.slots[pos].transform.position).ToList();
        Tween moveTween = BattleCommonMethods.MoveAlongPath(path, moveInfo.uiBattleItem.transform);
        if (moveTween == null)
        {
            Debug.LogError("MoveTween is null");
            yield return null;
        } else 
        {
            yield return moveTween.WaitForCompletion();
            BattleManager.Instance.chessboardManager.ShowMovePath(moveInfo.uiBattleItem.itemID);
            yield return null;
        }
    }

    public static IEnumerator UseNormalItem(NormalItemUseInfo normalItemUseInfo)
    {
        // 使用普通物品逻辑
        Debug.Log("Using normal item: " + normalItemUseInfo.item.title);
        // 假设使用需要1秒
        yield return new WaitForSeconds(1.0f);
    }

    public static IEnumerator UseEquip(EquipUseInfo equipUseInfo)
    {
        // 使用装备逻辑
        Debug.Log("Using equip: " + equipUseInfo.item.title);
        // 假设使用需要1秒
        yield return new WaitForSeconds(1.0f);
    }
}