using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class UICollectCharacterButton : MonoBehaviour
{
    public Image selfBG;
    public List<Sprite> bgSprite;
    public TextMeshProUGUI title;
    public TextMeshProUGUI race;
    public Image jobIcon;
    public List<UISkillButton> skillButtons;
    private Dictionary<AttributeType, UIPropertyIconDisplay> propertyDisplays = new Dictionary<AttributeType, UIPropertyIconDisplay>();
    public System.IDisposable disposable;
    public CharacterModel model;

    // Start is called before the first frame update
    void Awake()
    {
        foreach (var propertyDisplay in GetComponentsInChildren<UIPropertyIconDisplay>())
        {
            propertyDisplays.Add(propertyDisplay.attributeType, propertyDisplay);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(CharacterModel character)
    {
        if (character != null)
        {
            model = character;
            disposable.IfNotNull(dis => { dis.Dispose(); });
            disposable = NorneStore.Instance.ObservableObject<CharacterModel>(character)
                .AsObservable().TakeUntilDestroy(this).Subscribe(cm =>
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
                });
        }
        else
        {
            Debug.Log("UITeamInfoPage setup character is null");
        }
    }
}
