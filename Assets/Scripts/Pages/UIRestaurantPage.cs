using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Linq;

public class UIRestaurantPage : MonoBehaviour
{
    public List<StoreItemDefine> sellableItems;
    public List<UIFoodOption> foodOptions;
    public List<UICharacterHungryItem> characterHungryItems;
    private int timeLeft = -1;
    // Start is called before the first frame update
    void Start()
    {
        this.sellableItems= DataManager.Instance.StoreItems.Values.Where(item => item.sellType == SellType.restaurant).ToList();
        timeLeft = GameManager.Instance.timeLeft.Value;
        foreach(var index in Enumerable.Range(0, foodOptions.Count))
        {
            foodOptions[index].selfButton.OnClickAsObservable().TakeUntilDestroy(this).Subscribe(time =>
            {
                BuyItem(index);
            });
        }
        GenerateItems();

        foreach(var index in Enumerable.Range(0, characterHungryItems.Count))
        {
            if (index < GameManager.Instance.characterRelaysDic.Count)
            {
                characterHungryItems[index].Setup(GameManager.Instance.characterRelaysDic.GetValueByIndex(index).Value);
                characterHungryItems[index].gameObject.SetActive(true);
            } else {
                characterHungryItems[index].gameObject.SetActive(false);
            }
        }

        GameManager.Instance.timeLeft.AsObservable().TakeUntilDestroy(this).Subscribe(time =>
        {
            if (timeLeft != time)
            {
                timeLeft = time;
                GenerateItems();
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
            GenerateItems();
        }
    }

    public void GenerateItems()
    {
        var itemList = RefreshItems();
        if(foodOptions.Count == itemList.Count)
        {
            for (int i = 0; i < foodOptions.Count; i++)
            {
                foodOptions[i].Setup(itemList[i]);
            }
        } else 
        {
            Debug.LogError("UIRestaurantPage GenerateItems foodOptions.Count != itemList.Count");
        }
    }

    public List<StoreItemModel> RefreshItems()
    {
        // 如果请求的数量大于可用的项目数量进行调整
        var count = Mathf.Min(3, sellableItems.Count);

        // 使用 HashSet 确保选择的项是唯一的
        HashSet<int> selectedIndices = new HashSet<int>();
        List<StoreItemDefine> selectedItems = new List<StoreItemDefine>();

        while (selectedItems.Count < count)
        {
            int randomIndex = Random.Range(0, sellableItems.Count);
            if (selectedIndices.Add(randomIndex)) // 如果成功添加，则意味着该索引是唯一的
            {
                selectedItems.Add(sellableItems[randomIndex]);
            }
        }
        return selectedItems.Select(Item=>new StoreItemModel(Item)).ToList();
    }

    public void BuyItem(int index)
    {
        var actualPrice = foodOptions[index].itemModel.price + foodOptions[index].itemModel.foodModel.priceFloatFactor;
        if (actualPrice > GameManager.Instance.featherCoin.Value)
        {
            //钱不够买
            UITip tip = UIManager.Instance.Show<UITip>();
            //todo
            tip.UpdateTip("buy_food_no_money");
        } else if (!GameManager.Instance.repository.remainOpacity)
        {
            //仓库空间不够了
            UITip tip = UIManager.Instance.Show<UITip>();
            //todo
            tip.UpdateTip("bag_beyond_limit");
        } else
        {
            foodOptions[index].ItemSold(true);
            GameManager.Instance.FeatherCoinChanged(-actualPrice);
            GameManager.Instance.repository.AddItem(foodOptions[index].itemModel);
        }
    }
}
