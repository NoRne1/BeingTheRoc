using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class UIPopButtonGroup : MonoBehaviour
{
    public bool AutoHidden
    {
        get { return autoHidden; }
        set
        {
            autoHidden = value;
            buttonGroup.transform.position = value ? hiddenPosition : visiblePosition;
        }
    }
    private bool autoHidden = true;

    public CheckPointerEnter checkPointerEnter; // 底部的CheckArea身上的脚本
    private CheckPointerEnter checkPointerEnterSelf;
    public GameObject buttonGroup; // 要弹出的按钮
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
        buttonGroup.transform.position = AutoHidden ? hiddenPosition : visiblePosition;
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
        LeanTween.move(buttonGroup, visiblePosition, 1f / animationSpeed).setEaseOutBounce();
    }

    void HideButton()
    {
        isButtonVisible = false;
        LeanTween.move(buttonGroup, hiddenPosition, 1f / animationSpeed).setEaseInBack();
    }
}
