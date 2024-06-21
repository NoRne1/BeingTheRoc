using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Attributes
{
    public int level = 0;
    public int remainExp { get { return exp - GlobalAccess.levelUpExp * level; } }
    public int exp = 0;
    //初始值
    AttributeData Initial = new AttributeData();
    //成长值
    AttributeData Growth = new AttributeData();
    //装备值
    AttributeData Equip = new AttributeData();
    //难度修正值
    public AttributeData Difficulty = new AttributeData();
    //buff值
    public AttributeData Buff = new AttributeData();
    //最终值
    public AttributeData Final = new AttributeData();
    //等级
    //动态属性(HP,MP)
    public AttributeDynamic dynamicAttr;

    public int currentHP
    {
        get { return dynamicAttr.currentHP; }
        set { dynamicAttr.currentHP = Mathf.Min(MaxHP, value); }
    }
    public int currentShield
    {
        get { return dynamicAttr.currentShield; }
        set { dynamicAttr.currentShield = value; }
    }
    public int currentEnergy
    {
        get { return dynamicAttr.currentEnergy; }
        set { dynamicAttr.currentEnergy = Mathf.Min(Energy, value); }
    }
    /// <summary>
    /// 生命
    /// </summary>
    public int MaxHP { get { return this.Final.MaxHP; } }
    /// <summary>
    /// 力量
    /// </summary>
    public int Strength { get { return this.Final.Strength; } }
    /// <summary>
    /// 防御
    /// </summary>
    public int Defense { get { return this.Final.Defense; } }
    /// <summary>
    /// 闪避
    /// </summary>
    public int Dodge { get { return this.Final.Dodge; } }
    /// <summary>
    /// 命中
    /// </summary>
    public int Accuracy { get { return this.Final.Accuracy; } }
    /// <summary>
    /// 角色行动速度
    /// </summary>
    public int Speed { get { return this.Final.Speed; } }
    /// <summary>
    /// 行动力
    /// </summary>
    public int Mobility { get { return this.Final.Mobility; } }
    /// <summary>
    /// 精力
    /// </summary>
    public int Energy { get { return this.Final.Energy; } }
    /// <summary>
    /// 幸运
    /// </summary>
    public int Lucky { get { return this.Final.Lucky; } }
    //初始化自己的角色
    public void Init(CharacterDefine define)
    {
        this.LoadInitAttribute(this.Initial, define, false);
        this.level = 0;
        this.LoadFinalAttributes();
    }
    //初始化预制的角色
    public void Init(CharacterDefine define, int level, List<StoreItemDefine> equips)
    {
        this.LoadInitAttribute(this.Initial, define, false);
        this.level = level;
        this.LoadGrowthAttribute(this.Growth, define.Job, level, false);
        this.LoadEquipAttributes(this.Equip, equips, false);
        
        this.LoadFinalAttributes();
    }
    //加载初始属性
    private void LoadInitAttribute(AttributeData attr, CharacterDefine define, bool loadFinal = true)
    {
        attr.MaxHP = define.MaxHP;
        attr.Strength = define.Strength;
        attr.Defense = define.Defense;
        attr.Dodge = define.Dodge;
        attr.Accuracy = define.Accuracy;
        attr.Speed = define.Speed;
        attr.Mobility = define.Mobility;
        attr.Energy = define.Energy;
        attr.Lucky = define.Lucky;
        if (loadFinal)
        {
            LoadFinalAttributes();
        }
    }
    //加载成长属性
    private void LoadGrowthAttribute(AttributeData attr, JobType job, int level, bool loadFinal = true)
    {
        //todo add GrowthAttribute
        if (loadFinal)
        {
            LoadFinalAttributes();
        }
    }
    //加载装备属性
    private void LoadEquipAttributes(AttributeData attr, List<StoreItemDefine> equips, bool loadFinal = true)
    {
        attr.Reset();
        if (equips == null) return;
        //所有装备加成加起来得到装备总属性加成
        foreach (var define in equips)
        {
            attr = attr + define.attr;
        }
        if (loadFinal)
        {
            LoadFinalAttributes();
        }
    }

    public void LoadFinalAttributes()
    {
        for (int i = (int)AttributeType.MaxHP; i < (int)AttributeType.MAX; i++)
        {
            this.Final.Data[i] = this.Initial.Data[i] + this.Growth.Data[i] + this.Difficulty.Data[i] + this.Equip.Data[i] + this.Buff.Data[i];
        }
    }
}
