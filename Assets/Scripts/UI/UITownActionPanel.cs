using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public enum TownActionType
{
    bar = 0,
    forge = 1,
    shop = 2,
    game = 3,
    restaurant = 4,
}

public static class TownActionTypeExtensions
{
    public static PageType ToPageType(this TownActionType actionType)
    {
        switch(actionType)
        {
            case TownActionType.bar:
                return PageType.bar;
            case TownActionType.forge:
                return PageType.forge;
            case TownActionType.shop:
                return PageType.shop;
            case TownActionType.game:
                return PageType.game;
            case TownActionType.restaurant:
                return PageType.restaurant;
            default:
                throw new ArgumentException("Invalid TownActionType: " + actionType);
        }
    }
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
        action_icon.overrideSprite = Resloader.LoadSprite(define.iconResource, ConstValue.townActionPath);
        title.text = GameUtil.Instance.GetDisplayString(define.titleIndex);
        desc.text = GameUtil.Instance.GetDisplayString(define.descIndex);
    }

    public void OnClicked()
    {
        GameManager.Instance.SwitchPage(type.ToPageType());
    }
}
