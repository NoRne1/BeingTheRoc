using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectItemModel
{
    public int ID { get; set; }
    public CollectItemModelType type { get; set; }
    public string title { get; set; }
    public int num { get; set; }
    public string Resource { get; set; }

    public CollectItemModel(CollectItemDefine define)
    {
        this.ID = define.ID;
        this.type = define.type;
        this.title = define.title;
        this.num = define.num + GameUtil.Instance.GetTrulyFloatFactor(define.floatNum);
        this.Resource = define.Resource;
    }
}
