using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class UIPropertyPanel : MonoBehaviour
{
    public TextMeshProUGUI title;
    public ToggleGroup toggleGroup;
    private Dictionary<AttributeType, UIPropertyDisplay> propertyDisplays = new Dictionary<AttributeType, UIPropertyDisplay>();
    public UIPropertyChangeButton changeButton;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI expText;

    public Button levelUpButton;
    public Slider expSlider;

    public List<UISkillButton> skillButtons;

    public System.IDisposable disposable;
    // Start is called before the first frame update
    void Awake()
    {
        foreach (var propertyDisplay in GetComponentsInChildren<UIPropertyDisplay>())
        {
            propertyDisplays.Add(propertyDisplay.attributeType, propertyDisplay);
            propertyDisplay.SetupKey(propertyDisplay.attributeType.ToString());
        }
    }

    void Start() 
    {
        changeButton.expandedSubject.AsObservable().Subscribe(expanded => {
            //取消选中
            toggleGroup.GetFirstActiveToggle().IfNotNull(toggle => { toggle.isOn = false; });
            //设置toggle可交互
            foreach(var propertyDisplay in propertyDisplays.Values) 
            {
                propertyDisplay.SetToggleActive(expanded);
            }
        }).AddTo(this);

        foreach(var propertyDisplay in propertyDisplays.Values) 
        {
            propertyDisplay.selfToggle.OnValueChangedAsObservable().Subscribe(selected => {
                changeButton.SetSelectedAttributeType(selected ? propertyDisplay.attributeType : AttributeType.None);
            }).AddTo(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(CharacterModel character)
    {
        expSlider.maxValue = GlobalAccess.levelUpExp;
        if (character != null)
        {
            disposable.IfNotNull(dis => { dis.Dispose(); });
            disposable = NorneStore.Instance.ObservableObject<CharacterModel>(character)
                .AsObservable().TakeUntilDestroy(this).Subscribe(cm =>
                {
                    levelText.text = "Lv: " + cm.attributes.level.ToString();
                    expSlider.value = Mathf.Min(cm.attributes.remainExp, GlobalAccess.levelUpExp);
                    expText.text = cm.attributes.remainExp.ToString() + "/" + GlobalAccess.levelUpExp.ToString();

                    levelUpButton.gameObject.SetActive(cm.attributes.remainExp > GlobalAccess.levelUpExp &&
                        cm.attributes.level < GlobalAccess.maxLevel);
                    title.text = cm.Name;
                    changeButton.remainPoints.text = cm.attributes.RemainPropertyPoints.ToString();
                    foreach (var propertyDisplay in propertyDisplays.Values)
                    {
                        propertyDisplay.SetupValue(cm.attributes.getFinalPropertyValue(propertyDisplay.attributeType).ToString());
                    }
                    skillButtons[0].Setup(cm.BornSkill == -1 ? null : DataManager.Instance.Skills[cm.BornSkill]);
                    skillButtons[1].Setup(cm.Skill1 == -1 ? null : DataManager.Instance.Skills[cm.Skill1]);
                    skillButtons[2].Setup(cm.Skill2 == -1 ? null : DataManager.Instance.Skills[cm.Skill2]);
                    skillButtons[3].Setup(cm.Skill3 == -1 ? null : DataManager.Instance.Skills[cm.Skill3]);
                    skillButtons[1].gameObject.SetActive((int)cm.Level >= (int)GeneralLevel.green);
                    skillButtons[2].gameObject.SetActive((int)cm.Level >= (int)GeneralLevel.blue);
                    skillButtons[3].gameObject.SetActive((int)cm.Level >= (int)GeneralLevel.red);
                });
        }
        else
        {
            Debug.Log("UITeamInfoPage setup character is null");
        }
    }

    public void Setup(BattleItem battleItem)
    {
        expSlider.maxValue = GlobalAccess.levelUpExp;
        if (battleItem != null)
        {
            disposable.IfNotNull(dis => { dis.Dispose(); });
            disposable = NorneStore.Instance.ObservableObject<BattleItem>(battleItem)
                .AsObservable().TakeUntilDestroy(this).Subscribe(bi =>
                {
                    levelText.text = "Lv: " + bi.attributes.level.ToString();
                    expSlider.value = Mathf.Min(bi.attributes.remainExp, GlobalAccess.levelUpExp);
                    expText.text = bi.attributes.remainExp.ToString() + "/" + GlobalAccess.levelUpExp.ToString();
                    levelUpButton.gameObject.SetActive(bi.attributes.remainExp > GlobalAccess.levelUpExp &&
                        bi.attributes.level < GlobalAccess.maxLevel);
                    title.text = bi.Name;
                    foreach (var propertyDisplay in propertyDisplays.Values) 
                    {
                        propertyDisplay.SetupValue(bi.attributes.getFinalPropertyValue(propertyDisplay.attributeType).ToString());
                    }
                    skillButtons[0].Setup(bi.BornSkill == -1 ? null : DataManager.Instance.Skills[bi.BornSkill]);
                    skillButtons[1].Setup(bi.Skill1 == -1 ? null : DataManager.Instance.Skills[bi.Skill1]);
                    skillButtons[2].Setup(bi.Skill2 == -1 ? null : DataManager.Instance.Skills[bi.Skill2]);
                    skillButtons[3].Setup(bi.Skill3 == -1 ? null : DataManager.Instance.Skills[bi.Skill3]);
                    skillButtons[1].gameObject.SetActive((int)bi.Level >= (int)GeneralLevel.green);
                    skillButtons[2].gameObject.SetActive((int)bi.Level >= (int)GeneralLevel.blue);
                    skillButtons[3].gameObject.SetActive((int)bi.Level >= (int)GeneralLevel.red);
                });
        }
        else
        {
            Debug.Log("UITeamInfoPage setup battleItem is null");
        }
    }
}
