using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMoveBarDayItem : MonoBehaviour
{
    public Sprite timeIcon;
    public Sprite quitIcon;

    public Image icon;
    public TextMeshProUGUI remainActingTime;
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
        if (item.type == BattleItemType.time)
        {
            remainActingTime.text = Mathf.CeilToInt(item.remainActingDistance / item.attributes.Speed).ToString();
            icon.overrideSprite = timeIcon;
        } else if (item.type == BattleItemType.quitTime)
        {
            remainActingTime.text = Mathf.CeilToInt(item.remainActingDistance / item.attributes.Speed).ToString();
            icon.overrideSprite = quitIcon;
        } else {
            Debug.LogError("UIMoveBarDayItem BattleItemType != time error!");
        }
    }
}
