using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMoveBar : MonoBehaviour
{
    public GameObject firstItemPerfab;
    public GameObject otherItemPerfab;
    public GameObject dayItemPerfab;
    public ObjectPool firstItemPool;
    public ObjectPool otherItemPool;
    public ObjectPool dayItemPool;
    public Transform itemFather;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init()
    {
        firstItemPool = new ObjectPool(firstItemPerfab, 1, itemFather);
        otherItemPool = new ObjectPool(otherItemPerfab, 6, itemFather);
        dayItemPool = new ObjectPool(dayItemPerfab, 1, itemFather);
    }

    public void Show(List<BattleItem> battleItems)
    {
        firstItemPool.ReturnAllObject();
        otherItemPool.ReturnAllObject();
        dayItemPool.ReturnAllObject();
        for (int i = 0; i < Mathf.Min(battleItems.Count, GlobalAccess.moveBarMaxShowNum); i++)
        {
            if (i == 0 && battleItems[i].battleItemType != BattleItemType.time)
            {
                //有firstItem
                GameObject firstItem = firstItemPool.GetObjectFromPool();
                firstItem.GetComponent<UIMoveBarFirstItem>().Setup(battleItems[i]);
                firstItem.transform.SetSiblingIndex(i);
            } else
            {
                switch (battleItems[i].battleItemType)
                {
                    case BattleItemType.time:
                        GameObject dayItem = dayItemPool.GetObjectFromPool();
                        dayItem.GetComponent<UIMoveBarDayItem>().Setup(battleItems[i]);
                        dayItem.transform.SetSiblingIndex(i);
                        break;
                    case BattleItemType.player:
                    case BattleItemType.enemy:
                    case BattleItemType.sceneItem:
                        GameObject otherItem = otherItemPool.GetObjectFromPool();
                        otherItem.GetComponent<UIMoveBarOtherItem>().Setup(battleItems[i]);
                        otherItem.transform.SetSiblingIndex(i);
                        break;
                }
            }
        }
    }
}
