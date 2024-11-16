using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterHint : UIHintBase
{
    public Image selfBG;
    public List<Sprite> bgSprite;
    public TextMeshProUGUI title;
    public TextMeshProUGUI race;
    public Image jobIcon;
    public List<UISkillButton> skillButtons;
    private Dictionary<AttributeType, UIPropertyIconDisplay> propertyDisplays = new Dictionary<AttributeType, UIPropertyIconDisplay>();
    
    public TextMeshProUGUI priceText;
    public int price;

    public System.IDisposable disposable;
    public CharacterModel model;

    void Awake()
    {
        foreach (var propertyDisplay in GetComponentsInChildren<UIPropertyIconDisplay>())
        {
            propertyDisplays.Add(propertyDisplay.attributeType, propertyDisplay);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    public void Update()
    {
        updatePostion();
    }

    public void Setup(CharacterModel cm)
    {
        selfBG.overrideSprite = bgSprite[(int)cm.Level];
        title.text = cm.Name;
        race.text = cm.define.Race;
        jobIcon.overrideSprite = Resloader.LoadSprite(cm.Job.ToString(), ConstValue.jobIconsPath);
        
        skillButtons[0].Setup(cm.BornSkill == -1 ? null : DataManager.Instance.Skills[cm.BornSkill]);
        skillButtons[1].Setup(cm.Skill1 == -1 ? null : DataManager.Instance.Skills[cm.Skill1]);
        skillButtons[2].Setup(cm.Skill2 == -1 ? null : DataManager.Instance.Skills[cm.Skill2]);
        skillButtons[3].Setup(cm.Skill3 == -1 ? null : DataManager.Instance.Skills[cm.Skill3]);
        skillButtons[1].gameObject.SetActive((int)cm.Level >= (int)GeneralLevel.green);
        skillButtons[2].gameObject.SetActive((int)cm.Level >= (int)GeneralLevel.blue);
        skillButtons[3].gameObject.SetActive((int)cm.Level >= (int)GeneralLevel.red);

        foreach (var propertyDisplay in propertyDisplays.Values)
        {
            propertyDisplay.SetupValue(cm.attributes.getFinalPropertyValue(propertyDisplay.attributeType).ToString());
        }
        price = (((int)cm.Level + 1) * 500) + GameUtil.Instance.GetTrulyFloatFactor(((int)cm.Level + 1) * 100);
        priceText.text = price.ToString();
    }
}
