using System.Collections;
using System.Collections.Generic;
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
                slots.Add(slot.postion, slot);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
