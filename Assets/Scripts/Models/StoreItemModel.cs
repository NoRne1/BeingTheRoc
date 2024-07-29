using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class StoreItemModel : StoreItemDefine
{
    public string uuid;
    public string characterID = "";
    public Vector2Int position; // 在背包中的位置
    public int rotationAngle; // 旋转角度
    private int tempRotationAngle; // 旋转角度
    public List<Effect> effects = new List<Effect>();

    public EquipDefine equipDefine;
    public ExpendableItemDefine expendableItemDefine;
    public TreasureDefine treasureDefine;
    public GoodsDefine goodsDefine;
    public SpecialItemDefine specialItemDefine;
    public StoreItemModel()
    {}
    public StoreItemModel(StoreItemDefine define)
    {
        uuid = GameUtil.Instance.GenerateUniqueId();
        ID = define.ID;
        type = define.type;
        subID = define.subID;
        title = define.title;
        level = define.level;
        price = define.price;
        iconResource = define.iconResource;
        desc = define.desc;
        ExtraEntry1 = define.ExtraEntry1;
        ExtraEntry2 = define.ExtraEntry2;
        ExtraEntry3 = define.ExtraEntry3;

        switch (type)
        {
            case ItemType.equip:
                equipDefine = GameUtil.Instance.DeepCopy(DataManager.Instance.EquipDefines[subID]);
                if (equipDefine.effect1 != null)
                {
                    effects.Add(equipDefine.effect1);
                }
                if (equipDefine.effect2 != null)
                {
                    effects.Add(equipDefine.effect2);
                }
                if (equipDefine.effect3 != null)
                {
                    effects.Add(equipDefine.effect3);
                }
                equipDefine.OccupiedCellsInit();
                break;
            case ItemType.expendable:
                expendableItemDefine = GameUtil.Instance.DeepCopy(DataManager.Instance.ExpendableItemDefines[subID]);
                if (expendableItemDefine.effect != null)
                {
                    effects.Add(expendableItemDefine.effect);
                }
                break;
            case ItemType.treasure:
                treasureDefine = GameUtil.Instance.DeepCopy(DataManager.Instance.TreasureDefines[subID]);
                break;
            case ItemType.economicGoods:
                goodsDefine = GameUtil.Instance.DeepCopy(DataManager.Instance.GoodsDefines[subID]);
                break;
            case ItemType.special:
                specialItemDefine = GameUtil.Instance.DeepCopy(DataManager.Instance.SpecialItemDefines[subID]);
                break;
        }
    }

    public void Equip(string characterID, Vector2Int position)
    {
        if (CanEquip())
        {
            this.characterID = characterID;
            this.position = position;
            this.rotationAngle = tempRotationAngle;
            this.equipDefine.occupiedCells = new List<Vector2Int>(equipDefine.tempOccupiedCells);
        }
    }

    public void ResetRotate()
    {
        if (CanEquip())
        {
            this.tempRotationAngle = rotationAngle;
            this.equipDefine.tempOccupiedCells = new List<Vector2Int>(equipDefine.occupiedCells);
        }
    }

    public void Unequip()
    {
        if (CanEquip())
        {
            this.characterID = "";
            this.position = Vector2Int.zero;
            this.rotationAngle = 0;
            equipDefine.OccupiedCellsInit();
        }
    }

    public void Rotate(int angle)
    {
        if (CanEquip())
        {
            List<Vector2Int> points;
            if (equipDefine.tempOccupiedCells == null || equipDefine.tempOccupiedCells.Count == 0)
            {
                return;
            }
            points = new List<Vector2Int>(equipDefine.tempOccupiedCells);
            // 对每个点进行旋转和平移
            for (int i = 0; i < points.Count; i++)
            {
                Vector2Int point = points[i]; // 获取当前点的副本
                int newX, newY;

                // 根据给定的角度执行旋转
                switch (angle)
                {
                    case 0:
                        newX = point.x;
                        newY = point.y;
                        break;
                    case 90:
                        newX = -point.y;
                        newY = point.x;
                        break;
                    case 180:
                        newX = -point.x;
                        newY = -point.y;
                        break;
                    case 270:
                        newX = point.y;
                        newY = -point.x;
                        break;
                    default:
                        throw new ArgumentException("Invalid angle. Please enter 0, 90, 180, or 270.");
                }

                // 更新点的坐标
                points[i] = new Vector2Int(newX, newY);
            }
            equipDefine.tempOccupiedCells = points;
            tempRotationAngle = (tempRotationAngle + angle) % 360;
        }
    }
}
