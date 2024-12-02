using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRepositorSlot : MonoBehaviour
{
    public int itemID;
    public StoreItemModel item;
    public HintComponent hint;
    
    void Awake()
    {
        hint = GetComponent<HintComponent>();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    public void Update()
    {

    }

    public void Setup(int itemID)
    {
        Transform itemImage = transform.GetChild(0);
        this.itemID = itemID;
        if (itemID != -1)
        {
            var define = DataManager.Instance.StoreItems[itemID];
            itemImage.GetComponent<Image>().overrideSprite =
                Resloader.LoadSprite(define.iconResource, ConstValue.equipsPath);
            itemImage.gameObject.SetActive(true);
            if (hint != null){ hint.Setup(define); }
        }
        else
        {
            itemImage.gameObject.SetActive(false);
            if (hint != null){ hint.Reset(); }
        }
    }

    public void Setup(StoreItemModel item)
    {
        Transform itemImage = transform.GetChild(0);
        if (item != null)
        {
            this.itemID = item.ID;
            this.item = item;
            itemImage.GetComponent<Image>().overrideSprite =
                Resloader.LoadSprite(item.iconResource, ConstValue.equipsPath);
            itemImage.gameObject.SetActive(true);
            if (hint != null){ hint.Setup(item); }
        }
        else
        {
            this.itemID = -1;
            itemImage.gameObject.SetActive(false);
            if (hint != null){ hint.Reset(); }
        }
    }
}
