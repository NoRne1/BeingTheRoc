using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

public class CheckPointerEnter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public BehaviorSubject<bool> isMouseEnter = new BehaviorSubject<bool>(false);
    private Coroutine exitCoroutine;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseEnter.OnNext(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 取消之前的延迟更新
        if (exitCoroutine != null)
            StopCoroutine(exitCoroutine);

        // 开启延迟更新
        exitCoroutine = StartCoroutine(DelayedExitUpdate());
    }

    private IEnumerator DelayedExitUpdate()
    {
        // 延迟更新退出的目的是按钮浮出逻辑依靠CombineLatest，当鼠标从检测区域移入按钮区域时，
        // 先触发检测区域的exit，这时CombineLatest中的两个值都是false，这会使按钮直接触发隐藏

        // 等待一小段时间后再更新isMouseEnter的值
        yield return new WaitForSeconds(0.1f); // 调整这个时间间隔以满足你的需求
        isMouseEnter.OnNext(false);
    }
}
