using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class UIForgePage : MonoBehaviour
{
    public List<Toggle> panelToggles;
    public Sprite selectedSprite;
    public Sprite unselectedSprite;
    public UIForgeEnhancePanel enhancePanel;
    public UIForgeMergePanel mergePanel;
    // Start is called before the first frame update
    void Start()
    {
        foreach(var index in Enumerable.Range(0, panelToggles.Count)) 
        {
            panelToggles[index].OnValueChangedAsObservable().Subscribe(selected => {
                ToggleAnim(panelToggles[index], selected);
                if(selected) { SwitchPanel(index); }
            }).AddTo(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ToggleAnim(Toggle toggle, bool selected)
    {
        toggle.GetComponent<Image>().overrideSprite = selected ? selectedSprite : unselectedSprite;

        var rectTransform = toggle.GetComponent<RectTransform>();
        Vector2 sizeDelta = rectTransform.sizeDelta;

        sizeDelta.y = selected ? 60 : 80;
        rectTransform.sizeDelta = sizeDelta;

        // 使用 DOTween 动画变化高度
        rectTransform.DOSizeDelta(selected ? new Vector2(sizeDelta.x, 80) : new Vector2(sizeDelta.x, 60), 0.3f).SetEase(Ease.OutQuad);
    }

    private void SwitchPanel(int index)
    {
        if (index == 0)
        {
            enhancePanel.gameObject.SetActive(true);
            mergePanel.gameObject.SetActive(false);
        } else if (index == 1){
            enhancePanel.gameObject.SetActive(false);
            mergePanel.gameObject.SetActive(true);
        } else {
            Debug.LogError("Unknown panel index!");
        }
    }
}
