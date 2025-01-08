using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;


public enum BuffType
{
    none = -1,
    normal = 0,
    weather = 1,
}

public class BuffModel: BuffDefine
{
    public string uuId;
    public string ownerID;
    public string casterID;
    public BuffDefine buffDefine;
    public int num = 1;

    public BuffType buffType = BuffType.none;

    public BuffModel(BuffDefine buffDefine, string ownerID, string casterID, BuffType buffType)
    {
        uuId = GameUtil.Instance.GenerateUniqueId();
        this.buffType = buffType;
        this.ownerID = ownerID;
        this.casterID = casterID;
        this.buffDefine = buffDefine;
        this.ID = buffDefine.ID;
        this.Name = buffDefine.Name;
        this.Description = buffDefine.Description;
        this.Resource = buffDefine.Resource;
        this.Duration = buffDefine.Duration;
        this.MethodName = buffDefine.MethodName;
        this.PropertyType = buffDefine.PropertyType;
        this.Value = buffDefine.Value;
        this.Duration = buffDefine.Duration;
        this.InvokeTime = buffDefine.InvokeTime;
        this.DecreaseTime = buffDefine.DecreaseTime;
        this.MergeType = buffDefine.MergeType;

        //this.OnAdd();
    }
    ////添加buff
    //private void OnAdd()
    //{
    //    if (this.buffDefine.Effect != BuffEffect.None)
    //    {
    //        this.owner.effectManager.AddEffect(this.buffDefine.Effect);
    //    }
    //    AddAttr();

    //    NBuffInfo buffInfo = new NBuffInfo()
    //    {
    //        //每个人身上唯一标识的buffId
    //        buffId = this.buffId,
    //        //buff在配置表中的id
    //        buffType = this.buffDefine.ID,
    //        casterId = this.context.caster.entityId,
    //        ownerId = this.owner.entityId,
    //        Action = BuffAction.Add
    //    };
    //    context.battle.AddBuffAction(buffInfo);
    //}
    ////移除buff
    //private void OnRemove()
    //{
    //    if (this.buffDefine.Effect != BuffEffect.None)
    //    {
    //        this.owner.effectManager.RemoveEffect(this.buffDefine.Effect);
    //    }
    //    RemoveAttr();
    //    isStoped = true;
    //    NBuffInfo buffInfo = new NBuffInfo()
    //    {
    //        //每个人身上唯一标识的buffId
    //        buffId = this.buffId,
    //        //buff在配置表中的id
    //        buffType = this.buffDefine.ID,
    //        casterId = this.context.caster.entityId,
    //        ownerId = this.owner.entityId,
    //        Action = BuffAction.Remove
    //    };
    //    context.battle.AddBuffAction(buffInfo);
    //}
    ////添加buff属性改变
    //private void AddAttr()
    //{
    //    if (this.buffDefine.DEFRatio != 0)
    //    {
    //        this.owner.attributes.Buff.DEF += this.owner.attributes.Basic.DEF * this.buffDefine.DEFRatio;
    //        this.owner.attributes.LoadFinalAttributes();
    //    }
    //}
    ////移除buff属性改变
    //private void RemoveAttr()
    //{
    //    if (this.buffDefine.DEFRatio != 0)
    //    {
    //        this.owner.attributes.Buff.DEF -= this.owner.attributes.Basic.DEF * this.buffDefine.DEFRatio;
    //        this.owner.attributes.LoadFinalAttributes();
    //    }
    //}
    //private void DoBuffDamage()
    //{
    //    this.hitTimes++;
    //    NDamageInfo damageInfo = this.CalcBuffDamage(context.caster);
    //    Log.InfoFormat("Buff[{0}].DoBuffDamage[{1}] Damage:{2}", this.buffDefine.Name, this.owner.Name, damageInfo.Damage);
    //    this.owner.DoDamage(damageInfo, context.caster);

    //    NBuffInfo buffInfo = new NBuffInfo()
    //    {
    //        //每个人身上唯一标识的buffId
    //        buffId = this.buffId,
    //        //buff在配置表中的id
    //        buffType = this.buffDefine.ID,
    //        casterId = this.context.caster.entityId,
    //        ownerId = this.owner.entityId,
    //        Action = BuffAction.Hit,
    //        damageInfo = damageInfo
    //    };
    //    context.battle.AddBuffAction(buffInfo);
    //}
    ////计算技能伤害
    //private NDamageInfo CalcBuffDamage(Creature caster)
    //{
    //    //技能伤害
    //    float ad = this.buffDefine.AD + caster.attributes.AD * this.buffDefine.ADFactor;
    //    float ap = this.buffDefine.AP + caster.attributes.AP * this.buffDefine.APFactor;
    //    //算上防御减免的
    //    float addmg = ad * (1 - owner.attributes.DEF / (owner.attributes.DEF + 100));
    //    float apdmg = ap * (1 - owner.attributes.MDEF / (owner.attributes.MDEF + 100));
    //    //ad,ap相加的总伤害
    //    float final = addmg + apdmg;

    //    NDamageInfo damageInfo = new NDamageInfo();
    //    damageInfo.entityId = owner.entityId;
    //    damageInfo.Damage = Math.Max(1, (int)final);
    //    return damageInfo;
    //}
    //internal void Update()
    //{
    //    if (isStoped) return;
    //    this.time += TimeUtil.deltaTime;
    //    if (this.buffDefine.Interval > 0)
    //    {
    //        if (this.time > this.buffDefine.Interval * (this.hitTimes + 1))
    //        {
    //            this.DoBuffDamage();
    //        }
    //    }
    //    if (time > this.buffDefine.Duration)
    //    {
    //        this.OnRemove();
    //    }
    //}
}
