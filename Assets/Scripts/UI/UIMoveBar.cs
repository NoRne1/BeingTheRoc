using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMoveBar : MonoBehaviour
{
    public GameObject firstItemPrefab;
    public GameObject otherItemPrefab;
    public GameObject dayItemPrefab;
    public ButtonObjectPool firstItemPool;
    public ButtonObjectPool otherItemPool;
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
        firstItemPool = new ButtonObjectPool(firstItemPrefab, 1, itemFather, (button) => { BattleManager.Instance.MoveBarItemClicked(button); });
        otherItemPool = new ButtonObjectPool(otherItemPrefab, 6, itemFather, (button) => { BattleManager.Instance.MoveBarItemClicked(button); });
        dayItemPool = new ObjectPool(dayItemPrefab, 1, itemFather);
    }

    public void Show(List<string> battleItems)
    {
        firstItemPool.ReturnAllObject();
        otherItemPool.ReturnAllObject();
        dayItemPool.ReturnAllObject();
        for (int i = 0; i < Mathf.Min(battleItems.Count, GlobalAccess.moveBarMaxShowNum); i++)
        {
            var battleItem = GlobalAccess.GetBattleItem(battleItems[i]);
            if (battleItem.attributes.Speed == 0) { continue; }
            if (i == 0 && battleItem.type != BattleItemType.time && battleItem.type != BattleItemType.quitTime &&
                battleItem.remainActingDistance == 0)
            {
                //有firstItem
                GameObject firstItem = firstItemPool.GetObjectFromPool();
                firstItem.GetComponent<UIMoveBarFirstItem>().Setup(battleItem);
                firstItem.transform.SetSiblingIndex(i);
            } else
            {
                switch (battleItem.type)
                {
                    case BattleItemType.time:
                        GameObject dayItem = dayItemPool.GetObjectFromPool();
                        dayItem.GetComponent<UIMoveBarDayItem>().Setup(battleItem);
                        dayItem.transform.SetSiblingIndex(i);
                        break;
                    case BattleItemType.quitTime:
                        GameObject quitTimeItem = dayItemPool.GetObjectFromPool();
                        quitTimeItem.GetComponent<UIMoveBarDayItem>().Setup(battleItem);
                        quitTimeItem.transform.SetSiblingIndex(i);
                        break;
                    case BattleItemType.character:
                    case BattleItemType.sceneItem:
                        GameObject otherItem = otherItemPool.GetObjectFromPool();
                        otherItem.GetComponent<UIMoveBarOtherItem>().Setup(battleItem);
                        otherItem.transform.SetSiblingIndex(i);
                        break;
                }
            }
        }
    }
}
