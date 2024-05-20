using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

public enum HintType
{
    normal = 0,
    storeItem = 1,
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
                    case HintType.normal:
                        UIDescHint hint = UIManager.Instance.Show<UIDescHint>(CanvasType.tooltip);
                        hint.UpdateDesc(hint_text);
                        break;
                    case HintType.storeItem:
                        break;
                }
            }
            else
            {
                switch (type)
                {
                    case HintType.normal:
                        UIManager.Instance.Close<UIDescHint>();
                        break;
                    case HintType.storeItem:
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


