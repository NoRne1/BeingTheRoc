using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class UIBaseInfoPanel : MonoBehaviour
{
    public Image itemIcon;
    public Image hpIcon;
    public TextMeshProUGUI title;
    public RectTransform titleGroupTransform;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI expText;
    public TextMeshProUGUI hungryText;

    public Button levelUpButton;
    public Slider expSlider;
    public Slider hungrySlider;

    public List<UISkillButton> skillButtons;

    public System.IDisposable disposable;

    void Start() 
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(CharacterModel character)
    {
        expSlider.maxValue = GlobalAccess.levelUpExp;
        hungrySlider.maxValue = character.MaxHungry;
        if (character != null)
        {
            disposable.IfNotNull(dis => { dis.Dispose(); });
            disposable = NorneStore.Instance.ObservableObject<CharacterModel>(character)
                .AsObservable().TakeUntilDestroy(this).Subscribe(cm =>
                {
                    itemIcon.overrideSprite = Resloader.LoadSprite(cm.Resource, ConstValue.battleItemsPath);
                    hpIcon.fillAmount = cm.attributes.currentHP * 1.0f / cm.attributes.MaxHP;
                    levelText.text = "Lv: " + cm.attributes.level.ToString();
                    expSlider.value = Mathf.Min(cm.attributes.remainExp, GlobalAccess.levelUpExp);
                    expText.text = cm.attributes.remainExp.ToString() + "/" + GlobalAccess.levelUpExp.ToString();

                    hungrySlider.value = cm.CurrentHungry;
                    hungryText.text = cm.CurrentHungry.ToString() + "/" + cm.MaxHungry.ToString();

                    levelUpButton.gameObject.SetActive(cm.attributes.remainExp > GlobalAccess.levelUpExp &&
                        cm.attributes.level < GlobalAccess.maxLevel);
                    title.text = cm.Name;
                    title.color = GlobalAccess.GetLevelColor(cm.Level);
                    Canvas.ForceUpdateCanvases();
                    LayoutRebuilder.ForceRebuildLayoutImmediate(titleGroupTransform);
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
        hungrySlider.maxValue = battleItem.MaxHungry;
        if (battleItem != null)
        {
            disposable.IfNotNull(dis => { dis.Dispose(); });
            disposable = NorneStore.Instance.ObservableObject<BattleItem>(battleItem)
                .AsObservable().TakeUntilDestroy(this).Subscribe(bi =>
                {
                    itemIcon.overrideSprite = Resloader.LoadSprite(bi.Resource, ConstValue.battleItemsPath);
                    hpIcon.fillAmount = bi.attributes.currentHP * 1.0f / bi.attributes.MaxHP;
                    levelText.text = "Lv: " + bi.attributes.level.ToString();
                    expSlider.value = Mathf.Min(bi.attributes.remainExp, GlobalAccess.levelUpExp);
                    expText.text = bi.attributes.remainExp.ToString() + "/" + GlobalAccess.levelUpExp.ToString();

                    hungrySlider.value = bi.CurrentHungry;
                    hungryText.text = bi.CurrentHungry.ToString() + "/" + bi.MaxHungry.ToString();

                    levelUpButton.gameObject.SetActive(bi.attributes.remainExp > GlobalAccess.levelUpExp &&
                        bi.attributes.level < GlobalAccess.maxLevel);
                    title.text = bi.Name;
                    title.color = GlobalAccess.GetLevelColor(bi.Level);
                    Canvas.ForceUpdateCanvases();
                    LayoutRebuilder.ForceRebuildLayoutImmediate(titleGroupTransform);
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
