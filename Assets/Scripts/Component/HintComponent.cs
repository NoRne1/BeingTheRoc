using System;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum HintType
{
    none = 0,
    normal = 1,
    storeItem = 2,
    skill = 3,
}

public class HintComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private HintType type;
    private UIHintBase hintObject;
    private string hint_text = null;
    private StoreItemDefine storeItem = null;
    private SkillDefine skill = null;
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
                        storeItemHint.Setup(storeItem);
                        hintObject = storeItemHint;
                        break;
                    case HintType.skill:
                        UISkillHint skillHint = UIManager.Instance.Show<UISkillHint>(CanvasType.tooltip);
                        skillHint.Setup(skill);
                        hintObject = skillHint;
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
                        UIManager.Instance.Close<UIDescHint>();
                        break;
                    case HintType.storeItem:
                        UIManager.Instance.Close<UIStoreItemHint>();
                        break;
                    case HintType.skill:
                        UIManager.Instance.Close<UISkillHint>();
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
        hint_text = GameUtil.Instance.GetDisplayString(text);
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
}


