using System.Collections.Generic;
using UnityEngine;



public class StoreItemModel : StoreItemDefine
{
    public string uuid;
    public Vector2Int position; // 在背包中的位置
    public int rotationAngle; // 旋转角度

    public StoreItemModel(StoreItemDefine define)
    {
        uuid = GameUtil.Instance.GenerateUniqueId();
        ID = define.ID;
        type = define.type;
        iconResource = define.iconResource;
        level = define.level;
        title = define.title;
        price = define.price;
    }

    public void Equip(Vector2Int position, int rotationAngle)
    {
        if (type != ItemType.none)
        {
            this.position = position;
            this.rotationAngle = rotationAngle;
        }
    }

    public void unEquip()
    {
        this.position = Vector2Int.zero;
        this.rotationAngle = 0;
    }
}
