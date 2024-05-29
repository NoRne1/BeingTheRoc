using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISkillButton : MonoBehaviour
{
    public Image icon;
    public HintComponent hint;
    // Start is called before the first frame update
    void Start()
    {
    }

    public void Setup(SkillDefine skill)
    {
        if (skill != null)
        {
            icon.overrideSprite = Resloader.LoadSprite(skill.Resource, ConstValue.skillsPath);
            hint.Setup(skill);
        } else
        {
            icon.overrideSprite = Resloader.LoadSprite("Skill_Lock", ConstValue.skillsPath);
            hint.Reset();
        }
    }
}
