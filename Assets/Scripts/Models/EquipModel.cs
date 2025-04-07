using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EquipModel : EquipDefine
{
    public AttributeData attr;
    public List<EquipExtraEntryModel> extraEntryModels;
    //for deepcopy
    public EquipModel(){}
    public EquipModel(EquipDefine define)
    {
        ID = define.ID;
        elementsType = define.elementsType;
        equipType = define.equipType;
        invokeType = define.invokeType;
        equipClass = define.equipClass;
        isExpendable = define.isExpendable;
        targetRange = define.targetRange;
        takeEnergy = define.takeEnergy;
        equipResource = define.equipResource;
        // just for attack
        baseAccuracy = define.baseAccuracy;
        effect1 = define.effect1;
        effect2 = define.effect2;
        effect3 = define.effect3;
        //每一点能量能够造成的威胁值
        attackThreaten = define.attackThreaten;
        //每一点能量能够造成的保护值
        protectAbility = define.protectAbility;
        attr = new AttributeData();
        extraEntryModels = new List<EquipExtraEntryModel>();
    }

    public void AddExtraEntrys(List<EquipExtraEntryModel> models)
    {
        extraEntryModels.Clear();
        extraEntryModels.AddRange(models);
        RefreshAttr();
    }

    public void RefreshAttr()
    {
        attr.Reset();
        foreach(var model in extraEntryModels.Where(model=> model.effect.effectType == EffectType.property))
        {
            attr.SetAttr(model.effect.propertyType.Value, model.effect.Value + attr.GetAttr(model.effect.propertyType.Value));
        }
    }
}
