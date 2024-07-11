using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialItemDefine
{
    public int ID { get; set; }

    public SpecialItemDefine Copy()
    {
        SpecialItemDefine copy = (SpecialItemDefine)this.MemberwiseClone();

        return copy;
    }
}
