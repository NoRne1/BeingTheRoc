using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHintBase : UIWindow
{
    public void updatePostion()
    {
        // 获取鼠标在屏幕上的位置
        Vector2 screenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        // 计算大小
        Vector2 prefabSize = gameObject.GetComponent<RectTransform>().sizeDelta;

        // 计算合适的偏移量
        Vector2 offset = GameUtil.Instance.CalculateOffset(screenPosition, prefabSize);

        Vector2 temp = Camera.main.ScreenToWorldPoint(screenPosition + offset);
        // 更新位置
        gameObject.transform.position = new Vector3(temp.x, temp.y, gameObject.transform.position.z);
    }

    public IEnumerator InitLayoutPosition()
    {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        updatePostion();
    }
}
