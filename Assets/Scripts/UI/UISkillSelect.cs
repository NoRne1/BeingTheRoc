using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UI;
using UniRx;

public class UISkillSelect : UIWindow
{
    public List<UISkillItem> skillItems = new List<UISkillItem>();
    public Action<SkillDefine> selectedAction = null;
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i< skillItems.Count; i++)
        {
            int index = i;
            skillItems[i].GetComponent<Button>().OnClickAsObservable().TakeUntilDestroy(this).Subscribe(_ =>
            {
                SkillSelected(skillItems[index].skill);
            });
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(CharacterModel character)
    {
        List<SkillDefine> skills = GetSkillOptions(character);
        for (int i = 0; i < skills.Count; i++)
        {
            skillItems[i].Setup(skills[i]);
        }
    }

    public void SkillSelected(SkillDefine skill)
    {
        if (selectedAction != null)
        {
            selectedAction(skill);
        }
        
        Close();
    }

    public List<SkillDefine> GetSkillOptions(CharacterModel character)
    {
        List<SkillDefine> result = new List<SkillDefine>();
        List<SkillDefine> skillRepo = DataManager.Instance.Skills.Values.Where((skill) =>
        {
            return skill.Job == character.Job || skill.Job == JobType.General;
        }).ToList();
        if (character.attributes.level == 0 && character.define.Skill1 != -1)
        {
            if (character.define.Skill1 == 1) 
            {
                //混元剑坯技能需要特殊处理
                if (GameUtil.Instance.GetRandomRate(50))
                {
                    result.Add(DataManager.Instance.Skills[2]);
                } else
                {
                    result.Add(DataManager.Instance.Skills[3]);
                }
            } else 
            {
                result.Add(DataManager.Instance.Skills[character.define.Skill1]);
            }
        } else if (character.attributes.level == 1 && character.define.Skill2 != -1)
        {
            if (character.define.Skill1 == 4) 
            {
                if (GameUtil.Instance.GetRandomRate(50))
                {
                    result.Add(DataManager.Instance.Skills[5]);
                } else
                {
                    result.Add(DataManager.Instance.Skills[6]);
                }
            } else 
            {
                result.Add(DataManager.Instance.Skills[character.define.Skill2]);
            }
            result.Add(DataManager.Instance.Skills[character.define.Skill2]);
        } else if (character.attributes.level == 2 && character.define.Skill3 != -1)
        {
            if (character.define.Skill1 == 7) 
            {
                if (GameUtil.Instance.GetRandomRate(50))
                {
                    result.Add(DataManager.Instance.Skills[8]);
                } else
                {
                    result.Add(DataManager.Instance.Skills[9]);
                }
            } else 
            {
                result.Add(DataManager.Instance.Skills[character.define.Skill3]);
            }
            result.Add(DataManager.Instance.Skills[character.define.Skill3]);
        }
        while (result.Count < 3)
        {
            int randomInt = UnityEngine.Random.Range(0, skillRepo.Count);
            if (result.Select((skill) => skill.ID).ToList().Contains(skillRepo[randomInt].ID))
            {
                continue;
            }
            result.Add(skillRepo[randomInt]);
        }
        return result;
    }
}
