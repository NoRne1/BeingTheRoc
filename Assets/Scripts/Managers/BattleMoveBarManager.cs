using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleMoveBarManager
{
    private BattleManager battleManager = BattleManager.Instance;
    public UIMoveBar moveBar;

    public BattleMoveBarManager(UIMoveBar moveBar)
    {
        this.moveBar = moveBar;
        moveBar.Init();
    }

    public void Init() {
        
    }

    public void CalcBattleItemAndShow(float time)
    {
        if (time >= 0)
        {
            foreach (string uuid in battleManager.battleItemManager.roundBattleItemIDs)
            {
                var item = GlobalAccess.GetBattleItem(uuid);
                item.remainActingDistance = Mathf.Max(0, item.remainActingDistance - time * item.attributes.Speed);
                GlobalAccess.SaveBattleItem(item);
            }
            RefreshMoveBar();
        }
        else if (time == -999)
        {
            foreach (string uuid in battleManager.battleItemManager.roundBattleItemIDs)
            {
                var item = GlobalAccess.GetBattleItem(uuid);
                item.remainActingDistance = GlobalAccess.roundDistance;
                GlobalAccess.SaveBattleItem(item);
            }
            RefreshMoveBar();
        }
    }

    public void RefreshMoveBar() 
    {
        battleManager.battleItemManager.ResortBattleItems();
        moveBar.Show(battleManager.battleItemManager.roundBattleItemIDs);
    }
}
