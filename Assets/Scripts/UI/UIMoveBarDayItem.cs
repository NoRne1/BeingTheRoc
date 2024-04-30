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
        remainActingTime.text = item.remainActingTime.ToString();
        if (item.battleItemType == BattleItemType.time)
        {

        } else {
            Debug.LogError("UIMoveBarDayItem BattleItemType != time error!");
        }
    }
}
