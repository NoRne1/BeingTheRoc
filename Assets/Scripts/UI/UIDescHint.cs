using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIDescHint: UIHintBase
{
    public TextMeshProUGUI desc_hint;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    public void Update()
    {
        updatePostion();
    }

    public void Setup(string text)
    {
        desc_hint.text = text;
        StartCoroutine(SetupComplete());
    }
}
