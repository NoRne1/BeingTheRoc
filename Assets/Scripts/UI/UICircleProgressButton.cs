using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class UICircleProgressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Image progressCircle;  // 用于显示圆形进度的Image（进度条）
    public float holdDuration = 2.0f;  // 长按持续时间
    public float decaySpeed = 4.0f;    // 松手后进度条快速衰减的速度
    public Func<bool> progressCheck = () => true;  // 外部传入的条件函数,判断是否启用进度条
    public Func<bool> buttonCheck = () => true;  // 外部传入的条件函数，判断按钮是否可以执行
    public Action onProgressCompleteAction;  // 外部传入的完成进度时要执行的函数
    public Action onImmediateAction;   // 外部传入的当条件不满足时执行的函数

    private bool isHolding = false;
    private float holdTime = 0f;
    private bool isDecaying = false;

    //长按行为执行后会重置holdTime，导致短按逻辑错误执行
    private bool progressActionDone = false;

    void Start()
    {
        if (progressCircle != null)
        {
            progressCircle.fillAmount = 0;
            progressCircle.gameObject.SetActive(false);  // 开始时隐藏进度条
        }
    }

    void Update()
    {
        // 如果正在长按并且条件检查通过
        if (isHolding && progressCheck != null && progressCheck.Invoke())
        {
            holdTime += Time.deltaTime;
            UpdateProgressCircle(holdTime / holdDuration);

            // 如果进度条满了，执行外部传入的完成函数
            if (holdTime >= holdDuration)
            {
                onProgressCompleteAction?.Invoke();
                ResetHold();
            }
        }
        // 如果松开按钮，并且进度条有剩余，则开始衰减
        else if (isDecaying)
        {
            holdTime -= Time.deltaTime * decaySpeed;
            UpdateProgressCircle(holdTime / holdDuration);

            // 进度条回退至0时隐藏
            if (holdTime <= 0)
            {
                holdTime = 0;
                isDecaying = false;
                progressCircle.gameObject.SetActive(false);
            }
        }
    }

    // 更新进度条显示
    private void UpdateProgressCircle(float progress)
    {
        if (progressCircle != null)
        {
            progressCircle.fillAmount = Mathf.Clamp01(progress);
            if (progressCircle.fillAmount > 0 && !progressCircle.gameObject.activeSelf)
            {
                progressCircle.gameObject.SetActive(true);
            }
        }
    }

    // 按下按钮时
    public void OnPointerDown(PointerEventData eventData)
    {
        progressActionDone = false;
        if (buttonCheck.Invoke())
        {
            isHolding = true;
            isDecaying = false;
        }
    }

    // 松开按钮时
    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
        if (holdTime < 0.1f && !progressActionDone)
        {
            // 短按，判断buttonCheck，执行onImmediateAction
            if (buttonCheck.Invoke())
            {
                onImmediateAction?.Invoke();
            }
        }
        if (holdTime > 0f)
        {
            // 长按，开始衰减
            isDecaying = true;
        }
    }

    // 重置长按状态
    private void ResetHold()
    {
        progressActionDone = true;
        isHolding = false;
        holdTime = 0;
        UpdateProgressCircle(0);
        progressCircle.gameObject.SetActive(false);
    }
}
