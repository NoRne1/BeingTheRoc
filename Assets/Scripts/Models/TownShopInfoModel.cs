using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class TownShopInfoModel
{
    public int timeLeft;
    public BehaviorSubject<List<StoreItemModel>> sellingItems;

    public TownShopInfoModel()
    {
        timeLeft = 999;
        sellingItems = new BehaviorSubject<List<StoreItemModel>>(new List<StoreItemModel>());
    }

}
