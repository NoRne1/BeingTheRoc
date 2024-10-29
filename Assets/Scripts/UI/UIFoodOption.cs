using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIFoodOption: MonoBehaviour
{
    public Image foodIcon;
    public TextMeshProUGUI foodName;
    public List<UIFoodProperty> uIFoodPropertys; 
    public TextMeshProUGUI priceText;
    public Button selfButton;
    public Image soldIcon;
    public RectTransform rectTransform;

    public StoreItemModel itemModel;
    

    public void Setup(StoreItemModel itemModel)
    {
        if (itemModel.type != ItemType.food || itemModel.foodModel == null) { return; }
        this.itemModel = itemModel;
        foodIcon.overrideSprite = Resloader.LoadSprite(itemModel.iconResource, ConstValue.foodIconsPath);
        foodName.text = itemModel.title;
        foreach (var index in Enumerable.Range(0, uIFoodPropertys.Count))
        {
            if (index < itemModel.foodModel.foodPropertys.Count)
            {
                var property = itemModel.foodModel.foodPropertys[index];
                uIFoodPropertys[index].Setup(property.type, property.value);
                uIFoodPropertys[index].gameObject.SetActive(true);
            } else 
            {
                uIFoodPropertys[index].gameObject.SetActive(false);
            }
        }
        priceText.text = (itemModel.price + itemModel.foodModel.priceFloatFactor).ToString();
        ItemSold(false);
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    public void ItemSold(bool sold)
    {
        selfButton.enabled = !sold;
        soldIcon.gameObject.SetActive(sold);
    }
}
