using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UniRx;

public class RepositoryModel
{
	public int opacity = 12;
	public BehaviorSubject<List<StoreItemModel>> itemsRelay;
	public bool remainOpacity { get { return itemsRelay.Value.Count < 12; } }
    // Use this for initialization
    public RepositoryModel()
	{
        itemsRelay = new BehaviorSubject<List<StoreItemModel>>(new List<StoreItemModel>(opacity));
    }

	public void AddItem(StoreItemModel item)
	{
		if(!remainOpacity)
		{
            //错误请求
            UITip tip = UIManager.Instance.Show<UITip>();
            tip.UpdateTip(DataManager.Instance.Language["general_error_tip"] + "0004");
			return;
        }
        itemsRelay.Value.Add(item);
        itemsRelay.OnNext(itemsRelay.Value);
    }

	public void RemoveItem(string uuid)
	{
        itemsRelay.Value.RemoveAll(value => uuid == value.uuid);
        itemsRelay.OnNext(itemsRelay.Value);
    }
}

