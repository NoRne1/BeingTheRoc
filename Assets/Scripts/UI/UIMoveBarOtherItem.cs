using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIMoveBarOtherItem : MonoBehaviour
{
    public Image bg;
    public Image icon;
    public TextMeshProUGUI remainActingTime;
    public BattleItem item;
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
        this.item = item;
        icon.overrideSprite = Resloader.LoadSprite(item.Resource);
        remainActingTime.text = item.remainActingTime.ToString();
        switch (item.battleItemType)
        {
            case BattleItemType.player:
                bg.overrideSprite = Resloader.LoadSprite("move_bar_other_blue_bg");
                break;
            case BattleItemType.enemy:
                bg.overrideSprite = Resloader.LoadSprite("move_bar_other_red_bg");
                break;
            case BattleItemType.sceneItem:
                bg.overrideSprite = Resloader.LoadSprite("move_bar_other_grey_bg");
                break;
            case BattleItemType.time:
                Debug.LogError("UIMoveBarOtherItem BattleItemType == time error!");
                break;
        }
    }
}
