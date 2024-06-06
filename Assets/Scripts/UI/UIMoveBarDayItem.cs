using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIMoveBarDayItem : MonoBehaviour
{
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
        if (item.battleItemType == BattleItemType.time)
        {
            remainActingTime.text = Mathf.CeilToInt(item.remainActingDistance / item.Speed).ToString();
        } else {
            Debug.LogError("UIMoveBarDayItem BattleItemType != time error!");
        }
    }
}
