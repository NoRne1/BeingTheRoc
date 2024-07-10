using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class UIChessboard : MonoBehaviour
{
    public Dictionary<Vector2, UIChessboardSlot> slots = new Dictionary<Vector2, UIChessboardSlot>();
    // Start is called before the first frame update
    void Start()
    {
        UIChessboardSlot[] tempSlots = GetComponentsInChildren<UIChessboardSlot>();
        if(tempSlots.Length > 0)
        {
            foreach(UIChessboardSlot slot in tempSlots)
            {
                slots.Add(slot.position, slot);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetColors(Dictionary<Vector2, ChessboardSlotColor> slotColors)
    {
        foreach (var slotColor in slotColors)
        {
            slots[slotColor.Key].SetColor(slotColor.Value);
        }
    }

    public void ResetColors()
    {
        foreach (var slot in slots.Values)
        {
            slot.SetColor(ChessboardSlotColor.none);
        }
    }

    public void ResetMiddle(bool flag, bool animated = true)
    {
        var rectTransform = GetComponent<RectTransform>();
        Vector2 targetPosition = flag ? new Vector2(0, rectTransform.anchoredPosition.y) : new Vector2(255, rectTransform.anchoredPosition.y);
        if (animated)
        {
            // 动画时长可以根据需要调整，这里设置为0.5秒
            float duration = 0.5f;
            // 使用 DOAnchorPos 方法来做插值动画
            rectTransform.DOAnchorPos(targetPosition, duration);
        } else
        {
            rectTransform.anchoredPosition = targetPosition;
        }
    }
}
