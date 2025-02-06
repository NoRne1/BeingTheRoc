using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISkillItem : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI desc;
    public SkillDefine skill;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(SkillDefine skill)
    {
        this.skill = skill;
        icon.overrideSprite = Resloader.LoadSprite(skill.Resource, ConstValue.skillsPath);
        desc.text = GameUtil.Instance.GetDirectDisplayString(skill.Desc);
    }
}
