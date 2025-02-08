using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIConfirmWindow : UIWindow
{
    public TextMeshProUGUI confirmText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void Update()
    {
    }

    public void UpdateTip(string text)
    {
        confirmText.text = GameUtil.Instance.GetDirectDisplayString(text);
    }
}
