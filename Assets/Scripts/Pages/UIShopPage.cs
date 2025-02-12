using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Linq;
using UnityEditor.UI;
using TMPro;

public class UIShopPage : MonoBehaviour
{
    public Button shoperButton;
    public GameObject wheatPurchasePanel;
    public Button wheatPurchaseCloseButton;
    public TMP_InputField wheatPurchaseInputField;
    public List<StoreItemDefine> sellableItems;
    public List<UIShopItem> shopItems;
    private TownShopInfoModel shopInfo;
    private System.IDisposable itemsDispose;

    // Start is called before the first frame update
    private void Awake() {
        this.sellableItems= DataManager.Instance.StoreItems.Values.Where(item => item.sellType == SellType.shop).ToList();
    }

    void Start()
    {
        GameManager.Instance.timeLeft.AsObservable().Subscribe(time =>
        {
            if (enabled)
            {
                TimeRefresh();
            }
        }).AddTo(this);

        // 绑定按钮点击事件
        shoperButton.onClick.AddListener(OnShopButtonClicked);
        wheatPurchaseCloseButton.onClick.AddListener(OnPurchaseCloseButtonClicked);

        // 绑定输入框的回车事件
        wheatPurchaseInputField.onEndEdit.AddListener(OnInputFieldEndEdit);
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        itemsDispose.IfNotNull(dispose => { dispose.Dispose(); });
        if (MapManager.Instance.CurrentTownNode.model.shopInfo == null)
        {
            MapManager.Instance.CurrentTownNode.model.shopInfo = new TownShopInfoModel();
            this.shopInfo = MapManager.Instance.CurrentTownNode.model.shopInfo;
            TimeRefresh();
            itemsDispose = shopInfo.sellingItems.AsObservable().Subscribe(items =>
            {
                if (enabled)
                {
                    Setup(items);
                }
            }).AddTo(this);
        } else 
        {
            this.shopInfo = MapManager.Instance.CurrentTownNode.model.shopInfo;
            TimeRefresh();
            itemsDispose = shopInfo.sellingItems.AsObservable().Subscribe(items =>
            {
                if (enabled)
                {
                    Setup(items);
                }
            }).AddTo(this);
        }
    }

    private void Setup(List<StoreItemModel> items)
    {
        for (int i = 0; i < shopItems.Count; i++)
        {
            if(i < items.Count)
            {
                shopItems[i].SetStoreItemInfo(items[i]);
            } else
            {
                shopItems[i].SetStoreItemInfo(null);
            }
        }
    }
    //时间刷新
    public void TimeRefresh()
    {
        if (shopInfo != null)
        {
            if (shopInfo.timeLeft != GameManager.Instance.timeLeft.Value)
            {
                var timeInterval = shopInfo.timeLeft - GameManager.Instance.timeLeft.Value;
                if (timeInterval > 6)
                {
                    //超过两天没更新，直接刷新整个商店
                    shopInfo.sellingItems.Value.Clear();
                    AppendItems(shopInfo.sellingItems.Value);
                } else if (timeInterval > 0)
                {
                    for(var i = timeInterval; i > 0; i--)
                    {
                        SoldItems(shopInfo.sellingItems.Value);
                        DiscountItems(shopInfo.sellingItems.Value);
                    }
                    if (GameManager.Instance.timeLeft.Value / 3 != shopInfo.timeLeft / 3)
                    {
                        //过天补货
                        AppendItems(shopInfo.sellingItems.Value);
                    }
                }
            }
            shopInfo.sellingItems.OnNext(shopInfo.sellingItems.Value);
            shopInfo.timeLeft = GameManager.Instance.timeLeft.Value;
        }
    }

    //补货刷新逻辑
    public void AppendItems(List<StoreItemModel> items)
    {
        if (shopInfo != null)
        {
            List<StoreItemDefine> sellableItemsCopy = new List<StoreItemDefine>(sellableItems);
            var ids = items.Select(item=>item.ID).ToList();
            sellableItemsCopy.RemoveAll(item => ids.Contains(item.ID));
            // 随机选取 refreshCount 个物品，确保与上一次选取的物品不同
            int appendCount = UnityEngine.Random.Range(3, shopItems.Count) - items.Count;
            System.Random random = new System.Random();

            while (items.Count < appendCount)
            {
                int index = random.Next(sellableItemsCopy.Count);
                StoreItemDefine selectedItemDefine = sellableItemsCopy[index];
                items.Add(new StoreItemModel(selectedItemDefine));
                sellableItemsCopy.RemoveAt(index);
            }
        }
    }
    //卖货刷新逻辑
    public void SoldItems(List<StoreItemModel> items)
    {
        if (shopInfo != null)
        {
            items.RemoveAll(item=>GameUtil.Instance.GetRandomRate(30/item.discount));
        }
    }
    //打折刷新逻辑
    public void DiscountItems(List<StoreItemModel> items)
    {
        if (shopInfo != null)
        {
            foreach(var item in items)
            {
                if (item.discount > 0.5f && GameUtil.Instance.GetRandomRate(50))
                {
                    item.discount = Mathf.Max(0.5f, UnityEngine.Random.Range(0.6f, 0.9f) * item.discount);
                }
            }
        }
    }

    public void BuyItem(int index)
    {
        if (shopItems[index].info == null)
        {
            //错误请求
            UITip tip = UIManager.Instance.Show<UITip>();
            tip.UpdateGeneralTip("0002");
        } else if (shopItems[index].info.realPrice > GameManager.Instance.featherCoin.Value)
        {
            //钱不够买
            UITip tip = UIManager.Instance.Show<UITip>();
            //todo
            tip.UpdateTip("buy_item_no_money");
        } else if (shopItems[index].info.type == ItemType.treasure) {
            //treasure不占仓库，需特殊处理
            shopItems[index].ItemSold(true);
            GameManager.Instance.FeatherCoinChanged(-shopItems[index].info.realPrice);
            GameManager.Instance.treasureManager.AddTreasure(shopItems[index].info);
        } else if (!GameManager.Instance.repository.remainOpacity)
        {
            //仓库空间不够了
            UITip tip = UIManager.Instance.Show<UITip>();
            //todo
            tip.UpdateTip("bag_beyond_limit");
        } else
        {
            shopItems[index].ItemSold(true);
            GameManager.Instance.FeatherCoinChanged(-shopItems[index].info.realPrice);
            GameManager.Instance.repository.AddItem(shopItems[index].info);
        }
    }

    void OnShopButtonClicked()
    {
        // 显示输入框
        wheatPurchasePanel.gameObject.SetActive(true);
        wheatPurchaseInputField.Select(); // 让输入框自动获取焦点
    }

    void OnPurchaseCloseButtonClicked()
    {
        wheatPurchasePanel.gameObject.SetActive(false);
    }

    void OnInputFieldEndEdit(string input)
    {
        // 检查是否按下了回车键
        if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
        {
            // 尝试将输入转换为整数
            if (int.TryParse(input, out int number))
            {
                // 调用现有的函数
                ProcessPurchaseWheat(number);
            }
            else
            {
                Debug.Log("请输入有效的数字！");
            }

            // 隐藏输入框
            wheatPurchasePanel.gameObject.SetActive(false);
        }
    }

    void ProcessPurchaseWheat(int amount)
    {
        if (amount > 0)
        {
            //买粮食
            if (amount * GlobalAccess.wheatPrice > GameManager.Instance.featherCoin.Value)
            {
                //钱不够买
                UITip tip = UIManager.Instance.Show<UITip>();
                //todo
                tip.UpdateTip("buy_item_no_money");
            } else
            {
                GameManager.Instance.FeatherCoinChanged(-amount * GlobalAccess.wheatPrice);
                GameManager.Instance.WheatCoinChanged(amount);
            }
        } else if (amount < 0) {
            //卖粮食
            if (-amount > GameManager.Instance.wheatCoin.Value)
            {
                //钱不够买
                UITip tip = UIManager.Instance.Show<UITip>();
                //todo
                tip.UpdateTip("sell_wheat_no_wheat");
            } else
            {
                GameManager.Instance.FeatherCoinChanged(-amount * GlobalAccess.wheatSellPrice);
                GameManager.Instance.WheatCoinChanged(amount);
            }
        }
    }
}
