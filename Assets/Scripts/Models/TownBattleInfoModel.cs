using UnityEngine;
using System.Collections;
using UniRx;
using System.Linq;
using System.Collections.Generic;

public class TownBattleInfoModel
{
    public List<Vector2> initPlaceSlots;

    public TownBattleInfoModel(List<Vector2> initPlaceSlots)
    {
        this.initPlaceSlots = initPlaceSlots;
    }
}

