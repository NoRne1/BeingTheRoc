using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIEventButton : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI desc;

    public EventButton buttonInfo;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(EventButton button)
    {
        buttonInfo = button;
        title.text = GameUtil.Instance.GetDirectDisplayString(button.title);
        desc.text = GameUtil.Instance.GetDirectDisplayString(button.desc);
    }
}
