using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UniRx;
using UniRx.Triggers;

public class DescHintComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //[Multiline]//允许多行输入
    public string text = "多语言文本字段名";
    private string hint_text = null;
    public BehaviorSubject<bool> isMouseEnter = new BehaviorSubject<bool>(false);

    public void Start()
    {
        DataManager.Instance.DataLoaded.AsObservable().Subscribe(loaded =>
        {
            if (loaded)
            {
                hint_text = DataManager.Instance.Language[text];
            }
        });

        this.isMouseEnter.AsObservable().DistinctUntilChanged().Subscribe(isEnter =>
        {
            if (isEnter & hint_text != null)
            {
                //Vector3 vector3 = new Vector3(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
                UIDescHint hint = UIManager.Instance.Show<UIDescHint>(CanvasType.tooltip);
                hint.UpdateDesc(hint_text);
                print("DescHint show");
            }
            else
            {
                UIManager.Instance.Close<UIDescHint>();
                print("DescHint Close");
            }
        });
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseEnter.OnNext(true);
        print("isMouseEnter: true");
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseEnter.OnNext(false);
        print("isMouseEnter: false");
    }
}


