using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using System;

public class Attributes
{
    public int level = 0;
    public int remainExp { get { return exp - GlobalAccess.levelUpExp * level; } }
    public int exp = 0;
    public int maxPropertyPoints = 0;
    public int hungryConsume = 25;
    private int remainPropertyPoints = 0;
    //初始值
    AttributeData Initial = new AttributeData();
    //成长值
    AttributeData Growth = new AttributeData();
    //物品加成值
    public AttributeData ItemEffect = new AttributeData();
    //战斗中装备加成值
    public AttributeData InBattle = new AttributeData();
    //装备值
    AttributeData Equip = new AttributeData();
    //难度修正值
    public AttributeData Difficulty = new AttributeData();
    //skill值
    public AttributeData Skill = new AttributeData();
    //buff值
    public AttributeData Buff = new AttributeData();
    //最终值
    public AttributeData Final = new AttributeData();
    //等级
    //动态属性(HP,MP)
    private AttributeDynamic dynamicAttr = new AttributeDynamic(-1, -1, -1, -1, -1);

    public Subject<int> hpChangeSubject = new Subject<int>();
    public Subject<Unit> deadSubject = new Subject<Unit>();
    public int currentHP
    {
        get 
        { 
            return dynamicAttr.currentHP; 
        }
        set 
        {
            int newHp = Mathf.Min(MaxHP, value);
            int change = newHp - dynamicAttr.currentHP;
            dynamicAttr.currentHP = newHp;

            if (change < 0)
            {
                dynamicAttr.lostHP -= change;
            }
            if (newHp <= 0)
            {
                deadSubject.OnNext(Unit.Default);
            }
            hpChangeSubject.OnNext(change);
        }
    }

    public int currentHungry
    {
        get 
        { 
            return dynamicAttr.currentHungry; 
        }
        set 
        {
            dynamicAttr.currentHungry = Math.Max(0, Math.Min(MaxHP, value));
        }
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
    public int lostHP
    {
        get { return dynamicAttr.lostHP; }
        set { dynamicAttr.lostHP = value; }
    }

    public int RemainPropertyPoints
    {
        get { return remainPropertyPoints; }
        set { remainPropertyPoints = Mathf.Max(0, value); }
    }
    /// <summary>
    /// 生命
    /// </summary>
    public int MaxHP { get { return this.Final.MaxHP; } }
    /// <summary>
    /// 饥饿度
    /// </summary>
    public int MaxHungry { get { return this.Final.MaxHungry; } }
    /// <summary>
    /// 力量
    /// </summary>
    public int Strength { get { return this.Final.Strength; } }
    /// <summary>
    /// 法力
    /// </summary>
    public int Magic { get { return this.Final.Magic; } }
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
    /// 嘲讽
    /// </summary>
    public int Taunt { get { return this.Final.Taunt; } }

    private Subject<Unit> updateSubject;

    // just for deepcopy
    public Attributes(){}

    public Attributes(Subject<Unit> subject)
    {
        updateSubject = subject;
    }

    //初始化自己的角色
    public void Init(CharacterDefine define)
    {
        this.LoadInitAttribute(this.Initial, define, false);
        this.level = 0;
        this.LoadFinalAttributes();
        dynamicAttr.currentHP = MaxHP;
        dynamicAttr.currentHungry = MaxHungry;
    }
    //初始化预制的角色
    public void Init(CharacterDefine define, int level)
    {
        this.LoadInitAttribute(this.Initial, define, false);
        this.level = level;
        this.LoadGrowthAttribute(this.Growth, define.Job, level, false);
        this.LoadFinalAttributes();
        dynamicAttr.currentHP = MaxHP;
        dynamicAttr.currentHungry = MaxHungry;
    }

    public void SetUpdateSubject(Subject<Unit> subject)
    {
        updateSubject = subject;
    }

    public void BattleInit()
    {
        dynamicAttr.currentShield = 0;
        dynamicAttr.currentEnergy = 0;
        dynamicAttr.lostHP = 0;
    }

    public void ResetGrowthProperty()
    {
        Growth = new AttributeData();
        RemainPropertyPoints = maxPropertyPoints;
        LoadFinalAttributes();
    }

    public bool setGrowthPropertyValue(AttributeType type, int change) 
    {
        var ruleFactor = GlobalAccess.GetPropertyTransferRuleFactor(type);
        if (ruleFactor == -1) 
        { 
            BlackBarManager.Instance.AddMessage(GameUtil.Instance.GetDisplayString("choose_property_tip"));
            return false; 
        }
        if ((change >= 0 && Mathf.Abs(ruleFactor * change) <= remainPropertyPoints) || 
        (change < 0 && remainPropertyPoints + Mathf.Abs(ruleFactor * change) <= maxPropertyPoints && Growth.GetAttr(type) + change >= 0)) 
        {
            //加值时，变化的绝对值需要小于remainPropertyPoints，减值时，当前值加变化的绝对值要小于maxPropertyPoints
            Growth.SetAttr(type, Growth.GetAttr(type) + change);
            remainPropertyPoints -= ruleFactor * change;
            LoadFinalAttributes();
            return true;
        } else  
        {
            BlackBarManager.Instance.AddMessage(GameUtil.Instance.GetDisplayString("change_property_error"));
            return false;
        }
    }

    public int getFinalPropertyValue(AttributeType type) 
    {
        switch (type)
        {
            case AttributeType.MaxHP:
                return MaxHP;
            case AttributeType.MaxHungry:
                return MaxHungry;
            case AttributeType.Strength:
                return Strength;
            case AttributeType.Magic:
                return Magic;
            case AttributeType.Speed:
                return Speed;
            case AttributeType.Mobility:
                return Mobility;
            case AttributeType.Energy:
                return Energy;
            case AttributeType.Taunt:
                return Taunt;
            default:
                Debug.LogError("unknown AttributeType!");
                return -1;
        }
    }

    // just for init
    public void UpdateInitSpeed(int speed)
    {
        Initial.Speed = speed;
        LoadFinalAttributes();
    }

    public void UpdateInitMaxHP(int hp)
    {
        Initial.MaxHP = hp;
        LoadFinalAttributes();
        dynamicAttr.currentHP = MaxHP;
    }

    //加载初始属性
    private void LoadInitAttribute(AttributeData attr, CharacterDefine define, bool loadFinal = true)
    {
        attr.MaxHP = define.MaxHP + GameUtil.Instance.GetTrulyFloatFactor(define.MaxHPFloat);
        attr.Strength = define.Strength + GameUtil.Instance.GetTrulyFloatFactor(define.StrengthFloat);
        attr.Magic = define.Magic + GameUtil.Instance.GetTrulyFloatFactor(define.MagicFloat);
        attr.Speed = define.Speed + GameUtil.Instance.GetTrulyFloatFactor(define.SpeedFloat);
        attr.Mobility = define.Mobility;
        attr.Energy = define.Energy;
        attr.Taunt = GlobalAccess.GetTauntForJob(define.Job);
        // 默认就为0，所以不用赋值0
        //attr.Protection = 0;
        //attr.EnchanceDamage = 0;
        //attr.AgainstDamage = 0;

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
    public void LoadEquipAttributes(List<StoreItemModel> equips, bool loadFinal = true)
    {
        Equip.Reset();
        if (equips == null) return;
        //所有装备加成加起来得到装备总属性加成
        foreach (var define in equips)
        {
            Equip = Equip + define.equipModel.attr;
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
            var type = (AttributeType)i;
            this.Final.SetAttr(type, this.Initial.GetAttr(type) + this.Growth.GetAttr(type) +
                this.ItemEffect.GetAttr(type) + this.InBattle.GetAttr(type) + this.Difficulty.GetAttr(type) +
                this.Equip.GetAttr(type) + this.Skill.GetAttr(type) + this.Buff.GetAttr(type));
        }
        updateSubject.OnNext(Unit.Default);
    }
}
