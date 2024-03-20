using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum TownActionType
{
    bar = 0,
    forge = 1,
    shop = 2,
    train = 3,
    walk = 4
}

public class UITownActionPanel : MonoBehaviour
{
    public Image action_icon;
    public TextMeshProUGUI title;
    public TextMeshProUGUI desc;

    public TownActionType type;
    public TownActionDefine define;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetActionType(TownActionType type)
    {
        this.type = type;
        define = DataManager.Instance.TownActions[(int)type];
        action_icon.overrideSprite = Resloader.Load<Sprite>(ConstValue.spritePath + define.iconResource);
        title.text = DataManager.Instance.Language[define.titleIndex].ReplaceNewLines();
        desc.text = DataManager.Instance.Language[define.descIndex].ReplaceNewLines();
    }
}
