using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

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

    private string hint_text = null;
    private StoreItemDefine storeItem = null;
    private SkillDefine skill = null;
    public BehaviorSubject<bool> isMouseEnter = new BehaviorSubject<bool>(false);

    public void Start()
    {
        //DataManager.Instance.DataLoaded.AsObservable().TakeUntilDestroy(this).Subscribe(loaded =>
        //{
        //    if (loaded)
        //    {
        //        hint_text = DataManager.Instance.Language[text];
        //    }
        //});

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
                        break;
                    case HintType.storeItem:
                        UIStoreItemHint storeItemHint = UIManager.Instance.Show<UIStoreItemHint>(CanvasType.tooltip);
                        storeItemHint.Setup(storeItem);
                        break;
                    case HintType.skill:
                        UISkillHint skillHint = UIManager.Instance.Show<UISkillHint>(CanvasType.tooltip);
                        skillHint.Setup(skill);
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseEnter.OnNext(true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
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
        hint_text = DataManager.Instance.Language[text];
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


