using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpendableItemDefine
{
    public int ID { get; set; }
    public Effect effect { get; set; }

    public ExpendableItemDefine Copy()
    {
        ExpendableItemDefine copy = (ExpendableItemDefine)this.MemberwiseClone();
        copy.effect = effect.Copy();
        return copy;
    }
}
