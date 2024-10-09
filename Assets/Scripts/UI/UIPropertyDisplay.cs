using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPropertyDisplay : MonoBehaviour
{
    public AttributeType attributeType;
    public Toggle selfToggle;
    public TextMeshProUGUI key;
    public TextMeshProUGUI value;
    public Image selectedBG;

    public void SetupKey(string keyText) 
    {
        key.text = GameUtil.Instance.GetDisplayString(keyText);
    }
    public void SetupValue(string valueText) 
    {
        value.text = GameUtil.Instance.GetDisplayString(valueText);
    }

    public void SetToggleActive(bool active)
    {
        selfToggle.interactable = active;
    }
}
