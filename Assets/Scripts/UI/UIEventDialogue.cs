using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIEventDialogue : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI content;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(EventDialogue dialogue)
    {
        nameText.text = GameUtil.Instance.GetDirectDisplayString(dialogue.name);
        content.text = GameUtil.Instance.GetDirectDisplayString(dialogue.content);
    }
}
