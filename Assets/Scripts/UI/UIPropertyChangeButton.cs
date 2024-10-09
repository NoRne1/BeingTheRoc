using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using DG.Tweening;
using TMPro;

public class UIPropertyChangeButton : UICircleProgressButton
{
    public Button changeButton;
    public TextMeshProUGUI remainPoints;
    public Button plusButton;
    public Button minusButton;

    public GameObject transferRuleObject;
    public TextMeshProUGUI transferRuleText;
    public float animationDuration = 0.5f;

    public BehaviorSubject<bool> expandedSubject = new BehaviorSubject<bool>(false);
    private BehaviorSubject<AttributeType> selectedAttributeType = new BehaviorSubject<AttributeType>(AttributeType.None);
    public AttributeType SelectedAttributeType { get { return selectedAttributeType.Value; } }
    void Start()
    {
        plusButton.gameObject.SetActive(false);
        minusButton.gameObject.SetActive(false);
        transferRuleObject.SetActive(false);
        selectedAttributeType.Subscribe(attributeType => {
            var rule = GlobalAccess.GetPropertyTransferRuleFactor(attributeType);
            transferRuleText.text = rule.ToString();
        });
    }

    public void ToggleButtons()
    {
        SetSelectedAttributeType(AttributeType.None);
        expandedSubject.OnNext(!expandedSubject.Value);
        
        if (expandedSubject.Value)
        {
            ShowButtons();
        }
        else
        {
            HideButtons();
        }
    }

    void ShowButtons()
    {
        plusButton.gameObject.SetActive(true);
        minusButton.gameObject.SetActive(true);
        transferRuleObject.SetActive(true);
        
        // 移动和透明度动画
        plusButton.transform.DOMove(GameUtil.Instance.GetMovedWorldPosition(transform.position, new Vector3(0, -80, 0)), animationDuration).SetEase(Ease.OutCubic);
        minusButton.transform.DOMove(GameUtil.Instance.GetMovedWorldPosition(transform.position, new Vector3(0, -145, 0)), animationDuration).SetEase(Ease.OutCubic);
        transferRuleObject.transform.DOMove(GameUtil.Instance.GetMovedWorldPosition(transform.position, new Vector3(0, 70, 0)), animationDuration).SetEase(Ease.OutCubic);
        plusButton.GetComponent<CanvasGroup>().DOFade(1, animationDuration).SetEase(Ease.OutCubic);
        minusButton.GetComponent<CanvasGroup>().DOFade(1, animationDuration).SetEase(Ease.OutCubic);
        transferRuleObject.GetComponent<CanvasGroup>().DOFade(1, animationDuration).SetEase(Ease.OutCubic);
    }

    void HideButtons()
    {
        // 移动回原始位置和透明度变化
        plusButton.transform.DOMove(transform.position, animationDuration).SetEase(Ease.OutCubic);
        minusButton.transform.DOMove(transform.position, animationDuration).SetEase(Ease.OutCubic);
        transferRuleObject.transform.DOMove(transform.position, animationDuration).SetEase(Ease.OutCubic);
        plusButton.GetComponent<CanvasGroup>().DOFade(0, animationDuration).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            plusButton.gameObject.SetActive(false);
        });
        minusButton.GetComponent<CanvasGroup>().DOFade(0, animationDuration).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            minusButton.gameObject.SetActive(false);
        });
        transferRuleObject.GetComponent<CanvasGroup>().DOFade(0, animationDuration).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            transferRuleObject.SetActive(false);
        });
    }

    public void SetSelectedAttributeType(AttributeType type) 
    {
        selectedAttributeType.OnNext(type);
    }
}
