using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipExtraEntryDefine
{
    public int ID { get; set; }
    public int equipID { get; set; }
    public GeneralLevel level { get; set; }
    public Effect effect { get; set; }
    public int valueFloat { get; set; }
    public string descString { get; set; }
}

public class EquipExtraEntryModel: EquipExtraEntryDefine
{
    public EquipExtraEntryDefine define;
    public EquipExtraEntryModel(EquipExtraEntryDefine define)
    {
        this.define = define;
        this.ID = define.ID;
        this.equipID = define.equipID;
        this.level = define.level;
        this.effect = define.effect;
        this.valueFloat = GameUtil.Instance.GetTrulyFloatFactor(define.valueFloat);
        this.effect.value += this.valueFloat;
        this.descString = define.descString;
    }
}

