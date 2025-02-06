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
    public GameObject beforeDiscountPriceObject;
    public TextMeshProUGUI beforeDiscountPrice;
    public TextMeshProUGUI realPrice;
    public StoreItemModel info;
    public Button buyButton;
    public GameObject soldIcon;

    public HintComponent hintComponent;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetStoreItemInfo(StoreItemModel info)
    {
        if(info != null)
        {
            this.info = info;
            iconBG.color = GlobalAccess.GetLevelColor(info.level);
            icon.overrideSprite = Resloader.LoadSprite(info.iconResource, ConstValue.equipsPath);
            title.color = GlobalAccess.GetLevelColor(info.level);
            title.text = GameUtil.Instance.GetDirectDisplayString(info.title);
            beforeDiscountPriceObject.gameObject.SetActive(info.discount != 1.0f);
            beforeDiscountPrice.text = info.price.ToString();
            realPrice.text = info.realPrice.ToString();
            realPrice.color = info.discount != 1.0f ? Color.red : GameUtil.Instance.hexToColor("#824E11");
            hintComponent.Setup(info);
            this.gameObject.SetActive(true);
        } else
        {
            this.info = null;
            hintComponent.Reset();
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
