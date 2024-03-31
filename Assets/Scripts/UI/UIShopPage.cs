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

        GameManager.Instance.timeLeft.AsObservable().Subscribe(time =>
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
                StoreItemDefine temp = new StoreItemDefine();
                temp.ID = -1;
                shopItems[i].SetStoreItemInfo(temp);
            }
            
        }
    }
}
