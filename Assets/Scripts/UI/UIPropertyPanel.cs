using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class UIPropertyPanel : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI MaxHP_key;
    public TextMeshProUGUI Strength_key;
    public TextMeshProUGUI Defense_key;
    public TextMeshProUGUI Dodge_key;
    public TextMeshProUGUI Accuracy_key;
    public TextMeshProUGUI Speed_key;
    public TextMeshProUGUI Mobility_key;
    public TextMeshProUGUI Energy_key;

    public TextMeshProUGUI levelText;
    public TextMeshProUGUI expText;

    public TextMeshProUGUI MaxHP;
    public TextMeshProUGUI Strength;
    public TextMeshProUGUI Defense;
    public TextMeshProUGUI Dodge;
    public TextMeshProUGUI Accuracy;
    public TextMeshProUGUI Speed;
    public TextMeshProUGUI Mobility;
    public TextMeshProUGUI Energy;

    public Button levelUpButton;
    public Slider expSlider;

    public List<UISkillButton> skillButtons;

    public System.IDisposable disposable;
    // Start is called before the first frame update
    void Awake()
    {
        MaxHP_key.text = GameUtil.Instance.GetDisplayString("health");
        Strength_key.text = GameUtil.Instance.GetDisplayString("strength");
        Defense_key.text = GameUtil.Instance.GetDisplayString("defense");
        Dodge_key.text = GameUtil.Instance.GetDisplayString("dodge");
        Accuracy_key.text = GameUtil.Instance.GetDisplayString("accuracy");
        Speed_key.text = GameUtil.Instance.GetDisplayString("speed");
        Mobility_key.text = GameUtil.Instance.GetDisplayString("mobility");
        Energy_key.text = GameUtil.Instance.GetDisplayString("energy");
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
                    MaxHP.text = cm.attributes.MaxHP.ToString();
                    Strength.text = cm.attributes.Strength.ToString();
                    Defense.text = cm.attributes.Defense.ToString();
                    Dodge.text = cm.attributes.Dodge.ToString();
                    Accuracy.text = cm.attributes.Accuracy.ToString();
                    Speed.text = cm.attributes.Speed.ToString();
                    Mobility.text = cm.attributes.Mobility.ToString();
                    Energy.text = cm.attributes.Energy.ToString();
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
                    MaxHP.text = bi.attributes.MaxHP.ToString();
                    Strength.text = bi.attributes.Strength.ToString();
                    Defense.text = bi.attributes.Defense.ToString();
                    Dodge.text = bi.attributes.Dodge.ToString();
                    Accuracy.text = bi.attributes.Accuracy.ToString();
                    Speed.text = bi.attributes.Speed.ToString();
                    Mobility.text = bi.attributes.Mobility.ToString();
                    Energy.text = bi.attributes.Energy.ToString();
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