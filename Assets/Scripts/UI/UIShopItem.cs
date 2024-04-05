using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UIShopItem : MonoBehaviour
{
    public Image iconBG;
    public Image icon;
    public TextMeshProUGUI title;
    public TextMeshProUGUI price;
    public StoreItemDefine info;
    public Button buyButton;
    public GameObject soldIcon;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetStoreItemInfo(StoreItemDefine info)
    {
        if(info != null)
        {
            this.info = info;
            iconBG.color = GlobalAccess.GetLevelColor(info.level);
            icon.overrideSprite = Resloader.LoadSprite(info.iconResource);
            title.color = GlobalAccess.GetLevelColor(info.level);

            title.text = DataManager.Instance.Language[info.title];
            price.text = info.price.ToString();
            this.gameObject.SetActive(true);
        } else
        {
            this.info = null;
            this.gameObject.SetActive(false);
        }
        ItemSold(false);
    }

    public void ItemSold(bool sold)
    {
        buyButton.enabled = !sold;
        soldIcon.SetActive(sold);
    }
}
