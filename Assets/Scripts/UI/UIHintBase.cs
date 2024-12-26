using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHintBase : UIWindow
{
    public Vector2 prefabSize;
    private bool setFlag = false;
    public void updatePostion()
    {
        if (setFlag)
        {
            // 获取鼠标在屏幕上的位置   
            Vector2 screenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            // 计算合适的偏移量
            Vector2 offset = GameUtil.Instance.CalculateOffset(screenPosition, prefabSize);
            Vector2 temp = Camera.main.ScreenToWorldPoint(screenPosition + offset);
            // 更新位置
            gameObject.transform.position = new Vector3(temp.x, temp.y, gameObject.transform.position.z);
        }
    }

    public IEnumerator SetupComplete()
    {
        // 强制更新布局
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.GetComponent<RectTransform>());
        // 等待布局调整完成
        yield return new WaitForEndOfFrame();
        // 获取最新尺寸
        prefabSize = gameObject.GetComponent<RectTransform>().sizeDelta;
        setFlag = true;
    }
}
