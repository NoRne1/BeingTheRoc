using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIShowBuffItem : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI desc;
    public GameObject numObj;
    public TextMeshProUGUI num;

    public void Setup(BuffModel buff)
    {
        icon.overrideSprite = Resloader.LoadSprite(buff.Resource, ConstValue.buffsPath);
        desc.text = GameUtil.Instance.GetDisplayString(buff.Description);
        numObj.SetActive(buff.num > 1);
        desc.text = buff.num.ToString();
    }
}
