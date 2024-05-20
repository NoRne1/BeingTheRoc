using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEquipRange : MonoBehaviour
{
    public Dictionary<Vector2, UIEquipSlot> slots = new Dictionary<Vector2, UIEquipSlot>();
    // Start is called before the first frame update
    void Start()
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

    public void Setup(StoreItemModel item)
    {
        foreach (var slot in slots.Values)
        {
            slot.gameObject.SetActive(false);
        }
        foreach (var slotPos in item.OccupiedCells)
        {
            slots[slotPos].gameObject.SetActive(true);
        }
    }
}
