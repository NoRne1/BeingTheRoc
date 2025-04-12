using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class UIFiveElements : MonoBehaviour
{
    public FiveElementsPropertyDisplay propertyDisplay;
    public CheckPointerEnter pointerEnter;
    public GameObject labels;

    public void Start()
    {
        pointerEnter.isMouseEnter.AsObservable()
            .Subscribe(isEnter =>
            {
                labels.gameObject.SetActive(isEnter);
            })
            .AddTo(this);
    }
    public void Setup(FiveElements elementsData)
    {
        if (!elementsData.isValid)
        {
            gameObject.SetActive(false);
        }
        else
        {
            propertyDisplay.UpdateElements(elementsData);
            gameObject.SetActive(true);
        }
    }
}
