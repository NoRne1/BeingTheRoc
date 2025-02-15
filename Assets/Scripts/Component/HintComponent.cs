using System;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public enum HintType
{
    none = 0,
    normal = 1,
    storeItem = 2,
    skill = 3,
    character = 4,
    town = 5,
    feature = 6,
}

public class HintComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private HintType type;
    public UIHintBase hintObject;
    private string hint_text = null;
    private StoreItemDefine storeItem = null;
    private SkillDefine skill = null;
    public CharacterModel Character { get { return character; } }
    private CharacterModel character = null;
    private TownModel townModel = null;
    private FeatureDefine featureDefine = null;
    public BehaviorSubject<bool> isMouseEnter = new BehaviorSubject<bool>(false);

    public void Start()
    {
        this.isMouseEnter.AsObservable().DistinctUntilChanged().TakeUntilDestroy(this).Subscribe(isEnter =>
        {
            if (isEnter)
            {
                switch (type)
                {
                    case HintType.none:
                        break;
                    case HintType.normal:
                        UIDescHint descHint = UIManager.Instance.Show<UIDescHint>(CanvasType.tooltip);
                        descHint.Setup(hint_text);
                        hintObject = descHint;
                        break;
                    case HintType.storeItem:
                        UIStoreItemHint storeItemHint = UIManager.Instance.Show<UIStoreItemHint>(CanvasType.tooltip);
                        if(storeItem is StoreItemModel)
                        {
                            storeItemHint.Setup((StoreItemModel)storeItem);
                        } else {
                            storeItemHint.Setup(storeItem);
                        }
                        hintObject = storeItemHint;
                        break;
                    case HintType.skill:
                        UISkillHint skillHint = UIManager.Instance.Show<UISkillHint>(CanvasType.tooltip);
                        skillHint.Setup(skill);
                        hintObject = skillHint;
                        break;
                    case HintType.character:
                        UICharacterHint characterHint = UIManager.Instance.Show<UICharacterHint>(CanvasType.tooltip);
                        characterHint.Setup(character);
                        hintObject = characterHint;
                        break;
                    case HintType.town:
                        UITownHint townHint = UIManager.Instance.Show<UITownHint>(CanvasType.tooltip);
                        townHint.Setup(townModel);
                        hintObject = townHint;
                        break;
                    case HintType.feature:
                        UIDescHint featureHint = UIManager.Instance.Show<UIDescHint>(CanvasType.tooltip);
                        featureHint.Setup(GameUtil.Instance.GetDirectDisplayString(featureDefine.Desc));
                        hintObject = featureHint;
                        break;
                }
            }
            else
            {
                switch (type)
                {
                    case HintType.none:
                        break;
                    case HintType.normal:
                        UIManager.Instance.Close<UIDescHint>(false);
                        break;
                    case HintType.storeItem:
                        UIManager.Instance.Close<UIStoreItemHint>(false);
                        break;
                    case HintType.skill:
                        UIManager.Instance.Close<UISkillHint>(false);
                        break;
                    case HintType.character:
                        UIManager.Instance.Close<UICharacterHint>(false);
                        break;
                    case HintType.town:
                        UIManager.Instance.Close<UITownHint>(false);
                        break;
                    case HintType.feature:
                        UIManager.Instance.Close<UIDescHint>(false);
                        break;
                }
            }
        });
    }

    private void OnDestroy()
    {
        if (hintObject != null)
        {
            Destroy(hintObject.gameObject);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("HintComponent OnPointerEnter: " + eventData.ToString());
        isMouseEnter.OnNext(true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("HintComponent OnPointerExit: " + eventData.ToString());
        isMouseEnter.OnNext(false);
    }

    public void Reset()
    {
        type = HintType.none;
        hint_text = null;
        storeItem = null;
        skill = null;
    }

    public void Setup(string text)
    {
        type = HintType.normal;
        hint_text = GameUtil.Instance.GetDirectDisplayString(text);
    }

    public void Setup(StoreItemDefine storeItem)
    {
        type = HintType.storeItem;
        this.storeItem = storeItem;
    }

    public void Setup(SkillDefine skill)
    {
        type = HintType.skill;
        this.skill = skill;
    }

    public void Setup(CharacterModel character)
    {
        type = HintType.character;
        this.character = character;
    }

    public void Setup(TownModel townModel)
    {
        type = HintType.town;
        this.townModel = townModel;
    }

    public void Setup(FeatureDefine featureDefine)
    {
        type = HintType.feature;
        this.featureDefine = featureDefine;
    }
}


