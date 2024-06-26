using System.Collections;
using System.Collections.Generic;
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
}
