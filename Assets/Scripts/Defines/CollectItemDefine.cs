using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CollectItemModelType
{
    feather = 0,
    wheat = 1,
}

public class CollectItemDefine
{
    public int ID { get; set; }
    public CollectItemModelType type { get; set; }
    public string title { get; set; }
    public int num { get; set; }
    public int floatNum { get; set; }

    public string Resource { get; set; }

}
