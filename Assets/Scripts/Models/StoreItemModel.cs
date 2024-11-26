using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

public class StoreItemModel : StoreItemDefine
{
    public string uuid;
    public string characterID = "";
    public Vector2Int position; // 在背包中的位置
    public int rotationAngle; // 旋转角度
    private int tempRotationAngle; // 旋转角度
    public List<Effect> effects = new List<Effect>();

    public EquipModel equipModel;
    public ExpendableItemDefine expendableItemDefine;
    public TreasureDefine treasureDefine;
    public GoodsDefine goodsDefine;
    public SpecialItemDefine specialItemDefine;
    public FoodModel foodModel;

    public bool equipCanUse = true;
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
                equipModel = new EquipModel(DataManager.Instance.EquipDefines[subID]);
                if (equipModel.effect1 != null)
                {
                    effects.Add(equipModel.effect1);
                }
                if (equipModel.effect2 != null)
                {
                    effects.Add(equipModel.effect2);
                }
                if (equipModel.effect3 != null)
                {
                    effects.Add(equipModel.effect3);
                }
                equipModel.OccupiedCellsInit();
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
            case ItemType.food:
                foodModel = new FoodModel(DataManager.Instance.FoodDefines[subID]);
                effects.AddRange(foodModel.GetEffects());
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
            this.equipModel.occupiedCells = new List<Vector2Int>(equipModel.tempOccupiedCells);
        }
    }

    public void ResetRotate()
    {
        if (CanEquip())
        {
            this.tempRotationAngle = rotationAngle;
            this.equipModel.tempOccupiedCells = new List<Vector2Int>(equipModel.OccupiedCells);
        }
    }

    public void Unequip()
    {
        if (CanEquip())
        {
            this.characterID = "";
            this.position = Vector2Int.zero;
            this.rotationAngle = 0;
            equipModel.OccupiedCellsInit();
        }
    }

    public void Rotate(int angle)
    {
        if (CanEquip())
        {
            List<Vector2Int> points;
            if (equipModel.tempOccupiedCells == null || equipModel.tempOccupiedCells.Count == 0)
            {
                return;
            }
            points = new List<Vector2Int>(equipModel.tempOccupiedCells);
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
            equipModel.tempOccupiedCells = points;
            tempRotationAngle = (tempRotationAngle + angle) % 360;
        }
    }

    public string GetFoodDesc() 
    {
        if (type == ItemType.food && foodModel != null)
        {
            // 根据 foodPropertys 的数量创建 args 数组
            object[] args = new object[foodModel.foodPropertys.Count];
            
            for (int i = 0; i < foodModel.foodPropertys.Count; i++)
            {
                var foodProperty = foodModel.foodPropertys[i];
                // 将值和 floatFactor 相加并设置到 args 中
                args[i] = foodProperty.trulyValue;
            }
            
            //todo 需要从 GameUtil.Instance.GetDisplayString 拿
            return string.Format(desc, args).ReplaceNewLines();
        }
        return "";
    }

    public bool CanEquipEnhance()
    {
        return type == ItemType.equip && !equipModel.isExpendable;
    }
}
