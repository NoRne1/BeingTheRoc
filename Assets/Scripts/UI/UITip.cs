using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UITip : UIWindow
{
    public TextMeshProUGUI tip;
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
        tip.text = text;
    }
}
