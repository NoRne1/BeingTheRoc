using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFruitLuckyResult : UIFruitResultBase
{
    public Image icon;
    public override void Reset()
    {
        icon.color = Color.grey;
    } 
    public override void SetResult(int num)
    {
        if (num == 0)
        {
            icon.color = Color.grey;
        } else if (num == 1)
        {
            icon.color = Color.red;
        } else {
            Debug.LogError("UIFruitLuckyResult SetResult unexpected num");
        }
    }
}
