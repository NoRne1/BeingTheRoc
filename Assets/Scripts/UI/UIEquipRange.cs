using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEquipRange : MonoBehaviour
{
    public Dictionary<Vector2, UIEquipSlot> slots = new Dictionary<Vector2, UIEquipSlot>();

    void Awake()
    {
        UIEquipSlot[] tempSlots = GetComponentsInChildren<UIEquipSlot>();
        if (tempSlots.Length > 0)
        {
            foreach (UIEquipSlot slot in tempSlots)
            {
                slots.Add(slot.position, slot);
            }
        }
    }

    public bool Setup(StoreItemModel item)
    {
        foreach (var slot in slots.Values)
        {
            slot.gameObject.SetActive(false);
        }
        if (item.OccupiedCells == null)
        {
            gameObject.SetActive(false);
            return false;
        } else
        {
            foreach (var slotPos in item.OccupiedCells)
            {
                slots[slotPos].gameObject.SetActive(true);
            }
            gameObject.SetActive(true);
            return true;
        }
    }
}
