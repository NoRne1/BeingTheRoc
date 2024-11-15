using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Linq;

public class UIShopPage : MonoBehaviour
{
    public List<StoreItemDefine> sellableItems;
    public List<UIShopItem> shopItems;
    private List<int> lastSelectedItemIDs = new List<int>();
    private int timeLeft = -1;
    // Start is called before the first frame update
    void Start()
    {
        this.sellableItems= DataManager.Instance.StoreItems.Values.Where(item => item.sellType == SellType.shop).ToList();
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
        if(timeLeft != -1 && timeLeft != GameManager.Instance.timeLeft.Value)
        {
            timeLeft = GameManager.Instance.timeLeft.Value;
            GenerateShopItems();
        }
    }

    public void GenerateShopItems()
    {
        var itemList = RefreshItems();
        for (int i = 0; i < shopItems.Count; i++)
        {
            if(i < itemList.Count)
            {
                shopItems[i].SetStoreItemInfo(itemList[i]);
            } else
            {
                shopItems[i].SetStoreItemInfo(null);
            }
        }
    }

    public List<StoreItemDefine> RefreshItems()
    {
        List<StoreItemDefine> sellableItemsCopy = new List<StoreItemDefine>(sellableItems);
        // 随机选取 refreshCount 个物品，确保与上一次选取的物品不同
        int refreshCount = Random.Range(3, shopItems.Count);
        System.Random random = new System.Random();
        List<StoreItemDefine> newSelectedItems = new List<StoreItemDefine>();

        while (newSelectedItems.Count < refreshCount)
        {
            int index = random.Next(sellableItemsCopy.Count);
            StoreItemDefine selectedItemDefine = sellableItemsCopy[index];

            if (!lastSelectedItemIDs.Contains(selectedItemDefine.ID))
            {
                newSelectedItems.Add(selectedItemDefine);
                sellableItemsCopy.RemoveAt(index);
            }
        }

        // 更新上一次选取的物品列表
        lastSelectedItemIDs = newSelectedItems.Select(item => item.ID).ToList();

        return newSelectedItems;
    }

    public void BuyItem(int index)
    {
        if (shopItems[index].info == null)
        {
            //错误请求
            UITip tip = UIManager.Instance.Show<UITip>();
            tip.UpdateGeneralTip("0002");
        } else if (shopItems[index].info.price > GameManager.Instance.featherCoin.Value)
        {
            //钱不够买
            UITip tip = UIManager.Instance.Show<UITip>();
            //todo
            tip.UpdateGeneralTip("钱不够买，todo！");
        } else if (shopItems[index].info.type == ItemType.treasure) {
            //treasure不占仓库，需特殊处理
            shopItems[index].ItemSold(true);
            GameManager.Instance.FeatherCoinChanged(-shopItems[index].info.price);
            GameManager.Instance.treasureManager.AddTreasure(new StoreItemModel(shopItems[index].info));
        } else if (!GameManager.Instance.repository.remainOpacity)
        {
            //仓库空间不够了
            UITip tip = UIManager.Instance.Show<UITip>();
            //todo
            tip.UpdateGeneralTip("仓库空间不够，逻辑未处理，todo！");
        } else
        {
            shopItems[index].ItemSold(true);
            GameManager.Instance.FeatherCoinChanged(-shopItems[index].info.price);
            GameManager.Instance.repository.AddItem(new StoreItemModel(shopItems[index].info));
        }
    }
}
