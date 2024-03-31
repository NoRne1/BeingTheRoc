using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIDescHint: UIWindow
{
    public TextMeshProUGUI desc_hint;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void Update()
    {
        var position = Input.mousePosition;
        transform.position = position + new Vector3(10, 10, 0);
    }

    public void UpdateDesc(string text)
    {
        desc_hint.text = text;
    }
}
