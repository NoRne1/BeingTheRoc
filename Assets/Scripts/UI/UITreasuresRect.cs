using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITreasuresRect : MonoBehaviour
{
    public List<UITreasureItem> treasureItems = new List<UITreasureItem>();
    public Button moreButton;

    public void Setup(List<(StoreItemModel, int)> treasures)
    {
        Reset();
        treasures.Sort((x, y) => x.Item1.treasureDefine.invokeType == TreasureInvokeType.battleUse ? -1 : 1);
        for(int i = 0; i < Mathf.Min(treasureItems.Count, treasures.Count); i++)
        {
            treasureItems[i].Setup(treasures[i].Item1, treasures[i].Item2);
            treasureItems[i].gameObject.SetActive(true);
        }
        moreButton.gameObject.SetActive(treasures.Count > treasureItems.Count);
    }

    public void Reset()
    {
        foreach(var item in treasureItems)
        {
            item.Reset();
            item.gameObject.SetActive(false);
        }
    }
}
