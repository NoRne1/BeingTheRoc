using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIShowBuffItem : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI title;
    public TextMeshProUGUI desc;
    public GameObject numObj;
    public TextMeshProUGUI num;

    public void Setup(BuffModel buff)
    {
        icon.overrideSprite = Resloader.LoadSprite(buff.Resource, ConstValue.buffsPath);
        title.text = GameUtil.Instance.GetDirectDisplayString(buff.Name);
        desc.text = GameUtil.Instance.GetDirectDisplayString(buff.Description);
        numObj.SetActive(buff.num > 1);
        num.text = buff.num.ToString();
    }
}
