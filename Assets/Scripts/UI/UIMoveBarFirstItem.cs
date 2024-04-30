using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMoveBarFirstItem : MonoBehaviour
{
    public Image bg;
    public Image icon;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(BattleItem item)
    {
        icon.overrideSprite = Resloader.LoadSprite(item.Resource);
        switch (item.battleItemType)
        {
            case BattleItemType.player:
                bg.overrideSprite = Resloader.LoadSprite("move_bar_first_blue_bg");
                break;
            case BattleItemType.enemy:
                bg.overrideSprite = Resloader.LoadSprite("move_bar_first_red_bg");
                break;
            case BattleItemType.sceneItem:
                bg.overrideSprite = Resloader.LoadSprite("move_bar_first_grey_bg");
                break;
            case BattleItemType.time:
                Debug.LogError("UIMoveBarFirstItem BattleItemType == time error!");
                break;
        }
    }
}
