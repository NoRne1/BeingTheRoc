using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using DG.Tweening;

public class PopObjComponent : MonoBehaviour
{
    //public bool Hidden
    //{
    //    get { return hidden; }
    //    set
    //    {
    //        hidden = value;
    //        gameObject.SetActive();
    //    }
    //}
    //public bool hidden = false;
    public bool AutoHidden
    {
        get { return autoHidden; }
        set
        {
            autoHidden = value;
            objGroup.transform.position = value ? hiddenPosition : visiblePosition;
        }
    }
    private bool autoHidden = true;

    public CheckPointerEnter checkPointerEnter; // 底部的CheckArea身上的脚本
    private CheckPointerEnter checkPointerEnterSelf;
    public GameObject objGroup; // 要弹出的物体
    public float hoverTime = 1f; // 鼠标悬停时间
    public float animationSpeed = 1f; // 动画速度

    private bool isHovering;
    private bool isButtonVisible;
    private float hoverTimer;
    public Transform hiddenTransform;
    public Transform visibleTransform;
    
    private Vector3 hiddenPosition
    {
        get { return hiddenTransform.position; }
    }
    private Vector3 visiblePosition
    {
        get { return visibleTransform.position; }
    }


    void Start()
    {
        // 初始化按钮位置
        objGroup.transform.position = AutoHidden ? hiddenPosition : visiblePosition;
        checkPointerEnterSelf = GetComponent<CheckPointerEnter>();
        System.IObservable<bool>[] array = {checkPointerEnter.isMouseEnter.AsObservable(), checkPointerEnterSelf.isMouseEnter.AsObservable() };
        array.CombineLatest().Subscribe( isEnters => {
            if(AutoHidden)
            {
                bool isEnter = false;
                foreach (var item in isEnters)
                {
                    if (item == true)
                    {
                        isEnter = true;
                        break;
                    }
                }
                if (isEnter)
                {
                    isHovering = true;
                }
                else
                {
                    isHovering = false;
                    hoverTimer = 0;
                    if (isButtonVisible)
                    {
                        // 隐藏按钮
                        HideButton();
                    }
                }
            }
        });
    }

    void Update()
    {
        if (AutoHidden && isHovering)
        {
            hoverTimer += Time.deltaTime;

            if (hoverTimer >= hoverTime && !isButtonVisible)
            {
                // 显示按钮
                ShowButton();
            }
        }
    }

    void ShowButton()
    {
        isButtonVisible = true;
        objGroup.transform.DOMove(visiblePosition, 1f / animationSpeed)
            .SetEase(Ease.OutBounce); // 使用 Ease.OutBounce 缓动函数实现弹跳效果
    }

    void HideButton()
    {
        isButtonVisible = false;
        objGroup.transform.DOMove(hiddenPosition, 1f / animationSpeed)
            .SetEase(Ease.InBack); // 使用 Ease.InBack 缓动函数实现回弹效果
    }
}
