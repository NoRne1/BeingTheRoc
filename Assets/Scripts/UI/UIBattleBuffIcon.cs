using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class UIBattleBuffIcon : MonoBehaviour
{
    public Image icon;
    public GameObject buffNumObject;
    public TextMeshProUGUI buffNumText;

    public void Setup(BuffModel buff)
    {
        icon.overrideSprite = Resloader.LoadSprite(buff.Resource, ConstValue.buffsPath);
        buffNumObject.SetActive(buff.num > 1);
        buffNumText.text = buff.num.ToString();
    }
}