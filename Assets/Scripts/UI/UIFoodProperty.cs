using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIFoodProperty : MonoBehaviour
{
    public TextMeshProUGUI keyText;
    public TextMeshProUGUI valueText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(PropertyType type, int value)
    {
        keyText.text = type.ToString();
        valueText.color = value > 0 ? GameUtil.Instance.hexToColor("#CAFFBF") : GameUtil.Instance.hexToColor("#FFADAD");
        valueText.text = value.ToString();
    }
}
