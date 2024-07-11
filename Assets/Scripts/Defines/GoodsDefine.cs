using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoodsDefine
{
    public int ID { get; set; }
    public int minPrice { get; set; }
    public int maxPrice { get; set; }

    public GoodsDefine Copy()
    {
        GoodsDefine copy = (GoodsDefine)this.MemberwiseClone();

        return copy;
    }
}
