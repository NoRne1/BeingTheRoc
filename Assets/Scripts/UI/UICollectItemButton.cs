using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UICollectItemButton : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI title;
    public CollectItemModel model;
    // Start is called before the first frame update
    public void Setup(CollectItemModel model)
    {
        this.model = model;
        icon.overrideSprite = Resloader.LoadSprite(model.Resource, ConstValue.collectItemIconsPath);
        title.text = model.title + "x" + model.num.ToString();
    }
}
