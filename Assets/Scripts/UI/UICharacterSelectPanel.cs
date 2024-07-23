using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterSelectPanel : MonoBehaviour
{
    public TextMeshProUGUI please_select_tip;
    public GameObject character_info_panel;

    public TextMeshProUGUI MaxHP_key;
    public TextMeshProUGUI Strength_key;
    public TextMeshProUGUI Defense_key;
    public TextMeshProUGUI Dodge_key;
    public TextMeshProUGUI Accuracy_key;
    public TextMeshProUGUI Speed_key;
    public TextMeshProUGUI Mobility_key;
    public TextMeshProUGUI Energy_key;

    public TextMeshProUGUI Go_ahead_key;

    public Image Character_icon;
    public TextMeshProUGUI MaxHP;
    public TextMeshProUGUI Strength;
    public TextMeshProUGUI Defense;
    public TextMeshProUGUI Dodge;
    public TextMeshProUGUI Accuracy;
    public TextMeshProUGUI Speed;
    public TextMeshProUGUI Mobility;
    public TextMeshProUGUI Energy;

    public List<UISkillButton> skillButtons;

    private int SelectedCharacterId = -1;

    // Start is called before the first frame update
    void Start()
    {
        DataManager.Instance.DataLoaded.AsObservable().TakeUntilDestroy(this).Subscribe(loaded =>
        {
            if (loaded)
            {
                this.Init();
            }
        });
        please_select_tip.text = GameUtil.Instance.GetDisplayString("select_character");
        please_select_tip.gameObject.SetActive(true);
        character_info_panel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init()
    {
        MaxHP_key.text = GameUtil.Instance.GetDisplayString("health");
        Strength_key.text = GameUtil.Instance.GetDisplayString("strength");
        Defense_key.text = GameUtil.Instance.GetDisplayString("defense");
        Dodge_key.text = GameUtil.Instance.GetDisplayString("dodge");
        Accuracy_key.text = GameUtil.Instance.GetDisplayString("accuracy");
        Speed_key.text = GameUtil.Instance.GetDisplayString("speed");
        Mobility_key.text = GameUtil.Instance.GetDisplayString("mobility");
        Energy_key.text = GameUtil.Instance.GetDisplayString("energy");

        Go_ahead_key.text = GameUtil.Instance.GetDisplayString("go_ahead");
    }

    public void SelectedCharacter(int id)
    {
        if (DataManager.Instance.Characters.Keys.ToList().Contains(id))
        {
            CharacterDefine cd = DataManager.Instance.Characters[id];
            MaxHP.text = cd.MaxHP.ToString();
            Strength.text = cd.Strength.ToString();
            Defense.text = cd.Defense.ToString();
            Dodge.text = cd.Dodge.ToString();
            Accuracy.text = cd.Accuracy.ToString();
            Speed.text = cd.Speed.ToString();
            Mobility.text = cd.Mobility.ToString();
            Energy.text = cd.Energy.ToString();
            Character_icon.overrideSprite = Resloader.LoadSprite(cd.Resource, ConstValue.playersPath);
            this.SelectedCharacterId = id;
            please_select_tip.gameObject.SetActive(false);
            character_info_panel.SetActive(true);

            skillButtons[0].Setup(DataManager.Instance.Skills[cd.BornSkill]);
            skillButtons[1].Setup(DataManager.Instance.Skills[cd.Skill1]);
            skillButtons[2].Setup(DataManager.Instance.Skills[cd.Skill2]);
            skillButtons[3].Setup(DataManager.Instance.Skills[cd.Skill3]);
        } else
        {
            UITip tip = UIManager.Instance.Show<UITip>();
            tip.UpdateTip(GameUtil.Instance.GetDisplayString("wrong_character_selected_tip"));
        }
        
    }

    public void GoAhead()
    {
        if (SelectedCharacterId == -1)
        {
            UITip tip = UIManager.Instance.Show<UITip>();
            tip.UpdateTip(GameUtil.Instance.GetDisplayString("no_character_selected_tip"));
        } else if (DataManager.Instance.Characters.Keys.ToList().Contains(SelectedCharacterId))
        {
            GlobalAccess.CurrentCharacterId = SelectedCharacterId;
            SceneManager.Instance.LoadScene("game");
        } else {
            UITip tip = UIManager.Instance.Show<UITip>();
            tip.UpdateTip(GameUtil.Instance.GetDisplayString("wrong_character_selected_tip"));
        }

    }
}
