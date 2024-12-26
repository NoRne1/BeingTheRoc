using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UISkillHint : UIHintBase
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI desc;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        updatePostion();
    }

    public void Setup(SkillDefine skill)
    {
        title.text = skill.Title;
        desc.text = skill.Desc;
        StartCoroutine(SetupComplete());
    }
}
