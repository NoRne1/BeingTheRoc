using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class UIShopPage : MonoBehaviour
{
    public List<UIShopItem> shopItems;
    private int timeLeft;
    // Start is called before the first frame update
    void Start()
    {
        timeLeft = GameManager.Instance.timeLeft.Value;
        GenerateShopItems();

        GameManager.Instance.timeLeft.AsObservable().TakeUntilDestroy(this).Subscribe(time =>
        {
            if (timeLeft != time)
            {
                timeLeft = time;
                GenerateShopItems();
            }
        });
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        if(timeLeft != GameManager.Instance.timeLeft.Value)
        {
            timeLeft = GameManager.Instance.timeLeft.Value;
            GenerateShopItems();
        }
    }

    public void GenerateShopItems()
    {
        int refreshCount = Random.Range(3, shopItems.Count);
        for (int i = 0; i < shopItems.Count; i++)
        {
            if(i < refreshCount)
            {
                shopItems[i].SetStoreItemInfo(DataManager.Instance.StoreItems
                    [Random.Range(0, DataManager.Instance.StoreItems.Count)]);
            } else
            {
                shopItems[i].SetStoreItemInfo(null);
            }
            
        }
    }

    public void BuyItem(int index)
    {
        if (shopItems[index].info == null)
        {
            //错误请求
            UITip tip = UIManager.Instance.Show<UITip>();
            tip.UpdateTip(DataManager.Instance.Language["general_error_tip"] + "0002");
        } else if (shopItems[index].info.price > GameManager.Instance.featherCoin.Value)
        {
            //钱不够买
            UITip tip = UIManager.Instance.Show<UITip>();
            //todo
            tip.UpdateTip(DataManager.Instance.Language["go_next_town_tip"]);
        } else if (shopItems[index].info.type == ItemType.treasure) {
            //treasure不占仓库，需特殊处理
            GameManager.Instance.treasureManager.AddTreasure(new StoreItemModel(shopItems[index].info));
        } else if (!GameManager.Instance.repository.remainOpacity)
        {
            //仓库空间不够了
            UITip tip = UIManager.Instance.Show<UITip>();
            //todo
            tip.UpdateTip(DataManager.Instance.Language["general_error_tip"]);
        } else
        {
            shopItems[index].ItemSold(true);
            GameManager.Instance.CoinChanged(-shopItems[index].info.price);
            GameManager.Instance.repository.AddItem(new StoreItemModel(shopItems[index].info));
        }
    }
}
