using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

public enum HintType
{
    none = 0,
    normal = 1,
    storeItem = 2,
}

public class HintComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private HintType type;

    private string hint_text = null;
    private StoreItemModel storeItem = null;
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
    }

    public void Setup(string text)
    {
        type = HintType.normal;
        hint_text = DataManager.Instance.Language[text];
    }

    public void Setup(StoreItemModel storeItem)
    {
        type = HintType.storeItem;
        this.storeItem = storeItem;
    }
}


