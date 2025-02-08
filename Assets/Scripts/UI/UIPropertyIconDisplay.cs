using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPropertyIconDisplay : MonoBehaviour
{
    public AttributeType attributeType;
    public TextMeshProUGUI value;
    public void SetupValue(string valueText) 
    {
        value.text = GameUtil.Instance.GetDirectDisplayString(valueText);
    }
}
