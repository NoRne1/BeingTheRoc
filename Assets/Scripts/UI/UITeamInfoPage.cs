using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;

public class UITeamInfoPage : MonoBehaviour
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

    public Image Character_icon;

    public TextMeshProUGUI descText;
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
    public CharacterModel character;
    public BattleItem battleItem;
    // Start is called before the first frame update
    void Start()
    {
        MaxHP_key.text = DataManager.Instance.Language["health"];
        Strength_key.text = DataManager.Instance.Language["strength"];
        Defense_key.text = DataManager.Instance.Language["defense"];
        Dodge_key.text = DataManager.Instance.Language["dodge"];
        Accuracy_key.text = DataManager.Instance.Language["accuracy"];
        Speed_key.text = DataManager.Instance.Language["speed"];
        Mobility_key.text = DataManager.Instance.Language["mobility"];
        Energy_key.text = DataManager.Instance.Language["energy"];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateCharacter(CharacterModel character)
    {
        expSlider.maxValue = GlobalAccess.levelUpExp;
        this.character = character;
        this.battleItem = null;
        if (character != null)
        {
            disposable.IfNotNull(dis => { dis.Dispose(); });
            disposable = NorneStore.Instance.ObservableObject<CharacterModel>(character)
                .AsObservable().TakeUntilDestroy(this).Subscribe(cm =>
            {
                descText.text = cm.Desc;
                levelText.text = "Lv: " + cm.level.ToString();
                expSlider.value = Mathf.Min(cm.remainExp, GlobalAccess.levelUpExp);
                expText.text = cm.remainExp.ToString() + "/" + GlobalAccess.levelUpExp.ToString();

                levelUpButton.gameObject.SetActive(cm.remainExp > GlobalAccess.levelUpExp &&
                    cm.level < GlobalAccess.maxLevel);
                title.text = cm.Name;
                MaxHP.text = cm.MaxHP.ToString();
                Strength.text = cm.Strength.ToString();
                Defense.text = cm.Defense.ToString();
                Dodge.text = cm.Dodge.ToString();
                Accuracy.text = cm.Accuracy.ToString();
                Speed.text = cm.Speed.ToString();
                Mobility.text = cm.Mobility.ToString();
                Energy.text = cm.Energy.ToString();
                Character_icon.overrideSprite = Resloader.LoadSprite(cm.Resource, ConstValue.playersPath);

                skillButtons[0].Setup(cm.BornSkill == -1 ? null : DataManager.Instance.Skills[cm.BornSkill]);
                skillButtons[1].Setup(cm.Skill1 == -1 ? null : DataManager.Instance.Skills[cm.Skill1]);
                skillButtons[2].Setup(cm.Skill2 == -1 ? null : DataManager.Instance.Skills[cm.Skill2]);
                skillButtons[3].Setup(cm.Skill3 == -1 ? null : DataManager.Instance.Skills[cm.Skill3]);
                skillButtons[1].gameObject.SetActive((int)cm.Level >= (int)GeneralLevel.green);
                skillButtons[2].gameObject.SetActive((int)cm.Level >= (int)GeneralLevel.blue);
                skillButtons[3].gameObject.SetActive((int)cm.Level >= (int)GeneralLevel.red);
            });
        } else
        {
            Debug.Log("UITeamInfoPage setup character is null");
        }
    }

    public void UpdateBattleItem(BattleItem battleItem)
    {
        expSlider.maxValue = GlobalAccess.levelUpExp;
        this.character = null;
        this.battleItem = battleItem;
        if (battleItem != null)
        {
            disposable.IfNotNull(dis => { dis.Dispose(); });
            disposable = NorneStore.Instance.ObservableObject<BattleItem>(battleItem)
                .AsObservable().TakeUntilDestroy(this).Subscribe(bi =>
                {
                    descText.text = bi.Desc;
                    levelText.text = "Lv: " + bi.level.ToString();
                    expSlider.value = Mathf.Min(bi.remainExp, GlobalAccess.levelUpExp);
                    expText.text = bi.remainExp.ToString() + "/" + GlobalAccess.levelUpExp.ToString();
                    levelUpButton.gameObject.SetActive(bi.remainExp > GlobalAccess.levelUpExp &&
                        bi.level < GlobalAccess.maxLevel);
                    title.text = bi.Name;
                    MaxHP.text = bi.MaxHP.ToString();
                    Strength.text = bi.Strength.ToString();
                    Defense.text = bi.Defense.ToString();
                    Dodge.text = bi.Dodge.ToString();
                    Accuracy.text = bi.Accuracy.ToString();
                    Speed.text = bi.Speed.ToString();
                    Mobility.text = bi.Mobility.ToString();
                    Energy.text = bi.Energy.ToString();
                    Character_icon.overrideSprite = Resloader.LoadSprite(bi.Resource, ConstValue.playersPath);
                });
        }
        else
        {
            Debug.Log("UITeamInfoPage setup battleItem is null");
        }
    }
}
