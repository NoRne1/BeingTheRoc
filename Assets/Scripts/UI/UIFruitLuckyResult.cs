using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFruitLuckyResult : UIFruitResultBase
{
    public Image icon;
    public Sprite normalSprite;
    public Sprite selectedSprite;
    public bool Selected { get { return selected; }}
    private bool selected;
    public void SetSelected(bool selected)
    {
        this.selected = selected;
        icon.overrideSprite = selected ? selectedSprite : normalSprite;
    }

    public override void Reset()
    {
        SetSelected(false);
    } 
    public override void SetResult(int num)
    {
        if (num == 0)
        {
            SetSelected(false);
        } else if (num == 1)
        {
            SetSelected(true);
        } else {
            Debug.LogError("UIFruitLuckyResult SetResult unexpected num");
        }
    }
}
